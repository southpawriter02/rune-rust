// ═══════════════════════════════════════════════════════════════════════════════
// TalentPointDisplay.cs
// Displays the player's talent point allocation status.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the player's talent point allocation status.
/// </summary>
/// <remarks>
/// <para>Shows available and spent points in a clear format:</para>
/// <code>
/// Talent Points: 3 Available | 12 Spent
/// </code>
/// <para>Color coding:</para>
/// <list type="bullet">
///   <item><description>Available points: Yellow when &gt; 0, Gray when 0</description></item>
///   <item><description>Spent points: Gray</description></item>
///   <item><description>Label text: Default terminal color</description></item>
/// </list>
/// <para>Supports animation when points are spent with a brief flash effect.</para>
/// </remarks>
/// <example>
/// <code>
/// var display = new TalentPointDisplay(terminal, config, logger);
/// display.SetPosition(5, 2);
/// display.RenderPoints(3, 12);
/// </code>
/// </example>
public class TalentPointDisplay
{
    private readonly ITerminalService _terminalService;
    private readonly NodeTooltipConfig _config;
    private readonly ILogger<TalentPointDisplay>? _logger;

    private int _availablePoints;
    private int _spentPoints;
    private (int X, int Y) _position;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the TalentPointDisplay component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when terminalService is null.
    /// </exception>
    public TalentPointDisplay(
        ITerminalService terminalService,
        IOptions<NodeTooltipConfig>? config = null,
        ILogger<TalentPointDisplay>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config?.Value ?? NodeTooltipConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("TalentPointDisplay component initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current available points.
    /// </summary>
    public int AvailablePoints => _availablePoints;

    /// <summary>
    /// Gets the current spent points.
    /// </summary>
    public int SpentPoints => _spentPoints;

    /// <summary>
    /// Gets the total points (available + spent).
    /// </summary>
    public int TotalPoints => _availablePoints + _spentPoints;

    // ═══════════════════════════════════════════════════════════════
    // POSITION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the display position.
    /// </summary>
    /// <param name="x">The X coordinate (column).</param>
    /// <param name="y">The Y coordinate (row).</param>
    public void SetPosition(int x, int y)
    {
        _position = (x, y);

        _logger?.LogDebug("Set talent point display position to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Gets the current display position.
    /// </summary>
    /// <returns>A tuple of (X, Y) coordinates.</returns>
    public (int X, int Y) GetPosition() => _position;

    // ═══════════════════════════════════════════════════════════════
    // RENDERING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the talent point display.
    /// </summary>
    /// <param name="available">The number of available points.</param>
    /// <param name="spent">The number of spent points.</param>
    public void RenderPoints(int available, int spent)
    {
        _availablePoints = available;
        _spentPoints = spent;

        // Clear previous display
        var clearText = new string(' ', _config.PointDisplayWidth);
        _terminalService.WriteAt(_position.X, _position.Y, clearText);

        // Render label
        const string label = "Talent Points: ";
        _terminalService.WriteAt(_position.X, _position.Y, label);

        // Render available points with color
        var availableText = $"{available} Available";
        var availableColor = available > 0
            ? _config.AvailablePointsColor
            : _config.SpentPointsColor;

        var availableX = _position.X + label.Length;
        _terminalService.WriteColoredAt(
            availableX,
            _position.Y,
            availableText,
            availableColor);

        // Render separator
        var separatorX = availableX + availableText.Length;
        _terminalService.WriteAt(separatorX, _position.Y, " | ");

        // Render spent points
        var spentText = $"{spent} Spent";
        var spentX = separatorX + 3;
        _terminalService.WriteColoredAt(
            spentX,
            _position.Y,
            spentText,
            _config.SpentPointsColor);

        _logger?.LogDebug(
            "Rendered talent points: {Available} available, {Spent} spent",
            available,
            spent);
    }

    /// <summary>
    /// Updates the available points display.
    /// </summary>
    /// <param name="newAvailable">The new available point count.</param>
    public void UpdateAvailable(int newAvailable)
    {
        var delta = _availablePoints - newAvailable;
        var newSpent = _spentPoints + delta;

        RenderPoints(newAvailable, newSpent);

        _logger?.LogDebug(
            "Updated points: {NewAvailable} available (delta: {Delta})",
            newAvailable, delta);
    }

    /// <summary>
    /// Animates a point spend with a brief flash effect.
    /// </summary>
    /// <remarks>
    /// <para>Animation sequence:</para>
    /// <list type="number">
    ///   <item><description>Available points flash white</description></item>
    ///   <item><description>Brief pause (100ms)</description></item>
    ///   <item><description>Return to normal color</description></item>
    /// </list>
    /// </remarks>
    public void AnimatePointSpend()
    {
        var flashText = $"{_availablePoints} Available";
        var flashX = _position.X + 15; // After "Talent Points: "

        // Flash to white
        _terminalService.WriteColoredAt(
            flashX,
            _position.Y,
            flashText,
            ConsoleColor.White);

        // Brief wait
        Thread.Sleep(100);

        // Return to normal color
        var normalColor = _availablePoints > 0
            ? _config.AvailablePointsColor
            : _config.SpentPointsColor;

        _terminalService.WriteColoredAt(
            flashX,
            _position.Y,
            flashText,
            normalColor);

        _logger?.LogDebug("Animated point spend flash effect");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The tooltip configuration used for display settings.</returns>
    public NodeTooltipConfig GetConfig() => _config;

    /// <summary>
    /// Creates a DTO representing the current point display state.
    /// </summary>
    /// <returns>A talent point display DTO.</returns>
    public TalentPointDisplayDto ToDto() => new(
        Available: _availablePoints,
        Spent: _spentPoints,
        Total: TotalPoints);

    /// <summary>
    /// Clears the talent point display from the screen.
    /// </summary>
    public void Clear()
    {
        var clearText = new string(' ', _config.PointDisplayWidth);
        _terminalService.WriteAt(_position.X, _position.Y, clearText);

        _logger?.LogDebug("Cleared talent point display");
    }
}
