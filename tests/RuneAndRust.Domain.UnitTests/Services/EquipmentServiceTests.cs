using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class EquipmentServiceTests
{
    private EquipmentService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var mockLogger = new Mock<ILogger<EquipmentService>>();
        _service = new EquipmentService(mockLogger.Object);
    }

    [Test]
    public void TryEquipByName_WithValidWeapon_ReturnsSuccess()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword = Item.CreateSword();
        player.TryPickUpItem(sword);

        // Act
        var result = _service.TryEquipByName(player, "Rusty Sword");

        // Assert
        result.Success.Should().BeTrue();
        result.EquippedItem.Should().Be(sword);
        player.IsSlotOccupied(EquipmentSlot.Weapon).Should().BeTrue();
        player.Inventory.Contains(sword).Should().BeFalse();
    }

    [Test]
    public void TryEquipByName_WithItemNotInInventory_ReturnsItemNotFound()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.TryEquipByName(player, "Rusty Sword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have");
    }

    [Test]
    public void TryEquip_WithNonEquippableItem_ReturnsNotEquippable()
    {
        // Arrange
        var player = new Player("TestHero");
        var potion = Item.CreateHealthPotion();
        player.TryPickUpItem(potion);

        // Act
        var result = _service.TryEquip(player, potion);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be equipped");
    }

    [Test]
    public void TryEquip_WithOccupiedSlot_SwapsItems()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword1 = Item.CreateSword();
        var sword2 = Item.CreateSword();
        player.TryPickUpItem(sword1);
        player.TryPickUpItem(sword2);

        // Equip first sword
        _service.TryEquip(player, sword1);

        // Act - equip second sword
        var result = _service.TryEquip(player, sword2);

        // Assert
        result.Success.Should().BeTrue();
        result.WasSwapped.Should().BeTrue();
        result.UnequippedItem.Should().Be(sword1);
        result.EquippedItem.Should().Be(sword2);
        player.GetEquippedItem(EquipmentSlot.Weapon).Should().Be(sword2);
        player.Inventory.Contains(sword1).Should().BeTrue();
    }

    [Test]
    public void TryUnequip_WithEquippedItem_ReturnsSuccess()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword = Item.CreateSword();
        player.TryPickUpItem(sword);
        _service.TryEquip(player, sword);

        // Act
        var result = _service.TryUnequip(player, EquipmentSlot.Weapon);

        // Assert
        result.Success.Should().BeTrue();
        result.UnequippedItem.Should().Be(sword);
        player.IsSlotOccupied(EquipmentSlot.Weapon).Should().BeFalse();
        player.Inventory.Contains(sword).Should().BeTrue();
    }

    [Test]
    public void TryUnequip_WithEmptySlot_ReturnsSlotEmpty()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.TryUnequip(player, EquipmentSlot.Weapon);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Nothing is equipped");
    }

    [Test]
    public void TryParseSlot_WithValidSlotName_ReturnsTrue()
    {
        // Act & Assert
        EquipmentService.TryParseSlot("weapon", out var slot).Should().BeTrue();
        slot.Should().Be(EquipmentSlot.Weapon);

        EquipmentService.TryParseSlot("ARMOR", out slot).Should().BeTrue();
        slot.Should().Be(EquipmentSlot.Armor);

        EquipmentService.TryParseSlot("Helmet", out slot).Should().BeTrue();
        slot.Should().Be(EquipmentSlot.Helmet);
    }

    [Test]
    public void TryParseSlot_WithInvalidSlotName_ReturnsFalse()
    {
        // Act & Assert
        EquipmentService.TryParseSlot("invalid", out _).Should().BeFalse();
        EquipmentService.TryParseSlot("", out _).Should().BeFalse();
        EquipmentService.TryParseSlot(null, out _).Should().BeFalse();
    }

    [Test]
    public void GetValidSlotNames_ReturnsAllSlots()
    {
        // Act
        var result = EquipmentService.GetValidSlotNames();

        // Assert
        result.Should().Contain("weapon");
        result.Should().Contain("armor");
        result.Should().Contain("shield");
        result.Should().Contain("helmet");
        result.Should().Contain("boots");
        result.Should().Contain("ring");
        result.Should().Contain("amulet");
    }
}
