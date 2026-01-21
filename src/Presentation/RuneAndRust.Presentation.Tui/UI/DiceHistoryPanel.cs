// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryPanel.cs
// Main UI component for displaying comprehensive dice roll history.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Records;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Panel component that displays comprehensive dice roll history including
/// recent rolls, statistics, luck rating, streaks, and distribution.
/// </summary>
/// <remarks>
/// <para>This is the main container component for the dice history display:</para>
/// <list type="bullet">
///   <item><description>Recent rolls section with critical highlighting</description></item>
///   <item><description>Statistics section (total, nat 20s, nat 1s, average)</description></item>
///   <item><description>Luck rating section with color-coded indicator</description></item>
///   <item><description>Streak indicator (current and longest)</description></item>
///   <item><description>Roll distribution chart with 20 bars</description></item>
/// </list>
/// <para>
/// This component visualizes data from IDiceHistoryService and does not
/// perform any dice calculations. All statistics and history are retrieved
/// from the Application layer services.
/// </para>
/// </remarks>
public class DiceHistoryPanel
{
    private readonly ITerminalService _terminal;
    private readonly DiceRollRenderer _rollRenderer;
    private readonly StreakIndicator _streakIndicator;
    private readonly RollDistributionChart _distributionChart;
    private readonly DiceHistoryPanelConfig _config;
    private readonly ILogger<DiceHistoryPanel>? _logger;

    private int _positionX;
    private int _positionY;
    private int _displayCount;

    // ═══════════════════════════════════════════════════════════════════════════
    // Panel Sections Y-Offsets (relative to panel position)
    // ═══════════════════════════════════════════════════════════════════════════

    private const int HeaderY = 0;
    private const int RecentRollsSectionY = 3;
    private const int StatsSectionY = 8;
    private const int DistributionSectionY = 16;

    /// <summary>
    /// Creates a new dice history panel with the specified dependencies.
    /// </summary>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="rollRenderer">Renderer for individual dice rolls.</param>
    /// <param name="streakIndicator">Streak indicator component.</param>
    /// <param name="distributionChart">Roll distribution chart component.</param>
    /// <param name="config">Panel configuration settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public DiceHistoryPanel(
        ITerminalService terminal,
        DiceRollRenderer rollRenderer,
        StreakIndicator streakIndicator,
        RollDistributionChart distributionChart,
        DiceHistoryPanelConfig? config = null,
        ILogger<DiceHistoryPanel>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _rollRenderer = rollRenderer ?? throw new ArgumentNullException(nameof(rollRenderer));
        _streakIndicator = streakIndicator ?? throw new ArgumentNullException(nameof(streakIndicator));
        _distributionChart = distributionChart ?? throw new ArgumentNullException(nameof(distributionChart));
        _config = config ?? new DiceHistoryPanelConfig();
        _logger = logger;

        _displayCount = _config.RecentRollDisplayCount;

        _logger?.LogDebug("DiceHistoryPanel initialized with display count: {Count}", _displayCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Configuration
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the position for rendering the panel.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _positionX = x;
        _positionY = y;
        _logger?.LogDebug("Panel position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Sets the number of recent rolls to display.
    /// </summary>
    /// <param name="count">Number of rolls to display (default 20).</param>
    public void SetDisplayCount(int count)
    {
        _displayCount = count;
        _logger?.LogDebug("Display count set to {Count}", count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Main Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the complete dice history panel with all sections.
    /// </summary>
    /// <param name="displayDto">Complete dice history display data.</param>
    /// <remarks>
    /// <para>Renders the following sections in order:</para>
    /// <list type="number">
    ///   <item><description>Panel header "DICE HISTORY"</description></item>
    ///   <item><description>Recent rolls section</description></item>
    ///   <item><description>Statistics section (left column)</description></item>
    ///   <item><description>Luck rating section (right column)</description></item>
    ///   <item><description>Streak indicator</description></item>
    ///   <item><description>Roll distribution chart</description></item>
    ///   <item><description>Panel footer</description></item>
    /// </list>
    /// </remarks>
    public void Render(DiceHistoryDisplayDto displayDto)
    {
        ArgumentNullException.ThrowIfNull(displayDto, nameof(displayDto));

        _logger?.LogDebug(
            "Rendering dice history panel: {TotalRolls} total rolls",
            displayDto.TotalRolls);

        // Check for empty history
        if (!displayDto.HasHistory)
        {
            RenderEmptyHistory();
            return;
        }

        // Render all sections
        RenderHeader();
        RenderRecentRolls(displayDto.RecentRolls);
        RenderStatisticsAndLuck(displayDto);
        RenderStreaks(displayDto);
        RenderDistribution(displayDto);
        RenderFooter();

        _logger?.LogInformation(
            "Rendered dice history panel with {TotalRolls} total rolls, average {Average:F1}",
            displayDto.TotalRolls, displayDto.AverageRoll);
    }

    /// <summary>
    /// Renders the recent roll history section.
    /// </summary>
    /// <param name="rolls">Collection of recent dice rolls.</param>
    public void RenderHistory(IEnumerable<DiceRollRecord> rolls)
    {
        ArgumentNullException.ThrowIfNull(rolls, nameof(rolls));

        var rollList = rolls.Take(_displayCount).ToList();
        _logger?.LogDebug("Rendering recent rolls section with {Count} rolls", rollList.Count);

        var formattedRolls = _rollRenderer.FormatRollList(rollList);
        RenderRecentRollsSection(formattedRolls);
    }

    /// <summary>
    /// Shows the statistics section including total rolls, nat 20s, nat 1s, and average.
    /// </summary>
    /// <param name="totalRolls">Total number of rolls.</param>
    /// <param name="nat20s">Number of natural 20s.</param>
    /// <param name="nat20Percentage">Natural 20 percentage.</param>
    /// <param name="nat1s">Number of natural 1s.</param>
    /// <param name="nat1Percentage">Natural 1 percentage.</param>
    /// <param name="average">Average roll value.</param>
    /// <param name="expected">Expected average (10.5 for d20).</param>
    public void ShowStatistics(
        int totalRolls,
        int nat20s,
        float nat20Percentage,
        int nat1s,
        float nat1Percentage,
        float average,
        float expected)
    {
        var x = _positionX + 3;
        var y = _positionY + StatsSectionY;

        _terminal.WriteAt(x, y, "STATISTICS");
        _terminal.WriteAt(x, y + 1, new string('-', 10));

        _terminal.WriteAt(x, y + 2, $"Total d20 Rolls: {totalRolls}");
        _terminal.WriteAt(x, y + 3, $"Natural 20s: {nat20s} ({nat20Percentage:F1}%)");
        _terminal.WriteAt(x, y + 4, $"Natural 1s: {nat1s} ({nat1Percentage:F1}%)");
        _terminal.WriteAt(x, y + 5, $"Average Roll: {average:F1}");
        _terminal.WriteAt(x, y + 6, $"Expected: {expected:F1}");
    }

    /// <summary>
    /// Shows the luck rating based on deviation from expected average.
    /// </summary>
    /// <param name="deviation">Deviation percentage from expected (positive = lucky).</param>
    public void ShowLuckRating(float deviation)
    {
        var x = _positionX + 35;
        var y = _positionY + StatsSectionY;

        _terminal.WriteAt(x, y, "LUCK RATING");
        _terminal.WriteAt(x, y + 1, new string('-', 11));

        var ratingText = _config.GetLuckRatingText(deviation);
        var deviationText = deviation >= 0 ? $"(+{deviation:F1}%)" : $"({deviation:F1}%)";
        var color = _config.GetLuckColor(deviation);

        _terminal.WriteColoredAt(x, y + 2, $"{ratingText} {deviationText}", color);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Rendering Methods
    // ═══════════════════════════════════════════════════════════════════════════

    private void RenderHeader()
    {
        var headerLine = new string('=', _config.PanelWidth - 2);
        var title = "DICE HISTORY";
        var titlePadding = (_config.PanelWidth - title.Length - 2) / 2;

        _terminal.WriteAt(_positionX, _positionY + HeaderY, $"+{headerLine}+");
        _terminal.WriteAt(
            _positionX,
            _positionY + HeaderY + 1,
            $"|{new string(' ', titlePadding)}{title}{new string(' ', _config.PanelWidth - title.Length - titlePadding - 2)}|");
        _terminal.WriteAt(_positionX, _positionY + HeaderY + 2, $"+{headerLine}+");
    }

    private void RenderFooter()
    {
        var footerLine = new string('=', _config.PanelWidth - 2);
        var footerY = _positionY + DistributionSectionY + 24;
        _terminal.WriteAt(_positionX, footerY, $"+{footerLine}+");
    }

    private void RenderRecentRolls(IReadOnlyList<int> recentRolls)
    {
        var formattedRolls = _rollRenderer.FormatRollList(recentRolls);
        RenderRecentRollsSection(formattedRolls);
    }

    private void RenderRecentRollsSection(string formattedRolls)
    {
        var x = _positionX + 3;
        var y = _positionY + RecentRollsSectionY;

        _terminal.WriteAt(x, y, "RECENT ROLLS (d20)");
        _terminal.WriteAt(x, y + 1, new string('-', 18));
        _terminal.WriteAt(x, y + 2, formattedRolls);
        _terminal.WriteAt(x, y + 4, new string('-', _config.PanelWidth - 8));
    }

    private void RenderStatisticsAndLuck(DiceHistoryDisplayDto displayDto)
    {
        ShowStatistics(
            displayDto.TotalRolls,
            displayDto.NaturalTwenties,
            displayDto.NaturalTwentyPercentage,
            displayDto.NaturalOnes,
            displayDto.NaturalOnePercentage,
            displayDto.AverageRoll,
            displayDto.ExpectedAverage);

        ShowLuckRating(displayDto.LuckDeviation);
    }

    private void RenderStreaks(DiceHistoryDisplayDto displayDto)
    {
        var x = _positionX + 35;
        var y = _positionY + StatsSectionY + 4;

        _streakIndicator.RenderStreak(
            Math.Abs(displayDto.CurrentStreak),
            displayDto.IsLuckyStreak,
            x, y);

        _streakIndicator.ShowLongestStreaks(
            displayDto.LongestLuckyStreak,
            displayDto.LongestUnluckyStreak,
            x, y + 3);
    }

    private void RenderDistribution(DiceHistoryDisplayDto displayDto)
    {
        var x = _positionX + 3;
        var y = _positionY + DistributionSectionY;

        _terminal.WriteAt(x, y, "ROLL DISTRIBUTION");
        _terminal.WriteAt(x, y + 1, new string('-', 17));

        var distributionDto = new RollDistributionDto(displayDto.Distribution, displayDto.TotalRolls);
        _distributionChart.RenderDistribution(distributionDto, x, y + 3);
    }

    private void RenderEmptyHistory()
    {
        _logger?.LogWarning("No roll history available to display");

        RenderHeader();

        var x = _positionX + 3;
        var y = _positionY + RecentRollsSectionY;

        _terminal.WriteAt(x, y, "No roll history available.");
        _terminal.WriteAt(x, y + 2, "Make some dice rolls to see your statistics!");

        RenderFooter();
    }
}
