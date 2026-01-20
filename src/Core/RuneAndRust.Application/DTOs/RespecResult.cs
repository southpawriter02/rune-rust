namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of a respec (talent point reallocation) operation.
/// </summary>
/// <remarks>
/// <para>RespecResult provides comprehensive feedback about the respec operation:</para>
/// <list type="bullet">
///   <item><description>Success status and specific result type for UI feedback</description></item>
///   <item><description>Points refunded and gold spent on success</description></item>
///   <item><description>Number of abilities removed from the player</description></item>
///   <item><description>Detailed failure reasons for user display</description></item>
/// </list>
/// <para>Use the factory methods to create instances:</para>
/// <code>
/// var success = RespecResult.Success(8, 200, 4);
/// var failed = RespecResult.CannotAfford(200, 150);
/// </code>
/// </remarks>
public record RespecResult(
    bool IsSuccess,
    RespecResultType ResultType,
    int PointsRefunded,
    int GoldSpent,
    int AbilitiesRemoved,
    string? FailureReason)
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful respec result.
    /// </summary>
    /// <param name="pointsRefunded">Total talent points returned to the player.</param>
    /// <param name="goldSpent">Gold deducted for the respec operation.</param>
    /// <param name="abilitiesRemoved">Number of abilities removed from the player.</param>
    /// <returns>A successful RespecResult.</returns>
    /// <remarks>
    /// Called when respec completes successfully. The player's allocations
    /// have been cleared, points refunded, gold deducted, and abilities removed.
    /// </remarks>
    public static RespecResult Success(int pointsRefunded, int goldSpent, int abilitiesRemoved)
        => new(
            IsSuccess: true,
            ResultType: RespecResultType.Success,
            PointsRefunded: pointsRefunded,
            GoldSpent: goldSpent,
            AbilitiesRemoved: abilitiesRemoved,
            FailureReason: null);

    /// <summary>
    /// Creates a result indicating the player cannot afford the respec.
    /// </summary>
    /// <param name="costRequired">The gold cost of the respec.</param>
    /// <param name="goldAvailable">The gold the player currently has.</param>
    /// <returns>A failed RespecResult with CannotAfford type.</returns>
    /// <remarks>
    /// The failure reason includes both required and available amounts
    /// for clear user feedback.
    /// </remarks>
    public static RespecResult CannotAfford(int costRequired, int goldAvailable)
        => new(
            IsSuccess: false,
            ResultType: RespecResultType.CannotAfford,
            PointsRefunded: 0,
            GoldSpent: 0,
            AbilitiesRemoved: 0,
            FailureReason: $"Respec costs {costRequired} gold, but you only have {goldAvailable}");

    /// <summary>
    /// Creates a result indicating there are no allocations to reset.
    /// </summary>
    /// <returns>A failed RespecResult with NoAllocations type.</returns>
    /// <remarks>
    /// This is not necessarily an error - the player simply has no
    /// talent points invested to refund.
    /// </remarks>
    public static RespecResult NothingToRespec()
        => new(
            IsSuccess: false,
            ResultType: RespecResultType.NoAllocations,
            PointsRefunded: 0,
            GoldSpent: 0,
            AbilitiesRemoved: 0,
            FailureReason: "No talent allocations to reset");

    /// <summary>
    /// Creates a result indicating the respec feature is disabled.
    /// </summary>
    /// <returns>A failed RespecResult with Disabled type.</returns>
    /// <remarks>
    /// Returned when <see cref="Interfaces.IRespecConfiguration.IsRespecEnabled"/>
    /// is set to false.
    /// </remarks>
    public static RespecResult Disabled()
        => new(
            IsSuccess: false,
            ResultType: RespecResultType.Disabled,
            PointsRefunded: 0,
            GoldSpent: 0,
            AbilitiesRemoved: 0,
            FailureReason: "Respec is currently disabled");

    /// <summary>
    /// Creates a result indicating the player's level is too low.
    /// </summary>
    /// <param name="requiredLevel">The minimum level required for respec.</param>
    /// <param name="currentLevel">The player's current level.</param>
    /// <returns>A failed RespecResult with LevelTooLow type.</returns>
    /// <remarks>
    /// Players must reach a minimum level (typically 2) before respec
    /// becomes available.
    /// </remarks>
    public static RespecResult LevelTooLow(int requiredLevel, int currentLevel)
        => new(
            IsSuccess: false,
            ResultType: RespecResultType.LevelTooLow,
            PointsRefunded: 0,
            GoldSpent: 0,
            AbilitiesRemoved: 0,
            FailureReason: $"Must be level {requiredLevel} to respec (currently level {currentLevel})");
}

/// <summary>
/// Types of respec operation results.
/// </summary>
/// <remarks>
/// Provides specific categorization of respec outcomes for
/// UI display and programmatic handling.
/// </remarks>
public enum RespecResultType
{
    /// <summary>
    /// Respec completed successfully. All allocations cleared,
    /// points refunded, gold deducted, and abilities removed.
    /// </summary>
    Success,

    /// <summary>
    /// Player does not have enough gold for the respec cost.
    /// </summary>
    CannotAfford,

    /// <summary>
    /// Player has no talent allocations to reset.
    /// </summary>
    NoAllocations,

    /// <summary>
    /// Respec feature is currently disabled in configuration.
    /// </summary>
    Disabled,

    /// <summary>
    /// Player has not reached the minimum level for respec.
    /// </summary>
    LevelTooLow
}
