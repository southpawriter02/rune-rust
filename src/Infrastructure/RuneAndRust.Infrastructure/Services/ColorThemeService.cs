using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Configuration;

namespace RuneAndRust.Infrastructure.Services;

/// <summary>
/// Manages color themes for styled console output.
/// </summary>
/// <remarks>
/// Provides theme-based coloring with fallback for terminals without color support.
/// Themes are loaded at construction and can be switched at runtime.
/// </remarks>
public class ColorThemeService : IColorThemeService
{
    private readonly ITerminalService _terminal;
    private readonly ILogger<ColorThemeService> _logger;
    private readonly Dictionary<string, ThemeDefinition> _themes;
    
    private ThemeDefinition _currentThemeDefinition;
    
    /// <inheritdoc/>
    public string CurrentTheme => _currentThemeDefinition.Name;
    
    /// <inheritdoc/>
    public IReadOnlyList<string> AvailableThemes { get; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ColorThemeService"/> with default themes.
    /// </summary>
    /// <param name="terminal">Terminal service for output operations.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public ColorThemeService(
        ITerminalService terminal,
        ILogger<ColorThemeService> logger)
    {
        _terminal = terminal;
        _logger = logger;
        
        // Initialize with built-in themes
        _themes = CreateDefaultThemes();
        AvailableThemes = _themes.Keys.ToList();
        
        // Set default theme
        _currentThemeDefinition = _themes["dark"];
        
        _logger.LogInformation("ColorThemeService initialized with theme '{Theme}'",
            _currentThemeDefinition.Name);
    }
    
    /// <inheritdoc/>
    public void SetTheme(string themeName)
    {
        var key = themeName.ToLowerInvariant();
        if (!_themes.TryGetValue(key, out var theme))
        {
            throw new ArgumentException(
                $"Theme '{themeName}' not found. Available: {string.Join(", ", AvailableThemes)}");
        }
        
        _currentThemeDefinition = theme;
        _logger.LogInformation("Theme changed to '{Theme}'", themeName);
    }
    
    /// <inheritdoc/>
    public ConsoleColor GetColor(MessageType type)
    {
        if (_currentThemeDefinition.MessageColors.TryGetValue(type, out var color))
        {
            return color;
        }
        
        _logger.LogDebug("No color defined for {Type}, using default", type);
        return _currentThemeDefinition.Foreground;
    }
    
    /// <inheritdoc/>
    public void WriteColored(string text, MessageType type)
    {
        if (!_terminal.SupportsColor)
        {
            _terminal.Write(text);
            return;
        }
        
        var previousColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = GetColor(type);
            _terminal.Write(text);
        }
        finally
        {
            Console.ForegroundColor = previousColor;
        }
    }
    
    /// <inheritdoc/>
    public void WriteColoredAt(int x, int y, string text, MessageType type)
    {
        _terminal.SetCursorPosition(x, y);
        WriteColored(text, type);
    }
    
    /// <summary>
    /// Creates the default theme definitions.
    /// </summary>
    private static Dictionary<string, ThemeDefinition> CreateDefaultThemes()
    {
        return new Dictionary<string, ThemeDefinition>
        {
            ["dark"] = new ThemeDefinition(
                Name: "Dark Theme",
                Background: ConsoleColor.Black,
                Foreground: ConsoleColor.Gray,
                MessageColors: new Dictionary<MessageType, ConsoleColor>
                {
                    // System
                    [MessageType.Default] = ConsoleColor.Gray,
                    [MessageType.Info] = ConsoleColor.Cyan,
                    [MessageType.Warning] = ConsoleColor.Yellow,
                    [MessageType.Error] = ConsoleColor.Red,
                    
                    // Combat
                    [MessageType.CombatHit] = ConsoleColor.Red,
                    [MessageType.CombatMiss] = ConsoleColor.DarkGray,
                    [MessageType.CombatHeal] = ConsoleColor.Green,
                    [MessageType.CombatDamage] = ConsoleColor.DarkRed,
                    [MessageType.CombatCritical] = ConsoleColor.Magenta,
                    
                    // Loot
                    [MessageType.LootCommon] = ConsoleColor.White,
                    [MessageType.LootUncommon] = ConsoleColor.Green,
                    [MessageType.LootRare] = ConsoleColor.Blue,
                    [MessageType.LootEpic] = ConsoleColor.Magenta,
                    [MessageType.LootLegendary] = ConsoleColor.Yellow,
                    
                    // Other
                    [MessageType.Dialogue] = ConsoleColor.Yellow,
                    [MessageType.Description] = ConsoleColor.DarkGray,
                    [MessageType.Command] = ConsoleColor.White,
                    [MessageType.Success] = ConsoleColor.Green,
                    [MessageType.Failure] = ConsoleColor.Red
                }),
            
            ["light"] = new ThemeDefinition(
                Name: "Light Theme",
                Background: ConsoleColor.White,
                Foreground: ConsoleColor.Black,
                MessageColors: new Dictionary<MessageType, ConsoleColor>
                {
                    // System
                    [MessageType.Default] = ConsoleColor.Black,
                    [MessageType.Info] = ConsoleColor.DarkCyan,
                    [MessageType.Warning] = ConsoleColor.DarkYellow,
                    [MessageType.Error] = ConsoleColor.DarkRed,
                    
                    // Combat
                    [MessageType.CombatHit] = ConsoleColor.DarkRed,
                    [MessageType.CombatMiss] = ConsoleColor.Gray,
                    [MessageType.CombatHeal] = ConsoleColor.DarkGreen,
                    [MessageType.CombatDamage] = ConsoleColor.Red,
                    [MessageType.CombatCritical] = ConsoleColor.DarkMagenta,
                    
                    // Loot
                    [MessageType.LootCommon] = ConsoleColor.Black,
                    [MessageType.LootUncommon] = ConsoleColor.DarkGreen,
                    [MessageType.LootRare] = ConsoleColor.DarkBlue,
                    [MessageType.LootEpic] = ConsoleColor.DarkMagenta,
                    [MessageType.LootLegendary] = ConsoleColor.DarkYellow,
                    
                    // Other
                    [MessageType.Dialogue] = ConsoleColor.DarkYellow,
                    [MessageType.Description] = ConsoleColor.Gray,
                    [MessageType.Command] = ConsoleColor.Black,
                    [MessageType.Success] = ConsoleColor.DarkGreen,
                    [MessageType.Failure] = ConsoleColor.DarkRed
                })
        };
    }
}
