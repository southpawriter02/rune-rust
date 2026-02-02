namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a momentum gain operation.
/// </summary>
/// <remarks>
/// <para>
/// MomentumGainResult is an immutable record that captures all relevant
/// information about a momentum gain transaction, including threshold changes
/// and chain bonuses from consecutive hits.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>PreviousMomentum: Momentum value before the gain</item>
/// <item>NewMomentum: Momentum value after the gain</item>
/// <item>AmountGained: The delta (capped at MaxMomentum)</item>
/// <item>Source: What triggered the momentum gain</item>
/// <item>ChainBonus: Bonus from consecutive hits, if any</item>
/// <item>ThresholdChanged: Whether crossing a threshold boundary</item>
/// <item>NewThreshold: The new threshold if changed</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PreviousMomentum">Momentum value before gain.</param>
/// <param name="NewMomentum">Momentum value after gain.</param>
/// <param name="AmountGained">Amount of momentum gained.</param>
/// <param name="Source">Source of momentum generation.</param>
/// <param name="ChainBonus">Bonus from consecutive hits, if any.</param>
/// <param name="ThresholdChanged">Whether threshold changed.</param>
/// <param name="NewThreshold">New threshold if changed.</param>
public record MomentumGainResult(
    int PreviousMomentum,
    int NewMomentum,
    int AmountGained,
    MomentumSource Source,
    int? ChainBonus,
    bool ThresholdChanged,
    MomentumThreshold? NewThreshold)
{
    /// <summary>
    /// Gets whether momentum reached maximum (100).
    /// </summary>
    /// <remarks>
    /// When capped, additional momentum gain is lost.
    /// </remarks>
    public bool CappedAtMaximum => NewMomentum == 100;

    /// <summary>
    /// Gets whether this gain triggered entry into Unstoppable threshold.
    /// </summary>
    /// <remarks>
    /// Unstoppable gains unlock +10% crit, 2 bonus attacks, and full heal on kill.
    /// </remarks>
    public bool IsUnstoppableGain =>
        ThresholdChanged && NewThreshold == MomentumThreshold.Unstoppable;

    /// <summary>
    /// Creates a display string for this momentum gain.
    /// </summary>
    /// <returns>Human-readable description of the momentum gain.</returns>
    public string ToDisplayString()
    {
        var result = $"Momentum +{AmountGained} from {Source} ({PreviousMomentum} → {NewMomentum})";
        if (ChainBonus.HasValue)
            result += $" [+{ChainBonus} chain]";
        if (ThresholdChanged)
            result += $" [Threshold: {NewThreshold}]";
        return result;
    }
}
