// ═══════════════════════════════════════════════════════════════════════════════
// RunicTrap.cs
// Immutable value object representing a runic trap placed on the battlefield
// by the Rúnasmiðr specialization. Traps deal damage when triggered by enemy
// movement and are invisible to non-allies.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a runic trap placed on the battlefield by the Rúnasmiðr.
/// </summary>
/// <remarks>
/// <para>
/// Runic Trap is a Tier 2 ability that places an invisible trap in an adjacent
/// space. When an enemy enters the space, the trap triggers and deals 3d6 damage.
/// Allies can see and safely pass through trap positions.
/// </para>
/// <para>
/// Key mechanics:
/// </para>
/// <list type="bullet">
///   <item><description><b>Damage:</b> 3d6 on trigger (average ~10.5 damage)</description></item>
///   <item><description><b>Detection:</b> DC 14 Perception check to spot</description></item>
///   <item><description><b>Duration:</b> 1 hour if not triggered</description></item>
///   <item><description><b>Limit:</b> Maximum 3 active traps per character</description></item>
///   <item><description><b>Visibility:</b> Owner and allies can see trap positions</description></item>
///   <item><description><b>One-use:</b> Destroyed after triggering</description></item>
/// </list>
/// <para>
/// This is an immutable value object — the <see cref="Trigger"/> method returns
/// a <see cref="TrapTriggerResult"/> and a new trap instance (via tuple) with
/// the triggered state set. The original instance is never modified.
/// </para>
/// <para>
/// <b>Cost:</b> 3 AP, 2 Rune Charges.
/// <b>Tier:</b> 2 (requires 8 PP invested in Rúnasmiðr tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var trap = RunicTrap.Create(ownerId, (5, 3));
/// // trap.IsTriggered = false, trap.DamageDice = "3d6"
///
/// var (result, updatedTrap) = trap.Trigger(enemyId, random);
/// // result.Success = true, updatedTrap.IsTriggered = true
/// </code>
/// </example>
/// <seealso cref="TrapTriggerResult"/>
/// <seealso cref="TrapTriggerType"/>
/// <seealso cref="RuneAndRust.Domain.Enums.RunasmidrAbilityId"/>
public sealed record RunicTrap
{
    /// <summary>
    /// Default damage dice expression for runic traps.
    /// </summary>
    public const string DefaultDamageDice = "3d6";

    /// <summary>
    /// Default Perception DC required to detect a runic trap.
    /// </summary>
    public const int DefaultDetectionDc = 14;

    /// <summary>
    /// Default duration before the trap expires, in hours.
    /// </summary>
    public const int DefaultDurationHours = 1;

    /// <summary>
    /// Maximum number of active traps a single character can maintain.
    /// </summary>
    public const int MaxActiveTraps = 3;

    /// <summary>
    /// Number of d6 dice rolled for trap damage.
    /// </summary>
    public const int DamageDiceCount = 3;

    /// <summary>
    /// Number of sides on each damage die.
    /// </summary>
    public const int DamageDiceSides = 6;

    /// <summary>
    /// Gets the unique identifier for this trap instance.
    /// </summary>
    public Guid TrapId { get; init; }

    /// <summary>
    /// Gets the ID of the character who placed this trap.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the grid position where the trap is placed (X, Y coordinates).
    /// </summary>
    public (int X, int Y) Position { get; init; }

    /// <summary>
    /// Gets the damage dice expression for this trap (always "3d6").
    /// </summary>
    public string DamageDice { get; init; } = DefaultDamageDice;

    /// <summary>
    /// Gets the Perception DC required to detect this trap.
    /// </summary>
    public int DetectionDc { get; init; } = DefaultDetectionDc;

    /// <summary>
    /// Gets the trigger type for this trap.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="TrapTriggerType.Movement"/> for Tier 2.
    /// </remarks>
    public TrapTriggerType TriggerType { get; init; } = TrapTriggerType.Movement;

    /// <summary>
    /// Gets the timestamp when this trap was placed.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when this trap expires (1 hour after placement).
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets whether this trap has been triggered.
    /// </summary>
    public bool IsTriggered { get; init; }

    /// <summary>
    /// Gets the ID of the character who triggered this trap, if any.
    /// </summary>
    public Guid? TriggeredByCharacterId { get; init; }

    /// <summary>
    /// Gets the timestamp when this trap was triggered, if applicable.
    /// </summary>
    public DateTime? TriggeredAt { get; init; }

    /// <summary>
    /// Gets whether this trap has expired (current time is past expiration).
    /// </summary>
    /// <remarks>
    /// Uses <see cref="DateTime.UtcNow"/> for the comparison. A triggered trap
    /// is not considered expired — it is simply consumed.
    /// </remarks>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Gets whether this trap is still active (not triggered and not expired).
    /// </summary>
    public bool IsActive => !IsTriggered && !IsExpired;

    /// <summary>
    /// Creates a new runic trap at the specified position.
    /// </summary>
    /// <param name="ownerId">ID of the character placing the trap.</param>
    /// <param name="position">Grid position (X, Y) for the trap.</param>
    /// <returns>A new RunicTrap instance set to expire in 1 hour.</returns>
    public static RunicTrap Create(Guid ownerId, (int X, int Y) position)
    {
        var now = DateTime.UtcNow;
        return new RunicTrap
        {
            TrapId = Guid.NewGuid(),
            OwnerId = ownerId,
            Position = position,
            DamageDice = DefaultDamageDice,
            DetectionDc = DefaultDetectionDc,
            TriggerType = TrapTriggerType.Movement,
            CreatedAt = now,
            ExpiresAt = now.AddHours(DefaultDurationHours),
            IsTriggered = false,
            TriggeredByCharacterId = null,
            TriggeredAt = null
        };
    }

    /// <summary>
    /// Creates a runic trap with a specific expiration time (for testing).
    /// </summary>
    /// <param name="ownerId">ID of the character placing the trap.</param>
    /// <param name="position">Grid position (X, Y) for the trap.</param>
    /// <param name="expiresAt">Explicit expiration timestamp.</param>
    /// <returns>A new RunicTrap instance with the specified expiration.</returns>
    public static RunicTrap CreateWithExpiration(
        Guid ownerId,
        (int X, int Y) position,
        DateTime expiresAt)
    {
        return new RunicTrap
        {
            TrapId = Guid.NewGuid(),
            OwnerId = ownerId,
            Position = position,
            DamageDice = DefaultDamageDice,
            DetectionDc = DefaultDetectionDc,
            TriggerType = TrapTriggerType.Movement,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsTriggered = false,
            TriggeredByCharacterId = null,
            TriggeredAt = null
        };
    }

    /// <summary>
    /// Triggers the trap when an enemy enters the space, rolling damage.
    /// </summary>
    /// <param name="targetId">ID of the character who triggered the trap.</param>
    /// <param name="random">
    /// Random instance for dice rolling. If null, uses <see cref="Random.Shared"/>.
    /// </param>
    /// <returns>
    /// A tuple of (result, updatedTrap). The result contains damage dealt;
    /// the updated trap has <see cref="IsTriggered"/> set to <c>true</c>.
    /// If the trap was already triggered or expired, returns a failed result
    /// and the unmodified trap.
    /// </returns>
    public (TrapTriggerResult Result, RunicTrap UpdatedTrap) Trigger(
        Guid targetId,
        Random? random = null)
    {
        // Cannot trigger an already-triggered trap
        if (IsTriggered)
        {
            return (
                TrapTriggerResult.Failed("Trap has already been triggered."),
                this);
        }

        // Cannot trigger an expired trap
        if (IsExpired)
        {
            return (
                TrapTriggerResult.Failed("Trap has expired."),
                this);
        }

        // Roll 3d6 damage
        var rng = random ?? Random.Shared;
        var totalDamage = 0;
        for (var i = 0; i < DamageDiceCount; i++)
        {
            totalDamage += rng.Next(1, DamageDiceSides + 1);
        }

        // Mark trap as triggered (immutable — create new instance)
        var triggeredTrap = this with
        {
            IsTriggered = true,
            TriggeredByCharacterId = targetId,
            TriggeredAt = DateTime.UtcNow
        };

        var result = TrapTriggerResult.Triggered(TrapId, targetId, totalDamage);

        return (result, triggeredTrap);
    }

    /// <summary>
    /// Determines if a specific character can see this trap.
    /// </summary>
    /// <param name="characterId">ID of the character to check visibility for.</param>
    /// <returns>
    /// <c>true</c> if the character is the trap's owner; otherwise, <c>false</c>.
    /// Future versions will extend this to include party/ally logic.
    /// </returns>
    public bool IsVisibleTo(Guid characterId) => characterId == OwnerId;

    /// <summary>
    /// Determines if this trap can be detected via a Perception check.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the trap is active (not triggered and not expired);
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool CanBeDetected() => !IsTriggered && !IsExpired;

    /// <summary>
    /// Gets the remaining time before this trap expires.
    /// </summary>
    /// <returns>
    /// The remaining time as a <see cref="TimeSpan"/>. Returns <see cref="TimeSpan.Zero"/>
    /// if the trap has already expired.
    /// </returns>
    public TimeSpan GetRemainingTime()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets the remaining time formatted for display.
    /// </summary>
    /// <returns>
    /// A human-readable string like "45m", "30s", or "0h 55m".
    /// </returns>
    public string GetRemainingTimeDisplay()
    {
        var remaining = GetRemainingTime();

        if (remaining <= TimeSpan.Zero)
            return "expired";

        if (remaining.TotalMinutes < 1)
            return $"{(int)remaining.TotalSeconds}s";

        if (remaining.TotalHours < 1)
            return $"{(int)remaining.TotalMinutes}m";

        return $"{remaining.Hours}h {remaining.Minutes}m";
    }

    /// <summary>
    /// Returns a human-readable representation of the trap state.
    /// </summary>
    public override string ToString()
    {
        var status = IsTriggered ? "TRIGGERED" : IsExpired ? "EXPIRED" : "ACTIVE";
        return $"Runic Trap [{status}] at ({Position.X}, {Position.Y}): " +
               $"{DamageDice} damage, DC {DetectionDc} ({GetRemainingTimeDisplay()})";
    }
}
