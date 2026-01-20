// ═══════════════════════════════════════════════════════════════════════════════
// CraftedItemQualityExtensions.cs
// Extension methods for the CraftedItemQuality enum providing display names,
// color codes, and sorting utilities for quality tiers.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="CraftedItemQuality"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Provides utility methods for working with crafted item quality tiers including:
/// </para>
/// <list type="bullet">
///   <item><description>Human-readable display names for UI presentation</description></item>
///   <item><description>Hex color codes for visual differentiation</description></item>
///   <item><description>Sort ordering for consistent quality tier ordering</description></item>
/// </list>
/// <para>
/// Color scheme follows common RPG conventions:
/// </para>
/// <list type="bullet">
///   <item><description>Standard (White): Common quality items</description></item>
///   <item><description>Fine (Green): Uncommon quality items</description></item>
///   <item><description>Masterwork (Blue): Rare quality items</description></item>
///   <item><description>Legendary (Orange): Exceptional quality items</description></item>
/// </list>
/// </remarks>
public static class CraftedItemQualityExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Display Name Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a human-readable display name for the quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to get the display name for.</param>
    /// <returns>
    /// A localized display name string suitable for UI presentation.
    /// Returns the enum name as a fallback for unknown values.
    /// </returns>
    /// <example>
    /// <code>
    /// var quality = CraftedItemQuality.Masterwork;
    /// var displayName = quality.GetDisplayName(); // Returns "Masterwork"
    /// </code>
    /// </example>
    public static string GetDisplayName(this CraftedItemQuality quality) => quality switch
    {
        CraftedItemQuality.Standard => "Standard",
        CraftedItemQuality.Fine => "Fine",
        CraftedItemQuality.Masterwork => "Masterwork",
        CraftedItemQuality.Legendary => "Legendary",
        _ => quality.ToString()
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Color Code Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the hex color code associated with the quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to get the color code for.</param>
    /// <returns>
    /// A hex color code string in the format "#RRGGBB" for use in UI styling.
    /// Returns white (#FFFFFF) as a fallback for unknown values.
    /// </returns>
    /// <remarks>
    /// <para>Color codes follow common RPG quality tier conventions:</para>
    /// <list type="bullet">
    ///   <item><description>Standard: #FFFFFF (White) - Common items</description></item>
    ///   <item><description>Fine: #1EFF00 (Green) - Uncommon items</description></item>
    ///   <item><description>Masterwork: #0070DD (Blue) - Rare items</description></item>
    ///   <item><description>Legendary: #FF8000 (Orange) - Exceptional items</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var quality = CraftedItemQuality.Fine;
    /// var color = quality.GetColorCode(); // Returns "#1EFF00"
    /// </code>
    /// </example>
    public static string GetColorCode(this CraftedItemQuality quality) => quality switch
    {
        CraftedItemQuality.Standard => "#FFFFFF",
        CraftedItemQuality.Fine => "#1EFF00",
        CraftedItemQuality.Masterwork => "#0070DD",
        CraftedItemQuality.Legendary => "#FF8000",
        _ => "#FFFFFF"
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Sorting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the sort order value for the quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to get the sort order for.</param>
    /// <returns>
    /// An integer representing the sort order, where lower values appear first.
    /// Standard = 0, Fine = 1, Masterwork = 2, Legendary = 3.
    /// </returns>
    /// <remarks>
    /// The sort order matches the enum's underlying integer value, providing
    /// a natural ordering from lowest quality (Standard) to highest (Legendary).
    /// </remarks>
    /// <example>
    /// <code>
    /// var items = craftedItems.OrderBy(i => i.Quality.GetSortOrder());
    /// </code>
    /// </example>
    public static int GetSortOrder(this CraftedItemQuality quality) => (int)quality;

    // ═══════════════════════════════════════════════════════════════════════════
    // Comparison Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this quality tier is higher than another.
    /// </summary>
    /// <param name="quality">The quality tier to compare.</param>
    /// <param name="other">The quality tier to compare against.</param>
    /// <returns>
    /// <c>true</c> if this quality is strictly higher than the other quality;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var legendary = CraftedItemQuality.Legendary;
    /// var standard = CraftedItemQuality.Standard;
    /// bool isHigher = legendary.IsHigherThan(standard); // Returns true
    /// </code>
    /// </example>
    public static bool IsHigherThan(this CraftedItemQuality quality, CraftedItemQuality other) =>
        quality > other;

    /// <summary>
    /// Determines whether this quality tier is at least as high as another.
    /// </summary>
    /// <param name="quality">The quality tier to compare.</param>
    /// <param name="other">The quality tier to compare against.</param>
    /// <returns>
    /// <c>true</c> if this quality is greater than or equal to the other quality;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var fine = CraftedItemQuality.Fine;
    /// bool meetsThreshold = fine.IsAtLeast(CraftedItemQuality.Fine); // Returns true
    /// </code>
    /// </example>
    public static bool IsAtLeast(this CraftedItemQuality quality, CraftedItemQuality other) =>
        quality >= other;
}
