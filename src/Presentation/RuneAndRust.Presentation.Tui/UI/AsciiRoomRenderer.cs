using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders ASCII art for rooms with optional entity overlays.
/// </summary>
/// <remarks>
/// Supports:
/// - Custom room art from <see cref="IRoomArtProvider"/>
/// - Symbol-based coloring
/// - Entity overlay on static art
/// - Legend rendering
/// - Default fallback for rooms without custom art
/// </remarks>
public class AsciiRoomRenderer
{
    private readonly IRoomArtProvider _artProvider;
    private readonly ITerminalService _terminal;
    private readonly ScreenLayout _layout;
    private readonly ILogger<AsciiRoomRenderer>? _logger;
    
    /// <summary>
    /// Initializes a new instance of <see cref="AsciiRoomRenderer"/>.
    /// </summary>
    /// <param name="artProvider">Provider for room art definitions.</param>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="layout">Screen layout for panel access.</param>
    /// <param name="logger">Optional logger.</param>
    public AsciiRoomRenderer(
        IRoomArtProvider artProvider,
        ITerminalService terminal,
        ScreenLayout layout,
        ILogger<AsciiRoomRenderer>? logger = null)
    {
        _artProvider = artProvider;
        _terminal = terminal;
        _layout = layout;
        _logger = logger;
    }
    
    /// <summary>
    /// Renders room art by room type ID.
    /// </summary>
    /// <param name="roomTypeId">The room type ID.</param>
    /// <returns>List of rendered lines.</returns>
    public IReadOnlyList<string> RenderRoom(string roomTypeId)
    {
        var art = _artProvider.GetArtForRoom(roomTypeId);
        if (art == null)
        {
            _logger?.LogDebug("No custom art for room '{RoomType}', using default", roomTypeId);
            return CreateDefaultArt(roomTypeId);
        }
        
        return art.ArtLines;
    }
    
    /// <summary>
    /// Renders room art with entity overlays.
    /// </summary>
    /// <param name="roomTypeId">The room type ID.</param>
    /// <param name="entities">Entities to overlay (name, x, y, symbol).</param>
    /// <returns>List of rendered lines with entity overlays.</returns>
    public IReadOnlyList<string> RenderRoomWithEntities(
        string roomTypeId,
        IReadOnlyList<(string Name, int X, int Y, char Symbol)> entities)
    {
        var art = _artProvider.GetArtForRoom(roomTypeId);
        if (art == null)
        {
            return CreateDefaultArt(roomTypeId);
        }
        
        return OverlayEntities(art.ArtLines, entities);
    }
    
    /// <summary>
    /// Renders room art to the main content panel.
    /// </summary>
    /// <param name="roomTypeId">The room type ID.</param>
    public void RenderToPanel(string roomTypeId)
    {
        var panel = _layout.GetPanel(PanelPosition.MainContent);
        if (panel == null)
        {
            _logger?.LogWarning("MainContent panel not found");
            return;
        }
        
        var art = _artProvider.GetArtForRoom(roomTypeId);
        if (art == null)
        {
            _logger?.LogDebug("No custom art for room '{RoomType}', using default", roomTypeId);
            _layout.RenderToPanel(PanelPosition.MainContent, CreateDefaultArt(roomTypeId));
            return;
        }
        
        var contentArea = panel.Value.ContentArea;
        var scaledArt = ScaleArt(art.ArtLines, contentArea.Width);
        
        RenderArtWithColors(scaledArt, art.SymbolColors, contentArea);
        
        if (art.ShowLegend)
        {
            RenderLegend(art.Legend, art.SymbolColors, contentArea);
        }
    }
    
    /// <summary>
    /// Scales art to fit the target width.
    /// </summary>
    /// <param name="art">Art lines to scale.</param>
    /// <param name="targetWidth">Target width in characters.</param>
    /// <returns>Scaled art lines.</returns>
    public IReadOnlyList<string> ScaleArt(IReadOnlyList<string> art, int targetWidth)
    {
        if (art.Count == 0) return art;
        
        var maxWidth = art.Max(line => line.Length);
        
        if (maxWidth <= targetWidth)
        {
            // Center the art
            var padding = (targetWidth - maxWidth) / 2;
            return art.Select(line => new string(' ', padding) + line).ToList();
        }
        
        // Truncate if too wide
        _logger?.LogDebug("Truncating art from {OriginalWidth} to {TargetWidth}", 
            maxWidth, targetWidth);
        return art.Select(line => 
            line.Length > targetWidth ? line[..targetWidth] : line
        ).ToList();
    }
    
    /// <summary>
    /// Overlays entity symbols on the art.
    /// </summary>
    /// <param name="art">Base art lines.</param>
    /// <param name="entities">Entities to overlay.</param>
    /// <returns>Art with entity overlays.</returns>
    public IReadOnlyList<string> OverlayEntities(
        IReadOnlyList<string> art, 
        IReadOnlyList<(string Name, int X, int Y, char Symbol)> entities)
    {
        if (!entities.Any()) return art;
        
        var mutableArt = art.Select(line => line.ToCharArray()).ToList();
        
        foreach (var entity in entities)
        {
            var (_, x, y, symbol) = entity;
            if (y >= 0 && y < mutableArt.Count && 
                x >= 0 && x < mutableArt[y].Length)
            {
                mutableArt[y][x] = symbol;
            }
        }
        
        return mutableArt.Select(chars => new string(chars)).ToList();
    }
    
    /// <summary>
    /// Renders a legend for the room art.
    /// </summary>
    /// <param name="legend">Symbol to description mapping.</param>
    /// <param name="colors">Optional symbol to color mapping.</param>
    /// <param name="contentArea">Available content area.</param>
    public void RenderLegend(
        IReadOnlyDictionary<char, string> legend,
        IReadOnlyDictionary<char, ConsoleColor>? colors,
        (int X, int Y, int Width, int Height) contentArea)
    {
        if (!legend.Any()) return;
        
        var legendParts = legend.Select(kvp => 
        {
            var color = colors?.GetValueOrDefault(kvp.Key);
            return (Symbol: kvp.Key, Description: kvp.Value, Color: color);
        }).ToList();
        
        // Render legend at bottom of content area
        var legendText = string.Join("  ", 
            legendParts.Select(p => $"{p.Symbol}={p.Description}"));
        
        var y = contentArea.Y + contentArea.Height - 1;
        _terminal.SetCursorPosition(contentArea.X, y);
        _terminal.Write($"Legend: {legendText}");
    }
    
    /// <summary>
    /// Renders art with colored symbols.
    /// </summary>
    private void RenderArtWithColors(
        IReadOnlyList<string> art,
        IReadOnlyDictionary<char, ConsoleColor>? symbolColors,
        (int X, int Y, int Width, int Height) contentArea)
    {
        for (var lineIndex = 0; lineIndex < art.Count && lineIndex < contentArea.Height; lineIndex++)
        {
            _terminal.SetCursorPosition(contentArea.X, contentArea.Y + lineIndex);
            var line = art[lineIndex];
            
            if (symbolColors == null || !symbolColors.Any())
            {
                _terminal.Write(line);
                continue;
            }
            
            foreach (var ch in line)
            {
                if (symbolColors.TryGetValue(ch, out var color))
                {
                    var prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    _terminal.Write(ch.ToString());
                    Console.ForegroundColor = prevColor;
                }
                else
                {
                    _terminal.Write(ch.ToString());
                }
            }
        }
    }
    
    /// <summary>
    /// Creates default art for rooms without custom art.
    /// </summary>
    private static IReadOnlyList<string> CreateDefaultArt(string roomTypeId)
    {
        return new[]
        {
            "┌─────────────────────────────────────────┐",
            "│                                         │",
            $"│  {CenterText(roomTypeId, 37)}  │",
            "│                                         │",
            "│           (No custom art)               │",
            "│                                         │",
            "└─────────────────────────────────────────┘"
        };
    }
    
    private static string CenterText(string text, int width)
    {
        if (text.Length >= width) return text[..width];
        var padding = (width - text.Length) / 2;
        return text.PadLeft(padding + text.Length).PadRight(width);
    }
}
