using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Analysis;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Simulation;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Monte Carlo simulation service for validating combat balance.
/// Runs batch combat simulations and produces TTK, win rate, and resource consumption metrics.
/// </summary>
/// <remarks>
/// v0.3.13b: The Combat Simulator.
/// Thread safety note: Sequential execution required due to shared GameState singleton.
/// </remarks>
public class CombatAuditService : ICombatAuditService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CombatAuditService> _logger;

    /// <summary>
    /// Maximum rounds before aborting a match (safety check).
    /// </summary>
    private const int MaxRoundsPerMatch = 100;

    /// <summary>
    /// Variance threshold for WARNING severity (5%).
    /// </summary>
    private const double WarningThreshold = 5.0;

    /// <summary>
    /// Variance threshold for CRITICAL severity (15%).
    /// </summary>
    private const double CriticalThreshold = 15.0;

    /// <summary>
    /// Expected win rate bounds for Standard tier enemies.
    /// </summary>
    private const double ExpectedWinRateMin = 70.0;
    private const double ExpectedWinRateMax = 90.0;

    /// <summary>
    /// Expected average rounds per encounter.
    /// </summary>
    private const double ExpectedAvgRoundsMin = 3.0;
    private const double ExpectedAvgRoundsMax = 8.0;

    /// <summary>
    /// Expected player hit rate bounds.
    /// </summary>
    private const double ExpectedHitRateMin = 65.0;
    private const double ExpectedHitRateMax = 85.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatAuditService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory for creating fresh DI scopes per match.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CombatAuditService(
        IServiceScopeFactory scopeFactory,
        ILogger<CombatAuditService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CombatAuditReport> RunAuditAsync(CombatAuditConfiguration config)
    {
        _logger.LogInformation(
            "[CombatAudit] Starting audit: {Iterations} matches, {Archetype} vs {Enemy} (Level {Level})",
            config.Iterations, config.PlayerArchetype, config.EnemyTemplateId, config.PlayerLevel);

        var stats = new CombatStatistics();

        // SEQUENTIAL LOOP - Required for thread safety (shared GameState, non-thread-safe Random)
        for (int i = 0; i < config.Iterations; i++)
        {
            using var scope = _scopeFactory.CreateScope();
            var matchResult = await RunSingleMatchAsync(scope, config, stats);
            stats.RecordEncounterResult(matchResult);

            // Progress logging every 10% for large batches
            if (config.Iterations >= 100 && (i + 1) % (config.Iterations / 10) == 0)
            {
                _logger.LogDebug(
                    "[CombatAudit] Progress: {Completed}/{Total} ({Percent}%)",
                    i + 1, config.Iterations, (i + 1) * 100 / config.Iterations);
            }
        }

        // Calculate variance flags
        var flags = CalculateVarianceFlags(stats, config);

        // Generate markdown report
        var markdown = GenerateMarkdownReport(stats, config, flags);

        _logger.LogInformation(
            "[CombatAudit] Complete. Win Rate: {WinRate:F1}%, Avg Rounds: {AvgRounds:F1}, Flags: {FlagCount}",
            stats.WinRate, stats.AvgRoundsPerEncounter, flags.Count(f => !f.IsWithinBounds));

        return new CombatAuditReport(stats, markdown, flags);
    }

    /// <summary>
    /// Runs a single combat match and returns the result.
    /// </summary>
    /// <param name="scope">The DI scope for this match.</param>
    /// <param name="config">The audit configuration.</param>
    /// <param name="stats">The statistics accumulator for per-attack tracking.</param>
    /// <returns>The result of the combat match.</returns>
    private async Task<CombatMatchResult> RunSingleMatchAsync(
        IServiceScope scope,
        CombatAuditConfiguration config,
        CombatStatistics stats)
    {
        // Get required services
        var combatService = scope.ServiceProvider.GetRequiredService<ICombatService>();
        var characterFactory = scope.ServiceProvider.GetRequiredService<CharacterFactory>();
        var enemyFactory = scope.ServiceProvider.GetRequiredService<IEnemyFactory>();
        var gameState = scope.ServiceProvider.GetRequiredService<GameState>();
        var attackResolution = scope.ServiceProvider.GetRequiredService<IAttackResolutionService>();

        // Reset game state for fresh simulation
        gameState.Reset();
        gameState.Phase = GamePhase.Exploration;
        gameState.IsSessionActive = true;

        // Create player character
        var character = characterFactory.CreateSimple(
            "SimHero",
            LineageType.Human,
            config.PlayerArchetype);
        gameState.CurrentCharacter = character;

        // Create enemy from template
        var enemy = await enemyFactory.CreateByIdAsync(config.EnemyTemplateId, config.PlayerLevel);

        // Start combat
        combatService.StartCombat(new List<Enemy> { enemy });

        // Initialize simulation agent
        var playerAgent = new SimulatedPlayerAgent();
        int rounds = 0;
        int staminaStart = character.CurrentStamina;

        _logger.LogTrace(
            "[Match] Starting: {Player} (HP:{PHP}, Stam:{PStam}) vs {Enemy} (HP:{EHP})",
            character.Name, character.MaxHP, character.CurrentStamina,
            enemy.Name, enemy.MaxHp);

        // Combat loop
        while (gameState.CombatState != null && rounds < MaxRoundsPerMatch)
        {
            var state = gameState.CombatState;
            var active = state.ActiveCombatant;

            if (active == null)
            {
                _logger.LogWarning("[Match] No active combatant, ending match");
                break;
            }

            // Check if current combatant is dead
            if (active.CurrentHp <= 0)
            {
                combatService.RemoveDefeatedCombatant(active);

                // Check victory condition
                if (combatService.CheckVictoryCondition())
                {
                    _logger.LogTrace("[Match] Victory! All enemies defeated.");
                    combatService.EndCombat();
                    break;
                }

                continue;
            }

            if (active.IsPlayer)
            {
                // Player turn
                var action = playerAgent.DecideAction(active, state);
                ExecuteSimulatedPlayerAction(combatService, active, action, stats, state);
            }
            else
            {
                // Enemy turn - use sync method to skip UX delay
                var enemyHpBefore = active.CurrentHp;
                var playerCombatant = state.TurnOrder.FirstOrDefault(c => c.IsPlayer);
                var playerHpBefore = playerCombatant?.CurrentHp ?? 0;

                combatService.ProcessEnemyTurnSync(active);

                // Record enemy attack statistics
                if (playerCombatant != null && playerCombatant.CurrentHp < playerHpBefore)
                {
                    var damage = playerHpBefore - playerCombatant.CurrentHp;
                    stats.RecordEnemyAttack(true, damage);
                }
                else if (playerCombatant != null)
                {
                    // Enemy attacked but missed (or used non-damaging action)
                    stats.RecordEnemyAttack(false, 0);
                }
            }

            rounds++;

            // Check for defeat (player death)
            var player = gameState.CombatState?.TurnOrder.FirstOrDefault(c => c.IsPlayer);
            if (player != null && player.CurrentHp <= 0)
            {
                _logger.LogTrace("[Match] Defeat! Player has fallen.");
                break;
            }

            // Check for enemy defeat
            var enemies = gameState.CombatState?.TurnOrder.Where(c => !c.IsPlayer && c.CurrentHp > 0).ToList();
            if (enemies == null || enemies.Count == 0)
            {
                _logger.LogTrace("[Match] Victory! All enemies defeated.");
                combatService.EndCombat();
                break;
            }
        }

        // Determine winner
        var finalPlayer = gameState.CombatState?.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        var finalEnemy = gameState.CombatState?.TurnOrder.FirstOrDefault(c => !c.IsPlayer);
        var playerWon = finalPlayer?.CurrentHp > 0 && (finalEnemy == null || finalEnemy.CurrentHp <= 0);

        // If combat state was cleared by EndCombat, check game phase
        if (gameState.CombatState == null)
        {
            playerWon = gameState.Phase == GamePhase.Exploration;
        }

        var staminaSpent = staminaStart - (finalPlayer?.CurrentStamina ?? 0);
        if (staminaSpent < 0) staminaSpent = 0; // Regeneration can exceed starting stamina

        _logger.LogTrace(
            "[Match] Result: {Winner}, Rounds: {Rounds}, HP Remaining: {HP}/{MaxHP}",
            playerWon ? "Player Win" : "Enemy Win",
            rounds,
            finalPlayer?.CurrentHp ?? 0,
            finalPlayer?.MaxHp ?? character.MaxHP);

        return new CombatMatchResult(
            PlayerWon: playerWon,
            RoundsElapsed: rounds,
            PlayerHPRemaining: finalPlayer?.CurrentHp ?? 0,
            EnemyHPRemaining: finalEnemy?.CurrentHp ?? 0,
            StaminaSpent: staminaSpent);
    }

    /// <summary>
    /// Executes a player action decided by the simulation agent.
    /// </summary>
    /// <param name="combatService">The combat service.</param>
    /// <param name="player">The player combatant.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="stats">The statistics accumulator.</param>
    /// <param name="combatState">The current combat state.</param>
    private void ExecuteSimulatedPlayerAction(
        ICombatService combatService,
        Combatant player,
        CombatAction action,
        CombatStatistics stats,
        CombatState combatState)
    {
        if (action.Type != ActionType.Attack || action.TargetId == null)
        {
            // Pass turn for non-attack actions
            combatService.NextTurn();
            return;
        }

        // Get attack type
        var attackType = action.AttackType ?? AttackType.Standard;

        // Find the target by ID in the combat state to get their name
        var target = combatState.TurnOrder.FirstOrDefault(c => c.Id == action.TargetId);
        if (target == null)
        {
            // Fallback: find first alive enemy
            target = combatState.TurnOrder.FirstOrDefault(c => !c.IsPlayer && c.CurrentHp > 0);
        }

        if (target == null)
        {
            combatService.NextTurn();
            return;
        }

        // Execute the attack using the actual target name
        var result = combatService.ExecutePlayerAttack(target.Name, attackType);

        // Record player attack statistics based on result message
        // The result message contains damage info or "miss" indication
        if (result.Contains("miss", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("dodges", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("blocks", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            stats.RecordPlayerAttack(false, 0);
        }
        else
        {
            // Try to extract damage from result message
            // Messages typically contain "deals X damage" or "X damage"
            var damageMatch = System.Text.RegularExpressions.Regex.Match(result, @"(\d+)\s*damage");
            if (damageMatch.Success && int.TryParse(damageMatch.Groups[1].Value, out var damage))
            {
                stats.RecordPlayerAttack(true, damage);
            }
            else
            {
                // Assume hit with unknown damage
                stats.RecordPlayerAttack(true, 0);
            }
        }

        // Advance to next turn (ExecutePlayerAttack doesn't do this automatically)
        combatService.NextTurn();
    }

    /// <summary>
    /// Calculates variance flags by comparing actual metrics to expected bounds.
    /// </summary>
    private List<CombatVarianceFlag> CalculateVarianceFlags(
        CombatStatistics stats,
        CombatAuditConfiguration config)
    {
        var flags = new List<CombatVarianceFlag>();

        // Win Rate check
        flags.Add(new CombatVarianceFlag(
            "Win Rate",
            stats.WinRate,
            ExpectedWinRateMin,
            ExpectedWinRateMax,
            ClassifyVariance(stats.WinRate, ExpectedWinRateMin, ExpectedWinRateMax)));

        // Average Rounds check
        flags.Add(new CombatVarianceFlag(
            "Avg Rounds",
            stats.AvgRoundsPerEncounter,
            ExpectedAvgRoundsMin,
            ExpectedAvgRoundsMax,
            ClassifyVariance(stats.AvgRoundsPerEncounter, ExpectedAvgRoundsMin, ExpectedAvgRoundsMax)));

        // Player Hit Rate check
        flags.Add(new CombatVarianceFlag(
            "Player Hit Rate",
            stats.PlayerHitRate,
            ExpectedHitRateMin,
            ExpectedHitRateMax,
            ClassifyVariance(stats.PlayerHitRate, ExpectedHitRateMin, ExpectedHitRateMax)));

        // Enemy Hit Rate check (same bounds as player for symmetry)
        flags.Add(new CombatVarianceFlag(
            "Enemy Hit Rate",
            stats.EnemyHitRate,
            ExpectedHitRateMin,
            ExpectedHitRateMax,
            ClassifyVariance(stats.EnemyHitRate, ExpectedHitRateMin, ExpectedHitRateMax)));

        return flags;
    }

    /// <summary>
    /// Classifies variance magnitude into severity levels.
    /// </summary>
    private static VarianceSeverity ClassifyVariance(double actual, double expectedMin, double expectedMax)
    {
        if (actual >= expectedMin && actual <= expectedMax)
            return VarianceSeverity.Ok;

        var deviation = actual < expectedMin
            ? expectedMin - actual
            : actual - expectedMax;

        if (deviation > CriticalThreshold)
            return VarianceSeverity.Critical;
        if (deviation > WarningThreshold)
            return VarianceSeverity.Warning;

        return VarianceSeverity.Ok;
    }

    /// <summary>
    /// Generates a human-readable markdown report from the audit results.
    /// </summary>
    private string GenerateMarkdownReport(
        CombatStatistics stats,
        CombatAuditConfiguration config,
        List<CombatVarianceFlag> flags)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("# Combat Audit Report");
        sb.AppendLine();
        sb.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss} | **Matches:** {stats.TotalEncounters:N0}");
        sb.AppendLine($"**Player:** {config.PlayerArchetype} (Level {config.PlayerLevel}) | **Enemy:** {config.EnemyTemplateId}");
        sb.AppendLine();

        // Summary Table
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| **Win Rate** | {stats.WinRate:F1}% |");
        sb.AppendLine($"| **Average Rounds** | {stats.AvgRoundsPerEncounter:F1} |");
        sb.AppendLine($"| **Avg HP Remaining (on Win)** | {stats.AvgHPRemainingOnWin:F0} |");
        sb.AppendLine($"| **Player Hit Rate** | {stats.PlayerHitRate:F1}% |");
        sb.AppendLine($"| **Avg Damage per Hit** | {stats.AvgPlayerDamagePerHit:F1} |");
        sb.AppendLine($"| **Avg Stamina per Encounter** | {stats.AvgStaminaPerEncounter:F0} |");
        sb.AppendLine();

        // Detailed Statistics
        sb.AppendLine("## Detailed Statistics");
        sb.AppendLine();

        sb.AppendLine("### Player Performance");
        sb.AppendLine();
        sb.AppendLine($"- **Total Attacks:** {stats.TotalPlayerHits + stats.TotalPlayerMisses:N0}");
        sb.AppendLine($"- **Hits:** {stats.TotalPlayerHits:N0} ({stats.PlayerHitRate:F1}%)");
        sb.AppendLine($"- **Misses:** {stats.TotalPlayerMisses:N0} ({100 - stats.PlayerHitRate:F1}%)");
        sb.AppendLine($"- **Total Damage Dealt:** {stats.TotalPlayerDamageDealt:N0}");
        sb.AppendLine($"- **Total Stamina Spent:** {stats.TotalPlayerStaminaSpent:N0}");
        sb.AppendLine();

        sb.AppendLine("### Enemy Performance");
        sb.AppendLine();
        sb.AppendLine($"- **Total Attacks:** {stats.TotalEnemyHits + stats.TotalEnemyMisses:N0}");
        sb.AppendLine($"- **Hits:** {stats.TotalEnemyHits:N0} ({stats.EnemyHitRate:F1}%)");
        sb.AppendLine($"- **Misses:** {stats.TotalEnemyMisses:N0} ({100 - stats.EnemyHitRate:F1}%)");
        sb.AppendLine($"- **Total Damage Dealt:** {stats.TotalEnemyDamageDealt:N0}");
        sb.AppendLine($"- **Total Damage to Player:** {stats.TotalPlayerDamageReceived:N0}");
        sb.AppendLine();

        // Balance Assessment
        sb.AppendLine("## Balance Assessment");
        sb.AppendLine();
        sb.AppendLine("| Metric | Actual | Expected Range | Status |");
        sb.AppendLine("|--------|--------|----------------|--------|");

        foreach (var flag in flags)
        {
            var status = GetStatusIndicator(flag.Severity);
            sb.AppendLine($"| {flag.Metric} | {flag.ActualValue:F1} | {flag.ExpectedMin:F0}-{flag.ExpectedMax:F0} | {status} |");
        }
        sb.AppendLine();

        // Anomalies Section
        var anomalies = flags.Where(f => !f.IsWithinBounds).ToList();
        sb.AppendLine("## Anomalies");
        sb.AppendLine();

        if (anomalies.Count == 0)
        {
            sb.AppendLine("*None detected. All metrics are within acceptable bounds.*");
        }
        else
        {
            var criticals = anomalies.Where(f => f.Severity == VarianceSeverity.Critical).ToList();
            var warnings = anomalies.Where(f => f.Severity == VarianceSeverity.Warning).ToList();

            if (criticals.Count > 0)
            {
                sb.AppendLine("### Critical Deviations");
                sb.AppendLine();
                foreach (var flag in criticals)
                {
                    var direction = flag.ActualValue < flag.ExpectedMin ? "below" : "above";
                    sb.AppendLine($"- **{flag.Metric}**: {flag.ActualValue:F1} is {direction} expected range ({flag.ExpectedMin:F0}-{flag.ExpectedMax:F0})");
                }
                sb.AppendLine();
            }

            if (warnings.Count > 0)
            {
                sb.AppendLine("### Warnings");
                sb.AppendLine();
                foreach (var flag in warnings)
                {
                    var direction = flag.ActualValue < flag.ExpectedMin ? "below" : "above";
                    sb.AppendLine($"- **{flag.Metric}**: {flag.ActualValue:F1} is {direction} expected range ({flag.ExpectedMin:F0}-{flag.ExpectedMax:F0})");
                }
                sb.AppendLine();
            }
        }

        // Footer
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by CombatAuditService v0.3.13b*");

        return sb.ToString();
    }

    /// <summary>
    /// Gets a status indicator string for the given severity level.
    /// </summary>
    private static string GetStatusIndicator(VarianceSeverity severity) => severity switch
    {
        VarianceSeverity.Ok => "OK",
        VarianceSeverity.Warning => "WARN",
        VarianceSeverity.Critical => "CRIT",
        _ => "?"
    };
}
