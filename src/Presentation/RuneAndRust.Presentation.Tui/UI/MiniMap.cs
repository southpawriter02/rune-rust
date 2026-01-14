using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders a mini-map of explored rooms in the sidebar.
/// </summary>
/// <remarks>
/// Displays:
/// - Explored rooms (█)
/// - Current room (@)
/// - Known adjacent unexplored rooms (?)
/// - Room connections (─│)
/// </remarks>
public class MiniMap
{
    private readonly IExplorationTracker _exploration;
    private readonly ITerminalService _terminal;
    private readonly ScreenLayout _layout;
    private readonly ILogger<MiniMap>? _logger;
    
    private int _scrollX = 0;
    private int _scrollY = 0;
    
    // Room cell dimensions
    private const int RoomWidth = 5;
    private const int RoomHeight = 3;
    private const int ConnectionLength = 2;
    private const int CellWidth = RoomWidth + ConnectionLength;   // 7
    private const int CellHeight = RoomHeight + 1;                 // 4
    
    // Symbols
    private const char CurrentRoomSymbol = '@';
    private const char ExploredRoomSymbol = '█';
    private const char UnexploredSymbol = '?';
    
    /// <summary>
    /// Initializes a new instance of <see cref="MiniMap"/>.
    /// </summary>
    /// <param name="exploration">Exploration tracker.</param>
    /// <param name="terminal">Terminal service.</param>
    /// <param name="layout">Screen layout.</param>
    /// <param name="logger">Optional logger.</param>
    public MiniMap(
        IExplorationTracker exploration,
        ITerminalService terminal,
        ScreenLayout layout,
        ILogger<MiniMap>? logger = null)
    {
        _exploration = exploration;
        _terminal = terminal;
        _layout = layout;
        _logger = logger;
        
        _exploration.OnRoomExplored += OnRoomExplored;
    }
    
    /// <summary>
    /// Renders the mini-map for a set of room positions.
    /// </summary>
    /// <param name="currentRoomId">The current room ID.</param>
    /// <param name="roomPositions">Room IDs to map positions.</param>
    /// <returns>Rendered map lines.</returns>
    public IReadOnlyList<string> Render(
        Guid currentRoomId,
        IReadOnlyDictionary<Guid, (int X, int Y)> roomPositions)
    {
        var lines = new List<string>();
        
        // Calculate bounds
        var minX = roomPositions.Values.Min(p => p.X);
        var maxX = roomPositions.Values.Max(p => p.X);
        var minY = roomPositions.Values.Min(p => p.Y);
        var maxY = roomPositions.Values.Max(p => p.Y);
        
        // Offset to normalize positions
        var offsetX = -minX;
        var offsetY = -minY;
        
        // Create grid
        var width = (maxX - minX + 1) * CellWidth + 1;
        var height = (maxY - minY + 1) * CellHeight;
        var grid = new char[height, width];
        
        // Fill with spaces
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            grid[y, x] = ' ';
        
        // Render each room
        foreach (var (roomId, pos) in roomPositions)
        {
            var cellX = (pos.X + offsetX) * CellWidth;
            var cellY = (pos.Y + offsetY) * CellHeight;
            
            var isExplored = _exploration.IsExplored(roomId);
            var isKnown = _exploration.KnownAdjacentRooms.Contains(roomId);
            var isCurrent = roomId == currentRoomId;
            
            if (!isExplored && !isKnown && !isCurrent)
                continue;
            
            char symbol;
            if (isCurrent)
                symbol = CurrentRoomSymbol;
            else if (isExplored)
                symbol = ExploredRoomSymbol;
            else
                symbol = UnexploredSymbol;
            
            RenderRoomToGrid(grid, cellX, cellY, symbol);
        }
        
        // Convert grid to lines
        for (var y = 0; y < height; y++)
        {
            var line = new string(Enumerable.Range(0, width)
                .Select(x => grid[y, x]).ToArray()).TrimEnd();
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);
        }
        
        _logger?.LogDebug("Mini-map rendered with {ExploredCount} explored, {AdjacentCount} adjacent",
            _exploration.ExploredRooms.Count, _exploration.KnownAdjacentRooms.Count);
        
        return lines;
    }
    
    /// <summary>
    /// Renders to the sidebar panel.
    /// </summary>
    /// <param name="currentRoomId">Current room ID.</param>
    /// <param name="roomPositions">Room position map.</param>
    public void RenderToPanel(
        Guid currentRoomId, 
        IReadOnlyDictionary<Guid, (int X, int Y)> roomPositions)
    {
        var panel = _layout.GetPanel(PanelPosition.Sidebar);
        if (panel == null)
        {
            _logger?.LogWarning("Sidebar panel not found for mini-map");
            return;
        }
        
        var contentArea = panel.Value.ContentArea;
        
        // Render title
        _terminal.SetCursorPosition(contentArea.X, contentArea.Y);
        var title = "Mini-Map";
        var titlePadding = Math.Max(0, (contentArea.Width - title.Length) / 2);
        _terminal.Write(new string(' ', titlePadding) + title);
        
        // Render map
        var mapLines = Render(currentRoomId, roomPositions);
        
        for (var i = 0; i < mapLines.Count && i < contentArea.Height - 2; i++)
        {
            _terminal.SetCursorPosition(contentArea.X, contentArea.Y + 2 + i);
            var line = mapLines[i];
            if (line.Length > contentArea.Width)
                line = line[..contentArea.Width];
            _terminal.Write(line);
        }
    }
    
    /// <summary>
    /// Scrolls the map view.
    /// </summary>
    /// <param name="dx">Horizontal scroll amount.</param>
    /// <param name="dy">Vertical scroll amount.</param>
    public void Scroll(int dx, int dy)
    {
        _scrollX += dx;
        _scrollY += dy;
    }
    
    /// <summary>
    /// Resets scroll position.
    /// </summary>
    public void ResetScroll()
    {
        _scrollX = 0;
        _scrollY = 0;
    }
    
    /// <summary>
    /// Gets the current scroll position.
    /// </summary>
    public (int X, int Y) ScrollPosition => (_scrollX, _scrollY);
    
    private static void RenderRoomToGrid(char[,] grid, int x, int y, char symbol)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        
        // Check bounds
        if (y < 0 || y + 2 >= height || x < 0 || x + 4 >= width)
            return;
        
        // Top border: ┌───┐
        grid[y, x] = '┌';
        grid[y, x + 1] = '─';
        grid[y, x + 2] = '─';
        grid[y, x + 3] = '─';
        grid[y, x + 4] = '┐';
        
        // Middle: │ X │
        grid[y + 1, x] = '│';
        grid[y + 1, x + 1] = ' ';
        grid[y + 1, x + 2] = symbol;
        grid[y + 1, x + 3] = ' ';
        grid[y + 1, x + 4] = '│';
        
        // Bottom border: └───┘
        grid[y + 2, x] = '└';
        grid[y + 2, x + 1] = '─';
        grid[y + 2, x + 2] = '─';
        grid[y + 2, x + 3] = '─';
        grid[y + 2, x + 4] = '┘';
    }
    
    private void OnRoomExplored(Guid roomId)
    {
        _logger?.LogDebug("Room explored: {RoomId}, triggering map refresh", roomId);
    }
}
