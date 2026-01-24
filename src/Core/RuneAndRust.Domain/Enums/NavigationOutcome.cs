namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the possible outcomes of a navigation attempt.
/// </summary>
/// <remarks>
/// <para>
/// Navigation outcomes determine travel time penalties and consequences:
/// <list type="bullet">
///   <item><description><see cref="Success"/>: Reached destination on schedule (x1.0 time)</description></item>
///   <item><description><see cref="PartialSuccess"/>: Reached destination with delays (x1.25 time)</description></item>
///   <item><description><see cref="Failure"/>: Got lost, must retry navigation (x1.5 time)</description></item>
///   <item><description><see cref="Fumble"/>: Critically lost, entered a dangerous area</description></item>
/// </list>
/// </para>
/// <para>
/// Outcome determination by skill check results:
/// <list type="bullet">
///   <item><description>Net successes ≥ DC: Full success</description></item>
///   <item><description>Net successes ≥ DC - 2: Partial success</description></item>
///   <item><description>Net successes &lt; DC - 2: Failure</description></item>
///   <item><description>0 successes + ≥1 botch: Fumble</description></item>
/// </list>
/// </para>
/// </remarks>
public enum NavigationOutcome
{
    /// <summary>
    /// Full success - reached destination on schedule.
    /// </summary>
    /// <remarks>
    /// Achieved when net successes meet or exceed the target DC.
    /// Travel time multiplier: x1.0 (no penalty).
    /// </remarks>
    Success = 0,

    /// <summary>
    /// Partial success - reached destination with minor delays.
    /// </summary>
    /// <remarks>
    /// Achieved when net successes are within 2 of the target DC but not meeting it.
    /// Travel time multiplier: x1.25 (25% longer travel time).
    /// </remarks>
    PartialSuccess = 1,

    /// <summary>
    /// Failure - got lost, must backtrack and retry.
    /// </summary>
    /// <remarks>
    /// Achieved when net successes are more than 2 below the target DC.
    /// Travel time multiplier: x1.5 (50% longer travel time).
    /// The navigator must attempt navigation again to reach the destination.
    /// </remarks>
    Failure = 2,

    /// <summary>
    /// Fumble - critically lost and entered a dangerous area.
    /// </summary>
    /// <remarks>
    /// Achieved when the skill check results in 0 successes with at least 1 botch.
    /// The navigator stumbles into a dangerous area (hazard zone, hostile territory,
    /// or glitch pocket) and gains the Disoriented fumble consequence.
    /// </remarks>
    Fumble = 3
}

/// <summary>
/// Extension methods for <see cref="NavigationOutcome"/>.
/// </summary>
public static class NavigationOutcomeExtensions
{
    /// <summary>
    /// Gets the time multiplier applied to travel for this outcome.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>A multiplier to apply to base travel time.</returns>
    public static float GetTimeMultiplier(this NavigationOutcome outcome)
    {
        return outcome switch
        {
            NavigationOutcome.Success => 1.0f,
            NavigationOutcome.PartialSuccess => 1.25f,
            NavigationOutcome.Failure => 1.5f,
            NavigationOutcome.Fumble => 1.0f, // Fumble has other consequences
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets the human-readable display name for this outcome.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this NavigationOutcome outcome)
    {
        return outcome switch
        {
            NavigationOutcome.Success => "Success",
            NavigationOutcome.PartialSuccess => "Partial Success",
            NavigationOutcome.Failure => "Failure",
            NavigationOutcome.Fumble => "Fumble",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a description of the navigation outcome.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>A descriptive string explaining what happened.</returns>
    public static string GetDescription(this NavigationOutcome outcome)
    {
        return outcome switch
        {
            NavigationOutcome.Success =>
                "Navigation successful! Reached destination on schedule.",
            NavigationOutcome.PartialSuccess =>
                "Navigation successful with minor delays. Travel took 25% longer than expected.",
            NavigationOutcome.Failure =>
                "Got lost! Must backtrack and try again. Travel will take 50% longer.",
            NavigationOutcome.Fumble =>
                "Critically lost! Stumbled into a dangerous area.",
            _ => "Unknown navigation outcome."
        };
    }

    /// <summary>
    /// Determines whether the destination was reached.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>True if the destination was reached (Success or PartialSuccess).</returns>
    public static bool ReachedDestination(this NavigationOutcome outcome)
    {
        return outcome is NavigationOutcome.Success or NavigationOutcome.PartialSuccess;
    }

    /// <summary>
    /// Determines whether the navigator got lost.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>True if the navigator got lost (Failure or Fumble).</returns>
    public static bool GotLost(this NavigationOutcome outcome)
    {
        return outcome is NavigationOutcome.Failure or NavigationOutcome.Fumble;
    }

    /// <summary>
    /// Determines whether this outcome results in entering a dangerous area.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>True if a dangerous area was entered (Fumble only).</returns>
    public static bool EnteredDangerousArea(this NavigationOutcome outcome)
    {
        return outcome == NavigationOutcome.Fumble;
    }
}
