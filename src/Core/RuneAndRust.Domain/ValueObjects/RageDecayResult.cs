namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a rage decay operation.
/// </summary>
/// <remarks>
/// <para>
/// RageDecayResult is an immutable record that captures all relevant
/// information about a rage decay transaction, including threshold changes.
/// </para>
/// <para>
/// Key Properties:
/// <list type="bullet">
/// <item>PreviousRage: Rage value before the decay</item>
/// <item>NewRage: Rage value after the decay</item>
/// <item>AmountDecayed: The delta (clamped at MinRage)</item>
/// <item>ThresholdChanged: Whether crossing a threshold boundary</item>
/// <item>NewThreshold: The new threshold if changed</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PreviousRage">Rage value before decay.</param>
/// <param name="NewRage">Rage value after decay.</param>
/// <param name="AmountDecayed">Amount of rage decayed.</param>
/// <param name="ThresholdChanged">Whether threshold changed.</param>
/// <param name="NewThreshold">New threshold if changed.</param>
public record RageDecayResult(
    int PreviousRage,
    int NewRage,
    int AmountDecayed,
    bool ThresholdChanged,
    RageThreshold? NewThreshold)
{
    /// <summary>
    /// Gets whether rage fell to zero.
    /// </summary>
    /// <remarks>
    /// When zeroed, all rage bonuses are lost.
    /// </remarks>
    public bool ZeroedOut => NewRage == 0;

    /// <summary>
    /// Gets whether this decay exited FrenzyBeyondReason.
    /// </summary>
    /// <remarks>
    /// Exiting frenzy removes forced attack and party stress reduction.
    /// </remarks>
    public bool ExitedFrenzy =>
        ThresholdChanged &&
        PreviousRage > 80 &&
        NewRage <= 80;

    /// <summary>
    /// Creates a display string for this rage decay.
    /// </summary>
    /// <returns>Human-readable description of the rage decay.</returns>
    public string ToDisplayString() =>
        $"Rage -{AmountDecayed} " +
        $"({PreviousRage} → {NewRage})" +
        (ThresholdChanged ? $" [Threshold: {NewThreshold}]" : "");
}
