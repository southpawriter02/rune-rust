// ═══════════════════════════════════════════════════════════════════════════════
// EclipseZone.cs
// Immutable value object managing the Eclipse zone of darkness created by the
// Myrk-gengr Capstone ability. Tracks zone geometry, duration, caster benefits,
// and enemy effects.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an active Eclipse zone of total darkness.
/// </summary>
/// <remarks>
/// <para>
/// Eclipse creates an 8-space radius zone of impenetrable darkness centered on
/// the caster's position at time of casting. All non-magical light within the
/// zone is automatically extinguished; magical lights receive a DC 16 save.
/// </para>
/// <para><strong>Caster Benefits:</strong></para>
/// <list type="bullet">
///   <item><description>50% concealment (miss chance) against attacks</description></item>
///   <item><description>+10 Shadow Essence regeneration per turn</description></item>
/// </list>
/// <para><strong>Enemy Effects:</strong></para>
/// <list type="bullet">
///   <item><description>Blinded condition (disadvantage on attacks)</description></item>
///   <item><description>-4 penalty to Perception checks</description></item>
///   <item><description>Half movement speed</description></item>
/// </list>
/// <para>
/// The zone always applies +2 Corruption when created. It lasts 3 turns and
/// is usable once per combat.
/// </para>
/// <example>
/// <code>
/// var zone = EclipseZone.Create(casterId, centerX: 10, centerY: 15);
/// // zone.IsActive = true, Radius = 8, RemainingTurns = 3
///
/// bool inZone = zone.IsPositionInZone(12, 17);
/// // inZone = true (within 8-space radius)
///
/// var ticked = zone.TickDown();
/// // ticked.RemainingTurns = 2
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="Enums.MyrkgengrAbilityId"/>
/// <seealso cref="Enums.LightSaveResult"/>
public sealed record EclipseZone
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Duration of the Eclipse zone in turns.</summary>
    public const int BaseDuration = 3;

    /// <summary>Radius of the Eclipse zone in spaces.</summary>
    public const int DefaultRadius = 8;

    /// <summary>Shadow Essence regenerated per turn within the zone.</summary>
    public const int EssenceRegenPerTurn = 10;

    /// <summary>Concealment chance (percentage) for the caster.</summary>
    public const int ConcealmentChance = 50;

    /// <summary>Corruption always applied when Eclipse is created.</summary>
    public const int MandatoryCorruption = 2;

    /// <summary>DC for magical light sources to resist extinguishing.</summary>
    public const int LightSaveDC = 16;

    /// <summary>Perception penalty for enemies inside the zone.</summary>
    public const int EnemyPerceptionPenalty = -4;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ID of the caster who created the zone.
    /// </summary>
    public Guid CasterId { get; private init; }

    /// <summary>
    /// X coordinate of the zone center.
    /// </summary>
    public int CenterX { get; private init; }

    /// <summary>
    /// Y coordinate of the zone center.
    /// </summary>
    public int CenterY { get; private init; }

    /// <summary>
    /// Radius of the zone in spaces.
    /// </summary>
    public int Radius { get; private init; }

    /// <summary>
    /// Number of turns remaining for the zone.
    /// </summary>
    public int RemainingTurns { get; private init; }

    /// <summary>
    /// Whether the zone is currently active.
    /// </summary>
    public bool IsActive { get; private init; }

    /// <summary>
    /// Total corruption applied by this zone creation.
    /// </summary>
    public int CorruptionApplied { get; private init; }

    /// <summary>
    /// When the zone was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new active Eclipse zone centered at the specified position.
    /// </summary>
    /// <param name="casterId">ID of the caster.</param>
    /// <param name="centerX">X coordinate for the zone center.</param>
    /// <param name="centerY">Y coordinate for the zone center.</param>
    /// <returns>A new active Eclipse zone with default radius and duration.</returns>
    public static EclipseZone Create(Guid casterId, int centerX, int centerY)
    {
        return new EclipseZone
        {
            CasterId = casterId,
            CenterX = centerX,
            CenterY = centerY,
            Radius = DefaultRadius,
            RemainingTurns = BaseDuration,
            IsActive = true,
            CorruptionApplied = MandatoryCorruption,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // State Transitions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Decrements the remaining turns by one. If turns reach zero, the zone ends.
    /// </summary>
    /// <returns>A new state with decremented turns, or an inactive state if expired.</returns>
    public EclipseZone TickDown()
    {
        if (!IsActive)
            return this;

        var newTurns = RemainingTurns - 1;

        if (newTurns <= 0)
        {
            return this with
            {
                IsActive = false,
                RemainingTurns = 0
            };
        }

        return this with { RemainingTurns = newTurns };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Queries
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines whether a position is within the Eclipse zone.
    /// Uses Euclidean distance for circular zone calculation.
    /// </summary>
    /// <param name="x">X coordinate to check.</param>
    /// <param name="y">Y coordinate to check.</param>
    /// <returns><c>true</c> if the position is within the zone radius.</returns>
    public bool IsPositionInZone(int x, int y)
    {
        if (!IsActive)
            return false;

        var dx = x - CenterX;
        var dy = y - CenterY;
        var distanceSquared = (dx * dx) + (dy * dy);

        return distanceSquared <= Radius * Radius;
    }

    /// <summary>
    /// Gets the caster benefits while inside the zone.
    /// </summary>
    /// <returns>
    /// A tuple of (concealmentChance, essenceRegenPerTurn).
    /// </returns>
    public (int ConcealmentChance, int EssenceRegenPerTurn) GetCasterBenefits()
    {
        if (!IsActive)
            return (0, 0);

        return (ConcealmentChance, EssenceRegenPerTurn);
    }

    /// <summary>
    /// Gets the penalties applied to enemies inside the zone.
    /// </summary>
    /// <returns>
    /// A tuple of (perceptionPenalty, isBlinded, movementHalved).
    /// </returns>
    public (int PerceptionPenalty, bool IsBlinded, bool MovementHalved) GetEnemyPenalties()
    {
        if (!IsActive)
            return (0, false, false);

        return (EnemyPerceptionPenalty, true, true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of the Eclipse zone.
    /// </summary>
    public override string ToString() =>
        $"EclipseZone(Active={IsActive}, Center=({CenterX},{CenterY}), " +
        $"Radius={Radius}, Turns={RemainingTurns}, Corruption={CorruptionApplied})";
}
