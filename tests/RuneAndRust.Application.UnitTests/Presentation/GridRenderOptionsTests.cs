using FluentAssertions;
using RuneAndRust.Presentation.Shared.DTOs;

namespace RuneAndRust.Application.UnitTests.Presentation;

/// <summary>
/// Unit tests for <see cref="GridRenderOptions"/> (v0.5.0c).
/// </summary>
[TestFixture]
public class GridRenderOptionsTests
{
    [Test]
    public void Default_HasExpectedValues()
    {
        // Act
        var options = GridRenderOptions.Default;

        // Assert
        options.UseBoxDrawing.Should().BeTrue();
        options.ShowCoordinates.Should().BeTrue();
        options.ShowLegend.Should().BeTrue();
        options.Compact.Should().BeFalse();
    }

    [Test]
    public void CompactDefault_HasCompactTrue()
    {
        // Act
        var options = GridRenderOptions.CompactDefault;

        // Assert
        options.Compact.Should().BeTrue();
    }

    [Test]
    public void TurnNumber_CanBeSet()
    {
        // Act
        var options = new GridRenderOptions { TurnNumber = 5 };

        // Assert
        options.TurnNumber.Should().Be(5);
    }
}
