using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

/// <summary>
/// Unit tests for ProgressionService (v0.0.8b).
/// </summary>
[TestFixture]
public class ProgressionServiceTests
{
    private ProgressionService _progressionService = null!;
    private Mock<ILogger<ProgressionService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ProgressionService>>();
        _progressionService = new ProgressionService(_loggerMock.Object);
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

    [Test]
    public void CheckForLevelUp_BelowThreshold_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 100);

        // Act
        var result = _progressionService.CheckForLevelUp(player);

        // Assert
        result.DidLevelUp.Should().BeFalse();
        result.OldLevel.Should().Be(1);
        result.NewLevel.Should().Be(1);
    }

    [Test]
    public void CheckForLevelUp_AtThreshold_ReturnsLevelUp()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);

        // Act
        var result = _progressionService.CheckForLevelUp(player);

        // Assert
        result.DidLevelUp.Should().BeTrue();
        result.OldLevel.Should().Be(1);
        result.NewLevel.Should().Be(2);
        result.LevelsGained.Should().Be(1);
    }

    [Test]
    public void CheckForLevelUp_MultiLevel_ReturnsCorrectLevels()
    {
        // Arrange - 500 XP should be level 5 (500 >= 500)
        var player = CreateTestPlayer(level: 1, experience: 500);

        // Act
        var result = _progressionService.CheckForLevelUp(player);

        // Assert
        result.DidLevelUp.Should().BeTrue();
        result.OldLevel.Should().Be(1);
        result.NewLevel.Should().Be(5);
        result.LevelsGained.Should().Be(4);
        result.IsMultiLevel.Should().BeTrue();
    }

    [Test]
    public void ApplyLevelUp_UpdatesPlayerLevel()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        // Act
        _progressionService.ApplyLevelUp(player, levelUpResult);

        // Assert
        player.Level.Should().Be(2);
    }

    [Test]
    public void ApplyLevelUp_AppliesStatIncreases()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);
        var oldMaxHealth = player.Stats.MaxHealth;
        var oldAttack = player.Stats.Attack;
        var oldDefense = player.Stats.Defense;
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        // Act
        _progressionService.ApplyLevelUp(player, levelUpResult);

        // Assert
        player.Stats.MaxHealth.Should().Be(oldMaxHealth + 5);
        player.Stats.Attack.Should().Be(oldAttack + 1);
        player.Stats.Defense.Should().Be(oldDefense + 1);
    }

    [Test]
    public void ApplyLevelUp_HealsToNewMax()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);
        player.TakeDamage(50); // Reduce health
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        // Act
        _progressionService.ApplyLevelUp(player, levelUpResult);

        // Assert
        player.Health.Should().Be(player.Stats.MaxHealth); // Should be fully healed
    }

    [Test]
    public void ApplyLevelUp_MultiLevel_AppliesCorrectStatIncreases()
    {
        // Arrange - 500 XP gets us to level 5 (4 levels gained)
        var player = CreateTestPlayer(level: 1, experience: 500);
        var oldMaxHealth = player.Stats.MaxHealth;
        var oldAttack = player.Stats.Attack;
        var oldDefense = player.Stats.Defense;
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        // Act
        _progressionService.ApplyLevelUp(player, levelUpResult);

        // Assert
        player.Stats.MaxHealth.Should().Be(oldMaxHealth + 20); // 5 * 4
        player.Stats.Attack.Should().Be(oldAttack + 4);         // 1 * 4
        player.Stats.Defense.Should().Be(oldDefense + 4);       // 1 * 4
    }

    [Test]
    public void ApplyLevelUp_NoLevelUp_DoesNotChangePlayer()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 100);
        var levelBefore = player.Level;
        var statsBefore = player.Stats;
        var noneResult = LevelUpResult.None(1);

        // Act
        var result = _progressionService.ApplyLevelUp(player, noneResult);

        // Assert
        result.DidLevelUp.Should().BeFalse();
        player.Level.Should().Be(levelBefore);
        player.Stats.Should().Be(statsBefore);
    }

    [Test]
    public void ApplyLevelUp_WithAbilityCallback_CollectsUnlockedAbilities()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        IReadOnlyList<string> GetAbilitiesAtLevel(int level)
        {
            return level == 2 ? new[] { "Power Strike" } : Array.Empty<string>();
        }

        // Act
        var result = _progressionService.ApplyLevelUp(player, levelUpResult, GetAbilitiesAtLevel);

        // Assert
        result.HasNewAbilities.Should().BeTrue();
        result.NewAbilities.Should().Contain("Power Strike");
    }

    [Test]
    public void ApplyLevelUp_MultiLevel_CollectsAllUnlockedAbilities()
    {
        // Arrange - 300 XP gets us to level 3 (2 levels gained)
        var player = CreateTestPlayer(level: 1, experience: 300);
        var levelUpResult = _progressionService.CheckForLevelUp(player);

        IReadOnlyList<string> GetAbilitiesAtLevel(int level)
        {
            return level switch
            {
                2 => new[] { "Power Strike" },
                3 => new[] { "Shield Bash" },
                _ => Array.Empty<string>()
            };
        }

        // Act
        var result = _progressionService.ApplyLevelUp(player, levelUpResult, GetAbilitiesAtLevel);

        // Assert
        result.HasNewAbilities.Should().BeTrue();
        result.NewAbilities.Should().HaveCount(2);
        result.NewAbilities.Should().Contain("Power Strike");
        result.NewAbilities.Should().Contain("Shield Bash");
    }

    [Test]
    public void CheckAndApplyLevelUp_CombinedOperation()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);
        var oldLevel = player.Level;

        // Act
        var result = _progressionService.CheckAndApplyLevelUp(player);

        // Assert
        result.DidLevelUp.Should().BeTrue();
        result.OldLevel.Should().Be(oldLevel);
        result.NewLevel.Should().Be(2);
        player.Level.Should().Be(2);
    }

    [Test]
    public void CheckAndApplyLevelUp_NoLevelUp_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 100);

        // Act
        var result = _progressionService.CheckAndApplyLevelUp(player);

        // Assert
        result.DidLevelUp.Should().BeFalse();
        player.Level.Should().Be(1);
    }

    [Test]
    public void GetStatIncreasesForLevels_ReturnsCorrectModifiers()
    {
        // Act
        var modifiers = ProgressionService.GetStatIncreasesForLevels(3);

        // Assert
        modifiers.MaxHealth.Should().Be(15); // 5 * 3
        modifiers.Attack.Should().Be(3);      // 1 * 3
        modifiers.Defense.Should().Be(3);     // 1 * 3
    }

    [Test]
    public void GetExperienceForNextLevel_ReturnsCorrectThreshold()
    {
        // Act & Assert
        _progressionService.GetExperienceForNextLevel(1).Should().Be(200);
        _progressionService.GetExperienceForNextLevel(2).Should().Be(300);
        _progressionService.GetExperienceForNextLevel(5).Should().Be(600);
    }

    [Test]
    public void GetExperienceUntilNextLevel_ReturnsRemainingXp()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 150);

        // Act
        var remaining = _progressionService.GetExperienceUntilNextLevel(player);

        // Assert
        remaining.Should().Be(50); // Need 200, have 150
    }

    [Test]
    public void GetExperienceUntilNextLevel_AtThreshold_ReturnsZero()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, experience: 200);

        // Act
        var remaining = _progressionService.GetExperienceUntilNextLevel(player);

        // Assert
        remaining.Should().Be(0); // At or above threshold
    }

    [Test]
    public void CheckForLevelUp_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _progressionService.CheckForLevelUp(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ApplyLevelUp_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var result = LevelUpResult.SingleLevel(1);

        // Act
        var act = () => _progressionService.ApplyLevelUp(null!, result);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
