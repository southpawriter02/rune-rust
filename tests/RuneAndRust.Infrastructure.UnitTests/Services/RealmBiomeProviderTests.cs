using FluentAssertions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="RealmBiomeProvider"/> implementation.
/// </summary>
[TestFixture]
public class RealmBiomeProviderTests
{
    private Mock<ILogger<RealmBiomeProvider>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<RealmBiomeProvider>>();
    }

    [Test]
    public void GetAllBiomes_WithLoadedData_ReturnsAllBiomes()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetAllBiomes();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.RealmId == RealmId.Midgard);
        result.Should().Contain(b => b.RealmId == RealmId.Muspelheim);
    }

    [Test]
    public void GetBiome_ValidRealmId_ReturnsBiome()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetBiome(RealmId.Midgard);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Midgard");
        result.Subtitle.Should().Be("The Tamed Ruin");
    }

    [Test]
    public void GetBiome_InvalidRealmId_ReturnsNull()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetBiome(RealmId.Asgard);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetZonesForRealm_ValidRealm_ReturnsZones()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZonesForRealm(RealmId.Midgard);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(z => z.Name == "The Greatwood");
        result.Should().Contain(z => z.Name == "The Scar");
    }

    [Test]
    public void GetZonesForRealm_InvalidRealm_ReturnsEmptyList()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZonesForRealm(RealmId.Asgard);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetEnvironmentalCondition_ValidType_ReturnsCondition()
    {
        // Arrange
        var conditions = CreateTestConditions();
        var provider = CreateProvider([], conditions);

        // Act
        var result = provider.GetEnvironmentalCondition(EnvironmentalConditionType.IntenseHeat);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Intense Heat");
        result.BaseDc.Should().Be(12);
        result.DamageDice.Should().Be("2d6");
    }

    [Test]
    public void GetEnvironmentalCondition_InvalidType_ReturnsNull()
    {
        // Arrange
        var conditions = CreateTestConditions();
        var provider = CreateProvider([], conditions);

        // Act
        var result = provider.GetEnvironmentalCondition(EnvironmentalConditionType.TotalDarkness);

        // Assert
        result.Should().BeNull();
    }

    private RealmBiomeProvider CreateProvider(
        IEnumerable<RealmBiomeDefinition> biomes,
        IEnumerable<EnvironmentalCondition> conditions)
    {
        return new RealmBiomeProvider(biomes, conditions, _loggerMock.Object);
    }

    private static List<RealmBiomeDefinition> CreateTestBiomes()
    {
        var midgardZones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create("the-greatwood", "The Greatwood", RealmId.Midgard, conditionDcModifier: 2),
            RealmBiomeZone.Create("the-scar", "The Scar", RealmId.Midgard, conditionDcModifier: 0)
        };

        return
        [
            RealmBiomeDefinition.Create(
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
                zones: midgardZones),

            RealmBiomeDefinition.Create(
                RealmId.Muspelheim,
                "Muspelheim",
                "The Burning Caldera",
                deckNumber: 8,
                preGlitchFunction: "Geothermal Power",
                postGlitchState: "Volcanic nightmare",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 300,
                    AethericIntensity = 0.4f,
                    HumidityPercent = 10,
                    LightLevel = 0.6f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.8f
                },
                primaryCondition: EnvironmentalConditionType.IntenseHeat,
                minVerticalZone: VerticalZone.LowerRoots,
                maxVerticalZone: VerticalZone.GroundLevel)
        ];
    }

    private static List<EnvironmentalCondition> CreateTestConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.IntenseHeat,
                "Intense Heat",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d6",
                damageType: "Fire",
                frequency: "Per Turn",
                description: "Volcanic heat sears flesh",
                mitigations: ["Fire Resistance", "Cooling equipment"]),

            EnvironmentalCondition.Create(
                EnvironmentalConditionType.ExtremeCold,
                "Extreme Cold",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d6",
                damageType: "Cold",
                frequency: "Per Turn",
                description: "Flash-frozen temperatures",
                mitigations: ["Cold Resistance", "Heating equipment"])
        ];
    }
}
