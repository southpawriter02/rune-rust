using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the ObjectSpawner service.
/// Validates room object population during dungeon generation.
/// </summary>
public class ObjectSpawnerTests
{
    private readonly Mock<ILogger<ObjectSpawner>> _mockLogger;
    private readonly Mock<IInteractableObjectRepository> _mockRepository;
    private readonly ObjectSpawner _sut;

    public ObjectSpawnerTests()
    {
        _mockLogger = new Mock<ILogger<ObjectSpawner>>();
        _mockRepository = new Mock<IInteractableObjectRepository>();
        _sut = new ObjectSpawner(_mockRepository.Object, _mockLogger.Object);
    }

    #region SpawnObjectsInRoomAsync Tests

    [Fact]
    public async Task SpawnObjectsInRoomAsync_SpawnsMinimumTwoObjects()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        var count = await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        count.Should().BeGreaterThanOrEqualTo(2);
        spawnedObjects.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_SpawnsMaximumThreeObjects()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var counts = new List<int>();

        // Act - Run multiple times to verify upper bound
        for (int i = 0; i < 50; i++)
        {
            var count = await _sut.SpawnObjectsInRoomAsync(Guid.NewGuid());
            counts.Add(count);
        }

        // Assert
        counts.Should().OnlyContain(c => c >= 2 && c <= 3);
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ClearsExistingObjects()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        _mockRepository.Verify(r => r.ClearRoomObjectsAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_SavesChanges()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ObjectsHaveCorrectRoomId()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        spawnedObjects.Should().OnlyContain(o => o.RoomId == roomId);
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ObjectsHaveNonEmptyNames()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        spawnedObjects.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o.Name));
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ObjectsHaveDescriptions()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        await _sut.SpawnObjectsInRoomAsync(roomId);

        // Assert
        spawnedObjects.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o.Description));
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ContainersAreMarkedCorrectly()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act - Run multiple times to increase chance of getting containers
        for (int i = 0; i < 20; i++)
        {
            await _sut.SpawnObjectsInRoomAsync(Guid.NewGuid());
        }

        // Assert
        var containers = spawnedObjects.Where(o => o.ObjectType == ObjectType.Container).ToList();
        containers.Should().OnlyContain(o => o.IsContainer == true);
    }

    [Theory]
    [InlineData(BiomeType.Ruin)]
    [InlineData(BiomeType.Industrial)]
    [InlineData(BiomeType.Organic)]
    [InlineData(BiomeType.Void)]
    public async Task SpawnObjectsInRoomAsync_AcceptsAllBiomeTypes(BiomeType biome)
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var count = await _sut.SpawnObjectsInRoomAsync(roomId, biome);

        // Assert
        count.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region SpawnObjectsInRoomsAsync Tests

    [Fact]
    public async Task SpawnObjectsInRoomsAsync_SpawnsInAllRooms()
    {
        // Arrange
        var roomIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        var totalCount = await _sut.SpawnObjectsInRoomsAsync(roomIds);

        // Assert
        totalCount.Should().BeGreaterThanOrEqualTo(6); // At least 2 per room * 3 rooms
        _mockRepository.Verify(r => r.ClearRoomObjectsAsync(It.IsAny<Guid>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SpawnObjectsInRoomsAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var roomIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var callCount = 0;
        var perRoomCounts = new List<int>();

        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs =>
            {
                perRoomCounts.Add(objs.Count());
            });

        // Act
        var totalCount = await _sut.SpawnObjectsInRoomsAsync(roomIds);

        // Assert
        totalCount.Should().Be(perRoomCounts.Sum());
    }

    [Fact]
    public async Task SpawnObjectsInRoomsAsync_EmptyList_ReturnsZero()
    {
        // Arrange
        var roomIds = Enumerable.Empty<Guid>();

        // Act
        var totalCount = await _sut.SpawnObjectsInRoomsAsync(roomIds);

        // Assert
        totalCount.Should().Be(0);
    }

    #endregion

    #region Object Variety Tests

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ProducesVariedObjectTypes()
    {
        // Arrange
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act - Spawn many objects across multiple rooms
        for (int i = 0; i < 50; i++)
        {
            await _sut.SpawnObjectsInRoomAsync(Guid.NewGuid());
        }

        // Assert - Should see multiple different object types
        var objectTypes = spawnedObjects.Select(o => o.ObjectType).Distinct().ToList();
        objectTypes.Count.Should().BeGreaterThan(2, "should produce varied object types");
    }

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ProducesVariedObjectNames()
    {
        // Arrange
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act - Spawn many objects
        for (int i = 0; i < 50; i++)
        {
            await _sut.SpawnObjectsInRoomAsync(Guid.NewGuid());
        }

        // Assert - Should see multiple different names
        var uniqueNames = spawnedObjects.Select(o => o.Name).Distinct().ToList();
        uniqueNames.Count.Should().BeGreaterThan(5, "should produce varied object names");
    }

    #endregion

    #region Description Quality Tests

    [Fact]
    public async Task SpawnObjectsInRoomAsync_ObjectsHaveThreeTierDescriptions()
    {
        // Arrange
        var spawnedObjects = new List<InteractableObject>();
        _mockRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<InteractableObject>>()))
            .Callback<IEnumerable<InteractableObject>>(objs => spawnedObjects.AddRange(objs));

        // Act
        await _sut.SpawnObjectsInRoomAsync(Guid.NewGuid());

        // Assert - All objects should have base, detailed, and expert descriptions
        foreach (var obj in spawnedObjects)
        {
            obj.Description.Should().NotBeNullOrWhiteSpace($"{obj.Name} should have base description");
            obj.DetailedDescription.Should().NotBeNullOrWhiteSpace($"{obj.Name} should have detailed description");
            obj.ExpertDescription.Should().NotBeNullOrWhiteSpace($"{obj.Name} should have expert description");
        }
    }

    #endregion
}
