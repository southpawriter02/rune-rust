using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of a hazard activation attempt (v0.3.3a).
/// Contains damage dealt, statuses applied, and state transition info.
/// </summary>
/// <param name="WasTriggered">Whether the hazard actually activated.</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="Message">Narrative message describing the activation.</param>
/// <param name="TotalDamage">Total damage dealt to target(s).</param>
/// <param name="TotalHealing">Total healing applied to target(s).</param>
/// <param name="StatusesApplied">List of status effect names applied.</param>
/// <param name="NewState">The hazard's state after activation.</param>
public record HazardResult(
    bool WasTriggered,
    string HazardName,
    string Message,
    int TotalDamage,
    int TotalHealing,
    List<string> StatusesApplied,
    HazardState NewState
)
{
    /// <summary>
    /// Represents a hazard that did not trigger.
    /// </summary>
    public static HazardResult None => new(
        false,
        string.Empty,
        string.Empty,
        0,
        0,
        new List<string>(),
        HazardState.Dormant
    );
}
