using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Player experience-related functionality (v0.0.8a).
/// </summary>
[TestFixture]
public class PlayerExperienceTests
{
    [Test]
    public void NewPlayer_HasZeroExperience()
    {
        // Arrange & Act
        var player = new Player("TestHero");

        // Assert
        player.Experience.Should().Be(0);
    }

    [Test]
    public void AddExperience_PositiveAmount_IncreasesExperience()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        player.AddExperience(100);

        // Assert
        player.Experience.Should().Be(100);
    }

    [Test]
    public void AddExperience_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var act = () => player.AddExperience(-10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddExperience_ReturnsNewTotal()
    {
        // Arrange
        var player = new Player("TestHero");
        player.AddExperience(50);

        // Act
        var result = player.AddExperience(25);

        // Assert
        result.Should().Be(75);
        player.Experience.Should().Be(75);
    }

    [Test]
    public void AddExperience_ZeroAmount_DoesNotChangeExperience()
    {
        // Arrange
        var player = new Player("TestHero");
        player.AddExperience(100);

        // Act
        var result = player.AddExperience(0);

        // Assert
        result.Should().Be(100);
        player.Experience.Should().Be(100);
    }

    [Test]
    public void ExperienceToNextLevel_Level1_Returns200()
    {
        // Arrange
        var player = new Player("TestHero");

        // Assert (Level 1 -> Level 2 requires 200 XP)
        player.Level.Should().Be(1);
        player.ExperienceToNextLevel.Should().Be(200); // (1+1) * 100 = 200
    }

    [Test]
    public void ExperienceProgressPercent_NoExperience_ReturnsZero()
    {
        // Arrange
        var player = new Player("TestHero");

        // Assert
        player.ExperienceProgressPercent.Should().Be(0);
    }

    [Test]
    public void ExperienceProgressPercent_PartialProgress_CalculatesCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");
        player.AddExperience(50); // 50/200 = 25%

        // Assert
        player.ExperienceProgressPercent.Should().Be(25);
    }

    [Test]
    public void ExperienceProgressPercent_HalfProgress_ReturnsFifty()
    {
        // Arrange
        var player = new Player("TestHero");
        player.AddExperience(100); // 100/200 = 50%

        // Assert
        player.ExperienceProgressPercent.Should().Be(50);
    }

    [Test]
    public void ExperienceProgressPercent_ExceedsRequired_ClampedTo100()
    {
        // Arrange
        var player = new Player("TestHero");
        player.AddExperience(300); // 300/200 = 150% -> clamped to 100

        // Assert
        player.ExperienceProgressPercent.Should().Be(100);
    }

    [Test]
    public void AddExperience_MultipleAdditions_AccumulatesCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        player.AddExperience(25);
        player.AddExperience(25);
        player.AddExperience(50);

        // Assert
        player.Experience.Should().Be(100);
        player.ExperienceProgressPercent.Should().Be(50);
    }
}
