using FluentAssertions;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class SkillDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesSkill()
    {
        // Act
        var skill = SkillDefinition.Create(
            id: "perception",
            name: "Perception",
            description: "Noticing hidden things.",
            primaryAttribute: "wits");

        // Assert
        skill.Id.Should().Be("perception");
        skill.Name.Should().Be("Perception");
        skill.PrimaryAttribute.Should().Be("wits");
        skill.BaseDicePool.Should().Be("1d10");
    }

    [Test]
    public void Create_WithSecondaryAttribute_SetsSecondaryAttribute()
    {
        // Act
        var skill = SkillDefinition.Create(
            id: "athletics",
            name: "Athletics",
            description: "Physical feats.",
            primaryAttribute: "might",
            secondaryAttribute: "fortitude");

        // Assert
        skill.HasSecondaryAttribute.Should().BeTrue();
        skill.SecondaryAttribute.Should().Be("fortitude");
    }

    [Test]
    public void Create_WithInvalidPrimaryAttribute_ThrowsArgumentException()
    {
        // Act
        var act = () => SkillDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            primaryAttribute: "invalid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("primaryAttribute");
    }

    [Test]
    public void Create_WithInvalidDicePool_ThrowsArgumentException()
    {
        // Act
        var act = () => SkillDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            primaryAttribute: "wits",
            baseDicePool: "invalid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("baseDicePool");
    }

    [Test]
    public void RequiresTraining_WhenUntrainedPenalty_ReturnsTrue()
    {
        // Act
        var skill = SkillDefinition.Create(
            id: "lockpicking",
            name: "Lockpicking",
            description: "Open locks.",
            primaryAttribute: "finesse",
            allowUntrained: true,
            untrainedPenalty: 5);

        // Assert
        skill.RequiresTraining.Should().BeTrue();
        skill.UntrainedPenalty.Should().Be(5);
    }
}
