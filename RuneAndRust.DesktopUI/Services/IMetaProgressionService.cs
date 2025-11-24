using RuneAndRust.Core;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.15: Service interface for accessing meta-progression data.
/// Aggregates achievements, unlocks, cosmetics, and account statistics.
/// </summary>
public interface IMetaProgressionService
{
    /// <summary>
    /// Gets the current account ID (defaults to 1 for single-player).
    /// </summary>
    int CurrentAccountId { get; }

    /// <summary>
    /// Loads the complete meta-progression state for the current account.
    /// </summary>
    MetaProgressionState LoadMetaProgression();

    /// <summary>
    /// Gets achievements filtered by category.
    /// </summary>
    List<AchievementWithProgress> GetAchievementsByCategory(string category);

    /// <summary>
    /// Gets all unlocked cosmetics for the current account.
    /// </summary>
    List<Cosmetic> GetUnlockedCosmetics();

    /// <summary>
    /// Gets all account unlocks with their status.
    /// </summary>
    List<AccountUnlock> GetAccountUnlocks();
}

/// <summary>
/// v0.43.15: Aggregated meta-progression state for UI display.
/// </summary>
public class MetaProgressionState
{
    /// <summary>
    /// Account progression data with statistics.
    /// </summary>
    public AccountProgression? Account { get; set; }

    /// <summary>
    /// All achievements with progress.
    /// </summary>
    public List<AchievementWithProgress> Achievements { get; set; } = new();

    /// <summary>
    /// All account unlocks.
    /// </summary>
    public List<AccountUnlock> Unlocks { get; set; } = new();

    /// <summary>
    /// All unlocked cosmetics.
    /// </summary>
    public List<Cosmetic> UnlockedCosmetics { get; set; } = new();

    /// <summary>
    /// Current milestone tier.
    /// </summary>
    public MilestoneTier? CurrentTier { get; set; }

    // Convenience statistics
    public int TotalAchievements => Achievements.Count;
    public int UnlockedAchievements => Achievements.Count(a => a.IsUnlocked);
    public int TotalRuns => Account?.TotalCampaignsCompleted ?? 0;
    public int SuccessfulRuns => Account?.TotalCampaignsCompleted ?? 0;
    public int HighestLegend => Account?.CurrentMilestoneTier ?? 1;
    public int TotalAchievementPoints => Account?.TotalAchievementPoints ?? 0;
    public int HighestNewGamePlus => Account?.HighestNewGamePlusTier ?? 0;
    public int HighestEndlessWave => Account?.HighestEndlessWave ?? 0;

    // Note: TotalPlayTime would require session tracking not yet implemented
    public TimeSpan TotalPlayTime => TimeSpan.Zero;
}

/// <summary>
/// v0.43.15: Achievement with its progress for a specific account.
/// </summary>
public class AchievementWithProgress
{
    public Achievement Achievement { get; set; } = new();
    public AchievementProgress? Progress { get; set; }

    // Convenience properties
    public string AchievementId => Achievement.AchievementId;
    public string Name => Achievement.Name;
    public string Description => Achievement.Description;
    public string FlavorText => Achievement.FlavorText;
    public AchievementCategory Category => Achievement.Category;
    public int AchievementPoints => Achievement.AchievementPoints;
    public bool IsSecret => Achievement.IsSecret;
    public int RequiredProgress => Achievement.RequiredProgress;
    public List<string> RewardIds => Achievement.RewardIds;

    public int CurrentProgress => Progress?.CurrentProgress ?? 0;
    public bool IsUnlocked => Progress?.IsUnlocked ?? false;
    public DateTime? UnlockedAt => Progress?.UnlockedAt;
    public float ProgressPercentage => RequiredProgress > 0 ? (float)CurrentProgress / RequiredProgress : 0f;
}
