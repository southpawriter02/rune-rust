using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for ArchitecturalStyle entity.
/// </summary>
[TestFixture]
public class ArchitecturalStyleTests
{
    [Test]
    public void Create_WithValidParameters_CreatesStyle()
    {
        // Act
        var style = ArchitecturalStyle.Create("rough-hewn", "Rough-Hewn", "Crude tunnels");

        // Assert
        style.StyleId.Should().Be("rough-hewn");
        style.Name.Should().Be("Rough-Hewn");
    }

    [Test]
    public void IsCompatibleWith_EmptyBiomes_ReturnsTrue()
    {
        // Arrange - empty means compatible with all
        var style = ArchitecturalStyle.Create("test", "Test", "Test");

        // Act & Assert
        style.IsCompatibleWith("any-biome").Should().BeTrue();
    }

    [Test]
    public void IsCompatibleWith_MatchingBiome_ReturnsTrue()
    {
        // Arrange
        var style = ArchitecturalStyle.Create("test", "Test", "Test",
            compatibleBiomes: new[] { "fungal-caverns", "flooded-depths" });

        // Act & Assert
        style.IsCompatibleWith("fungal-caverns").Should().BeTrue();
        style.IsCompatibleWith("stone-corridors").Should().BeFalse();
    }

    [Test]
    public void IsValidAtDepth_WithinRange_ReturnsTrue()
    {
        // Arrange
        var style = ArchitecturalStyle.Create("test", "Test", "Test",
            minDepth: 2, maxDepth: 5);

        // Act & Assert
        style.IsValidAtDepth(2).Should().BeTrue();
        style.IsValidAtDepth(5).Should().BeTrue();
        style.IsValidAtDepth(1).Should().BeFalse();
        style.IsValidAtDepth(6).Should().BeFalse();
    }

    [Test]
    public void Create_WithRulesAndDescriptors_StoresThemCorrectly()
    {
        // Act
        var style = ArchitecturalStyle.Create("test", "Test", "Test",
            descriptors: StyleDescriptors.RoughHewn,
            rules: StyleRules.Grand);

        // Assert
        style.Descriptors.Walls.Should().NotBeEmpty();
        style.Rules.MinRoomSize.Should().Be(6);
    }
}
