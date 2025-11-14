namespace RuneAndRust.Core;

/// <summary>
/// v0.21.1: Universal Stance System (v2.0 Canonical Migration)
/// Combat postures that declare metaphysical intent - how characters interface with Aethelgard's broken reality.
/// Only one stance can be active at a time.
/// Stances can be switched once per turn as a free action.
/// </summary>
public enum StanceType
{
    /// <summary>
    /// v2.0: Calculated Stance - Balanced interface with broken reality (default).
    /// No modifiers - baseline operational state.
    /// </summary>
    Calculated,

    /// <summary>
    /// v2.0: Aggressive Stance - Channel chaotic Aether for overwhelming offense.
    /// +4 damage, -3 Defense, -2 WILL checks (more vulnerable to psychic stress)
    /// </summary>
    Aggressive,

    /// <summary>
    /// v2.0: Defensive Stance - Reinforce personal coherence against external chaos.
    /// +2 Soak, +2 WILL checks, -25% damage output, -5 Stamina regen
    /// </summary>
    Defensive,

    /// <summary>
    /// Legacy: Evasive Stance - Specialized stance for Skirmisher builds (v0.7).
    /// +3 Defense, -50% damage dealt.
    /// Kept for backward compatibility.
    /// </summary>
    Evasive
}

/// <summary>
/// v0.21.1: Represents an active combat stance and its effects (v2.0 canonical values).
/// v2.0 Source: Migrated from v2.0 dice pool system to Rune & Rust flat bonus architecture.
/// </summary>
public class Stance
{
    public StanceType Type { get; set; } = StanceType.Calculated;

    // v2.0 Canonical Modifiers (Flat Bonuses)

    /// <summary>
    /// v2.0: Flat damage bonus added to all attacks.
    /// Aggressive: +4 (represents v2.0's +2 damage dice average of ~4)
    /// </summary>
    public int FlatDamageBonus { get; set; } = 0;

    /// <summary>
    /// v2.0: Defense Score modification.
    /// Aggressive: -3 (direct v2.0 value)
    /// </summary>
    public int DefenseModifier { get; set; } = 0;

    /// <summary>
    /// v2.0: Flat Soak bonus (damage reduction before applying to HP).
    /// Defensive: +2 (direct v2.0 value)
    /// </summary>
    public int SoakBonus { get; set; } = 0;

    /// <summary>
    /// v2.0: WILL check modifier for Trauma Economy (Resolve Checks).
    /// Aggressive: -2 (more vulnerable to psychic stress)
    /// Defensive: +2 (reinforced coherence)
    /// </summary>
    public int WillModifier { get; set; } = 0;

    /// <summary>
    /// v2.0: Damage output multiplier (percentage-based).
    /// Defensive: 0.75f (-25% damage output penalty)
    /// Legacy Evasive: 0.5f (-50% damage output)
    /// </summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// v2.0: Stamina regeneration penalty per turn.
    /// Defensive: -5 (reduced regen while in defensive posture)
    /// </summary>
    public int StaminaRegenPenalty { get; set; } = 0;

    // Legacy Properties (backward compatibility with v0.7 Evasive Stance)

    /// <summary>
    /// Legacy: Defense score bonus for Evasive Stance.
    /// Evasive: +3 Defense
    /// </summary>
    public int DefenseBonus { get; set; } = 0;

    /// <summary>
    /// v2.0: Factory method for Calculated Stance (default).
    /// Balanced interface with broken reality - no modifiers.
    /// </summary>
    public static Stance CreateCalculatedStance()
    {
        return new Stance
        {
            Type = StanceType.Calculated,
            FlatDamageBonus = 0,
            DefenseModifier = 0,
            SoakBonus = 0,
            WillModifier = 0,
            DamageMultiplier = 1.0f,
            StaminaRegenPenalty = 0,
            DefenseBonus = 0
        };
    }

    /// <summary>
    /// v2.0: Factory method for Aggressive Stance.
    /// Channel tainted Aether: +4 damage, -3 Defense, -2 WILL checks.
    /// High-risk, high-reward offense that exposes character to danger.
    /// </summary>
    public static Stance CreateAggressiveStance()
    {
        return new Stance
        {
            Type = StanceType.Aggressive,
            FlatDamageBonus = 4,            // v2.0: +2 dice average = +4 flat
            DefenseModifier = -3,           // v2.0: -3 Defense Score
            SoakBonus = 0,
            WillModifier = -2,              // v2.0: -2 to WILL checks (vulnerable to psychic stress)
            DamageMultiplier = 1.0f,
            StaminaRegenPenalty = 0,
            DefenseBonus = 0
        };
    }

    /// <summary>
    /// v2.0: Factory method for Defensive Stance.
    /// Reinforce personal coherence: +2 Soak, +2 WILL checks, -25% damage, -5 Stamina regen.
    /// Prioritizes survival and mental fortitude over offensive output.
    /// </summary>
    public static Stance CreateDefensiveStance()
    {
        return new Stance
        {
            Type = StanceType.Defensive,
            FlatDamageBonus = 0,
            DefenseModifier = 0,
            SoakBonus = 2,                  // v2.0: +2 Soak
            WillModifier = 2,               // v2.0: +2 to WILL/STURDINESS checks
            DamageMultiplier = 0.75f,       // v2.0: -25% damage output
            StaminaRegenPenalty = -5,       // v2.0: Reduced stamina regen
            DefenseBonus = 0
        };
    }

    /// <summary>
    /// Legacy: Factory method for Evasive Stance (v0.7 Skirmisher specialization).
    /// +3 Defense, -50% damage output.
    /// Kept for backward compatibility, not part of v2.0 core stances.
    /// </summary>
    public static Stance CreateEvasiveStance()
    {
        return new Stance
        {
            Type = StanceType.Evasive,
            FlatDamageBonus = 0,
            DefenseModifier = 0,
            SoakBonus = 0,
            WillModifier = 0,
            DamageMultiplier = 0.5f,        // -50% damage (legacy)
            StaminaRegenPenalty = 0,
            DefenseBonus = 3                // +3 Defense (legacy)
        };
    }

    /// <summary>
    /// Legacy alias: Create Balanced Stance (redirects to Calculated).
    /// For backward compatibility with existing code.
    /// </summary>
    public static Stance CreateBalancedStance() => CreateCalculatedStance();

    /// <summary>
    /// Legacy alias: Create Offensive Stance (redirects to Aggressive).
    /// For backward compatibility with existing code.
    /// </summary>
    public static Stance CreateOffensiveStance() => CreateAggressiveStance();
}
