using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Jötunheim biome definition matches canonical lore
/// and v0.19.4a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Jötunheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Jötunheim (Deck 07) — The Industrial Graveyard: A colossal graveyard of broken machinery
/// and giant remains with GiantScale as the primary environmental condition,
/// vertical range Z-1 to Z+3, and a scale factor of 3.0 that triples fall damage
/// and physical effects.
/// </para>
/// </remarks>
[TestFixture]
public class JotunheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalJotunheim();
        var conditions = CreateJotunheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void JotunheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Jotunheim);

        // Assert
        zones.Should().HaveCount(3, "Jötunheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Bone Yards",
            "Utgard's Shadow",
            "The Grinding Hall");
    }

    [Test]
    public void JotunheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var jotunheim = _provider.GetBiome(RealmId.Jotunheim);

        // Assert
        jotunheim.Should().NotBeNull();
        jotunheim!.BaseProperties.TemperatureCelsius.Should().Be(10);
        jotunheim.BaseProperties.AethericIntensity.Should().Be(0.2f);
        jotunheim.BaseProperties.HumidityPercent.Should().Be(40);
        jotunheim.BaseProperties.LightLevel.Should().Be(0.5f);
        jotunheim.BaseProperties.ScaleFactor.Should().Be(3.0f, "Jötunheim has GiantScale 3.0x multiplier");
        jotunheim.BaseProperties.CorrosionRate.Should().Be(0.5f);
    }

    [Test]
    public void JotunheimConfig_PrimaryCondition_IsGiantScale()
    {
        // Act
        var jotunheim = _provider.GetBiome(RealmId.Jotunheim);

        // Assert
        jotunheim.Should().NotBeNull();
        jotunheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.GiantScale);
    }

    [Test]
    public void JotunheimConfig_VerticalRange_IsUpperRoots_To_Canopy()
    {
        // Act
        var jotunheim = _provider.GetBiome(RealmId.Jotunheim);

        // Assert
        jotunheim.Should().NotBeNull();
        jotunheim!.MinVerticalZone.Should().Be(VerticalZone.UpperRoots, "Z-1 = UpperRoots");
        jotunheim.MaxVerticalZone.Should().Be(VerticalZone.Canopy, "Z+3 = Canopy");
    }

    [Test]
    public void JotunheimConfig_BoneYardsZone_HasGiantScale_DC10()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Jotunheim, "jotunheim-bone-yards");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Bone Yards");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.GiantScale);
        zone.ConditionDcModifier.Should().Be(-2, "DC modifier -2 applied to base DC 12 = effective DC 10");
        zone.GetEffectiveDc(12).Should().Be(10, "base DC 12 + modifier -2 = 10");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(10);
        zone.OverrideProperties.ScaleFactor.Should().Be(3.0f, "GiantScale 3.0x applies in all zones");
    }

    [Test]
    public void JotunheimConfig_UtgardsShadowZone_HasGiantScale_DC12()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Jotunheim, "jotunheim-utgards-shadow");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Utgard's Shadow");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.GiantScale);
        zone.ConditionDcModifier.Should().Be(0, "DC modifier 0 applied to base DC 12 = effective DC 12");
        zone.GetEffectiveDc(12).Should().Be(12, "base DC 12 + modifier 0 = 12");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(8, "Shadow causes cold effect (-2°C from base)");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.7f, "Increased corrosion from corroded machinery");
        zone.OverrideProperties.LightLevel.Should().Be(0.3f, "Reduced light from massive structures casting shadows");
    }

    [Test]
    public void JotunheimConfig_GrindingHallZone_HasGiantScale_DC16()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Jotunheim, "jotunheim-grinding-hall");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Grinding Hall");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.GiantScale);
        zone.ConditionDcModifier.Should().Be(4, "DC modifier +4 applied to base DC 12 = effective DC 16");
        zone.GetEffectiveDc(12).Should().Be(16, "base DC 12 + modifier 4 = 16");
        zone.DamageOverride.Should().Be("2d6", "Active machinery causes direct damage");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(5, "Extreme cold from deep industrial zone");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.8f, "Maximum corrosion rate in this zone");
        zone.OverrideProperties.LightLevel.Should().Be(0.2f, "Near darkness in the grinding chamber");
    }

    [Test]
    public void JotunheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Jotunheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void JotunheimConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Jotunheim);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void JotunheimConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var jotunheim = _provider.GetBiome(RealmId.Jotunheim);

        // Assert
        jotunheim.Should().NotBeNull();
        jotunheim!.DeckNumber.Should().Be(7, "Jötunheim is Deck 07");
        jotunheim.Subtitle.Should().Be("The Industrial Graveyard");
        jotunheim.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    [Test]
    public void JotunheimConfig_ScaleFactor_IsGiantScale()
    {
        // Act
        var jotunheim = _provider.GetBiome(RealmId.Jotunheim);

        // Assert — verify the IsGiantScale property fires for scale > 2.0
        jotunheim.Should().NotBeNull();
        jotunheim!.BaseProperties.IsGiantScale.Should().BeTrue(
            "Scale factor of 3.0 exceeds the 2.0 threshold for IsGiantScale");
        jotunheim.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "10°C is within the comfortable range, not a thermal extreme");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Jötunheim biome definition with all 3 lore zones.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Jötunheim (Deck 07 — The Industrial Graveyard) features GiantScale mechanics
    /// with a 3.0x scale multiplier. All zones share the GiantScale condition.
    /// </para>
    /// <para>
    /// The three zones progress in difficulty: Bone Yards (entry, DC 10),
    /// Utgard's Shadow (mid, DC 12), and The Grinding Hall (extreme, DC 16).
    /// </para>
    /// </remarks>
    private static List<RealmBiomeDefinition> CreateCanonicalJotunheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "jotunheim-bone-yards",
                "The Bone Yards",
                RealmId.Jotunheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 10,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 40,
                    LightLevel = 0.5f,
                    ScaleFactor = 3.0f,
                    CorrosionRate = 0.5f
                },
                overrideCondition: EnvironmentalConditionType.GiantScale,
                conditionDcModifier: -2,
                factionPools: ["constructs", "blighted-beasts", "humanoid"],
                description: "Scattered remains of giant skeletons and broken mechanical parts. Entry-level zone with moderate navigation challenges."),

            RealmBiomeZone.Create(
                "jotunheim-utgards-shadow",
                "Utgard's Shadow",
                RealmId.Jotunheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 8,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 40,
                    LightLevel = 0.3f,
                    ScaleFactor = 3.0f,
                    CorrosionRate = 0.7f
                },
                overrideCondition: EnvironmentalConditionType.GiantScale,
                conditionDcModifier: 0,
                factionPools: ["constructs", "blighted-beasts"],
                description: "Massive iron and steel structures casting deep shadows with corroded machinery and treacherous platforms."),

            RealmBiomeZone.Create(
                "jotunheim-grinding-hall",
                "The Grinding Hall",
                RealmId.Jotunheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 5,
                    AethericIntensity = 0.3f,
                    HumidityPercent = 35,
                    LightLevel = 0.2f,
                    ScaleFactor = 3.0f,
                    CorrosionRate = 0.8f
                },
                overrideCondition: EnvironmentalConditionType.GiantScale,
                conditionDcModifier: 4,
                damageOverride: "2d6",
                factionPools: ["constructs"],
                description: "Central chamber with colossal grinding mechanisms still partially operational.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Jotunheim,
                "Jötunheim",
                "The Industrial Graveyard",
                deckNumber: 7,
                preGlitchFunction: "Megafauna Habitats and Ecological Reserves",
                postGlitchState: "Colossal graveyard of broken machinery and giant remains; GiantScale mechanics triple fall damage and physical effects.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 10,
                    AethericIntensity = 0.2f,
                    HumidityPercent = 40,
                    LightLevel = 0.5f,
                    ScaleFactor = 3.0f,
                    CorrosionRate = 0.5f
                },
                primaryCondition: EnvironmentalConditionType.GiantScale,
                minVerticalZone: VerticalZone.UpperRoots,
                maxVerticalZone: VerticalZone.Canopy,
                zones: zones,
                flavorQuote: "The bones of giants grind beneath your feet. Everything here was built for something far larger than you.",
                colorPalette: "rust-iron-gray-bone-white-dark-green")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Jötunheim zones.
    /// </summary>
    /// <remarks>
    /// GiantScale is a non-damage condition — it applies scale multiplier effects
    /// rather than dealing direct environmental damage. Check attribute is AGILITY
    /// for navigating oversized terrain.
    /// </remarks>
    private static List<EnvironmentalCondition> CreateJotunheimConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.GiantScale,
                "Giant Scale",
                "AGILITY",
                baseDc: 12,
                damageDice: "0",
                damageType: "",
                frequency: "Situational",
                description: "Everything is 3-10x normal scale. Increased fall distances, oversized obstacles.",
                mitigations: ["Climbing gear", "Size-altering equipment", "Flight"])
        ];
    }
}
