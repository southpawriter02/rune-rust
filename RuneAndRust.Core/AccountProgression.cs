namespace RuneAndRust.Core;

/// <summary>
/// v0.41: Account-wide progression tracking
/// Stores meta-progression data that persists across all characters
/// </summary>
public class AccountProgression
{
    public int AccountId { get; set; }
    public int TotalAchievementPoints { get; set; } = 0;
    public int CurrentMilestoneTier { get; set; } = 1; // Starts at Tier 1: Initiate
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Statistics (for tracking purposes)
    public int TotalCharactersCreated { get; set; } = 0;
    public int TotalCampaignsCompleted { get; set; } = 0;
    public int TotalBossesDefeated { get; set; } = 0;
    public int TotalAchievementsUnlocked { get; set; } = 0;
    public int HighestNewGamePlusTier { get; set; } = 0;
    public int HighestEndlessWave { get; set; } = 0;
}

/// <summary>
/// v0.41: Account unlock type categories
/// </summary>
public enum AccountUnlockType
{
    Convenience,     // Quality of life improvements (skip tutorial, extra loadout slot)
    Variety,         // Alternative options (alternative starts, spec unlocks)
    Progression,     // Minor progression boosts (+5% Legend, starting resources)
    Cosmetic,        // Visual customization (titles, portraits)
    Knowledge        // Lore and information (bestiary, codex)
}

/// <summary>
/// v0.41: Individual account unlock definition
/// </summary>
public class AccountUnlock
{
    public string UnlockId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountUnlockType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string RequirementDescription { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }

    // Optional parameters for unlock application
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// v0.41: Account unlock progress for a specific account
/// </summary>
public class AccountUnlockProgress
{
    public int ProgressId { get; set; }
    public int AccountId { get; set; }
    public string UnlockId { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }
}
