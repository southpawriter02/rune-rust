namespace RuneAndRust.Core;

/// <summary>
/// v0.21.1: Advanced Stance System
/// Combat postures that modify accuracy, mitigation, and damage output.
/// Only one stance can be active at a time.
/// Stances can be switched once per turn (default limit).
/// </summary>
public enum StanceType
{
    /// <summary>
    /// Default stance with no bonuses or penalties.
    /// Balanced approach with no tactical modifiers.
    /// </summary>
    Balanced,

    /// <summary>
    /// v0.21.1: Defensive Stance - Prioritizes survival over offense.
    /// -10% Accuracy, +15% Mitigation, -10% Damage
    /// </summary>
    Defensive,

    /// <summary>
    /// v0.21.1: Offensive Stance - Aggressive combat posture.
    /// +15% Accuracy, -10% Mitigation, +5% Damage
    /// </summary>
    Offensive,

    /// <summary>
    /// Skirmisher: Evasive Stance - Specialized stance for high mobility builds.
    /// +3 Defense, -50% damage dealt (legacy v0.7 stance, kept for backward compatibility)
    /// </summary>
    Evasive
}

/// <summary>
/// v0.21.1: Represents an active stance and its combat effects.
/// Stances modify accuracy, mitigation, and damage output.
/// </summary>
public class Stance
{
    public StanceType Type { get; set; } = StanceType.Balanced;

    /// <summary>
    /// v0.21.1: Accuracy modifier (1.0 = normal, 1.15 = +15%, 0.90 = -10%)
    /// Applied to attack rolls and ability success calculations.
    /// </summary>
    public float AccuracyModifier { get; set; } = 1.0f;

    /// <summary>
    /// v0.21.1: Mitigation modifier (1.0 = normal, 1.15 = +15% damage reduction, 0.90 = -10% reduction)
    /// Applied when receiving damage (higher = better defense).
    /// </summary>
    public float MitigationModifier { get; set; } = 1.0f;

    /// <summary>
    /// v0.21.1: Damage dealt multiplier (1.0 = normal, 1.05 = +5%, 0.90 = -10%)
    /// Applied to all outgoing damage.
    /// </summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Flat damage reduction bonus (Soak) - legacy property for backward compatibility
    /// </summary>
    public int SoakBonus { get; set; } = 0;

    /// <summary>
    /// Defense score bonus (for Evasive Stance) - legacy property
    /// </summary>
    public int DefenseBonus { get; set; } = 0;

    /// <summary>
    /// Bonus dice to defensive Resolve Checks - legacy property
    /// </summary>
    public int DefensiveBonusDice { get; set; } = 0;

    /// <summary>
    /// v0.21.1: Factory method for Defensive Stance
    /// -10% Accuracy, +15% Mitigation, -10% Damage
    /// Prioritizes survival and damage reduction over offense.
    /// </summary>
    public static Stance CreateDefensiveStance()
    {
        return new Stance
        {
            Type = StanceType.Defensive,
            AccuracyModifier = 0.90f,      // -10% accuracy
            MitigationModifier = 1.15f,     // +15% damage reduction
            DamageMultiplier = 0.90f,       // -10% damage output
            SoakBonus = 0,
            DefenseBonus = 0,
            DefensiveBonusDice = 0
        };
    }

    /// <summary>
    /// v0.21.1: Factory method for Balanced Stance (default)
    /// No modifiers - neutral combat posture.
    /// </summary>
    public static Stance CreateBalancedStance()
    {
        return new Stance
        {
            Type = StanceType.Balanced,
            AccuracyModifier = 1.0f,        // No accuracy modifier
            MitigationModifier = 1.0f,      // No mitigation modifier
            DamageMultiplier = 1.0f,        // No damage modifier
            SoakBonus = 0,
            DefenseBonus = 0,
            DefensiveBonusDice = 0
        };
    }

    /// <summary>
    /// v0.21.1: Factory method for Offensive Stance
    /// +15% Accuracy, -10% Mitigation, +5% Damage
    /// Aggressive combat posture that trades defense for offense.
    /// </summary>
    public static Stance CreateOffensiveStance()
    {
        return new Stance
        {
            Type = StanceType.Offensive,
            AccuracyModifier = 1.15f,       // +15% accuracy
            MitigationModifier = 0.90f,     // -10% damage reduction (more vulnerable)
            DamageMultiplier = 1.05f,       // +5% damage output
            SoakBonus = 0,
            DefenseBonus = 0,
            DefensiveBonusDice = 0
        };
    }

    /// <summary>
    /// Legacy: Factory method for Evasive Stance (Skirmisher specialization)
    /// +3 Defense, -50% damage dealt.
    /// Kept for backward compatibility with v0.7 system.
    /// </summary>
    public static Stance CreateEvasiveStance()
    {
        return new Stance
        {
            Type = StanceType.Evasive,
            AccuracyModifier = 1.0f,
            MitigationModifier = 1.0f,
            DamageMultiplier = 0.5f,        // -50% damage (legacy)
            SoakBonus = 0,
            DefenseBonus = 3,               // +3 Defense (legacy)
            DefensiveBonusDice = 0
        };
    }
}
