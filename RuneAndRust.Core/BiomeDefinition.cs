namespace RuneAndRust.Core;

/// <summary>
/// Defines a biome for procedural generation (v0.10)
/// Acts as a style guide for the Dynamic Room Engine
/// </summary>
public class BiomeDefinition
{
    // Identity
    public string BiomeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Template Pool (v0.10)
    public List<string> AvailableTemplates { get; set; } = new();

    // Aesthetic Properties for Description Generation (v0.10)
    public Dictionary<string, List<string>> DescriptorCategories { get; set; } = new();
    // Examples:
    // "Adjectives": ["Corroded", "Decaying", "Twisted"]
    // "Details": ["Runic glyphs flicker weakly", "Condensation drips from pipes"]
    // "Sounds": ["hissing steam", "groaning metal"]
    // "Smells": ["ozone", "rust", "decay"]

    // Generation Parameters (v0.10)
    public int MinRoomCount { get; set; } = 5;
    public int MaxRoomCount { get; set; } = 7;
    public float BranchingProbability { get; set; } = 0.4f; // 40% chance per eligible node
    public float SecretRoomProbability { get; set; } = 0.2f; // 20% chance

    // v0.11+ Properties (prepared for future use)
    public BiomeElementTable? Elements { get; set; } = null;

    /// <summary>
    /// Gets a random adjective from this biome
    /// </summary>
    public string? GetRandomAdjective(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Adjectives", out var adjectives) || adjectives.Count == 0)
            return null;

        return adjectives[rng.Next(adjectives.Count)];
    }

    /// <summary>
    /// Gets a random detail from this biome
    /// </summary>
    public string? GetRandomDetail(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Details", out var details) || details.Count == 0)
            return null;

        return details[rng.Next(details.Count)];
    }

    /// <summary>
    /// Gets a random ambient sound from this biome
    /// </summary>
    public string? GetRandomSound(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Sounds", out var sounds) || sounds.Count == 0)
            return null;

        return sounds[rng.Next(sounds.Count)];
    }

    /// <summary>
    /// Validates that the biome has sufficient data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(BiomeId)) return false;
        if (string.IsNullOrEmpty(Name)) return false;
        if (AvailableTemplates.Count == 0) return false;
        if (MinRoomCount < 3) return false;
        if (MaxRoomCount < MinRoomCount) return false;

        return true;
    }
}

/// <summary>
/// Biome element table for v0.11+ (enemy/hazard/loot population)
/// Currently a placeholder for future implementation
/// </summary>
public class BiomeElementTable
{
    public List<BiomeElement> Elements { get; set; } = new();
}

/// <summary>
/// Individual element that can appear in a biome (v0.11+)
/// </summary>
public class BiomeElement
{
    public string ElementName { get; set; } = string.Empty;
    public BiomeElementType ElementType { get; set; } = BiomeElementType.DescriptionDetail;
    public float Weight { get; set; } = 1.0f;
    public string? AssociatedDataId { get; set; } = null;
}

/// <summary>
/// Types of elements that can appear in biomes (v0.10 - v0.12)
/// </summary>
public enum BiomeElementType
{
    // v0.10 (used now)
    RoomTemplate,
    DescriptionDetail,
    AmbientCondition,

    // v0.11 (population phase)
    DormantProcess,      // Enemy spawn
    DynamicHazard,       // Environmental dangers
    StaticTerrain,       // Cover, chasms, etc.
    LootNode,            // Resource veins, containers

    // v0.12 (polish phase)
    CoherentGlitchRule   // Environmental storytelling rules
}
