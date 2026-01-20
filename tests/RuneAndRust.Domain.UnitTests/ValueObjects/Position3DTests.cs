using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the Position3D value object.
/// </summary>
[TestFixture]
public class Position3DTests
{
    // ===== Constructor Tests =====

    [Test]
    public void Constructor_WithCoordinates_CreatesPosition()
    {
        // Arrange & Act
        var position = new Position3D(1, 2, 3);

        // Assert
        position.X.Should().Be(1);
        position.Y.Should().Be(2);
        position.Z.Should().Be(3);
    }

    [Test]
    public void Origin_ReturnsZeroCoordinates()
    {
        // Arrange & Act
        var origin = Position3D.Origin;

        // Assert
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
        origin.Z.Should().Be(0);
    }

    // ===== Move Method Tests =====

    [Test]
    public void Move_WithDeltas_ReturnsNewPosition()
    {
        // Arrange
        var position = new Position3D(1, 2, 3);

        // Act
        var moved = position.Move(2, 3, -1);

        // Assert
        moved.X.Should().Be(3);
        moved.Y.Should().Be(5);
        moved.Z.Should().Be(2);
    }

    [Test]
    public void MoveUp_DecreasesZ()
    {
        // Arrange
        var position = new Position3D(0, 0, 5);

        // Act
        var moved = position.MoveUp();

        // Assert
        moved.Z.Should().Be(4);
        moved.X.Should().Be(0);
        moved.Y.Should().Be(0);
    }

    [Test]
    public void MoveDown_IncreasesZ()
    {
        // Arrange
        var position = new Position3D(0, 0, 5);

        // Act
        var moved = position.MoveDown();

        // Assert
        moved.Z.Should().Be(6);
        moved.X.Should().Be(0);
        moved.Y.Should().Be(0);
    }

    // ===== Move(Direction) Tests =====

    [TestCase(Direction.North, 0, 1, 0)]
    [TestCase(Direction.South, 0, -1, 0)]
    [TestCase(Direction.East, 1, 0, 0)]
    [TestCase(Direction.West, -1, 0, 0)]
    [TestCase(Direction.Up, 0, 0, -1)]
    [TestCase(Direction.Down, 0, 0, 1)]
    public void Move_WithDirection_MovesCorrectly(Direction direction, int expectedX, int expectedY, int expectedZ)
    {
        // Arrange
        var position = Position3D.Origin;

        // Act
        var moved = position.Move(direction);

        // Assert
        moved.X.Should().Be(expectedX);
        moved.Y.Should().Be(expectedY);
        moved.Z.Should().Be(expectedZ);
    }

    [Test]
    public void Move_WithInvalidDirection_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var position = Position3D.Origin;

        // Act
        var act = () => position.Move((Direction)99);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ===== Conversion Tests =====

    [Test]
    public void ToPosition2D_ReturnsPositionWithSameXY()
    {
        // Arrange
        var position3D = new Position3D(5, 10, 15);

        // Act
        var position2D = position3D.ToPosition2D();

        // Assert
        position2D.X.Should().Be(5);
        position2D.Y.Should().Be(10);
    }

    [Test]
    public void FromPosition2D_CreatesPosition3DWithSpecifiedZ()
    {
        // Arrange
        var position2D = new Position(3, 7);

        // Act
        var position3D = Position3D.FromPosition2D(position2D, 2);

        // Assert
        position3D.X.Should().Be(3);
        position3D.Y.Should().Be(7);
        position3D.Z.Should().Be(2);
    }

    [Test]
    public void FromPosition2D_DefaultsZToZero()
    {
        // Arrange
        var position2D = new Position(3, 7);

        // Act
        var position3D = Position3D.FromPosition2D(position2D);

        // Assert
        position3D.Z.Should().Be(0);
    }

    // ===== ManhattanDistance Tests =====

    [Test]
    public void ManhattanDistanceTo_ReturnsSumOfAbsoluteDifferences()
    {
        // Arrange
        var p1 = new Position3D(0, 0, 0);
        var p2 = new Position3D(3, 4, 5);

        // Act
        var distance = p1.ManhattanDistanceTo(p2);

        // Assert
        distance.Should().Be(12); // |3| + |4| + |5|
    }

    [Test]
    public void ManhattanDistanceTo_WithNegativeCoordinates_ReturnsCorrectDistance()
    {
        // Arrange
        var p1 = new Position3D(-2, 3, 1);
        var p2 = new Position3D(2, -1, 4);

        // Act
        var distance = p1.ManhattanDistanceTo(p2);

        // Assert
        distance.Should().Be(11); // |4| + |4| + |3|
    }

    // ===== ToString Tests =====

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var position = new Position3D(3, 5, 2);

        // Act
        var result = position.ToString();

        // Assert
        result.Should().Be("(3, 5, Z=2)");
    }

    // ===== Equality Tests =====

    [Test]
    public void Equals_WithSameCoordinates_ReturnsTrue()
    {
        // Arrange
        var p1 = new Position3D(1, 2, 3);
        var p2 = new Position3D(1, 2, 3);

        // Act & Assert
        p1.Should().Be(p2);
        (p1 == p2).Should().BeTrue();
    }

    [Test]
    public void Equals_WithDifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var p1 = new Position3D(1, 2, 3);
        var p2 = new Position3D(1, 2, 4);

        // Act & Assert
        p1.Should().NotBe(p2);
        (p1 != p2).Should().BeTrue();
    }
}
