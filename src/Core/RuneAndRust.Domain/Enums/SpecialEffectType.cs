namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of special effects available on Myth-Forged items.
/// Each effect has specific trigger conditions and combat implications.
/// </summary>
/// <remarks>
/// <para>
/// Special effects are the defining features of Myth-Forged (Tier 4) items,
/// providing powerful unique modifiers such as armor penetration, life steal,
/// elemental damage, and passive detection abilities.
/// </para>
/// <para>
/// Effects are grouped into categories:
/// <list type="bullet">
///   <item><description><b>Combat Effects</b> (1-5): IgnoreArmor, LifeSteal, Cleave, Phase, Reflect</description></item>
///   <item><description><b>Elemental Effects</b> (6-8): FireDamage, IceDamage, LightningDamage</description></item>
///   <item><description><b>Triggered Effects</b> (9-10): Slow, AutoHide</description></item>
///   <item><description><b>Passive Effects</b> (11-14): Detection, CriticalBonus, DamageReduction, FearAura</description></item>
/// </list>
/// </para>
/// <para>
/// Each effect type has an expected <see cref="EffectTriggerType"/> that determines
/// when the effect activates during combat or gameplay. The <see cref="ValueObjects.SpecialEffect"/>
/// value object validates that trigger types match expected values for each effect type.
/// </para>
/// <para>
/// Type values are explicitly assigned (0-14) to ensure stable serialization
/// and database storage. New types should be added at the end if needed.
/// </para>
/// </remarks>
/// <seealso cref="EffectTriggerType"/>
/// <seealso cref="ValueObjects.SpecialEffect"/>
public enum SpecialEffectType
{
    /// <summary>
    /// No effect (default).
    /// </summary>
    /// <remarks>
    /// Used as a default value for uninitialized or null effects.
    /// Cannot be used when creating a valid <see cref="ValueObjects.SpecialEffect"/>.
    /// </remarks>
    None = 0,

    // ==================== Combat Effects ====================

    /// <summary>
    /// Attacks ignore target's armor/defense completely.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnAttack"/></para>
    /// <para>
    /// When attacking with this effect, the target's defense value is treated as 0
    /// for damage calculation purposes. Does not affect other defensive abilities
    /// like blocking or parrying.
    /// </para>
    /// </remarks>
    IgnoreArmor = 1,

    /// <summary>
    /// Heals the attacker for a percentage of damage dealt.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnDamageDealt"/></para>
    /// <para>
    /// The magnitude value represents the percentage of damage dealt that is
    /// converted to healing (0.0-1.0, e.g., 0.15 = 15% healing).
    /// Healing is calculated after all damage modifiers are applied.
    /// </para>
    /// </remarks>
    LifeSteal = 2,

    /// <summary>
    /// Attack hits adjacent enemies in addition to primary target.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnAttack"/></para>
    /// <para>
    /// Cleave attacks hit all enemies adjacent to the primary target.
    /// Each hit uses the same attack roll but applies separate damage calculations.
    /// The magnitude value is typically 1.0 (full damage) or less for reduced splash damage.
    /// </para>
    /// </remarks>
    Cleave = 3,

    /// <summary>
    /// Attack cannot be blocked or parried.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnAttack"/></para>
    /// <para>
    /// Phase attacks pass through defensive reactions like blocking, parrying,
    /// or shield blocks. The target may still dodge or use other evasive abilities.
    /// </para>
    /// </remarks>
    Phase = 4,

    /// <summary>
    /// Returns a percentage of damage taken back to attacker.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnDamageTaken"/></para>
    /// <para>
    /// When the wearer receives damage, a percentage is reflected back to the
    /// attacker (0.0-1.0, e.g., 0.25 = 25% reflection). Reflected damage ignores
    /// the attacker's armor and is dealt as the same damage type received.
    /// </para>
    /// </remarks>
    Reflect = 5,

    // ==================== Elemental Effects ====================

    /// <summary>
    /// Deals additional fire damage on hit.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnHit"/></para>
    /// <para>
    /// When an attack successfully hits, bonus fire damage is applied.
    /// The magnitude value is the flat damage amount added.
    /// Requires a damageTypeId (typically "fire") in the <see cref="ValueObjects.SpecialEffect"/>.
    /// </para>
    /// </remarks>
    FireDamage = 6,

    /// <summary>
    /// Deals additional ice damage on hit.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnHit"/></para>
    /// <para>
    /// When an attack successfully hits, bonus ice damage is applied.
    /// The magnitude value is the flat damage amount added.
    /// Requires a damageTypeId (typically "ice") in the <see cref="ValueObjects.SpecialEffect"/>.
    /// </para>
    /// </remarks>
    IceDamage = 7,

    /// <summary>
    /// Deals additional lightning damage on hit.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnHit"/></para>
    /// <para>
    /// When an attack successfully hits, bonus lightning damage is applied.
    /// The magnitude value is the flat damage amount added.
    /// Requires a damageTypeId (typically "lightning") in the <see cref="ValueObjects.SpecialEffect"/>.
    /// </para>
    /// </remarks>
    LightningDamage = 8,

    // ==================== Triggered Effects ====================

    /// <summary>
    /// Reduces target's movement speed on hit.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnHit"/></para>
    /// <para>
    /// When an attack successfully hits, the target's movement speed is reduced.
    /// The magnitude value represents the duration in seconds or turns.
    /// The slow effect may stack with other movement-reducing effects.
    /// </para>
    /// </remarks>
    Slow = 9,

    /// <summary>
    /// Automatically enters [Hidden] state after defeating an enemy.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.OnKill"/></para>
    /// <para>
    /// When the wearer kills an enemy, they automatically enter the hidden state
    /// as if they had successfully used a stealth ability. This allows for
    /// chain assassinations and escape from combat.
    /// </para>
    /// </remarks>
    AutoHide = 10,

    // ==================== Passive Effects ====================

    /// <summary>
    /// Reveals hidden enemies and traps within range.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.Passive"/></para>
    /// <para>
    /// While the item is equipped, hidden enemies, concealed traps, and
    /// secret passages within the detection range are automatically revealed.
    /// The magnitude value may affect detection range or success chance.
    /// </para>
    /// </remarks>
    Detection = 11,

    /// <summary>
    /// Increases critical hit chance.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.Passive"/></para>
    /// <para>
    /// While the item is equipped, the wearer's critical hit chance is increased.
    /// The magnitude value represents the bonus chance (0.0-1.0, e.g., 0.10 = +10%).
    /// This stacks additively with other critical chance modifiers.
    /// </para>
    /// </remarks>
    CriticalBonus = 12,

    /// <summary>
    /// Reduces all incoming damage by a flat amount.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.Passive"/></para>
    /// <para>
    /// While the item is equipped, all incoming damage is reduced by a flat value.
    /// The magnitude value is the damage reduction amount. This applies after
    /// percentage-based reductions but before minimum damage calculations.
    /// </para>
    /// </remarks>
    DamageReduction = 13,

    /// <summary>
    /// Intimidates nearby enemies, potentially causing fear.
    /// </summary>
    /// <remarks>
    /// <para><b>Trigger:</b> <see cref="EffectTriggerType.Passive"/></para>
    /// <para>
    /// While the item is equipped, nearby enemies may become frightened or
    /// intimidated, potentially affecting their combat behavior or causing
    /// them to flee. Lower-level enemies are more susceptible.
    /// </para>
    /// </remarks>
    FearAura = 14
}
