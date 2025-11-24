using Xunit;
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.41: Tests for meta-progression system
/// </summary>
public class MetaProgressionTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly string _connectionString;
    private readonly AccountProgressionRepository _accountRepo;
    private readonly AchievementRepository _achievementRepo;
    private readonly CosmeticRepository _cosmeticRepo;
    private readonly AlternativeStartRepository _alternativeStartRepo;

    public MetaProgressionTests()
    {
        // Create test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_metaprogression_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_testDbPath}";

        // Initialize repositories (creates tables)
        _accountRepo = new AccountProgressionRepository(_connectionString);
        _achievementRepo = new AchievementRepository(_connectionString);
        _cosmeticRepo = new CosmeticRepository(_connectionString);
        _alternativeStartRepo = new AlternativeStartRepository(_connectionString);

        // Seed test data
        var seeder = new MetaProgressionSeeder(_connectionString);
        seeder.SeedAll();
    }

    public void Dispose()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region Account Progression Tests

    [Fact]
    public void CreateAccount_ShouldCreateNewAccount()
    {
        // Act
        var accountId = _accountRepo.CreateAccount();

        // Assert
        Assert.True(accountId > 0);

        var account = _accountRepo.GetById(accountId);
        Assert.NotNull(account);
        Assert.Equal(0, account.TotalAchievementPoints);
        Assert.Equal(1, account.CurrentMilestoneTier);
    }

    [Fact]
    public void UpdateAccount_ShouldPersistChanges()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var account = _accountRepo.GetById(accountId)!;

        // Act
        account.TotalAchievementPoints = 100;
        account.TotalBossesDefeated = 5;
        _accountRepo.Update(account);

        // Assert
        var updated = _accountRepo.GetById(accountId)!;
        Assert.Equal(100, updated.TotalAchievementPoints);
        Assert.Equal(5, updated.TotalBossesDefeated);
    }

    [Fact]
    public void UnlockBenefit_ShouldMarkUnlockAsUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();

        // Act
        _accountRepo.UnlockBenefit(accountId, "legend_boost_5");

        // Assert
        var unlocks = _accountRepo.GetUnlockedBenefits(accountId);
        Assert.Contains(unlocks, u => u.UnlockId == "legend_boost_5");
    }

    #endregion

    #region Achievement Tests

    [Fact]
    public void GetAllAchievements_ShouldReturnSeededAchievements()
    {
        // Act
        var achievements = _achievementRepo.GetAll();

        // Assert
        Assert.NotEmpty(achievements);
        Assert.Contains(achievements, a => a.AchievementId == "first_steps");
        Assert.Contains(achievements, a => a.AchievementId == "survivor");
    }

    [Fact]
    public void TrackProgress_ShouldIncrementProgress()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();

        // Act
        var progress = _achievementRepo.IncrementProgress(accountId, "first_steps", 1);

        // Assert
        Assert.Equal(1, progress.CurrentProgress);
    }

    [Fact]
    public void UnlockAchievement_ShouldMarkAsUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();

        // Act
        _achievementRepo.UnlockAchievement(accountId, "first_steps");

        // Assert
        Assert.True(_achievementRepo.IsUnlocked(accountId, "first_steps"));
    }

    [Fact]
    public void GetByCategory_ShouldReturnOnlyMatchingCategory()
    {
        // Act
        var milestoneAchievements = _achievementRepo.GetByCategory(AchievementCategory.Milestone);

        // Assert
        Assert.NotEmpty(milestoneAchievements);
        Assert.All(milestoneAchievements, a => Assert.Equal(AchievementCategory.Milestone, a.Category));
    }

    #endregion

    #region Cosmetic Tests

    [Fact]
    public void GetAllCosmetics_ShouldReturnSeededCosmetics()
    {
        // Act
        var cosmetics = _cosmeticRepo.GetAll();

        // Assert
        Assert.NotEmpty(cosmetics);
        Assert.Contains(cosmetics, c => c.CosmeticId == "title_survivor");
    }

    [Fact]
    public void UnlockCosmetic_ShouldMarkAsUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();

        // Act
        _cosmeticRepo.UnlockCosmetic(accountId, "title_survivor");

        // Assert
        Assert.True(_cosmeticRepo.IsUnlocked(accountId, "title_survivor"));
    }

    [Fact]
    public void CreateLoadout_ShouldPersistLoadout()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        _cosmeticRepo.UnlockCosmetic(accountId, "title_survivor");
        _cosmeticRepo.UnlockCosmetic(accountId, "ui_theme_default");

        var loadout = new CosmeticLoadout
        {
            AccountId = accountId,
            LoadoutName = "Test Loadout",
            SelectedTitle = "title_survivor",
            SelectedUITheme = "ui_theme_default",
            IsActive = true
        };

        // Act
        var loadoutId = _cosmeticRepo.CreateLoadout(loadout);

        // Assert
        var retrieved = _cosmeticRepo.GetLoadoutById(loadoutId);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Loadout", retrieved.LoadoutName);
        Assert.Equal("title_survivor", retrieved.SelectedTitle);
    }

    [Fact]
    public void GetActiveLoadout_ShouldReturnActiveLoadout()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        _cosmeticRepo.UnlockCosmetic(accountId, "ui_theme_default");

        var loadout = new CosmeticLoadout
        {
            AccountId = accountId,
            LoadoutName = "Active Test",
            SelectedUITheme = "ui_theme_default",
            IsActive = true
        };
        _cosmeticRepo.CreateLoadout(loadout);

        // Act
        var active = _cosmeticRepo.GetActiveLoadout(accountId);

        // Assert
        Assert.NotNull(active);
        Assert.Equal("Active Test", active.LoadoutName);
        Assert.True(active.IsActive);
    }

    #endregion

    #region Alternative Start Tests

    [Fact]
    public void GetAllStarts_ShouldReturnSeededStarts()
    {
        // Act
        var starts = _alternativeStartRepo.GetAll();

        // Assert
        Assert.NotEmpty(starts);
        Assert.Contains(starts, s => s.StartId == "standard_start");
        Assert.Contains(starts, s => s.StartId == "veterans_return");
    }

    [Fact]
    public void UnlockStart_ShouldMarkAsUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();

        // Act
        _alternativeStartRepo.UnlockStart(accountId, "veterans_return");

        // Assert
        Assert.True(_alternativeStartRepo.IsUnlocked(accountId, "veterans_return"));
    }

    [Fact]
    public void GetUnlockedStarts_ShouldReturnOnlyUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        _alternativeStartRepo.UnlockStart(accountId, "standard_start");
        _alternativeStartRepo.UnlockStart(accountId, "veterans_return");

        // Act
        var unlocked = _alternativeStartRepo.GetUnlockedStarts(accountId);

        // Assert
        Assert.Equal(2, unlocked.Count);
        Assert.All(unlocked, s => Assert.True(s.IsUnlocked));
    }

    #endregion

    #region Service Tests

    [Fact]
    public void AccountProgressionService_OnBossDefeated_ShouldIncrementCount()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var service = new AccountProgressionService(
            _accountRepo, _achievementRepo, _cosmeticRepo, _alternativeStartRepo);

        // Act
        service.OnBossDefeated(accountId);
        service.OnBossDefeated(accountId);

        // Assert
        var account = _accountRepo.GetById(accountId)!;
        Assert.Equal(2, account.TotalBossesDefeated);
    }

    [Fact]
    public void AchievementService_UnlockAchievement_ShouldAwardPoints()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var service = new AchievementService(_achievementRepo, _accountRepo, _cosmeticRepo);

        // Act
        service.UnlockAchievement(accountId, "first_steps"); // 5 points

        // Assert
        var account = _accountRepo.GetById(accountId)!;
        Assert.Equal(5, account.TotalAchievementPoints);
        Assert.Equal(1, account.TotalAchievementsUnlocked);
    }

    [Fact]
    public void MilestoneService_CheckProgression_ShouldAdvanceTier()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var milestoneService = new MilestoneService(
            _accountRepo, _cosmeticRepo, _alternativeStartRepo);

        // Set achievement points to tier 2 threshold
        var account = _accountRepo.GetById(accountId)!;
        account.TotalAchievementPoints = 50; // Tier 2 requires 50 points
        _accountRepo.Update(account);

        // Act
        milestoneService.CheckMilestoneTierProgression(accountId);

        // Assert
        var updated = _accountRepo.GetById(accountId)!;
        Assert.Equal(2, updated.CurrentMilestoneTier);
    }

    [Fact]
    public void CosmeticService_ApplyLoadout_ShouldValidateUnlocked()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var service = new CosmeticService(_cosmeticRepo);

        var loadout = new CosmeticLoadout
        {
            AccountId = accountId,
            LoadoutName = "Test",
            SelectedTitle = "title_not_unlocked" // This cosmetic doesn't exist or isn't unlocked
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.CreateLoadout(loadout));
    }

    [Fact]
    public void AlternativeStartService_InitializeCharacter_ShouldApplyModifications()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        _alternativeStartRepo.UnlockStart(accountId, "veterans_return");

        var service = new AlternativeStartService(_alternativeStartRepo);
        var character = new PlayerCharacter
        {
            Name = "Test Hero",
            CurrentMilestone = 1,
            Currency = 50
        };

        // Act
        service.InitializeCharacterWithScenario(accountId, character, "veterans_return");

        // Assert - veterans_return gives 100 currency
        Assert.Equal(100, character.Currency);
    }

    #endregion

    #region Milestone Tier Tests

    [Fact]
    public void GetAllTiers_ShouldReturn10Tiers()
    {
        // Act
        var tiers = _accountRepo.GetAllTiers();

        // Assert
        Assert.Equal(10, tiers.Count);
        Assert.Equal("Initiate", tiers[0].TierName);
        Assert.Equal("Transcendent", tiers[9].TierName);
    }

    [Fact]
    public void GetProgressToNextTier_ShouldCalculateCorrectly()
    {
        // Arrange
        var accountId = _accountRepo.CreateAccount();
        var account = _accountRepo.GetById(accountId)!;
        account.TotalAchievementPoints = 25; // Halfway to Tier 2 (requires 50)
        _accountRepo.Update(account);

        var milestoneService = new MilestoneService(
            _accountRepo, _cosmeticRepo, _alternativeStartRepo);

        // Act
        var (currentPoints, requiredPoints, progressPercent) =
            milestoneService.GetProgressToNextTier(accountId);

        // Assert
        Assert.Equal(25, currentPoints);
        Assert.Equal(50, requiredPoints);
        Assert.Equal(50f, progressPercent); // 50% progress
    }

    #endregion
}
