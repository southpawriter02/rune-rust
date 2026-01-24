namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Overall status of a tracking pursuit.
/// </summary>
/// <remarks>
/// <para>
/// Tracks the high-level state of a tracking attempt:
/// <list type="bullet">
///   <item><description>Active: Pursuit is ongoing</description></item>
///   <item><description>Abandoned: Player chose to stop tracking</description></item>
///   <item><description>TargetFound: Target was successfully located</description></item>
///   <item><description>TrailCold: Trail is unrecoverable</description></item>
/// </list>
/// </para>
/// <para>
/// Distinct from <see cref="TrackingPhase"/> which tracks the current step
/// in the tracking procedure. Status indicates whether the overall pursuit
/// is still active or has reached a terminal state.
/// </para>
/// </remarks>
public enum TrackingStatus
{
    /// <summary>
    /// Tracking is actively in progress.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pursuit is ongoing and the player can continue making tracking checks.
    /// This is the default state for a newly initiated tracking attempt.
    /// </para>
    /// </remarks>
    Active = 0,

    /// <summary>
    /// Tracking was abandoned by the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player chose to stop tracking before finding the target.
    /// The tracking state is preserved in case the player wants to review history.
    /// </para>
    /// </remarks>
    Abandoned = 1,

    /// <summary>
    /// Target was successfully located.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pursuit reached a successful conclusion with the target found.
    /// This typically triggers an encounter or other game event.
    /// </para>
    /// </remarks>
    TargetFound = 2,

    /// <summary>
    /// Trail went cold and is unrecoverable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The trail cannot be followed further. This occurs after:
    /// <list type="bullet">
    ///   <item><description>3 failed acquisition attempts</description></item>
    ///   <item><description>3 failed recovery attempts</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Player must find a new lead to resume tracking the target.
    /// </para>
    /// </remarks>
    TrailCold = 3
}
