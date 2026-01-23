namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current status of a multi-stage climbing attempt.
/// </summary>
/// <remarks>
/// <para>
/// Climb status tracks the lifecycle of a climbing attempt from initiation
/// through completion, abandonment, or failure. The status determines what
/// actions are available to the character during their climb.
/// </para>
/// <para>
/// Status Flow:
/// <list type="bullet">
///   <item><description>InProgress → Completed (all stages cleared)</description></item>
///   <item><description>InProgress → Fallen (fumble triggered fall)</description></item>
///   <item><description>InProgress → Abandoned (player chose to quit)</description></item>
///   <item><description>InProgress → SlippedToGround (regressed past stage 0)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ClimbStatus
{
    /// <summary>
    /// Climb attempt is currently active with stages remaining.
    /// </summary>
    /// <remarks>
    /// Character is on the climbing surface and can attempt the next stage,
    /// abandon the climb, or continue climbing.
    /// </remarks>
    InProgress = 0,

    /// <summary>
    /// Climb attempt succeeded - character reached the target height.
    /// </summary>
    /// <remarks>
    /// All required stages have been completed. The character is now
    /// at the top of the climb and can continue movement.
    /// </remarks>
    Completed = 1,

    /// <summary>
    /// Character fell during the climb due to a fumble.
    /// </summary>
    /// <remarks>
    /// A fumble (0 successes + ≥1 botch) triggered [The Slip] consequence.
    /// Fall damage will be calculated based on the height reached.
    /// </remarks>
    Fallen = 2,

    /// <summary>
    /// Character voluntarily abandoned the climb.
    /// </summary>
    /// <remarks>
    /// Safe exit from the climb. Character returns to ground level
    /// without taking fall damage.
    /// </remarks>
    Abandoned = 3,

    /// <summary>
    /// Character slipped back to ground level through failures.
    /// </summary>
    /// <remarks>
    /// Repeated failures caused regression below stage 0. Character
    /// is back at ground level but did not take fall damage.
    /// </remarks>
    SlippedToGround = 4
}
