using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Controls;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Controls;

/// <summary>
/// Unit tests for <see cref="MovementRangeOverlay"/>.
/// </summary>
[TestFixture]
public class MovementRangeOverlayTests
{
    /// <summary>
    /// Verifies opacity is 0.7 at distance 0 (adjacent/origin).
    /// </summary>
    [Test]
    public void CalculateOpacity_AtDistance0_Returns0Point7()
    {
        // Act
        var opacity = MovementRangeOverlay.CalculateOpacity(0, 5);

        // Assert
        opacity.Should().BeApproximately(0.7, 0.01);
    }

    /// <summary>
    /// Verifies opacity is 0.2 at max distance (minimum visible).
    /// </summary>
    [Test]
    public void CalculateOpacity_AtMaxDistance_Returns0Point2()
    {
        // Act
        var opacity = MovementRangeOverlay.CalculateOpacity(5, 5);

        // Assert
        opacity.Should().BeApproximately(0.2, 0.01);
    }

    /// <summary>
    /// Verifies opacity interpolates correctly at mid-range.
    /// </summary>
    [Test]
    public void CalculateOpacity_AtMidDistance_ReturnsInterpolatedValue()
    {
        // Act
        var opacity = MovementRangeOverlay.CalculateOpacity(2, 4);

        // Assert
        // Formula: 0.7 - (0.5 * 2/4) = 0.7 - 0.25 = 0.45
        opacity.Should().BeApproximately(0.45, 0.01);
    }

    /// <summary>
    /// Verifies distance calculation uses Manhattan distance.
    /// </summary>
    [Test]
    public void CalculateDistance_WithDiagonalPositions_ReturnsManhattanDistance()
    {
        // Arrange
        var a = new GridPosition(0, 0);
        var b = new GridPosition(3, 4);

        // Act
        var distance = MovementRangeOverlay.CalculateDistance(a, b);

        // Assert
        distance.Should().Be(7); // |3-0| + |4-0| = 7
    }

    /// <summary>
    /// Verifies opacity returns 0.5 when maxDistance is 0 (edge case).
    /// </summary>
    [Test]
    public void CalculateOpacity_WithZeroMaxDistance_Returns0Point5()
    {
        // Act
        var opacity = MovementRangeOverlay.CalculateOpacity(0, 0);

        // Assert
        opacity.Should().BeApproximately(0.5, 0.01);
    }
}
