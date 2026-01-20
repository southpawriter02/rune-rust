using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a zone effect template loaded from configuration.
/// </summary>
/// <remarks>
/// <para>ZoneDefinition is an immutable domain entity that describes a persistent area
/// effect that can be created during combat. Zones occupy a shape of cells on the
/// combat grid and apply effects to entities within their area each turn.</para>
/// <para>Zone definitions are loaded from JSON configuration (zones.json) and used
/// by IZoneEffectService (Application layer) to create active zone instances.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description><see cref="ZoneId"/> - Unique identifier for configuration and tracking</description></item>
///   <item><description><see cref="EffectType"/> - Type of effect (Damage, Healing, Buff, etc.)</description></item>
///   <item><description><see cref="Shape"/> - Shape of the zone area (Circle, Square, Line, etc.)</description></item>
///   <item><description><see cref="Radius"/> - Size of the zone in cells</description></item>
///   <item><description><see cref="Duration"/> - How many turns the zone persists</description></item>
///   <item><description><see cref="AffectsFriendly"/>/<see cref="AffectsEnemy"/> - Targeting rules</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Create a damage zone definition
/// var wallOfFire = ZoneDefinition.Create(
///     zoneId: "wall-of-fire",
///     name: "Wall of Fire",
///     description: "A blazing wall that burns enemies",
///     effectType: ZoneEffectType.Damage,
///     shape: ZoneShape.Line,
///     radius: 4,
///     duration: 5,
///     affectsFriendly: false,
///     affectsEnemy: true)
///     .WithDamage("2d6", "fire");
/// </code>
/// </example>
public class ZoneDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this zone definition instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this zone type (e.g., "wall-of-fire", "healing-circle").
    /// </summary>
    /// <remarks>
    /// <para>Used for configuration loading and zone creation.</para>
    /// <para>Always stored in lowercase.</para>
    /// </remarks>
    public string ZoneId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the zone (e.g., "Wall of Fire").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the detailed description of the zone's effect.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the type of effect this zone applies.
    /// </summary>
    /// <remarks>
    /// <para>Determines which properties are used (DamageValue, HealValue, StatusEffectId, etc.).</para>
    /// <para><see cref="ZoneEffectType.Mixed"/> uses multiple properties.</para>
    /// </remarks>
    public ZoneEffectType EffectType { get; private set; }

    /// <summary>
    /// Gets the shape of the zone on the combat grid.
    /// </summary>
    /// <remarks>
    /// <para><see cref="ZoneShape.Line"/> and <see cref="ZoneShape.Cone"/> require a direction
    /// parameter when the zone is created.</para>
    /// </remarks>
    public ZoneShape Shape { get; private set; }

    /// <summary>
    /// Gets the radius of the zone in cells.
    /// </summary>
    /// <remarks>
    /// <para>Interpretation varies by shape:</para>
    /// <list type="bullet">
    ///   <item><description>Circle: Euclidean distance from center</description></item>
    ///   <item><description>Square: Half-width of the square</description></item>
    ///   <item><description>Line: Length of the line from center</description></item>
    ///   <item><description>Cone: Maximum extent from center</description></item>
    ///   <item><description>Ring: Outer radius of the ring</description></item>
    /// </list>
    /// </remarks>
    public int Radius { get; private set; }

    /// <summary>
    /// Gets the duration of the zone in turns.
    /// </summary>
    /// <remarks>
    /// <para>Zone decrements duration each tick and is removed when duration reaches 0.</para>
    /// </remarks>
    public int Duration { get; private set; }

    /// <summary>
    /// Gets the damage dice notation for Damage and Mixed type zones.
    /// </summary>
    /// <remarks>
    /// <para>Standard dice notation (e.g., "2d6", "1d8+2").</para>
    /// <para>Null for non-damage zones.</para>
    /// </remarks>
    public string? DamageValue { get; private set; }

    /// <summary>
    /// Gets the damage type for damage zones.
    /// </summary>
    /// <remarks>
    /// <para>Common types: fire, cold, lightning, acid, poison, radiant, necrotic, physical.</para>
    /// <para>Null for non-damage zones.</para>
    /// </remarks>
    public string? DamageType { get; private set; }

    /// <summary>
    /// Gets the healing dice notation for Healing and Mixed type zones.
    /// </summary>
    /// <remarks>
    /// <para>Standard dice notation (e.g., "1d6+2", "2d4").</para>
    /// <para>Null for non-healing zones.</para>
    /// </remarks>
    public string? HealValue { get; private set; }

    /// <summary>
    /// Gets the status effect ID for Buff, Debuff, Terrain, and Mixed type zones.
    /// </summary>
    /// <remarks>
    /// <para>References a status effect definition (e.g., "slowed", "entangled").</para>
    /// <para>Null if the zone doesn't apply status effects.</para>
    /// </remarks>
    public string? StatusEffectId { get; private set; }

    /// <summary>
    /// Gets the terrain modifier for Terrain type zones.
    /// </summary>
    /// <remarks>
    /// <para>Modifiers: DifficultTerrain, BlockingTerrain, DangerousTerrain.</para>
    /// <para>Null for non-terrain zones.</para>
    /// </remarks>
    public string? TerrainModifier { get; private set; }

    /// <summary>
    /// Gets whether this zone affects friendly entities (allies of the caster).
    /// </summary>
    public bool AffectsFriendly { get; private set; }

    /// <summary>
    /// Gets whether this zone affects enemy entities (opponents of the caster).
    /// </summary>
    public bool AffectsEnemy { get; private set; }

    /// <summary>
    /// Gets monster types specifically affected by this zone.
    /// </summary>
    /// <remarks>
    /// <para>Empty list means all applicable targets are affected.</para>
    /// <para>Used for type-specific zones like Consecration affecting undead.</para>
    /// </remarks>
    public IReadOnlyList<string> MonsterTypes { get; private set; } = [];

    /// <summary>
    /// Gets the path to the zone's icon asset.
    /// </summary>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core and serialization.
    /// </summary>
    private ZoneDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new zone definition with the specified core properties.
    /// </summary>
    /// <param name="zoneId">Unique string identifier (will be lowercased).</param>
    /// <param name="name">Display name of the zone.</param>
    /// <param name="description">Description of the zone's effect.</param>
    /// <param name="effectType">Type of effect the zone applies.</param>
    /// <param name="shape">Shape of the zone area.</param>
    /// <param name="radius">Radius of the zone in cells (must be positive).</param>
    /// <param name="duration">Duration in turns (must be positive).</param>
    /// <param name="affectsFriendly">Whether the zone affects friendly entities.</param>
    /// <param name="affectsEnemy">Whether the zone affects enemy entities.</param>
    /// <returns>A new ZoneDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when zoneId or name is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when radius or duration is not positive.</exception>
    public static ZoneDefinition Create(
        string zoneId,
        string name,
        string description,
        ZoneEffectType effectType,
        ZoneShape shape,
        int radius,
        int duration,
        bool affectsFriendly,
        bool affectsEnemy)
    {
        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId, nameof(zoneId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius, nameof(radius));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration, nameof(duration));

        return new ZoneDefinition
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            EffectType = effectType,
            Shape = shape,
            Radius = radius,
            Duration = duration,
            AffectsFriendly = affectsFriendly,
            AffectsEnemy = affectsEnemy
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures damage properties for the zone.
    /// </summary>
    /// <param name="damageValue">Damage dice notation (e.g., "2d6").</param>
    /// <param name="damageType">Type of damage (e.g., "fire", "cold").</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithDamage(string damageValue, string damageType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(damageValue, nameof(damageValue));
        ArgumentException.ThrowIfNullOrWhiteSpace(damageType, nameof(damageType));

        DamageValue = damageValue;
        DamageType = damageType.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Configures healing properties for the zone.
    /// </summary>
    /// <param name="healValue">Healing dice notation (e.g., "1d6+2").</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithHealing(string healValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(healValue, nameof(healValue));

        HealValue = healValue;
        return this;
    }

    /// <summary>
    /// Configures status effect for the zone.
    /// </summary>
    /// <param name="statusEffectId">Status effect definition ID.</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithStatusEffect(string statusEffectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(statusEffectId, nameof(statusEffectId));

        StatusEffectId = statusEffectId.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Configures terrain modifier for the zone.
    /// </summary>
    /// <param name="terrainModifier">Terrain modifier type.</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithTerrainModifier(string terrainModifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(terrainModifier, nameof(terrainModifier));

        TerrainModifier = terrainModifier;
        return this;
    }

    /// <summary>
    /// Configures monster type restrictions for the zone.
    /// </summary>
    /// <param name="monsterTypes">Monster types affected by this zone.</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithMonsterTypes(IEnumerable<string> monsterTypes)
    {
        MonsterTypes = monsterTypes?.Select(t => t.ToLowerInvariant()).ToList() ?? [];
        return this;
    }

    /// <summary>
    /// Configures the icon path for the zone.
    /// </summary>
    /// <param name="iconPath">Path to the icon asset.</param>
    /// <returns>This instance for method chaining.</returns>
    public ZoneDefinition WithIcon(string iconPath)
    {
        IconPath = iconPath;
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this zone deals damage.
    /// </summary>
    /// <returns>True if the zone has damage configured.</returns>
    public bool DealsDamage() => !string.IsNullOrEmpty(DamageValue);

    /// <summary>
    /// Checks if this zone provides healing.
    /// </summary>
    /// <returns>True if the zone has healing configured.</returns>
    public bool ProvidesHealing() => !string.IsNullOrEmpty(HealValue);

    /// <summary>
    /// Checks if this zone applies a status effect.
    /// </summary>
    /// <returns>True if the zone has a status effect configured.</returns>
    public bool AppliesStatusEffect() => !string.IsNullOrEmpty(StatusEffectId);

    /// <summary>
    /// Checks if this zone modifies terrain.
    /// </summary>
    /// <returns>True if the zone has terrain modification configured.</returns>
    public bool ModifiesTerrain() => !string.IsNullOrEmpty(TerrainModifier);

    /// <summary>
    /// Checks if this zone targets specific monster types.
    /// </summary>
    /// <returns>True if monster type restrictions exist.</returns>
    public bool HasMonsterTypeRestriction() => MonsterTypes.Any();

    /// <summary>
    /// Checks if a specific monster type is affected by this zone.
    /// </summary>
    /// <param name="monsterType">The monster type to check.</param>
    /// <returns>True if the monster type is affected.</returns>
    public bool AffectsMonsterType(string monsterType)
    {
        // No restriction means all types affected
        if (!MonsterTypes.Any()) return true;

        return MonsterTypes.Any(t => t.Equals(monsterType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this zone requires a direction (for Line or Cone shapes).
    /// </summary>
    /// <returns>True if the zone requires a direction parameter.</returns>
    public bool RequiresDirection() => Shape == ZoneShape.Line || Shape == ZoneShape.Cone;

    // ═══════════════════════════════════════════════════════════════
    // OBJECT OVERRIDES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this zone definition.
    /// </summary>
    /// <returns>A string showing the name, ID, shape, and radius.</returns>
    public override string ToString() => $"{Name} ({ZoneId}): {Shape} r{Radius}";
}
