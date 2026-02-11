// ═══════════════════════════════════════════════════════════════════════════════
// BloodiedState.cs
// Value object representing an enemy's bloodied state (at or below 50% HP),
// used to trigger the Blood Scent passive ability.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an enemy's bloodied state for the Berserkr's Blood Scent passive.
/// </summary>
/// <remarks>
/// <para>
/// A target is considered "bloodied" when at or below 50% of maximum HP.
/// The Blood Scent passive grants +10 Rage when an enemy first becomes bloodied,
/// and provides +1 Attack bonus against bloodied targets.
/// </para>
/// <para>
/// This value object captures a snapshot of the target's HP state at the
/// moment of evaluation, enabling detection of the bloodied transition
/// (when a target goes from above 50% to at or below 50%).
/// </para>
/// <example>
/// <code>
/// var state = BloodiedState.Create(targetId, "Draugr", currentHp: 25, maxHp: 50);
/// // state.IsBloodied = true (25 ≤ 25, which is 50% of 50)
///
/// var justBloodied = state.WasJustBloodied(previousHp: 30);
/// // justBloodied = true (was above 50% threshold, now at or below)
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="RageResource"/>
public sealed record BloodiedState
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Unique identifier of the target creature.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the target creature.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Target's current hit points at the time of evaluation.
    /// </summary>
    public int CurrentHp { get; init; }

    /// <summary>
    /// Target's maximum hit points.
    /// </summary>
    public int MaxHp { get; init; }

    /// <summary>
    /// Timestamp when the target was evaluated for bloodied state.
    /// </summary>
    public DateTime BloodiedAt { get; init; }

    /// <summary>
    /// Whether the target is currently bloodied (at or below 50% HP).
    /// </summary>
    public bool IsBloodied => MaxHp > 0 && CurrentHp <= (MaxHp / 2);

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a BloodiedState snapshot for a target.
    /// </summary>
    /// <param name="targetId">Target creature's unique ID.</param>
    /// <param name="targetName">Target creature's display name.</param>
    /// <param name="currentHp">Target's current HP.</param>
    /// <param name="maxHp">Target's maximum HP. Must be positive.</param>
    /// <returns>A new BloodiedState snapshot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxHp"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetName"/> is null or whitespace.</exception>
    public static BloodiedState Create(
        Guid targetId,
        string targetName,
        int currentHp,
        int maxHp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHp, 1);

        return new BloodiedState
        {
            TargetId = targetId,
            TargetName = targetName,
            CurrentHp = Math.Max(currentHp, 0),
            MaxHp = maxHp,
            BloodiedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines if the target was just bloodied (crossed the 50% threshold).
    /// </summary>
    /// <param name="previousHp">Target's HP before the current damage.</param>
    /// <returns>
    /// <c>true</c> if the target was above 50% HP at <paramref name="previousHp"/>
    /// and is now at or below 50% in <see cref="CurrentHp"/>.
    /// </returns>
    public bool WasJustBloodied(int previousHp)
    {
        if (MaxHp <= 0) return false;

        var threshold = MaxHp / 2;
        return previousHp > threshold && CurrentHp <= threshold;
    }

    /// <summary>
    /// Gets the target's current HP as a percentage of maximum.
    /// </summary>
    /// <returns>Integer percentage (0–100).</returns>
    public int GetBloodiedPercentage() =>
        MaxHp > 0 ? (int)((double)CurrentHp / MaxHp * 100) : 0;

    /// <summary>
    /// Gets a human-readable description of the bloodied state.
    /// </summary>
    /// <returns>Formatted description string.</returns>
    public string GetDescription() => IsBloodied
        ? $"{TargetName} is bloodied ({GetBloodiedPercentage()}% HP)"
        : $"{TargetName} is not bloodied ({GetBloodiedPercentage()}% HP)";

    /// <summary>
    /// Returns a diagnostic representation of this state.
    /// </summary>
    public override string ToString() =>
        $"BloodiedState({TargetName}, HP={CurrentHp}/{MaxHp}, Bloodied={IsBloodied})";
}
