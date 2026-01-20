namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of a talent point allocation attempt.
/// </summary>
/// <remarks>
/// <para>AllocationResult provides detailed feedback about talent point spending operations,
/// including success/failure status, result type, and relevant context.</para>
/// <para>Possible outcomes:</para>
/// <list type="bullet">
///   <item><description>Success: Point was allocated to the node, rank increased</description></item>
///   <item><description>Failed: Generic failure with reason message</description></item>
///   <item><description>InsufficientPoints: Player lacks required points</description></item>
///   <item><description>AtMaxRank: Node is already at maximum rank</description></item>
///   <item><description>PrerequisitesNotMet: Node or stat prerequisites not satisfied</description></item>
/// </list>
/// </remarks>
/// <param name="IsSuccess">Whether the allocation was successful.</param>
/// <param name="ResultType">The type of result.</param>
/// <param name="NodeId">The node ID (if applicable).</param>
/// <param name="NewRank">The new rank after allocation (if successful).</param>
/// <param name="FailureReason">The reason for failure (if failed).</param>
public record AllocationResult(
    bool IsSuccess,
    AllocationResultType ResultType,
    string? NodeId,
    int? NewRank,
    string? FailureReason)
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful allocation result.
    /// </summary>
    /// <param name="nodeId">The ID of the node that received the point.</param>
    /// <param name="newRank">The node's new rank after allocation.</param>
    /// <returns>A successful AllocationResult.</returns>
    public static AllocationResult Success(string nodeId, int newRank)
        => new(true, AllocationResultType.Success, nodeId, newRank, null);

    /// <summary>
    /// Creates a generic failed allocation result.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    /// <returns>A failed AllocationResult.</returns>
    public static AllocationResult Failed(string reason)
        => new(false, AllocationResultType.Failed, null, null, reason);

    /// <summary>
    /// Creates a failed result due to insufficient talent points.
    /// </summary>
    /// <param name="required">The number of points required.</param>
    /// <param name="available">The number of points the player has.</param>
    /// <returns>An AllocationResult indicating insufficient points.</returns>
    public static AllocationResult InsufficientPoints(int required, int available)
        => new(false, AllocationResultType.InsufficientPoints, null, null,
            $"Need {required} points, have {available}");

    /// <summary>
    /// Creates a failed result because the node is at maximum rank.
    /// </summary>
    /// <param name="nodeId">The node ID that is maxed.</param>
    /// <param name="maxRank">The maximum rank of the node.</param>
    /// <returns>An AllocationResult indicating max rank reached.</returns>
    public static AllocationResult AtMaxRank(string nodeId, int maxRank)
        => new(false, AllocationResultType.AtMaxRank, nodeId, maxRank,
            $"Already at max rank {maxRank}");

    /// <summary>
    /// Creates a failed result because prerequisites are not met.
    /// </summary>
    /// <param name="reasons">The list of unmet prerequisites.</param>
    /// <returns>An AllocationResult indicating prerequisites not met.</returns>
    public static AllocationResult PrerequisitesNotMet(IEnumerable<string> reasons)
        => new(false, AllocationResultType.PrerequisitesNotMet, null, null,
            string.Join("; ", reasons));
}

/// <summary>
/// Types of talent point allocation results.
/// </summary>
/// <remarks>
/// Provides specific categorization of allocation outcomes for
/// easier handling by calling code.
/// </remarks>
public enum AllocationResultType
{
    /// <summary>
    /// Point was successfully allocated to the node.
    /// </summary>
    Success,

    /// <summary>
    /// Generic failure (see FailureReason for details).
    /// </summary>
    Failed,

    /// <summary>
    /// Player does not have enough unspent talent points.
    /// </summary>
    InsufficientPoints,

    /// <summary>
    /// Node is already at its maximum rank.
    /// </summary>
    AtMaxRank,

    /// <summary>
    /// Node and/or stat prerequisites are not satisfied.
    /// </summary>
    PrerequisitesNotMet
}
