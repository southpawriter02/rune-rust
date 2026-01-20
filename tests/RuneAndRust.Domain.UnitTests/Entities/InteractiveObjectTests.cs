using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for InteractiveObject entity.
/// </summary>
[TestFixture]
public class InteractiveObjectTests
{
    [Test]
    public void Create_WithValidParameters_CreatesObject()
    {
        // Act
        var obj = InteractiveObject.Create(
            "iron-door", "Iron Door", "A heavy iron door.",
            InteractiveObjectType.Door);

        // Assert
        obj.DefinitionId.Should().Be("iron-door");
        obj.Name.Should().Be("Iron Door");
        obj.State.Should().Be(ObjectState.Closed);
    }

    [Test]
    public void TrySetState_WhenNormal_ReturnsTrue()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "", InteractiveObjectType.Door);

        // Act
        var result = obj.TrySetState(ObjectState.Open);

        // Assert
        result.Should().BeTrue();
        obj.State.Should().Be(ObjectState.Open);
    }

    [Test]
    public void TrySetState_WhenBroken_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "", InteractiveObjectType.Door);
        obj.TrySetState(ObjectState.Broken);

        // Act
        var result = obj.TrySetState(ObjectState.Open);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CanPerformInteraction_WhenAllowed_ReturnsTrue()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act & Assert
        obj.CanPerformInteraction(InteractionType.Open).Should().BeTrue();
        obj.CanPerformInteraction(InteractionType.Push).Should().BeFalse();
    }

    [Test]
    public void GetDefaultInteraction_WhenClosed_ReturnsOpen()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Closed,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act
        var defaultAction = obj.GetDefaultInteraction();

        // Assert
        defaultAction.Should().Be(InteractionType.Open);
    }

    [Test]
    public void GetDefaultInteraction_WhenOpen_ReturnsClose()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Open,
            allowedInteractions: [InteractionType.Open, InteractionType.Close]);

        // Act
        var defaultAction = obj.GetDefaultInteraction();

        // Assert
        defaultAction.Should().Be(InteractionType.Close);
    }

    [Test]
    public void MatchesKeyword_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Iron Door", "",
            InteractiveObjectType.Door,
            keywords: ["door", "iron"]);

        // Act & Assert
        obj.MatchesKeyword("DOOR").Should().BeTrue();
        obj.MatchesKeyword("Iron").Should().BeTrue();
    }

    [Test]
    public void IsCurrentlyBlocking_WhenClosedAndBlocks_ReturnsTrue()
    {
        // Arrange
        var obj = InteractiveObject.Create("door", "Door", "",
            InteractiveObjectType.Door, ObjectState.Closed,
            blocksPassage: true, blockedDirection: Direction.North);

        // Act & Assert
        obj.IsCurrentlyBlocking.Should().BeTrue();
        obj.BlockedDirection.Should().Be(Direction.North);
    }

    [Test]
    public void Create_WithBlocksPassage_SetsBlockingProperties()
    {
        // Arrange & Act
        var obj = InteractiveObject.Create(
            "door", "Door", "", InteractiveObjectType.Door,
            blocksPassage: true, blockedDirection: Direction.North);

        // Assert
        obj.BlocksPassage.Should().BeTrue();
        obj.BlockedDirection.Should().Be(Direction.North);
    }

    [Test]
    public void IsCurrentlyBlocking_WhenLockedAndBlocks_ReturnsTrue()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "door", "Door", "", InteractiveObjectType.Door,
            ObjectState.Locked, blocksPassage: true, blockedDirection: Direction.North);

        // Assert
        obj.IsCurrentlyBlocking.Should().BeTrue();
    }

    [Test]
    public void Reset_WhenNotBroken_RestoresDefaultState()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "door", "Door", "", InteractiveObjectType.Door, ObjectState.Closed);
        obj.TrySetState(ObjectState.Open);

        // Act
        var result = obj.Reset();

        // Assert
        result.Should().BeTrue();
        obj.State.Should().Be(ObjectState.Closed);
    }

    [Test]
    public void FromDefinition_CreatesCorrectObject()
    {
        // Arrange
        var definition = new InteractiveObjectDefinition
        {
            Id = "test-door",
            Name = "Test Door",
            ObjectType = InteractiveObjectType.Door,
            DefaultState = ObjectState.Closed,
            BlocksPassage = true
        };

        // Act
        var obj = InteractiveObject.FromDefinition(definition);

        // Assert
        obj.DefinitionId.Should().Be("test-door");
        obj.Name.Should().Be("Test Door");
        obj.ObjectType.Should().Be(InteractiveObjectType.Door);
        obj.BlocksPassage.Should().BeTrue();
    }

    [Test]
    public void CanPerformInteraction_WhenBroken_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "door", "Door", "", InteractiveObjectType.Door,
            allowedInteractions: [InteractionType.Open]);
        obj.TrySetState(ObjectState.Broken);

        // Assert
        obj.CanPerformInteraction(InteractionType.Open).Should().BeFalse();
        obj.CanInteract.Should().BeFalse();
    }
}

