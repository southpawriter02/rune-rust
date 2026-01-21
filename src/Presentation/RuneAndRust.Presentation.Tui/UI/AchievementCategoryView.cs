// ═══════════════════════════════════════════════════════════════════════════════
// AchievementCategoryView.cs
// Renders achievement category sections with headers and achievement cards.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders achievement category sections with headers and achievement cards.
/// </summary>
/// <remarks>
/// <para>
/// Each category section includes:
/// </para>
/// <list type="bullet">
///   <item><description>Category header with icon and colored name</description></item>
///   <item><description>List of achievement cards within the category</description></item>
/// </list>
/// <para>
/// Category icons:
/// </para>
/// <list type="bullet">
///   <item><description>[C] Combat - Red</description></item>
///   <item><description>[E] Exploration - Cyan</description></item>
///   <item><description>[P] Progression - Green</description></item>
///   <item><description>[L] Collection - Yellow</description></item>
///   <item><description>[!] Challenge - Magenta</description></item>
///   <item><description>[?] Secret - DarkGray</description></item>
/// </list>
/// </remarks>
public class AchievementCategoryView
{
    private readonly ITerminalService _terminalService;
    private readonly ILogger<AchievementCategoryView> _logger;

    /// <summary>
    /// Creates a new instance of the AchievementCategoryView component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="terminalService"/> is null.
    /// </exception>
    public AchievementCategoryView(
        ITerminalService terminalService,
        ILogger<AchievementCategoryView>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AchievementCategoryView>.Instance;
    }

    /// <summary>
    /// Renders a category section with its achievements.
    /// </summary>
    /// <param name="category">The achievement category.</param>
    /// <param name="achievements">The list of achievements to display.</param>
    /// <param name="x">The X coordinate on the terminal.</param>
    /// <param name="y">The Y coordinate on the terminal.</param>
    /// <param name="achievementCard">The card renderer for individual achievements.</param>
    /// <returns>The Y position after rendering (for layout chaining).</returns>
    public int RenderCategory(
        AchievementCategory category,
        IReadOnlyList<AchievementDisplayDto> achievements,
        int x,
        int y,
        AchievementCard achievementCard)
    {
        ArgumentNullException.ThrowIfNull(achievements);
        ArgumentNullException.ThrowIfNull(achievementCard);

        var currentY = y;

        // Skip empty categories
        if (achievements.Count == 0)
        {
            _logger.LogDebug("Skipping empty category: {Category}", category);
            return currentY;
        }

        // Render category header
        currentY = RenderCategoryHeader(category, achievements.Count, x, currentY);

        // Render each achievement card
        foreach (var achievement in achievements)
        {
            currentY = achievementCard.RenderAchievement(achievement, x + 2, currentY);
            currentY++; // Add spacing between cards
        }

        _logger.LogDebug(
            "Rendered category {Category} with {Count} achievements",
            category, achievements.Count);

        return currentY;
    }

    /// <summary>
    /// Renders the category header with icon and count.
    /// </summary>
    /// <param name="category">The achievement category.</param>
    /// <param name="achievementCount">The number of achievements in this category.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The Y position after rendering.</returns>
    private int RenderCategoryHeader(AchievementCategory category, int achievementCount, int x, int y)
    {
        var icon = GetCategoryIcon(category);
        var color = GetCategoryColor(category);
        var categoryName = category.ToString();

        // Format: "[C] COMBAT (8 achievements)"
        var headerText = $"{icon} {categoryName.ToUpperInvariant()} ({achievementCount} achievement{(achievementCount != 1 ? "s" : "")})";

        _terminalService.WriteColoredAt(x, y, headerText, color);

        // Add underline
        var underline = new string('─', headerText.Length);
        _terminalService.WriteColoredAt(x, y + 1, underline, color);

        return y + 2;
    }

    /// <summary>
    /// Gets the icon for a category.
    /// </summary>
    /// <param name="category">The achievement category.</param>
    /// <returns>
    /// The category icon string:
    /// <list type="bullet">
    ///   <item><description>[C] - Combat</description></item>
    ///   <item><description>[E] - Exploration</description></item>
    ///   <item><description>[P] - Progression</description></item>
    ///   <item><description>[L] - Collection</description></item>
    ///   <item><description>[!] - Challenge</description></item>
    ///   <item><description>[?] - Secret</description></item>
    /// </list>
    /// </returns>
    public string GetCategoryIcon(AchievementCategory category)
    {
        return category switch
        {
            AchievementCategory.Combat => "[C]",
            AchievementCategory.Exploration => "[E]",
            AchievementCategory.Progression => "[P]",
            AchievementCategory.Collection => "[L]",
            AchievementCategory.Challenge => "[!]",
            AchievementCategory.Secret => "[?]",
            _ => "[*]"
        };
    }

    /// <summary>
    /// Gets the display color for a category.
    /// </summary>
    /// <param name="category">The achievement category.</param>
    /// <returns>
    /// The <see cref="ConsoleColor"/> for the category.
    /// </returns>
    public ConsoleColor GetCategoryColor(AchievementCategory category)
    {
        return category switch
        {
            AchievementCategory.Combat => ConsoleColor.Red,
            AchievementCategory.Exploration => ConsoleColor.Cyan,
            AchievementCategory.Progression => ConsoleColor.Green,
            AchievementCategory.Collection => ConsoleColor.Yellow,
            AchievementCategory.Challenge => ConsoleColor.Magenta,
            AchievementCategory.Secret => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}
