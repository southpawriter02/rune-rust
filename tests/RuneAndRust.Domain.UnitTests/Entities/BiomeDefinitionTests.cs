using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for BiomeDefinition entity.
/// </summary>
[TestFixture]
public class BiomeDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesBiome()
    {
        // Act
        var biome = BiomeDefinition.Create(
            "fungal-caverns",
            "Fungal Caverns",
            "Bioluminescent fungi fill these caves.",
            minDepth: 2,
            maxDepth: 5,
            baseWeight: 80);

        // Assert
        biome.BiomeId.Should().Be("fungal-caverns");
        biome.Name.Should().Be("Fungal Caverns");
        biome.MinDepth.Should().Be(2);
        biome.MaxDepth.Should().Be(5);
        biome.BaseWeight.Should().Be(80);
    }

    [Test]
    public void Create_WithNullOrWhitespaceBiomeId_ThrowsArgumentException()
    {
        // Act
        var act = () => BiomeDefinition.Create("", "Name", "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithMaxDepthLessThanMinDepth_ThrowsArgumentException()
    {
        // Act
        var act = () => BiomeDefinition.Create("test", "Test", "Desc", minDepth: 5, maxDepth: 2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*MaxDepth*MinDepth*");
    }

    [Test]
    public void IsValidForDepth_WithDepthInRange_ReturnsTrue()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", minDepth: 2, maxDepth: 5);

        // Act & Assert
        biome.IsValidForDepth(2).Should().BeTrue();
        biome.IsValidForDepth(3).Should().BeTrue();
        biome.IsValidForDepth(5).Should().BeTrue();
    }

    [Test]
    public void IsValidForDepth_WithDepthBelowMin_ReturnsFalse()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", minDepth: 2, maxDepth: 5);

        // Act & Assert
        biome.IsValidForDepth(1).Should().BeFalse();
        biome.IsValidForDepth(0).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_WithDepthAboveMax_ReturnsFalse()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", minDepth: 2, maxDepth: 5);

        // Act & Assert
        biome.IsValidForDepth(6).Should().BeFalse();
        biome.IsValidForDepth(10).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_WithNoMaxDepth_ReturnsTrueForHighDepth()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", minDepth: 4, maxDepth: null);

        // Act & Assert
        biome.IsValidForDepth(4).Should().BeTrue();
        biome.IsValidForDepth(100).Should().BeTrue();
        biome.IsValidForDepth(3).Should().BeFalse();
    }

    [Test]
    public void HasTag_WithMatchingTag_ReturnsTrue()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", tags: new[] { "underground", "natural" });

        // Act & Assert
        biome.HasTag("underground").Should().BeTrue();
        biome.HasTag("UNDERGROUND").Should().BeTrue(); // Case insensitive
    }

    [Test]
    public void HasTag_WithNoMatchingTag_ReturnsFalse()
    {
        // Arrange
        var biome = BiomeDefinition.Create("test", "Test", "Desc", tags: new[] { "underground" });

        // Act & Assert
        biome.HasTag("aquatic").Should().BeFalse();
    }
}
