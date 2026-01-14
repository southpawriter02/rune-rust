using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Player skill collection (v0.4.3c).
/// </summary>
[TestFixture]
public class PlayerSkillsCollectionTests
{
    private Player CreateTestPlayer() => new Player("TestPlayer");

    [Test]
    public void AddSkill_AddsToCollection()
    {
        // Arrange
        var player = CreateTestPlayer();
        var skill = PlayerSkill.Create("perception", player.Id);

        // Act
        player.AddSkill(skill);

        // Assert
        player.Skills.Should().ContainKey("perception");
        player.HasSkill("perception").Should().BeTrue();
    }

    [Test]
    public void GetSkill_ReturnsCorrectSkill()
    {
        // Arrange
        var player = CreateTestPlayer();
        var skill = PlayerSkill.Create("stealth", player.Id);
        player.AddSkill(skill);

        // Act
        var retrieved = player.GetSkill("stealth");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.SkillId.Should().Be("stealth");
    }

    [Test]
    public void GetSkill_WithUnknownId_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var skill = player.GetSkill("unknown");

        // Assert
        skill.Should().BeNull();
    }

    [Test]
    public void GetSkillProficiency_ReturnsCorrectLevel()
    {
        // Arrange
        var player = CreateTestPlayer();
        var skill = PlayerSkill.Create("perception", player.Id,
            SkillProficiency.Expert, 400);
        player.AddSkill(skill);

        // Act
        var proficiency = player.GetSkillProficiency("perception");

        // Assert
        proficiency.Should().Be(SkillProficiency.Expert);
    }

    [Test]
    public void GetSkillProficiency_UnknownSkill_ReturnsUntrained()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var proficiency = player.GetSkillProficiency("unknown");

        // Assert
        proficiency.Should().Be(SkillProficiency.Untrained);
    }
}
