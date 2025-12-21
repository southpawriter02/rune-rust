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
/// </summary>
public class EnvironmentPopulator : IEnvironmentPopulator
{
    private readonly IRepository<HazardTemplate> _hazardTemplateRepo;
    private readonly IRepository<AmbientCondition> _conditionRepo;
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
    /// <param name="diceService">Dice service for random selection.</param>
    /// <param name="logger">Logger instance.</param>
    public EnvironmentPopulator(
        IRepository<HazardTemplate> hazardTemplateRepo,
        IRepository<AmbientCondition> conditionRepo,
        IDiceService diceService,
        ILogger<EnvironmentPopulator> logger)
    {
        _hazardTemplateRepo = hazardTemplateRepo;
        _conditionRepo = conditionRepo;
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
}
