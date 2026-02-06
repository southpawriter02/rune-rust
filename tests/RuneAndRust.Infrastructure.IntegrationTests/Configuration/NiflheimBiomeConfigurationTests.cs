using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Niflheim biome definition matches canonical lore
/// and v0.19.3a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Niflheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Niflheim (Deck 05) — The Frozen Tomb: A flash-frozen wasteland of lethal cold
/// with ExtremeCold as the primary environmental condition, vertical range Z-2 to Z+1.
/// </para>
/// </remarks>
[TestFixture]
public class NiflheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalNiflheim();
        var conditions = CreateNiflheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void NiflheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Niflheim);

        // Assert
        zones.Should().HaveCount(3, "Niflheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Frost Marches",
            "The Living Glacier",
            "Hvergelmir's Heart");
    }

    [Test]
    public void NiflheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var niflheim = _provider.GetBiome(RealmId.Niflheim);

        // Assert
        niflheim.Should().NotBeNull();
        niflheim!.BaseProperties.TemperatureCelsius.Should().Be(-40);
        niflheim.BaseProperties.AethericIntensity.Should().Be(0.3f);
        niflheim.BaseProperties.HumidityPercent.Should().Be(15);
        niflheim.BaseProperties.LightLevel.Should().Be(0.5f);
        niflheim.BaseProperties.ScaleFactor.Should().Be(1.0f);
        niflheim.BaseProperties.CorrosionRate.Should().Be(0.0f);
    }

    [Test]
    public void NiflheimConfig_PrimaryCondition_IsExtremeCold()
    {
        // Act
        var niflheim = _provider.GetBiome(RealmId.Niflheim);

        // Assert
        niflheim.Should().NotBeNull();
        niflheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.ExtremeCold);
    }

    [Test]
    public void NiflheimConfig_VerticalRange_IsLowerRoots_To_LowerTrunk()
    {
        // Act
        var niflheim = _provider.GetBiome(RealmId.Niflheim);

        // Assert
        niflheim.Should().NotBeNull();
        niflheim!.MinVerticalZone.Should().Be(VerticalZone.LowerRoots);
        niflheim.MaxVerticalZone.Should().Be(VerticalZone.LowerTrunk);
    }

    [Test]
    public void NiflheimConfig_FrostMarchesZone_HasExtremeCold_DC10()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Niflheim, "niflheim-frost-marches");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Frost Marches");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ExtremeCold);
        zone.ConditionDcModifier.Should().Be(-2, "DC modifier -2 applied to base DC 12 = effective DC 10");
        zone.GetEffectiveDc(12).Should().Be(10, "base DC 12 + modifier -2 = 10");
        zone.DamageOverride.Should().Be("2d4");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(-40);
    }

    [Test]
    public void NiflheimConfig_LivingGlacierZone_HasModerateCold_DC14()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Niflheim, "niflheim-living-glacier");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Living Glacier");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ExtremeCold);
        zone.ConditionDcModifier.Should().Be(2, "DC modifier +2 applied to base DC 12 = effective DC 14");
        zone.GetEffectiveDc(12).Should().Be(14, "base DC 12 + modifier 2 = 14");
        zone.DamageOverride.Should().Be("2d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(-55);
        zone.OverrideProperties.LightLevel.Should().Be(0.4f, "Reduced visibility in glacial interior");
    }

    [Test]
    public void NiflheimConfig_HvergelmirsHeartZone_HasExtremeCold_DC18()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Niflheim, "niflheim-hvergelmirs-heart");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Hvergelmir's Heart");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ExtremeCold);
        zone.ConditionDcModifier.Should().Be(6, "DC modifier +6 applied to base DC 12 = effective DC 18");
        zone.GetEffectiveDc(12).Should().Be(18, "base DC 12 + modifier 6 = 18");
        zone.DamageOverride.Should().Be("3d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(-80);
        zone.OverrideProperties.AethericIntensity.Should().Be(0.2f, "Extreme cold suppresses aetheric conductivity");
    }

    [Test]
    public void NiflheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Niflheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void NiflheimConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Niflheim);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void NiflheimConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var niflheim = _provider.GetBiome(RealmId.Niflheim);

        // Assert
        niflheim.Should().NotBeNull();
        niflheim!.DeckNumber.Should().Be(5, "Niflheim is Deck 05");
        niflheim.Subtitle.Should().Be("The Frozen Tomb");
        niflheim.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Niflheim biome definition with all 3 lore zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateCanonicalNiflheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "niflheim-frost-marches",
                "The Frost Marches",
                RealmId.Niflheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = -40,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 15,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.ExtremeCold,
                conditionDcModifier: -2,
                damageOverride: "2d4",
                factionPools: ["frost-creatures", "constructs", "blighted-beasts"],
                description: "The outer reaches of Niflheim where cold is extreme but survivable. Rolling plains of packed snow and ice."),

            RealmBiomeZone.Create(
                "niflheim-living-glacier",
                "The Living Glacier",
                RealmId.Niflheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = -55,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 10,
                    LightLevel = 0.4f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.ExtremeCold,
                conditionDcModifier: 2,
                damageOverride: "2d6",
                factionPools: ["frost-creatures", "constructs"],
                description: "Intermediate zone of active glacier formation with treacherous crevasses and shifting ice."),

            RealmBiomeZone.Create(
                "niflheim-hvergelmirs-heart",
                "Hvergelmir's Heart",
                RealmId.Niflheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = -80,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 5,
                    LightLevel = 0.3f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.ExtremeCold,
                conditionDcModifier: 6,
                damageOverride: "3d6",
                factionPools: ["frost-creatures"],
                description: "The frozen core of Niflheim — an enormous ice cavern housing the heart-spring of all cold.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Niflheim,
                "Niflheim",
                "The Frozen Tomb",
                deckNumber: 5,
                preGlitchFunction: "Cryogenic Storage and Deep Archives",
                postGlitchState: "Flash-frozen wasteland of lethal cold; equipment becomes brittle and fragile under extreme cryogenic exposure.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = -40,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 15,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                primaryCondition: EnvironmentalConditionType.ExtremeCold,
                minVerticalZone: VerticalZone.LowerRoots,
                maxVerticalZone: VerticalZone.LowerTrunk,
                zones: zones,
                flavorQuote: "The cold does not kill. It preserves — forever.",
                colorPalette: "pale-blue-white-crystalline-silver")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Niflheim zones.
    /// </summary>
    private static List<EnvironmentalCondition> CreateNiflheimConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.ExtremeCold,
                "Extreme Cold",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d6",
                damageType: "Cold",
                frequency: "Per Turn",
                description: "Lethal cryogenic temperatures cause frostbite, hypothermia, and equipment brittleness.",
                mitigations: ["Cold Resistance", "Heating equipment", "Warm clothing"])
        ];
    }
}
