namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Phases of the multi-stage tracking procedure.
/// </summary>
/// <remarks>
/// <para>
/// Tracking proceeds through distinct phases, each with different check
/// requirements and consequences:
/// </para>
/// <list type="bullet">
///   <item><description>Acquisition: Find the initial trail</description></item>
///   <item><description>Pursuit: Maintain trail over distance</description></item>
///   <item><description>ClosingIn: Final approach to target</description></item>
///   <item><description>Lost: Trail temporarily lost, recovery needed</description></item>
///   <item><description>Cold: Trail unrecoverable</description></item>
/// </list>
/// <para>
/// Phase transitions follow a state machine pattern where success advances
/// the pursuit and failure may transition to Lost or Cold phases.
/// </para>
/// </remarks>
public enum TrackingPhase
{
    /// <summary>
    /// Initial phase: Finding and identifying the trail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// First check to locate signs of passage. On success, automatically
    /// identifies trail type and direction. Optional additional checks
    /// can estimate target count (DC 10) and trail age (DC 12).
    /// </para>
    /// <para>
    /// Failure allows retry after 10 minutes at DC +2.
    /// Three failures mark trail as Cold (unfindable).
    /// </para>
    /// </remarks>
    Acquisition = 0,

    /// <summary>
    /// Following the trail across terrain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Subsequent checks to maintain the trail over distance.
    /// Check frequency depends on terrain:
    /// <list type="bullet">
    ///   <item><description>Open wasteland: Every 2 miles</description></item>
    ///   <item><description>Moderate ruins: Every 1 mile</description></item>
    ///   <item><description>Dense ruins: Every 0.5 miles</description></item>
    ///   <item><description>Labyrinthine: Every room/intersection</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Failure transitions to Lost phase.
    /// </para>
    /// </remarks>
    Pursuit = 1,

    /// <summary>
    /// Final approach phase when near the target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered when within 500 feet of target.
    /// DC modifiers by distance:
    /// <list type="bullet">
    ///   <item><description>Within 500 ft: -4 DC (target may detect pursuit)</description></item>
    ///   <item><description>Within 100 ft: -6 DC (contested vs target Perception)</description></item>
    ///   <item><description>Within 50 ft: Auto-success (immediate encounter)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Success locates target and initiates encounter.
    /// Failure may alert the target to pursuit.
    /// </para>
    /// </remarks>
    ClosingIn = 2,

    /// <summary>
    /// Trail has been lost; recovery procedures needed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entered when a pursuit check fails. Recovery options:
    /// <list type="bullet">
    ///   <item><description>Partial loss: Backtrack (10 min, same DC)</description></item>
    ///   <item><description>Full loss: Spiral search (30 min, +4 DC)</description></item>
    ///   <item><description>Critical loss: Return to last known (1 hr, +8 DC or new lead)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Three failed recovery attempts mark trail as Cold.
    /// </para>
    /// </remarks>
    Lost = 3,

    /// <summary>
    /// Trail is unrecoverable; tracking attempt has ended.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state indicating the trail cannot be followed further.
    /// Entered after 3 failed acquisition attempts or 3 failed recovery attempts.
    /// </para>
    /// <para>
    /// Player must find a new lead to resume tracking.
    /// </para>
    /// </remarks>
    Cold = 4
}
