using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for InteractableObject entity persistence.
/// Uses InMemory database to test InteractableObjectRepository operations.
/// </summary>
public class InteractableObjectPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly InteractableObjectRepository _repository;
    private readonly Guid _testRoomId = Guid.NewGuid();

    public InteractableObjectPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var genericLoggerMock = new Mock<ILogger<GenericRepository<InteractableObject>>>();
        var objectLoggerMock = new Mock<ILogger<InteractableObjectRepository>>();

        _repository = new InteractableObjectRepository(_context, genericLoggerMock.Object, objectLoggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistObject()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");

        // Act
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(obj.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Rusted Chest");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllProperties()
    {
        // Arrange
        var obj = new InteractableObject
        {
            RoomId = _testRoomId,
            Name = "Full Object",
            ObjectType = ObjectType.Container,
            Description = "Base description.",
            DetailedDescription = "Detailed description.",
            ExpertDescription = "Expert description.",
            IsContainer = true,
            IsOpen = true,
            IsLocked = true,
            LockDifficulty = 3,
            HasBeenExamined = true,
            HighestExaminationTier = 2
        };

        // Act
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(obj.Id);
        retrieved.Should().NotBeNull();
        retrieved!.RoomId.Should().Be(_testRoomId);
        retrieved.ObjectType.Should().Be(ObjectType.Container);
        retrieved.Description.Should().Be("Base description.");
        retrieved.DetailedDescription.Should().Be("Detailed description.");
        retrieved.ExpertDescription.Should().Be("Expert description.");
        retrieved.IsContainer.Should().BeTrue();
        retrieved.IsOpen.Should().BeTrue();
        retrieved.IsLocked.Should().BeTrue();
        retrieved.LockDifficulty.Should().Be(3);
        retrieved.HasBeenExamined.Should().BeTrue();
        retrieved.HighestExaminationTier.Should().Be(2);
    }

    #endregion

    #region GetByRoomIdAsync Tests

    [Fact]
    public async Task GetByRoomIdAsync_ReturnsObjectsInRoom()
    {
        // Arrange
        var obj1 = CreateTestObject("Object 1");
        var obj2 = CreateTestObject("Object 2");
        var objOtherRoom = CreateTestObject("Other Room Object");
        objOtherRoom.RoomId = Guid.NewGuid();

        await _repository.AddAsync(obj1);
        await _repository.AddAsync(obj2);
        await _repository.AddAsync(objOtherRoom);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRoomIdAsync(_testRoomId);

        // Assert
        result.Should().HaveCount(2);
        result.Select(o => o.Name).Should().Contain("Object 1");
        result.Select(o => o.Name).Should().Contain("Object 2");
    }

    [Fact]
    public async Task GetByRoomIdAsync_EmptyRoom_ReturnsEmptyList()
    {
        // Arrange
        var emptyRoomId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByRoomIdAsync(emptyRoomId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRoomIdAsync_ReturnsOrderedByName()
    {
        // Arrange
        await _repository.AddAsync(CreateTestObject("Zebra"));
        await _repository.AddAsync(CreateTestObject("Alpha"));
        await _repository.AddAsync(CreateTestObject("Middle"));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRoomIdAsync(_testRoomId);

        // Assert
        result.Select(o => o.Name).Should().BeInAscendingOrder();
    }

    #endregion

    #region GetByNameInRoomAsync Tests

    [Fact]
    public async Task GetByNameInRoomAsync_FindsExactMatch()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameInRoomAsync(_testRoomId, "Rusted Chest");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Rusted Chest");
    }

    [Fact]
    public async Task GetByNameInRoomAsync_CaseInsensitive()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameInRoomAsync(_testRoomId, "rusted chest");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByNameInRoomAsync_TrimsWhitespace()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameInRoomAsync(_testRoomId, "  Rusted Chest  ");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByNameInRoomAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameInRoomAsync(_testRoomId, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameInRoomAsync_WrongRoom_ReturnsNull()
    {
        // Arrange
        var obj = CreateTestObject("Rusted Chest");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameInRoomAsync(Guid.NewGuid(), "Rusted Chest");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetContainersInRoomAsync Tests

    [Fact]
    public async Task GetContainersInRoomAsync_ReturnsOnlyContainers()
    {
        // Arrange
        var container1 = CreateTestObject("Chest");
        container1.IsContainer = true;

        var container2 = CreateTestObject("Locker");
        container2.IsContainer = true;

        var furniture = CreateTestObject("Table");
        furniture.IsContainer = false;
        furniture.ObjectType = ObjectType.Furniture;

        await _repository.AddAsync(container1);
        await _repository.AddAsync(container2);
        await _repository.AddAsync(furniture);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetContainersInRoomAsync(_testRoomId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.IsContainer);
    }

    #endregion

    #region ObjectExistsInRoomAsync Tests

    [Fact]
    public async Task ObjectExistsInRoomAsync_ExistingObject_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("Test Object");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.ObjectExistsInRoomAsync(_testRoomId, "Test Object");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ObjectExistsInRoomAsync_NonExisting_ReturnsFalse()
    {
        // Act
        var result = await _repository.ObjectExistsInRoomAsync(_testRoomId, "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleObjects()
    {
        // Arrange
        var objects = new[]
        {
            CreateTestObject("Object 1"),
            CreateTestObject("Object 2"),
            CreateTestObject("Object 3")
        };

        // Act
        await _repository.AddRangeAsync(objects);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _repository.GetByRoomIdAsync(_testRoomId);
        result.Should().HaveCount(3);
    }

    #endregion

    #region ClearRoomObjectsAsync Tests

    [Fact]
    public async Task ClearRoomObjectsAsync_RemovesAllObjectsInRoom()
    {
        // Arrange
        await _repository.AddAsync(CreateTestObject("Object 1"));
        await _repository.AddAsync(CreateTestObject("Object 2"));
        await _repository.SaveChangesAsync();

        // Act
        await _repository.ClearRoomObjectsAsync(_testRoomId);

        // Assert
        var result = await _repository.GetByRoomIdAsync(_testRoomId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearRoomObjectsAsync_DoesNotAffectOtherRooms()
    {
        // Arrange
        var otherRoomId = Guid.NewGuid();
        var objInTestRoom = CreateTestObject("Test Room Object");
        var objInOtherRoom = CreateTestObject("Other Room Object");
        objInOtherRoom.RoomId = otherRoomId;

        await _repository.AddAsync(objInTestRoom);
        await _repository.AddAsync(objInOtherRoom);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.ClearRoomObjectsAsync(_testRoomId);

        // Assert
        var otherRoomObjects = await _repository.GetByRoomIdAsync(otherRoomId);
        otherRoomObjects.Should().HaveCount(1);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingObject()
    {
        // Arrange
        var obj = CreateTestObject("Original Name");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        obj.Name = "Updated Name";
        obj.IsOpen = true;
        obj.HighestExaminationTier = 2;
        await _repository.UpdateAsync(obj);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(obj.Id);
        retrieved!.Name.Should().Be("Updated Name");
        retrieved.IsOpen.Should().BeTrue();
        retrieved.HighestExaminationTier.Should().Be(2);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldRemoveObject()
    {
        // Arrange
        var obj = CreateTestObject("To Delete");
        await _repository.AddAsync(obj);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(obj.Id);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(obj.Id);
        retrieved.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private InteractableObject CreateTestObject(string name)
    {
        return new InteractableObject
        {
            RoomId = _testRoomId,
            Name = name,
            ObjectType = ObjectType.Container,
            Description = $"A {name.ToLower()}.",
            DetailedDescription = "Detailed description.",
            ExpertDescription = "Expert description.",
            IsContainer = true,
            IsOpen = false,
            IsLocked = false
        };
    }

    #endregion
}
