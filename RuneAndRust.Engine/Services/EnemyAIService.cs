using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements archetype-based AI decision logic for enemy combatants.
/// Uses weighted probability tables modified by state triggers.
/// </summary>
public class EnemyAIService : IEnemyAIService
{
    private readonly IDiceService _dice;
    private readonly IAttackResolutionService _attackResolution;
    private readonly ILogger<EnemyAIService> _logger;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="EnemyAIService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for probability rolls.</param>
    /// <param name="attackResolution">The attack resolution service for stamina checks.</param>
    /// <param name="logger">The logger for traceability.</param>
    public EnemyAIService(
        IDiceService dice,
        IAttackResolutionService attackResolution,
        ILogger<EnemyAIService> logger)
    {
        _dice = dice;
        _attackResolution = attackResolution;
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
            _logger.LogDebug("[AI] No valid target found, passing turn");
            return new CombatAction(ActionType.Pass, enemy.Id, null, null, "finds no threats.");
        }

        // Dispatch to archetype-specific logic
        return enemy.Archetype switch
        {
            EnemyArchetype.Tank => ExecuteTankLogic(enemy, target),
            EnemyArchetype.GlassCannon => ExecuteAggressiveLogic(enemy, target),
            EnemyArchetype.Swarm => ExecuteSwarmLogic(enemy, target),
            EnemyArchetype.Support => ExecuteSupportLogic(enemy, target),
            EnemyArchetype.Caster => ExecuteCasterLogic(enemy, target),
            EnemyArchetype.Boss => ExecuteBossLogic(enemy, target),
            _ => ExecuteAggressiveLogic(enemy, target) // DPS and default
        };
    }

    /// <summary>
    /// Executes aggressive AI logic (DPS/GlassCannon archetypes).
    /// Prefers standard attacks with 20% chance for heavy attacks.
    /// Flees when cowardly and below 25% HP.
    /// </summary>
    private CombatAction ExecuteAggressiveLogic(Combatant self, Combatant target)
    {
        // Check flee trigger (Cowardly tag + low HP)
        var hpPercent = self.MaxHp > 0 ? (float)self.CurrentHp / self.MaxHp : 1f;
        if (self.Tags.Contains("Cowardly") && hpPercent < LowHpThreshold)
        {
            _logger.LogDebug("[AI] Trigger matched: Cowardly+LowHP (Val: {Hp}%)", (int)(hpPercent * 100));
            return new CombatAction(ActionType.Flee, self.Id, null, null, "panics and attempts to flee!");
        }

        // Roll for attack type selection
        var roll = _dice.RollSingle(100, "AI Behavior Check");
        _logger.LogDebug("[AI] Behavior roll: {Roll}", roll);

        // Heavy attack if roll >= 80 and enough stamina
        if (roll >= HeavyAttackThreshold && _attackResolution.CanAffordAttack(self, AttackType.Heavy))
        {
            _logger.LogInformation("[AI] {Name} chose Heavy Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Heavy,
                "winds up a devastating blow!");
        }

        // Standard attack as default
        if (_attackResolution.CanAffordAttack(self, AttackType.Standard))
        {
            _logger.LogInformation("[AI] {Name} chose Standard Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Standard,
                "attacks with practiced precision.");
        }

        // Fallback to light attack if low on stamina
        if (_attackResolution.CanAffordAttack(self, AttackType.Light))
        {
            _logger.LogWarning("[AI] Wanted Standard but insufficient Stamina. Fallback: Light Attack");
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Light,
                "makes a quick jab.");
        }

        // No stamina for any attack
        _logger.LogWarning("[AI] {Name} has no stamina, passing turn", self.Name);
        return new CombatAction(ActionType.Pass, self.Id, null, null, "hesitates, exhausted.");
    }

    /// <summary>
    /// Executes tank AI logic.
    /// Defends when below 40% HP, otherwise 60% attack / 40% defend split.
    /// </summary>
    private CombatAction ExecuteTankLogic(Combatant self, Combatant target)
    {
        var hpPercent = self.MaxHp > 0 ? (float)self.CurrentHp / self.MaxHp : 1f;

        // Defend when wounded
        if (hpPercent < WoundedThreshold)
        {
            _logger.LogDebug("[AI] Trigger matched: Tank+Wounded (Val: {Hp}%)", (int)(hpPercent * 100));
            return new CombatAction(ActionType.Defend, self.Id, null, null, "raises their guard defensively.");
        }

        // Roll for behavior (60% attack, 40% defend even when healthy)
        var roll = _dice.RollSingle(100, "AI Behavior Check");

        if (roll < 60 && _attackResolution.CanAffordAttack(self, AttackType.Standard))
        {
            _logger.LogInformation("[AI] {Name} chose Standard Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Standard,
                "swings with mechanical precision.");
        }

        // Default to defensive stance
        _logger.LogInformation("[AI] {Name} chose Defend. Roll: {Roll}", self.Name, roll);
        return new CombatAction(ActionType.Defend, self.Id, null, null, "braces for impact.");
    }

    /// <summary>
    /// Executes swarm AI logic.
    /// Always uses light attacks to conserve stamina for numbers advantage.
    /// </summary>
    private CombatAction ExecuteSwarmLogic(Combatant self, Combatant target)
    {
        // Swarm: Always light attacks (conserve stamina for numbers)
        if (_attackResolution.CanAffordAttack(self, AttackType.Light))
        {
            _logger.LogInformation("[AI] {Name} (Swarm) chose Light Attack vs {Target}",
                self.Name, target.Name);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Light,
                "darts in for a quick strike.");
        }

        return new CombatAction(ActionType.Pass, self.Id, null, null, "circles warily.");
    }

    /// <summary>
    /// Executes support AI logic.
    /// Prefers light attacks (70%) to conserve stamina for future abilities.
    /// </summary>
    private CombatAction ExecuteSupportLogic(Combatant self, Combatant target)
    {
        // Support: Prefers light attacks (saving stamina for future abilities)
        // Note: Actual buff/debuff abilities will be added in v0.2.2c
        var roll = _dice.RollSingle(100, "AI Behavior Check");

        if (roll < 70 && _attackResolution.CanAffordAttack(self, AttackType.Light))
        {
            _logger.LogInformation("[AI] {Name} (Support) chose Light Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Light,
                "takes a cautious swing.");
        }

        if (_attackResolution.CanAffordAttack(self, AttackType.Standard))
        {
            _logger.LogInformation("[AI] {Name} (Support) chose Standard Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Standard,
                "strikes opportunistically.");
        }

        return new CombatAction(ActionType.Pass, self.Id, null, null, "assesses the situation.");
    }

    /// <summary>
    /// Executes caster AI logic.
    /// Uses standard attacks with ranged flavor text.
    /// </summary>
    private CombatAction ExecuteCasterLogic(Combatant self, Combatant target)
    {
        // Caster: Standard attacks (ranged flavor text, same mechanics)
        // Note: Actual ranged abilities will be added in v0.2.2c
        if (_attackResolution.CanAffordAttack(self, AttackType.Standard))
        {
            _logger.LogInformation("[AI] {Name} (Caster) chose Standard Attack vs {Target}",
                self.Name, target.Name);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Standard,
                "hurls corrupted energy!");
        }

        return new CombatAction(ActionType.Pass, self.Id, null, null, "gathers power.");
    }

    /// <summary>
    /// Executes boss AI logic.
    /// Aggressive with 50% heavy attack chance.
    /// </summary>
    private CombatAction ExecuteBossLogic(Combatant self, Combatant target)
    {
        // Boss: Aggressive, prefers heavy attacks
        var roll = _dice.RollSingle(100, "AI Behavior Check");

        if (roll >= 50 && _attackResolution.CanAffordAttack(self, AttackType.Heavy))
        {
            _logger.LogInformation("[AI] {Name} (Boss) chose Heavy Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Heavy,
                "unleashes a devastating strike!");
        }

        if (_attackResolution.CanAffordAttack(self, AttackType.Standard))
        {
            _logger.LogInformation("[AI] {Name} (Boss) chose Standard Attack vs {Target}. Roll: {Roll}",
                self.Name, target.Name, roll);
            return new CombatAction(ActionType.Attack, self.Id, target.Id, AttackType.Standard,
                "presses the assault.");
        }

        return new CombatAction(ActionType.Defend, self.Id, null, null, "readies for the next phase.");
    }
}
