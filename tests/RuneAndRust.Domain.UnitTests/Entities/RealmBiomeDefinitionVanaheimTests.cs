using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Domain-level unit tests for Vanaheim-specific realm biome definition behavior.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate entity-level behavior on the <see cref="RealmBiomeDefinition"/>
/// and <see cref="RealmBiomeZone"/> classes when configured for Vanaheim's MutagenicSpores
/// mechanics. Tests focus on zone lookups, effective properties/conditions, and
/// derived property behavior.
/// </para>
/// <para>
/// Vanaheim features a warm, humid bio-hazard environment with cumulative Corruption
/// from spore exposure. The Canopy Sea zone has a 2x corruption multiplier reflected
/// in its doubled damage dice (2d4 vs. 1d4 in Undergrowth).
/// </para>
/// </remarks>
[TestFixture]
public class RealmBiomeDefinitionVanaheimTests
{
    private RealmBiomeDefinition _vanaheim = null!;

    [SetUp]
    public void SetUp()
    {
        _vanaheim = CreateVanaheim();
    }

    [Test]
    public void GetEffectiveProperties_WithUndergrowthZoneId_ReturnsBaseProperties()
    {
        // Act
        var props = _vanaheim.GetEffectiveProperties("vanaheim-undergrowth");

        // Assert — Undergrowth uses base biome properties
        props.TemperatureCelsius.Should().Be(28, "Undergrowth has base temperature");
        props.HumidityPercent.Should().Be(85, "Base high humidity");
        props.ScaleFactor.Should().Be(1.0f, "Normal scale in Vanaheim");
    }

    [Test]
    public void GetEffectiveProperties_WithCanopySeaZoneId_ReturnsElevatedProperties()
    {
        // Act
        var props = _vanaheim.GetEffectiveProperties("vanaheim-canopy-sea");

        // Assert — Canopy Sea has elevated temperature, humidity, and aetheric intensity
        props.TemperatureCelsius.Should().Be(31, "Hot upper layer (+3°C from base)");
        props.HumidityPercent.Should().Be(95, "Near-saturated humidity in aerial fungal cloud");
        props.AethericIntensity.Should().Be(0.6f, "Elevated aetheric from dense biological activity");
        props.LightLevel.Should().Be(0.4f, "Reduced visibility through dense spore clouds");
    }

    [Test]
    public void GetEffectiveProperties_WithYmirsRootsZoneId_ReturnsDeepCavernProperties()
    {
        // Act
        var props = _vanaheim.GetEffectiveProperties("vanaheim-ymirs-roots");

        // Assert — Ymir's Roots has geothermal warmth and low light
        props.TemperatureCelsius.Should().Be(33, "Geothermal warmth (+5°C from base)");
        props.HumidityPercent.Should().Be(90, "High humidity in deep root caverns");
        props.LightLevel.Should().Be(0.3f, "Low light in underground root systems");
        props.AethericIntensity.Should().Be(0.6f, "Elevated aetheric saturation (+0.1 from base)");
    }

    [Test]
    public void GetEffectiveProperties_WithNullZoneId_ReturnsBaseProperties()
    {
        // Act
        var props = _vanaheim.GetEffectiveProperties(null);

        // Assert
        props.TemperatureCelsius.Should().Be(28);
        props.HumidityPercent.Should().Be(85);
        props.AethericIntensity.Should().Be(0.5f);
    }

    [Test]
    public void GetEffectiveCondition_WithAnyVanaheimZoneId_ReturnsMutagenicSpores()
    {
        // Act & Assert — all Vanaheim zones share the MutagenicSpores condition
        _vanaheim.GetEffectiveCondition("vanaheim-undergrowth")
            .Should().Be(EnvironmentalConditionType.MutagenicSpores);
        _vanaheim.GetEffectiveCondition("vanaheim-canopy-sea")
            .Should().Be(EnvironmentalConditionType.MutagenicSpores);
        _vanaheim.GetEffectiveCondition("vanaheim-ymirs-roots")
            .Should().Be(EnvironmentalConditionType.MutagenicSpores);
    }

    [Test]
    public void GetEffectiveCondition_WithNullZoneId_ReturnsPrimaryCondition()
    {
        // Act
        var condition = _vanaheim.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
    }

    [Test]
    public void GetZone_WithValidId_ReturnsCorrectZone()
    {
        // Act
        var zone = _vanaheim.GetZone("vanaheim-canopy-sea");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Canopy Sea");
        zone.ParentRealm.Should().Be(RealmId.Vanaheim);
    }

    [Test]
    public void GetZone_WithInvalidId_ReturnsNull()
    {
        // Act
        var zone = _vanaheim.GetZone("nonexistent-zone");

        // Assert
        zone.Should().BeNull("Zone ID does not exist in Vanaheim");
    }

    [Test]
    public void BaseProperties_IsThermalExtreme_ReturnsFalse()
    {
        // Assert — 28°C is warm but not extreme (threshold: > 45°C or < -10°C)
        _vanaheim.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "28°C is warm but within comfortable range for tropical environments");
    }

    [Test]
    public void BaseProperties_IsGiantScale_ReturnsFalse()
    {
        // Assert — 1.0 scale is normal
        _vanaheim.BaseProperties.IsGiantScale.Should().BeFalse(
            "Vanaheim has normal 1.0 scale factor");
    }

    [Test]
    public void BaseProperties_IsAethericallyActive_ReturnsFalse()
    {
        // Assert — 0.5 aetheric is below the 0.6 threshold
        _vanaheim.BaseProperties.IsAethericallyActive.Should().BeFalse(
            "0.5 aetheric intensity is below the > 0.6 threshold for IsAethericallyActive");
    }

    [Test]
    public void CanopySeaZone_AethericIntensity_IsActive()
    {
        // Act — Canopy Sea has elevated aetheric at 0.6
        var zone = _vanaheim.GetZone("vanaheim-canopy-sea");

        // Assert — 0.6 is at threshold but IsAethericallyActive requires > 0.6
        zone.Should().NotBeNull();
        zone!.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.IsAethericallyActive.Should().BeFalse(
            "0.6 aetheric intensity is at threshold but IsAethericallyActive requires > 0.6");
    }

    [Test]
    public void FullName_IncludesSubtitle()
    {
        // Assert
        _vanaheim.FullName.Should().Be("Vanaheim — The Overgrown Laboratory");
    }

    [Test]
    public void HasZones_ReturnsTrue()
    {
        // Assert
        _vanaheim.HasZones.Should().BeTrue("Vanaheim has 3 configured zones");
    }

    [Test]
    public void VerticalRangeDescription_ShowsFullRange()
    {
        // Assert
        _vanaheim.VerticalRangeDescription.Should().Be("LowerRoots to Canopy");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates a Vanaheim biome definition with all 3 canonical zones for domain testing.
    /// </summary>
    private static RealmBiomeDefinition CreateVanaheim()
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
                damageOverride: "1d4"),

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
                damageOverride: "2d4"),

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
                damageOverride: "1d6")
        };

        return RealmBiomeDefinition.Create(
            RealmId.Vanaheim,
            "Vanaheim",
            "The Overgrown Laboratory",
            deckNumber: 3,
            preGlitchFunction: "Biodome Agricultural Sector",
            postGlitchState: "Abandoned magical research facility consumed by mutagenic botanical mutations.",
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
            colorPalette: "deep-green-spore-yellow-vine-brown-bioluminescent");
    }
}
