using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Manages screen layout with multiple panels.
/// </summary>
/// <remarks>
/// Provides a panel-based layout system that:
/// <list type="bullet">
/// <item><description>Calculates responsive panel dimensions based on terminal size</description></item>
/// <item><description>Renders content to specific panel regions</description></item>
/// <item><description>Draws Unicode/ASCII borders around panels</description></item>
/// <item><description>Handles terminal resize events</description></item>
/// </list>
/// </remarks>
public class ScreenLayout : IDisposable
{
    private readonly ITerminalService _terminal;
    private readonly ILogger<ScreenLayout> _logger;
    private readonly Dictionary<PanelPosition, Panel> _panels = new();
    
    private bool _isBelowMinimumSize;
    private bool _disposed;
    
    /// <summary>
    /// Minimum required terminal size for proper layout.
    /// </summary>
    public static (int Width, int Height) MinimumSize => (80, 24);
    
    /// <summary>
    /// Gets whether the current terminal size is below minimum.
    /// </summary>
    public bool IsBelowMinimumSize => _isBelowMinimumSize;
    
    /// <summary>
    /// Event raised when the layout is recalculated.
    /// </summary>
    public event Action? OnLayoutChanged;
    
    /// <summary>
    /// Initializes a new instance of <see cref="ScreenLayout"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for output operations.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public ScreenLayout(ITerminalService terminal, ILogger<ScreenLayout> logger)
    {
        _terminal = terminal;
        _logger = logger;
        
        _terminal.OnResize += HandleResize;
        RecalculateLayout();
    }
    
    /// <summary>
    /// Gets a panel by position.
    /// </summary>
    /// <param name="position">The panel position.</param>
    /// <returns>The panel, or null if not defined.</returns>
    public Panel? GetPanel(PanelPosition position)
    {
        return _panels.TryGetValue(position, out var panel) ? panel : null;
    }
    
    /// <summary>
    /// Gets all defined panels.
    /// </summary>
    public IReadOnlyDictionary<PanelPosition, Panel> Panels => _panels;
    
    /// <summary>
    /// Renders content lines to a specific panel.
    /// </summary>
    /// <param name="position">The target panel position.</param>
    /// <param name="lines">The lines to render.</param>
    public void RenderToPanel(PanelPosition position, IReadOnlyList<string> lines)
    {
        if (!_panels.TryGetValue(position, out var panel))
        {
            _logger.LogWarning("No panel defined for position {Position}", position);
            return;
        }
        
        var content = panel.ContentArea;
        
        for (var i = 0; i < Math.Min(lines.Count, content.Height); i++)
        {
            _terminal.SetCursorPosition(content.X, content.Y + i);
            
            var line = lines[i];
            if (line.Length > content.Width)
            {
                line = line[..content.Width];
            }
            else if (line.Length < content.Width)
            {
                line = line.PadRight(content.Width);
            }
            
            _terminal.Write(line);
        }
        
        // Clear remaining lines
        for (var i = lines.Count; i < content.Height; i++)
        {
            _terminal.SetCursorPosition(content.X, content.Y + i);
            _terminal.Write(new string(' ', content.Width));
        }
    }
    
    /// <summary>
    /// Clears a panel's content area.
    /// </summary>
    /// <param name="position">The panel to clear.</param>
    public void ClearPanel(PanelPosition position)
    {
        if (!_panels.TryGetValue(position, out var panel))
            return;
        
        var content = panel.ContentArea;
        _terminal.ClearRegion(content.X, content.Y, content.Width, content.Height);
    }
    
    /// <summary>
    /// Recalculates all panel positions based on current terminal size.
    /// </summary>
    public void RecalculateLayout()
    {
        var (width, height) = _terminal.GetSize();
        
        // Check minimum size
        _isBelowMinimumSize = width < MinimumSize.Width || height < MinimumSize.Height;
        
        if (_isBelowMinimumSize)
        {
            _logger.LogWarning("Terminal size {Width}x{Height} is below minimum {MinWidth}x{MinHeight}",
                width, height, MinimumSize.Width, MinimumSize.Height);
        }
        
        _panels.Clear();
        
        // Calculate responsive dimensions
        var sidebarWidth = Math.Min(30, (int)(width * 0.25));
        var mainWidth = width - sidebarWidth - 1; // 1 for separator
        
        const int footerHeight = 3; // Include borders
        const int inputHeight = 1;
        var contentHeight = height - footerHeight - inputHeight;
        
        // Define panels
        _panels[PanelPosition.MainContent] = new Panel(
            PanelPosition.MainContent,
            X: 0,
            Y: 0,
            Width: mainWidth,
            Height: contentHeight,
            Title: null,
            HasBorder: true);
        
        _panels[PanelPosition.Sidebar] = new Panel(
            PanelPosition.Sidebar,
            X: mainWidth,
            Y: 0,
            Width: sidebarWidth + 1,
            Height: contentHeight,
            Title: "Status",
            HasBorder: true);
        
        _panels[PanelPosition.Footer] = new Panel(
            PanelPosition.Footer,
            X: 0,
            Y: contentHeight,
            Width: width,
            Height: footerHeight,
            Title: null,
            HasBorder: true);
        
        _panels[PanelPosition.Input] = new Panel(
            PanelPosition.Input,
            X: 0,
            Y: height - inputHeight,
            Width: width,
            Height: inputHeight,
            Title: null,
            HasBorder: false);
        
        _logger.LogDebug("Layout recalculated: Main={MainW}x{MainH}, Sidebar={SidebarW}, Footer={FooterH}",
            mainWidth, contentHeight, sidebarWidth, footerHeight);
        
        OnLayoutChanged?.Invoke();
    }
    
    /// <summary>
    /// Draws borders for all panels.
    /// </summary>
    public void DrawBorders()
    {
        foreach (var (_, panel) in _panels)
        {
            if (panel.HasBorder)
            {
                DrawPanelBorder(panel);
            }
        }
    }
    
    /// <summary>
    /// Draws border for a single panel.
    /// </summary>
    private void DrawPanelBorder(Panel panel)
    {
        if (panel.Width < 2 || panel.Height < 2)
            return;
            
        var useUnicode = _terminal.SupportsUnicode;
        
        // Box-drawing characters
        var horizontal = useUnicode ? '─' : '-';
        var vertical = useUnicode ? '│' : '|';
        var topLeft = useUnicode ? '┌' : '+';
        var topRight = useUnicode ? '┐' : '+';
        var bottomLeft = useUnicode ? '└' : '+';
        var bottomRight = useUnicode ? '┘' : '+';
        
        // Top border
        _terminal.SetCursorPosition(panel.X, panel.Y);
        _terminal.Write(topLeft.ToString());
        
        if (panel.Title != null && panel.Width > 6)
        {
            var titleSpace = panel.Width - 6;
            var truncatedTitle = panel.Title.Length > titleSpace 
                ? panel.Title[..(titleSpace - 3)] + "..." 
                : panel.Title;
            var leftPadding = (panel.Width - 2 - truncatedTitle.Length - 2) / 2;
            var rightPadding = panel.Width - 2 - leftPadding - truncatedTitle.Length - 2;
            
            _terminal.Write(new string(horizontal, leftPadding));
            _terminal.Write($" {truncatedTitle} ");
            _terminal.Write(new string(horizontal, rightPadding));
        }
        else
        {
            _terminal.Write(new string(horizontal, panel.Width - 2));
        }
        _terminal.Write(topRight.ToString());
        
        // Side borders
        for (var row = 1; row < panel.Height - 1; row++)
        {
            _terminal.SetCursorPosition(panel.X, panel.Y + row);
            _terminal.Write(vertical.ToString());
            _terminal.SetCursorPosition(panel.X + panel.Width - 1, panel.Y + row);
            _terminal.Write(vertical.ToString());
        }
        
        // Bottom border
        _terminal.SetCursorPosition(panel.X, panel.Y + panel.Height - 1);
        _terminal.Write(bottomLeft.ToString());
        _terminal.Write(new string(horizontal, panel.Width - 2));
        _terminal.Write(bottomRight.ToString());
    }
    
    /// <summary>
    /// Handles terminal resize events.
    /// </summary>
    private void HandleResize((int Width, int Height) newSize)
    {
        _logger.LogDebug("Handling resize to {Width}x{Height}", newSize.Width, newSize.Height);
        _terminal.Clear();
        RecalculateLayout();
        DrawBorders();
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _terminal.OnResize -= HandleResize;
        GC.SuppressFinalize(this);
    }
}
