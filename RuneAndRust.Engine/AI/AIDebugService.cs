using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for debugging and visualizing AI decision-making.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class AIDebugService : IAIDebugService
{
    private readonly ILogger<AIDebugService> _logger;
    private bool _debugMode = false;

    // Store decision logs for report generation
    private readonly ConcurrentDictionary<Guid, List<DecisionLog>> _decisionLogs = new();

    public AIDebugService(ILogger<AIDebugService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void EnableDebugMode()
    {
        _debugMode = true;
        _logger.LogInformation("AI Debug Mode ENABLED");
    }

    /// <inheritdoc/>
    public void DisableDebugMode()
    {
        _debugMode = false;
        _logger.LogInformation("AI Debug Mode DISABLED");
    }

    /// <inheritdoc/>
    public bool IsDebugModeEnabled()
    {
        return _debugMode;
    }

    /// <inheritdoc/>
    public void LogDecision(Enemy enemy, EnemyAction action, DecisionContext context)
    {
        if (!_debugMode)
        {
            return;
        }

        var targetName = action.Target switch
        {
            PlayerCharacter pc => pc.Name,
            Enemy e => e.Name,
            _ => "None"
        };

        var debugInfo = new
        {
            Enemy = new
            {
                enemy.Id,
                enemy.Name,
                enemy.AIArchetype,
                HP = $"{enemy.CurrentHP}/{enemy.MaxHP}"
            },
            Decision = new
            {
                Target = targetName,
                AbilityId = action.SelectedAbilityId,
                MoveTo = action.MoveTo,
                AggressionModifier = action.AggressionModifier,
                Priority = action.Priority
            },
            Context = new
            {
                context.IntelligenceLevel,
                ThreatCount = context.ThreatAssessments.Count,
                AvailableAbilities = context.AvailableAbilityIds.Count,
                context.IsIntentionalError,
                context.ErrorType
            },
            Reasoning = context.Reasoning
        };

        _logger.LogInformation(
            "[AI DEBUG] {Enemy} decision:\n{Decision}",
            enemy.Name,
            JsonSerializer.Serialize(debugInfo, new JsonSerializerOptions { WriteIndented = true }));

        // Store for report generation (use a placeholder encounter ID for now)
        var encounterId = Guid.Empty; // TODO: Get actual encounter ID from context
        StoreDecisionLog(encounterId, enemy, action, context);
    }

    /// <inheritdoc/>
    public AIDecisionReport GenerateDecisionReport(Guid encounterId)
    {
        if (!_decisionLogs.TryGetValue(encounterId, out var logs) || !logs.Any())
        {
            _logger.LogWarning("No decision logs found for encounter {EncounterId}", encounterId);
            return new AIDecisionReport
            {
                EncounterId = encounterId,
                TotalDecisions = 0
            };
        }

        var report = new AIDecisionReport
        {
            EncounterId = encounterId,
            TotalDecisions = logs.Count,
            AverageDecisionTimeMs = logs.Average(l => l.DecisionTimeMs),
            DecisionsByArchetype = logs
                .GroupBy(l => l.Archetype)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostCommonTargets = logs
                .Where(l => !string.IsNullOrEmpty(l.TargetName))
                .GroupBy(l => l.TargetName)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count()),
            AbilityUsageFrequency = logs
                .GroupBy(l => l.AbilityId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => $"Ability{g.Key}", g => g.Count()),
            IntentionalErrors = logs.Count(l => l.IsIntentionalError),
            AverageIntelligenceLevel = logs.Average(l => l.IntelligenceLevel)
        };

        _logger.LogInformation(
            "Decision report for encounter {EncounterId}: {Total} decisions, {Errors} errors, avg intelligence {AvgIntel:F1}",
            encounterId, report.TotalDecisions, report.IntentionalErrors, report.AverageIntelligenceLevel);

        return report;
    }

    /// <inheritdoc/>
    public void LogPerformanceWarning(Enemy enemy, long durationMs)
    {
        if (durationMs > 50)
        {
            _logger.LogWarning(
                "AI decision for {Enemy} (ID: {EnemyId}) took {Ms}ms (threshold: 50ms)",
                enemy.Name, enemy.Id, durationMs);
        }
    }

    /// <summary>
    /// Stores a decision log for later report generation.
    /// </summary>
    private void StoreDecisionLog(Guid encounterId, Enemy enemy, EnemyAction action, DecisionContext context)
    {
        var targetName = action.Target switch
        {
            PlayerCharacter pc => pc.Name,
            Enemy e => e.Name,
            _ => string.Empty
        };

        var log = new DecisionLog
        {
            Timestamp = DateTime.UtcNow,
            EnemyId = enemy.Id,
            EnemyName = enemy.Name,
            Archetype = enemy.AIArchetype,
            TargetName = targetName,
            AbilityId = action.SelectedAbilityId,
            IntelligenceLevel = context.IntelligenceLevel,
            IsIntentionalError = context.IsIntentionalError,
            DecisionTimeMs = 0 // TODO: Track actual decision time
        };

        _decisionLogs.AddOrUpdate(
            encounterId,
            _ => new List<DecisionLog> { log },
            (_, existingLogs) =>
            {
                existingLogs.Add(log);
                return existingLogs;
            });
    }

    /// <summary>
    /// Clears decision logs for an encounter.
    /// </summary>
    public void ClearEncounterLogs(Guid encounterId)
    {
        _decisionLogs.TryRemove(encounterId, out _);
        _logger.LogDebug("Cleared decision logs for encounter {EncounterId}", encounterId);
    }

    /// <summary>
    /// Clears all decision logs.
    /// </summary>
    public void ClearAllLogs()
    {
        _decisionLogs.Clear();
        _logger.LogInformation("Cleared all AI decision logs");
    }
}

/// <summary>
/// Internal class for storing individual decision logs.
/// </summary>
internal class DecisionLog
{
    public DateTime Timestamp { get; set; }
    public int EnemyId { get; set; }
    public string EnemyName { get; set; } = string.Empty;
    public AIArchetype Archetype { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public int AbilityId { get; set; }
    public int IntelligenceLevel { get; set; }
    public bool IsIntentionalError { get; set; }
    public long DecisionTimeMs { get; set; }
}
