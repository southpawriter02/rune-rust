using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a navigation attempt.
/// </summary>
/// <remarks>
/// <para>
/// NavigationResult captures all aspects of a navigation check outcome:
/// <list type="bullet">
///   <item><description>Success level determines whether destination was reached</description></item>
///   <item><description>Time modifier reflects delays from getting lost or finding shortcuts</description></item>
///   <item><description>Hazard information captures dangerous area encounters on fumbles</description></item>
/// </list>
/// </para>
/// <para>
/// Outcome determination based on skill check results:
/// <list type="bullet">
///   <item><description>Net successes ≥ DC: Full success (×1.0 time)</description></item>
///   <item><description>Net successes ≥ DC - 2: Partial success (×1.25 time)</description></item>
///   <item><description>Net successes &lt; DC - 2: Failure (×1.5 time)</description></item>
///   <item><description>0 successes + ≥1 botch: Fumble (dangerous area)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Outcome">The navigation outcome (Success, PartialSuccess, Failure, Fumble).</param>
/// <param name="TimeModifier">Multiplier applied to travel time (1.0 = normal, 1.5 = lost).</param>
/// <param name="NetSuccesses">The net successes from the skill check.</param>
/// <param name="TargetDc">The DC that was attempted.</param>
/// <param name="HazardEncountered">Whether a hazard was encountered (fumble only).</param>
/// <param name="DangerousAreaType">The type of dangerous area entered (fumble only).</param>
/// <param name="RollDetails">Optional details about the dice roll for logging/display.</param>
public readonly record struct NavigationResult(
    NavigationOutcome Outcome,
    float TimeModifier,
    int NetSuccesses,
    int TargetDc,
    bool HazardEncountered = false,
    DangerousAreaType? DangerousAreaType = null,
    string? RollDetails = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether navigation was successful (reached destination).
    /// </summary>
    /// <remarks>
    /// True for both <see cref="NavigationOutcome.Success"/> and
    /// <see cref="NavigationOutcome.PartialSuccess"/> outcomes.
    /// </remarks>
    public bool ReachedDestination => Outcome.ReachedDestination();

    /// <summary>
    /// Gets whether the character got lost and needs to retry.
    /// </summary>
    /// <remarks>
    /// True only for <see cref="NavigationOutcome.Failure"/>.
    /// Fumbles are handled separately via <see cref="EnteredDangerousArea"/>.
    /// </remarks>
    public bool GotLost => Outcome == NavigationOutcome.Failure;

    /// <summary>
    /// Gets whether a fumble occurred, entering a dangerous area.
    /// </summary>
    /// <remarks>
    /// True only for <see cref="NavigationOutcome.Fumble"/>.
    /// When true, <see cref="DangerousAreaType"/> will be set.
    /// </remarks>
    public bool EnteredDangerousArea => Outcome == NavigationOutcome.Fumble;

    /// <summary>
    /// Gets whether the navigation was a complete failure (lost or fumbled).
    /// </summary>
    public bool Failed => Outcome is NavigationOutcome.Failure or NavigationOutcome.Fumble;

    /// <summary>
    /// Gets the margin by which the check succeeded or failed.
    /// </summary>
    /// <remarks>
    /// Positive values indicate success margin (how much above DC).
    /// Negative values indicate failure margin (how much below DC).
    /// </remarks>
    public int Margin => NetSuccesses - TargetDc;

    /// <summary>
    /// Gets the percentage time penalty as a readable value.
    /// </summary>
    /// <remarks>
    /// Returns values like 0, 25, or 50 representing the percentage increase in travel time.
    /// </remarks>
    public int TimeModifierPercent => (int)((TimeModifier - 1.0f) * 100);

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful navigation result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the skill check.</param>
    /// <param name="targetDc">The target DC that was met or exceeded.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A navigation result indicating successful arrival on schedule.</returns>
    public static NavigationResult Success(int netSuccesses, int targetDc, string? rollDetails = null) =>
        new(
            NavigationOutcome.Success,
            TimeModifier: 1.0f,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            HazardEncountered: false,
            DangerousAreaType: null,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates a partial success result (reached with delay).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the skill check.</param>
    /// <param name="targetDc">The target DC.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A navigation result indicating arrival with 25% time penalty.</returns>
    public static NavigationResult PartialSuccess(int netSuccesses, int targetDc, string? rollDetails = null) =>
        new(
            NavigationOutcome.PartialSuccess,
            TimeModifier: 1.25f,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            HazardEncountered: false,
            DangerousAreaType: null,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates a failure result (got lost, must retry).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the skill check.</param>
    /// <param name="targetDc">The target DC.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A navigation result indicating getting lost with 50% time penalty.</returns>
    public static NavigationResult Failure(int netSuccesses, int targetDc, string? rollDetails = null) =>
        new(
            NavigationOutcome.Failure,
            TimeModifier: 1.5f,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            HazardEncountered: false,
            DangerousAreaType: null,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates a fumble result (entered dangerous area).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the skill check (typically 0).</param>
    /// <param name="targetDc">The target DC.</param>
    /// <param name="areaType">The type of dangerous area entered.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A navigation result indicating a fumble with dangerous area entry.</returns>
    public static NavigationResult Fumble(
        int netSuccesses,
        int targetDc,
        DangerousAreaType areaType,
        string? rollDetails = null) =>
        new(
            NavigationOutcome.Fumble,
            TimeModifier: 1.0f, // Fumble has other consequences
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            HazardEncountered: true,
            DangerousAreaType: areaType,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates an empty result representing no navigation attempted.
    /// </summary>
    /// <returns>An empty navigation result.</returns>
    public static NavigationResult Empty() =>
        new(
            NavigationOutcome.Failure,
            TimeModifier: 1.0f,
            NetSuccesses: 0,
            TargetDc: 0,
            HazardEncountered: false,
            DangerousAreaType: null,
            RollDetails: "No navigation attempted");

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string describing the navigation outcome.
    /// </summary>
    /// <returns>A formatted string suitable for display to the player.</returns>
    public string ToDisplayString()
    {
        return Outcome switch
        {
            NavigationOutcome.Success =>
                $"Navigation successful! Reached destination on schedule. ({NetSuccesses} successes vs DC {TargetDc})",

            NavigationOutcome.PartialSuccess =>
                $"Navigation successful with minor delays. Travel time +25%. ({NetSuccesses} successes vs DC {TargetDc})",

            NavigationOutcome.Failure =>
                $"Got lost! Must backtrack and try again. Travel time +50%. ({NetSuccesses} successes vs DC {TargetDc})",

            NavigationOutcome.Fumble =>
                $"Critically lost! Stumbled into {DangerousAreaType?.GetDisplayName() ?? "a dangerous area"}. " +
                $"({NetSuccesses} successes vs DC {TargetDc})",

            _ => "Unknown navigation outcome"
        };
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var details = $"NavigationResult\n" +
                      $"  Outcome: {Outcome.GetDisplayName()}\n" +
                      $"  Net Successes: {NetSuccesses}\n" +
                      $"  Target DC: {TargetDc}\n" +
                      $"  Margin: {Margin:+0;-0;0}\n" +
                      $"  Time Modifier: ×{TimeModifier:F2} ({TimeModifierPercent}%)\n" +
                      $"  Reached Destination: {ReachedDestination}\n";

        if (HazardEncountered)
        {
            details += $"  Hazard Encountered: Yes\n" +
                       $"  Dangerous Area: {DangerousAreaType?.GetDisplayName() ?? "Unknown"}\n";
        }

        if (!string.IsNullOrEmpty(RollDetails))
        {
            details += $"  Roll Details: {RollDetails}\n";
        }

        return details;
    }

    /// <summary>
    /// Returns a human-readable description of the navigation result.
    /// </summary>
    /// <returns>A formatted string describing the outcome.</returns>
    public override string ToString()
    {
        var outcome = Outcome.GetDisplayName();
        if (HazardEncountered && DangerousAreaType.HasValue)
        {
            return $"Navigation {outcome}: {NetSuccesses} vs DC {TargetDc}, entered {DangerousAreaType.Value.GetDisplayName()}";
        }
        return $"Navigation {outcome}: {NetSuccesses} vs DC {TargetDc} (×{TimeModifier:F2} time)";
    }
}
