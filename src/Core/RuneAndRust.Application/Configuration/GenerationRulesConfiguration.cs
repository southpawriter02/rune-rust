namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for procedural generation rules.
/// </summary>
public class GenerationRulesConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Maximum rooms allowed per dungeon level.
    /// </summary>
    public int MaxRoomsPerLevel { get; init; } = 50;

    /// <summary>
    /// Minimum exits per generated room.
    /// </summary>
    public int MinExitsPerRoom { get; init; } = 1;

    /// <summary>
    /// Maximum exits per generated room.
    /// </summary>
    public int MaxExitsPerRoom { get; init; } = 4;

    /// <summary>
    /// Difficulty multiplier applied per depth level.
    /// Default: 0.15 (15% increase per level).
    /// </summary>
    public float DepthDifficultyMultiplier { get; init; } = 0.15f;

    /// <summary>
    /// Loot quality multiplier applied per depth level.
    /// Default: 0.10 (10% increase per level).
    /// </summary>
    public float DepthLootQualityMultiplier { get; init; } = 0.10f;

    /// <summary>
    /// Base chance for stairs down to appear.
    /// </summary>
    public float StairsDownChanceBase { get; init; } = 0.08f;

    /// <summary>
    /// Maximum depth for dungeon generation.
    /// </summary>
    public int MaxDungeonDepth { get; init; } = 10;

    /// <summary>
    /// Biome transition depth configurations.
    /// </summary>
    public IReadOnlyDictionary<string, BiomeDepthRange> BiomeTransitionDepths { get; init; } =
        new Dictionary<string, BiomeDepthRange>();

    /// <summary>
    /// Gets the biome depth range for a specific biome.
    /// </summary>
    public BiomeDepthRange? GetBiomeRange(string biomeId) =>
        BiomeTransitionDepths.TryGetValue(biomeId, out var range) ? range : null;
}

/// <summary>
/// Defines the depth range and transition probability for a biome.
/// </summary>
public class BiomeDepthRange
{
    /// <summary>
    /// Minimum depth where this biome can appear.
    /// </summary>
    public int MinDepth { get; init; }

    /// <summary>
    /// Maximum depth for this biome (-1 = no limit).
    /// </summary>
    public int MaxDepth { get; init; } = -1;

    /// <summary>
    /// Probability (0.0 to 1.0) of transitioning to this biome.
    /// </summary>
    public float TransitionProbability { get; init; } = 1.0f;

    /// <summary>
    /// Checks if a depth is within this biome's range.
    /// </summary>
    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth >= 0 && depth > MaxDepth) return false;
        return true;
    }
}
