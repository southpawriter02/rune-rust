// ═══════════════════════════════════════════════════════════════════════════════
// AchievementDisplayDto.cs
// DTO for displaying achievement information in the UI.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// DTO for displaying achievement information in the UI.
/// </summary>
/// <remarks>
/// <para>
/// This record combines achievement definition data with player progress for display.
/// It is used by <see cref="UI.AchievementCard"/> and <see cref="UI.AchievementPanel"/>
/// to render achievement information.
/// </para>
/// <para>
/// Secret achievements show placeholder content (???) when not unlocked,
/// controlled by the <see cref="IsSecret"/> property.
/// </para>
/// </remarks>
/// <param name="Id">The unique achievement identifier.</param>
/// <param name="Name">The display name of the achievement.</param>
/// <param name="Description">The description explaining how to unlock the achievement.</param>
/// <param name="Category">The achievement category for grouping and filtering.</param>
/// <param name="Tier">The achievement rarity tier (Bronze, Silver, Gold, Platinum).</param>
/// <param name="TargetValue">The target value required for completion.</param>
/// <param name="CurrentValue">The player's current progress value.</param>
/// <param name="IsUnlocked">Whether the achievement has been unlocked.</param>
/// <param name="IsSecret">Whether this is a secret achievement (hidden until unlocked).</param>
/// <param name="PointValue">The point value awarded for unlocking.</param>
/// <param name="UnlockedAt">The timestamp when the achievement was unlocked, if applicable.</param>
public record AchievementDisplayDto(
    string Id,
    string Name,
    string Description,
    AchievementCategory Category,
    AchievementTier Tier,
    int TargetValue,
    int CurrentValue,
    bool IsUnlocked,
    bool IsSecret,
    int PointValue,
    DateTime? UnlockedAt)
{
    /// <summary>
    /// Gets whether the achievement is partially complete (has progress but not unlocked).
    /// </summary>
    public bool IsInProgress => !IsUnlocked && CurrentValue > 0;

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public int Percentage => TargetValue > 0 
        ? Math.Min((CurrentValue * 100) / TargetValue, 100) 
        : 0;

    /// <summary>
    /// Gets the display name, respecting secret status.
    /// </summary>
    /// <returns>The name or "???" if secret and locked.</returns>
    public string GetDisplayName() => IsSecret && !IsUnlocked ? "???" : Name;

    /// <summary>
    /// Gets the display description, respecting secret status.
    /// </summary>
    /// <returns>The description or "???" if secret and locked.</returns>
    public string GetDisplayDescription() => IsSecret && !IsUnlocked ? "???" : Description;
}
