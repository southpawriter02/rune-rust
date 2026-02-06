using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Vanaheim biome definition matches canonical lore
/// and v0.19.4a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Vanaheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Vanaheim (Deck 03) — The Overgrown Laboratory: An abandoned magical research facility
/// consumed by mutagenic botanical mutations with MutagenicSpores as the primary
/// environmental condition, vertical range Z-2 to Z+3. Spore exposure causes
/// cumulative Corruption with zone-specific multipliers.
/// </para>
/// </remarks>
[TestFixture]
public class VanaheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalVanaheim();
        var conditions = CreateVanaheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void VanaheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Vanaheim);

        // Assert
        zones.Should().HaveCount(3, "Vanaheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Undergrowth",
            "The Canopy Sea",
            "Ymir's Roots");
    }

    [Test]
    public void VanaheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var vanaheim = _provider.GetBiome(RealmId.Vanaheim);

        // Assert
        vanaheim.Should().NotBeNull();
        vanaheim!.BaseProperties.TemperatureCelsius.Should().Be(28);
        vanaheim.BaseProperties.AethericIntensity.Should().Be(0.5f);
        vanaheim.BaseProperties.HumidityPercent.Should().Be(85, "High humidity bio-hazard environment");
        vanaheim.BaseProperties.LightLevel.Should().Be(0.6f);
        vanaheim.BaseProperties.ScaleFactor.Should().Be(1.0f, "Normal scale in Vanaheim");
        vanaheim.BaseProperties.CorrosionRate.Should().Be(0.4f);
    }

    [Test]
    public void VanaheimConfig_PrimaryCondition_IsMutagenicSpores()
    {
        // Act
        var vanaheim = _provider.GetBiome(RealmId.Vanaheim);

        // Assert
        vanaheim.Should().NotBeNull();
        vanaheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
    }

    [Test]
    public void VanaheimConfig_VerticalRange_IsLowerRoots_To_Canopy()
    {
        // Act
        var vanaheim = _provider.GetBiome(RealmId.Vanaheim);

        // Assert
        vanaheim.Should().NotBeNull();
        vanaheim!.MinVerticalZone.Should().Be(VerticalZone.LowerRoots, "Z-2 = LowerRoots");
        vanaheim.MaxVerticalZone.Should().Be(VerticalZone.Canopy, "Z+3 = Canopy");
    }

    [Test]
    public void VanaheimConfig_UndergrowthZone_HasMutagenicSpores_DC10()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Vanaheim, "vanaheim-undergrowth");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Undergrowth");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
        zone.ConditionDcModifier.Should().Be(-2, "DC modifier -2 applied to base DC 12 = effective DC 10");
        zone.GetEffectiveDc(12).Should().Be(10, "base DC 12 + modifier -2 = 10");
        zone.DamageOverride.Should().Be("1d4", "Low spore density causes minimal damage");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(28);
        zone.OverrideProperties.HumidityPercent.Should().Be(85);
    }

    [Test]
    public void VanaheimConfig_CanopySeaZone_HasMutagenicSpores_DC16_DoubledCorruption()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Vanaheim, "vanaheim-canopy-sea");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Canopy Sea");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
        zone.ConditionDcModifier.Should().Be(4, "DC modifier +4 applied to base DC 12 = effective DC 16");
        zone.GetEffectiveDc(12).Should().Be(16, "base DC 12 + modifier 4 = 16");
        zone.DamageOverride.Should().Be("2d4", "2x corruption multiplier reflected in doubled damage dice");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(31, "Hot upper layer (+3°C from base)");
        zone.OverrideProperties.HumidityPercent.Should().Be(95, "Near-saturated humidity in aerial cloud");
        zone.OverrideProperties.AethericIntensity.Should().Be(0.6f, "Elevated aetheric saturation in canopy");
    }

    [Test]
    public void VanaheimConfig_YmirsRootsZone_HasMutagenicSpores_DC12()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Vanaheim, "vanaheim-ymirs-roots");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Ymir's Roots");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
        zone.ConditionDcModifier.Should().Be(0, "DC modifier 0 applied to base DC 12 = effective DC 12");
        zone.GetEffectiveDc(12).Should().Be(12, "base DC 12 + modifier 0 = 12");
        zone.DamageOverride.Should().Be("1d6", "Moderate spore damage in ancient root caverns");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(33, "Geothermal warmth (+5°C from base)");
        zone.OverrideProperties.HumidityPercent.Should().Be(90, "High humidity in deep caverns");
        zone.OverrideProperties.AethericIntensity.Should().Be(0.6f, "Elevated aetheric saturation (+0.1 from base)");
    }

    [Test]
    public void VanaheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Vanaheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void VanaheimConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Vanaheim);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void VanaheimConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var vanaheim = _provider.GetBiome(RealmId.Vanaheim);

        // Assert
        vanaheim.Should().NotBeNull();
        vanaheim!.DeckNumber.Should().Be(3, "Vanaheim is Deck 03");
        vanaheim.Subtitle.Should().Be("The Overgrown Laboratory");
        vanaheim.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    [Test]
    public void VanaheimConfig_BaseProperties_ReflectBioHazardEnvironment()
    {
        // Act
        var vanaheim = _provider.GetBiome(RealmId.Vanaheim);

        // Assert — verify biome properties reflect a warm, humid bio-hazard realm
        vanaheim.Should().NotBeNull();
        vanaheim!.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "28°C is warm but not a thermal extreme");
        vanaheim.BaseProperties.IsGiantScale.Should().BeFalse(
            "Vanaheim has normal 1.0 scale");
        vanaheim.BaseProperties.IsAethericallyActive.Should().BeFalse(
            "0.5 aetheric intensity is below the 0.6 threshold for active");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Vanaheim biome definition with all 3 lore zones.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vanaheim (Deck 03 — The Overgrown Laboratory) features MutagenicSpores mechanics
    /// with cumulative Corruption. Zone-specific damage dice reflect the corruption
    /// multiplier: Undergrowth 1d4 (1x), Canopy Sea 2d4 (2x), Ymir's Roots 1d6 (1x).
    /// </para>
    /// <para>
    /// The three zones provide varied difficulty: Undergrowth (entry, DC 10),
    /// Ymir's Roots (mid, DC 12), and The Canopy Sea (extreme, DC 16).
    /// </para>
    /// </remarks>
    private static List<RealmBiomeDefinition> CreateCanonicalVanaheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "vanaheim-undergrowth",
                "The Undergrowth",
                RealmId.Vanaheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 28,
                    AethericIntensity = 0.5f,
                    HumidityPercent = 85,
                    LightLevel = 0.6f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.4f
                },
                overrideCondition: EnvironmentalConditionType.MutagenicSpores,
                conditionDcModifier: -2,
                damageOverride: "1d4",
                factionPools: ["blighted-beasts", "humanoid", "constructs"],
                description: "Dense ground-level vegetation with massive root systems and initial spore exposure."),

            RealmBiomeZone.Create(
                "vanaheim-canopy-sea",
                "The Canopy Sea",
                RealmId.Vanaheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 31,
                    AethericIntensity = 0.6f,
                    HumidityPercent = 95,
                    LightLevel = 0.4f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.5f
                },
                overrideCondition: EnvironmentalConditionType.MutagenicSpores,
                conditionDcModifier: 4,
                damageOverride: "2d4",
                factionPools: ["blighted-beasts"],
                description: "Dense aerial fungal cloud with floating platforms and extreme biological activity. 2x corruption multiplier."),

            RealmBiomeZone.Create(
                "vanaheim-ymirs-roots",
                "Ymir's Roots",
                RealmId.Vanaheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 33,
                    AethericIntensity = 0.6f,
                    HumidityPercent = 90,
                    LightLevel = 0.3f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.5f
                },
                overrideCondition: EnvironmentalConditionType.MutagenicSpores,
                conditionDcModifier: 0,
                damageOverride: "1d6",
                factionPools: ["blighted-beasts", "constructs"],
                description: "Ancient root systems forming cavern walls with slow-moving biomass and ancient corruption sources.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Vanaheim,
                "Vanaheim",
                "The Overgrown Laboratory",
                deckNumber: 3,
                preGlitchFunction: "Biodome Agricultural Sector",
                postGlitchState: "Abandoned magical research facility consumed by mutagenic botanical mutations; spore exposure causes cumulative Corruption.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 28,
                    AethericIntensity = 0.5f,
                    HumidityPercent = 85,
                    LightLevel = 0.6f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.4f
                },
                primaryCondition: EnvironmentalConditionType.MutagenicSpores,
                minVerticalZone: VerticalZone.LowerRoots,
                maxVerticalZone: VerticalZone.Canopy,
                zones: zones,
                flavorQuote: "The green does not grow. It hunts.",
                colorPalette: "deep-green-spore-yellow-vine-brown-bioluminescent")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Vanaheim zones.
    /// </summary>
    /// <remarks>
    /// MutagenicSpores cause Poison damage and operate on a Per Hour frequency.
    /// Constitution (STURDINESS) saves determine spore resistance.
    /// </remarks>
    private static List<EnvironmentalCondition> CreateVanaheimConditions()
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
                description: "Fungal spores that cause cellular mutation and biological corruption.",
                mitigations: ["Bio-filter", "Sealed suit", "Antifungal treatment"])
        ];
    }
}
