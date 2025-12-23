using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements utility-based AI decision logic for enemy combatants.
/// Evaluates all valid actions and selects via weighted random (v0.2.4b).
/// </summary>
public class EnemyAIService : IEnemyAIService
{
    private readonly IDiceService _dice;
    private readonly IAttackResolutionService _attackResolution;
    private readonly IAbilityService _abilityService;
    private readonly ILogger<EnemyAIService> _logger;

    // ═══════════════════════════════════════════════════════════════════════
    // HP/State Thresholds
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// HP threshold (25%) that triggers flee behavior for cowardly enemies.
    /// </summary>
    private const float LowHpThreshold = 0.25f;

    /// <summary>
    /// HP threshold (40%) that triggers defensive behavior for tanks.
    /// </summary>
    private const float WoundedThreshold = 0.40f;

    /// <summary>
    /// Roll threshold (d100 >= 80) that triggers heavy attack for aggressive archetypes.
    /// </summary>
    private const int HeavyAttackThreshold = 80;

    // ═══════════════════════════════════════════════════════════════════════
    // Utility Scoring Constants (v0.2.4b)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base score for all action evaluations.
    /// </summary>
    private const int BaseScore = 50;

    /// <summary>
    /// Bonus when heal ability is available and HP is below 30%.
    /// </summary>
    private const int CriticalHpHealBonus = 50;

    /// <summary>
    /// Penalty when heal ability is available but HP is above 80%.
    /// </summary>
    private const int WastefulHealPenalty = -40;

    /// <summary>
    /// Bonus for damage abilities when target HP is below 20% (kill range).
    /// </summary>
    private const int KillRangeBonus = 30;

    /// <summary>
    /// Penalty when ability cost exceeds 50% of current stamina.
    /// </summary>
    private const int StaminaConservationPenalty = -20;

    /// <summary>
    /// Penalty when a debuff ability targets someone already debuffed with that status.
    /// </summary>
    private const int RedundantDebuffPenalty = -100;

    /// <summary>
    /// Bonus for GlassCannon archetype when selecting damage abilities.
    /// </summary>
    private const int ArchetypeDamageBonus = 20;

    /// <summary>
    /// Bonus for Tank archetype when selecting Defend action while below 50% HP.
    /// </summary>
    private const int TankDefendBonus = 25;

    /// <summary>
    /// Minimum score threshold; actions scoring below this are filtered out.
    /// </summary>
    private const int MinimumActionScore = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnemyAIService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for probability rolls.</param>
    /// <param name="attackResolution">The attack resolution service for stamina checks.</param>
    /// <param name="abilityService">The ability service for ability validation (v0.2.4b).</param>
    /// <param name="logger">The logger for traceability.</param>
    public EnemyAIService(
        IDiceService dice,
        IAttackResolutionService attackResolution,
        IAbilityService abilityService,
        ILogger<EnemyAIService> logger)
    {
        _dice = dice;
        _attackResolution = attackResolution;
        _abilityService = abilityService;
        _logger = logger;
    }

    /// <inheritdoc />
    public CombatAction DetermineAction(Combatant enemy, CombatState state)
    {
        _logger.LogTrace(
            "[AI] {Name} (Arch:{Archetype}) thinking. HP: {Hp}% Stm: {Stamina}",
            enemy.Name,
            enemy.Archetype,
            enemy.MaxHp > 0 ? (int)((float)enemy.CurrentHp / enemy.MaxHp * 100) : 0,
            enemy.CurrentStamina);

        // Find target (simple: the player)
        var target = state.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        if (target == null)
        {
            _logger.LogWarning("[AI] No player target found");
            return new CombatAction(ActionType.Pass, enemy.Id, null, null, "finds no threats.");
        }

        // Build list of scored actions (v0.2.4b utility scoring)
        var scoredActions = new List<(CombatAction Action, int Score)>();

        // 1. Evaluate Basic Attacks (baseline)
        EvaluateBasicAttacks(enemy, target, scoredActions);

        // 2. Evaluate Active Abilities
        EvaluateAbilities(enemy, target, scoredActions);

        // 3. Evaluate Defend (archetype-specific)
        EvaluateDefend(enemy, scoredActions);

        // 4. Evaluate Flee (if Cowardly tag and low HP)
        EvaluateFlee(enemy, scoredActions);

        // 5. Weighted selection
        return SelectBestAction(scoredActions, enemy);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Utility Scoring Methods (v0.2.4b)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Evaluates basic attack options and adds scored actions to the list.
    /// </summary>
    private void EvaluateBasicAttacks(Combatant enemy, Combatant target,
        List<(CombatAction Action, int Score)> scoredActions)
    {
        var targetHpPercent = target.MaxHp > 0 ? (float)target.CurrentHp / target.MaxHp : 1f;

        // Heavy Attack
        if (_attackResolution.CanAffordAttack(enemy, AttackType.Heavy))
        {
            int score = BaseScore + 15; // Heavier attacks get slight bonus
            if (targetHpPercent < 0.2f) score += KillRangeBonus;
            if (enemy.Archetype == EnemyArchetype.GlassCannon) score += ArchetypeDamageBonus;
            if (enemy.Archetype == EnemyArchetype.Boss) score += 10;

            scoredActions.Add((new CombatAction(ActionType.Attack, enemy.Id, target.Id,
                AttackType.Heavy, "winds up a devastating blow!"), score));
        }

        // Standard Attack
        if (_attackResolution.CanAffordAttack(enemy, AttackType.Standard))
        {
            int score = BaseScore + 5;
            if (targetHpPercent < 0.2f) score += KillRangeBonus / 2;
            if (enemy.Archetype == EnemyArchetype.GlassCannon) score += ArchetypeDamageBonus / 2;

            scoredActions.Add((new CombatAction(ActionType.Attack, enemy.Id, target.Id,
                AttackType.Standard, "attacks with practiced precision."), score));
        }

        // Light Attack (fallback, lower score)
        if (_attackResolution.CanAffordAttack(enemy, AttackType.Light))
        {
            int score = BaseScore - 10;
            if (enemy.Archetype == EnemyArchetype.Swarm) score += 20; // Swarm prefers light attacks

            scoredActions.Add((new CombatAction(ActionType.Attack, enemy.Id, target.Id,
                AttackType.Light, "makes a quick jab."), score));
        }
    }

    /// <summary>
    /// Evaluates active abilities and adds scored actions to the list.
    /// </summary>
    private void EvaluateAbilities(Combatant enemy, Combatant target,
        List<(CombatAction Action, int Score)> scoredActions)
    {
        foreach (var ability in enemy.Abilities)
        {
            if (!_abilityService.CanUse(enemy, ability))
            {
                _logger.LogTrace("[AI] {Enemy} cannot use {Ability}: blocked by cooldown/resources",
                    enemy.Name, ability.Name);
                continue;
            }

            int score = CalculateAbilityScore(enemy, ability, target);

            if (score >= MinimumActionScore)
            {
                var action = new CombatAction(
                    ActionType.UseAbility,
                    enemy.Id,
                    target.Id,
                    AbilityId: ability.Id,
                    FlavorText: $"uses {ability.Name}!");

                scoredActions.Add((action, score));
                _logger.LogTrace("[AI] Evaluated {Ability}: Score {Score}", ability.Name, score);
            }
        }
    }

    /// <summary>
    /// Calculates utility score for an ability based on context.
    /// </summary>
    private int CalculateAbilityScore(Combatant user, ActiveAbility ability, Combatant target)
    {
        int score = BaseScore;
        var userHpPercent = user.MaxHp > 0 ? (float)user.CurrentHp / user.MaxHp : 1f;
        var targetHpPercent = target.MaxHp > 0 ? (float)target.CurrentHp / target.MaxHp : 1f;

        // Heal scoring
        if (ability.EffectScript.Contains("HEAL", StringComparison.OrdinalIgnoreCase))
        {
            if (userHpPercent < 0.3f) score += CriticalHpHealBonus;
            else if (userHpPercent > 0.8f) score += WastefulHealPenalty;
        }

        // Damage scoring
        if (ability.EffectScript.Contains("DAMAGE", StringComparison.OrdinalIgnoreCase))
        {
            if (targetHpPercent < 0.2f) score += KillRangeBonus;

            // Archetype bonus for damage dealers
            if (user.Archetype == EnemyArchetype.GlassCannon)
                score += ArchetypeDamageBonus;
        }

        // Status effect scoring (avoid redundant debuffs)
        if (ability.EffectScript.Contains("STATUS", StringComparison.OrdinalIgnoreCase))
        {
            var statusType = ParseStatusFromEffectScript(ability.EffectScript);
            if (statusType != null && target.StatusEffects.Any(s => s.Type == statusType))
                score += RedundantDebuffPenalty;
        }

        // Resource conservation
        if (user.MaxStamina > 0 && ability.StaminaCost > user.CurrentStamina * 0.5)
            score += StaminaConservationPenalty;

        return score;
    }

    /// <summary>
    /// Parses the status effect type from an EffectScript STATUS command.
    /// </summary>
    private StatusEffectType? ParseStatusFromEffectScript(string effectScript)
    {
        // FORMAT: STATUS:Type:Duration:Stacks
        var commands = effectScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var cmd in commands)
        {
            var parts = cmd.Trim().Split(':');
            if (parts.Length >= 2 &&
                parts[0].Equals("STATUS", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<StatusEffectType>(parts[1], ignoreCase: true, out var statusType))
                    return statusType;
            }
        }
        return null;
    }

    /// <summary>
    /// Evaluates defend action based on archetype and HP.
    /// </summary>
    private void EvaluateDefend(Combatant enemy, List<(CombatAction Action, int Score)> scoredActions)
    {
        var hpPercent = enemy.MaxHp > 0 ? (float)enemy.CurrentHp / enemy.MaxHp : 1f;
        int score = BaseScore - 15; // Defend is generally less preferred

        // Tank bonus when wounded
        if (enemy.Archetype == EnemyArchetype.Tank)
        {
            if (hpPercent < WoundedThreshold)
            {
                score += TankDefendBonus + 30; // Strong preference when wounded
            }
            else
            {
                score += 10; // Tanks like defending even when healthy
            }
        }

        // Support archetype tends to defend more
        if (enemy.Archetype == EnemyArchetype.Support && hpPercent < 0.5f)
            score += 15;

        scoredActions.Add((new CombatAction(ActionType.Defend, enemy.Id, null,
            FlavorText: "braces for impact."), score));
    }

    /// <summary>
    /// Evaluates flee action if enemy is cowardly and low HP.
    /// </summary>
    private void EvaluateFlee(Combatant enemy, List<(CombatAction Action, int Score)> scoredActions)
    {
        if (!enemy.Tags.Contains("Cowardly")) return;

        var hpPercent = enemy.MaxHp > 0 ? (float)enemy.CurrentHp / enemy.MaxHp : 1f;

        if (hpPercent < LowHpThreshold)
        {
            // Very high priority when cowardly and low HP
            int score = BaseScore + 80;
            _logger.LogDebug("[AI] Cowardly+LowHP trigger. Flee score: {Score}", score);

            scoredActions.Add((new CombatAction(ActionType.Flee, enemy.Id, null,
                FlavorText: "panics and attempts to flee!"), score));
        }
    }

    /// <summary>
    /// Selects an action using weighted random selection.
    /// Higher-scoring actions are more likely but not guaranteed.
    /// </summary>
    private CombatAction SelectBestAction(List<(CombatAction Action, int Score)> actions,
        Combatant enemy)
    {
        if (actions.Count == 0)
        {
            _logger.LogWarning("[AI] {Enemy} has no valid actions, passing turn", enemy.Name);
            return new CombatAction(ActionType.Pass, enemy.Id, null,
                FlavorText: "hesitates, exhausted.");
        }

        // Filter out negative scores
        var validActions = actions.Where(a => a.Score >= MinimumActionScore).ToList();
        if (validActions.Count == 0)
        {
            _logger.LogWarning("[AI] {Enemy} has no actions above minimum score, passing turn", enemy.Name);
            return new CombatAction(ActionType.Pass, enemy.Id, null,
                FlavorText: "hesitates, uncertain.");
        }

        // Normalize scores (shift so minimum is 1 to avoid zero weights)
        var minScore = validActions.Min(a => a.Score);
        var normalizedActions = validActions
            .Select(a => (a.Action, Weight: Math.Max(1, a.Score - minScore + 1)))
            .ToList();

        var totalWeight = normalizedActions.Sum(a => a.Weight);
        var roll = _dice.RollSingle(totalWeight, "AI Action Selection");

        int cumulative = 0;
        foreach (var (action, weight) in normalizedActions)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                _logger.LogDebug("[AI] {Enemy} selected {Action} (AbilityId: {AbilityId})",
                    enemy.Name, action.Type, action.AbilityId);
                return action;
            }
        }

        // Fallback to highest score (should never reach here)
        var bestAction = validActions.OrderByDescending(a => a.Score).First().Action;
        _logger.LogDebug("[AI] {Enemy} fallback to best action: {Action}", enemy.Name, bestAction.Type);
        return bestAction;
    }
}
