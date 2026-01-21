// ═══════════════════════════════════════════════════════════════════════════════
// GridUtilitiesTests.cs
// Unit tests for GridUtilities.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="GridUtilities"/>.
/// </summary>
[TestFixture]
public class GridUtilitiesTests
{
    // ═══════════════════════════════════════════════════════════════
    // CALCULATE GRID POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 5, 0, 0)]
    [TestCase(4, 5, 4, 0)]
    [TestCase(5, 5, 0, 1)]
    [TestCase(12, 5, 2, 2)]
    public void CalculateGridPosition_ConvertsCorrectly(
        int index, int width, int expectedX, int expectedY)
    {
        // Arrange & Act
        var (x, y) = GridUtilities.CalculateGridPosition(index, width);

        // Assert
        x.Should().Be(expectedX);
        y.Should().Be(expectedY);
    }

    [Test]
    public void CalculateGridPosition_WhenWidthIsZeroOrNegative_Throws()
    {
        // Arrange & Act
        var actZero = () => GridUtilities.CalculateGridPosition(5, 0);
        var actNegative = () => GridUtilities.CalculateGridPosition(5, -1);

        // Assert
        actZero.Should().Throw<ArgumentOutOfRangeException>();
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CALCULATE LINEAR INDEX TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0, 5, 0)]
    [TestCase(4, 0, 5, 4)]
    [TestCase(0, 1, 5, 5)]
    [TestCase(2, 2, 5, 12)]
    public void CalculateLinearIndex_ConvertsCorrectly(
        int x, int y, int width, int expectedIndex)
    {
        // Arrange & Act
        var result = GridUtilities.CalculateLinearIndex(x, y, width);

        // Assert
        result.Should().Be(expectedIndex);
    }

    // ═══════════════════════════════════════════════════════════════
    // IS VALID POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0, 8, 8, true)]   // Top-left corner
    [TestCase(7, 7, 8, 8, true)]   // Bottom-right corner
    [TestCase(4, 4, 8, 8, true)]   // Center
    [TestCase(-1, 0, 8, 8, false)] // Negative X
    [TestCase(0, -1, 8, 8, false)] // Negative Y
    [TestCase(8, 0, 8, 8, false)]  // X out of bounds
    [TestCase(0, 8, 8, 8, false)]  // Y out of bounds
    public void IsValidPosition_ValidatesCorrectly(
        int x, int y, int width, int height, bool expected)
    {
        // Arrange & Act
        var result = GridUtilities.IsValidPosition(x, y, width, height);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsValidPosition_WithTuple_ValidatesCorrectly()
    {
        // Arrange
        var validPosition = (X: 4, Y: 4);
        var invalidPosition = (X: 10, Y: 10);

        // Act & Assert
        GridUtilities.IsValidPosition(validPosition, 8, 8).Should().BeTrue();
        GridUtilities.IsValidPosition(invalidPosition, 8, 8).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ADJACENT POSITIONS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetAdjacentPositions_ReturnsFourCardinalDirections()
    {
        // Arrange & Act
        var adjacent = GridUtilities.GetAdjacentPositions(4, 4).ToList();

        // Assert
        adjacent.Should().HaveCount(4);
        adjacent.Should().Contain((3, 4)); // Left
        adjacent.Should().Contain((5, 4)); // Right
        adjacent.Should().Contain((4, 3)); // Up
        adjacent.Should().Contain((4, 5)); // Down
    }

    [Test]
    public void GetAllAdjacentPositions_ReturnsEightDirections()
    {
        // Arrange & Act
        var adjacent = GridUtilities.GetAllAdjacentPositions(4, 4).ToList();

        // Assert
        adjacent.Should().HaveCount(8);
        // Should include cardinal directions
        adjacent.Should().Contain((3, 4)); // Left
        adjacent.Should().Contain((5, 4)); // Right
        // Should include diagonals
        adjacent.Should().Contain((3, 3)); // NorthWest
        adjacent.Should().Contain((5, 5)); // SouthEast
    }

    [Test]
    public void GetValidAdjacentPositions_AtCorner_ReturnsOnlyValidCells()
    {
        // Arrange (top-left corner)
        var x = 0;
        var y = 0;

        // Act
        var valid = GridUtilities.GetValidAdjacentPositions(x, y, 8, 8).ToList();

        // Assert
        valid.Should().HaveCount(2);
        valid.Should().Contain((1, 0)); // Right
        valid.Should().Contain((0, 1)); // Down
    }

    // ═══════════════════════════════════════════════════════════════
    // DISTANCE CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0, 3, 4, 7)]
    [TestCase(5, 5, 5, 5, 0)]
    [TestCase(0, 0, 5, 0, 5)]
    [TestCase(0, 0, 0, 5, 5)]
    public void CalculateManhattanDistance_CalculatesCorrectly(
        int x1, int y1, int x2, int y2, int expected)
    {
        // Arrange & Act
        var result = GridUtilities.CalculateManhattanDistance(x1, y1, x2, y2);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(0, 0, 3, 4, 4)]
    [TestCase(5, 5, 5, 5, 0)]
    [TestCase(0, 0, 5, 0, 5)]
    [TestCase(0, 0, 3, 3, 3)]
    public void CalculateChebyshevDistance_CalculatesCorrectly(
        int x1, int y1, int x2, int y2, int expected)
    {
        // Arrange & Act
        var result = GridUtilities.CalculateChebyshevDistance(x1, y1, x2, y2);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET DIRECTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(5, 5, 5, 4, Direction.North)]
    [TestCase(5, 5, 6, 5, Direction.East)]
    [TestCase(5, 5, 5, 6, Direction.South)]
    [TestCase(5, 5, 4, 5, Direction.West)]
    [TestCase(5, 5, 6, 4, Direction.NorthEast)]
    [TestCase(5, 5, 6, 6, Direction.SouthEast)]
    [TestCase(5, 5, 4, 6, Direction.SouthWest)]
    [TestCase(5, 5, 4, 4, Direction.NorthWest)]
    [TestCase(5, 5, 5, 5, Direction.None)]
    public void GetDirection_ReturnsCorrectDirection(
        int fromX, int fromY, int toX, int toY, Direction expected)
    {
        // Arrange & Act
        var result = GridUtilities.GetDirection(fromX, fromY, toX, toY);

        // Assert
        result.Should().Be(expected);
    }
}
