using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Dungeon level query methods.
/// </summary>
[TestFixture]
public class DungeonLevelTests
{
    private Dungeon CreateDungeonWithMultipleLevels()
    {
        var dungeon = new Dungeon("Test Dungeon");

        // Level 0 (Z=0) - 3 rooms
        var room1 = new Room("Room 1", "First room", new Position3D(0, 0, 0));
        var room2 = new Room("Room 2", "Second room", new Position3D(1, 0, 0));
        var room3 = new Room("Room 3", "Third room", new Position3D(0, 1, 0));

        // Level 1 (Z=1) - 2 rooms
        var room4 = new Room("Room 4", "Fourth room", new Position3D(0, 0, 1));
        var room5 = new Room("Room 5", "Fifth room", new Position3D(1, 0, 1));

        dungeon.AddRoom(room1, isStartingRoom: true);
        dungeon.AddRoom(room2);
        dungeon.AddRoom(room3);
        dungeon.AddRoom(room4);
        dungeon.AddRoom(room5);

        return dungeon;
    }

    [Test]
    public void GetRoomsOnLevel_ReturnsCorrectRooms()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();

        // Act
        var level0Rooms = dungeon.GetRoomsOnLevel(0);
        var level1Rooms = dungeon.GetRoomsOnLevel(1);

        // Assert
        level0Rooms.Should().HaveCount(3);
        level1Rooms.Should().HaveCount(2);
        level0Rooms.All(r => r.Position.Z == 0).Should().BeTrue();
        level1Rooms.All(r => r.Position.Z == 1).Should().BeTrue();
    }

    [Test]
    public void GetRoomsOnLevel_EmptyLevel_ReturnsEmpty()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();

        // Act
        var level2Rooms = dungeon.GetRoomsOnLevel(2);

        // Assert
        level2Rooms.Should().BeEmpty();
    }

    [Test]
    public void GetExploredLevels_ReturnsOnlyVisitedLevels()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();
        var rooms = dungeon.Rooms.Values.ToList();

        // Mark some rooms as visited on level 0
        rooms[0].MarkVisited();
        rooms[1].MarkVisited();

        // Act
        var exploredLevels = dungeon.GetExploredLevels();

        // Assert
        exploredLevels.Should().HaveCount(1);
        exploredLevels.Should().Contain(0);
        exploredLevels.Should().NotContain(1);
    }

    [Test]
    public void GetExploredLevels_MultipleVisitedLevels_ReturnsSorted()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();
        var rooms = dungeon.Rooms.Values.ToList();

        // Mark rooms on both levels as visited
        rooms.First(r => r.Position.Z == 0).MarkVisited();
        rooms.First(r => r.Position.Z == 1).MarkVisited();

        // Act
        var exploredLevels = dungeon.GetExploredLevels();

        // Assert
        exploredLevels.Should().BeEquivalentTo(new[] { 0, 1 });
        exploredLevels.Should().BeInAscendingOrder();
    }

    [Test]
    public void GetLevelBounds_ReturnsCorrectBounds()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();

        // Act
        var (minX, maxX, minY, maxY) = dungeon.GetLevelBounds(0);

        // Assert
        minX.Should().Be(0);
        maxX.Should().Be(1);
        minY.Should().Be(0);
        maxY.Should().Be(1);
    }

    [Test]
    public void GetLevelBounds_EmptyLevel_ReturnsZeros()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();

        // Act
        var (minX, maxX, minY, maxY) = dungeon.GetLevelBounds(99);

        // Assert
        minX.Should().Be(0);
        maxX.Should().Be(0);
        minY.Should().Be(0);
        maxY.Should().Be(0);
    }

    [Test]
    public void GetExplorationStatsForLevel_ReturnsCorrectCounts()
    {
        // Arrange
        var dungeon = CreateDungeonWithMultipleLevels();
        var rooms = dungeon.Rooms.Values.Where(r => r.Position.Z == 0).ToList();

        // Mark first room as visited, second as cleared
        rooms[0].MarkVisited();
        rooms[1].MarkVisited();
        rooms[1].MarkCleared();

        // Act
        var stats = dungeon.GetExplorationStatsForLevel(0);

        // Assert
        stats.Should().ContainKey(ExplorationState.Unexplored);
        stats[ExplorationState.Unexplored].Should().Be(1);
        stats[ExplorationState.Visited].Should().Be(1);
        stats[ExplorationState.Cleared].Should().Be(1);
    }
}
