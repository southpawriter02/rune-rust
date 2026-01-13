using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room interactive object management.
/// </summary>
[TestFixture]
public class RoomInteractableTests
{
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new Room("Test Room", "A test room.", Position3D.Origin);
    }

    private static InteractiveObject CreateTestObject(
        string id = "test",
        string name = "Test Object",
        bool blocksPassage = false,
        Direction? blockedDirection = null,
        ObjectState state = ObjectState.Closed,
        bool isVisible = true)
    {
        var obj = InteractiveObject.Create(
            id, name, "", InteractiveObjectType.Door, state,
            blocksPassage: blocksPassage, blockedDirection: blockedDirection,
            isVisible: isVisible);
        return obj;
    }

    [Test]
    public void AddInteractable_AddsToCollection()
    {
        // Arrange
        var obj = CreateTestObject();

        // Act
        _room.AddInteractable(obj);

        // Assert
        _room.HasInteractables.Should().BeTrue();
        _room.Interactables.Should().Contain(obj);
    }

    [Test]
    public void RemoveInteractable_RemovesFromCollection()
    {
        // Arrange
        var obj = CreateTestObject();
        _room.AddInteractable(obj);

        // Act
        var result = _room.RemoveInteractable(obj);

        // Assert
        result.Should().BeTrue();
        _room.HasInteractables.Should().BeFalse();
    }

    [Test]
    public void GetInteractableByKeyword_FindsObject()
    {
        // Arrange
        var door = InteractiveObject.Create(
            "door", "Iron Door", "", InteractiveObjectType.Door,
            keywords: ["door", "iron"]);
        _room.AddInteractable(door);

        // Act
        var found = _room.GetInteractableByKeyword("door");

        // Assert
        found.Should().Be(door);
    }

    [Test]
    public void GetBlockingObject_ReturnsBlockingObject()
    {
        // Arrange
        var door = CreateTestObject(
            blocksPassage: true,
            blockedDirection: Direction.North,
            state: ObjectState.Closed);
        _room.AddInteractable(door);

        // Act
        var blocking = _room.GetBlockingObject(Direction.North);

        // Assert
        blocking.Should().Be(door);
    }

    [Test]
    public void IsDirectionBlocked_WhenBlocked_ReturnsTrue()
    {
        // Arrange
        var door = CreateTestObject(
            blocksPassage: true,
            blockedDirection: Direction.North,
            state: ObjectState.Closed);
        _room.AddInteractable(door);

        // Act & Assert
        _room.IsDirectionBlocked(Direction.North).Should().BeTrue();
        _room.IsDirectionBlocked(Direction.South).Should().BeFalse();
    }

    [Test]
    public void GetVisibleInteractables_ReturnsOnlyVisible()
    {
        // Arrange
        var visible = CreateTestObject("visible", "Visible Door", isVisible: true);
        var hidden = CreateTestObject("hidden", "Hidden Door", isVisible: false);

        _room.AddInteractable(visible);
        _room.AddInteractable(hidden);

        // Act
        var visibleObjects = _room.GetVisibleInteractables().ToList();

        // Assert
        visibleObjects.Should().HaveCount(1);
        visibleObjects.Should().Contain(visible);
        visibleObjects.Should().NotContain(hidden);
    }
}
