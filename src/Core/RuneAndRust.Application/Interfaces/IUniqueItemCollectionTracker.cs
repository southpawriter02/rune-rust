namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Interface for tracking unique item collection progress and achievements.
/// Provides hooks for the future achievement system.
/// </summary>
/// <remarks>
/// <para>
/// IUniqueItemCollectionTracker serves as the integration point between the
/// Myth-Forged item drop system (v0.16.5) and a future achievement system.
/// Implementations will maintain persistent collection state across game sessions.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
///   <item><description>Record unique item drops as they occur</description></item>
///   <item><description>Provide collection statistics for UI display</description></item>
///   <item><description>Calculate progress toward collection achievements</description></item>
///   <item><description>Determine which achievements have been earned</description></item>
/// </list>
/// </para>
/// <para>
/// This interface is designed for future integration:
/// <code>
/// // Future: Achievement service can subscribe to drops
/// services.AddScoped&lt;IUniqueItemCollectionTracker, UniqueItemCollectionTracker&gt;();
/// 
/// // Future: Check achievements after drops
/// if (tracker.IsAchievementEarned(UniqueAchievementType.CollectorBronze))
/// {
///     achievementService.Award(UniqueAchievementType.CollectorBronze);
/// }
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="UniqueItemAchievements"/>
/// <seealso cref="UniqueAchievementType"/>
/// <seealso cref="AchievementThreshold"/>
public interface IUniqueItemCollectionTracker
{
    /// <summary>
    /// Called when a unique item drops.
    /// Records the drop and updates collection statistics.
    /// </summary>
    /// <param name="dropEvent">The drop event from v0.16.5e.</param>
    /// <remarks>
    /// <para>
    /// Implementations should:
    /// <list type="number">
    ///   <item><description>Record the item in the collection tracker</description></item>
    ///   <item><description>Update class-specific counts based on item affinities</description></item>
    ///   <item><description>Set timestamp markers (first drop, last drop)</description></item>
    ///   <item><description>Persist the updated state if applicable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In the loot generation service
    /// var dropEvent = MythForgedDropEvent.Create(item, sourceType, sourceId, isFirst, level);
    /// _collectionTracker.OnUniqueDropped(dropEvent);
    /// </code>
    /// </example>
    void OnUniqueDropped(MythForgedDropEvent dropEvent);

    /// <summary>
    /// Gets the current collection statistics.
    /// </summary>
    /// <returns>Achievement progress data including found items and timestamps.</returns>
    /// <remarks>
    /// The returned <see cref="UniqueItemAchievements"/> contains comprehensive
    /// statistics including total found, per-class counts, and timestamps.
    /// </remarks>
    UniqueItemAchievements GetCollectionStats();

    /// <summary>
    /// Gets all achievements that have been earned.
    /// </summary>
    /// <returns>List of earned achievement types.</returns>
    /// <remarks>
    /// Does not include <see cref="UniqueAchievementType.ClassMaster"/> achievements
    /// as they require class-specific checking via <see cref="IsAchievementEarned"/>.
    /// </remarks>
    IReadOnlyList<UniqueAchievementType> GetEarnedAchievements();

    /// <summary>
    /// Gets progress toward a specific achievement.
    /// </summary>
    /// <param name="type">The achievement type to check.</param>
    /// <param name="classId">Class ID for ClassMaster achievements (optional).</param>
    /// <returns>Progress from 0.0 to 1.0.</returns>
    /// <example>
    /// <code>
    /// var progress = tracker.GetProgressToward(UniqueAchievementType.CollectorBronze);
    /// Console.WriteLine($"Bronze progress: {progress:P0}");
    /// // "Bronze progress: 60%"
    /// </code>
    /// </example>
    decimal GetProgressToward(UniqueAchievementType type, string? classId = null);

    /// <summary>
    /// Gets the next unearned achievement milestone.
    /// </summary>
    /// <returns>The next threshold, or null if all are earned.</returns>
    /// <remarks>
    /// Returns the first unearned threshold from <see cref="AchievementThreshold.Defaults.All"/>
    /// in order of required count (FirstMythForged → Bronze → Silver → Gold).
    /// </remarks>
    AchievementThreshold? GetNextMilestone();

    /// <summary>
    /// Checks if a specific achievement has been earned.
    /// </summary>
    /// <param name="type">The achievement type to check.</param>
    /// <param name="classId">Class ID for ClassMaster achievements (optional).</param>
    /// <returns><c>true</c> if the achievement has been earned; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (tracker.IsAchievementEarned(UniqueAchievementType.FirstMythForged))
    /// {
    ///     // Player has found at least one legendary item
    /// }
    /// 
    /// if (tracker.IsAchievementEarned(UniqueAchievementType.ClassMaster, "warrior"))
    /// {
    ///     // Player has found all warrior-specific uniques
    /// }
    /// </code>
    /// </example>
    bool IsAchievementEarned(UniqueAchievementType type, string? classId = null);

    /// <summary>
    /// Checks if a specific unique item has been found.
    /// </summary>
    /// <param name="itemId">The item ID to check.</param>
    /// <returns><c>true</c> if the item has been found; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Item IDs are compared case-insensitively.
    /// </remarks>
    bool HasFoundItem(string itemId);

    /// <summary>
    /// Gets items remaining to find for full collection.
    /// </summary>
    /// <returns>
    /// The number of unique items not yet discovered.
    /// Returns 0 if the collection is complete.
    /// </returns>
    int GetRemainingItemCount();
}
