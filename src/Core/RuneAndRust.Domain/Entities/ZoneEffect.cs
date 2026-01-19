using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an active persistent area effect on the combat grid.
/// </summary>
/// <remarks>
/// <para>ZoneEffect is a runtime instance of a <see cref="ZoneDefinition"/>, created when
/// an ability places a zone on the combat grid. The zone occupies a set of cells determined
/// by its shape and radius, and applies effects to entities within its area each turn.</para>
/// <para>Zones are managed by IZoneEffectService (Application layer)
/// and are ticked during the turn processing phase of combat.</para>
/// <para>Key lifecycle:</para>
/// <list type="bullet">
///   <item><description>Created via <see cref="Create"/> factory method with affected cells pre-calculated</description></item>
///   <item><description>Effects applied each turn via the zone service's tick processing</description></item>
///   <item><description>Duration decremented each tick via <see cref="Tick"/></description></item>
///   <item><description>Removed when <see cref="IsExpired"/> becomes true</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Create a zone effect from a definition
/// var zone = ZoneEffect.Create(
///     definition: wallOfFireDef,
///     center: new GridPosition(5, 5),
///     casterId: playerId,
///     affectedCells: calculatedCells);
///
/// // Check if a position is in the zone
/// if (zone.ContainsPosition(enemyPosition))
/// {
///     // Apply zone effect to enemy
/// }
///
/// // Tick the zone at end of round
/// if (zone.Tick())
/// {
///     // Zone has expired, remove it
/// }
/// </code>
/// </example>
public class ZoneEffect : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this zone effect instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the zone definition ID this effect was created from.
    /// </summary>
    public string ZoneId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the zone.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the type of effect this zone applies.
    /// </summary>
    public ZoneEffectType EffectType { get; private set; }

    /// <summary>
    /// Gets the shape of the zone.
    /// </summary>
    public ZoneShape Shape { get; private set; }

    /// <summary>
    /// Gets the center position of the zone on the combat grid.
    /// </summary>
    public GridPosition Center { get; private set; }

    /// <summary>
    /// Gets the radius of the zone in cells.
    /// </summary>
    public int Radius { get; private set; }

    /// <summary>
    /// Gets the remaining turns until the zone expires.
    /// </summary>
    public int RemainingDuration { get; private set; }

    /// <summary>
    /// Gets the ID of the entity that created this zone.
    /// </summary>
    public Guid CasterId { get; private set; }

    /// <summary>
    /// Gets the damage dice notation for this zone.
    /// </summary>
    /// <remarks>
    /// <para>Copied from the zone definition at creation time.</para>
    /// <para>Null if the zone doesn't deal damage.</para>
    /// </remarks>
    public string? DamageValue { get; private set; }

    /// <summary>
    /// Gets the damage type for this zone.
    /// </summary>
    public string? DamageType { get; private set; }

    /// <summary>
    /// Gets the healing dice notation for this zone.
    /// </summary>
    /// <remarks>
    /// <para>Copied from the zone definition at creation time.</para>
    /// <para>Null if the zone doesn't heal.</para>
    /// </remarks>
    public string? HealValue { get; private set; }

    /// <summary>
    /// Gets the status effect ID this zone applies.
    /// </summary>
    public string? StatusEffectId { get; private set; }

    /// <summary>
    /// Gets the terrain modifier for this zone.
    /// </summary>
    public string? TerrainModifier { get; private set; }

    /// <summary>
    /// Gets whether this zone affects friendly entities.
    /// </summary>
    public bool AffectsFriendly { get; private set; }

    /// <summary>
    /// Gets whether this zone affects enemy entities.
    /// </summary>
    public bool AffectsEnemy { get; private set; }

    /// <summary>
    /// Gets all grid cells affected by this zone.
    /// </summary>
    /// <remarks>
    /// <para>Calculated at creation time based on shape, radius, and center position.</para>
    /// <para>Cells outside grid bounds are excluded during calculation.</para>
    /// </remarks>
    public IReadOnlyList<GridPosition> AffectedCells { get; private set; } = [];

    /// <summary>
    /// Gets the timestamp when this zone was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this zone has expired (duration reached 0).
    /// </summary>
    public bool IsExpired => RemainingDuration <= 0;

    /// <summary>
    /// Gets the number of cells this zone affects.
    /// </summary>
    public int CellCount => AffectedCells.Count;

    /// <summary>
    /// Gets whether this zone deals damage.
    /// </summary>
    public bool DealsDamage => !string.IsNullOrEmpty(DamageValue);

    /// <summary>
    /// Gets whether this zone provides healing.
    /// </summary>
    public bool ProvidesHealing => !string.IsNullOrEmpty(HealValue);

    /// <summary>
    /// Gets whether this zone applies a status effect.
    /// </summary>
    public bool AppliesStatusEffect => !string.IsNullOrEmpty(StatusEffectId);

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private ZoneEffect() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a zone effect from a definition.
    /// </summary>
    /// <param name="definition">The zone definition to create from.</param>
    /// <param name="center">The center position of the zone.</param>
    /// <param name="casterId">The ID of the entity creating the zone.</param>
    /// <param name="affectedCells">The pre-calculated affected cell positions.</param>
    /// <returns>A new ZoneEffect instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition or affectedCells is null.</exception>
    /// <remarks>
    /// <para>Affected cells should be pre-calculated by the zone service based on
    /// shape, radius, direction, and grid bounds.</para>
    /// </remarks>
    public static ZoneEffect Create(
        ZoneDefinition definition,
        GridPosition center,
        Guid casterId,
        IEnumerable<GridPosition> affectedCells)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));
        ArgumentNullException.ThrowIfNull(affectedCells, nameof(affectedCells));

        return new ZoneEffect
        {
            Id = Guid.NewGuid(),
            ZoneId = definition.ZoneId,
            Name = definition.Name,
            EffectType = definition.EffectType,
            Shape = definition.Shape,
            Center = center,
            Radius = definition.Radius,
            RemainingDuration = definition.Duration,
            CasterId = casterId,
            DamageValue = definition.DamageValue,
            DamageType = definition.DamageType,
            HealValue = definition.HealValue,
            StatusEffectId = definition.StatusEffectId,
            TerrainModifier = definition.TerrainModifier,
            AffectsFriendly = definition.AffectsFriendly,
            AffectsEnemy = definition.AffectsEnemy,
            AffectedCells = affectedCells.ToList(),
            CreatedAt = DateTime.UtcNow
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Decrements the zone's remaining duration by one turn.
    /// </summary>
    /// <returns>True if the zone has expired after this tick; otherwise, false.</returns>
    /// <remarks>
    /// <para>Called once per round during zone processing.</para>
    /// <para>Zones with remaining duration of 0 or less should be removed.</para>
    /// </remarks>
    public bool Tick()
    {
        RemainingDuration--;
        return RemainingDuration <= 0;
    }

    /// <summary>
    /// Checks if a position is within this zone's affected area.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within the zone; otherwise, false.</returns>
    public bool ContainsPosition(GridPosition position)
    {
        return AffectedCells.Contains(position);
    }

    /// <summary>
    /// Gets the count of affected cells.
    /// </summary>
    /// <returns>The number of cells in this zone.</returns>
    public int GetCellCount() => AffectedCells.Count;

    /// <summary>
    /// Extends the zone's duration by the specified number of turns.
    /// </summary>
    /// <param name="turns">Number of turns to add to the duration.</param>
    /// <remarks>
    /// <para>Used for abilities that refresh or extend zone duration.</para>
    /// </remarks>
    public void ExtendDuration(int turns)
    {
        if (turns > 0)
        {
            RemainingDuration += turns;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // OBJECT OVERRIDES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this zone effect.
    /// </summary>
    /// <returns>A string showing the name, center position, and remaining duration.</returns>
    public override string ToString() => $"{Name} at {Center} ({RemainingDuration} turns left)";
}
