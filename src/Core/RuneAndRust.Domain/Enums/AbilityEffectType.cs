namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of effects an ability can have when used.
/// </summary>
public enum AbilityEffectType
{
    /// <summary>
    /// Deals immediate damage to the target.
    /// </summary>
    Damage = 0,

    /// <summary>
    /// Restores health to the target.
    /// </summary>
    Heal = 1,

    /// <summary>
    /// Applies a positive stat modifier to the target.
    /// </summary>
    Buff = 2,

    /// <summary>
    /// Applies a negative stat modifier to the target.
    /// </summary>
    Debuff = 3,

    /// <summary>
    /// Deals damage over multiple turns.
    /// </summary>
    DamageOverTime = 4,

    /// <summary>
    /// Heals over multiple turns.
    /// </summary>
    HealOverTime = 5,

    /// <summary>
    /// Creates a damage-absorbing shield.
    /// </summary>
    Shield = 6,

    /// <summary>
    /// Grants resource to the caster.
    /// </summary>
    ResourceGain = 7,

    /// <summary>
    /// Resets cooldown of another ability.
    /// </summary>
    CooldownReset = 8,

    /// <summary>
    /// Forces target to attack caster (taunt).
    /// </summary>
    Taunt = 9,

    /// <summary>
    /// Prevents target from acting (stun).
    /// </summary>
    Stun = 10,

    /// <summary>
    /// Summons an entity (future feature).
    /// </summary>
    Summon = 11,

    /// <summary>
    /// Teleports the caster or target (future feature).
    /// </summary>
    Teleport = 12
}
