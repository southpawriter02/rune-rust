// ═══════════════════════════════════════════════════════════════════════════════
// AchievementCard.cs
// Renders individual achievement cards with status, progress, and rarity.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders individual achievement cards with status, progress, and rarity.
/// </summary>
/// <remarks>
/// <para>
/// Achievement cards display comprehensive information about each achievement:
/// </para>
/// <list type="bullet">
///   <item><description>Status indicator: [x] unlocked, [~] in progress, ( ) not started, [L] locked secret</description></item>
///   <item><description>Achievement name and description</description></item>
///   <item><description>Rarity badge with point value</description></item>
///   <item><description>Progress bar for incremental achievements</description></item>
/// </list>
/// <para>
/// Secret achievements display placeholder content ("???") until unlocked.
/// </para>
/// </remarks>
/// <example>
/// Card layout:
/// <code>
/// +---------------------------------------------------------------+
/// | [x] FIRST BLOOD                                   Bronze (10) |
/// |   Defeat your first monster                                   |
/// |   [########################################] 100%             |
/// +---------------------------------------------------------------+
/// </code>
/// </example>
public class AchievementCard
{
    private readonly RarityBadge _rarityBadge;
    private readonly ProgressBarRenderer _progressRenderer;
    private readonly ITerminalService _terminalService;
    private readonly AchievementPanelConfig _config;
    private readonly ILogger<AchievementCard> _logger;

    /// <summary>
    /// Creates a new instance of the AchievementCard component.
    /// </summary>
    /// <param name="rarityBadge">The rarity badge renderer.</param>
    /// <param name="progressRenderer">The progress bar renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for card display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public AchievementCard(
        RarityBadge rarityBadge,
        ProgressBarRenderer progressRenderer,
        ITerminalService terminalService,
        AchievementPanelConfig? config = null,
        ILogger<AchievementCard>? logger = null)
    {
        _rarityBadge = rarityBadge ?? throw new ArgumentNullException(nameof(rarityBadge));
        _progressRenderer = progressRenderer ?? throw new ArgumentNullException(nameof(progressRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new AchievementPanelConfig();
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AchievementCard>.Instance;
    }

    /// <summary>
    /// Renders an achievement card at the specified position.
    /// </summary>
    /// <param name="achievement">The achievement display data.</param>
    /// <param name="x">The X coordinate on the terminal.</param>
    /// <param name="y">The Y coordinate on the terminal.</param>
    /// <returns>The Y position after rendering (for layout chaining).</returns>
    public int RenderAchievement(AchievementDisplayDto achievement, int x, int y)
    {
        var cardWidth = _config.CardWidth;
        var currentY = y;

        // Render card top border
        var borderLine = new string('-', cardWidth - 2);
        _terminalService.WriteAt(x, currentY, $"+{borderLine}+");
        currentY++;

        // Check if this is a secret achievement that hasn't been unlocked
        if (achievement.IsSecret && !achievement.IsUnlocked)
        {
            currentY = RenderSecretPlaceholder(achievement.Tier, x, currentY, cardWidth);
        }
        else
        {
            currentY = RenderAchievementContent(achievement, x, currentY, cardWidth);
        }

        // Render card bottom border
        _terminalService.WriteAt(x, currentY, $"+{borderLine}+");
        currentY++;

        _logger.LogDebug(
            "Rendered achievement card: {Name} at ({X}, {Y})",
            achievement.IsSecret && !achievement.IsUnlocked ? "???" : achievement.Name,
            x, y);

        return currentY;
    }

    /// <summary>
    /// Gets the status indicator for an achievement's unlock state.
    /// </summary>
    /// <param name="isUnlocked">Whether the achievement is unlocked.</param>
    /// <param name="hasProgress">Whether there is partial progress.</param>
    /// <returns>
    /// The status indicator string:
    /// <list type="bullet">
    ///   <item><description>"[x]" - Unlocked (completed)</description></item>
    ///   <item><description>"[~]" - In progress (partial completion)</description></item>
    ///   <item><description>"( )" - Not started (no progress)</description></item>
    /// </list>
    /// </returns>
    public string GetStatusIndicator(bool isUnlocked, bool hasProgress)
    {
        if (isUnlocked)
        {
            return "[x]";
        }

        return hasProgress ? "[~]" : "( )";
    }

    /// <summary>
    /// Renders the secret achievement placeholder content.
    /// </summary>
    /// <param name="tier">The achievement tier (for rarity display).</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="cardWidth">The width of the card.</param>
    /// <returns>The Y position after rendering.</returns>
    private int RenderSecretPlaceholder(AchievementTier tier, int x, int y, int cardWidth)
    {
        var currentY = y;
        var rarityBadgeText = _rarityBadge.RenderBadge(tier);

        // Line 1: Locked indicator and hidden name with rarity
        var statusPart = "[L] ??? (Secret Achievement)";
        var padding = cardWidth - 4 - statusPart.Length - rarityBadgeText.Length;
        var line1 = $"| {statusPart}{new string(' ', Math.Max(padding, 1))}{rarityBadgeText} |";
        _terminalService.WriteAt(x, currentY, line1);
        currentY++;

        // Line 2: Hidden description
        var descPlaceholder = "???";
        var line2 = $"|   {descPlaceholder}{new string(' ', cardWidth - 6 - descPlaceholder.Length)} |";
        _terminalService.WriteAt(x, currentY, line2);
        currentY++;

        // Line 3: Locked progress bar
        var lockedBarWidth = _config.ProgressBarWidth;
        var lockedBar = $"[{new string('.', lockedBarWidth)}] Locked";
        var line3 = $"|   {lockedBar}{new string(' ', cardWidth - 6 - lockedBar.Length)} |";
        _terminalService.WriteAt(x, currentY, line3);
        currentY++;

        return currentY;
    }

    /// <summary>
    /// Renders the full achievement content (for non-secret or unlocked achievements).
    /// </summary>
    /// <param name="achievement">The achievement to render.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="cardWidth">The width of the card.</param>
    /// <returns>The Y position after rendering.</returns>
    private int RenderAchievementContent(AchievementDisplayDto achievement, int x, int y, int cardWidth)
    {
        var currentY = y;

        // Line 1: Status indicator, name, and rarity badge
        var statusIndicator = GetStatusIndicator(achievement.IsUnlocked, achievement.CurrentValue > 0);
        var rarityBadgeText = _rarityBadge.RenderBadge(achievement.Tier);
        
        // Calculate available width for name
        var nameWidth = cardWidth - 6 - statusIndicator.Length - rarityBadgeText.Length;
        var displayName = achievement.Name.Length > nameWidth
            ? achievement.Name[..(nameWidth - 3)] + "..."
            : achievement.Name.PadRight(nameWidth);

        var line1 = $"| {statusIndicator} {displayName}{rarityBadgeText} |";
        _terminalService.WriteAt(x, currentY, line1);
        currentY++;

        // Line 2: Description
        var descWidth = cardWidth - 6;
        var displayDesc = achievement.Description.Length > descWidth
            ? achievement.Description[..(descWidth - 3)] + "..."
            : achievement.Description.PadRight(descWidth);

        var line2 = $"|   {displayDesc} |";
        _terminalService.WriteAt(x, currentY, line2);
        currentY++;

        // Line 3: Progress bar
        var progressBarWidth = _config.ProgressBarWidth;
        var progressBar = _progressRenderer.RenderProgressBar(
            achievement.CurrentValue,
            achievement.TargetValue,
            progressBarWidth);

        string progressText;
        if (achievement.IsUnlocked)
        {
            progressText = "100%";
        }
        else if (achievement.TargetValue > 1)
        {
            // Show count and percentage for incremental achievements
            var pct = _progressRenderer.FormatPercentage(achievement.CurrentValue, achievement.TargetValue);
            progressText = $"{achievement.CurrentValue}/{achievement.TargetValue} ({pct})";
        }
        else
        {
            progressText = _progressRenderer.FormatPercentage(achievement.CurrentValue, achievement.TargetValue);
        }

        var line3Content = $"   {progressBar} {progressText}";
        var line3 = $"|{line3Content.PadRight(cardWidth - 3)}|";
        _terminalService.WriteAt(x, currentY, line3);
        currentY++;

        return currentY;
    }
}
