using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for assessing the threat level of targets.
/// Calculates threat scores based on damage output, HP, positioning, abilities, and status effects.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class ThreatAssessmentService : IThreatAssessmentService
{
    private readonly ILogger<ThreatAssessmentService> _logger;
    private readonly IAIConfigurationRepository _configRepo;
    private readonly CoverService? _coverService;

    // Cache for threat weights (avoid repeated DB queries)
    private Dictionary<AIArchetype, AIThreatWeights>? _weightsCache;

    public ThreatAssessmentService(
        ILogger<ThreatAssessmentService> logger,
        IAIConfigurationRepository configRepo,
        CoverService? coverService = null)
    {
        _logger = logger;
        _configRepo = configRepo;
        _coverService = coverService;
    }

    /// <inheritdoc/>
    public async Task<ThreatAssessment> AssessThreatAsync(
        Enemy assessor,
        object target,
        BattlefieldState state)
    {
        var weights = await GetThreatWeightsAsync(assessor.AIArchetype);
        var factors = new Dictionary<ThreatFactor, float>();

        // Extract target info (polymorphic handling)
        var (targetId, targetName, targetHP, targetMaxHP, targetPos) = ExtractTargetInfo(target);

        // Factor 1: Damage Output (highest weight for most archetypes)
        var damageScore = await EvaluateDamageThreatAsync(target);
        factors[ThreatFactor.DamageOutput] = damageScore * (float)weights.DamageWeight;

        // Factor 2: Current HP (inverse - low HP = higher priority)
        var hpScore = EvaluateHPThreat(target);
        factors[ThreatFactor.CurrentHP] = hpScore * (float)weights.HPWeight;

        // Factor 3: Positioning
        var positionScore = EvaluatePositionThreat(target, state.Grid);
        factors[ThreatFactor.Positioning] = positionScore * (float)weights.PositionWeight;

        // Factor 4: Abilities
        var abilityScore = await EvaluateAbilityThreatAsync(target);
        factors[ThreatFactor.Abilities] = abilityScore * (float)weights.AbilityWeight;

        // Factor 5: Status Effects
        var statusScore = EvaluateStatusEffectThreat(target);
        factors[ThreatFactor.StatusEffects] = statusScore * (float)weights.StatusWeight;

        var totalScore = factors.Values.Sum();

        var reasoning = GenerateReasoningText(factors);

        _logger.LogInformation(
            "Threat Assessment: {EnemyId} ({Archetype}) → {TargetName} = {TotalScore:F1} " +
            "(Dmg={Dmg:F1}, HP={HP:F1}, Pos={Pos:F1}, Abil={Abil:F1}, Status={Status:F1})",
            assessor.Id, assessor.AIArchetype, targetName, totalScore,
            factors[ThreatFactor.DamageOutput],
            factors[ThreatFactor.CurrentHP],
            factors[ThreatFactor.Positioning],
            factors[ThreatFactor.Abilities],
            factors[ThreatFactor.StatusEffects]);

        return new ThreatAssessment
        {
            Target = target,
            TargetId = targetId,
            TargetName = targetName,
            TotalThreatScore = totalScore,
            FactorScores = factors,
            Reasoning = reasoning,
            AssessorArchetype = assessor.AIArchetype
        };
    }

    /// <inheritdoc/>
    public async Task<AIThreatWeights> GetThreatWeightsAsync(AIArchetype archetype)
    {
        // Initialize cache if needed
        if (_weightsCache == null)
        {
            _weightsCache = await _configRepo.GetAllThreatWeightsAsync();
        }

        if (_weightsCache.TryGetValue(archetype, out var weights))
        {
            return weights;
        }

        _logger.LogWarning("No threat weights found for archetype {Archetype}, using Tactical defaults", archetype);

        // Fallback to Tactical if not found
        return _weightsCache.GetValueOrDefault(AIArchetype.Tactical) ?? CreateDefaultWeights(archetype);
    }

    /// <inheritdoc/>
    public async Task<float> EvaluateDamageThreatAsync(object target, int turns = 3)
    {
        // TODO: Implement damage history tracking in v0.42.2
        // For now, use a simplified heuristic based on target type

        if (target is PlayerCharacter player)
        {
            // Estimate damage based on attributes
            var estimatedDamage = player.Attributes.Might * 3f; // Rough estimate
            return Math.Min(estimatedDamage, 100f);
        }

        if (target is Enemy enemy)
        {
            var estimatedDamage = enemy.BaseDamageDice * 3.5f + enemy.DamageBonus;
            return Math.Min(estimatedDamage, 100f);
        }

        return 10f; // Default low threat
    }

    /// <inheritdoc/>
    public float EvaluateHPThreat(object target)
    {
        var (_, _, hp, maxHP, _) = ExtractTargetInfo(target);

        if (maxHP <= 0)
            return 0f;

        var hpPercent = (float)hp / maxHP;

        // Inverse relationship: low HP = high threat score (easier to finish off)
        var inverseThreat = (1.0f - hpPercent) * 20f;

        return inverseThreat;
    }

    /// <inheritdoc/>
    public float EvaluatePositionThreat(object target, BattlefieldGrid? grid)
    {
        if (grid == null)
            return 0f;

        var (_, _, _, _, targetPos) = ExtractTargetInfo(target);

        if (!targetPos.HasValue)
            return 0f;

        float score = 0f;

        // Elevated targets are harder to reach (-5)
        if (targetPos.Value.Elevation > 0)
            score -= 5f;

        // Targets in cover are lower priority (-3 per cover level)
        if (_coverService != null)
        {
            var coverLevel = _coverService.GetCoverLevel(grid, targetPos.Value);
            score -= (int)coverLevel * 3f;
        }

        // Isolated targets are higher priority (+8)
        // TODO: Implement proper ally proximity check
        // For now, simplified heuristic

        return score;
    }

    /// <inheritdoc/>
    public async Task<float> EvaluateAbilityThreatAsync(object target)
    {
        // TODO: Implement full ability threat assessment in v0.42.2
        // For now, simplified heuristic

        if (target is PlayerCharacter player)
        {
            float threat = 0f;

            // Estimate based on character archetype/class
            // High Might = likely high damage abilities
            if (player.Attributes.Might > 10)
                threat += 15f;

            // High Finesse = likely status effect abilities
            if (player.Attributes.Finesse > 10)
                threat += 10f;

            return Math.Min(threat, 30f);
        }

        return 0f;
    }

    /// <inheritdoc/>
    public float EvaluateStatusEffectThreat(object target)
    {
        float threat = 0f;

        // Check for status effects on target
        if (target is PlayerCharacter player)
        {
            foreach (var effect in player.StatusEffects)
            {
                if (effect.Category == StatusEffectCategory.Buff)
                    threat += 5f; // Buffed enemies are more dangerous
                else if (effect.Category == StatusEffectCategory.ControlDebuff ||
                         effect.Category == StatusEffectCategory.DamageOverTime)
                    threat -= 3f; // Debuffed enemies are less dangerous
            }
        }
        else if (target is Enemy enemy)
        {
            foreach (var effect in enemy.StatusEffects)
            {
                if (effect.Category == StatusEffectCategory.Buff)
                    threat += 5f;
                else if (effect.Category == StatusEffectCategory.ControlDebuff ||
                         effect.Category == StatusEffectCategory.DamageOverTime)
                    threat -= 3f;
            }
        }

        return Math.Clamp(threat, -10f, 10f);
    }

    // ===== HELPER METHODS =====

    private (string id, string name, int hp, int maxHP, GridPosition? pos) ExtractTargetInfo(object target)
    {
        if (target is PlayerCharacter player)
        {
            return (player.Id.ToString(), player.Name, player.HP, player.MaxHP, player.Position);
        }

        if (target is Enemy enemy)
        {
            return (enemy.Id, enemy.Name, enemy.HP, enemy.MaxHP, enemy.Position);
        }

        return (string.Empty, "Unknown", 0, 1, null);
    }

    private string GenerateReasoningText(Dictionary<ThreatFactor, float> factors)
    {
        var primary = factors.OrderByDescending(kvp => Math.Abs(kvp.Value)).First();
        return $"Primary threat factor: {primary.Key} ({primary.Value:F1})";
    }

    private AIThreatWeights CreateDefaultWeights(AIArchetype archetype)
    {
        // Fallback default weights (Tactical pattern)
        return new AIThreatWeights
        {
            Archetype = archetype,
            ArchetypeName = archetype.ToString(),
            DamageWeight = 0.30m,
            HPWeight = 0.25m,
            PositionWeight = 0.25m,
            AbilityWeight = 0.15m,
            StatusWeight = 0.05m
        };
    }
}
