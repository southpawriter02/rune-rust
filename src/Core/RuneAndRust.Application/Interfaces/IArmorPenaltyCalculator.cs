// ═══════════════════════════════════════════════════════════════════════════════
// IArmorPenaltyCalculator.cs
// Interface for armor penalty calculation services.
// Version: 0.16.2d
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for calculating effective armor penalties.
/// </summary>
/// <remarks>
/// <para>
/// IArmorPenaltyCalculator orchestrates the penalty calculation pipeline,
/// combining armor category data, proficiency effects, and archetype proficiency
/// mappings to produce effective penalties for gameplay use.
/// </para>
/// <para>
/// The calculation pipeline:
/// <list type="number">
///   <item><description>Get proficiency effects (multiplier, tier reduction, modifiers)</description></item>
///   <item><description>Apply tier reduction to get effective tier</description></item>
///   <item><description>Get base penalties for the effective tier</description></item>
///   <item><description>Apply penalty multiplier if NonProficient</description></item>
///   <item><description>Build EffectiveArmorPenalties result with metadata</description></item>
/// </list>
/// </para>
/// <para>
/// Key calculation rules:
/// <list type="table">
///   <listheader>
///     <term>Proficiency</term>
///     <description>Penalty Multiplier | Tier Reduction | Modifiers</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>2.0x | None | Attack -2</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>1.0x | None | None</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>1.0x | -1 tier | None</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>1.0x | -1 tier | Defense +1</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="EffectiveArmorPenalties"/>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorProficiencyLevel"/>
public interface IArmorPenaltyCalculator
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Core Calculation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates effective penalties for an armor category and proficiency level.
    /// </summary>
    /// <param name="category">The armor category being worn.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this category.</param>
    /// <returns>
    /// An <see cref="EffectiveArmorPenalties"/> containing the calculated penalties,
    /// modifiers, and metadata about the calculation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the core calculation method used by the combat and equipment systems.
    /// </para>
    /// <para>
    /// Calculation examples:
    /// <list type="bullet">
    ///   <item><description>Proficient + Heavy: Standard Heavy penalties, no modifiers</description></item>
    ///   <item><description>NonProficient + Heavy: Doubled penalties, -2 attack</description></item>
    ///   <item><description>Expert + Heavy: Medium tier penalties, no modifiers</description></item>
    ///   <item><description>Master + Heavy: Medium tier penalties, +1 defense</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var penalties = _calculator.CalculatePenalties(
    ///     ArmorCategory.Heavy,
    ///     ArmorProficiencyLevel.Expert);
    /// 
    /// Console.WriteLine(penalties.EffectiveTier);  // 1 (Medium)
    /// Console.WriteLine(penalties.WasTierReduced); // True
    /// </code>
    /// </example>
    EffectiveArmorPenalties CalculatePenalties(
        ArmorCategory category,
        ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Calculates penalties for an archetype wearing specific armor.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier (kebab-case).</param>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// An <see cref="EffectiveArmorPenalties"/> using the archetype's starting
    /// proficiency level for the specified category.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency data exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This convenience method looks up the archetype's starting proficiency
    /// for the category, then delegates to <see cref="CalculatePenalties"/>.
    /// </para>
    /// <para>
    /// Common archetype proficiencies:
    /// <list type="bullet">
    ///   <item><description>Warrior: Light, Medium, Heavy, Shields</description></item>
    ///   <item><description>Skirmisher: Light, Medium</description></item>
    ///   <item><description>Mystic: Light only</description></item>
    ///   <item><description>Adept: Light, Medium</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Mystic wearing Medium armor (non-proficient)
    /// var penalties = _calculator.CalculateForArchetype("mystic", ArmorCategory.Medium);
    /// Console.WriteLine(penalties.WasMultiplied);     // True
    /// Console.WriteLine(penalties.AttackModifier);    // -2
    /// </code>
    /// </example>
    EffectiveArmorPenalties CalculateForArchetype(
        string archetypeId,
        ArmorCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Tier Calculation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the effective tier after applying proficiency tier reduction.
    /// </summary>
    /// <param name="category">The armor category.</param>
    /// <param name="proficiencyLevel">The proficiency level.</param>
    /// <returns>
    /// The effective weight tier (0=Light, 1=Medium, 2=Heavy, -1=N/A).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tier reduction is applied when Expert or Master proficiency is used:
    /// <list type="bullet">
    ///   <item><description>Heavy (Tier 2) → Medium (Tier 1)</description></item>
    ///   <item><description>Medium (Tier 1) → Light (Tier 0)</description></item>
    ///   <item><description>Light (Tier 0) → Light (Tier 0) (no change)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Shields (Tier -1) are not affected by tier reduction.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier = _calculator.GetEffectiveTier(ArmorCategory.Heavy, ArmorProficiencyLevel.Expert);
    /// Console.WriteLine(tier); // 1 (Medium penalties apply)
    /// </code>
    /// </example>
    int GetEffectiveTier(ArmorCategory category, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Gets the penalties for a specific weight tier.
    /// </summary>
    /// <param name="tier">The weight tier (0=Light, 1=Medium, 2=Heavy).</param>
    /// <returns>
    /// The <see cref="ArmorPenalties"/> for the specified tier.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Used to get penalties for the effective tier after tier reduction.
    /// </para>
    /// <para>
    /// Standard tier penalties:
    /// <list type="bullet">
    ///   <item><description>Tier 0 (Light): No penalties</description></item>
    ///   <item><description>Tier 1 (Medium): -1d10 Agi, +2 Stam, -5ft Move, Stealth Disadv</description></item>
    ///   <item><description>Tier 2 (Heavy): -2d10 Agi, +5 Stam, -10ft Move, Stealth Disadv</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mediumPenalties = _calculator.GetPenaltiesForTier(1);
    /// Console.WriteLine(mediumPenalties.AgilityDicePenalty); // -1
    /// </code>
    /// </example>
    ArmorPenalties GetPenaltiesForTier(int tier);

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the proficiency level would double penalties for the category.
    /// </summary>
    /// <param name="category">The armor category.</param>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// <c>true</c> if penalties would be doubled (NonProficient); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Use for UI preview before equipping armor. Shows warning if true.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (_calculator.WouldDoublePenalties(ArmorCategory.Heavy, ArmorProficiencyLevel.NonProficient))
    /// {
    ///     ShowWarning("Wearing this armor will double all penalties!");
    /// }
    /// </code>
    /// </example>
    bool WouldDoublePenalties(ArmorCategory category, ArmorProficiencyLevel level);

    /// <summary>
    /// Checks if the proficiency level would reduce the tier for the category.
    /// </summary>
    /// <param name="category">The armor category.</param>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// <c>true</c> if tier reduction would apply (Expert/Master with Heavy or Medium);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Use for UI display showing tier reduction benefits.
    /// Returns <c>false</c> for Light armor (already minimum tier).
    /// </remarks>
    /// <example>
    /// <code>
    /// if (_calculator.WouldReduceTier(ArmorCategory.Heavy, ArmorProficiencyLevel.Expert))
    /// {
    ///     ShowBenefit("Your expertise reduces Heavy to Medium-tier penalties!");
    /// }
    /// </code>
    /// </example>
    bool WouldReduceTier(ArmorCategory category, ArmorProficiencyLevel level);
}
