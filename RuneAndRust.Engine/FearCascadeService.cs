using RuneAndRust.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Fear Cascade operation
/// </summary>
public class CascadeResult
{
    public bool Success { get; set; }
    public List<int> NewlyFearedEnemies { get; set; } = new();
    public List<int> DamagedEnemies { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public int TotalDamage { get; set; }
}

/// <summary>
/// WILL Resolve check result
/// </summary>
public class ResolveCheckResult
{
    public bool Success { get; set; }
    public int Successes { get; set; }
    public int DC { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// v0.28.2: Service for Fear Cascade ability (Echo-Caller Tier 3)
/// Handles chain reaction of panic spreading through enemy formations
/// Already-Feared enemies auto-fail and take Psychic damage
/// New enemies make WILL Resolve checks or become Feared
/// </summary>
public class FearCascadeService
{
    private static readonly ILogger _log = Log.ForContext<FearCascadeService>();
    private readonly DiceService _diceService;
    private readonly string _connectionString;

    public FearCascadeService(string connectionString)
    {
        _connectionString = connectionString;
        _diceService = new DiceService();
        _log.Debug("FearCascadeService initialized");
    }

    /// <summary>
    /// Trigger Fear Cascade - AoE fear spread with damage to already-Feared enemies
    /// </summary>
    /// <param name="casterId">Character casting the ability</param>
    /// <param name="targetId">Center target for the cascade</param>
    /// <param name="radius">Radius in tiles (3 by default)</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Cascade result with affected enemies</returns>
    public CascadeResult TriggerFearCascade(
        int casterId,
        int targetId,
        int radius,
        int rank)
    {
        using var operation = _log.BeginOperation("TriggerFearCascade: CasterID={CasterId}, TargetID={TargetId}, Radius={Radius}, Rank={Rank}",
            casterId, targetId, radius, rank);

        try
        {
            // Get enemies within radius
            var enemiesInRadius = GetEnemiesWithinRadius(targetId, radius);

            // Calculate parameters based on rank
            var (dc, fearDuration, damageDice) = rank switch
            {
                1 => (14, 2, 2),  // DC 14, 2 turns Fear, 2d6 damage
                2 => (15, 3, 3),  // DC 15, 3 turns Fear, 3d6 damage
                3 => (16, 3, 4),  // DC 16, 3 turns Fear, 4d6 damage
                _ => (14, 2, 2)
            };

            var newlyFeared = new List<int>();
            var damaged = new List<int>();
            int totalDamage = 0;

            // Process each enemy in radius
            foreach (var enemy in enemiesInRadius)
            {
                bool alreadyFeared = CheckIfEnemyFeared(enemy.EnemyID);

                if (alreadyFeared)
                {
                    // Auto-fail, take Psychic damage
                    int damage = RollDamage(damageDice, 6);
                    ApplyDamage(enemy.EnemyID, damage, "Psychic");
                    damaged.Add(enemy.EnemyID);
                    totalDamage += damage;

                    _log.Information("Fear Cascade auto-damage: EnemyID={EnemyId} ({Name}) already Feared, took {Damage} Psychic damage",
                        enemy.EnemyID, enemy.Name, damage);
                }
                else
                {
                    // Make WILL Resolve check
                    var checkResult = PerformWILLResolveCheck(enemy, dc);

                    if (!checkResult.Success)
                    {
                        // Failed check - become Feared
                        ApplyStatusEffect(enemy.EnemyID, "Feared", fearDuration);
                        newlyFeared.Add(enemy.EnemyID);

                        // Trigger Terror Feedback for caster (restore Aether)
                        TriggerTerrorFeedback(casterId);

                        _log.Information("Fear Cascade: EnemyID={EnemyId} ({Name}) failed WILL check (DC {DC}), became Feared for {Duration} turns",
                            enemy.EnemyID, enemy.Name, dc, fearDuration);
                    }
                    else
                    {
                        _log.Information("Fear Cascade: EnemyID={EnemyId} ({Name}) passed WILL check (DC {DC}), resisted Fear",
                            enemy.EnemyID, enemy.Name, dc);
                    }
                }
            }

            // Rank 3: Echo Chain auto-spreads to one enemy outside radius
            if (rank >= 3)
            {
                var outsideEnemy = FindEnemyOutsideRadius(targetId, radius);
                if (outsideEnemy != null)
                {
                    ApplyStatusEffect(outsideEnemy.EnemyID, "Feared", fearDuration);
                    newlyFeared.Add(outsideEnemy.EnemyID);

                    // Trigger Terror Feedback for caster
                    TriggerTerrorFeedback(casterId);

                    // Track Echo Chain
                    TrackEchoChain(casterId);

                    _log.Information("Fear Cascade Echo Chain: Spread to EnemyID={EnemyId} ({Name}) outside radius",
                        outsideEnemy.EnemyID, outsideEnemy.Name);
                }
            }

            string message = BuildCascadeMessage(newlyFeared.Count, damaged.Count, totalDamage, rank);

            _log.Information("Fear Cascade completed: CasterID={CasterId}, NewFeared={Feared}, Damaged={Damaged}, TotalDamage={Damage}",
                casterId, newlyFeared.Count, damaged.Count, totalDamage);

            return new CascadeResult
            {
                Success = true,
                NewlyFearedEnemies = newlyFeared,
                DamagedEnemies = damaged,
                TotalDamage = totalDamage,
                Message = message
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to trigger Fear Cascade: CasterID={CasterId}", casterId);
            throw;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Perform WILL Resolve check for an enemy
    /// </summary>
    private ResolveCheckResult PerformWILLResolveCheck(Enemy enemy, int dc)
    {
        int willScore = enemy.WILL ?? 2; // Default to 2 if not set
        var checkResult = _diceService.SkillCheck(willScore, dc);

        return new ResolveCheckResult
        {
            Success = checkResult.Success,
            Successes = checkResult.Successes,
            DC = dc,
            Message = checkResult.Success
                ? $"{enemy.Name} resisted Fear (WILL check DC {dc})"
                : $"{enemy.Name} failed WILL check (DC {dc}), became Feared"
        };
    }

    /// <summary>
    /// Roll damage dice
    /// </summary>
    private int RollDamage(int diceCount, int diceSize)
    {
        int total = 0;
        for (int i = 0; i < diceCount; i++)
        {
            total += _diceService.Roll(1, diceSize);
        }
        return total;
    }

    /// <summary>
    /// Build Fear Cascade result message
    /// </summary>
    private string BuildCascadeMessage(int newlyFeared, int damaged, int totalDamage, int rank)
    {
        var parts = new List<string>();

        if (newlyFeared > 0)
        {
            parts.Add($"{newlyFeared} enem{(newlyFeared == 1 ? "y" : "ies")} became Feared");
        }

        if (damaged > 0)
        {
            parts.Add($"{damaged} already-Feared enem{(damaged == 1 ? "y" : "ies")} took {totalDamage} Psychic damage");
        }

        if (rank >= 3 && newlyFeared > 0)
        {
            parts.Add("[Echo Chain] Spread beyond initial radius");
        }

        string baseMessage = "Fear Cascade: ";
        if (parts.Count > 0)
        {
            baseMessage += string.Join(", ", parts) + "!";
        }
        else
        {
            baseMessage += "No enemies affected.";
        }

        return baseMessage;
    }

    /// <summary>
    /// Track Echo Chain for statistics
    /// </summary>
    private void TrackEchoChain(int casterId)
    {
        // Would call EchoCallerService.IncrementEchoChainCount()
        _log.Debug("Echo Chain tracked for caster {CasterId}", casterId);
    }

    /// <summary>
    /// Trigger Terror Feedback passive (restore Aether when applying Fear)
    /// </summary>
    private void TriggerTerrorFeedback(int casterId)
    {
        // Placeholder - would call EchoCallerService or ResourceService to restore Aether
        // Based on Terror Feedback rank: Rank 1 = 10, Rank 2 = 15, Rank 3 = 20 + Empowered
        _log.Debug("Terror Feedback triggered for caster {CasterId}", casterId);
    }

    // Placeholder methods for integration with actual combat/status effect systems
    private List<Enemy> GetEnemiesWithinRadius(int targetId, int radius)
    {
        // Placeholder - would query actual battlefield grid service
        return new List<Enemy>();
    }

    private Enemy? FindEnemyOutsideRadius(int targetId, int radius)
    {
        // Placeholder - would find first enemy outside the radius
        return null;
    }

    private bool CheckIfEnemyFeared(int enemyId)
    {
        // Placeholder - would check actual status effect service
        return false;
    }

    private void ApplyDamage(int enemyId, int damage, string damageType)
    {
        // Placeholder - would call actual combat service
    }

    private void ApplyStatusEffect(int enemyId, string effect, int duration)
    {
        // Placeholder - would call actual status effect service
    }

    #endregion
}
