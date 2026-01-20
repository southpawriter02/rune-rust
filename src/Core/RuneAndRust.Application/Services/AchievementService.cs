// ═══════════════════════════════════════════════════════════════════════════════
// AchievementService.cs
// Service for checking and managing player achievements.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for checking and managing player achievements.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the core functionality for the achievement system:
/// </para>
/// <list type="bullet">
///   <item><description>Evaluates achievement conditions against player statistics</description></item>
///   <item><description>Unlocks achievements when all conditions are met</description></item>
///   <item><description>Publishes events when achievements unlock</description></item>
///   <item><description>Calculates progress for locked achievements</description></item>
/// </list>
/// <para>
/// The service integrates with:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="IAchievementProvider"/> for reading achievement definitions</description></item>
///   <item><description><see cref="IStatisticsService"/> for reading player statistics</description></item>
///   <item><description><see cref="IDiceHistoryService"/> for reading dice statistics</description></item>
///   <item><description><see cref="IGameRenderer"/> for displaying unlock notifications</description></item>
/// </list>
/// </remarks>
public class AchievementService : IAchievementService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IAchievementProvider _achievementProvider;
    private readonly IStatisticsService _statisticsService;
    private readonly IDiceHistoryService _diceHistoryService;
    private readonly IGameRenderer _gameRenderer;
    private readonly ILogger<AchievementService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementService"/> class.
    /// </summary>
    /// <param name="achievementProvider">Provider for achievement definitions. Required.</param>
    /// <param name="statisticsService">Service for player statistics. Required.</param>
    /// <param name="diceHistoryService">Service for dice roll history. Required.</param>
    /// <param name="gameRenderer">Renderer for displaying notifications. Optional.</param>
    /// <param name="logger">Logger for diagnostics. Required.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public AchievementService(
        IAchievementProvider achievementProvider,
        IStatisticsService statisticsService,
        IDiceHistoryService diceHistoryService,
        IGameRenderer? gameRenderer,
        ILogger<AchievementService> logger)
    {
        _achievementProvider = achievementProvider
            ?? throw new ArgumentNullException(nameof(achievementProvider));
        _statisticsService = statisticsService
            ?? throw new ArgumentNullException(nameof(statisticsService));
        _diceHistoryService = diceHistoryService
            ?? throw new ArgumentNullException(nameof(diceHistoryService));
        _gameRenderer = gameRenderer!;
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "AchievementService initialized with {Count} achievements",
            _achievementProvider.GetAchievementCount());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ACHIEVEMENT CHECKING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<string> CheckAchievements(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug(
            "Checking achievements for player {PlayerId} ({PlayerName})",
            player.Id,
            player.Name);

        // Get player statistics for condition evaluation
        var statistics = _statisticsService.GetPlayerStatistics(player);
        var diceHistory = _diceHistoryService.GetHistory(player);
        var allAchievements = _achievementProvider.GetAllAchievements();

        var unlockedThisCheck = new List<string>();

        foreach (var achievement in allAchievements)
        {
            // Skip already unlocked achievements
            if (player.HasAchievement(achievement.AchievementId))
            {
                _logger.LogDebug(
                    "Achievement {AchievementId} already unlocked, skipping",
                    achievement.AchievementId);
                continue;
            }

            // Evaluate all conditions for this achievement
            _logger.LogDebug(
                "Evaluating achievement {AchievementId}: {ConditionCount} conditions",
                achievement.AchievementId,
                achievement.Conditions.Count);

            if (EvaluateConditions(achievement.Conditions, statistics, diceHistory, player))
            {
                // All conditions met - unlock the achievement
                UnlockAchievement(player, achievement);
                unlockedThisCheck.Add(achievement.AchievementId);
            }
        }

        if (unlockedThisCheck.Count > 0)
        {
            _logger.LogInformation(
                "Player {PlayerName} unlocked {Count} achievements: {Achievements}",
                player.Name,
                unlockedThisCheck.Count,
                string.Join(", ", unlockedThisCheck));
        }

        return unlockedThisCheck.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<PlayerAchievement> GetUnlockedAchievements(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug(
            "Getting unlocked achievements for player {PlayerId}",
            player.Id);

        return player.Achievements;
    }

    /// <inheritdoc />
    public IReadOnlyList<AchievementProgress> GetProgress(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug(
            "Getting achievement progress for player {PlayerId}",
            player.Id);

        var statistics = _statisticsService.GetPlayerStatistics(player);
        var diceHistory = _diceHistoryService.GetHistory(player);
        var allAchievements = _achievementProvider.GetAllAchievements();
        var progressList = new List<AchievementProgress>();

        foreach (var achievement in allAchievements)
        {
            var progress = CalculateProgress(achievement, statistics, diceHistory, player);
            progressList.Add(progress);
        }

        _logger.LogDebug(
            "Calculated progress for {Count} achievements",
            progressList.Count);

        return progressList.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<AchievementProgress> GetProgressByCategory(
        Player player,
        AchievementCategory category)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug(
            "Getting achievement progress for player {PlayerId} in category {Category}",
            player.Id,
            category);

        var statistics = _statisticsService.GetPlayerStatistics(player);
        var diceHistory = _diceHistoryService.GetHistory(player);
        var achievements = _achievementProvider.GetAchievementsByCategory(category);
        var progressList = new List<AchievementProgress>();

        foreach (var achievement in achievements)
        {
            var progress = CalculateProgress(achievement, statistics, diceHistory, player);
            progressList.Add(progress);
        }

        _logger.LogDebug(
            "Calculated progress for {Count} achievements in category {Category}",
            progressList.Count,
            category);

        return progressList.AsReadOnly();
    }

    /// <inheritdoc />
    public int GetTotalPoints(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var points = player.GetTotalAchievementPoints();

        _logger.LogDebug(
            "Player {PlayerId} has {Points} total achievement points",
            player.Id,
            points);

        return points;
    }

    /// <inheritdoc />
    public bool IsUnlocked(Player player, string achievementId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));

        var isUnlocked = player.HasAchievement(achievementId);

        _logger.LogDebug(
            "Achievement {AchievementId} unlocked for player {PlayerId}: {IsUnlocked}",
            achievementId,
            player.Id,
            isUnlocked);

        return isUnlocked;
    }

    /// <inheritdoc />
    public int GetUnlockedCount(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var count = player.Achievements.Count;

        _logger.LogDebug(
            "Player {PlayerId} has {Count} unlocked achievements",
            player.Id,
            count);

        return count;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - CONDITION EVALUATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Evaluates all conditions for an achievement.
    /// </summary>
    /// <param name="conditions">The conditions to evaluate.</param>
    /// <param name="statistics">The player's statistics.</param>
    /// <param name="diceHistory">The player's dice roll history.</param>
    /// <param name="player">The player being evaluated.</param>
    /// <returns>True if all conditions are met.</returns>
    private bool EvaluateConditions(
        IReadOnlyList<AchievementCondition> conditions,
        PlayerStatistics statistics,
        DiceRollHistory diceHistory,
        Player player)
    {
        // Achievement with no conditions can never unlock
        if (conditions.Count == 0)
        {
            _logger.LogWarning(
                "Achievement has no conditions, cannot unlock");
            return false;
        }

        // All conditions must be met (AND logic)
        foreach (var condition in conditions)
        {
            var value = GetStatisticValue(condition.StatisticName, statistics, diceHistory, player);
            var isMet = condition.Evaluate(value);

            _logger.LogDebug(
                "Condition {StatName} {Operator} {Target}: player has {Actual}, result: {Result}",
                condition.StatisticName,
                condition.OperatorSymbol,
                condition.Value,
                value,
                isMet);

            if (!isMet)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Unlocks an achievement for a player.
    /// </summary>
    /// <param name="player">The player unlocking the achievement.</param>
    /// <param name="achievement">The achievement to unlock.</param>
    private void UnlockAchievement(Player player, AchievementDefinition achievement)
    {
        // Add the achievement to the player's collection
        player.AddAchievement(achievement.AchievementId, achievement.Points);

        _logger.LogInformation(
            "Achievement unlocked: {Name} ({AchievementId}) +{Points} points for player {PlayerName}",
            achievement.Name,
            achievement.AchievementId,
            achievement.Points,
            player.Name);

        // Create and log the event
        var unlockEvent = new AchievementUnlockedEvent(
            player.Id,
            achievement.AchievementId,
            achievement.Name,
            achievement.Description,
            achievement.Tier.ToString(),
            achievement.Points);

        _logger.LogDebug(
            "Created AchievementUnlockedEvent: {Message}",
            unlockEvent.Message);

        // Display notification if renderer is available
        if (_gameRenderer != null)
        {
            // Note: In a real implementation, this would use an async notification queue
            // For now, we log that we would display the notification
            _logger.LogDebug(
                "Would display achievement notification for: {AchievementName}",
                achievement.Name);
        }
    }

    /// <summary>
    /// Calculates progress for an achievement.
    /// </summary>
    /// <param name="achievement">The achievement to calculate progress for.</param>
    /// <param name="statistics">The player's statistics.</param>
    /// <param name="diceHistory">The player's dice roll history.</param>
    /// <param name="player">The player being evaluated.</param>
    /// <returns>The achievement progress record.</returns>
    private AchievementProgress CalculateProgress(
        AchievementDefinition achievement,
        PlayerStatistics statistics,
        DiceRollHistory diceHistory,
        Player player)
    {
        var isUnlocked = player.HasAchievement(achievement.AchievementId);
        var conditionProgressList = new List<ConditionProgress>();
        var totalProgress = 0.0;

        foreach (var condition in achievement.Conditions)
        {
            var currentValue = GetStatisticValue(
                condition.StatisticName,
                statistics,
                diceHistory,
                player);

            var progress = condition.GetProgress(currentValue);
            var isMet = condition.Evaluate(currentValue);

            conditionProgressList.Add(new ConditionProgress(
                condition,
                currentValue,
                condition.Value,
                progress,
                isMet));

            totalProgress += progress;
        }

        // Calculate overall progress as average (or 1.0 if unlocked)
        var overallProgress = achievement.Conditions.Count > 0
            ? totalProgress / achievement.Conditions.Count
            : 0.0;

        if (isUnlocked)
        {
            overallProgress = 1.0;
        }

        return new AchievementProgress(
            achievement,
            isUnlocked,
            conditionProgressList.AsReadOnly(),
            overallProgress);
    }

    /// <summary>
    /// Gets a statistic value by name from player statistics or dice history.
    /// </summary>
    /// <param name="statisticName">The name of the statistic.</param>
    /// <param name="statistics">The player's statistics.</param>
    /// <param name="diceHistory">The player's dice roll history.</param>
    /// <param name="player">The player.</param>
    /// <returns>The statistic value, or 0 if not found.</returns>
    private long GetStatisticValue(
        string statisticName,
        PlayerStatistics statistics,
        DiceRollHistory diceHistory,
        Player player)
    {
        // Map statistic names to actual values (case-insensitive)
        var value = statisticName.ToLowerInvariant() switch
        {
            // Combat statistics
            "monsterskilled" => statistics.MonstersKilled,
            "bosseskilled" => statistics.BossesKilled,
            "damagedone" or "damagedealt" => statistics.TotalDamageDealt,
            "damagetaken" => statistics.TotalDamageTaken,
            "criticalhits" => statistics.CriticalHits,
            "flawlessbosskills" => statistics.FlawlessBossKills,

            // Exploration statistics
            "roomsdiscovered" => statistics.RoomsDiscovered,
            "secretsfound" => statistics.SecretsFound,
            "chestsopened" => statistics.ChestsOpened,

            // Progression statistics
            "levelsgained" or "level" => player.Level,
            "totalxpearned" => statistics.TotalXpEarned,
            "questscompleted" => statistics.QuestsCompleted,

            // Collection statistics
            "goldearned" => statistics.TotalGoldEarned,
            "itemsfound" => statistics.ItemsFound,
            "itemscrafted" => statistics.ItemsCrafted,

            // Dice statistics (from DiceRollHistory)
            "totalnaturaltwenties" or "nat20count" => diceHistory.TotalNaturalTwenties,
            "longestnat20streak" => diceHistory.LongestNat20Streak,
            "totalrollcount" => diceHistory.TotalRollCount,

            // Time statistics
            "totalplaytimeminutes" => statistics.TotalPlaytimeMinutes,
            "fastestcompletionminutes" => statistics.FastestCompletionMinutes ?? long.MaxValue,

            // Unknown statistic
            _ => LogUnknownStatistic(statisticName)
        };

        return value;
    }

    /// <summary>
    /// Logs an unknown statistic name and returns 0.
    /// </summary>
    /// <param name="statisticName">The unknown statistic name.</param>
    /// <returns>Always returns 0.</returns>
    private long LogUnknownStatistic(string statisticName)
    {
        _logger.LogWarning(
            "Unknown statistic name: {StatName}",
            statisticName);
        return 0;
    }
}
