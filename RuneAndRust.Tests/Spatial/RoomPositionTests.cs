using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core.Spatial;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for RoomPosition struct (v0.39.1)
/// </summary>
[TestClass]
public class RoomPositionTests
{
    [TestMethod]
    public void Constructor_ValidCoordinates_CreatesPosition()
    {
        // Arrange & Act
        var position = new RoomPosition(5, -3, 2);

        // Assert
        Assert.AreEqual(5, position.X);
        Assert.AreEqual(-3, position.Y);
        Assert.AreEqual(2, position.Z);
    }

    [TestMethod]
    public void Origin_ReturnsZeroCoordinates()
    {
        // Arrange & Act
        var origin = RoomPosition.Origin;

        // Assert
        Assert.AreEqual(0, origin.X);
        Assert.AreEqual(0, origin.Y);
        Assert.AreEqual(0, origin.Z);
    }

    [TestMethod]
    public void ManhattanDistanceHorizontal_SameLevel_CalculatesCorrectly()
    {
        // Arrange
        var pos1 = new RoomPosition(0, 0, 0);
        var pos2 = new RoomPosition(3, 4, 0);

        // Act
        var distance = pos1.ManhattanDistanceHorizontal(pos2);

        // Assert
        Assert.AreEqual(7, distance); // |0-3| + |0-4| = 7
    }

    [TestMethod]
    public void ManhattanDistanceHorizontal_DifferentLevels_IgnoresZ()
    {
        // Arrange
        var pos1 = new RoomPosition(0, 0, 0);
        var pos2 = new RoomPosition(3, 4, 10); // Z difference ignored

        // Act
        var distance = pos1.ManhattanDistanceHorizontal(pos2);

        // Assert
        Assert.AreEqual(7, distance); // Still 7, Z ignored
    }

    [TestMethod]
    public void ManhattanDistance3D_IncludesVerticalDistance()
    {
        // Arrange
        var pos1 = new RoomPosition(0, 0, 0);
        var pos2 = new RoomPosition(3, 4, 2);

        // Act
        var distance = pos1.ManhattanDistance3D(pos2);

        // Assert
        Assert.AreEqual(9, distance); // |0-3| + |0-4| + |0-2| = 9
    }

    [TestMethod]
    public void IsDirectlyAboveOrBelow_SameXY_DifferentZ_ReturnsTrue()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(5, 5, -2);

        // Act
        var result = pos1.IsDirectlyAboveOrBelow(pos2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsDirectlyAboveOrBelow_DifferentXY_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(6, 5, -2); // Different X

        // Act
        var result = pos1.IsDirectlyAboveOrBelow(pos2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsDirectlyAboveOrBelow_SameXYZ_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(5, 5, 0); // Same Z

        // Act
        var result = pos1.IsDirectlyAboveOrBelow(pos2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsOnSameLevel_SameZ_ReturnsTrue()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 3, 1);
        var pos2 = new RoomPosition(8, 9, 1); // Same Z

        // Act
        var result = pos1.IsOnSameLevel(pos2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsOnSameLevel_DifferentZ_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 3, 1);
        var pos2 = new RoomPosition(5, 3, 2); // Different Z

        // Act
        var result = pos1.IsOnSameLevel(pos2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void VerticalDistanceTo_CalculatesAbsoluteDifference()
    {
        // Arrange
        var pos1 = new RoomPosition(0, 0, 2);
        var pos2 = new RoomPosition(0, 0, -1);

        // Act
        var distance = pos1.VerticalDistanceTo(pos2);

        // Assert
        Assert.AreEqual(3, distance); // |2 - (-1)| = 3
    }

    [TestMethod]
    public void IsAdjacentHorizontal_NorthNeighbor_ReturnsTrue()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(5, 6, 0); // North

        // Act
        var result = pos1.IsAdjacentHorizontal(pos2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsAdjacentHorizontal_EastNeighbor_ReturnsTrue()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(6, 5, 0); // East

        // Act
        var result = pos1.IsAdjacentHorizontal(pos2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsAdjacentHorizontal_DiagonalNeighbor_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(6, 6, 0); // Diagonal

        // Act
        var result = pos1.IsAdjacentHorizontal(pos2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsAdjacentHorizontal_DifferentZ_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(5, 5, 0);
        var pos2 = new RoomPosition(5, 6, 1); // Different Z

        // Act
        var result = pos1.IsAdjacentHorizontal(pos2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equality_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var pos1 = new RoomPosition(3, 4, 5);
        var pos2 = new RoomPosition(3, 4, 5);

        // Act & Assert
        Assert.AreEqual(pos1, pos2);
        Assert.IsTrue(pos1 == pos2);
        Assert.IsFalse(pos1 != pos2);
    }

    [TestMethod]
    public void Equality_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var pos1 = new RoomPosition(3, 4, 5);
        var pos2 = new RoomPosition(3, 4, 6); // Different Z

        // Act & Assert
        Assert.AreNotEqual(pos1, pos2);
        Assert.IsFalse(pos1 == pos2);
        Assert.IsTrue(pos1 != pos2);
    }

    [TestMethod]
    public void GetHashCode_SameCoordinates_SameHash()
    {
        // Arrange
        var pos1 = new RoomPosition(3, 4, 5);
        var pos2 = new RoomPosition(3, 4, 5);

        // Act
        var hash1 = pos1.GetHashCode();
        var hash2 = pos2.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var position = new RoomPosition(3, -4, 2);

        // Act
        var result = position.ToString();

        // Assert
        Assert.AreEqual("(3, -4, 2)", result);
    }

    [TestMethod]
    public void ToCompassString_OriginPosition_ReturnsGround()
    {
        // Arrange
        var position = RoomPosition.Origin;

        // Act
        var result = position.ToCompassString();

        // Assert
        Assert.AreEqual("Ground", result);
    }

    [TestMethod]
    public void ToCompassString_NorthEastUp_FormatsCorrectly()
    {
        // Arrange
        var position = new RoomPosition(2, 3, 1);

        // Act
        var result = position.ToCompassString();

        // Assert
        Assert.AreEqual("2E3N 1↑", result);
    }

    [TestMethod]
    public void ToCompassString_SouthWestDown_FormatsCorrectly()
    {
        // Arrange
        var position = new RoomPosition(-1, -2, -3);

        // Act
        var result = position.ToCompassString();

        // Assert
        Assert.AreEqual("1W2S 3↓", result);
    }

    [TestMethod]
    public void ToCompassString_OnlyVertical_ShowsLayerOnly()
    {
        // Arrange
        var position = new RoomPosition(0, 0, -2);

        // Act
        var result = position.ToCompassString();

        // Assert
        Assert.AreEqual("2↓", result);
    }
}
