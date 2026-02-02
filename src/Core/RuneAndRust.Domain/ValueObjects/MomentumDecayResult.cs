namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a momentum decay operation.
/// </summary>
/// <remarks>
/// <para>
/// MomentumDecayResult is an immutable record that captures all relevant
/// information about a momentum decay transaction, including threshold changes
/// and chain break status.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>PreviousMomentum: Momentum value before the decay</item>
/// <item>NewMomentum: Momentum value after the decay</item>
/// <item>AmountDecayed: The delta (clamped at MinMomentum)</item>
/// <item>DecayReason: What triggered the decay (e.g., "Missed Attack", "Stunned")</item>
/// <item>ChainBroken: Whether the consecutive hit chain was broken</item>
/// <item>ThresholdChanged: Whether crossing a threshold boundary</item>
/// <item>NewThreshold: The new threshold if changed</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PreviousMomentum">Momentum value before decay.</param>
/// <param name="NewMomentum">Momentum value after decay.</param>
/// <param name="AmountDecayed">Amount of momentum decayed.</param>
/// <param name="DecayReason">Reason for decay (e.g., "Missed Attack", "Stunned").</param>
/// <param name="ChainBroken">Whether the consecutive hit chain was broken.</param>
/// <param name="ThresholdChanged">Whether threshold changed.</param>
/// <param name="NewThreshold">New threshold if changed.</param>
public record MomentumDecayResult(
    int PreviousMomentum,
    int NewMomentum,
    int AmountDecayed,
    string DecayReason,
    bool ChainBroken,
    bool ThresholdChanged,
    MomentumThreshold? NewThreshold)
{
    /// <summary>
    /// Gets whether momentum fell to zero.
    /// </summary>
    /// <remarks>
    /// When zeroed, all momentum bonuses are lost.
    /// </remarks>
    public bool ZeroedOut => NewMomentum == 0;

    /// <summary>
    /// Gets whether this is a full reset (decay amount is 100).
    /// </summary>
    /// <remarks>
    /// Full resets typically occur from stun/freeze effects.
    /// </remarks>
    public bool IsFullReset => AmountDecayed == 100;

    /// <summary>
    /// Gets whether this decay exited Unstoppable threshold.
    /// </summary>
    /// <remarks>
    /// Exiting Unstoppable removes critical bonus and heal on kill.
    /// </remarks>
    public bool ExitedUnstoppable =>
        ThresholdChanged &&
        PreviousMomentum > 80 &&
        NewMomentum <= 80;

    /// <summary>
    /// Creates a display string for this momentum decay.
    /// </summary>
    /// <returns>Human-readable description of the momentum decay.</returns>
    public string ToDisplayString()
    {
        var result = $"Momentum -{AmountDecayed} ({DecayReason}) ({PreviousMomentum} → {NewMomentum})";
        if (ChainBroken)
            result += " [Chain Broken!]";
        if (ThresholdChanged)
            result += $" [Threshold: {NewThreshold}]";
        return result;
    }
}
