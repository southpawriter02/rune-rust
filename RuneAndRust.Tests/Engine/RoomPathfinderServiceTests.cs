using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for RoomPathfinderService (v0.3.20c).
/// Validates BFS pathfinding, fog of war constraints, and feature searches.
/// </summary>
public class RoomPathfinderServiceTests
{
    private readonly Mock<ILogger<RoomPathfinderService>> _mockLogger;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly RoomPathfinderService _sut;

    public RoomPathfinderServiceTests()
    {
        _mockLogger = new Mock<ILogger<RoomPathfinderService>>();
        _mockRoomRepository = new Mock<IRoomRepository>();

        _sut = new RoomPathfinderService(
            _mockLogger.Object,
            _mockRoomRepository.Object);
    }

    #region FindPathAsync Tests

    [Fact]
    public async Task FindPath_ReturnsEmpty_WhenAlreadyAtDestination()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var visitedRooms = new HashSet<Guid> { roomId };

        // Act
        var result = await _sut.FindPathAsync(roomId, roomId, visitedRooms);

        // Assert
        result.Success.Should().BeTrue();
        result.RoomPath.Should().NotBeNull();
        result.RoomPath.Should().BeEmpty();
        result.Directions.Should().BeEmpty();
    }

    [Fact]
    public async Task FindPath_ReturnsPath_WhenDirectlyConnected()
    {
        // Arrange
        var startRoom = CreateRoom(0, 0, 0);
        var endRoom = CreateRoom(1, 0, 0);
        startRoom.Exits[Direction.East] = endRoom.Id;

        var visitedRooms = new HashSet<Guid> { startRoom.Id, endRoom.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { startRoom, endRoom });

        // Act
        var result = await _sut.FindPathAsync(startRoom.Id, endRoom.Id, visitedRooms);

        // Assert
        result.Success.Should().BeTrue();
        result.RoomPath.Should().ContainSingle();
        result.RoomPath![0].Should().Be(endRoom.Id);
        result.Directions.Should().ContainSingle();
        result.Directions![0].Should().Be(Direction.East);
    }

    [Fact]
    public async Task FindPath_FindsShortestPath_ThroughMultipleRooms()
    {
        // Arrange: A -> B -> D (short) vs A -> C -> D (also valid but same length)
        //    A --E-- B --E-- D
        //    |              |
        //    S              N
        //    |              |
        //    C -----W------ (no direct to D)
        var roomA = CreateRoom(0, 0, 0);
        var roomB = CreateRoom(1, 0, 0);
        var roomC = CreateRoom(0, -1, 0);
        var roomD = CreateRoom(2, 0, 0);

        roomA.Exits[Direction.East] = roomB.Id;
        roomA.Exits[Direction.South] = roomC.Id;
        roomB.Exits[Direction.West] = roomA.Id;
        roomB.Exits[Direction.East] = roomD.Id;
        roomC.Exits[Direction.North] = roomA.Id;
        roomD.Exits[Direction.West] = roomB.Id;

        var visitedRooms = new HashSet<Guid> { roomA.Id, roomB.Id, roomC.Id, roomD.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA, roomB, roomC, roomD });

        // Act
        var result = await _sut.FindPathAsync(roomA.Id, roomD.Id, visitedRooms);

        // Assert
        result.Success.Should().BeTrue();
        result.RoomPath.Should().HaveCount(2); // B, D
        result.Directions.Should().HaveCount(2); // East, East
        result.Directions![0].Should().Be(Direction.East);
        result.Directions![1].Should().Be(Direction.East);
    }

    [Fact]
    public async Task FindPath_RespectsVisitedConstraint()
    {
        // Arrange: A connects to B connects to C, but B is not visited
        var roomA = CreateRoom(0, 0, 0);
        var roomB = CreateRoom(1, 0, 0);
        var roomC = CreateRoom(2, 0, 0);

        roomA.Exits[Direction.East] = roomB.Id;
        roomB.Exits[Direction.West] = roomA.Id;
        roomB.Exits[Direction.East] = roomC.Id;
        roomC.Exits[Direction.West] = roomB.Id;

        // Only A and C are visited (B is unexplored - fog of war)
        var visitedRooms = new HashSet<Guid> { roomA.Id, roomC.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA, roomC }); // B not returned

        // Act
        var result = await _sut.FindPathAsync(roomA.Id, roomC.Id, visitedRooms);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("No path through explored areas");
    }

    [Fact]
    public async Task FindPath_ReturnsFail_WhenDestinationNotVisited()
    {
        // Arrange
        var startId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var visitedRooms = new HashSet<Guid> { startId }; // Dest not in visited

        // Act
        var result = await _sut.FindPathAsync(startId, destId, visitedRooms);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("not been explored");
    }

    [Fact]
    public async Task FindPath_ReturnsFail_WhenNoPathExists()
    {
        // Arrange: Two disconnected rooms
        var roomA = CreateRoom(0, 0, 0);
        var roomB = CreateRoom(10, 10, 0); // Far away, no exits

        var visitedRooms = new HashSet<Guid> { roomA.Id, roomB.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA, roomB });

        // Act
        var result = await _sut.FindPathAsync(roomA.Id, roomB.Id, visitedRooms);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("No path through explored areas");
    }

    #endregion

    #region FindNearestFeatureAsync Tests

    [Fact]
    public async Task FindNearestFeature_ReturnsClosest()
    {
        // Arrange: A -> B -> C (anchor) and A -> D (anchor)
        // D is closer (1 step) than C (2 steps)
        var roomA = CreateRoom(0, 0, 0);
        var roomB = CreateRoom(1, 0, 0);
        var roomC = CreateRoom(2, 0, 0, RoomFeature.RunicAnchor);
        var roomD = CreateRoom(0, 1, 0, RoomFeature.RunicAnchor);

        roomA.Exits[Direction.East] = roomB.Id;
        roomA.Exits[Direction.North] = roomD.Id;
        roomB.Exits[Direction.East] = roomC.Id;

        var visitedRooms = new HashSet<Guid> { roomA.Id, roomB.Id, roomC.Id, roomD.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA, roomB, roomC, roomD });

        // Act
        var result = await _sut.FindNearestFeatureAsync(roomA.Id, RoomFeature.RunicAnchor, visitedRooms);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(roomD.Id); // D is closer
    }

    [Fact]
    public async Task FindNearestFeature_ReturnsNull_WhenNoneExist()
    {
        // Arrange
        var roomA = CreateRoom(0, 0, 0);
        var roomB = CreateRoom(1, 0, 0); // No feature
        roomA.Exits[Direction.East] = roomB.Id;

        var visitedRooms = new HashSet<Guid> { roomA.Id, roomB.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA, roomB });

        // Act
        var result = await _sut.FindNearestFeatureAsync(roomA.Id, RoomFeature.RunicAnchor, visitedRooms);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindNearestFeature_ReturnsStartRoom_WhenItHasFeature()
    {
        // Arrange
        var roomA = CreateRoom(0, 0, 0, RoomFeature.RunicAnchor);

        var visitedRooms = new HashSet<Guid> { roomA.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { roomA });

        // Act
        var result = await _sut.FindNearestFeatureAsync(roomA.Id, RoomFeature.RunicAnchor, visitedRooms);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(roomA.Id);
    }

    #endregion

    #region FindRoomsByNameAsync Tests

    [Fact]
    public async Task FindRoomsByName_CaseInsensitive()
    {
        // Arrange
        var room1 = CreateRoom(0, 0, 0);
        room1.Name = "Entry Hall";
        var room2 = CreateRoom(1, 0, 0);
        room2.Name = "Rusted Corridor";

        var visitedRooms = new HashSet<Guid> { room1.Id, room2.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { room1, room2 });

        // Act - search with different casing
        var result = await _sut.FindRoomsByNameAsync("ENTRY", visitedRooms);

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("Entry Hall");
    }

    [Fact]
    public async Task FindRoomsByName_PartialMatch()
    {
        // Arrange
        var room1 = CreateRoom(0, 0, 0);
        room1.Name = "Entry Hall";
        var room2 = CreateRoom(1, 0, 0);
        room2.Name = "Grand Entry";

        var visitedRooms = new HashSet<Guid> { room1.Id, room2.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { room1, room2 });

        // Act
        var result = await _sut.FindRoomsByNameAsync("Entry", visitedRooms);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindRoomsByName_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);
        room.Name = "Entry Hall";

        var visitedRooms = new HashSet<Guid> { room.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { room });

        // Act
        var result = await _sut.FindRoomsByNameAsync("Throne", visitedRooms);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static Room CreateRoom(int x, int y, int z, RoomFeature? feature = null)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = $"Room {x},{y},{z}",
            Description = "A test room.",
            Position = new Coordinate(x, y, z),
            BiomeType = BiomeType.Ruin,
            DangerLevel = DangerLevel.Safe
        };

        if (feature.HasValue)
        {
            room.Features.Add(feature.Value);
        }

        return room;
    }

    #endregion
}
