using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Static mapping of biome types to valid condition types (v0.3.3c).
/// Used by EnvironmentPopulator for thematic consistency during dungeon generation.
/// Uses existing BiomeType enum values: Ruin, Industrial, Organic, Void.
/// </summary>
public static class BiomeEnvironmentMapping
{
    /// <summary>
    /// Gets valid condition types for a biome.
    /// </summary>
    /// <param name="biome">The biome type to get conditions for.</param>
    /// <returns>List of condition types valid for this biome.</returns>
    public static List<ConditionType> GetConditionTypes(BiomeType biome) => biome switch
    {
        BiomeType.Ruin => new List<ConditionType>
        {
            ConditionType.LowVisibility,
            ConditionType.DreadPresence
        },
        BiomeType.Industrial => new List<ConditionType>
        {
            ConditionType.ToxicAtmosphere,
            ConditionType.StaticField,
            ConditionType.ScorchingHeat
        },
        BiomeType.Organic => new List<ConditionType>
        {
            ConditionType.BlightedGround,
            ConditionType.PsychicResonance,
            ConditionType.ToxicAtmosphere
        },
        BiomeType.Void => new List<ConditionType>
        {
            ConditionType.PsychicResonance,
            ConditionType.DreadPresence,
            ConditionType.DeepCold
        },
        _ => new List<ConditionType> { ConditionType.LowVisibility }
    };

    /// <summary>
    /// Gets base hazard/condition spawn chance modifier for danger level.
    /// Higher danger levels increase the probability of environmental hazards.
    /// </summary>
    /// <param name="level">The danger level of the room.</param>
    /// <returns>A multiplier (0.0-1.0) to add to base spawn chance.</returns>
    public static float GetDangerMultiplier(DangerLevel level) => level switch
    {
        DangerLevel.Safe => 0.1f,      // 10% bonus
        DangerLevel.Unstable => 0.3f,  // 30% bonus
        DangerLevel.Hostile => 0.5f,   // 50% bonus
        DangerLevel.Lethal => 0.7f,    // 70% bonus
        _ => 0.2f
    };
}
