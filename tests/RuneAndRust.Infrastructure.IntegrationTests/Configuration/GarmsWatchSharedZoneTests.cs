using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate Garm's Watch as a shared transition zone
/// between Svartalfheim and Helheim per v0.19.2a design specification.
/// </summary>
/// <remarks>
/// <para>
/// Garm's Watch is administratively assigned to Helheim but serves as a neutral
/// checkpoint providing safe passage between both realms. It negates all
/// realm-specific environmental conditions (TotalDarkness, ToxicAtmosphere).
/// </para>
/// </remarks>
[TestFixture]
public class GarmsWatchSharedZoneTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateBothRealms();
        var conditions = CreateConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void GarmsWatch_IsAccessibleFromHelheimRealm()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull("Garm's Watch should be accessible via Helheim realm");
        zone!.Name.Should().Be("Garm's Watch");
    }

    [Test]
    public void GarmsWatch_NegatesEnvironmentalConditions()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.OverrideCondition.Should().Be(EnvironmentalConditionType.None,
            "Garm's Watch negates TotalDarkness and ToxicAtmosphere");
    }

    [Test]
    public void GarmsWatch_HasSufficientLightLevel()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.LightLevel.Should().Be(0.5f,
            "Garm's Watch provides sufficient visibility (0.5) for safe passage");
    }

    [Test]
    public void GarmsWatch_HasZeroCorrosion()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.CorrosionRate.Should().Be(0.0f,
            "Neutral zone should have no corrosion damage to equipment");
    }

    [Test]
    public void GarmsWatch_EffectiveDc_IsBaseDcOnly()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.ConditionDcModifier.Should().Be(0,
            "Garm's Watch has no DC modifier since conditions are negated");
        zone.GetEffectiveDc(12).Should().Be(12, "No modifier applied");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates both Svartalfheim and Helheim biome definitions.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateBothRealms()
    {
        var svartalfheimZones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "svartalfheim-forge-core",
                "Forge Core",
                RealmId.Svartalfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 45,
                    AethericIntensity = 0.5f,
                    HumidityPercent = 20,
                    LightLevel = 0.3f,
                    ScaleFactor = 0.9f,
                    CorrosionRate = 0.1f
                },
                overrideCondition: EnvironmentalConditionType.IntenseHeat,
                conditionDcModifier: 0,
                damageOverride: "1d6",
                factionPools: ["dvergr", "constructs"],
                description: "Sacred forges and crafting areas.")
        };

        var helheimZones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "helheim-upper-bile",
                "Upper Bile",
                RealmId.Helheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 28,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 90,
                    LightLevel = 0.4f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.7f
                },
                overrideCondition: EnvironmentalConditionType.ToxicAtmosphere,
                conditionDcModifier: 0,
                damageOverride: "1d6",
                factionPools: ["forlorn", "blighted-beasts", "constructs"],
                description: "Surface processing chambers."),

            RealmBiomeZone.Create(
                "helheim-garms-watch",
                "Garm's Watch",
                RealmId.Helheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 26,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 60,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.None,
                conditionDcModifier: 0,
                factionPools: ["humanoid", "dvergr"],
                description: "Fortified checkpoint and neutral transition zone.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Svartalfheim,
                "Svartalfheim",
                "The Dvergr Forges",
                deckNumber: 6,
                preGlitchFunction: "Industrial Manufacturing and Resource Processing",
                postGlitchState: "Lightless underground manufacturing complex.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 25,
                    AethericIntensity = 0.4f,
                    HumidityPercent = 30,
                    LightLevel = 0.3f,
                    ScaleFactor = 0.9f,
                    CorrosionRate = 0.1f
                },
                primaryCondition: EnvironmentalConditionType.TotalDarkness,
                minVerticalZone: VerticalZone.LowerRoots,
                maxVerticalZone: VerticalZone.GroundLevel,
                zones: svartalfheimZones),

            RealmBiomeDefinition.Create(
                RealmId.Helheim,
                "Helheim",
                "The Gangrenous Gut",
                deckNumber: 9,
                preGlitchFunction: "Medical and Recycling Facilities",
                postGlitchState: "Toxic waste reclamation facility consumed by biological corruption.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 28,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 90,
                    LightLevel = 0.4f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.8f
                },
                primaryCondition: EnvironmentalConditionType.ToxicAtmosphere,
                minVerticalZone: VerticalZone.DeepRoots,
                maxVerticalZone: VerticalZone.UpperRoots,
                zones: helheimZones)
        ];
    }

    /// <summary>
    /// Creates the environmental conditions used by both realms.
    /// </summary>
    private static List<EnvironmentalCondition> CreateConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.TotalDarkness,
                "Total Darkness",
                "AGILITY",
                baseDc: 12,
                damageDice: "0",
                damageType: "",
                frequency: "Continuous",
                description: "Complete absence of natural light.",
                mitigations: ["Darkvision", "Light source", "Echolocation equipment"]),

            EnvironmentalCondition.Create(
                EnvironmentalConditionType.ToxicAtmosphere,
                "Toxic Atmosphere",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d4",
                damageType: "Poison",
                frequency: "Per Turn",
                description: "Industrial pollutants and biological decay.",
                mitigations: ["Poison Resistance", "Respirator", "Environmental seal"]),

            EnvironmentalCondition.Create(
                EnvironmentalConditionType.IntenseHeat,
                "Intense Heat",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d6",
                damageType: "Fire",
                frequency: "Per Turn",
                description: "Extreme forge temperatures cause burns and exhaustion.",
                mitigations: ["Fire Resistance", "Cooling equipment", "Hearth shelter"])
        ];
    }
}
