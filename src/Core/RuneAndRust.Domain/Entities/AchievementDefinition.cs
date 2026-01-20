// ═══════════════════════════════════════════════════════════════════════════════
// AchievementDefinition.cs
// Entity representing the definition of an achievement that can be unlocked.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents the definition of an achievement that can be unlocked by players.
/// </summary>
/// <remarks>
/// <para>
/// Achievement definitions are loaded from JSON configuration and serve as templates.
/// Each definition specifies:
/// </para>
/// <list type="bullet">
///   <item><description><b>Basic info:</b> ID, name, description</description></item>
///   <item><description><b>Classification:</b> category and tier</description></item>
///   <item><description><b>Unlock conditions:</b> list of statistic-based conditions</description></item>
///   <item><description><b>Secret status:</b> whether hidden until unlocked</description></item>
/// </list>
/// <para>
/// Points are derived from the tier enum value:
/// Bronze=10, Silver=25, Gold=50, Platinum=100.
/// </para>
/// <para>
/// This entity is used by the AchievementService (v0.12.1b) to check if a player
/// has unlocked achievements based on their statistics.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create an achievement definition
/// var conditions = new List&lt;AchievementCondition&gt;
/// {
///     new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100)
/// };
///
/// var achievement = AchievementDefinition.Create(
///     achievementId: "monster-slayer",
///     name: "Monster Slayer",
///     description: "Defeat 100 monsters",
///     category: AchievementCategory.Combat,
///     tier: AchievementTier.Silver,
///     conditions: conditions);
///
/// // Get the point value (derived from tier)
/// int points = achievement.Points; // 25 (Silver tier)
///
/// // Check display name for secret achievements
/// string displayName = achievement.GetDisplayName(isUnlocked: false);
/// </code>
/// </example>
public sealed class AchievementDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL STORAGE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal storage for achievement conditions.
    /// </summary>
    private readonly List<AchievementCondition> _conditions = new();

    // ═══════════════════════════════════════════════════════════════════════════
    // IDENTITY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this achievement definition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary key for the AchievementDefinition entity,
    /// satisfying the <see cref="IEntity"/> interface requirement.
    /// </para>
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the string identifier used in configuration and lookups.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a human-readable identifier used in JSON configuration files
    /// and for looking up achievements by a stable key (e.g., "first-blood",
    /// "monster-slayer").
    /// </para>
    /// </remarks>
    /// <example>"first-blood", "monster-slayer", "lucky-streak"</example>
    public string AchievementId { get; private set; } = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name of the achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the player-facing name shown in the achievements list.
    /// For secret achievements, use <see cref="GetDisplayName(bool)"/> which
    /// returns "???" when the achievement is locked.
    /// </para>
    /// </remarks>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description explaining how to unlock the achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This description helps players understand what they need to do to
    /// unlock the achievement. For secret achievements, use
    /// <see cref="GetDisplayDescription(bool)"/> which returns a placeholder
    /// message when locked.
    /// </para>
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the optional path to the achievement icon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by the presentation layer to display a visual icon for the achievement.
    /// May be null if no custom icon is defined.
    /// </para>
    /// </remarks>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CLASSIFICATION PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the category this achievement belongs to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Categories help organize achievements in the UI:
    /// Combat, Exploration, Progression, Collection, Challenge, Secret.
    /// </para>
    /// </remarks>
    public AchievementCategory Category { get; private set; }

    /// <summary>
    /// Gets the tier/difficulty level of this achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tier determines the point value and indicates difficulty:
    /// Bronze (10 pts), Silver (25 pts), Gold (50 pts), Platinum (100 pts).
    /// </para>
    /// </remarks>
    public AchievementTier Tier { get; private set; }

    /// <summary>
    /// Gets the point value for unlocking this achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Points are derived directly from the <see cref="Tier"/> enum value:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Bronze = 10 points</description></item>
    ///   <item><description>Silver = 25 points</description></item>
    ///   <item><description>Gold = 50 points</description></item>
    ///   <item><description>Platinum = 100 points</description></item>
    /// </list>
    /// </remarks>
    public int Points => (int)Tier;

    /// <summary>
    /// Gets whether this is a secret achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Secret achievements are hidden in the UI until unlocked.
    /// Their name and description are replaced with placeholders.
    /// Use <see cref="GetDisplayName(bool)"/> and <see cref="GetDisplayDescription(bool)"/>
    /// to get the appropriate display text based on unlock status.
    /// </para>
    /// </remarks>
    public bool IsSecret { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONDITION PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the conditions that must be met to unlock this achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All conditions must be satisfied for the achievement to unlock (AND logic).
    /// Conditions are evaluated against player statistics by the AchievementService.
    /// </para>
    /// </remarks>
    public IReadOnlyList<AchievementCondition> Conditions => _conditions.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern and EF Core.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="Create"/> factory method to create new instances.
    /// </para>
    /// </remarks>
    private AchievementDefinition() { }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new AchievementDefinition.
    /// </summary>
    /// <param name="achievementId">The string identifier for this achievement.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The description of how to unlock.</param>
    /// <param name="category">The achievement category.</param>
    /// <param name="tier">The achievement tier.</param>
    /// <param name="conditions">The unlock conditions (all must be met).</param>
    /// <param name="isSecret">Whether this is a hidden achievement (default false).</param>
    /// <param name="iconPath">Optional icon path (default null).</param>
    /// <returns>A new AchievementDefinition instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="achievementId"/>, <paramref name="name"/>,
    /// or <paramref name="description"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="conditions"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method enforces validation rules:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Achievement ID, name, and description must not be empty</description></item>
    ///   <item><description>Conditions collection must not be null (but may be empty)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var conditions = new List&lt;AchievementCondition&gt;
    /// {
    ///     new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 1)
    /// };
    ///
    /// var achievement = AchievementDefinition.Create(
    ///     achievementId: "first-blood",
    ///     name: "First Blood",
    ///     description: "Defeat your first monster",
    ///     category: AchievementCategory.Combat,
    ///     tier: AchievementTier.Bronze,
    ///     conditions: conditions);
    /// </code>
    /// </example>
    public static AchievementDefinition Create(
        string achievementId,
        string name,
        string description,
        AchievementCategory category,
        AchievementTier tier,
        IEnumerable<AchievementCondition> conditions,
        bool isSecret = false,
        string? iconPath = null)
    {
        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        // Validate conditions collection
        ArgumentNullException.ThrowIfNull(conditions, nameof(conditions));

        // Create and initialize the definition
        var definition = new AchievementDefinition
        {
            Id = Guid.NewGuid(),
            AchievementId = achievementId,
            Name = name,
            Description = description,
            Category = category,
            Tier = tier,
            IsSecret = isSecret,
            IconPath = iconPath
        };

        // Add all conditions to the internal list
        definition._conditions.AddRange(conditions);

        return definition;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name, respecting secret status.
    /// </summary>
    /// <param name="isUnlocked">Whether the achievement has been unlocked by the player.</param>
    /// <returns>
    /// The achievement name if unlocked or not secret;
    /// otherwise "???" if secret and locked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use this method instead of accessing <see cref="Name"/> directly
    /// when displaying achievement names in the UI to properly handle
    /// secret achievements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var name = achievement.GetDisplayName(isUnlocked: false);
    /// // Returns "???" if secret, or the actual name otherwise
    /// </code>
    /// </example>
    public string GetDisplayName(bool isUnlocked)
    {
        return IsSecret && !isUnlocked ? "???" : Name;
    }

    /// <summary>
    /// Gets the display description, respecting secret status.
    /// </summary>
    /// <param name="isUnlocked">Whether the achievement has been unlocked by the player.</param>
    /// <returns>
    /// The achievement description if unlocked or not secret;
    /// otherwise a placeholder message if secret and locked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use this method instead of accessing <see cref="Description"/> directly
    /// when displaying achievement descriptions in the UI to properly handle
    /// secret achievements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var desc = achievement.GetDisplayDescription(isUnlocked: false);
    /// // Returns "Hidden achievement - unlock to reveal!" if secret
    /// </code>
    /// </example>
    public string GetDisplayDescription(bool isUnlocked)
    {
        return IsSecret && !isUnlocked
            ? "Hidden achievement - unlock to reveal!"
            : Description;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a string representation of this achievement definition.
    /// </summary>
    /// <returns>A string describing the achievement.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(achievement.ToString());
    /// // Output: Achievement[first-blood]: First Blood (Combat, Bronze, 10 pts)
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Achievement[{AchievementId}]: {Name} ({Category}, {Tier}, {Points} pts)";
}
