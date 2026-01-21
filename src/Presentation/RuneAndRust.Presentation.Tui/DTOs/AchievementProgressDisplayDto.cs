// ═══════════════════════════════════════════════════════════════════════════════
// AchievementProgressDisplayDto.cs
// DTO for displaying achievement progress information.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// DTO for displaying achievement progress information.
/// </summary>
/// <remarks>
/// <para>
/// This record provides progress-specific display data for achievement
/// tracking visualization, including current/target values and percentage.
/// </para>
/// <para>
/// Used by <see cref="Renderers.ProgressBarRenderer"/> to render progress bars.
/// </para>
/// </remarks>
/// <param name="AchievementId">The unique achievement identifier.</param>
/// <param name="CurrentValue">The player's current progress value.</param>
/// <param name="TargetValue">The target value required for completion.</param>
/// <param name="Percentage">The completion percentage (0-100).</param>
/// <param name="IsComplete">Whether the achievement is complete.</param>
public record AchievementProgressDisplayDto(
    string AchievementId,
    int CurrentValue,
    int TargetValue,
    int Percentage,
    bool IsComplete)
{
    /// <summary>
    /// Gets a formatted progress string (e.g., "47/100").
    /// </summary>
    public string ProgressDisplay => $"{CurrentValue}/{TargetValue}";

    /// <summary>
    /// Gets a formatted percentage string (e.g., "47%").
    /// </summary>
    public string PercentageDisplay => $"{Percentage}%";

    /// <summary>
    /// Gets whether there is any progress (CurrentValue > 0).
    /// </summary>
    public bool HasProgress => CurrentValue > 0;
}
