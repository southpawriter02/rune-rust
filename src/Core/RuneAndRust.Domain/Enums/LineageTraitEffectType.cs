// ═══════════════════════════════════════════════════════════════════════════════
// LineageTraitEffectType.cs
// Enum defining the types of effects that lineage traits can apply.
// Version: 0.17.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of effects that lineage traits can apply.
/// </summary>
/// <remarks>
/// <para>
/// LineageTraitEffectType categorizes how a trait modifies gameplay:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="BonusDiceToSkill"/>: Adds dice to skill checks under certain conditions
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="PercentageModifier"/>: Scales incoming or outgoing values by percentage
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="BonusDiceToResolve"/>: Adds dice to resolve checks (e.g., vs hazards)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="PassiveAuraBonus"/>: Applies a percentage bonus to resource pools
///     </description>
///   </item>
/// </list>
/// <para>
/// Effect types are explicitly assigned (0-3) to ensure stable serialization
/// and database storage.
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.LineageTrait"/>
public enum LineageTraitEffectType
{
    /// <summary>
    /// Adds bonus dice to specific skill checks when conditions are met.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by traits like [Survivor's Resolve] which adds +1d10 to Rhetoric
    /// checks when interacting with Clan-Born NPCs.
    /// </para>
    /// <para>
    /// Required properties for this effect type:
    /// <list type="bullet">
    ///   <item><description>BonusDice: The number of bonus dice to add (must be positive)</description></item>
    ///   <item><description>TargetCheck: The skill check type affected (e.g., "rhetoric")</description></item>
    ///   <item><description>TargetCondition: Optional condition for valid targets</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BonusDiceToSkill = 0,

    /// <summary>
    /// Modifies incoming or outgoing values by a percentage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by traits like [Primal Clarity] which reduces Psychic Stress
    /// by 10%. Positive values increase the value, negative values decrease it.
    /// </para>
    /// <para>
    /// Required properties for this effect type:
    /// <list type="bullet">
    ///   <item><description>PercentModifier: The percentage modifier (-1.0 to 1.0)</description></item>
    ///   <item><description>TriggerCondition: When the modifier activates</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    PercentageModifier = 1,

    /// <summary>
    /// Adds bonus dice to resolve checks (mental/physical resistance).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by traits like [Hazard Acclimation] which adds +1d10 to
    /// Sturdiness Resolve checks versus environmental hazards.
    /// </para>
    /// <para>
    /// Required properties for this effect type:
    /// <list type="bullet">
    ///   <item><description>BonusDice: The number of bonus dice to add (must be positive)</description></item>
    ///   <item><description>TargetCheck: The resolve check type affected (e.g., "sturdiness")</description></item>
    ///   <item><description>TriggerCondition: When the bonus applies</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BonusDiceToResolve = 2,

    /// <summary>
    /// Applies a percentage bonus to a resource pool (HP, AP, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by traits like [Aether-Tainted] which increases Maximum
    /// Aether Pool by 10%. Applied during derived stat calculation.
    /// </para>
    /// <para>
    /// Required properties for this effect type:
    /// <list type="bullet">
    ///   <item><description>PercentModifier: The percentage bonus (typically positive)</description></item>
    ///   <item><description>TriggerCondition: The calculation context (e.g., "max_ap_calculation")</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    PassiveAuraBonus = 3
}
