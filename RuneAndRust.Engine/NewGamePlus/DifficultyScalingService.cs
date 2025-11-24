using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using Serilog;

namespace RuneAndRust.Engine.NewGamePlus;

/// <summary>
/// v0.40.1: Difficulty Scaling Service
/// Applies New Game+ scaling to enemies, bosses, corruption, and rewards
/// </summary>
public class DifficultyScalingService
{
    private static readonly ILogger _log = Log.ForContext<DifficultyScalingService>();
    private readonly NewGamePlusService _ngPlusService;

    public DifficultyScalingService(NewGamePlusService ngPlusService)
    {
        _ngPlusService = ngPlusService;
    }

    // ═════════════════════════════════════════════════════════════
    // ENEMY SCALING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Apply NG+ scaling to an enemy
    /// </summary>
    public Enemy ApplyNGPlusScaling(Enemy baseEnemy, int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            // Normal mode - no scaling
            return baseEnemy;
        }

        var scaling = _ngPlusService.GetScalingForTier(ngPlusTier);
        if (scaling == null)
        {
            _log.Warning("No scaling found for tier {Tier}, using unscaled enemy", ngPlusTier);
            return baseEnemy;
        }

        // Create a scaled copy (don't modify original)
        var scaledEnemy = CloneEnemy(baseEnemy);

        // Apply HP scaling
        scaledEnemy.MaxHP = (int)(baseEnemy.MaxHP * scaling.DifficultyMultiplier);
        scaledEnemy.HP = scaledEnemy.MaxHP;

        // Apply damage scaling (if enemy has BaseDamage field)
        // Note: In the actual codebase, damage might be derived from level/attributes
        // This is a simplified example

        // Apply level increase
        scaledEnemy.Level += scaling.EnemyLevelIncrease;

        // Recalculate derived stats based on new level
        RecalculateEnemyStats(scaledEnemy, scaling.DifficultyMultiplier);

        _log.Debug(
            "Scaled enemy {EnemyId} for NG+{Tier}: HP {BaseHP} → {ScaledHP}, Level {BaseLevel} → {ScaledLevel}",
            baseEnemy.Id, ngPlusTier, baseEnemy.MaxHP, scaledEnemy.MaxHP, baseEnemy.Level, scaledEnemy.Level);

        return scaledEnemy;
    }

    /// <summary>
    /// Apply NG+ scaling to a list of enemies
    /// </summary>
    public List<Enemy> ApplyNGPlusScalingBulk(List<Enemy> enemies, int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return enemies;
        }

        var scaledEnemies = new List<Enemy>();
        foreach (var enemy in enemies)
        {
            scaledEnemies.Add(ApplyNGPlusScaling(enemy, ngPlusTier));
        }

        _log.Information("Applied NG+{Tier} scaling to {Count} enemies", ngPlusTier, enemies.Count);
        return scaledEnemies;
    }

    private Enemy CloneEnemy(Enemy enemy)
    {
        // Deep clone enemy (simplified - would need proper deep copy in production)
        return new Enemy
        {
            Id = enemy.Id,
            Name = enemy.Name,
            Type = enemy.Type,
            Faction = enemy.Faction,
            Level = enemy.Level,
            HP = enemy.HP,
            MaxHP = enemy.MaxHP,
            Stamina = enemy.Stamina,
            MaxStamina = enemy.MaxStamina,
            Attributes = new Attributes
            {
                Might = enemy.Attributes.Might,
                Finesse = enemy.Attributes.Finesse,
                Wits = enemy.Attributes.Wits,
                Will = enemy.Attributes.Will,
                Sturdiness = enemy.Attributes.Sturdiness
            },
            Armor = enemy.Armor,
            Soak = enemy.Soak,
            Speed = enemy.Speed,
            Evasion = enemy.Evasion,
            IsBoss = enemy.IsBoss,
            Abilities = new List<Ability>(enemy.Abilities),
            LootTableId = enemy.LootTableId
        };
    }

    private void RecalculateEnemyStats(Enemy enemy, float difficultyMultiplier)
    {
        // Recalculate stats based on increased level and difficulty
        // This is simplified - actual implementation would depend on enemy stat formulas

        // Scale attributes slightly (10% boost per tier)
        float attributeMultiplier = 1.0f + ((difficultyMultiplier - 1.0f) * 0.2f);

        enemy.Attributes.Might = (int)(enemy.Attributes.Might * attributeMultiplier);
        enemy.Attributes.Finesse = (int)(enemy.Attributes.Finesse * attributeMultiplier);
        enemy.Attributes.Wits = (int)(enemy.Attributes.Wits * attributeMultiplier);
        enemy.Attributes.Will = (int)(enemy.Attributes.Will * attributeMultiplier);
        enemy.Attributes.Sturdiness = (int)(enemy.Attributes.Sturdiness * attributeMultiplier);

        // Scale defensive stats
        enemy.Armor = (int)(enemy.Armor * 1.1f); // +10% armor per tier
        enemy.Soak = (int)(enemy.Soak * 1.1f); // +10% soak per tier
        enemy.Evasion = Math.Min(15, (int)(enemy.Evasion * 1.05f)); // +5% evasion, capped at 15
    }

    // ═════════════════════════════════════════════════════════════
    // CORRUPTION & TRAUMA SCALING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get corruption rate multiplier for a tier
    /// </summary>
    public float GetCorruptionRateMultiplier(int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return 1.0f;
        }

        var scaling = _ngPlusService.GetScalingForTier(ngPlusTier);
        return scaling?.CorruptionRateMultiplier ?? 1.0f;
    }

    /// <summary>
    /// Apply NG+ scaling to corruption gain
    /// </summary>
    public int ApplyCorruptionScaling(int baseCorruption, int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return baseCorruption;
        }

        var multiplier = GetCorruptionRateMultiplier(ngPlusTier);
        var scaledCorruption = (int)(baseCorruption * multiplier);

        _log.Debug(
            "Scaled corruption for NG+{Tier}: {Base} × {Mult} = {Scaled}",
            ngPlusTier, baseCorruption, multiplier, scaledCorruption);

        return scaledCorruption;
    }

    /// <summary>
    /// Apply NG+ scaling to Psychic Stress gain
    /// </summary>
    public int ApplyStressScaling(int baseStress, int ngPlusTier)
    {
        // Stress doesn't scale with NG+ tier (only corruption does)
        // But we keep this method for consistency and future changes
        return baseStress;
    }

    // ═════════════════════════════════════════════════════════════
    // REWARD SCALING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get Legend reward multiplier for a tier
    /// </summary>
    public float GetLegendRewardMultiplier(int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return 1.0f;
        }

        var scaling = _ngPlusService.GetScalingForTier(ngPlusTier);
        return scaling?.LegendRewardMultiplier ?? 1.0f;
    }

    /// <summary>
    /// Apply NG+ scaling to Legend reward
    /// </summary>
    public int ApplyLegendScaling(int baseLegend, int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return baseLegend;
        }

        var multiplier = GetLegendRewardMultiplier(ngPlusTier);
        var scaledLegend = (int)(baseLegend * multiplier);

        _log.Debug(
            "Scaled Legend reward for NG+{Tier}: {Base} × {Mult} = {Scaled}",
            ngPlusTier, baseLegend, multiplier, scaledLegend);

        return scaledLegend;
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS PHASE SCALING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get boss phase threshold reduction for a tier
    /// Used to make boss phases trigger earlier (more challenging)
    /// </summary>
    public float GetBossPhaseThresholdReduction(int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return 0.0f;
        }

        var scaling = _ngPlusService.GetScalingForTier(ngPlusTier);
        return scaling?.BossPhaseThresholdReduction ?? 0.0f;
    }

    /// <summary>
    /// Apply NG+ scaling to boss phase HP threshold
    /// </summary>
    public float ApplyBossPhaseThresholdScaling(float baseThreshold, int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return baseThreshold;
        }

        var reduction = GetBossPhaseThresholdReduction(ngPlusTier);
        var scaledThreshold = Math.Max(0.1f, baseThreshold - reduction); // Minimum 10% HP

        _log.Debug(
            "Scaled boss phase threshold for NG+{Tier}: {Base} - {Reduction} = {Scaled}",
            ngPlusTier, baseThreshold, reduction, scaledThreshold);

        return scaledThreshold;
    }

    // ═════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get summary of scaling effects for a tier
    /// </summary>
    public string GetScalingSummary(int ngPlusTier)
    {
        if (ngPlusTier == 0)
        {
            return "Normal difficulty (no scaling)";
        }

        var scaling = _ngPlusService.GetScalingForTier(ngPlusTier);
        if (scaling == null)
        {
            return "Unknown tier";
        }

        return $"NG+{ngPlusTier}: " +
               $"Enemies {scaling.DifficultyMultiplier:F1}x HP/Dmg, " +
               $"+{scaling.EnemyLevelIncrease} levels, " +
               $"Corruption {scaling.CorruptionRateMultiplier:F2}x, " +
               $"Legend {scaling.LegendRewardMultiplier:F2}x";
    }
}
