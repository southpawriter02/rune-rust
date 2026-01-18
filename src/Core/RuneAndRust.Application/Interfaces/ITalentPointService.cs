using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing talent point allocation and tracking.
/// </summary>
/// <remarks>
/// <para>ITalentPointService provides the core API for the talent point economy:</para>
/// <list type="bullet">
///   <item><description>Query point status (unspent, earned, spent)</description></item>
///   <item><description>Award points (typically on level-up)</description></item>
///   <item><description>Spend points on talent tree nodes</description></item>
///   <item><description>Track allocations per player</description></item>
///   <item><description>Check spending eligibility</description></item>
/// </list>
/// <para>The service integrates with:</para>
/// <list type="bullet">
///   <item><description>IAbilityTreeProvider for node definitions</description></item>
///   <item><description>IPrerequisiteValidator for prerequisite checks</description></item>
///   <item><description>IGameEventLogger for event logging</description></item>
/// </list>
/// </remarks>
public interface ITalentPointService
{
    // ═══════════════════════════════════════════════════════════════
    // POINT QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unspent talent points for a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>The number of unspent talent points.</returns>
    int GetUnspentPoints(Player player);

    /// <summary>
    /// Gets the total talent points ever earned by a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>The total points earned (never decreases).</returns>
    int GetTotalPointsEarned(Player player);

    /// <summary>
    /// Gets the total talent points currently spent by a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>The sum of points invested in all nodes.</returns>
    int GetTotalPointsSpent(Player player);

    /// <summary>
    /// Gets a summary of a player's talent point status.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>A TalentPointSummary with all point statistics.</returns>
    TalentPointSummary GetPointSummary(Player player);

    // ═══════════════════════════════════════════════════════════════
    // POINT OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Awards talent points to a player.
    /// </summary>
    /// <param name="player">The player to award points to.</param>
    /// <param name="count">The number of points to award (must be positive).</param>
    /// <remarks>
    /// <para>Typically called by LevelUpService when a player levels up.</para>
    /// <para>Publishes TalentPointEarnedEvent on success.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is not positive.</exception>
    void AwardPoints(Player player, int count);

    /// <summary>
    /// Spends a point on a talent tree node.
    /// </summary>
    /// <param name="player">The player spending the point.</param>
    /// <param name="nodeId">The ID of the node to allocate to.</param>
    /// <returns>An AllocationResult indicating success or failure.</returns>
    /// <remarks>
    /// <para>Validates the following before allocating:</para>
    /// <list type="bullet">
    ///   <item><description>Node exists in a talent tree</description></item>
    ///   <item><description>Player has sufficient unspent points</description></item>
    ///   <item><description>Node is not already at max rank</description></item>
    ///   <item><description>All prerequisites are met (via IPrerequisiteValidator)</description></item>
    /// </list>
    /// <para>On success:</para>
    /// <list type="bullet">
    ///   <item><description>Creates or updates the TalentAllocation</description></item>
    ///   <item><description>Deducts points from player</description></item>
    ///   <item><description>Publishes TalentPointSpentEvent</description></item>
    ///   <item><description>Publishes AbilityUnlockedEvent (if first rank)</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentException">Thrown when nodeId is null or whitespace.</exception>
    AllocationResult SpendPoint(Player player, string nodeId);

    // ═══════════════════════════════════════════════════════════════
    // ALLOCATION QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the allocation for a specific node.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="nodeId">The node ID to look up.</param>
    /// <returns>The TalentAllocation if the player has invested, null otherwise.</returns>
    TalentAllocation? GetAllocation(Player player, string nodeId);

    /// <summary>
    /// Gets the current rank of a node for a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="nodeId">The node ID to look up.</param>
    /// <returns>The current rank (0 if not allocated).</returns>
    int GetNodeRank(Player player, string nodeId);

    /// <summary>
    /// Gets all allocations for a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>A read-only list of all talent allocations.</returns>
    IReadOnlyList<TalentAllocation> GetAllAllocations(Player player);

    /// <summary>
    /// Gets allocations for a specific ability tree.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="treeId">The tree ID to filter by.</param>
    /// <returns>A read-only list of allocations within the specified tree.</returns>
    IReadOnlyList<TalentAllocation> GetAllocationsForTree(Player player, string treeId);

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY CHECKS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a player can spend on a specific node.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if the player can allocate to this node; otherwise, false.</returns>
    /// <remarks>
    /// <para>Returns true only if all conditions are met:</para>
    /// <list type="bullet">
    ///   <item><description>Node exists</description></item>
    ///   <item><description>Player has enough points</description></item>
    ///   <item><description>Node is not at max rank</description></item>
    ///   <item><description>All prerequisites are met</description></item>
    /// </list>
    /// </remarks>
    bool CanSpendOn(Player player, string nodeId);

    /// <summary>
    /// Gets all nodes that a player can currently spend on.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>A read-only list of nodes the player can allocate to.</returns>
    /// <remarks>
    /// Filters by the player's class tree and checks all eligibility criteria.
    /// </remarks>
    IReadOnlyList<AbilityTreeNode> GetAvailableNodes(Player player);
}
