namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines when a special effect activates during combat or gameplay.
/// </summary>
/// <remarks>
/// <para>
/// Effect trigger types determine the timing of special effect activation.
/// Each <see cref="SpecialEffectType"/> has an expected trigger type that
/// is validated when creating a <see cref="ValueObjects.SpecialEffect"/>.
/// </para>
/// <para>
/// Trigger types are ordered by combat timing:
/// <list type="number">
///   <item><description><see cref="Passive"/> - Always active while equipped</description></item>
///   <item><description><see cref="OnAttack"/> - Before hit calculation</description></item>
///   <item><description><see cref="OnHit"/> - After successful hit</description></item>
///   <item><description><see cref="OnDamageDealt"/> - After damage applied to target</description></item>
///   <item><description><see cref="OnDamageTaken"/> - When wearer receives damage</description></item>
///   <item><description><see cref="OnKill"/> - When wearer defeats an enemy</description></item>
/// </list>
/// </para>
/// <para>
/// Type values are explicitly assigned (0-5) to ensure stable serialization
/// and database storage. New types should be added at the end if needed.
/// </para>
/// </remarks>
/// <seealso cref="SpecialEffectType"/>
/// <seealso cref="ValueObjects.SpecialEffect"/>
public enum EffectTriggerType
{
    /// <summary>
    /// Effect is always active while item is equipped.
    /// </summary>
    /// <remarks>
    /// Passive effects provide constant benefits without requiring specific
    /// combat actions. Examples include <see cref="SpecialEffectType.Detection"/>,
    /// <see cref="SpecialEffectType.CriticalBonus"/>, <see cref="SpecialEffectType.DamageReduction"/>,
    /// and <see cref="SpecialEffectType.FearAura"/>.
    /// </remarks>
    Passive = 0,

    /// <summary>
    /// Triggers when initiating an attack (before hit calculation).
    /// </summary>
    /// <remarks>
    /// OnAttack effects modify the attack before determining if it hits.
    /// Examples include <see cref="SpecialEffectType.IgnoreArmor"/>,
    /// <see cref="SpecialEffectType.Cleave"/>, and <see cref="SpecialEffectType.Phase"/>.
    /// </remarks>
    OnAttack = 1,

    /// <summary>
    /// Triggers when an attack successfully hits (after hit calculation).
    /// </summary>
    /// <remarks>
    /// OnHit effects are applied only when an attack successfully connects.
    /// Examples include <see cref="SpecialEffectType.FireDamage"/>,
    /// <see cref="SpecialEffectType.IceDamage"/>, <see cref="SpecialEffectType.LightningDamage"/>,
    /// and <see cref="SpecialEffectType.Slow"/>.
    /// </remarks>
    OnHit = 2,

    /// <summary>
    /// Triggers after damage has been dealt to target.
    /// </summary>
    /// <remarks>
    /// OnDamageDealt effects trigger after final damage is calculated and applied.
    /// The primary example is <see cref="SpecialEffectType.LifeSteal"/> which heals
    /// based on actual damage dealt.
    /// </remarks>
    OnDamageDealt = 3,

    /// <summary>
    /// Triggers when the wearer receives damage.
    /// </summary>
    /// <remarks>
    /// OnDamageTaken effects trigger when the item's wearer takes damage from any source.
    /// The primary example is <see cref="SpecialEffectType.Reflect"/> which returns
    /// a portion of damage to the attacker.
    /// </remarks>
    OnDamageTaken = 4,

    /// <summary>
    /// Triggers when the wearer defeats an enemy.
    /// </summary>
    /// <remarks>
    /// OnKill effects trigger when the wearer's attack reduces an enemy to 0 or fewer hit points.
    /// The primary example is <see cref="SpecialEffectType.AutoHide"/> which automatically
    /// enters the hidden state after a kill.
    /// </remarks>
    OnKill = 5
}
