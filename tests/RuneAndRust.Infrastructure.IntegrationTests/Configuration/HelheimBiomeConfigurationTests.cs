using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Helheim biome definition matches canonical lore
/// and v0.19.2a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Helheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Helheim (Deck 09) — The Gangrenous Gut: A toxic waste reclamation facility
/// with ToxicAtmosphere as the primary environmental condition, vertical range Z-3 to Z-1.
/// </para>
/// </remarks>
[TestFixture]
public class HelheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalHelheim();
        var conditions = CreateHelheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void HelheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Helheim);

        // Assert
        zones.Should().HaveCount(3, "Helheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "Upper Bile",
            "The Dissolution Pits",
            "Garm's Watch");
    }

    [Test]
    public void HelheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var helheim = _provider.GetBiome(RealmId.Helheim);

        // Assert
        helheim.Should().NotBeNull();
        helheim!.BaseProperties.TemperatureCelsius.Should().Be(28);
        helheim.BaseProperties.AethericIntensity.Should().Be(0.2f);
        helheim.BaseProperties.HumidityPercent.Should().Be(90);
        helheim.BaseProperties.LightLevel.Should().Be(0.4f);
        helheim.BaseProperties.ScaleFactor.Should().Be(1.0f);
        helheim.BaseProperties.CorrosionRate.Should().Be(0.8f);
    }

    [Test]
    public void HelheimConfig_PrimaryCondition_IsToxicAtmosphere()
    {
        // Act
        var helheim = _provider.GetBiome(RealmId.Helheim);

        // Assert
        helheim.Should().NotBeNull();
        helheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.ToxicAtmosphere);
    }

    [Test]
    public void HelheimConfig_VerticalRange_IsMinusThree_To_MinusOne()
    {
        // Act
        var helheim = _provider.GetBiome(RealmId.Helheim);

        // Assert
        helheim.Should().NotBeNull();
        helheim!.MinVerticalZone.Should().Be(VerticalZone.DeepRoots);
        helheim.MaxVerticalZone.Should().Be(VerticalZone.UpperRoots);
    }

    [Test]
    public void HelheimConfig_UpperBileZone_HasToxicAtmosphere_DC12()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-upper-bile");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Upper Bile");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ToxicAtmosphere);
        zone.ConditionDcModifier.Should().Be(0, "No DC modifier in Upper Bile");
        zone.GetEffectiveDc(12).Should().Be(12, "base DC 12 + modifier 0 = 12");
        zone.DamageOverride.Should().Be("1d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.CorrosionRate.Should().Be(0.7f);
    }

    [Test]
    public void HelheimConfig_DissolutionPitsZone_HasToxicAtmosphere_DC20()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-dissolution-pits");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Dissolution Pits");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ToxicAtmosphere);
        zone.ConditionDcModifier.Should().Be(8, "DC modifier +8 applied to base DC 12 = effective DC 20");
        zone.GetEffectiveDc(12).Should().Be(20, "base DC 12 + modifier 8 = 20");
        zone.DamageOverride.Should().Be("3d8");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.HumidityPercent.Should().Be(95);
        zone.OverrideProperties.LightLevel.Should().Be(0.2f);
        zone.OverrideProperties.CorrosionRate.Should().Be(0.9f);
    }

    [Test]
    public void HelheimConfig_GarmsWatchZone_HasNoHazard()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Garm's Watch");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.None,
            "Garm's Watch negates all realm-specific environmental conditions");
        zone.ConditionDcModifier.Should().Be(0);
    }

    [Test]
    public void HelheimConfig_GarmsWatchZone_HasNeutralEnvironment()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Helheim, "helheim-garms-watch");

        // Assert
        zone.Should().NotBeNull();
        zone!.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(26);
        zone.OverrideProperties.HumidityPercent.Should().Be(60);
        zone.OverrideProperties.LightLevel.Should().Be(0.5f, "Sufficient visibility for safe passage");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.0f, "No corrosion in neutral zone");
    }

    [Test]
    public void HelheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Helheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Helheim biome definition with all 3 lore zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateCanonicalHelheim()
    {
        var zones = new List<RealmBiomeZone>
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
                description: "Surface processing chambers with bioluminescent molds and waste collection."),

            RealmBiomeZone.Create(
                "helheim-dissolution-pits",
                "The Dissolution Pits",
                RealmId.Helheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 28,
                    AethericIntensity = 0.1f,
                    HumidityPercent = 95,
                    LightLevel = 0.2f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.9f
                },
                overrideCondition: EnvironmentalConditionType.ToxicAtmosphere,
                conditionDcModifier: 8,
                damageOverride: "3d8",
                factionPools: ["forlorn", "blighted-beasts"],
                description: "Deep recycling vats with caustic chemical solutions and toxic mist."),

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
                description: "Fortified checkpoint and neutral transition zone between Svartalfheim and Helheim.")
        };

        return
        [
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
                zones: zones,
                flavorQuote: "The Gut digests all. Even hope.",
                colorPalette: "sickly-green-bile-yellow-rust-black")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Helheim zones.
    /// </summary>
    private static List<EnvironmentalCondition> CreateHelheimConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.ToxicAtmosphere,
                "Toxic Atmosphere",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d4",
                damageType: "Poison",
                frequency: "Per Turn",
                description: "Industrial pollutants and biological decay create a poisonous miasma.",
                mitigations: ["Poison Resistance", "Respirator", "Environmental seal"])
        ];
    }
}
