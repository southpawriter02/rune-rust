namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the stages of Momentum for the Storm Blade specialization.
/// </summary>
/// <remarks>
/// <para>
/// Momentum is a flow-state resource that rewards sustained aggression
/// and punishes interruption. It requires maintaining attack chains to
/// stay high, but provides increasing bonuses with each threshold.
/// </para>
/// <para>
/// Threshold Ranges (based on Momentum 0-100):
/// <list type="bullet">
/// <item>Stationary: 0-20 momentum — No flow, no bonuses</item>
/// <item>Moving: 21-40 momentum — Building flow, +1 attack/defense</item>
/// <item>Flowing: 41-60 momentum — Active chain attacks, 1 bonus attack</item>
/// <item>Surging: 61-80 momentum — High momentum, reduced recovery</item>
/// <item>Unstoppable: 81-100 momentum — Ultimate flow state, +10% crit</item>
/// </list>
/// </para>
/// </remarks>
public enum MomentumThreshold
{
    /// <summary>
    /// Stationary state (0-20 momentum).
    /// </summary>
    /// <remarks>
    /// No flow state. No combat bonuses.
    /// </remarks>
    Stationary = 0,

    /// <summary>
    /// Moving state (21-40 momentum).
    /// </summary>
    /// <remarks>
    /// Building momentum. Small attack and defense bonuses (+1 each).
    /// Movement bonus begins scaling.
    /// </remarks>
    Moving = 1,

    /// <summary>
    /// Flowing state (41-60 momentum).
    /// </summary>
    /// <remarks>
    /// Active flow state. Enables chain attacks with 1 bonus attack.
    /// Attack and defense bonuses increase to +2 each.
    /// </remarks>
    Flowing = 2,

    /// <summary>
    /// Surging state (61-80 momentum).
    /// </summary>
    /// <remarks>
    /// High momentum. 1 bonus attack and strong bonuses (+3 attack/defense).
    /// Reduced recovery time between attacks.
    /// </remarks>
    Surging = 3,

    /// <summary>
    /// Unstoppable state (81-100 momentum).
    /// </summary>
    /// <remarks>
    /// Maximum flow state. 2 bonus attacks, +4 attack/defense,
    /// +10% critical hit chance, and full heal on kill.
    /// </remarks>
    Unstoppable = 4
}
