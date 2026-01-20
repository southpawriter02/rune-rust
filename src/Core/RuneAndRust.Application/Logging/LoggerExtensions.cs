using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Logging;

/// <summary>
/// Extension methods for common logging patterns.
/// Provides consistent, structured logging for player actions, combat, and state changes.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs player action with context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="player">The player performing the action.</param>
    /// <param name="action">The action being performed.</param>
    /// <param name="target">Optional target of the action.</param>
    /// <param name="result">Optional result of the action.</param>
    public static void LogPlayerAction(
        this ILogger logger,
        Player player,
        string action,
        string? target = null,
        string? result = null)
    {
        if (target != null && result != null)
        {
            logger.LogInformation(
                LogTemplates.PlayerActionResult,
                player.Name, action, result);
        }
        else if (target != null)
        {
            logger.LogInformation(
                LogTemplates.PlayerActionWithTarget,
                player.Name, action, target);
        }
        else
        {
            logger.LogInformation(
                LogTemplates.PlayerAction,
                player.Name, action);
        }
    }

    /// <summary>
    /// Logs combat event with full context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="player">The player in combat.</param>
    /// <param name="monster">The monster being fought.</param>
    /// <param name="damageDealt">Damage dealt by the player.</param>
    /// <param name="damageReceived">Damage received by the player.</param>
    /// <param name="monsterDefeated">Whether the monster was defeated.</param>
    /// <param name="playerDefeated">Whether the player was defeated.</param>
    public static void LogCombat(
        this ILogger logger,
        Player player,
        Monster monster,
        int damageDealt,
        int damageReceived,
        bool monsterDefeated,
        bool playerDefeated)
    {
        logger.LogInformation(
            "Combat: {PlayerName} vs {MonsterName} - " +
            "Dealt: {DamageDealt}, Received: {DamageReceived}, " +
            "MonsterDefeated: {MonsterDefeated}, PlayerDefeated: {PlayerDefeated}",
            player.Name, monster.Name,
            damageDealt, damageReceived,
            monsterDefeated, playerDefeated);
    }

    /// <summary>
    /// Logs state change with before/after values.
    /// </summary>
    /// <typeparam name="T">The type of the value being changed.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="entityType">The type of entity being changed.</param>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <param name="propertyName">The name of the property being changed.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void LogStateChange<T>(
        this ILogger logger,
        string entityType,
        object entityId,
        string propertyName,
        T oldValue,
        T newValue)
    {
        logger.LogInformation(
            "{EntityType} {EntityId} {PropertyName} changed: {OldValue} -> {NewValue}",
            entityType, entityId, propertyName, oldValue, newValue);
    }

    /// <summary>
    /// Creates a performance scope for the operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">The name of the operation being timed.</param>
    /// <param name="logLevel">The log level for normal completion (default: Debug).</param>
    /// <param name="warningThresholdMs">Threshold in ms above which a warning is logged (default: 1000).</param>
    /// <returns>A PerformanceScope that logs elapsed time on disposal.</returns>
    public static PerformanceScope BeginPerformanceScope(
        this ILogger logger,
        string operationName,
        LogLevel logLevel = LogLevel.Debug,
        long warningThresholdMs = 1000)
    {
        return new PerformanceScope(logger, operationName, logLevel, warningThresholdMs);
    }

    /// <summary>
    /// Logs configuration loading with item count.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of items loaded.</param>
    /// <param name="itemType">The type of items loaded.</param>
    /// <param name="source">The source of the configuration.</param>
    public static void LogConfigurationLoaded(
        this ILogger logger,
        int count,
        string itemType,
        string source)
    {
        logger.LogInformation(
            LogTemplates.ConfigurationLoaded,
            count, itemType, source);
    }

    /// <summary>
    /// Logs entity not found scenario.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="entityType">The type of entity not found.</param>
    /// <param name="entityId">The identifier that was searched for.</param>
    public static void LogEntityNotFound(
        this ILogger logger,
        string entityType,
        object entityId)
    {
        logger.LogDebug(
            LogTemplates.EntityNotFound,
            entityType, entityId);
    }
}
