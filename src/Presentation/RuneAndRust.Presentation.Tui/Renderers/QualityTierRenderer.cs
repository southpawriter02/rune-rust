// ═══════════════════════════════════════════════════════════════════════════════
// QualityTierRenderer.cs
// Renders quality tier displays with star ratings and colors.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders quality tier displays with star ratings, names, and colors.
/// </summary>
/// <remarks>
/// <para>Provides consistent formatting for quality tiers across all crafting UI.</para>
/// <para>Quality is displayed using star ratings:</para>
/// <list type="table">
///   <listheader>
///     <term>Quality</term>
///     <description>Stars</description>
///   </listheader>
///   <item><term>Common</term><description>★</description></item>
///   <item><term>Uncommon</term><description>★★</description></item>
///   <item><term>Rare</term><description>★★★</description></item>
///   <item><term>Epic</term><description>★★★★</description></item>
///   <item><term>Legendary</term><description>★★★★★</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new QualityTierRenderer(config);
/// var stars = renderer.GetQualityStars(ItemQuality.Rare); // "★★★"
/// var color = renderer.GetQualityColor(ItemQuality.Rare); // ConsoleColor.Blue
/// </code>
/// </example>
public class QualityTierRenderer
{
    private readonly RecipeBrowserConfig _config;

    // Star character used for quality display
    private const char StarChar = '★';

    /// <summary>
    /// Creates a new instance of the QualityTierRenderer.
    /// </summary>
    /// <param name="config">Configuration for quality display settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public QualityTierRenderer(RecipeBrowserConfig? config = null)
    {
        _config = config ?? RecipeBrowserConfig.CreateDefault();
    }

    /// <summary>
    /// Formats the quality display with name and stars.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    /// <returns>The formatted quality string (e.g., "RARE (★★★)").</returns>
    /// <example>
    /// <code>
    /// var formatted = renderer.FormatQuality(ItemQuality.Epic);
    /// // Returns: "EPIC (★★★★)"
    /// </code>
    /// </example>
    public string FormatQuality(ItemQuality quality)
    {
        var name = GetQualityName(quality);
        var stars = GetQualityStars(quality);
        return $"{name} ({stars})";
    }

    /// <summary>
    /// Gets the star rating string for a quality tier.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    /// <returns>The star rating string (1-5 stars).</returns>
    /// <remarks>
    /// The number of stars corresponds to the numeric value of the quality enum.
    /// </remarks>
    /// <example>
    /// <code>
    /// renderer.GetQualityStars(ItemQuality.Common);    // "★"
    /// renderer.GetQualityStars(ItemQuality.Legendary); // "★★★★★"
    /// </code>
    /// </example>
    public string GetQualityStars(ItemQuality quality)
    {
        // The enum value directly represents star count (Common=1, Legendary=5)
        var starCount = (int)quality;

        // Clamp to valid range just in case
        starCount = Math.Clamp(starCount, 1, 5);

        return new string(StarChar, starCount);
    }

    /// <summary>
    /// Gets the display name for a quality tier.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    /// <returns>The quality name in uppercase (e.g., "RARE").</returns>
    public string GetQualityName(ItemQuality quality)
    {
        return quality switch
        {
            ItemQuality.Common => "COMMON",
            ItemQuality.Uncommon => "UNCOMMON",
            ItemQuality.Rare => "RARE",
            ItemQuality.Epic => "EPIC",
            ItemQuality.Legendary => "LEGENDARY",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Gets the console color for a quality tier.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    /// <returns>The console color to use for the quality display.</returns>
    /// <remarks>
    /// <para>Color meanings follow standard RPG conventions:</para>
    /// <list type="bullet">
    ///   <item><description>Gray = Common (basic)</description></item>
    ///   <item><description>Green = Uncommon (improved)</description></item>
    ///   <item><description>Blue = Rare (notable)</description></item>
    ///   <item><description>Magenta = Epic (exceptional)</description></item>
    ///   <item><description>Yellow = Legendary (masterwork)</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetQualityColor(ItemQuality quality)
    {
        return quality switch
        {
            ItemQuality.Common => _config.CommonQualityColor,
            ItemQuality.Uncommon => _config.UncommonQualityColor,
            ItemQuality.Rare => _config.RareQualityColor,
            ItemQuality.Epic => _config.EpicQualityColor,
            ItemQuality.Legendary => _config.LegendaryQualityColor,
            _ => ConsoleColor.White
        };
    }

    /// <summary>
    /// Creates a QualityResultDto from a quality tier.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    /// <returns>A complete QualityResultDto with name and stars.</returns>
    public QualityResultDto CreateQualityResult(ItemQuality quality)
    {
        return new QualityResultDto(
            Quality: quality,
            QualityName: GetQualityName(quality),
            Stars: GetQualityStars(quality));
    }
}
