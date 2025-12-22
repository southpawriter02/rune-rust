using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Populates rooms with biome-appropriate hazards and conditions (v0.3.3c).
/// Uses database-stored HazardTemplates and AmbientConditions filtered by biome tags.
/// Danger level affects spawn probability.
/// Updated in v0.4.0 to support BiomeElement-based spawning with spawn rule evaluation.
/// </summary>
public class EnvironmentPopulator : IEnvironmentPopulator
{
    private readonly IRepository<HazardTemplate> _hazardTemplateRepo;
    private readonly IRepository<AmbientCondition> _conditionRepo;
    private readonly IRepository<BiomeElement> _biomeElementRepo;
    private readonly IElementSpawnEvaluator _spawnEvaluator;
    private readonly IDiceService _diceService;
    private readonly ILogger<EnvironmentPopulator> _logger;

    /// <summary>
    /// Base chance (0.0-1.0) for a room to receive a hazard.
    /// Modified by danger level via BiomeEnvironmentMapping.GetDangerMultiplier.
    /// </summary>
    private const float BaseHazardChance = 0.2f;

    /// <summary>
    /// Base chance (0.0-1.0) for a room to receive a condition.
    /// Modified by danger level via BiomeEnvironmentMapping.GetDangerMultiplier.
    /// </summary>
    private const float BaseConditionChance = 0.15f;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentPopulator"/> class.
    /// </summary>
    /// <param name="hazardTemplateRepo">Repository for hazard templates.</param>
    /// <param name="conditionRepo">Repository for ambient conditions.</param>
    /// <param name="biomeElementRepo">Repository for biome elements (v0.4.0).</param>
    /// <param name="spawnEvaluator">Spawn rule evaluator (v0.4.0).</param>
    /// <param name="diceService">Dice service for random selection.</param>
    /// <param name="logger">Logger instance.</param>
    public EnvironmentPopulator(
        IRepository<HazardTemplate> hazardTemplateRepo,
        IRepository<AmbientCondition> conditionRepo,
        IRepository<BiomeElement> biomeElementRepo,
        IElementSpawnEvaluator spawnEvaluator,
        IDiceService diceService,
        ILogger<EnvironmentPopulator> logger)
    {
        _hazardTemplateRepo = hazardTemplateRepo;
        _conditionRepo = conditionRepo;
        _biomeElementRepo = biomeElementRepo;
        _spawnEvaluator = spawnEvaluator;
        _diceService = diceService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Room> PopulateRoomAsync(Room room)
    {
        var biome = room.BiomeType;
        var dangerMultiplier = BiomeEnvironmentMapping.GetDangerMultiplier(room.DangerLevel);

        var hazardChance = BaseHazardChance + dangerMultiplier;
        var conditionChance = BaseConditionChance + dangerMultiplier;

        _logger.LogDebug(
            "[Environment] Populating {RoomName} (Biome: {Biome}, Danger: {Danger}, HazardChance: {HazardChance:P0}, ConditionChance: {ConditionChance:P0})",
            room.Name, biome, room.DangerLevel, hazardChance, conditionChance);

        // Roll for hazard
        if (RollChance(hazardChance))
        {
            await AssignHazardAsync(room, biome);
        }

        // Roll for condition (only if room doesn't already have one)
        if (room.ConditionId == null && RollChance(conditionChance))
        {
            await AssignConditionAsync(room, biome);
        }

        return room;
    }

    /// <inheritdoc/>
    public async Task PopulateDungeonAsync(IEnumerable<Room> rooms)
    {
        _logger.LogInformation("[Environment] Populating dungeon rooms");

        var roomList = rooms.ToList();
        var hazardCount = 0;
        var conditionCount = 0;

        foreach (var room in roomList)
        {
            var hadHazard = room.Hazards.Count;
            var hadCondition = room.ConditionId != null;

            await PopulateRoomAsync(room);

            if (room.Hazards.Count > hadHazard) hazardCount++;
            if (room.ConditionId != null && !hadCondition) conditionCount++;
        }

        _logger.LogInformation(
            "[Environment] Dungeon population complete. Assigned {HazardCount} hazards and {ConditionCount} conditions to {RoomCount} rooms",
            hazardCount, conditionCount, roomList.Count);
    }

    /// <summary>
    /// Attempts to assign a hazard to a room based on its biome.
    /// </summary>
    private async Task AssignHazardAsync(Room room, BiomeType biome)
    {
        var templates = await _hazardTemplateRepo.GetAllAsync();
        var validTemplates = templates
            .Where(t => t.BiomeTags.Count == 0 || t.BiomeTags.Contains(biome))
            .ToList();

        if (validTemplates.Count == 0)
        {
            _logger.LogWarning("[Environment] No valid hazard templates for biome {Biome}", biome);
            return;
        }

        var template = validTemplates[_diceService.RollSingle(validTemplates.Count, "Hazard selection") - 1];

        // Create hazard instance from template
        var hazard = new DynamicHazard
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Name = template.Name,
            Description = template.Description,
            HazardType = template.HazardType,
            Trigger = template.Trigger,
            EffectScript = template.EffectScript,
            MaxCooldown = template.MaxCooldown,
            OneTimeUse = template.OneTimeUse,
            State = HazardState.Dormant
        };

        room.Hazards.Add(hazard);

        _logger.LogInformation(
            "[Environment] Assigned hazard [{HazardName}] to room {RoomName}",
            hazard.Name, room.Name);
    }

    /// <summary>
    /// Attempts to assign a condition to a room based on its biome.
    /// </summary>
    private async Task AssignConditionAsync(Room room, BiomeType biome)
    {
        var validTypes = BiomeEnvironmentMapping.GetConditionTypes(biome);
        var conditions = await _conditionRepo.GetAllAsync();
        var validConditions = conditions
            .Where(c => c.BiomeTags.Count == 0 || c.BiomeTags.Contains(biome))
            .Where(c => validTypes.Contains(c.Type))
            .ToList();

        if (validConditions.Count == 0)
        {
            _logger.LogDebug("[Environment] No valid conditions for biome {Biome}", biome);
            return;
        }

        var condition = validConditions[_diceService.RollSingle(validConditions.Count, "Condition selection") - 1];
        room.ConditionId = condition.Id;

        _logger.LogInformation(
            "[Environment] Assigned condition [{ConditionName}] to room {RoomName}",
            condition.Name, room.Name);
    }

    /// <summary>
    /// Rolls a percentile check against a target chance.
    /// </summary>
    /// <param name="chance">The target chance (0.0-1.0).</param>
    /// <returns>True if the roll succeeds (roll <= chance).</returns>
    private bool RollChance(float chance)
    {
        var roll = _diceService.RollSingle(100, "Environment population") / 100.0f;
        return roll <= chance;
    }

    /// <summary>
    /// Populates a room with biome elements using spawn rule evaluation (v0.4.0).
    /// This is the new template-based population method that integrates with the Dynamic Room Engine.
    /// Spawns elements in dependency order: Enemies → Hazards → Terrain → Loot → Conditions.
    /// </summary>
    /// <param name="room">The room to populate.</param>
    /// <param name="template">The room template (provides archetype, size metadata).</param>
    /// <param name="biomeId">The biome identifier for filtering elements.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PopulateRoomAsync(Room room, RoomTemplate template, string biomeId)
    {
        _logger.LogInformation(
            "[Environment] Populating room {RoomName} (Template: {TemplateId}, Biome: {BiomeId})",
            room.Name, template.TemplateId, biomeId);

        // Get all biome elements for this biome
        var allElements = await _biomeElementRepo.GetAllAsync();
        var biomeElements = allElements.Where(e => e.BiomeId == biomeId).ToList();

        if (biomeElements.Count == 0)
        {
            _logger.LogWarning("[Environment] No biome elements found for biome: {BiomeId}", biomeId);
            return;
        }

        var context = new SpawnContext();

        // Spawn in dependency order (Phase 1: enemies and hazards only)
        // Phase 2 TODO: Add terrain, loot, full dependency resolution

        // 1. Spawn Enemies (DormantProcess)
        await SpawnElementsByTypeAsync(room, template, biomeElements, context, "DormantProcess");

        // 2. Spawn Hazards (DynamicHazard)
        await SpawnElementsByTypeAsync(room, template, biomeElements, context, "DynamicHazard");

        // 3. Spawn Conditions (AmbientCondition)
        if (room.ConditionId == null)
        {
            await SpawnElementsByTypeAsync(room, template, biomeElements, context, "AmbientCondition");
        }

        _logger.LogInformation(
            "[Environment] Room population complete. {RoomName} has {HazardCount} hazards, condition: {HasCondition}",
            room.Name, room.Hazards.Count, room.ConditionId.HasValue);
    }

    /// <summary>
    /// Spawns elements of a specific type using weighted random selection and spawn rule evaluation.
    /// </summary>
    private async Task SpawnElementsByTypeAsync(
        Room room,
        RoomTemplate template,
        List<BiomeElement> biomeElements,
        SpawnContext context,
        string elementType)
    {
        // Filter elements by type
        var candidates = biomeElements
            .Where(e => e.ElementType == elementType)
            .ToList();

        if (candidates.Count == 0)
        {
            _logger.LogDebug("[Environment] No {ElementType} elements available for spawning", elementType);
            return;
        }

        // Apply spawn rule filtering
        var validCandidates = candidates
            .Where(e => _spawnEvaluator.CanSpawn(e, room, template, context))
            .ToList();

        if (validCandidates.Count == 0)
        {
            _logger.LogDebug("[Environment] No valid {ElementType} elements after spawn rule evaluation", elementType);
            return;
        }

        // Calculate adjusted weights
        var weightedCandidates = validCandidates
            .Select(e => new
            {
                Element = e,
                AdjustedWeight = _spawnEvaluator.GetAdjustedWeight(e, room, template)
            })
            .ToList();

        // Perform weighted random selection
        var totalWeight = weightedCandidates.Sum(c => c.AdjustedWeight);

        if (totalWeight <= 0)
        {
            _logger.LogWarning("[Environment] Total weight is zero for {ElementType} elements", elementType);
            return;
        }

        var roll = (float)_diceService.RollSingle(10000, $"{elementType} selection") / 10000.0f * totalWeight;
        var cumulativeWeight = 0.0f;
        BiomeElement? selectedElement = null;

        foreach (var candidate in weightedCandidates)
        {
            cumulativeWeight += candidate.AdjustedWeight;
            if (roll <= cumulativeWeight)
            {
                selectedElement = candidate.Element;
                break;
            }
        }

        if (selectedElement == null)
        {
            _logger.LogWarning("[Environment] Weighted selection failed for {ElementType}", elementType);
            return;
        }

        // Spawn the selected element
        await SpawnElementAsync(room, selectedElement, context);
    }

    /// <summary>
    /// Spawns a specific biome element into the room.
    /// </summary>
    private async Task SpawnElementAsync(Room room, BiomeElement element, SpawnContext context)
    {
        _logger.LogDebug("[Environment] Spawning {ElementType}: {ElementName} (AssociatedDataId: {DataId})",
            element.ElementType, element.ElementName, element.AssociatedDataId);

        switch (element.ElementType)
        {
            case "DormantProcess":
                // TODO Phase 2: Spawn enemy entities
                context.SpawnedEnemyTypes.Add(element.AssociatedDataId);
                _logger.LogInformation("[Environment] Enemy spawning not yet implemented: {ElementName}", element.ElementName);
                break;

            case "DynamicHazard":
                await SpawnHazardFromElementAsync(room, element);
                context.SpawnedHazardTypes.Add(element.AssociatedDataId);
                break;

            case "StaticTerrain":
                // TODO Phase 2: Spawn terrain obstacles
                _logger.LogInformation("[Environment] Terrain spawning not yet implemented: {ElementName}", element.ElementName);
                break;

            case "LootNode":
                // TODO Phase 2: Spawn loot containers
                _logger.LogInformation("[Environment] Loot spawning not yet implemented: {ElementName}", element.ElementName);
                break;

            case "AmbientCondition":
                await SpawnConditionFromElementAsync(room, element);
                break;

            default:
                _logger.LogWarning("[Environment] Unknown element type: {ElementType}", element.ElementType);
                break;
        }
    }

    /// <summary>
    /// Spawns a hazard from a biome element by looking up the associated HazardTemplate.
    /// </summary>
    private async Task SpawnHazardFromElementAsync(Room room, BiomeElement element)
    {
        var templates = await _hazardTemplateRepo.GetAllAsync();
        var template = templates.FirstOrDefault(t => t.Name.Equals(element.AssociatedDataId, StringComparison.OrdinalIgnoreCase));

        if (template == null)
        {
            _logger.LogWarning("[Environment] HazardTemplate not found for AssociatedDataId: {DataId}", element.AssociatedDataId);
            return;
        }

        var hazard = new DynamicHazard
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Name = template.Name,
            Description = template.Description,
            HazardType = template.HazardType,
            Trigger = template.Trigger,
            EffectScript = template.EffectScript,
            MaxCooldown = template.MaxCooldown,
            OneTimeUse = template.OneTimeUse,
            State = HazardState.Dormant
        };

        room.Hazards.Add(hazard);

        _logger.LogInformation("[Environment] Spawned hazard: {HazardName} in {RoomName}",
            hazard.Name, room.Name);
    }

    /// <summary>
    /// Spawns an ambient condition from a biome element by looking up the associated AmbientCondition.
    /// </summary>
    private async Task SpawnConditionFromElementAsync(Room room, BiomeElement element)
    {
        var conditions = await _conditionRepo.GetAllAsync();
        var condition = conditions.FirstOrDefault(c => c.Name.Equals(element.AssociatedDataId, StringComparison.OrdinalIgnoreCase));

        if (condition == null)
        {
            _logger.LogWarning("[Environment] AmbientCondition not found for AssociatedDataId: {DataId}", element.AssociatedDataId);
            return;
        }

        room.ConditionId = condition.Id;

        _logger.LogInformation("[Environment] Assigned condition: {ConditionName} to {RoomName}",
            condition.Name, room.Name);
    }
}
