namespace RuneAndRust.Core;

/// <summary>
/// v0.41: Achievement category types
/// </summary>
public enum AchievementCategory
{
    Milestone,       // Campaign progression markers
    Combat,          // Combat mastery
    Exploration,     // Discovery and exploration
    Challenge,       // Extreme difficulty achievements
    Narrative,       // Story moments and quests
    Collection,      // Bestiary, codex, equipment completion
    Social           // Multiplayer/leaderboards (v2.0+)
}

/// <summary>
/// v0.41: Achievement definition
/// Tracks player accomplishments and awards points
/// </summary>
public class Achievement
{
    public string AchievementId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AchievementCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FlavorText { get; set; } = string.Empty;
    public int AchievementPoints { get; set; } = 0; // 5-50 points based on difficulty
    public bool IsSecret { get; set; } = false; // Hidden until unlocked
    public string IconId { get; set; } = string.Empty;

    // Progress tracking
    public int RequiredProgress { get; set; } = 1; // Default: single trigger

    // Rewards (cosmetics, titles, unlocks)
    public List<string> RewardIds { get; set; } = new();
}

/// <summary>
/// v0.41: Achievement progress for a specific account
/// </summary>
public class AchievementProgress
{
    public int ProgressId { get; set; }
    public int AccountId { get; set; }
    public string AchievementId { get; set; } = string.Empty;
    public int CurrentProgress { get; set; } = 0;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }
}

/// <summary>
/// v0.41: Achievement reward mapping
/// Maps achievements to their reward items
/// </summary>
public class AchievementReward
{
    public int RewardId { get; set; }
    public string AchievementId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty; // Cosmetic, Unlock, Title
    public string RewardItemId { get; set; } = string.Empty;
}
