using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Simulation;

/// <summary>
/// Simulated player AI agent for headless combat simulations.
/// Uses a simple heuristic strategy to make "reasonable" player decisions.
/// </summary>
/// <remarks>
/// This agent implements a baseline strategy suitable for balance testing.
/// It prioritizes: 1) Survival (not implemented v0.3.13b), 2) Finishers, 3) Resource-aware attacks.
/// </remarks>
public class SimulatedPlayerAgent
{
    private readonly ILogger<SimulatedPlayerAgent>? _logger;

    /// <summary>
    /// Stamina threshold for using heavy attacks.
    /// </summary>
    private const int HeavyAttackThreshold = 40;

    /// <summary>
    /// Stamina threshold for using standard attacks.
    /// </summary>
    private const int StandardAttackThreshold = 20;

    public SimulatedPlayerAgent(ILogger<SimulatedPlayerAgent>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Decides the next action for the simulated player.
    /// </summary>
    /// <param name="player">The player combatant.</param>
    /// <param name="state">The current combat state.</param>
    /// <returns>The decided combat action.</returns>
    public CombatAction DecideAction(Combatant player, CombatState state)
    {
        // Find a valid target
        var target = state.TurnOrder.FirstOrDefault(c => !c.IsPlayer && c.CurrentHp > 0);

        if (target == null)
        {
            _logger?.LogDebug("[SimAgent] No valid target found, passing turn");
            return new CombatAction(ActionType.Pass, player.Id, null);
        }

        // Priority 1: Survival (HP < 30%)
        // Note: Healing items not in scope for v0.3.13b - would require inventory access
        // Future: if (player.CurrentHp < player.MaxHp * 0.3) { use potion }

        // Priority 2: Finisher (enemy HP low enough for likely kill with heavy attack)
        // Estimate: Heavy attack could do up to 2x weapon die damage
        var estimatedMaxDamage = player.WeaponDamageDie * 2;
        if (target.CurrentHp <= estimatedMaxDamage && player.CurrentStamina >= HeavyAttackThreshold)
        {
            _logger?.LogDebug("[SimAgent] Using Heavy Attack as finisher on {Target} (HP: {HP})",
                target.Name, target.CurrentHp);
            return new CombatAction(
                ActionType.Attack,
                player.Id,
                target.Id,
                AttackType.Heavy);
        }

        // Priority 3: Resource-Aware Attack Selection
        if (player.CurrentStamina >= HeavyAttackThreshold)
        {
            _logger?.LogDebug("[SimAgent] Using Heavy Attack (Stamina: {Stamina})", player.CurrentStamina);
            return new CombatAction(
                ActionType.Attack,
                player.Id,
                target.Id,
                AttackType.Heavy);
        }
        else if (player.CurrentStamina >= StandardAttackThreshold)
        {
            _logger?.LogDebug("[SimAgent] Using Standard Attack (Stamina: {Stamina})", player.CurrentStamina);
            return new CombatAction(
                ActionType.Attack,
                player.Id,
                target.Id,
                AttackType.Standard);
        }
        else
        {
            _logger?.LogDebug("[SimAgent] Using Light Attack (Stamina: {Stamina})", player.CurrentStamina);
            return new CombatAction(
                ActionType.Attack,
                player.Id,
                target.Id,
                AttackType.Light);
        }
    }
}
