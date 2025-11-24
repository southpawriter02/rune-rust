namespace RuneAndRust.Core.NewGamePlus;

/// <summary>
/// v0.40.1: New Game+ scaling parameters for a specific tier
/// Defines how difficulty scales at each NG+ tier
/// </summary>
public class NGPlusScaling
{
    /// <summary>NG+ tier (1-5)</summary>
    public int Tier { get; set; }

    /// <summary>Enemy HP and damage multiplier (1.5-3.5)</summary>
    public float DifficultyMultiplier { get; set; }

    /// <summary>Enemy level increase (+2 per tier)</summary>
    public int EnemyLevelIncrease { get; set; }

    /// <summary>Boss phase HP threshold reduction (0.10-0.50)</summary>
    public float BossPhaseThresholdReduction { get; set; }

    /// <summary>Corruption accumulation rate multiplier (1.25-2.25)</summary>
    public float CorruptionRateMultiplier { get; set; }

    /// <summary>Legend reward bonus multiplier (1.15-1.75)</summary>
    public float LegendRewardMultiplier { get; set; }

    /// <summary>Tier description</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Calculate difficulty multiplier for a given tier
    /// Formula: 1.0 + (tier × 0.5)
    /// </summary>
    public static float GetDifficultyMultiplier(NewGamePlusTier tier)
    {
        return 1.0f + ((int)tier * 0.5f);
    }

    /// <summary>
    /// Calculate enemy level increase for a given tier
    /// Formula: tier × 2
    /// </summary>
    public static int GetLevelIncrease(NewGamePlusTier tier)
    {
        return (int)tier * 2;
    }

    /// <summary>
    /// Calculate boss phase threshold reduction for a given tier
    /// Formula: tier × 0.10
    /// </summary>
    public static float GetBossPhaseThresholdReduction(NewGamePlusTier tier)
    {
        return (int)tier * 0.10f;
    }

    /// <summary>
    /// Calculate corruption rate multiplier for a given tier
    /// Formula: 1.0 + (tier × 0.25)
    /// </summary>
    public static float GetCorruptionRateMultiplier(NewGamePlusTier tier)
    {
        return 1.0f + ((int)tier * 0.25f);
    }

    /// <summary>
    /// Calculate legend reward multiplier for a given tier
    /// Formula: 1.0 + (tier × 0.15)
    /// </summary>
    public static float GetLegendRewardMultiplier(NewGamePlusTier tier)
    {
        return 1.0f + ((int)tier * 0.15f);
    }
}
