// ═══════════════════════════════════════════════════════════════════════════════
// ComboChainIndicator.cs
// UI component for displaying combo chain progress and continuation options.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// UI component for displaying combo chain progress, available continuations,
/// accumulated bonuses, and chain break notifications.
/// </summary>
/// <remarks>
/// <para>
/// Follows the Combat UI Component Pattern established in v0.13.0a.
/// Receives data from <see cref="IComboService"/> and <see cref="IComboProvider"/>
/// via DTOs and delegates rendering to <see cref="ComboRenderer"/>.
/// </para>
/// <para>
/// Key features:
/// </para>
/// <list type="bullet">
///   <item><description>Render active combo chains with step progress</description></item>
///   <item><description>Show available continuation options</description></item>
///   <item><description>Display accumulated bonus effects</description></item>
///   <item><description>Show break/completion notifications</description></item>
/// </list>
/// </remarks>
public class ComboChainIndicator
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IComboService _comboService;
    private readonly IComboProvider _comboProvider;
    private readonly ComboRenderer _renderer;
    private readonly ITerminalService _terminal;
    private readonly ILogger<ComboChainIndicator>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════════════════

    private IReadOnlyList<ComboDisplayDto> _activeChains = [];
    private IReadOnlyList<ComboContinuationDto> _availableContinuations = [];

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ComboChainIndicator"/>.
    /// </summary>
    /// <param name="comboService">Service for retrieving combo progress.</param>
    /// <param name="comboProvider">Provider for combo definitions.</param>
    /// <param name="renderer">Renderer for combo display elements.</param>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ComboChainIndicator(
        IComboService comboService,
        IComboProvider comboProvider,
        ComboRenderer renderer,
        ITerminalService terminal,
        ILogger<ComboChainIndicator>? logger = null)
    {
        _comboService = comboService ?? throw new ArgumentNullException(nameof(comboService));
        _comboProvider = comboProvider ?? throw new ArgumentNullException(nameof(comboProvider));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _logger = logger;

        _logger?.LogDebug("ComboChainIndicator initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHAIN RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the current combo chain progress for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to display combo progress for.</param>
    /// <returns>List of formatted lines for the combo chain display.</returns>
    public IReadOnlyList<string> RenderChain(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var progress = _comboService.GetActiveProgress(combatant);
        if (progress.Count == 0)
        {
            _activeChains = [];
            _logger?.LogDebug("No active combos for combatant {CombatantId}", combatant.Id);
            return [];
        }

        // Map progress to display DTOs
        _activeChains = progress
            .Select(MapToDisplayDto)
            .Where(dto => dto is not null)
            .Cast<ComboDisplayDto>()
            .ToList();

        // Render each chain
        var lines = new List<string>();
        foreach (var chain in _activeChains)
        {
            lines.AddRange(RenderSingleChain(chain));
        }

        _logger?.LogDebug("Rendered {ChainCount} active combo chain(s) for combatant {CombatantId}",
            _activeChains.Count, combatant.Id);

        return lines;
    }

    /// <summary>
    /// Shows available combo continuation options based on current progress.
    /// </summary>
    /// <param name="combatant">The combatant to show continuations for.</param>
    /// <returns>List of formatted lines for continuation options.</returns>
    public IReadOnlyList<string> ShowAvailablePrompt(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var hints = _comboService.GetComboHints(combatant);
        if (hints.Count == 0)
        {
            _availableContinuations = [];
            return [];
        }

        // Map hints to continuation DTOs
        _availableContinuations = hints
            .Select((h, index) => MapToContinuationDto(h, index + 1))
            .ToList();

        // Render continuation prompt
        var lines = new List<string>
        {
            "",
            "Continue combo:"
        };

        foreach (var continuation in _availableContinuations)
        {
            lines.Add(_renderer.FormatContinuationOption(continuation));
        }

        _logger?.LogDebug("Displayed {HintCount} combo continuation(s) for combatant {CombatantId}",
            _availableContinuations.Count, combatant.Id);

        return lines;
    }

    /// <summary>
    /// Gets the display lines for accumulated bonus on active combos.
    /// </summary>
    /// <param name="combatant">The combatant to display bonuses for.</param>
    /// <returns>List of formatted lines for bonus display.</returns>
    public IReadOnlyList<string> DisplayBonus(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var lines = new List<string>();

        foreach (var chain in _activeChains)
        {
            if (chain.BonusEffects.Count == 0)
            {
                continue;
            }

            lines.Add(_renderer.FormatBonus(chain.ProgressPercent));

            foreach (var effect in chain.BonusEffects)
            {
                lines.Add($"  → {effect}");
            }
        }

        return lines;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BREAK / COMPLETION NOTIFICATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the chain break notification when a combo fails or expires.
    /// </summary>
    /// <param name="result">The combo action result indicating failure.</param>
    /// <returns>List of formatted lines for break notification.</returns>
    public IReadOnlyList<string> ShowChainBreak(ComboActionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.ActionType != ComboActionType.Failed)
        {
            return [];
        }

        var breakDisplay = new ComboBreakDisplayDto(
            ComboName: result.ComboName,
            ChainLength: result.CurrentStep ?? 0,
            Reason: "Combo expired");

        var message = _renderer.FormatBreakMessage(breakDisplay);

        _logger?.LogInformation("Combo '{ComboName}' broken at step {Step}",
            result.ComboName, result.CurrentStep);

        return ["", message];
    }

    /// <summary>
    /// Shows the combo completion notification with final bonuses.
    /// </summary>
    /// <param name="result">The combo action result indicating completion.</param>
    /// <param name="definition">The completed combo's definition.</param>
    /// <returns>List of formatted lines for completion notification.</returns>
    public IReadOnlyList<string> ShowCompletion(ComboActionResult result, ComboDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(definition);

        if (result.ActionType != ComboActionType.Completed)
        {
            return [];
        }

        var completionDisplay = new ComboCompletionDisplayDto(
            ComboName: result.ComboName,
            TotalSteps: definition.StepCount,
            BonusEffects: definition.BonusEffects
                .Select(e => e.GetDescription())
                .ToList());

        var lines = new List<string>
        {
            "",
            _renderer.FormatCompletionMessage(completionDisplay)
        };

        foreach (var effect in completionDisplay.BonusEffects)
        {
            lines.Add($"  ✓ {effect}");
        }

        _logger?.LogInformation("Combo '{ComboName}' completed with {EffectCount} bonus effect(s)",
            result.ComboName, definition.BonusEffects.Count);

        return lines;
    }

    /// <summary>
    /// Clears the combo display state.
    /// </summary>
    public void Clear()
    {
        _activeChains = [];
        _availableContinuations = [];
        _logger?.LogDebug("Combo display cleared");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the currently active combo chains.
    /// </summary>
    /// <returns>List of active combo display DTOs.</returns>
    public IReadOnlyList<ComboDisplayDto> GetActiveChains() => _activeChains;

    /// <summary>
    /// Gets the available continuation options.
    /// </summary>
    /// <returns>List of continuation DTOs.</returns>
    public IReadOnlyList<ComboContinuationDto> GetContinuations() => _availableContinuations;

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps ComboProgress to ComboDisplayDto.
    /// </summary>
    private ComboDisplayDto? MapToDisplayDto(ComboProgress progress)
    {
        var definition = _comboProvider.GetCombo(progress.ComboId);
        if (definition is null)
        {
            _logger?.LogWarning("Combo definition not found for ID '{ComboId}'", progress.ComboId);
            return null;
        }

        var steps = definition.Steps
            .Select(s => MapStepToDisplayDto(s, progress.CurrentStep))
            .ToList();

        var progressPercent = definition.StepCount > 0
            ? (int)((progress.CurrentStep - 1) * 100.0 / definition.StepCount)
            : 0;

        return new ComboDisplayDto(
            ComboId: progress.ComboId,
            ComboName: definition.Name,
            Steps: steps,
            CurrentStep: progress.CurrentStep,
            TotalSteps: definition.StepCount,
            WindowRemaining: progress.WindowRemaining,
            BonusEffects: definition.BonusEffects
                .Select(e => e.GetDescription())
                .ToList(),
            ProgressPercent: progressPercent);
    }

    /// <summary>
    /// Maps a ComboStep to ComboStepDisplayDto.
    /// </summary>
    private ComboStepDisplayDto MapStepToDisplayDto(ComboStep step, int currentStep)
    {
        var state = step.StepNumber < currentStep
            ? ComboStepState.Completed
            : step.StepNumber == currentStep
                ? ComboStepState.Current
                : ComboStepState.Pending;

        return new ComboStepDisplayDto(
            StepNumber: step.StepNumber,
            AbilityId: step.AbilityId,
            AbilityName: FormatAbilityName(step.AbilityId),
            State: state,
            TargetRequirement: step.TargetRequirement.ToString());
    }

    /// <summary>
    /// Maps a ComboHint to ComboContinuationDto.
    /// </summary>
    private ComboContinuationDto MapToContinuationDto(ComboHint hint, int optionNumber)
    {
        var definition = _comboProvider.GetCombo(hint.ComboId);

        return new ComboContinuationDto(
            OptionNumber: optionNumber,
            AbilityId: hint.NextAbilityId,
            AbilityName: FormatAbilityName(hint.NextAbilityId),
            ComboName: hint.ComboName,
            StepProgress: $"{hint.CurrentStep}/{hint.TotalSteps}",
            WindowRemaining: hint.WindowRemaining,
            BonusPreview: GetBonusPreview(definition));
    }

    /// <summary>
    /// Formats ability ID as display name.
    /// </summary>
    private static string FormatAbilityName(string abilityId)
    {
        // Format: "shield-bash" → "Shield Bash"
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(abilityId.Replace("_", " ").Replace("-", " "));
    }

    /// <summary>
    /// Gets bonus preview text from definition.
    /// </summary>
    private static string GetBonusPreview(ComboDefinition? definition)
    {
        if (definition is null || definition.BonusEffects.Count == 0)
        {
            return "No bonus";
        }

        return definition.BonusEffects[0].GetDescription();
    }

    /// <summary>
    /// Renders a single combo chain to display lines.
    /// </summary>
    private IReadOnlyList<string> RenderSingleChain(ComboDisplayDto chain)
    {
        var lines = new List<string>
        {
            $"COMBO: {chain.ComboName}"
        };

        // Build step sequence: [✓Strike] → [>Slash] → [?Thrust]
        var separator = _renderer.GetChainSeparator();
        var stepStrings = chain.Steps
            .Select(s => _renderer.FormatChainStep(s.AbilityName, s.State));
        var stepSequence = string.Join(separator, stepStrings);
        lines.Add(stepSequence);

        // Window timer
        var windowDisplay = _renderer.FormatWindowRemaining(chain.WindowRemaining);
        lines.Add($"Window: {windowDisplay}");

        return lines;
    }
}
