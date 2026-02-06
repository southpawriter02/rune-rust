using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Domain-level unit tests for Jötunheim-specific realm biome definition behavior.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate entity-level behavior on the <see cref="RealmBiomeDefinition"/>
/// and <see cref="RealmBiomeZone"/> classes when configured for Jötunheim's GiantScale
/// mechanics. Tests focus on zone lookups, effective properties/conditions, and
/// derived property behavior.
/// </para>
/// <para>
/// Jötunheim is unique in the Nine Realms system for having a scale factor of 3.0,
/// making it the first realm where <see cref="RealmBiomeProperties.IsGiantScale"/>
/// returns true.
/// </para>
/// </remarks>
[TestFixture]
public class RealmBiomeDefinitionJotunheimTests
{
    private RealmBiomeDefinition _jotunheim = null!;

    [SetUp]
    public void SetUp()
    {
        _jotunheim = CreateJotunheim();
    }

    [Test]
    public void GetEffectiveProperties_WithBoneYardsZoneId_ReturnsZoneOverrides()
    {
        // Act
        var props = _jotunheim.GetEffectiveProperties("jotunheim-bone-yards");

        // Assert — Bone Yards inherits base properties
        props.TemperatureCelsius.Should().Be(10, "Bone Yards has base temperature");
        props.ScaleFactor.Should().Be(3.0f, "GiantScale 3.0 applies in all Jötunheim zones");
    }

    [Test]
    public void GetEffectiveProperties_WithUtgardsShadowZoneId_ReturnsColdShadowProperties()
    {
        // Act
        var props = _jotunheim.GetEffectiveProperties("jotunheim-utgards-shadow");

        // Assert — Utgard's Shadow has reduced temperature and light
        props.TemperatureCelsius.Should().Be(8, "Shadow zones are colder (-2°C from base)");
        props.LightLevel.Should().Be(0.3f, "Massive structures cast deep shadows");
        props.CorrosionRate.Should().Be(0.7f, "Corroded machinery increases environmental corrosion");
    }

    [Test]
    public void GetEffectiveProperties_WithGrindingHallZoneId_ReturnsExtremeProperties()
    {
        // Act
        var props = _jotunheim.GetEffectiveProperties("jotunheim-grinding-hall");

        // Assert — Grinding Hall has extreme conditions
        props.TemperatureCelsius.Should().Be(5, "Deep industrial zone is coldest");
        props.LightLevel.Should().Be(0.2f, "Near darkness in machinery chamber");
        props.CorrosionRate.Should().Be(0.8f, "Maximum corrosion from active grinding mechanisms");
    }

    [Test]
    public void GetEffectiveProperties_WithNullZoneId_ReturnsBaseProperties()
    {
        // Act
        var props = _jotunheim.GetEffectiveProperties(null);

        // Assert — null returns base realm properties
        props.TemperatureCelsius.Should().Be(10);
        props.ScaleFactor.Should().Be(3.0f);
        props.CorrosionRate.Should().Be(0.5f);
    }

    [Test]
    public void GetEffectiveCondition_WithAnyJotunheimZoneId_ReturnsGiantScale()
    {
        // Act & Assert — all Jötunheim zones share the GiantScale condition
        _jotunheim.GetEffectiveCondition("jotunheim-bone-yards")
            .Should().Be(EnvironmentalConditionType.GiantScale);
        _jotunheim.GetEffectiveCondition("jotunheim-utgards-shadow")
            .Should().Be(EnvironmentalConditionType.GiantScale);
        _jotunheim.GetEffectiveCondition("jotunheim-grinding-hall")
            .Should().Be(EnvironmentalConditionType.GiantScale);
    }

    [Test]
    public void GetEffectiveCondition_WithNullZoneId_ReturnsPrimaryCondition()
    {
        // Act
        var condition = _jotunheim.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.GiantScale);
    }

    [Test]
    public void GetZone_WithValidId_ReturnsCorrectZone()
    {
        // Act
        var zone = _jotunheim.GetZone("jotunheim-grinding-hall");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Grinding Hall");
        zone.ParentRealm.Should().Be(RealmId.Jotunheim);
    }

    [Test]
    public void GetZone_WithInvalidId_ReturnsNull()
    {
        // Act
        var zone = _jotunheim.GetZone("nonexistent-zone");

        // Assert
        zone.Should().BeNull("Zone ID does not exist in Jötunheim");
    }

    [Test]
    public void BaseProperties_IsGiantScale_ReturnsTrue()
    {
        // Assert — 3.0 scale exceeds the 2.0 threshold
        _jotunheim.BaseProperties.IsGiantScale.Should().BeTrue(
            "Jötunheim has a scale factor of 3.0, exceeding the 2.0 IsGiantScale threshold");
    }

    [Test]
    public void BaseProperties_IsThermalExtreme_ReturnsFalse()
    {
        // Assert — 10°C is within comfortable range
        _jotunheim.BaseProperties.IsThermalExtreme.Should().BeFalse(
            "10°C is cool but not extreme (range: > 45°C or < -10°C)");
    }

    [Test]
    public void FullName_IncludesSubtitle()
    {
        // Assert
        _jotunheim.FullName.Should().Be("Jötunheim — The Industrial Graveyard");
    }

    [Test]
    public void HasZones_ReturnsTrue()
    {
        // Assert
        _jotunheim.HasZones.Should().BeTrue("Jötunheim has 3 configured zones");
    }

    [Test]
    public void VerticalRangeDescription_ShowsFullRange()
    {
        // Assert
        _jotunheim.VerticalRangeDescription.Should().Be("UpperRoots to Canopy");
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Creates a Jötunheim biome definition with all 3 canonical zones for domain testing.
    /// </summary>
    private static RealmBiomeDefinition CreateJotunheim()
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
                conditionDcModifier: -2),

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
                conditionDcModifier: 0),

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
                damageOverride: "2d6")
        };

        return RealmBiomeDefinition.Create(
            RealmId.Jotunheim,
            "Jötunheim",
            "The Industrial Graveyard",
            deckNumber: 7,
            preGlitchFunction: "Megafauna Habitats and Ecological Reserves",
            postGlitchState: "Colossal graveyard of broken machinery and giant remains.",
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
            flavorQuote: "The bones of giants grind beneath your feet.",
            colorPalette: "rust-iron-gray-bone-white-dark-green");
    }
}
