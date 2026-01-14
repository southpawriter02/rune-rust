using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for PlayerSkill entity (v0.4.3c).
/// </summary>
[TestFixture]
public class PlayerSkillTests
{
    private SkillDefinition CreateTestDefinition() =>
        SkillDefinition.Create("test-skill", "Test Skill", "A test.", "wits");

    [Test]
    public void Create_WithValidParameters_CreatesPlayerSkill()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var skill = PlayerSkill.Create("perception", playerId);

        // Assert
        skill.SkillId.Should().Be("perception");
        skill.PlayerId.Should().Be(playerId);
        skill.Proficiency.Should().Be(SkillProficiency.Untrained);
        skill.Experience.Should().Be(0);
    }

    [Test]
    public void Create_WithStartingProficiency_SetsCorrectly()
    {
        // Act
        var skill = PlayerSkill.Create("test", Guid.NewGuid(),
            SkillProficiency.Journeyman, 200);

        // Assert
        skill.Proficiency.Should().Be(SkillProficiency.Journeyman);
        skill.Experience.Should().Be(200);
    }

    [Test]
    public void AddExperience_IncreasesExperience()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid());
        var definition = CreateTestDefinition();

        // Act
        skill.AddExperience(25, definition);

        // Assert
        skill.Experience.Should().Be(25);
    }

    [Test]
    public void AddExperience_CrossesThreshold_IncreasesProficiency()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid(),
            SkillProficiency.Novice, 45);
        var definition = CreateTestDefinition();

        // Act
        var leveledUp = skill.AddExperience(10, definition); // 45 + 10 = 55 >= 50

        // Assert
        leveledUp.Should().BeTrue();
        skill.Proficiency.Should().Be(SkillProficiency.Apprentice);
    }

    [Test]
    public void RecordUsage_IncrementsTimesUsed()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid());

        // Act
        skill.RecordUsage(true);
        skill.RecordUsage(false);
        skill.RecordUsage(true);

        // Assert
        skill.TimesUsed.Should().Be(3);
        skill.SuccessfulChecks.Should().Be(2);
    }

    [Test]
    public void GetSuccessRate_ReturnsCorrectPercentage()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid());
        skill.RecordUsage(true);
        skill.RecordUsage(true);
        skill.RecordUsage(false);
        skill.RecordUsage(false);

        // Act
        var rate = skill.GetSuccessRate();

        // Assert
        rate.Should().Be(50);
    }

    [Test]
    public void GetSuccessRate_WithNoUsage_ReturnsZero()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid());

        // Act
        var rate = skill.GetSuccessRate();

        // Assert
        rate.Should().Be(0);
    }

    [Test]
    public void SetProficiency_UpdatesExperienceMinimum()
    {
        // Arrange
        var skill = PlayerSkill.Create("test", Guid.NewGuid());
        var definition = CreateTestDefinition();

        // Act
        skill.SetProficiency(SkillProficiency.Expert, definition);

        // Assert
        skill.Proficiency.Should().Be(SkillProficiency.Expert);
        skill.Experience.Should().BeGreaterOrEqualTo(350);
    }
}
