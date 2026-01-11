using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;

namespace RuneAndRust.Application.UnitTests.Configuration;

[TestFixture]
public class RoomTemplateTests
{
    [Test]
    public void IsValidForDepth_WithinRange_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinDepth = 2,
            MaxDepth = 5
        };

        // Act & Assert
        template.IsValidForDepth(3).Should().BeTrue();
    }

    [Test]
    public void IsValidForDepth_BelowMinDepth_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinDepth = 2,
            MaxDepth = 5
        };

        // Act & Assert
        template.IsValidForDepth(1).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_AboveMaxDepth_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinDepth = 2,
            MaxDepth = 5
        };

        // Act & Assert
        template.IsValidForDepth(6).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_NoMaxDepth_AcceptsAnyDepthAboveMin()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinDepth = 2,
            MaxDepth = -1 // No limit
        };

        // Act & Assert
        template.IsValidForDepth(100).Should().BeTrue();
    }

    [Test]
    public void GetExitProbability_ExistingDirection_ReturnsProbability()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ExitProbabilities = new Dictionary<string, float>
            {
                ["north"] = 0.75f
            }
        };

        // Act & Assert
        template.GetExitProbability("north").Should().Be(0.75f);
    }

    [Test]
    public void GetExitProbability_CaseInsensitive_ReturnsProbability()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ExitProbabilities = new Dictionary<string, float>
            {
                ["north"] = 0.75f
            }
        };

        // Act & Assert
        template.GetExitProbability("NORTH").Should().Be(0.75f);
    }

    [Test]
    public void GetExitProbability_MissingDirection_ReturnsZero()
    {
        // Arrange
        var template = new RoomTemplate();

        // Act & Assert
        template.GetExitProbability("east").Should().Be(0f);
    }
}
