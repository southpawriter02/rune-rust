namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Captures the complete outcome of a single chase round, including both
/// participants' obstacle attempts and the resulting distance change.
/// </summary>
/// <remarks>
/// <para>
/// Each chase round consists of an obstacle generated, both participants
/// attempting to overcome it, and distance adjustments based on their results.
/// This value object provides all information needed to narrate the round
/// and update the chase state.
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase round results.
/// </para>
/// </remarks>
/// <param name="RoundNumber">The sequential round number (1-based).</param>
/// <param name="Obstacle">The obstacle both participants attempted.</param>
/// <param name="FleeingNetSuccesses">The fleeing character's net successes.</param>
/// <param name="FleeingOutcome">The fleeing character's check outcome.</param>
/// <param name="FleeingDistanceChange">Distance change from fleeing result.</param>
/// <param name="PursuerNetSuccesses">The pursuer's net successes.</param>
/// <param name="PursuerOutcome">The pursuer's check outcome.</param>
/// <param name="PursuerDistanceChange">Distance change from pursuer result.</param>
/// <param name="PreviousDistance">Distance before this round.</param>
/// <param name="NewDistance">Distance after this round.</param>
/// <param name="ChaseStatus">The chase status after this round.</param>
public readonly record struct ChaseRoundResult(
    int RoundNumber,
    ChaseObstacle Obstacle,
    int FleeingNetSuccesses,
    SkillOutcome FleeingOutcome,
    int FleeingDistanceChange,
    int PursuerNetSuccesses,
    SkillOutcome PursuerOutcome,
    int PursuerDistanceChange,
    int PreviousDistance,
    int NewDistance,
    ChaseStatus ChaseStatus)
{
    /// <summary>
    /// Gets the net distance change this round (positive = fleeing gained ground).
    /// </summary>
    public int NetDistanceChange => NewDistance - PreviousDistance;

    /// <summary>
    /// Indicates whether the chase ended this round.
    /// </summary>
    public bool IsChaseEnded => ChaseStatus is not ChaseStatus.InProgress;

    /// <summary>
    /// Indicates whether the fleeing character escaped this round.
    /// </summary>
    public bool FleeingEscaped => ChaseStatus == ChaseStatus.Escaped;

    /// <summary>
    /// Indicates whether the fleeing character was caught this round.
    /// </summary>
    public bool FleeingCaught => ChaseStatus == ChaseStatus.Caught;

    /// <summary>
    /// Determines if the fleeing character won this round (gained ground).
    /// </summary>
    public bool FleeingWonRound => NetDistanceChange > 0;

    /// <summary>
    /// Determines if the pursuer won this round (closed gap).
    /// </summary>
    public bool PursuerWonRound => NetDistanceChange < 0;

    /// <summary>
    /// Determines if the round was a tie (no distance change).
    /// </summary>
    public bool WasTie => NetDistanceChange == 0;

    /// <summary>
    /// Gets whether the fleeing character succeeded on the obstacle.
    /// </summary>
    public bool FleeingSucceeded => FleeingOutcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether the pursuer succeeded on the obstacle.
    /// </summary>
    public bool PursuerSucceeded => PursuerOutcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Creates a display string summarizing the round outcome.
    /// </summary>
    /// <returns>Formatted round summary for UI display.</returns>
    public string ToDisplayString()
    {
        var change = NetDistanceChange switch
        {
            > 0 => $"+{NetDistanceChange} (pulling ahead)",
            < 0 => $"{NetDistanceChange} (pursuer closing)",
            _ => "0 (neck and neck)"
        };

        return $"Round {RoundNumber}: {Obstacle.TypeName} - Distance {change} → {NewDistance}";
    }

    /// <summary>
    /// Gets a narrative description of the round outcome.
    /// </summary>
    /// <param name="fleeingName">Name of the fleeing character.</param>
    /// <param name="pursuerName">Name of the pursuer.</param>
    /// <returns>Narrative text describing what happened.</returns>
    public string GetNarrative(string fleeingName, string pursuerName)
    {
        var fleeingResult = FleeingSucceeded
            ? Obstacle.SuccessDescription
            : Obstacle.FailureDescription;

        var pursuerResult = PursuerSucceeded
            ? $"{pursuerName} clears the obstacle."
            : $"{pursuerName} struggles with the obstacle.";

        var distance = ChaseStatus switch
        {
            ChaseStatus.Caught => $"{pursuerName} catches {fleeingName}!",
            ChaseStatus.Escaped => $"{fleeingName} escapes into the shadows!",
            _ when NetDistanceChange > 0 => $"{fleeingName} pulls ahead!",
            _ when NetDistanceChange < 0 => $"{pursuerName} closes the gap!",
            _ => "They remain neck and neck!"
        };

        return $"{fleeingResult} {pursuerResult} {distance}";
    }

    /// <summary>
    /// Creates a round result for a successful fleeing attempt.
    /// </summary>
    public static ChaseRoundResult Create(
        int roundNumber,
        ChaseObstacle obstacle,
        int fleeingNetSuccesses,
        SkillOutcome fleeingOutcome,
        int fleeingDistanceChange,
        int pursuerNetSuccesses,
        SkillOutcome pursuerOutcome,
        int pursuerDistanceChange,
        int previousDistance,
        int newDistance,
        ChaseStatus status)
    {
        return new ChaseRoundResult(
            roundNumber,
            obstacle,
            fleeingNetSuccesses,
            fleeingOutcome,
            fleeingDistanceChange,
            pursuerNetSuccesses,
            pursuerOutcome,
            pursuerDistanceChange,
            previousDistance,
            newDistance,
            status);
    }
}
