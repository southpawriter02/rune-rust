using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the current boss phase with name, number, and phase-specific hints.
/// </summary>
/// <remarks>
/// <para>The phase indicator shows players what phase the boss is in and provides
/// ability hints to help them prepare for phase-specific mechanics.</para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>Phase number and name display (e.g., "Phase: 2 - EMPOWERED")</description></item>
///   <item><description>Behavior-based color coding</description></item>
///   <item><description>Ability hint tree display</description></item>
///   <item><description>Phase transition visual effect box</description></item>
///   <item><description>Stat modifier display for enraged phases</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var indicator = new PhaseIndicator(renderer, terminalService, config, logger);
/// 
/// // Render current phase
/// var phaseDto = new PhaseDisplayDto(2, "Empowered", BossBehavior.Aggressive, hints, modifiers);
/// indicator.RenderPhase(phaseDto);
/// 
/// // Show phase transition
/// var transitionDto = new PhaseTransitionDto(1, 2, "Empowered", "Rise, my minions!", "Skeleton King");
/// indicator.ShowTransitionEffect(transitionDto);
/// </code>
/// </example>
public class PhaseIndicator
{
    private readonly BossStatusRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly BossStatusDisplayConfig _config;
    private readonly ILogger<PhaseIndicator>? _logger;

    private PhaseDisplayDto? _currentPhase;
    private bool _isShowingTransition;

    /// <summary>
    /// Creates a new instance of the PhaseIndicator.
    /// </summary>
    /// <param name="renderer">The renderer for formatting phase elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public PhaseIndicator(
        BossStatusRenderer renderer,
        ITerminalService terminalService,
        BossStatusDisplayConfig? config = null,
        ILogger<PhaseIndicator>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? BossStatusDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "PhaseIndicator initialized at position ({X}, {Y})",
            _config.PhaseIndicator.StartX, _config.PhaseIndicator.StartY);
    }

    /// <summary>
    /// Renders the current phase indicator.
    /// </summary>
    /// <param name="phaseDto">The phase display data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="phaseDto"/> is null.</exception>
    /// <remarks>
    /// <para>Renders the following elements:</para>
    /// <list type="number">
    ///   <item><description>Phase header with number and name</description></item>
    ///   <item><description>Ability hints (up to MaxHints)</description></item>
    ///   <item><description>Stat modifiers (if behavior is Enraged)</description></item>
    /// </list>
    /// </remarks>
    public void RenderPhase(PhaseDisplayDto phaseDto)
    {
        _currentPhase = phaseDto ?? throw new ArgumentNullException(nameof(phaseDto));

        _logger?.LogDebug(
            "Rendering phase indicator: Phase {Number} - {Name}",
            phaseDto.PhaseNumber,
            phaseDto.PhaseName);

        // Render phase header
        var phaseText = _renderer.FormatPhaseText(phaseDto.PhaseName, phaseDto.PhaseNumber);
        var phaseColor = _renderer.GetPhaseColor(phaseDto.Behavior);

        _terminalService.WriteColoredAt(
            _config.PhaseIndicator.StartX,
            _config.PhaseIndicator.StartY,
            phaseText,
            phaseColor);

        // Render ability hints if available
        if (phaseDto.AbilityHints.Count > 0)
        {
            RenderAbilityHints(phaseDto.AbilityHints);
        }

        // Render stat modifier summary if behavior is Enraged
        if (phaseDto.Behavior == BossBehavior.Enraged && phaseDto.StatModifiers.Count > 0)
        {
            RenderStatModifiers(phaseDto.StatModifiers);
        }

        _logger?.LogInformation(
            "Phase indicator rendered: Phase {Number} - {Name} ({Behavior})",
            phaseDto.PhaseNumber, phaseDto.PhaseName, phaseDto.Behavior);
    }

    /// <summary>
    /// Displays the phase transition visual effect.
    /// </summary>
    /// <param name="transitionDto">The transition display data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transitionDto"/> is null.</exception>
    /// <remarks>
    /// <para>The transition effect:</para>
    /// <list type="number">
    ///   <item><description>Displays a centered box with transition info</description></item>
    ///   <item><description>Holds for configured duration (default 2000ms)</description></item>
    ///   <item><description>Clears the box when complete</description></item>
    /// </list>
    /// <para>If a transition is already showing, this call is ignored.</para>
    /// </remarks>
    public void ShowTransitionEffect(PhaseTransitionDto transitionDto)
    {
        ArgumentNullException.ThrowIfNull(transitionDto);

        if (_isShowingTransition)
        {
            _logger?.LogDebug("Skipping transition - already showing");
            return;
        }

        _isShowingTransition = true;

        try
        {
            _logger?.LogInformation(
                "Showing phase transition: Phase {Old} -> Phase {New} ({Name})",
                transitionDto.OldPhaseNumber,
                transitionDto.NewPhaseNumber,
                transitionDto.PhaseName);

            // Render transition box
            var transitionBox = _renderer.FormatTransitionBox(transitionDto);

            var boxX = _config.TransitionEffect.CenterX - (_config.TransitionEffect.BoxWidth / 2);
            var boxY = _config.TransitionEffect.StartY;

            // Draw each line of the box
            for (var index = 0; index < transitionBox.Count; index++)
            {
                _terminalService.WriteColoredAt(
                    boxX,
                    boxY + index,
                    transitionBox[index],
                    _config.Colors.TransitionBoxColor);
            }

            // Hold for configured duration
            _terminalService.FlashDelay(_config.TransitionEffect.DisplayDurationMs);

            // Clear transition box
            ClearTransitionBox(boxX, boxY, transitionBox.Count);
        }
        finally
        {
            _isShowingTransition = false;
        }
    }

    /// <summary>
    /// Gets phase description hints for the given boss phase.
    /// </summary>
    /// <param name="phase">The boss phase definition.</param>
    /// <returns>A list of description strings for the phase.</returns>
    /// <remarks>
    /// <para>Generates descriptions from:</para>
    /// <list type="bullet">
    ///   <item><description>Boss behavior (e.g., "Aggressive attacks")</description></item>
    ///   <item><description>Stat modifiers with percentages</description></item>
    ///   <item><description>Summon configuration (if applicable)</description></item>
    /// </list>
    /// </remarks>
    public IReadOnlyList<string> GetPhaseDescription(BossPhase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);

        var descriptions = new List<string>();

        // Add behavior description
        descriptions.Add(GetBehaviorDescription(phase.Behavior));

        // Add stat modifier descriptions
        // StatModifiers is IReadOnlyList<StatModifier> where StatModifier has StatId, Value, ModifierType
        foreach (var modifier in phase.StatModifiers)
        {
            // For percentage modifiers, Value is already in decimal form (e.g., 0.3 = +30%)
            // For flat modifiers, we display the flat value
            var changeText = modifier.Value >= 0
                ? $"+{modifier.Value * 100:F0}%"
                : $"{modifier.Value * 100:F0}%";
            descriptions.Add($"{FormatStatName(modifier.StatId)} {changeText}");
        }

        // Add summon info if applicable
        // SummonConfig is a struct with IsValid property
        if (phase.SummonConfig.IsValid)
        {
            descriptions.Add($"Summons {phase.SummonConfig.MonsterDefinitionId}");
        }

        _logger?.LogDebug(
            "Generated {Count} phase descriptions for Phase {Number}",
            descriptions.Count, phase.PhaseNumber);

        return descriptions;
    }

    /// <summary>
    /// Clears the phase indicator display.
    /// </summary>
    /// <remarks>
    /// Resets internal state and clears the display area.
    /// </remarks>
    public void Clear()
    {
        _currentPhase = null;

        var clearWidth = _config.PhaseIndicator.Width;
        var clearHeight = _config.PhaseIndicator.Height;
        var clearLine = new string(' ', clearWidth);

        for (var row = 0; row < clearHeight; row++)
        {
            _terminalService.WriteAt(
                _config.PhaseIndicator.StartX,
                _config.PhaseIndicator.StartY + row,
                clearLine);
        }

        _logger?.LogDebug("Cleared phase indicator display");
    }

    /// <summary>
    /// Gets whether a phase transition is currently showing.
    /// </summary>
    public bool IsShowingTransition => _isShowingTransition;

    /// <summary>
    /// Gets the current phase number, if any.
    /// </summary>
    public int? CurrentPhaseNumber => _currentPhase?.PhaseNumber;

    #region Private Methods

    /// <summary>
    /// Renders the ability hints in a tree format.
    /// </summary>
    private void RenderAbilityHints(IReadOnlyList<string> hints)
    {
        var hintY = _config.PhaseIndicator.StartY + 1;
        var maxHints = Math.Min(hints.Count, _config.PhaseIndicator.MaxHints);

        for (var index = 0; index < maxHints; index++)
        {
            var hint = hints[index];
            // Use tree-style prefix: "|--" for intermediate, "+--" for last
            var prefix = index == maxHints - 1 ? "  +-- " : "  |-- ";
            var hintText = $"{prefix}{hint}";

            _terminalService.WriteAt(
                _config.PhaseIndicator.StartX,
                hintY + index,
                hintText);
        }

        _logger?.LogDebug("Rendered {Count} ability hints", maxHints);
    }

    /// <summary>
    /// Renders stat modifiers for enraged phases.
    /// </summary>
    private void RenderStatModifiers(IReadOnlyDictionary<string, float> modifiers)
    {
        var modifierY = _config.PhaseIndicator.StartY + 1 + _config.PhaseIndicator.MaxHints;
        var modifierText = _renderer.FormatStatModifiers(modifiers);

        _terminalService.WriteColoredAt(
            _config.PhaseIndicator.StartX,
            modifierY,
            modifierText,
            _config.Colors.EnragedColor);

        _logger?.LogDebug("Rendered stat modifiers: '{Modifiers}'", modifierText);
    }

    /// <summary>
    /// Clears the transition effect box.
    /// </summary>
    private void ClearTransitionBox(int x, int y, int lineCount)
    {
        var clearLine = new string(' ', _config.TransitionEffect.BoxWidth);
        for (var i = 0; i < lineCount; i++)
        {
            _terminalService.WriteAt(x, y + i, clearLine);
        }

        _logger?.LogDebug("Cleared transition box at ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Gets a human-readable description for a boss behavior.
    /// </summary>
    private static string GetBehaviorDescription(BossBehavior behavior)
    {
        return behavior switch
        {
            BossBehavior.Aggressive => "Aggressive attacks",
            BossBehavior.Tactical => "Tactical positioning",
            BossBehavior.Defensive => "Defensive stance",
            BossBehavior.Enraged => "Enraged - all-out attacks!",
            BossBehavior.Summoner => "Summons minions",
            _ => "Unknown behavior"
        };
    }

    /// <summary>
    /// Formats a stat ID to a human-readable name.
    /// </summary>
    private static string FormatStatName(string statId)
    {
        return statId switch
        {
            "damage" => "Damage",
            "defense" => "Defense",
            "attackSpeed" => "Attack Speed",
            "moveSpeed" => "Move Speed",
            _ => statId
        };
    }

    #endregion
}
