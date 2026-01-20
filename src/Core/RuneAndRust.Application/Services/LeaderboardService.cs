// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardService.cs
// Service for managing leaderboards with local JSON persistence.
// Version: 0.12.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Application.Configuration;
using System.Text.Json;

/// <summary>
/// Service for managing leaderboards with local JSON persistence.
/// </summary>
/// <remarks>
/// <para>This service:</para>
/// <list type="bullet">
///   <item><description>Loads leaderboard data from JSON on first access</description></item>
///   <item><description>Saves leaderboard data after each submission</description></item>
///   <item><description>Enforces maximum entries per category (default 100)</description></item>
///   <item><description>Calculates scores based on player statistics</description></item>
/// </list>
/// </remarks>
public class LeaderboardService : ILeaderboardService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IStatisticsService _statisticsService;
    private readonly IAchievementService _achievementService;
    private readonly LeaderboardOptions _options;
    private readonly ILogger<LeaderboardService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// In-memory list of leaderboard entries.
    /// </summary>
    private List<LeaderboardEntry> _entries = new();

    /// <summary>
    /// Flag indicating whether data has been loaded from disk.
    /// </summary>
    private bool _isLoaded;

    /// <summary>
    /// JSON serializer options for consistent serialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderboardService"/> class.
    /// </summary>
    /// <param name="statisticsService">Service for accessing player statistics.</param>
    /// <param name="achievementService">Service for accessing achievement points.</param>
    /// <param name="options">Configuration options for the leaderboard system.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <exception cref="ArgumentNullException">If any required dependency is null.</exception>
    public LeaderboardService(
        IStatisticsService statisticsService,
        IAchievementService achievementService,
        IOptions<LeaderboardOptions> options,
        ILogger<LeaderboardService> logger)
    {
        _statisticsService = statisticsService
            ?? throw new ArgumentNullException(nameof(statisticsService));
        _achievementService = achievementService
            ?? throw new ArgumentNullException(nameof(achievementService));
        _options = options?.Value
            ?? throw new ArgumentNullException(nameof(options));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "LeaderboardService initialized with MaxEntriesPerCategory={MaxEntries}, DataFilePath={DataPath}",
            _options.MaxEntriesPerCategory,
            _options.DataFilePath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public LeaderboardEntry? SubmitScore(Player player, LeaderboardCategory category)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Ensure leaderboard data is loaded from disk
        EnsureLoaded();

        _logger.LogDebug(
            "Attempting score submission for {PlayerName} in {Category}",
            player.Name,
            category);

        // Calculate the score for this category
        var score = CalculateScoreForCategory(player, category);

        // Create the entry
        var entry = LeaderboardEntry.Create(
            playerName: player.Name,
            className: player.ClassId ?? "Unknown",
            level: player.Level,
            score: score,
            category: category,
            completionTime: category == LeaderboardCategory.Speedrun
                ? GetCompletionTime(player)
                : null);

        // Check if score qualifies for leaderboard
        if (!QualifiesForLeaderboard(entry, category))
        {
            _logger.LogDebug(
                "Score {Score} did not qualify for {Category} leaderboard",
                score,
                category);
            return null;
        }

        // Add entry and enforce limits
        _entries.Add(entry);
        EnforceMaxEntries(category);
        SaveLeaderboard();

        _logger.LogInformation(
            "Score submitted: {PlayerName} scored {Score} in {Category}",
            player.Name,
            score,
            category);

        return entry;
    }

    /// <inheritdoc />
    public IReadOnlyList<LeaderboardEntry> SubmitAllScores(
        Player player,
        bool isVictory,
        TimeSpan? completionTime)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Ensure leaderboard data is loaded from disk
        EnsureLoaded();

        _logger.LogDebug(
            "Submitting all scores for {PlayerName}, isVictory={IsVictory}",
            player.Name,
            isVictory);

        var submittedEntries = new List<LeaderboardEntry>();

        // Always submit high score
        var highScoreEntry = SubmitScore(player, LeaderboardCategory.HighScore);
        if (highScoreEntry is not null)
        {
            submittedEntries.Add(highScoreEntry);
        }

        // Submit speedrun only on victory
        if (isVictory && completionTime.HasValue)
        {
            var speedrunEntry = SubmitSpeedrunScore(player, completionTime.Value);
            if (speedrunEntry is not null)
            {
                submittedEntries.Add(speedrunEntry);
            }
        }

        // Submit no-death if player hasn't died
        var statistics = _statisticsService.GetPlayerStatistics(player);
        if (statistics.DeathCount == 0)
        {
            var noDeathEntry = SubmitScore(player, LeaderboardCategory.NoDeath);
            if (noDeathEntry is not null)
            {
                submittedEntries.Add(noDeathEntry);
            }
        }

        // Achievement points
        var achievementEntry = SubmitScore(player, LeaderboardCategory.AchievementPoints);
        if (achievementEntry is not null)
        {
            submittedEntries.Add(achievementEntry);
        }

        // Boss slayer
        var bossEntry = SubmitScore(player, LeaderboardCategory.BossSlayer);
        if (bossEntry is not null)
        {
            submittedEntries.Add(bossEntry);
        }

        _logger.LogInformation(
            "Submitted {Count} scores for {PlayerName}",
            submittedEntries.Count,
            player.Name);

        return submittedEntries.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<LeaderboardEntry> GetLeaderboard(
        LeaderboardCategory category,
        int count = 10)
    {
        EnsureLoaded();

        _logger.LogDebug(
            "Getting leaderboard for {Category}, count={Count}",
            category,
            count);

        // Filter entries by category
        var categoryEntries = _entries
            .Where(e => e.Category == category)
            .ToList();

        // Sort based on category rules:
        // - Speedrun: ascending (lower time is better)
        // - All others: descending (higher score is better)
        var sorted = category == LeaderboardCategory.Speedrun
            ? categoryEntries.OrderBy(e => e.Score).ToList()
            : categoryEntries.OrderByDescending(e => e.Score).ToList();

        return sorted.Take(count).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public int GetPlayerRank(Player player, LeaderboardCategory category)
    {
        ArgumentNullException.ThrowIfNull(player);
        EnsureLoaded();

        // Get full leaderboard for category (up to max entries)
        var leaderboard = GetLeaderboard(category, _options.MaxEntriesPerCategory);

        // Find player's position (1-indexed)
        var index = leaderboard.ToList().FindIndex(e =>
            e.PlayerName.Equals(player.Name, StringComparison.OrdinalIgnoreCase));

        var rank = index >= 0 ? index + 1 : 0;

        _logger.LogDebug(
            "Player {PlayerName} rank in {Category}: {Rank}",
            player.Name,
            category,
            rank);

        return rank;
    }

    /// <inheritdoc />
    public LeaderboardEntry? GetPlayerBest(Player player, LeaderboardCategory category)
    {
        ArgumentNullException.ThrowIfNull(player);
        EnsureLoaded();

        // Find all entries for this player in this category
        var playerEntries = _entries
            .Where(e => e.Category == category &&
                        e.PlayerName.Equals(player.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (playerEntries.Count == 0)
        {
            _logger.LogDebug(
                "No entries found for {PlayerName} in {Category}",
                player.Name,
                category);
            return null;
        }

        // Return best entry based on category sort rules
        var best = category == LeaderboardCategory.Speedrun
            ? playerEntries.MinBy(e => e.Score)
            : playerEntries.MaxBy(e => e.Score);

        _logger.LogDebug(
            "Best entry for {PlayerName} in {Category}: {Score}",
            player.Name,
            category,
            best?.Score ?? 0);

        return best;
    }

    /// <inheritdoc />
    public long CalculateScore(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Get player statistics and achievement points
        var statistics = _statisticsService.GetPlayerStatistics(player);
        var achievementPoints = _achievementService.GetTotalPoints(player);

        // Calculate base score from activities
        // Formula: (monstersKilled × 10) + (bossesKilled × 500) + (roomsDiscovered × 25) + goldEarned
        long baseScore = 0;
        baseScore += statistics.MonstersKilled * 10L;
        baseScore += statistics.BossesKilled * 500L;
        baseScore += statistics.RoomsDiscovered * 25L;
        baseScore += statistics.GoldEarned;

        // Apply level multiplier (10% bonus per level)
        var levelMultiplier = 1.0 + (player.Level * 0.1);

        // Calculate achievement bonus (achievement points × 10)
        var achievementBonus = achievementPoints * 10L;

        // Calculate death penalty (deaths × 100)
        var deathPenalty = statistics.DeathCount * 100L;

        // Final score calculation
        var finalScore = (long)((baseScore * levelMultiplier) + achievementBonus - deathPenalty);

        // Ensure score is never negative
        finalScore = Math.Max(0, finalScore);

        _logger.LogDebug(
            "Calculated score for {PlayerName}: base={BaseScore}, multiplier={Multiplier:F2}, " +
            "achievement={AchievementBonus}, penalty={DeathPenalty}, final={FinalScore}",
            player.Name,
            baseScore,
            levelMultiplier,
            achievementBonus,
            deathPenalty,
            finalScore);

        return finalScore;
    }

    /// <inheritdoc />
    public int GetTotalEntryCount()
    {
        EnsureLoaded();
        return _entries.Count;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the score for a specific category.
    /// </summary>
    /// <param name="player">The player to calculate score for.</param>
    /// <param name="category">The leaderboard category.</param>
    /// <returns>The calculated score for the specified category.</returns>
    private long CalculateScoreForCategory(Player player, LeaderboardCategory category)
    {
        var statistics = _statisticsService.GetPlayerStatistics(player);

        return category switch
        {
            LeaderboardCategory.HighScore => CalculateScore(player),
            LeaderboardCategory.Speedrun => (long)(GetCompletionTime(player)?.TotalSeconds ?? long.MaxValue),
            LeaderboardCategory.NoDeath => player.Level,
            LeaderboardCategory.AchievementPoints => _achievementService.GetTotalPoints(player),
            LeaderboardCategory.BossSlayer => statistics.BossesKilled,
            _ => 0
        };
    }

    /// <summary>
    /// Submits a speedrun score with completion time.
    /// </summary>
    /// <param name="player">The player submitting the score.</param>
    /// <param name="completionTime">The total completion time.</param>
    /// <returns>The created entry, or null if it didn't qualify.</returns>
    private LeaderboardEntry? SubmitSpeedrunScore(Player player, TimeSpan completionTime)
    {
        var entry = LeaderboardEntry.Create(
            playerName: player.Name,
            className: player.ClassId ?? "Unknown",
            level: player.Level,
            score: (long)completionTime.TotalSeconds,
            category: LeaderboardCategory.Speedrun,
            completionTime: completionTime);

        if (!QualifiesForLeaderboard(entry, LeaderboardCategory.Speedrun))
        {
            _logger.LogDebug(
                "Speedrun time {Time} did not qualify for leaderboard",
                completionTime);
            return null;
        }

        _entries.Add(entry);
        EnforceMaxEntries(LeaderboardCategory.Speedrun);
        SaveLeaderboard();

        _logger.LogInformation(
            "Speedrun submitted: {PlayerName} completed in {Time}",
            player.Name,
            completionTime);

        return entry;
    }

    /// <summary>
    /// Gets the completion time from player statistics.
    /// </summary>
    /// <param name="player">The player to get completion time for.</param>
    /// <returns>The total playtime as completion time, or null if zero.</returns>
    private TimeSpan? GetCompletionTime(Player player)
    {
        var statistics = _statisticsService.GetPlayerStatistics(player);
        return statistics.TotalPlaytime > TimeSpan.Zero
            ? statistics.TotalPlaytime
            : null;
    }

    /// <summary>
    /// Checks if a score qualifies for the leaderboard.
    /// </summary>
    /// <param name="entry">The entry to check.</param>
    /// <param name="category">The category to check against.</param>
    /// <returns>True if the score qualifies, false otherwise.</returns>
    private bool QualifiesForLeaderboard(LeaderboardEntry entry, LeaderboardCategory category)
    {
        // Get current entries for this category
        var categoryEntries = _entries
            .Where(e => e.Category == category)
            .ToList();

        // If below max entries, always qualifies
        if (categoryEntries.Count < _options.MaxEntriesPerCategory)
        {
            return true;
        }

        // For speedrun, lower score (faster time) is better
        if (category == LeaderboardCategory.Speedrun)
        {
            var highestTime = categoryEntries.Max(e => e.Score);
            return entry.Score < highestTime;
        }

        // For other categories, higher score is better
        var lowestScore = categoryEntries.Min(e => e.Score);
        return entry.Score > lowestScore;
    }

    /// <summary>
    /// Enforces the maximum entries limit for a category.
    /// </summary>
    /// <param name="category">The category to enforce limits on.</param>
    private void EnforceMaxEntries(LeaderboardCategory category)
    {
        var categoryEntries = _entries
            .Where(e => e.Category == category)
            .ToList();

        // If within limits, nothing to do
        if (categoryEntries.Count <= _options.MaxEntriesPerCategory)
        {
            return;
        }

        // Sort and keep only top entries
        var sorted = category == LeaderboardCategory.Speedrun
            ? categoryEntries.OrderBy(e => e.Score).ToList()
            : categoryEntries.OrderByDescending(e => e.Score).ToList();

        var toRemove = sorted.Skip(_options.MaxEntriesPerCategory).ToList();

        foreach (var entry in toRemove)
        {
            _entries.Remove(entry);
        }

        _logger.LogDebug(
            "Enforced max entries for {Category}, removed {Count} entries",
            category,
            toRemove.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERSISTENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures leaderboard data is loaded from disk.
    /// </summary>
    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        LoadLeaderboard();
        _isLoaded = true;
    }

    /// <summary>
    /// Loads leaderboard data from the JSON file.
    /// </summary>
    private void LoadLeaderboard()
    {
        var filePath = _options.DataFilePath;

        if (!File.Exists(filePath))
        {
            _logger.LogInformation(
                "Leaderboard file not found at {FilePath}, starting with empty leaderboard",
                filePath);
            _entries = new List<LeaderboardEntry>();
            return;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<LeaderboardData>(json, JsonOptions);

            _entries = data?.Entries ?? new List<LeaderboardEntry>();

            _logger.LogInformation(
                "Loaded {Count} leaderboard entries from {FilePath}",
                _entries.Count,
                filePath);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse leaderboard file at {FilePath}", filePath);
            _entries = new List<LeaderboardEntry>();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to read leaderboard file at {FilePath}", filePath);
            _entries = new List<LeaderboardEntry>();
        }
    }

    /// <summary>
    /// Saves leaderboard data to the JSON file.
    /// </summary>
    private void SaveLeaderboard()
    {
        var filePath = _options.DataFilePath;

        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory {Directory} for leaderboard data", directory);
            }

            var data = new LeaderboardData { Entries = _entries };
            var json = JsonSerializer.Serialize(data, JsonOptions);

            File.WriteAllText(filePath, json);

            _logger.LogDebug(
                "Saved {Count} leaderboard entries to {FilePath}",
                _entries.Count,
                filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save leaderboard file to {FilePath}", filePath);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DATA MODELS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Data model for JSON serialization of leaderboard data.
    /// </summary>
    private class LeaderboardData
    {
        /// <summary>
        /// Gets or sets the list of leaderboard entries.
        /// </summary>
        public List<LeaderboardEntry> Entries { get; set; } = new();
    }
}
