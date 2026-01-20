namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Quality tiers for resources affecting their value and crafting outcomes.
/// </summary>
/// <remarks>
/// <para>
/// Higher quality resources are rarer, more valuable, and provide bonuses
/// when used in crafting. Quality affects both the sale value and the
/// potential quality of crafted items.
/// </para>
/// <para>
/// Quality tier benefits:
/// <list type="bullet">
///   <item><description>Common - Base value, no bonuses (most common)</description></item>
///   <item><description>Fine - 1.5x value, +1 crafting bonus</description></item>
///   <item><description>Rare - 3x value, +2 crafting bonus</description></item>
///   <item><description>Legendary - 10x value, +3 crafting bonus (extremely rare)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ResourceQuality
{
    /// <summary>
    /// Standard quality resources. No bonuses.
    /// Most commonly found through basic gathering.
    /// </summary>
    Common,

    /// <summary>
    /// Above-average quality. +1 crafting bonus, 1.5x value.
    /// Found through skilled gathering or in better locations.
    /// </summary>
    Fine,

    /// <summary>
    /// High quality, valuable resources. +2 crafting bonus, 3x value.
    /// Found in difficult locations or from challenging creatures.
    /// </summary>
    Rare,

    /// <summary>
    /// Exceptional, legendary quality. +3 crafting bonus, 10x value.
    /// Extremely rare, often from unique sources.
    /// </summary>
    Legendary
}

/// <summary>
/// Extension methods for <see cref="ResourceQuality"/>.
/// </summary>
/// <remarks>
/// Provides utility methods for calculating value multipliers, crafting bonuses,
/// and display information for resource quality tiers.
/// </remarks>
public static class ResourceQualityExtensions
{
    /// <summary>
    /// Gets the value multiplier for this quality tier.
    /// </summary>
    /// <param name="quality">The resource quality.</param>
    /// <returns>The multiplier applied to base value.</returns>
    /// <remarks>
    /// <para>
    /// Multipliers by quality tier:
    /// <list type="bullet">
    ///   <item><description>Common: 1.0x (no change)</description></item>
    ///   <item><description>Fine: 1.5x</description></item>
    ///   <item><description>Rare: 3.0x</description></item>
    ///   <item><description>Legendary: 10.0x</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var multiplier = ResourceQuality.Fine.GetValueMultiplier(); // 1.5f
    /// var actualValue = (int)(baseValue * multiplier);
    /// </code>
    /// </example>
    public static float GetValueMultiplier(this ResourceQuality quality) => quality switch
    {
        ResourceQuality.Common => 1.0f,
        ResourceQuality.Fine => 1.5f,
        ResourceQuality.Rare => 3.0f,
        ResourceQuality.Legendary => 10.0f,
        _ => 1.0f
    };

    /// <summary>
    /// Gets the crafting bonus provided by this quality tier.
    /// </summary>
    /// <param name="quality">The resource quality.</param>
    /// <returns>The bonus added to crafting rolls when using this quality.</returns>
    /// <remarks>
    /// <para>
    /// Crafting bonuses by quality tier:
    /// <list type="bullet">
    ///   <item><description>Common: +0</description></item>
    ///   <item><description>Fine: +1</description></item>
    ///   <item><description>Rare: +2</description></item>
    ///   <item><description>Legendary: +3</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = ResourceQuality.Rare.GetCraftingBonus(); // 2
    /// var craftRoll = diceRoll + skillModifier + bonus;
    /// </code>
    /// </example>
    public static int GetCraftingBonus(this ResourceQuality quality) => quality switch
    {
        ResourceQuality.Common => 0,
        ResourceQuality.Fine => 1,
        ResourceQuality.Rare => 2,
        ResourceQuality.Legendary => 3,
        _ => 0
    };

    /// <summary>
    /// Gets the display color for this quality tier (for UI rendering).
    /// </summary>
    /// <param name="quality">The resource quality.</param>
    /// <returns>A hex color string for display.</returns>
    /// <remarks>
    /// <para>
    /// Colors follow standard RPG rarity conventions:
    /// <list type="bullet">
    ///   <item><description>Common: White (#FFFFFF)</description></item>
    ///   <item><description>Fine: Green (#1EFF00)</description></item>
    ///   <item><description>Rare: Blue (#0070DD)</description></item>
    ///   <item><description>Legendary: Orange (#FF8000)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static string GetDisplayColor(this ResourceQuality quality) => quality switch
    {
        ResourceQuality.Common => "#FFFFFF",    // White
        ResourceQuality.Fine => "#1EFF00",      // Green
        ResourceQuality.Rare => "#0070DD",      // Blue
        ResourceQuality.Legendary => "#FF8000", // Orange
        _ => "#FFFFFF"
    };

    /// <summary>
    /// Gets the display name with proper formatting.
    /// </summary>
    /// <param name="quality">The resource quality.</param>
    /// <returns>Human-readable quality name.</returns>
    /// <example>
    /// <code>
    /// var label = ResourceQuality.Fine.GetDisplayName(); // "Fine"
    /// </code>
    /// </example>
    public static string GetDisplayName(this ResourceQuality quality) => quality switch
    {
        ResourceQuality.Common => "Common",
        ResourceQuality.Fine => "Fine",
        ResourceQuality.Rare => "Rare",
        ResourceQuality.Legendary => "Legendary",
        _ => "Unknown"
    };
}
