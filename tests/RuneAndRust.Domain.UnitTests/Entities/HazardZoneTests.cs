using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for the HazardZone entity.
/// </summary>
[TestFixture]
public class HazardZoneTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsHazardZone()
    {
        // Arrange & Act
        var hazard = HazardZone.Create(
            definitionId: "fire-pit",
            name: "Blazing Fire Pit",
            description: "Flames leap from a pit.",
            hazardType: HazardType.Fire,
            damagePerTurnDice: "2d6",
            damageType: "fire");

        // Assert
        hazard.Id.Should().NotBeEmpty();
        hazard.DefinitionId.Should().Be("fire-pit"); // Lowercased
        hazard.Name.Should().Be("Blazing Fire Pit");
        hazard.HazardType.Should().Be(HazardType.Fire);
        hazard.DamagePerTurnDice.Should().Be("2d6");
        hazard.DamageType.Should().Be("fire");
        hazard.IsActive.Should().BeTrue();
        hazard.IsPermanent.Should().BeTrue(); // Default duration is -1
    }

    [Test]
    public void FromDefinition_CreatesHazardCorrectly()
    {
        // Arrange
        var definition = new HazardDefinition
        {
            Id = "poison-gas-cloud",
            Name = "Poison Gas Cloud",
            Description = "A toxic cloud of gas.",
            HazardType = HazardType.PoisonGas,
            DamagePerTurn = new HazardDamageDefinition
            {
                Dice = "1d6",
                DamageType = "poison"
            },
            StatusEffects = new List<string> { "poisoned" },
            StatusDuration = 3,
            Save = new HazardSaveDefinition
            {
                Attribute = "Fortitude",
                DC = 13,
                Negates = false
            },
            Duration = -1,
            Keywords = new List<string> { "gas", "poison", "cloud" }
        };

        // Act
        var hazard = HazardZone.FromDefinition(definition);

        // Assert
        hazard.DefinitionId.Should().Be("poison-gas-cloud");
        hazard.Name.Should().Be("Poison Gas Cloud");
        hazard.HazardType.Should().Be(HazardType.PoisonGas);
        hazard.DamagePerTurnDice.Should().Be("1d6");
        hazard.DamageType.Should().Be("poison");
        hazard.StatusEffects.Should().Contain("poisoned");
        hazard.HasSave.Should().BeTrue();
        hazard.Save!.Value.Attribute.Should().Be("Fortitude");
        hazard.Save!.Value.DC.Should().Be(13);
        hazard.Keywords.Should().Contain("gas");
        hazard.IsPermanent.Should().BeTrue();
    }

    [Test]
    public void ProcessTurnTick_WhenPermanent_ReturnsFalse()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "test", "Test Hazard", "Test",
            HazardType.Fire,
            duration: -1);

        // Act
        var expired = hazard.ProcessTurnTick();

        // Assert
        expired.Should().BeFalse();
        hazard.IsActive.Should().BeTrue();
        hazard.IsPermanent.Should().BeTrue();
    }

    [Test]
    public void ProcessTurnTick_WhenTemporary_DecrementsAndDeactivates()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "test", "Test Hazard", "Test",
            HazardType.Fire,
            duration: 2);

        // Act - First tick
        var expired1 = hazard.ProcessTurnTick();
        var duration1 = hazard.Duration;

        // Second tick - should expire
        var expired2 = hazard.ProcessTurnTick();

        // Assert
        expired1.Should().BeFalse();
        duration1.Should().Be(1);
        expired2.Should().BeTrue();
        hazard.IsActive.Should().BeFalse();
        hazard.IsExpired.Should().BeTrue();
    }

    [Test]
    public void Deactivate_SetsInactive()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "test", "Test Hazard", "Test",
            HazardType.Fire);

        // Act
        hazard.Deactivate();

        // Assert
        hazard.IsActive.Should().BeFalse();
    }

    [Test]
    public void MatchesKeyword_FindsExactMatch()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Blazing Fire Pit", "Test",
            HazardType.Fire,
            keywords: new[] { "fire", "pit", "flames" });

        // Act & Assert
        hazard.MatchesKeyword("fire").Should().BeTrue();
        hazard.MatchesKeyword("pit").Should().BeTrue();
        hazard.MatchesKeyword("flames").Should().BeTrue();
        hazard.MatchesKeyword("water").Should().BeFalse();
    }

    [Test]
    public void MatchesKeyword_IsCaseInsensitive()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Blazing Fire Pit", "Test",
            HazardType.Fire,
            keywords: new[] { "fire" });

        // Act & Assert
        hazard.MatchesKeyword("FIRE").Should().BeTrue();
        hazard.MatchesKeyword("Fire").Should().BeTrue();
        hazard.MatchesKeyword("FiRe").Should().BeTrue();
    }

    [Test]
    public void MatchesKeyword_MatchesNamePartially()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Blazing Fire Pit", "Test",
            HazardType.Fire);

        // Act & Assert
        hazard.MatchesKeyword("Blazing").Should().BeTrue();
        hazard.MatchesKeyword("Fire").Should().BeTrue();
    }

    [Test]
    public void GetHazardTypeDescription_ReturnsCorrectDescription()
    {
        // Arrange
        var fireHazard = HazardZone.Create("test", "Test", "Test", HazardType.Fire);
        var poisonHazard = HazardZone.Create("test", "Test", "Test", HazardType.PoisonGas);
        var electricHazard = HazardZone.Create("test", "Test", "Test", HazardType.Electricity);

        // Act
        var fireDesc = fireHazard.GetHazardTypeDescription();
        var poisonDesc = poisonHazard.GetHazardTypeDescription();
        var electricDesc = electricHazard.GetHazardTypeDescription();

        // Assert
        fireDesc.Should().Be("Flames burn throughout the area");
        poisonDesc.Should().Be("Poisonous gas fills the area");
        electricDesc.Should().Be("Electrical energy crackles through the air");
    }
}
