using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Alfheim biome definition matches canonical lore
/// and v0.19.5a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Alfheim definition (3 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Alfheim (Deck 02 — The Glimmering Wound) — Bioluminescent wasteland saturated with
/// reality-warping aetheric radiation. RealityFlux as the primary environmental condition,
/// vertical range Z 0 to Z+3, blinding light (1.0) and very high aetheric intensity (0.9).
/// </para>
/// </remarks>
[TestFixture]
public class AlfheimBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalAlfheim();
        var conditions = CreateAlfheimConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void AlfheimConfig_HasThreeCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Alfheim);

        // Assert
        zones.Should().HaveCount(3, "Alfheim has 3 canonical lore zones");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Luminous Waste",
            "The Prismatic Maze",
            "The Dreaming Core");
    }

    [Test]
    public void AlfheimConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var alfheim = _provider.GetBiome(RealmId.Alfheim);

        // Assert
        alfheim.Should().NotBeNull();
        alfheim!.BaseProperties.TemperatureCelsius.Should().Be(22);
        alfheim.BaseProperties.AethericIntensity.Should().Be(0.9f);
        alfheim.BaseProperties.HumidityPercent.Should().Be(30);
        alfheim.BaseProperties.LightLevel.Should().Be(1.0f, "Alfheim has blinding light");
        alfheim.BaseProperties.ScaleFactor.Should().Be(1.0f, "Standard scale in Alfheim");
        alfheim.BaseProperties.CorrosionRate.Should().Be(0.1f, "Minimal corrosion — structures held by reality force");
    }

    [Test]
    public void AlfheimConfig_PrimaryCondition_IsRealityFlux()
    {
        // Act
        var alfheim = _provider.GetBiome(RealmId.Alfheim);

        // Assert
        alfheim.Should().NotBeNull();
        alfheim!.PrimaryCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
    }

    [Test]
    public void AlfheimConfig_VerticalRange_IsGroundLevel_To_Canopy()
    {
        // Act
        var alfheim = _provider.GetBiome(RealmId.Alfheim);

        // Assert
        alfheim.Should().NotBeNull();
        alfheim!.MinVerticalZone.Should().Be(VerticalZone.GroundLevel, "Z 0 = GroundLevel");
        alfheim.MaxVerticalZone.Should().Be(VerticalZone.Canopy, "Z+3 = Canopy");
    }

    [Test]
    public void AlfheimConfig_LuminousWasteZone_HasRealityFlux_DC10()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Alfheim, "alfheim-luminous-waste");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Luminous Waste");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        zone.ConditionDcModifier.Should().Be(-2, "DC modifier -2 applied to base DC 12 = effective DC 10");
        zone.GetEffectiveDc(12).Should().Be(10, "base DC 12 + modifier -2 = 10");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(0.85f, "Slightly reduced aetheric in entry zone");
        zone.OverrideProperties.LightLevel.Should().Be(1.0f, "Blinding light across the Waste");
    }

    [Test]
    public void AlfheimConfig_PrismaticMazeZone_HasRealityFlux_DC16()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Alfheim, "alfheim-prismatic-maze");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Prismatic Maze");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        zone.ConditionDcModifier.Should().Be(4, "DC modifier +4 applied to base DC 12 = effective DC 16");
        zone.GetEffectiveDc(12).Should().Be(16, "base DC 12 + modifier 4 = 16");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(0.95f, "Near-maximum aetheric in the maze");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.2f, "Increased corrosion from reality fracturing");
    }

    [Test]
    public void AlfheimConfig_DreamingCoreZone_HasRealityFlux_DC14()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Alfheim, "alfheim-dreaming-core");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Dreaming Core");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        zone.ConditionDcModifier.Should().Be(2, "DC modifier +2 applied to base DC 12 = effective DC 14");
        zone.GetEffectiveDc(12).Should().Be(14, "base DC 12 + modifier 2 = 14");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(1.0f, "Maximum aetheric saturation in the Core");
        zone.OverrideProperties.LightLevel.Should().Be(0.7f, "Light dims as reality collapses");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.3f, "Highest corrosion rate in Alfheim");
    }

    [Test]
    public void AlfheimConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Alfheim);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void AlfheimConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Alfheim);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void AlfheimConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var alfheim = _provider.GetBiome(RealmId.Alfheim);

        // Assert
        alfheim.Should().NotBeNull();
        alfheim!.DeckNumber.Should().Be(2, "Alfheim is Deck 02");
        alfheim.Subtitle.Should().Be("The Glimmering Wound");
        alfheim.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    [Test]
    public void AlfheimConfig_AethericIntensity_IsHighEnoughForCpsRisk()
    {
        // Act
        var alfheim = _provider.GetBiome(RealmId.Alfheim);

        // Assert — verify the IsAethericallyActive property fires for aetheric > 0.6
        alfheim.Should().NotBeNull();
        alfheim!.BaseProperties.IsAethericallyActive.Should().BeTrue(
            "Aetheric intensity of 0.9 exceeds the 0.6 threshold for IsAethericallyActive");
        alfheim.BaseProperties.IsGiantScale.Should().BeFalse(
            "Scale factor 1.0 does not trigger IsGiantScale");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Alfheim biome definition with all 3 lore zones.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alfheim (Deck 02 — The Glimmering Wound) features RealityFlux mechanics
    /// with blinding light (1.0) and very high aetheric intensity (0.9). All zones
    /// share the RealityFlux condition with escalating aetheric saturation.
    /// </para>
    /// <para>
    /// The three zones progress in aetheric intensity: Luminous Waste (entry, DC 10),
    /// Prismatic Maze (difficult, DC 16), and Dreaming Core (deep, DC 14).
    /// </para>
    /// </remarks>
    private static List<RealmBiomeDefinition> CreateCanonicalAlfheim()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "alfheim-luminous-waste",
                "The Luminous Waste",
                RealmId.Alfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 22,
                    AethericIntensity = 0.85f,
                    HumidityPercent = 30,
                    LightLevel = 1.0f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.1f
                },
                overrideCondition: EnvironmentalConditionType.RealityFlux,
                conditionDcModifier: -2,
                factionPools: ["constructs", "aetheric-entities"],
                description: "Blinding expanse of crystallized light and fractured research equipment. Entry-level zone with moderate RealityFlux accumulation."),

            RealmBiomeZone.Create(
                "alfheim-prismatic-maze",
                "The Prismatic Maze",
                RealmId.Alfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 22,
                    AethericIntensity = 0.95f,
                    HumidityPercent = 25,
                    LightLevel = 0.9f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.2f
                },
                overrideCondition: EnvironmentalConditionType.RealityFlux,
                conditionDcModifier: 4,
                factionPools: ["aetheric-entities", "genius-loci"],
                description: "Labyrinthine corridors of refracted light where perception and reality diverge."),

            RealmBiomeZone.Create(
                "alfheim-dreaming-core",
                "The Dreaming Core",
                RealmId.Alfheim,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 22,
                    AethericIntensity = 1.0f,
                    HumidityPercent = 35,
                    LightLevel = 0.7f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.3f
                },
                overrideCondition: EnvironmentalConditionType.RealityFlux,
                conditionDcModifier: 2,
                factionPools: ["genius-loci", "aetheric-entities"],
                description: "Heart of the aetheric research complex where reality has fully collapsed.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Alfheim,
                "Alfheim",
                "The Glimmering Wound",
                deckNumber: 2,
                preGlitchFunction: "Aetheric Research Laboratories",
                postGlitchState: "Bioluminescent wasteland saturated with reality-warping aetheric radiation; blinding light and psychic distortion create a hostile, hallucinatory environment.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 22,
                    AethericIntensity = 0.9f,
                    HumidityPercent = 30,
                    LightLevel = 1.0f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.1f
                },
                primaryCondition: EnvironmentalConditionType.RealityFlux,
                minVerticalZone: VerticalZone.GroundLevel,
                maxVerticalZone: VerticalZone.Canopy,
                zones: zones,
                flavorQuote: "The light does not illuminate. It remembers.",
                colorPalette: "blinding-white-prismatic-iridescent-pale-gold-ghostly-blue")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Alfheim zones.
    /// </summary>
    /// <remarks>
    /// RealityFlux is a cumulative stress condition — it causes psychic damage (1d8)
    /// and reality distortion. Check attribute is WILL for resisting mental distortion.
    /// </remarks>
    private static List<EnvironmentalCondition> CreateAlfheimConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.RealityFlux,
                "Reality Flux",
                "WILL",
                baseDc: 12,
                damageDice: "1d8",
                damageType: "Psychic",
                frequency: "Per Turn",
                description: "Unstable reality warps perception and causes psychic damage.",
                mitigations: ["Psi-shield", "Mental training", "Reality anchor"])
        ];
    }
}
