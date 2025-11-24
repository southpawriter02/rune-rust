using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Service for tracking and managing achievements
/// Monitors gameplay events and unlocks achievements
/// </summary>
public class AchievementService
{
    private readonly ILogger _logger;
    private readonly AchievementRepository _achievementRepo;
    private readonly AccountProgressionRepository _accountRepo;
    private readonly CosmeticRepository _cosmeticRepo;

    public AchievementService(
        AchievementRepository achievementRepo,
        AccountProgressionRepository accountRepo,
        CosmeticRepository cosmeticRepo)
    {
        _logger = Log.ForContext<AchievementService>();
        _achievementRepo = achievementRepo;
        _accountRepo = accountRepo;
        _cosmeticRepo = cosmeticRepo;
    }

    #region Achievement Tracking

    /// <summary>
    /// Track progress toward an achievement
    /// </summary>
    public void TrackProgress(int accountId, string achievementId, int increment = 1)
    {
        _logger.Debug("Tracking achievement progress: AccountID={AccountId}, AchievementID={AchievementId}, Increment={Increment}",
            accountId, achievementId, increment);

        var achievement = _achievementRepo.GetById(achievementId);
        if (achievement == null)
        {
            _logger.Warning("Achievement not found: {AchievementId}", achievementId);
            return;
        }

        var progress = _achievementRepo.GetProgress(accountId, achievementId);
        if (progress == null)
        {
            _logger.Warning("Failed to get achievement progress");
            return;
        }

        // Check if already unlocked
        if (progress.IsUnlocked)
        {
            _logger.Debug("Achievement already unlocked: {AchievementId}", achievementId);
            return;
        }

        // Increment progress
        progress = _achievementRepo.IncrementProgress(accountId, achievementId, increment);

        _logger.Debug("Achievement progress: {Current}/{Required}",
            progress.CurrentProgress, achievement.RequiredProgress);

        // Check if achievement should be unlocked
        if (progress.CurrentProgress >= achievement.RequiredProgress)
        {
            UnlockAchievement(accountId, achievementId);
        }
    }

    /// <summary>
    /// Unlock an achievement
    /// </summary>
    public void UnlockAchievement(int accountId, string achievementId)
    {
        _logger.Information("Unlocking achievement: AccountID={AccountId}, AchievementID={AchievementId}",
            accountId, achievementId);

        var achievement = _achievementRepo.GetById(achievementId);
        if (achievement == null)
        {
            _logger.Warning("Achievement not found: {AchievementId}", achievementId);
            return;
        }

        // Check if already unlocked
        if (_achievementRepo.IsUnlocked(accountId, achievementId))
        {
            _logger.Debug("Achievement already unlocked: {AchievementId}", achievementId);
            return;
        }

        // Unlock the achievement
        _achievementRepo.UnlockAchievement(accountId, achievementId);

        // Award achievement points
        var account = _accountRepo.GetById(accountId);
        if (account != null)
        {
            account.TotalAchievementPoints += achievement.AchievementPoints;
            account.TotalAchievementsUnlocked++;
            _accountRepo.Update(account);

            _logger.Information("Achievement points awarded: +{Points} (Total: {Total})",
                achievement.AchievementPoints, account.TotalAchievementPoints);
        }

        // Unlock rewards (cosmetics, titles, etc.)
        foreach (var rewardId in achievement.RewardIds)
        {
            UnlockReward(accountId, rewardId);
        }

        _logger.Information("Achievement unlocked successfully: {AchievementName} ({Points} points)",
            achievement.Name, achievement.AchievementPoints);

        // Display achievement notification (future: UI integration)
        DisplayAchievementNotification(achievement);
    }

    /// <summary>
    /// Check if achievement is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string achievementId)
    {
        return _achievementRepo.IsUnlocked(accountId, achievementId);
    }

    /// <summary>
    /// Get all achievements for account with progress
    /// </summary>
    public List<(Achievement Achievement, AchievementProgress? Progress)> GetAchievementsWithProgress(int accountId)
    {
        _logger.Debug("Getting achievements with progress: AccountID={AccountId}", accountId);

        var achievements = _achievementRepo.GetAll();
        var result = new List<(Achievement, AchievementProgress?)>();

        foreach (var achievement in achievements)
        {
            var progress = _achievementRepo.GetProgress(accountId, achievement.AchievementId);
            result.Add((achievement, progress));
        }

        return result;
    }

    /// <summary>
    /// Get unlocked achievements for account
    /// </summary>
    public List<Achievement> GetUnlockedAchievements(int accountId)
    {
        return _achievementRepo.GetUnlockedAchievements(accountId);
    }

    /// <summary>
    /// Get achievements by category
    /// </summary>
    public List<Achievement> GetByCategory(AchievementCategory category)
    {
        return _achievementRepo.GetByCategory(category);
    }

    #endregion

    #region Event Handlers (called during gameplay)

    /// <summary>
    /// Called when tutorial is completed
    /// </summary>
    public void OnTutorialCompleted(int accountId)
    {
        TrackProgress(accountId, "first_steps");
    }

    /// <summary>
    /// Called when campaign is completed
    /// </summary>
    public void OnCampaignCompleted(int accountId)
    {
        TrackProgress(accountId, "survivor");
    }

    /// <summary>
    /// Called when player reaches max level
    /// </summary>
    public void OnMaxLevelReached(int accountId)
    {
        TrackProgress(accountId, "legend");
    }

    /// <summary>
    /// Called when a boss is defeated
    /// </summary>
    public void OnBossDefeated(int accountId, string bossId, bool flawless = false)
    {
        // Track general boss kill
        TrackProgress(accountId, "boss_slayer");

        // Track specific boss achievements
        TrackProgress(accountId, $"boss_defeated_{bossId}");

        // Track flawless boss kills
        if (flawless)
        {
            TrackProgress(accountId, $"boss_flawless_{bossId}");
            TrackProgress(accountId, "untouchable");
        }
    }

    /// <summary>
    /// Called when sector is completed without taking damage
    /// </summary>
    public void OnPerfectSectorClear(int accountId)
    {
        TrackProgress(accountId, "untouchable");
    }

    /// <summary>
    /// Called when New Game+ tier is completed
    /// </summary>
    public void OnNewGamePlusCompleted(int accountId, int tier)
    {
        TrackProgress(accountId, $"ng_plus_tier_{tier}");

        if (tier >= 5)
        {
            TrackProgress(accountId, "transcendent");
        }
    }

    /// <summary>
    /// Called when endless mode wave is reached
    /// </summary>
    public void OnEndlessWaveReached(int accountId, int wave)
    {
        // Track milestone waves
        if (wave >= 50)
        {
            TrackProgress(accountId, "endless_legend");
        }
        else if (wave >= 30)
        {
            TrackProgress(accountId, "immortal");
        }
        else if (wave >= 20)
        {
            TrackProgress(accountId, "endurance");
        }
    }

    /// <summary>
    /// Called when campaign is completed without acquiring Trauma
    /// </summary>
    public void OnTraumaFreeCampaignCompleted(int accountId)
    {
        TrackProgress(accountId, "iron_will");
    }

    /// <summary>
    /// Called when all specializations are unlocked
    /// </summary>
    public void OnAllSpecializationsUnlocked(int accountId)
    {
        TrackProgress(accountId, "master_of_all");
    }

    /// <summary>
    /// Called when codex entry is unlocked
    /// </summary>
    public void OnCodexEntryUnlocked(int accountId, string biomeId)
    {
        TrackProgress(accountId, $"codex_{biomeId}");
    }

    /// <summary>
    /// Called when enemy type is examined
    /// </summary>
    public void OnEnemyExamined(int accountId, string enemyType)
    {
        TrackProgress(accountId, "bestiary_complete");
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Unlock reward from achievement
    /// </summary>
    private void UnlockReward(int accountId, string rewardId)
    {
        _logger.Debug("Unlocking reward: AccountID={AccountId}, RewardID={RewardId}",
            accountId, rewardId);

        // Check if reward is a cosmetic
        var cosmetic = _cosmeticRepo.GetById(rewardId);
        if (cosmetic != null)
        {
            _cosmeticRepo.UnlockCosmetic(accountId, rewardId);
            _logger.Information("Cosmetic unlocked: {CosmeticName}", cosmetic.Name);
            return;
        }

        // Check if reward is an account unlock
        var unlock = _accountRepo.GetUnlocks(accountId)
            .FirstOrDefault(u => u.UnlockId == rewardId);
        if (unlock != null)
        {
            _accountRepo.UnlockBenefit(accountId, rewardId);
            _logger.Information("Account unlock unlocked: {UnlockName}", unlock.Name);
            return;
        }

        _logger.Warning("Reward not found: {RewardId}", rewardId);
    }

    /// <summary>
    /// Display achievement notification
    /// (Future: integrate with UI system)
    /// </summary>
    private void DisplayAchievementNotification(Achievement achievement)
    {
        _logger.Information("🏆 ACHIEVEMENT UNLOCKED: {AchievementName} (+{Points} points)",
            achievement.Name, achievement.AchievementPoints);

        if (!string.IsNullOrEmpty(achievement.FlavorText))
        {
            _logger.Information("   {FlavorText}", achievement.FlavorText);
        }

        // Future: Send notification to UI system
    }

    #endregion
}
