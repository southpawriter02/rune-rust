// ═══════════════════════════════════════════════════════════════════════════════
// ComboRenderer.cs
// Renderer for combo chain display elements.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renderer for combo chain display elements including step formatting,
/// bonus display, and color mapping.
/// </summary>
/// <remarks>
/// <para>
/// Follows the Combat UI Component Pattern established in v0.13.0a.
/// All visual configuration is driven by <c>combo-display.json</c>.
/// </para>
/// <para>
/// Key responsibilities:
/// </para>
/// <list type="bullet">
///   <item><description>Format chain steps with state symbols</description></item>
///   <item><description>Format bonus percentages</description></item>
///   <item><description>Map step states to colors</description></item>
///   <item><description>Format window timer with urgency</description></item>
///   <item><description>Format break/completion messages</description></item>
/// </list>
/// </remarks>
public class ComboRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ITerminalService _terminal;
    private readonly ComboDisplayConfig _config;
    private readonly ILogger<ComboRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ComboRenderer"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for capability detection.</param>
    /// <param name="config">Combo display configuration.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ComboRenderer(
        ITerminalService terminal,
        IOptions<ComboDisplayConfig>? config = null,
        ILogger<ComboRenderer>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config?.Value ?? ComboDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("ComboRenderer initialized with BonusMultiplierPerStep={Multiplier}",
            _config.Display.BonusMultiplierPerStep);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STEP FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a combo chain step for display.
    /// </summary>
    /// <param name="abilityName">The ability name for this step.</param>
    /// <param name="state">The current state of this step.</param>
    /// <returns>Formatted step string with state indicator (e.g., "[✓Strike]").</returns>
    public string FormatChainStep(string abilityName, ComboStepState state)
    {
        var prefix = GetStepSymbol(state);
        var formatted = $"[{prefix}{abilityName}]";

        _logger?.LogDebug("Formatted step '{Ability}' with state {State} as '{Formatted}'",
            abilityName, state, formatted);

        return formatted;
    }

    /// <summary>
    /// Gets the symbol for a step state.
    /// </summary>
    /// <param name="state">The step state.</param>
    /// <returns>Symbol string (Unicode or ASCII based on terminal support).</returns>
    public string GetStepSymbol(ComboStepState state)
    {
        var useUnicode = _terminal.SupportsUnicode;

        return state switch
        {
            ComboStepState.Completed => useUnicode
                ? _config.Symbols.CompletedStep
                : _config.Symbols.CompletedStepAscii,
            ComboStepState.Current => useUnicode
                ? _config.Symbols.CurrentStep
                : _config.Symbols.CurrentStepAscii,
            ComboStepState.Pending => useUnicode
                ? _config.Symbols.PendingStep
                : _config.Symbols.PendingStepAscii,
            _ => " "
        };
    }

    /// <summary>
    /// Gets the color for a combo step based on its state.
    /// </summary>
    /// <param name="state">The step state.</param>
    /// <returns>Console color for the step.</returns>
    public ConsoleColor GetStepColor(ComboStepState state)
    {
        return state switch
        {
            ComboStepState.Completed => _config.Colors.CompletedStep,
            ComboStepState.Current => _config.Colors.CurrentStep,
            ComboStepState.Pending => _config.Colors.PendingStep,
            _ => ConsoleColor.White
        };
    }

    /// <summary>
    /// Gets the chain separator string.
    /// </summary>
    /// <returns>Arrow string (Unicode or ASCII).</returns>
    public string GetChainSeparator()
    {
        return _terminal.SupportsUnicode
            ? _config.Symbols.ChainArrow
            : _config.Symbols.ChainArrowAscii;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BONUS FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the combo bonus percentage for display.
    /// </summary>
    /// <param name="progressPercent">Progress percentage (0-100).</param>
    /// <returns>Formatted bonus string (e.g., "+25% damage bonus").</returns>
    public string FormatBonus(int progressPercent)
    {
        if (progressPercent <= 0)
        {
            return _config.Messages.BuildingCombo;
        }

        // Calculate bonus based on progress
        var bonusPercent = progressPercent * _config.Display.BonusMultiplierPerStep / 100;
        return $"+{bonusPercent}% damage bonus";
    }

    /// <summary>
    /// Gets the color for bonus display based on progress.
    /// </summary>
    /// <param name="progressPercent">Progress percentage.</param>
    /// <returns>Console color for the bonus display.</returns>
    public ConsoleColor GetBonusColor(int progressPercent)
    {
        return progressPercent switch
        {
            >= 75 => _config.Colors.HighBonus,
            >= 50 => _config.Colors.MediumBonus,
            >= 25 => _config.Colors.LowBonus,
            _ => _config.Colors.NoBonus
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WINDOW TIMER
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the remaining window time for display.
    /// </summary>
    /// <param name="turnsRemaining">Number of turns remaining.</param>
    /// <returns>Formatted window string (e.g., "2 turns (!)").</returns>
    public string FormatWindowRemaining(int turnsRemaining)
    {
        if (turnsRemaining <= 0)
        {
            return "EXPIRED";
        }

        var turnWord = turnsRemaining == 1 ? "turn" : "turns";
        var urgency = turnsRemaining <= _config.Display.UrgentWindowThreshold
            ? " (!)"
            : "";

        return $"{turnsRemaining} {turnWord}{urgency}";
    }

    /// <summary>
    /// Gets the color for window timer based on urgency.
    /// </summary>
    /// <param name="turnsRemaining">Number of turns remaining.</param>
    /// <returns>Console color for the window timer.</returns>
    public ConsoleColor GetWindowColor(int turnsRemaining)
    {
        return turnsRemaining switch
        {
            <= 1 => _config.Colors.UrgentWindow,
            <= 2 => _config.Colors.WarningWindow,
            _ => _config.Colors.NormalWindow
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTINUATION OPTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a continuation option for the available prompt.
    /// </summary>
    /// <param name="continuation">The continuation DTO.</param>
    /// <returns>Formatted continuation option string.</returns>
    public string FormatContinuationOption(ComboContinuationDto continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        var bonusPart = _config.Display.ShowBonusPreview
            ? $" - {continuation.BonusPreview}"
            : "";

        return $"  [{continuation.OptionNumber}] {continuation.AbilityName}{bonusPart}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BREAK / COMPLETION MESSAGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the combo break message.
    /// </summary>
    /// <param name="breakInfo">Break information DTO.</param>
    /// <returns>Formatted break message.</returns>
    public string FormatBreakMessage(ComboBreakDisplayDto breakInfo)
    {
        ArgumentNullException.ThrowIfNull(breakInfo);

        return $"{_config.Messages.ComboBreak} {breakInfo.ComboName} ended at step {breakInfo.ChainLength}";
    }

    /// <summary>
    /// Gets the color for break notifications.
    /// </summary>
    /// <returns>Console color for break messages.</returns>
    public ConsoleColor GetBreakColor() => _config.Colors.ComboBreak;

    /// <summary>
    /// Formats the combo completion message.
    /// </summary>
    /// <param name="completion">Completion information DTO.</param>
    /// <returns>Formatted completion message.</returns>
    public string FormatCompletionMessage(ComboCompletionDisplayDto completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        return $"{_config.Messages.ComboComplete} {completion.ComboName} ({completion.TotalSteps} steps)";
    }

    /// <summary>
    /// Gets the color for completion notifications.
    /// </summary>
    /// <returns>Console color for completion messages.</returns>
    public ConsoleColor GetCompletionColor() => _config.Colors.ComboComplete;
}
