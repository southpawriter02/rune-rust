using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for BiomeTransition entity.
/// </summary>
[TestFixture]
public class BiomeTransitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesTransition()
    {
        // Act
        var transition = BiomeTransition.Create("stone", "fungal", TransitionStyle.Gradual);

        // Assert
        transition.SourceBiomeId.Should().Be("stone");
        transition.TargetBiomeId.Should().Be("fungal");
        transition.Style.Should().Be(TransitionStyle.Gradual);
    }

    [Test]
    public void AppliesTo_MatchingPair_ReturnsTrue()
    {
        // Arrange
        var transition = BiomeTransition.Create("stone", "fungal");

        // Act & Assert
        transition.AppliesTo("stone", "fungal").Should().BeTrue();
    }

    [Test]
    public void AppliesTo_ReversePair_Bidirectional_ReturnsTrue()
    {
        // Arrange
        var transition = BiomeTransition.Create("stone", "fungal", isBidirectional: true);

        // Act & Assert
        transition.AppliesTo("fungal", "stone").Should().BeTrue();
    }

    [Test]
    public void IsValidAtDepth_WithinRange_ReturnsTrue()
    {
        // Arrange
        var transition = BiomeTransition.Create("stone", "fungal", minDepth: 2, maxDepth: 5);

        // Act & Assert
        transition.IsValidAtDepth(2).Should().BeTrue();
        transition.IsValidAtDepth(5).Should().BeTrue();
        transition.IsValidAtDepth(1).Should().BeFalse();
        transition.IsValidAtDepth(6).Should().BeFalse();
    }
}
