// ═══════════════════════════════════════════════════════════════════════════════
// ILeaderboardService.cs
// Service interface for managing leaderboard entries and score submission.
// Version: 0.12.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Service for managing leaderboard entries and score submission.
/// </summary>
/// <remarks>
/// <para>The leaderboard service provides:</para>
/// <list type="bullet">
///   <item><description>Score submission for completed runs</description></item>
///   <item><description>Leaderboard retrieval by category</description></item>
///   <item><description>Player ranking queries</description></item>
///   <item><description>Score calculation for different categories</description></item>
/// </list>
/// <para>
/// Leaderboard data is persisted locally in JSON format for offline play support.
/// Each category maintains a maximum of 100 entries, with the lowest-scoring
/// entries removed when the limit is exceeded.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Submit a score after game completion
/// var entry = leaderboardService.SubmitScore(player, LeaderboardCategory.HighScore);
/// if (entry is not null)
/// {
///     Console.WriteLine($"New high score: {entry.Score:N0}");
/// }
/// 
/// // Get top 10 entries
/// var topScores = leaderboardService.GetLeaderboard(LeaderboardCategory.HighScore, 10);
/// foreach (var score in topScores)
/// {
///     Console.WriteLine($"{score.PlayerName}: {score.GetScoreDisplay()}");
/// }
/// 
/// // Check player's rank
/// var rank = leaderboardService.GetPlayerRank(player, LeaderboardCategory.HighScore);
/// Console.WriteLine($"Your rank: #{rank}");
/// </code>
/// </example>
public interface ILeaderboardService
{
    /// <summary>
    /// Submits a score for a player in a specific category.
    /// </summary>
    /// <param name="player">The player submitting the score.</param>
    /// <param name="category">The leaderboard category.</param>
    /// <returns>The created entry, or null if score didn't qualify.</returns>
    /// <remarks>
    /// <para>
    /// A score qualifies for the leaderboard if:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>The category has fewer than the maximum entries (100)</description></item>
    ///   <item><description>Or the score beats the lowest score for descending categories</description></item>
    ///   <item><description>Or the score beats the highest score for ascending categories (Speedrun)</description></item>
    /// </list>
    /// </remarks>
    LeaderboardEntry? SubmitScore(Player player, LeaderboardCategory category);

    /// <summary>
    /// Submits scores for all applicable categories.
    /// </summary>
    /// <param name="player">The player submitting scores.</param>
    /// <param name="isVictory">Whether the game was completed successfully.</param>
    /// <param name="completionTime">The total playtime if victory.</param>
    /// <returns>List of entries that qualified for the leaderboards.</returns>
    /// <remarks>
    /// <para>
    /// This method automatically submits to applicable categories:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>HighScore - Always submitted</description></item>
    ///   <item><description>Speedrun - Only on victory with completion time</description></item>
    ///   <item><description>NoDeath - Only if player has zero deaths</description></item>
    ///   <item><description>AchievementPoints - Always submitted</description></item>
    ///   <item><description>BossSlayer - Always submitted</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<LeaderboardEntry> SubmitAllScores(
        Player player,
        bool isVictory,
        TimeSpan? completionTime);

    /// <summary>
    /// Gets the leaderboard entries for a category.
    /// </summary>
    /// <param name="category">The category to retrieve.</param>
    /// <param name="count">Maximum number of entries to return. Default is 10.</param>
    /// <returns>Ranked list of leaderboard entries.</returns>
    /// <remarks>
    /// Entries are sorted by score:
    /// <list type="bullet">
    ///   <item><description>Descending for most categories (highest score first)</description></item>
    ///   <item><description>Ascending for Speedrun (fastest time first)</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<LeaderboardEntry> GetLeaderboard(
        LeaderboardCategory category,
        int count = 10);

    /// <summary>
    /// Gets the player's rank in a specific category.
    /// </summary>
    /// <param name="player">The player to look up.</param>
    /// <param name="category">The category to check.</param>
    /// <returns>The rank (1-based), or 0 if not on the leaderboard.</returns>
    int GetPlayerRank(Player player, LeaderboardCategory category);

    /// <summary>
    /// Gets the player's best entry in a category.
    /// </summary>
    /// <param name="player">The player to look up.</param>
    /// <param name="category">The category to check.</param>
    /// <returns>The player's best entry, or null if none exists.</returns>
    LeaderboardEntry? GetPlayerBest(Player player, LeaderboardCategory category);

    /// <summary>
    /// Calculates the high score for a player.
    /// </summary>
    /// <param name="player">The player to calculate score for.</param>
    /// <returns>The calculated high score.</returns>
    /// <remarks>
    /// <para>
    /// High score formula:
    /// </para>
    /// <para>
    /// baseScore = (monstersKilled × 10) + (bossesKilled × 500) + (roomsDiscovered × 25) + goldEarned
    /// </para>
    /// <para>
    /// levelMultiplier = 1.0 + (level × 0.1)
    /// </para>
    /// <para>
    /// achievementBonus = achievementPoints × 10
    /// </para>
    /// <para>
    /// deathPenalty = totalDeaths × 100
    /// </para>
    /// <para>
    /// finalScore = (baseScore × levelMultiplier) + achievementBonus - deathPenalty
    /// </para>
    /// </remarks>
    long CalculateScore(Player player);

    /// <summary>
    /// Gets the total number of entries across all categories.
    /// </summary>
    /// <returns>Total entry count.</returns>
    int GetTotalEntryCount();
}
