// ═══════════════════════════════════════════════════════════════════════════════
// RecipeDiscoveryNotification.cs
// Displays a notification popup when a new recipe is discovered.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays a notification popup when a new recipe is discovered.
/// </summary>
/// <remarks>
/// <para>Shows a 5-second auto-dismiss popup with the recipe name and
/// crafting station information.</para>
/// <para>Display format:</para>
/// <code>
/// +=====================================================+
/// |  [!]  NEW RECIPE DISCOVERED!                        |
/// +=====================================================+
/// |                                                     |
/// |  STEEL BLADE                                        |
/// |  Forge at: Blacksmith                               |
/// |                                                     |
/// +=====================================================+
/// </code>
/// </remarks>
public class RecipeDiscoveryNotification
{
    private readonly ITerminalService _terminalService;
    private readonly RecipeBrowserConfig _config;
    private readonly ILogger<RecipeDiscoveryNotification> _logger;

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
    /// Creates a new instance of the RecipeDiscoveryNotification component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for notification settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public RecipeDiscoveryNotification(
        ITerminalService terminalService,
        RecipeBrowserConfig? config = null,
        ILogger<RecipeDiscoveryNotification>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? RecipeBrowserConfig.CreateDefault();
        _logger = logger ?? NullLogger<RecipeDiscoveryNotification>.Instance;

        _logger.LogDebug("RecipeDiscoveryNotification initialized");
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
    /// Shows the discovery notification asynchronously with auto-dismiss.
    /// </summary>
    /// <param name="recipe">The discovered recipe.</param>
    public async Task ShowDiscoveryAsync(RecipeDiscoveryDto recipe)
    {
        // Cancel any existing notification
        _dismissCancellation?.Cancel();
        _dismissCancellation = new CancellationTokenSource();

        _isVisible = true;
        RenderNotification(recipe);

        _logger.LogInformation("Recipe discovered: {RecipeName}", recipe.Name);

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
    /// Shows the discovery notification synchronously.
    /// </summary>
    /// <param name="recipe">The discovered recipe.</param>
    public void ShowDiscovery(RecipeDiscoveryDto recipe)
    {
        _isVisible = true;
        RenderNotification(recipe);

        _logger.LogInformation("Recipe discovered: {RecipeName}", recipe.Name);
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

        _logger.LogDebug("Recipe discovery notification hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void RenderNotification(RecipeDiscoveryDto recipe)
    {
        var width = _config.NotificationWidth;
        var x = _notificationPosition.X;
        var y = _notificationPosition.Y;

        // Top border
        var topBorder = $"+{new string('=', width - 2)}+";
        _terminalService.WriteColoredAt(x, y, topBorder, _config.NotificationBorderColor);

        // Header line with [!] indicator
        var header = "  [!]  NEW RECIPE DISCOVERED!";
        var headerPadding = width - header.Length - 2;
        var headerLine = $"|{header}{new string(' ', Math.Max(0, headerPadding))}|";
        _terminalService.WriteColoredAt(x, y + 1, headerLine, _config.NotificationBorderColor);
        _terminalService.WriteColoredAt(x + 3, y + 1, "[!]", _config.NotificationHighlightColor);
        _terminalService.WriteColoredAt(x + 8, y + 1, "NEW RECIPE DISCOVERED!", _config.NotificationHighlightColor);

        // Separator
        var separator = $"+{new string('=', width - 2)}+";
        _terminalService.WriteColoredAt(x, y + 2, separator, _config.NotificationBorderColor);

        // Empty line
        var emptyLine = $"|{new string(' ', width - 2)}|";
        _terminalService.WriteAt(x, y + 3, emptyLine);

        // Recipe name
        var nameLine = $"|  {recipe.Name}";
        var namePadding = width - nameLine.Length - 1;
        _terminalService.WriteAt(x, y + 4, nameLine + new string(' ', Math.Max(0, namePadding)) + "|");
        _terminalService.WriteColoredAt(x + 3, y + 4, recipe.Name, ConsoleColor.White);

        // Station info
        var stationLine = $"|  Forge at: {recipe.StationName}";
        var stationPadding = width - stationLine.Length - 1;
        _terminalService.WriteAt(x, y + 5, stationLine + new string(' ', Math.Max(0, stationPadding)) + "|");

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
