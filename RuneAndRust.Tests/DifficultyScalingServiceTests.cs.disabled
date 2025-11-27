using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using RuneAndRust.Engine.NewGamePlus;
using RuneAndRust.Persistence;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.40.1: Unit tests for DifficultyScalingService
/// Tests enemy/boss scaling, corruption multipliers, and legend rewards
/// </summary>
public class DifficultyScalingServiceTests : IDisposable
{
    private readonly NewGamePlusRepository _repository;
    private readonly CarryoverService _carryoverService;
    private readonly NewGamePlusService _ngPlusService;
    private readonly DifficultyScalingService _scalingService;
    private readonly string _testDbPath;

    public DifficultyScalingServiceTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_scaling_{Guid.NewGuid()}.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_testDbPath)!);

        _repository = new NewGamePlusRepository(Path.GetDirectoryName(_testDbPath));
        _carryoverService = new CarryoverService();
        _ngPlusService = new NewGamePlusService(_repository, _carryoverService);
        _scalingService = new DifficultyScalingService(_ngPlusService);
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    // ═════════════════════════════════════════════════════════════
    // ENEMY SCALING TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void ApplyNGPlusScaling_Tier0_NoScaling()
    {
        // Arrange
        var baseEnemy = CreateTestEnemy(hp: 100, level: 10);

        // Act
        var scaledEnemy = _scalingService.ApplyNGPlusScaling(baseEnemy, ngPlusTier: 0);

        // Assert
        Assert.Equal(100, scaledEnemy.MaxHP);
        Assert.Equal(10, scaledEnemy.Level);
    }

    [Fact]
    public void ApplyNGPlusScaling_Tier1_150PercentScaling()
    {
        // Arrange
        var baseEnemy = CreateTestEnemy(hp: 100, level: 10);

        // Act
        var scaledEnemy = _scalingService.ApplyNGPlusScaling(baseEnemy, ngPlusTier: 1);

        // Assert
        Assert.Equal(150, scaledEnemy.MaxHP); // 100 × 1.5
        Assert.Equal(12, scaledEnemy.Level); // 10 + 2
        Assert.Equal(150, scaledEnemy.HP); // HP matches MaxHP
    }

    [Fact]
    public void ApplyNGPlusScaling_Tier2_200PercentScaling()
    {
        // Arrange
        var baseEnemy = CreateTestEnemy(hp: 100, level: 10);

        // Act
        var scaledEnemy = _scalingService.ApplyNGPlusScaling(baseEnemy, ngPlusTier: 2);

        // Assert
        Assert.Equal(200, scaledEnemy.MaxHP); // 100 × 2.0
        Assert.Equal(14, scaledEnemy.Level); // 10 + 4
    }

    [Fact]
    public void ApplyNGPlusScaling_Tier5_350PercentScaling()
    {
        // Arrange
        var baseEnemy = CreateTestEnemy(hp: 100, level: 10);

        // Act
        var scaledEnemy = _scalingService.ApplyNGPlusScaling(baseEnemy, ngPlusTier: 5);

        // Assert
        Assert.Equal(350, scaledEnemy.MaxHP); // 100 × 3.5
        Assert.Equal(20, scaledEnemy.Level); // 10 + 10
    }

    [Fact]
    public void ApplyNGPlusScaling_DoesNotModifyOriginal()
    {
        // Arrange
        var baseEnemy = CreateTestEnemy(hp: 100, level: 10);
        var originalHP = baseEnemy.MaxHP;
        var originalLevel = baseEnemy.Level;

        // Act
        var scaledEnemy = _scalingService.ApplyNGPlusScaling(baseEnemy, ngPlusTier: 3);

        // Assert
        Assert.Equal(originalHP, baseEnemy.MaxHP); // Original unchanged
        Assert.Equal(originalLevel, baseEnemy.Level); // Original unchanged
        Assert.NotEqual(baseEnemy.MaxHP, scaledEnemy.MaxHP); // Scaled is different
    }

    [Fact]
    public void ApplyNGPlusScalingBulk_ScalesAllEnemies()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(hp: 100, level: 10, id: "enemy_1"),
            CreateTestEnemy(hp: 150, level: 12, id: "enemy_2"),
            CreateTestEnemy(hp: 200, level: 15, id: "enemy_3")
        };

        // Act
        var scaledEnemies = _scalingService.ApplyNGPlusScalingBulk(enemies, ngPlusTier: 2);

        // Assert
        Assert.Equal(3, scaledEnemies.Count);
        Assert.Equal(200, scaledEnemies[0].MaxHP); // 100 × 2.0
        Assert.Equal(300, scaledEnemies[1].MaxHP); // 150 × 2.0
        Assert.Equal(400, scaledEnemies[2].MaxHP); // 200 × 2.0
    }

    // ═════════════════════════════════════════════════════════════
    // CORRUPTION SCALING TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void GetCorruptionRateMultiplier_Tier0_Returns100Percent()
    {
        // Act
        var multiplier = _scalingService.GetCorruptionRateMultiplier(ngPlusTier: 0);

        // Assert
        Assert.Equal(1.0f, multiplier);
    }

    [Fact]
    public void GetCorruptionRateMultiplier_Tier1_Returns125Percent()
    {
        // Act
        var multiplier = _scalingService.GetCorruptionRateMultiplier(ngPlusTier: 1);

        // Assert
        Assert.Equal(1.25f, multiplier);
    }

    [Fact]
    public void GetCorruptionRateMultiplier_Tier5_Returns225Percent()
    {
        // Act
        var multiplier = _scalingService.GetCorruptionRateMultiplier(ngPlusTier: 5);

        // Assert
        Assert.Equal(2.25f, multiplier);
    }

    [Fact]
    public void ApplyCorruptionScaling_Tier0_NoScaling()
    {
        // Act
        var scaled = _scalingService.ApplyCorruptionScaling(baseCorruption: 10, ngPlusTier: 0);

        // Assert
        Assert.Equal(10, scaled);
    }

    [Fact]
    public void ApplyCorruptionScaling_Tier1_125PercentScaling()
    {
        // Act
        var scaled = _scalingService.ApplyCorruptionScaling(baseCorruption: 10, ngPlusTier: 1);

        // Assert
        Assert.Equal(12, scaled); // 10 × 1.25 = 12.5 → 12 (int)
    }

    [Fact]
    public void ApplyCorruptionScaling_Tier5_225PercentScaling()
    {
        // Act
        var scaled = _scalingService.ApplyCorruptionScaling(baseCorruption: 10, ngPlusTier: 5);

        // Assert
        Assert.Equal(22, scaled); // 10 × 2.25 = 22.5 → 22 (int)
    }

    // ═════════════════════════════════════════════════════════════
    // LEGEND REWARD SCALING TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void GetLegendRewardMultiplier_Tier0_Returns100Percent()
    {
        // Act
        var multiplier = _scalingService.GetLegendRewardMultiplier(ngPlusTier: 0);

        // Assert
        Assert.Equal(1.0f, multiplier);
    }

    [Fact]
    public void GetLegendRewardMultiplier_Tier1_Returns115Percent()
    {
        // Act
        var multiplier = _scalingService.GetLegendRewardMultiplier(ngPlusTier: 1);

        // Assert
        Assert.Equal(1.15f, multiplier);
    }

    [Fact]
    public void GetLegendRewardMultiplier_Tier5_Returns175Percent()
    {
        // Act
        var multiplier = _scalingService.GetLegendRewardMultiplier(ngPlusTier: 5);

        // Assert
        Assert.Equal(1.75f, multiplier);
    }

    [Fact]
    public void ApplyLegendScaling_Tier0_NoScaling()
    {
        // Act
        var scaled = _scalingService.ApplyLegendScaling(baseLegend: 100, ngPlusTier: 0);

        // Assert
        Assert.Equal(100, scaled);
    }

    [Fact]
    public void ApplyLegendScaling_Tier1_115PercentScaling()
    {
        // Act
        var scaled = _scalingService.ApplyLegendScaling(baseLegend: 100, ngPlusTier: 1);

        // Assert
        Assert.Equal(115, scaled); // 100 × 1.15
    }

    [Fact]
    public void ApplyLegendScaling_Tier5_175PercentScaling()
    {
        // Act
        var scaled = _scalingService.ApplyLegendScaling(baseLegend: 100, ngPlusTier: 5);

        // Assert
        Assert.Equal(175, scaled); // 100 × 1.75
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS PHASE THRESHOLD SCALING TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void GetBossPhaseThresholdReduction_Tier0_NoReduction()
    {
        // Act
        var reduction = _scalingService.GetBossPhaseThresholdReduction(ngPlusTier: 0);

        // Assert
        Assert.Equal(0.0f, reduction);
    }

    [Fact]
    public void GetBossPhaseThresholdReduction_Tier1_10PercentReduction()
    {
        // Act
        var reduction = _scalingService.GetBossPhaseThresholdReduction(ngPlusTier: 1);

        // Assert
        Assert.Equal(0.10f, reduction);
    }

    [Fact]
    public void GetBossPhaseThresholdReduction_Tier5_50PercentReduction()
    {
        // Act
        var reduction = _scalingService.GetBossPhaseThresholdReduction(ngPlusTier: 5);

        // Assert
        Assert.Equal(0.50f, reduction);
    }

    [Fact]
    public void ApplyBossPhaseThresholdScaling_Tier0_NoChange()
    {
        // Act
        var scaled = _scalingService.ApplyBossPhaseThresholdScaling(baseThreshold: 0.75f, ngPlusTier: 0);

        // Assert
        Assert.Equal(0.75f, scaled);
    }

    [Fact]
    public void ApplyBossPhaseThresholdScaling_Tier3_30PercentReduction()
    {
        // Act: Phase 2 normally triggers at 75% HP
        var scaled = _scalingService.ApplyBossPhaseThresholdScaling(baseThreshold: 0.75f, ngPlusTier: 3);

        // Assert
        Assert.Equal(0.45f, scaled); // 0.75 - 0.30 = 0.45 (triggers at 45% HP)
    }

    [Fact]
    public void ApplyBossPhaseThresholdScaling_MinimumThreshold_CappedAt10Percent()
    {
        // Act: Phase 3 normally triggers at 33% HP
        var scaled = _scalingService.ApplyBossPhaseThresholdScaling(baseThreshold: 0.33f, ngPlusTier: 5);

        // Assert
        Assert.Equal(0.10f, scaled); // 0.33 - 0.50 = -0.17, capped at 0.10
    }

    // ═════════════════════════════════════════════════════════════
    // UTILITY TESTS
    // ═════════════════════════════════════════════════════════════

    [Fact]
    public void GetScalingSummary_Tier0_DescribesNormalDifficulty()
    {
        // Act
        var summary = _scalingService.GetScalingSummary(ngPlusTier: 0);

        // Assert
        Assert.Contains("Normal", summary);
        Assert.Contains("no scaling", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetScalingSummary_Tier1_DescribesScaling()
    {
        // Act
        var summary = _scalingService.GetScalingSummary(ngPlusTier: 1);

        // Assert
        Assert.Contains("NG+1", summary);
        Assert.Contains("1.5x", summary);
        Assert.Contains("+2", summary);
    }

    [Fact]
    public void GetScalingSummary_Tier5_DescribesMaximumScaling()
    {
        // Act
        var summary = _scalingService.GetScalingSummary(ngPlusTier: 5);

        // Assert
        Assert.Contains("NG+5", summary);
        Assert.Contains("3.5x", summary);
        Assert.Contains("+10", summary);
    }

    // ═════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═════════════════════════════════════════════════════════════

    private Enemy CreateTestEnemy(int hp, int level, string id = "test_enemy")
    {
        return new Enemy
        {
            Id = id,
            Name = $"Test Enemy ({id})",
            Type = EnemyType.Forlorn,
            Faction = Faction.Forlorn,
            Level = level,
            HP = hp,
            MaxHP = hp,
            Stamina = 50,
            MaxStamina = 50,
            Attributes = new Attributes
            {
                Might = 12,
                Finesse = 10,
                Wits = 8,
                Will = 8,
                Sturdiness = 10
            },
            Armor = 2,
            Soak = 1,
            Speed = 3,
            Evasion = 8,
            IsBoss = false,
            Abilities = new List<Ability>(),
            LootTableId = "basic_loot"
        };
    }
}
