using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Master orchestrator for all enemy AI systems (v0.42).
/// Coordinates threat assessment, target selection, ability prioritization,
/// boss AI, difficulty scaling, and performance monitoring.
/// </summary>
public class EnemyAIOrchestrator
{
    private readonly IThreatAssessmentService _threatService;
    private readonly ITargetSelectionService _targetService;
    private readonly ISituationalAnalysisService _situationalService;
    private readonly IAbilityPrioritizationService _abilityService;
    private readonly IBossAIService _bossService;
    private readonly IAbilityRotationService _rotationService;
    private readonly IAddManagementService _addService;
    private readonly IAdaptiveDifficultyService _adaptiveService;
    private readonly IDifficultyScalingService _difficultyService;
    private readonly IChallengeSectorAIService _sectorService;
    private readonly IAIPerformanceMonitor _performanceMonitor;
    private readonly IAIDebugService _debugService;
    private readonly ILogger<EnemyAIOrchestrator> _logger;

    public EnemyAIOrchestrator(
        IThreatAssessmentService threatService,
        ITargetSelectionService targetService,
        ISituationalAnalysisService situationalService,
        IAbilityPrioritizationService abilityService,
        IBossAIService bossService,
        IAbilityRotationService rotationService,
        IAddManagementService addService,
        IAdaptiveDifficultyService adaptiveService,
        IDifficultyScalingService difficultyService,
        IChallengeSectorAIService sectorService,
        IAIPerformanceMonitor performanceMonitor,
        IAIDebugService debugService,
        ILogger<EnemyAIOrchestrator> logger)
    {
        _threatService = threatService;
        _targetService = targetService;
        _situationalService = situationalService;
        _abilityService = abilityService;
        _bossService = bossService;
        _rotationService = rotationService;
        _addService = addService;
        _adaptiveService = adaptiveService;
        _difficultyService = difficultyService;
        _sectorService = sectorService;
        _performanceMonitor = performanceMonitor;
        _debugService = debugService;
        _logger = logger;
    }

    /// <summary>
    /// Main entry point: Decides the best action for an enemy.
    /// Integrates all v0.42 AI systems with performance monitoring.
    /// </summary>
    /// <param name="enemy">The enemy making the decision.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <param name="isBoss">Whether this is a boss enemy.</param>
    /// <param name="challengeModifiers">Active Challenge Sector modifiers (if any).</param>
    /// <returns>The decided enemy action.</returns>
    public async Task<EnemyAction> DecideActionAsync(
        Enemy enemy,
        BattlefieldState state,
        bool isBoss = false,
        List<ChallengeSectorModifier>? challengeModifiers = null)
    {
        return await _performanceMonitor.MonitorPerformanceAsync(
            $"DecideAction_{enemy.AIArchetype}",
            async () => await DecideActionInternalAsync(enemy, state, isBoss, challengeModifiers));
    }

    private async Task<EnemyAction> DecideActionInternalAsync(
        Enemy enemy,
        BattlefieldState state,
        bool isBoss,
        List<ChallengeSectorModifier>? challengeModifiers)
    {
        _logger.LogDebug(
            "Enemy {EnemyId} ({Archetype}) deciding action (Boss: {IsBoss})",
            enemy.Id, enemy.AIArchetype, isBoss);

        // Get current intelligence level based on difficulty
        var intelligenceLevel = await _difficultyService.GetAIIntelligenceLevelAsync();

        // Create decision context
        var context = new DecisionContext
        {
            IntelligenceLevel = intelligenceLevel,
            Reasoning = $"{enemy.AIArchetype} AI decision"
        };

        EnemyAction action;

        // Boss AI path
        if (isBoss)
        {
            action = await DecideBossActionAsync(enemy, state, context);
        }
        // Normal enemy path
        else
        {
            action = await DecideNormalEnemyActionAsync(enemy, state, context);
        }

        // Apply difficulty scaling (intelligence-based behavior)
        action = await _difficultyService.ApplyIntelligenceScalingAsync(action, intelligenceLevel, state);

        // Apply Challenge Sector modifiers if present
        if (challengeModifiers != null && challengeModifiers.Count > 0)
        {
            await _sectorService.AdaptToSectorModifiersAsync(enemy, action, challengeModifiers, state);
        }

        // Debug logging if enabled
        if (_debugService.IsDebugModeEnabled())
        {
            _debugService.LogDecision(enemy, action, context);
        }

        _logger.LogInformation(
            "Enemy {EnemyId} decided: Target={Target}, Ability={AbilityId}, Intelligence={Intel}",
            enemy.Id,
            action.Target?.GetType().Name ?? "None",
            action.SelectedAbilityId,
            intelligenceLevel);

        return action;
    }

    /// <summary>
    /// Decides action for boss enemies (uses boss AI systems).
    /// </summary>
    private async Task<EnemyAction> DecideBossActionAsync(
        Enemy boss,
        BattlefieldState state,
        DecisionContext context)
    {
        _logger.LogDebug("Processing boss AI for {BossId}", boss.Id);

        // Get boss configuration
        var bossConfig = await _bossService.GetBossConfigurationAsync(boss.EnemyTypeId);

        if (bossConfig == null)
        {
            _logger.LogWarning(
                "No boss configuration for type {BossTypeId}, falling back to normal AI",
                boss.EnemyTypeId);
            return await DecideNormalEnemyActionAsync(boss, state, context);
        }

        // Determine current phase
        var currentPhase = _bossService.DeterminePhase(boss);

        // Check for phase transition
        if (_bossService.ShouldTransitionPhase(boss, currentPhase))
        {
            var newPhase = _bossService.DeterminePhase(boss);
            await _bossService.ExecutePhaseTransitionAsync(boss, newPhase, state);
            _rotationService.ResetRotation(boss);
            currentPhase = newPhase;
        }

        // Manage adds
        if (bossConfig.UsesAdds)
        {
            await _addService.ManageAddsAsync(boss, state);
        }

        // Get ability rotation for current phase
        var rotation = await _rotationService.GetPhaseRotationAsync(boss.EnemyTypeId, currentPhase);

        // Select target using threat assessment
        var target = await SelectTargetAsync(boss, state, context);

        // Select ability from rotation or fallback to ability prioritization
        var abilityId = 0;
        if (rotation != null)
        {
            var rotationAction = await _rotationService.SelectNextAbilityInRotationAsync(boss, rotation, state);
            // Extract ability ID from rotation action (placeholder until ability system integrated)
            abilityId = 1; // TODO: Extract from rotationAction
        }
        else
        {
            // Fallback to ability prioritization
            var abilityScore = await _abilityService.PrioritizeAbilityAsync(boss, state);
            abilityId = abilityScore?.AbilityId ?? 0;
        }

        // Apply adaptive difficulty if enabled
        object? adaptiveModification = null;
        if (bossConfig.UsesAdaptiveDifficulty)
        {
            var isEnabled = await _adaptiveService.IsAdaptiveDifficultyEnabledAsync(boss);
            if (isEnabled)
            {
                var playerStrategy = _adaptiveService.AnalyzePlayerStrategy(state);
                adaptiveModification = await _adaptiveService.ApplyCounterStrategiesAsync(boss, playerStrategy, state);

                if (adaptiveModification != null)
                {
                    context.Reasoning += " | Applied adaptive counter-strategy";
                }
            }
        }

        return new EnemyAction
        {
            Actor = boss,
            Target = target,
            SelectedAbilityId = abilityId,
            Context = context,
            Priority = 1,
            AggressionModifier = CalculateAggressionModifier(boss, bossConfig)
        };
    }

    /// <summary>
    /// Decides action for normal (non-boss) enemies.
    /// </summary>
    private async Task<EnemyAction> DecideNormalEnemyActionAsync(
        Enemy enemy,
        BattlefieldState state,
        DecisionContext context)
    {
        _logger.LogDebug("Processing normal AI for {EnemyId}", enemy.Id);

        // Analyze situation
        var situation = await _situationalService.AnalyzeSituationAsync(state);
        context.Reasoning += $" | Tactical advantage: {situation.TacticalAdvantage}";

        // Assess threats and select target
        var target = await SelectTargetAsync(enemy, state, context);

        // Prioritize abilities
        var abilityScore = await _abilityService.PrioritizeAbilityAsync(enemy, state);
        var abilityId = abilityScore?.AbilityId ?? 0;

        if (abilityScore != null)
        {
            context.Reasoning += $" | Ability score: {abilityScore.TotalScore:F1}";
        }

        return new EnemyAction
        {
            Actor = enemy,
            Target = target,
            SelectedAbilityId = abilityId,
            Context = context,
            Priority = 1,
            AggressionModifier = CalculateAggressionModifier(enemy, null)
        };
    }

    /// <summary>
    /// Selects the best target using threat assessment and archetype behavior.
    /// </summary>
    private async Task<object?> SelectTargetAsync(
        Enemy enemy,
        BattlefieldState state,
        DecisionContext context)
    {
        // Get all potential targets (living players)
        var potentialTargets = state.PlayerParty
            .Where(p => p.CurrentHP > 0)
            .ToList();

        if (!potentialTargets.Any())
        {
            _logger.LogWarning("No valid targets for enemy {EnemyId}", enemy.Id);
            return null;
        }

        // Assess threat for each target
        var assessments = new List<ThreatAssessment>();
        foreach (var target in potentialTargets)
        {
            var assessment = await _threatService.AssessThreatAsync(enemy, target, state);
            assessments.Add(assessment);
        }

        context.ThreatAssessments = assessments;

        // Select target using archetype-specific logic
        var selectedTarget = await _targetService.SelectTargetAsync(enemy, state);

        if (selectedTarget != null)
        {
            var selectedAssessment = assessments.FirstOrDefault(a => a.Target == selectedTarget);
            if (selectedAssessment != null)
            {
                context.Reasoning += $" | Target threat: {selectedAssessment.TotalThreatScore:F1}";
            }
        }

        return selectedTarget;
    }

    /// <summary>
    /// Calculates aggression modifier based on archetype and boss configuration.
    /// </summary>
    private decimal CalculateAggressionModifier(Enemy enemy, BossConfiguration? bossConfig)
    {
        decimal baseAggression = 0m;

        // Boss aggression
        if (bossConfig != null)
        {
            baseAggression = (bossConfig.BaseAggressionLevel - 3) * 0.2m; // Map 1-5 to -0.4 to +0.4
        }

        // Archetype aggression
        var archetypeModifier = enemy.AIArchetype switch
        {
            AIArchetype.Aggressive => 0.5m,
            AIArchetype.Reckless => 0.8m,
            AIArchetype.Defensive => -0.3m,
            AIArchetype.Cautious => -0.2m,
            AIArchetype.Support => -0.4m,
            AIArchetype.Tactical => 0.0m,
            AIArchetype.Control => 0.1m,
            AIArchetype.Ambusher => 0.3m,
            _ => 0.0m
        };

        return baseAggression + archetypeModifier;
    }

    /// <summary>
    /// Gets performance metrics from the monitor.
    /// </summary>
    public Dictionary<string, PerformanceMetrics> GetPerformanceMetrics()
    {
        return _performanceMonitor.GetMetrics();
    }

    /// <summary>
    /// Generates a performance summary report.
    /// </summary>
    public string GeneratePerformanceSummary()
    {
        return ((AIPerformanceMonitor)_performanceMonitor).GeneratePerformanceSummary();
    }

    /// <summary>
    /// Generates a decision report for an encounter.
    /// </summary>
    public AIDecisionReport GenerateDecisionReport(Guid encounterId)
    {
        return _debugService.GenerateDecisionReport(encounterId);
    }

    /// <summary>
    /// Enables AI debug mode.
    /// </summary>
    public void EnableDebugMode()
    {
        _debugService.EnableDebugMode();
        _logger.LogInformation("AI Debug Mode enabled via orchestrator");
    }

    /// <summary>
    /// Disables AI debug mode.
    /// </summary>
    public void DisableDebugMode()
    {
        _debugService.DisableDebugMode();
        _logger.LogInformation("AI Debug Mode disabled via orchestrator");
    }
}
