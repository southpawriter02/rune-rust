using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for BiomeHazard entity.
/// </summary>
[TestFixture]
public class BiomeHazardTests
{
    [Test]
    public void Create_WithValidData_CreatesHazard()
    {
        // Act
        var hazard = BiomeHazard.Create(
            "lava-pool", "Lava Pool", "Molten rock.",
            "fire", 15, HazardTrigger.OnEnter());

        // Assert
        hazard.HazardId.Should().Be("lava-pool");
        hazard.Name.Should().Be("Lava Pool");
        hazard.DamageTypeId.Should().Be("fire");
        hazard.BaseDamage.Should().Be(15);
    }

    [Test]
    public void Create_WithDefaults_HasExpectedDefaults()
    {
        // Act
        var hazard = BiomeHazard.Create(
            "trap", "Trap", "A trap.",
            "physical", 5, HazardTrigger.OnEnter());

        // Assert
        hazard.Persistent.Should().BeTrue();
        hazard.CanDisarm.Should().BeFalse();
        hazard.StatusEffects.Should().BeEmpty();
    }

    [Test]
    public void CanSpawnIn_WithMatchingBiome_ReturnsTrue()
    {
        // Arrange
        var hazard = BiomeHazard.Create(
            "trap", "Trap", "A trap.", "physical", 5,
            HazardTrigger.OnEnter(),
            biomeIds: new[] { "volcanic-caverns", "fungal-caverns" });

        // Act & Assert
        hazard.CanSpawnIn("volcanic-caverns").Should().BeTrue();
        hazard.CanSpawnIn("VOLCANIC-CAVERNS").Should().BeTrue();
    }

    [Test]
    public void CanSpawnIn_WithEmptyBiomes_ReturnsTrue()
    {
        // Arrange - empty biome list means can spawn anywhere
        var hazard = BiomeHazard.Create(
            "trap", "Trap", "A trap.", "physical", 5,
            HazardTrigger.OnEnter());

        // Act & Assert
        hazard.CanSpawnIn("any-biome").Should().BeTrue();
    }
}
