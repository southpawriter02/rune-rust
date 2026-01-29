// ═══════════════════════════════════════════════════════════════════════════════
// IArchetypeArmorProficiencyProvider.cs
// Interface for accessing archetype-armor proficiency data.
// Version: 0.16.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to archetype-armor proficiency data.
/// </summary>
/// <remarks>
/// <para>
/// IArchetypeArmorProficiencyProvider abstracts access to the archetype-armor
/// proficiency matrix, enabling lookup of starting proficiencies and Galdr
/// interference rules for any archetype-armor combination.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
///   <item><description>Load and cache proficiency data from configuration</description></item>
///   <item><description>Provide proficiency lookups by archetype and armor category</description></item>
///   <item><description>Provide Galdr interference rule lookups</description></item>
///   <item><description>Validate configuration completeness</description></item>
/// </list>
/// </para>
/// <para>
/// Implementation note: The concrete provider loads data from
/// config/archetype-armor-proficiencies.json and caches it for efficient
/// lookup during gameplay.
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeArmorProficiencySet"/>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorProficiencyLevel"/>
/// <seealso cref="GaldrInterference"/>
public interface IArchetypeArmorProficiencyProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Set Retrieval
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the complete proficiency set for an archetype.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier (kebab-case).</param>
    /// <returns>
    /// The <see cref="ArchetypeArmorProficiencySet"/> for the specified archetype.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// Use this method when you need the complete proficiency information,
    /// including all categories and Galdr rules, for an archetype.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticSet = provider.GetProficiencySet("mystic");
    /// Console.WriteLine(mysticSet.FormatProficientCategories()); // "Light"
    /// </code>
    /// </example>
    ArchetypeArmorProficiencySet GetProficiencySet(string archetypeId);

    /// <summary>
    /// Gets all configured proficiency sets.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="ArchetypeArmorProficiencySet"/> instances.
    /// </returns>
    /// <remarks>
    /// Use for administrative purposes, validation, or iterating over all archetypes.
    /// </remarks>
    IReadOnlyList<ArchetypeArmorProficiencySet> GetAllProficiencySets();

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the starting proficiency level for a specific archetype-armor combination.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <see cref="ArmorProficiencyLevel.Proficient"/> if the archetype starts proficient;
    /// otherwise, <see cref="ArmorProficiencyLevel.NonProficient"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method returns the character's initial proficiency level at creation.
    /// Characters may improve proficiency through gameplay training.
    /// </para>
    /// <para>
    /// Note: Starting proficiency is never Expert or Master; those levels
    /// require significant in-game advancement.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var level = provider.GetStartingProficiency("warrior", ArmorCategory.Heavy);
    /// Console.WriteLine(level); // Proficient
    /// 
    /// var mysticLevel = provider.GetStartingProficiency("mystic", ArmorCategory.Heavy);
    /// Console.WriteLine(mysticLevel); // NonProficient
    /// </code>
    /// </example>
    ArmorProficiencyLevel GetStartingProficiency(string archetypeId, ArmorCategory category);

    /// <summary>
    /// Determines if an archetype is proficient with a specific armor category.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if the archetype starts proficient with this category;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// Convenience method equivalent to checking if
    /// <see cref="GetStartingProficiency"/> returns Proficient.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (provider.IsProficient("skirmisher", ArmorCategory.Medium))
    /// {
    ///     Console.WriteLine("Skirmisher can wear medium armor effectively");
    /// }
    /// </code>
    /// </example>
    bool IsProficient(string archetypeId, ArmorCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Galdr Interference Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the Galdr interference rules for an archetype wearing specific armor.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// The <see cref="GaldrInterference"/> rules if the archetype has them;
    /// otherwise, <c>null</c> for non-caster archetypes.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// Returns <c>null</c> for Warrior and Skirmisher archetypes since they
    /// have no Galdr/WITS abilities affected by armor.
    /// </remarks>
    /// <example>
    /// <code>
    /// var interference = provider.GetGaldrInterference("mystic", ArmorCategory.Medium);
    /// if (interference.HasValue)
    /// {
    ///     Console.WriteLine($"Penalty: {interference.Value.MediumArmorPenalty}"); // -2
    /// }
    /// </code>
    /// </example>
    GaldrInterference? GetGaldrInterference(string archetypeId, ArmorCategory category);

    /// <summary>
    /// Determines if the specified armor category blocks Galdr casting for an archetype.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if Galdr/WITS abilities are completely blocked;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Returns <c>false</c> for non-caster archetypes (no Galdr to block).
    /// </para>
    /// <para>
    /// For Mystics: Heavy armor and Shields return <c>true</c>.
    /// For Adepts: Only Shields return <c>true</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (provider.IsGaldrBlocked("mystic", ArmorCategory.Heavy))
    /// {
    ///     Console.WriteLine("Mystic cannot cast Galdr in heavy armor!");
    /// }
    /// </code>
    /// </example>
    bool IsGaldrBlocked(string archetypeId, ArmorCategory category);

    /// <summary>
    /// Gets the Galdr/WITS penalty for an archetype wearing specific armor.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// The penalty value (negative number), or 0 if no penalty applies.
    /// Returns 0 for non-caster archetypes or if the armor blocks casting.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// Common return values:
    /// <list type="bullet">
    ///   <item><description>-2: Medium armor penalty (Mystic/Adept)</description></item>
    ///   <item><description>-4: Heavy armor penalty (Adept only)</description></item>
    ///   <item><description>0: Light armor, non-caster, or blocked</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var penalty = provider.GetGaldrPenalty("adept", ArmorCategory.Heavy);
    /// Console.WriteLine($"WITS penalty: {penalty}"); // -4
    /// </code>
    /// </example>
    int GetGaldrPenalty(string archetypeId, ArmorCategory category);

    /// <summary>
    /// Determines if an archetype can cast Galdr while wearing the specified armor.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    /// <param name="equippedCategory">The armor category currently equipped.</param>
    /// <returns>
    /// <c>true</c> if Galdr/WITS abilities can be used (possibly with penalty);
    /// <c>false</c> if completely blocked.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no proficiency set exists for the specified archetype.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Returns <c>true</c> for non-caster archetypes (they don't use Galdr).
    /// </para>
    /// <para>
    /// This is the inverse of <see cref="IsGaldrBlocked"/>. Use to determine
    /// if a character can attempt Galdr actions at all.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (provider.CanCastGaldr("mystic", equippedArmor.Category))
    /// {
    ///     var penalty = provider.GetGaldrPenalty("mystic", equippedArmor.Category);
    ///     // Apply penalty to Galdr roll
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Cannot cast Galdr in this armor!");
    /// }
    /// </code>
    /// </example>
    bool CanCastGaldr(string archetypeId, ArmorCategory equippedCategory);

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that all required archetype configurations are present.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all required archetypes have proficiency configurations;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation checks:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All core archetypes (warrior, skirmisher, mystic, adept) are configured</description></item>
    ///   <item><description>Each archetype has at least one proficient category</description></item>
    ///   <item><description>Proficient and non-proficient categories don't overlap</description></item>
    ///   <item><description>Caster archetypes have Galdr interference rules</description></item>
    /// </list>
    /// <para>
    /// Call during application startup to ensure configuration completeness.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!provider.ValidateConfiguration())
    /// {
    ///     _logger.LogError("Archetype armor proficiency configuration is incomplete!");
    /// }
    /// </code>
    /// </example>
    bool ValidateConfiguration();
}
