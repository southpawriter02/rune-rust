using FluentAssertions;
using RuneAndRust.Core.ValueObjects;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Coordinate value object.
/// Validates coordinate creation, equality, and offset operations.
/// </summary>
public class CoordinateTests
{
    #region Constructor Tests

    [Fact]
    public void Coordinate_Constructor_ShouldSetXYZ()
    {
        // Arrange & Act
        var coord = new Coordinate(1, 2, 3);

        // Assert
        coord.X.Should().Be(1);
        coord.Y.Should().Be(2);
        coord.Z.Should().Be(3);
    }

    [Fact]
    public void Coordinate_Constructor_CanHandleNegativeValues()
    {
        // Arrange & Act
        var coord = new Coordinate(-5, -10, -15);

        // Assert
        coord.X.Should().Be(-5);
        coord.Y.Should().Be(-10);
        coord.Z.Should().Be(-15);
    }

    [Fact]
    public void Coordinate_Constructor_CanHandleZeroValues()
    {
        // Arrange & Act
        var coord = new Coordinate(0, 0, 0);

        // Assert
        coord.X.Should().Be(0);
        coord.Y.Should().Be(0);
        coord.Z.Should().Be(0);
    }

    #endregion

    #region Origin Tests

    [Fact]
    public void Coordinate_Origin_ShouldBeZeroZeroZero()
    {
        // Arrange & Act
        var origin = Coordinate.Origin;

        // Assert
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
        origin.Z.Should().Be(0);
    }

    [Fact]
    public void Coordinate_Origin_ShouldEqualNewZeroCoordinate()
    {
        // Arrange
        var origin = Coordinate.Origin;
        var zero = new Coordinate(0, 0, 0);

        // Assert
        origin.Should().Be(zero);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Coordinate_EqualCoordinates_ShouldBeEqual()
    {
        // Arrange
        var coord1 = new Coordinate(5, 10, 15);
        var coord2 = new Coordinate(5, 10, 15);

        // Assert
        coord1.Should().Be(coord2);
        (coord1 == coord2).Should().BeTrue();
    }

    [Fact]
    public void Coordinate_DifferentCoordinates_ShouldNotBeEqual()
    {
        // Arrange
        var coord1 = new Coordinate(1, 2, 3);
        var coord2 = new Coordinate(4, 5, 6);

        // Assert
        coord1.Should().NotBe(coord2);
        (coord1 != coord2).Should().BeTrue();
    }

    [Fact]
    public void Coordinate_DifferentX_ShouldNotBeEqual()
    {
        // Arrange
        var coord1 = new Coordinate(1, 2, 3);
        var coord2 = new Coordinate(99, 2, 3);

        // Assert
        coord1.Should().NotBe(coord2);
    }

    [Fact]
    public void Coordinate_DifferentY_ShouldNotBeEqual()
    {
        // Arrange
        var coord1 = new Coordinate(1, 2, 3);
        var coord2 = new Coordinate(1, 99, 3);

        // Assert
        coord1.Should().NotBe(coord2);
    }

    [Fact]
    public void Coordinate_DifferentZ_ShouldNotBeEqual()
    {
        // Arrange
        var coord1 = new Coordinate(1, 2, 3);
        var coord2 = new Coordinate(1, 2, 99);

        // Assert
        coord1.Should().NotBe(coord2);
    }

    [Fact]
    public void Coordinate_GetHashCode_SameForEqualCoordinates()
    {
        // Arrange
        var coord1 = new Coordinate(5, 10, 15);
        var coord2 = new Coordinate(5, 10, 15);

        // Assert
        coord1.GetHashCode().Should().Be(coord2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void Coordinate_ToString_ReturnsFormattedString()
    {
        // Arrange
        var coord = new Coordinate(1, 2, 3);

        // Act
        var result = coord.ToString();

        // Assert
        result.Should().Be("(1, 2, 3)");
    }

    [Fact]
    public void Coordinate_ToString_HandlesNegativeValues()
    {
        // Arrange
        var coord = new Coordinate(-1, -2, -3);

        // Act
        var result = coord.ToString();

        // Assert
        result.Should().Be("(-1, -2, -3)");
    }

    [Fact]
    public void Coordinate_ToString_Origin()
    {
        // Arrange & Act
        var result = Coordinate.Origin.ToString();

        // Assert
        result.Should().Be("(0, 0, 0)");
    }

    #endregion

    #region Offset Tests

    [Fact]
    public void Coordinate_Offset_ReturnsNewCoordinate()
    {
        // Arrange
        var original = new Coordinate(0, 0, 0);

        // Act
        var result = original.Offset(1, 2, 3);

        // Assert
        result.Should().Be(new Coordinate(1, 2, 3));
    }

    [Fact]
    public void Coordinate_Offset_DoesNotModifyOriginal()
    {
        // Arrange
        var original = new Coordinate(5, 5, 5);

        // Act
        var result = original.Offset(1, 1, 1);

        // Assert
        original.Should().Be(new Coordinate(5, 5, 5));
        result.Should().Be(new Coordinate(6, 6, 6));
    }

    [Fact]
    public void Coordinate_Offset_HandlesNegativeOffset()
    {
        // Arrange
        var original = new Coordinate(10, 10, 10);

        // Act
        var result = original.Offset(-5, -3, -1);

        // Assert
        result.Should().Be(new Coordinate(5, 7, 9));
    }

    [Fact]
    public void Coordinate_Offset_ZeroOffset_ReturnsSameValues()
    {
        // Arrange
        var original = new Coordinate(7, 8, 9);

        // Act
        var result = original.Offset(0, 0, 0);

        // Assert
        result.Should().Be(original);
    }

    [Theory]
    [InlineData(1, 0, 0)]  // North
    [InlineData(-1, 0, 0)] // South
    [InlineData(0, 1, 0)]  // East
    [InlineData(0, -1, 0)] // West
    [InlineData(0, 0, 1)]  // Up
    [InlineData(0, 0, -1)] // Down
    public void Coordinate_Offset_CardinalDirections(int dx, int dy, int dz)
    {
        // Arrange
        var origin = Coordinate.Origin;

        // Act
        var result = origin.Offset(dx, dy, dz);

        // Assert
        result.Should().Be(new Coordinate(dx, dy, dz));
    }

    #endregion

    #region Record Semantics Tests

    [Fact]
    public void Coordinate_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Coordinate(1, 2, 3);

        // Act
        var modified = original with { X = 99 };

        // Assert
        modified.X.Should().Be(99);
        modified.Y.Should().Be(2);
        modified.Z.Should().Be(3);
        original.X.Should().Be(1); // Original unchanged
    }

    #endregion
}
