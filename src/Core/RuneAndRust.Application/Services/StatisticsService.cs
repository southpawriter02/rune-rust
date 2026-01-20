// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsService.cs
// Service implementation for tracking and querying player statistics.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for tracking and querying player statistics.
/// </summary>
/// <remarks>
/// <para>
/// This service provides a centralized API for all statistics operations.
/// It handles the creation of <see cref="PlayerStatistics"/> entities for
/// new players and ensures proper logging of all statistic updates.
/// </para>
/// <para>This service:</para>
/// <list type="bullet">
///   <item><description>Updates statistics on the <see cref="PlayerStatistics"/> entity</description></item>
///   <item><description>Groups statistics by <see cref="StatisticCategory"/></description></item>
///   <item><description>Calculates derived metrics via <see cref="GetMetrics"/></description></item>
///   <item><description>Computes combat rating from K/D ratio, crit rate, and boss kills</description></item>
/// </list>
/// <para>
/// Game services (combat, exploration, crafting, etc.) should inject this service
/// and call the appropriate recording methods when gameplay events occur.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a combat service
/// _statisticsService.RecordDamageDealt(player, damage, wasCritical);
/// _statisticsService.RecordMonsterKill(player, monster.Type, monster.IsBoss);
///
/// // Query statistics
/// var metrics = _statisticsService.GetMetrics(player);
/// Console.WriteLine($"Combat Rating: {metrics.CombatRating}");
/// </code>
/// </example>
public class StatisticsService : IStatisticsService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger instance for diagnostics and debugging.
    /// </summary>
    private readonly ILogger<StatisticsService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public StatisticsService(ILogger<StatisticsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("StatisticsService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECORDING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="PlayerStatistics.IncrementStat"/> after ensuring
    /// the player has a statistics entity. Logs the increment operation at Debug level.
    /// </para>
    /// </remarks>
    public void IncrementStat(Player player, string statName, int amount = 1)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(statName, nameof(statName));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Incrementing {StatName} by {Amount} for player {PlayerId}",
            statName,
            amount,
            player.Id);

        stats.IncrementStat(statName, amount);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Records the monster kill on the statistics entity, then separately
    /// increments the boss kills counter if <paramref name="isBoss"/> is true.
    /// </para>
    /// </remarks>
    public void RecordMonsterKill(Player player, string monsterType, bool isBoss = false)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(monsterType, nameof(monsterType));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Recording monster kill: {MonsterType} (boss: {IsBoss}) for player {PlayerId}",
            monsterType,
            isBoss,
            player.Id);

        stats.RecordMonsterKill(monsterType);

        if (isBoss)
        {
            stats.IncrementStat("bossesKilled");

            _logger.LogDebug(
                "Boss kill recorded for player {PlayerId}. Total bosses killed: {BossesKilled}",
                player.Id,
                stats.BossesKilled);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Updates damage dealt, total attacks, and optionally critical hits.
    /// The total attacks counter is always incremented to maintain accurate rate calculations.
    /// </para>
    /// </remarks>
    public void RecordDamageDealt(Player player, int amount, bool wasCritical)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Recording damage dealt: {Amount} (critical: {WasCritical}) for player {PlayerId}",
            amount,
            wasCritical,
            player.Id);

        stats.IncrementStat("damageDealt", amount);
        stats.IncrementStat("totalAttacks");

        if (wasCritical)
        {
            stats.IncrementStat("criticalHits");

            _logger.LogDebug(
                "Critical hit recorded for player {PlayerId}. Total crits: {CriticalHits}",
                player.Id,
                stats.CriticalHits);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Records the damage amount to the total damage received counter.
    /// This is used for calculating average damage received metrics.
    /// </para>
    /// </remarks>
    public void RecordDamageReceived(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Recording damage received: {Amount} for player {PlayerId}",
            amount,
            player.Id);

        stats.IncrementStat("damageReceived", amount);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Increments the rooms discovered counter. Call this when the player
    /// enters a room they haven't visited before.
    /// </para>
    /// </remarks>
    public void RecordRoomDiscovered(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Recording room discovered for player {PlayerId}. Total rooms: {RoomsDiscovered}",
            player.Id,
            stats.RoomsDiscovered + 1);

        stats.IncrementStat("roomsDiscovered");
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Adds the session time to the player's total playtime and updates
    /// the last played timestamp. Logged at Information level since this
    /// is typically called infrequently (end of session).
    /// </para>
    /// </remarks>
    public void RecordPlaytime(Player player, TimeSpan sessionTime)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var stats = GetOrCreateStatistics(player);

        _logger.LogInformation(
            "Recording playtime: {SessionTime} for player {PlayerId}. Total playtime: {TotalPlaytime}",
            sessionTime,
            player.Id,
            stats.TotalPlaytime + sessionTime);

        stats.RecordPlaytime(sessionTime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="PlayerStatistics.GetStatistic"/> which returns 0
    /// for unknown statistic names.
    /// </para>
    /// </remarks>
    public long GetStatistic(Player player, string statName)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(statName, nameof(statName));

        var stats = GetOrCreateStatistics(player);
        return stats.GetStatistic(statName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Retrieves all statistics belonging to the specified category.
    /// Uses <see cref="GetStatNamesForCategory"/> to determine which
    /// statistics belong to each category.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, long> GetCategoryStatistics(
        Player player,
        StatisticCategory category)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var stats = GetOrCreateStatistics(player);
        var result = new Dictionary<string, long>();

        var statNames = GetStatNamesForCategory(category);
        foreach (var statName in statNames)
        {
            result[statName] = stats.GetStatistic(statName);
        }

        _logger.LogDebug(
            "Retrieved {Count} statistics for category {Category} for player {PlayerId}",
            result.Count,
            category,
            player.Id);

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Returns the underlying <see cref="PlayerStatistics"/> entity, creating
    /// it if it doesn't exist. Use this when you need access to properties
    /// not exposed through the service interface (e.g., MonstersByType).
    /// </para>
    /// </remarks>
    public PlayerStatistics GetPlayerStatistics(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        return GetOrCreateStatistics(player);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Calculates all derived metrics from raw statistics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>AverageDamagePerHit: Total damage / total attacks</description></item>
    ///   <item><description>CriticalHitRate: Critical hits / total attacks</description></item>
    ///   <item><description>MissRate: Attacks missed / total attacks</description></item>
    ///   <item><description>TrapAvoidanceRate: Traps avoided / total traps</description></item>
    ///   <item><description>GoldBalance: Gold earned - gold spent</description></item>
    ///   <item><description>AverageSessionLength: Total playtime / session count</description></item>
    ///   <item><description>CombatRating: Calculated from K/D, crits, misses, bosses</description></item>
    /// </list>
    /// <para>
    /// All division operations are protected against division by zero.
    /// </para>
    /// </remarks>
    public StatisticsMetrics GetMetrics(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var stats = GetOrCreateStatistics(player);

        _logger.LogDebug(
            "Calculating metrics for player {PlayerId}",
            player.Id);

        // Calculate successful hits (total attacks minus misses)
        var successfulHits = stats.TotalAttacks - stats.AttacksMissed;

        // Calculate average damage per hit (avoid division by zero)
        var avgDamagePerHit = successfulHits > 0
            ? (double)stats.TotalDamageDealt / successfulHits
            : 0.0;

        // Calculate average damage received
        // Use a reasonable estimate of hits taken based on damage received
        // For simplicity, assume average hit deals ~10 damage
        var estimatedHitsTaken = stats.TotalDamageReceived > 0
            ? Math.Max(1, stats.TotalDamageReceived / 10)
            : 0;
        var avgDamageReceived = estimatedHitsTaken > 0
            ? (double)stats.TotalDamageReceived / estimatedHitsTaken
            : 0.0;

        // Calculate critical hit rate
        var critRate = stats.TotalAttacks > 0
            ? (double)stats.CriticalHits / stats.TotalAttacks
            : 0.0;

        // Calculate miss rate
        var missRate = stats.TotalAttacks > 0
            ? (double)stats.AttacksMissed / stats.TotalAttacks
            : 0.0;

        // Calculate trap avoidance rate
        var totalTraps = stats.TrapsTriggered + stats.TrapsAvoided;
        var trapAvoidRate = totalTraps > 0
            ? (double)stats.TrapsAvoided / totalTraps
            : 0.0;

        // Calculate gold balance
        var goldBalance = stats.GoldEarned - stats.GoldSpent;

        // Calculate average session length
        var avgSessionLength = stats.SessionCount > 0
            ? TimeSpan.FromTicks(stats.TotalPlaytime.Ticks / stats.SessionCount)
            : TimeSpan.Zero;

        // Calculate combat rating
        var combatRating = CalculateCombatRating(stats);

        _logger.LogDebug(
            "Metrics calculated for player {PlayerId}: CombatRating={CombatRating}, CritRate={CritRate:P1}, MissRate={MissRate:P1}",
            player.Id,
            combatRating,
            critRate,
            missRate);

        return new StatisticsMetrics(
            AverageDamagePerHit: avgDamagePerHit,
            AverageDamageReceived: avgDamageReceived,
            CriticalHitRate: critRate,
            MissRate: missRate,
            TrapAvoidanceRate: trapAvoidRate,
            GoldBalance: goldBalance,
            AverageSessionLength: avgSessionLength,
            CombatRating: combatRating);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or creates the statistics entity for a player.
    /// </summary>
    /// <param name="player">The player whose statistics to retrieve or create.</param>
    /// <returns>The player's statistics entity.</returns>
    /// <remarks>
    /// <para>
    /// If the player does not have a statistics entity, one is created and
    /// initialized automatically. This ensures all players have valid statistics
    /// regardless of when they were created.
    /// </para>
    /// </remarks>
    private PlayerStatistics GetOrCreateStatistics(Player player)
    {
        if (player.Statistics is null)
        {
            _logger.LogDebug(
                "Creating new statistics entity for player {PlayerId}",
                player.Id);

            player.InitializeStatistics(PlayerStatistics.Create(player.Id));
        }

        return player.Statistics!;
    }

    /// <summary>
    /// Gets the statistic names for a specific category.
    /// </summary>
    /// <param name="category">The category to get statistic names for.</param>
    /// <returns>A read-only list of statistic names belonging to the category.</returns>
    /// <remarks>
    /// <para>
    /// Returns the statistic names that belong to each category. These names
    /// are used for grouping statistics in the category statistics query.
    /// </para>
    /// <para>
    /// The Dice category is a placeholder for v0.12.0b and returns an empty array.
    /// </para>
    /// </remarks>
    private static IReadOnlyList<string> GetStatNamesForCategory(StatisticCategory category)
    {
        return category switch
        {
            StatisticCategory.Combat => new[]
            {
                "monstersKilled",
                "bossesKilled",
                "damageDealt",
                "damageReceived",
                "criticalHits",
                "attacksMissed",
                "abilitiesUsed",
                "deathCount",
                "totalAttacks"
            },
            StatisticCategory.Exploration => new[]
            {
                "roomsDiscovered",
                "secretsFound",
                "trapsTriggered",
                "trapsAvoided",
                "doorsOpened",
                "chestsOpened"
            },
            StatisticCategory.Progression => new[]
            {
                "xpEarned",
                "levelsGained",
                "itemsFound",
                "itemsCrafted",
                "goldEarned",
                "goldSpent",
                "questsCompleted",
                "puzzlesSolved",
                "resourcesGathered"
            },
            StatisticCategory.Time => new[]
            {
                "playtimeSeconds",
                "sessionCount"
            },
            // Dice category is a placeholder for v0.12.0b
            StatisticCategory.Dice => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Calculates the combat rating based on player statistics.
    /// </summary>
    /// <param name="stats">The player's statistics entity.</param>
    /// <returns>The calculated combat rating tier.</returns>
    /// <remarks>
    /// <para>Combat rating score calculation (0-100):</para>
    /// <list type="bullet">
    ///   <item><description>K/D ratio contribution: ratio × 4, capped at 40 points</description></item>
    ///   <item><description>Critical hit rate contribution: rate × 200, capped at 20 points</description></item>
    ///   <item><description>Miss rate penalty: rate × 100, up to -20 points</description></item>
    ///   <item><description>Boss kills contribution: kills × 5, capped at 20 points</description></item>
    /// </list>
    /// <para>Rating thresholds:</para>
    /// <list type="bullet">
    ///   <item><description>Legend: 90+</description></item>
    ///   <item><description>Master: 75-89</description></item>
    ///   <item><description>Veteran: 60-74</description></item>
    ///   <item><description>Skilled: 45-59</description></item>
    ///   <item><description>Journeyman: 30-44</description></item>
    ///   <item><description>Apprentice: 15-29</description></item>
    ///   <item><description>Novice: 0-14</description></item>
    /// </list>
    /// </remarks>
    private static CombatRating CalculateCombatRating(PlayerStatistics stats)
    {
        // Calculate kill/death ratio (avoid division by zero)
        // If no deaths, use monster kills as the effective ratio
        var kdRatio = stats.DeathCount > 0
            ? (double)stats.MonstersKilled / stats.DeathCount
            : stats.MonstersKilled;

        // Calculate critical hit rate
        var critRate = stats.TotalAttacks > 0
            ? (double)stats.CriticalHits / stats.TotalAttacks
            : 0.0;

        // Calculate miss rate
        // Default to 1.0 (100% miss rate) if no attacks made, which will penalize new players
        // but quickly improves once they start hitting
        var missRate = stats.TotalAttacks > 0
            ? (double)stats.AttacksMissed / stats.TotalAttacks
            : 1.0;

        // Calculate score (0-100)
        var score = 0.0;

        // K/D ratio contribution (up to 40 points)
        // A K/D of 10:1 gives full points
        score += Math.Min(40, kdRatio * 4);

        // Critical hit rate contribution (up to 20 points)
        // A 10% crit rate gives full points
        score += Math.Min(20, critRate * 200);

        // Miss rate penalty (subtract up to 20 points)
        // A 20% miss rate gives full penalty
        score -= Math.Min(20, missRate * 100);

        // Boss kills contribution (up to 20 points)
        // 4 boss kills give full points
        score += Math.Min(20, stats.BossesKilled * 5);

        // Clamp score to valid range
        score = Math.Clamp(score, 0, 100);

        // Determine rating tier based on score
        return score switch
        {
            >= 90 => CombatRating.Legend,
            >= 75 => CombatRating.Master,
            >= 60 => CombatRating.Veteran,
            >= 45 => CombatRating.Skilled,
            >= 30 => CombatRating.Journeyman,
            >= 15 => CombatRating.Apprentice,
            _ => CombatRating.Novice
        };
    }
}
