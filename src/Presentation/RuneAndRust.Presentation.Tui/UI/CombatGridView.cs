using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders the enhanced combat grid view with coordinate labels and cell highlighting.
/// </summary>
/// <remarks>
/// Components:
/// - 8x8 tactical grid with A-H / 1-8 labels
/// - Entity position markers (@, M, A)
/// - Terrain type indicators (., ~, #)
/// - Cell highlighting for movement/attack/ability ranges
/// </remarks>
public class CombatGridView
{
    private readonly ITerminalService _terminal;
    private readonly ScreenLayout _layout;
    private readonly ILogger<CombatGridView>? _logger;
    
    private readonly HashSet<(int X, int Y)> _highlightedCells = new();
    private HighlightType _currentHighlightType = HighlightType.Movement;
    private (int X, int Y)? _selectedCell;
    
    // Grid display constants
    private const int CellWidth = 3;
    private const int GridSize = 8;
    
    // Entity symbols
    private const char PlayerSymbol = '@';
    private const char MonsterSymbol = 'M';
    private const char AllySymbol = 'A';
    
    // Terrain symbols
    private static readonly Dictionary<string, char> TerrainSymbols = new()
    {
        { "open", '.' },
        { "water", '~' },
        { "wall", '#' },
        { "difficult", ',' },
        { "hazard", '^' }
    };
    
    /// <summary>
    /// Initializes a new instance of <see cref="CombatGridView"/>.
    /// </summary>
    public CombatGridView(
        ITerminalService terminal,
        ScreenLayout layout,
        ILogger<CombatGridView>? logger = null)
    {
        _terminal = terminal;
        _layout = layout;
        _logger = logger;
    }
    
    /// <summary>
    /// Renders the combat grid.
    /// </summary>
    /// <param name="gridData">Grid cells as terrain type strings.</param>
    /// <param name="entities">Entities with positions (name, x, y, isPlayer, isAlly).</param>
    /// <returns>Rendered grid lines.</returns>
    public IReadOnlyList<string> RenderGrid(
        string[,] gridData,
        IReadOnlyList<(string Name, int X, int Y, bool IsPlayer, bool IsAlly)> entities)
    {
        var lines = new List<string>();
        var width = gridData.GetLength(0);
        var height = gridData.GetLength(1);
        
        // Column labels (A-H)
        var colLabels = "    " + string.Join(" ", 
            Enumerable.Range(0, Math.Min(width, GridSize)).Select(i => $" {(char)('A' + i)} "));
        lines.Add(colLabels);
        
        // Grid rows
        for (var row = 0; row < Math.Min(height, GridSize); row++)
        {
            // Top border
            if (row == 0)
            {
                lines.Add($"   ┌{string.Join("┬", Enumerable.Repeat("───", Math.Min(width, GridSize)))}┐");
            }
            else
            {
                lines.Add($"   ├{string.Join("┼", Enumerable.Repeat("───", Math.Min(width, GridSize)))}┤");
            }
            
            // Row content with row number
            var rowContent = $" {row + 1} │";
            for (var col = 0; col < Math.Min(width, GridSize); col++)
            {
                var terrain = gridData[col, row];
                var entity = entities.FirstOrDefault(e => e.X == col && e.Y == row);
                
                char symbol;
                if (entity != default)
                {
                    symbol = entity.IsPlayer ? PlayerSymbol : 
                             entity.IsAlly ? AllySymbol : MonsterSymbol;
                }
                else
                {
                    symbol = TerrainSymbols.GetValueOrDefault(terrain?.ToLowerInvariant() ?? "open", '.');
                }
                
                var isHighlighted = _highlightedCells.Contains((col, row));
                var isSelected = _selectedCell == (col, row);
                
                // Mark highlighted/selected cells
                if (isSelected)
                    rowContent += $"[{symbol}]│";
                else if (isHighlighted)
                    rowContent += $"*{symbol}*│";
                else
                    rowContent += $" {symbol} │";
            }
            lines.Add(rowContent);
        }
        
        // Bottom border
        lines.Add($"   └{string.Join("┴", Enumerable.Repeat("───", Math.Min(width, GridSize)))}┘");
        
        // Legend
        lines.Add("");
        lines.Add($"Legend: {PlayerSymbol}=You  {MonsterSymbol}=Monster  {AllySymbol}=Ally  .=Open  ~=Water  #=Wall");
        
        _logger?.LogDebug("Combat grid rendered: {Width}x{Height} with {EntityCount} entities", 
            width, height, entities.Count);
        
        return lines;
    }
    
    /// <summary>
    /// Highlights cells for movement, attack, or ability range.
    /// </summary>
    /// <param name="cells">Cells to highlight.</param>
    /// <param name="type">Type of highlight.</param>
    public void HighlightCells(IEnumerable<(int X, int Y)> cells, HighlightType type)
    {
        _highlightedCells.Clear();
        foreach (var cell in cells)
        {
            _highlightedCells.Add(cell);
        }
        _currentHighlightType = type;
        
        _logger?.LogDebug("Highlighted {Count} cells with type {Type}", 
            _highlightedCells.Count, type);
    }
    
    /// <summary>
    /// Clears all cell highlighting.
    /// </summary>
    public void ClearHighlights()
    {
        _highlightedCells.Clear();
        _selectedCell = null;
    }
    
    /// <summary>
    /// Sets the currently selected cell.
    /// </summary>
    /// <param name="position">Cell position, or null to clear.</param>
    public void SetSelectedCell((int X, int Y)? position)
    {
        _selectedCell = position;
    }
    
    /// <summary>
    /// Gets the current highlight type.
    /// </summary>
    public HighlightType CurrentHighlightType => _currentHighlightType;
    
    /// <summary>
    /// Gets currently highlighted cell positions.
    /// </summary>
    public IReadOnlySet<(int X, int Y)> HighlightedCells => _highlightedCells;
    
    /// <summary>
    /// Gets the selected cell position.
    /// </summary>
    public (int X, int Y)? SelectedCell => _selectedCell;
    
    /// <summary>
    /// Gets the color for a highlight type.
    /// </summary>
    public static ConsoleColor GetHighlightColor(HighlightType type) => type switch
    {
        HighlightType.Movement => ConsoleColor.DarkBlue,
        HighlightType.Attack => ConsoleColor.DarkRed,
        HighlightType.Ability => ConsoleColor.DarkMagenta,
        HighlightType.Threatened => ConsoleColor.DarkYellow,
        HighlightType.Selected => ConsoleColor.Yellow,
        _ => ConsoleColor.Gray
    };
    
    /// <summary>
    /// Renders to the main content panel.
    /// </summary>
    public void RenderToPanel(
        string[,] gridData,
        IReadOnlyList<(string Name, int X, int Y, bool IsPlayer, bool IsAlly)> entities)
    {
        var panel = _layout.GetPanel(PanelPosition.MainContent);
        if (panel == null)
        {
            _logger?.LogWarning("MainContent panel not found for combat grid");
            return;
        }
        
        var lines = RenderGrid(gridData, entities);
        var area = panel.Value.ContentArea;
        
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
