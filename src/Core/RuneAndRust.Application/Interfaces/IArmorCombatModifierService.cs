// =============================================================================
// IArmorCombatModifierService.cs
// =============================================================================
// v0.16.2f - Combat Integration
// =============================================================================
// Interface for applying armor penalties to combat actions and determining
// Galdr casting eligibility. Consumes penalty calculations from
// IArmorPenaltyCalculator and archetype data from IArchetypeArmorProficiencyProvider.
// =============================================================================

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for applying armor penalties to combat actions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IArmorCombatModifierService"/> provides a unified interface for retrieving
/// all armor-related combat modifiers. It integrates:
/// </para>
/// <list type="bullet">
/// <item><description>Penalty calculations from <see cref="IArmorPenaltyCalculator"/></description></item>
/// <item><description>Archetype proficiency data from <see cref="IArchetypeArmorProficiencyProvider"/></description></item>
/// <item><description>Galdr/WITS interference rules</description></item>
/// </list>
/// <para>
/// Consumers can retrieve either the complete <see cref="CombatArmorState"/> or
/// individual modifiers as needed for their specific use case.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get complete combat state
/// var state = armorCombatService.GetCombatState("mystic", ArmorCategory.Heavy);
/// if (!state.CanCastGaldr)
/// {
///     ShowMessage("Cannot cast Galdr while wearing heavy armor!");
/// }
/// 
/// // Get individual modifiers for combat calculations
/// var attackMod = armorCombatService.GetAttackModifier(
///     ArmorCategory.Heavy, ArmorProficiencyLevel.NonProficient);
/// </code>
/// </example>
/// <seealso cref="CombatArmorState"/>
/// <seealso cref="IArmorPenaltyCalculator"/>
/// <seealso cref="IArchetypeArmorProficiencyProvider"/>
public interface IArmorCombatModifierService
{
    // =========================================================================
    // Complete State Methods
    // =========================================================================

    /// <summary>
    /// Gets the complete combat armor state for a character.
    /// </summary>
    /// <param name="archetypeId">The character's archetype ID (e.g., "warrior", "mystic").</param>
    /// <param name="armorCategory">The category of equipped armor.</param>
    /// <returns>
    /// A <see cref="CombatArmorState"/> containing all penalties, Galdr interference,
    /// and display warnings.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="archetypeId"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for obtaining all armor-related combat modifiers.
    /// The returned state includes:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Effective armor penalties (agility, stamina, movement)</description></item>
    /// <item><description>Attack and defense modifiers</description></item>
    /// <item><description>Galdr blocking and penalty information</description></item>
    /// <item><description>Stealth disadvantage status</description></item>
    /// <item><description>Display warnings for UI feedback</description></item>
    /// </list>
    /// </remarks>
    CombatArmorState GetCombatState(string archetypeId, ArmorCategory armorCategory);

    // =========================================================================
    // Galdr Interference Methods
    // =========================================================================

    /// <summary>
    /// Determines whether a character can cast Galdr with their current armor.
    /// </summary>
    /// <param name="archetypeId">The character's archetype ID.</param>
    /// <param name="armorCategory">The category of equipped armor.</param>
    /// <returns>
    /// <c>true</c> if Galdr casting is allowed (possibly with penalty);
    /// <c>false</c> if Galdr is completely blocked.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="archetypeId"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// For caster archetypes (Mystic, Adept), certain armor categories block Galdr entirely:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Mystic: Heavy armor and Shields block Galdr</description></item>
    /// <item><description>Adept: Shields block WITS-based abilities</description></item>
    /// </list>
    /// <para>
    /// Non-caster archetypes (Warrior, Skirmisher) are never blocked.
    /// </para>
    /// </remarks>
    bool CanCastGaldr(string archetypeId, ArmorCategory armorCategory);

    /// <summary>
    /// Gets the Galdr/WITS penalty for casting with armor.
    /// </summary>
    /// <param name="archetypeId">The character's archetype ID.</param>
    /// <param name="armorCategory">The category of equipped armor.</param>
    /// <returns>
    /// The penalty to apply to Galdr checks (0 or negative), or 0 if blocked.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="archetypeId"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// Penalties vary by archetype and armor category:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Mystic: Medium armor imposes -2 Galdr penalty</description></item>
    /// <item><description>Adept: Medium armor imposes -2 WITS, Heavy imposes -4 WITS</description></item>
    /// </list>
    /// <para>
    /// Returns 0 if Galdr is blocked (penalty is irrelevant when blocked).
    /// </para>
    /// </remarks>
    int GetGaldrPenalty(string archetypeId, ArmorCategory armorCategory);

    // =========================================================================
    // Individual Modifier Methods
    // =========================================================================

    /// <summary>
    /// Gets the agility dice modifier from armor penalties.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns>The agility dice penalty (0 or negative).</returns>
    /// <remarks>
    /// Affects agility-based skill checks and saving throws.
    /// Non-proficient characters suffer doubled penalties.
    /// </remarks>
    int GetAgilityModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Gets the stamina cost modifier from armor penalties.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns>The additional stamina cost (0 or positive).</returns>
    /// <remarks>
    /// Increases stamina consumption for actions while wearing armor.
    /// Non-proficient characters suffer doubled penalties.
    /// </remarks>
    int GetStaminaCostModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Gets the movement penalty from armor.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns>The movement penalty (0 or negative).</returns>
    /// <remarks>
    /// Reduces movement speed. Non-proficient characters suffer doubled penalties.
    /// Expert and Master proficiency can reduce tier penalties.
    /// </remarks>
    int GetMovementModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Gets the attack modifier from armor penalties or bonuses.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns>The attack modifier (negative for penalty, positive for bonus).</returns>
    /// <remarks>
    /// Non-proficient characters suffer -2 attack penalty.
    /// High proficiency does not grant attack bonuses.
    /// </remarks>
    int GetAttackModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Gets the defense modifier from armor penalties or bonuses.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns>The defense modifier (negative for penalty, positive for bonus).</returns>
    /// <remarks>
    /// Master proficiency grants +1 defense bonus.
    /// Non-proficient characters do not suffer additional defense penalty.
    /// </remarks>
    int GetDefenseModifier(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);

    /// <summary>
    /// Determines whether armor imposes stealth disadvantage.
    /// </summary>
    /// <param name="armorCategory">The category of armor.</param>
    /// <param name="proficiencyLevel">The character's proficiency with this armor category.</param>
    /// <returns><c>true</c> if stealth checks have disadvantage; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Heavy armor typically imposes stealth disadvantage regardless of proficiency.
    /// Medium armor may impose disadvantage for non-proficient characters.
    /// </remarks>
    bool HasStealthDisadvantage(ArmorCategory armorCategory, ArmorProficiencyLevel proficiencyLevel);
}
