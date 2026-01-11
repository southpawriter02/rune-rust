using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for procedural room generation.
/// </summary>
public class RoomGeneratorService : IRoomGeneratorService
{
    private readonly RoomTemplateConfiguration _templateConfig;
    private readonly GenerationRulesConfiguration _rulesConfig;
    private readonly BiomeConfiguration _biomeConfig;
    private readonly ILogger<RoomGeneratorService> _logger;

    /// <summary>
    /// Creates a new RoomGeneratorService.
    /// </summary>
    public RoomGeneratorService(
        RoomTemplateConfiguration templateConfig,
        GenerationRulesConfiguration rulesConfig,
        BiomeConfiguration biomeConfig,
        ILogger<RoomGeneratorService> logger)
    {
        _templateConfig = templateConfig ?? throw new ArgumentNullException(nameof(templateConfig));
        _rulesConfig = rulesConfig ?? throw new ArgumentNullException(nameof(rulesConfig));
        _biomeConfig = biomeConfig ?? throw new ArgumentNullException(nameof(biomeConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public GeneratedRoomResult GenerateRoom(Position3D position, string biomeId, int seed)
    {
        var random = new Random(seed);
        var difficultyModifier = GetDepthDifficultyModifier(position.Z);

        // Select template
        var template = SelectTemplate(biomeId, position.Z, random);
        if (template == null)
        {
            _logger.LogWarning(
                "No valid template found for biome {Biome} at depth {Depth}, using fallback",
                biomeId, position.Z);
            template = CreateFallbackTemplate(biomeId);
        }

        // Generate room name from pattern (use NamePattern directly for now)
        var name = template.NamePattern;

        // Generate description from pattern
        var description = template.DescriptionPattern;

        // Create room
        var room = new Room(name, description, position);

        // Set environment context
        var environment = CreateEnvironmentContext(biomeId, template);
        room.SetEnvironment(environment);

        // Determine exits
        var exits = DetermineExits(template, seed, null).ToList();

        _logger.LogDebug(
            "Generated room '{Name}' at {Position} using template {Template} (biome: {Biome})",
            name, position, template.TemplateId, biomeId);

        return new GeneratedRoomResult(room, exits, template.TemplateId, biomeId, difficultyModifier);
    }

    /// <inheritdoc />
    public Room GetOrGenerateRoom(Dungeon dungeon, Position3D position, Direction? fromDirection = null)
    {
        ArgumentNullException.ThrowIfNull(dungeon);

        // Check if room already exists
        var existingRoom = dungeon.GetRoomByPosition(position);
        if (existingRoom != null)
        {
            _logger.LogDebug("Returning existing room at {Position}", position);
            return existingRoom;
        }

        // Check if we can generate
        if (!dungeon.CanGenerateAt(position))
        {
            _logger.LogWarning("Cannot generate room at {Position} - room limit reached", position);
            throw new InvalidOperationException($"Cannot generate room at {position}: room limit reached for level");
        }

        // Generate new room
        var baseSeed = dungeon.GetHashCode();
        var seed = GenerationContext.CreatePositionSeed(position, baseSeed);
        var biomeId = DetermineBiomeForDepth(position.Z, seed);

        var result = GenerateRoom(position, biomeId, seed);

        // Add to dungeon
        dungeon.AddRoom(result.Room);

        _logger.LogInformation(
            "Generated new room '{Name}' at {Position}",
            result.Room.Name, position);

        return result.Room;
    }

    /// <inheritdoc />
    public IEnumerable<Direction> DetermineExits(
        RoomTemplate template,
        int seed,
        Direction? guaranteedExit = null)
    {
        var random = new Random(seed + 1); // Offset seed for exit generation
        var exits = new List<Direction>();

        // Get exit slots from template
        var exitSlots = template.GetSlotsByType(SlotType.Exit).ToList();
        var exitProbabilities = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        // Extract exit probabilities from slot constraints
        foreach (var slot in exitSlots)
        {
            var direction = slot.GetConstraint("direction");
            if (!string.IsNullOrEmpty(direction))
            {
                exitProbabilities[direction] = slot.EffectiveFillProbability;
            }
        }

        // Use default probabilities if no exit slots defined
        if (exitProbabilities.Count == 0)
        {
            exitProbabilities = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["north"] = 0.5f,
                ["south"] = 0.5f,
                ["east"] = 0.5f,
                ["west"] = 0.5f
            };
        }

        // Check each direction
        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            var directionName = direction.ToString().ToLowerInvariant();
            var probability = exitProbabilities.TryGetValue(directionName, out var prob) ? prob : 0f;

            // Guaranteed exit always included
            if (guaranteedExit.HasValue && direction == guaranteedExit.Value)
            {
                exits.Add(direction);
                continue;
            }

            // Roll for probability
            if (random.NextDouble() < probability)
            {
                exits.Add(direction);
            }
        }

        // Ensure minimum exits
        var minExits = _rulesConfig.MinExitsPerRoom;
        while (exits.Count < minExits && exits.Count < 6)
        {
            var availableDirections = Enum.GetValues<Direction>()
                .Where(d => !exits.Contains(d))
                .ToList();

            if (availableDirections.Count == 0) break;

            var randomDir = availableDirections[random.Next(availableDirections.Count)];
            exits.Add(randomDir);
        }

        // Enforce maximum exits
        var maxExits = _rulesConfig.MaxExitsPerRoom;
        if (exits.Count > maxExits && guaranteedExit.HasValue)
        {
            exits = exits
                .OrderBy(e => e == guaranteedExit.Value ? 0 : 1)
                .ThenBy(_ => random.Next())
                .Take(maxExits)
                .ToList();
        }

        return exits;
    }

    /// <inheritdoc />
    public float GetDepthDifficultyModifier(int depth)
    {
        if (depth <= 0) return 1.0f;
        return 1.0f + (depth * _rulesConfig.DepthDifficultyMultiplier);
    }

    /// <inheritdoc />
    public string DetermineBiomeForDepth(int depth, int seed)
    {
        var random = new Random(seed + 2); // Offset seed for biome selection

        // Check biome transition depths (try deeper biomes first)
        var validTransitions = _rulesConfig.BiomeTransitionDepths
            .Where(t => t.Value.IsValidForDepth(depth))
            .OrderByDescending(t => t.Value.MinDepth)
            .ToList();

        foreach (var transition in validTransitions)
        {
            var biomeId = transition.Key;
            var range = transition.Value;

            // Check transition probability
            if (random.NextDouble() < range.TransitionProbability)
            {
                return biomeId;
            }
        }

        // Default to dungeon
        return "dungeon";
    }

    // ===== Private Methods =====

    private RoomTemplate? SelectTemplate(string biomeId, int depth, Random random)
    {
        var validTemplates = _templateConfig.Templates.Values
            .Where(t => t.IsValidForBiome(biomeId))
            .Where(t => t.IsValidForDepth(depth))
            .ToList();

        if (validTemplates.Count == 0) return null;

        // Weighted random selection
        var totalWeight = validTemplates.Sum(t => t.Weight);
        if (totalWeight <= 0) return validTemplates.First();

        var roll = random.Next(totalWeight);
        var cumulative = 0;

        foreach (var template in validTemplates)
        {
            cumulative += template.Weight;
            if (roll < cumulative)
            {
                return template;
            }
        }

        return validTemplates.Last();
    }

    private static RoomTemplate CreateFallbackTemplate(string biome) =>
        new(
            templateId: "fallback",
            namePattern: "Dark Chamber",
            descriptionPattern: "A room shrouded in darkness. You can barely make out the walls.",
            validBiomes: [biome, "dungeon", "cave", "volcanic"],
            roomType: RoomType.Standard,
            slots: [],
            weight: 1,
            minDepth: 0,
            maxDepth: null,
            tags: ["fallback"]);

    private EnvironmentContext CreateEnvironmentContext(string biomeId, RoomTemplate template)
    {
        var categoryValues = new Dictionary<string, string>
        {
            ["biome"] = biomeId
        };

        // Get biome defaults
        if (_biomeConfig.Biomes.TryGetValue(biomeId, out var biome))
        {
            foreach (var kvp in biome.DefaultCategoryValues)
            {
                categoryValues[kvp.Key] = kvp.Value;
            }
        }

        // Combine biome and template tags
        var tags = new List<string>();
        if (_biomeConfig.Biomes.TryGetValue(biomeId, out var biomeDef))
        {
            tags.AddRange(biomeDef.ImpliedTags);
        }
        tags.AddRange(template.Tags);

        return new EnvironmentContext(categoryValues, tags);
    }
}
