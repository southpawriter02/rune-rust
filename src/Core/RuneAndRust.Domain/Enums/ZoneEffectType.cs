namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of effects that zone abilities can apply to combatants.
/// </summary>
/// <remarks>
/// <para>Zone effect types determine how the zone interacts with entities within its area:</para>
/// <list type="bullet">
///   <item><description><see cref="Damage"/> - Deals damage each turn to affected entities</description></item>
///   <item><description><see cref="Healing"/> - Restores health each turn to affected entities</description></item>
///   <item><description><see cref="Buff"/> - Applies a positive status effect while in the zone</description></item>
///   <item><description><see cref="Debuff"/> - Applies a negative status effect while in the zone</description></item>
///   <item><description><see cref="Terrain"/> - Modifies movement and terrain properties</description></item>
///   <item><description><see cref="Mixed"/> - Combines multiple effect types in one zone</description></item>
/// </list>
/// <para>
/// Zones are processed during the tick phase of combat, applying their effects to all
/// entities within their affected cells based on the zone's friendly/enemy targeting settings.
/// </para>
/// </remarks>
public enum ZoneEffectType
{
    /// <summary>
    /// Zone deals damage to affected entities each turn.
    /// </summary>
    /// <remarks>
    /// <para>Examples: Wall of Fire, Acid Pool, Lightning Storm</para>
    /// <para>Uses the zone's DamageValue (dice notation) and DamageType properties.</para>
    /// </remarks>
    Damage,

    /// <summary>
    /// Zone restores health to affected entities each turn.
    /// </summary>
    /// <remarks>
    /// <para>Examples: Healing Circle, Rejuvenation Field, Life Spring</para>
    /// <para>Uses the zone's HealValue (dice notation) property.</para>
    /// </remarks>
    Healing,

    /// <summary>
    /// Zone applies a beneficial status effect while entities remain in the area.
    /// </summary>
    /// <remarks>
    /// <para>Examples: Blessing Aura, Haste Field, Protection Zone</para>
    /// <para>Uses the zone's StatusEffectId to reference the buff definition.</para>
    /// </remarks>
    Buff,

    /// <summary>
    /// Zone applies a detrimental status effect while entities remain in the area.
    /// </summary>
    /// <remarks>
    /// <para>Examples: Slow Field, Curse Zone, Weakness Aura</para>
    /// <para>Uses the zone's StatusEffectId to reference the debuff definition.</para>
    /// </remarks>
    Debuff,

    /// <summary>
    /// Zone modifies terrain properties and movement characteristics.
    /// </summary>
    /// <remarks>
    /// <para>Examples: Web, Ice Patch, Difficult Terrain, Entanglement</para>
    /// <para>Uses TerrainModifier for movement penalties and optionally StatusEffectId
    /// for additional effects like immobilization.</para>
    /// </remarks>
    Terrain,

    /// <summary>
    /// Zone combines multiple effect types (damage, healing, and/or status effects).
    /// </summary>
    /// <remarks>
    /// <para>Examples: Consecration (heals allies, damages undead), Corrupted Ground</para>
    /// <para>Uses multiple properties: DamageValue, HealValue, and/or StatusEffectId
    /// based on the zone's configuration.</para>
    /// </remarks>
    Mixed
}
