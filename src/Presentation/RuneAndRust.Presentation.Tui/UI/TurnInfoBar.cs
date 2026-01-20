using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders the turn information bar at the bottom of combat view.
/// </summary>
/// <remarks>
/// Displays:
/// - Round number
/// - Current turn entity name
/// - Action point availability
/// - Movement remaining
/// - Bonus action availability
/// </remarks>
public class TurnInfoBar
{
    private readonly ITerminalService _terminal;
    private readonly ILogger<TurnInfoBar>? _logger;
    
    // Symbols
    private const char ActionAvailable = '+';
    private const char ActionUsed = 'x';
    
    /// <summary>
    /// Initializes a new instance of <see cref="TurnInfoBar"/>.
    /// </summary>
    public TurnInfoBar(
        ITerminalService terminal,
        ILogger<TurnInfoBar>? logger = null)
    {
        _terminal = terminal;
        _logger = logger;
    }
    
    /// <summary>
    /// Renders the turn information bar.
    /// </summary>
    /// <param name="round">Current combat round.</param>
    /// <param name="turnName">Name of entity whose turn it is.</param>
    /// <param name="isPlayerTurn">Whether it's the player's turn.</param>
    /// <param name="actionAvailable">Whether action is available.</param>
    /// <param name="movementRemaining">Movement points remaining.</param>
    /// <param name="movementMax">Maximum movement points.</param>
    /// <param name="bonusAvailable">Whether bonus action is available.</param>
    /// <param name="hasBonusAction">Whether entity has bonus action capability.</param>
    /// <returns>Rendered turn info string.</returns>
    public string Render(
        int round,
        string turnName,
        bool isPlayerTurn,
        bool actionAvailable,
        int movementRemaining,
        int movementMax,
        bool bonusAvailable = false,
        bool hasBonusAction = false)
    {
        var parts = new List<string>();
        
        // Round number
        parts.Add($"Round {round}");
        
        // Whose turn
        var turnText = isPlayerTurn ? "YOUR TURN" : $"{turnName.ToUpperInvariant()}'S TURN";
        parts.Add(turnText);
        
        // Action availability
        var actionSymbol = actionAvailable ? ActionAvailable : ActionUsed;
        var actionStatus = actionAvailable ? "Available" : "Used";
        parts.Add($"Action: [{actionSymbol}] {actionStatus}");
        
        // Movement remaining
        parts.Add($"Move: {movementRemaining}/{movementMax}");
        
        // Bonus action (if applicable)
        if (hasBonusAction)
        {
            var bonusSymbol = bonusAvailable ? ActionAvailable : ActionUsed;
            parts.Add($"Bonus: [{bonusSymbol}]");
        }
        
        var separator = "  |  ";
        var result = string.Join(separator, parts);
        
        _logger?.LogDebug("Turn info: Round {Round}, {TurnName}", round, turnName);
        
        return result;
    }
    
    /// <summary>
    /// Renders to a panel area.
    /// </summary>
    public void RenderToPanel(
        int round,
        string turnName,
        bool isPlayerTurn,
        bool actionAvailable,
        int movementRemaining,
        int movementMax,
        bool bonusAvailable,
        bool hasBonusAction,
        (int X, int Y, int Width, int Height) area)
    {
        var text = Render(round, turnName, isPlayerTurn, actionAvailable, 
            movementRemaining, movementMax, bonusAvailable, hasBonusAction);
        
        // Center in bar
        var padding = Math.Max(0, (area.Width - text.Length) / 2);
        
        _terminal.SetCursorPosition(area.X + padding, area.Y);
        
        if (text.Length > area.Width)
            text = text[..area.Width];
        
        _terminal.Write(text);
    }
}
