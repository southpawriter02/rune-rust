namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the possible outcomes and states of a chase encounter.
/// </summary>
/// <remarks>
/// <para>
/// A chase begins in InProgress and ends when one of the terminal states
/// (Caught, Escaped, Abandoned, TimedOut) is reached. The status determines
/// what actions are available and how the chase should be resolved narratively.
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase status tracking.
/// </para>
/// </remarks>
public enum ChaseStatus
{
    /// <summary>
    /// The chase is ongoing. Both participants are still actively involved.
    /// </summary>
    /// <remarks>
    /// While InProgress, rounds continue to be processed and obstacles generated.
    /// Distance changes affect the outcome until a terminal state is reached.
    /// </remarks>
    InProgress = 0,

    /// <summary>
    /// The fleeing character has been caught. Distance reached 0 or below.
    /// </summary>
    /// <remarks>
    /// The pursuer has successfully closed the gap completely. Combat may resume
    /// or narrative consequences apply. The fleeing character cannot escape
    /// without a new opportunity.
    /// </remarks>
    Caught = 1,

    /// <summary>
    /// The fleeing character has escaped. Distance reached 6 or above.
    /// </summary>
    /// <remarks>
    /// The fleeing character has successfully evaded pursuit. The pursuer has
    /// lost the trail and cannot continue the chase without new information
    /// about the target's location.
    /// </remarks>
    Escaped = 2,

    /// <summary>
    /// One of the participants voluntarily abandoned the chase.
    /// </summary>
    /// <remarks>
    /// Either the pursuer gave up pursuit or the fleeing character stopped running.
    /// The chase ends without a definitive caught/escaped resolution.
    /// </remarks>
    Abandoned = 3,

    /// <summary>
    /// The chase exceeded its maximum round limit without resolution.
    /// </summary>
    /// <remarks>
    /// Only applies when MaxRounds is set on the ChaseState. The narrative
    /// context determines the outcome (e.g., fleeing character escapes by
    /// default, or situation changes).
    /// </remarks>
    TimedOut = 4
}
