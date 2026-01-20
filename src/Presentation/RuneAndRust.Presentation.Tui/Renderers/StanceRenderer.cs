// ═══════════════════════════════════════════════════════════════════════════════
// StanceRenderer.cs
// Renderer for combat stance display elements.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renderer for combat stance display elements including icons,
/// modifiers, and switch comparisons.
/// </summary>
/// <remarks>
/// <para>
/// Follows the Combat UI Component Pattern established in v0.13.0a.
/// All visual configuration is driven by <c>stance-display.json</c>.
/// </para>
/// </remarks>
public class StanceRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ITerminalService _terminal;
    private readonly StanceDisplayConfig _config;
    private readonly ILogger<StanceRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="StanceRenderer"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for capability detection.</param>
    /// <param name="config">Stance display configuration.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public StanceRenderer(
        ITerminalService terminal,
        IOptions<StanceDisplayConfig>? config = null,
        ILogger<StanceRenderer>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config?.Value ?? StanceDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("StanceRenderer initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STANCE ICONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the icon for a stance category.
    /// </summary>
    /// <param name="category">The stance category string.</param>
    /// <returns>Icon string for the stance.</returns>
    public string GetStanceIcon(string category)
    {
        var useUnicode = _terminal.SupportsUnicode;

        return category.ToLowerInvariant() switch
        {
            "aggressive" => useUnicode ? _config.Icons.Aggressive : _config.Icons.AggressiveAscii,
            "defensive" => useUnicode ? _config.Icons.Defensive : _config.Icons.DefensiveAscii,
            "balanced" => useUnicode ? _config.Icons.Balanced : _config.Icons.BalancedAscii,
            "tactical" => useUnicode ? _config.Icons.Tactical : _config.Icons.TacticalAscii,
            "special" => useUnicode ? _config.Icons.Special : _config.Icons.SpecialAscii,
            _ => useUnicode ? _config.Icons.Default : _config.Icons.DefaultAscii
        };
    }

    /// <summary>
    /// Gets the color for a stance category.
    /// </summary>
    /// <param name="category">The stance category string.</param>
    /// <returns>Console color for the stance.</returns>
    public ConsoleColor GetStanceColor(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "aggressive" => _config.Colors.Aggressive,
            "defensive" => _config.Colors.Defensive,
            "balanced" => _config.Colors.Balanced,
            "tactical" => _config.Colors.Tactical,
            "special" => _config.Colors.Special,
            _ => ConsoleColor.White
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MODIFIER FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a stat modifier for display.
    /// </summary>
    /// <param name="statName">Name of the stat.</param>
    /// <param name="value">Modifier value.</param>
    /// <param name="isPercentage">Whether this is a percentage modifier.</param>
    /// <returns>Formatted modifier string (e.g., "+2 Attack", "-1 Defense").</returns>
    public string FormatModifier(string statName, int value, bool isPercentage = false)
    {
        var sign = value >= 0 ? "+" : "";
        var suffix = isPercentage ? "%" : "";
        return $"{sign}{value}{suffix} {statName}";
    }

    /// <summary>
    /// Gets the color for a modifier based on whether it's positive.
    /// </summary>
    /// <param name="isPositive">Whether the modifier is positive.</param>
    /// <returns>Console color for the modifier.</returns>
    public ConsoleColor GetModifierColor(bool isPositive)
    {
        return isPositive
            ? _config.Colors.PositiveModifier
            : _config.Colors.NegativeModifier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SWITCH COMPARISON
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the stance switch comparison display.
    /// </summary>
    /// <param name="switchDto">The stance switch information.</param>
    /// <returns>List of comparison lines.</returns>
    public IReadOnlyList<string> FormatSwitchComparison(StanceSwitchDto switchDto)
    {
        ArgumentNullException.ThrowIfNull(switchDto);

        var lines = new List<string>();

        // Build a map of "to" modifiers for lookup
        var toModifierMap = switchDto.ToModifiers
            .ToDictionary(m => m.StatName, m => m);

        // Compare each "from" modifier
        foreach (var fromMod in switchDto.FromModifiers)
        {
            if (toModifierMap.TryGetValue(fromMod.StatName, out var toMod))
            {
                // Modifier exists in both stances
                var changeIndicator = GetChangeIndicator(fromMod.Value, toMod.Value);
                lines.Add($"{toMod.DisplayString} {changeIndicator}");
                toModifierMap.Remove(fromMod.StatName);
            }
            else
            {
                // Modifier removed in new stance
                lines.Add($"{fromMod.DisplayString} (removed)");
            }
        }

        // Add new modifiers not in previous stance
        foreach (var newMod in toModifierMap.Values)
        {
            lines.Add($"{newMod.DisplayString} (new)");
        }

        return lines;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the change indicator text.
    /// </summary>
    private static string GetChangeIndicator(int fromValue, int toValue)
    {
        if (toValue != fromValue)
        {
            var sign = fromValue >= 0 ? "+" : "";
            return $"(was {sign}{fromValue})";
        }
        return "(unchanged)";
    }
}
