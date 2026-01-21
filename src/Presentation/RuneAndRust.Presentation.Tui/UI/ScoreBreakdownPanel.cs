// ═══════════════════════════════════════════════════════════════════════════════
// ScoreBreakdownPanel.cs
// UI component for displaying detailed score component breakdown.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays a detailed breakdown of score components.
/// </summary>
/// <remarks>
/// <para>
/// Shows how a score was calculated with base components (monsters killed, rooms discovered),
/// multipliers (level bonus, achievement bonus), and penalties (death penalty).
/// </para>
/// <para>Display format:</para>
/// <code>
/// SCORE BREAKDOWN:
/// --------------------------------------------------
/// |-- Monsters Killed: 127 x 10 = 1,270
/// |-- Bosses Killed: 3 x 500 = 1,500
/// |-- Rooms Discovered: 42 x 25 = 1,050
/// +-- Gold Earned: 15,340
/// |-- Level Multiplier: x1.8
/// +-- Achievement Bonus: 4,850
/// +-- Death Penalty: -300
/// 
/// TOTAL: 52,340
/// </code>
/// </remarks>
public class ScoreBreakdownPanel
{
    private readonly ITerminalService _terminalService;
    private readonly LeaderboardViewConfig _config;
    private readonly ILogger<ScoreBreakdownPanel>? _logger;

    /// <summary>
    /// Creates a new instance of the ScoreBreakdownPanel component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public ScoreBreakdownPanel(
        ITerminalService terminalService,
        LeaderboardViewConfig? config = null,
        ILogger<ScoreBreakdownPanel>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new LeaderboardViewConfig();
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the score breakdown.
    /// </summary>
    /// <param name="components">The score component DTOs.</param>
    /// <param name="totalScore">The total score.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The Y coordinate after the last rendered line.</returns>
    public int RenderBreakdown(
        IReadOnlyList<ScoreComponentDto> components,
        long totalScore,
        int x,
        int y)
    {
        _logger?.LogDebug("Rendering score breakdown with {Count} components", components.Count);

        _terminalService.WriteAt(x, y, "SCORE BREAKDOWN:");
        _terminalService.WriteAt(x, y + 1, new string('-', 50));

        var currentY = y + 2;

        // Render base components
        var baseComponents = components.Where(c => !c.IsMultiplier && !c.IsPenalty).ToList();
        foreach (var component in baseComponents)
        {
            var line = FormatComponent(component);
            var prefix = component == baseComponents.Last() && !components.Any(c => c.IsMultiplier || c.IsPenalty)
                ? "+"
                : "|";
            _terminalService.WriteAt(x, currentY, $"{prefix}-- {line}");
            currentY++;
        }

        // Render multipliers
        var multipliers = components.Where(c => c.IsMultiplier).ToList();
        foreach (var multiplier in multipliers)
        {
            var line = FormatMultiplier(multiplier);
            var prefix = multiplier == multipliers.Last() && !components.Any(c => c.IsPenalty)
                ? "+"
                : "|";
            _terminalService.WriteAt(x, currentY, $"{prefix}-- {line}");
            currentY++;
        }

        // Render penalties
        var penalties = components.Where(c => c.IsPenalty).ToList();
        foreach (var penalty in penalties)
        {
            var line = FormatPenalty(penalty);
            var prefix = penalty == penalties.Last() ? "+" : "|";
            _terminalService.WriteAt(x, currentY, $"{prefix}-- {line}");
            currentY++;
        }

        // Render total
        currentY++;
        _terminalService.WriteAt(x, currentY, $"TOTAL: {totalScore:N0}");

        _logger?.LogDebug("Rendered score breakdown with total: {Total}", totalScore);
        return currentY + 1;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a base score component.
    /// </summary>
    /// <param name="component">The component to format.</param>
    /// <returns>The formatted component string.</returns>
    /// <remarks>
    /// Format: "Name: count x points = total" or "Name: total" for flat values.
    /// </remarks>
    public string FormatComponent(ScoreComponentDto component)
    {
        if (component.PointsEach > 0 && component.Count > 0)
        {
            return $"{component.Name}: {component.Count} x {component.PointsEach} = {component.TotalPoints:N0}";
        }
        return $"{component.Name}: {component.TotalPoints:N0}";
    }

    /// <summary>
    /// Formats a multiplier component.
    /// </summary>
    /// <param name="multiplier">The multiplier to format.</param>
    /// <returns>The formatted multiplier string.</returns>
    /// <remarks>
    /// For multipliers stored as int * 100 (e.g., 180 = x1.8),
    /// displays as "Name: x1.8". For flat bonuses, displays as "Name: total".
    /// </remarks>
    public string FormatMultiplier(ScoreComponentDto multiplier)
    {
        if (multiplier.Name.Contains("Multiplier"))
        {
            var value = multiplier.TotalPoints / 100.0;
            return $"{multiplier.Name}: x{value:F1}";
        }
        return $"{multiplier.Name}: {multiplier.TotalPoints:N0}";
    }

    /// <summary>
    /// Formats a penalty component.
    /// </summary>
    /// <param name="penalty">The penalty to format.</param>
    /// <returns>The formatted penalty string with negative prefix.</returns>
    public string FormatPenalty(ScoreComponentDto penalty)
    {
        return $"{penalty.Name}: -{Math.Abs(penalty.TotalPoints):N0}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PARTIAL RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows only the multiplier components.
    /// </summary>
    /// <param name="multipliers">The multiplier components.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void ShowMultipliers(IEnumerable<ScoreComponentDto> multipliers, int x, int y)
    {
        var currentY = y;
        foreach (var multiplier in multipliers)
        {
            _terminalService.WriteAt(x, currentY, $"|-- {FormatMultiplier(multiplier)}");
            currentY++;
        }
    }

    /// <summary>
    /// Shows only the penalty components.
    /// </summary>
    /// <param name="penalties">The penalty components.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void ShowPenalties(IEnumerable<ScoreComponentDto> penalties, int x, int y)
    {
        var currentY = y;
        foreach (var penalty in penalties)
        {
            _terminalService.WriteAt(x, currentY, $"+-- {FormatPenalty(penalty)}");
            currentY++;
        }
    }
}
