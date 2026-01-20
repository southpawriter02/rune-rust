// ═══════════════════════════════════════════════════════════════════════════════
// ZoneRenderer.cs
// Renders zone symbols, colors, and formatted text for AoE zone overlays.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders zone symbols, colors, and formatted text for AoE zone overlays.
/// </summary>
/// <remarks>
/// <para>
/// Provides all visual mapping for zone display:
/// </para>
/// <list type="bullet">
///   <item><description>Zone type to symbol mapping</description></item>
///   <item><description>Damage type to symbol/color mapping</description></item>
///   <item><description>Duration formatting</description></item>
///   <item><description>Hazard icon rendering (Unicode/ASCII)</description></item>
///   <item><description>Cell content formatting</description></item>
/// </list>
/// <para>
/// Configuration is loaded from <c>zone-display.json</c> via dependency injection.
/// </para>
/// </remarks>
public class ZoneRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ITerminalService _terminal;
    private readonly ZoneDisplayConfig _config;
    private readonly ILogger<ZoneRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ZoneRenderer"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for capability detection.</param>
    /// <param name="config">Zone display configuration (optional, uses defaults if null).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ZoneRenderer(
        ITerminalService terminal,
        IOptions<ZoneDisplayConfig>? config = null,
        ILogger<ZoneRenderer>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config?.Value ?? ZoneDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("ZoneRenderer initialized with {DamageTypeCount} damage types configured",
            _config.DamageTypeSymbols.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYMBOL METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display symbol for a zone type.
    /// </summary>
    /// <param name="zoneType">Type of zone.</param>
    /// <returns>Single character symbol.</returns>
    /// <remarks>
    /// Returns the configured symbol for the zone type:
    /// <list type="bullet">
    ///   <item><description>Damage: 'X' (but damage zones typically use damage type symbol)</description></item>
    ///   <item><description>Control: '!'</description></item>
    ///   <item><description>Debuff: '-'</description></item>
    ///   <item><description>Buff: '+'</description></item>
    ///   <item><description>Preview: '.'</description></item>
    /// </list>
    /// </remarks>
    public char GetZoneSymbol(ZoneType zoneType)
    {
        var symbol = zoneType switch
        {
            ZoneType.Damage => _config.Symbols.Damage,
            ZoneType.Control => _config.Symbols.Control,
            ZoneType.Debuff => _config.Symbols.Debuff,
            ZoneType.Buff => _config.Symbols.Buff,
            ZoneType.Preview => _config.Symbols.Preview,
            _ => '?'
        };

        _logger?.LogDebug("Mapped zone type {ZoneType} to symbol '{Symbol}'", zoneType, symbol);
        return symbol;
    }

    /// <summary>
    /// Gets the symbol for a specific damage type.
    /// </summary>
    /// <param name="damageType">Damage type string (e.g., "fire", "ice").</param>
    /// <returns>Single character symbol.</returns>
    /// <remarks>
    /// Common mappings:
    /// <list type="bullet">
    ///   <item><description>fire → 'F'</description></item>
    ///   <item><description>ice/cold/frost → 'I'</description></item>
    ///   <item><description>poison → 'P'</description></item>
    ///   <item><description>lightning/electric → 'L'</description></item>
    ///   <item><description>acid → 'A'</description></item>
    ///   <item><description>necrotic/shadow → 'N'</description></item>
    ///   <item><description>holy/radiant → 'H'</description></item>
    /// </list>
    /// Returns 'D' for unknown damage types.
    /// </remarks>
    public char GetDamageTypeSymbol(string? damageType)
    {
        if (string.IsNullOrEmpty(damageType))
        {
            return 'D'; // Default damage symbol
        }

        var normalizedType = damageType.ToLowerInvariant();

        // Check configuration first
        if (_config.DamageTypeSymbols.TryGetValue(normalizedType, out var configuredSymbol))
        {
            _logger?.LogDebug("Mapped damage type '{DamageType}' to symbol '{Symbol}' from config",
                damageType, configuredSymbol);
            return configuredSymbol;
        }

        // Fallback switch for common variants
        var symbol = normalizedType switch
        {
            "fire" => 'F',
            "ice" or "cold" or "frost" => 'I',
            "poison" => 'P',
            "lightning" or "electric" or "shock" => 'L',
            "acid" => 'A',
            "necrotic" or "shadow" or "dark" => 'N',
            "holy" or "radiant" or "light" => 'H',
            "bleed" or "physical" => 'B',
            _ => 'D'
        };

        if (symbol == 'D')
        {
            _logger?.LogWarning("Unknown damage type '{DamageType}', using default symbol 'D'", damageType);
        }
        else
        {
            _logger?.LogDebug("Mapped damage type '{DamageType}' to symbol '{Symbol}'", damageType, symbol);
        }

        return symbol;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the console color for a zone type.
    /// </summary>
    /// <param name="zoneType">Type of zone.</param>
    /// <returns>Console color for rendering.</returns>
    public ConsoleColor GetZoneColor(ZoneType zoneType)
    {
        var color = zoneType switch
        {
            ZoneType.Damage => _config.Colors.Damage,
            ZoneType.Control => _config.Colors.Control,
            ZoneType.Debuff => _config.Colors.Debuff,
            ZoneType.Buff => _config.Colors.Buff,
            ZoneType.Preview => _config.Colors.Preview,
            _ => ConsoleColor.Gray
        };

        _logger?.LogDebug("Mapped zone type {ZoneType} to color {Color}", zoneType, color);
        return color;
    }

    /// <summary>
    /// Gets the console color for a specific damage type.
    /// </summary>
    /// <param name="damageType">Damage type string.</param>
    /// <returns>Console color for rendering.</returns>
    public ConsoleColor GetDamageTypeColor(string? damageType)
    {
        if (string.IsNullOrEmpty(damageType))
        {
            return ConsoleColor.Gray;
        }

        var normalizedType = damageType.ToLowerInvariant();

        // Check configuration first
        if (_config.DamageTypeColors.TryGetValue(normalizedType, out var configuredColor))
        {
            return configuredColor;
        }

        // Fallback switch
        return normalizedType switch
        {
            "fire" => ConsoleColor.Red,
            "ice" or "cold" or "frost" => ConsoleColor.Cyan,
            "poison" => ConsoleColor.DarkGreen,
            "lightning" or "electric" or "shock" => ConsoleColor.Yellow,
            "acid" => ConsoleColor.DarkYellow,
            "necrotic" or "shadow" or "dark" => ConsoleColor.DarkMagenta,
            "holy" or "radiant" or "light" => ConsoleColor.White,
            "bleed" or "physical" => ConsoleColor.DarkRed,
            _ => ConsoleColor.Gray
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATTING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats zone duration for display.
    /// </summary>
    /// <param name="remainingTurns">Remaining turns or null if permanent.</param>
    /// <returns>Formatted duration string.</returns>
    /// <remarks>
    /// Returns:
    /// <list type="bullet">
    ///   <item><description><c>null</c> → "permanent"</description></item>
    ///   <item><description><c>0</c> or less → "expiring"</description></item>
    ///   <item><description><c>1</c> → "1 turn"</description></item>
    ///   <item><description><c>N</c> → "N turns"</description></item>
    /// </list>
    /// </remarks>
    public string FormatZoneDuration(int? remainingTurns)
    {
        if (!remainingTurns.HasValue)
        {
            return "permanent";
        }

        if (remainingTurns.Value <= 0)
        {
            return "expiring";
        }

        return remainingTurns.Value == 1 ? "1 turn" : $"{remainingTurns.Value} turns";
    }

    /// <summary>
    /// Gets the hazard icon for a damage type.
    /// </summary>
    /// <param name="damageType">Damage type string.</param>
    /// <returns>Hazard icon string (Unicode or ASCII).</returns>
    /// <remarks>
    /// Returns Unicode icons if the terminal supports Unicode:
    /// <list type="bullet">
    ///   <item><description>fire → 🔥</description></item>
    ///   <item><description>ice → ❄</description></item>
    ///   <item><description>poison → ☠</description></item>
    ///   <item><description>lightning → ⚡</description></item>
    /// </list>
    /// Falls back to single character symbols on ASCII-only terminals.
    /// </remarks>
    public string GetHazardIcon(string? damageType)
    {
        // ASCII fallback if Unicode not supported
        if (!_terminal.SupportsUnicode)
        {
            return GetDamageTypeSymbol(damageType).ToString();
        }

        if (string.IsNullOrEmpty(damageType))
        {
            return _config.HazardIcons.GetValueOrDefault("default", "⚠");
        }

        var normalizedType = damageType.ToLowerInvariant();

        // Check configuration first
        if (_config.HazardIcons.TryGetValue(normalizedType, out var configuredIcon))
        {
            return configuredIcon;
        }

        // Fallback switch
        return normalizedType switch
        {
            "fire" => "🔥",
            "ice" or "cold" or "frost" => "❄",
            "poison" => "☠",
            "lightning" or "electric" or "shock" => "⚡",
            "acid" => "💧",
            "necrotic" or "shadow" or "dark" => "💀",
            "holy" or "radiant" or "light" => "✨",
            _ => _config.HazardIcons.GetValueOrDefault("default", "⚠")
        };
    }

    /// <summary>
    /// Formats a complete zone cell display.
    /// </summary>
    /// <param name="zone">Zone display data.</param>
    /// <returns>Formatted cell content (3 characters).</returns>
    /// <remarks>
    /// Returns the zone symbol padded with spaces for grid alignment.
    /// Uses damage type symbol if available, otherwise zone type symbol.
    /// </remarks>
    public string FormatZoneCell(ZoneDisplayDto zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        var symbol = !string.IsNullOrEmpty(zone.DamageType)
            ? GetDamageTypeSymbol(zone.DamageType)
            : GetZoneSymbol(zone.ZoneType);

        return $" {symbol} ";
    }

    /// <summary>
    /// Gets the ASCII fallback symbol for a zone type.
    /// </summary>
    /// <param name="zoneType">Type of zone.</param>
    /// <returns>ASCII character.</returns>
    public char GetAsciiFallbackSymbol(ZoneType zoneType)
    {
        return zoneType switch
        {
            ZoneType.Damage => 'X',
            ZoneType.Control => '!',
            ZoneType.Debuff => '-',
            ZoneType.Buff => '+',
            ZoneType.Preview => '.',
            _ => '?'
        };
    }
}
