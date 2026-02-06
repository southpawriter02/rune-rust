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
    public void GetZonesForRealm_Midgard_ReturnsFourZones()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZonesForRealm(RealmId.Midgard);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(z => z.Name == "The Greatwood");
        result.Should().Contain(z => z.Name == "The Asgardian Scar");
        result.Should().Contain(z => z.Name == "The Souring Mires");
        result.Should().Contain(z => z.Name == "The Serpent Fjords");
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
    public void GetZone_MidgardGreatwood_ReturnsCorrectZone()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZone(RealmId.Midgard, "midgard-greatwood");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("The Greatwood");
        result.ConditionDcModifier.Should().Be(-4);
        result.HasFactionPools.Should().BeTrue();
        result.FactionPools.Should().Contain("blighted-beasts");
    }

    [Test]
    public void GetZone_MidgardScar_HasCorrectConditionOverride()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZone(RealmId.Midgard, "midgard-scar");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("The Asgardian Scar");
        result.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        result.ConditionDcModifier.Should().Be(2);
        result.OverrideProperties.Should().NotBeNull();
        result.OverrideProperties!.AethericIntensity.Should().Be(0.8f);
    }

    [Test]
    public void GetZone_MidgardMires_HasCorrectProperties()
    {
        // Arrange
        var biomes = CreateTestBiomes();
        var provider = CreateProvider(biomes, []);

        // Act
        var result = provider.GetZone(RealmId.Midgard, "midgard-mires");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("The Souring Mires");
        result.OverrideCondition.Should().Be(EnvironmentalConditionType.ToxicAtmosphere);
        result.OverrideProperties.Should().NotBeNull();
        result.OverrideProperties!.HumidityPercent.Should().Be(90);
        result.OverrideProperties.CorrosionRate.Should().Be(0.4f);
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

    /// <summary>
    /// Creates test biome data with canonical Midgard zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateTestBiomes()
    {
        var midgardZones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "midgard-greatwood",
                "The Greatwood",
                RealmId.Midgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 18,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 75,
                    LightLevel = 0.4f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.2f
                },
                overrideCondition: EnvironmentalConditionType.MutagenicSpores,
                conditionDcModifier: -4,
                factionPools: ["blighted-beasts", "constructs", "humanoid"]),

            RealmBiomeZone.Create(
                "midgard-scar",
                "The Asgardian Scar",
                RealmId.Midgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 18,
                    AethericIntensity = 0.8f,
                    HumidityPercent = 60,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.5f
                },
                overrideCondition: EnvironmentalConditionType.RealityFlux,
                conditionDcModifier: 2,
                factionPools: ["forlorn", "constructs", "blighted-beasts"]),

            RealmBiomeZone.Create(
                "midgard-mires",
                "The Souring Mires",
                RealmId.Midgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 18,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 90,
                    LightLevel = 0.6f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.4f
                },
                overrideCondition: EnvironmentalConditionType.ToxicAtmosphere,
                conditionDcModifier: -4,
                factionPools: ["humanoid", "blighted-beasts"]),

            RealmBiomeZone.Create(
                "midgard-fjords",
                "The Serpent Fjords",
                RealmId.Midgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 12,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 80,
                    LightLevel = 0.8f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.2f
                },
                overrideCondition: EnvironmentalConditionType.None,
                conditionDcModifier: 0,
                factionPools: ["blighted-beasts", "humanoid"])
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
