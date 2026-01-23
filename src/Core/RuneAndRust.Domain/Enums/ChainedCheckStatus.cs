namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the current status of a chained skill check.
/// </summary>
/// <remarks>
/// <para>
/// Chained checks progress through multiple steps. This enum tracks the overall
/// state of the chain, not individual step outcomes.
/// </para>
/// </remarks>
public enum ChainedCheckStatus
{
    /// <summary>
    /// The chain has not started processing steps yet.
    /// </summary>
    /// <remarks>
    /// Initial state after creating a ChainedCheckState entity.
    /// </remarks>
    NotStarted = 0,

    /// <summary>
    /// The chain is currently in progress with steps remaining.
    /// </summary>
    /// <remarks>
    /// At least one step has been attempted, and the chain has not yet
    /// succeeded or failed.
    /// </remarks>
    InProgress = 1,

    /// <summary>
    /// The current step failed but retries are available.
    /// </summary>
    /// <remarks>
    /// The player can choose to retry the current step or abandon the chain.
    /// </remarks>
    AwaitingRetry = 2,

    /// <summary>
    /// All steps completed successfully.
    /// </summary>
    /// <remarks>
    /// Terminal state. The chain cannot be continued or retried.
    /// </remarks>
    Succeeded = 3,

    /// <summary>
    /// The chain failed and cannot continue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state. Occurs when:
    /// <list type="bullet">
    ///   <item><description>A step fails with no retries remaining</description></item>
    ///   <item><description>A fumble occurs on a step with no retry option</description></item>
    ///   <item><description>The player abandons the chain</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Failed = 4
}
