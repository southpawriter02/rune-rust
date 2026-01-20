using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing combat log entries.
/// </summary>
/// <remarks>
/// <para>CombatLogService provides factory methods for creating log entries
/// and helper methods for common combat events.</para>
/// </remarks>
public class CombatLogService
{
    private readonly ILogger<CombatLogService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Creates a new CombatLogService instance.
    /// </summary>
    /// <param name="logger">Logger for service diagnostics.</param>
    /// <param name="eventLogger">Optional event logger for comprehensive tracking.</param>
    public CombatLogService(
        ILogger<CombatLogService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
    }

    /// <summary>
    /// Logs combat start to the encounter.
    /// </summary>
    public void LogCombatStart(CombatEncounter encounter, int enemyCount)
    {
        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.CombatStart,
            $"Combat begins against {enemyCount} enemies!");

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);

        _eventLogger?.LogCombat("CombatStart", entry.Message,
            data: new Dictionary<string, object>
            {
                ["round"] = encounter.RoundNumber,
                ["enemyCount"] = enemyCount
            });
    }

    /// <summary>
    /// Logs round start to the encounter.
    /// </summary>
    public void LogRoundStart(CombatEncounter encounter)
    {
        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.RoundStart,
            $"Round {encounter.RoundNumber} begins");

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }

    /// <summary>
    /// Logs an attack action to the encounter.
    /// </summary>
    public void LogAttack(CombatEncounter encounter, string actorName, string targetName,
        int damage, bool isCritical = false, bool isMiss = false)
    {
        var message = isMiss
            ? $"{actorName} attacks {targetName} but misses!"
            : isCritical
                ? $"{actorName} lands a CRITICAL hit on {targetName} for {damage} damage!"
                : $"{actorName} attacks {targetName} for {damage} damage";

        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.Attack,
            message,
            actorName,
            targetName,
            damage,
            isCritical: isCritical,
            isMiss: isMiss);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);

        _eventLogger?.LogCombat("Attack", entry.Message,
            data: new Dictionary<string, object>
            {
                ["round"] = encounter.RoundNumber,
                ["actor"] = actorName,
                ["target"] = targetName,
                ["damage"] = damage,
                ["isCritical"] = isCritical,
                ["isMiss"] = isMiss
            });
    }

    /// <summary>
    /// Logs a heal action to the encounter.
    /// </summary>
    public void LogHeal(CombatEncounter encounter, string actorName, string targetName, int healing)
    {
        var isSelfHeal = actorName == targetName;
        var message = isSelfHeal
            ? $"{actorName} heals for {healing} HP"
            : $"{actorName} heals {targetName} for {healing} HP";

        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.Heal,
            message,
            actorName,
            targetName,
            healing: healing);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }

    /// <summary>
    /// Logs a defend action to the encounter.
    /// </summary>
    public void LogDefend(CombatEncounter encounter, string actorName)
    {
        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.Defend,
            $"{actorName} takes a defensive stance",
            actorName);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }

    /// <summary>
    /// Logs a flee attempt to the encounter.
    /// </summary>
    public void LogFleeAttempt(CombatEncounter encounter, string actorName, bool success,
        int roll, int dc, int? damageTaken = null)
    {
        var message = success
            ? $"{actorName} successfully flees! (Rolled {roll} vs DC {dc})"
            : $"{actorName} fails to flee! (Rolled {roll} vs DC {dc})" +
              (damageTaken > 0 ? $" Takes {damageTaken} opportunity attack damage!" : "");

        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.Flee,
            message,
            actorName,
            damage: damageTaken);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }

    /// <summary>
    /// Logs a combatant defeat to the encounter.
    /// </summary>
    public void LogDefeat(CombatEncounter encounter, string actorName, string defeatedName)
    {
        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.Defeat,
            $"{defeatedName} has been defeated!",
            actorName,
            defeatedName);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);

        _eventLogger?.LogCombat("Defeat", entry.Message,
            data: new Dictionary<string, object>
            {
                ["round"] = encounter.RoundNumber,
                ["actor"] = actorName,
                ["defeated"] = defeatedName
            });
    }

    /// <summary>
    /// Logs an AI decision to the encounter (for debugging/display).
    /// </summary>
    public void LogAIDecision(CombatEncounter encounter, string actorName,
        AIAction action, string reasoning, string? targetName = null)
    {
        var message = targetName != null
            ? $"{actorName} decides to {action.ToString().ToLower()} {targetName}: {reasoning}"
            : $"{actorName} decides to {action.ToString().ToLower()}: {reasoning}";

        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.AIDecision,
            message,
            actorName,
            targetName);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }

    /// <summary>
    /// Logs combat end to the encounter.
    /// </summary>
    public void LogCombatEnd(CombatEncounter encounter, CombatState resultState)
    {
        var message = resultState switch
        {
            CombatState.Victory => "Victory! All enemies defeated!",
            CombatState.PlayerDefeated => "Defeat... The player has fallen.",
            CombatState.Fled => "Escaped from combat.",
            _ => "Combat has ended."
        };

        var entry = CombatLogEntry.Create(
            encounter.RoundNumber,
            CombatLogType.CombatEnd,
            message);

        encounter.AddLogEntry(entry);
        _logger.LogDebug("Combat log: {Message}", entry.Message);
    }
}
