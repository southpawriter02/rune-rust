using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders a prominent health bar for boss encounters with phase markers
/// and damage animation support.
/// </summary>
/// <remarks>
/// <para>The boss health bar is displayed at the top of the combat view during
/// boss encounters. It shows the boss name, current/max HP, a visual bar
/// with phase threshold markers, and animates damage changes.</para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>Centered boss name header with decorative symbols</description></item>
///   <item><description>Health text with current/max HP and thousands separators</description></item>
///   <item><description>Color-coded health bar based on percentage thresholds</description></item>
///   <item><description>Phase markers at transition thresholds</description></item>
///   <item><description>Damage animation with flash effect and delta display</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var healthBar = new BossHealthBar(renderer, terminalService, config, logger);
/// 
/// // Set up phase markers
/// var markers = new List&lt;PhaseMarkerDto&gt;
/// {
///     new(2, "Empowered", 75),
///     new(3, "Enraged", 50)
/// };
/// healthBar.ShowPhaseMarkers(markers);
/// 
/// // Render the health bar
/// var healthDto = new BossHealthDisplayDto("skeleton-king", "Skeleton King", 2847, 5000, 57, 2);
/// healthBar.RenderBar(healthDto);
/// 
/// // Animate damage
/// var damageDto = new DamageAnimationDto(2997, 2847, 150);
/// healthBar.AnimateDamage(damageDto);
/// </code>
/// </example>
public class BossHealthBar
{
    private readonly BossHealthBarRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly BossHealthDisplayConfig _config;
    private readonly ILogger<BossHealthBar>? _logger;

    private BossHealthDisplayDto? _currentState;
    private IReadOnlyList<PhaseMarkerDto> _phaseMarkers = Array.Empty<PhaseMarkerDto>();
    private bool _isAnimating;

    /// <summary>
    /// Creates a new instance of the BossHealthBar.
    /// </summary>
    /// <param name="renderer">The renderer for formatting health bar elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public BossHealthBar(
        BossHealthBarRenderer renderer,
        ITerminalService terminalService,
        IOptions<BossHealthDisplayConfig>? config = null,
        ILogger<BossHealthBar>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config?.Value ?? BossHealthDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "BossHealthBar initialized with bar width {BarWidth}, total width {TotalWidth}",
            _config.BarWidth, _config.TotalWidth);
    }

    /// <summary>
    /// Renders the complete boss health bar display.
    /// </summary>
    /// <param name="healthDto">The current boss health state.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="healthDto"/> is null.</exception>
    /// <remarks>
    /// <para>Renders the following elements in order:</para>
    /// <list type="number">
    ///   <item><description>Boss name header (centered, decorated)</description></item>
    ///   <item><description>Health text (current/max HP)</description></item>
    ///   <item><description>Health bar with fill based on percentage</description></item>
    ///   <item><description>Phase markers on the bar (if configured)</description></item>
    ///   <item><description>Phase labels below the bar (if configured)</description></item>
    /// </list>
    /// </remarks>
    public void RenderBar(BossHealthDisplayDto healthDto)
    {
        _currentState = healthDto ?? throw new ArgumentNullException(nameof(healthDto));

        _logger?.LogDebug(
            "Rendering boss health bar for {BossName}: {Current}/{Max} ({Percent}%)",
            healthDto.BossName,
            healthDto.CurrentHealth,
            healthDto.MaxHealth,
            healthDto.HealthPercent);

        // Render boss name header
        RenderBossNameHeader(healthDto.BossName);

        // Render health text
        RenderHealthText(healthDto.CurrentHealth, healthDto.MaxHealth);

        // Render the health bar with fill
        RenderHealthBarFill(healthDto.HealthPercent);

        // Render phase markers below bar
        if (_phaseMarkers.Count > 0)
        {
            RenderPhaseMarkerLabels();
        }

        _logger?.LogInformation(
            "Boss health bar rendered for '{BossName}' - Phase {Phase}, {Percent}% health",
            healthDto.BossName, healthDto.CurrentPhaseNumber, healthDto.HealthPercent);
    }

    /// <summary>
    /// Sets the phase markers to display on the health bar.
    /// </summary>
    /// <param name="phases">The phase marker DTOs with threshold positions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="phases"/> is null.</exception>
    /// <remarks>
    /// Phase markers are displayed at positions corresponding to health percentage thresholds.
    /// The 100% threshold (starting phase) is typically excluded from display.
    /// </remarks>
    public void ShowPhaseMarkers(IEnumerable<PhaseMarkerDto> phases)
    {
        _phaseMarkers = phases?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(phases));

        _logger?.LogDebug(
            "Set {Count} phase markers for boss health bar: {Markers}",
            _phaseMarkers.Count,
            string.Join(", ", _phaseMarkers.Select(m => $"P{m.PhaseNumber}@{m.ThresholdPercent}%")));
    }

    /// <summary>
    /// Animates damage taken by the boss with flash effect and delta display.
    /// </summary>
    /// <param name="animationDto">The damage animation data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="animationDto"/> is null.</exception>
    /// <remarks>
    /// <para>The animation consists of three steps:</para>
    /// <list type="number">
    ///   <item><description>Flash the bar with damage color (brief)</description></item>
    ///   <item><description>Display the damage delta text (e.g., "[-150]")</description></item>
    ///   <item><description>Update the bar fill to reflect new health</description></item>
    /// </list>
    /// <para>If an animation is already in progress, this call is ignored.</para>
    /// </remarks>
    public void AnimateDamage(DamageAnimationDto animationDto)
    {
        ArgumentNullException.ThrowIfNull(animationDto);

        if (_isAnimating)
        {
            _logger?.LogDebug("Skipping animation - already animating");
            return;
        }

        _isAnimating = true;

        try
        {
            _logger?.LogDebug(
                "Animating damage: {Previous} -> {Current} ({Delta})",
                animationDto.PreviousHealth,
                animationDto.CurrentHealth,
                animationDto.DamageAmount);

            // Flash the bar with damage color
            FlashDamageEffect(animationDto.DamageAmount);

            // Show the damage delta
            ShowDamageDelta(animationDto.DamageAmount);

            // Update to new health value
            if (_currentState != null)
            {
                var updatedDto = _currentState with
                {
                    CurrentHealth = animationDto.CurrentHealth,
                    HealthPercent = CalculatePercent(
                        animationDto.CurrentHealth,
                        _currentState.MaxHealth)
                };
                RenderBar(updatedDto);
            }

            // Clear flash after brief delay (handled by terminal service timing)
            ClearDamageEffect();

            _logger?.LogInformation(
                "Damage animation complete: {Damage} damage dealt",
                animationDto.DamageAmount);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    /// <summary>
    /// Updates the health bar to reflect new health values without animation.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    /// <param name="maxHealth">The maximum health value.</param>
    /// <remarks>
    /// Use this method for silent health updates (e.g., healing over time)
    /// that don't require visual feedback.
    /// </remarks>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_currentState == null)
        {
            _logger?.LogWarning("Cannot update health - no current state");
            return;
        }

        _logger?.LogDebug(
            "Updating health without animation: {Current}/{Max}",
            currentHealth, maxHealth);

        var updatedDto = _currentState with
        {
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            HealthPercent = CalculatePercent(currentHealth, maxHealth)
        };

        RenderBar(updatedDto);
    }

    /// <summary>
    /// Clears the boss health bar display from the terminal.
    /// </summary>
    /// <remarks>
    /// Should be called when a boss encounter ends or when transitioning
    /// away from combat view.
    /// </remarks>
    public void Clear()
    {
        _currentState = null;
        _phaseMarkers = Array.Empty<PhaseMarkerDto>();

        // Clear the display area
        var clearLine = new string(' ', _config.TotalWidth);
        for (var row = 0; row < _config.TotalHeight; row++)
        {
            _terminalService.WriteAt(_config.StartX, _config.StartY + row, clearLine);
        }

        _logger?.LogDebug("Cleared boss health bar display");
    }

    /// <summary>
    /// Gets whether the health bar is currently displaying a boss.
    /// </summary>
    public bool IsActive => _currentState != null;

    /// <summary>
    /// Gets the current boss ID being displayed, if any.
    /// </summary>
    public string? CurrentBossId => _currentState?.BossId;

    #region Private Rendering Methods

    /// <summary>
    /// Renders the boss name as a centered header.
    /// </summary>
    private void RenderBossNameHeader(string bossName)
    {
        var header = _renderer.FormatBossNameHeader(bossName, _config.BarWidth);
        var headerColor = _config.Colors.HeaderColor;

        _terminalService.WriteColoredAt(
            _config.StartX,
            _config.StartY,
            header,
            headerColor);

        _logger?.LogDebug("Rendered boss name header: '{Header}'", header.Trim());
    }

    /// <summary>
    /// Renders the health text (HP: current / max).
    /// </summary>
    private void RenderHealthText(int current, int max)
    {
        var healthText = _renderer.FormatHealthText(current, max);
        var textY = _config.StartY + 1;

        _terminalService.WriteAt(
            _config.StartX + _config.TextIndent,
            textY,
            healthText);

        _logger?.LogDebug("Rendered health text: '{HealthText}'", healthText);
    }

    /// <summary>
    /// Renders the health bar fill with phase markers.
    /// </summary>
    private void RenderHealthBarFill(int healthPercent)
    {
        var barY = _config.StartY + 2;
        var healthColor = _renderer.GetHealthColor(healthPercent);

        // Render bar border
        var barBorder = _renderer.FormatBarBorder(_config.BarWidth);
        _terminalService.WriteAt(_config.StartX + _config.TextIndent, barY, barBorder);

        // Render filled portion
        var fillWidth = _renderer.CalculateFillWidth(healthPercent, _config.BarWidth);
        var emptyWidth = _config.BarWidth - fillWidth;

        var fillChars = new string(_config.Symbols.FilledChar, fillWidth);
        var emptyChars = new string(_config.Symbols.EmptyChar, emptyWidth);

        // Write filled portion with health color
        _terminalService.WriteColoredAt(
            _config.StartX + _config.TextIndent + 1,
            barY,
            fillChars,
            healthColor);

        // Write empty portion
        _terminalService.WriteAt(
            _config.StartX + _config.TextIndent + 1 + fillWidth,
            barY,
            emptyChars);

        // Render phase markers on the bar
        foreach (var marker in _phaseMarkers)
        {
            // Don't show 100% marker (starting phase)
            if (marker.ThresholdPercent < 100)
            {
                var markerX = CalculateMarkerPosition(marker.ThresholdPercent);
                _terminalService.WriteColoredAt(
                    markerX,
                    barY,
                    _config.Symbols.PhaseMarkerChar.ToString(),
                    _config.Colors.PhaseMarkerColor);
            }
        }

        _logger?.LogDebug(
            "Rendered health bar: {FillWidth}/{BarWidth} filled ({Percent}%), color={Color}",
            fillWidth, _config.BarWidth, healthPercent, healthColor);
    }

    /// <summary>
    /// Renders phase labels below the health bar.
    /// </summary>
    private void RenderPhaseMarkerLabels()
    {
        var labelY = _config.StartY + 3;

        foreach (var marker in _phaseMarkers)
        {
            // Don't label 100% marker (starting phase)
            if (marker.ThresholdPercent < 100)
            {
                var markerX = CalculateMarkerPosition(marker.ThresholdPercent);
                var labelLines = _renderer.FormatPhaseLabel(marker.PhaseNumber, marker.ThresholdPercent)
                    .Split('\n');

                // Render each line of the label
                for (var i = 0; i < labelLines.Length; i++)
                {
                    var label = labelLines[i];
                    // Center the label under the marker
                    var labelOffset = label.Length / 2;
                    var labelX = Math.Max(_config.StartX, markerX - labelOffset);

                    _terminalService.WriteAt(labelX, labelY + i, label);
                }
            }
        }

        _logger?.LogDebug("Rendered {Count} phase marker labels", _phaseMarkers.Count(m => m.ThresholdPercent < 100));
    }

    /// <summary>
    /// Flashes the health bar with damage/healing color.
    /// </summary>
    private void FlashDamageEffect(int damageAmount)
    {
        if (_currentState == null) return;

        var barY = _config.StartY + 2;
        var flashColor = damageAmount > 0
            ? _renderer.GetDamageFlashColor()
            : _renderer.GetHealingFlashColor();

        // Briefly flash the entire bar with damage/healing color
        var fillWidth = _renderer.CalculateFillWidth(_currentState.HealthPercent, _config.BarWidth);
        var fillChars = new string(_config.Symbols.FilledChar, fillWidth);

        _terminalService.WriteColoredAt(
            _config.StartX + _config.TextIndent + 1,
            barY,
            fillChars,
            flashColor);

        // Terminal service handles timing delay
        _terminalService.FlashDelay(_config.Animation.FlashDurationMs);

        _logger?.LogDebug("Flashed bar with {Color} for {Duration}ms", flashColor, _config.Animation.FlashDurationMs);
    }

    /// <summary>
    /// Shows the damage delta text.
    /// </summary>
    private void ShowDamageDelta(int damageAmount)
    {
        var deltaText = _renderer.FormatDamageDelta(damageAmount);
        if (string.IsNullOrEmpty(deltaText)) return;

        var deltaY = _config.StartY + 1;
        var deltaX = _config.StartX + _config.BarWidth - deltaText.Length;

        var deltaColor = damageAmount > 0
            ? _config.Colors.DamageColor
            : _config.Colors.HealingColor;

        _terminalService.WriteColoredAt(
            deltaX,
            deltaY,
            deltaText,
            deltaColor);

        // Brief display before clearing
        _terminalService.FlashDelay(_config.Animation.DeltaDisplayMs);

        _logger?.LogDebug("Displayed damage delta: '{Delta}' at ({X}, {Y})", deltaText, deltaX, deltaY);
    }

    /// <summary>
    /// Clears the damage delta text area.
    /// </summary>
    private void ClearDamageEffect()
    {
        // Clear the delta text area
        var deltaY = _config.StartY + 1;
        var clearText = new string(' ', 15); // Max delta text width

        _terminalService.WriteAt(
            _config.StartX + _config.BarWidth - 15,
            deltaY,
            clearText);

        _logger?.LogDebug("Cleared damage delta display");
    }

    /// <summary>
    /// Calculates the X position for a phase marker.
    /// </summary>
    private int CalculateMarkerPosition(int thresholdPercent)
    {
        var barStartX = _config.StartX + _config.TextIndent + 1;
        return _renderer.CalculateMarkerPosition(thresholdPercent, barStartX, _config.BarWidth);
    }

    /// <summary>
    /// Calculates health percentage from current and max values.
    /// </summary>
    private static int CalculatePercent(int current, int max)
    {
        if (max <= 0) return 0;
        return (int)Math.Ceiling((current / (float)max) * 100);
    }

    #endregion
}
