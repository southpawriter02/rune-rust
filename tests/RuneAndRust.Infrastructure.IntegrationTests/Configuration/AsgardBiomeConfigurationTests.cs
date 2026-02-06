using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.IntegrationTests.Configuration;

/// <summary>
/// Integration tests that validate the Asgard biome definition matches canonical lore
/// and v0.19.5a design specification requirements.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the complete Asgard definition (4 canonical zones, faction pools,
/// condition overrides, DC modifiers) using the internal test constructor of RealmBiomeProvider.
/// </para>
/// <para>
/// Asgard (Deck 01 — The Shattered Spire) — Reality-warped command deck with CpsExposure
/// as the primary environmental condition. Unique characteristics: 4 zones (most of any realm),
/// vertical range Z+1 to Z+4 (only realm reaching Orbital), maximum aetheric intensity (1.0),
/// and zero corrosion (temporal stasis preserves all structures).
/// </para>
/// </remarks>
[TestFixture]
public class AsgardBiomeConfigurationTests
{
    private RealmBiomeProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RealmBiomeProvider>();

        var biomes = CreateCanonicalAsgard();
        var conditions = CreateAsgardConditions();
        _provider = new RealmBiomeProvider(biomes, conditions, logger);
    }

    [Test]
    public void AsgardConfig_HasFourCanonicalZones()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Asgard);

        // Assert — Asgard is the only realm with 4 zones
        zones.Should().HaveCount(4, "Asgard has 4 canonical lore zones — the most of any realm");
        zones.Select(z => z.Name).Should().BeEquivalentTo(
            "The Shattered Spire",
            "Heimdallr's Platform",
            "Odin's Archive",
            "Orbital Zone");
    }

    [Test]
    public void AsgardConfig_BaseProperties_MatchSpecification()
    {
        // Act
        var asgard = _provider.GetBiome(RealmId.Asgard);

        // Assert
        asgard.Should().NotBeNull();
        asgard!.BaseProperties.TemperatureCelsius.Should().Be(15);
        asgard.BaseProperties.AethericIntensity.Should().Be(1.0f, "Maximum aetheric intensity");
        asgard.BaseProperties.HumidityPercent.Should().Be(20);
        asgard.BaseProperties.LightLevel.Should().Be(0.8f, "Dim light in Asgard");
        asgard.BaseProperties.ScaleFactor.Should().Be(1.0f, "Standard scale — cognitive hazard, not spatial");
        asgard.BaseProperties.CorrosionRate.Should().Be(0.0f, "Zero corrosion — temporal stasis preserves structures");
    }

    [Test]
    public void AsgardConfig_PrimaryCondition_IsCpsExposure()
    {
        // Act
        var asgard = _provider.GetBiome(RealmId.Asgard);

        // Assert
        asgard.Should().NotBeNull();
        asgard!.PrimaryCondition.Should().Be(EnvironmentalConditionType.CpsExposure);
    }

    [Test]
    public void AsgardConfig_VerticalRange_IsLowerTrunk_To_Orbital()
    {
        // Act
        var asgard = _provider.GetBiome(RealmId.Asgard);

        // Assert — Asgard is the only realm reaching Orbital (Z+4)
        asgard.Should().NotBeNull();
        asgard!.MinVerticalZone.Should().Be(VerticalZone.LowerTrunk, "Z+1 = LowerTrunk");
        asgard.MaxVerticalZone.Should().Be(VerticalZone.Orbital, "Z+4 = Orbital — unique to Asgard");
    }

    [Test]
    public void AsgardConfig_ShatteredSpireZone_HasCpsExposure_DC12()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Asgard, "asgard-shattered-spire");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Shattered Spire");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.CpsExposure);
        zone.ConditionDcModifier.Should().Be(0, "DC modifier 0 applied to base DC 12 = effective DC 12");
        zone.GetEffectiveDc(12).Should().Be(12, "base DC 12 + modifier 0 = 12");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(0.9f, "Slightly reduced aetheric in entry zone");
        zone.OverrideProperties.LightLevel.Should().Be(0.7f, "Dim light from broken structures");
    }

    [Test]
    public void AsgardConfig_HeimdallrsPlatformZone_HasCpsExposure_DC14()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Asgard, "asgard-heimdallrs-platform");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Heimdallr's Platform");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.CpsExposure);
        zone.ConditionDcModifier.Should().Be(2, "DC modifier +2 applied to base DC 12 = effective DC 14");
        zone.GetEffectiveDc(12).Should().Be(14, "base DC 12 + modifier 2 = 14");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(1.0f, "Maximum aetheric at broadcast station");
    }

    [Test]
    public void AsgardConfig_OdinsArchiveZone_HasCpsExposure_DC18()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Asgard, "asgard-odins-archive");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Odin's Archive");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.CpsExposure);
        zone.ConditionDcModifier.Should().Be(6, "DC modifier +6 applied to base DC 12 = effective DC 18");
        zone.GetEffectiveDc(12).Should().Be(18, "base DC 12 + modifier 6 = 18");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(12, "Archive is colder (-3°C from base)");
        zone.OverrideProperties.LightLevel.Should().Be(0.5f, "Low light in archival vaults");
        zone.OverrideProperties.CorrosionRate.Should().Be(0.1f, "Slight corrosion from temporal strain");
    }

    [Test]
    public void AsgardConfig_OrbitalZone_HasCpsExposure_DC18()
    {
        // Act
        var zone = _provider.GetZone(RealmId.Asgard, "asgard-orbital-zone");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Orbital Zone");
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.CpsExposure);
        zone.ConditionDcModifier.Should().Be(6, "DC modifier +6 applied to base DC 12 = effective DC 18");
        zone.GetEffectiveDc(12).Should().Be(18, "base DC 12 + modifier 6 = 18");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.TemperatureCelsius.Should().Be(10, "Coldest zone in Asgard");
        zone.OverrideProperties.LightLevel.Should().Be(0.3f, "Dim in orbital ring segments");
        zone.OverrideProperties.HumidityPercent.Should().Be(10, "Very low humidity in orbit");
    }

    [Test]
    public void AsgardConfig_AllZoneIds_AreUnique()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Asgard);
        var zoneIds = zones.Select(z => z.ZoneId).ToList();

        // Assert
        zoneIds.Should().OnlyHaveUniqueItems("Zone IDs must be unique within a realm");
    }

    [Test]
    public void AsgardConfig_AllZones_HaveFactionPools()
    {
        // Act
        var zones = _provider.GetZonesForRealm(RealmId.Asgard);

        // Assert
        foreach (var zone in zones)
        {
            zone.FactionPools.Should().NotBeEmpty(
                $"Zone '{zone.Name}' should have at least one faction pool");
        }
    }

    [Test]
    public void AsgardConfig_RealmMetadata_MatchesCanon()
    {
        // Act
        var asgard = _provider.GetBiome(RealmId.Asgard);

        // Assert
        asgard.Should().NotBeNull();
        asgard!.DeckNumber.Should().Be(1, "Asgard is Deck 01");
        asgard.Subtitle.Should().Be("The Shattered Spire");
        asgard.FlavorQuote.Should().NotBeNullOrWhiteSpace("Flavor quote should be defined");
    }

    [Test]
    public void AsgardConfig_AethericIntensity_IsMaximum()
    {
        // Act
        var asgard = _provider.GetBiome(RealmId.Asgard);

        // Assert — verify derived properties for maximum aetheric realm
        asgard.Should().NotBeNull();
        asgard!.BaseProperties.IsAethericallyActive.Should().BeTrue(
            "Aetheric intensity of 1.0 is the maximum possible value");
        asgard.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "15°C is cool but within the comfortable range");
        asgard.BaseProperties.IsGiantScale.Should().BeFalse(
            "Scale factor 1.0 — cognitive hazard, not spatial distortion");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates the canonical Asgard biome definition with all 4 lore zones.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Asgard (Deck 01 — The Shattered Spire) features CpsExposure mechanics
    /// with maximum aetheric intensity (1.0). It is the only realm with 4 zones
    /// and the only realm reaching the Orbital vertical zone (Z+4).
    /// </para>
    /// <para>
    /// The four zones progress in CPS intensity: Shattered Spire (entry, DC 12),
    /// Heimdallr's Platform (Signal zone, DC 14), Odin's Archive (knowledge, DC 18),
    /// and Orbital Zone (extreme, DC 18).
    /// </para>
    /// </remarks>
    private static List<RealmBiomeDefinition> CreateCanonicalAsgard()
    {
        var zones = new List<RealmBiomeZone>
        {
            RealmBiomeZone.Create(
                "asgard-shattered-spire",
                "The Shattered Spire",
                RealmId.Asgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 15,
                    AethericIntensity = 0.9f,
                    HumidityPercent = 20,
                    LightLevel = 0.7f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.CpsExposure,
                conditionDcModifier: 0,
                factionPools: ["constructs", "temporal-echoes"],
                description: "Broken command tower fragments floating in temporal stasis. Entry-level zone with moderate CPS accumulation."),

            RealmBiomeZone.Create(
                "asgard-heimdallrs-platform",
                "Heimdallr's Platform",
                RealmId.Asgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 15,
                    AethericIntensity = 1.0f,
                    HumidityPercent = 20,
                    LightLevel = 0.8f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.CpsExposure,
                conditionDcModifier: 2,
                factionPools: ["constructs", "temporal-echoes"],
                description: "Automated broadcast station transmitting temporal coordinates and navigation clues."),

            RealmBiomeZone.Create(
                "asgard-odins-archive",
                "Odin's Archive",
                RealmId.Asgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 12,
                    AethericIntensity = 1.0f,
                    HumidityPercent = 15,
                    LightLevel = 0.5f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.1f
                },
                overrideCondition: EnvironmentalConditionType.CpsExposure,
                conditionDcModifier: 6,
                factionPools: ["temporal-echoes"],
                description: "Vast repository of pre-Glitch knowledge preserved in temporal stasis."),

            RealmBiomeZone.Create(
                "asgard-orbital-zone",
                "Orbital Zone",
                RealmId.Asgard,
                overrideProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 10,
                    AethericIntensity = 1.0f,
                    HumidityPercent = 10,
                    LightLevel = 0.3f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                overrideCondition: EnvironmentalConditionType.CpsExposure,
                conditionDcModifier: 6,
                factionPools: ["temporal-echoes", "constructs"],
                description: "Orbital ring segments where multiple timelines converge. Gate-locked; requires completion of the Heimdallr Protocol.")
        };

        return
        [
            RealmBiomeDefinition.Create(
                RealmId.Asgard,
                "Asgard",
                "The Shattered Spire",
                deckNumber: 1,
                preGlitchFunction: "Command and Control Hub",
                postGlitchState: "Reality-warped command deck haunted by temporal loops and cognitive paradoxes; the Heimdallr Signal broadcasts navigation data at the cost of sanity.",
                baseProperties: new RealmBiomeProperties
                {
                    TemperatureCelsius = 15,
                    AethericIntensity = 1.0f,
                    HumidityPercent = 20,
                    LightLevel = 0.8f,
                    ScaleFactor = 1.0f,
                    CorrosionRate = 0.0f
                },
                primaryCondition: EnvironmentalConditionType.CpsExposure,
                minVerticalZone: VerticalZone.LowerTrunk,
                maxVerticalZone: VerticalZone.Orbital,
                zones: zones,
                flavorQuote: "You have been here before. You will be here again. The Spire remembers what you have not yet done.",
                colorPalette: "steel-blue-temporal-gold-void-black-lightning-white")
        ];
    }

    /// <summary>
    /// Creates the environmental conditions relevant to Asgard zones.
    /// </summary>
    /// <remarks>
    /// CpsExposure (Cognitive Paradox Syndrome) is a cumulative stress condition —
    /// it causes stress damage (1d4) and cognitive paradox progression. Check attribute
    /// is WILL for resisting paradox exposure. Frequency is Per Hour for sustained exposure.
    /// </remarks>
    private static List<EnvironmentalCondition> CreateAsgardConditions()
    {
        return
        [
            EnvironmentalCondition.Create(
                EnvironmentalConditionType.CpsExposure,
                "CPS Exposure",
                "WILL",
                baseDc: 12,
                damageDice: "1d4",
                damageType: "Stress",
                frequency: "Per Hour",
                description: "Psychic radiation that accelerates Cognitive Paradox Syndrome progression.",
                mitigations: ["Psi-dampener", "Mental fortitude", "Limited exposure"])
        ];
    }
}
