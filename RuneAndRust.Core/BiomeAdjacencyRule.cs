namespace RuneAndRust.Core;

/// <summary>
/// v0.39.2: Defines compatibility level between two biomes
/// </summary>
public enum BiomeCompatibility
{
    /// <summary>
    /// Biomes can be directly adjacent with minimal or no transition rooms
    /// </summary>
    Compatible,

    /// <summary>
    /// Biomes require 1-3 transition rooms to logically blend environments
    /// </summary>
    RequiresTransition,

    /// <summary>
    /// Biomes cannot coexist in the same sector (e.g., Muspelheim + Niflheim)
    /// </summary>
    Incompatible
}

/// <summary>
/// v0.39.2: Defines adjacency rules between two biomes
/// Specifies compatibility, transition requirements, and thematic guidance
/// </summary>
public class BiomeAdjacencyRule
{
    public int AdjacencyId { get; set; }
    public string BiomeA { get; set; } = string.Empty;
    public string BiomeB { get; set; } = string.Empty;
    public BiomeCompatibility Compatibility { get; set; } = BiomeCompatibility.Compatible;
    public int MinTransitionRooms { get; set; } = 0;
    public int MaxTransitionRooms { get; set; } = 3;
    public string? TransitionTheme { get; set; } = null;
    public string? Notes { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if this rule applies to the given biome pair (order-independent)
    /// </summary>
    public bool AppliesToBiomes(string biome1, string biome2)
    {
        return (BiomeA == biome1 && BiomeB == biome2) ||
               (BiomeA == biome2 && BiomeB == biome1);
    }

    /// <summary>
    /// Gets the recommended number of transition rooms for this adjacency
    /// </summary>
    public int GetRecommendedTransitionCount(Random rng)
    {
        if (Compatibility == BiomeCompatibility.Incompatible)
            return 0;

        if (Compatibility == BiomeCompatibility.Compatible && MinTransitionRooms == 0)
            return rng.Next(0, 2); // 0-1 transition rooms

        // RequiresTransition: Use min-max range
        return rng.Next(MinTransitionRooms, MaxTransitionRooms + 1);
    }

    /// <summary>
    /// Validates the rule for logical consistency
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(BiomeA) || string.IsNullOrEmpty(BiomeB))
            return false;

        if (MinTransitionRooms < 0 || MaxTransitionRooms < MinTransitionRooms)
            return false;

        if (Compatibility == BiomeCompatibility.Incompatible && (MinTransitionRooms > 0 || MaxTransitionRooms > 0))
            return false;

        return true;
    }
}
