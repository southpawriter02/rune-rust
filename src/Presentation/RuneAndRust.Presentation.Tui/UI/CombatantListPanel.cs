using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders the list of combatants with health and status.
/// </summary>
/// <remarks>
/// Displays:
/// - Turn order with current turn indicator (►)
/// - HP/MP bars per combatant
/// - Status effect icons
/// - Level and class information
/// </remarks>
public class CombatantListPanel
{
    private readonly ITerminalService _terminal;
    private readonly HealthBar _healthBar;
    private readonly ILogger<CombatantListPanel>? _logger;
    
    // Display constants
    private const int CombatantEntryHeight = 3;
    private const char TurnIndicator = '>';
    private const char PlayerSymbol = '@';
    private const char MonsterSymbol = 'M';
    
    /// <summary>
    /// Initializes a new instance of <see cref="CombatantListPanel"/>.
    /// </summary>
    public CombatantListPanel(
        ITerminalService terminal,
        HealthBar healthBar,
        ILogger<CombatantListPanel>? logger = null)
    {
        _terminal = terminal;
        _healthBar = healthBar;
        _logger = logger;
    }
    
    /// <summary>
    /// Renders the combatant list with health bars and status.
    /// </summary>
    /// <param name="combatants">Combatant data (name, hp, mp, isPlayer, isCurrentTurn).</param>
    /// <returns>List of rendered lines.</returns>
    public IReadOnlyList<string> Render(
        IReadOnlyList<(string Name, int CurrentHp, int MaxHp, int CurrentMp, int MaxMp, bool IsPlayer, bool IsCurrentTurn, IReadOnlyList<string> StatusEffects)> combatants)
    {
        var lines = new List<string>();
        lines.Add("─── COMBATANTS ───");
        lines.Add("");
        
        foreach (var c in combatants)
        {
            // Line 1: Turn indicator, symbol, name
            var turnMarker = c.IsCurrentTurn ? TurnIndicator : ' ';
            var symbol = c.IsPlayer ? PlayerSymbol : MonsterSymbol;
            lines.Add($"{turnMarker} {symbol} {c.Name}");
            
            // Line 2: HP bar
            var hpBar = _healthBar.Render(c.CurrentHp, c.MaxHp, 20, BarStyle.Compact);
            lines.Add($"   HP {hpBar} {c.CurrentHp}/{c.MaxHp}");
            
            // Line 3: MP bar (if has mana) or status effects
            if (c.MaxMp > 0)
            {
                var mpBar = _healthBar.Render(c.CurrentMp, c.MaxMp, 20, BarStyle.Compact);
                lines.Add($"   MP {mpBar} {c.CurrentMp}/{c.MaxMp}");
            }
            
            // Status effects
            if (c.StatusEffects.Any())
            {
                var statuses = string.Join(" ", c.StatusEffects.Take(3).Select(s => $"[{s}]"));
                lines.Add($"   {statuses}");
            }
            
            lines.Add("");
        }
        
        // Legend
        lines.Add($"{TurnIndicator} = Current Turn");
        
        _logger?.LogDebug("Rendered combatant list with {Count} combatants", combatants.Count);
        
        return lines;
    }
    
    /// <summary>
    /// Renders to a panel area.
    /// </summary>
    public void RenderToPanel(
        IReadOnlyList<(string Name, int CurrentHp, int MaxHp, int CurrentMp, int MaxMp, bool IsPlayer, bool IsCurrentTurn, IReadOnlyList<string> StatusEffects)> combatants,
        (int X, int Y, int Width, int Height) area)
    {
        var lines = Render(combatants);
        
        for (var i = 0; i < lines.Count && i < area.Height; i++)
        {
            _terminal.SetCursorPosition(area.X, area.Y + i);
            var line = lines[i];
            if (line.Length > area.Width)
                line = line[..area.Width];
            _terminal.Write(line);
        }
    }
}
