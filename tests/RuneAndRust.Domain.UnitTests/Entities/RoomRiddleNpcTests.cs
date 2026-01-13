using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room riddle NPC functionality (v0.4.2c).
/// </summary>
[TestFixture]
public class RoomRiddleNpcTests
{
    private Room CreateTestRoom() =>
        new Room("Test Room", "A room", Position3D.Origin);

    [Test]
    public void AddRiddleNpc_AddsNpcToCollection()
    {
        // Arrange
        var room = CreateTestRoom();
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "");

        // Act
        room.AddRiddleNpc(npc);

        // Assert
        room.RiddleNpcs.Should().HaveCount(1);
        room.HasRiddleNpcs.Should().BeTrue();
    }

    [Test]
    public void AddRiddleNpc_DoesNotAddDuplicate()
    {
        // Arrange
        var room = CreateTestRoom();
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "");

        // Act
        room.AddRiddleNpc(npc);
        room.AddRiddleNpc(npc);

        // Assert
        room.RiddleNpcs.Should().HaveCount(1);
    }

    [Test]
    public void RemoveRiddleNpc_RemovesFromCollection()
    {
        // Arrange
        var room = CreateTestRoom();
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "");
        room.AddRiddleNpc(npc);

        // Act
        var result = room.RemoveRiddleNpc(npc);

        // Assert
        result.Should().BeTrue();
        room.RiddleNpcs.Should().BeEmpty();
    }

    [Test]
    public void GetRiddleNpcByKeyword_FindsNpc()
    {
        // Arrange
        var room = CreateTestRoom();
        var npc = RiddleNpc.Create("Ancient Sphinx", "", "riddle-1", "", "");
        room.AddRiddleNpc(npc);

        // Act
        var found = room.GetRiddleNpcByKeyword("sphinx");

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Ancient Sphinx");
    }

    [Test]
    public void IsDirectionBlockedByNpc_WhenBlocked_ReturnsTrue()
    {
        // Arrange
        var room = CreateTestRoom();
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "",
            blocksPassage: true, blockedDirection: Direction.North);
        room.AddRiddleNpc(npc);

        // Act & Assert
        room.IsDirectionBlockedByNpc(Direction.North).Should().BeTrue();
        room.IsDirectionBlockedByNpc(Direction.South).Should().BeFalse();
    }
}
