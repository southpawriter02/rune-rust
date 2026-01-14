using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provides room art from configuration.
/// </summary>
/// <remarks>
/// Creates default art definitions for common room types.
/// Production implementation would load from JSON configuration.
/// </remarks>
public class RoomArtProvider : IRoomArtProvider
{
    private readonly Dictionary<string, RoomArtDefinition> _artDefinitions;
    private readonly DefaultArtSettings _defaultSettings;
    private readonly ILogger<RoomArtProvider>? _logger;
    
    /// <inheritdoc/>
    public IReadOnlyList<string> AvailableArtIds { get; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="RoomArtProvider"/>.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    public RoomArtProvider(ILogger<RoomArtProvider>? logger = null)
    {
        _logger = logger;
        _defaultSettings = new DefaultArtSettings();
        _artDefinitions = CreateDefaultArtDefinitions();
        AvailableArtIds = _artDefinitions.Keys.ToList();
        
        _logger?.LogInformation("RoomArtProvider initialized with {Count} art definitions",
            _artDefinitions.Count);
    }
    
    /// <inheritdoc/>
    public RoomArtDefinition? GetArtForRoom(string roomTypeId)
    {
        var normalizedId = roomTypeId.ToLowerInvariant();
        if (_artDefinitions.TryGetValue(normalizedId, out var art))
        {
            return art;
        }
        
        _logger?.LogDebug("No art found for room type '{RoomType}'", roomTypeId);
        return null;
    }
    
    /// <inheritdoc/>
    public DefaultArtSettings GetDefaultArt() => _defaultSettings;
    
    /// <inheritdoc/>
    public bool HasArt(string roomTypeId)
    {
        return _artDefinitions.ContainsKey(roomTypeId.ToLowerInvariant());
    }
    
    /// <summary>
    /// Creates default art definitions for common room types.
    /// </summary>
    private static Dictionary<string, RoomArtDefinition> CreateDefaultArtDefinitions()
    {
        return new Dictionary<string, RoomArtDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["dark-cave"] = new RoomArtDefinition(
                RoomId: "dark-cave",
                ArtLines: new[]
                {
                    "    ╔═══════════════════════════════════════╗",
                    "   ╔╝                                       ╚╗",
                    "  ╔╝   ~~~   ▲   ~~~                         ╚╗",
                    " ╔╝    ~~~  ▲▲▲  ~~~    ☠                     ╚╗",
                    "╔╝         ▲▲▲▲▲                               ╚╗",
                    "║    ░░     cave     ░░                         ║",
                    "╚╗        entrance                             ╔╝",
                    " ╚╗   ░░░           ░░░                       ╔╝",
                    "  ╚═══════════════════════════════════════════╝"
                },
                Legend: new Dictionary<char, string>
                {
                    ['~'] = "water",
                    ['▲'] = "stalagmite",
                    ['☠'] = "skeleton",
                    ['░'] = "shadow"
                },
                SymbolColors: new Dictionary<char, ConsoleColor>
                {
                    ['~'] = ConsoleColor.Blue,
                    ['▲'] = ConsoleColor.DarkGray,
                    ['☠'] = ConsoleColor.White,
                    ['░'] = ConsoleColor.DarkGray
                }),
            
            ["throne-room"] = new RoomArtDefinition(
                RoomId: "throne-room",
                ArtLines: new[]
                {
                    "╔═════════════════════════════════════════════╗",
                    "║  ◊   ◊   ◊       THRONE ROOM       ◊   ◊   ◊║",
                    "║                                             ║",
                    "║   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ║",
                    "║   ▓                                   ▓   ║",
                    "║   ▓              ╔═══╗                ▓   ║",
                    "║   ▓              ║ ♔ ║                ▓   ║",
                    "║   ▓              ╚═══╝                ▓   ║",
                    "║   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ║",
                    "║                 entrance                   ║",
                    "╚═════════════════════════════════════════════╝"
                },
                Legend: new Dictionary<char, string>
                {
                    ['◊'] = "torch",
                    ['▓'] = "carpet",
                    ['♔'] = "throne"
                },
                SymbolColors: new Dictionary<char, ConsoleColor>
                {
                    ['◊'] = ConsoleColor.Yellow,
                    ['▓'] = ConsoleColor.DarkRed,
                    ['♔'] = ConsoleColor.Yellow
                }),
            
            ["forest-clearing"] = new RoomArtDefinition(
                RoomId: "forest-clearing",
                ArtLines: new[]
                {
                    "     ♣   ♣        ♣   ♣        ♣   ♣     ",
                    "   ♣   ♣    ♣        ♣    ♣   ♣   ♣   ♣ ",
                    "         ┌────────────────────┐         ",
                    "    ♣    │    Forest Clearing │    ♣    ",
                    "         │                    │         ",
                    "   ♣     │   ✿   ✿   ✿   ✿   │     ♣   ",
                    "         │                    │         ",
                    "    ♣    └────────────────────┘    ♣    ",
                    "   ♣   ♣    ♣        ♣    ♣   ♣   ♣   ♣ ",
                    "     ♣   ♣        ♣   ♣        ♣   ♣     "
                },
                Legend: new Dictionary<char, string>
                {
                    ['♣'] = "tree",
                    ['✿'] = "flower"
                },
                SymbolColors: new Dictionary<char, ConsoleColor>
                {
                    ['♣'] = ConsoleColor.DarkGreen,
                    ['✿'] = ConsoleColor.Magenta
                })
        };
    }
}
