namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a rage gain operation.
/// </summary>
/// <remarks>
/// <para>
/// RageGainResult is an immutable record that captures all relevant
/// information about a rage gain transaction, including threshold changes.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>PreviousRage: Rage value before the gain</item>
/// <item>NewRage: Rage value after the gain</item>
/// <item>AmountGained: The delta (capped at MaxRage)</item>
/// <item>Source: What triggered the rage gain</item>
/// <item>ThresholdChanged: Whether crossing a threshold boundary</item>
/// <item>NewThreshold: The new threshold if changed</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PreviousRage">Rage value before gain.</param>
/// <param name="NewRage">Rage value after gain.</param>
/// <param name="AmountGained">Amount of rage gained.</param>
/// <param name="Source">Source of rage generation.</param>
/// <param name="ThresholdChanged">Whether threshold changed.</param>
/// <param name="NewThreshold">New threshold if changed.</param>
public record RageGainResult(
    int PreviousRage,
    int NewRage,
    int AmountGained,
    RageSource Source,
    bool ThresholdChanged,
    RageThreshold? NewThreshold)
{
    /// <summary>
    /// Gets whether rage reached maximum (100).
    /// </summary>
    /// <remarks>
    /// When capped, additional rage gain is lost.
    /// </remarks>
    public bool CappedAtMaximum => NewRage == 100;

    /// <summary>
    /// Gets whether this gain triggered entry into FrenzyBeyondReason.
    /// </summary>
    /// <remarks>
    /// Critical gains require special handling (forced attack, party effects).
    /// </remarks>
    public bool IsCriticalGain =>
        ThresholdChanged && NewThreshold == RageThreshold.FrenzyBeyondReason;

    /// <summary>
    /// Creates a display string for this rage gain.
    /// </summary>
    /// <returns>Human-readable description of the rage gain.</returns>
    public string ToDisplayString() =>
        $"Rage +{AmountGained} from {Source} " +
        $"({PreviousRage} → {NewRage})" +
        (ThresholdChanged ? $" [Threshold: {NewThreshold}]" : "");
}
