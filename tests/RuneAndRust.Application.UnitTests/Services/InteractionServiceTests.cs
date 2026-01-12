using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for InteractionService.
/// </summary>
[TestFixture]
public class InteractionServiceTests
{
    private InteractionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new InteractionService();
    }

    [Test]
    public void Interact_WithDefaultInteraction_PerformsAction()
    {
        // Arrange
        var door = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Closed,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act
        var result = _service.Interact(door);

        // Assert
        result.Success.Should().BeTrue();
        result.InteractionType.Should().Be(InteractionType.Open);
    }

    [Test]
    public void Interact_WithBrokenObject_ReturnsFailed()
    {
        // Arrange
        var door = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Broken);

        // Act
        var result = _service.Interact(door, InteractionType.Open);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("broken");
    }

    [Test]
    public void Open_WhenClosed_OpensObject()
    {
        // Arrange
        var chest = InteractiveObject.Create("chest", "Chest", "",
            InteractiveObjectType.Chest, ObjectState.Closed,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act
        var result = _service.Open(chest);

        // Assert
        result.Success.Should().BeTrue();
        chest.State.Should().Be(ObjectState.Open);
    }

    [Test]
    public void Open_WhenAlreadyOpen_ReturnsFailed()
    {
        // Arrange
        var chest = InteractiveObject.Create("chest", "Chest", "",
            InteractiveObjectType.Chest, ObjectState.Open);

        // Act
        var result = _service.Open(chest);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already open");
    }

    [Test]
    public void Open_WhenLocked_ReturnsFailed()
    {
        // Arrange
        var door = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Locked);

        // Act
        var result = _service.Open(door);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("locked");
    }

    [Test]
    public void Close_WhenOpen_ClosesObject()
    {
        // Arrange
        var door = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Open,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act
        var result = _service.Close(door);

        // Assert
        result.Success.Should().BeTrue();
        door.State.Should().Be(ObjectState.Closed);
    }

    [Test]
    public void Examine_ReturnsDescription()
    {
        // Arrange
        var lever = InteractiveObject.Create("lever", "Rusty Lever",
            "An old lever covered in rust.",
            InteractiveObjectType.Lever, ObjectState.Inactive);

        // Act
        var result = _service.Examine(lever);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Rusty Lever");
        result.Message.Should().Contain("inactive");
    }

    [Test]
    public void FindObject_WithMatchingKeyword_ReturnsObject()
    {
        // Arrange
        var objects = new[]
        {
            InteractiveObject.Create("door", "Iron Door", "", InteractiveObjectType.Door),
            InteractiveObject.Create("chest", "Wooden Chest", "", InteractiveObjectType.Chest)
        };

        // Act
        var found = _service.FindObject(objects, "chest");

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Wooden Chest");
    }
}
