using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Persistence;
using Serilog;

// Resolve ambiguous RoomArchetype reference
using PopulationRoomArchetype = RuneAndRust.Core.Population.RoomArchetype;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.2: Service for generating biome transition zones
/// Creates smooth environmental transitions between biomes with gradual blending
/// </summary>
public class BiomeTransitionService
{
    private static readonly ILogger _log = Log.ForContext<BiomeTransitionService>();
    private readonly BiomeAdjacencyRepository _adjacencyRepo;
    private readonly BiomeBlendingService _blendingService;

    public BiomeTransitionService(
        BiomeAdjacencyRepository adjacencyRepository,
        BiomeBlendingService blendingService)
    {
        _adjacencyRepo = adjacencyRepository;
        _blendingService = blendingService;
        _log.Information("BiomeTransitionService initialized");
    }

    /// <summary>
    /// Generates a transition zone between two biomes
    /// </summary>
    /// <param name="fromBiome">Source biome</param>
    /// <param name="toBiome">Destination biome</param>
    /// <param name="transitionRoomCount">Number of transition rooms to generate</param>
    /// <param name="rng">Random number generator</param>
    /// <returns>List of transition rooms with progressive blend ratios</returns>
    /// <exception cref="InvalidOperationException">Thrown if biomes are incompatible</exception>
    public List<Room> GenerateTransitionZone(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        int transitionRoomCount,
        Random rng)
    {
        _log.Information(
            "Generating transition: {FromBiome} → {ToBiome}, Rooms={Count}",
            fromBiome.BiomeId, toBiome.BiomeId, transitionRoomCount);

        // Get adjacency rule
        var adjacencyRule = _adjacencyRepo.GetRule(fromBiome.BiomeId, toBiome.BiomeId);

        if (adjacencyRule == null)
        {
            _log.Warning("No adjacency rule found for {BiomeA} <-> {BiomeB}, assuming compatible",
                fromBiome.BiomeId, toBiome.BiomeId);
            adjacencyRule = CreateDefaultCompatibleRule(fromBiome.BiomeId, toBiome.BiomeId);
        }

        // Check compatibility
        if (adjacencyRule.Compatibility == BiomeCompatibility.Incompatible)
        {
            _log.Error("Attempted to create transition between incompatible biomes: {BiomeA} <-> {BiomeB}",
                fromBiome.BiomeId, toBiome.BiomeId);
            throw new InvalidOperationException(
                $"Biomes {fromBiome.BiomeId} and {toBiome.BiomeId} cannot be adjacent. " +
                $"They are marked as incompatible in the adjacency matrix.");
        }

        // Validate transition room count
        if (transitionRoomCount < adjacencyRule.MinTransitionRooms)
        {
            _log.Warning("Transition room count {Count} is less than minimum {Min}, adjusting",
                transitionRoomCount, adjacencyRule.MinTransitionRooms);
            transitionRoomCount = adjacencyRule.MinTransitionRooms;
        }

        if (transitionRoomCount > adjacencyRule.MaxTransitionRooms)
        {
            _log.Warning("Transition room count {Count} exceeds maximum {Max}, adjusting",
                transitionRoomCount, adjacencyRule.MaxTransitionRooms);
            transitionRoomCount = adjacencyRule.MaxTransitionRooms;
        }

        // Generate transition rooms
        var transitionRooms = new List<Room>();

        for (int i = 0; i < transitionRoomCount; i++)
        {
            // Calculate blend ratio (linear interpolation)
            var progress = (float)(i + 1) / (transitionRoomCount + 1);
            var fromWeight = 1.0f - progress;
            var toWeight = progress;

            _log.Debug(
                "Transition room {Index}/{Total}: {FromWeight:P0} {FromBiome}, {ToWeight:P0} {ToBiome}",
                i + 1, transitionRoomCount,
                fromWeight, fromBiome.BiomeId,
                toWeight, toBiome.BiomeId);

            var room = GenerateBlendedRoom(
                fromBiome,
                toBiome,
                fromWeight,
                toWeight,
                adjacencyRule.TransitionTheme ?? "Environmental transition",
                i,
                rng);

            transitionRooms.Add(room);
        }

        _log.Information("Generated {Count} transition rooms from {FromBiome} to {ToBiome}",
            transitionRooms.Count, fromBiome.BiomeId, toBiome.BiomeId);

        return transitionRooms;
    }

    /// <summary>
    /// Generates a single blended room combining elements from two biomes
    /// </summary>
    private Room GenerateBlendedRoom(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        float fromWeight,
        float toWeight,
        string transitionTheme,
        int transitionIndex,
        Random rng)
    {
        var room = new Room
        {
            RoomId = GenerateRoomId(fromBiome.BiomeId, toBiome.BiomeId, transitionIndex),
            PrimaryBiome = fromBiome.BiomeId,
            SecondaryBiome = toBiome.BiomeId,
            BiomeBlendRatio = toWeight,  // 0.0 = all fromBiome, 1.0 = all toBiome
            Archetype = PopulationRoomArchetype.Chamber,  // Transitions are typically chambers
            IsProcedurallyGenerated = true,
            GeneratedNodeType = NodeType.Main
        };

        // Blend descriptors using BiomeBlendingService
        var blendedDescriptor = _blendingService.BlendBiomeDescriptors(
            fromBiome,
            toBiome,
            fromWeight,
            toWeight,
            rng);

        room.Name = blendedDescriptor.Name;
        room.Description = blendedDescriptor.Description;

        // Add transition theme note to description
        if (!string.IsNullOrEmpty(transitionTheme))
        {
            room.Description += $" {GetTransitionFlavorText(transitionTheme, fromWeight, toWeight)}";
        }

        _log.Debug("Generated blended room: {RoomName} ({Blend:P0} → {ToBiome})",
            room.Name, toWeight, toBiome.BiomeId);

        return room;
    }

    /// <summary>
    /// Gets flavor text for the transition based on theme and blend ratio
    /// </summary>
    private string GetTransitionFlavorText(string theme, float fromWeight, float toWeight)
    {
        var blendCategory = toWeight switch
        {
            < 0.3f => "early",
            < 0.7f => "mid",
            _ => "late"
        };

        return theme.ToLower() switch
        {
            var t when t.Contains("geothermal") || t.Contains("heat") => blendCategory switch
            {
                "early" => "Geothermal activity increases subtly. The air begins to warm.",
                "mid" => "Heat intensifies noticeably. Condensation turns to steam.",
                _ => "Volcanic heat dominates. Metal surfaces glow faintly."
            },
            var t when t.Contains("cooling") || t.Contains("cold") => blendCategory switch
            {
                "early" => "Temperature drops gradually. Frost forms in shadowed corners.",
                "mid" => "Cold penetrates deeper. Water begins to freeze.",
                _ => "Extreme cold dominates. Ice encases every surface."
            },
            var t when t.Contains("aetheric") => blendCategory switch
            {
                "early" => "Faint Aetheric resonance tingles at the edge of perception.",
                "mid" => "Aetheric energy becomes palpable. Reality feels malleable.",
                _ => "Aetheric saturation warps space and light visibly."
            },
            var t when t.Contains("scale") => blendCategory switch
            {
                "early" => "Architecture grows noticeably larger. Doorways tower overhead.",
                "mid" => "Colossal proportions dominate. Everything dwarfs human scale.",
                _ => "Built for giants. The scale is overwhelming."
            },
            _ => blendCategory switch
            {
                "early" => "Subtle environmental changes suggest a transition.",
                "mid" => "The environmental shift becomes unmistakable.",
                _ => "The new biome clearly dominates the surroundings."
            }
        };
    }

    /// <summary>
    /// Generates a unique room ID for transition rooms
    /// </summary>
    private string GenerateRoomId(string fromBiome, string toBiome, int index)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        return $"transition_{fromBiome}_{toBiome}_{index}_{timestamp}";
    }

    /// <summary>
    /// Creates a default compatible rule for biome pairs without explicit rules
    /// </summary>
    private BiomeAdjacencyRule CreateDefaultCompatibleRule(string biomeA, string biomeB)
    {
        return new BiomeAdjacencyRule
        {
            BiomeA = biomeA,
            BiomeB = biomeB,
            Compatibility = BiomeCompatibility.Compatible,
            MinTransitionRooms = 0,
            MaxTransitionRooms = 1,
            TransitionTheme = "Standard transition",
            Notes = "Auto-generated default rule"
        };
    }

    /// <summary>
    /// Determines optimal transition count for a biome pair
    /// </summary>
    public int GetOptimalTransitionCount(string fromBiome, string toBiome, Random rng)
    {
        var rule = _adjacencyRepo.GetRule(fromBiome, toBiome);

        if (rule == null)
        {
            _log.Debug("No rule found for {BiomeA} <-> {BiomeB}, using default 1 transition room",
                fromBiome, toBiome);
            return 1;
        }

        return rule.GetRecommendedTransitionCount(rng);
    }

    /// <summary>
    /// Checks if two biomes can be adjacent
    /// </summary>
    public bool CanBiomesBeAdjacent(string biomeA, string biomeB)
    {
        var rule = _adjacencyRepo.GetRule(biomeA, biomeB);

        if (rule == null)
        {
            _log.Debug("No rule found for {BiomeA} <-> {BiomeB}, assuming compatible", biomeA, biomeB);
            return true;
        }

        return rule.Compatibility != BiomeCompatibility.Incompatible;
    }

    /// <summary>
    /// Gets the transition theme for a biome pair
    /// </summary>
    public string? GetTransitionTheme(string biomeA, string biomeB)
    {
        var rule = _adjacencyRepo.GetRule(biomeA, biomeB);
        return rule?.TransitionTheme;
    }
}

/// <summary>
/// v0.39.2: Result of descriptor blending
/// Contains blended name, description, and sensory details
/// </summary>
public class BlendedDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Adjectives { get; set; } = new();
    public List<string> Sounds { get; set; } = new();
    public List<string> Smells { get; set; } = new();
    public Dictionary<string, string> Details { get; set; } = new();
}
