// ═══════════════════════════════════════════════════════════════════════════════
// AchievementUnlockedEvent.cs
// Event raised when a player unlocks an achievement.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Events;

/// <summary>
/// Event raised when a player unlocks an achievement.
/// </summary>
/// <remarks>
/// <para>
/// This event is published by the AchievementService when an achievement's
/// conditions are all satisfied. Systems can subscribe to this event to:
/// </para>
/// <list type="bullet">
///   <item><description>Display unlock notifications in the UI</description></item>
///   <item><description>Play achievement unlock sounds</description></item>
///   <item><description>Update achievement displays and counters</description></item>
///   <item><description>Log achievement unlocks for analytics</description></item>
///   <item><description>Trigger other achievement-related functionality</description></item>
/// </list>
/// <para>
/// The event captures all relevant information at unlock time to avoid
/// the need for additional lookups by event handlers.
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the player who unlocked the achievement.</param>
/// <param name="AchievementId">
/// The achievement definition ID that was unlocked (e.g., "first-blood", "monster-slayer").
/// </param>
/// <param name="AchievementName">The display name of the achievement for UI purposes.</param>
/// <param name="Description">The description of the achievement.</param>
/// <param name="Tier">The tier of the achievement (Bronze, Silver, Gold, Platinum).</param>
/// <param name="PointsAwarded">The points awarded for this achievement.</param>
/// <example>
/// <code>
/// // Creating an event when an achievement unlocks
/// var unlockEvent = new AchievementUnlockedEvent(
///     PlayerId: player.Id,
///     AchievementId: "first-blood",
///     AchievementName: "First Blood",
///     Description: "Defeat your first monster",
///     Tier: "Bronze",
///     PointsAwarded: 10);
/// 
/// // Event handlers can display notifications
/// Console.WriteLine($"🏆 {unlockEvent.AchievementName} unlocked! +{unlockEvent.PointsAwarded} pts");
/// </code>
/// </example>
public record AchievementUnlockedEvent(
    Guid PlayerId,
    string AchievementId,
    string AchievementName,
    string Description,
    string Tier,
    int PointsAwarded)
{
    /// <summary>
    /// Gets the timestamp when this event occurred (UTC).
    /// </summary>
    /// <remarks>
    /// Automatically set to <see cref="DateTime.UtcNow"/> at event creation
    /// to ensure consistent timezone handling.
    /// </remarks>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a formatted message for logging or display purposes.
    /// </summary>
    /// <example>"Achievement unlocked: First Blood (+10 pts)"</example>
    public string Message => $"Achievement unlocked: {AchievementName} (+{PointsAwarded} pts)";
}
