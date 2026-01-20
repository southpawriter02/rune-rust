// ═══════════════════════════════════════════════════════════════════════════════
// AchievementProgress.cs
// Records for tracking achievement progress in the UI.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Models;

/// <summary>
/// Represents the progress towards an achievement for display purposes.
/// </summary>
/// <remarks>
/// <para>
/// Progress tracking enables the UI to display how close a player is to
/// unlocking an achievement, even when not all conditions are met. This
/// provides motivation and clear goals for players.
/// </para>
/// <para>Progress tracking includes:</para>
/// <list type="bullet">
///   <item><description>The achievement definition for display information</description></item>
///   <item><description>Whether the achievement is already unlocked</description></item>
///   <item><description>Per-condition progress details showing current vs. target values</description></item>
///   <item><description>Overall progress as a percentage (0.0 to 1.0)</description></item>
/// </list>
/// <para>
/// This record is immutable and should be recreated when player statistics change.
/// </para>
/// </remarks>
/// <param name="Definition">
/// The achievement definition containing name, description, tier, and conditions.
/// </param>
/// <param name="IsUnlocked">
/// Whether the player has unlocked this achievement. If true, 
/// <see cref="OverallProgress"/> should be 1.0.
/// </param>
/// <param name="ConditionProgress">
/// Progress details for each condition. May be empty if the achievement has no conditions.
/// </param>
/// <param name="OverallProgress">
/// Overall progress from 0.0 (not started) to 1.0 (complete).
/// Calculated as the average of all condition progress values.
/// </param>
/// <example>
/// <code>
/// // Display achievement progress in UI
/// foreach (var progress in achievementService.GetProgress(player))
/// {
///     var name = progress.Definition.GetDisplayName(progress.IsUnlocked);
///     var percent = progress.ProgressPercent;
///     Console.WriteLine($"{name}: {percent}%");
/// }
/// </code>
/// </example>
public record AchievementProgress(
    AchievementDefinition Definition,
    bool IsUnlocked,
    IReadOnlyList<ConditionProgress> ConditionProgress,
    double OverallProgress)
{
    /// <summary>
    /// Gets whether this achievement is partially complete (has some but not full progress).
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if the achievement is not unlocked but has made some progress
    /// (OverallProgress > 0). Useful for highlighting achievements in progress.
    /// </remarks>
    public bool IsPartiallyComplete => !IsUnlocked && OverallProgress > 0.0;

    /// <summary>
    /// Gets the progress as a display percentage (0-100).
    /// </summary>
    /// <remarks>
    /// Truncates to integer for display. For example, 0.675 becomes 67%.
    /// </remarks>
    public int ProgressPercent => (int)(OverallProgress * 100);

    /// <summary>
    /// Gets the display name of the achievement, respecting secret status.
    /// </summary>
    /// <remarks>
    /// Convenience property that calls <see cref="AchievementDefinition.GetDisplayName"/>
    /// with the current unlock status.
    /// </remarks>
    public string DisplayName => Definition.GetDisplayName(IsUnlocked);

    /// <summary>
    /// Gets the display description of the achievement, respecting secret status.
    /// </summary>
    /// <remarks>
    /// Convenience property that calls <see cref="AchievementDefinition.GetDisplayDescription"/>
    /// with the current unlock status.
    /// </remarks>
    public string DisplayDescription => Definition.GetDisplayDescription(IsUnlocked);
}

/// <summary>
/// Represents progress for a single achievement condition.
/// </summary>
/// <remarks>
/// <para>
/// Each achievement can have multiple conditions that must all be satisfied
/// for the achievement to unlock. This record tracks progress for a single
/// condition, enabling detailed progress UI.
/// </para>
/// <para>
/// For conditions with <see cref="Domain.Enums.ComparisonOperator.GreaterThanOrEqual"/>,
/// progress is calculated as <c>CurrentValue / TargetValue</c> (capped at 1.0).
/// For other operators (Equals, LessThanOrEqual), progress is binary (0 or 1).
/// </para>
/// </remarks>
/// <param name="Condition">
/// The condition being tracked (contains statistic name, operator, and target value).
/// </param>
/// <param name="CurrentValue">
/// The player's current value for the statistic.
/// </param>
/// <param name="TargetValue">
/// The target value required to satisfy the condition.
/// Same as <see cref="AchievementCondition.Value"/>.
/// </param>
/// <param name="Progress">
/// Progress from 0.0 to 1.0. 1.0 means the condition is met.
/// </param>
/// <param name="IsMet">
/// Whether this condition is currently satisfied.
/// </param>
/// <example>
/// <code>
/// // Display condition progress
/// foreach (var cond in achievementProgress.ConditionProgress)
/// {
///     var status = cond.IsMet ? "✓" : "○";
///     Console.WriteLine($"{status} {cond.Condition.StatisticName}: {cond.CurrentValue}/{cond.TargetValue}");
/// }
/// </code>
/// </example>
public record ConditionProgress(
    AchievementCondition Condition,
    long CurrentValue,
    long TargetValue,
    double Progress,
    bool IsMet)
{
    /// <summary>
    /// Gets the progress as a display percentage (0-100).
    /// </summary>
    public int ProgressPercent => (int)(Progress * 100);

    /// <summary>
    /// Gets a human-readable description of the condition.
    /// </summary>
    /// <example>"monstersKilled >= 100"</example>
    public string Description => Condition.Description;

    /// <summary>
    /// Gets a formatted string showing current/target progress.
    /// </summary>
    /// <example>"82/100"</example>
    public string ProgressDisplay => $"{CurrentValue}/{TargetValue}";
}
