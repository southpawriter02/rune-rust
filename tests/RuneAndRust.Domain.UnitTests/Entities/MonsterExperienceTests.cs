using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Monster experience value functionality (v0.0.8a).
/// </summary>
[TestFixture]
public class MonsterExperienceTests
{
    [Test]
    public void Monster_HasExperienceValueProperty()
    {
        // Arrange
        var monster = new Monster("Test Monster", "A test monster", 1, new Stats(30, 5, 2), experienceValue: 50);

        // Assert
        monster.ExperienceValue.Should().Be(50);
    }

    [Test]
    public void Monster_Constructor_DefaultsExperienceToZero()
    {
        // Arrange & Act
        var monster = new Monster("Test Monster", "A test monster", 1, new Stats(30, 5, 2));

        // Assert
        monster.ExperienceValue.Should().Be(0);
    }

    [Test]
    public void Monster_NegativeExperience_ClampsToZero()
    {
        // Arrange & Act
        var monster = new Monster("Test Monster", "A test monster", 1, new Stats(30, 5, 2), experienceValue: -10);

        // Assert
        monster.ExperienceValue.Should().Be(0);
    }

    [Test]
    public void CreateGoblin_Has25ExperienceValue()
    {
        // Arrange & Act
        var goblin = Monster.CreateGoblin();

        // Assert
        goblin.ExperienceValue.Should().Be(25);
    }

    [Test]
    public void CreateSkeleton_Has20ExperienceValue()
    {
        // Arrange & Act
        var skeleton = Monster.CreateSkeleton();

        // Assert
        skeleton.ExperienceValue.Should().Be(20);
    }

    [Test]
    public void CreateOrc_Has40ExperienceValue()
    {
        // Arrange & Act
        var orc = Monster.CreateOrc();

        // Assert
        orc.ExperienceValue.Should().Be(40);
    }

    [Test]
    public void CreateGoblinShaman_Has30ExperienceValue()
    {
        // Arrange & Act
        var shaman = Monster.CreateGoblinShaman();

        // Assert
        shaman.ExperienceValue.Should().Be(30);
    }

    [Test]
    public void CreateSlime_Has15ExperienceValue()
    {
        // Arrange & Act
        var slime = Monster.CreateSlime();

        // Assert
        slime.ExperienceValue.Should().Be(15);
    }

    [Test]
    public void Monster_LargeExperienceValue_Accepted()
    {
        // Arrange & Act
        var boss = new Monster("Dragon", "A fearsome dragon", 10, new Stats(500, 50, 30), experienceValue: 1000);

        // Assert
        boss.ExperienceValue.Should().Be(1000);
    }
}
