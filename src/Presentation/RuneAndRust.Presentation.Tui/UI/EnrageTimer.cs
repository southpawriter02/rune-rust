using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays enrage status including warnings when approaching enrage phase
/// and active enrage indicator with stat modifier display.
/// </summary>
/// <remarks>
/// <para>Enrage in v0.10.4b is phase-based (triggered by health thresholds),
/// not time-based. This component shows warnings when boss health is
/// approaching an enrage phase threshold and displays stat modifiers
/// when enrage is active.</para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>Warning display when approaching enrage threshold</description></item>
///   <item><description>Color-coded warnings based on proximity (Yellow, Orange, Red)</description></item>
///   <item><description>Active enrage indicator with "[!!!] ENRAGED"</description></item>
///   <item><description>Stat modifier display (e.g., "+50% damage, +25% attack speed")</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var timer = new EnrageTimer(renderer, terminalService, config, logger);
/// 
/// // Show enrage warning
/// var warningDto = new EnrageStatusDto(false, 8, new Dictionary&lt;string, float&gt;());
/// timer.RenderTimer(warningDto);
/// 
/// // Show active enrage
/// var enragedDto = new EnrageStatusDto(true, null, new Dictionary&lt;string, float&gt; { { "damage", 1.5f } });
/// timer.RenderTimer(enragedDto);
/// </code>
/// </example>
public class EnrageTimer
{
    private readonly BossStatusRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly BossStatusDisplayConfig _config;
    private readonly ILogger<EnrageTimer>? _logger;

    private EnrageStatusDto? _currentStatus;

    /// <summary>
    /// Creates a new instance of the EnrageTimer.
    /// </summary>
    /// <param name="renderer">The renderer for formatting enrage elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public EnrageTimer(
        BossStatusRenderer renderer,
        ITerminalService terminalService,
        BossStatusDisplayConfig? config = null,
        ILogger<EnrageTimer>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? BossStatusDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "EnrageTimer initialized at position ({X}, {Y})",
            _config.EnrageTimer.StartX, _config.EnrageTimer.StartY);
    }

    /// <summary>
    /// Renders the enrage status display.
    /// </summary>
    /// <param name="statusDto">The enrage status data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statusDto"/> is null.</exception>
    /// <remarks>
    /// <para>Renders one of three states:</para>
    /// <list type="number">
    ///   <item><description>Active enrage with stat modifiers</description></item>
    ///   <item><description>Warning when within threshold of enrage phase</description></item>
    ///   <item><description>Cleared when neither condition is met</description></item>
    /// </list>
    /// </remarks>
    public void RenderTimer(EnrageStatusDto statusDto)
    {
        _currentStatus = statusDto ?? throw new ArgumentNullException(nameof(statusDto));

        _logger?.LogDebug(
            "Rendering enrage status: IsEnraged={IsEnraged}, HealthToEnrage={Health}%",
            statusDto.IsEnraged,
            statusDto.HealthPercentToEnrage);

        if (statusDto.IsEnraged)
        {
            // Boss is currently in an enraged phase
            ShowEnrageActive(statusDto);
        }
        else if (statusDto.HealthPercentToEnrage.HasValue &&
                 statusDto.HealthPercentToEnrage.Value <= _config.EnrageTimer.WarningThreshold)
        {
            // Boss is approaching an enrage phase threshold
            ShowEnrageWarning(statusDto.HealthPercentToEnrage.Value);
        }
        else
        {
            // No enrage warning needed
            Clear();
        }
    }

    /// <summary>
    /// Displays a warning that the boss is approaching enrage threshold.
    /// </summary>
    /// <param name="healthPercentToEnrage">Health percentage until enrage triggers.</param>
    /// <remarks>
    /// <para>Warning colors based on proximity:</para>
    /// <list type="bullet">
    ///   <item><description>&gt;10%: Yellow (low urgency)</description></item>
    ///   <item><description>5-10%: DarkYellow/Orange (medium urgency)</description></item>
    ///   <item><description>&lt;5%: Red (critical urgency)</description></item>
    /// </list>
    /// </remarks>
    public void ShowEnrageWarning(int healthPercentToEnrage)
    {
        _logger?.LogDebug("Showing enrage warning at {Percent}% to enrage", healthPercentToEnrage);

        var warningText = _renderer.FormatEnrageWarning(healthPercentToEnrage);
        var warningColor = _renderer.GetWarningColor(healthPercentToEnrage);

        _terminalService.WriteColoredAt(
            _config.EnrageTimer.StartX,
            _config.EnrageTimer.StartY,
            warningText,
            warningColor);

        _logger?.LogInformation(
            "Enrage warning displayed: {Percent}% to enrage",
            healthPercentToEnrage);
    }

    /// <summary>
    /// Displays the active enrage indicator with stat modifiers.
    /// </summary>
    /// <param name="statusDto">The enrage status data.</param>
    /// <remarks>
    /// <para>Displays two lines:</para>
    /// <list type="number">
    ///   <item><description>"[!!!] ENRAGED" header in enraged color</description></item>
    ///   <item><description>Stat modifiers (e.g., "+50% damage, +25% attack speed")</description></item>
    /// </list>
    /// </remarks>
    public void ShowEnrageActive(EnrageStatusDto statusDto)
    {
        _logger?.LogInformation("Boss is ENRAGED");

        // Render enrage header
        var headerText = _renderer.FormatEnrageActive();

        _terminalService.WriteColoredAt(
            _config.EnrageTimer.StartX,
            _config.EnrageTimer.StartY,
            headerText,
            _config.Colors.EnragedColor);

        // Render stat modifiers
        if (statusDto.StatModifiers.Count > 0)
        {
            var modifierText = _renderer.FormatEnrageModifiers(statusDto.StatModifiers);
            _terminalService.WriteColoredAt(
                _config.EnrageTimer.StartX,
                _config.EnrageTimer.StartY + 1,
                modifierText,
                _config.Colors.EnragedColor);

            _logger?.LogDebug("Rendered enrage modifiers: '{Modifiers}'", modifierText);
        }
    }

    /// <summary>
    /// Clears the enrage status display.
    /// </summary>
    /// <remarks>
    /// Resets internal state and clears the display area.
    /// </remarks>
    public void Clear()
    {
        _currentStatus = null;

        var clearWidth = _config.EnrageTimer.Width;
        var clearHeight = _config.EnrageTimer.Height;
        var clearLine = new string(' ', clearWidth);

        for (var row = 0; row < clearHeight; row++)
        {
            _terminalService.WriteAt(
                _config.EnrageTimer.StartX,
                _config.EnrageTimer.StartY + row,
                clearLine);
        }

        _logger?.LogDebug("Cleared enrage timer display");
    }

    /// <summary>
    /// Gets whether the boss is currently enraged.
    /// </summary>
    public bool IsEnraged => _currentStatus?.IsEnraged ?? false;

    /// <summary>
    /// Gets the current health percentage to enrage, if any.
    /// </summary>
    public int? HealthPercentToEnrage => _currentStatus?.HealthPercentToEnrage;
}
