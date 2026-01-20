using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for HazardService.
/// </summary>
[TestFixture]
public class HazardServiceTests
{
    private SeededRandomService _random = null!;
    private HazardService _service = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new HazardService(_random);
    }

    [Test]
    public void GetHazard_ExistingId_ReturnsHazard()
    {
        // Act
        var hazard = _service.GetHazard("lava-pool");

        // Assert
        hazard.Should().NotBeNull();
        hazard!.Name.Should().Be("Lava Pool");
    }

    [Test]
    public void GetHazard_UnknownId_ReturnsNull()
    {
        // Act
        var hazard = _service.GetHazard("nonexistent");

        // Assert
        hazard.Should().BeNull();
    }

    [Test]
    public void GetHazardsForBiome_ReturnsMatchingHazards()
    {
        // Act
        var hazards = _service.GetHazardsForBiome("volcanic-caverns");

        // Assert
        hazards.Should().Contain(h => h.HazardId == "lava-pool");
    }

    [Test]
    public void TriggerHazard_WhenAvoided_ReturnsAvoidedResult()
    {
        // Arrange
        var hazard = _service.GetHazard("lava-pool")!;

        // Act - roll exceeds DC
        var result = _service.TriggerHazard(hazard, "volcanic-caverns", avoidanceRoll: 15);

        // Assert
        result.Triggered.Should().BeFalse();
        result.Avoided.Should().BeTrue();
        result.DamageDealt.Should().Be(0);
    }

    [Test]
    public void TriggerHazard_WhenTriggered_AppliesDamageAndEffects()
    {
        // Arrange
        var hazard = _service.GetHazard("lava-pool")!;

        // Act - roll below DC
        var result = _service.TriggerHazard(hazard, "volcanic-caverns", avoidanceRoll: 5);

        // Assert
        result.Triggered.Should().BeTrue();
        result.DamageDealt.Should().BeGreaterThan(0);
        result.StatusEffectsApplied.Should().Contain("burning");
    }

    [Test]
    public void TryDisarm_CannotDisarm_ReturnsCannotDisarmResult()
    {
        // Arrange
        var hazard = _service.GetHazard("lava-pool")!; // Lava pool can't be disarmed

        // Act
        var result = _service.TryDisarm(hazard, skillRoll: 20);

        // Assert
        result.CannotDisarm.Should().BeTrue();
        result.Success.Should().BeFalse();
    }

    [Test]
    public void TryDisarm_Success_ReturnsSuccessResult()
    {
        // Arrange
        var hazard = _service.GetHazard("bone-trap")!;

        // Act - high roll
        var result = _service.TryDisarm(hazard, skillRoll: 15);

        // Assert
        result.Success.Should().BeTrue();
        result.CriticalFailure.Should().BeFalse();
    }

    [Test]
    public void TryDisarm_CriticalFailure_ReturnsTriggerResult()
    {
        // Arrange
        var hazard = _service.GetHazard("bone-trap")!;

        // Act - roll of 1 = critical failure
        var result = _service.TryDisarm(hazard, skillRoll: 1);

        // Assert
        result.CriticalFailure.Should().BeTrue();
        result.TriggerResult.Should().NotBeNull();
        result.TriggerResult!.DamageDealt.Should().BeGreaterThan(0);
    }
}
