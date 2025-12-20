using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages the lifecycle of combat encounters including starting, advancing turns, and ending combat.
/// </summary>
public class CombatService : ICombatService
{
    private readonly GameState _gameState;
    private readonly IInitiativeService _initiative;
    private readonly IAttackResolutionService _attackResolution;
    private readonly ILogger<CombatService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatService"/> class.
    /// </summary>
    /// <param name="gameState">The shared game state singleton.</param>
    /// <param name="initiative">The initiative service for turn order calculations.</param>
    /// <param name="attackResolution">The attack resolution service for combat mechanics.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CombatService(
        GameState gameState,
        IInitiativeService initiative,
        IAttackResolutionService attackResolution,
        ILogger<CombatService> logger)
    {
        _gameState = gameState;
        _initiative = initiative;
        _attackResolution = attackResolution;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void StartCombat(List<Enemy> enemies)
    {
        _logger.LogInformation("Initializing Combat. Enemies: {Count}", enemies.Count);

        if (_gameState.CurrentCharacter == null)
        {
            _logger.LogWarning("Cannot start combat without active character");
            return;
        }

        var state = new CombatState();

        // Add player
        var player = Combatant.FromCharacter(_gameState.CurrentCharacter);
        _initiative.RollInitiative(player);
        state.TurnOrder.Add(player);
        _logger.LogDebug("Added player {Name} to combat", player.Name);

        // Add enemies
        foreach (var enemy in enemies)
        {
            var combatant = Combatant.FromEnemy(enemy);
            _initiative.RollInitiative(combatant);
            state.TurnOrder.Add(combatant);
            _logger.LogDebug("Added enemy {Name} to combat", combatant.Name);
        }

        // Sort by initiative
        state.TurnOrder = _initiative.SortTurnOrder(state.TurnOrder);

        // Update global state
        _gameState.CombatState = state;
        _gameState.Phase = GamePhase.Combat;

        _logger.LogInformation("Combat Started. Round {Round}. Active: {Name}",
            state.RoundNumber, state.ActiveCombatant?.Name ?? "None");
    }

    /// <inheritdoc/>
    public void NextTurn()
    {
        if (_gameState.CombatState == null)
        {
            _logger.LogWarning("NextTurn called but no combat is active");
            return;
        }

        var state = _gameState.CombatState;
        state.TurnIndex++;

        if (state.TurnIndex >= state.TurnOrder.Count)
        {
            state.TurnIndex = 0;
            state.RoundNumber++;
            _logger.LogInformation("Round {Round} begins", state.RoundNumber);
        }

        _logger.LogInformation("Turn: {Name} ({Type})",
            state.ActiveCombatant?.Name ?? "None",
            state.ActiveCombatant?.IsPlayer == true ? "Player" : "Enemy");
    }

    /// <inheritdoc/>
    public void EndCombat()
    {
        _logger.LogInformation("Combat Ended");
        _gameState.Phase = GamePhase.Exploration;
        _gameState.CombatState = null;
    }

    /// <inheritdoc/>
    public string ExecutePlayerAttack(string targetName, AttackType attackType)
    {
        var state = _gameState.CombatState;

        // Validate combat state
        if (state == null)
        {
            _logger.LogWarning("ExecutePlayerAttack called but no combat is active");
            return "You are not in combat.";
        }

        // Validate it's the player's turn
        if (!state.IsPlayerTurn)
        {
            _logger.LogDebug("Attack attempted but it is not the player's turn");
            return "It is not your turn.";
        }

        var attacker = state.ActiveCombatant!;

        // Find target by name (partial match, case-insensitive)
        var target = state.TurnOrder.FirstOrDefault(c =>
            !c.IsPlayer && c.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            _logger.LogDebug("Target '{TargetName}' not found in combat", targetName);
            return $"Target '{targetName}' not found.";
        }

        // Check stamina affordability
        var staminaCost = _attackResolution.GetStaminaCost(attackType);
        if (!_attackResolution.CanAffordAttack(attacker, attackType))
        {
            _logger.LogDebug(
                "{Attacker} cannot afford {AttackType} attack. Stamina: {Current}/{Cost}",
                attacker.Name, attackType, attacker.CurrentStamina, staminaCost);
            return $"Not enough stamina for {attackType.ToString().ToLower()} attack. Need {staminaCost}, have {attacker.CurrentStamina}.";
        }

        // Deduct stamina cost
        attacker.CurrentStamina -= staminaCost;
        _logger.LogDebug(
            "{Attacker} spent {Cost} stamina. Remaining: {Current}/{Max}",
            attacker.Name, staminaCost, attacker.CurrentStamina, attacker.MaxStamina);

        // Resolve the attack
        var result = _attackResolution.ResolveMeleeAttack(attacker, target, attackType);

        _logger.LogInformation(
            "{Attacker} attacks {Target} ({AttackType}): {Outcome}",
            attacker.Name, target.Name, attackType, result.Outcome);

        // Apply damage to target
        if (result.IsHit)
        {
            target.CurrentHp -= result.FinalDamage;
            _logger.LogDebug(
                "{Target} took {Damage} damage. HP: {Current}/{Max}",
                target.Name, result.FinalDamage, target.CurrentHp, target.MaxHp);

            // Check for death
            if (target.CurrentHp <= 0)
            {
                _logger.LogWarning("{Target} was slain! HP: 0/{Max}", target.Name, target.MaxHp);
                RemoveDefeatedCombatant(target);

                // Check victory condition
                if (CheckVictoryCondition())
                {
                    _logger.LogInformation("Combat Victory! All enemies defeated.");
                    return BuildVictoryMessage(result, target);
                }

                return BuildDeathMessage(result, target);
            }

            return BuildHitMessage(result, target);
        }

        return BuildMissMessage(result, target);
    }

    /// <inheritdoc/>
    public void RemoveDefeatedCombatant(Combatant combatant)
    {
        var state = _gameState.CombatState;
        if (state == null) return;

        var index = state.TurnOrder.IndexOf(combatant);
        if (index < 0) return;

        state.TurnOrder.Remove(combatant);
        _logger.LogDebug("Removed {Name} from turn order", combatant.Name);

        // Adjust turn index if necessary
        if (index < state.TurnIndex)
        {
            state.TurnIndex--;
        }
        else if (index == state.TurnIndex && state.TurnIndex >= state.TurnOrder.Count)
        {
            state.TurnIndex = 0;
        }
    }

    /// <inheritdoc/>
    public bool CheckVictoryCondition()
    {
        var state = _gameState.CombatState;
        if (state == null) return false;

        // Victory if no enemies remain
        return !state.TurnOrder.Any(c => !c.IsPlayer);
    }

    /// <inheritdoc/>
    public string GetCombatStatus()
    {
        var state = _gameState.CombatState;
        if (state == null)
        {
            return "No active combat.";
        }

        var lines = new List<string>
        {
            $"=== COMBAT STATUS (Round {state.RoundNumber}) ==="
        };

        foreach (var combatant in state.TurnOrder)
        {
            var turnMarker = combatant == state.ActiveCombatant ? " <<" : "";
            var typeLabel = combatant.IsPlayer ? "[PLAYER]" : "[ENEMY]";

            lines.Add($"  {typeLabel} {combatant.Name}: HP {combatant.CurrentHp}/{combatant.MaxHp}, Stamina {combatant.CurrentStamina}/{combatant.MaxStamina}{turnMarker}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    #region Message Builders

    private static string BuildHitMessage(AttackResult result, Combatant target)
    {
        var outcomeText = result.Outcome switch
        {
            AttackOutcome.Glancing => "Glancing blow!",
            AttackOutcome.Solid => "Solid hit!",
            AttackOutcome.Critical => "CRITICAL HIT!",
            _ => "Hit!"
        };

        return $"{outcomeText} You dealt {result.FinalDamage} damage to {target.Name}. (HP: {target.CurrentHp}/{target.MaxHp})";
    }

    private static string BuildMissMessage(AttackResult result, Combatant target)
    {
        return result.Outcome == AttackOutcome.Fumble
            ? $"FUMBLE! Your attack goes wildly astray, missing {target.Name} entirely."
            : $"Miss! Your attack was evaded by {target.Name}.";
    }

    private static string BuildDeathMessage(AttackResult result, Combatant target)
    {
        var outcomeText = result.Outcome switch
        {
            AttackOutcome.Critical => "CRITICAL STRIKE!",
            _ => "FATAL BLOW!"
        };

        return $"{outcomeText} You struck {target.Name} for {result.FinalDamage} damage. They fall dead!";
    }

    private static string BuildVictoryMessage(AttackResult result, Combatant target)
    {
        return $"VICTORY! You struck down {target.Name} with {result.FinalDamage} damage. All enemies defeated!";
    }

    #endregion
}
