namespace RuneAndRust.Domain.Constants;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Color constants for quality tiers.
/// Provides hex colors and names for consistent styling across TUI and GUI.
/// </summary>
/// <remarks>
/// <para>
/// This static class defines the visual presentation colors for each
/// <see cref="QualityTier"/> level. Colors follow the Norse-inspired
/// naming conventions established in the quality tier system.
/// </para>
/// <para>
/// Color values are hex strings suitable for direct use in CSS, HTML,
/// or UI frameworks. Each tier has an associated color name for
/// accessibility and text-based descriptions.
/// </para>
/// <para>
/// The tier prefixes (e.g., "[Myth-Forged]") are used for item name
/// formatting in drop announcements and inventory displays.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get the color for a legendary item
/// var color = TierColors.GetColor(QualityTier.MythForged);
/// Console.WriteLine($"Legendary items are displayed in {color}"); // "#FFD700"
/// 
/// // Get the tier prefix for display
/// var prefix = TierColors.GetTierPrefix(QualityTier.ClanForged);
/// Console.WriteLine($"{prefix} Dwarven Axe"); // "[Clan-Forged] Dwarven Axe"
/// </code>
/// </example>
/// <seealso cref="QualityTier"/>
public static class TierColors
{
    #region Color Constants

    /// <summary>
    /// Tier 0: Jury-Rigged equipment color (Gray).
    /// </summary>
    /// <value>Hex color string "#808080".</value>
    /// <remarks>
    /// Gray represents makeshift equipment cobbled together from scraps.
    /// This is the lowest tier and most common equipment type.
    /// </remarks>
    public const string JuryRigged = "#808080";

    /// <summary>
    /// Tier 1: Scavenged equipment color (White).
    /// </summary>
    /// <value>Hex color string "#FFFFFF".</value>
    /// <remarks>
    /// White represents common finds that are functional but unremarkable.
    /// Standard equipment found on defeated enemies or in basic containers.
    /// </remarks>
    public const string Scavenged = "#FFFFFF";

    /// <summary>
    /// Tier 2: Clan-Forged equipment color (Green).
    /// </summary>
    /// <value>Hex color string "#00FF00".</value>
    /// <remarks>
    /// Green represents quality craftsmanship from regional clan smiths.
    /// These items show skilled workmanship and often bear clan markings.
    /// </remarks>
    public const string ClanForged = "#00FF00";

    /// <summary>
    /// Tier 3: Optimized equipment color (Purple).
    /// </summary>
    /// <value>Hex color string "#800080".</value>
    /// <remarks>
    /// Purple represents exceptional gear optimized for combat effectiveness.
    /// The best non-legendary equipment available, possibly with minor properties.
    /// </remarks>
    public const string Optimized = "#800080";

    /// <summary>
    /// Tier 4: Myth-Forged/Legendary equipment color (Gold).
    /// </summary>
    /// <value>Hex color string "#FFD700".</value>
    /// <remarks>
    /// Gold represents legendary items of mythic origin with unique properties.
    /// These unique artifacts have special effects that can define a playstyle.
    /// Only one of each myth-forged item can drop per run.
    /// </remarks>
    public const string MythForged = "#FFD700";

    /// <summary>
    /// Border decoration color for legendary announcements.
    /// </summary>
    /// <value>Hex color string "#FFD700" (Gold).</value>
    /// <remarks>
    /// Used for the decorative borders in drop announcement frames.
    /// Matches the Myth-Forged color for visual consistency.
    /// </remarks>
    public const string LegendaryBorder = "#FFD700";

    /// <summary>
    /// Accent color for special effect text.
    /// </summary>
    /// <value>Hex color string "#FFAA00" (Orange-Gold).</value>
    /// <remarks>
    /// Used to highlight special effect names and descriptions.
    /// Slightly different from gold to provide visual contrast.
    /// </remarks>
    public const string EffectAccent = "#FFAA00";

    #endregion

    #region Static Methods

    /// <summary>
    /// Gets the hex color for a quality tier.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <returns>Hex color string (e.g., "#FFD700" for Myth-Forged).</returns>
    /// <example>
    /// <code>
    /// var color = TierColors.GetColor(QualityTier.MythForged);
    /// // Returns "#FFD700"
    /// </code>
    /// </example>
    public static string GetColor(QualityTier tier) =>
        tier switch
        {
            QualityTier.JuryRigged => JuryRigged,
            QualityTier.Scavenged => Scavenged,
            QualityTier.ClanForged => ClanForged,
            QualityTier.Optimized => Optimized,
            QualityTier.MythForged => MythForged,
            _ => Scavenged
        };

    /// <summary>
    /// Gets the hex color by tier index (0-4).
    /// </summary>
    /// <param name="tierIndex">The tier index (0 = Jury-Rigged, 4 = Myth-Forged).</param>
    /// <returns>Hex color string for the specified tier index.</returns>
    /// <remarks>
    /// Tier indices outside the valid range (0-4) default to Scavenged (white).
    /// This method is useful when working with numeric tier values directly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = TierColors.GetColorByIndex(4);
    /// // Returns "#FFD700" (Myth-Forged)
    /// </code>
    /// </example>
    public static string GetColorByIndex(int tierIndex) =>
        tierIndex switch
        {
            0 => JuryRigged,
            1 => Scavenged,
            2 => ClanForged,
            3 => Optimized,
            4 => MythForged,
            _ => Scavenged
        };

    /// <summary>
    /// Gets the display name of a tier's color.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <returns>The color name (e.g., "Gold" for Myth-Forged).</returns>
    /// <remarks>
    /// Color names are useful for text-based descriptions and accessibility.
    /// </remarks>
    /// <example>
    /// <code>
    /// var colorName = TierColors.GetColorName(QualityTier.MythForged);
    /// Console.WriteLine($"Legendary items glow {colorName}"); // "Legendary items glow Gold"
    /// </code>
    /// </example>
    public static string GetColorName(QualityTier tier) =>
        tier switch
        {
            QualityTier.JuryRigged => "Gray",
            QualityTier.Scavenged => "White",
            QualityTier.ClanForged => "Green",
            QualityTier.Optimized => "Purple",
            QualityTier.MythForged => "Gold",
            _ => "White"
        };

    /// <summary>
    /// Gets the tier prefix text with brackets for item name formatting.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <returns>
    /// The tier prefix (e.g., "[Myth-Forged]") or empty string for unknown tiers.
    /// </returns>
    /// <remarks>
    /// Tier prefixes are used when displaying item names to indicate quality.
    /// The prefix is placed before the item name in drop announcements
    /// and inventory displays.
    /// </remarks>
    /// <example>
    /// <code>
    /// var prefix = TierColors.GetTierPrefix(QualityTier.MythForged);
    /// var itemName = "Shadowfang Blade";
    /// Console.WriteLine($"{prefix} {itemName}"); // "[Myth-Forged] Shadowfang Blade"
    /// </code>
    /// </example>
    public static string GetTierPrefix(QualityTier tier) =>
        tier switch
        {
            QualityTier.JuryRigged => "[Jury-Rigged]",
            QualityTier.Scavenged => "[Scavenged]",
            QualityTier.ClanForged => "[Clan-Forged]",
            QualityTier.Optimized => "[Optimized]",
            QualityTier.MythForged => "[Myth-Forged]",
            _ => ""
        };

    /// <summary>
    /// Logs the color retrieval for diagnostic purposes.
    /// </summary>
    /// <param name="tier">The quality tier being queried.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>The hex color string for the specified tier.</returns>
    /// <remarks>
    /// This overload provides logging support for debugging and diagnostics.
    /// Use the non-logging overload for performance-critical paths.
    /// </remarks>
    public static string GetColor(QualityTier tier, ILogger? logger)
    {
        var color = GetColor(tier);

        logger?.LogDebug(
            "Retrieved color {Color} for quality tier {Tier} ({TierName})",
            color,
            tier,
            GetColorName(tier));

        return color;
    }

    #endregion
}
