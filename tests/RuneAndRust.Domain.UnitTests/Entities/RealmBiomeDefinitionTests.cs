using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for <see cref="RealmBiomeDefinition"/> entity.
/// </summary>
[TestFixture]
public class RealmBiomeDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Act
        var definition = CreateTestDefinition();

        // Assert
        definition.RealmId.Should().Be(RealmId.Midgard);
        definition.Name.Should().Be("Midgard");
        definition.Subtitle.Should().Be("The Tamed Ruin");
        definition.DeckNumber.Should().Be(4);
        definition.PrimaryCondition.Should().Be(EnvironmentalConditionType.None);
    }

    [Test]
    public void Create_WithInvalidDeckNumber_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 10, // Invalid
            preGlitchFunction: "Test",
            postGlitchState: "Test",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.LowerTrunk);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_WithInvalidVerticalZoneRange_ThrowsArgumentException()
    {
        // Act
        var act = () => RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 4,
            preGlitchFunction: "Test",
            postGlitchState: "Test",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.Canopy, // Higher than max
            maxVerticalZone: VerticalZone.GroundLevel);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetEffectiveProperties_NoZone_ReturnsBaseProperties()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act
        var properties = definition.GetEffectiveProperties(null);

        // Assert
        properties.TemperatureCelsius.Should().Be(18);
        properties.LightLevel.Should().Be(0.7f);
    }

    [Test]
    public void GetEffectiveProperties_ValidZone_ReturnsZoneOverride()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var properties = definition.GetEffectiveProperties("midgard-mires");

        // Assert
        properties.Should().NotBeNull();
        properties!.HumidityPercent.Should().Be(90); // Zone override
    }

    [Test]
    public void GetEffectiveProperties_InvalidZone_ReturnsBaseProperties()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var properties = definition.GetEffectiveProperties("nonexistent-zone");

        // Assert
        properties.TemperatureCelsius.Should().Be(18); // Base properties
    }

    [Test]
    public void GetEffectiveCondition_NoZone_ReturnsPrimaryCondition()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act
        var condition = definition.GetEffectiveCondition(null);

        // Assert
        condition.Should().Be(EnvironmentalConditionType.None);
    }

    [Test]
    public void GetZone_ValidId_ReturnsZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("midgard-greatwood");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Greatwood");
    }

    [Test]
    public void GetZone_CaseInsensitive_ReturnsZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("MIDGARD-GREATWOOD");

        // Assert
        zone.Should().NotBeNull();
    }

    [Test]
    public void FullName_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act & Assert
        definition.FullName.Should().Be("Midgard — The Tamed Ruin");
    }

    [Test]
    public void HasZones_WithZones_ReturnsTrue()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act & Assert
        definition.HasZones.Should().BeTrue();
    }

    [Test]
    public void HasZones_WithoutZones_ReturnsFalse()
    {
        // Arrange
        var definition = CreateTestDefinition();

        // Act & Assert
        definition.HasZones.Should().BeFalse();
    }

    // ── v0.19.1a: Midgard Canonical Zone Tests ──────────────────────────

    [Test]
    public void Create_WithFactionPools_StoresCorrectPools()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var greatwood = definition.GetZone("midgard-greatwood");

        // Assert
        greatwood.Should().NotBeNull();
        greatwood!.FactionPools.Should().NotBeNull();
        greatwood.FactionPools.Should().HaveCount(3);
        greatwood.FactionPools.Should().Contain("blighted-beasts");
        greatwood.FactionPools.Should().Contain("constructs");
        greatwood.FactionPools.Should().Contain("humanoid");
        greatwood.HasFactionPools.Should().BeTrue();
    }

    [Test]
    public void GetZone_GreatwoodById_ReturnsCorrectZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("midgard-greatwood");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Greatwood");
        zone.ParentRealm.Should().Be(RealmId.Midgard);
        zone.ConditionDcModifier.Should().Be(-4);
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
        zone.DamageOverride.Should().Be("1d4");
    }

    [Test]
    public void GetZone_ScarById_ReturnsCorrectZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("midgard-scar");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Asgardian Scar");
        zone.ParentRealm.Should().Be(RealmId.Midgard);
        zone.ConditionDcModifier.Should().Be(2);
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.RealityFlux);
        zone.DamageOverride.Should().Be("2d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.AethericIntensity.Should().Be(0.8f);
        zone.OverrideProperties.CorrosionRate.Should().Be(0.5f);
    }

    [Test]
    public void GetZone_MiresById_ReturnsCorrectZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("midgard-mires");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Souring Mires");
        zone.ParentRealm.Should().Be(RealmId.Midgard);
        zone.ConditionDcModifier.Should().Be(-4);
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.ToxicAtmosphere);
        zone.DamageOverride.Should().Be("1d6");
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.HumidityPercent.Should().Be(90);
        zone.OverrideProperties.CorrosionRate.Should().Be(0.4f);
    }

    [Test]
    public void GetZone_FjordsById_ReturnsCorrectZone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var zone = definition.GetZone("midgard-fjords");

        // Assert
        zone.Should().NotBeNull();
        zone!.Name.Should().Be("The Serpent Fjords");
        zone.ParentRealm.Should().Be(RealmId.Midgard);
        zone.ConditionDcModifier.Should().Be(0);
        zone.OverrideCondition.Should().Be(EnvironmentalConditionType.None);
        zone.OverrideProperties.Should().NotBeNull();
        zone.OverrideProperties!.HumidityPercent.Should().Be(80);
        zone.OverrideProperties.TemperatureCelsius.Should().Be(12);
    }

    [Test]
    public void GetEffectiveCondition_GreatwoodZone_ReturnsMutagenicSpores()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var condition = definition.GetEffectiveCondition("midgard-greatwood");

        // Assert
        condition.Should().Be(EnvironmentalConditionType.MutagenicSpores);
    }

    [Test]
    public void GetEffectiveCondition_ScarZone_ReturnsRealityFlux()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var condition = definition.GetEffectiveCondition("midgard-scar");

        // Assert
        condition.Should().Be(EnvironmentalConditionType.RealityFlux);
    }

    [Test]
    public void GetEffectiveCondition_FjordsZone_ReturnsNone()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var condition = definition.GetEffectiveCondition("midgard-fjords");

        // Assert
        condition.Should().Be(EnvironmentalConditionType.None);
    }

    [Test]
    public void MidgardZones_AllHaveUniqueFactionPools()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();

        // Act
        var allFactionPoolIds = definition.Zones
            .Where(z => z.HasFactionPools)
            .SelectMany(z => z.FactionPools!)
            .Distinct()
            .ToList();

        // Assert — verify all 4 faction pool types are represented
        allFactionPoolIds.Should().Contain("blighted-beasts");
        allFactionPoolIds.Should().Contain("constructs");
        allFactionPoolIds.Should().Contain("humanoid");
        allFactionPoolIds.Should().Contain("forlorn");
    }

    [Test]
    public void MidgardZones_ScarHasHighestEffectiveDc()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();
        const int baseDc = 12;

        // Act
        var scarZone = definition.GetZone("midgard-scar");
        var effectiveDc = scarZone!.GetEffectiveDc(baseDc);

        // Assert — DC 14 = base 12 + modifier +2
        effectiveDc.Should().Be(14);
    }

    [Test]
    public void MidgardZones_GreatwoodHasLowestEffectiveDc()
    {
        // Arrange
        var definition = CreateMidgardWithCanonicalZones();
        const int baseDc = 12;

        // Act
        var greatwoodZone = definition.GetZone("midgard-greatwood");
        var effectiveDc = greatwoodZone!.GetEffectiveDc(baseDc);

        // Assert — DC 8 = base 12 + modifier (-4)
        effectiveDc.Should().Be(8);
    }

    // ── Helper Methods ──────────────────────────────────────────────────

    private static RealmBiomeDefinition CreateTestDefinition()
    {
        return RealmBiomeDefinition.Create(
            RealmId.Midgard,
            "Midgard",
            "The Tamed Ruin",
            deckNumber: 4,
            preGlitchFunction: "Civilian Habitation",
            postGlitchState: "Agricultural heartland",
            baseProperties: RealmBiomeProperties.Temperate(),
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.LowerTrunk);
    }

    /// <summary>
    /// Creates a Midgard definition with all 4 canonical lore zones.
    /// </summary>
    private static RealmBiomeDefinition CreateMidgardWithCanonicalZones()
    {
        var zones = new List<RealmBiomeZone>
        {
            // The Greatwood — Feral forest, MutagenicSpores (light)
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

            // The Asgardian Scar — Blight epicenter, RealityFlux
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

            // The Souring Mires — Toxic wetland, ToxicAtmosphere (light)
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

            // The Serpent Fjords — Poisoned coast, no ambient condition
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

        return RealmBiomeDefinition.Create(
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
            zones: zones,
            flavorQuote: "The Wall is the soul of the Hold.",
            colorPalette: "green-brown-gray-rust");
    }
}
