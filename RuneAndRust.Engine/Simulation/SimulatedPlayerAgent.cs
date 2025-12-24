using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Simulation;

public class SimulatedPlayerAgent
{
    private readonly ICombatService _combat;

    public SimulatedPlayerAgent(ICombatService combat)
    {
        _combat = combat;
    }

    public async Task TakeTurnAsync(Character player, Enemy target)
    {
        // Simple Heuristic: Heal if critical
        // Use capitalized HP properties to match Character.cs definition
        if (player.CurrentHP < player.MaxHP * 0.3)
        {
            // Simplified: In real imp, check inventory for potion
            // For v0.3.13b, we might assume infinite potions or skip healing
            // Future: Implement potion usage logic
        }

        // Aggressive Strategy: Dump Stamina on Heavy Attacks
        if (player.CurrentStamina >= 40)
        {
             await _combat.ExecutePlayerAttackAsync(AttackType.Heavy, target);
             return;
        }

        // Conservation Strategy
        await _combat.ExecutePlayerAttackAsync(AttackType.Light, target);
    }
}
