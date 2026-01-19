using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for handling talent point reallocation (respec) operations.
/// </summary>
/// <remarks>
/// <para>IRespecService provides the core API for the respec system:</para>
/// <list type="bullet">
///   <item><description>Query respec eligibility and costs</description></item>
///   <item><description>Execute respec operations</description></item>
///   <item><description>Preview respec effects before execution</description></item>
///   <item><description>Check configuration and player requirements</description></item>
/// </list>
/// <para>The service integrates with:</para>
/// <list type="bullet">
///   <item><description>IAbilityTreeProvider for determining abilities to remove</description></item>
///   <item><description>IRespecConfiguration for cost and eligibility settings</description></item>
///   <item><description>IGameEventLogger for event logging</description></item>
///   <item><description>Player's currency and talent allocation systems</description></item>
/// </list>
/// <para>Cost formula: BaseRespecCost + (PlayerLevel × LevelMultiplier)</para>
/// </remarks>
public interface IRespecService
{
    // ═══════════════════════════════════════════════════════════════
    // COST QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the gold cost for a player to respec.
    /// </summary>
    /// <param name="player">The player to calculate cost for.</param>
    /// <returns>The gold cost based on player level and configuration.</returns>
    /// <remarks>
    /// <para>Cost formula: BaseRespecCost + (Level × LevelMultiplier)</para>
    /// <para>Example with defaults (base=100, multiplier=10):</para>
    /// <list type="bullet">
    ///   <item><description>Level 2: 100 + (2 × 10) = 120 gold</description></item>
    ///   <item><description>Level 10: 100 + (10 × 10) = 200 gold</description></item>
    ///   <item><description>Level 20: 100 + (20 × 10) = 300 gold</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    int GetRespecCost(Player player);

    /// <summary>
    /// Checks if a player can afford the respec cost.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has enough gold; otherwise, false.</returns>
    /// <remarks>
    /// Compares the player's current gold against the calculated respec cost.
    /// Does not check other eligibility requirements like level or allocations.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    bool CanAffordRespec(Player player);

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a player has any talent allocations to reset.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has at least one allocation; otherwise, false.</returns>
    /// <remarks>
    /// Returns false if the player has never spent any talent points.
    /// This is a prerequisite for performing a respec.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    bool HasAllocations(Player player);

    /// <summary>
    /// Gets the total points that would be refunded by a respec.
    /// </summary>
    /// <param name="player">The player to calculate refund for.</param>
    /// <returns>The total talent points that would be returned.</returns>
    /// <remarks>
    /// <para>Calculates the sum of all points spent across all allocations.
    /// For multi-rank nodes, this counts the total points invested (rank × point cost).</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    int GetRefundAmount(Player player);

    /// <summary>
    /// Gets the list of ability IDs that would be removed by a respec.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>A read-only list of ability IDs granted by talent allocations.</returns>
    /// <remarks>
    /// <para>Returns abilities granted by the player's current talent allocations.
    /// These abilities will be removed from the player during respec since
    /// they were unlocked via the talent tree.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    IReadOnlyList<string> GetAbilitiesToRemove(Player player);

    // ═══════════════════════════════════════════════════════════════
    // RESPEC OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs a complete respec for a player.
    /// </summary>
    /// <param name="player">The player to respec.</param>
    /// <returns>A RespecResult indicating success or failure with details.</returns>
    /// <remarks>
    /// <para>Validates the following before performing respec:</para>
    /// <list type="bullet">
    ///   <item><description>Respec feature is enabled in configuration</description></item>
    ///   <item><description>Player meets minimum level requirement</description></item>
    ///   <item><description>Player has allocations to reset</description></item>
    ///   <item><description>Player can afford the gold cost</description></item>
    /// </list>
    /// <para>On success, the following operations are performed in order:</para>
    /// <list type="bullet">
    ///   <item><description>Refunds all spent talent points to unspent pool</description></item>
    ///   <item><description>Clears all talent allocations</description></item>
    ///   <item><description>Deducts gold cost from player</description></item>
    ///   <item><description>Removes abilities granted by talents</description></item>
    ///   <item><description>Publishes RespecCompletedEvent</description></item>
    /// </list>
    /// <para>On failure, returns a RespecResult with the specific failure reason.
    /// No changes are made to the player's state on failure.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    RespecResult Respec(Player player);
}
