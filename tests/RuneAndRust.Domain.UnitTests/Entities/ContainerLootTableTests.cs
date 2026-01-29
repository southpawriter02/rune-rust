using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="ContainerLootTable"/> entity.
/// </summary>
/// <remarks>
/// Tests cover factory method creation, state transitions, loot operations,
/// and computed properties for the container loot table entity.
/// </remarks>
[TestFixture]
public class ContainerLootTableTests
{
    #region Factory Method Tests

    /// <summary>
    /// Verifies that Create with default parameters creates correctly configured container.
    /// </summary>
    [Test]
    public void Create_WithDefaults_CreatesDiscoveredContainer()
    {
        // Arrange & Act
        var container = ContainerLootTable.Create(ContainerType.SmallChest);

        // Assert
        container.Id.Should().NotBeEmpty();
        container.Type.Should().Be(ContainerType.SmallChest);
        container.State.Should().Be(ContainerLootState.Discovered);
        container.Contents.Should().BeNull();
        container.RoomId.Should().BeNull();
        container.LootedAt.Should().BeNull();
        container.IsLooted.Should().BeFalse();
        container.CanLoot.Should().BeFalse();
        container.NeedsDiscovery.Should().BeFalse();
        container.IsLocked.Should().BeFalse();
        container.HasGeneratedContents.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateHidden creates container in Undiscovered state.
    /// </summary>
    [Test]
    public void CreateHidden_CreatesUndiscoveredContainer()
    {
        // Arrange & Act
        var container = ContainerLootTable.CreateHidden(ContainerType.HiddenCache);

        // Assert
        container.State.Should().Be(ContainerLootState.Undiscovered);
        container.NeedsDiscovery.Should().BeTrue();
        container.CanLoot.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateLocked creates container in Locked state.
    /// </summary>
    [Test]
    public void CreateLocked_CreatesLockedContainer()
    {
        // Arrange & Act
        var container = ContainerLootTable.CreateLocked(ContainerType.LargeChest);

        // Assert
        container.State.Should().Be(ContainerLootState.Locked);
        container.IsLocked.Should().BeTrue();
        container.CanLoot.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create with roomId assigns room correctly.
    /// </summary>
    [Test]
    public void Create_WithRoomId_AssignsRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var container = ContainerLootTable.Create(
            ContainerType.MediumChest,
            roomId: roomId);

        // Assert
        container.RoomId.Should().Be(roomId);
    }

    #endregion

    #region Discover State Transition Tests

    /// <summary>
    /// Verifies that Discover transitions from Undiscovered to Discovered.
    /// </summary>
    [Test]
    public void Discover_WhenUndiscovered_TransitionsToDiscovered()
    {
        // Arrange
        var container = ContainerLootTable.CreateHidden(ContainerType.HiddenCache);

        // Act
        var result = container.Discover();

        // Assert
        result.Should().BeTrue();
        container.State.Should().Be(ContainerLootState.Discovered);
        container.NeedsDiscovery.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Discover fails when already discovered.
    /// </summary>
    [Test]
    public void Discover_WhenAlreadyDiscovered_ReturnsFalse()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);

        // Act
        var result = container.Discover();

        // Assert
        result.Should().BeFalse();
        container.State.Should().Be(ContainerLootState.Discovered);
    }

    #endregion

    #region Unlock State Transition Tests

    /// <summary>
    /// Verifies that Unlock transitions from Locked to Open.
    /// </summary>
    [Test]
    public void Unlock_WhenLocked_TransitionsToOpen()
    {
        // Arrange
        var container = ContainerLootTable.CreateLocked(ContainerType.LargeChest);

        // Act
        var result = container.Unlock();

        // Assert
        result.Should().BeTrue();
        container.State.Should().Be(ContainerLootState.Open);
        container.IsLocked.Should().BeFalse();
        container.CanLoot.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Unlock fails when not locked.
    /// </summary>
    [Test]
    public void Unlock_WhenNotLocked_ReturnsFalse()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);

        // Act
        var result = container.Unlock();

        // Assert
        result.Should().BeFalse();
        container.State.Should().Be(ContainerLootState.Discovered);
    }

    #endregion

    #region Open State Transition Tests

    /// <summary>
    /// Verifies that Open transitions from Discovered to Open.
    /// </summary>
    [Test]
    public void Open_WhenDiscovered_TransitionsToOpen()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);

        // Act
        var result = container.Open();

        // Assert
        result.Should().BeTrue();
        container.State.Should().Be(ContainerLootState.Open);
        container.CanLoot.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Open fails when locked (must use Unlock instead).
    /// </summary>
    [Test]
    public void Open_WhenLocked_ReturnsFalse()
    {
        // Arrange
        var container = ContainerLootTable.CreateLocked(ContainerType.LargeChest);

        // Act
        var result = container.Open();

        // Assert
        result.Should().BeFalse();
        container.State.Should().Be(ContainerLootState.Locked);
    }

    #endregion

    #region SetContents Tests

    /// <summary>
    /// Verifies that SetContents assigns contents correctly.
    /// </summary>
    [Test]
    public void SetContents_WhenNotSet_AssignsContents()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1);

        // Act
        container.SetContents(contents);

        // Assert
        container.Contents.Should().Be(contents);
        container.HasGeneratedContents.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SetContents throws when called twice.
    /// </summary>
    [Test]
    public void SetContents_WhenAlreadySet_ThrowsInvalidOperationException()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1);
        container.SetContents(contents);

        // Act
        var act = () => container.SetContents(contents);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been generated*");
    }

    #endregion

    #region Loot Tests

    /// <summary>
    /// Verifies that Loot when open returns contents and sets looted state.
    /// </summary>
    [Test]
    public void Loot_WhenOpen_ReturnsContentsAndSetsLooted()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1);
        container.SetContents(contents);

        // Act
        var lootedContents = container.Loot();

        // Assert
        lootedContents.Should().Be(contents);
        container.State.Should().Be(ContainerLootState.Looted);
        container.IsLooted.Should().BeTrue();
        container.LootedAt.Should().NotBeNull();
        container.LootedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Verifies that Loot when already looted returns empty.
    /// </summary>
    [Test]
    public void Loot_WhenAlreadyLooted_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        container.SetContents(ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1));
        container.Loot(); // First loot

        // Act
        var secondLoot = container.Loot();

        // Assert
        secondLoot.Should().Be(ContainerContents.Empty);
        container.IsLooted.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Loot when not open returns empty.
    /// </summary>
    [Test]
    public void Loot_WhenNotOpen_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        // Container is in Discovered state, not Open

        // Act
        var result = container.Loot();

        // Assert
        result.Should().Be(ContainerContents.Empty);
        container.State.Should().Be(ContainerLootState.Discovered);
        container.IsLooted.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Loot without contents returns empty.
    /// </summary>
    [Test]
    public void Loot_WhenNoContentsSet_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        // Contents not set

        // Act
        var result = container.Loot();

        // Assert
        result.Should().Be(ContainerContents.Empty);
        container.State.Should().Be(ContainerLootState.Looted);
    }

    #endregion

    #region GetDisplayContents Tests

    /// <summary>
    /// Verifies that GetDisplayContents returns contents when open.
    /// </summary>
    [Test]
    public void GetDisplayContents_WhenOpen_ReturnsContents()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1);
        container.SetContents(contents);

        // Act
        var displayContents = container.GetDisplayContents();

        // Assert
        displayContents.Should().Be(contents);
        container.State.Should().Be(ContainerLootState.Open); // State unchanged
    }

    /// <summary>
    /// Verifies that GetDisplayContents returns empty when looted.
    /// </summary>
    [Test]
    public void GetDisplayContents_WhenLooted_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        container.SetContents(ContainerContents.Create(
            new List<string> { "sword-iron" },
            25,
            1));
        container.Loot();

        // Act
        var displayContents = container.GetDisplayContents();

        // Assert
        displayContents.Should().Be(ContainerContents.Empty);
    }

    #endregion

    #region Full Workflow Tests

    /// <summary>
    /// Verifies complete hidden container workflow: discover → open → loot.
    /// </summary>
    [Test]
    public void FullWorkflow_HiddenContainer_SuccessfullyLoots()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var container = ContainerLootTable.CreateHidden(
            ContainerType.HiddenCache,
            roomId);
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron", "potion-health" },
            150,
            2);

        // Act - Discover
        container.NeedsDiscovery.Should().BeTrue();
        var discovered = container.Discover();

        // Act - Open
        container.Open();

        // Act - Set Contents (loot service would do this)
        container.SetContents(contents);

        // Act - Loot
        var lootedContents = container.Loot();

        // Assert
        discovered.Should().BeTrue();
        lootedContents.ItemCount.Should().Be(2);
        lootedContents.CurrencyAmount.Should().Be(150);
        container.IsLooted.Should().BeTrue();
        container.RoomId.Should().Be(roomId);
        container.LootedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies complete locked container workflow: unlock → loot.
    /// </summary>
    [Test]
    public void FullWorkflow_LockedContainer_SuccessfullyLoots()
    {
        // Arrange
        var container = ContainerLootTable.CreateLocked(ContainerType.LargeChest);
        var contents = ContainerContents.Create(
            new List<string> { "armor-plate" },
            100,
            3);

        // Act - Unlock (key used or lockpick succeeded)
        container.IsLocked.Should().BeTrue();
        var unlocked = container.Unlock();

        // Act - Set Contents and Loot
        container.SetContents(contents);
        var lootedContents = container.Loot();

        // Assert
        unlocked.Should().BeTrue();
        lootedContents.Should().Be(contents);
        container.IsLooted.Should().BeTrue();
    }

    #endregion
}
