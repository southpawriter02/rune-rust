using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for Room entity persistence.
/// Uses InMemory database to test RoomRepository operations.
/// </summary>
public class RoomPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly RoomRepository _repository;

    public RoomPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var genericLoggerMock = new Mock<ILogger<GenericRepository<Room>>>();
        var roomLoggerMock = new Mock<ILogger<RoomRepository>>();

        _repository = new RoomRepository(_context, genericLoggerMock.Object, roomLoggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistRoom()
    {
        // Arrange
        var room = new Room
        {
            Name = "Test Room",
            Description = "A test room.",
            Position = new Coordinate(1, 2, 3)
        };

        // Act
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(room.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Room");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCoordinate()
    {
        // Arrange
        var position = new Coordinate(5, 10, -3);
        var room = new Room
        {
            Name = "Positioned Room",
            Position = position
        };

        // Act
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(room.Id);
        retrieved!.Position.X.Should().Be(5);
        retrieved.Position.Y.Should().Be(10);
        retrieved.Position.Z.Should().Be(-3);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistExits()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var room = new Room
        {
            Name = "Room With Exits",
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, targetId },
                { Direction.Down, Guid.NewGuid() }
            }
        };

        // Act
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(room.Id);
        retrieved!.Exits.Should().HaveCount(2);
        retrieved.Exits[Direction.North].Should().Be(targetId);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingRoom_ReturnsRoom()
    {
        // Arrange
        var room = new Room { Name = "Find Me" };
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(room.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByPositionAsync Tests

    [Fact]
    public async Task GetByPositionAsync_ExistingPosition_ReturnsRoom()
    {
        // Arrange
        var position = new Coordinate(7, 8, 9);
        var room = new Room
        {
            Name = "Positioned",
            Position = position
        };
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPositionAsync(position);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Positioned");
    }

    [Fact]
    public async Task GetByPositionAsync_NonExistentPosition_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByPositionAsync(new Coordinate(999, 999, 999));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPositionAsync_MatchesExactPosition()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "Room1", Position = new Coordinate(0, 0, 0) });
        await _repository.AddAsync(new Room { Name = "Room2", Position = new Coordinate(0, 0, 1) });
        await _repository.AddAsync(new Room { Name = "Room3", Position = new Coordinate(0, 1, 0) });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPositionAsync(new Coordinate(0, 0, 1));

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Room2");
    }

    #endregion

    #region GetStartingRoomAsync Tests

    [Fact]
    public async Task GetStartingRoomAsync_WithStartingRoom_ReturnsIt()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "Regular", IsStartingRoom = false });
        await _repository.AddAsync(new Room { Name = "Start", IsStartingRoom = true });
        await _repository.AddAsync(new Room { Name = "Another", IsStartingRoom = false });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetStartingRoomAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Start");
        result.IsStartingRoom.Should().BeTrue();
    }

    [Fact]
    public async Task GetStartingRoomAsync_NoStartingRoom_ReturnsNull()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "Regular1", IsStartingRoom = false });
        await _repository.AddAsync(new Room { Name = "Regular2", IsStartingRoom = false });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetStartingRoomAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region PositionExistsAsync Tests

    [Fact]
    public async Task PositionExistsAsync_ExistingPosition_ReturnsTrue()
    {
        // Arrange
        await _repository.AddAsync(new Room { Position = new Coordinate(1, 2, 3) });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.PositionExistsAsync(new Coordinate(1, 2, 3));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PositionExistsAsync_NonExistentPosition_ReturnsFalse()
    {
        // Act
        var result = await _repository.PositionExistsAsync(new Coordinate(100, 200, 300));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllRoomsAsync Tests

    [Fact]
    public async Task GetAllRoomsAsync_MultipleRooms_ReturnsAll()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "Room1" });
        await _repository.AddAsync(new Room { Name = "Room2" });
        await _repository.AddAsync(new Room { Name = "Room3" });
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllRoomsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllRoomsAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetAllRoomsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsOrderedByPosition()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "C", Position = new Coordinate(0, 0, 1) });
        await _repository.AddAsync(new Room { Name = "A", Position = new Coordinate(0, 0, -1) });
        await _repository.AddAsync(new Room { Name = "B", Position = new Coordinate(0, 0, 0) });
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllRoomsAsync()).ToList();

        // Assert
        result[0].Name.Should().Be("A"); // Z = -1
        result[1].Name.Should().Be("B"); // Z = 0
        result[2].Name.Should().Be("C"); // Z = 1
    }

    #endregion

    #region ClearAllRoomsAsync Tests

    [Fact]
    public async Task ClearAllRoomsAsync_RemovesAllRooms()
    {
        // Arrange
        await _repository.AddAsync(new Room { Name = "Room1" });
        await _repository.AddAsync(new Room { Name = "Room2" });
        await _repository.SaveChangesAsync();

        // Act
        await _repository.ClearAllRoomsAsync();

        // Assert
        var remaining = await _repository.GetAllRoomsAsync();
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAllRoomsAsync_EmptyDatabase_DoesNotThrow()
    {
        // Act
        var action = () => _repository.ClearAllRoomsAsync();

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleRooms()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { Name = "Batch1" },
            new Room { Name = "Batch2" },
            new Room { Name = "Batch3" }
        };

        // Act
        await _repository.AddRangeAsync(rooms);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _repository.GetAllRoomsAsync();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_DoesNotThrow()
    {
        // Act
        var action = async () =>
        {
            await _repository.AddRangeAsync(new List<Room>());
            await _repository.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesRoomProperties()
    {
        // Arrange
        var room = new Room { Name = "Original" };
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Act
        room.Name = "Updated";
        room.Description = "New description";
        await _repository.UpdateAsync(room);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(room.Id);
        retrieved!.Name.Should().Be("Updated");
        retrieved.Description.Should().Be("New description");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExits()
    {
        // Arrange
        var room = new Room { Name = "WithExits" };
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Act
        room.Exits[Direction.North] = Guid.NewGuid();
        await _repository.UpdateAsync(room);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(room.Id);
        retrieved!.Exits.Should().ContainKey(Direction.North);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesRoom()
    {
        // Arrange
        var room = new Room { Name = "ToDelete" };
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(room.Id);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _repository.GetByIdAsync(room.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        // Act
        var action = async () =>
        {
            await _repository.DeleteAsync(Guid.NewGuid());
            await _repository.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Room Entity Defaults Tests

    [Fact]
    public void Room_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Id.Should().NotBe(Guid.Empty);
        room.Name.Should().BeEmpty();
        room.Description.Should().BeEmpty();
        room.Position.Should().Be(Coordinate.Origin);
        room.Exits.Should().BeEmpty();
        room.IsStartingRoom.Should().BeFalse();
    }

    #endregion
}
