using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Svartalfheim biome definition matches canonical lore
/// and v0.19.2a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Svartalfheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Svartalfheim (Deck 06) — The Dvergr Forges: An underground manufacturing complex
/// with TotalDarkness as the primary environmental condition, vertical range Z-2 to Z0.
/// </para>
/// </remarks>
[TestFixture]
public class SvartalfheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalSvartalfheim();
        var conditions = CreateSvartalfheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void SvartalfheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Svartalfheim);

        // Assert
        zones.Should().HaveCount(3, "Svartalfheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Bright Halls",
            "The Black Veins",
            "Forge Core");
    }

    [Test]
    public void SvartalfheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var svartalfheim = _provider.GetBiome(RealmId.Svartalfheim);

        // Assert
        svartalfheim.Should().NotBeNull();
        svartalfheim!.BaseProperties.TemperatureCelsius.Should().Be(25);
        svartalfheim.BaseProperties.AethericIntensity.Should().Be(0.4f);
        svartalfheim.BaseProperties.HumidityPercent.Should().Be(30);
        svartalfheim.BaseProperties.LightLevel.Should().Be(0.3f);
        svartalfheim.BaseProperties.ScaleFactor.Should().Be(0.9f);
        svartalfheim.BaseProperties.CorrosionRate.Should().Be(0.1f);
    }

    [Test]
    public void SvartalfheimConfig_PrimaryCondition_IsTotalDarkness()
    {
        // Act
        var svartalfheim = _provider.GetBiome(RealmId.Svartalfheim);

        // Assert
        svartalfheim.Should().NotBeNull();
        svartalfheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.TotalDarkness);
    }

    [Test]
    public void SvartalfheimConfig_VerticalRange_IsMinusTwo_To_Zero()
    {
        // Act
        var svartalfheim = _provider.GetBiome(RealmId.Svartalfheim);

        // Assert
        svartalfheim.Should().NotBeNull();
        svartalfheim!.MinVerticalZone.Should().Be(VerticalZone.LowerRoots);
        svartalfheim.MaxVerticalZone.Should().Be(VerticalZone.GroundLevel);
    }

    [Test]
    public void SvartalfheimConfig_BrightHallsZone_HasNoHazard()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Svartalfheim, "svartalfheim-bright-halls");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Bright Halls");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.None);
        zone.ConditionDcModifier.Should().Be(0);
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.LightLevel.Should().Be(0.6f);
        zone.Description.Should().Contain("trading halls");
    }

    [Test]
    public void SvartalfheimConfig_BlackVeinsZone_HasTotalDarkness_DC16()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Svartalfheim, "svartalfheim-black-veins");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Black Veins");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.TotalDarkness);
        zone.ConditionDcModifier.Should().Be(4, "DC modifier +4 applied to base DC 12 = effective DC 16");
        zone.GetEffectiveDc(12).Should().Be(16, "base DC 12 + modifier 4 = 16");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.LightLevel.Should().Be(0.0f, "Total darkness means zero ambient light");
    }

    [Test]
    public void SvartalfheimConfig_ForgeCoreZone_HasIntenseHeat_DC12()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Svartalfheim, "svartalfheim-forge-core");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Forge Core");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.IntenseHeat);
        zone.ConditionDcModifier.Should().Be(0, "No DC modifier in Forge Core");
        zone.GetEffectiveDc(12).Should().Be(12, "base DC 12 + modifier 0 = 12");
        zone.DamageOverride.Should().Be("1d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(45, "Forge Core has extreme heat");
        zone.OverrideProperties.LightLevel.Should().Be(0.3f, "Forgelight provides partial visibility");
    }

    [Test]
    public void SvartalfheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Svartalfheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Svartalfheim biome definition with all 3 lore zones.
    /// </summary>
    private static List<RealmBiomeDefinition> CreateCanonicalSvartalfheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "svartalfheim-bright-halls",
                "The Bright Halls",
                RealmId.Svartalfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 25,
                    AethericIntensity = 0.4f,
                    HumidityPercent = 25,
                    LightLevel = 0.6f,
                    ScaleFactor = 0.9f,
                    CorrosionRate = 0.1f
                },
                overrideCondition: EnvironmentalConditionType.None,
                conditionDcModifier: 0,
                factionPools: ["dvergr", "humanoid", "constructs"],
                description: "Upper trading halls with merchant stalls, administrative chambers, and the only sustained illumination in Svartalfheim."),

            RealmBiomeZone.Create(
                "svartalfheim-black-veins",
                "The Black Veins",
                RealmId.Svartalfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 25,
                    AethericIntensity = 0.4f,
                    HumidityPercent = 28,
                    LightLevel = 0.0f,
                    ScaleFactor = 0.9f,
                    CorrosionRate = 0.1f
                },
                overrideCondition: EnvironmentalConditionType.TotalDarkness,
                conditionDcModifier: 4,
                factionPools: ["dvergr", "constructs", "blighted-beasts"],
                description: "Ancient mining tunnels with absolute darkness and obsidian veins."),

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
                description: "Sacred forges and crafting areas with intense magical flames.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Svartalfheim,
                "Svartalfheim",
                "The Dvergr Forges",
                deckNumber: 6,
                preGlitchFunction: "Industrial Manufacturing and Resource Processing",
                postGlitchState: "Lightless underground manufacturing complex; perpetual darkness broken only by sacred forges and trade illumination.",
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
                zones: zones,
                flavorQuote: "In the dark, the hammer sings. In the forge, the world is remade.",
                colorPalette: "black-orange-iron-obsidian")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Svartalfheim zones.
    /// </summary>
    private static List<EnvironmentalCondition> CreateSvartalfheimConditions()
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
                description: "Complete absence of natural light. All actions requiring sight have disadvantage.",
                mitigations: ["Darkvision", "Light source", "Echolocation equipment"]),

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
