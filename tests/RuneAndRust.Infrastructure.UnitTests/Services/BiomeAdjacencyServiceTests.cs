using FluentAssertions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Infrastructure.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="BiomeAdjacencyService"/> implementation.
/// </summary>
[TestFixture]
public class BiomeAdjacencyServiceTests
{
    private Mock<ILogger<BiomeAdjacencyService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<BiomeAdjacencyService>>();
    }

    [Test]
    public void CanBiomesNeighbor_CompatibleRealms_ReturnsTrue()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.CanBiomesNeighbor(RealmId.Midgard, RealmId.Vanaheim);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanBiomesNeighbor_RequiresTransitionRealms_ReturnsTrue()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.CanBiomesNeighbor(RealmId.Midgard, RealmId.Alfheim);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanBiomesNeighbor_IncompatibleRealms_ReturnsFalse()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.CanBiomesNeighbor(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CanBiomesNeighbor_SameRealm_ReturnsFalse()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.CanBiomesNeighbor(RealmId.Midgard, RealmId.Midgard);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetCompatibility_FireIce_ReturnsIncompatible()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.GetCompatibility(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        result.Should().Be(BiomeCompatibility.Incompatible);
    }

    [Test]
    public void GetCompatibility_FireBio_ReturnsIncompatible()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.GetCompatibility(RealmId.Muspelheim, RealmId.Vanaheim);

        // Assert
        result.Should().Be(BiomeCompatibility.Incompatible);
    }

    [Test]
    public void GetCompatibility_SameRealm_ReturnsCompatible()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.GetCompatibility(RealmId.Midgard, RealmId.Midgard);

        // Assert
        result.Should().Be(BiomeCompatibility.Compatible);
    }

    [Test]
    public void GetTransitionRoomCount_RequiresTransition_ReturnsRange()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var (min, max) = service.GetTransitionRoomCount(RealmId.Midgard, RealmId.Alfheim);

        // Assert
        min.Should().Be(1);
        max.Should().Be(2);
    }

    [Test]
    public void GetTransitionRoomCount_Compatible_ReturnsZero()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var (min, max) = service.GetTransitionRoomCount(RealmId.Midgard, RealmId.Vanaheim);

        // Assert
        min.Should().Be(0);
        max.Should().Be(0);
    }

    [Test]
    public void GetTransitionRoomCount_Incompatible_ThrowsException()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var act = () => service.GetTransitionRoomCount(RealmId.Muspelheim, RealmId.Niflheim);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*incompatible*");
    }

    [Test]
    public void GetTransitionTheme_HasTheme_ReturnsTheme()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.GetTransitionTheme(RealmId.Midgard, RealmId.Vanaheim);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("River");
    }

    [Test]
    public void GetAdjacentRealms_Midgard_ReturnsExpectedList()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);

        // Act
        var result = service.GetAdjacentRealms(RealmId.Midgard);

        // Assert
        result.Should().Contain(RealmId.Vanaheim);
        result.Should().Contain(RealmId.Alfheim);
        result.Should().NotContain(RealmId.Midgard);
    }

    [Test]
    public void ValidateRealmConfiguration_ValidPairs_ReturnsTrue()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);
        var pairs = new List<(RealmId, RealmId)>
        {
            (RealmId.Midgard, RealmId.Vanaheim),
            (RealmId.Midgard, RealmId.Alfheim)
        };

        // Act
        var result = service.ValidateRealmConfiguration(pairs);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValidateRealmConfiguration_IncompatiblePair_ReturnsFalse()
    {
        // Arrange
        var rules = CreateTestRules();
        var service = CreateService(rules);
        var pairs = new List<(RealmId, RealmId)>
        {
            (RealmId.Muspelheim, RealmId.Niflheim)
        };

        // Act
        var result = service.ValidateRealmConfiguration(pairs);

        // Assert
        result.Should().BeFalse();
    }

    private BiomeAdjacencyService CreateService(IEnumerable<AdjacencyRule> rules)
    {
        return new BiomeAdjacencyService(rules, _loggerMock.Object);
    }

    private static List<AdjacencyRule> CreateTestRules()
    {
        return
        [
            // Incompatible pairs
            AdjacencyRule.Incompatible(RealmId.Muspelheim, RealmId.Niflheim),
            AdjacencyRule.Incompatible(RealmId.Muspelheim, RealmId.Vanaheim),

            // Compatible pairs
            AdjacencyRule.Compatible(
                RealmId.Midgard,
                RealmId.Vanaheim,
                "River ferries upstream... canopy shadows deepen..."),

            // Transition required
            AdjacencyRule.WithTransition(
                RealmId.Midgard,
                RealmId.Alfheim,
                minRooms: 1,
                maxRooms: 2,
                "Reality shifts... the air thickens with aetheric energy...")
        ];
    }
}
