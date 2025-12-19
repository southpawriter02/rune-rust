using FluentAssertions;
using RuneAndRust.Core.Entities;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the InventoryItem join entity.
/// Validates inventory entry creation and properties.
/// </summary>
public class InventoryItemTests
{
    #region Key Properties Tests

    [Fact]
    public void InventoryItem_CharacterId_CanBeSet()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.CharacterId = characterId;

        // Assert
        inventoryItem.CharacterId.Should().Be(characterId);
    }

    [Fact]
    public void InventoryItem_ItemId_CanBeSet()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.ItemId = itemId;

        // Assert
        inventoryItem.ItemId.Should().Be(itemId);
    }

    #endregion

    #region Stack Properties Tests

    [Fact]
    public void InventoryItem_Quantity_DefaultsToOne()
    {
        // Arrange & Act
        var inventoryItem = new InventoryItem();

        // Assert
        inventoryItem.Quantity.Should().Be(1);
    }

    [Fact]
    public void InventoryItem_Quantity_CanBeSet()
    {
        // Arrange
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.Quantity = 10;

        // Assert
        inventoryItem.Quantity.Should().Be(10);
    }

    [Fact]
    public void InventoryItem_SlotPosition_DefaultsToZero()
    {
        // Arrange & Act
        var inventoryItem = new InventoryItem();

        // Assert
        inventoryItem.SlotPosition.Should().Be(0);
    }

    [Fact]
    public void InventoryItem_SlotPosition_CanBeSet()
    {
        // Arrange
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.SlotPosition = 5;

        // Assert
        inventoryItem.SlotPosition.Should().Be(5);
    }

    #endregion

    #region Equipment State Tests

    [Fact]
    public void InventoryItem_IsEquipped_DefaultsToFalse()
    {
        // Arrange & Act
        var inventoryItem = new InventoryItem();

        // Assert
        inventoryItem.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public void InventoryItem_IsEquipped_CanBeSetToTrue()
    {
        // Arrange
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.IsEquipped = true;

        // Assert
        inventoryItem.IsEquipped.Should().BeTrue();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void InventoryItem_Character_CanBeAssigned()
    {
        // Arrange
        var character = new Character { Name = "Test Hero" };
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.Character = character;

        // Assert
        inventoryItem.Character.Should().BeSameAs(character);
        inventoryItem.Character.Name.Should().Be("Test Hero");
    }

    [Fact]
    public void InventoryItem_Item_CanBeAssigned()
    {
        // Arrange
        var item = new Item { Name = "Health Potion" };
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.Item = item;

        // Assert
        inventoryItem.Item.Should().BeSameAs(item);
        inventoryItem.Item.Name.Should().Be("Health Potion");
    }

    [Fact]
    public void InventoryItem_CanHoldEquipment()
    {
        // Arrange
        var equipment = new Equipment { Name = "Iron Sword" };
        var inventoryItem = new InventoryItem();

        // Act
        inventoryItem.Item = equipment;

        // Assert
        inventoryItem.Item.Should().BeOfType<Equipment>();
        inventoryItem.Item.Name.Should().Be("Iron Sword");
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void InventoryItem_AddedAt_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var inventoryItem = new InventoryItem();

        // Assert
        var after = DateTime.UtcNow;
        inventoryItem.AddedAt.Should().BeOnOrAfter(before);
        inventoryItem.AddedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void InventoryItem_LastModified_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var inventoryItem = new InventoryItem();

        // Assert
        var after = DateTime.UtcNow;
        inventoryItem.LastModified.Should().BeOnOrAfter(before);
        inventoryItem.LastModified.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Full Entry Creation Tests

    [Fact]
    public void InventoryItem_CanCreateCompleteEntry()
    {
        // Arrange
        var character = new Character { Name = "Test Hero" };
        var item = new Item { Name = "Healing Salve", Weight = 100 };

        // Act
        var inventoryItem = new InventoryItem
        {
            CharacterId = character.Id,
            ItemId = item.Id,
            Character = character,
            Item = item,
            Quantity = 3,
            SlotPosition = 2,
            IsEquipped = false
        };

        // Assert
        inventoryItem.CharacterId.Should().Be(character.Id);
        inventoryItem.ItemId.Should().Be(item.Id);
        inventoryItem.Character.Should().BeSameAs(character);
        inventoryItem.Item.Should().BeSameAs(item);
        inventoryItem.Quantity.Should().Be(3);
        inventoryItem.SlotPosition.Should().Be(2);
        inventoryItem.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public void InventoryItem_CanCreateEquippedEntry()
    {
        // Arrange
        var character = new Character { Name = "Test Hero" };
        var equipment = new Equipment { Name = "Iron Helm" };

        // Act
        var inventoryItem = new InventoryItem
        {
            CharacterId = character.Id,
            ItemId = equipment.Id,
            Character = character,
            Item = equipment,
            Quantity = 1,
            IsEquipped = true
        };

        // Assert
        inventoryItem.IsEquipped.Should().BeTrue();
        inventoryItem.Item.Should().BeOfType<Equipment>();
    }

    #endregion
}
