using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Persistent state for a multi-phase tracking pursuit.
/// </summary>
/// <remarks>
/// <para>
/// Tracks all information about an ongoing pursuit including:
/// <list type="bullet">
///   <item><description>Current phase and overall status</description></item>
///   <item><description>Trail characteristics and degradation</description></item>
///   <item><description>Check history and cumulative modifiers</description></item>
///   <item><description>Distance covered and terrain traversed</description></item>
/// </list>
/// </para>
/// <para>
/// TrackingState is an aggregate root that manages its own state transitions
/// and validates phase changes according to the tracking state machine:
/// <code>
/// Acquisition → Pursuit → ClosingIn → TargetFound
///      ↓            ↓           ↓
///      └──────── Lost ←────────┘
///                  ↓
///                Cold
/// </code>
/// </para>
/// <para>
/// Phase transition rules:
/// <list type="bullet">
///   <item><description>Acquisition → Pursuit: On successful acquisition check</description></item>
///   <item><description>Pursuit → ClosingIn: When within 500ft of target</description></item>
///   <item><description>Any → Lost: On failed pursuit/closing check (except from Cold)</description></item>
///   <item><description>Lost → Pursuit: On successful recovery</description></item>
///   <item><description>Lost → Cold: After 3 failed recovery attempts</description></item>
///   <item><description>Acquisition → Cold: After 3 failed acquisition attempts</description></item>
/// </list>
/// </para>
/// </remarks>
public class TrackingState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE IDENTITY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Unique identifier for this tracking pursuit.
    /// </summary>
    /// <remarks>
    /// Generated as a GUID string when tracking is initiated.
    /// Used to reference this specific tracking state in subsequent operations.
    /// </remarks>
    public string TrackingId { get; private set; } = string.Empty;

    /// <summary>
    /// Identifier of the character performing the tracking.
    /// </summary>
    /// <remarks>
    /// References the player or NPC character using the Wasteland Survival skill.
    /// A character can only have one active tracking pursuit at a time.
    /// </remarks>
    public string TrackerId { get; private set; } = string.Empty;

    /// <summary>
    /// Description of what is being tracked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// May be specific ("goblin patrol", "wounded raider") or general
    /// ("large creature", "group of humanoids").
    /// </para>
    /// <para>
    /// This description may be updated as more information is discovered
    /// during the tracking process (via EstimateTargetCount, etc.).
    /// </para>
    /// </remarks>
    public string TargetDescription { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE MACHINE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Current phase of the tracking procedure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tracks the current step in the multi-phase tracking process:
    /// <list type="bullet">
    ///   <item><description>Acquisition: Finding the initial trail</description></item>
    ///   <item><description>Pursuit: Following the trail</description></item>
    ///   <item><description>ClosingIn: Final approach to target</description></item>
    ///   <item><description>Lost: Trail temporarily lost</description></item>
    ///   <item><description>Cold: Trail unrecoverable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public TrackingPhase CurrentPhase { get; private set; }

    /// <summary>
    /// Age classification of the trail being followed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Determines the base DC for tracking checks:
    /// <list type="bullet">
    ///   <item><description>Obvious: DC 8</description></item>
    ///   <item><description>Fresh: DC 12</description></item>
    ///   <item><description>Standard: DC 16</description></item>
    ///   <item><description>Old: DC 20</description></item>
    ///   <item><description>Obscured: DC 24</description></item>
    ///   <item><description>Cold: DC 28 (Master rank only)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public TrailAge TrailAge { get; private set; }

    /// <summary>
    /// Overall status of the tracking pursuit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indicates whether tracking is:
    /// <list type="bullet">
    ///   <item><description>Active: Pursuit in progress</description></item>
    ///   <item><description>Abandoned: Player chose to stop</description></item>
    ///   <item><description>TargetFound: Successfully located target</description></item>
    ///   <item><description>TrailCold: Trail unrecoverable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public TrackingStatus Status { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROGRESS TRACKING PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Total distance covered during pursuit in miles.
    /// </summary>
    /// <remarks>
    /// Accumulated across all successful pursuit checks.
    /// Used to determine check frequency based on terrain type.
    /// </remarks>
    public float DistanceCovered { get; private set; }

    /// <summary>
    /// Cumulative DC modifier from time elapsed and failed attempts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Increases by:
    /// <list type="bullet">
    ///   <item><description>+1 per hour elapsed</description></item>
    ///   <item><description>+2 per failed acquisition retry</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Does not include base DC or condition modifiers from TrackingModifiers.
    /// This represents degradation of the trail over time.
    /// </para>
    /// </remarks>
    public int CumulativeDcModifier { get; private set; }

    /// <summary>
    /// Number of failed attempts in current phase.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Resets when phase changes successfully. Used to track the 3-failure
    /// threshold for trail going cold:
    /// <list type="bullet">
    ///   <item><description>3 failed acquisition attempts → Cold</description></item>
    ///   <item><description>3 failed recovery attempts → Cold</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int FailedAttemptsInPhase { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // HISTORY COLLECTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// History of all tracking checks performed.
    /// </summary>
    /// <remarks>
    /// Provides an audit trail of all checks made during this pursuit,
    /// including successes and failures, for narrative replay and debugging.
    /// </remarks>
    public IReadOnlyList<TrackingCheck> CheckHistory => _checkHistory.AsReadOnly();
    private readonly List<TrackingCheck> _checkHistory;

    /// <summary>
    /// History of terrain types traversed during pursuit.
    /// </summary>
    /// <remarks>
    /// Records the navigation terrain type at each check point.
    /// Used for determining appropriate check intervals and modifiers.
    /// </remarks>
    public IReadOnlyList<NavigationTerrainType> TerrainHistory => _terrainHistory.AsReadOnly();
    private readonly List<NavigationTerrainType> _terrainHistory;

    // ═══════════════════════════════════════════════════════════════════════════
    // TIMESTAMP PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Timestamp when tracking began.
    /// </summary>
    /// <remarks>
    /// Recorded in UTC when the tracking pursuit is initiated.
    /// Used for calculating elapsed time modifiers.
    /// </remarks>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Timestamp of last check performed.
    /// </summary>
    /// <remarks>
    /// Updated in UTC after each tracking check.
    /// Used to calculate time elapsed since last action.
    /// </remarks>
    public DateTime LastCheckAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // ESTIMATED INFORMATION PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Estimated distance to target in feet (if known).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only populated when:
    /// <list type="bullet">
    ///   <item><description>In ClosingIn phase (always populated)</description></item>
    ///   <item><description>Through special abilities or equipment</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// ClosingIn phase modifiers based on distance:
    /// <list type="bullet">
    ///   <item><description>Within 500 ft: -4 DC</description></item>
    ///   <item><description>Within 100 ft: -6 DC</description></item>
    ///   <item><description>Within 50 ft: Auto-success</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int? EstimatedDistanceToTarget { get; private set; }

    /// <summary>
    /// Estimated number of targets (if determined).
    /// </summary>
    /// <remarks>
    /// Determined through optional DC 10 contested check during acquisition.
    /// Null if not yet estimated or estimation failed.
    /// </remarks>
    public int? EstimatedTargetCount { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core and factory method use.
    /// </summary>
    /// <remarks>
    /// Initializes empty collections. Use the <see cref="Create"/> factory
    /// method for creating new tracking states.
    /// </remarks>
    private TrackingState()
    {
        _checkHistory = new List<TrackingCheck>();
        _terrainHistory = new List<NavigationTerrainType>();
    }

    /// <summary>
    /// Creates a new tracking state for a pursuit.
    /// </summary>
    /// <param name="trackerId">ID of the tracking character.</param>
    /// <param name="targetDescription">Description of the target.</param>
    /// <param name="trailAge">Age classification of the trail.</param>
    /// <returns>A new TrackingState in Acquisition phase.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="trackerId"/> or <paramref name="targetDescription"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Initializes a new tracking pursuit with:
    /// <list type="bullet">
    ///   <item><description>Unique tracking ID (GUID)</description></item>
    ///   <item><description>Acquisition phase</description></item>
    ///   <item><description>Active status</description></item>
    ///   <item><description>Zero distance and modifiers</description></item>
    ///   <item><description>Current UTC timestamps</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static TrackingState Create(
        string trackerId,
        string targetDescription,
        TrailAge trailAge)
    {
        ArgumentNullException.ThrowIfNull(trackerId, nameof(trackerId));
        ArgumentNullException.ThrowIfNull(targetDescription, nameof(targetDescription));

        if (string.IsNullOrWhiteSpace(trackerId))
        {
            throw new ArgumentException("Tracker ID cannot be empty.", nameof(trackerId));
        }

        if (string.IsNullOrWhiteSpace(targetDescription))
        {
            throw new ArgumentException("Target description cannot be empty.", nameof(targetDescription));
        }

        return new TrackingState
        {
            TrackingId = Guid.NewGuid().ToString(),
            TrackerId = trackerId,
            TargetDescription = targetDescription,
            CurrentPhase = TrackingPhase.Acquisition,
            TrailAge = trailAge,
            DistanceCovered = 0f,
            CumulativeDcModifier = 0,
            FailedAttemptsInPhase = 0,
            Status = TrackingStatus.Active,
            StartedAt = DateTime.UtcNow,
            LastCheckAt = DateTime.UtcNow,
            _checkHistory = { },
            _terrainHistory = { }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base difficulty class for the current trail.
    /// </summary>
    /// <remarks>
    /// The base DC is determined by the trail age enum value:
    /// Obvious=8, Fresh=12, Standard=16, Old=20, Obscured=24, Cold=28.
    /// </remarks>
    public int BaseDc => (int)TrailAge;

    /// <summary>
    /// Gets the effective difficulty class including cumulative modifiers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// EffectiveDc = BaseDc + CumulativeDcModifier
    /// </para>
    /// <para>
    /// Does not include condition modifiers (blood trail, rain, etc.)
    /// which are calculated separately in <see cref="TrackingModifiers"/>.
    /// The final DC for a check is: EffectiveDc + TrackingModifiers.TotalDcModifier
    /// </para>
    /// </remarks>
    public int EffectiveDc => BaseDc + CumulativeDcModifier;

    /// <summary>
    /// Whether the trail can still be followed.
    /// </summary>
    /// <remarks>
    /// Returns true only when Status is Active.
    /// False for Abandoned, TargetFound, or TrailCold statuses.
    /// </remarks>
    public bool IsActive => Status == TrackingStatus.Active;

    /// <summary>
    /// Whether the tracking has reached a terminal state.
    /// </summary>
    /// <remarks>
    /// Returns true when Status is not Active (Abandoned, TargetFound, or TrailCold).
    /// No further tracking checks can be made once complete.
    /// </remarks>
    public bool IsComplete => Status != TrackingStatus.Active;

    /// <summary>
    /// Gets the total number of checks performed.
    /// </summary>
    public int TotalChecks => _checkHistory.Count;

    /// <summary>
    /// Gets the number of successful checks performed.
    /// </summary>
    public int SuccessfulChecks => _checkHistory.Count(c => c.IsSuccess);

    /// <summary>
    /// Gets the number of failed checks performed.
    /// </summary>
    public int FailedChecks => _checkHistory.Count(c => c.IsFailure);

    // ═══════════════════════════════════════════════════════════════════════════
    // CHECK RECORDING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records a successful tracking check.
    /// </summary>
    /// <param name="check">The check result to record.</param>
    /// <param name="distanceAdvanced">Distance advanced in miles.</param>
    /// <param name="terrain">Terrain type traversed.</param>
    /// <exception cref="ArgumentNullException">Thrown when check is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when tracking is not active.</exception>
    /// <remarks>
    /// <para>
    /// Records the check in history, adds terrain to history, advances distance,
    /// updates the last check timestamp, and resets the failed attempts counter.
    /// </para>
    /// </remarks>
    public void RecordSuccess(TrackingCheck check, float distanceAdvanced, NavigationTerrainType terrain)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot record check for completed tracking (Status: {Status}).");
        }

        _checkHistory.Add(check);
        _terrainHistory.Add(terrain);
        DistanceCovered += Math.Max(0, distanceAdvanced);
        LastCheckAt = DateTime.UtcNow;
        FailedAttemptsInPhase = 0; // Reset on success
    }

    /// <summary>
    /// Records a failed tracking check.
    /// </summary>
    /// <param name="check">The check result to record.</param>
    /// <exception cref="ArgumentNullException">Thrown when check is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when tracking is not active.</exception>
    /// <remarks>
    /// Records the check in history, updates the last check timestamp,
    /// and increments the failed attempts counter for the current phase.
    /// </remarks>
    public void RecordFailure(TrackingCheck check)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot record check for completed tracking (Status: {Status}).");
        }

        _checkHistory.Add(check);
        LastCheckAt = DateTime.UtcNow;
        FailedAttemptsInPhase++;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE TRANSITION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Transitions to the Pursuit phase after successful acquisition.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if not in Acquisition phase.
    /// </exception>
    /// <remarks>
    /// Called after the tracker successfully finds and identifies the trail.
    /// Resets the failed attempts counter for the new phase.
    /// </remarks>
    public void TransitionToPursuit()
    {
        if (CurrentPhase != TrackingPhase.Acquisition)
        {
            throw new InvalidOperationException(
                $"Cannot transition to Pursuit from {CurrentPhase}. Must be in Acquisition phase.");
        }

        CurrentPhase = TrackingPhase.Pursuit;
        FailedAttemptsInPhase = 0;
    }

    /// <summary>
    /// Transitions to the ClosingIn phase when near target.
    /// </summary>
    /// <param name="estimatedDistance">Estimated distance to target in feet.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if not in Pursuit phase.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if estimatedDistance is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Called when the tracker is within 500 feet of the target.
    /// Sets the estimated distance and resets the failed attempts counter.
    /// </para>
    /// <para>
    /// ClosingIn phase uses different DC modifiers based on distance:
    /// <list type="bullet">
    ///   <item><description>500 ft: -4 DC</description></item>
    ///   <item><description>100 ft: -6 DC</description></item>
    ///   <item><description>50 ft: Auto-success</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void TransitionToClosingIn(int estimatedDistance)
    {
        if (CurrentPhase != TrackingPhase.Pursuit)
        {
            throw new InvalidOperationException(
                $"Cannot transition to ClosingIn from {CurrentPhase}. Must be in Pursuit phase.");
        }

        if (estimatedDistance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedDistance),
                "Estimated distance cannot be negative.");
        }

        CurrentPhase = TrackingPhase.ClosingIn;
        EstimatedDistanceToTarget = estimatedDistance;
        FailedAttemptsInPhase = 0;
    }

    /// <summary>
    /// Transitions to the Lost phase after a failed pursuit check.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if in terminal Cold phase.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Entered when a pursuit or closing check fails.
    /// Recovery options include:
    /// <list type="bullet">
    ///   <item><description>Backtrack: Same DC, 10 minutes</description></item>
    ///   <item><description>Spiral Search: +4 DC, 30 minutes</description></item>
    ///   <item><description>Return to Last Known: +8 DC, 1 hour</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void TransitionToLost()
    {
        if (CurrentPhase == TrackingPhase.Cold)
        {
            throw new InvalidOperationException(
                "Cannot transition from Cold phase - trail is unrecoverable.");
        }

        CurrentPhase = TrackingPhase.Lost;
        FailedAttemptsInPhase = 0;
    }

    /// <summary>
    /// Transitions to the Cold phase when trail is unrecoverable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state entered when:
    /// <list type="bullet">
    ///   <item><description>3 failed acquisition attempts</description></item>
    ///   <item><description>3 failed recovery attempts</description></item>
    ///   <item><description>Fumble during acquisition</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Once in Cold phase, the tracking cannot be resumed.
    /// The player must find a new lead to start fresh tracking.
    /// </para>
    /// </remarks>
    public void TransitionToCold()
    {
        CurrentPhase = TrackingPhase.Cold;
        Status = TrackingStatus.TrailCold;
    }

    /// <summary>
    /// Recovers from Lost phase back to Pursuit.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if not in Lost phase.
    /// </exception>
    /// <remarks>
    /// Called after a successful recovery check (backtrack, spiral search,
    /// or return to last known). Resets the failed attempts counter.
    /// </remarks>
    public void RecoverToPursuit()
    {
        if (CurrentPhase != TrackingPhase.Lost)
        {
            throw new InvalidOperationException(
                $"Cannot recover from {CurrentPhase}. Must be in Lost phase.");
        }

        CurrentPhase = TrackingPhase.Pursuit;
        FailedAttemptsInPhase = 0;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATUS UPDATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Marks tracking as complete when target is found.
    /// </summary>
    /// <remarks>
    /// Called when the final closing check succeeds and the encounter begins.
    /// This is a terminal state - no further tracking checks can be made.
    /// </remarks>
    public void MarkTargetFound()
    {
        Status = TrackingStatus.TargetFound;
    }

    /// <summary>
    /// Marks tracking as abandoned by the player.
    /// </summary>
    /// <remarks>
    /// Called when the player chooses to stop tracking.
    /// This is a terminal state - no further tracking checks can be made.
    /// The tracking state is preserved for history purposes.
    /// </remarks>
    public void Abandon()
    {
        Status = TrackingStatus.Abandoned;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MODIFIER UPDATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Increases the cumulative DC modifier.
    /// </summary>
    /// <param name="amount">Amount to increase by (must be non-negative).</param>
    /// <remarks>
    /// <para>
    /// Used for:
    /// <list type="bullet">
    ///   <item><description>Time elapsed: +1 per hour</description></item>
    ///   <item><description>Failed acquisition retry: +2 per retry</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Negative values are ignored to prevent accidental DC reduction.
    /// </para>
    /// </remarks>
    public void IncreaseCumulativeModifier(int amount)
    {
        CumulativeDcModifier += Math.Max(0, amount);
    }

    /// <summary>
    /// Updates the estimated target count.
    /// </summary>
    /// <param name="count">Estimated number of targets (minimum 1).</param>
    /// <remarks>
    /// Determined through optional DC 10 contested check during acquisition.
    /// Values less than 1 are clamped to 1.
    /// </remarks>
    public void SetEstimatedTargetCount(int count)
    {
        EstimatedTargetCount = Math.Max(1, count);
    }

    /// <summary>
    /// Updates the estimated distance to target.
    /// </summary>
    /// <param name="distanceFeet">Distance in feet (minimum 0).</param>
    /// <remarks>
    /// Updated during ClosingIn phase as the tracker approaches.
    /// Negative values are clamped to 0.
    /// </remarks>
    public void UpdateDistanceToTarget(int distanceFeet)
    {
        EstimatedDistanceToTarget = Math.Max(0, distanceFeet);
    }

    /// <summary>
    /// Updates the target description with newly discovered information.
    /// </summary>
    /// <param name="newDescription">The updated target description.</param>
    /// <exception cref="ArgumentException">Thrown when description is null or empty.</exception>
    /// <remarks>
    /// Used when the tracker learns more about what they're following
    /// (e.g., "group of humanoids" becomes "raider patrol, 4-5 members").
    /// </remarks>
    public void UpdateTargetDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
        {
            throw new ArgumentException("Target description cannot be empty.", nameof(newDescription));
        }

        TargetDescription = newDescription;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted summary of the tracking state.
    /// </summary>
    /// <returns>A string describing the current tracking status.</returns>
    public override string ToString()
    {
        var phaseInfo = CurrentPhase switch
        {
            TrackingPhase.Acquisition => "Finding trail",
            TrackingPhase.Pursuit => $"Following ({DistanceCovered:F1} mi)",
            TrackingPhase.ClosingIn => $"Closing in ({EstimatedDistanceToTarget} ft)",
            TrackingPhase.Lost => "Trail lost",
            TrackingPhase.Cold => "Trail cold",
            _ => "Unknown"
        };

        return $"Tracking [{TrackingId[..8]}...]: {TargetDescription} | {Status} | {phaseInfo}";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete tracking state details.</returns>
    public string ToDetailedString()
    {
        return $"TrackingState [{TrackingId}]\n" +
               $"  Tracker: {TrackerId}\n" +
               $"  Target: {TargetDescription}\n" +
               $"  Status: {Status}, Phase: {CurrentPhase}\n" +
               $"  Trail Age: {TrailAge} (Base DC: {BaseDc})\n" +
               $"  Cumulative Modifier: +{CumulativeDcModifier} (Effective DC: {EffectiveDc})\n" +
               $"  Distance Covered: {DistanceCovered:F2} miles\n" +
               $"  Failed Attempts in Phase: {FailedAttemptsInPhase}\n" +
               $"  Checks: {TotalChecks} total ({SuccessfulChecks} success, {FailedChecks} failed)\n" +
               $"  Started: {StartedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
               $"  Last Check: {LastCheckAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
               $"  Estimated Target Count: {EstimatedTargetCount?.ToString() ?? "Unknown"}\n" +
               $"  Estimated Distance: {EstimatedDistanceToTarget?.ToString() ?? "Unknown"} ft";
    }
}
