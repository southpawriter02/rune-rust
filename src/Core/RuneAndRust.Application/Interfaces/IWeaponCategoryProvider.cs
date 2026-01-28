// ═══════════════════════════════════════════════════════════════════════════════
// IWeaponCategoryProvider.cs
// Interface defining the contract for accessing weapon category data.
// Version: 0.16.1b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to weapon category metadata.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of weapon category data, allowing
/// it to be loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should:
/// </para>
/// <list type="bullet">
///   <item><description>Cache loaded definitions for performance</description></item>
///   <item><description>Validate that all 11 weapon categories have corresponding entries</description></item>
///   <item><description>Provide fallback behavior when configuration is missing</description></item>
///   <item><description>Log configuration loading and validation results</description></item>
/// </list>
/// <para>
/// Usage example:
/// </para>
/// <code>
/// // Inject via DI
/// public class CombatService(IWeaponCategoryProvider categoryProvider)
/// {
///     public AttributeType GetWeaponAttribute(WeaponCategory category)
///     {
///         var definition = categoryProvider.GetDefinition(category);
///         return definition.PrimaryAttribute;
///     }
/// }
/// </code>
/// <para>
/// The 11 weapon categories are grouped by primary attribute:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Attribute</term>
///     <description>Categories</description>
///   </listheader>
///   <item>
///     <term>MIGHT</term>
///     <description>Axes, Swords, Hammers, Polearms, Shields</description>
///   </item>
///   <item>
///     <term>FINESSE</term>
///     <description>Daggers, Bows</description>
///   </item>
///   <item>
///     <term>WILL</term>
///     <description>Staves, ArcaneImplements</description>
///   </item>
///   <item>
///     <term>WITS</term>
///     <description>Crossbows, Firearms</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="WeaponCategoryDefinition"/>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="AttributeType"/>
public interface IWeaponCategoryProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Core Definition Access Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the definition for a specific weapon category.
    /// </summary>
    /// <param name="category">The weapon category.</param>
    /// <returns>
    /// The <see cref="WeaponCategoryDefinition"/> containing all metadata
    /// for the specified category.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the definition for the specified category is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for retrieving category metadata. It returns
    /// a complete definition object that can be used to:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Determine primary attribute for attack/damage calculations</description></item>
    ///   <item><description>Check special training requirements</description></item>
    ///   <item><description>Display category information in UI</description></item>
    ///   <item><description>Get example weapons for tooltips</description></item>
    /// </list>
    /// <para>
    /// Implementations should cache definitions after initial load for performance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var swordsDefinition = provider.GetDefinition(WeaponCategory.Swords);
    /// Console.WriteLine($"Swords use {swordsDefinition.PrimaryAttribute}"); // "Might"
    /// Console.WriteLine($"Examples: {swordsDefinition.FormatExamples()}");
    /// </code>
    /// </example>
    WeaponCategoryDefinition GetDefinition(WeaponCategory category);

    /// <summary>
    /// Gets definitions for all weapon categories.
    /// </summary>
    /// <returns>
    /// A read-only list of all category definitions, ordered by enum value ascending
    /// (Axes → Swords → ... → ArcaneImplements).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Useful for:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Populating UI elements showing all weapon categories</description></item>
    ///   <item><description>Validating configuration completeness</description></item>
    ///   <item><description>Generating filter or selection lists</description></item>
    /// </list>
    /// <para>
    /// The returned list should always contain exactly 11 elements, one for each
    /// <see cref="WeaponCategory"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display all weapon categories in equipment UI
    /// foreach (var definition in provider.GetAllDefinitions())
    /// {
    ///     Console.WriteLine($"{definition.DisplayName}: {definition.FormatPrimaryAttribute()}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<WeaponCategoryDefinition> GetAllDefinitions();

    // ═══════════════════════════════════════════════════════════════════════════
    // Attribute Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all categories that use a specific primary attribute.
    /// </summary>
    /// <param name="attribute">The primary attribute to filter by.</param>
    /// <returns>
    /// A read-only list of categories using that attribute for attack/damage.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Useful for filtering equipment by character build or showing
    /// weapons that match a character's primary attribute.
    /// </para>
    /// <para>Expected results by attribute:</para>
    /// <list type="bullet">
    ///   <item><description>MIGHT: Axes, Swords, Hammers, Polearms, Shields (5 categories)</description></item>
    ///   <item><description>FINESSE: Daggers, Bows (2 categories)</description></item>
    ///   <item><description>WILL: Staves, ArcaneImplements (2 categories)</description></item>
    ///   <item><description>WITS: Crossbows, Firearms (2 categories)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find all FINESSE weapons for a Skirmisher build
    /// var finesseCategories = provider.GetCategoriesByAttribute(AttributeType.Finesse);
    /// // Returns: [Daggers, Bows]
    /// </code>
    /// </example>
    IReadOnlyList<WeaponCategory> GetCategoriesByAttribute(AttributeType attribute);

    /// <summary>
    /// Gets the primary attribute for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category.</param>
    /// <returns>
    /// The primary attribute (Might, Finesse, Will, or Wits) used for
    /// attack and damage calculations.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Convenience method that extracts only the primary attribute from the full definition.
    /// Equivalent to calling <c>GetDefinition(category).PrimaryAttribute</c>.
    /// </para>
    /// <para>
    /// Use this method when you only need the attribute, not the full definition object.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var attr = provider.GetPrimaryAttribute(WeaponCategory.Daggers);
    /// Console.WriteLine($"Daggers scale with {attr}"); // "Finesse"
    /// </code>
    /// </example>
    AttributeType GetPrimaryAttribute(WeaponCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Special Training Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a category requires special training beyond archetype defaults.
    /// </summary>
    /// <param name="category">The weapon category.</param>
    /// <returns>
    /// <c>true</c> if special training is required to use weapons of this category
    /// without penalties; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Currently, only <see cref="WeaponCategory.Firearms"/> requires special training.
    /// All other categories can be learned through standard archetype proficiencies
    /// or normal training methods.
    /// </para>
    /// <para>
    /// Convenience method equivalent to calling <c>GetDefinition(category).RequiresSpecialTraining</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (provider.RequiresSpecialTraining(weaponCategory))
    /// {
    ///     Logger.LogWarning("This weapon requires special training!");
    /// }
    /// </code>
    /// </example>
    bool RequiresSpecialTraining(WeaponCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category.</param>
    /// <returns>
    /// The localized display name for UI presentation (e.g., "Swords", "Arcane Implements").
    /// </returns>
    /// <remarks>
    /// <para>
    /// Display names are loaded from configuration and may differ from enum names.
    /// For example, <see cref="WeaponCategory.ArcaneImplements"/> displays as "Arcane Implements".
    /// </para>
    /// <para>
    /// Convenience method equivalent to calling <c>GetDefinition(category).DisplayName</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// string displayName = provider.GetDisplayName(WeaponCategory.ArcaneImplements);
    /// categoryLabel.Text = displayName; // "Arcane Implements"
    /// </code>
    /// </example>
    string GetDisplayName(WeaponCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that all required weapon categories have definition entries.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all 11 weapon categories have corresponding definition entries;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called during application startup to verify that
    /// the configuration is complete. Missing entries may indicate a configuration
    /// file error or version mismatch.
    /// </para>
    /// <para>
    /// Implementations should log warnings or errors for missing entries to
    /// help diagnose configuration issues.
    /// </para>
    /// <para>
    /// All 11 categories must have definitions:
    /// Axes, Swords, Hammers, Daggers, Polearms, Staves, Bows, Crossbows,
    /// Shields, Firearms, ArcaneImplements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During application startup
    /// if (!categoryProvider.ValidateConfiguration())
    /// {
    ///     Logger.LogError("Weapon category configuration incomplete - some categories missing!");
    ///     // Use fallback defaults or abort startup
    /// }
    /// </code>
    /// </example>
    bool ValidateConfiguration();
}
