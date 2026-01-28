// ═══════════════════════════════════════════════════════════════════════════════
// IArmorCategoryProvider.cs
// Interface for armor category metadata access.
// Version: 0.16.2b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to armor category metadata.
/// </summary>
/// <remarks>
/// <para>
/// IArmorCategoryProvider abstracts access to armor category definitions,
/// enabling configuration-driven category metadata and testability through
/// mock implementations.
/// </para>
/// <para>
/// Implementations should load category data from configuration files
/// (armor-categories.json) and cache definitions for efficient repeated access.
/// </para>
/// <para>
/// This interface provides methods for:
/// <list type="bullet">
///   <item><description>Retrieving category definitions by category type</description></item>
///   <item><description>Accessing base penalties for categories</description></item>
///   <item><description>Determining weight tiers for penalty reduction</description></item>
///   <item><description>Checking special training requirements</description></item>
///   <item><description>Validating configuration completeness</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorCategoryDefinition"/>
/// <seealso cref="ArmorPenalties"/>
public interface IArmorCategoryProvider
{
    /// <summary>
    /// Gets the definition for a specific armor category.
    /// </summary>
    /// <param name="category">The armor category to retrieve.</param>
    /// <returns>
    /// The <see cref="ArmorCategoryDefinition"/> for the specified category.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no definition is configured for the specified category.
    /// </exception>
    /// <example>
    /// <code>
    /// var heavyDef = _categoryProvider.GetDefinition(ArmorCategory.Heavy);
    /// Console.WriteLine($"{heavyDef.DisplayName}: {heavyDef.Penalties}");
    /// // Output: "Heavy Armor: Agility: -2d10, Stamina: +5, Movement: -10 ft, Stealth: Disadvantage"
    /// </code>
    /// </example>
    ArmorCategoryDefinition GetDefinition(ArmorCategory category);

    /// <summary>
    /// Gets definitions for all armor categories.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="ArmorCategoryDefinition"/> instances.
    /// </returns>
    /// <remarks>
    /// Returns definitions in enum order: Light, Medium, Heavy, Shields, Specialized.
    /// The list should contain exactly 5 definitions.
    /// </remarks>
    /// <example>
    /// <code>
    /// foreach (var def in _categoryProvider.GetAllDefinitions())
    /// {
    ///     Console.WriteLine($"{def.DisplayName}: Tier {def.WeightTier}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ArmorCategoryDefinition> GetAllDefinitions();

    /// <summary>
    /// Gets the base penalties for an armor category.
    /// </summary>
    /// <param name="category">The armor category to retrieve penalties for.</param>
    /// <returns>
    /// The <see cref="ArmorPenalties"/> for the specified category.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to <c>GetDefinition(category).Penalties</c>
    /// but may be more efficient in implementations that cache penalties separately.
    /// </para>
    /// <para>
    /// The returned penalties are base values that may be modified by:
    /// <list type="bullet">
    ///   <item><description>Non-proficiency doubling (2.0x multiplier)</description></item>
    ///   <item><description>Expert/Master tier reduction</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var penalties = _categoryProvider.GetPenalties(ArmorCategory.Medium);
    /// Console.WriteLine($"Agility penalty: {penalties.FormatAgilityPenalty()}");
    /// // Output: "Agility penalty: -1d10"
    /// </code>
    /// </example>
    ArmorPenalties GetPenalties(ArmorCategory category);

    /// <summary>
    /// Gets the weight tier for an armor category.
    /// </summary>
    /// <param name="category">The armor category to retrieve the tier for.</param>
    /// <returns>
    /// The weight tier value (0 for Light, 1 for Medium, 2 for Heavy),
    /// or -1 if the category doesn't use the tier system (Shields).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Weight tiers determine how tier reduction from Expert/Master proficiency applies:
    /// <list type="bullet">
    ///   <item><description>Tier 2 (Heavy) → reduced to Tier 1 (Medium) → reduced to Tier 0 (Light)</description></item>
    ///   <item><description>Tier -1 (Shields) → not affected by tier reduction</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyTier = _categoryProvider.GetWeightTier(ArmorCategory.Heavy);
    /// Console.WriteLine($"Heavy armor weight tier: {heavyTier}");
    /// // Output: "Heavy armor weight tier: 2"
    /// </code>
    /// </example>
    int GetWeightTier(ArmorCategory category);

    /// <summary>
    /// Checks if a category requires special training.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if special training is required; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Special training is distinct from standard proficiency:
    /// <list type="bullet">
    ///   <item><description>Standard proficiency can be gained through combat experience</description></item>
    ///   <item><description>Special training requires faction-specific instruction</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Currently, only <see cref="ArmorCategory.Specialized"/> requires special training.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (_categoryProvider.RequiresSpecialTraining(ArmorCategory.Specialized))
    /// {
    ///     Console.WriteLine("This armor requires special faction training.");
    /// }
    /// </code>
    /// </example>
    bool RequiresSpecialTraining(ArmorCategory category);

    /// <summary>
    /// Gets the display name for a category.
    /// </summary>
    /// <param name="category">The armor category to retrieve the name for.</param>
    /// <returns>
    /// The human-readable display name (e.g., "Heavy Armor", "Shields").
    /// </returns>
    /// <remarks>
    /// This is a convenience method for UI display without loading the full definition.
    /// </remarks>
    /// <example>
    /// <code>
    /// var name = _categoryProvider.GetDisplayName(ArmorCategory.Medium);
    /// Console.WriteLine(name); // Output: "Medium Armor"
    /// </code>
    /// </example>
    string GetDisplayName(ArmorCategory category);

    /// <summary>
    /// Gets categories that use the weight tier system.
    /// </summary>
    /// <returns>
    /// A read-only list of categories that participate in tier reduction.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns Light, Medium, and Heavy categories.
    /// </para>
    /// <para>
    /// Shields and Specialized armor do not use the tier system and are excluded.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// foreach (var category in _categoryProvider.GetTieredCategories())
    /// {
    ///     var tier = _categoryProvider.GetWeightTier(category);
    ///     Console.WriteLine($"{category}: Tier {tier}");
    /// }
    /// // Output:
    /// // Light: Tier 0
    /// // Medium: Tier 1
    /// // Heavy: Tier 2
    /// </code>
    /// </example>
    IReadOnlyList<ArmorCategory> GetTieredCategories();

    /// <summary>
    /// Validates that all categories have definitions.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all 5 armor categories are configured; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called during application startup to verify
    /// configuration completeness before processing game logic.
    /// </para>
    /// <para>
    /// Implementation should log detailed information about any missing
    /// or invalid definitions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!_categoryProvider.ValidateConfiguration())
    /// {
    ///     throw new InvalidOperationException("Armor category configuration is incomplete.");
    /// }
    /// </code>
    /// </example>
    bool ValidateConfiguration();
}
