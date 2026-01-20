namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a player initiates a respec (talent point reallocation) operation.
/// </summary>
/// <remarks>
/// <para>Fired at the start of a respec operation before any changes are made.
/// This allows systems to prepare for the upcoming talent reset.</para>
/// <list type="bullet">
///   <item><description>Useful for UI feedback and progress indicators</description></item>
///   <item><description>Can be used for analytics tracking</description></item>
///   <item><description>Fired before validation checks complete</description></item>
/// </list>
/// </remarks>
/// <param name="PlayerId">The player requesting the respec.</param>
/// <param name="CurrentLevel">The player's level at the time of respec request.</param>
/// <param name="AllocationsCount">The number of talent allocations to be reset.</param>
/// <param name="EstimatedCost">The gold cost for the respec operation.</param>
public record RespecStartedEvent(
    Guid PlayerId,
    int CurrentLevel,
    int AllocationsCount,
    int EstimatedCost);

/// <summary>
/// Published when a respec operation completes successfully.
/// </summary>
/// <remarks>
/// <para>Fired after all respec operations have completed:</para>
/// <list type="bullet">
///   <item><description>Talent points have been refunded to the player</description></item>
///   <item><description>All talent allocations have been cleared</description></item>
///   <item><description>Gold cost has been deducted from the player</description></item>
///   <item><description>Abilities granted by talents have been removed</description></item>
/// </list>
/// <para>This event is NOT fired if the respec fails for any reason
/// (insufficient gold, disabled, level too low, etc.).</para>
/// </remarks>
/// <param name="PlayerId">The player who completed the respec.</param>
/// <param name="PointsRefunded">The total talent points returned to the player.</param>
/// <param name="GoldSpent">The gold cost deducted for the respec.</param>
/// <param name="AbilitiesRemoved">The number of abilities removed from the player.</param>
/// <param name="PlayerLevel">The player's level at the time of respec.</param>
public record RespecCompletedEvent(
    Guid PlayerId,
    int PointsRefunded,
    int GoldSpent,
    int AbilitiesRemoved,
    int PlayerLevel);

/// <summary>
/// Published when a respec operation fails.
/// </summary>
/// <remarks>
/// <para>Fired when a respec cannot be completed due to:</para>
/// <list type="bullet">
///   <item><description>Insufficient gold to pay the respec cost</description></item>
///   <item><description>Respec feature is disabled in configuration</description></item>
///   <item><description>Player level is below the minimum requirement</description></item>
///   <item><description>No talent allocations exist to reset</description></item>
/// </list>
/// <para>The FailureReason provides a user-friendly message explaining why
/// the respec could not be completed.</para>
/// </remarks>
/// <param name="PlayerId">The player whose respec failed.</param>
/// <param name="FailureReason">A human-readable explanation of the failure.</param>
/// <param name="ResultType">The specific type of failure that occurred.</param>
public record RespecFailedEvent(
    Guid PlayerId,
    string FailureReason,
    string ResultType);
