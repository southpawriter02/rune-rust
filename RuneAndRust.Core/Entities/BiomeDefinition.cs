namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a biome configuration loaded from JSON data.
/// Defines available templates, descriptor categories, and generation parameters.
/// </summary>
public class BiomeDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for this biome (database primary key).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the biome identifier (e.g., "the_roots").
    /// Must be unique across all biomes.
    /// </summary>
    public string BiomeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this biome (e.g., "[The Roots]").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the narrative description of this biome.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of available template IDs for this biome.
    /// Example: ["collapsed_entry_hall", "maintenance_access", "reactor_core"]
    /// </summary>
    public List<string> AvailableTemplates { get; set; } = new();

    /// <summary>
    /// Gets or sets the descriptor categories for variable substitution and ambiance.
    /// Contains nested arrays for Adjectives, Details, Sounds, Smells.
    /// </summary>
    public BiomeDescriptorCategories DescriptorCategories { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum number of rooms to generate for this biome.
    /// </summary>
    public int MinRoomCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of rooms to generate for this biome.
    /// </summary>
    public int MaxRoomCount { get; set; }

    /// <summary>
    /// Gets or sets the probability (0.0-1.0) of generating branching paths.
    /// </summary>
    public float BranchingProbability { get; set; }

    /// <summary>
    /// Gets or sets the probability (0.0-1.0) of generating secret rooms.
    /// </summary>
    public float SecretRoomProbability { get; set; }
}

/// <summary>
/// Represents nested descriptor categories within a biome definition.
/// Used for variable substitution and atmospheric text generation.
/// </summary>
public class BiomeDescriptorCategories
{
    /// <summary>
    /// Gets or sets the list of adjectives for {Adjective} token substitution.
    /// Example: ["Corroded", "Decaying", "Twisted"]
    /// </summary>
    public List<string> Adjectives { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of detail sentences for {Detail} token substitution.
    /// Example: ["Runic glyphs flicker weakly on the walls"]
    /// </summary>
    public List<string> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of sound descriptions for atmospheric text.
    /// Example: ["hissing steam escaping from fractured pipes"]
    /// </summary>
    public List<string> Sounds { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of smell descriptions for atmospheric text.
    /// Example: ["ozone from arcing power conduits", "metallic tang of rust"]
    /// </summary>
    public List<string> Smells { get; set; } = new();
}
