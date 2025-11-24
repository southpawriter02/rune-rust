using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using RuneAndRust.Engine.NewGamePlus;
using RuneAndRust.Persistence;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.40.1: Unit tests for NewGamePlusService
/// Tests tier management, initialization, and completion logic
/// </summary>
public class NewGamePlusServiceTests : IDisposable
{
    private readonly NewGamePlusRepository _repository;
    private readonly CarryoverService _carryoverService;
    private readonly NewGamePlusService _service;
    private readonly DifficultyScalingService _scalingService;
    private readonly string _testDbPath;

    public NewGamePlusServiceTests()
    {
        // Create a temporary test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_ng_plus_{Guid.NewGuid()}.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_testDbPath)!);

        // Initialize repositories and services
        _repository = new NewGamePlusRepository(Path.GetDirectoryName(_testDbPath));
        _carryoverService = new CarryoverService();
        _service = new NewGamePlusService(_repository, _carryoverService);
        _scalingService = new DifficultyScalingService(_service);

        // Initialize database schema
        InitializeTestDatabase();
    }

    public void Dispose()
    {
        // Cleanup test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    private void InitializeTestDatabase()
    {
        // Run the schema initialization script
        var schemaPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "Data", "v0.40.1_new_game_plus_schema.sql");

        if (File.Exists(schemaPath))
        {
            var schema = File.ReadAllText(schemaPath);
            // Execute schema against test database
            // Note: This would require actual SQL execution - simplified for now
        }
    }

    // ═════════════════════════════════════════════════════════════
    // TIER AVAILABILITY TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void CanAccessTier_CampaignNotComplete_ReturnsFalse()
    {
        // Arrange
        var characterId = 1;
        // Character has NOT completed campaign (default state)

        // Act
        var canAccess = _service.CanAccessTier(characterId, tier: 1);

        // Assert
        Assert.False(canAccess, "Should not access NG+1 without campaign completion");
    }

    [Fact]
    public void CanAccessTier_CampaignComplete_NG1Available()
    {
        // Arrange
        var characterId = 2;
        _repository.MarkCampaignComplete(characterId);

        // Act
        var canAccess = _service.CanAccessTier(characterId, tier: 1);

        // Assert
        Assert.True(canAccess, "Should access NG+1 after campaign completion");
    }

    [Fact]
    public void CanAccessTier_SkippingTiers_ReturnsFalse()
    {
        // Arrange
        var characterId = 3;
        _repository.MarkCampaignComplete(characterId);
        _repository.SetHighestNGPlusTier(characterId, tier: 1);

        // Act
        var canAccessNG2 = _service.CanAccessTier(characterId, tier: 2);
        var canAccessNG3 = _service.CanAccessTier(characterId, tier: 3);

        // Assert
        Assert.True(canAccessNG2, "Should access NG+2 after completing NG+1");
        Assert.False(canAccessNG3, "Should NOT skip from NG+1 to NG+3");
    }

    [Fact]
    public void CanAccessTier_InvalidTier_ReturnsFalse()
    {
        // Arrange
        var characterId = 4;
        _repository.MarkCampaignComplete(characterId);

        // Act
        var canAccessNG0 = _service.CanAccessTier(characterId, tier: 0);
        var canAccessNG6 = _service.CanAccessTier(characterId, tier: 6);

        // Assert
        Assert.False(canAccessNG0, "Tier 0 is invalid");
        Assert.False(canAccessNG6, "Tier 6 exceeds maximum (5)");
    }

    [Fact]
    public void GetAvailableTiers_CampaignNotComplete_EmptyList()
    {
        // Arrange
        var characterId = 5;

        // Act
        var info = _service.GetAvailableTiers(characterId);

        // Assert
        Assert.False(info.HasCompletedCampaign);
        Assert.Empty(info.AvailableTiers);
        Assert.Equal("Complete the campaign to unlock New Game+", info.NextTierRequirement);
    }

    [Fact]
    public void GetAvailableTiers_CompletedNG1_TiersUpToNG2Available()
    {
        // Arrange
        var characterId = 6;
        _repository.MarkCampaignComplete(characterId);
        _repository.SetHighestNGPlusTier(characterId, tier: 1);

        // Act
        var info = _service.GetAvailableTiers(characterId);

        // Assert
        Assert.True(info.HasCompletedCampaign);
        Assert.Equal(1, info.HighestTierCompleted);
        Assert.Equal(new[] { 1, 2 }, info.AvailableTiers);
        Assert.Contains("NG+1", info.NextTierRequirement);
    }

    // ═════════════════════════════════════════════════════════════
    // INITIALIZATION TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void InitializeNewGamePlus_CampaignNotComplete_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(characterId: 10);
        // Campaign not marked complete

        // Act
        var result = _service.InitializeNewGamePlus(character, targetTier: 1);

        // Assert
        Assert.False(result, "Should fail without campaign completion");
    }

    [Fact]
    public void InitializeNewGamePlus_InvalidTier_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(characterId: 11);
        _repository.MarkCampaignComplete(character.CharacterID);

        // Act
        var resultTier0 = _service.InitializeNewGamePlus(character, targetTier: 0);
        var resultTier6 = _service.InitializeNewGamePlus(character, targetTier: 6);

        // Assert
        Assert.False(resultTier0, "Tier 0 is invalid");
        Assert.False(resultTier6, "Tier 6 exceeds maximum");
    }

    [Fact]
    public void InitializeNewGamePlus_SkippingTier_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(characterId: 12);
        _repository.MarkCampaignComplete(character.CharacterID);
        _repository.SetHighestNGPlusTier(character.CharacterID, tier: 1);

        // Act: Try to jump from NG+1 to NG+3
        var result = _service.InitializeNewGamePlus(character, targetTier: 3);

        // Assert
        Assert.False(result, "Should not allow skipping from NG+1 to NG+3");
    }

    [Fact]
    public void InitializeNewGamePlus_ValidTier_Success()
    {
        // Arrange
        var character = CreateTestCharacter(characterId: 13);
        character.PsychicStress = 50;
        character.Corruption = 30;
        character.Traumas.Add(new Trauma { TraumaId = "test_trauma" });

        _repository.MarkCampaignComplete(character.CharacterID);

        // Act
        var result = _service.InitializeNewGamePlus(character, targetTier: 1);

        // Assert
        Assert.True(result, "Should successfully initialize NG+1");
        Assert.Equal(0, character.PsychicStress); // Reset
        Assert.Equal(0, character.Corruption); // Reset
        Assert.Empty(character.Traumas); // Reset
        Assert.Equal(1, _repository.GetCurrentNGPlusTier(character.CharacterID));
    }

    // ═════════════════════════════════════════════════════════════
    // COMPLETION TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void CompleteNewGamePlusTier_FirstCompletion_UnlocksNextTier()
    {
        // Arrange
        var characterId = 20;
        _repository.MarkCampaignComplete(characterId);
        _repository.SetCurrentNGPlusTier(characterId, tier: 1);
        _repository.SetHighestNGPlusTier(characterId, tier: 0);

        // Act
        var result = _service.CompleteNewGamePlusTier(characterId, playtimeSeconds: 7200);

        // Assert
        Assert.True(result);
        Assert.Equal(1, _repository.GetHighestNGPlusTier(characterId));
        Assert.Equal(1, _repository.GetNGPlusCompletionCount(characterId));
    }

    [Fact]
    public void CompleteNewGamePlusTier_RepeatedCompletion_DoesNotIncreaseHighest()
    {
        // Arrange
        var characterId = 21;
        _repository.MarkCampaignComplete(characterId);
        _repository.SetCurrentNGPlusTier(characterId, tier: 1);
        _repository.SetHighestNGPlusTier(characterId, tier: 2); // Already completed NG+2

        // Act
        var result = _service.CompleteNewGamePlusTier(characterId);

        // Assert
        Assert.True(result);
        Assert.Equal(2, _repository.GetHighestNGPlusTier(characterId)); // Unchanged
        Assert.Equal(1, _repository.GetNGPlusCompletionCount(characterId)); // Still increments
    }

    [Fact]
    public void GetCompletions_MultipleCompletions_ReturnsAll()
    {
        // Arrange
        var characterId = 22;
        _repository.MarkCampaignComplete(characterId);

        // Complete NG+1 three times
        _repository.SetCurrentNGPlusTier(characterId, tier: 1);
        _service.CompleteNewGamePlusTier(characterId, playtimeSeconds: 7200);
        _service.CompleteNewGamePlusTier(characterId, playtimeSeconds: 6800);
        _service.CompleteNewGamePlusTier(characterId, playtimeSeconds: 7400);

        // Act
        var completions = _service.GetCompletions(characterId);

        // Assert
        Assert.Equal(3, completions.Count);
        Assert.All(completions, c => Assert.Equal(1, c.CompletedTier));
    }

    // ═════════════════════════════════════════════════════════════
    // SCALING PARAMETER TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void GetScalingForTier_Tier0_ReturnsNoScaling()
    {
        // Act
        var scaling = _service.GetScalingForTier(tier: 0);

        // Assert
        Assert.NotNull(scaling);
        Assert.Equal(1.0f, scaling.DifficultyMultiplier);
        Assert.Equal(0, scaling.EnemyLevelIncrease);
        Assert.Equal(0.0f, scaling.BossPhaseThresholdReduction);
        Assert.Equal(1.0f, scaling.CorruptionRateMultiplier);
        Assert.Equal(1.0f, scaling.LegendRewardMultiplier);
    }

    [Fact]
    public void GetScalingForTier_Tier1_Correct150PercentScaling()
    {
        // Act
        var scaling = _service.GetScalingForTier(tier: 1);

        // Assert
        Assert.NotNull(scaling);
        Assert.Equal(1.5f, scaling.DifficultyMultiplier);
        Assert.Equal(2, scaling.EnemyLevelIncrease);
        Assert.Equal(0.10f, scaling.BossPhaseThresholdReduction);
        Assert.Equal(1.25f, scaling.CorruptionRateMultiplier);
        Assert.Equal(1.15f, scaling.LegendRewardMultiplier);
    }

    [Fact]
    public void GetScalingForTier_Tier5_CorrectMaximumScaling()
    {
        // Act
        var scaling = _service.GetScalingForTier(tier: 5);

        // Assert
        Assert.NotNull(scaling);
        Assert.Equal(3.5f, scaling.DifficultyMultiplier);
        Assert.Equal(10, scaling.EnemyLevelIncrease);
        Assert.Equal(0.50f, scaling.BossPhaseThresholdReduction);
        Assert.Equal(2.25f, scaling.CorruptionRateMultiplier);
        Assert.Equal(1.75f, scaling.LegendRewardMultiplier);
    }

    [Fact]
    public void GetAllScalingTiers_Returns5Tiers()
    {
        // Act
        var tiers = _service.GetAllScalingTiers();

        // Assert
        Assert.Equal(5, tiers.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, tiers.Select(t => t.Tier));
    }

    // ═════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═════════════════════════════════════════════════════════════

    private PlayerCharacter CreateTestCharacter(int characterId, int level = 25)
    {
        return new PlayerCharacter
        {
            CharacterID = characterId,
            Name = $"Test Character {characterId}",
            Class = CharacterClass.Warrior,
            CurrentMilestone = level,
            CurrentLegend = 5000,
            ProgressionPoints = 25,
            Attributes = new Attributes
            {
                Might = 16,
                Finesse = 14,
                Wits = 12,
                Will = 15,
                Sturdiness = 13
            },
            HP = 80,
            MaxHP = 100,
            Currency = 500,
            PsychicStress = 0,
            Corruption = 0,
            Traumas = new List<Trauma>(),
            Inventory = new List<Equipment>(),
            Abilities = new List<Ability>()
        };
    }
}
