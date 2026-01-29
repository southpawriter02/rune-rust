using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.UnitTests.Fakes;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Interfaces;

/// <summary>
/// Contract tests for <see cref="IContainerStateRepository"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// These tests define the expected behavior for any implementation of the
/// container state repository. The abstract class allows the same tests
/// to be executed against different implementations (in-memory, database, etc.).
/// </para>
/// <para>
/// To test a new implementation, create a concrete test class that extends
/// this abstract class and implements <see cref="CreateRepository"/>.
/// </para>
/// </remarks>
[TestFixture]
public abstract class ContainerStateRepositoryTests
{
    /// <summary>
    /// Creates a new instance of the repository implementation to test.
    /// </summary>
    /// <returns>A new repository instance.</returns>
    protected abstract IContainerStateRepository CreateRepository();

    private IContainerStateRepository _repository = null!;

    /// <summary>
    /// Sets up a fresh repository instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _repository = CreateRepository();
    }

    #region SaveContainerAsync / GetContainerAsync Tests

    /// <summary>
    /// Verifies that a container can be saved and retrieved with its state intact.
    /// </summary>
    [Test]
    public async Task SaveAndRetrieve_Container_PersistsState()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var container = ContainerLootTable.Create(
            ContainerType.SmallChest,
            ContainerLootState.Discovered,
            roomId);
        container.Open();
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1);
        container.SetContents(contents);

        // Act
        await _repository.SaveContainerAsync(container);
        var retrieved = await _repository.GetContainerAsync(container.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(container.Id);
        retrieved.Type.Should().Be(ContainerType.SmallChest);
        retrieved.State.Should().Be(ContainerLootState.Open);
        retrieved.Contents.Should().Be(contents);
        retrieved.RoomId.Should().Be(roomId);
    }

    /// <summary>
    /// Verifies that retrieving a non-existent container returns null.
    /// </summary>
    [Test]
    public async Task GetContainerAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetContainerAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that saving an existing container updates it (upsert semantics).
    /// </summary>
    [Test]
    public async Task SaveContainerAsync_ExistingContainer_Updates()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        await _repository.SaveContainerAsync(container);

        // Act - Update state
        container.Open();
        await _repository.SaveContainerAsync(container);
        var retrieved = await _repository.GetContainerAsync(container.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.State.Should().Be(ContainerLootState.Open);
    }

    #endregion

    #region ResetAllContainersAsync Tests

    /// <summary>
    /// Verifies that reset clears all container states and returns the correct count.
    /// </summary>
    [Test]
    public async Task ResetAllContainers_ClearsAllState()
    {
        // Arrange
        var container1 = ContainerLootTable.Create(ContainerType.SmallChest);
        var container2 = ContainerLootTable.Create(ContainerType.MediumChest);
        await _repository.SaveContainerAsync(container1);
        await _repository.SaveContainerAsync(container2);

        // Act
        var clearedCount = await _repository.ResetAllContainersAsync();

        // Assert
        clearedCount.Should().Be(2);
        var count = await _repository.GetCountAsync();
        count.Should().Be(0);
    }

    /// <summary>
    /// Verifies that resetting an empty repository returns zero.
    /// </summary>
    [Test]
    public async Task ResetAllContainers_EmptyRepository_ReturnsZero()
    {
        // Act
        var clearedCount = await _repository.ResetAllContainersAsync();

        // Assert
        clearedCount.Should().Be(0);
    }

    #endregion

    #region GetContainersByRoomAsync Tests

    /// <summary>
    /// Verifies that room-based queries return only containers from that room.
    /// </summary>
    [Test]
    public async Task GetContainersByRoom_ReturnsOnlyRoomContainers()
    {
        // Arrange
        var roomA = Guid.NewGuid();
        var roomB = Guid.NewGuid();

        var containerA1 = ContainerLootTable.Create(
            ContainerType.SmallChest, roomId: roomA);
        var containerA2 = ContainerLootTable.Create(
            ContainerType.MediumChest, roomId: roomA);
        var containerB1 = ContainerLootTable.Create(
            ContainerType.LargeChest, roomId: roomB);

        await _repository.SaveContainersAsync(new[] { containerA1, containerA2, containerB1 });

        // Act
        var roomAContainers = await _repository.GetContainersByRoomAsync(roomA);

        // Assert
        roomAContainers.Should().HaveCount(2);
        roomAContainers.Should().Contain(c => c.Id == containerA1.Id);
        roomAContainers.Should().Contain(c => c.Id == containerA2.Id);
        roomAContainers.Should().NotContain(c => c.Id == containerB1.Id);
    }

    /// <summary>
    /// Verifies that querying a room with no containers returns empty list.
    /// </summary>
    [Test]
    public async Task GetContainersByRoom_NoContainers_ReturnsEmptyList()
    {
        // Arrange
        var emptyRoomId = Guid.NewGuid();

        // Act
        var containers = await _repository.GetContainersByRoomAsync(emptyRoomId);

        // Assert
        containers.Should().BeEmpty();
    }

    #endregion

    #region DeleteContainerAsync Tests

    /// <summary>
    /// Verifies that deleting an existing container returns true and removes it.
    /// </summary>
    [Test]
    public async Task DeleteContainerAsync_ExistingContainer_ReturnsTrueAndRemoves()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        await _repository.SaveContainerAsync(container);

        // Act
        var deleted = await _repository.DeleteContainerAsync(container.Id);

        // Assert
        deleted.Should().BeTrue();
        var exists = await _repository.ExistsAsync(container.Id);
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that deleting a non-existent container returns false.
    /// </summary>
    [Test]
    public async Task DeleteContainerAsync_NonExistent_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleted = await _repository.DeleteContainerAsync(nonExistentId);

        // Assert
        deleted.Should().BeFalse();
    }

    #endregion

    #region ExistsAsync / GetCountAsync Tests

    /// <summary>
    /// Verifies that ExistsAsync correctly identifies existing containers.
    /// </summary>
    [Test]
    public async Task ExistsAsync_ExistingContainer_ReturnsTrue()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        await _repository.SaveContainerAsync(container);

        // Act
        var exists = await _repository.ExistsAsync(container.Id);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that GetCountAsync returns the correct count.
    /// </summary>
    [Test]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var container1 = ContainerLootTable.Create(ContainerType.SmallChest);
        var container2 = ContainerLootTable.Create(ContainerType.MediumChest);
        var container3 = ContainerLootTable.Create(ContainerType.LargeChest);

        await _repository.SaveContainersAsync(new[] { container1, container2, container3 });

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(3);
    }

    #endregion

    #region GetLootedContainersAsync Tests

    /// <summary>
    /// Verifies that only looted containers are returned.
    /// </summary>
    [Test]
    public async Task GetLootedContainersAsync_ReturnsOnlyLootedContainers()
    {
        // Arrange
        var lootedContainer = ContainerLootTable.Create(ContainerType.SmallChest);
        lootedContainer.Open();
        lootedContainer.Loot();

        var openContainer = ContainerLootTable.Create(ContainerType.MediumChest);
        openContainer.Open();

        var discoveredContainer = ContainerLootTable.Create(ContainerType.LargeChest);

        await _repository.SaveContainersAsync(new[]
        {
            lootedContainer, openContainer, discoveredContainer
        });

        // Act
        var looted = await _repository.GetLootedContainersAsync();

        // Assert
        looted.Should().HaveCount(1);
        looted.Should().Contain(c => c.Id == lootedContainer.Id);
    }

    /// <summary>
    /// Verifies that empty list is returned when no containers are looted.
    /// </summary>
    [Test]
    public async Task GetLootedContainersAsync_NoLootedContainers_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        await _repository.SaveContainerAsync(container);

        // Act
        var looted = await _repository.GetLootedContainersAsync();

        // Assert
        looted.Should().BeEmpty();
    }

    #endregion
}

/// <summary>
/// Concrete test class for the in-memory implementation.
/// </summary>
[TestFixture]
public class InMemoryContainerStateRepositoryTests : ContainerStateRepositoryTests
{
    /// <inheritdoc/>
    protected override IContainerStateRepository CreateRepository()
    {
        return new InMemoryContainerStateRepository();
    }
}
