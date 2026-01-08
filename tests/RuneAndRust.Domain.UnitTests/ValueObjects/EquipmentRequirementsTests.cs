using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class EquipmentRequirementsTests
{
    [Test]
    public void None_HasNoRequirements()
    {
        // Act
        var requirements = EquipmentRequirements.None;

        // Assert
        requirements.HasRequirements.Should().BeFalse();
        requirements.MinMight.Should().BeNull();
        requirements.MinFortitude.Should().BeNull();
        requirements.MinWill.Should().BeNull();
        requirements.MinWits.Should().BeNull();
        requirements.MinFinesse.Should().BeNull();
        requirements.RequiredClassIds.Should().BeNull();
    }

    [Test]
    public void MeetsRequirements_WhenMet_ReturnsTrue()
    {
        // Arrange
        var requirements = EquipmentRequirements.ForFortitude(12);
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 14, 10, 10, 10) // Fortitude = 14
        );

        // Act & Assert
        requirements.MeetsRequirements(player).Should().BeTrue();
    }

    [Test]
    public void MeetsRequirements_WhenNotMet_ReturnsFalse()
    {
        // Arrange
        var requirements = EquipmentRequirements.ForFortitude(15);
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 12, 10, 10, 10) // Fortitude = 12
        );

        // Act & Assert
        requirements.MeetsRequirements(player).Should().BeFalse();
    }

    [Test]
    public void GetUnmetRequirements_ReturnsDescriptiveList()
    {
        // Arrange
        var requirements = new EquipmentRequirements
        {
            MinFortitude = 14,
            MinMight = 12
        };
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 10, 10, 10, 10) // Low stats
        );

        // Act
        var unmet = requirements.GetUnmetRequirements(player);

        // Assert
        unmet.Should().HaveCount(2);
        unmet.Should().Contain(r => r.Contains("Fortitude 14 required"));
        unmet.Should().Contain(r => r.Contains("Might 12 required"));
    }

    [Test]
    public void ForClasses_CreatesClassRequirement()
    {
        // Act
        var requirements = EquipmentRequirements.ForClasses("warrior", "knight");

        // Assert
        requirements.HasRequirements.Should().BeTrue();
        requirements.RequiredClassIds.Should().Contain("warrior");
        requirements.RequiredClassIds.Should().Contain("knight");
    }

    [Test]
    public void MeetsRequirements_WithClassRequirement_ChecksPlayerClass()
    {
        // Arrange
        var requirements = EquipmentRequirements.ForClasses("warrior");
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            PlayerAttributes.Default
        );
        player.SetClass("martial", "warrior");

        // Act & Assert
        requirements.MeetsRequirements(player).Should().BeTrue();
    }

    [Test]
    public void MeetsRequirements_WithClassRequirement_FailsForWrongClass()
    {
        // Arrange
        var requirements = EquipmentRequirements.ForClasses("warrior");
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            PlayerAttributes.Default
        );
        player.SetClass("caster", "mage");

        // Act & Assert
        requirements.MeetsRequirements(player).Should().BeFalse();
    }

    [Test]
    public void ToString_WithRequirements_FormatsCorrectly()
    {
        // Arrange
        var requirements = new EquipmentRequirements
        {
            MinFortitude = 14,
            MinMight = 12
        };

        // Act
        var result = requirements.ToString();

        // Assert
        result.Should().Contain("Fortitude 14");
        result.Should().Contain("Might 12");
    }

    [Test]
    public void ToString_WithNoRequirements_ReturnsNone()
    {
        // Act
        var result = EquipmentRequirements.None.ToString();

        // Assert
        result.Should().Be("None");
    }
}
