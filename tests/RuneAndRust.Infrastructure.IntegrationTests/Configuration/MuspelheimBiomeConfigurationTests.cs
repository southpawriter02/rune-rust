using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Muspelheim biome definition matches canonical lore
/// and v0.19.3a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Muspelheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Muspelheim (Deck 08) — The Eternal Meltdown: A volcanic nightmare realm of eternal
/// flames with IntenseHeat as the primary environmental condition, vertical range Z-1 to Z+2.
/// </para>
/// </remarks>
[TestFixture]
public class MuspelheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalMuspelheim();
        var conditions = CreateMuspelheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void MuspelheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Muspelheim);

        // Assert
        zones.Should().HaveCount(3, "Muspelheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Hearth Perimeter",
            "The Slag Fields",
            "Surtr's Furnace");
    }

    [Test]
    public void MuspelheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var muspelheim = _provider.GetBiome(RealmId.Muspelheim);

        // Assert
        muspelheim.Should().NotBeNull();
        muspelheim!.BaseProperties.TemperatureCelsius.Should().Be(85);
        muspelheim.BaseProperties.AethericIntensity.Should().Be(0.6f);
        muspelheim.BaseProperties.HumidityPercent.Should().Be(5);
        muspelheim.BaseProperties.LightLevel.Should().Be(0.9f);
        muspelheim.BaseProperties.ScaleFactor.Should().Be(1.0f);
        muspelheim.BaseProperties.CorrosionRate.Should().Be(0.3f);
    }

    [Test]
    public void MuspelheimConfig_PrimaryCondition_IsIntenseHeat()
    {
        // Act
        var muspelheim = _provider.GetBiome(RealmId.Muspelheim);

        // Assert
        muspelheim.Should().NotBeNull();
        muspelheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.IntenseHeat);
    }

    [Test]
    public void MuspelheimConfig_VerticalRange_IsUpperRoots_To_UpperTrunk()
    {
        // Act
        var muspelheim = _provider.GetBiome(RealmId.Muspelheim);

        // Assert
        muspelheim.Should().NotBeNull();
        muspelheim!.MinVerticalZone.Should().Be(VerticalZone.UpperRoots);
        muspelheim.MaxVerticalZone.Should().Be(VerticalZone.UpperTrunk);
    }

    [Test]
    public void MuspelheimConfig_HearthPerimeterZone_HasIntenseHeat_DC10()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Muspelheim, "muspelheim-hearth-perimeter");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Hearth Perimeter");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.IntenseHeat);
        zone.ConditionDcModifier.Should().Be(-2, "DC modifier -2 applied to base DC 12 = effective DC 10");
        zone.GetEffectiveDc(12).Should().Be(10, "base DC 12 + modifier -2 = 10");
        zone.DamageOverride.Should().Be("2d4");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(85);
    }

    [Test]
    public void MuspelheimConfig_SlagFieldsZone_HasElevatedHeat_DC14()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Muspelheim, "muspelheim-slag-fields");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Slag Fields");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.IntenseHeat);
        zone.ConditionDcModifier.Should().Be(2, "DC modifier +2 applied to base DC 12 = effective DC 14");
        zone.GetEffectiveDc(12).Should().Be(14, "base DC 12 + modifier 2 = 14");
        zone.DamageOverride.Should().Be("2d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(130);
    }

    [Test]
    public void MuspelheimConfig_SurtrsFurnaceZone_HasExtremeHeat_DC18()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Muspelheim, "muspelheim-surtrs-furnace");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Surtr's Furnace");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.IntenseHeat);
        zone.ConditionDcModifier.Should().Be(6, "DC modifier +6 applied to base DC 12 = effective DC 18");
        zone.GetEffectiveDc(12).Should().Be(18, "base DC 12 + modifier 6 = 18");
        zone.DamageOverride.Should().Be("3d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(200);
        zone.OverrideProperties.LightLevel.Should().Be(1.0f, "Molten core provides maximum illumination");
    }

    [Test]
    public void MuspelheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Muspelheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void MuspelheimConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Muspelheim);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void MuspelheimConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var muspelheim = _provider.GetBiome(RealmId.Muspelheim);

        // Assert
        muspelheim.Should().NotBeNull();
        muspelheim!.DeckNumber.Should().Be(8, "Muspelheim is Deck 08");
        muspelheim.Subtitle.Should().Be("The Eternal Meltdown");
        muspelheim.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Muspelheim biome definition with all 3 lore zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateCanonicalMuspelheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "muspelheim-hearth-perimeter",
                "The Hearth Perimeter",
                RealmId.Muspelheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 85,
                    AethericIntensity = 0.6f,
                    HumidityPercent = 5,
                    LightLevel = 0.9f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.3f
                },
                overrideCondition: EnvironmentalConditionType.IntenseHeat,
                conditionDcModifier: -2,
                damageOverride: "2d4",
                factionPools: ["fire-elementals", "constructs", "blighted-beasts"],
                description: "Outer ring of Muspelheim where explorers first encounter the heat. Rocky terrain with occasional lava flows visible in the distance."),

            RealmBiomeZone.Create(
                "muspelheim-slag-fields",
                "The Slag Fields",
                RealmId.Muspelheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 130,
                    AethericIntensity = 0.6f,
                    HumidityPercent = 3,
                    LightLevel = 0.8f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.4f
                },
                overrideCondition: EnvironmentalConditionType.IntenseHeat,
                conditionDcModifier: 2,
                damageOverride: "2d6",
                factionPools: ["fire-elementals", "constructs"],
                description: "Intermediate zone of cooled and re-heating lava formations with dangerous lava pockets."),

            RealmBiomeZone.Create(
                "muspelheim-surtrs-furnace",
                "Surtr's Furnace",
                RealmId.Muspelheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 200,
                    AethericIntensity = 0.7f,
                    HumidityPercent = 1,
                    LightLevel = 1.0f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.5f
                },
                overrideCondition: EnvironmentalConditionType.IntenseHeat,
                conditionDcModifier: 6,
                damageOverride: "3d6",
                factionPools: ["fire-elementals"],
                description: "The molten core of Muspelheim with active lava flows and extreme thermal radiation.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Muspelheim,
                "Muspelheim",
                "The Eternal Meltdown",
                deckNumber: 8,
                preGlitchFunction: "Geothermal Power Generation",
                postGlitchState: "Volcanic nightmare realm of eternal flames; extreme heat degrades equipment and accumulates fatigue.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 85,
                    AethericIntensity = 0.6f,
                    HumidityPercent = 5,
                    LightLevel = 0.9f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.3f
                },
                primaryCondition: EnvironmentalConditionType.IntenseHeat,
                minVerticalZone: VerticalZone.UpperRoots,
                maxVerticalZone: VerticalZone.UpperTrunk,
                zones: zones,
                flavorQuote: "Everything burns. Even the stones weep fire.",
                colorPalette: "crimson-orange-charcoal-molten-gold")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Muspelheim zones.
    /// </summary>
    private static List<EnvironmentalCondition> CreateMuspelheimConditions()
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
                description: "Extreme thermal radiation from volcanic activity causes burns and equipment stress.",
                mitigations: ["Fire Resistance", "Cooling equipment", "Hearth shelter"])
        ];
    }
}
