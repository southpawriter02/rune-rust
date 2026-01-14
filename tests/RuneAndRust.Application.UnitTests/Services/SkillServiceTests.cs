using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for SkillService (v0.4.3c).
/// </summary>
[TestFixture]
public class SkillServiceTests
{
    private SkillService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new SkillService();
    }

    private Player CreateTestPlayer() => new Player("TestPlayer");

    [Test]
    public void GetSkillBonus_WithNoSkill_ReturnsZero()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var bonus = _service.GetSkillBonus(player, "perception");

        // Assert
        bonus.Should().Be(0);
    }

    [Test]
    public void GetSkillBonus_WithExpertSkill_ReturnsCorrectBonus()
    {
        // Arrange
        var player = CreateTestPlayer();
        var skill = PlayerSkill.Create("perception", player.Id,
            SkillProficiency.Expert, 400);
        player.AddSkill(skill);

        // Act
        var bonus = _service.GetSkillBonus(player, "perception");

        // Assert
        bonus.Should().Be(4); // Expert = 4 * BonusPerLevel(1)
    }

    [Test]
    public void AwardSkillExperience_CreatesSkillIfMissing()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.AwardSkillExperience(player, "perception", 25);

        // Assert
        result.ExperienceGained.Should().Be(25);
        player.HasSkill("perception").Should().BeTrue();
    }

    [Test]
    public void PerformSkillCheck_UntrainedBlockedSkill_ReturnsFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        // lockpicking has AllowUntrained = false

        // Act
        var result = _service.PerformSkillCheck(player, "lockpicking", 10);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("training");
    }

    [Test]
    public void InitializePlayerSkills_AddsAllSkills()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        _service.InitializePlayerSkills(player);

        // Assert
        player.Skills.Should().NotBeEmpty();
        player.HasSkill("perception").Should().BeTrue();
        player.HasSkill("stealth").Should().BeTrue();
    }

    [Test]
    public void GetPlayerSkills_ReturnsAllSkillInfo()
    {
        // Arrange
        var player = CreateTestPlayer();
        _service.InitializePlayerSkills(player);

        // Act
        var skills = _service.GetPlayerSkills(player).ToList();

        // Assert
        skills.Should().NotBeEmpty();
        skills.Should().Contain(s => s.SkillId == "perception");
    }
}
