using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Models;
using RuneAndRust.Engine.Simulation;

namespace RuneAndRust.Engine.Services;

public class CombatSimulationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CombatSimulationService> _logger;
    private readonly IEnemyFactory _enemyFactory;

    public CombatSimulationService(
        IServiceScopeFactory scopeFactory,
        ILogger<CombatSimulationService> logger,
        IEnemyFactory enemyFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _enemyFactory = enemyFactory;
    }

    public async Task<SimBatchResult> RunBatchAsync(string archetypeStr, string enemyId, int count)
    {
        _logger.LogInformation("Starting simulation: {Archetype} vs {Enemy} ({Count} matches).", archetypeStr, enemyId, count);

        // Parse archetype
        if (!Enum.TryParse<ArchetypeType>(archetypeStr, true, out var archetype))
        {
            archetype = ArchetypeType.Warrior; // Default
        }

        var results = new ConcurrentBag<SimMatchResult>();

        // Parallel processing for speed
        await Parallel.ForEachAsync(Enumerable.Range(0, count), async (_, token) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var combatService = scope.ServiceProvider.GetRequiredService<ICombatService>();
            var agent = new SimulatedPlayerAgent(combatService);
            var enemyFactory = scope.ServiceProvider.GetRequiredService<IEnemyFactory>();

            // Resolve GameState once per scope, outside the loop
            var gameState = scope.ServiceProvider.GetRequiredService<GameState>();

            // Setup isolated combat context
            // Note: CharacterFactory isn't injected here as we create a simple transient one
            var player = CharacterFactory.CreateSimple("SimHero", LineageType.Human, archetype);
            var enemy = enemyFactory.CreateById(enemyId);

            if (enemy == null)
            {
                // Fallback for simulation if ID invalid
                enemy = enemyFactory.CreateById("und_draugr_01");
            }

            combatService.StartCombat(new List<Enemy> { enemy! });

            int turns = 0;
            int startStamina = player.CurrentStamina;

            // Combat Loop
            // We need to inspect internal state of combat service or loop until EndCombat returns result
            // Since CombatService doesn't expose IsCombatActive directly via interface usually, we rely on checking state

            // HACK: For simulation, we assume CombatService state is managed correctly.
            // We'll loop up to a max limit to prevent infinite loops.
            while (player.CurrentHP > 0 && enemy!.CurrentHP > 0 && turns < 100)
            {
                turns++;

                if (gameState.CombatState == null) break;

                var active = gameState.CombatState.ActiveCombatant;
                if (active == null) break;

                if (active.IsPlayer)
                {
                    // Player Turn
                    // Map active combatant to Character entity roughly
                    await agent.TakeTurnAsync(player, enemy!);
                }
                else
                {
                    // Enemy Turn
                    // Need to find the Enemy combatant wrapper
                     var enemyCombatant = gameState.CombatState.TurnOrder.First(c => !c.IsPlayer);
                     await combatService.ProcessEnemyTurnAsync(enemyCombatant);
                }
            }

            var winner = player.CurrentHP > 0 ? "Player" : "Enemy";
            results.Add(new SimMatchResult(
                Winner: winner,
                Turns: turns,
                PlayerRemainingHp: player.CurrentHP,
                PlayerMaxHp: player.MaxHP,
                StaminaSpent: startStamina - player.CurrentStamina,
                PlayerDied: player.CurrentHP <= 0
            ));
        });

        // Aggregation
        var list = results.ToList();
        var winRate = (double)list.Count(r => r.Winner == "Player") / count * 100.0;
        var avgTurns = list.Average(r => r.Turns);
        var avgHp = list.Average(r => r.PlayerRemainingHp);
        var avgStamina = list.Average(r => r.StaminaSpent);

        _logger.LogInformation("Simulation complete. Win Rate: {WinRate:F1}%. Avg Turns: {AvgTurns:F1}.", winRate, avgTurns);

        return new SimBatchResult(
            Archetype: archetype.ToString(),
            EnemyId: enemyId,
            TotalMatches: count,
            WinRate: winRate,
            AvgTurns: avgTurns,
            AvgHpRemaining: avgHp,
            AvgStaminaSpent: avgStamina
        );
    }
}
