using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Player level-up functionality (v0.0.8b).
/// </summary>
[TestFixture]
public class PlayerLevelUpTests
{
    private Player CreateTestPlayer(Stats? stats = null)
    {
        return new Player("TestHero", stats ?? new Stats(100, 10, 5));
    }

    [Test]
    public void ApplyLevelStatModifiers_IncreasesStats()
    {
        // Arrange
        var player = CreateTestPlayer();
        var modifiers = new LevelStatModifiers(10, 2, 3);

        // Act
        player.ApplyLevelStatModifiers(modifiers);

        // Assert
        player.Stats.MaxHealth.Should().Be(110);
        player.Stats.Attack.Should().Be(12);
        player.Stats.Defense.Should().Be(8);
    }

    [Test]
    public void ApplyLevelStatModifiers_WithHealToMax_RestoresHealth()
    {
        // Arrange
        var player = CreateTestPlayer();
        // Simulate some damage
        player.TakeDamage(50);
        player.Health.Should().BeLessThan(100);

        var modifiers = LevelStatModifiers.DefaultLevelUp;

        // Act
        player.ApplyLevelStatModifiers(modifiers, healToNewMax: true);

        // Assert
        player.Stats.MaxHealth.Should().Be(105);
        player.Health.Should().Be(105); // Fully healed to new max
    }

    [Test]
    public void ApplyLevelStatModifiers_WithoutHealToMax_PreservesCurrentHealth()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.TakeDamage(50);
        var healthBefore = player.Health;

        var modifiers = LevelStatModifiers.DefaultLevelUp;

        // Act
        player.ApplyLevelStatModifiers(modifiers, healToNewMax: false);

        // Assert
        player.Stats.MaxHealth.Should().Be(105);
        player.Health.Should().Be(healthBefore); // Health unchanged
    }

    [Test]
    public void GetExperienceForLevel_Level1_ReturnsZero()
    {
        // Act
        var xp = Player.GetExperienceForLevel(1);

        // Assert
        xp.Should().Be(0);
    }

    [Test]
    public void GetExperienceForLevel_Level2_Returns200()
    {
        // Act
        var xp = Player.GetExperienceForLevel(2);

        // Assert
        xp.Should().Be(200);
    }

    [Test]
    public void GetExperienceForLevel_Level3_Returns300()
    {
        // Act
        var xp = Player.GetExperienceForLevel(3);

        // Assert
        xp.Should().Be(300);
    }

    [Test]
    public void GetExperienceForLevel_Level10_Returns1000()
    {
        // Act
        var xp = Player.GetExperienceForLevel(10);

        // Assert
        xp.Should().Be(1000);
    }

    [Test]
    public void GetLevelForExperience_Zero_ReturnsLevel1()
    {
        // Act
        var level = Player.GetLevelForExperience(0);

        // Assert
        level.Should().Be(1);
    }

    [Test]
    public void GetLevelForExperience_199_ReturnsLevel1()
    {
        // Act (just below level 2 threshold)
        var level = Player.GetLevelForExperience(199);

        // Assert
        level.Should().Be(1);
    }

    [Test]
    public void GetLevelForExperience_200_ReturnsLevel2()
    {
        // Act (exactly at level 2 threshold)
        var level = Player.GetLevelForExperience(200);

        // Assert
        level.Should().Be(2);
    }

    [Test]
    public void GetLevelForExperience_299_ReturnsLevel2()
    {
        // Act (just below level 3 threshold)
        var level = Player.GetLevelForExperience(299);

        // Assert
        level.Should().Be(2);
    }

    [Test]
    public void GetLevelForExperience_300_ReturnsLevel3()
    {
        // Act (exactly at level 3 threshold)
        var level = Player.GetLevelForExperience(300);

        // Assert
        level.Should().Be(3);
    }

    [Test]
    public void GetLevelForExperience_1000_ReturnsLevel10()
    {
        // Act
        var level = Player.GetLevelForExperience(1000);

        // Assert
        level.Should().Be(10);
    }

    [Test]
    public void GetLevelForExperience_NegativeValue_ReturnsLevel1()
    {
        // Act
        var level = Player.GetLevelForExperience(-100);

        // Assert
        level.Should().Be(1);
    }

    [Test]
    public void SetLevel_UpdatesLevel()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        player.SetLevel(5);

        // Assert
        player.Level.Should().Be(5);
    }

    [Test]
    public void ExperienceToNextLevel_AfterLevelUp_UpdatesCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Level.Should().Be(1);
        player.ExperienceToNextLevel.Should().Be(200); // Level 2 threshold

        // Act
        player.SetLevel(2);

        // Assert
        player.ExperienceToNextLevel.Should().Be(300); // Level 3 threshold
    }
}
