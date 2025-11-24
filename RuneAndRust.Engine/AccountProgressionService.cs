using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Service for managing account-wide progression
/// Handles account creation, unlocks, and statistics tracking
/// </summary>
public class AccountProgressionService
{
    private readonly ILogger _logger;
    private readonly AccountProgressionRepository _accountRepo;
    private readonly AchievementRepository _achievementRepo;
    private readonly CosmeticRepository _cosmeticRepo;
    private readonly AlternativeStartRepository _alternativeStartRepo;

    public AccountProgressionService(
        AccountProgressionRepository accountRepo,
        AchievementRepository achievementRepo,
        CosmeticRepository cosmeticRepo,
        AlternativeStartRepository alternativeStartRepo)
    {
        _logger = Log.ForContext<AccountProgressionService>();
        _accountRepo = accountRepo;
        _achievementRepo = achievementRepo;
        _cosmeticRepo = cosmeticRepo;
        _alternativeStartRepo = alternativeStartRepo;
    }

    #region Account Management

    /// <summary>
    /// Create a new account with default progression
    /// </summary>
    public int CreateAccount()
    {
        _logger.Information("Creating new account");

        var accountId = _accountRepo.CreateAccount();

        // Unlock default cosmetics and alternative starts
        UnlockDefaultContent(accountId);

        _logger.Information("Account created successfully: AccountID={AccountId}", accountId);
        return accountId;
    }

    /// <summary>
    /// Get account progression
    /// </summary>
    public AccountProgression? GetAccount(int accountId)
    {
        return _accountRepo.GetById(accountId);
    }

    /// <summary>
    /// Update account statistics after character creation
    /// </summary>
    public void OnCharacterCreated(int accountId)
    {
        _logger.Information("Incrementing character created count: AccountID={AccountId}", accountId);

        var account = _accountRepo.GetById(accountId);
        if (account == null)
        {
            _logger.Warning("Account not found: {AccountId}", accountId);
            return;
        }

        account.TotalCharactersCreated++;
        _accountRepo.Update(account);

        _logger.Information("Character created count updated: Total={Total}",
            account.TotalCharactersCreated);
    }

    /// <summary>
    /// Update account statistics after campaign completion
    /// </summary>
    public void OnCampaignCompleted(int accountId)
    {
        _logger.Information("Incrementing campaign completed count: AccountID={AccountId}", accountId);

        var account = _accountRepo.GetById(accountId);
        if (account == null) return;

        account.TotalCampaignsCompleted++;
        _accountRepo.Update(account);

        _logger.Information("Campaign completed count updated: Total={Total}",
            account.TotalCampaignsCompleted);
    }

    /// <summary>
    /// Update account statistics after boss defeat
    /// </summary>
    public void OnBossDefeated(int accountId)
    {
        _logger.Information("Incrementing boss defeated count: AccountID={AccountId}", accountId);

        var account = _accountRepo.GetById(accountId);
        if (account == null) return;

        account.TotalBossesDefeated++;
        _accountRepo.Update(account);

        _logger.Information("Boss defeated count updated: Total={Total}",
            account.TotalBossesDefeated);
    }

    /// <summary>
    /// Update highest New Game+ tier achieved
    /// </summary>
    public void UpdateHighestNewGamePlusTier(int accountId, int tier)
    {
        _logger.Information("Updating highest NG+ tier: AccountID={AccountId}, Tier={Tier}",
            accountId, tier);

        var account = _accountRepo.GetById(accountId);
        if (account == null) return;

        if (tier > account.HighestNewGamePlusTier)
        {
            account.HighestNewGamePlusTier = tier;
            _accountRepo.Update(account);

            _logger.Information("Highest NG+ tier updated: Tier={Tier}", tier);
        }
    }

    /// <summary>
    /// Update highest endless wave achieved
    /// </summary>
    public void UpdateHighestEndlessWave(int accountId, int wave)
    {
        _logger.Information("Updating highest endless wave: AccountID={AccountId}, Wave={Wave}",
            accountId, wave);

        var account = _accountRepo.GetById(accountId);
        if (account == null) return;

        if (wave > account.HighestEndlessWave)
        {
            account.HighestEndlessWave = wave;
            _accountRepo.Update(account);

            _logger.Information("Highest endless wave updated: Wave={Wave}", wave);
        }
    }

    #endregion

    #region Account Unlocks

    /// <summary>
    /// Get all unlocked benefits for account
    /// </summary>
    public List<AccountUnlock> GetUnlockedBenefits(int accountId)
    {
        _logger.Debug("Retrieving unlocked benefits: AccountID={AccountId}", accountId);

        return _accountRepo.GetUnlockedBenefits(accountId);
    }

    /// <summary>
    /// Get all unlocks with unlock status
    /// </summary>
    public List<AccountUnlock> GetAllUnlocks(int accountId)
    {
        _logger.Debug("Retrieving all unlocks: AccountID={AccountId}", accountId);

        return _accountRepo.GetUnlocks(accountId);
    }

    /// <summary>
    /// Unlock a specific benefit for account
    /// </summary>
    public void UnlockBenefit(int accountId, string unlockId)
    {
        _logger.Information("Unlocking benefit: AccountID={AccountId}, UnlockID={UnlockId}",
            accountId, unlockId);

        _accountRepo.UnlockBenefit(accountId, unlockId);

        _logger.Information("Benefit unlocked successfully");
    }

    /// <summary>
    /// Check if a specific unlock is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string unlockId)
    {
        return _accountRepo.IsUnlocked(accountId, unlockId);
    }

    /// <summary>
    /// Apply account unlocks to a character
    /// </summary>
    public void ApplyAccountUnlocksToCharacter(int accountId, PlayerCharacter character)
    {
        _logger.Information("Applying account unlocks to character: AccountID={AccountId}, Character={CharacterName}",
            accountId, character.Name);

        var unlocks = GetUnlockedBenefits(accountId);

        foreach (var unlock in unlocks)
        {
            ApplyUnlockToCharacter(character, unlock);
        }

        _logger.Information("Applied {Count} account unlocks to character", unlocks.Count);
    }

    /// <summary>
    /// Apply a single unlock to a character
    /// </summary>
    private void ApplyUnlockToCharacter(PlayerCharacter character, AccountUnlock unlock)
    {
        _logger.Debug("Applying unlock to character: UnlockID={UnlockId}", unlock.UnlockId);

        switch (unlock.UnlockId)
        {
            case "legend_boost_5":
                // +5% Legend gain
                var legendBoost = character.CurrentLegend * 0.05f;
                character.CurrentLegend += (int)legendBoost;
                _logger.Debug("Applied Legend boost: +5%");
                break;

            case "starting_resources_50":
                // +50% starting resources
                character.Currency = (int)(character.Currency * 1.5f);
                _logger.Debug("Applied starting resources boost: +50%");
                break;

            case "extra_loadout_slot":
                // Extra equipment loadout slot (future feature)
                _logger.Debug("Extra loadout slot available (not yet implemented)");
                break;

            // Add more unlock applications as needed

            default:
                _logger.Debug("Unlock has no direct character application: {UnlockId}", unlock.UnlockId);
                break;
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Unlock default content for new accounts
    /// </summary>
    private void UnlockDefaultContent(int accountId)
    {
        _logger.Debug("Unlocking default content for new account: AccountID={AccountId}", accountId);

        // Unlock standard start (always available)
        if (_alternativeStartRepo.GetById("standard_start") != null)
        {
            _alternativeStartRepo.UnlockStart(accountId, "standard_start");
        }

        // Unlock basic cosmetics (titles, portraits)
        var defaultCosmetics = new[]
        {
            "title_survivor",
            "portrait_default_warrior",
            "portrait_default_mystic",
            "portrait_default_adept",
            "portrait_default_skirmisher",
            "ui_theme_default"
        };

        foreach (var cosmeticId in defaultCosmetics)
        {
            var cosmetic = _cosmeticRepo.GetById(cosmeticId);
            if (cosmetic != null)
            {
                _cosmeticRepo.UnlockCosmetic(accountId, cosmeticId);
            }
        }

        // Create default cosmetic loadout
        var defaultLoadout = new CosmeticLoadout
        {
            AccountId = accountId,
            LoadoutName = "Default",
            SelectedTitle = null,
            SelectedPortrait = null,
            SelectedUITheme = "ui_theme_default",
            IsActive = true
        };
        _cosmeticRepo.CreateLoadout(defaultLoadout);

        _logger.Information("Default content unlocked for account {AccountId}", accountId);
    }

    #endregion
}
