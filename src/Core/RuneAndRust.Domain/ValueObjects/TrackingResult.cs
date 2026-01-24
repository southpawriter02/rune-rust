using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a tracking attempt or phase transition.
/// </summary>
/// <remarks>
/// <para>
/// Encapsulates the complete outcome of any tracking operation including:
/// <list type="bullet">
///   <item><description>Whether the check succeeded</description></item>
///   <item><description>Phase before and after the check</description></item>
///   <item><description>The updated tracking state</description></item>
///   <item><description>Narrative description for the player</description></item>
///   <item><description>Available next actions</description></item>
///   <item><description>Any discovered information</description></item>
/// </list>
/// </para>
/// <para>
/// TrackingResult is returned by all ITrackingService methods that perform
/// checks (InitiateTracking, ContinuePursuit, AttemptRecovery) and provides
/// everything needed to update the game state and display feedback to the player.
/// </para>
/// </remarks>
/// <param name="IsSuccess">Whether the tracking check succeeded.</param>
/// <param name="Phase">Current tracking phase after this result.</param>
/// <param name="PreviousPhase">Phase before this check was made.</param>
/// <param name="Check">The check that was performed.</param>
/// <param name="TrackingState">Updated tracking state after this result.</param>
/// <param name="NarrativeDescription">Flavor text describing the result for the player.</param>
/// <param name="NextActions">Available actions the player can take next.</param>
/// <param name="DiscoveredInfo">Any new information discovered during this check.</param>
public readonly record struct TrackingResult(
    bool IsSuccess,
    TrackingPhase Phase,
    TrackingPhase PreviousPhase,
    TrackingCheck Check,
    TrackingState TrackingState,
    string NarrativeDescription,
    IReadOnlyList<string> NextActions,
    TrackingDiscovery? DiscoveredInfo)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether the phase changed as a result of this check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// True when the tracking advanced to a new phase or regressed:
    /// <list type="bullet">
    ///   <item><description>Acquisition → Pursuit (success)</description></item>
    ///   <item><description>Pursuit → ClosingIn (within range)</description></item>
    ///   <item><description>Pursuit/ClosingIn → Lost (failure)</description></item>
    ///   <item><description>Lost → Pursuit (recovery success)</description></item>
    ///   <item><description>Any → Cold (3 failures or fumble)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool PhaseChanged => Phase != PreviousPhase;

    /// <summary>
    /// Whether the tracking has completed (found target or trail cold).
    /// </summary>
    /// <remarks>
    /// Returns true when the TrackingState has reached a terminal status.
    /// No further tracking checks can be made after completion.
    /// </remarks>
    public bool IsComplete => TrackingState.IsComplete;

    /// <summary>
    /// Whether the trail has gone cold.
    /// </summary>
    /// <remarks>
    /// <para>
    /// True when the trail is unrecoverable due to:
    /// <list type="bullet">
    ///   <item><description>3 failed acquisition attempts</description></item>
    ///   <item><description>3 failed recovery attempts</description></item>
    ///   <item><description>Fumble during acquisition</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When trail goes cold, the player must find a new lead to resume tracking.
    /// </para>
    /// </remarks>
    public bool TrailWentCold => Phase == TrackingPhase.Cold;

    /// <summary>
    /// Whether the target was found.
    /// </summary>
    /// <remarks>
    /// True when the final closing check succeeded and encounter begins.
    /// The tracking is complete and the target has been located.
    /// </remarks>
    public bool TargetFound => TrackingState.Status == TrackingStatus.TargetFound;

    /// <summary>
    /// Whether the trail was lost on this check.
    /// </summary>
    /// <remarks>
    /// True when transitioning to Lost phase from Pursuit or ClosingIn.
    /// Recovery options are available in the NextActions list.
    /// </remarks>
    public bool TrailLost => Phase == TrackingPhase.Lost && PreviousPhase != TrackingPhase.Lost;

    /// <summary>
    /// Whether recovery was successful.
    /// </summary>
    /// <remarks>
    /// True when successfully recovering from Lost phase back to Pursuit.
    /// </remarks>
    public bool RecoverySuccessful => IsSuccess && PreviousPhase == TrackingPhase.Lost && Phase == TrackingPhase.Pursuit;

    /// <summary>
    /// Whether this was the initial acquisition.
    /// </summary>
    /// <remarks>
    /// True when the check was an acquisition attempt (finding the initial trail).
    /// </remarks>
    public bool WasAcquisition => PreviousPhase == TrackingPhase.Acquisition;

    /// <summary>
    /// Whether new information was discovered.
    /// </summary>
    /// <remarks>
    /// True when DiscoveredInfo contains any information
    /// (target count, trail age, direction, or additional details).
    /// </remarks>
    public bool HasDiscovery => DiscoveredInfo.HasValue &&
        (DiscoveredInfo.Value.TargetCount.HasValue ||
         DiscoveredInfo.Value.TrailAge.HasValue ||
         !string.IsNullOrEmpty(DiscoveredInfo.Value.Direction) ||
         !string.IsNullOrEmpty(DiscoveredInfo.Value.AdditionalInfo));

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a success result with optional discovery information.
    /// </summary>
    /// <param name="phase">The current phase after the successful check.</param>
    /// <param name="previousPhase">The phase before the check.</param>
    /// <param name="check">The tracking check that was performed.</param>
    /// <param name="state">The updated tracking state.</param>
    /// <param name="narrative">Descriptive text for the player.</param>
    /// <param name="nextActions">List of available actions.</param>
    /// <param name="discovery">Optional information discovered during the check.</param>
    /// <returns>A TrackingResult indicating success.</returns>
    /// <remarks>
    /// Use this factory method for all successful tracking checks including
    /// acquisition, pursuit continuation, closing in, and recovery.
    /// </remarks>
    public static TrackingResult Success(
        TrackingPhase phase,
        TrackingPhase previousPhase,
        TrackingCheck check,
        TrackingState state,
        string narrative,
        IReadOnlyList<string> nextActions,
        TrackingDiscovery? discovery = null)
    {
        return new TrackingResult(
            IsSuccess: true,
            Phase: phase,
            PreviousPhase: previousPhase,
            Check: check,
            TrackingState: state,
            NarrativeDescription: narrative,
            NextActions: nextActions,
            DiscoveredInfo: discovery);
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="phase">The current phase after the failed check.</param>
    /// <param name="previousPhase">The phase before the check.</param>
    /// <param name="check">The tracking check that was performed.</param>
    /// <param name="state">The updated tracking state.</param>
    /// <param name="narrative">Descriptive text for the player.</param>
    /// <param name="nextActions">List of available recovery actions.</param>
    /// <returns>A TrackingResult indicating failure.</returns>
    /// <remarks>
    /// Use this factory method for failed tracking checks. The nextActions
    /// list should include available recovery options when appropriate.
    /// </remarks>
    public static TrackingResult Failure(
        TrackingPhase phase,
        TrackingPhase previousPhase,
        TrackingCheck check,
        TrackingState state,
        string narrative,
        IReadOnlyList<string> nextActions)
    {
        return new TrackingResult(
            IsSuccess: false,
            Phase: phase,
            PreviousPhase: previousPhase,
            Check: check,
            TrackingState: state,
            NarrativeDescription: narrative,
            NextActions: nextActions,
            DiscoveredInfo: null);
    }

    /// <summary>
    /// Creates a result for when the target is found.
    /// </summary>
    /// <param name="previousPhase">The phase before the final check (usually ClosingIn).</param>
    /// <param name="check">The final tracking check.</param>
    /// <param name="state">The updated tracking state with TargetFound status.</param>
    /// <param name="narrative">Descriptive text about finding the target.</param>
    /// <returns>A TrackingResult indicating the target was found.</returns>
    /// <remarks>
    /// Used when the final closing check succeeds and encounter begins.
    /// NextActions will typically be empty as tracking is complete.
    /// </remarks>
    public static TrackingResult TargetLocated(
        TrackingPhase previousPhase,
        TrackingCheck check,
        TrackingState state,
        string narrative)
    {
        return new TrackingResult(
            IsSuccess: true,
            Phase: TrackingPhase.ClosingIn,
            PreviousPhase: previousPhase,
            Check: check,
            TrackingState: state,
            NarrativeDescription: narrative,
            NextActions: Array.Empty<string>(),
            DiscoveredInfo: null);
    }

    /// <summary>
    /// Creates a result for when the trail goes cold.
    /// </summary>
    /// <param name="previousPhase">The phase before the trail went cold.</param>
    /// <param name="check">The check that resulted in the trail going cold.</param>
    /// <param name="state">The updated tracking state with TrailCold status.</param>
    /// <param name="narrative">Descriptive text about losing the trail.</param>
    /// <returns>A TrackingResult indicating the trail is unrecoverable.</returns>
    /// <remarks>
    /// Used when the trail becomes unrecoverable (3 failed attempts or fumble).
    /// NextActions may include "Find new lead" or similar options.
    /// </remarks>
    public static TrackingResult TrailGoneCold(
        TrackingPhase previousPhase,
        TrackingCheck check,
        TrackingState state,
        string narrative)
    {
        return new TrackingResult(
            IsSuccess: false,
            Phase: TrackingPhase.Cold,
            PreviousPhase: previousPhase,
            Check: check,
            TrackingState: state,
            NarrativeDescription: narrative,
            NextActions: new[] { "Find a new lead to resume tracking" },
            DiscoveredInfo: null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted summary of the result.
    /// </summary>
    /// <returns>A string describing the tracking result status and narrative.</returns>
    public override string ToString()
    {
        var status = IsSuccess ? "SUCCESS" : "FAILURE";

        if (TargetFound)
        {
            status = "TARGET FOUND";
        }
        else if (TrailWentCold)
        {
            status = "TRAIL COLD";
        }
        else if (TrailLost)
        {
            status = "TRAIL LOST";
        }
        else if (RecoverySuccessful)
        {
            status = "RECOVERED";
        }

        return $"Tracking {status}: {NarrativeDescription}";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var discoveryStr = HasDiscovery
            ? $"\n  Discovery: {DiscoveredInfo}"
            : string.Empty;

        var actionsStr = NextActions.Count > 0
            ? $"\n  Next Actions: {string.Join(", ", NextActions)}"
            : string.Empty;

        return $"TrackingResult\n" +
               $"  Success: {IsSuccess}\n" +
               $"  Phase: {PreviousPhase} → {Phase}{(PhaseChanged ? " (CHANGED)" : "")}\n" +
               $"  Check: {Check}\n" +
               $"  Narrative: {NarrativeDescription}" +
               discoveryStr +
               actionsStr;
    }
}

/// <summary>
/// Information discovered during tracking.
/// </summary>
/// <remarks>
/// <para>
/// Populated when the tracker learns additional information through:
/// <list type="bullet">
///   <item><description>Successful acquisition (automatic direction)</description></item>
///   <item><description>Optional estimate target count check (DC 10)</description></item>
///   <item><description>Optional estimate trail age check (DC 12)</description></item>
///   <item><description>Critical success bonus information</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="TargetCount">
/// Estimated number of targets based on trail signs.
/// Determined through optional DC 10 contested check.
/// </param>
/// <param name="TrailAge">
/// Estimated age of the trail.
/// Determined through optional DC 12 check.
/// </param>
/// <param name="Direction">
/// Direction of travel (automatically determined on successful acquisition).
/// Examples: "Northeast", "Following the river", "Toward the ruins".
/// </param>
/// <param name="AdditionalInfo">
/// Any other discovered details from critical successes or special abilities.
/// Examples: "One target is limping", "They stopped here to rest", "Signs of a struggle".
/// </param>
public readonly record struct TrackingDiscovery(
    int? TargetCount,
    TrailAge? TrailAge,
    string? Direction,
    string? AdditionalInfo)
{
    /// <summary>
    /// Creates a discovery with only direction information.
    /// </summary>
    /// <param name="direction">The direction of travel.</param>
    /// <returns>A TrackingDiscovery with direction only.</returns>
    /// <remarks>
    /// Used for basic successful acquisition where only direction is automatically determined.
    /// </remarks>
    public static TrackingDiscovery DirectionOnly(string direction)
    {
        return new TrackingDiscovery(null, null, direction, null);
    }

    /// <summary>
    /// Creates a discovery with target count estimate.
    /// </summary>
    /// <param name="count">The estimated target count.</param>
    /// <param name="direction">Optional direction information.</param>
    /// <returns>A TrackingDiscovery with target count.</returns>
    /// <remarks>
    /// Used when the optional DC 10 contested check to estimate target count succeeds.
    /// </remarks>
    public static TrackingDiscovery WithTargetCount(int count, string? direction = null)
    {
        return new TrackingDiscovery(count, null, direction, null);
    }

    /// <summary>
    /// Creates a discovery with trail age estimate.
    /// </summary>
    /// <param name="age">The estimated trail age.</param>
    /// <param name="direction">Optional direction information.</param>
    /// <returns>A TrackingDiscovery with trail age.</returns>
    /// <remarks>
    /// Used when the optional DC 12 check to estimate trail age succeeds.
    /// </remarks>
    public static TrackingDiscovery WithTrailAge(TrailAge age, string? direction = null)
    {
        return new TrackingDiscovery(null, age, direction, null);
    }

    /// <summary>
    /// Creates a full discovery with all information.
    /// </summary>
    /// <param name="targetCount">The estimated target count.</param>
    /// <param name="trailAge">The estimated trail age.</param>
    /// <param name="direction">The direction of travel.</param>
    /// <param name="additionalInfo">Any additional discovered details.</param>
    /// <returns>A TrackingDiscovery with all information populated.</returns>
    /// <remarks>
    /// Used for critical successes or when multiple optional checks succeed.
    /// </remarks>
    public static TrackingDiscovery Full(
        int targetCount,
        TrailAge trailAge,
        string direction,
        string? additionalInfo = null)
    {
        return new TrackingDiscovery(targetCount, trailAge, direction, additionalInfo);
    }

    /// <summary>
    /// Returns a formatted summary of the discovered information.
    /// </summary>
    /// <returns>A comma-separated list of discovered details.</returns>
    public override string ToString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(Direction))
        {
            parts.Add($"Direction: {Direction}");
        }

        if (TargetCount.HasValue)
        {
            parts.Add($"Targets: ~{TargetCount.Value}");
        }

        if (TrailAge.HasValue)
        {
            parts.Add($"Age: {TrailAge.Value}");
        }

        if (!string.IsNullOrEmpty(AdditionalInfo))
        {
            parts.Add(AdditionalInfo);
        }

        return parts.Count > 0
            ? string.Join(", ", parts)
            : "No additional information";
    }
}
