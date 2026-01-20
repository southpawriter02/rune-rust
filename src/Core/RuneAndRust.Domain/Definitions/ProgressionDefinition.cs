using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines the progression system configuration for the game.
/// </summary>
/// <remarks>
/// Loaded from config/progression.json. Controls XP/Level terminology,
/// progression curves, stat bonuses per level, and max level cap.
/// </remarks>
public class ProgressionDefinition
{
    /// <summary>
    /// Gets the display name for experience points.
    /// </summary>
    /// <example>"XP", "Glory", "Essence", "Power"</example>
    public string ExperienceTerminology { get; init; } = "XP";

    /// <summary>
    /// Gets the display name for character levels.
    /// </summary>
    /// <example>"Level", "Rank", "Tier", "Grade"</example>
    public string LevelTerminology { get; init; } = "Level";

    /// <summary>
    /// Gets the maximum level a player can achieve.
    /// </summary>
    /// <remarks>
    /// Level-ups stop at this level. XP continues to accumulate.
    /// Set to 0 for no cap.
    /// </remarks>
    public int MaxLevel { get; init; } = 20;

    /// <summary>
    /// Gets the progression curve type.
    /// </summary>
    public ProgressionCurve CurveType { get; init; } = ProgressionCurve.Linear;

    /// <summary>
    /// Gets the base XP required for level 2.
    /// </summary>
    /// <remarks>
    /// This is the foundation for all progression curve calculations.
    /// </remarks>
    public int BaseXpRequirement { get; init; } = 100;

    /// <summary>
    /// Gets the multiplier for exponential progression curves.
    /// </summary>
    /// <remarks>
    /// Each level requires (previous level XP * multiplier) XP.
    /// Only used when CurveType is Exponential.
    /// </remarks>
    public float XpMultiplier { get; init; } = 1.5f;

    /// <summary>
    /// Gets the default stat bonuses applied per level.
    /// </summary>
    /// <remarks>
    /// These values are applied unless a class has specific GrowthRates
    /// or a LevelDefinition has custom StatBonuses.
    /// </remarks>
    public StatBonusConfig DefaultStatBonuses { get; init; } = new();

    /// <summary>
    /// Gets custom level definitions for specific levels.
    /// </summary>
    /// <remarks>
    /// Allows overriding XP requirements, stat bonuses, and adding
    /// custom rewards for specific levels. Keyed by level number.
    /// </remarks>
    public IReadOnlyDictionary<int, LevelDefinition> LevelOverrides { get; init; } =
        new Dictionary<int, LevelDefinition>();

    /// <summary>
    /// Gets whether to heal the player to full on level-up.
    /// </summary>
    public bool HealOnLevelUp { get; init; } = true;

    /// <summary>
    /// Creates a default progression definition with standard values.
    /// </summary>
    public static ProgressionDefinition Default => new();

    /// <summary>
    /// Calculates the cumulative XP required to reach a specific level.
    /// </summary>
    /// <param name="level">The target level.</param>
    /// <returns>The total XP needed to reach that level.</returns>
    public int GetExperienceForLevel(int level)
    {
        if (level <= 1) return 0;
        if (MaxLevel > 0 && level > MaxLevel) level = MaxLevel;

        // Check for custom override first
        if (LevelOverrides.TryGetValue(level, out var customLevel) && customLevel.XpRequired.HasValue)
        {
            return customLevel.XpRequired.Value;
        }

        return CurveType switch
        {
            ProgressionCurve.Linear => CalculateLinearXp(level),
            ProgressionCurve.Exponential => CalculateExponentialXp(level),
            ProgressionCurve.Custom => GetCustomXpForLevel(level),
            _ => CalculateLinearXp(level)
        };
    }

    /// <summary>
    /// Gets what level corresponds to a given XP amount.
    /// </summary>
    /// <param name="experience">The experience points.</param>
    /// <returns>The level the player should be at.</returns>
    public int GetLevelForExperience(int experience)
    {
        var maxLevelToCheck = MaxLevel > 0 ? MaxLevel : 100; // Safety limit

        for (var level = maxLevelToCheck; level >= 1; level--)
        {
            if (experience >= GetExperienceForLevel(level))
            {
                return level;
            }
        }

        return 1;
    }

    /// <summary>
    /// Gets the stat bonuses for leveling up, considering class growth rates.
    /// </summary>
    /// <param name="level">The level being gained.</param>
    /// <param name="classGrowthRates">Optional class-specific growth rates.</param>
    /// <returns>The stat modifiers to apply.</returns>
    public LevelStatModifiers GetStatBonusesForLevel(int level, LevelStatModifiers? classGrowthRates = null)
    {
        // Check for level-specific override
        if (LevelOverrides.TryGetValue(level, out var levelDef) && levelDef.StatBonuses != null)
        {
            return levelDef.StatBonuses.ToLevelStatModifiers();
        }

        // Use class growth rates if provided
        if (classGrowthRates.HasValue && classGrowthRates.Value.HasModifications)
        {
            return classGrowthRates.Value;
        }

        // Fall back to default stat bonuses
        return DefaultStatBonuses.ToLevelStatModifiers();
    }

    private int CalculateLinearXp(int level)
    {
        // Linear: level * baseXp
        // Level 2 = 200, Level 3 = 300, Level 4 = 400...
        // This matches the v0.0.8b formula: Level N = N * 100 XP
        return level * BaseXpRequirement;
    }

    private int CalculateExponentialXp(int level)
    {
        // Exponential: cumulative sum of baseXp * (multiplier ^ (level - 2))
        // Level 2 = 100, Level 3 = 250, Level 4 = 475...
        if (level == 2) return BaseXpRequirement;

        var cumulative = 0;
        var xpForLevel = (float)BaseXpRequirement;

        for (var l = 2; l <= level; l++)
        {
            cumulative += (int)xpForLevel;
            xpForLevel *= XpMultiplier;
        }

        return cumulative;
    }

    private int GetCustomXpForLevel(int level)
    {
        // For custom curves, must be defined in LevelOverrides
        // Fall back to linear if not defined
        if (LevelOverrides.TryGetValue(level, out var levelDef) && levelDef.XpRequired.HasValue)
        {
            return levelDef.XpRequired.Value;
        }

        return CalculateLinearXp(level);
    }
}
