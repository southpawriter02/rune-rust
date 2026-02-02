namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the sources of Momentum generation.
/// </summary>
/// <remarks>
/// Each source determines how momentum is gained and may have
/// different multipliers or flat values. Momentum rewards
/// sustained aggression and mobile combat.
/// </remarks>
public enum MomentumSource
{
    /// <summary>
    /// Momentum from a successful attack hit.
    /// </summary>
    /// <remarks>
    /// Flat: 10 momentum per successful attack.
    /// Core mechanic for momentum generation.
    /// </remarks>
    SuccessfulAttack = 0,

    /// <summary>
    /// Bonus momentum from defeating an enemy.
    /// </summary>
    /// <remarks>
    /// Flat: 20 momentum per kill.
    /// Significantly faster momentum generation.
    /// </remarks>
    KillingBlow = 1,

    /// <summary>
    /// Momentum from consecutive successful attacks.
    /// </summary>
    /// <remarks>
    /// Formula: ConsecutiveHits × 5 bonus momentum.
    /// Encourages attack chains without missing.
    /// </remarks>
    ChainAttack = 2,

    /// <summary>
    /// Momentum from movement/repositioning actions.
    /// </summary>
    /// <remarks>
    /// Formula: floor(DistanceMoved / 2) momentum.
    /// Encourages mobile combat style.
    /// </remarks>
    MovementAction = 3
}
