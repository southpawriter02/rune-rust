namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of a rest action containing recovery deltas and status information.
/// Used by the UI to display the rest summary screen.
/// </summary>
/// <param name="HpRecovered">Amount of HP recovered during rest.</param>
/// <param name="StaminaRecovered">Amount of Stamina recovered during rest.</param>
/// <param name="StressRecovered">Amount of Stress relieved during rest.</param>
/// <param name="SuppliesConsumed">Whether supplies (Ration + Water) were consumed.</param>
/// <param name="IsExhausted">Whether the character became Exhausted due to missing supplies.</param>
/// <param name="TimeAdvancedMinutes">World time advanced in minutes (default 480 = 8 hours).</param>
public record RestResult(
    int HpRecovered,
    int StaminaRecovered,
    int StressRecovered,
    bool SuppliesConsumed,
    bool IsExhausted,
    int TimeAdvancedMinutes = 480
);
