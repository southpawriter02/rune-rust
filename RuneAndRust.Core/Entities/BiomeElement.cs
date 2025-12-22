namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a spawnable element within a biome (enemies, hazards, terrain, loot, conditions).
/// Loaded from biome JSON data with spawn rules and weights for procedural placement.
/// </summary>
public class BiomeElement
{
    /// <summary>
    /// Gets or sets the unique identifier for this element (database primary key).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the biome identifier this element belongs to (e.g., "the_roots").
    /// Foreign key to BiomeDefinitions.
    /// </summary>
    public string BiomeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of this element.
    /// Example: "Rust-Horror", "Steam Vent", "Hidden Container"
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the element type category.
    /// Values: "DormantProcess" (enemies), "DynamicHazard", "StaticTerrain", "LootNode", "AmbientCondition"
    /// </summary>
    public string ElementType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the spawn probability weight (0.0-1.0).
    /// Higher weights increase likelihood of selection during weighted random spawning.
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// Gets or sets the spawn cost for budget-based spawning systems.
    /// Used to limit total number of expensive elements per room.
    /// </summary>
    public int SpawnCost { get; set; }

    /// <summary>
    /// Gets or sets the associated data identifier for entity lookup.
    /// References existing entity names (e.g., HazardTemplate.Name = "steam_vent", Enemy type ID, etc.).
    /// </summary>
    public string AssociatedDataId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the spawn rules for this element.
    /// Contains rule properties like NeverInEntryHall, OnlyInLargeRooms, RequiresRoomNameContains, etc.
    /// </summary>
    public ElementSpawnRules SpawnRules { get; set; } = new();
}

/// <summary>
/// Represents spawn rule constraints for a biome element.
/// All properties are optional; missing rules are treated as "no restriction".
/// </summary>
public class ElementSpawnRules
{
    /// <summary>
    /// Gets or sets whether this element cannot spawn in entry hall rooms.
    /// </summary>
    public bool? NeverInEntryHall { get; set; }

    /// <summary>
    /// Gets or sets whether this element cannot spawn in boss arena rooms.
    /// </summary>
    public bool? NeverInBossArena { get; set; }

    /// <summary>
    /// Gets or sets whether this element can only spawn in large rooms.
    /// </summary>
    public bool? OnlyInLargeRooms { get; set; }

    /// <summary>
    /// Gets or sets the required archetype for this element to spawn.
    /// Example: "BossArena" for boss-only elements.
    /// </summary>
    public string? RequiredArchetype { get; set; }

    /// <summary>
    /// Gets or sets the list of keywords the room name must contain for this element to spawn.
    /// Example: ["Geothermal", "Pumping", "Maintenance"]
    /// </summary>
    public List<string>? RequiresRoomNameContains { get; set; }

    /// <summary>
    /// Gets or sets whether this element has higher weight in secret rooms.
    /// </summary>
    public bool? HigherWeightInSecretRooms { get; set; }

    /// <summary>
    /// Gets or sets the weight multiplier for secret rooms (used when HigherWeightInSecretRooms is true).
    /// Example: 4.0 = 4x normal weight
    /// </summary>
    public float? SecretRoomWeightMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the required enemy type that must be present in the room for this element to spawn.
    /// Example: "RustHorror" (for Toxic Spore Cloud which requires Rust-Horror enemy).
    /// </summary>
    public string? RequiresEnemyType { get; set; }

    /// <summary>
    /// Gets or sets the required hazard type that must be present in the room for this element to spawn.
    /// Example: "UnstableCeiling" (for Rubble Pile which requires Unstable Ceiling hazard).
    /// </summary>
    public string? RequiresHazardType { get; set; }
}
