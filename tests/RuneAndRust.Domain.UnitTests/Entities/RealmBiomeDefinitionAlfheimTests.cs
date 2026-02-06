using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Domain-level unit tests for Alfheim-specific realm biome definition behavior.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate entity-level behavior on the <see cref="RealmBiomeDefinition"/>
/// and <see cref="RealmBiomeZone"/> classes when configured for Alfheim's RealityFlux
/// mechanics. Tests focus on zone lookups, effective properties/conditions, and
/// derived property behavior.
/// </para>
/// <para>
/// Alfheim (Deck 02 — The Glimmering Wound) is unique for its blinding light (1.0),
/// very high aetheric intensity (0.9), and RealityFlux as primary condition. It is
/// the first realm where <see cref="RealmBiomeProperties.IsAethericallyActive"/>
/// returns true at the base level.
/// </para>
/// </remarks>
[TestFixture]
public class RealmBiomeDefinitionAlfheimTests
{
    private RealmBiomeDefinition _alfheim = null!;

    [SetUp]
    public void SetUp()
    {
        _alfheim = CreateAlfheim();
    }

    [Test]
    public void GetEffectiveProperties_WithLuminousWasteZoneId_ReturnsZoneOverrides()
    {
        // Act
        var props = _alfheim.GetEffectiveProperties("alfheim-luminous-waste");

        // Assert — Luminous Waste is the entry zone with slightly reduced aetheric
        props.TemperatureCelsius.Should().Be(22, "Luminous Waste has base temperature");
        props.AethericIntensity.Should().Be(0.85f, "Slightly reduced from base 0.9");
        props.LightLevel.Should().Be(1.0f, "Blinding light across the Waste");
    }

    [Test]
    public void GetEffectiveProperties_WithPrismaticMazeZoneId_ReturnsHighDifficultyProperties()
    {
        // Act
        var props = _alfheim.GetEffectiveProperties("alfheim-prismatic-maze");

        // Assert — Prismatic Maze has elevated aetheric and increased corrosion
        props.AethericIntensity.Should().Be(0.95f, "Near-maximum aetheric in the maze");
        props.LightLevel.Should().Be(0.9f, "Slightly reduced light from refraction");
        props.CorrosionRate.Should().Be(0.2f, "Doubled corrosion from reality fracturing");
    }

    [Test]
    public void GetEffectiveProperties_WithDreamingCoreZoneId_ReturnsMaxAethericProperties()
    {
        // Act
        var props = _alfheim.GetEffectiveProperties("alfheim-dreaming-core");

        // Assert — Dreaming Core has maximum aetheric and reduced light
        props.AethericIntensity.Should().Be(1.0f, "Maximum aetheric saturation in the Core");
        props.LightLevel.Should().Be(0.7f, "Light dims as reality collapses");
        props.CorrosionRate.Should().Be(0.3f, "Highest corrosion rate in Alfheim");
    }

    [Test]
    public void GetEffectiveProperties_WithNullZoneId_ReturnsBaseProperties()
    {
        // Act
        var props = _alfheim.GetEffectiveProperties(null);

        // Assert — null returns base realm properties
        props.TemperatureCelsius.Should().Be(22);
        props.AethericIntensity.Should().Be(0.9f);
        props.LightLevel.Should().Be(1.0f);
        props.CorrosionRate.Should().Be(0.1f);
    }

    [Test]
    public void GetEffectiveCondition_WithAnyAlfheimZoneId_ReturnsRealityFlux()
    {
        // Act & Assert — all Alfheim zones share the RealityFlux condition
        _alfheim.GetEffectiveCondition("alfheim-luminous-waste")
            .Should().Be(EnvironmentalConditionType.RealityFlux);
        _alfheim.GetEffectiveCondition("alfheim-prismatic-maze")
            .Should().Be(EnvironmentalConditionType.RealityFlux);
        _alfheim.GetEffectiveCondition("alfheim-dreaming-core")
            .Should().Be(EnvironmentalConditionType.RealityFlux);
    }

    [Test]
    public void GetEffectiveCondition_WithNullZoneId_ReturnsPrimaryCondition()
    {
        // Act
        var condition = _alfheim.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.RealityFlux);
    }

    [Test]
    public void GetZone_WithValidId_ReturnsCorrectZone()
    {
        // Act
        var zone = _alfheim.GetZone("alfheim-dreaming-core");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Dreaming Core");
        zone.ParentRealm.Should().Be(RealmId.Alfheim);
    }

    [Test]
    public void GetZone_WithInvalidId_ReturnsNull()
    {
        // Act
        var zone = _alfheim.GetZone("nonexistent-zone");

        // Assert
        zone.Should().BeNull("Zone ID does not exist in Alfheim");
    }

    [Test]
    public void BaseProperties_IsAethericallyActive_ReturnsTrue()
    {
        // Assert — 0.9 exceeds the 0.6 threshold
        _alfheim.BaseProperties.IsAethericallyActive.Should().BeTrue(
            "Alfheim has aetheric intensity of 0.9, exceeding the 0.6 IsAethericallyActive threshold");
    }

    [Test]
    public void BaseProperties_IsThermalExtreme_ReturnsFalse()
    {
        // Assert — 22°C is well within comfortable range
        _alfheim.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "22°C is temperate (range: > 45°C or < -10°C)");
    }

    [Test]
    public void BaseProperties_IsGiantScale_ReturnsFalse()
    {
        // Assert — scale factor 1.0 is normal
        _alfheim.BaseProperties.IsGiantScale.Should().BeFalse(
            "Scale factor 1.0 does not exceed the 2.0 IsGiantScale threshold");
    }

    [Test]
    public void FullName_IncludesSubtitle()
    {
        // Assert
        _alfheim.FullName.Should().Be("Alfheim — The Glimmering Wound");
    }

    [Test]
    public void HasZones_ReturnsTrue()
    {
        // Assert
        _alfheim.HasZones.Should().BeTrue("Alfheim has 3 configured zones");
    }

    [Test]
    public void VerticalRangeDescription_ShowsFullRange()
    {
        // Assert
        _alfheim.VerticalRangeDescription.Should().Be("GroundLevel to Canopy");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates an Alfheim biome definition with all 3 canonical zones for domain testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alfheim (Deck 02 — The Glimmering Wound) features RealityFlux as its primary
    /// environmental condition with blinding light (1.0) and very high aetheric
    /// intensity (0.9). It is the primary cognitive hazard realm for reality distortion.
    /// </para>
    /// <para>
    /// The three zones progress in aetheric intensity: Luminous Waste (entry, DC 10),
    /// Prismatic Maze (difficult, DC 16), and Dreaming Core (deep, DC 14).
    /// </para>
    /// </remarks>
    private static RealmBiomeDefinition CreateAlfheim()
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
                conditionDcModifier: -2),

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
                conditionDcModifier: 4),

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
                conditionDcModifier: 2)
        };

        return RealmBiomeDefinition.Create(
            RealmId.Alfheim,
            "Alfheim",
            "The Glimmering Wound",
            deckNumber: 2,
            preGlitchFunction: "Aetheric Research Laboratories",
            postGlitchState: "Bioluminescent wasteland saturated with reality-warping aetheric radiation.",
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
            colorPalette: "blinding-white-prismatic-iridescent-pale-gold-ghostly-blue");
    }
}
