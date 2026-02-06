using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Midgard biome definition matches canonical lore
/// and scope breakdown requirements.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the complete Midgard definition (all 4 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// JSON file loading integration is not tested here because the domain entities use
/// private constructors/setters, requiring DTO-based deserialization which is tracked
/// as a separate infrastructure concern.
/// </para>
/// </remarks>
[TestFixture]
public class MidgardBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalMidgard();
        var conditions = CreateMidgardConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void MidgardConfig_HasFourCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Midgard);

        // Assert
        zones.Should().HaveCount(4, "Midgard has 4 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Greatwood",
            "The Asgardian Scar",
            "The Souring Mires",
            "The Serpent Fjords");
    }

    [Test]
    public void MidgardConfig_BaseProperties_MatchTemperateDefaults()
    {
        // Act
        var midgard = _provider.GetBiome(RealmId.Midgard);

        // Assert
        midgard.Should().NotBeNull();
        midgard!.BaseProperties.TemperatureCelsius.Should().Be(18);
        midgard.BaseProperties.AethericIntensity.Should().Be(0.3f);
        midgard.BaseProperties.HumidityPercent.Should().Be(60);
        midgard.BaseProperties.LightLevel.Should().Be(0.7f);
        midgard.BaseProperties.ScaleFactor.Should().Be(1.0f);
        midgard.BaseProperties.CorrosionRate.Should().Be(0.2f);
    }

    [Test]
    public void MidgardConfig_GreatwoodZone_HasCorrectOverrides()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Midgard, "midgard-greatwood");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Greatwood");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
        zone.ConditionDcModifier.Should().Be(-4);
        zone.DamageOverride.Should().Be("1d4");
        zone.Description.Should().Contain("Feral forest");
    }

    [Test]
    public void MidgardConfig_ScarZone_HasBlightProperties()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Midgard, "midgard-scar");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Asgardian Scar");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        zone.ConditionDcModifier.Should().Be(2);
        zone.DamageOverride.Should().Be("2d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(0.8f);
        zone.OverrideProperties.CorrosionRate.Should().Be(0.5f);
    }

    [Test]
    public void MidgardConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Midgard);

        // Assert
        foreach (var zone in zones)
        {
            zone.HasFactionPools.Should().BeTrue(
                $"Zone '{zone.Name}' should have faction pools defined");
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void MidgardConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Midgard);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Midgard biome definition with all 4 lore zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateCanonicalMidgard()
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
                damageOverride: "1d4",
                factionPools: ["blighted-beasts", "constructs", "humanoid"],
                description: "Feral forest descended from curated pre-Glitch parks."),

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
                damageOverride: "2d6",
                factionPools: ["forlorn", "constructs", "blighted-beasts"],
                description: "Colossal impact crater where portions of Asgard crashed."),

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
                damageOverride: "1d6",
                factionPools: ["humanoid", "blighted-beasts"],
                description: "Toxic wetlands draining industrial runoff."),

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
                factionPools: ["blighted-beasts", "humanoid"],
                description: "Poisoned coastlines where Hafgufa's Brood patrol the shallows.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Midgard,
                "Midgard",
                "The Tamed Ruin",
                deckNumber: 4,
                preGlitchFunction: "Civilian Habitation and Agricultural Production",
                postGlitchState: "Agricultural heartland; most populous realm",
                baseProperties: RealmBiomeProperties.Temperate(),
                primaryCondition: EnvironmentalConditionType.None,
                minVerticalZone: VerticalZone.GroundLevel,
                maxVerticalZone: VerticalZone.LowerTrunk,
                zones: midgardZones,
                flavorQuote: "The Wall is the soul of the Hold.",
                colorPalette: "green-brown-gray-rust")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Midgard zones.
    /// </summary>
    private static List<EnvironmentalCondition> CreateMidgardConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.MutagenicSpores,
                "Mutagenic Spores",
                "STURDINESS",
                baseDc: 12,
                damageDice: "1d6",
                damageType: "Poison",
                frequency: "Per Hour",
                description: "Fungal spores that cause cellular mutation",
                mitigations: ["Bio-filter", "Sealed suit", "Antifungal treatment"]),

            EnvironmentalCondition.Create(
                EnvironmentalConditionType.RealityFlux,
                "Reality Flux",
                "WILL",
                baseDc: 12,
                damageDice: "1d8",
                damageType: "Psychic",
                frequency: "Per Turn",
                description: "Unstable reality warps perception",
                mitigations: ["Psi-shield", "Mental training", "Reality anchor"]),

            EnvironmentalCondition.Create(
                EnvironmentalConditionType.ToxicAtmosphere,
                "Toxic Atmosphere",
                "STURDINESS",
                baseDc: 12,
                damageDice: "2d4",
                damageType: "Poison",
                frequency: "Per Turn",
                description: "Industrial pollutants and biological decay",
                mitigations: ["Poison Resistance", "Respirator", "Environmental seal"])
        ];
    }
}
