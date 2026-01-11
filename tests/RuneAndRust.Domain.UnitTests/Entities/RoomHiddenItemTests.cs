using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room hidden item methods.
/// </summary>
[TestFixture]
public class RoomHiddenItemTests
{
    [Test]
    public void AddHiddenItem_AddsToHiddenItemsList()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var item = new Item("Ancient Key", "A rusty key.", ItemType.Quest);
        var hiddenItem = HiddenItem.Create(item, 12);

        // Act
        room.AddHiddenItem(hiddenItem);

        // Assert
        room.HiddenItems.Should().HaveCount(1);
        room.HiddenItems[0].Item.Name.Should().Be("Ancient Key");
    }

    [Test]
    public void RevealHiddenItem_MovesItemToRegularItemsList()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var item = new Item("Ancient Key", "A rusty key.", ItemType.Quest);
        var hiddenItem = HiddenItem.Create(item, 12);
        room.AddHiddenItem(hiddenItem);

        // Act
        var revealed = room.RevealHiddenItem(hiddenItem.Id);

        // Assert
        revealed.Should().NotBeNull();
        revealed!.Name.Should().Be("Ancient Key");
        room.HiddenItems.Should().HaveCount(0);
        room.Items.Should().HaveCount(1);
        room.Items[0].Name.Should().Be("Ancient Key");
    }

    [Test]
    public void RevealHiddenItem_ReturnsNullForNonExistentId()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);

        // Act
        var result = room.RevealHiddenItem(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetUndiscoveredHiddenItems_ReturnsOnlyUndiscoveredItems()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var item1 = new Item("Key1", "Key 1", ItemType.Quest);
        var item2 = new Item("Key2", "Key 2", ItemType.Quest);
        var hidden1 = HiddenItem.Create(item1, 10);
        var hidden2 = HiddenItem.Create(item2, 15);

        room.AddHiddenItem(hidden1);
        room.AddHiddenItem(hidden2);

        // Act
        var undiscovered = room.GetUndiscoveredHiddenItems();

        // Assert
        undiscovered.Should().HaveCount(2);
    }
}
