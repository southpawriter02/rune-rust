namespace RuneAndRust.Core;

/// <summary>
/// v0.7.1: Stance System
/// Warriors (and potentially other archetypes) can enter stances that modify their combat behavior.
/// Only one stance can be active at a time.
/// </summary>
public enum StanceType
{
    /// <summary>
    /// Default stance with no bonuses or penalties
    /// </summary>
    Balanced,

    /// <summary>
    /// Warrior: Defensive Stance - +3 Soak, -25% damage dealt, +1d to defensive Resolve Checks
    /// </summary>
    Defensive,

    /// <summary>
    /// Reserved for future specialization abilities (e.g., Aggressive Stance)
    /// </summary>
    Aggressive
}

/// <summary>
/// Represents an active stance and its effects
/// </summary>
public class Stance
{
    public StanceType Type { get; set; } = StanceType.Balanced;

    /// <summary>
    /// Flat damage reduction bonus (Soak)
    /// </summary>
    public int SoakBonus { get; set; } = 0;

    /// <summary>
    /// Damage dealt multiplier (1.0 = normal, 0.75 = -25%, 1.25 = +25%)
    /// </summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Bonus dice to defensive Resolve Checks
    /// </summary>
    public int DefensiveBonusDice { get; set; } = 0;

    /// <summary>
    /// Factory method for Defensive Stance
    /// </summary>
    public static Stance CreateDefensiveStance()
    {
        return new Stance
        {
            Type = StanceType.Defensive,
            SoakBonus = 3,
            DamageMultiplier = 0.75f, // -25% damage
            DefensiveBonusDice = 1
        };
    }

    /// <summary>
    /// Factory method for Balanced Stance (default)
    /// </summary>
    public static Stance CreateBalancedStance()
    {
        return new Stance
        {
            Type = StanceType.Balanced,
            SoakBonus = 0,
            DamageMultiplier = 1.0f,
            DefensiveBonusDice = 0
        };
    }

    /// <summary>
    /// Factory method for Aggressive Stance (reserved for future use)
    /// </summary>
    public static Stance CreateAggressiveStance()
    {
        return new Stance
        {
            Type = StanceType.Aggressive,
            SoakBonus = -2,
            DamageMultiplier = 1.25f, // +25% damage
            DefensiveBonusDice = -1
        };
    }
}
