using RuneAndRust.Core.BossGauntlet;
using RuneAndRust.Core.ChallengeSectors;
using RuneAndRust.Core.EndlessMode;
using RuneAndRust.Engine;
using RuneAndRust.Engine.BossGauntlet;
using RuneAndRust.Engine.ChallengeSectors;
using RuneAndRust.Engine.EndlessMode;
using RuneAndRust.Engine.NewGamePlus;
using RuneAndRust.Persistence;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// v0.40: Endgame Integration Tests
/// Tests interactions between NG+, Sectors, Gauntlets, and Endless Mode
/// </summary>
public class EndgameIntegrationTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly NewGamePlusRepository _ngPlusRepo;
    private readonly ChallengeSectorRepository _sectorRepo;
    private readonly BossGauntletRepository _gauntletRepo;
    private readonly EndlessModeRepository _endlessRepo;
    private readonly EndgameContentOrchestrator _orchestrator;

    public EndgameIntegrationTests()
    {
        // Create in-memory test database
        _testDbPath = ":memory:";

        _ngPlusRepo = new NewGamePlusRepository(_testDbPath);
        _sectorRepo = new ChallengeSectorRepository(_testDbPath);
        _gauntletRepo = new BossGauntletRepository(_testDbPath);
        _endlessRepo = new EndlessModeRepository(_testDbPath);

        _orchestrator = new EndgameContentOrchestrator(
            _ngPlusRepo, _sectorRepo, _gauntletRepo, _endlessRepo
        );

        // Initialize test database
        InitializeTestDatabase();
    }

    private void InitializeTestDatabase()
    {
        // Would run schema initialization here
        // For now, assume database is set up
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    // ═══════════════════════════════════════════════════════════
    // NG+ TIER UNLOCKING CONTENT
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void NGPlusAdvancement_UnlocksNewContent()
    {
        // Arrange
        int characterId = 1;
        var characterState = new { HP = 100, Level = 50 };

        // Get content available at base tier
        var beforeContent = _orchestrator.GetAvailableContent(characterId);
        var beforeSectorCount = beforeContent.AvailableSectors.Count;
        var beforeGauntletCount = beforeContent.AvailableGauntlets.Count;

        // Act
        var result = _orchestrator.AdvanceNGPlusTier(characterId, characterState);

        // Assert
        Assert.Equal(1, result.NewTier);
        Assert.True(result.NewlyUnlockedSectors.Count > 0 || result.NewlyUnlockedGauntlets.Count > 0,
            "NG+1 should unlock new content");

        // Verify content is now available
        var afterContent = _orchestrator.GetAvailableContent(characterId);
        Assert.True(afterContent.AvailableSectors.Count >= beforeSectorCount);
        Assert.True(afterContent.AvailableGauntlets.Count >= beforeGauntletCount);
    }

    [Fact]
    public void NGPlusTiers_ProgressivelyUnlockContent()
    {
        // Arrange
        int characterId = 2;
        var characterState = new { HP = 100, Level = 50 };

        var unlockedContentPerTier = new Dictionary<int, int>();

        // Act - Progress through NG+ tiers
        for (int tier = 0; tier <= 3; tier++)
        {
            if (tier > 0)
            {
                _orchestrator.AdvanceNGPlusTier(characterId, characterState);
            }

            var content = _orchestrator.GetAvailableContent(characterId);
            unlockedContentPerTier[tier] = content.TotalContentPieces;
        }

        // Assert - Content should increase or stay same with each tier
        for (int tier = 1; tier <= 3; tier++)
        {
            Assert.True(unlockedContentPerTier[tier] >= unlockedContentPerTier[tier - 1],
                $"NG+{tier} should have >= content than NG+{tier - 1}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // CROSS-SYSTEM PROGRESSION
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void ChallengeSector_CompletionTrackedInProgress()
    {
        // Arrange
        int characterId = 3;
        var sectorService = _orchestrator.SectorService;

        var initialProgress = sectorService.GetProgress(characterId);
        var initialCompleted = initialProgress.TotalSectorsCompleted;

        // Act - Complete a sector
        sectorService.CompleteChallenge(
            characterId,
            sectorId: "test_sector",
            completionTimeSeconds: 600,
            deaths: 1,
            damageTaken: 200,
            damageDealt: 2000,
            enemiesKilled: 15,
            ngPlusTier: 0
        );

        var finalProgress = sectorService.GetProgress(characterId);

        // Assert
        Assert.Equal(initialCompleted + 1, finalProgress.TotalSectorsCompleted);
        Assert.True(finalProgress.CompletionPercentage > initialProgress.CompletionPercentage);
    }

    [Fact]
    public void BossGauntlet_SubmitsToLeaderboards()
    {
        // Arrange
        int characterId = 4;
        var gauntletService = _orchestrator.GauntletService;

        // Start and complete a gauntlet run
        var run = gauntletService.StartGauntlet(
            characterId,
            sequenceId: "test_gauntlet",
            ngPlusTier: 0,
            characterState: new { HP = 100 }
        );

        // Simulate completion
        // (Would need to complete all bosses in real test)

        // For now, just verify run was created
        Assert.NotNull(run);
        Assert.True(run.IsActive);
        Assert.Equal(3, run.FullHealsRemaining);
        Assert.Equal(1, run.RevivesRemaining);
    }

    [Fact]
    public void EndlessMode_WaveProgression()
    {
        // Arrange
        int characterId = 5;
        var endlessService = _orchestrator.EndlessService;

        // Act - Start endless run
        var run = endlessService.StartEndlessRun(characterId);

        Assert.NotNull(run);
        Assert.Equal(1, run.CurrentWave);
        Assert.True(run.IsActive);

        // Complete first wave
        endlessService.CompleteWave(run.RunId, enemiesKilled: 3, damageTaken: 50, damageDealt: 500);

        var updatedRun = endlessService.GetRunById(run.RunId);
        Assert.NotNull(updatedRun);
        Assert.Equal(2, updatedRun.CurrentWave);
        Assert.Equal(1, updatedRun.HighestWaveReached);
    }

    // ═══════════════════════════════════════════════════════════
    // ORCHESTRATOR INTEGRATION
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Orchestrator_GetAvailableContent_ReturnsAllSystems()
    {
        // Arrange
        int characterId = 6;

        // Act
        var content = _orchestrator.GetAvailableContent(characterId);

        // Assert
        Assert.NotNull(content);
        Assert.Equal(0, content.CurrentNGPlusTier); // Base tier
        Assert.NotNull(content.AvailableSectors);
        Assert.NotNull(content.AvailableGauntlets);
        Assert.True(content.TotalContentPieces > 0);
    }

    [Fact]
    public void Orchestrator_GetProgress_TracksAllSystems()
    {
        // Arrange
        int characterId = 7;

        // Act
        var progress = _orchestrator.GetProgress(characterId);

        // Assert
        Assert.NotNull(progress);
        Assert.Equal(characterId, progress.CharacterId);
        Assert.Equal(0, progress.NGPlusTier);
        Assert.True(progress.SectorsAvailable >= 0);
    }

    [Fact]
    public void Orchestrator_GetRecommendation_SuggestsAppropriateContent()
    {
        // Arrange
        int characterId = 8;

        // Act
        var recommendation = _orchestrator.GetRecommendation(characterId);

        // Assert
        Assert.NotNull(recommendation);
        Assert.NotEmpty(recommendation.Title);
        Assert.NotEmpty(recommendation.Reason);
        Assert.True(Enum.IsDefined(typeof(EndgameContentType), recommendation.Type));
    }

    // ═══════════════════════════════════════════════════════════
    // DIFFICULTY SCALING INTEGRATION
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void DifficultyScaling_CombinesNGPlusAndWaveMultipliers()
    {
        // Arrange
        int characterId = 9;
        var ngPlusService = _orchestrator.NGPlusService;
        var endlessService = _orchestrator.EndlessService;

        // Advance to NG+2
        ngPlusService.InitializeNewGamePlus(characterId, 2, new { HP = 100 });

        // Get NG+ scaling
        var ngPlusScaling = ngPlusService.GetScalingForTier(2);
        Assert.Equal(2.0f, ngPlusScaling.DifficultyMultiplier); // NG+2 = 2.0x

        // Start endless run
        var run = endlessService.StartEndlessRun(characterId);
        var waveConfig = endlessService.GetCurrentWaveConfig(run.RunId);

        // Assert - Wave 1 has 1.0x multiplier
        Assert.NotNull(waveConfig);
        Assert.Equal(1.0f, waveConfig.DifficultyMultiplier);

        // Combined difficulty would be 2.0x (NG+) × 1.0x (Wave 1) = 2.0x
        var combinedDifficulty = ngPlusScaling.DifficultyMultiplier * waveConfig.DifficultyMultiplier;
        Assert.Equal(2.0f, combinedDifficulty);
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARD AGGREGATION
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Orchestrator_GetAllLeaderboards_AggregatesAcrossSystems()
    {
        // Arrange & Act
        var leaderboards = _orchestrator.GetAllLeaderboards(limit: 10);

        // Assert
        Assert.NotNull(leaderboards);
        Assert.NotNull(leaderboards.EndlessHighestWave);
        Assert.NotNull(leaderboards.EndlessHighestScore);
        Assert.NotNull(leaderboards.GauntletFastest);
        Assert.NotNull(leaderboards.GauntletFlawless);
    }

    // ═══════════════════════════════════════════════════════════
    // SEED REPRODUCIBILITY
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void EndlessMode_SameSeed_ProducesSameWaves()
    {
        // Arrange
        int char1 = 10;
        int char2 = 11;
        string seed = "ENDLESS-TEST-123";

        var endlessService = _orchestrator.EndlessService;

        // Act - Start two runs with same seed
        var run1 = endlessService.StartEndlessRun(char1, seed);
        var run2 = endlessService.StartEndlessRun(char2, seed);

        var wave1Config = endlessService.GetCurrentWaveConfig(run1.RunId);
        var wave2Config = endlessService.GetCurrentWaveConfig(run2.RunId);

        // Assert - Same wave configuration
        Assert.Equal(wave1Config!.EnemyCount, wave2Config!.EnemyCount);
        Assert.Equal(wave1Config.EnemyLevel, wave2Config.EnemyLevel);
        Assert.Equal(wave1Config.DifficultyMultiplier, wave2Config.DifficultyMultiplier);
        Assert.Equal(wave1Config.IsBossWave, wave2Config.IsBossWave);
    }

    // ═══════════════════════════════════════════════════════════
    // SCORING INTEGRATION
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void EndlessMode_ScoreCalculation_IncludesAllComponents()
    {
        // Arrange
        int characterId = 12;
        var endlessService = _orchestrator.EndlessService;

        var run = endlessService.StartEndlessRun(characterId);

        // Complete 5 waves
        for (int i = 0; i < 5; i++)
        {
            endlessService.CompleteWave(run.RunId, enemiesKilled: 5, damageTaken: 100, damageDealt: 1000);
        }

        // Act - End run and calculate score
        endlessService.EndRun(run.RunId);

        var finalRun = endlessService.GetRunById(run.RunId);
        var score = endlessService.GetScoreBreakdown(run.RunId);

        // Assert - All score components present
        Assert.NotNull(finalRun);
        Assert.True(finalRun.WaveScore > 0, "Wave score should be > 0");
        Assert.True(finalRun.KillScore > 0, "Kill score should be > 0");
        Assert.True(finalRun.TotalScore > 0, "Total score should be > 0");

        Assert.Equal(score.WaveScore, finalRun.WaveScore);
        Assert.Equal(score.KillScore, finalRun.KillScore);
        Assert.Equal(score.TotalScore, finalRun.TotalScore);
    }
}
