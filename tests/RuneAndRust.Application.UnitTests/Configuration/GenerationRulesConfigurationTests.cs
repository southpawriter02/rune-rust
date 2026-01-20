using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;

namespace RuneAndRust.Application.UnitTests.Configuration;

[TestFixture]
public class GenerationRulesConfigurationTests
{
    [Test]
    public void GetBiomeRange_ExistingBiome_ReturnsRange()
    {
        // Arrange
        var config = new GenerationRulesConfiguration
        {
            BiomeTransitionDepths = new Dictionary<string, BiomeDepthRange>
            {
                ["cave"] = new BiomeDepthRange
                {
                    MinDepth = 2,
                    MaxDepth = 7,
                    TransitionProbability = 0.6f
                }
            }
        };

        // Act
        var range = config.GetBiomeRange("cave");

        // Assert
        range.Should().NotBeNull();
        range!.MinDepth.Should().Be(2);
        range.MaxDepth.Should().Be(7);
        range.TransitionProbability.Should().Be(0.6f);
    }

    [Test]
    public void GetBiomeRange_NonExistingBiome_ReturnsNull()
    {
        // Arrange
        var config = new GenerationRulesConfiguration();

        // Act & Assert
        config.GetBiomeRange("volcanic").Should().BeNull();
    }

    [Test]
    public void BiomeDepthRange_IsValidForDepth_WithinRange_ReturnsTrue()
    {
        // Arrange
        var range = new BiomeDepthRange
        {
            MinDepth = 2,
            MaxDepth = 7
        };

        // Act & Assert
        range.IsValidForDepth(4).Should().BeTrue();
    }

    [Test]
    public void BiomeDepthRange_IsValidForDepth_BelowMin_ReturnsFalse()
    {
        // Arrange
        var range = new BiomeDepthRange
        {
            MinDepth = 2,
            MaxDepth = 7
        };

        // Act & Assert
        range.IsValidForDepth(1).Should().BeFalse();
    }

    [Test]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new GenerationRulesConfiguration();

        // Assert
        config.MaxRoomsPerLevel.Should().Be(50);
        config.MinExitsPerRoom.Should().Be(1);
        config.MaxExitsPerRoom.Should().Be(4);
        config.DepthDifficultyMultiplier.Should().Be(0.15f);
        config.DepthLootQualityMultiplier.Should().Be(0.10f);
    }
}
