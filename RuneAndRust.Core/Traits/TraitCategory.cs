namespace RuneAndRust.Core.Traits;

/// <summary>
/// Categories for organizing creature traits by theme.
/// Each category corresponds to a numeric range in the CreatureTrait enum.
/// </summary>
public enum TraitCategory
{
    /// <summary>Temporal traits (100-199): Glitch-induced time anomalies, causality distortions.</summary>
    Temporal = 1,

    /// <summary>Corruption traits (200-299): Runic Blight infection, data corruption, reality degradation.</summary>
    Corruption = 2,

    /// <summary>Mechanical traits (300-399): Pre-Glitch automation, Iron Heart power systems.</summary>
    Mechanical = 3,

    /// <summary>Psychic traits (400-499): Mental attacks, Forlorn corruption, trauma infliction.</summary>
    Psychic = 4,

    /// <summary>Mobility traits (500-599): Movement advantages, positioning control.</summary>
    Mobility = 5,

    /// <summary>Defensive traits (600-699): Damage mitigation, survival mechanics.</summary>
    Defensive = 6,

    /// <summary>Offensive traits (700-799): Enhanced damage, special attack modifiers.</summary>
    Offensive = 7,

    /// <summary>Unique/Exotic traits (800-899): One-of-a-kind mechanics.</summary>
    Unique = 8,

    /// <summary>Resistance traits (900-999): Damage type resistances, elemental affinities.</summary>
    Resistance = 9,

    /// <summary>Strategy/AI Behavior traits (1000-1099): Tactical decision-making patterns.</summary>
    Strategy = 10,

    /// <summary>Sensory traits (1100-1199): Perception abilities, detection methods.</summary>
    Sensory = 11,

    /// <summary>Combat Condition traits (1200-1299): Environmental adaptations, situational modifiers.</summary>
    CombatCondition = 12
}

/// <summary>
/// Extension methods for trait category operations.
/// </summary>
public static class TraitCategoryExtensions
{
    /// <summary>
    /// Gets the category for a given creature trait based on its numeric value.
    /// </summary>
    public static TraitCategory GetCategory(this CreatureTrait trait)
    {
        var value = (int)trait;
        return value switch
        {
            >= 100 and < 200 => TraitCategory.Temporal,
            >= 200 and < 300 => TraitCategory.Corruption,
            >= 300 and < 400 => TraitCategory.Mechanical,
            >= 400 and < 500 => TraitCategory.Psychic,
            >= 500 and < 600 => TraitCategory.Mobility,
            >= 600 and < 700 => TraitCategory.Defensive,
            >= 700 and < 800 => TraitCategory.Offensive,
            >= 800 and < 900 => TraitCategory.Unique,
            >= 900 and < 1000 => TraitCategory.Resistance,
            >= 1000 and < 1100 => TraitCategory.Strategy,
            >= 1100 and < 1200 => TraitCategory.Sensory,
            >= 1200 and < 1300 => TraitCategory.CombatCondition,
            _ => throw new ArgumentOutOfRangeException(nameof(trait), $"Unknown trait category for value {value}")
        };
    }

    /// <summary>
    /// Gets the minimum trait ID for a category.
    /// </summary>
    public static int GetMinValue(this TraitCategory category)
    {
        return category switch
        {
            TraitCategory.Temporal => 100,
            TraitCategory.Corruption => 200,
            TraitCategory.Mechanical => 300,
            TraitCategory.Psychic => 400,
            TraitCategory.Mobility => 500,
            TraitCategory.Defensive => 600,
            TraitCategory.Offensive => 700,
            TraitCategory.Unique => 800,
            TraitCategory.Resistance => 900,
            TraitCategory.Strategy => 1000,
            TraitCategory.Sensory => 1100,
            TraitCategory.CombatCondition => 1200,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }

    /// <summary>
    /// Gets the maximum trait ID for a category (exclusive).
    /// </summary>
    public static int GetMaxValue(this TraitCategory category)
    {
        return category switch
        {
            TraitCategory.Temporal => 200,
            TraitCategory.Corruption => 300,
            TraitCategory.Mechanical => 400,
            TraitCategory.Psychic => 500,
            TraitCategory.Mobility => 600,
            TraitCategory.Defensive => 700,
            TraitCategory.Offensive => 800,
            TraitCategory.Unique => 900,
            TraitCategory.Resistance => 1000,
            TraitCategory.Strategy => 1100,
            TraitCategory.Sensory => 1200,
            TraitCategory.CombatCondition => 1300,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }
}
