// ═══════════════════════════════════════════════════════════════════════════════
// GatheringDisplay.cs
// Displays the gathering action with dice check and yield results.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the gathering action with dice check and yield results.
/// </summary>
/// <remarks>
/// <para>Shows the dice roll breakdown, skill modifiers, success/failure result,
/// and gathered resources on success.</para>
/// <para>Display format:</para>
/// <code>
/// GATHERING: Healing Herbs
/// ─────────────────────────
/// Skill Check: Herbalism (DC 10)
///
/// Rolling: 1d20 + 3 (Herbalism)
/// Result: [14] + 3 = 17  [x] SUCCESS!
///
/// Yield: [H] Healing Herb x2
/// </code>
/// </remarks>
public class GatheringDisplay
{
    private readonly DiceCheckRenderer _diceRenderer;
    private readonly ResourceStackRenderer _stackRenderer;
    private readonly ITerminalService _terminalService;
    private readonly GatheringDisplayConfig _config;
    private readonly ILogger<GatheringDisplay> _logger;

    private (int X, int Y) _displayPosition;
    private bool _isVisible;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether the display is currently visible.</summary>
    public bool IsVisible => _isVisible;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the GatheringDisplay component.
    /// </summary>
    /// <param name="diceRenderer">The dice check renderer.</param>
    /// <param name="stackRenderer">The resource stack renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for gathering display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public GatheringDisplay(
        DiceCheckRenderer diceRenderer,
        ResourceStackRenderer stackRenderer,
        ITerminalService terminalService,
        GatheringDisplayConfig? config = null,
        ILogger<GatheringDisplay>? logger = null)
    {
        _diceRenderer = diceRenderer ?? throw new ArgumentNullException(nameof(diceRenderer));
        _stackRenderer = stackRenderer ?? throw new ArgumentNullException(nameof(stackRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? GatheringDisplayConfig.CreateDefault();
        _logger = logger ?? NullLogger<GatheringDisplay>.Instance;

        _logger.LogDebug(
            "GatheringDisplay initialized with {Width}x{Height} dimensions",
            _config.DisplayWidth,
            _config.DisplayHeight);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the display position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _displayPosition = (x, y);
        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Shows the dice check for a gathering action.
    /// </summary>
    /// <param name="check">The gathering check data.</param>
    public void ShowDiceCheck(GatheringCheckDisplayDto check)
    {
        _isVisible = true;

        // Clear display area
        ClearDisplay();

        var y = _displayPosition.Y;

        // Header
        var header = $"GATHERING: {check.NodeName}";
        _terminalService.WriteColoredAt(_displayPosition.X, y, header, _config.HeaderColor);
        _terminalService.WriteAt(_displayPosition.X, y + 1, new string('─', header.Length));

        // Skill check line
        var skillCheckLine = _diceRenderer.FormatDC(check.SkillName, check.DifficultyClass);
        _terminalService.WriteAt(_displayPosition.X, y + 2, skillCheckLine);

        // Rolling line (shows what dice are being rolled)
        var rollingLine = $"Rolling: 1d20 + {check.SkillModifier} ({check.SkillName})";
        _terminalService.WriteAt(_displayPosition.X, y + 4, rollingLine);

        _logger.LogDebug(
            "Showed dice check for {NodeName}, DC {DC}",
            check.NodeName,
            check.DifficultyClass);
    }

    /// <summary>
    /// Shows the result of the dice check.
    /// </summary>
    /// <param name="result">The gathering result data.</param>
    public void ShowResult(GatheringResultDto result)
    {
        var y = _displayPosition.Y + 5;

        // Result line with roll breakdown
        var rollBreakdown = _diceRenderer.FormatRoll(result.RawRoll, result.Modifier, result.Total);
        var resultIndicator = _diceRenderer.FormatResult(result.Success);

        var resultLine = $"Result: {rollBreakdown}  {resultIndicator}";
        _terminalService.WriteAt(_displayPosition.X, y, resultLine);

        // Color the result indicator
        var indicatorX = _displayPosition.X + 8 + rollBreakdown.Length + 2;
        var indicatorColor = result.Success ? _config.SuccessColor : _config.FailureColor;
        _terminalService.WriteColoredAt(indicatorX, y, resultIndicator, indicatorColor);

        _logger.LogInformation(
            "Gathering result: {Result}, Roll {Total} vs DC {DC}",
            result.Success ? "SUCCESS" : "FAILED",
            result.Total,
            result.DifficultyClass);
    }

    /// <summary>
    /// Shows the yield from successful gathering.
    /// </summary>
    /// <param name="yield">The gathered resources.</param>
    public void ShowYield(IEnumerable<GatheredResourceDto> yield)
    {
        var y = _displayPosition.Y + 7;
        var yieldList = yield.ToList();

        if (yieldList.Count == 0)
        {
            _terminalService.WriteColoredAt(
                _displayPosition.X,
                y,
                "No resources gathered.",
                _config.FailureColor);
            return;
        }

        // Yield header
        _terminalService.WriteAt(_displayPosition.X, y, "Yield: ");

        // Render each gathered resource
        var yieldX = _displayPosition.X + 7;
        foreach (var resource in yieldList)
        {
            var icon = _stackRenderer.GetTypeIcon(resource.ResourceType);
            var iconColor = _stackRenderer.GetTypeColor(resource.ResourceType);
            var yieldText = $"{resource.ResourceName} x{resource.Quantity}";

            _terminalService.WriteColoredAt(yieldX, y, icon, iconColor);
            _terminalService.WriteAt(yieldX + 4, y, yieldText);

            y++;
        }

        _logger.LogDebug("Showed yield with {Count} resource types", yieldList.Count);
    }

    /// <summary>
    /// Animates the gathering process (optional visual feedback).
    /// </summary>
    public void AnimateGathering()
    {
        // Simple text-based animation
        var y = _displayPosition.Y + 4;
        var frames = new[] { "Gathering.", "Gathering..", "Gathering..." };

        foreach (var frame in frames)
        {
            _terminalService.WriteAt(_displayPosition.X, y, frame + "   ");
            Thread.Sleep(_config.AnimationDelayMs);
        }

        // Clear animation line
        _terminalService.WriteAt(_displayPosition.X, y, new string(' ', 20));
    }

    /// <summary>
    /// Hides the gathering display.
    /// </summary>
    public void Hide()
    {
        if (!_isVisible) return;

        ClearDisplay();
        _isVisible = false;

        _logger.LogDebug("Gathering display hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void ClearDisplay()
    {
        var blankLine = new string(' ', _config.DisplayWidth);
        for (var y = 0; y < _config.DisplayHeight; y++)
        {
            _terminalService.WriteAt(_displayPosition.X, _displayPosition.Y + y, blankLine);
        }
    }

    #endregion
}
