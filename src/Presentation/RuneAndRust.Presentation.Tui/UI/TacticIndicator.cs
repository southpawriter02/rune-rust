using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders tactic indicators showing the current coordinated
/// behavior of a monster group.
/// </summary>
/// <remarks>
/// <para>The tactic indicator displays:</para>
/// <list type="bullet">
///   <item><description>Tactic name (e.g., "Flanking Assault")</description></item>
///   <item><description>Role-specific action descriptions</description></item>
///   <item><description>Highlight effect when tactic is executed</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var indicator = new TacticIndicator(renderer, terminal, config, logger);
/// 
/// // Render tactic with role assignments
/// var dto = new TacticDisplayDto("Flank", "Flanking Assault", "Surround target", roleAssignments);
/// indicator.RenderTactic(dto);
/// 
/// // Highlight when executing
/// indicator.HighlightActiveTactic();
/// 
/// // Clear display
/// indicator.Clear();
/// </code>
/// </example>
public class TacticIndicator
{
    private readonly MonsterGroupRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly MonsterGroupDisplayConfig _config;
    private readonly ILogger<TacticIndicator>? _logger;

    private TacticDisplayDto? _currentTactic;
    private bool _isHighlighted;

    /// <summary>
    /// Creates a new instance of the TacticIndicator.
    /// </summary>
    /// <param name="renderer">The renderer for formatting tactic elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public TacticIndicator(
        MonsterGroupRenderer renderer,
        ITerminalService terminalService,
        MonsterGroupDisplayConfig? config = null,
        ILogger<TacticIndicator>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? MonsterGroupDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "TacticIndicator initialized at row offset {Row}",
            _config.TacticIndicatorRow);
    }

    #region Public Methods

    /// <summary>
    /// Renders the current tactic indicator.
    /// </summary>
    /// <param name="tacticDto">The tactic display data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tacticDto"/> is null.</exception>
    /// <remarks>
    /// <para>Renders:</para>
    /// <list type="number">
    ///   <item><description>Tactic title: "TACTIC: {Name}"</description></item>
    ///   <item><description>Role assignments in tree format</description></item>
    /// </list>
    /// </remarks>
    public void RenderTactic(TacticDisplayDto tacticDto)
    {
        _currentTactic = tacticDto ?? throw new ArgumentNullException(nameof(tacticDto));
        _isHighlighted = false;

        _logger?.LogDebug(
            "Rendering tactic indicator: {TacticName}",
            tacticDto.TacticName);

        var tacticY = _config.StartY + _config.TacticIndicatorRow;

        // Render tactic title
        var titleText = $"TACTIC: {tacticDto.TacticName}";
        _terminalService.WriteColoredAt(
            _config.StartX + _config.TextIndent,
            tacticY,
            titleText,
            _config.Colors.TacticColor);

        // Render role assignments below title
        RenderRoleAssignments(tacticDto.RoleAssignments, tacticY + 1);

        _logger?.LogInformation(
            "Tactic indicator rendered: {TacticName} with {RoleCount} role assignments",
            tacticDto.TacticName, tacticDto.RoleAssignments.Count);
    }

    /// <summary>
    /// Shows detailed tactic information.
    /// </summary>
    /// <param name="tacticDto">The tactic display data.</param>
    /// <remarks>
    /// Displays the tactic description if available.
    /// </remarks>
    public void ShowTacticDetails(TacticDisplayDto? tacticDto)
    {
        if (tacticDto == null) return;

        _logger?.LogDebug(
            "Showing tactic details: {TacticName} - {Description}",
            tacticDto.TacticName,
            tacticDto.Description);

        var detailY = _config.StartY + _config.TacticIndicatorRow + 1;

        // Show description if available
        if (!string.IsNullOrEmpty(tacticDto.Description))
        {
            _terminalService.WriteAt(
                _config.StartX + _config.TextIndent,
                detailY,
                tacticDto.Description);
        }
    }

    /// <summary>
    /// Highlights the current tactic to draw attention.
    /// </summary>
    /// <remarks>
    /// Changes the tactic title to the highlight color (default: Cyan).
    /// Used to emphasize when the tactic is being executed.
    /// </remarks>
    public void HighlightActiveTactic()
    {
        if (_currentTactic == null)
        {
            _logger?.LogDebug("No tactic to highlight");
            return;
        }

        _isHighlighted = true;

        var tacticY = _config.StartY + _config.TacticIndicatorRow;
        var titleText = $"TACTIC: {_currentTactic.TacticName}";

        // Use highlight color for emphasis
        _terminalService.WriteColoredAt(
            _config.StartX + _config.TextIndent,
            tacticY,
            titleText,
            _config.Colors.TacticHighlightColor);

        _logger?.LogDebug("Highlighted tactic: {TacticName}", _currentTactic.TacticName);
    }

    /// <summary>
    /// Clears the tactic indicator display.
    /// </summary>
    /// <remarks>
    /// Resets internal state and clears the tactic area from the terminal.
    /// Clears tactic title and role assignments (4 lines).
    /// </remarks>
    public void Clear()
    {
        _currentTactic = null;
        _isHighlighted = false;

        var clearLine = new string(' ', _config.TotalWidth);
        var tacticY = _config.StartY + _config.TacticIndicatorRow;

        // Clear tactic title and role assignments (4 lines)
        for (var i = 0; i < 4; i++)
        {
            _terminalService.WriteAt(_config.StartX, tacticY + i, clearLine);
        }

        _logger?.LogDebug("Cleared tactic indicator");
    }

    /// <summary>
    /// Gets whether the tactic indicator is currently highlighted.
    /// </summary>
    public bool IsHighlighted => _isHighlighted;

    /// <summary>
    /// Gets the current tactic name, or null if none displayed.
    /// </summary>
    public string? CurrentTacticName => _currentTactic?.TacticName;

    #endregion

    #region Private Methods

    /// <summary>
    /// Renders role assignments in tree format.
    /// </summary>
    private void RenderRoleAssignments(IReadOnlyList<RoleAssignmentDto> assignments, int startY)
    {
        for (var i = 0; i < assignments.Count; i++)
        {
            var assignment = assignments[i];
            var isLast = i == assignments.Count - 1;
            var prefix = isLast ? _config.Symbols.TreeEnd : _config.Symbols.TreeBranch;
            var text = $"{prefix}{assignment.RoleName}: {assignment.ActionDescription}";

            _terminalService.WriteAt(
                _config.StartX + _config.TextIndent,
                startY + i,
                text);
        }
    }

    #endregion
}
