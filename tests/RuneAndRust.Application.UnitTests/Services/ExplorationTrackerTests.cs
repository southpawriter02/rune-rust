using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ExplorationTracker"/>.
/// </summary>
[TestFixture]
public class ExplorationTrackerTests
{
    private ExplorationTracker _tracker = null!;

    [SetUp]
    public void Setup()
    {
        _tracker = new ExplorationTracker();
    }

    #region MarkExplored Tests

    [Test]
    public void MarkExplored_NewRoom_AddsToExploredRooms()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        _tracker.MarkExplored(roomId);

        // Assert
        _tracker.ExploredRooms.Should().Contain(roomId);
        _tracker.IsExplored(roomId).Should().BeTrue();
    }

    [Test]
    public void MarkExplored_SameRoomTwice_DoesNotDuplicate()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        _tracker.MarkExplored(roomId);
        _tracker.MarkExplored(roomId);

        // Assert
        _tracker.ExploredRooms.Count.Should().Be(1);
    }

    [Test]
    public void MarkExplored_RaisesOnRoomExploredEvent()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        Guid? eventRoomId = null;
        _tracker.OnRoomExplored += id => eventRoomId = id;

        // Act
        _tracker.MarkExplored(roomId);

        // Assert
        eventRoomId.Should().Be(roomId);
    }

    [Test]
    public void MarkExplored_RemovesFromKnownAdjacent()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _tracker.MarkKnownAdjacent(roomId);
        _tracker.KnownAdjacentRooms.Should().Contain(roomId);

        // Act
        _tracker.MarkExplored(roomId);

        // Assert
        _tracker.KnownAdjacentRooms.Should().NotContain(roomId);
    }

    #endregion

    #region MarkKnownAdjacent Tests

    [Test]
    public void MarkKnownAdjacent_NewRoom_AddsToKnownAdjacent()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        _tracker.MarkKnownAdjacent(roomId);

        // Assert
        _tracker.KnownAdjacentRooms.Should().Contain(roomId);
    }

    [Test]
    public void MarkKnownAdjacent_AlreadyExplored_DoesNotAdd()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _tracker.MarkExplored(roomId);

        // Act
        _tracker.MarkKnownAdjacent(roomId);

        // Assert
        _tracker.KnownAdjacentRooms.Should().NotContain(roomId);
    }

    #endregion

    #region IsExplored Tests

    [Test]
    public void IsExplored_UnexploredRoom_ReturnsFalse()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act & Assert
        _tracker.IsExplored(roomId).Should().BeFalse();
    }

    [Test]
    public void IsExplored_ExploredRoom_ReturnsTrue()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _tracker.MarkExplored(roomId);

        // Act & Assert
        _tracker.IsExplored(roomId).Should().BeTrue();
    }

    #endregion

    #region Reset Tests

    [Test]
    public void Reset_ClearsAllData()
    {
        // Arrange
        var room1 = Guid.NewGuid();
        var room2 = Guid.NewGuid();
        _tracker.MarkExplored(room1);
        _tracker.MarkKnownAdjacent(room2);

        // Act
        _tracker.Reset();

        // Assert
        _tracker.ExploredRooms.Should().BeEmpty();
        _tracker.KnownAdjacentRooms.Should().BeEmpty();
    }

    #endregion
}
