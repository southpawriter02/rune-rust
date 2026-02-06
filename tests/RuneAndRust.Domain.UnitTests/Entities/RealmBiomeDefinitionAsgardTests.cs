using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Domain-level unit tests for Asgard-specific realm biome definition behavior.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate entity-level behavior on the <see cref="RealmBiomeDefinition"/>
/// and <see cref="RealmBiomeZone"/> classes when configured for Asgard's CpsExposure
/// mechanics. Tests focus on zone lookups, effective properties/conditions, and
/// derived property behavior.
/// </para>
/// <para>
/// Asgard (Deck 01 — The Shattered Spire) is unique as the only realm with 4 zones
/// and the only realm utilizing the Orbital (Z +4) vertical zone. It has maximum
/// aetheric intensity (1.0) and CpsExposure as its primary condition.
/// </para>
/// </remarks>
[TestFixture]
public class RealmBiomeDefinitionAsgardTests
{
    private RealmBiomeDefinition _asgard = null!;

    [SetUp]
    public void SetUp()
    {
        _asgard = CreateAsgard();
    }

    [Test]
    public void GetEffectiveProperties_WithShatteredSpireZoneId_ReturnsReducedLightProperties()
    {
        // Act
        var props = _asgard.GetEffectiveProperties("asgard-shattered-spire");

        // Assert — Shattered Spire has reduced light and aetheric
        props.TemperatureCelsius.Should().Be(15, "Shattered Spire has base temperature");
        props.AethericIntensity.Should().Be(0.9f, "Slightly reduced from base 1.0");
        props.LightLevel.Should().Be(0.7f, "Dim light from broken structures");
        props.CorrosionRate.Should().Be(0.0f, "Temporal stasis prevents corrosion");
    }

    [Test]
    public void GetEffectiveProperties_WithHeimdallrsPlatformZoneId_ReturnsMaxAethericProperties()
    {
        // Act
        var props = _asgard.GetEffectiveProperties("asgard-heimdallrs-platform");

        // Assert — Heimdallr's Platform has maximum aetheric from the Signal
        props.AethericIntensity.Should().Be(1.0f, "Maximum aetheric at broadcast station");
        props.LightLevel.Should().Be(0.8f, "Normal light at the platform");
        props.CorrosionRate.Should().Be(0.0f, "Temporal stasis prevents all corrosion");
    }

    [Test]
    public void GetEffectiveProperties_WithOdinsArchiveZoneId_ReturnsDimLightProperties()
    {
        // Act
        var props = _asgard.GetEffectiveProperties("asgard-odins-archive");

        // Assert — Odin's Archive has reduced temperature and dim light
        props.TemperatureCelsius.Should().Be(12, "Archive is colder (-3°C from base)");
        props.LightLevel.Should().Be(0.5f, "Low light in the archival vaults");
        props.CorrosionRate.Should().Be(0.1f, "Slight corrosion from temporal strain on materials");
    }

    [Test]
    public void GetEffectiveProperties_WithOrbitalZoneId_ReturnsExtremeProperties()
    {
        // Act
        var props = _asgard.GetEffectiveProperties("asgard-orbital-zone");

        // Assert — Orbital Zone has extreme conditions
        props.TemperatureCelsius.Should().Be(10, "Coldest zone in Asgard");
        props.LightLevel.Should().Be(0.3f, "Dim in orbital ring segments");
        props.HumidityPercent.Should().Be(10, "Very low humidity in orbital zone");
        props.CorrosionRate.Should().Be(0.0f, "No corrosion in temporal stasis");
    }

    [Test]
    public void GetEffectiveProperties_WithNullZoneId_ReturnsBaseProperties()
    {
        // Act
        var props = _asgard.GetEffectiveProperties(null);

        // Assert — null returns base realm properties
        props.TemperatureCelsius.Should().Be(15);
        props.AethericIntensity.Should().Be(1.0f);
        props.LightLevel.Should().Be(0.8f);
        props.CorrosionRate.Should().Be(0.0f);
    }

    [Test]
    public void GetEffectiveCondition_WithAnyAsgardZoneId_ReturnsCpsExposure()
    {
        // Act & Assert — all Asgard zones share the CpsExposure condition
        _asgard.GetEffectiveCondition("asgard-shattered-spire")
            .Should().Be(EnvironmentalConditionType.CpsExposure);
        _asgard.GetEffectiveCondition("asgard-heimdallrs-platform")
            .Should().Be(EnvironmentalConditionType.CpsExposure);
        _asgard.GetEffectiveCondition("asgard-odins-archive")
            .Should().Be(EnvironmentalConditionType.CpsExposure);
        _asgard.GetEffectiveCondition("asgard-orbital-zone")
            .Should().Be(EnvironmentalConditionType.CpsExposure);
    }

    [Test]
    public void GetEffectiveCondition_WithNullZoneId_ReturnsPrimaryCondition()
    {
        // Act
        var condition = _asgard.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.CpsExposure);
    }

    [Test]
    public void GetZone_WithValidId_ReturnsCorrectZone()
    {
        // Act
        var zone = _asgard.GetZone("asgard-orbital-zone");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("Orbital Zone");
        zone.ParentRealm.Should().Be(RealmId.Asgard);
    }

    [Test]
    public void GetZone_WithInvalidId_ReturnsNull()
    {
        // Act
        var zone = _asgard.GetZone("nonexistent-zone");

        // Assert
        zone.Should().BeNull("Zone ID does not exist in Asgard");
    }

    [Test]
    public void BaseProperties_IsAethericallyActive_ReturnsTrue()
    {
        // Assert — 1.0 exceeds the 0.6 threshold (maximum possible)
        _asgard.BaseProperties.IsAethericallyActive.Should().BeTrue(
            "Asgard has aetheric intensity of 1.0, the maximum value");
    }

    [Test]
    public void BaseProperties_IsThermalExtreme_ReturnsFalse()
    {
        // Assert — 15°C is within comfortable range
        _asgard.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "15°C is cool but not extreme (range: > 45°C or < -10°C)");
    }

    [Test]
    public void FullName_IncludesSubtitle()
    {
        // Assert
        _asgard.FullName.Should().Be("Asgard — The Shattered Spire");
    }

    [Test]
    public void HasZones_WithFourZones_ReturnsTrue()
    {
        // Assert — Asgard is the only realm with 4 zones
        _asgard.HasZones.Should().BeTrue("Asgard has 4 configured zones");
        _asgard.Zones.Count.Should().Be(4, "Asgard is the only realm with 4 sub-zones");
    }

    [Test]
    public void VerticalRangeDescription_ShowsLowerTrunkToOrbital()
    {
        // Assert — Asgard is the only realm that reaches Orbital (Z +4)
        _asgard.VerticalRangeDescription.Should().Be("LowerTrunk to Orbital");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates an Asgard biome definition with all 4 canonical zones for domain testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Asgard (Deck 01 — The Shattered Spire) features CpsExposure as its primary
    /// environmental condition with maximum aetheric intensity (1.0). It is the only
    /// realm with 4 zones and the only one reaching the Orbital vertical zone (Z +4).
    /// </para>
    /// <para>
    /// The four zones progress in CPS intensity: Shattered Spire (entry, DC 12),
    /// Heimdallr's Platform (Signal zone, DC 14), Odin's Archive (knowledge, DC 18),
    /// and Orbital Zone (extreme, DC 18).
    /// </para>
    /// </remarks>
    private static RealmBiomeDefinition CreateAsgard()
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
                conditionDcModifier: 0),

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
                conditionDcModifier: 2),

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
                conditionDcModifier: 6),

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
                conditionDcModifier: 6)
        };

        return RealmBiomeDefinition.Create(
            RealmId.Asgard,
            "Asgard",
            "The Shattered Spire",
            deckNumber: 1,
            preGlitchFunction: "Command and Control Hub",
            postGlitchState: "Reality-warped command deck haunted by temporal loops and cognitive paradoxes.",
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
            colorPalette: "steel-blue-temporal-gold-void-black-lightning-white");
    }
}
