using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Central manager for account progression system
/// Orchestrates all meta-progression services and provides unified API
/// </summary>
public class AccountManager
{
    private readonly ILogger _logger;
    private readonly AccountProgressionRepository _accountRepo;
    private readonly AchievementRepository _achievementRepo;
    private readonly CosmeticRepository _cosmeticRepo;
    private readonly AlternativeStartRepository _alternativeStartRepo;

    public AccountProgressionService AccountService { get; }
    public AchievementService AchievementService { get; }
    public MilestoneService MilestoneService { get; }
    public CosmeticService CosmeticService { get; }
    public AlternativeStartService AlternativeStartService { get; }

    public AccountManager(string connectionString)
    {
        _logger = Log.ForContext<AccountManager>();

        // Initialize repositories
        _accountRepo = new AccountProgressionRepository(connectionString);
        _achievementRepo = new AchievementRepository(connectionString);
        _cosmeticRepo = new CosmeticRepository(connectionString);
        _alternativeStartRepo = new AlternativeStartRepository(connectionString);

        // Initialize services
        AccountService = new AccountProgressionService(
            _accountRepo, _achievementRepo, _cosmeticRepo, _alternativeStartRepo);
        AchievementService = new AchievementService(
            _achievementRepo, _accountRepo, _cosmeticRepo);
        MilestoneService = new MilestoneService(
            _accountRepo, _cosmeticRepo, _alternativeStartRepo);
        CosmeticService = new CosmeticService(_cosmeticRepo);
        AlternativeStartService = new AlternativeStartService(_alternativeStartRepo);

        _logger.Information("AccountManager initialized");
    }

    #region Account Management

    /// <summary>
    /// Create new account and get account ID
    /// </summary>
    public int CreateAccount()
    {
        _logger.Information("Creating new account via AccountManager");
        return AccountService.CreateAccount();
    }

    /// <summary>
    /// Get account progression data
    /// </summary>
    public AccountProgression? GetAccount(int accountId)
    {
        return AccountService.GetAccount(accountId);
    }

    #endregion

    #region Character Creation Integration

    /// <summary>
    /// Create character with account progression integration
    /// </summary>
    public PlayerCharacter CreateCharacterWithAccountProgress(
        int accountId,
        CharacterClass characterClass,
        string characterName,
        string? alternativeStartId = null)
    {
        _logger.Information("Creating character with account progress: AccountID={AccountId}, Class={Class}, Name={Name}, AlternativeStart={AlternativeStart}",
            accountId, characterClass, characterName, alternativeStartId);

        // Verify alternative start is unlocked if specified
        if (!string.IsNullOrEmpty(alternativeStartId))
        {
            if (!_alternativeStartRepo.IsUnlocked(accountId, alternativeStartId))
            {
                _logger.Warning("Alternative start not unlocked, using standard start: {StartId}", alternativeStartId);
                alternativeStartId = "standard_start";
            }
        }

        // Create character with account integration
        var character = CharacterFactory.CreateCharacter(
            characterClass,
            characterName,
            accountId,
            AccountService,
            alternativeStartId,
            AlternativeStartService);

        _logger.Information("Character created with account integration: {CharacterName}", characterName);

        return character;
    }

    #endregion

    #region Gameplay Event Tracking

    /// <summary>
    /// Track tutorial completion
    /// </summary>
    public void OnTutorialCompleted(int accountId)
    {
        _logger.Information("Tutorial completed: AccountID={AccountId}", accountId);
        AchievementService.OnTutorialCompleted(accountId);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track campaign completion
    /// </summary>
    public void OnCampaignCompleted(int accountId, bool traumaFree = false)
    {
        _logger.Information("Campaign completed: AccountID={AccountId}, TraumaFree={TraumaFree}",
            accountId, traumaFree);

        AccountService.OnCampaignCompleted(accountId);
        AchievementService.OnCampaignCompleted(accountId);

        if (traumaFree)
        {
            AchievementService.OnTraumaFreeCampaignCompleted(accountId);
        }

        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track boss defeat
    /// </summary>
    public void OnBossDefeated(int accountId, string bossId, bool flawless = false)
    {
        _logger.Information("Boss defeated: AccountID={AccountId}, BossID={BossId}, Flawless={Flawless}",
            accountId, bossId, flawless);

        AccountService.OnBossDefeated(accountId);
        AchievementService.OnBossDefeated(accountId, bossId, flawless);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track level reached
    /// </summary>
    public void OnLevelReached(int accountId, int level)
    {
        _logger.Debug("Level reached: AccountID={AccountId}, Level={Level}", accountId, level);

        if (level >= 20)
        {
            AchievementService.OnMaxLevelReached(accountId);
            CheckMilestoneProgression(accountId);
        }
    }

    /// <summary>
    /// Track New Game+ tier completion
    /// </summary>
    public void OnNewGamePlusCompleted(int accountId, int tier)
    {
        _logger.Information("New Game+ completed: AccountID={AccountId}, Tier={Tier}",
            accountId, tier);

        AccountService.UpdateHighestNewGamePlusTier(accountId, tier);
        AchievementService.OnNewGamePlusCompleted(accountId, tier);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track endless mode wave reached
    /// </summary>
    public void OnEndlessWaveReached(int accountId, int wave)
    {
        _logger.Information("Endless wave reached: AccountID={AccountId}, Wave={Wave}",
            accountId, wave);

        AccountService.UpdateHighestEndlessWave(accountId, wave);
        AchievementService.OnEndlessWaveReached(accountId, wave);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track perfect sector clear (no damage taken)
    /// </summary>
    public void OnPerfectSectorClear(int accountId)
    {
        _logger.Information("Perfect sector clear: AccountID={AccountId}", accountId);
        AchievementService.OnPerfectSectorClear(accountId);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track all specializations unlocked
    /// </summary>
    public void OnAllSpecializationsUnlocked(int accountId)
    {
        _logger.Information("All specializations unlocked: AccountID={AccountId}", accountId);
        AchievementService.OnAllSpecializationsUnlocked(accountId);
        CheckMilestoneProgression(accountId);
    }

    /// <summary>
    /// Track codex entry unlock
    /// </summary>
    public void OnCodexEntryUnlocked(int accountId, string biomeId)
    {
        _logger.Debug("Codex entry unlocked: AccountID={AccountId}, BiomeID={BiomeId}",
            accountId, biomeId);
        AchievementService.OnCodexEntryUnlocked(accountId, biomeId);
    }

    /// <summary>
    /// Track enemy examination
    /// </summary>
    public void OnEnemyExamined(int accountId, string enemyType)
    {
        _logger.Debug("Enemy examined: AccountID={AccountId}, EnemyType={EnemyType}",
            accountId, enemyType);
        AchievementService.OnEnemyExamined(accountId, enemyType);
    }

    /// <summary>
    /// Track custom achievement progress
    /// </summary>
    public void TrackAchievementProgress(int accountId, string achievementId, int increment = 1)
    {
        AchievementService.TrackProgress(accountId, achievementId, increment);
        CheckMilestoneProgression(accountId);
    }

    #endregion

    #region Milestone & Progression

    /// <summary>
    /// Check milestone tier progression
    /// Should be called after any achievement unlock
    /// </summary>
    public void CheckMilestoneProgression(int accountId)
    {
        MilestoneService.CheckMilestoneTierProgression(accountId);
    }

    /// <summary>
    /// Get current milestone tier
    /// </summary>
    public MilestoneTier? GetCurrentMilestoneTier(int accountId)
    {
        return MilestoneService.GetCurrentTier(accountId);
    }

    /// <summary>
    /// Get progress to next tier
    /// </summary>
    public (int CurrentPoints, int RequiredPoints, float ProgressPercent) GetProgressToNextTier(int accountId)
    {
        return MilestoneService.GetProgressToNextTier(accountId);
    }

    #endregion

    #region Achievement & Cosmetic Access

    /// <summary>
    /// Get all achievements with progress for account
    /// </summary>
    public List<(Achievement Achievement, AchievementProgress? Progress)> GetAchievementsWithProgress(int accountId)
    {
        return AchievementService.GetAchievementsWithProgress(accountId);
    }

    /// <summary>
    /// Get unlocked achievements
    /// </summary>
    public List<Achievement> GetUnlockedAchievements(int accountId)
    {
        return AchievementService.GetUnlockedAchievements(accountId);
    }

    /// <summary>
    /// Get unlocked cosmetics
    /// </summary>
    public List<Cosmetic> GetUnlockedCosmetics(int accountId)
    {
        return CosmeticService.GetUnlockedCosmetics(accountId);
    }

    /// <summary>
    /// Get active cosmetic loadout
    /// </summary>
    public CosmeticLoadout? GetActiveLoadout(int accountId)
    {
        return CosmeticService.GetActiveLoadout(accountId);
    }

    /// <summary>
    /// Get unlocked alternative starts
    /// </summary>
    public List<AlternativeStart> GetUnlockedAlternativeStarts(int accountId)
    {
        return AlternativeStartService.GetUnlockedStarts(accountId);
    }

    #endregion

    #region Statistics & Summary

    /// <summary>
    /// Get account statistics summary
    /// </summary>
    public AccountStatistics GetAccountStatistics(int accountId)
    {
        var account = AccountService.GetAccount(accountId);
        if (account == null)
        {
            return new AccountStatistics();
        }

        var unlockedAchievements = AchievementService.GetUnlockedAchievements(accountId);
        var currentTier = MilestoneService.GetCurrentTier(accountId);
        var (currentPoints, requiredPoints, progressPercent) = MilestoneService.GetProgressToNextTier(accountId);

        return new AccountStatistics
        {
            AccountId = accountId,
            TotalAchievementPoints = account.TotalAchievementPoints,
            TotalAchievementsUnlocked = account.TotalAchievementsUnlocked,
            CurrentMilestoneTier = account.CurrentMilestoneTier,
            CurrentMilestoneTierName = currentTier?.TierName ?? "Unknown",
            PointsToNextTier = requiredPoints - currentPoints,
            ProgressToNextTierPercent = progressPercent,
            TotalCharactersCreated = account.TotalCharactersCreated,
            TotalCampaignsCompleted = account.TotalCampaignsCompleted,
            TotalBossesDefeated = account.TotalBossesDefeated,
            HighestNewGamePlusTier = account.HighestNewGamePlusTier,
            HighestEndlessWave = account.HighestEndlessWave
        };
    }

    /// <summary>
    /// Display account summary to console
    /// </summary>
    public void DisplayAccountSummary(int accountId)
    {
        var stats = GetAccountStatistics(accountId);
        var currentTier = MilestoneService.GetCurrentTier(accountId);

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  ACCOUNT PROGRESSION - ID: {accountId}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"  Milestone Tier: {currentTier?.TierName ?? "Unknown"} (Tier {stats.CurrentMilestoneTier}/10)");
        Console.WriteLine($"  Achievement Points: {stats.TotalAchievementPoints} ({stats.ProgressToNextTierPercent:F1}% to next tier)");
        Console.WriteLine($"  Achievements Unlocked: {stats.TotalAchievementsUnlocked}");
        Console.WriteLine();
        Console.WriteLine("  Statistics:");
        Console.WriteLine($"    Characters Created: {stats.TotalCharactersCreated}");
        Console.WriteLine($"    Campaigns Completed: {stats.TotalCampaignsCompleted}");
        Console.WriteLine($"    Bosses Defeated: {stats.TotalBossesDefeated}");
        Console.WriteLine($"    Highest NG+ Tier: {stats.HighestNewGamePlusTier}");
        Console.WriteLine($"    Highest Endless Wave: {stats.HighestEndlessWave}");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    #endregion
}

/// <summary>
/// Account statistics summary
/// </summary>
public class AccountStatistics
{
    public int AccountId { get; set; }
    public int TotalAchievementPoints { get; set; }
    public int TotalAchievementsUnlocked { get; set; }
    public int CurrentMilestoneTier { get; set; }
    public string CurrentMilestoneTierName { get; set; } = string.Empty;
    public int PointsToNextTier { get; set; }
    public float ProgressToNextTierPercent { get; set; }
    public int TotalCharactersCreated { get; set; }
    public int TotalCampaignsCompleted { get; set; }
    public int TotalBossesDefeated { get; set; }
    public int HighestNewGamePlusTier { get; set; }
    public int HighestEndlessWave { get; set; }
}
