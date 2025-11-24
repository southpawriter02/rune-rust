using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for selecting optimal targets based on threat assessment and AI archetype.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class TargetSelectionService : ITargetSelectionService
{
    private readonly ILogger<TargetSelectionService> _logger;
    private readonly IThreatAssessmentService _threatService;
    private readonly IBehaviorPatternService _behaviorService;

    public TargetSelectionService(
        ILogger<TargetSelectionService> logger,
        IThreatAssessmentService threatService,
        IBehaviorPatternService behaviorService)
    {
        _logger = logger;
        _threatService = threatService;
        _behaviorService = behaviorService;
    }

    /// <inheritdoc/>
    public async Task<object?> SelectTargetAsync(
        Enemy enemy,
        List<object> potentialTargets,
        BattlefieldState state)
    {
        var archetype = await _behaviorService.GetArchetypeAsync(enemy);

        if (!potentialTargets.Any())
        {
            _logger.LogWarning("No potential targets provided for enemy {EnemyId}", enemy.Id);
            return null;
        }

        // Step 1: Assess threat for all targets
        var assessments = new List<ThreatAssessment>();
        foreach (var target in potentialTargets)
        {
            var assessment = await _threatService.AssessThreatAsync(enemy, target, state);
            assessments.Add(assessment);
        }

        // Step 2: Apply archetype modifiers
        var modifiedScores = await ApplyArchetypeModifiersAsync(archetype, assessments, state);

        // Step 3: Filter invalid targets
        var validTargets = modifiedScores
            .Where(kvp => IsValidTarget(enemy, kvp.Key, state))
            .ToList();

        if (!validTargets.Any())
        {
            _logger.LogWarning("No valid targets for enemy {EnemyId} after filtering", enemy.Id);
            return null;
        }

        // Step 4: Select highest-scoring target
        var selected = validTargets.OrderByDescending(kvp => kvp.Value).First();

        var selectedName = ExtractTargetName(selected.Key);

        _logger.LogInformation(
            "Target Selected: {EnemyId} ({Archetype}) → {TargetName} (score={Score:F1})",
            enemy.Id, archetype, selectedName, selected.Value);

        return selected.Key;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<object, float>> ApplyArchetypeModifiersAsync(
        AIArchetype archetype,
        List<ThreatAssessment> assessments,
        BattlefieldState state)
    {
        var scores = new Dictionary<object, float>();

        foreach (var assessment in assessments)
        {
            float score = assessment.TotalThreatScore;
            var target = assessment.Target;

            // Apply archetype-specific logic
            switch (archetype)
            {
                case AIArchetype.Aggressive:
                    // Heavily prioritize high-damage dealers
                    if (assessment.FactorScores[ThreatFactor.DamageOutput] > 30)
                        score *= 1.3f;
                    break;

                case AIArchetype.Defensive:
                    // Deprioritize if it means leaving allies undefended
                    // Check if any ally has low HP
                    var criticalAllies = state.Enemies.Any(e => e.IsAlive && e.HP < e.MaxHP * 0.5f);
                    if (criticalAllies)
                        score *= 0.7f; // Lower priority, should protect allies instead
                    break;

                case AIArchetype.Cautious:
                    // Avoid risky targets (those in good positions)
                    if (assessment.FactorScores[ThreatFactor.Positioning] < 0)
                        score *= 0.6f; // Target is in strong position, avoid
                    break;

                case AIArchetype.Reckless:
                    // Just target highest damage dealer, ignore everything else
                    score = assessment.FactorScores[ThreatFactor.DamageOutput];
                    break;

                case AIArchetype.Tactical:
                    // No modifiers - use pure threat assessment
                    break;

                case AIArchetype.Support:
                    // Support targets allies for healing (handled separately in SelectHealTargetAsync)
                    // For offensive actions, target threats to wounded allies
                    break;

                case AIArchetype.Control:
                    // Prioritize targets with dangerous abilities
                    if (assessment.FactorScores[ThreatFactor.Abilities] > 15)
                        score *= 1.4f;
                    break;

                case AIArchetype.Ambusher:
                    // Heavily prioritize isolated, low-HP targets
                    var lowHP = assessment.FactorScores[ThreatFactor.CurrentHP] > 10;
                    var isolated = assessment.FactorScores[ThreatFactor.Positioning] > 5;

                    if (lowHP && isolated)
                        score *= 1.5f;
                    break;
            }

            scores[target] = score;
        }

        return scores;
    }

    /// <inheritdoc/>
    public bool IsValidTarget(Enemy enemy, object target, BattlefieldState state)
    {
        // Extract target info
        if (target is PlayerCharacter player)
        {
            // Check basic validity
            if (!player.IsAlive)
                return false;

            // TODO: Check for untargetable status effects (v0.42.2)

            // TODO: Check range for ranged enemies (v0.42.2)

            return true;
        }

        if (target is Enemy targetEnemy)
        {
            // For healing targets (Support archetype)
            if (!targetEnemy.IsAlive)
                return false;

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Enemy?> SelectHealTargetAsync(
        Enemy healer,
        List<Enemy> allies,
        BattlefieldState state)
    {
        var livingAllies = allies.Where(e => e.IsAlive && e.HP < e.MaxHP).ToList();

        if (!livingAllies.Any())
        {
            _logger.LogDebug("No injured allies to heal for {EnemyId}", healer.Id);
            return null;
        }

        // Prioritize lowest HP% ally
        var mostWounded = livingAllies
            .OrderBy(e => e.MaxHP > 0 ? (float)e.HP / e.MaxHP : 1f)
            .First();

        _logger.LogInformation(
            "Heal Target Selected: {HealerId} → {TargetName} ({HP}/{MaxHP})",
            healer.Id, mostWounded.Name, mostWounded.HP, mostWounded.MaxHP);

        return mostWounded;
    }

    // ===== HELPER METHODS =====

    private string ExtractTargetName(object target)
    {
        return target switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Name,
            _ => "Unknown"
        };
    }
}
