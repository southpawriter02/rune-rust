using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

/// <summary>
/// Unit tests for ExperienceService (v0.0.8a).
/// </summary>
[TestFixture]
public class ExperienceServiceTests
{
    private ExperienceService _experienceService = null!;
    private Mock<ILogger<ExperienceService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ExperienceService>>();
        _experienceService = new ExperienceService(_loggerMock.Object);
    }

    private Player CreateTestPlayer()
    {
        return new Player("TestHero");
    }

    private Monster CreateDefeatedMonster(int experienceValue = 25)
    {
        var monster = new Monster("Goblin", "A goblin", 1, new Stats(30, 5, 2), experienceValue: experienceValue);
        // Defeat the monster by dealing fatal damage
        monster.TakeDamage(100);
        return monster;
    }

    private Monster CreateAliveMonster(int experienceValue = 25)
    {
        return new Monster("Goblin", "A goblin", 1, new Stats(30, 5, 2), experienceValue: experienceValue);
    }

    [Test]
    public void AwardExperienceFromMonster_DefeatedMonster_AwardsXp()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateDefeatedMonster(25);

        // Act
        var result = _experienceService.AwardExperienceFromMonster(player, monster);

        // Assert
        result.DidGainExperience.Should().BeTrue();
        result.AmountGained.Should().Be(25);
        result.NewTotal.Should().Be(25);
        player.Experience.Should().Be(25);
    }

    [Test]
    public void AwardExperienceFromMonster_AliveMonster_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateAliveMonster(25);

        // Act
        var result = _experienceService.AwardExperienceFromMonster(player, monster);

        // Assert
        result.DidGainExperience.Should().BeFalse();
        result.AmountGained.Should().Be(0);
        player.Experience.Should().Be(0);
    }

    [Test]
    public void AwardExperienceFromMonster_MonsterWithZeroXp_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateDefeatedMonster(0);

        // Act
        var result = _experienceService.AwardExperienceFromMonster(player, monster);

        // Assert
        result.DidGainExperience.Should().BeFalse();
        result.AmountGained.Should().Be(0);
    }

    [Test]
    public void AwardExperience_AddsToPlayerTotal()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddExperience(50);

        // Act
        var result = _experienceService.AwardExperience(player, 25, "Quest reward");

        // Assert
        result.DidGainExperience.Should().BeTrue();
        result.AmountGained.Should().Be(25);
        result.NewTotal.Should().Be(75);
        result.PreviousTotal.Should().Be(50);
        result.Source.Should().Be("Quest reward");
        player.Experience.Should().Be(75);
    }

    [Test]
    public void AwardExperience_ZeroAmount_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddExperience(100);

        // Act
        var result = _experienceService.AwardExperience(player, 0, "Empty reward");

        // Assert
        result.DidGainExperience.Should().BeFalse();
        result.AmountGained.Should().Be(0);
        result.NewTotal.Should().Be(100);
        player.Experience.Should().Be(100);
    }

    [Test]
    public void AwardExperience_NegativeAmount_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddExperience(100);

        // Act
        var result = _experienceService.AwardExperience(player, -10, "Invalid");

        // Assert
        result.DidGainExperience.Should().BeFalse();
        result.AmountGained.Should().Be(0);
        player.Experience.Should().Be(100);
    }

    [Test]
    public void AwardExperience_ReturnsCorrectResult()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _experienceService.AwardExperience(player, 100, "Combat victory");

        // Assert
        result.Should().NotBeNull();
        result.AmountGained.Should().Be(100);
        result.NewTotal.Should().Be(100);
        result.PreviousTotal.Should().Be(0);
        result.Source.Should().Be("Combat victory");
    }

    [Test]
    public void GetDefaultMonsterXp_Goblin_Returns25()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Goblin");

        // Assert
        xp.Should().Be(25);
    }

    [Test]
    public void GetDefaultMonsterXp_Skeleton_Returns20()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Skeleton");

        // Assert
        xp.Should().Be(20);
    }

    [Test]
    public void GetDefaultMonsterXp_Orc_Returns40()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Orc");

        // Assert
        xp.Should().Be(40);
    }

    [Test]
    public void GetDefaultMonsterXp_GoblinShaman_Returns30()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Goblin Shaman");

        // Assert
        xp.Should().Be(30);
    }

    [Test]
    public void GetDefaultMonsterXp_Slime_Returns15()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Slime");

        // Assert
        xp.Should().Be(15);
    }

    [Test]
    public void GetDefaultMonsterXp_UnknownMonster_Returns10()
    {
        // Act
        var xp = ExperienceService.GetDefaultMonsterXp("Unknown Beast");

        // Assert
        xp.Should().Be(10);
    }

    [Test]
    public void GetDefaultMonsterXp_CaseInsensitive()
    {
        // Act & Assert
        ExperienceService.GetDefaultMonsterXp("GOBLIN").Should().Be(25);
        ExperienceService.GetDefaultMonsterXp("goblin").Should().Be(25);
        ExperienceService.GetDefaultMonsterXp("GoBLiN").Should().Be(25);
    }

    [Test]
    public void AwardExperienceFromMonster_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var monster = CreateDefeatedMonster();

        // Act
        var act = () => _experienceService.AwardExperienceFromMonster(null!, monster);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AwardExperienceFromMonster_NullMonster_ThrowsArgumentNullException()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var act = () => _experienceService.AwardExperienceFromMonster(player, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
