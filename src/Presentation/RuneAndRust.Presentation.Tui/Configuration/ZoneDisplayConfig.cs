// ═══════════════════════════════════════════════════════════════════════════════
// ZoneDisplayConfig.cs
// Configuration for AoE zone display visuals in the combat UI.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for zone display visuals.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/zone-display.json</c> via dependency injection.
/// Provides configurable symbols, colors, and display settings for AoE zone overlays.
/// </para>
/// <para>
/// Use <see cref="CreateDefault"/> to get default configuration when no file is present.
/// </para>
/// </remarks>
public record ZoneDisplayConfig
{
    /// <summary>
    /// Symbols for zone types.
    /// </summary>
    /// <remarks>
    /// Single character symbols displayed on the grid for each zone type.
    /// Damage zones use <see cref="DamageTypeSymbols"/> instead.
    /// </remarks>
    public required ZoneSymbolConfig Symbols { get; init; }

    /// <summary>
    /// Symbols for specific damage types.
    /// </summary>
    /// <remarks>
    /// Maps damage type strings (e.g., "fire", "ice") to single character symbols.
    /// Used for <see cref="ZoneType.Damage"/> zones.
    /// </remarks>
    public required Dictionary<string, char> DamageTypeSymbols { get; init; }

    /// <summary>
    /// Colors for zone types.
    /// </summary>
    /// <remarks>
    /// Console colors used to render zone symbols on the grid.
    /// </remarks>
    public required ZoneColorConfig Colors { get; init; }

    /// <summary>
    /// Colors for specific damage types.
    /// </summary>
    /// <remarks>
    /// Maps damage type strings to console colors.
    /// Overrides base <see cref="ZoneType.Damage"/> color for specific damage types.
    /// </remarks>
    public required Dictionary<string, ConsoleColor> DamageTypeColors { get; init; }

    /// <summary>
    /// Unicode hazard icons.
    /// </summary>
    /// <remarks>
    /// Maps damage type strings to Unicode icons for rich terminal display.
    /// Falls back to ASCII symbols when Unicode is not supported.
    /// </remarks>
    public required Dictionary<string, string> HazardIcons { get; init; }

    /// <summary>
    /// Display behavior settings.
    /// </summary>
    /// <remarks>
    /// Controls which elements are shown in the UI.
    /// </remarks>
    public required ZoneDisplaySettings Display { get; init; }

    /// <summary>
    /// Creates a default configuration.
    /// </summary>
    /// <returns>Default zone display configuration.</returns>
    /// <remarks>
    /// Used when no configuration file is present or for testing.
    /// </remarks>
    public static ZoneDisplayConfig CreateDefault() => new()
    {
        Symbols = new ZoneSymbolConfig
        {
            Damage = 'X',
            Control = '!',
            Debuff = '-',
            Buff = '+',
            Preview = '.'
        },
        DamageTypeSymbols = new Dictionary<string, char>
        {
            ["fire"] = 'F',
            ["ice"] = 'I',
            ["poison"] = 'P',
            ["lightning"] = 'L',
            ["acid"] = 'A',
            ["necrotic"] = 'N',
            ["holy"] = 'H'
        },
        Colors = new ZoneColorConfig
        {
            Damage = ConsoleColor.Red,
            Control = ConsoleColor.Magenta,
            Debuff = ConsoleColor.DarkYellow,
            Buff = ConsoleColor.Green,
            Preview = ConsoleColor.DarkGray
        },
        DamageTypeColors = new Dictionary<string, ConsoleColor>
        {
            ["fire"] = ConsoleColor.Red,
            ["ice"] = ConsoleColor.Cyan,
            ["poison"] = ConsoleColor.DarkGreen,
            ["lightning"] = ConsoleColor.Yellow,
            ["acid"] = ConsoleColor.DarkYellow,
            ["necrotic"] = ConsoleColor.DarkMagenta,
            ["holy"] = ConsoleColor.White
        },
        HazardIcons = new Dictionary<string, string>
        {
            ["fire"] = "🔥",
            ["ice"] = "❄",
            ["poison"] = "☠",
            ["lightning"] = "⚡",
            ["acid"] = "💧",
            ["necrotic"] = "💀",
            ["holy"] = "✨",
            ["default"] = "⚠"
        },
        Display = new ZoneDisplaySettings
        {
            ShowLegend = true,
            ShowDuration = true,
            ShowDamagePerTurn = true
        }
    };
}

/// <summary>
/// Symbols for zone types.
/// </summary>
/// <remarks>
/// Each property maps a <see cref="ZoneType"/> to a single character symbol.
/// </remarks>
public record ZoneSymbolConfig
{
    /// <summary>Symbol for damage zones (default: 'X').</summary>
    public char Damage { get; init; }

    /// <summary>Symbol for control zones (default: '!').</summary>
    public char Control { get; init; }

    /// <summary>Symbol for debuff zones (default: '-').</summary>
    public char Debuff { get; init; }

    /// <summary>Symbol for buff zones (default: '+').</summary>
    public char Buff { get; init; }

    /// <summary>Symbol for preview zones (default: '.').</summary>
    public char Preview { get; init; }
}

/// <summary>
/// Colors for zone types.
/// </summary>
/// <remarks>
/// Each property maps a <see cref="ZoneType"/> to a console color.
/// </remarks>
public record ZoneColorConfig
{
    /// <summary>Color for damage zones (default: Red).</summary>
    public ConsoleColor Damage { get; init; }

    /// <summary>Color for control zones (default: Magenta).</summary>
    public ConsoleColor Control { get; init; }

    /// <summary>Color for debuff zones (default: DarkYellow).</summary>
    public ConsoleColor Debuff { get; init; }

    /// <summary>Color for buff zones (default: Green).</summary>
    public ConsoleColor Buff { get; init; }

    /// <summary>Color for preview zones (default: DarkGray).</summary>
    public ConsoleColor Preview { get; init; }
}

/// <summary>
/// Display behavior settings.
/// </summary>
/// <remarks>
/// Controls which UI elements are rendered for zones.
/// </remarks>
public record ZoneDisplaySettings
{
    /// <summary>Whether to show the zone legend below the grid.</summary>
    public bool ShowLegend { get; init; }

    /// <summary>Whether to show duration in the legend.</summary>
    public bool ShowDuration { get; init; }

    /// <summary>Whether to show damage per turn in the legend.</summary>
    public bool ShowDamagePerTurn { get; init; }
}
