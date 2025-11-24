using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.Engine.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine;

/// <summary>
/// EnemyAI integration with v0.42 AI orchestrator.
/// This partial class adds optional orchestrator support while maintaining backward compatibility.
/// v0.42: Enemy AI Improvements & Behavior Polish - Final Integration
/// </summary>
public partial class EnemyAI
{
    private EnemyAIOrchestrator? _orchestrator;
    private ILogger<EnemyAI>? _aiLogger;

    /// <summary>
    /// Enables v0.42 AI orchestrator for this EnemyAI instance.
    /// Once enabled, decisions will use the full tactical AI system.
    /// </summary>
    /// <param name="orchestrator">The AI orchestrator to use.</param>
    /// <param name="logger">Optional logger for AI decisions.</param>
    public void EnableV042Orchestrator(EnemyAIOrchestrator orchestrator, ILogger<EnemyAI>? logger = null)
    {
        _orchestrator = orchestrator;
        _aiLogger = logger;
        _aiLogger?.LogInformation("v0.42 AI Orchestrator enabled for EnemyAI");
    }

    /// <summary>
    /// Disables the v0.42 orchestrator, reverting to legacy AI.
    /// </summary>
    public void DisableV042Orchestrator()
    {
        _orchestrator = null;
        _aiLogger?.LogInformation("v0.42 AI Orchestrator disabled, reverting to legacy AI");
    }

    /// <summary>
    /// Checks if v0.42 orchestrator is enabled.
    /// </summary>
    public bool IsV042OrchestratorEnabled() => _orchestrator != null;

    /// <summary>
    /// Determines enemy action using v0.42 orchestrator if enabled, otherwise falls back to legacy AI.
    /// This async version should be used when orchestrator is available.
    /// </summary>
    /// <param name="enemy">The enemy making the decision.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <param name="isBoss">Whether this is a boss enemy.</param>
    /// <param name="challengeModifiers">Active Challenge Sector modifiers.</param>
    /// <returns>Enemy action decision.</returns>
    public async Task<Core.AI.EnemyAction> DetermineActionV042Async(
        Enemy enemy,
        BattlefieldState state,
        bool isBoss = false,
        List<ChallengeSectorModifier>? challengeModifiers = null)
    {
        if (_orchestrator == null)
        {
            _aiLogger?.LogWarning(
                "v0.42 orchestrator not enabled, falling back to legacy AI for enemy {EnemyId}",
                enemy.Id);

            // Convert legacy action to v0.42 action format
            var legacyAction = DetermineAction(enemy);
            return ConvertLegacyActionToV042(enemy, legacyAction);
        }

        return await _orchestrator.DecideActionAsync(enemy, state, isBoss, challengeModifiers);
    }

    /// <summary>
    /// Converts a legacy EnemyAction enum to v0.42 EnemyAction class format.
    /// </summary>
    private Core.AI.EnemyAction ConvertLegacyActionToV042(Enemy enemy, EnemyAction legacyAction)
    {
        return new Core.AI.EnemyAction
        {
            Actor = enemy,
            Target = null, // Legacy system doesn't track target
            SelectedAbilityId = (int)legacyAction,
            AggressionModifier = 0m,
            Context = new DecisionContext
            {
                IntelligenceLevel = 0,
                Reasoning = $"Legacy AI: {legacyAction}"
            },
            Priority = 1
        };
    }

    /// <summary>
    /// Gets performance metrics from the orchestrator.
    /// </summary>
    /// <returns>Performance metrics dictionary, or empty if orchestrator not enabled.</returns>
    public Dictionary<string, PerformanceMetrics> GetPerformanceMetrics()
    {
        if (_orchestrator == null)
        {
            return new Dictionary<string, PerformanceMetrics>();
        }

        return _orchestrator.GetPerformanceMetrics();
    }

    /// <summary>
    /// Generates a performance summary report.
    /// </summary>
    /// <returns>Performance summary string.</returns>
    public string GeneratePerformanceSummary()
    {
        if (_orchestrator == null)
        {
            return "v0.42 Orchestrator not enabled. No performance metrics available.";
        }

        return _orchestrator.GeneratePerformanceSummary();
    }

    /// <summary>
    /// Generates a decision report for an encounter.
    /// </summary>
    /// <param name="encounterId">Combat encounter ID.</param>
    /// <returns>Decision report.</returns>
    public AIDecisionReport GenerateDecisionReport(System.Guid encounterId)
    {
        if (_orchestrator == null)
        {
            return new AIDecisionReport
            {
                EncounterId = encounterId,
                TotalDecisions = 0
            };
        }

        return _orchestrator.GenerateDecisionReport(encounterId);
    }

    /// <summary>
    /// Enables AI debug mode (verbose logging).
    /// </summary>
    public void EnableDebugMode()
    {
        if (_orchestrator != null)
        {
            _orchestrator.EnableDebugMode();
        }
        else
        {
            _aiLogger?.LogWarning("Cannot enable debug mode: v0.42 orchestrator not enabled");
        }
    }

    /// <summary>
    /// Disables AI debug mode.
    /// </summary>
    public void DisableDebugMode()
    {
        if (_orchestrator != null)
        {
            _orchestrator.DisableDebugMode();
        }
    }
}
