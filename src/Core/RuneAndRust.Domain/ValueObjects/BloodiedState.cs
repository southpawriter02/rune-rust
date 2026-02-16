using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a snapshot of an enemy's HP state for Blood Scent passive detection.
/// Used by the Berserkr specialization to determine when an enemy becomes bloodied
/// (HP drops to or below 50% of maximum) during combat.
/// </summary>
/// <remarks>
/// <para>The "bloodied" condition is a standard TTRPG mechanic indicating that a target
/// has been reduced to half health or less. For the Berserkr:</para>
/// <list type="bullet">
/// <item>Blood Scent passive grants +10 Rage when an enemy first becomes bloodied</item>
/// <item>Blood Scent also grants +1 Attack against bloodied targets</item>
/// </list>
/// <para>The key method <see cref="WasJustBloodied"/> detects the transition from
/// non-bloodied to bloodied state, preventing duplicate Rage gains.</para>
/// </remarks>
public sealed record BloodiedState
{
    /// <summary>
    /// Unique identifier of the target whose HP state is being tracked.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the target for logging and combat log purposes.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Current hit points of the target at the time of snapshot.
    /// </summary>
    public int CurrentHp { get; init; }

    /// <summary>
    /// Maximum hit points of the target.
    /// </summary>
    public int MaxHp { get; init; }

    /// <summary>
    /// Gets whether the target is currently bloodied (HP at or below 50% of maximum).
    /// </summary>
    /// <remarks>
    /// A target with 0 HP is still considered bloodied (dead/dying, but still bloodied).
    /// A target with MaxHp of 0 is never considered bloodied.
    /// </remarks>
    public bool IsBloodied => MaxHp > 0 && CurrentHp <= MaxHp / 2;

    /// <summary>
    /// UTC timestamp when this bloodied state was first detected.
    /// Null if the target has never been bloodied.
    /// </summary>
    public DateTime? BloodiedAt { get; init; }

    /// <summary>
    /// Creates a new BloodiedState snapshot for the specified target.
    /// </summary>
    /// <param name="targetId">Unique identifier of the target.</param>
    /// <param name="targetName">Display name of the target.</param>
    /// <param name="currentHp">Current HP of the target.</param>
    /// <param name="maxHp">Maximum HP of the target.</param>
    /// <returns>A new BloodiedState with computed bloodied status.</returns>
    public static BloodiedState Create(Guid targetId, string targetName, int currentHp, int maxHp)
    {
        var state = new BloodiedState
        {
            TargetId = targetId,
            TargetName = targetName,
            CurrentHp = currentHp,
            MaxHp = maxHp
        };

        // Set BloodiedAt if currently bloodied
        return state.IsBloodied
            ? state with { BloodiedAt = DateTime.UtcNow }
            : state;
    }

    /// <summary>
    /// Determines whether the target just transitioned into the bloodied state
    /// based on its previous HP value.
    /// </summary>
    /// <param name="previousHp">The target's HP before the most recent damage.</param>
    /// <returns>
    /// True if the target was NOT bloodied at <paramref name="previousHp"/> but IS bloodied
    /// at <see cref="CurrentHp"/>; False otherwise.
    /// </returns>
    /// <remarks>
    /// This method prevents duplicate Blood Scent triggers by ensuring Rage
    /// is only gained on the actual transition, not on subsequent damage to
    /// an already-bloodied target.
    /// </remarks>
    public bool WasJustBloodied(int previousHp)
    {
        if (MaxHp <= 0)
            return false;

        var wasBloodiedBefore = previousHp <= MaxHp / 2;
        return !wasBloodiedBefore && IsBloodied;
    }
}
