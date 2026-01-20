using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

/// <summary>
/// Unit tests for ProgressionService configuration features (v0.0.8c).
/// </summary>
[TestFixture]
public class ProgressionServiceConfigTests
{
    private Mock<ILogger<ProgressionService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ProgressionService>>();
    }

    private Player CreateTestPlayer(int level = 1, int experience = 0)
    {
        var player = new Player("TestHero", new Stats(100, 10, 5));
        player.SetLevel(level);
        if (experience > 0)
        {
            player.AddExperience(experience);
        }
        return player;
    }

    // ===== Constructor and Configuration Tests =====

    [Test]
    public void Constructor_WithNullProgression_UsesDefault()
    {
        var service = new ProgressionService(_loggerMock.Object, null);

        service.Progression.Should().NotBeNull();
        service.Progression.MaxLevel.Should().Be(20);
        service.Progression.CurveType.Should().Be(ProgressionCurve.Linear);
    }

    [Test]
    public void Constructor_WithCustomProgression_UsesProvidedConfig()
    {
        var customProgression = new ProgressionDefinition
        {
            ExperienceTerminology = "Glory",
            LevelTerminology = "Rank",
            MaxLevel = 50,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 200
        };

        var service = new ProgressionService(_loggerMock.Object, customProgression);

        service.Progression.ExperienceTerminology.Should().Be("Glory");
        service.Progression.LevelTerminology.Should().Be("Rank");
        service.Progression.MaxLevel.Should().Be(50);
    }

    [Test]
    public void Progression_Property_ReturnsConfiguration()
    {
        var progression = new ProgressionDefinition { MaxLevel = 30 };
        var service = new ProgressionService(_loggerMock.Object, progression);

        service.Progression.MaxLevel.Should().Be(30);
    }

    // ===== Max Level Cap Tests =====

    [Test]
    public void CheckForLevelUp_AtMaxLevel_ReturnsNone()
    {
        var progression = new ProgressionDefinition
        {
            MaxLevel = 5,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 5, experience: 10000);

        var result = service.CheckForLevelUp(player);

        result.DidLevelUp.Should().BeFalse();
        result.NewLevel.Should().Be(5);
    }

    [Test]
    public void CheckForLevelUp_WouldExceedMaxLevel_CapsAtMax()
    {
        var progression = new ProgressionDefinition
        {
            MaxLevel = 5,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 3, experience: 10000);

        var result = service.CheckForLevelUp(player);

        result.DidLevelUp.Should().BeTrue();
        result.NewLevel.Should().Be(5); // Capped at max
        result.LevelsGained.Should().Be(2);
    }

    [Test]
    public void CheckForLevelUp_ZeroMaxLevel_NoCapApplied()
    {
        var progression = new ProgressionDefinition
        {
            MaxLevel = 0, // No cap
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 1, experience: 10000);

        var result = service.CheckForLevelUp(player);

        result.DidLevelUp.Should().BeTrue();
        result.NewLevel.Should().BeGreaterThan(50); // Should be very high
    }

    // ===== HealOnLevelUp Configuration Tests =====

    [Test]
    public void ApplyLevelUp_HealOnLevelUpEnabled_HealsToMax()
    {
        var progression = new ProgressionDefinition
        {
            HealOnLevelUp = true,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 1, experience: 200); // Level 2 requires 200 XP
        player.TakeDamage(50);
        var levelUpResult = service.CheckForLevelUp(player);

        service.ApplyLevelUp(player, levelUpResult);

        player.Health.Should().Be(player.Stats.MaxHealth);
    }

    [Test]
    public void ApplyLevelUp_HealOnLevelUpDisabled_DoesNotHeal()
    {
        var progression = new ProgressionDefinition
        {
            HealOnLevelUp = false,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 1, experience: 200); // Level 2 requires 200 XP
        player.TakeDamage(50);
        var healthBefore = player.Health;
        var levelUpResult = service.CheckForLevelUp(player);

        service.ApplyLevelUp(player, levelUpResult);

        // Health should be the same (not healed), just maxHealth increased
        player.Health.Should().Be(healthBefore);
        player.Stats.MaxHealth.Should().BeGreaterThan(100);
    }

    // ===== Custom Stat Bonuses Tests =====

    [Test]
    public void GetStatIncreasesForLevels_UsesCustomDefaultBonuses()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig
            {
                MaxHealth = 20,
                Attack = 5,
                Defense = 3
            }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        var modifiers = service.GetStatIncreasesForLevels(1, 2);

        modifiers.MaxHealth.Should().Be(20);
        modifiers.Attack.Should().Be(5);
        modifiers.Defense.Should().Be(3);
    }

    [Test]
    public void GetStatIncreasesForLevels_UsesLevelOverrides()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 },
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [5] = new LevelDefinition
                {
                    Level = 5,
                    StatBonuses = new StatBonusConfig { MaxHealth = 50, Attack = 10, Defense = 10 }
                }
            }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        // Level 4 -> 5 should use the override
        var modifiers = service.GetStatIncreasesForLevels(4, 5);

        modifiers.MaxHealth.Should().Be(50);
        modifiers.Attack.Should().Be(10);
        modifiers.Defense.Should().Be(10);
    }

    [Test]
    public void GetStatIncreasesForLevels_MultiLevel_AccumulatesCorrectly()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 },
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [3] = new LevelDefinition
                {
                    Level = 3,
                    StatBonuses = new StatBonusConfig { MaxHealth = 15, Attack = 3, Defense = 2 }
                }
            }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        // Level 1 -> 4: Level 2 default (+5), Level 3 override (+15), Level 4 default (+5)
        var modifiers = service.GetStatIncreasesForLevels(1, 4);

        modifiers.MaxHealth.Should().Be(25); // 5 + 15 + 5
        modifiers.Attack.Should().Be(5);      // 1 + 3 + 1
        modifiers.Defense.Should().Be(4);     // 1 + 2 + 1
    }

    [Test]
    public void GetStatIncreasesForLevels_WithClassGrowthRates_UsesClassRates()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var classGrowth = new LevelStatModifiers(15, 3, 2);

        var modifiers = service.GetStatIncreasesForLevels(1, 2, classGrowth);

        modifiers.MaxHealth.Should().Be(15);
        modifiers.Attack.Should().Be(3);
        modifiers.Defense.Should().Be(2);
    }

    // ===== Custom Rewards and Titles Tests =====

    [Test]
    public void GetCustomRewardsForLevel_NoOverride_ReturnsEmpty()
    {
        var progression = ProgressionDefinition.Default;
        var service = new ProgressionService(_loggerMock.Object, progression);

        var rewards = service.GetCustomRewardsForLevel(5);

        rewards.Should().BeEmpty();
    }

    [Test]
    public void GetCustomRewardsForLevel_WithOverride_ReturnsRewards()
    {
        var progression = new ProgressionDefinition
        {
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [5] = new LevelDefinition
                {
                    Level = 5,
                    CustomRewards = new[] { "Unlock: Advanced Training", "Bonus Gold" }
                }
            }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        var rewards = service.GetCustomRewardsForLevel(5);

        rewards.Should().HaveCount(2);
        rewards.Should().Contain("Unlock: Advanced Training");
        rewards.Should().Contain("Bonus Gold");
    }

    [Test]
    public void GetTitleForLevel_NoOverride_ReturnsNull()
    {
        var progression = ProgressionDefinition.Default;
        var service = new ProgressionService(_loggerMock.Object, progression);

        var title = service.GetTitleForLevel(5);

        title.Should().BeNull();
    }

    [Test]
    public void GetTitleForLevel_WithOverride_ReturnsTitle()
    {
        var progression = new ProgressionDefinition
        {
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [10] = new LevelDefinition { Level = 10, Title = "Elite" }
            }
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        var title = service.GetTitleForLevel(10);

        title.Should().Be("Elite");
    }

    // ===== XP Curve Tests =====

    [Test]
    public void GetExperienceForNextLevel_LinearCurve_CalculatesCorrectly()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            MaxLevel = 20
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        service.GetExperienceForNextLevel(1).Should().Be(200);  // Level 2 = 2*100
        service.GetExperienceForNextLevel(5).Should().Be(600);  // Level 6 = 6*100
    }

    [Test]
    public void GetExperienceForNextLevel_AtMaxLevel_ReturnsZero()
    {
        var progression = new ProgressionDefinition
        {
            MaxLevel = 5,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);

        var xp = service.GetExperienceForNextLevel(5);

        xp.Should().Be(0);
    }

    [Test]
    public void GetExperienceUntilNextLevel_AtMaxLevel_ReturnsZero()
    {
        var progression = new ProgressionDefinition
        {
            MaxLevel = 5,
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 5, experience: 1000);

        var remaining = service.GetExperienceUntilNextLevel(player);

        remaining.Should().Be(0);
    }

    [Test]
    public void GetExperienceUntilNextLevel_UsesConfiguredCurve()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100, // Custom base
            MaxLevel = 20
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 1, experience: 100);

        var remaining = service.GetExperienceUntilNextLevel(player);

        // Level 2 = 2*100 = 200, have 100, need 100 more
        remaining.Should().Be(100);
    }

    // ===== Integration Tests =====

    [Test]
    public void CheckAndApplyLevelUp_WithFullConfiguration_AppliesCorrectly()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            MaxLevel = 10,
            HealOnLevelUp = true,
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 10, Attack = 2, Defense = 2 },
            ExperienceTerminology = "Glory",
            LevelTerminology = "Rank"
        };
        var service = new ProgressionService(_loggerMock.Object, progression);
        var player = CreateTestPlayer(level: 1, experience: 200); // Need >= 200 for level 2
        player.TakeDamage(30);

        var result = service.CheckAndApplyLevelUp(player);

        result.DidLevelUp.Should().BeTrue();
        result.NewLevel.Should().Be(2);
        player.Level.Should().Be(2);
        player.Stats.MaxHealth.Should().Be(110); // 100 + 10
        player.Stats.Attack.Should().Be(12);      // 10 + 2
        player.Stats.Defense.Should().Be(7);      // 5 + 2
        player.Health.Should().Be(player.Stats.MaxHealth); // Healed
    }
}
