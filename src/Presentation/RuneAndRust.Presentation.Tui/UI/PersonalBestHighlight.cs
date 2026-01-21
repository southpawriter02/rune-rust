// ═══════════════════════════════════════════════════════════════════════════════
// PersonalBestHighlight.cs
// UI component for displaying the player's personal best entry with highlighting.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays and highlights the player's personal best entry.
/// </summary>
/// <remarks>
/// <para>
/// Shows the player's best score/time/floor count with their rank.
/// Can display a "NEW RECORD!" notification when a record is broken.
/// </para>
/// <para>Display format:</para>
/// <code>
/// Your Best: 52,340 (Rank #5)
/// *** NEW RECORD! ***
/// </code>
/// </remarks>
public class PersonalBestHighlight
{
    private readonly ITerminalService _terminalService;
    private readonly LeaderboardViewConfig _config;
    private readonly ILogger<PersonalBestHighlight>? _logger;

    private bool _isNewRecord;

    /// <summary>
    /// Creates a new instance of the PersonalBestHighlight component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public PersonalBestHighlight(
        ITerminalService terminalService,
        LeaderboardViewConfig? config = null,
        ILogger<PersonalBestHighlight>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new LeaderboardViewConfig();
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the player's personal best entry.
    /// </summary>
    /// <param name="personalBest">The player's best entry.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void RenderPersonalBest(LeaderboardDisplayDto personalBest, int x, int y)
    {
        var bestText = $"Your Best: {personalBest.Score:N0} (Rank #{personalBest.Rank})";
        _terminalService.WriteAt(x, y, bestText);

        if (_isNewRecord)
        {
            _terminalService.WriteColoredAt(x, y + 1, "*** NEW RECORD! ***", _config.NewRecordColor);
            _isNewRecord = false;
        }

        _logger?.LogDebug(
            "Rendered personal best: Rank {Rank}, Score {Score}",
            personalBest.Rank, personalBest.Score);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECORD MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Compares two entries to determine if a new record was set.
    /// </summary>
    /// <param name="current">The current entry.</param>
    /// <param name="previous">The previous best entry.</param>
    /// <remarks>
    /// Sets the new record flag if the current score exceeds the previous.
    /// </remarks>
    public void CompareToEntry(LeaderboardDisplayDto current, LeaderboardDisplayDto previous)
    {
        if (current.Score > previous.Score)
        {
            _isNewRecord = true;
            _logger?.LogInformation(
                "New record set! Previous: {Previous}, New: {New}",
                previous.Score, current.Score);
        }
    }

    /// <summary>
    /// Sets the new record flag for display.
    /// </summary>
    public void ShowNewRecord()
    {
        _isNewRecord = true;
        _logger?.LogDebug("New record flag set");
    }

    /// <summary>
    /// Clears the new record flag.
    /// </summary>
    public void ClearNewRecord()
    {
        _isNewRecord = false;
    }

    /// <summary>
    /// Gets whether the new record flag is currently set.
    /// </summary>
    public bool IsNewRecord => _isNewRecord;
}
