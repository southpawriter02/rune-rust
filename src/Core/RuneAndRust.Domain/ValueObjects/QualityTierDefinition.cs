// ═══════════════════════════════════════════════════════════════════════════════
// QualityTierDefinition.cs
// Value object representing a complete quality tier configuration including
// display properties, modifiers, and achievement thresholds.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a complete definition of a crafted item quality tier.
/// </summary>
/// <remarks>
/// <para>
/// This immutable value object combines all aspects of a quality tier:
/// </para>
/// <list type="bullet">
///   <item><description>The quality enum value for identification</description></item>
///   <item><description>Display properties (name, color code) for UI presentation</description></item>
///   <item><description>Modifiers (stat and value multipliers) for item scaling</description></item>
///   <item><description>Achievement thresholds (minimum margin, natural 20 requirement)</description></item>
/// </list>
/// <para>
/// Tier definitions are typically loaded from JSON configuration, allowing
/// game designers to adjust quality tier parameters without code changes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a Masterwork tier definition
/// var masterwork = new QualityTierDefinition
/// {
///     Quality = CraftedItemQuality.Masterwork,
///     DisplayName = "Masterwork",
///     ColorCode = "#0070DD",
///     Modifiers = QualityModifier.Create(1.25m, 2.5m),
///     MinimumMargin = 10,
///     RequiresNatural20 = false
/// };
/// </code>
/// </example>
public readonly record struct QualityTierDefinition
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Identification Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the quality enum value that this definition represents.
    /// </summary>
    /// <value>
    /// The <see cref="CraftedItemQuality"/> enum value for this tier.
    /// </value>
    /// <remarks>
    /// This property serves as the primary identifier for the tier and is used
    /// to look up tier definitions from the quality tier provider.
    /// </remarks>
    public CraftedItemQuality Quality { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this quality tier.
    /// </summary>
    /// <value>
    /// A localized string suitable for UI presentation, such as "Masterwork" or "Legendary".
    /// </value>
    /// <remarks>
    /// While the <see cref="CraftedItemQualityExtensions.GetDisplayName"/> extension
    /// method provides default display names, this property allows for customized
    /// or localized names loaded from configuration.
    /// </remarks>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the hex color code for visual styling of this quality tier.
    /// </summary>
    /// <value>
    /// A hex color code string in the format "#RRGGBB" for use in UI styling.
    /// </value>
    /// <remarks>
    /// <para>
    /// Default color scheme follows common RPG conventions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Standard: #FFFFFF (White)</description></item>
    ///   <item><description>Fine: #1EFF00 (Green)</description></item>
    ///   <item><description>Masterwork: #0070DD (Blue)</description></item>
    ///   <item><description>Legendary: #FF8000 (Orange)</description></item>
    /// </list>
    /// </remarks>
    public string ColorCode { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Modifier Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stat and value modifiers for this quality tier.
    /// </summary>
    /// <value>
    /// A <see cref="QualityModifier"/> containing the stat and value multipliers
    /// applied to items of this quality.
    /// </value>
    /// <remarks>
    /// <para>
    /// The modifiers determine how much item stats and values are scaled:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>StatMultiplier: Applied to attack, defense, and other numeric stats</description></item>
    ///   <item><description>ValueMultiplier: Applied to gold/currency value</description></item>
    /// </list>
    /// </remarks>
    public QualityModifier Modifiers { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Threshold Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the minimum roll margin required to achieve this quality tier.
    /// </summary>
    /// <value>
    /// The minimum difference between the crafting roll result and the DC
    /// required for this quality. A value of 0 means no margin requirement.
    /// </value>
    /// <remarks>
    /// <para>
    /// The margin is calculated as: rollResult - difficultyClass.
    /// </para>
    /// <para>
    /// Default thresholds:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Standard: Margin 0+ (any successful roll)</description></item>
    ///   <item><description>Fine: Margin 5+ (beat DC by 5 or more)</description></item>
    ///   <item><description>Masterwork: Margin 10+ (beat DC by 10 or more)</description></item>
    ///   <item><description>Legendary: Natural 20 required (margin ignored)</description></item>
    /// </list>
    /// </remarks>
    public int MinimumMargin { get; init; }

    /// <summary>
    /// Gets a value indicating whether this quality tier requires a natural 20 roll.
    /// </summary>
    /// <value>
    /// <c>true</c> if achieving this quality tier requires rolling a natural 20
    /// on the crafting check; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// When this property is <c>true</c>, the <see cref="MinimumMargin"/> property
    /// is ignored, and the quality can only be achieved by rolling a natural 20.
    /// This is typically used for the Legendary quality tier.
    /// </remarks>
    public bool RequiresNatural20 { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a Standard quality tier definition with default values.
    /// </summary>
    /// <returns>
    /// A <see cref="QualityTierDefinition"/> configured for Standard quality
    /// with 1.0x multipliers and no margin requirement.
    /// </returns>
    /// <remarks>
    /// This factory method provides a fallback/default tier definition when
    /// configuration is unavailable or for testing purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var standardTier = QualityTierDefinition.CreateStandard();
    /// // Quality = Standard, StatMultiplier = 1.0, ValueMultiplier = 1.0
    /// </code>
    /// </example>
    public static QualityTierDefinition CreateStandard() => new()
    {
        Quality = CraftedItemQuality.Standard,
        DisplayName = "Standard",
        ColorCode = "#FFFFFF",
        Modifiers = QualityModifier.Create(1.0m, 1.0m),
        MinimumMargin = 0,
        RequiresNatural20 = false
    };

    /// <summary>
    /// Creates a Fine quality tier definition with default values.
    /// </summary>
    /// <returns>
    /// A <see cref="QualityTierDefinition"/> configured for Fine quality
    /// with +10% stat multiplier and +50% value multiplier.
    /// </returns>
    /// <example>
    /// <code>
    /// var fineTier = QualityTierDefinition.CreateFine();
    /// // Quality = Fine, StatMultiplier = 1.10, ValueMultiplier = 1.5
    /// </code>
    /// </example>
    public static QualityTierDefinition CreateFine() => new()
    {
        Quality = CraftedItemQuality.Fine,
        DisplayName = "Fine",
        ColorCode = "#1EFF00",
        Modifiers = QualityModifier.Create(1.10m, 1.5m),
        MinimumMargin = 5,
        RequiresNatural20 = false
    };

    /// <summary>
    /// Creates a Masterwork quality tier definition with default values.
    /// </summary>
    /// <returns>
    /// A <see cref="QualityTierDefinition"/> configured for Masterwork quality
    /// with +25% stat multiplier and +150% value multiplier.
    /// </returns>
    /// <example>
    /// <code>
    /// var masterworkTier = QualityTierDefinition.CreateMasterwork();
    /// // Quality = Masterwork, StatMultiplier = 1.25, ValueMultiplier = 2.5
    /// </code>
    /// </example>
    public static QualityTierDefinition CreateMasterwork() => new()
    {
        Quality = CraftedItemQuality.Masterwork,
        DisplayName = "Masterwork",
        ColorCode = "#0070DD",
        Modifiers = QualityModifier.Create(1.25m, 2.5m),
        MinimumMargin = 10,
        RequiresNatural20 = false
    };

    /// <summary>
    /// Creates a Legendary quality tier definition with default values.
    /// </summary>
    /// <returns>
    /// A <see cref="QualityTierDefinition"/> configured for Legendary quality
    /// with +50% stat multiplier and +400% value multiplier, requiring a natural 20.
    /// </returns>
    /// <example>
    /// <code>
    /// var legendaryTier = QualityTierDefinition.CreateLegendary();
    /// // Quality = Legendary, StatMultiplier = 1.50, ValueMultiplier = 5.0
    /// </code>
    /// </example>
    public static QualityTierDefinition CreateLegendary() => new()
    {
        Quality = CraftedItemQuality.Legendary,
        DisplayName = "Legendary",
        ColorCode = "#FF8000",
        Modifiers = QualityModifier.Create(1.50m, 5.0m),
        MinimumMargin = 0,
        RequiresNatural20 = true
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this tier can be achieved with the given roll parameters.
    /// </summary>
    /// <param name="rollMargin">
    /// The margin by which the roll exceeded the DC (rollResult - DC).
    /// </param>
    /// <param name="isNatural20">
    /// <c>true</c> if the roll was a natural 20; otherwise, <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the roll parameters meet the requirements for this tier;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks both conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>If <see cref="RequiresNatural20"/> is true, <paramref name="isNatural20"/> must be true</description></item>
    ///   <item><description>If not requiring natural 20, the <paramref name="rollMargin"/> must be at least <see cref="MinimumMargin"/></description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var masterwork = QualityTierDefinition.CreateMasterwork();
    /// bool achieved = masterwork.CanBeAchieved(12, false); // true (margin 12 is at least 10)
    /// bool failed = masterwork.CanBeAchieved(8, false);    // false (margin 8 is less than 10)
    /// </code>
    /// </example>
    public bool CanBeAchieved(int rollMargin, bool isNatural20)
    {
        // If this tier requires a natural 20, check that condition
        if (RequiresNatural20)
        {
            return isNatural20;
        }

        // Otherwise, check the margin requirement
        return rollMargin >= MinimumMargin;
    }

    /// <summary>
    /// Returns a string representation of this tier definition for debugging purposes.
    /// </summary>
    /// <returns>
    /// A string containing the quality name and key properties.
    /// </returns>
    /// <example>
    /// <code>
    /// var tier = QualityTierDefinition.CreateMasterwork();
    /// Console.WriteLine(tier.ToDebugString());
    /// // Output: "QualityTierDefinition { Quality: Masterwork, StatMultiplier: 1.25x, MinMargin: 10 }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"QualityTierDefinition {{ Quality: {Quality}, StatMultiplier: {Modifiers.StatMultiplier:F2}x, " +
        $"MinMargin: {MinimumMargin}{(RequiresNatural20 ? ", RequiresNat20" : "")} }}";
}
