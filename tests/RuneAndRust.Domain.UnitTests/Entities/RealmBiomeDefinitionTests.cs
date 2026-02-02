using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for <see cref="RealmBiomeDefinition"/> entity.
/// </summary>
[TestFixture]
public class RealmBiomeDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Act
        var definition = CreateTestDefinition();

        // Assert
        definition.RealmId.Should().Be(RealmId.Midgard);
        definition.Name.Should().Be("Midgard");
        definition.Subtitle.Should().Be("The Tamed Ruin");
        definition.DeckNumber.Should().Be(4);
        definition.PrimaryCondition.Should().Be(EnvironmentalConditionType.None);
    }

    [Test]
    public void Create_WithInvalidDeckNumber_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 10, // Invalid
            preGlitchFunction: "Test",
            postGlitchState: "Test",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.LowerTrunk);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_WithInvalidVerticalZoneRange_ThrowsArgumentException()
    {
        // Act
        var act = () => RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 4,
            preGlitchFunction: "Test",
            postGlitchState: "Test",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.Canopy, // Higher than max
            maxVerticalZone: VerticalZone.GroundLevel);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetEffectiveProperties_NoZone_ReturnsBaseProperties()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act
        var properties = definition.GetEffectiveProperties(null);

        // Assert
        properties.TemperatureCelsius.Should().Be(18);
        properties.LightLevel.Should().Be(0.7f);
    }

    [Test]
    public void GetEffectiveProperties_ValidZone_ReturnsZoneOverride()
    {
        // Arrange
        var definition = CreateDefinitionWithZones();

        // Act
        var properties = definition.GetEffectiveProperties("the-mires");

        // Assert
        properties.Should().NotBeNull();
        properties!.HumidityPercent.Should().Be(95); // Zone override
    }

    [Test]
    public void GetEffectiveProperties_InvalidZone_ReturnsBaseProperties()
    {
        // Arrange
        var definition = CreateDefinitionWithZones();

        // Act
        var properties = definition.GetEffectiveProperties("nonexistent-zone");

        // Assert
        properties.TemperatureCelsius.Should().Be(18); // Base properties
    }

    [Test]
    public void GetEffectiveCondition_NoZone_ReturnsPrimaryCondition()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act
        var condition = definition.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.None);
    }

    [Test]
    public void GetZone_ValidId_ReturnsZone()
    {
        // Arrange
        var definition = CreateDefinitionWithZones();

        // Act
        var zone = definition.GetZone("the-greatwood");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Greatwood");
    }

    [Test]
    public void GetZone_CaseInsensitive_ReturnsZone()
    {
        // Arrange
        var definition = CreateDefinitionWithZones();

        // Act
        var zone = definition.GetZone("THE-GREATWOOD");

        // Assert
        zone.Should().NotBeNull();
    }

    [Test]
    public void FullName_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act & Assert
        definition.FullName.Should().Be("Midgard — The Tamed Ruin");
    }

    [Test]
    public void HasZones_WithZones_ReturnsTrue()
    {
        // Arrange
        var definition = CreateDefinitionWithZones();

        // Act & Assert
        definition.HasZones.Should().BeTrue();
    }

    [Test]
    public void HasZones_WithoutZones_ReturnsFalse()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act & Assert
        definition.HasZones.Should().BeFalse();
    }

    private static RealmBiomeDefinition CreateTestDefinition()
    {
        return RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 4,
            preGlitchFunction: "Civilian Habitation",
            postGlitchState: "Agricultural heartland",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.LowerTrunk);
    }

    private static RealmBiomeDefinition CreateDefinitionWithZones()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "the-greatwood",
                "The Greatwood",
                RealmId.Midgard,
                conditionDcModifier: 2),

            RealmBiomeZone.Create(
                "the-mires",
                "The Mires",
                RealmId.Midgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 22,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 95,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.4f
                },
                conditionDcModifier: 4)
        };

        return RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 4,
            preGlitchFunction: "Civilian Habitation",
            postGlitchState: "Agricultural heartland",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.LowerTrunk,
            zones: zones);
    }
}
