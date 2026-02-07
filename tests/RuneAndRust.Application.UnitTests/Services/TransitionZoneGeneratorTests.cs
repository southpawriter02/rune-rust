using FluentAssertions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="TransitionZoneGenerator"/> service.
/// </summary>
/// <remarks>
/// Tests cover transition zone generation, property interpolation,
/// compatibility checking, and multi-zone sequence generation.
/// </remarks>
[TestFixture]
public class TransitionZoneGeneratorTests
{
    private Mock<IBiomeAdjacencyService> _adjacencyServiceMock = null!;
    private Mock<IRealmBiomeProvider> _biomeProviderMock = null!;
    private Mock<ILogger<TransitionZoneGenerator>> _loggerMock = null!;
    private TransitionZoneGenerator _sut = null!;

    /// <summary>
    /// Properties representing Midgard — temperate, low danger.
    /// </summary>
    private static readonly RealmBiomeProperties MidgardProperties = new()
    {
        TemperatureCelsius = 18,
        AethericIntensity = 0.3f,
        HumidityPercent = 60,
        LightLevel = 0.7f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.2f
    };

    /// <summary>
    /// Properties representing Vanaheim — slightly warmer, more humid.
    /// </summary>
    private static readonly RealmBiomeProperties VanaheimProperties = new()
    {
        TemperatureCelsius = 28,
        AethericIntensity = 0.5f,
        HumidityPercent = 80,
        LightLevel = 0.6f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.3f
    };

    /// <summary>
    /// Properties representing Muspelheim — extreme heat, volcanic.
    /// </summary>
    private static readonly RealmBiomeProperties MuspelheimProperties = new()
    {
        TemperatureCelsius = 200,
        AethericIntensity = 0.6f,
        HumidityPercent = 10,
        LightLevel = 0.9f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.8f
    };

    /// <summary>
    /// Properties representing Niflheim — extreme cold, frozen.
    /// </summary>
    private static readonly RealmBiomeProperties NiflheimProperties = new()
    {
        TemperatureCelsius = -40,
        AethericIntensity = 0.4f,
        HumidityPercent = 30,
        LightLevel = 0.2f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.5f
    };

    /// <summary>
    /// Properties representing Jotunheim — cold, giant-scaled.
    /// </summary>
    private static readonly RealmBiomeProperties JotunheimProperties = new()
    {
        TemperatureCelsius = -15,
        AethericIntensity = 0.4f,
        HumidityPercent = 40,
        LightLevel = 0.5f,
        ScaleFactor = 5.0f,
        CorrosionRate = 0.3f
    };

    [SetUp]
    public void SetUp()
    {
        _adjacencyServiceMock = new Mock<IBiomeAdjacencyService>();
        _biomeProviderMock = new Mock<IRealmBiomeProvider>();
        _loggerMock = new Mock<ILogger<TransitionZoneGenerator>>();

        // Default biome definitions for common realms
        SetupBiome(RealmId.Midgard, "Midgard", "The Tamed Ruin", MidgardProperties);
        SetupBiome(RealmId.Vanaheim, "Vanaheim", "The Strangling Green", VanaheimProperties);
        SetupBiome(RealmId.Muspelheim, "Muspelheim", "The Burning Caldera", MuspelheimProperties);
        SetupBiome(RealmId.Niflheim, "Niflheim", "The Frozen Deeps", NiflheimProperties);
        SetupBiome(RealmId.Jotunheim, "Jotunheim", "The Land of Giants", JotunheimProperties);

        // Default compatibility: Midgard-Vanaheim=Compatible, Muspelheim-Niflheim=Incompatible
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Midgard, RealmId.Vanaheim))
            .Returns(BiomeCompatibility.Compatible);
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Midgard, RealmId.Jotunheim))
            .Returns(BiomeCompatibility.RequiresTransition);
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Muspelheim, RealmId.Niflheim))
            .Returns(BiomeCompatibility.Incompatible);
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Muspelheim, RealmId.Vanaheim))
            .Returns(BiomeCompatibility.Incompatible);

        // Default transition themes
        _adjacencyServiceMock.Setup(s => s.GetTransitionTheme(RealmId.Midgard, RealmId.Vanaheim))
            .Returns("River ferries upstream... canopy shadows deepen...");

        _sut = new TransitionZoneGenerator(
            _adjacencyServiceMock.Object,
            _biomeProviderMock.Object,
            _loggerMock.Object);
    }

    // ──────────────────────────────────────────────────────────────
    // GenerateTransition Tests
    // ──────────────────────────────────────────────────────────────

    [Test]
    public void GenerateTransition_CompatibleRealms_ReturnsValidZone()
    {
        // Act
        var result = _sut.GenerateTransition(RealmId.Midgard, RealmId.Vanaheim);

        // Assert
        result.Should().NotBeNull();
        result!.FromRealm.Should().Be(RealmId.Midgard);
        result.ToRealm.Should().Be(RealmId.Vanaheim);
        result.SequenceIndex.Should().Be(0);
        result.BlendFactor.Should().BeApproximately(0.5, 0.001);
    }

    [Test]
    public void GenerateTransition_IncompatibleRealms_ReturnsNull()
    {
        // Act
        var result = _sut.GenerateTransition(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GenerateTransition_SameRealm_ReturnsNull()
    {
        // Act
        var result = _sut.GenerateTransition(RealmId.Midgard, RealmId.Midgard);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GenerateTransition_HasBlendedProperties()
    {
        // Act
        var result = _sut.GenerateTransition(RealmId.Midgard, RealmId.Vanaheim);

        // Assert — interpolated at 50% between Midgard(18°C) and Vanaheim(28°C)
        result.Should().NotBeNull();
        result!.InterpolatedProperties.TemperatureCelsius.Should().Be(23); // (18+28)/2
        result.InterpolatedProperties.HumidityPercent.Should().Be(70); // (60+80)/2
        result.InterpolatedProperties.AethericIntensity.Should().BeApproximately(0.4f, 0.01f); // (0.3+0.5)/2
    }

    [Test]
    public void GenerateTransition_AppliesTransitionTheme()
    {
        // Act
        var result = _sut.GenerateTransition(RealmId.Midgard, RealmId.Vanaheim);

        // Assert
        result.Should().NotBeNull();
        result!.TransitionTheme.Should().NotBeNullOrEmpty();
        result.TransitionTheme.Should().Contain("River");
    }

    // ──────────────────────────────────────────────────────────────
    // InterpolateProperties Tests
    // ──────────────────────────────────────────────────────────────

    [Test]
    public void InterpolateProperties_FiftyPercentBlend_ReturnsAverage()
    {
        // Arrange — Midgard (18°C, 60% humidity) ↔ Vanaheim (28°C, 80% humidity)

        // Act
        var result = _sut.InterpolateProperties(MidgardProperties, VanaheimProperties, 0.5);

        // Assert — midpoint: 23°C, 70% humidity
        result.TemperatureCelsius.Should().Be(23);
        result.HumidityPercent.Should().Be(70);
        result.AethericIntensity.Should().BeApproximately(0.4f, 0.01f);
        result.LightLevel.Should().BeApproximately(0.65f, 0.01f);
        result.ScaleFactor.Should().BeApproximately(1.0f, 0.01f);
        result.CorrosionRate.Should().BeApproximately(0.25f, 0.01f);
    }

    // ──────────────────────────────────────────────────────────────
    // GenerateTransitionSequence Tests
    // ──────────────────────────────────────────────────────────────

    [Test]
    public void GenerateTransitionSequence_ThreeRooms_ReturnsThreeZones()
    {
        // Arrange
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Midgard, RealmId.Jotunheim))
            .Returns(BiomeCompatibility.RequiresTransition);

        // Act
        var result = _sut.GenerateTransitionSequence(RealmId.Midgard, RealmId.Jotunheim, 3);

        // Assert — 3 zones with blends at 0.25, 0.50, 0.75
        result.Should().HaveCount(3);
        result[0].BlendFactor.Should().BeApproximately(0.25, 0.01);
        result[1].BlendFactor.Should().BeApproximately(0.50, 0.01);
        result[2].BlendFactor.Should().BeApproximately(0.75, 0.01);

        // Verify sequence indices
        result[0].SequenceIndex.Should().Be(0);
        result[1].SequenceIndex.Should().Be(1);
        result[2].SequenceIndex.Should().Be(2);

        // Verify realms are consistent
        result.Should().AllSatisfy(z =>
        {
            z.FromRealm.Should().Be(RealmId.Midgard);
            z.ToRealm.Should().Be(RealmId.Jotunheim);
        });
    }

    [Test]
    public void GenerateTransitionSequence_RespectRoomCountLimits()
    {
        // Act — roomCount below 1 should throw
        var actBelow = () => _sut.GenerateTransitionSequence(RealmId.Midgard, RealmId.Jotunheim, 0);
        actBelow.Should().Throw<ArgumentOutOfRangeException>();

        // Act — roomCount above 3 should throw
        var actAbove = () => _sut.GenerateTransitionSequence(RealmId.Midgard, RealmId.Jotunheim, 4);
        actAbove.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ──────────────────────────────────────────────────────────────
    // CanGenerateTransition Tests
    // ──────────────────────────────────────────────────────────────

    [Test]
    public void CanGenerateTransition_MuspelheimNiflheim_ReturnsFalse()
    {
        // Act
        var result = _sut.CanGenerateTransition(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CanGenerateTransition_MuspelheimVanaheim_ReturnsFalse()
    {
        // Act
        var result = _sut.CanGenerateTransition(RealmId.Muspelheim, RealmId.Vanaheim);

        // Assert
        result.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────
    // Helper Methods
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets up a mock biome definition for a given realm.
    /// </summary>
    private void SetupBiome(
        RealmId realmId,
        string name,
        string subtitle,
        RealmBiomeProperties properties)
    {
        var biome = RealmBiomeDefinition.Create(
            realmId: realmId,
            name: name,
            subtitle: subtitle,
            deckNumber: (int)realmId,
            preGlitchFunction: "Test function",
            postGlitchState: "Test state",
            baseProperties: properties,
            primaryCondition: EnvironmentalConditionType.None,
            minVerticalZone: VerticalZone.GroundLevel,
            maxVerticalZone: VerticalZone.GroundLevel);

        _biomeProviderMock.Setup(p => p.GetBiome(realmId)).Returns(biome);
    }
}
