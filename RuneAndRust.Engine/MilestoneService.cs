using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Service for managing milestone tier progression
/// Tracks achievement points and advances tiers when thresholds are met
/// </summary>
public class MilestoneService
{
    private readonly ILogger _logger;
    private readonly AccountProgressionRepository _accountRepo;
    private readonly CosmeticRepository _cosmeticRepo;
    private readonly AlternativeStartRepository _alternativeStartRepo;

    public MilestoneService(
        AccountProgressionRepository accountRepo,
        CosmeticRepository cosmeticRepo,
        AlternativeStartRepository alternativeStartRepo)
    {
        _logger = Log.ForContext<MilestoneService>();
        _accountRepo = accountRepo;
        _cosmeticRepo = cosmeticRepo;
        _alternativeStartRepo = alternativeStartRepo;
    }

    #region Milestone Tier Management

    /// <summary>
    /// Check if account has met requirements for next milestone tier
    /// </summary>
    public void CheckMilestoneTierProgression(int accountId)
    {
        _logger.Debug("Checking milestone tier progression: AccountID={AccountId}", accountId);

        var account = _accountRepo.GetById(accountId);
        if (account == null)
        {
            _logger.Warning("Account not found: {AccountId}", accountId);
            return;
        }

        var currentTier = _accountRepo.GetTier(account.CurrentMilestoneTier);
        if (currentTier == null)
        {
            _logger.Warning("Current tier not found: {TierNumber}", account.CurrentMilestoneTier);
            return;
        }

        var nextTier = _accountRepo.GetTier(account.CurrentMilestoneTier + 1);
        if (nextTier == null)
        {
            // Max tier reached
            _logger.Debug("Max milestone tier reached: Tier={TierNumber}", account.CurrentMilestoneTier);
            return;
        }

        // Check if player has enough achievement points for next tier
        if (account.TotalAchievementPoints >= nextTier.RequiredAchievementPoints)
        {
            AdvanceToNextTier(accountId, nextTier);
        }
    }

    /// <summary>
    /// Get current milestone tier for account
    /// </summary>
    public MilestoneTier? GetCurrentTier(int accountId)
    {
        var account = _accountRepo.GetById(accountId);
        if (account == null) return null;

        return _accountRepo.GetTier(account.CurrentMilestoneTier);
    }

    /// <summary>
    /// Get all milestone tiers
    /// </summary>
    public List<MilestoneTier> GetAllTiers()
    {
        return _accountRepo.GetAllTiers();
    }

    /// <summary>
    /// Get progress toward next tier
    /// </summary>
    public (int CurrentPoints, int RequiredPoints, float ProgressPercent) GetProgressToNextTier(int accountId)
    {
        var account = _accountRepo.GetById(accountId);
        if (account == null)
        {
            return (0, 0, 0f);
        }

        var nextTier = _accountRepo.GetTier(account.CurrentMilestoneTier + 1);
        if (nextTier == null)
        {
            // Max tier reached
            return (account.TotalAchievementPoints, account.TotalAchievementPoints, 100f);
        }

        var currentTier = _accountRepo.GetTier(account.CurrentMilestoneTier);
        var currentTierPoints = currentTier?.RequiredAchievementPoints ?? 0;
        var pointsInCurrentTier = account.TotalAchievementPoints - currentTierPoints;
        var pointsNeededForNextTier = nextTier.RequiredAchievementPoints - currentTierPoints;
        var progressPercent = (float)pointsInCurrentTier / pointsNeededForNextTier * 100f;

        return (account.TotalAchievementPoints, nextTier.RequiredAchievementPoints, progressPercent);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Advance account to next milestone tier
    /// </summary>
    private void AdvanceToNextTier(int accountId, MilestoneTier nextTier)
    {
        _logger.Information("Advancing to milestone tier: AccountID={AccountId}, Tier={TierNumber} ({TierName})",
            accountId, nextTier.TierNumber, nextTier.TierName);

        // Update account tier
        _accountRepo.UpdateMilestoneTier(accountId, nextTier.TierNumber);

        // Unlock tier rewards
        UnlockTierRewards(accountId, nextTier);

        // Display milestone notification
        DisplayMilestoneNotification(nextTier);

        _logger.Information("Milestone tier advanced successfully: {TierName}", nextTier.TierName);

        // Check if another tier can be unlocked
        CheckMilestoneTierProgression(accountId);
    }

    /// <summary>
    /// Unlock all rewards for a milestone tier
    /// </summary>
    private void UnlockTierRewards(int accountId, MilestoneTier tier)
    {
        _logger.Information("Unlocking milestone tier rewards: Tier={TierNumber}", tier.TierNumber);

        // Unlock account unlocks
        foreach (var unlockId in tier.UnlockRewards)
        {
            try
            {
                _accountRepo.UnlockBenefit(accountId, unlockId);
                _logger.Information("Account unlock unlocked: {UnlockId}", unlockId);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to unlock account unlock: {UnlockId}", unlockId);
            }
        }

        // Unlock cosmetics
        foreach (var cosmeticId in tier.CosmeticRewards)
        {
            try
            {
                _cosmeticRepo.UnlockCosmetic(accountId, cosmeticId);
                _logger.Information("Cosmetic unlocked: {CosmeticId}", cosmeticId);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to unlock cosmetic: {CosmeticId}", cosmeticId);
            }
        }

        // Unlock alternative start
        if (!string.IsNullOrEmpty(tier.AlternativeStartUnlock))
        {
            try
            {
                _alternativeStartRepo.UnlockStart(accountId, tier.AlternativeStartUnlock);
                _logger.Information("Alternative start unlocked: {StartId}", tier.AlternativeStartUnlock);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to unlock alternative start: {StartId}", tier.AlternativeStartUnlock);
            }
        }

        _logger.Information("Milestone tier rewards unlocked: {UnlockCount} unlocks, {CosmeticCount} cosmetics",
            tier.UnlockRewards.Count, tier.CosmeticRewards.Count);
    }

    /// <summary>
    /// Display milestone notification
    /// (Future: integrate with UI system)
    /// </summary>
    private void DisplayMilestoneNotification(MilestoneTier tier)
    {
        _logger.Information("🎖️ MILESTONE TIER REACHED: Tier {TierNumber} - {TierName}",
            tier.TierNumber, tier.TierName);
        _logger.Information("   {Description}", tier.Description);

        // Future: Send notification to UI system
    }

    #endregion
}
