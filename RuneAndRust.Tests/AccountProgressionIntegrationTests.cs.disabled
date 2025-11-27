using Xunit;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.41: Integration tests for complete account progression flow
/// Tests the full meta-progression system end-to-end
/// </summary>
public class AccountProgressionIntegrationTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly string _connectionString;
    private readonly AccountManager _accountManager;

    public AccountProgressionIntegrationTests()
    {
        // Create test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_integration_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_testDbPath}";

        // Initialize AccountManager (will create all tables and seed data)
        _accountManager = new AccountManager(_connectionString);
    }

    public void Dispose()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region Full Progression Flow Tests

    [Fact]
    public void CompleteGameplayFlow_ShouldProgressThroughTiers()
    {
        // Act 1: Create account and character
        var accountId = _accountManager.CreateAccount();
        Assert.True(accountId > 0);

        var character = _accountManager.CreateCharacterWithAccountProgress(
            accountId: accountId,
            characterClass: CharacterClass.Warrior,
            characterName: "Test Hero",
            alternativeStartId: "standard_start"
        );

        Assert.NotNull(character);
        Assert.Equal("Test Hero", character.Name);

        // Verify starting tier
        var initialStats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(1, initialStats.CurrentMilestoneTier);
        Assert.Equal(0, initialStats.TotalAchievementPoints);

        // Act 2: Complete tutorial
        _accountManager.OnTutorialCompleted(accountId);

        var afterTutorial = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(5, afterTutorial.TotalAchievementPoints); // "First Steps" = 5 pts
        Assert.Equal(1, afterTutorial.TotalAchievementsUnlocked);

        // Act 3: Defeat some bosses
        _accountManager.OnBossDefeated(accountId, "test_boss_1", flawless: false);
        _accountManager.OnBossDefeated(accountId, "test_boss_2", flawless: true);
        _accountManager.OnBossDefeated(accountId, "test_boss_3", flawless: false);

        var afterBosses = _accountManager.GetAccountStatistics(accountId);
        Assert.True(afterBosses.TotalBossesDefeated >= 3);
        Assert.True(afterBosses.TotalAchievementPoints > 5); // Boss achievements earned

        // Act 4: Complete campaign
        _accountManager.OnCampaignCompleted(accountId, traumaFree: false);

        var afterCampaign = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(1, afterCampaign.TotalCampaignsCompleted);
        Assert.True(afterCampaign.TotalAchievementPoints >= 15); // Tutorial + Survivor + Boss achievements

        // Act 5: Reach max level
        _accountManager.OnLevelReached(accountId, 20);

        var afterMaxLevel = _accountManager.GetAccountStatistics(accountId);
        Assert.True(afterMaxLevel.TotalAchievementPoints >= 25); // Added "Legend" achievement

        // Verify tier progression occurred
        var finalStats = _accountManager.GetAccountStatistics(accountId);
        Assert.True(finalStats.CurrentMilestoneTier >= 1);

        // If enough points, should be tier 2+
        if (finalStats.TotalAchievementPoints >= 50)
        {
            Assert.True(finalStats.CurrentMilestoneTier >= 2);
        }
    }

    [Fact]
    public void NewGamePlusProgression_ShouldTrackHighestTier()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Complete multiple NG+ tiers
        _accountManager.OnNewGamePlusCompleted(accountId, 1);
        _accountManager.OnNewGamePlusCompleted(accountId, 3);
        _accountManager.OnNewGamePlusCompleted(accountId, 2); // Out of order
        _accountManager.OnNewGamePlusCompleted(accountId, 5);

        // Assert: Should track highest tier
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(5, stats.HighestNewGamePlusTier);

        // Should have unlocked NG+ achievements
        var unlockedAchievements = _accountManager.GetUnlockedAchievements(accountId);
        Assert.Contains(unlockedAchievements, a => a.AchievementId.Contains("ng_plus"));
    }

    [Fact]
    public void EndlessMode_ShouldTrackWaveMilestones()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Progress through waves
        _accountManager.OnEndlessWaveReached(accountId, 10);
        _accountManager.OnEndlessWaveReached(accountId, 20);
        _accountManager.OnEndlessWaveReached(accountId, 30);
        _accountManager.OnEndlessWaveReached(accountId, 40);
        _accountManager.OnEndlessWaveReached(accountId, 50);

        // Assert
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(50, stats.HighestEndlessWave);

        // Should have unlocked wave achievements
        var unlockedAchievements = _accountManager.GetUnlockedAchievements(accountId);
        Assert.True(unlockedAchievements.Any());
    }

    [Fact]
    public void TraumaFreeRun_ShouldUnlockIronWillAchievement()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Complete trauma-free campaign
        _accountManager.OnCampaignCompleted(accountId, traumaFree: true);

        // Assert
        var unlockedAchievements = _accountManager.GetUnlockedAchievements(accountId);
        Assert.Contains(unlockedAchievements, a => a.AchievementId == "iron_will");

        // Iron Will is worth 50 points
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.True(stats.TotalAchievementPoints >= 50);
    }

    #endregion

    #region Character Creation with Account Unlocks

    [Fact]
    public void CharacterCreation_WithAccountUnlocks_ShouldApplyBenefits()
    {
        // Arrange: Create account and unlock benefits
        var accountId = _accountManager.CreateAccount();

        // Manually unlock some benefits for testing
        _accountManager.AccountService.UnlockBenefit(accountId, "legend_boost_5");
        _accountManager.AccountService.UnlockBenefit(accountId, "starting_resources_50");

        // Act: Create character
        var character = _accountManager.CreateCharacterWithAccountProgress(
            accountId: accountId,
            characterClass: CharacterClass.Warrior,
            characterName: "Blessed Hero"
        );

        // Assert: Unlocks should be applied
        Assert.NotNull(character);

        // Starting resources should be boosted (50 base * 1.5 = 75)
        Assert.Equal(75, character.Currency);

        // Account should track character creation
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(1, stats.TotalCharactersCreated);
    }

    [Fact]
    public void AlternativeStart_VeteransReturn_ShouldSkipTutorial()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Unlock veteran's return
        _accountManager.AlternativeStartService.AlternativeStartService
            .UnlockStart(accountId, "veterans_return");

        // Act: Create character with alternative start
        var character = _accountManager.CreateCharacterWithAccountProgress(
            accountId: accountId,
            characterClass: CharacterClass.Warrior,
            characterName: "Veteran Warrior",
            alternativeStartId: "veterans_return"
        );

        // Assert: Should have applied veteran's start modifications
        Assert.NotNull(character);
        Assert.Equal(100, character.Currency); // Veteran's start gives 100 currency
    }

    [Fact]
    public void AlternativeStart_NotUnlocked_ShouldUseStandardStart()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Do NOT unlock advanced_explorer (requires NG+3)

        // Act: Try to create with locked start
        var character = _accountManager.CreateCharacterWithAccountProgress(
            accountId: accountId,
            characterClass: CharacterClass.Mystic,
            characterName: "Eager Mystic",
            alternativeStartId: "advanced_explorer" // LOCKED
        );

        // Assert: Should fall back to standard start
        Assert.NotNull(character);
        Assert.Equal(1, character.CurrentMilestone); // Level 1 (not level 5)
    }

    #endregion

    #region Cosmetic System Integration

    [Fact]
    public void UnlockCosmetics_ThroughAchievements_ShouldPersist()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Unlock achievement that rewards cosmetics
        _accountManager.OnTutorialCompleted(accountId);

        // Assert: Cosmetic should be unlocked
        var unlockedCosmetics = _accountManager.GetUnlockedCosmetics(accountId);
        Assert.Contains(unlockedCosmetics, c => c.CosmeticId == "title_initiate");
    }

    [Fact]
    public void CosmeticLoadout_Creation_ShouldPersist()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Unlock some cosmetics
        _accountManager.CosmeticService.UnlockCosmetic(accountId, "title_survivor");
        _accountManager.CosmeticService.UnlockCosmetic(accountId, "ui_theme_default");

        // Act: Create custom loadout
        var loadout = new CosmeticLoadout
        {
            AccountId = accountId,
            LoadoutName = "My Loadout",
            SelectedTitle = "title_survivor",
            SelectedUITheme = "ui_theme_default",
            IsActive = true
        };

        var loadoutId = _accountManager.CosmeticService.CosmeticService.CreateLoadout(loadout);

        // Assert: Loadout should be retrievable
        Assert.True(loadoutId > 0);

        var retrievedLoadout = _accountManager.CosmeticService.CosmeticService.GetLoadoutById(loadoutId);
        Assert.NotNull(retrievedLoadout);
        Assert.Equal("My Loadout", retrievedLoadout.LoadoutName);
        Assert.Equal("title_survivor", retrievedLoadout.SelectedTitle);
    }

    #endregion

    #region Milestone Tier Progression

    [Fact]
    public void MilestoneTierProgression_WithEnoughPoints_ShouldAdvanceTiers()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();
        var account = _accountManager.GetAccount(accountId)!;

        // Act: Manually set achievement points to tier 3 threshold
        account.TotalAchievementPoints = 150; // Tier 3 requires 150
        _accountManager.AccountService.AccountService.Update(account);

        // Trigger tier check
        _accountManager.CheckMilestoneProgression(accountId);

        // Assert: Should be tier 3
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(3, stats.CurrentMilestoneTier);
        Assert.Equal("Veteran", stats.CurrentMilestoneTierName);

        // Tier 3 unlocks should be available
        var unlocks = _accountManager.AccountService.GetUnlockedBenefits(accountId);
        Assert.Contains(unlocks, u => u.UnlockId == "legend_boost_5");
    }

    [Fact]
    public void MilestoneProgress_Calculation_ShouldBeAccurate()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();
        var account = _accountManager.GetAccount(accountId)!;

        // Set to halfway between tier 2 and tier 3
        account.TotalAchievementPoints = 100; // Tier 2 = 50, Tier 3 = 150
        _accountManager.AccountService.AccountService.Update(account);
        _accountManager.CheckMilestoneProgression(accountId);

        // Act
        var (currentPoints, requiredPoints, progressPercent) =
            _accountManager.GetProgressToNextTier(accountId);

        // Assert: Should be tier 2, 50% to tier 3
        Assert.Equal(100, currentPoints);
        Assert.Equal(150, requiredPoints);
        Assert.Equal(50.0f, progressPercent); // (100 - 50) / (150 - 50) = 50%
    }

    #endregion

    #region Achievement System Edge Cases

    [Fact]
    public void Achievement_AlreadyUnlocked_ShouldNotDuplicatePoints()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Complete tutorial twice
        _accountManager.OnTutorialCompleted(accountId);
        var pointsAfterFirst = _accountManager.GetAccountStatistics(accountId).TotalAchievementPoints;

        _accountManager.OnTutorialCompleted(accountId);
        var pointsAfterSecond = _accountManager.GetAccountStatistics(accountId).TotalAchievementPoints;

        // Assert: Points should not increase
        Assert.Equal(pointsAfterFirst, pointsAfterSecond);
    }

    [Fact]
    public void Achievement_IncrementalProgress_ShouldAccumulate()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Track incremental progress (e.g., kill 100 enemies)
        for (int i = 0; i < 100; i++)
        {
            _accountManager.TrackAchievementProgress(accountId, "boss_slayer", 1);
        }

        // Assert: Progress should accumulate
        var achievementsWithProgress = _accountManager.GetAchievementsWithProgress(accountId);
        var bossSlayerProgress = achievementsWithProgress
            .FirstOrDefault(a => a.Achievement.AchievementId == "boss_slayer").Progress;

        Assert.NotNull(bossSlayerProgress);
        Assert.True(bossSlayerProgress.CurrentProgress >= 1);
    }

    #endregion

    #region Multiple Character Tests

    [Fact]
    public void MultipleCharacters_ShouldShareAccountProgress()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act: Create multiple characters
        var warrior = _accountManager.CreateCharacterWithAccountProgress(
            accountId, CharacterClass.Warrior, "Warrior");
        var mystic = _accountManager.CreateCharacterWithAccountProgress(
            accountId, CharacterClass.Mystic, "Mystic");

        // Complete tutorial with first character
        _accountManager.OnTutorialCompleted(accountId);

        // Assert: Account should track 2 characters created, 1 tutorial
        var stats = _accountManager.GetAccountStatistics(accountId);
        Assert.Equal(2, stats.TotalCharactersCreated);
        Assert.Equal(1, stats.TotalAchievementsUnlocked); // Tutorial achievement

        // Both characters should benefit from account unlocks
        Assert.NotNull(warrior);
        Assert.NotNull(mystic);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void BulkAchievementTracking_ShouldBeEfficient()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act: Track 1000 achievement events
        for (int i = 0; i < 1000; i++)
        {
            _accountManager.TrackAchievementProgress(accountId, "boss_slayer", 1);
        }

        stopwatch.Stop();

        // Assert: Should complete in reasonable time (< 5 seconds)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Bulk tracking took {stopwatch.ElapsedMilliseconds}ms (expected < 5000ms)");
    }

    #endregion

    #region Display Tests

    [Fact]
    public void DisplayAccountSummary_ShouldNotThrow()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Simulate some progress
        _accountManager.OnTutorialCompleted(accountId);
        _accountManager.OnBossDefeated(accountId, "test_boss", flawless: true);

        // Act & Assert: Should not throw
        var exception = Record.Exception(() =>
            _accountManager.DisplayAccountSummary(accountId));

        Assert.Null(exception);
    }

    [Fact]
    public void GetAccountStatistics_WithNoProgress_ShouldReturnDefaults()
    {
        // Arrange
        var accountId = _accountManager.CreateAccount();

        // Act
        var stats = _accountManager.GetAccountStatistics(accountId);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalAchievementPoints);
        Assert.Equal(1, stats.CurrentMilestoneTier);
        Assert.Equal("Initiate", stats.CurrentMilestoneTierName);
    }

    #endregion
}
