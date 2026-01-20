// ═══════════════════════════════════════════════════════════════════════════════
// IStatisticsService.cs
// Interface for the player statistics tracking and querying service.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for tracking and querying player statistics.
/// </summary>
/// <remarks>
/// <para>
/// The statistics service provides a centralized API for recording and retrieving
/// player statistics. It handles the creation of statistics entities for new players
/// and ensures all statistic updates are properly logged and tracked.
/// </para>
/// <para>This service provides methods for:</para>
/// <list type="bullet">
///   <item><description>Recording statistic updates (e.g., monster kills, damage dealt)</description></item>
///   <item><description>Querying individual statistics by name</description></item>
///   <item><description>Retrieving statistics grouped by category</description></item>
///   <item><description>Calculating derived metrics (averages, rates, combat rating)</description></item>
/// </list>
/// <para>
/// Game services (combat, exploration, crafting, etc.) should inject this service
/// and call the appropriate recording methods when gameplay events occur.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a combat service
/// public class CombatService
/// {
///     private readonly IStatisticsService _statisticsService;
///
///     public void ProcessAttack(Player player, Monster monster, AttackResult result)
///     {
///         _statisticsService.RecordDamageDealt(player, result.Damage, result.WasCritical);
///
///         if (monster.IsDead)
///         {
///             _statisticsService.RecordMonsterKill(player, monster.Type, monster.IsBoss);
///         }
///     }
/// }
///
/// // Query statistics
/// var kills = statisticsService.GetStatistic(player, "monstersKilled");
/// var combatStats = statisticsService.GetCategoryStatistics(player, StatisticCategory.Combat);
/// var metrics = statisticsService.GetMetrics(player);
/// </code>
/// </example>
public interface IStatisticsService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // RECORDING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Increments a named statistic by the specified amount.
    /// </summary>
    /// <param name="player">The player whose statistics to update.</param>
    /// <param name="statName">The name of the statistic to increment (case-insensitive).</param>
    /// <param name="amount">The amount to increment by (default 1, must be non-negative).</param>
    /// <remarks>
    /// <para>
    /// This is the generic method for incrementing any tracked statistic.
    /// For common operations, prefer using the specialized recording methods
    /// which provide additional validation and logging.
    /// </para>
    /// <para>Supported statistic names include:</para>
    /// <list type="bullet">
    ///   <item><description>Combat: monstersKilled, damageDealt, criticalHits, etc.</description></item>
    ///   <item><description>Exploration: roomsDiscovered, secretsFound, doorsOpened, etc.</description></item>
    ///   <item><description>Progression: xpEarned, levelsGained, itemsCrafted, etc.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// statisticsService.IncrementStat(player, "secretsFound");
    /// statisticsService.IncrementStat(player, "goldEarned", 100);
    /// </code>
    /// </example>
    void IncrementStat(Player player, string statName, int amount = 1);

    /// <summary>
    /// Records a monster kill with type tracking.
    /// </summary>
    /// <param name="player">The player who killed the monster.</param>
    /// <param name="monsterType">The type/name of the monster killed.</param>
    /// <param name="isBoss">Whether the monster was a boss (default false).</param>
    /// <remarks>
    /// <para>
    /// This method increments the total monsters killed counter and tracks
    /// the kill by monster type. If <paramref name="isBoss"/> is true,
    /// the boss kills counter is also incremented.
    /// </para>
    /// <para>
    /// Monster types are normalized to lowercase for consistent tracking.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Regular monster kill
    /// statisticsService.RecordMonsterKill(player, "goblin");
    ///
    /// // Boss kill
    /// statisticsService.RecordMonsterKill(player, "Dragon Lord", isBoss: true);
    /// </code>
    /// </example>
    void RecordMonsterKill(Player player, string monsterType, bool isBoss = false);

    /// <summary>
    /// Records damage dealt by the player.
    /// </summary>
    /// <param name="player">The player who dealt damage.</param>
    /// <param name="amount">The amount of damage dealt (must be non-negative).</param>
    /// <param name="wasCritical">Whether the hit was a critical hit.</param>
    /// <remarks>
    /// <para>
    /// This method updates total damage dealt, total attacks, and if
    /// <paramref name="wasCritical"/> is true, also increments the critical hits counter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Normal hit
    /// statisticsService.RecordDamageDealt(player, 25, wasCritical: false);
    ///
    /// // Critical hit
    /// statisticsService.RecordDamageDealt(player, 50, wasCritical: true);
    /// </code>
    /// </example>
    void RecordDamageDealt(Player player, int amount, bool wasCritical);

    /// <summary>
    /// Records damage received by the player.
    /// </summary>
    /// <param name="player">The player who received damage.</param>
    /// <param name="amount">The amount of damage received (must be non-negative).</param>
    /// <remarks>
    /// <para>
    /// This method updates the total damage received counter, which is used
    /// for calculating average damage received metrics.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// statisticsService.RecordDamageReceived(player, 15);
    /// </code>
    /// </example>
    void RecordDamageReceived(Player player, int amount);

    /// <summary>
    /// Records that the player discovered a room.
    /// </summary>
    /// <param name="player">The player who discovered the room.</param>
    /// <remarks>
    /// <para>
    /// Call this method when a player enters a room for the first time.
    /// Increments the rooms discovered counter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When entering a new room
    /// if (!room.HasBeenVisited)
    /// {
    ///     statisticsService.RecordRoomDiscovered(player);
    /// }
    /// </code>
    /// </example>
    void RecordRoomDiscovered(Player player);

    /// <summary>
    /// Records session playtime.
    /// </summary>
    /// <param name="player">The player whose playtime to record.</param>
    /// <param name="sessionTime">The duration of the play session.</param>
    /// <remarks>
    /// <para>
    /// Call this method at the end of a play session or periodically during
    /// gameplay to track active play time. The session time is added to
    /// the player's total playtime.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At the end of a session
    /// var sessionDuration = DateTime.UtcNow - sessionStartTime;
    /// statisticsService.RecordPlaytime(player, sessionDuration);
    /// </code>
    /// </example>
    void RecordPlaytime(Player player, TimeSpan sessionTime);

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a specific statistic value.
    /// </summary>
    /// <param name="player">The player whose statistic to retrieve.</param>
    /// <param name="statName">The name of the statistic (case-insensitive).</param>
    /// <returns>The statistic value, or 0 if the statistic is not found.</returns>
    /// <remarks>
    /// <para>
    /// Returns the current value of a named statistic. For unknown statistic
    /// names, returns 0 rather than throwing an exception.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var monstersKilled = statisticsService.GetStatistic(player, "monstersKilled");
    /// var totalDamage = statisticsService.GetStatistic(player, "damageDealt");
    /// </code>
    /// </example>
    long GetStatistic(Player player, string statName);

    /// <summary>
    /// Gets all statistics for a specific category.
    /// </summary>
    /// <param name="player">The player whose statistics to retrieve.</param>
    /// <param name="category">The category to filter by.</param>
    /// <returns>
    /// A read-only dictionary mapping statistic names to their values
    /// for all statistics in the specified category.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns statistics grouped by category (Combat, Exploration, Progression, Time).
    /// The Dice category is a placeholder for v0.12.0b and returns an empty dictionary.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var combatStats = statisticsService.GetCategoryStatistics(player, StatisticCategory.Combat);
    /// foreach (var (name, value) in combatStats)
    /// {
    ///     Console.WriteLine($"{name}: {value}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyDictionary<string, long> GetCategoryStatistics(Player player, StatisticCategory category);

    /// <summary>
    /// Gets the player's statistics entity.
    /// </summary>
    /// <param name="player">The player whose statistics to retrieve.</param>
    /// <returns>The PlayerStatistics entity for the player.</returns>
    /// <remarks>
    /// <para>
    /// Returns the underlying statistics entity. If the player does not have
    /// a statistics entity, one will be created and initialized automatically.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = statisticsService.GetPlayerStatistics(player);
    /// var killsByType = stats.MonstersByType;
    /// </code>
    /// </example>
    PlayerStatistics GetPlayerStatistics(Player player);

    /// <summary>
    /// Gets calculated metrics derived from raw statistics.
    /// </summary>
    /// <param name="player">The player whose metrics to calculate.</param>
    /// <returns>A <see cref="StatisticsMetrics"/> record containing calculated metrics.</returns>
    /// <remarks>
    /// <para>
    /// This method calculates derived metrics from raw statistics including:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Average damage per hit</description></item>
    ///   <item><description>Critical hit rate</description></item>
    ///   <item><description>Miss rate</description></item>
    ///   <item><description>Trap avoidance rate</description></item>
    ///   <item><description>Gold balance</description></item>
    ///   <item><description>Average session length</description></item>
    ///   <item><description>Combat rating</description></item>
    /// </list>
    /// <para>
    /// All rate calculations handle division by zero gracefully, returning 0.0.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = statisticsService.GetMetrics(player);
    /// Console.WriteLine($"Combat Rating: {metrics.CombatRating}");
    /// Console.WriteLine($"Critical Hit Rate: {metrics.CriticalHitRateDisplay}");
    /// </code>
    /// </example>
    StatisticsMetrics GetMetrics(Player player);
}
