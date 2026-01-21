// ═══════════════════════════════════════════════════════════════════════════════
// AchievementUnlockNotification.cs
// Renders popup notification when an achievement is unlocked.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Notifications;

/// <summary>
/// Renders popup notification when an achievement is unlocked.
/// </summary>
/// <remarks>
/// <para>
/// Achievement unlock notifications provide immediate feedback to the player
/// when they complete an achievement. The notification includes:
/// </para>
/// <list type="bullet">
///   <item><description>Achievement name and description</description></item>
///   <item><description>Rarity tier and point value</description></item>
///   <item><description>Visual styling based on rarity</description></item>
/// </list>
/// <para>
/// Notifications auto-dismiss after the configured duration (default 5 seconds).
/// </para>
/// </remarks>
/// <example>
/// Notification layout:
/// <code>
/// ╔═══════════════════════════════════════════════════╗
/// ║            ★ ACHIEVEMENT UNLOCKED ★               ║
/// ╠═══════════════════════════════════════════════════╣
/// ║ FIRST BLOOD                          Bronze (10)  ║
/// ║ Defeat your first monster                         ║
/// ║                                                   ║
/// ║             +10 Achievement Points                ║
/// ╚═══════════════════════════════════════════════════╝
/// </code>
/// </example>
public class AchievementUnlockNotification
{
    private readonly UI.RarityBadge _rarityBadge;
    private readonly ITerminalService _terminalService;
    private readonly AchievementPanelConfig _config;
    private readonly ILogger<AchievementUnlockNotification> _logger;

    /// <summary>
    /// Creates a new instance of the AchievementUnlockNotification.
    /// </summary>
    /// <param name="rarityBadge">The rarity badge renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for notification settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required parameters are null.
    /// </exception>
    public AchievementUnlockNotification(
        UI.RarityBadge rarityBadge,
        ITerminalService terminalService,
        AchievementPanelConfig? config = null,
        ILogger<AchievementUnlockNotification>? logger = null)
    {
        _rarityBadge = rarityBadge ?? throw new ArgumentNullException(nameof(rarityBadge));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new AchievementPanelConfig();
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AchievementUnlockNotification>.Instance;
    }

    /// <summary>
    /// Gets the notification display duration in seconds.
    /// </summary>
    public int DurationSeconds => _config.NotificationDurationSeconds;

    /// <summary>
    /// Displays the achievement unlock notification.
    /// </summary>
    /// <param name="achievement">The unlocked achievement.</param>
    /// <param name="centerX">The center X position for the notification.</param>
    /// <param name="topY">The top Y position for the notification.</param>
    public void ShowUnlock(AchievementDisplayDto achievement, int centerX, int topY)
    {
        ArgumentNullException.ThrowIfNull(achievement);

        var width = _config.NotificationWidth;
        var halfWidth = width / 2;
        var startX = centerX - halfWidth;
        var currentY = topY;

        // Get tier color for styling
        var tierColor = _rarityBadge.GetTierColor(achievement.Tier);

        // Top border
        var borderLine = new string('═', width - 2);
        _terminalService.WriteColoredAt(startX, currentY, $"╔{borderLine}╗", tierColor);
        currentY++;

        // Title line: ★ ACHIEVEMENT UNLOCKED ★
        var title = "★ ACHIEVEMENT UNLOCKED ★";
        var titlePadding = (width - 4 - title.Length) / 2;
        var titleLine = $"║{new string(' ', titlePadding)}{title}{new string(' ', width - 3 - titlePadding - title.Length)}║";
        _terminalService.WriteColoredAt(startX, currentY, titleLine, tierColor);
        currentY++;

        // Separator
        _terminalService.WriteColoredAt(startX, currentY, $"╠{borderLine}╣", tierColor);
        currentY++;

        // Achievement name and rarity
        var rarityText = _rarityBadge.RenderBadge(achievement.Tier);
        var nameWidth = width - 6 - rarityText.Length;
        var displayName = achievement.Name.Length > nameWidth
            ? achievement.Name[..(nameWidth - 3)] + "..."
            : achievement.Name.PadRight(nameWidth);
        var nameLine = $"║ {displayName}{rarityText} ║";
        _terminalService.WriteAt(startX, currentY, nameLine);
        currentY++;

        // Description
        var descWidth = width - 4;
        var displayDesc = achievement.Description.Length > descWidth
            ? achievement.Description[..(descWidth - 3)] + "..."
            : achievement.Description.PadRight(descWidth);
        var descLine = $"║ {displayDesc} ║";
        _terminalService.WriteAt(startX, currentY, descLine);
        currentY++;

        // Empty line
        var emptyLine = $"║{new string(' ', width - 2)}║";
        _terminalService.WriteAt(startX, currentY, emptyLine);
        currentY++;

        // Points earned line
        var pointsText = $"+{achievement.PointValue} Achievement Points";
        var pointsPadding = (width - 4 - pointsText.Length) / 2;
        var pointsLine = $"║{new string(' ', pointsPadding)}{pointsText}{new string(' ', width - 3 - pointsPadding - pointsText.Length)}║";
        _terminalService.WriteColoredAt(startX, currentY, pointsLine, ConsoleColor.Green);
        currentY++;

        // Bottom border
        _terminalService.WriteColoredAt(startX, currentY, $"╚{borderLine}╝", tierColor);

        _logger.LogInformation(
            "Showing achievement unlock notification: {Name} ({Tier}, {Points} pts)",
            achievement.Name, achievement.Tier, achievement.PointValue);
    }

    /// <summary>
    /// Clears the notification area.
    /// </summary>
    /// <param name="centerX">The center X position.</param>
    /// <param name="topY">The top Y position.</param>
    public void Clear(int centerX, int topY)
    {
        var width = _config.NotificationWidth;
        var halfWidth = width / 2;
        var startX = centerX - halfWidth;
        var emptyLine = new string(' ', width);

        // Clear 8 lines (notification height)
        for (var i = 0; i < 8; i++)
        {
            _terminalService.WriteAt(startX, topY + i, emptyLine);
        }

        _logger.LogDebug("Cleared achievement notification area");
    }
}
