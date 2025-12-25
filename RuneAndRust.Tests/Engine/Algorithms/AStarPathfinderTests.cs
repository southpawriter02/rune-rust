using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Algorithms;

namespace RuneAndRust.Tests.Engine.Algorithms;

/// <summary>
/// Unit tests for the AStarPathfinder service (v0.3.18b - The Hot Path).
/// Verifies pathfinding logic, obstacle avoidance, and Manhattan distance calculations.
/// </summary>
public class AStarPathfinderTests
{
    private readonly AStarPathfinder _sut;
    private readonly ILogger<AStarPathfinder> _logger;
    private readonly ISpatialHashGrid _mockGrid;

    public AStarPathfinderTests()
    {
        _logger = Substitute.For<ILogger<AStarPathfinder>>();
        _mockGrid = Substitute.For<ISpatialHashGrid>();
        _sut = new AStarPathfinder(_logger);

        // By default, no positions are blocked
        _mockGrid.IsBlocked(Arg.Any<Coordinate>()).Returns(false);
    }

    #region FindPath - Basic Tests

    [Fact]
    public void FindPath_SameStartAndEnd_ReturnsEmptyPath()
    {
        // Arrange
        var position = new Coordinate(5, 5, 0);

        // Act
        var result = _sut.FindPath(position, position, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("no movement needed when start equals end");
    }

    [Fact]
    public void FindPath_AdjacentPositions_ReturnsSingleStep()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(1, 0, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Should().Be(end);
    }

    [Fact]
    public void FindPath_StraightLine_ReturnsDirectPath()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(5, 0, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5, "5 steps to move 5 units on X axis");
        result![^1].Should().Be(end, "path should end at destination");
    }

    [Fact]
    public void FindPath_DiagonalMovement_UsesManhattanPath()
    {
        // Arrange - 4-directional movement means diagonal is 2 steps
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(1, 1, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2, "diagonal requires 2 cardinal moves");
        result![^1].Should().Be(end);
    }

    [Fact]
    public void FindPath_PathExcludesStart_IncludesEnd()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(2, 0, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain(start, "path should not include starting position");
        result.Should().Contain(end, "path should include destination");
    }

    #endregion

    #region FindPath - Obstacle Tests

    [Fact]
    public void FindPath_EndPositionBlocked_ReturnsNull()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(5, 0, 0);
        _mockGrid.IsBlocked(end).Returns(true);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().BeNull("cannot reach a blocked destination");
    }

    [Fact]
    public void FindPath_AroundSingleObstacle_FindsAlternatePath()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(2, 0, 0);
        var obstacle = new Coordinate(1, 0, 0);
        _mockGrid.IsBlocked(obstacle).Returns(true);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain(obstacle, "path should avoid blocked position");
        result![^1].Should().Be(end);
    }

    [Fact]
    public void FindPath_AroundWall_FindsLongerPath()
    {
        // Arrange - Wall blocking direct path at Y=0
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(4, 0, 0);

        // Block positions 1,0 and 2,0 and 3,0
        _mockGrid.IsBlocked(new Coordinate(1, 0, 0)).Returns(true);
        _mockGrid.IsBlocked(new Coordinate(2, 0, 0)).Returns(true);
        _mockGrid.IsBlocked(new Coordinate(3, 0, 0)).Returns(true);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull("should find path around wall");
        result.Should().HaveCountGreaterThan(4, "detour should be longer than direct path");
        result![^1].Should().Be(end);
    }

    [Fact]
    public void FindPath_CompletelyBlocked_ReturnsNull()
    {
        // Arrange - Surround start position with obstacles
        var start = new Coordinate(5, 5, 0);
        var end = new Coordinate(10, 10, 0);

        // Block all 4 neighbors of start
        _mockGrid.IsBlocked(new Coordinate(5, 6, 0)).Returns(true);  // North
        _mockGrid.IsBlocked(new Coordinate(5, 4, 0)).Returns(true);  // South
        _mockGrid.IsBlocked(new Coordinate(6, 5, 0)).Returns(true);  // East
        _mockGrid.IsBlocked(new Coordinate(4, 5, 0)).Returns(true);  // West

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().BeNull("no path exists when start is surrounded");
    }

    #endregion

    #region GetDistance Tests

    [Fact]
    public void GetDistance_SamePosition_ReturnsZero()
    {
        // Arrange
        var position = new Coordinate(5, 5, 0);

        // Act
        var result = _sut.GetDistance(position, position);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetDistance_HorizontalDistance_CalculatesCorrectly()
    {
        // Arrange
        var a = new Coordinate(0, 0, 0);
        var b = new Coordinate(5, 0, 0);

        // Act
        var result = _sut.GetDistance(a, b);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetDistance_VerticalDistance_CalculatesCorrectly()
    {
        // Arrange
        var a = new Coordinate(0, 0, 0);
        var b = new Coordinate(0, 7, 0);

        // Act
        var result = _sut.GetDistance(a, b);

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void GetDistance_DiagonalDistance_ReturnsManhattan()
    {
        // Arrange
        var a = new Coordinate(0, 0, 0);
        var b = new Coordinate(3, 4, 0);

        // Act
        var result = _sut.GetDistance(a, b);

        // Assert
        result.Should().Be(7, "Manhattan distance is |3| + |4| = 7");
    }

    [Fact]
    public void GetDistance_NegativeCoordinates_CalculatesAbsoluteDistance()
    {
        // Arrange
        var a = new Coordinate(-3, -4, 0);
        var b = new Coordinate(3, 4, 0);

        // Act
        var result = _sut.GetDistance(a, b);

        // Assert
        result.Should().Be(14, "Manhattan distance is |6| + |8| = 14");
    }

    [Fact]
    public void GetDistance_IncludesZAxis()
    {
        // Arrange
        var a = new Coordinate(0, 0, 0);
        var b = new Coordinate(1, 1, 1);

        // Act
        var result = _sut.GetDistance(a, b);

        // Assert
        result.Should().Be(3, "Manhattan distance includes Z: |1| + |1| + |1| = 3");
    }

    [Fact]
    public void GetDistance_IsSymmetric()
    {
        // Arrange
        var a = new Coordinate(2, 5, 0);
        var b = new Coordinate(8, 3, 0);

        // Act
        var distanceAB = _sut.GetDistance(a, b);
        var distanceBA = _sut.GetDistance(b, a);

        // Assert
        distanceAB.Should().Be(distanceBA, "distance should be symmetric");
    }

    #endregion

    #region HasPath Tests

    [Fact]
    public void HasPath_PathExists_ReturnsTrue()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(5, 5, 0);

        // Act
        var result = _sut.HasPath(start, end, _mockGrid);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPath_NoPathExists_ReturnsFalse()
    {
        // Arrange
        var start = new Coordinate(5, 5, 0);
        var end = new Coordinate(10, 10, 0);

        // Block all neighbors
        _mockGrid.IsBlocked(new Coordinate(5, 6, 0)).Returns(true);
        _mockGrid.IsBlocked(new Coordinate(5, 4, 0)).Returns(true);
        _mockGrid.IsBlocked(new Coordinate(6, 5, 0)).Returns(true);
        _mockGrid.IsBlocked(new Coordinate(4, 5, 0)).Returns(true);

        // Act
        var result = _sut.HasPath(start, end, _mockGrid);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPath_SamePosition_ReturnsTrue()
    {
        // Arrange
        var position = new Coordinate(5, 5, 0);

        // Act
        var result = _sut.HasPath(position, position, _mockGrid);

        // Assert
        result.Should().BeTrue("already at destination");
    }

    #endregion

    #region Path Quality Tests

    [Fact]
    public void FindPath_ReturnsOptimalPath()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(3, 2, 0);
        var expectedLength = 5; // Manhattan distance

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(expectedLength, "A* should find optimal path");
    }

    [Fact]
    public void FindPath_PathIsContiguous()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(4, 3, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();

        // Verify each step is adjacent to the previous
        var current = start;
        foreach (var step in result!)
        {
            var distance = _sut.GetDistance(current, step);
            distance.Should().Be(1, $"each step should be adjacent (from {current} to {step})");
            current = step;
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FindPath_LargePath_Completes()
    {
        // Arrange
        var start = new Coordinate(0, 0, 0);
        var end = new Coordinate(50, 50, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(100, "50 + 50 steps for unobstructed path");
    }

    [Fact]
    public void FindPath_NegativeCoordinates_Works()
    {
        // Arrange
        var start = new Coordinate(-5, -5, 0);
        var end = new Coordinate(5, 5, 0);

        // Act
        var result = _sut.FindPath(start, end, _mockGrid);

        // Assert
        result.Should().NotBeNull();
        result![^1].Should().Be(end);
    }

    #endregion
}
