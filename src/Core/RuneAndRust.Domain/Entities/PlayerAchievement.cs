// ═══════════════════════════════════════════════════════════════════════════════
// PlayerAchievement.cs
// Entity representing an achievement unlocked by a player.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an achievement that has been unlocked by a player.
/// </summary>
/// <remarks>
/// <para>
/// Player achievements are records of unlocked achievements stored as part of the
/// Player aggregate. Each record captures:
/// </para>
/// <list type="bullet">
///   <item><description>The achievement definition ID (for lookups)</description></item>
///   <item><description>The timestamp when the achievement was unlocked</description></item>
///   <item><description>Points awarded at unlock time (captured for historical accuracy)</description></item>
/// </list>
/// <para>
/// Points are captured at unlock time rather than looked up dynamically to preserve
/// historical accuracy even if the achievement definition is modified later.
/// </para>
/// <para>
/// This entity is created by the <see cref="Player.AddAchievement"/> method when
/// the AchievementService determines all conditions for an achievement have been met.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating a new player achievement (via Player entity)
/// player.AddAchievement("first-blood", 10);
/// 
/// // The achievement is now accessible via the player's collection
/// var achievement = player.Achievements.FirstOrDefault(a => a.AchievementId == "first-blood");
/// Console.WriteLine($"Unlocked at: {achievement.UnlockedAt}");
/// Console.WriteLine($"Points: {achievement.PointsAwarded}");
/// </code>
/// </example>
public class PlayerAchievement : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this player achievement record.
    /// </summary>
    /// <remarks>
    /// This is the entity ID for persistence purposes, distinct from
    /// <see cref="AchievementId"/> which identifies the achievement definition.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the achievement definition ID that was unlocked.
    /// </summary>
    /// <remarks>
    /// This ID corresponds to <see cref="AchievementDefinition.AchievementId"/>
    /// and is used to look up the full achievement details from the provider.
    /// </remarks>
    /// <example>"first-blood", "monster-slayer", "boss-killer"</example>
    public string AchievementId { get; private set; } = null!;

    /// <summary>
    /// Gets the UTC timestamp when this achievement was unlocked.
    /// </summary>
    /// <remarks>
    /// The timestamp is captured at creation time using <see cref="DateTime.UtcNow"/>
    /// to ensure consistent timezone handling across different client environments.
    /// </remarks>
    public DateTime UnlockedAt { get; private set; }

    /// <summary>
    /// Gets the points awarded for this achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Points are captured at unlock time to preserve historical accuracy
    /// even if the achievement definition's point value changes later.
    /// </para>
    /// <para>
    /// This ensures that player totals remain consistent and that
    /// any UI showing "points earned" reflects what was actually awarded.
    /// </para>
    /// </remarks>
    public int PointsAwarded { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core and factory method.
    /// </summary>
    /// <remarks>
    /// EF Core requires a parameterless constructor for materialization.
    /// External code should use <see cref="Create"/> to create new instances.
    /// </remarks>
    private PlayerAchievement() { }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new PlayerAchievement record.
    /// </summary>
    /// <param name="achievementId">
    /// The achievement definition ID. Must not be null, empty, or whitespace.
    /// This ID should correspond to an existing <see cref="AchievementDefinition.AchievementId"/>.
    /// </param>
    /// <param name="points">
    /// The points to award for this achievement. Must be non-negative.
    /// Typically derived from the achievement's tier (Bronze=10, Silver=25, Gold=50, Platinum=100).
    /// </param>
    /// <returns>A new PlayerAchievement instance with a generated ID and current timestamp.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="achievementId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="points"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create an achievement record for a Silver tier achievement (25 points)
    /// var achievement = PlayerAchievement.Create("monster-slayer", 25);
    /// 
    /// // The achievement has a unique ID and current timestamp
    /// Console.WriteLine($"ID: {achievement.Id}");
    /// Console.WriteLine($"Unlocked at: {achievement.UnlockedAt}");
    /// </code>
    /// </example>
    public static PlayerAchievement Create(string achievementId, int points)
    {
        // Validate achievementId is not null, empty, or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));

        // Validate points is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(points, nameof(points));

        return new PlayerAchievement
        {
            Id = Guid.NewGuid(),
            AchievementId = achievementId,
            UnlockedAt = DateTime.UtcNow,
            PointsAwarded = points
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OVERRIDES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this player achievement.
    /// </summary>
    /// <returns>A string containing the achievement ID, unlock date, and points.</returns>
    /// <example>
    /// <code>
    /// var achievement = PlayerAchievement.Create("first-blood", 10);
    /// Console.WriteLine(achievement.ToString());
    /// // Output: PlayerAchievement[first-blood: 10 pts @ 2026-01-20 14:30:00]
    /// </code>
    /// </example>
    public override string ToString() =>
        $"PlayerAchievement[{AchievementId}: {PointsAwarded} pts @ {UnlockedAt:yyyy-MM-dd HH:mm:ss}]";
}
