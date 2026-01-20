// ═══════════════════════════════════════════════════════════════════════════════
// IAchievementService.cs
// Interface for the achievement checking and tracking service.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for checking, tracking, and managing player achievements.
/// </summary>
/// <remarks>
/// <para>
/// The achievement service provides a centralized API for:
/// </para>
/// <list type="bullet">
///   <item><description>Checking achievement conditions against player statistics</description></item>
///   <item><description>Unlocking achievements when all conditions are met</description></item>
///   <item><description>Tracking progress towards locked achievements</description></item>
///   <item><description>Querying unlocked achievements and total points</description></item>
/// </list>
/// <para>
/// This service should be called after player statistics are updated to detect
/// newly unlocked achievements. It integrates with:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="IAchievementProvider"/> for reading achievement definitions</description></item>
///   <item><description><see cref="IStatisticsService"/> for reading player statistics</description></item>
///   <item><description><see cref="IGameRenderer"/> for displaying unlock notifications</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // After a monster is killed
/// statisticsService.RecordMonsterKill(player, "goblin");
/// achievementService.CheckAchievements(player);
/// 
/// // Display achievements
/// var progress = achievementService.GetProgress(player);
/// foreach (var p in progress.Where(p => p.IsUnlocked))
/// {
///     Console.WriteLine($"✓ {p.Definition.Name}");
/// }
/// 
/// // Get total points
/// var points = achievementService.GetTotalPoints(player);
/// </code>
/// </example>
public interface IAchievementService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ACHIEVEMENT CHECKING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks all achievements and unlocks any whose conditions are now met.
    /// </summary>
    /// <param name="player">The player to check achievements for. Must not be null.</param>
    /// <returns>
    /// A list of achievement IDs that were newly unlocked during this check.
    /// Returns an empty list if no achievements unlocked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called after statistics are updated to detect
    /// newly unlocked achievements. It evaluates all achievement conditions
    /// against the player's current statistics.
    /// </para>
    /// <para>
    /// For each achievement that unlocks, this method:
    /// </para>
    /// <list type="number">
    ///   <item><description>Adds the achievement to the player's collection via <see cref="Player.AddAchievement"/></description></item>
    ///   <item><description>Creates an <see cref="Events.AchievementUnlockedEvent"/></description></item>
    ///   <item><description>Displays an unlock notification</description></item>
    ///   <item><description>Logs the achievement unlock</description></item>
    /// </list>
    /// <para>
    /// Already unlocked achievements are skipped. Achievements with no conditions
    /// are logged as warnings and never unlock.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After combat
    /// var newlyUnlocked = achievementService.CheckAchievements(player);
    /// if (newlyUnlocked.Count > 0)
    /// {
    ///     Console.WriteLine($"Unlocked {newlyUnlocked.Count} achievements!");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<string> CheckAchievements(Player player);

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all achievements the player has unlocked.
    /// </summary>
    /// <param name="player">The player to query. Must not be null.</param>
    /// <returns>
    /// Read-only list of unlocked player achievements.
    /// Returns an empty list if no achievements are unlocked.
    /// </returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Player.Achievements"/>.
    /// </remarks>
    IReadOnlyList<PlayerAchievement> GetUnlockedAchievements(Player player);

    /// <summary>
    /// Gets progress information for all achievements.
    /// </summary>
    /// <param name="player">The player to get progress for. Must not be null.</param>
    /// <returns>
    /// Progress records for all achievements, including both locked and unlocked.
    /// Ordered by category and then by tier.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method calculates progress for each achievement based on the player's
    /// current statistics. For unlocked achievements, <see cref="AchievementProgress.OverallProgress"/>
    /// is always 1.0.
    /// </para>
    /// <para>
    /// Use this for displaying the achievements screen with progress bars.
    /// </para>
    /// </remarks>
    IReadOnlyList<AchievementProgress> GetProgress(Player player);

    /// <summary>
    /// Gets progress for achievements in a specific category.
    /// </summary>
    /// <param name="player">The player to get progress for. Must not be null.</param>
    /// <param name="category">The category to filter by.</param>
    /// <returns>
    /// Progress records for achievements in the specified category.
    /// Returns an empty list if no achievements exist in the category.
    /// </returns>
    /// <remarks>
    /// Use this for filtered achievement views (e.g., Combat tab, Exploration tab).
    /// </remarks>
    IReadOnlyList<AchievementProgress> GetProgressByCategory(
        Player player,
        AchievementCategory category);

    /// <summary>
    /// Gets the total achievement points earned by the player.
    /// </summary>
    /// <param name="player">The player to query. Must not be null.</param>
    /// <returns>
    /// Total points from all unlocked achievements.
    /// Returns 0 if no achievements are unlocked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience wrapper around <see cref="Player.GetTotalAchievementPoints"/>.
    /// Points are captured at unlock time, so this reflects historical totals.
    /// </para>
    /// <para>
    /// Use this for leaderboard calculations or UI display.
    /// </para>
    /// </remarks>
    int GetTotalPoints(Player player);

    /// <summary>
    /// Checks if a specific achievement is unlocked.
    /// </summary>
    /// <param name="player">The player to query. Must not be null.</param>
    /// <param name="achievementId">
    /// The achievement ID to check. Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// <c>true</c> if the achievement is unlocked; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Player.HasAchievement"/>.
    /// </remarks>
    bool IsUnlocked(Player player, string achievementId);

    /// <summary>
    /// Gets the count of unlocked achievements.
    /// </summary>
    /// <param name="player">The player to query. Must not be null.</param>
    /// <returns>Number of unlocked achievements.</returns>
    int GetUnlockedCount(Player player);
}
