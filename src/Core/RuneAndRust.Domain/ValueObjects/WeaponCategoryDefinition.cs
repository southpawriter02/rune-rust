// ═══════════════════════════════════════════════════════════════════════════════
// WeaponCategoryDefinition.cs
// Value object containing metadata for a weapon category.
// Version: 0.16.1b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains metadata for a weapon category.
/// </summary>
/// <remarks>
/// <para>
/// WeaponCategoryDefinition is loaded from configuration (weapon-categories.json)
/// and provides all display and gameplay information for a weapon category.
/// </para>
/// <para>
/// This value object is immutable after creation. Use the factory method
/// <see cref="Create"/> to construct new instances with validation.
/// </para>
/// <para>
/// Each definition contains:
/// </para>
/// <list type="bullet">
///   <item><description>Display information (name, description, examples)</description></item>
///   <item><description>Primary attribute for attack/damage scaling</description></item>
///   <item><description>Special training requirements (Firearms only)</description></item>
/// </list>
/// <para>
/// Category to attribute mapping:
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
/// <param name="Category">The weapon category this definition describes.</param>
/// <param name="DisplayName">Human-readable category name.</param>
/// <param name="Description">Flavor text describing the category.</param>
/// <param name="ExampleWeapons">List of example weapon names.</param>
/// <param name="PrimaryAttribute">The attribute used for attack/damage calculations.</param>
/// <param name="RequiresSpecialTraining">Whether extra training beyond archetype defaults is needed.</param>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="AttributeType"/>
/// <seealso href="IWeaponCategoryProvider">IWeaponCategoryProvider interface in Application layer</seealso>
public readonly record struct WeaponCategoryDefinition(
    WeaponCategory Category,
    string DisplayName,
    string Description,
    IReadOnlyList<string> ExampleWeapons,
    AttributeType PrimaryAttribute,
    bool RequiresSpecialTraining)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the category as an integer value.
    /// </summary>
    /// <value>
    /// The integer representation of <see cref="Category"/> (0-10).
    /// </value>
    /// <remarks>
    /// Useful for serialization, comparison, and display purposes.
    /// Values range from 0 (Axes) to 10 (ArcaneImplements).
    /// </remarks>
    public int CategoryValue => (int)Category;

    /// <summary>
    /// Gets a value indicating whether this is a magical weapon category.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PrimaryAttribute"/> is <see cref="AttributeType.Will"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Magical categories include Staves and ArcaneImplements. These weapons
    /// enhance spellcasting and Galdr abilities.
    /// </remarks>
    public bool IsMagical => PrimaryAttribute == AttributeType.Will;

    /// <summary>
    /// Gets a value indicating whether this is a ranged weapon category.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is Bows, Crossbows, or Firearms;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Ranged categories allow attacks at distance and typically require
    /// ammunition or reload actions.
    /// </remarks>
    public bool IsRanged => Category is WeaponCategory.Bows
        or WeaponCategory.Crossbows
        or WeaponCategory.Firearms;

    /// <summary>
    /// Gets a value indicating whether this is a melee weapon category.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is not ranged and not ArcaneImplements;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Melee categories include physical close-combat weapons:
    /// Axes, Swords, Hammers, Daggers, Polearms, Staves, and Shields.
    /// </para>
    /// <para>
    /// ArcaneImplements are excluded as they are primarily spellcasting foci
    /// rather than physical weapons.
    /// </para>
    /// </remarks>
    public bool IsMelee => !IsRanged && Category != WeaponCategory.ArcaneImplements;

    /// <summary>
    /// Gets a value indicating whether this is a defensive category.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is Shields; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Shields provide defensive bonuses and blocking capabilities
    /// rather than primarily offensive damage.
    /// </remarks>
    public bool IsDefensive => Category == WeaponCategory.Shields;

    /// <summary>
    /// Gets a value indicating whether this category uses MIGHT for attacks.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PrimaryAttribute"/> is <see cref="AttributeType.Might"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// MIGHT-based categories: Axes, Swords, Hammers, Polearms, Shields.
    /// </remarks>
    public bool UsesMight => PrimaryAttribute == AttributeType.Might;

    /// <summary>
    /// Gets a value indicating whether this category uses FINESSE for attacks.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PrimaryAttribute"/> is <see cref="AttributeType.Finesse"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// FINESSE-based categories: Daggers, Bows.
    /// </remarks>
    public bool UsesFinesse => PrimaryAttribute == AttributeType.Finesse;

    /// <summary>
    /// Gets a value indicating whether this category uses WILL for attacks.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PrimaryAttribute"/> is <see cref="AttributeType.Will"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// WILL-based categories: Staves, ArcaneImplements.
    /// </remarks>
    public bool UsesWill => PrimaryAttribute == AttributeType.Will;

    /// <summary>
    /// Gets a value indicating whether this category uses WITS for attacks.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PrimaryAttribute"/> is <see cref="AttributeType.Wits"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// WITS-based categories: Crossbows, Firearms.
    /// </remarks>
    public bool UsesWits => PrimaryAttribute == AttributeType.Wits;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="WeaponCategoryDefinition"/> with validation.
    /// </summary>
    /// <param name="category">The weapon category.</param>
    /// <param name="displayName">Human-readable category name.</param>
    /// <param name="description">Flavor text describing the category.</param>
    /// <param name="exampleWeapons">List of example weapon names.</param>
    /// <param name="primaryAttribute">The attribute used for attack/damage.</param>
    /// <param name="requiresSpecialTraining">Whether special training is required.</param>
    /// <returns>A new validated <see cref="WeaponCategoryDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> or <paramref name="description"/>
    /// is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exampleWeapons"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var swordsDefinition = WeaponCategoryDefinition.Create(
    ///     WeaponCategory.Swords,
    ///     "Swords",
    ///     "Versatile bladed weapons balanced for both offense and defense.",
    ///     new[] { "Longsword", "Shortsword", "Greatsword", "Rapier" },
    ///     AttributeType.Might,
    ///     requiresSpecialTraining: false);
    /// </code>
    /// </example>
    public static WeaponCategoryDefinition Create(
        WeaponCategory category,
        string displayName,
        string description,
        IReadOnlyList<string> exampleWeapons,
        AttributeType primaryAttribute,
        bool requiresSpecialTraining)
    {
        // Validate string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(exampleWeapons);

        return new WeaponCategoryDefinition(
            category,
            displayName,
            description,
            exampleWeapons,
            primaryAttribute,
            requiresSpecialTraining);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats example weapons as a comma-separated string.
    /// </summary>
    /// <returns>
    /// A comma-separated list of example weapons, e.g., "Longsword, Shortsword, Greatsword".
    /// </returns>
    /// <remarks>
    /// Useful for displaying example weapons in tooltips or UI elements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = GetSwordsDefinition();
    /// Console.WriteLine(definition.FormatExamples());
    /// // Output: "Longsword, Shortsword, Greatsword, Rapier"
    /// </code>
    /// </example>
    public string FormatExamples() => string.Join(", ", ExampleWeapons);

    /// <summary>
    /// Formats the primary attribute for display.
    /// </summary>
    /// <returns>
    /// The primary attribute name in uppercase, e.g., "MIGHT", "FINESSE".
    /// </returns>
    /// <remarks>
    /// Uses uppercase convention consistent with game attribute display.
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = GetDaggersDefinition();
    /// Console.WriteLine(definition.FormatPrimaryAttribute()); // Output: "FINESSE"
    /// </code>
    /// </example>
    public string FormatPrimaryAttribute() =>
        PrimaryAttribute.ToString().ToUpperInvariant();

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    /// <returns>
    /// A formatted string showing the display name, primary attribute,
    /// and special training requirement if applicable.
    /// </returns>
    /// <example>
    /// <code>
    /// var definition = GetFirearmsDefinition();
    /// Console.WriteLine(definition.ToString());
    /// // Output: "Firearms (WITS) [Special Training]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{DisplayName} ({FormatPrimaryAttribute()})" +
        (RequiresSpecialTraining ? " [Special Training]" : "");

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var definition = GetAxesDefinition();
    /// Console.WriteLine(definition.ToDebugString());
    /// // Output: "WeaponCategoryDefinition { Category: Axes (0), Attr: MIGHT, Special: False, Examples: 4 }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"WeaponCategoryDefinition {{ Category: {Category} ({CategoryValue}), " +
        $"Attr: {FormatPrimaryAttribute()}, Special: {RequiresSpecialTraining}, " +
        $"Examples: {ExampleWeapons.Count} }}";
}
