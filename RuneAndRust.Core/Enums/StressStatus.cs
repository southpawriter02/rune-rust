namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the psychic stress level of a combatant.
/// Stress accumulates from traumatic events and affects combat performance.
/// </summary>
public enum StressStatus
{
    /// <summary>
    /// Stress 0-19. No mechanical penalties. Clear-headed and focused.
    /// </summary>
    Stable = 0,

    /// <summary>
    /// Stress 20-39. Minor UI indicator. Beginning to feel the weight.
    /// </summary>
    Unsettled = 1,

    /// <summary>
    /// Stress 40-59. Visible nervousness. Defense penalty may apply.
    /// </summary>
    Shaken = 2,

    /// <summary>
    /// Stress 60-79. Significant distress. Clear combat impairment.
    /// </summary>
    Distressed = 3,

    /// <summary>
    /// Stress 80-99. Near breaking point. Severe penalties.
    /// </summary>
    Fractured = 4,

    /// <summary>
    /// Stress 100. Breaking point reached. Triggers trauma event.
    /// </summary>
    Breaking = 5
}
