using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for executing multi-phase tracking pursuits.
/// </summary>
/// <remarks>
/// <para>
/// The tracking service implements the Extended Tracking System mechanics for the
/// Wasteland Survival skill. It manages the complete tracking lifecycle:
/// <list type="bullet">
///   <item><description>Initiating tracking (Acquisition phase)</description></item>
///   <item><description>Continuing pursuit across terrain</description></item>
///   <item><description>Recovering lost trails</description></item>
///   <item><description>Closing in on targets</description></item>
/// </list>
/// </para>
/// <para>
/// The service integrates with:
/// <list type="bullet">
///   <item><description>SkillCheckService for dice rolls</description></item>
///   <item><description><see cref="ITrackingStateRepository"/> for persistence</description></item>
///   <item><description>Configuration for trail ages and modifiers</description></item>
/// </list>
/// </para>
/// <para>
/// Phase transitions follow a state machine pattern:
/// <code>
/// Acquisition → Pursuit → ClosingIn → TargetFound
///      ↓            ↓           ↓
///      └──────── Lost ←────────┘
///                  ↓
///                Cold
/// </code>
/// </para>
/// </remarks>
public interface ITrackingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY TRACKING OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initiates a new tracking pursuit.
    /// </summary>
    /// <param name="player">The player initiating tracking.</param>
    /// <param name="targetDescription">Description of what to track.</param>
    /// <param name="trailAge">Age classification of the trail.</param>
    /// <param name="modifiers">Environmental and condition modifiers.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result of the acquisition check.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player, targetDescription, or modifiers is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the player already has an active tracking pursuit.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs an Acquisition phase check using Wasteland Survival skill.
    /// On success:
    /// <list type="bullet">
    ///   <item><description>Trail found and identified</description></item>
    ///   <item><description>Direction automatically determined</description></item>
    ///   <item><description>Transitions to Pursuit phase</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On failure:
    /// <list type="bullet">
    ///   <item><description>May retry after 10 minutes at DC +2</description></item>
    ///   <item><description>3 failures mark trail as Cold</description></item>
    ///   <item><description>Fumble immediately marks trail as Cold</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Cold trails (DC 28) require Master rank (Rank 5) in Wasteland Survival.
    /// </para>
    /// </remarks>
    Task<TrackingResult> InitiateTrackingAsync(
        Player player,
        string targetDescription,
        TrailAge trailAge,
        TrackingModifiers modifiers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues an active pursuit with another check.
    /// </summary>
    /// <param name="player">The player continuing pursuit.</param>
    /// <param name="trackingId">ID of the active tracking state.</param>
    /// <param name="modifiers">Current condition modifiers.</param>
    /// <param name="distanceAdvanced">Distance traveled since last check (in miles).</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result of the pursuit check.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player, trackingId, or modifiers is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when tracking not found, not active, or not in Pursuit phase.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Check frequency depends on terrain (from <see cref="TrackingModifiers"/>):
    /// <list type="bullet">
    ///   <item><description>Open wasteland: Every 2 miles</description></item>
    ///   <item><description>Moderate ruins: Every 1 mile</description></item>
    ///   <item><description>Dense ruins: Every 0.5 miles</description></item>
    ///   <item><description>Labyrinthine: Every room/intersection (~0.1 miles)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On success: Pursuit continues, distance updated.
    /// On failure: Transitions to Lost phase.
    /// </para>
    /// <para>
    /// When within 500 feet of target, automatically transitions to ClosingIn phase.
    /// </para>
    /// </remarks>
    Task<TrackingResult> ContinuePursuitAsync(
        Player player,
        string trackingId,
        TrackingModifiers modifiers,
        float distanceAdvanced,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to recover a lost trail.
    /// </summary>
    /// <param name="player">The player attempting recovery.</param>
    /// <param name="trackingId">ID of the tracking state.</param>
    /// <param name="recoveryType">Type of recovery procedure to use.</param>
    /// <param name="modifiers">Current condition modifiers.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result of the recovery attempt.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player, trackingId, or modifiers is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when tracking not found or not in Lost phase.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Recovery procedures and their DC modifiers:
    /// <list type="bullet">
    ///   <item><description>Backtrack: Same DC, 10 minutes</description></item>
    ///   <item><description>SpiralSearch: +4 DC, 30 minutes</description></item>
    ///   <item><description>ReturnToLastKnown: +8 DC, 1 hour</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On success: Returns to Pursuit phase.
    /// On failure: Increments failed attempts counter.
    /// After 3 failures: Trail goes Cold.
    /// </para>
    /// </remarks>
    Task<TrackingResult> AttemptRecoveryAsync(
        Player player,
        string trackingId,
        RecoveryType recoveryType,
        TrackingModifiers modifiers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a closing in check when near the target.
    /// </summary>
    /// <param name="player">The player closing in.</param>
    /// <param name="trackingId">ID of the tracking state.</param>
    /// <param name="modifiers">Current condition modifiers.</param>
    /// <param name="currentDistanceFeet">Current estimated distance to target in feet.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result of the closing check.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player, trackingId, or modifiers is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when tracking not found or not in ClosingIn phase.
    /// </exception>
    /// <remarks>
    /// <para>
    /// DC modifiers based on distance:
    /// <list type="bullet">
    ///   <item><description>Within 500 ft: -4 DC (target may detect pursuit)</description></item>
    ///   <item><description>Within 100 ft: -6 DC (contested vs target Perception)</description></item>
    ///   <item><description>Within 50 ft: Auto-success (immediate encounter)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On success: Target located, encounter begins, tracking complete.
    /// On failure: Target may be alerted, transitions to Lost.
    /// </para>
    /// </remarks>
    Task<TrackingResult> CloseInAsync(
        Player player,
        string trackingId,
        TrackingModifiers modifiers,
        int currentDistanceFeet,
        CancellationToken cancellationToken = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE QUERY OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current state of a tracking pursuit.
    /// </summary>
    /// <param name="trackingId">ID of the tracking state.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The tracking state, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when trackingId is null or empty.
    /// </exception>
    Task<TrackingState?> GetTrackingStateAsync(string trackingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active tracking for a player.
    /// </summary>
    /// <param name="playerId">The player's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Active tracking state, or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when playerId is null or empty.
    /// </exception>
    /// <remarks>
    /// A player can only have one active tracking pursuit at a time.
    /// </remarks>
    Task<TrackingState?> GetActiveTrackingAsync(string playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a player has an active tracking pursuit.
    /// </summary>
    /// <param name="playerId">The player's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True if the player has an active pursuit, false otherwise.</returns>
    Task<bool> HasActiveTrackingAsync(string playerId, CancellationToken cancellationToken = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING MANAGEMENT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Abandons an active tracking pursuit.
    /// </summary>
    /// <param name="trackingId">ID of the tracking to abandon.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when trackingId is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when tracking not found or already completed.
    /// </exception>
    /// <remarks>
    /// Sets the tracking status to Abandoned. The tracking history is preserved.
    /// The player can initiate a new tracking pursuit after abandoning.
    /// </remarks>
    Task AbandonTrackingAsync(string trackingId, CancellationToken cancellationToken = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTIONAL ESTIMATION CHECKS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to estimate target count (optional acquisition check).
    /// </summary>
    /// <param name="player">The player making the estimate.</param>
    /// <param name="trackingId">ID of the tracking state.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Estimated count on success, null on failure.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player or trackingId is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// DC 10 contested check. Can only be performed after successful acquisition.
    /// </para>
    /// <para>
    /// Provides an estimate of how many targets are leaving the trail.
    /// The accuracy varies based on success margin.
    /// </para>
    /// </remarks>
    Task<int?> EstimateTargetCountAsync(Player player, string trackingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to estimate trail age (optional acquisition check).
    /// </summary>
    /// <param name="player">The player making the estimate.</param>
    /// <param name="trackingId">ID of the tracking state.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Estimated age on success, null on failure.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player or trackingId is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// DC 12 check. Can only be performed after successful acquisition.
    /// </para>
    /// <para>
    /// Provides an estimate of how old the trail is, which may help
    /// predict how far ahead the target is.
    /// </para>
    /// </remarks>
    Task<TrailAge?> EstimateTrailAgeAsync(Player player, string trackingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Types of recovery procedures available when a trail is lost.
/// </summary>
/// <remarks>
/// <para>
/// Each recovery type has different time costs and DC modifiers:
/// <list type="bullet">
///   <item><description>Backtrack: Same DC, 10 minutes - retrace steps</description></item>
///   <item><description>SpiralSearch: +4 DC, 30 minutes - expand search area</description></item>
///   <item><description>ReturnToLastKnown: +8 DC, 1 hour - go back to last confirmed position</description></item>
/// </list>
/// </para>
/// <para>
/// The choice of recovery type should consider:
/// <list type="bullet">
///   <item><description>Time pressure (is the target getting away?)</description></item>
///   <item><description>Character's skill level (can they handle higher DC?)</description></item>
///   <item><description>Available resources (do they have time for longer searches?)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum RecoveryType
{
    /// <summary>
    /// Backtrack to where trail was last confirmed (same DC, 10 minutes).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The quickest option with no DC penalty. The tracker retraces their
    /// steps to find where they lost the trail.
    /// </para>
    /// <para>
    /// Best used when the trail was recently lost and the tracker is confident
    /// about where they went wrong.
    /// </para>
    /// </remarks>
    Backtrack = 0,

    /// <summary>
    /// Search in expanding spiral pattern (+4 DC, 30 minutes).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A methodical search pattern covering a wider area. More thorough
    /// than backtracking but takes longer and is harder.
    /// </para>
    /// <para>
    /// Best used when unsure where the trail was lost or when backtracking
    /// has already failed.
    /// </para>
    /// </remarks>
    SpiralSearch = 1,

    /// <summary>
    /// Return to last known confirmed position (+8 DC, 1 hour).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The most time-consuming option but most thorough. The tracker
    /// returns to a confirmed trail location and starts fresh.
    /// </para>
    /// <para>
    /// Best used as a last resort when other recovery methods have failed,
    /// or when the tracker is completely disoriented.
    /// </para>
    /// </remarks>
    ReturnToLastKnown = 2
}
