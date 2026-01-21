// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardView.cs
// Main UI component for displaying the leaderboard panel.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the leaderboard with ranked entries organized by category.
/// </summary>
/// <remarks>
/// <para>
/// Leaderboards show ranked player entries for various categories
/// (High Score, Speedrun, No Death, Achievement Points, Boss Slayer).
/// </para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>Category tabs with key mappings (1-5)</description></item>
///   <item><description>Top 3 special highlighting (Gold/Silver/Bronze)</description></item>
///   <item><description>Current player highlighting (Cyan)</description></item>
///   <item><description>Personal best section</description></item>
///   <item><description>Score breakdown panel</description></item>
/// </list>
/// </remarks>
public class LeaderboardView
{
    private readonly LeaderboardEntryRenderer _entryRenderer;
    private readonly PersonalBestHighlight _personalBestHighlight;
    private readonly ITerminalService _terminalService;
    private readonly LeaderboardViewConfig _config;
    private readonly ILogger<LeaderboardView>? _logger;

    private LeaderboardCategory _activeCategory;
    private IReadOnlyList<LeaderboardDisplayDto> _entries = Array.Empty<LeaderboardDisplayDto>();
    private LeaderboardDisplayDto? _playerBest;
    private string _currentPlayerId = string.Empty;
    private (int X, int Y) _panelPosition;

    /// <summary>
    /// Category definitions with display names.
    /// </summary>
    private static readonly (string Name, LeaderboardCategory Category)[] CategoryDefinitions =
    {
        ("High Score", LeaderboardCategory.HighScore),
        ("Speedrun", LeaderboardCategory.Speedrun),
        ("No Death", LeaderboardCategory.NoDeath),
        ("Ach Pts", LeaderboardCategory.AchievementPoints),
        ("Boss Slayer", LeaderboardCategory.BossSlayer)
    };

    /// <summary>
    /// Gets the currently active leaderboard category.
    /// </summary>
    public LeaderboardCategory ActiveCategory => _activeCategory;

    /// <summary>
    /// Creates a new instance of the LeaderboardView component.
    /// </summary>
    /// <param name="entryRenderer">The entry renderer for formatting entries.</param>
    /// <param name="personalBestHighlight">The personal best highlight component.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for leaderboard display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public LeaderboardView(
        LeaderboardEntryRenderer entryRenderer,
        PersonalBestHighlight personalBestHighlight,
        ITerminalService terminalService,
        LeaderboardViewConfig? config = null,
        ILogger<LeaderboardView>? logger = null)
    {
        _entryRenderer = entryRenderer ?? throw new ArgumentNullException(nameof(entryRenderer));
        _personalBestHighlight = personalBestHighlight ?? throw new ArgumentNullException(nameof(personalBestHighlight));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new LeaderboardViewConfig();
        _logger = logger;

        _activeCategory = LeaderboardCategory.HighScore;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the panel position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _panelPosition = (x, y);
    }

    /// <summary>
    /// Sets the current player ID for highlighting.
    /// </summary>
    /// <param name="playerId">The current player's ID.</param>
    public void SetCurrentPlayer(string playerId)
    {
        _currentPlayerId = playerId;
    }

    /// <summary>
    /// Sets the player's personal best entry for display.
    /// </summary>
    /// <param name="personalBest">The player's best entry.</param>
    public void SetPersonalBest(LeaderboardDisplayDto? personalBest)
    {
        _playerBest = personalBest;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CATEGORY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Selects a leaderboard category.
    /// </summary>
    /// <param name="category">The category to select.</param>
    public void SelectCategory(LeaderboardCategory category)
    {
        _activeCategory = category;
        _logger?.LogDebug("Leaderboard category changed to: {Category}", category);
    }

    /// <summary>
    /// Handles category input from user key press.
    /// </summary>
    /// <param name="key">The pressed key (1-5).</param>
    /// <returns>True if the category was changed, false if the key was invalid.</returns>
    public bool HandleCategoryInput(int key)
    {
        var category = key switch
        {
            1 => LeaderboardCategory.HighScore,
            2 => LeaderboardCategory.Speedrun,
            3 => LeaderboardCategory.NoDeath,
            4 => LeaderboardCategory.AchievementPoints,
            5 => LeaderboardCategory.BossSlayer,
            _ => _activeCategory
        };

        if (category != _activeCategory)
        {
            SelectCategory(category);
            return true;
        }

        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the leaderboard with the given entries.
    /// </summary>
    /// <param name="entries">The leaderboard entries to display.</param>
    public void RenderLeaderboard(IEnumerable<LeaderboardDisplayDto> entries)
    {
        _entries = entries.ToList();

        _logger?.LogDebug(
            "Rendering leaderboard: {EntryCount} entries for {Category}",
            _entries.Count, _activeCategory);

        // Clear panel area
        ClearPanel();

        // Render panel header
        RenderHeader();

        // Render category tabs
        RenderCategoryTabs();

        // Render column headers
        RenderColumnHeaders();

        // Render each entry
        RenderEntries();

        // Render personal best if available
        if (_playerBest is not null)
        {
            RenderPersonalBestSection();
        }

        _logger?.LogInformation(
            "Leaderboard rendered with {Count} entries for {Category}",
            _entries.Count, _activeCategory);
    }

    /// <summary>
    /// Highlights a specific player's entry.
    /// </summary>
    /// <param name="entry">The entry to highlight.</param>
    public void HighlightPlayerEntry(LeaderboardDisplayDto entry)
    {
        _logger?.LogDebug("Highlighting player entry: {PlayerName}", entry.PlayerName);
    }

    /// <summary>
    /// Shows the player's rank out of total entries.
    /// </summary>
    /// <param name="rank">The player's rank.</param>
    /// <param name="total">The total number of entries.</param>
    public void ShowPlayerRank(int rank, int total)
    {
        var rankText = $"Your Rank: #{rank} of {total}";
        _terminalService.WriteAt(
            _panelPosition.X + 2,
            _panelPosition.Y + _config.PanelHeight - 3,
            rankText);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE RENDERING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private void ClearPanel()
    {
        for (int y = 0; y < _config.PanelHeight; y++)
        {
            _terminalService.WriteAt(
                _panelPosition.X,
                _panelPosition.Y + y,
                new string(' ', _config.PanelWidth));
        }
    }

    private void RenderHeader()
    {
        var headerLine = new string('=', _config.PanelWidth - 2);
        var title = "LEADERBOARDS";
        var titlePadding = (_config.PanelWidth - title.Length - 2) / 2;

        _terminalService.WriteAt(_panelPosition.X, _panelPosition.Y, $"+{headerLine}+");
        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 1,
            $"|{new string(' ', titlePadding)}{title}{new string(' ', _config.PanelWidth - title.Length - titlePadding - 2)}|");
        _terminalService.WriteAt(_panelPosition.X, _panelPosition.Y + 2, $"+{headerLine}+");
    }

    private void RenderCategoryTabs()
    {
        var tabLine = " ";
        for (int i = 0; i < CategoryDefinitions.Length; i++)
        {
            var (name, category) = CategoryDefinitions[i];
            var isActive = category == _activeCategory;
            var keyNum = i + 1;
            var tabText = isActive ? $"[{keyNum}.{name}]" : $" {keyNum}.{name} ";
            tabLine += tabText + " ";
        }

        var paddedTabs = tabLine.PadRight(_config.PanelWidth - 2);
        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 3,
            $"|{paddedTabs}|");
        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 4,
            $"+{new string('=', _config.PanelWidth - 2)}+");
    }

    private void RenderColumnHeaders()
    {
        var categoryHeader = GetCategoryDisplayName(_activeCategory);
        var headerY = _panelPosition.Y + 6;

        _terminalService.WriteAt(_panelPosition.X + 3, headerY, $"{categoryHeader} LEADERBOARD");
        _terminalService.WriteAt(_panelPosition.X + 3, headerY + 1, new string('-', categoryHeader.Length + 12));

        // Column headers vary by category
        var columnHeader = GetColumnHeader();
        _terminalService.WriteAt(_panelPosition.X + 3, headerY + 3, columnHeader);
        _terminalService.WriteAt(_panelPosition.X + 3, headerY + 4, new string('=', _config.PanelWidth - 8));
    }

    private string GetColumnHeader()
    {
        var rankCol = "Rank".PadRight(_config.RankColumnWidth);
        var nameCol = "Name".PadRight(_config.NameColumnWidth);
        var classCol = "Class".PadRight(_config.ClassColumnWidth);
        var levelCol = "Level".PadRight(_config.LevelColumnWidth);
        var dateCol = "Date".PadRight(_config.DateColumnWidth);

        var scoreLabel = _activeCategory switch
        {
            LeaderboardCategory.Speedrun => "Time",
            LeaderboardCategory.NoDeath => "Floors",
            LeaderboardCategory.AchievementPoints => "Points",
            LeaderboardCategory.BossSlayer => "Bosses",
            _ => "Score"
        };
        var scoreCol = scoreLabel.PadRight(_config.ScoreColumnWidth);

        return $"{rankCol}{nameCol}{classCol}{levelCol}{scoreCol}{dateCol}";
    }

    private string GetCategoryDisplayName(LeaderboardCategory category)
    {
        return category switch
        {
            LeaderboardCategory.HighScore => "HIGH SCORE",
            LeaderboardCategory.Speedrun => "SPEEDRUN",
            LeaderboardCategory.NoDeath => "NO DEATH",
            LeaderboardCategory.AchievementPoints => "ACHIEVEMENT POINTS",
            LeaderboardCategory.BossSlayer => "BOSS SLAYER",
            _ => category.ToString().ToUpperInvariant()
        };
    }

    private void RenderEntries()
    {
        var startY = _panelPosition.Y + 11;
        var currentY = startY;

        foreach (var entry in _entries.Take(_config.MaxDisplayEntries))
        {
            var formattedEntry = _entryRenderer.FormatEntry(entry, _activeCategory, _config);
            var color = _entryRenderer.GetRankColor(entry.Rank, entry.IsCurrentPlayer);
            _terminalService.WriteColoredAt(_panelPosition.X + 3, currentY, formattedEntry, color);
            currentY++;
        }
    }

    private void RenderPersonalBestSection()
    {
        var separatorY = _panelPosition.Y + 11 + _config.MaxDisplayEntries + 1;
        _terminalService.WriteAt(
            _panelPosition.X + 3,
            separatorY,
            new string('-', _config.PanelWidth - 8));

        _personalBestHighlight.RenderPersonalBest(
            _playerBest!,
            _panelPosition.X + 3,
            separatorY + 1);
    }
}
