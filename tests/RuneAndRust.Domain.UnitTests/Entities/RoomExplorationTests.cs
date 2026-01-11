using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room exploration state tracking.
/// </summary>
[TestFixture]
public class RoomExplorationTests
{
    private Room CreateTestRoom()
    {
        return new Room("Test Room", "A test room.", Position3D.Origin);
    }

    [Test]
    public void ExplorationState_DefaultsToUnexplored()
    {
        // Arrange & Act
        var room = CreateTestRoom();

        // Assert
        room.ExplorationState.Should().Be(ExplorationState.Unexplored);
        room.IsVisited.Should().BeFalse();
        room.IsCleared.Should().BeFalse();
    }

    [Test]
    public void MarkVisited_FromUnexplored_ReturnsTrue()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var result = room.MarkVisited();

        // Assert
        result.Should().BeTrue();
        room.ExplorationState.Should().Be(ExplorationState.Visited);
        room.IsVisited.Should().BeTrue();
    }

    [Test]
    public void MarkVisited_AlreadyVisited_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();

        // Act
        var result = room.MarkVisited();

        // Assert
        result.Should().BeFalse();
        room.ExplorationState.Should().Be(ExplorationState.Visited);
    }

    [Test]
    public void MarkCleared_WhenCanBeCleared_ReturnsTrue()
    {
        // Arrange - create visited room with no monsters, no hidden exits
        var room = CreateTestRoom();
        room.MarkVisited();

        // Act
        var result = room.MarkCleared();

        // Assert
        result.Should().BeTrue();
        room.ExplorationState.Should().Be(ExplorationState.Cleared);
        room.IsCleared.Should().BeTrue();
    }

    [Test]
    public void MarkCleared_AlreadyCleared_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();
        room.MarkCleared();

        // Act
        var result = room.MarkCleared();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void MarkCleared_NotVisited_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var result = room.MarkCleared();

        // Assert
        result.Should().BeFalse();
        room.ExplorationState.Should().Be(ExplorationState.Unexplored);
    }

    [Test]
    public void CanBeCleared_VisitedNoMonstersNoHidden_ReturnsTrue()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();

        // Act & Assert
        room.CanBeCleared.Should().BeTrue();
    }

    [Test]
    public void CanBeCleared_NotVisited_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act & Assert
        room.CanBeCleared.Should().BeFalse();
    }

    [Test]
    public void CanBeCleared_HasHiddenExits_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();
        room.AddHiddenExit(Direction.North, Guid.NewGuid(), 15);

        // Act & Assert
        room.CanBeCleared.Should().BeFalse();
    }

    [Test]
    public void TryAutoCleared_CanBeCleared_ClearsRoom()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();

        // Act
        var result = room.TryAutoCleared();

        // Assert
        result.Should().BeTrue();
        room.IsCleared.Should().BeTrue();
    }

    [Test]
    public void TryAutoCleared_AlreadyCleared_ReturnsFalse()
    {
        // Arrange
        var room = CreateTestRoom();
        room.MarkVisited();
        room.MarkCleared();

        // Act
        var result = room.TryAutoCleared();

        // Assert
        result.Should().BeFalse();
    }
}
