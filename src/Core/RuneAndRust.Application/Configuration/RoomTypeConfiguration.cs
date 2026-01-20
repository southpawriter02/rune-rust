namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for room type definitions.
/// </summary>
public class RoomTypeConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Room type definitions keyed by type ID.
    /// </summary>
    public IReadOnlyDictionary<string, RoomTypeDefinition> RoomTypes { get; init; } =
        new Dictionary<string, RoomTypeDefinition>();
}

/// <summary>
/// Defines a room type with its properties and behaviors.
/// </summary>
public class RoomTypeDefinition
{
    /// <summary>
    /// Room type identifier (matches enum name).
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name for this room type.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description shown when entering rooms of this type.
    /// </summary>
    public string EntryDescription { get; init; } = string.Empty;

    /// <summary>
    /// Short indicator shown in room header.
    /// </summary>
    public string Indicator { get; init; } = string.Empty;

    /// <summary>
    /// Monster spawn multiplier (1.0 = normal, 0 = never).
    /// </summary>
    public float MonsterSpawnMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Loot quality multiplier.
    /// </summary>
    public float LootMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Whether this room type allows resting.
    /// </summary>
    public bool AllowsRest { get; init; }

    /// <summary>
    /// Description pool for atmospheric additions.
    /// </summary>
    public IReadOnlyList<string> DescriptionPool { get; init; } = [];
}
