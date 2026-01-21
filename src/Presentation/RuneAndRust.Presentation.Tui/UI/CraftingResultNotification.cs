// ═══════════════════════════════════════════════════════════════════════════════
// CraftingResultNotification.cs
// Displays the crafting result with quality tier after successful crafting.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the crafting result with quality tier after successful crafting.
/// </summary>
/// <remarks>
/// <para>Shows a 5-second auto-dismiss popup with the crafted item name
/// and quality tier using star rating display.</para>
/// <para>Display format:</para>
/// <code>
/// +=====================================================+
/// |  CRAFTING COMPLETE!                                 |
/// +=====================================================+
/// |                                                     |
/// |  Steel Blade                                        |
/// |  Quality: RARE (★★★)                               |
/// |                                                     |
/// +=====================================================+
/// </code>
/// </remarks>
public class CraftingResultNotification
{
    private readonly QualityTierRenderer _qualityRenderer;
    private readonly ITerminalService _terminalService;
    private readonly RecipeBrowserConfig _config;
    private readonly ILogger<CraftingResultNotification> _logger;

    private (int X, int Y) _notificationPosition;
    private bool _isVisible;
    private CancellationTokenSource? _dismissCancellation;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether the notification is currently visible.</summary>
    public bool IsVisible => _isVisible;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the CraftingResultNotification component.
    /// </summary>
    /// <param name="qualityRenderer">The quality tier renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for notification settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public CraftingResultNotification(
        QualityTierRenderer qualityRenderer,
        ITerminalService terminalService,
        RecipeBrowserConfig? config = null,
        ILogger<CraftingResultNotification>? logger = null)
    {
        _qualityRenderer = qualityRenderer ?? throw new ArgumentNullException(nameof(qualityRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? RecipeBrowserConfig.CreateDefault();
        _logger = logger ?? NullLogger<CraftingResultNotification>.Instance;

        _logger.LogDebug("CraftingResultNotification initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the notification position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _notificationPosition = (x, y);
    }

    /// <summary>
    /// Shows the crafting result notification asynchronously with auto-dismiss.
    /// </summary>
    /// <param name="result">The crafting result.</param>
    public async Task ShowResultAsync(CraftingResultDto result)
    {
        // Cancel any existing notification
        _dismissCancellation?.Cancel();
        _dismissCancellation = new CancellationTokenSource();

        _isVisible = true;
        RenderNotification(result);

        _logger.LogInformation(
            "Crafting complete: {ItemName} with quality {Quality}",
            result.ItemName,
            result.Quality);

        // Auto-dismiss after duration
        try
        {
            await Task.Delay(GetDuration(), _dismissCancellation.Token);
            Hide();
        }
        catch (TaskCanceledException)
        {
            // Notification was manually dismissed or replaced
        }
    }

    /// <summary>
    /// Shows the crafting result notification synchronously.
    /// </summary>
    /// <param name="result">The crafting result.</param>
    public void ShowResult(CraftingResultDto result)
    {
        _isVisible = true;
        RenderNotification(result);

        _logger.LogInformation(
            "Crafting complete: {ItemName} with quality {Quality}",
            result.ItemName,
            result.Quality);
    }

    /// <summary>
    /// Shows the quality tier display at a specific position.
    /// </summary>
    /// <param name="quality">The quality tier.</param>
    public void ShowQualityTier(ItemQuality quality)
    {
        var y = _notificationPosition.Y + 5;
        var x = _notificationPosition.X + 3;

        var qualityName = _qualityRenderer.GetQualityName(quality);
        var qualityStars = _qualityRenderer.GetQualityStars(quality);
        var qualityColor = _qualityRenderer.GetQualityColor(quality);

        var qualityText = $"Quality: {qualityName} ({qualityStars})";
        _terminalService.WriteColoredAt(x, y, qualityText, qualityColor);
    }

    /// <summary>
    /// Gets the notification duration.
    /// </summary>
    /// <returns>The duration as a TimeSpan.</returns>
    public TimeSpan GetDuration()
    {
        return TimeSpan.FromSeconds(_config.NotificationDurationSeconds);
    }

    /// <summary>
    /// Hides the notification.
    /// </summary>
    public void Hide()
    {
        if (!_isVisible) return;

        ClearNotification();
        _isVisible = false;
        _dismissCancellation?.Cancel();

        _logger.LogDebug("Crafting result notification hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void RenderNotification(CraftingResultDto result)
    {
        var width = _config.NotificationWidth;
        var x = _notificationPosition.X;
        var y = _notificationPosition.Y;

        // Top border
        var topBorder = $"+{new string('=', width - 2)}+";
        _terminalService.WriteColoredAt(x, y, topBorder, _config.NotificationBorderColor);

        // Header
        var header = "  CRAFTING COMPLETE!";
        var headerPadding = width - header.Length - 2;
        var headerLine = $"|{header}{new string(' ', Math.Max(0, headerPadding))}|";
        _terminalService.WriteColoredAt(x, y + 1, headerLine, _config.NotificationBorderColor);
        _terminalService.WriteColoredAt(x + 3, y + 1, "CRAFTING COMPLETE!", _config.NotificationHighlightColor);

        // Separator
        var separator = $"+{new string('=', width - 2)}+";
        _terminalService.WriteColoredAt(x, y + 2, separator, _config.NotificationBorderColor);

        // Empty line
        var emptyLine = $"|{new string(' ', width - 2)}|";
        _terminalService.WriteAt(x, y + 3, emptyLine);

        // Item name
        var nameLine = $"|  {result.ItemName}";
        var namePadding = width - nameLine.Length - 1;
        _terminalService.WriteAt(x, y + 4, nameLine + new string(' ', Math.Max(0, namePadding)) + "|");
        _terminalService.WriteColoredAt(x + 3, y + 4, result.ItemName, ConsoleColor.White);

        // Quality tier
        var qualityName = _qualityRenderer.GetQualityName(result.Quality);
        var qualityStars = _qualityRenderer.GetQualityStars(result.Quality);
        var qualityColor = _qualityRenderer.GetQualityColor(result.Quality);

        var qualityLine = $"|  Quality: {qualityName} ({qualityStars})";
        var qualityPadding = width - qualityLine.Length - 1;
        _terminalService.WriteAt(x, y + 5, qualityLine + new string(' ', Math.Max(0, qualityPadding)) + "|");
        _terminalService.WriteColoredAt(x + 12, y + 5, $"{qualityName} ({qualityStars})", qualityColor);

        // Empty line
        _terminalService.WriteAt(x, y + 6, emptyLine);

        // Bottom border
        var bottomBorder = $"+{new string('=', width - 2)}+";
        _terminalService.WriteColoredAt(x, y + 7, bottomBorder, _config.NotificationBorderColor);
    }

    private void ClearNotification()
    {
        var width = _config.NotificationWidth;
        var blankLine = new string(' ', width);

        for (var i = 0; i < 8; i++)
        {
            _terminalService.WriteAt(_notificationPosition.X, _notificationPosition.Y + i, blankLine);
        }
    }

    #endregion
}
