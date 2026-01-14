using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="GridPosition"/> value object (v0.5.0a).
/// </summary>
[TestFixture]
public class GridPositionTests
{
    // ===== Constructor Tests =====

    [Test]
    public void Constructor_WithCoordinates_CreatesPosition()
    {
        // Arrange & Act
        var position = new GridPosition(3, 5);

        // Assert
        position.X.Should().Be(3);
        position.Y.Should().Be(5);
    }

    [Test]
    public void Origin_ReturnsZeroCoordinates()
    {
        // Arrange & Act
        var origin = GridPosition.Origin;

        // Assert
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
    }

    // ===== Move Tests =====

    [TestCase(MovementDirection.North, 0, -1)]
    [TestCase(MovementDirection.South, 0, 1)]
    [TestCase(MovementDirection.East, 1, 0)]
    [TestCase(MovementDirection.West, -1, 0)]
    [TestCase(MovementDirection.NorthEast, 1, -1)]
    [TestCase(MovementDirection.NorthWest, -1, -1)]
    [TestCase(MovementDirection.SouthEast, 1, 1)]
    [TestCase(MovementDirection.SouthWest, -1, 1)]
    public void Move_AllDirections_ReturnsCorrectPositions(
        MovementDirection direction, int expectedDeltaX, int expectedDeltaY)
    {
        // Arrange
        var startX = 5;
        var startY = 5;
        var position = new GridPosition(startX, startY);

        // Act
        var moved = position.Move(direction);

        // Assert
        moved.X.Should().Be(startX + expectedDeltaX);
        moved.Y.Should().Be(startY + expectedDeltaY);
    }

    // ===== DistanceTo Tests (Chebyshev) =====

    [Test]
    public void DistanceTo_SamePosition_ReturnsZero()
    {
        // Arrange
        var pos1 = new GridPosition(3, 3);
        var pos2 = new GridPosition(3, 3);

        // Act
        var distance = pos1.DistanceTo(pos2);

        // Assert
        distance.Should().Be(0);
    }

    [Test]
    public void DistanceTo_CardinalDirection_ReturnsCorrectDistance()
    {
        // Arrange
        var pos1 = new GridPosition(0, 0);
        var pos2 = new GridPosition(5, 0);

        // Act
        var distance = pos1.DistanceTo(pos2);

        // Assert
        distance.Should().Be(5);
    }

    [Test]
    public void DistanceTo_DiagonalDirection_ReturnsChebyshevDistance()
    {
        // Arrange - diagonal movement counts as 1 per step
        var pos1 = new GridPosition(0, 0);
        var pos2 = new GridPosition(3, 3);

        // Act
        var distance = pos1.DistanceTo(pos2);

        // Assert - Chebyshev distance = max(|dx|, |dy|) = max(3, 3) = 3
        distance.Should().Be(3);
    }

    [Test]
    public void DistanceTo_MixedDirection_ReturnsChebyshevDistance()
    {
        // Arrange
        var pos1 = new GridPosition(0, 0);
        var pos2 = new GridPosition(4, 2);

        // Act
        var distance = pos1.DistanceTo(pos2);

        // Assert - max(4, 2) = 4
        distance.Should().Be(4);
    }

    // ===== ManhattanDistanceTo Tests =====

    [Test]
    public void ManhattanDistanceTo_ReturnsCorrectDistance()
    {
        // Arrange
        var pos1 = new GridPosition(0, 0);
        var pos2 = new GridPosition(3, 4);

        // Act
        var distance = pos1.ManhattanDistanceTo(pos2);

        // Assert - |3| + |4| = 7
        distance.Should().Be(7);
    }

    // ===== IsAdjacentTo Tests =====

    [Test]
    public void IsAdjacentTo_CardinalAdjacent_ReturnsTrue()
    {
        // Arrange
        var pos1 = new GridPosition(3, 3);
        var pos2 = new GridPosition(3, 4);

        // Act & Assert
        pos1.IsAdjacentTo(pos2).Should().BeTrue();
    }

    [Test]
    public void IsAdjacentTo_DiagonalAdjacent_ReturnsTrue()
    {
        // Arrange
        var pos1 = new GridPosition(3, 3);
        var pos2 = new GridPosition(4, 4);

        // Act & Assert
        pos1.IsAdjacentTo(pos2).Should().BeTrue();
    }

    [Test]
    public void IsAdjacentTo_NotAdjacent_ReturnsFalse()
    {
        // Arrange
        var pos1 = new GridPosition(0, 0);
        var pos2 = new GridPosition(2, 0);

        // Act & Assert
        pos1.IsAdjacentTo(pos2).Should().BeFalse();
    }

    [Test]
    public void IsAdjacentTo_SamePosition_ReturnsFalse()
    {
        // Arrange
        var pos1 = new GridPosition(3, 3);
        var pos2 = new GridPosition(3, 3);

        // Act & Assert
        pos1.IsAdjacentTo(pos2).Should().BeFalse();
    }

    // ===== ToString Tests =====

    [TestCase(0, 0, "A1")]
    [TestCase(7, 7, "H8")]
    [TestCase(2, 3, "C4")]
    [TestCase(0, 9, "A10")]
    public void ToString_ReturnsCorrectNotation(int x, int y, string expected)
    {
        // Arrange
        var position = new GridPosition(x, y);

        // Act
        var result = position.ToString();

        // Assert
        result.Should().Be(expected);
    }

    // ===== TryParse Tests =====

    [TestCase("A1", 0, 0)]
    [TestCase("H8", 7, 7)]
    [TestCase("C4", 2, 3)]
    [TestCase("a1", 0, 0)] // Case-insensitive
    [TestCase("A10", 0, 9)]
    public void TryParse_ValidNotation_ReturnsTrue(
        string input, int expectedX, int expectedY)
    {
        // Act
        var result = GridPosition.TryParse(input, out var position);

        // Assert
        result.Should().BeTrue();
        position.X.Should().Be(expectedX);
        position.Y.Should().Be(expectedY);
    }

    [TestCase("")]
    [TestCase("A")]
    [TestCase("1A")]
    [TestCase("AA")]
    [TestCase("A0")]
    [TestCase("A-1")]
    [TestCase(null)]
    public void TryParse_InvalidNotation_ReturnsFalse(string? input)
    {
        // Act
        var result = GridPosition.TryParse(input!, out var position);

        // Assert
        result.Should().BeFalse();
        position.Should().Be(default(GridPosition));
    }

    // ===== FromNotation Tests =====

    [Test]
    public void FromNotation_ValidInput_ReturnsPosition()
    {
        // Act
        var position = GridPosition.FromNotation("D5");

        // Assert
        position.X.Should().Be(3);
        position.Y.Should().Be(4);
    }

    [Test]
    public void FromNotation_InvalidInput_ThrowsArgumentException()
    {
        // Act
        var act = () => GridPosition.FromNotation("invalid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("notation")
            .WithMessage("*Invalid grid notation*");
    }

    // ===== Equality Tests =====

    [Test]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 5);

        // Assert
        pos1.Should().Be(pos2);
        (pos1 == pos2).Should().BeTrue();
    }

    [Test]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 6);

        // Assert
        pos1.Should().NotBe(pos2);
        (pos1 != pos2).Should().BeTrue();
    }
}
