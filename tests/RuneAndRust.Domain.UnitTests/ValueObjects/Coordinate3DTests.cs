using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class Coordinate3DTests
{
    [Test]
    public void Origin_ReturnsZeroCoordinate()
    {
        // Act
        var origin = Coordinate3D.Origin;

        // Assert
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
        origin.Z.Should().Be(0);
    }

    [Test]
    public void Constructor_CreatesCoordinateWithCorrectValues()
    {
        // Act
        var coord = new Coordinate3D(1, 2, 3);

        // Assert
        coord.X.Should().Be(1);
        coord.Y.Should().Be(2);
        coord.Z.Should().Be(3);
    }

    [TestCase(Direction.North, 0, 1, 0)]
    [TestCase(Direction.South, 0, -1, 0)]
    [TestCase(Direction.East, 1, 0, 0)]
    [TestCase(Direction.West, -1, 0, 0)]
    [TestCase(Direction.Up, 0, 0, 1)]
    [TestCase(Direction.Down, 0, 0, -1)]
    public void Move_ReturnsCorrectCoordinate(Direction direction, int expectedX, int expectedY, int expectedZ)
    {
        // Arrange
        var origin = Coordinate3D.Origin;

        // Act
        var result = origin.Move(direction);

        // Assert
        result.X.Should().Be(expectedX);
        result.Y.Should().Be(expectedY);
        result.Z.Should().Be(expectedZ);
    }

    [Test]
    public void Move_Diagonal_ReturnsSameCoordinate()
    {
        // Arrange
        var origin = Coordinate3D.Origin;

        // Act - diagonal directions are not supported in 3D grid
        var result = origin.Move(Direction.Northeast);

        // Assert
        result.Should().Be(origin);
    }

    [TestCase(0, 0, 0, 1, 1, 1, 3)]
    [TestCase(0, 0, 0, 2, 0, 0, 2)]
    [TestCase(-1, -1, -1, 1, 1, 1, 6)]
    [TestCase(0, 0, 0, 0, 0, 0, 0)]
    public void ManhattanDistanceTo_CalculatesCorrectly(
        int x1, int y1, int z1,
        int x2, int y2, int z2,
        int expectedDistance)
    {
        // Arrange
        var coord1 = new Coordinate3D(x1, y1, z1);
        var coord2 = new Coordinate3D(x2, y2, z2);

        // Act
        var distance = coord1.ManhattanDistanceTo(coord2);

        // Assert
        distance.Should().Be(expectedDistance);
    }

    [Test]
    public void GetAdjacentCoordinates_ReturnsSixNeighbors()
    {
        // Arrange
        var coord = new Coordinate3D(1, 1, 1);

        // Act
        var adjacent = coord.GetAdjacentCoordinates().ToList();

        // Assert
        adjacent.Should().HaveCount(6);
        adjacent.Should().Contain(new Coordinate3D(2, 1, 1)); // +X
        adjacent.Should().Contain(new Coordinate3D(0, 1, 1)); // -X
        adjacent.Should().Contain(new Coordinate3D(1, 2, 1)); // +Y
        adjacent.Should().Contain(new Coordinate3D(1, 0, 1)); // -Y
        adjacent.Should().Contain(new Coordinate3D(1, 1, 2)); // +Z
        adjacent.Should().Contain(new Coordinate3D(1, 1, 0)); // -Z
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var coord = new Coordinate3D(3, -2, 1);

        // Act
        var result = coord.ToString();

        // Assert
        result.Should().Be("(3, -2, 1)");
    }

    [Test]
    public void Equality_SameCoordinates_AreEqual()
    {
        // Arrange
        var coord1 = new Coordinate3D(1, 2, 3);
        var coord2 = new Coordinate3D(1, 2, 3);

        // Assert
        coord1.Should().Be(coord2);
        (coord1 == coord2).Should().BeTrue();
    }

    [Test]
    public void Equality_DifferentCoordinates_AreNotEqual()
    {
        // Arrange
        var coord1 = new Coordinate3D(1, 2, 3);
        var coord2 = new Coordinate3D(1, 2, 4);

        // Assert
        coord1.Should().NotBe(coord2);
        (coord1 != coord2).Should().BeTrue();
    }
}
