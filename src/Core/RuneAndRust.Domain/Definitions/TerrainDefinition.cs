using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration for a type of terrain loaded from JSON.
/// </summary>
/// <remarks>
/// <para>
/// Terrain definitions provide data-driven configuration for grid cell terrain.
/// Each definition specifies movement costs, hazard damage, display properties,
/// and other terrain-specific behaviors.
/// </para>
/// <para>
/// Definitions are immutable after creation and loaded from <c>config/terrain.json</c>.
/// </para>
/// </remarks>
public class TerrainDefinition
{
    /// <summary>
    /// Gets the unique identifier for this terrain type.
    /// </summary>
    /// <remarks>
    /// IDs are normalized to lowercase (e.g., "rubble", "fire", "acid-pool").
    /// </remarks>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name shown to players.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the terrain type category.
    /// </summary>
    public TerrainType Type { get; private set; } = TerrainType.Normal;

    /// <summary>
    /// Gets the movement cost multiplier (1.0 = normal, 2.0 = double).
    /// </summary>
    /// <remarks>
    /// Applied to base movement costs. Impassable terrain ignores this value.
    /// </remarks>
    public float MovementCostMultiplier { get; private set; } = 1.0f;

    /// <summary>
    /// Gets the damage dice expression for hazardous terrain.
    /// </summary>
    /// <remarks>
    /// Uses standard dice notation (e.g., "1d6", "2d4"). Null for non-hazardous terrain.
    /// </remarks>
    public string? DamageOnEntry { get; private set; }

    /// <summary>
    /// Gets the damage type for hazardous terrain.
    /// </summary>
    /// <remarks>
    /// References a damage type ID from <c>config/damage-types.json</c>.
    /// Examples: "fire", "acid", "piercing"
    /// </remarks>
    public string? DamageType { get; private set; }

    /// <summary>
    /// Gets whether this terrain blocks line of sight.
    /// </summary>
    /// <remarks>
    /// Walls and tall obstacles block LOS. Most terrain does not.
    /// </remarks>
    public bool BlocksLOS { get; private set; }

    /// <summary>
    /// Gets the ASCII display character for grid rendering.
    /// </summary>
    public char DisplayChar { get; private set; } = '.';

    /// <summary>
    /// Gets the description for legends and tooltips.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private TerrainDefinition() { }

    /// <summary>
    /// Creates a new terrain definition with the specified properties.
    /// </summary>
    /// <param name="id">Unique identifier (normalized to lowercase).</param>
    /// <param name="name">Display name.</param>
    /// <param name="type">Terrain type category.</param>
    /// <param name="movementCostMultiplier">Movement cost multiplier (default: 1.0).</param>
    /// <param name="damageOnEntry">Damage dice for hazardous terrain (optional).</param>
    /// <param name="damageType">Damage type ID (optional).</param>
    /// <param name="blocksLOS">Whether terrain blocks line of sight (default: false).</param>
    /// <param name="displayChar">ASCII display character (default: '.').</param>
    /// <param name="description">Description for legends (default: empty).</param>
    /// <returns>A new <see cref="TerrainDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or whitespace.</exception>
    public static TerrainDefinition Create(
        string id,
        string name,
        TerrainType type,
        float movementCostMultiplier = 1.0f,
        string? damageOnEntry = null,
        string? damageType = null,
        bool blocksLOS = false,
        char displayChar = '.',
        string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return new TerrainDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Type = type,
            MovementCostMultiplier = movementCostMultiplier,
            DamageOnEntry = damageOnEntry,
            DamageType = damageType,
            BlocksLOS = blocksLOS,
            DisplayChar = displayChar,
            Description = description
        };
    }

    /// <summary>
    /// Gets whether this terrain is passable by entities.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> for Normal, Difficult, and Hazardous terrain.
    /// Returns <c>false</c> for Impassable terrain.
    /// </remarks>
    public bool IsPassable => Type != TerrainType.Impassable;

    /// <summary>
    /// Gets whether this terrain deals damage on entry.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> only for Hazardous terrain with a valid damage expression.
    /// </remarks>
    public bool DealsDamage => Type == TerrainType.Hazardous && !string.IsNullOrEmpty(DamageOnEntry);
}
