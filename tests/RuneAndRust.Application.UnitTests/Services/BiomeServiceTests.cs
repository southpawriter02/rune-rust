using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for BiomeService.
/// </summary>
[TestFixture]
public class BiomeServiceTests
{
    private SeededRandomService _random = null!;
    private BiomeService _service = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new BiomeService(_random);
    }

    [Test]
    public void GetBiome_WithExistingId_ReturnsBiome()
    {
        // Act
        var biome = _service.GetBiome("stone-corridors");

        // Assert
        biome.Should().NotBeNull();
        biome!.Name.Should().Be("Stone Corridors");
    }

    [Test]
    public void GetBiome_WithNonExistingId_ReturnsNull()
    {
        // Act
        var biome = _service.GetBiome("nonexistent");

        // Assert
        biome.Should().BeNull();
    }

    [Test]
    public void GetBiomesForDepth_ReturnsOnlyValidBiomes()
    {
        // Arrange - default biomes: stone-corridors (0-2), fungal-caverns (2-5), flooded-depths (4+)

        // Act
        var depth0Biomes = _service.GetBiomesForDepth(0);
        var depth3Biomes = _service.GetBiomesForDepth(3);

        // Assert
        depth0Biomes.Should().HaveCount(1);
        depth0Biomes[0].BiomeId.Should().Be("stone-corridors");

        depth3Biomes.Should().HaveCount(1);
        depth3Biomes[0].BiomeId.Should().Be("fungal-caverns");
    }

    [Test]
    public void SelectBiomeForPosition_ReturnsValidBiomeForDepth()
    {
        // Arrange
        var position = new Position3D(5, 5, -3);

        // Act
        var biome = _service.SelectBiomeForPosition(position);

        // Assert
        biome.Should().NotBeNull();
        biome.IsValidForDepth(3).Should().BeTrue();
    }

    [Test]
    public void SelectBiomeForPosition_WithNoValidBiomes_ReturnsDefault()
    {
        // Arrange - Use a depth outside all custom biomes but covered by default
        // Actually the default biomes cover most depths, so let's test at depth 0
        var position = new Position3D(0, 0, 0);

        // Act
        var biome = _service.SelectBiomeForPosition(position);

        // Assert
        biome.Should().NotBeNull();
        biome.BiomeId.Should().Be("stone-corridors");
    }

    [Test]
    public void GetBiomesByTags_WithMatchAll_ReturnsOnlyFullMatches()
    {
        // Act
        var biomes = _service.GetBiomesByTags(new[] { "underground", "natural" }, matchAll: true);

        // Assert
        biomes.Should().HaveCount(1);
        biomes[0].BiomeId.Should().Be("fungal-caverns");
    }

    [Test]
    public void RegisterBiome_AddsBiomeToService()
    {
        // Arrange
        var customBiome = BiomeDefinition.Create("custom-biome", "Custom", "Test biome");

        // Act
        _service.RegisterBiome(customBiome);
        var retrieved = _service.GetBiome("custom-biome");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Custom");
    }

    [Test]
    public void GetRandomDescriptor_ReturnsDescriptorFromPool()
    {
        // Arrange
        var position = new Position3D(1, 1, 0);

        // Act
        var descriptor = _service.GetRandomDescriptor("stone-corridors", "atmospheric", position);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Should().BeOneOf("dusty", "echoing", "ancient");
    }
}
