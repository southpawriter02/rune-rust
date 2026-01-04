using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Result of a resonance modification operation.
/// </summary>
public record ResonanceResult(
    int PreviousValue,
    int NewValue,
    int RequestedAmount,
    int ActualAmount,
    ResonanceThreshold PreviousThreshold,
    ResonanceThreshold NewThreshold,
    string Source)
{
    /// <summary>
    /// True if the modification crossed a threshold boundary.
    /// </summary>
    public bool ThresholdChanged => PreviousThreshold != NewThreshold;

    /// <summary>
    /// True if resonance increased.
    /// </summary>
    public bool IsIncrease => ActualAmount > 0;

    /// <summary>
    /// True if resonance decreased.
    /// </summary>
    public bool IsDecrease => ActualAmount < 0;

    /// <summary>
    /// True if resonance hit maximum (100).
    /// </summary>
    public bool OverflowTriggered => NewValue >= ResonanceState.MaxResonance;

    /// <summary>
    /// True if resonance hit minimum (0).
    /// </summary>
    public bool FullyDissipated => NewValue <= ResonanceState.MinResonance;

    /// <summary>
    /// True if the requested amount was clamped (hit boundary).
    /// </summary>
    public bool WasClamped => RequestedAmount != ActualAmount;

    /// <summary>
    /// Factory for creating a "no change" result (non-Mystic characters).
    /// </summary>
    public static ResonanceResult NoChange(string source) => new(
        PreviousValue: 0,
        NewValue: 0,
        RequestedAmount: 0,
        ActualAmount: 0,
        PreviousThreshold: ResonanceThreshold.Dim,
        NewThreshold: ResonanceThreshold.Dim,
        Source: source);
}
