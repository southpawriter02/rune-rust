using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Handles text formatting and color selection for boss status display elements.
/// </summary>
/// <remarks>
/// <para>This renderer is stateless and handles all string formatting operations
/// for phase indicators, enrage timers, and vulnerability windows.</para>
/// <para>It reads configuration for colors and uses consistent formatting patterns
/// across all boss status UI elements.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new BossStatusRenderer(config, logger);
/// var phaseText = renderer.FormatPhaseText("Empowered", 2);
/// var color = renderer.GetPhaseColor(BossBehavior.Aggressive);
/// </code>
/// </example>
public class BossStatusRenderer
{
    private readonly BossStatusDisplayConfig _config;
    private readonly ILogger<BossStatusRenderer>? _logger;

    /// <summary>
    /// Creates a new instance of the BossStatusRenderer.
    /// </summary>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public BossStatusRenderer(
        BossStatusDisplayConfig? config = null,
        ILogger<BossStatusRenderer>? logger = null)
    {
        _config = config ?? BossStatusDisplayConfig.CreateDefault();
        _logger = logger;
    }

    #region Phase Formatting

    /// <summary>
    /// Formats the phase text display.
    /// </summary>
    /// <param name="phaseName">The phase display name.</param>
    /// <param name="phaseNumber">The phase number.</param>
    /// <returns>Formatted phase text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatPhaseText("Empowered", 2);
    /// // Returns: "Phase: 2 - EMPOWERED"
    /// </code>
    /// </example>
    public string FormatPhaseText(string phaseName, int phaseNumber)
    {
        ArgumentNullException.ThrowIfNull(phaseName);

        var result = $"Phase: {phaseNumber} - {phaseName.ToUpperInvariant()}";

        _logger?.LogDebug(
            "Formatted phase text: Phase {Number} '{Name}' -> '{Result}'",
            phaseNumber, phaseName, result);

        return result;
    }

    /// <summary>
    /// Gets the color for a phase based on boss behavior.
    /// </summary>
    /// <param name="behavior">The boss behavior enum.</param>
    /// <returns>The console color for the phase display.</returns>
    /// <remarks>
    /// <para>Maps each BossBehavior to a configured color:</para>
    /// <list type="bullet">
    ///   <item><description>Aggressive → Red</description></item>
    ///   <item><description>Tactical → Cyan</description></item>
    ///   <item><description>Defensive → Blue</description></item>
    ///   <item><description>Enraged → DarkRed</description></item>
    ///   <item><description>Summoner → Magenta</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetPhaseColor(BossBehavior behavior)
    {
        var color = behavior switch
        {
            BossBehavior.Aggressive => _config.Colors.AggressiveColor,
            BossBehavior.Tactical => _config.Colors.TacticalColor,
            BossBehavior.Defensive => _config.Colors.DefensiveColor,
            BossBehavior.Enraged => _config.Colors.EnragedColor,
            BossBehavior.Summoner => _config.Colors.SummonerColor,
            _ => ConsoleColor.White
        };

        _logger?.LogDebug("Mapped behavior {Behavior} to color {Color}", behavior, color);

        return color;
    }

    /// <summary>
    /// Formats the phase transition effect box.
    /// </summary>
    /// <param name="transitionDto">The transition data.</param>
    /// <returns>A list of strings representing box lines.</returns>
    /// <remarks>
    /// <para>Creates a centered box with:</para>
    /// <list type="bullet">
    ///   <item><description>Header: "[!] PHASE TRANSITION"</description></item>
    ///   <item><description>Boss entry text with phase name</description></item>
    ///   <item><description>Optional transition flavor text</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var lines = renderer.FormatTransitionBox(transitionDto);
    /// // Returns list with formatted box lines
    /// </code>
    /// </example>
    public IReadOnlyList<string> FormatTransitionBox(PhaseTransitionDto transitionDto)
    {
        ArgumentNullException.ThrowIfNull(transitionDto);

        var lines = new List<string>();
        var boxWidth = _config.TransitionEffect.BoxWidth;
        var border = new string('=', boxWidth - 2);

        // Top border
        lines.Add($"+{border}+");

        // Header
        var header = "  [!]  PHASE TRANSITION";
        lines.Add($"|{header.PadRight(boxWidth - 2)}|");

        // Separator
        lines.Add($"+{border}+");

        // Empty line
        lines.Add($"|{new string(' ', boxWidth - 2)}|");

        // Boss enters phase line
        var enterText = $"  {transitionDto.BossName} enters Phase {transitionDto.NewPhaseNumber}: {transitionDto.PhaseName}";
        // Truncate if too long
        if (enterText.Length > boxWidth - 4)
        {
            enterText = enterText[..(boxWidth - 7)] + "...";
        }
        lines.Add($"|{enterText.PadRight(boxWidth - 2)}|");

        // Empty line
        lines.Add($"|{new string(' ', boxWidth - 2)}|");

        // Transition text (if present)
        if (!string.IsNullOrEmpty(transitionDto.TransitionText))
        {
            var quoteText = $"  \"{transitionDto.TransitionText}\"";
            // Truncate if too long
            if (quoteText.Length > boxWidth - 4)
            {
                quoteText = quoteText[..(boxWidth - 7)] + "...\"";
            }
            lines.Add($"|{quoteText.PadRight(boxWidth - 2)}|");
            lines.Add($"|{new string(' ', boxWidth - 2)}|");
        }

        // Bottom border
        lines.Add($"+{border}+");

        _logger?.LogDebug(
            "Formatted transition box for '{BossName}' Phase {Old} -> {New}",
            transitionDto.BossName, transitionDto.OldPhaseNumber, transitionDto.NewPhaseNumber);

        return lines;
    }

    #endregion

    #region Enrage Formatting

    /// <summary>
    /// Formats the enrage warning text.
    /// </summary>
    /// <param name="healthPercentToEnrage">Health percentage until enrage.</param>
    /// <returns>Formatted warning text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatEnrageWarning(5);
    /// // Returns: "[!] Enrage in: 5% health"
    /// </code>
    /// </example>
    public string FormatEnrageWarning(int healthPercentToEnrage)
    {
        var result = $"[!] Enrage in: {healthPercentToEnrage}% health";

        _logger?.LogDebug("Formatted enrage warning: {Percent}% -> '{Result}'", healthPercentToEnrage, result);

        return result;
    }

    /// <summary>
    /// Formats the active enrage header.
    /// </summary>
    /// <returns>Formatted enrage active text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatEnrageActive();
    /// // Returns: "[!!!] ENRAGED"
    /// </code>
    /// </example>
    public string FormatEnrageActive()
    {
        return "[!!!] ENRAGED";
    }

    /// <summary>
    /// Formats enrage stat modifiers.
    /// </summary>
    /// <param name="modifiers">The stat modifiers dictionary (stat ID to multiplier).</param>
    /// <returns>Formatted modifier string.</returns>
    /// <remarks>
    /// <para>Multipliers are converted to percentages:</para>
    /// <list type="bullet">
    ///   <item><description>1.5 → "+50%"</description></item>
    ///   <item><description>0.75 → "-25%"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = new Dictionary&lt;string, float&gt; { { "damage", 1.5f }, { "attackSpeed", 1.25f } };
    /// var text = renderer.FormatEnrageModifiers(modifiers);
    /// // Returns: "+50% damage, +25% attack speed"
    /// </code>
    /// </example>
    public string FormatEnrageModifiers(IReadOnlyDictionary<string, float> modifiers)
    {
        ArgumentNullException.ThrowIfNull(modifiers);

        var parts = new List<string>();

        foreach (var (stat, multiplier) in modifiers)
        {
            var percentChange = (multiplier - 1.0f) * 100;
            var sign = percentChange >= 0 ? "+" : "";
            var statName = FormatStatName(stat);
            parts.Add($"{sign}{percentChange:F0}% {statName}");
        }

        var result = string.Join(", ", parts);

        _logger?.LogDebug("Formatted enrage modifiers: {Count} modifiers -> '{Result}'", modifiers.Count, result);

        return result;
    }

    /// <summary>
    /// Gets the warning color based on proximity to enrage.
    /// </summary>
    /// <param name="healthPercentToEnrage">Health percentage until enrage.</param>
    /// <returns>The console color for the warning.</returns>
    /// <remarks>
    /// <para>Color thresholds:</para>
    /// <list type="bullet">
    ///   <item><description>&lt;=5%: Critical (Red)</description></item>
    ///   <item><description>&lt;=10%: High (DarkYellow/Orange)</description></item>
    ///   <item><description>&gt;10%: Low (Yellow)</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetWarningColor(int healthPercentToEnrage)
    {
        var color = healthPercentToEnrage switch
        {
            <= 5 => _config.Colors.CriticalWarningColor,
            <= 10 => _config.Colors.HighWarningColor,
            _ => _config.Colors.LowWarningColor
        };

        _logger?.LogDebug("Mapped enrage warning {Percent}% to color {Color}", healthPercentToEnrage, color);

        return color;
    }

    #endregion

    #region Vulnerability Formatting

    /// <summary>
    /// Formats the vulnerability window display.
    /// </summary>
    /// <param name="turnsRemaining">Turns remaining in window.</param>
    /// <param name="damageMultiplier">The damage multiplier (e.g., 1.5).</param>
    /// <returns>Formatted vulnerability text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatVulnerabilityWindow(2, 1.5f);
    /// // Returns: "[!] VULNERABILITY WINDOW OPEN (2 turns) - Deal +50% damage!"
    /// </code>
    /// </example>
    public string FormatVulnerabilityWindow(int turnsRemaining, float damageMultiplier)
    {
        var bonusPercent = (damageMultiplier - 1.0f) * 100;
        var turnText = turnsRemaining == 1 ? "turn" : "turns";
        var result = $"[!] VULNERABILITY WINDOW OPEN ({turnsRemaining} {turnText}) - Deal +{bonusPercent:F0}% damage!";

        _logger?.LogDebug(
            "Formatted vulnerability window: {Turns} turns, {Multiplier}x -> '{Result}'",
            turnsRemaining, damageMultiplier, result);

        return result;
    }

    /// <summary>
    /// Formats the vulnerability urgent warning (1 turn remaining).
    /// </summary>
    /// <returns>Formatted urgent text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatVulnerabilityUrgent();
    /// // Returns: "[!!!] LAST CHANCE - Vulnerability closing!"
    /// </code>
    /// </example>
    public string FormatVulnerabilityUrgent()
    {
        return "[!!!] LAST CHANCE - Vulnerability closing!";
    }

    /// <summary>
    /// Formats the window closed notification.
    /// </summary>
    /// <returns>Formatted closed text.</returns>
    /// <example>
    /// <code>
    /// var text = renderer.FormatWindowClosed();
    /// // Returns: "[x] Vulnerability window closed"
    /// </code>
    /// </example>
    public string FormatWindowClosed()
    {
        return "[x] Vulnerability window closed";
    }

    #endregion

    #region Stat Formatting

    /// <summary>
    /// Formats stat modifiers for display.
    /// </summary>
    /// <param name="modifiers">The stat modifiers dictionary.</param>
    /// <returns>Formatted modifier string.</returns>
    /// <remarks>
    /// Delegates to <see cref="FormatEnrageModifiers"/> for consistent formatting.
    /// </remarks>
    public string FormatStatModifiers(IReadOnlyDictionary<string, float> modifiers)
    {
        return FormatEnrageModifiers(modifiers);
    }

    /// <summary>
    /// Formats a stat ID to a human-readable name.
    /// </summary>
    /// <param name="statId">The stat identifier.</param>
    /// <returns>Formatted stat name in lowercase.</returns>
    private static string FormatStatName(string statId)
    {
        return statId switch
        {
            "damage" => "damage",
            "defense" => "defense",
            "attackSpeed" => "attack speed",
            "moveSpeed" => "move speed",
            _ => statId
        };
    }

    #endregion

    /// <summary>
    /// Gets the configuration for display settings.
    /// </summary>
    /// <returns>The current display configuration.</returns>
    public BossStatusDisplayConfig GetConfig() => _config;
}
