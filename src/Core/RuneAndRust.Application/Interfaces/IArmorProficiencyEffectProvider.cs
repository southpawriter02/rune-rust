// ═══════════════════════════════════════════════════════════════════════════════
// IArmorProficiencyEffectProvider.cs
// Interface for accessing armor proficiency effect data from configuration.
// Version: 0.16.2a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to armor proficiency effect data.
/// </summary>
/// <remarks>
/// <para>
/// Implementations load armor proficiency effects from configuration (armor-proficiency-effects.json)
/// to determine penalties, bonuses, and behaviors for each armor proficiency level.
/// </para>
/// <para>
/// This interface centralizes armor proficiency data access to ensure consistent
/// penalty calculations and bonus application across the system.
/// </para>
/// <para>
/// Key responsibilities:
/// </para>
/// <list type="bullet">
///   <item><description>Load and cache armor proficiency effects from configuration</description></item>
///   <item><description>Provide effects for specific proficiency levels</description></item>
///   <item><description>Validate that all required levels are configured</description></item>
///   <item><description>Support penalty multiplier, tier reduction, and modifier lookups</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArmorProficiencyLevel"/>
/// <seealso cref="ArmorProficiencyEffect"/>
public interface IArmorProficiencyEffectProvider
{
    /// <summary>
    /// Gets the effect data for a specific armor proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level to retrieve effects for.</param>
    /// <returns>
    /// The <see cref="ArmorProficiencyEffect"/> for the specified level.
    /// </returns>
    /// <remarks>
    /// This method returns the complete effect data including penalty multiplier,
    /// attack modifier, defense modifier, tier reduction, and special property access.
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = _provider.GetEffect(ArmorProficiencyLevel.Master);
    /// if (effect.HasDefenseBonus)
    /// {
    ///     ApplyDefenseBonus(effect.DefenseModifier);
    /// }
    /// </code>
    /// </example>
    ArmorProficiencyEffect GetEffect(ArmorProficiencyLevel level);

    /// <summary>
    /// Gets all armor proficiency effects ordered by level.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="ArmorProficiencyEffect"/> instances,
    /// ordered from NonProficient to Master.
    /// </returns>
    /// <remarks>
    /// Useful for displaying proficiency progression in UI or for
    /// iterating over all possible effects.
    /// </remarks>
    IReadOnlyList<ArmorProficiencyEffect> GetAllEffects();

    /// <summary>
    /// Gets the penalty multiplier for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The penalty multiplier (1.0 for normal, 2.0 for doubled penalties).
    /// </returns>
    /// <remarks>
    /// Convenience method for armor penalty calculations without
    /// retrieving the full effect object.
    /// </remarks>
    decimal GetPenaltyMultiplier(ArmorProficiencyLevel level);

    /// <summary>
    /// Gets the attack modifier for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The attack roll modifier (-2 to 0).
    /// </returns>
    int GetAttackModifier(ArmorProficiencyLevel level);

    /// <summary>
    /// Gets the defense modifier for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The defense modifier (0 to +1).
    /// </returns>
    int GetDefenseModifier(ArmorProficiencyLevel level);

    /// <summary>
    /// Gets the tier reduction for a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The number of tiers to reduce for penalty calculation (0 to 1).
    /// </returns>
    /// <remarks>
    /// Expert and Master proficiencies have tier reduction of 1,
    /// causing Heavy armor to use Medium-tier penalties.
    /// </remarks>
    int GetTierReduction(ArmorProficiencyLevel level);

    /// <summary>
    /// Checks whether a proficiency level allows using special armor properties.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// <c>true</c> if special properties can be used; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// NonProficient characters cannot activate armor special properties
    /// (fire resistance, magic absorption, etc.).
    /// </remarks>
    bool CanUseSpecialProperties(ArmorProficiencyLevel level);

    /// <summary>
    /// Gets the display name for a proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>
    /// The human-readable display name (e.g., "Non-Proficient", "Master").
    /// </returns>
    string GetDisplayName(ArmorProficiencyLevel level);

    /// <summary>
    /// Validates that the configuration contains effects for all proficiency levels.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all four levels (NonProficient, Proficient, Expert, Master)
    /// have corresponding effect configurations; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method should be called during application startup to ensure
    /// the armor proficiency system is properly configured.
    /// </remarks>
    bool ValidateConfiguration();
}
