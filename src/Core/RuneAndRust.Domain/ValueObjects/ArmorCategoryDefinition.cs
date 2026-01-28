// ═══════════════════════════════════════════════════════════════════════════════
// ArmorCategoryDefinition.cs
// Value object containing metadata for an armor category.
// Version: 0.16.2b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains metadata for an armor category.
/// </summary>
/// <remarks>
/// <para>
/// ArmorCategoryDefinition is loaded from configuration (armor-categories.json)
/// and provides all display and gameplay information for an armor category.
/// </para>
/// <para>
/// Each definition includes:
/// <list type="bullet">
///   <item><description>Display name and flavor description</description></item>
///   <item><description>Example armor types in the category</description></item>
///   <item><description>Base penalties (agility, stamina, movement, stealth)</description></item>
///   <item><description>Weight tier for proficiency tier reduction</description></item>
///   <item><description>Whether special training is required</description></item>
/// </list>
/// </para>
/// <para>
/// The weight tier system enables penalty reduction for Expert/Master proficiency:
/// <list type="bullet">
///   <item><description>Tier 0 (Light): No penalties to reduce</description></item>
///   <item><description>Tier 1 (Medium): Can be reduced to Tier 0 (Light penalties)</description></item>
///   <item><description>Tier 2 (Heavy): Can be reduced to Tier 1 (Medium penalties)</description></item>
///   <item><description>Tier -1 (Shields): Not part of the tier reduction system</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Category">The armor category this definition describes.</param>
/// <param name="DisplayName">Human-readable category name for UI display.</param>
/// <param name="Description">Flavor text describing the category and its characteristics.</param>
/// <param name="ExampleArmors">List of example armor names in this category.</param>
/// <param name="Penalties">Base penalties for this category (modified by proficiency).</param>
/// <param name="WeightTier">Tier for penalty reduction (0=Light, 1=Medium, 2=Heavy, -1=N/A).</param>
/// <param name="RequiresSpecialTraining">Whether extra training is needed beyond standard proficiency.</param>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorPenalties"/>
public readonly record struct ArmorCategoryDefinition(
    ArmorCategory Category,
    string DisplayName,
    string Description,
    IReadOnlyList<string> ExampleArmors,
    ArmorPenalties Penalties,
    int WeightTier,
    bool RequiresSpecialTraining)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the category as an integer value.
    /// </summary>
    /// <value>
    /// The integer representation of <see cref="Category"/> (0-4).
    /// </value>
    /// <remarks>
    /// Useful for serialization, comparison, and configuration file compatibility.
    /// </remarks>
    public int CategoryValue => (int)Category;

    /// <summary>
    /// Gets whether this is light armor (no penalties).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Category"/> equals <see cref="ArmorCategory.Light"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Light armor has no penalties and is the baseline for tier reduction.
    /// </remarks>
    public bool IsLight => Category == ArmorCategory.Light;

    /// <summary>
    /// Gets whether this is medium armor.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Category"/> equals <see cref="ArmorCategory.Medium"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Medium armor provides balanced protection with moderate penalties.
    /// </remarks>
    public bool IsMedium => Category == ArmorCategory.Medium;

    /// <summary>
    /// Gets whether this is heavy armor.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Category"/> equals <see cref="ArmorCategory.Heavy"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Heavy armor provides maximum protection with severe penalties and blocks
    /// Galdr casting for Mystics.
    /// </remarks>
    public bool IsHeavy => Category == ArmorCategory.Heavy;

    /// <summary>
    /// Gets whether this is a shield.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Category"/> equals <see cref="ArmorCategory.Shields"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Shields are held defensive equipment with minimal penalties and are not
    /// affected by the tier reduction system.
    /// </remarks>
    public bool IsShield => Category == ArmorCategory.Shields;

    /// <summary>
    /// Gets whether this is specialized armor requiring special training.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Category"/> equals <see cref="ArmorCategory.Specialized"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Specialized armor includes exotic equipment like Servitor shells and
    /// symbiotic carapaces that require faction-specific training.
    /// </remarks>
    public bool IsSpecialized => Category == ArmorCategory.Specialized;

    /// <summary>
    /// Gets whether this category uses the weight tier system.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is Light, Medium, or Heavy; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Only Light, Medium, and Heavy armor participate in the tier reduction system.
    /// </para>
    /// <para>
    /// Shields and Specialized armor do not use this system:
    /// <list type="bullet">
    ///   <item><description>Shields have WeightTier = -1</description></item>
    ///   <item><description>Specialized armor has variable penalties per item</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool UsesWeightTierSystem =>
        Category is ArmorCategory.Light or ArmorCategory.Medium or ArmorCategory.Heavy;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates ArmorCategoryDefinition with validation.
    /// </summary>
    /// <param name="category">The armor category to define.</param>
    /// <param name="displayName">Human-readable name (non-empty).</param>
    /// <param name="description">Flavor text description (non-empty).</param>
    /// <param name="exampleArmors">List of example armor names (non-null).</param>
    /// <param name="penalties">Base penalties for this category.</param>
    /// <param name="weightTier">Weight tier for tier reduction (-1 to 3).</param>
    /// <param name="requiresSpecialTraining">Whether special training is required.</param>
    /// <returns>A new validated <see cref="ArmorCategoryDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> or <paramref name="description"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exampleArmors"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="weightTier"/> is outside the valid range (-1 to 3).
    /// </exception>
    /// <example>
    /// <code>
    /// var heavyDef = ArmorCategoryDefinition.Create(
    ///     ArmorCategory.Heavy,
    ///     "Heavy Armor",
    ///     "Maximum protection at the cost of mobility.",
    ///     new[] { "Half Plate", "Full Plate", "Fortress Armor" },
    ///     ArmorPenalties.Create(-2, 5, -10, true),
    ///     weightTier: 2,
    ///     requiresSpecialTraining: false);
    /// </code>
    /// </example>
    public static ArmorCategoryDefinition Create(
        ArmorCategory category,
        string displayName,
        string description,
        IReadOnlyList<string> exampleArmors,
        ArmorPenalties penalties,
        int weightTier,
        bool requiresSpecialTraining)
    {
        // Validate string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(exampleArmors);

        // Validate weight tier range (-1 to 3)
        ArgumentOutOfRangeException.ThrowIfLessThan(weightTier, -1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weightTier, 3);

        return new ArmorCategoryDefinition(
            category,
            displayName,
            description,
            exampleArmors,
            penalties,
            weightTier,
            requiresSpecialTraining);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats example armors as a comma-separated string.
    /// </summary>
    /// <returns>
    /// A comma-separated list of example armor names.
    /// </returns>
    /// <remarks>
    /// Useful for displaying examples in tooltips and UI panels.
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyDef = ... // Heavy armor definition
    /// Console.WriteLine(heavyDef.FormatExamples());
    /// // Output: "Half Plate, Full Plate, Fortress Armor"
    /// </code>
    /// </example>
    public string FormatExamples() => string.Join(", ", ExampleArmors);

    /// <summary>
    /// Creates a display string for debug/logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the category name, tier, and training requirement.
    /// </returns>
    /// <remarks>
    /// Provides a compact representation suitable for logging and debugging.
    /// </remarks>
    /// <example>
    /// <code>
    /// var specializedDef = ... // Specialized armor definition
    /// Console.WriteLine(specializedDef.ToString());
    /// // Output: "Specialized Armor (Tier 1) [Special Training]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{DisplayName} (Tier {WeightTier})" +
        (RequiresSpecialTraining ? " [Special Training]" : "");

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var heavyDef = ... // Heavy armor definition
    /// Console.WriteLine(heavyDef.ToDebugString());
    /// // Output: "ArmorCategoryDefinition { Category: Heavy (2), Tier: 2, Penalties: Agility: -2d10, ..., SpecialTraining: False }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"ArmorCategoryDefinition {{ Category: {Category} ({CategoryValue}), Tier: {WeightTier}, " +
        $"Penalties: {Penalties}, SpecialTraining: {RequiresSpecialTraining} }}";
}
