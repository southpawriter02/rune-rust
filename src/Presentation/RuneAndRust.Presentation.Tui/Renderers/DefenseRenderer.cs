// ═══════════════════════════════════════════════════════════════════════════════
// DefenseRenderer.cs
// Renderer for defense action display elements.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renderer for defense action display elements including timing bars,
/// action keys, and result formatting.
/// </summary>
/// <remarks>
/// <para>
/// Follows the Combat UI Component Pattern established in v0.13.0a.
/// All visual configuration is driven by <c>defense-display.json</c>.
/// </para>
/// </remarks>
public class DefenseRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly DefenseDisplayConfig _config;
    private readonly ILogger<DefenseRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="DefenseRenderer"/>.
    /// </summary>
    /// <param name="config">Defense display configuration.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public DefenseRenderer(
        IOptions<DefenseDisplayConfig>? config = null,
        ILogger<DefenseRenderer>? logger = null)
    {
        _config = config?.Value ?? DefenseDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("DefenseRenderer initialized with TimingBarWidth={Width}",
            _config.Display.TimingBarWidth);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ACTION KEY MAPPING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the keyboard key for a defense action type.
    /// </summary>
    /// <param name="defenseType">The defense type string.</param>
    /// <returns>The key character for this action.</returns>
    public char GetActionKey(string defenseType)
    {
        return defenseType.ToLowerInvariant() switch
        {
            "block" => _config.ActionKeys.Block,
            "dodge" => _config.ActionKeys.Dodge,
            "parry" => _config.ActionKeys.Parry,
            "counter" => _config.ActionKeys.Counter,
            _ => char.ToUpper(defenseType[0])
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIMING BAR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the reaction timing bar.
    /// </summary>
    /// <param name="progressPercent">Progress as percentage (0.0-1.0).</param>
    /// <returns>Formatted timing bar string (e.g., "[========..]").</returns>
    public string FormatTimingBar(float progressPercent)
    {
        var totalWidth = _config.Display.TimingBarWidth;
        var clampedPercent = Math.Clamp(progressPercent, 0f, 1f);
        var filledWidth = (int)(totalWidth * clampedPercent);
        var emptyWidth = totalWidth - filledWidth;

        var filled = new string(_config.Symbols.TimingBarFilled, filledWidth);
        var empty = new string(_config.Symbols.TimingBarEmpty, emptyWidth);

        return $"[{filled}{empty}]";
    }

    /// <summary>
    /// Gets the color for the timing bar based on remaining time.
    /// </summary>
    /// <param name="progressPercent">Progress as percentage (0.0-1.0).</param>
    /// <returns>Console color for the timing bar.</returns>
    public ConsoleColor GetTimingBarColor(float progressPercent)
    {
        return progressPercent switch
        {
            <= 0.25f => _config.Colors.TimingCritical,
            <= 0.50f => _config.Colors.TimingWarning,
            _ => _config.Colors.TimingNormal
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESULT FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the color for defense result display.
    /// </summary>
    /// <param name="success">Whether the defense succeeded.</param>
    /// <returns>Console color for the result.</returns>
    public ConsoleColor GetResultColor(bool success)
    {
        return success
            ? _config.Colors.DefenseSuccess
            : _config.Colors.DefenseFailure;
    }

    /// <summary>
    /// Formats the damage reduction display.
    /// </summary>
    /// <param name="originalDamage">Original incoming damage.</param>
    /// <param name="reducedDamage">Damage after reduction.</param>
    /// <returns>Formatted damage reduction string.</returns>
    public string FormatDamageReduction(int originalDamage, int reducedDamage)
    {
        var reduction = originalDamage - reducedDamage;
        return $"Damage reduced: {originalDamage} → {reducedDamage} (-{reduction})";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTACK HEADER
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the incoming attack header.
    /// </summary>
    /// <param name="attack">The incoming attack information.</param>
    /// <returns>Formatted attack header string.</returns>
    public string FormatIncomingAttack(IncomingAttackDto attack)
    {
        ArgumentNullException.ThrowIfNull(attack);
        return $"INCOMING ATTACK: {attack.AttackerName} {attack.AttackName} ({attack.Damage} damage)";
    }

    /// <summary>
    /// Gets the color for the incoming attack header.
    /// </summary>
    /// <returns>Console color for attack header.</returns>
    public ConsoleColor GetAttackHeaderColor() => _config.Colors.IncomingAttack;

    // ═══════════════════════════════════════════════════════════════════════════
    // ACTION AVAILABILITY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the color for an action based on availability.
    /// </summary>
    /// <param name="isAvailable">Whether the action is available.</param>
    /// <returns>Console color for the action.</returns>
    public ConsoleColor GetActionColor(bool isAvailable)
    {
        return isAvailable
            ? _config.Colors.ActionAvailable
            : _config.Colors.ActionUnavailable;
    }
}
