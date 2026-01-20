namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of status effects determining their behavior.
/// </summary>
/// <remarks>
/// <para>
/// Status effect types:
/// <list type="bullet">
///   <item><description>StatModifier — Modifies stats</description></item>
///   <item><description>DamageOverTime — Deals damage each turn</description></item>
///   <item><description>HealOverTime — Heals each turn</description></item>
///   <item><description>ActionPrevention — Prevents actions</description></item>
///   <item><description>Movement — Affects movement speed</description></item>
///   <item><description>Special — Custom effects</description></item>
/// </list>
/// </para>
/// </remarks>
public enum StatusEffectType
{
    /// <summary>Modifies one or more stats.</summary>
    StatModifier,

    /// <summary>Deals damage each turn (poison, burning, bleeding).</summary>
    DamageOverTime,

    /// <summary>Heals each turn (regeneration).</summary>
    HealOverTime,

    /// <summary>Prevents certain actions (stun, freeze, silence).</summary>
    ActionPrevention,

    /// <summary>Affects movement (slow, haste, root).</summary>
    Movement,

    /// <summary>Custom/special effects (extra actions, reflect, etc.).</summary>
    Special
}
