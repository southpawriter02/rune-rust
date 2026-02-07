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
/// Verification tests for adjacency compatibility matrix rules as they apply
/// to <see cref="TransitionZoneGenerator"/> behavior.
/// </summary>
/// <remarks>
/// These tests validate that the transition zone generator correctly interprets
/// the adjacency compatibility matrix for key realm pairs:
/// <list type="bullet">
/// <item>Compatible (C): Midgard ↔ Vanaheim → transition can be generated</item>
/// <item>RequiresTransition (RT): Midgard ↔ Muspelheim → transition can be generated</item>
/// <item>Incompatible (I): Muspelheim ↔ Niflheim → no transition possible</item>
/// <item>Incompatible (I): Muspelheim ↔ Vanaheim → no transition possible</item>
/// </list>
/// </remarks>
[TestFixture]
public class BiomeAdjacencyVerificationTests
{
    private Mock<IBiomeAdjacencyService> _adjacencyServiceMock = null!;
    private Mock<IRealmBiomeProvider> _biomeProviderMock = null!;
    private TransitionZoneGenerator _sut = null!;

    /// <summary>
    /// Temperate realm baseline properties.
    /// </summary>
    private static readonly RealmBiomeProperties TemperateProperties = new()
    {
        TemperatureCelsius = 18,
        AethericIntensity = 0.3f,
        HumidityPercent = 60,
        LightLevel = 0.7f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.2f
    };

    [SetUp]
    public void SetUp()
    {
        _adjacencyServiceMock = new Mock<IBiomeAdjacencyService>();
        _biomeProviderMock = new Mock<IRealmBiomeProvider>();
        var loggerMock = new Mock<ILogger<TransitionZoneGenerator>>();

        // Configure biome provider with dummy definitions for all tested realms
        foreach (var realm in Enum.GetValues<RealmId>())
        {
            var biome = RealmBiomeDefinition.Create(
                realmId: realm,
                name: realm.ToString(),
                subtitle: "Test",
                deckNumber: (int)realm,
                preGlitchFunction: "Test",
                postGlitchState: "Test",
                baseProperties: TemperateProperties,
                primaryCondition: EnvironmentalConditionType.None,
                minVerticalZone: VerticalZone.GroundLevel,
                maxVerticalZone: VerticalZone.GroundLevel);

            _biomeProviderMock.Setup(p => p.GetBiome(realm)).Returns(biome);
        }

        _sut = new TransitionZoneGenerator(
            _adjacencyServiceMock.Object,
            _biomeProviderMock.Object,
            loggerMock.Object);
    }

    [Test]
    public void GetCompatibility_MidgardVanaheim_ReturnsCompatible()
    {
        // Arrange — Midgard ↔ Vanaheim: Compatible (natural temperate neighbors)
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Midgard, RealmId.Vanaheim))
            .Returns(BiomeCompatibility.Compatible);

        // Act
        var canGenerate = _sut.CanGenerateTransition(RealmId.Midgard, RealmId.Vanaheim);
        var zone = _sut.GenerateTransition(RealmId.Midgard, RealmId.Vanaheim);

        // Assert — compatible realms allow transitions
        canGenerate.Should().BeTrue();
        zone.Should().NotBeNull();
    }

    [Test]
    public void GetCompatibility_MidgardMuspelheim_ReturnsRequiresTransition()
    {
        // Arrange — Midgard ↔ Muspelheim: RequiresTransition (temperate → extreme heat)
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Midgard, RealmId.Muspelheim))
            .Returns(BiomeCompatibility.RequiresTransition);

        // Act
        var canGenerate = _sut.CanGenerateTransition(RealmId.Midgard, RealmId.Muspelheim);
        var zone = _sut.GenerateTransition(RealmId.Midgard, RealmId.Muspelheim);

        // Assert — RequiresTransition realms can generate transition zones
        canGenerate.Should().BeTrue();
        zone.Should().NotBeNull();
    }

    [Test]
    public void GetCompatibility_MuspelheimNiflheim_ReturnsIncompatible()
    {
        // Arrange — Muspelheim ↔ Niflheim: Incompatible (Fire/Ice elemental opposition)
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Muspelheim, RealmId.Niflheim))
            .Returns(BiomeCompatibility.Incompatible);

        // Act
        var canGenerate = _sut.CanGenerateTransition(RealmId.Muspelheim, RealmId.Niflheim);
        var zone = _sut.GenerateTransition(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert — incompatible realms cannot generate transitions
        canGenerate.Should().BeFalse();
        zone.Should().BeNull();
    }

    [Test]
    public void GetCompatibility_MuspelheimVanaheim_ReturnsIncompatible()
    {
        // Arrange — Muspelheim ↔ Vanaheim: Incompatible (Fire/Bio — extreme heat destroys organic)
        _adjacencyServiceMock.Setup(s => s.GetCompatibility(RealmId.Muspelheim, RealmId.Vanaheim))
            .Returns(BiomeCompatibility.Incompatible);

        // Act
        var canGenerate = _sut.CanGenerateTransition(RealmId.Muspelheim, RealmId.Vanaheim);
        var zone = _sut.GenerateTransition(RealmId.Muspelheim, RealmId.Vanaheim);

        // Assert — incompatible realms cannot generate transitions
        canGenerate.Should().BeFalse();
        zone.Should().BeNull();
    }
}
