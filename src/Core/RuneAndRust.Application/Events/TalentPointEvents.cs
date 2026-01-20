namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a player earns talent points.
/// </summary>
/// <remarks>
/// <para>Fired when talent points are awarded to a player, typically:</para>
/// <list type="bullet">
///   <item><description>On level-up via LevelUpService integration</description></item>
///   <item><description>From quest rewards or achievements</description></item>
///   <item><description>Via administrative commands</description></item>
/// </list>
/// </remarks>
/// <param name="PlayerId">The player who earned the points.</param>
/// <param name="PointsEarned">The number of points earned.</param>
/// <param name="TotalUnspent">The player's total unspent points after earning.</param>
public record TalentPointEarnedEvent(
    Guid PlayerId,
    int PointsEarned,
    int TotalUnspent);

/// <summary>
/// Published when a player spends a talent point.
/// </summary>
/// <remarks>
/// <para>Fired when a talent point is successfully allocated to a tree node.
/// For multi-rank nodes, this fires for each rank purchased.</para>
/// </remarks>
/// <param name="PlayerId">The player who spent the point.</param>
/// <param name="NodeId">The node that received the point.</param>
/// <param name="NewRank">The node's new rank after spending.</param>
/// <param name="PointsSpent">The number of points spent (node's point cost).</param>
public record TalentPointSpentEvent(
    Guid PlayerId,
    string NodeId,
    int NewRank,
    int PointsSpent);

/// <summary>
/// Published when a player unlocks a new ability via talent allocation.
/// </summary>
/// <remarks>
/// <para>Fired when a player allocates their first rank into a talent node,
/// unlocking the associated ability. Does not fire for subsequent ranks
/// of the same node.</para>
/// </remarks>
/// <param name="PlayerId">The player who unlocked the ability.</param>
/// <param name="AbilityId">The ability definition ID that was unlocked.</param>
/// <param name="FromNodeId">The talent node that granted the ability.</param>
public record AbilityUnlockedEvent(
    Guid PlayerId,
    string AbilityId,
    string FromNodeId);
