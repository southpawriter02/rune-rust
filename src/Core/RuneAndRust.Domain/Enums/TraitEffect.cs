namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the mechanical effect types for monster traits.
/// </summary>
/// <remarks>
/// Each effect type determines how the trait modifies monster behavior or stats.
/// The EffectValue on MonsterTrait provides the magnitude of the effect.
/// </remarks>
public enum TraitEffect
{
    /// <summary>
    /// No mechanical effect (flavor only).
    /// </summary>
    None = 0,

    /// <summary>
    /// Heals a fixed amount of HP each turn.
    /// EffectValue = HP healed per turn.
    /// </summary>
    Regeneration = 1,

    /// <summary>
    /// Grants a defense/evasion bonus against melee attacks.
    /// EffectValue = Defense bonus.
    /// </summary>
    Flying = 2,

    /// <summary>
    /// Attacks may apply poison status effect.
    /// EffectValue = Poison damage per turn.
    /// </summary>
    Venomous = 3,

    /// <summary>
    /// Reduces all incoming damage by a flat amount.
    /// EffectValue = Damage reduction.
    /// </summary>
    Armored = 4,

    /// <summary>
    /// Increases damage dealt when below HP threshold.
    /// EffectValue = Damage bonus percentage.
    /// TriggerThreshold on MonsterTrait = HP percentage to activate.
    /// </summary>
    Berserker = 5,

    /// <summary>
    /// Can summon additional monsters (future implementation).
    /// EffectValue = Number of summons.
    /// </summary>
    Summoner = 6,

    /// <summary>
    /// Immune to a specific damage type (future implementation).
    /// EffectValue = Unused (damage type specified elsewhere).
    /// </summary>
    ElementalImmunity = 7,

    /// <summary>
    /// Attacks deal additional elemental damage (future implementation).
    /// EffectValue = Bonus damage.
    /// </summary>
    ElementalAttack = 8
}
