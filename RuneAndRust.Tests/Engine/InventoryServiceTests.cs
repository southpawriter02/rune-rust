using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Extensive tests for the InventoryService class.
/// Validates item management, equipment handling, burden calculation, and display formatting.
/// </summary>
public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _mockRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<InventoryService>> _mockLogger;
    private readonly InventoryService _sut;
    private readonly Character _testCharacter;
    private readonly Item _testItem;
    private readonly Equipment _testEquipment;

    public InventoryServiceTests()
    {
        _mockRepository = new Mock<IInventoryRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<InventoryService>>();
        _sut = new InventoryService(_mockRepository.Object, _mockEventBus.Object, _mockLogger.Object);

        _testCharacter = new Character
        {
            Name = "Test Hero",
            Might = 5 // Max capacity = 5 * 10000g = 50kg
        };

        _testItem = new Item
        {
            Name = "Health Potion",
            Weight = 100,
            IsStackable = true,
            MaxStackSize = 10
        };

        _testEquipment = new Equipment
        {
            Name = "Iron Sword",
            Weight = 1500,
            Slot = EquipmentSlot.MainHand,
            ItemType = ItemType.Weapon,
            DamageDie = 6,
            AttributeBonuses = new Dictionary<CharacterAttribute, int>
            {
                [CharacterAttribute.Might] = 1
            }
        };
    }

    #region AddItemAsync Tests

    [Fact]
    public async Task AddItemAsync_NewItem_ShouldSucceed()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id))
            .ReturnsAsync((InventoryItem?)null);
        _mockRepository
            .Setup(r => r.GetItemCountAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.AddItemAsync(_testCharacter, _testItem, 1);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Added");
        result.Message.Should().Contain(_testItem.Name);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<InventoryItem>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_StackableItem_ShouldIncreaseQuantity()
    {
        // Arrange
        var existingEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            Quantity = 3
        };
        _mockRepository
            .Setup(r => r.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _sut.AddItemAsync(_testCharacter, _testItem, 2);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Now have 5");
        existingEntry.Quantity.Should().Be(5);
        _mockRepository.Verify(r => r.UpdateAsync(existingEntry), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ExceedsMaxStack_ShouldFail()
    {
        // Arrange
        var existingEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            Quantity = 8
        };
        _mockRepository
            .Setup(r => r.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _sut.AddItemAsync(_testCharacter, _testItem, 5);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot stack more than");
    }

    [Fact]
    public async Task AddItemAsync_NonStackableItemAlreadyOwned_ShouldFail()
    {
        // Arrange
        var nonStackableItem = new Item { Name = "Unique Key", IsStackable = false };
        var existingEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = nonStackableItem.Id,
            Item = nonStackableItem,
            Quantity = 1
        };
        _mockRepository
            .Setup(r => r.GetByCharacterAndItemAsync(_testCharacter.Id, nonStackableItem.Id))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _sut.AddItemAsync(_testCharacter, nonStackableItem, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already have");
    }

    [Fact]
    public async Task AddItemAsync_InvalidQuantity_ShouldFail()
    {
        // Act
        var result = await _sut.AddItemAsync(_testCharacter, _testItem, 0);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid quantity");
    }

    [Fact]
    public async Task AddItemAsync_NegativeQuantity_ShouldFail()
    {
        // Act
        var result = await _sut.AddItemAsync(_testCharacter, _testItem, -1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid quantity");
    }

    #endregion

    #region RemoveItemAsync Tests

    [Fact]
    public async Task RemoveItemAsync_ExistingItem_ShouldSucceed()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            Quantity = 3
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Health Potion"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.RemoveItemAsync(_testCharacter, "Health Potion", 1);

        // Assert
        result.Success.Should().BeTrue();
        entry.Quantity.Should().Be(2);
        _mockRepository.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_AllQuantity_ShouldRemoveEntry()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            Quantity = 2
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Health Potion"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.RemoveItemAsync(_testCharacter, "Health Potion", 2);

        // Assert
        result.Success.Should().BeTrue();
        _mockRepository.Verify(r => r.RemoveAsync(_testCharacter.Id, entry.ItemId), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_NotEnoughQuantity_ShouldFail()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            Quantity = 1
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Health Potion"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.RemoveItemAsync(_testCharacter, "Health Potion", 5);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("only have 1");
    }

    [Fact]
    public async Task RemoveItemAsync_NotFound_ShouldFail()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "NonExistent"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.RemoveItemAsync(_testCharacter, "NonExistent", 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have");
    }

    #endregion

    #region DropItemAsync Tests

    [Fact]
    public async Task DropItemAsync_UnequippedItem_ShouldSucceed()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Health Potion"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.DropItemAsync(_testCharacter, "Health Potion");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("drop");
        _mockRepository.Verify(r => r.RemoveAsync(_testCharacter.Id, entry.ItemId), Times.Once);
    }

    [Fact]
    public async Task DropItemAsync_EquippedItem_ShouldFail()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = true
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.DropItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("unequip");
    }

    [Fact]
    public async Task DropItemAsync_KeyItem_ShouldFail()
    {
        // Arrange
        var keyItem = new Item { Name = "Ancient Key", ItemType = ItemType.KeyItem };
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = keyItem.Id,
            Item = keyItem,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Ancient Key"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.DropItemAsync(_testCharacter, "Ancient Key");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot drop");
    }

    [Fact]
    public async Task DropItemAsync_NotFound_ShouldFail()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "NonExistent"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.DropItemAsync(_testCharacter, "NonExistent");

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region EquipItemAsync Tests

    [Fact]
    public async Task EquipItemAsync_ValidEquipment_ShouldSucceed()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(entry);
        _mockRepository
            .Setup(r => r.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.MainHand))
            .ReturnsAsync((InventoryItem?)null);
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { entry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(1500);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("equip");
        entry.IsEquipped.Should().BeTrue();
    }

    [Fact]
    public async Task EquipItemAsync_ReplacesExistingItem_ShouldSucceed()
    {
        // Arrange
        var oldSword = new Equipment { Name = "Old Sword", Slot = EquipmentSlot.MainHand };
        var oldEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = oldSword.Id,
            Item = oldSword,
            IsEquipped = true
        };
        var newEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = false
        };

        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(newEntry);
        _mockRepository
            .Setup(r => r.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.MainHand))
            .ReturnsAsync(oldEntry);
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { newEntry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(1500);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("replacing");
        oldEntry.IsEquipped.Should().BeFalse();
        newEntry.IsEquipped.Should().BeTrue();
    }

    [Fact]
    public async Task EquipItemAsync_NonEquipment_ShouldFail()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Item = _testItem,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Health Potion"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "Health Potion");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be equipped");
    }

    [Fact]
    public async Task EquipItemAsync_AlreadyEquipped_ShouldFail()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = true
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already equipped");
    }

    [Fact]
    public async Task EquipItemAsync_RequirementsNotMet_ShouldFail()
    {
        // Arrange
        var heavySword = new Equipment
        {
            Name = "Heavy Sword",
            Slot = EquipmentSlot.MainHand,
            Requirements = new Dictionary<CharacterAttribute, int>
            {
                [CharacterAttribute.Might] = 10 // Character only has 5
            }
        };
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = heavySword.Id,
            Item = heavySword,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Heavy Sword"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "Heavy Sword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("requirements");
    }

    [Fact]
    public async Task EquipItemAsync_NotFound_ShouldFail()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "NonExistent"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.EquipItemAsync(_testCharacter, "NonExistent");

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region UnequipSlotAsync Tests

    [Fact]
    public async Task UnequipSlotAsync_EquippedSlot_ShouldSucceed()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = true
        };
        _mockRepository
            .Setup(r => r.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.MainHand))
            .ReturnsAsync(entry);
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.UnequipSlotAsync(_testCharacter, EquipmentSlot.MainHand);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("unequip");
        entry.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public async Task UnequipSlotAsync_EmptySlot_ShouldFail()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.Head))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.UnequipSlotAsync(_testCharacter, EquipmentSlot.Head);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Nothing equipped");
    }

    #endregion

    #region UnequipItemAsync Tests

    [Fact]
    public async Task UnequipItemAsync_EquippedItem_ShouldSucceed()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = true
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(entry);
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.UnequipItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeTrue();
        entry.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public async Task UnequipItemAsync_NotEquipped_ShouldFail()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = false
        };
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "Iron Sword"))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.UnequipItemAsync(_testCharacter, "Iron Sword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not equipped");
    }

    [Fact]
    public async Task UnequipItemAsync_NotFound_ShouldFail()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.FindByItemNameAsync(_testCharacter.Id, "NonExistent"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.UnequipItemAsync(_testCharacter, "NonExistent");

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region CalculateBurdenAsync Tests

    [Fact]
    public async Task CalculateBurdenAsync_UnderSeventyPercent_ShouldReturnLight()
    {
        // Arrange: MIGHT 5 = 50kg capacity, 30kg = 60%
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(30000); // 30kg

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Light);
    }

    [Fact]
    public async Task CalculateBurdenAsync_AtSeventyPercent_ShouldReturnHeavy()
    {
        // Arrange: MIGHT 5 = 50kg capacity, 35kg = 70%
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(35000); // 35kg

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Heavy);
    }

    [Fact]
    public async Task CalculateBurdenAsync_AtEightyPercent_ShouldReturnHeavy()
    {
        // Arrange: MIGHT 5 = 50kg capacity, 40kg = 80%
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(40000); // 40kg

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Heavy);
    }

    [Fact]
    public async Task CalculateBurdenAsync_AtNinetyPercent_ShouldReturnOverburdened()
    {
        // Arrange: MIGHT 5 = 50kg capacity, 45kg = 90%
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(45000); // 45kg

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Overburdened);
    }

    [Fact]
    public async Task CalculateBurdenAsync_OverOneHundredPercent_ShouldReturnOverburdened()
    {
        // Arrange: MIGHT 5 = 50kg capacity, 60kg = 120%
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(60000); // 60kg

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Overburdened);
    }

    [Fact]
    public async Task CalculateBurdenAsync_EmptyInventory_ShouldReturnLight()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.CalculateBurdenAsync(_testCharacter);

        // Assert
        result.Should().Be(BurdenState.Light);
    }

    #endregion

    #region GetMaxCapacity Tests

    [Fact]
    public void GetMaxCapacity_ShouldReturnMightTimesTenThousand()
    {
        // Act
        var result = _sut.GetMaxCapacity(_testCharacter);

        // Assert
        result.Should().Be(50000); // 5 * 10000g
    }

    [Fact]
    public void GetMaxCapacity_WithEquipmentBonuses_ShouldUseEffectiveAttribute()
    {
        // Arrange
        _testCharacter.EquipmentBonuses[CharacterAttribute.Might] = 2;

        // Act
        var result = _sut.GetMaxCapacity(_testCharacter);

        // Assert
        result.Should().Be(70000); // (5 + 2) * 10000g
    }

    #endregion

    #region CanMoveAsync Tests

    [Fact]
    public async Task CanMoveAsync_LightBurden_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(10000);

        // Act
        var result = await _sut.CanMoveAsync(_testCharacter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanMoveAsync_HeavyBurden_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(40000);

        // Act
        var result = await _sut.CanMoveAsync(_testCharacter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanMoveAsync_Overburdened_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(48000);

        // Act
        var result = await _sut.CanMoveAsync(_testCharacter);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RecalculateEquipmentBonusesAsync Tests

    [Fact]
    public async Task RecalculateEquipmentBonusesAsync_WithEquippedItems_ShouldApplyBonuses()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testEquipment.Id,
            Item = _testEquipment,
            IsEquipped = true
        };
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { entry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(1500);

        // Act
        await _sut.RecalculateEquipmentBonusesAsync(_testCharacter);

        // Assert
        _testCharacter.EquipmentBonuses.Should().ContainKey(CharacterAttribute.Might);
        _testCharacter.EquipmentBonuses[CharacterAttribute.Might].Should().Be(1);
    }

    [Fact]
    public async Task RecalculateEquipmentBonusesAsync_MultipleItems_ShouldStackBonuses()
    {
        // Arrange
        var helmet = new Equipment
        {
            Name = "Iron Helm",
            Slot = EquipmentSlot.Head,
            AttributeBonuses = new Dictionary<CharacterAttribute, int>
            {
                [CharacterAttribute.Might] = 1,
                [CharacterAttribute.Sturdiness] = 2
            }
        };
        var entries = new List<InventoryItem>
        {
            new() { Item = _testEquipment, IsEquipped = true },
            new() { Item = helmet, IsEquipped = true }
        };
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(entries);
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(3000);

        // Act
        await _sut.RecalculateEquipmentBonusesAsync(_testCharacter);

        // Assert
        _testCharacter.EquipmentBonuses[CharacterAttribute.Might].Should().Be(2); // 1 + 1
        _testCharacter.EquipmentBonuses[CharacterAttribute.Sturdiness].Should().Be(2);
    }

    [Fact]
    public async Task RecalculateEquipmentBonusesAsync_HeavyBurden_ShouldApplyPenalty()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(40000); // 80% = Heavy

        // Act
        await _sut.RecalculateEquipmentBonusesAsync(_testCharacter);

        // Assert
        _testCharacter.EquipmentBonuses[CharacterAttribute.Finesse].Should().Be(-2);
    }

    [Fact]
    public async Task RecalculateEquipmentBonusesAsync_NoItems_ShouldClearBonuses()
    {
        // Arrange
        _testCharacter.EquipmentBonuses[CharacterAttribute.Might] = 5; // Pre-existing
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        await _sut.RecalculateEquipmentBonusesAsync(_testCharacter);

        // Assert
        _testCharacter.EquipmentBonuses.Should().BeEmpty();
    }

    #endregion

    #region FormatInventoryDisplayAsync Tests

    [Fact]
    public async Task FormatInventoryDisplayAsync_EmptyInventory_ShouldShowEmpty()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.FormatInventoryDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("PACK");
        result.Should().Contain("(empty)");
        result.Should().Contain("Burden: Light");
    }

    [Fact]
    public async Task FormatInventoryDisplayAsync_WithItems_ShouldListItems()
    {
        // Arrange
        var entries = new List<InventoryItem>
        {
            new()
            {
                Item = _testItem,
                Quantity = 3,
                IsEquipped = false
            }
        };
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(entries);
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(300);

        // Act
        var result = await _sut.FormatInventoryDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("Health Potion");
        result.Should().Contain("x3");
        result.Should().Contain("0.3kg");
    }

    [Fact]
    public async Task FormatInventoryDisplayAsync_HeavyBurden_ShouldShowPenalty()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(40000);

        // Act
        var result = await _sut.FormatInventoryDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("Heavy (-2 Finesse)");
    }

    [Fact]
    public async Task FormatInventoryDisplayAsync_Overburdened_ShouldShowWarning()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(48000);

        // Act
        var result = await _sut.FormatInventoryDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("OVERBURDENED");
        result.Should().Contain("Cannot move");
    }

    #endregion

    #region FormatEquipmentDisplayAsync Tests

    [Fact]
    public async Task FormatEquipmentDisplayAsync_NoEquipment_ShouldShowEmptySlots()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _sut.FormatEquipmentDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("EQUIPMENT");
        result.Should().Contain("MainHand");
        result.Should().Contain("(empty)");
    }

    [Fact]
    public async Task FormatEquipmentDisplayAsync_WithEquipment_ShouldShowItems()
    {
        // Arrange
        var entries = new List<InventoryItem>
        {
            new()
            {
                Item = _testEquipment,
                IsEquipped = true
            }
        };
        _mockRepository
            .Setup(r => r.GetEquippedItemsAsync(_testCharacter.Id))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.FormatEquipmentDisplayAsync(_testCharacter);

        // Assert
        result.Should().Contain("MainHand");
        result.Should().Contain("Iron Sword");
    }

    #endregion
}
