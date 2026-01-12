using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for TransitionBlend value object.
/// </summary>
[TestFixture]
public class TransitionBlendTests
{
    [Test]
    public void ForPosition_WithHalfRatio_CreatesBlend()
    {
        // Act
        var blend = TransitionBlend.ForPosition("stone", "fungal", 0.5f);

        // Assert
        blend.SourceBiomeId.Should().Be("stone");
        blend.TargetBiomeId.Should().Be("fungal");
        blend.Ratio.Should().Be(0.5f);
    }

    [Test]
    public void Pure_CreatesPureBlend()
    {
        // Act
        var blend = TransitionBlend.Pure("stone");

        // Assert
        blend.IsPure.Should().BeTrue();
        blend.DominantBiome.Should().Be("stone");
    }

    [Test]
    public void IsPure_WithZeroRatio_ReturnsTrue()
    {
        // Act
        var blend = TransitionBlend.ForPosition("stone", "fungal", 0f);

        // Assert
        blend.IsPure.Should().BeTrue();
    }

    [Test]
    public void DominantBiome_WithHighRatio_ReturnsTarget()
    {
        // Act
        var blend = TransitionBlend.ForPosition("stone", "fungal", 0.7f);

        // Assert
        blend.DominantBiome.Should().Be("fungal");
    }

    [Test]
    public void DominantBiome_WithLowRatio_ReturnsSource()
    {
        // Act
        var blend = TransitionBlend.ForPosition("stone", "fungal", 0.3f);

        // Assert
        blend.DominantBiome.Should().Be("stone");
    }
}
