using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Performance;

namespace RuneAndRust.Tests.Engine.Performance;

/// <summary>
/// Unit tests for the SpatialHashGrid service (v0.3.18b - The Hot Path).
/// Verifies O(1) position lookups, collision detection, and thread safety.
/// </summary>
public class SpatialHashGridTests
{
    private readonly SpatialHashGrid _sut;
    private readonly ILogger<SpatialHashGrid> _logger;

    public SpatialHashGridTests()
    {
        _logger = Substitute.For<ILogger<SpatialHashGrid>>();
        _sut = new SpatialHashGrid(_logger);
    }

    #region Register Tests

    [Fact]
    public void Register_AddsEntityToGrid()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Coordinate(5, 5, 0);

        // Act
        _sut.Register(entityId, position);

        // Assert
        _sut.Count.Should().Be(1);
        _sut.IsBlocked(position).Should().BeTrue();
        _sut.GetEntityAt(position).Should().Be(entityId);
    }

    [Fact]
    public void Register_MultipleEntities_TracksAllPositions()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var entity3 = Guid.NewGuid();
        var pos1 = new Coordinate(0, 0, 0);
        var pos2 = new Coordinate(1, 0, 0);
        var pos3 = new Coordinate(0, 1, 0);

        // Act
        _sut.Register(entity1, pos1);
        _sut.Register(entity2, pos2);
        _sut.Register(entity3, pos3);

        // Assert
        _sut.Count.Should().Be(3);
        _sut.GetEntityAt(pos1).Should().Be(entity1);
        _sut.GetEntityAt(pos2).Should().Be(entity2);
        _sut.GetEntityAt(pos3).Should().Be(entity3);
    }

    [Fact]
    public void Register_ThrowsOnCollision()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var position = new Coordinate(5, 5, 0);
        _sut.Register(entity1, position);

        // Act
        Action act = () => _sut.Register(entity2, position);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already occupied*");
    }

    #endregion

    #region Move Tests

    [Fact]
    public void Move_UpdatesEntityPosition()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var oldPos = new Coordinate(0, 0, 0);
        var newPos = new Coordinate(1, 0, 0);
        _sut.Register(entityId, oldPos);

        // Act
        _sut.Move(entityId, oldPos, newPos);

        // Assert
        _sut.IsBlocked(oldPos).Should().BeFalse("old position should be empty");
        _sut.IsBlocked(newPos).Should().BeTrue("new position should be occupied");
        _sut.GetEntityAt(newPos).Should().Be(entityId);
    }

    [Fact]
    public void Move_ThrowsWhenNewPositionOccupied()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var pos1 = new Coordinate(0, 0, 0);
        var pos2 = new Coordinate(1, 0, 0);
        _sut.Register(entity1, pos1);
        _sut.Register(entity2, pos2);

        // Act
        Action act = () => _sut.Move(entity1, pos1, pos2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already occupied*");
    }

    [Fact]
    public void Move_ToSamePosition_DoesNotThrow()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Coordinate(5, 5, 0);
        _sut.Register(entityId, position);

        // Act
        Action act = () => _sut.Move(entityId, position, position);

        // Assert
        act.Should().NotThrow();
        _sut.GetEntityAt(position).Should().Be(entityId);
    }

    [Fact]
    public void Move_WhenEntityNotAtOldPosition_DoesNothing()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var actualPos = new Coordinate(0, 0, 0);
        var wrongOldPos = new Coordinate(5, 5, 0);
        var newPos = new Coordinate(1, 0, 0);
        _sut.Register(entityId, actualPos);

        // Act
        _sut.Move(entityId, wrongOldPos, newPos);

        // Assert - Entity should still be at original position
        _sut.GetEntityAt(actualPos).Should().Be(entityId);
        _sut.IsBlocked(newPos).Should().BeFalse();
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ClearsPosition()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Coordinate(5, 5, 0);
        _sut.Register(entityId, position);

        // Act
        _sut.Remove(entityId, position);

        // Assert
        _sut.Count.Should().Be(0);
        _sut.IsBlocked(position).Should().BeFalse();
        _sut.GetEntityAt(position).Should().BeNull();
    }

    [Fact]
    public void Remove_WrongEntity_DoesNotRemove()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var position = new Coordinate(5, 5, 0);
        _sut.Register(entity1, position);

        // Act
        _sut.Remove(entity2, position);

        // Assert - Position should still have entity1
        _sut.IsBlocked(position).Should().BeTrue();
        _sut.GetEntityAt(position).Should().Be(entity1);
    }

    #endregion

    #region Query Tests

    [Fact]
    public void IsBlocked_ReturnsTrueForOccupied()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var occupied = new Coordinate(5, 5, 0);
        var empty = new Coordinate(0, 0, 0);
        _sut.Register(entityId, occupied);

        // Act & Assert
        _sut.IsBlocked(occupied).Should().BeTrue();
        _sut.IsBlocked(empty).Should().BeFalse();
    }

    [Fact]
    public void GetEntityAt_ReturnsCorrectGuid()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Coordinate(3, 4, 0);
        _sut.Register(entityId, position);

        // Act
        var result = _sut.GetEntityAt(position);

        // Assert
        result.Should().Be(entityId);
    }

    [Fact]
    public void GetEntityAt_ReturnsNullForEmpty()
    {
        // Arrange
        var emptyPosition = new Coordinate(99, 99, 0);

        // Act
        var result = _sut.GetEntityAt(emptyPosition);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetEntityPosition_ReturnsCorrectPosition()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Coordinate(7, 8, 0);
        _sut.Register(entityId, position);

        // Act
        var result = _sut.GetEntityPosition(entityId);

        // Assert
        result.Should().Be(position);
    }

    [Fact]
    public void GetEntityPosition_ReturnsNullForUnregistered()
    {
        // Arrange
        var unknownEntity = Guid.NewGuid();

        // Act
        var result = _sut.GetEntityPosition(unknownEntity);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_RemovesAllEntities()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _sut.Register(Guid.NewGuid(), new Coordinate(i, 0, 0));
        }
        _sut.Count.Should().Be(10);

        // Act
        _sut.Clear();

        // Assert
        _sut.Count.Should().Be(0);
        _sut.IsBlocked(new Coordinate(5, 0, 0)).Should().BeFalse();
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void ThreadSafety_ConcurrentOperations_NoExceptions()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 100;
        var exceptions = new List<Exception>();
        var startPosition = 1000; // Start positions high to avoid collisions

        // Act
        var threads = Enumerable.Range(0, threadCount).Select(threadIndex => new Thread(() =>
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var entityId = Guid.NewGuid();
                    var position = new Coordinate(startPosition + threadIndex * 1000 + i, 0, 0);

                    _sut.Register(entityId, position);
                    _sut.IsBlocked(position);
                    _sut.GetEntityAt(position);
                    _sut.Remove(entityId, position);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // Assert
        exceptions.Should().BeEmpty("concurrent access should not cause exceptions");
    }

    #endregion
}
