using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Handles text formatting and color selection for boss health bar elements.
/// </summary>
/// <remarks>
/// <para>This renderer is stateless and handles all string formatting operations
/// for the boss health bar UI component.</para>
/// <para>It reads configuration for symbols and color thresholds, providing
/// consistent visual representation of boss health state.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new BossHealthBarRenderer(config, terminal, logger);
/// var healthText = renderer.FormatHealthText(2847, 5000);
/// var color = renderer.GetHealthColor(57);
/// </code>
/// </example>
public class BossHealthBarRenderer
{
    private readonly BossHealthDisplayConfig _config;
    private readonly ITerminalService _terminal;
    private readonly ILogger<BossHealthBarRenderer>? _logger;

    /// <summary>
    /// Creates a new instance of the BossHealthBarRenderer.
    /// </summary>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="terminal">Terminal service for capability detection.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="terminal"/> is null.</exception>
    public BossHealthBarRenderer(
        IOptions<BossHealthDisplayConfig>? config = null,
        ITerminalService? terminal = null,
        ILogger<BossHealthBarRenderer>? logger = null)
    {
        _config = config?.Value ?? BossHealthDisplayConfig.CreateDefault();
        _terminal = terminal!;
        _logger = logger;
    }

    /// <summary>
    /// Formats the boss name as a centered header.
    /// </summary>
    /// <param name="bossName">The boss display name.</param>
    /// <param name="totalWidth">The total width of the display area.</param>
    /// <returns>A centered, decorated boss name header.</returns>
    /// <remarks>
    /// The boss name is converted to uppercase and wrapped with the configured
    /// header prefix and suffix symbols (default: &lt;&lt; and &gt;&gt;).
    /// </remarks>
    /// <example>
    /// <code>
    /// var header = renderer.FormatBossNameHeader("Skeleton King", 60);
    /// // Returns: "                    &lt;&lt;  SKELETON KING  &gt;&gt;                    "
    /// </code>
    /// </example>
    public string FormatBossNameHeader(string bossName, int totalWidth)
    {
        ArgumentNullException.ThrowIfNull(bossName);

        // Decorate the boss name with prefix and suffix
        var decoratedName = $"{_config.Symbols.HeaderPrefix}  {bossName.ToUpperInvariant()}  {_config.Symbols.HeaderSuffix}";

        // Calculate padding for centering
        var padding = (totalWidth - decoratedName.Length) / 2;
        if (padding < 0) padding = 0;

        var result = decoratedName.PadLeft(padding + decoratedName.Length).PadRight(totalWidth);

        _logger?.LogDebug(
            "Formatted boss name header for '{BossName}' with width {Width}: '{Result}'",
            bossName, totalWidth, result.Trim());

        return result;
    }

    /// <summary>
    /// Formats the health text showing current and maximum values.
    /// </summary>
    /// <param name="current">Current health value.</param>
    /// <param name="max">Maximum health value.</param>
    /// <returns>Formatted health text with thousands separators.</returns>
    /// <remarks>
    /// Uses the "N0" format specifier for locale-aware thousands separators.
    /// </remarks>
    /// <example>
    /// <code>
    /// var text = renderer.FormatHealthText(2847, 5000);
    /// // Returns: "HP: 2,847 / 5,000"
    /// </code>
    /// </example>
    public string FormatHealthText(int current, int max)
    {
        var result = $"HP: {current:N0} / {max:N0}";

        _logger?.LogDebug(
            "Formatted health text: {Current}/{Max} -> '{Result}'",
            current, max, result);

        return result;
    }

    /// <summary>
    /// Determines the health bar color based on current health percentage.
    /// </summary>
    /// <param name="healthPercent">The current health percentage (0-100).</param>
    /// <returns>The console color for the health bar fill.</returns>
    /// <remarks>
    /// <para>Iterates through configured thresholds in descending order.</para>
    /// <para>Returns the first threshold color where health is above the threshold.</para>
    /// <para>Default thresholds: &gt;75% Green, &gt;50% Yellow, &gt;25% Orange, &gt;0% Red, 0% DarkRed.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = renderer.GetHealthColor(57); // Returns ConsoleColor.Yellow
    /// var critical = renderer.GetHealthColor(10); // Returns ConsoleColor.Red
    /// </code>
    /// </example>
    public ConsoleColor GetHealthColor(int healthPercent)
    {
        // Iterate thresholds from highest to lowest
        foreach (var threshold in _config.ColorThresholds.OrderByDescending(t => t.Percent))
        {
            if (healthPercent > threshold.Percent)
            {
                _logger?.LogDebug(
                    "Health {Percent}% matched threshold >{Threshold}%: {Color}",
                    healthPercent, threshold.Percent, threshold.Color);
                return threshold.Color;
            }
        }

        // Below all thresholds - use critical color
        _logger?.LogDebug(
            "Health {Percent}% below all thresholds, using critical color: {Color}",
            healthPercent, _config.Colors.CriticalColor);

        return _config.Colors.CriticalColor;
    }

    /// <summary>
    /// Formats the bar border characters.
    /// </summary>
    /// <param name="barWidth">The width of the bar interior.</param>
    /// <returns>The bar border string.</returns>
    /// <example>
    /// <code>
    /// var border = renderer.FormatBarBorder(10);
    /// // Returns: "+============+"
    /// </code>
    /// </example>
    public string FormatBarBorder(int barWidth)
    {
        var borderChars = new string(_config.Symbols.BorderChar, barWidth);
        return $"{_config.Symbols.CornerChar}{borderChars}{_config.Symbols.CornerChar}";
    }

    /// <summary>
    /// Formats a phase label for display below the health bar.
    /// </summary>
    /// <param name="phaseNumber">The phase number (1-based).</param>
    /// <param name="thresholdPercent">The health threshold percentage.</param>
    /// <returns>A formatted phase label string.</returns>
    /// <remarks>
    /// The label is formatted on two lines: phase name and percentage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var label = renderer.FormatPhaseLabel(2, 75);
    /// // Returns: "Phase 2\n (75%)"
    /// </code>
    /// </example>
    public string FormatPhaseLabel(int phaseNumber, int thresholdPercent)
    {
        return $"Phase {phaseNumber}\n ({thresholdPercent}%)";
    }

    /// <summary>
    /// Renders a phase marker character for the health bar.
    /// </summary>
    /// <param name="thresholdPercent">The health threshold percentage.</param>
    /// <param name="phaseNumber">The phase number for tooltip.</param>
    /// <returns>The phase marker character.</returns>
    public string RenderPhaseMarker(int thresholdPercent, int phaseNumber)
    {
        _logger?.LogDebug(
            "Rendering phase marker at {Percent}% for phase {Phase}",
            thresholdPercent, phaseNumber);

        return _config.Symbols.PhaseMarkerChar.ToString();
    }

    /// <summary>
    /// Gets the color used for damage flash effect.
    /// </summary>
    /// <returns>The flash color for damage animation.</returns>
    public ConsoleColor GetDamageFlashColor()
    {
        return _config.Colors.DamageFlashColor;
    }

    /// <summary>
    /// Gets the color used for healing flash effect.
    /// </summary>
    /// <returns>The flash color for healing animation.</returns>
    public ConsoleColor GetHealingFlashColor()
    {
        return _config.Colors.HealingColor;
    }

    /// <summary>
    /// Formats the damage delta display text.
    /// </summary>
    /// <param name="damageAmount">The amount of damage dealt.</param>
    /// <returns>Formatted damage delta string.</returns>
    /// <remarks>
    /// <para>Positive values are formatted as "[-X]" (damage).</para>
    /// <para>Negative values are formatted as "[+X]" (healing).</para>
    /// <para>Zero returns an empty string.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var damage = renderer.FormatDamageDelta(150);   // Returns: "[-150]"
    /// var healing = renderer.FormatDamageDelta(-50); // Returns: "[+50]"
    /// var zero = renderer.FormatDamageDelta(0);      // Returns: ""
    /// </code>
    /// </example>
    public string FormatDamageDelta(int damageAmount)
    {
        if (damageAmount > 0)
        {
            var result = $"[-{damageAmount:N0}]";
            _logger?.LogDebug("Formatted damage delta: {Amount} -> '{Result}'", damageAmount, result);
            return result;
        }

        if (damageAmount < 0)
        {
            var result = $"[+{Math.Abs(damageAmount):N0}]";
            _logger?.LogDebug("Formatted healing delta: {Amount} -> '{Result}'", damageAmount, result);
            return result;
        }

        return string.Empty;
    }

    /// <summary>
    /// Calculates the bar fill width for a given health percentage.
    /// </summary>
    /// <param name="healthPercent">The current health percentage (0-100).</param>
    /// <param name="barWidth">The total bar width in characters.</param>
    /// <returns>The number of filled characters.</returns>
    /// <remarks>
    /// Clamps the health percentage to 0-100 range before calculation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var fillWidth = renderer.CalculateFillWidth(75, 60); // Returns: 45
    /// </code>
    /// </example>
    public int CalculateFillWidth(int healthPercent, int barWidth)
    {
        var normalizedPercent = Math.Clamp(healthPercent, 0, 100) / 100.0;
        var result = (int)(barWidth * normalizedPercent);

        _logger?.LogDebug(
            "Calculated fill width: {Percent}% of {BarWidth} = {FillWidth}",
            healthPercent, barWidth, result);

        return result;
    }

    /// <summary>
    /// Calculates the X position for a phase marker on the health bar.
    /// </summary>
    /// <param name="thresholdPercent">The health percentage threshold (0-100).</param>
    /// <param name="barStartX">The starting X coordinate of the bar fill area.</param>
    /// <param name="barWidth">The total width of the bar fill area.</param>
    /// <returns>The X coordinate for the phase marker.</returns>
    /// <remarks>
    /// Example: For a 60-character bar with threshold at 75%:
    /// Position = barStartX + (60 * 0.75) = barStartX + 45
    /// </remarks>
    public int CalculateMarkerPosition(int thresholdPercent, int barStartX, int barWidth)
    {
        var normalizedPercent = Math.Clamp(thresholdPercent, 0, 100) / 100.0;
        var position = barStartX + (int)(barWidth * normalizedPercent);

        _logger?.LogDebug(
            "Calculated marker position: {Percent}% at barStart {Start}, width {Width} = position {Position}",
            thresholdPercent, barStartX, barWidth, position);

        return position;
    }

    /// <summary>
    /// Gets the configuration for display settings.
    /// </summary>
    /// <returns>The current display configuration.</returns>
    public BossHealthDisplayConfig GetConfig() => _config;
}
