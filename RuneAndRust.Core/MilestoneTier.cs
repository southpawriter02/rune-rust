namespace RuneAndRust.Core;

/// <summary>
/// v0.41: Milestone tier definition
/// Represents one of the 10 progression tiers in the account progression system
/// </summary>
public class MilestoneTier
{
    public int TierNumber { get; set; } // 1-10
    public string TierName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredAchievementPoints { get; set; } = 0;

    // Rewards for reaching this tier
    public List<string> UnlockRewards { get; set; } = new(); // Account unlock IDs
    public List<string> CosmeticRewards { get; set; } = new(); // Cosmetic IDs
    public string? AlternativeStartUnlock { get; set; } = null; // Alternative start ID
}

/// <summary>
/// v0.41: Account milestone progress
/// Tracks which milestone tier an account has reached
/// </summary>
public class AccountMilestoneProgress
{
    public int AccountId { get; set; }
    public int CurrentTierNumber { get; set; } = 1; // Starts at Tier 1
    public DateTime? LastTierReachedAt { get; set; }
}
