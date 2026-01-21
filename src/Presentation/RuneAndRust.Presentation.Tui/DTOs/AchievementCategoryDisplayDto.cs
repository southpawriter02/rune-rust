// ═══════════════════════════════════════════════════════════════════════════════
// AchievementCategoryDisplayDto.cs
// DTO for displaying achievement category information.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// DTO for displaying achievement category information.
/// </summary>
/// <remarks>
/// <para>
/// This record provides category summary information for filter tabs
/// in the achievement panel. Each category has an icon and color
/// for visual differentiation.
/// </para>
/// <para>
/// Used by <see cref="UI.AchievementPanel"/> for rendering category filter tabs.
/// </para>
/// </remarks>
/// <param name="Category">The achievement category enum value.</param>
/// <param name="Icon">The category icon string (e.g., "[C]" for Combat).</param>
/// <param name="Color">The console color for the category.</param>
/// <param name="AchievementCount">The total number of achievements in this category.</param>
/// <param name="UnlockedCount">The number of unlocked achievements in this category.</param>
public record AchievementCategoryDisplayDto(
    AchievementCategory Category,
    string Icon,
    ConsoleColor Color,
    int AchievementCount,
    int UnlockedCount)
{
    /// <summary>
    /// Gets the display name of the category.
    /// </summary>
    public string DisplayName => Category.ToString();

    /// <summary>
    /// Gets the completion percentage for this category.
    /// </summary>
    public int Percentage => AchievementCount > 0 
        ? (UnlockedCount * 100) / AchievementCount 
        : 0;

    /// <summary>
    /// Gets a formatted count string (e.g., "3/10").
    /// </summary>
    public string CountDisplay => $"{UnlockedCount}/{AchievementCount}";
}
