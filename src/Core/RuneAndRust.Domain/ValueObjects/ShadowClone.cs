// ═══════════════════════════════════════════════════════════════════════════════
// ShadowClone.cs
// Immutable value object representing an active Myrk-gengr shadow clone
// with behavior, lifecycle state, and spatial data.
// Version: 0.20.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a shadow clone created by the Myrk-gengr Shadow Clone ability.
/// </summary>
/// <remarks>
/// <para>
/// Shadow clones are illusory duplicates with 1 HP that last up to 1 minute.
/// They can be destroyed by any damage, consumed by Umbral Strike for advantage,
/// or expire after their duration elapses. Maximum 2 clones may be active
/// simultaneously.
/// </para>
/// <para>Clone lifecycle states:</para>
/// <list type="bullet">
///   <item><description><b>Active:</b> Clone exists on the battlefield</description></item>
///   <item><description><b>Destroyed:</b> Clone took damage (1 HP)</description></item>
///   <item><description><b>Consumed:</b> Clone was consumed by Umbral Strike for advantage</description></item>
///   <item><description><b>Expired:</b> Clone duration elapsed (60 seconds)</description></item>
/// </list>
/// <example>
/// <code>
/// var clone = ShadowClone.Create(ownerId, 15, 20, CloneBehavior.Mirror);
/// // clone.IsActive() == true, clone.HitPoints == 1
///
/// var destroyed = clone.Destroy();
/// // destroyed.IsActive() == false, destroyed.HitPoints == 0
///
/// var consumed = clone.Consume();
/// // consumed.IsActive() == false, consumed.WasConsumed == true
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="CloneBehavior"/>
/// <seealso cref="MyrkgengrAbilityId"/>
public sealed record ShadowClone
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Default clone duration in seconds (1 minute).</summary>
    public const int DefaultDurationSeconds = 60;

    /// <summary>Clone hit points (destroyed by any damage).</summary>
    public const int DefaultHitPoints = 1;

    /// <summary>Perception DC to detect this clone as an illusion.</summary>
    public const int DefaultDetectionDC = 14;

    /// <summary>Maximum number of active clones per caster.</summary>
    public const int MaxActiveClones = 2;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Unique identifier for this clone instance.</summary>
    public Guid CloneId { get; private init; }

    /// <summary>ID of the character who created this clone.</summary>
    public Guid OwnerId { get; private init; }

    /// <summary>X coordinate on the combat grid.</summary>
    public int X { get; private init; }

    /// <summary>Y coordinate on the combat grid.</summary>
    public int Y { get; private init; }

    /// <summary>Behavior pattern governing this clone's movement.</summary>
    public CloneBehavior Behavior { get; private init; }

    /// <summary>Timestamp when the clone was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Maximum lifetime in seconds.</summary>
    public int DurationSeconds { get; private init; } = DefaultDurationSeconds;

    /// <summary>Current hit points (0 = destroyed).</summary>
    public int HitPoints { get; private init; } = DefaultHitPoints;

    /// <summary>Perception DC for enemies to detect this clone as an illusion.</summary>
    public int DetectionDC { get; private init; } = DefaultDetectionDC;

    /// <summary>Whether this clone was consumed by Umbral Strike.</summary>
    public bool WasConsumed { get; private init; }

    /// <summary>Whether this clone was destroyed by damage.</summary>
    public bool WasDestroyed { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new active shadow clone at the specified position.
    /// </summary>
    /// <param name="ownerId">ID of the creating character.</param>
    /// <param name="x">X coordinate on the combat grid.</param>
    /// <param name="y">Y coordinate on the combat grid.</param>
    /// <param name="behavior">Behavior pattern for this clone.</param>
    /// <returns>A new active ShadowClone.</returns>
    public static ShadowClone Create(
        Guid ownerId,
        int x,
        int y,
        CloneBehavior behavior) => new()
    {
        CloneId = Guid.NewGuid(),
        OwnerId = ownerId,
        X = x,
        Y = y,
        Behavior = behavior,
        CreatedAt = DateTime.UtcNow,
        DurationSeconds = DefaultDurationSeconds,
        HitPoints = DefaultHitPoints,
        DetectionDC = DefaultDetectionDC,
        WasConsumed = false,
        WasDestroyed = false
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Destroys this clone (e.g., by taking damage).
    /// </summary>
    /// <returns>A new ShadowClone with 0 HP and WasDestroyed = true.</returns>
    public ShadowClone Destroy() => this with
    {
        HitPoints = 0,
        WasDestroyed = true
    };

    /// <summary>
    /// Consumes this clone for Umbral Strike advantage.
    /// </summary>
    /// <returns>A new ShadowClone with 0 HP and WasConsumed = true.</returns>
    public ShadowClone Consume() => this with
    {
        HitPoints = 0,
        WasConsumed = true
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Query
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Whether this clone is still active (has HP, not consumed, not expired).
    /// </summary>
    public bool IsActive() =>
        HitPoints > 0 && !WasConsumed && !WasDestroyed;

    /// <summary>
    /// Whether this clone has exceeded its maximum duration.
    /// </summary>
    public bool IsExpired() =>
        DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= DurationSeconds;

    /// <summary>
    /// Gets the remaining duration in seconds.
    /// </summary>
    /// <returns>Remaining seconds, minimum 0.</returns>
    public int GetRemainingDuration() =>
        Math.Max(0, DurationSeconds - (int)DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds);

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of this clone.
    /// </summary>
    public override string ToString() =>
        $"ShadowClone({CloneId:N8}, Pos=({X},{Y}), Behavior={Behavior}, " +
        $"HP={HitPoints}, Active={IsActive()})";
}
