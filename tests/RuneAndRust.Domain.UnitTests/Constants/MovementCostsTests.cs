using FluentAssertions;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Constants;

/// <summary>
/// Unit tests for the <see cref="MovementCosts"/> constants (v0.5.0b).
/// </summary>
[TestFixture]
public class MovementCostsTests
{
    // ===== GetCost Tests =====

    [TestCase(MovementDirection.North, 2)]
    [TestCase(MovementDirection.South, 2)]
    [TestCase(MovementDirection.East, 2)]
    [TestCase(MovementDirection.West, 2)]
    public void GetCost_CardinalDirections_Returns2Points(MovementDirection direction, int expected)
    {
        // Act
        var cost = MovementCosts.GetCost(direction);

        // Assert
        cost.Should().Be(expected);
    }

    [TestCase(MovementDirection.NorthEast, 3)]
    [TestCase(MovementDirection.NorthWest, 3)]
    [TestCase(MovementDirection.SouthEast, 3)]
    [TestCase(MovementDirection.SouthWest, 3)]
    public void GetCost_DiagonalDirections_Returns3Points(MovementDirection direction, int expected)
    {
        // Act
        var cost = MovementCosts.GetCost(direction);

        // Assert
        cost.Should().Be(expected);
    }

    // ===== SpeedToPoints Tests =====

    [TestCase(1, 2)]
    [TestCase(3, 6)]
    [TestCase(4, 8)]
    [TestCase(5, 10)]
    public void SpeedToPoints_ReturnsSpeedTimesMultiplier(int speed, int expected)
    {
        // Act
        var points = MovementCosts.SpeedToPoints(speed);

        // Assert
        points.Should().Be(expected);
    }

    // ===== PointsToSpeed Tests =====

    [TestCase(2, 1)]
    [TestCase(6, 3)]
    [TestCase(8, 4)]
    [TestCase(10, 5)]
    public void PointsToSpeed_ReturnsPointsDividedByMultiplier(int points, int expected)
    {
        // Act
        var speed = MovementCosts.PointsToSpeed(points);

        // Assert
        speed.Should().Be(expected);
    }

    // ===== GetDisplayCost Tests =====

    [TestCase(MovementDirection.North, "1")]
    [TestCase(MovementDirection.NorthEast, "1.5")]
    public void GetDisplayCost_ReturnsHumanReadableCost(MovementDirection direction, string expected)
    {
        // Act
        var displayCost = MovementCosts.GetDisplayCost(direction);

        // Assert
        displayCost.Should().Be(expected);
    }

    // ===== IsDiagonal Tests =====

    [TestCase(MovementDirection.North, false)]
    [TestCase(MovementDirection.NorthEast, true)]
    [TestCase(MovementDirection.South, false)]
    [TestCase(MovementDirection.SouthWest, true)]
    public void IsDiagonal_ReturnsCorrectValue(MovementDirection direction, bool expected)
    {
        // Act
        var isDiagonal = MovementCosts.IsDiagonal(direction);

        // Assert
        isDiagonal.Should().Be(expected);
    }
}
