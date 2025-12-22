using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the InventoryService.GetViewModelAsync method (v0.3.7a).
/// Validates the inventory view model generation for the new UI system.
/// </summary>
public class InventoryServiceViewModelTests
{
    private readonly Mock<IInventoryRepository> _mockRepository;
    private readonly Mock<ILogger<InventoryService>> _mockLogger;
    private readonly InventoryService _sut;
    private readonly Character _testCharacter;

    public InventoryServiceViewModelTests()
    {
        _mockRepository = new Mock<IInventoryRepository>();
        _mockLogger = new Mock<ILogger<InventoryService>>();
        _sut = new InventoryService(_mockRepository.Object, _mockLogger.Object);

        _testCharacter = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Hero",
            Might = 5 // Max capacity = 5 * 10000g = 50,000g (50kg)
        };
    }

    #region GetViewModelAsync Tests

    [Fact]
    public async Task GetViewModelAsync_EmptyInventory_ReturnsEmptyLists()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert
        result.CharacterName.Should().Be("Test Hero");
        result.BackpackItems.Should().BeEmpty();
        result.EquippedItems.Should().HaveCount(7); // All 7 slots
        result.EquippedItems.Values.Should().OnlyContain(v => v == null);
        result.CurrentWeight.Should().Be(0);
        result.BurdenPercentage.Should().Be(0);
        result.BurdenState.Should().Be(BurdenState.Light);
    }

    [Fact]
    public async Task GetViewModelAsync_WithEquippedItem_PopulatesEquippedDict()
    {
        // Arrange
        var sword = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Iron Sword",
            Weight = 1500,
            Slot = EquipmentSlot.MainHand,
            Quality = QualityTier.ClanForged,
            CurrentDurability = 80,
            MaxDurability = 100
        };
        var inventoryEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = sword.Id,
            Item = sword,
            Quantity = 1,
            IsEquipped = true
        };

        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { inventoryEntry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(1500);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert
        result.EquippedItems[EquipmentSlot.MainHand].Should().NotBeNull();
        result.EquippedItems[EquipmentSlot.MainHand]!.Name.Should().Be("Iron Sword");
        result.EquippedItems[EquipmentSlot.MainHand]!.Quality.Should().Be(QualityTier.ClanForged);
        result.EquippedItems[EquipmentSlot.MainHand]!.DurabilityPercentage.Should().Be(80);
        result.EquippedItems[EquipmentSlot.MainHand]!.IsBroken.Should().BeFalse();
        result.BackpackItems.Should().BeEmpty(); // Equipped item not in backpack
    }

    [Fact]
    public async Task GetViewModelAsync_AllSlotsEmpty_AllSlotsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert - All 7 slots should be present but null
        result.EquippedItems.Should().ContainKey(EquipmentSlot.MainHand);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.OffHand);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.Head);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.Body);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.Hands);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.Feet);
        result.EquippedItems.Should().ContainKey(EquipmentSlot.Accessory);

        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            result.EquippedItems[slot].Should().BeNull($"slot {slot} should be empty");
        }
    }

    [Fact]
    public async Task GetViewModelAsync_WithBackpackItems_ReturnsOrderedList()
    {
        // Arrange
        var potion = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Health Potion",
            Weight = 100,
            Quality = QualityTier.Scavenged,
            ItemType = ItemType.Consumable
        };
        var torch = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Torch",
            Weight = 200,
            Quality = QualityTier.JuryRigged,
            ItemType = ItemType.Material
        };

        var entry1 = new InventoryItem { Item = potion, Quantity = 1, SlotPosition = 1, IsEquipped = false };
        var entry2 = new InventoryItem { Item = torch, Quantity = 2, SlotPosition = 0, IsEquipped = false };

        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { entry1, entry2 });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(500);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert - Should be ordered by SlotPosition
        result.BackpackItems.Should().HaveCount(2);
        result.BackpackItems[0].Name.Should().Be("Torch"); // SlotPosition 0
        result.BackpackItems[1].Name.Should().Be("Health Potion"); // SlotPosition 1
        result.BackpackItems[0].Index.Should().Be(1); // 1-based display index
        result.BackpackItems[1].Index.Should().Be(2);
    }

    [Fact]
    public async Task GetViewModelAsync_CalculatesBurdenPercentageCorrectly()
    {
        // Arrange
        // Character has Might 5 = 50,000g capacity
        // Weight at 20,000g = 40% (Light)
        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem>());
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(20000);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert
        result.CurrentWeight.Should().Be(20000);
        result.MaxCapacity.Should().Be(50000);
        result.BurdenPercentage.Should().Be(40);
        result.BurdenState.Should().Be(BurdenState.Light);
    }

    [Fact]
    public async Task GetViewModelAsync_BrokenEquipment_SetsIsBrokenTrue()
    {
        // Arrange
        var brokenArmor = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Broken Armor",
            Weight = 2000,
            Slot = EquipmentSlot.Body,
            Quality = QualityTier.Scavenged,
            CurrentDurability = 0, // Broken
            MaxDurability = 100
        };
        var inventoryEntry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = brokenArmor.Id,
            Item = brokenArmor,
            Quantity = 1,
            IsEquipped = true
        };

        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { inventoryEntry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(2000);

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert
        result.EquippedItems[EquipmentSlot.Body].Should().NotBeNull();
        result.EquippedItems[EquipmentSlot.Body]!.IsBroken.Should().BeTrue();
        result.EquippedItems[EquipmentSlot.Body]!.DurabilityPercentage.Should().Be(0);
    }

    [Fact]
    public async Task GetViewModelAsync_SelectedIndexClamped_WhenOutOfRange()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Single Item",
            Weight = 100,
            Quality = QualityTier.Scavenged,
            ItemType = ItemType.Consumable
        };
        var entry = new InventoryItem { Item = item, Quantity = 1, SlotPosition = 0, IsEquipped = false };

        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { entry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(100);

        // Act - Pass index way out of range
        var result = await _sut.GetViewModelAsync(_testCharacter, selectedIndex: 999);

        // Assert - Should be clamped to valid range
        result.SelectedIndex.Should().Be(0); // Only 1 item, so max index is 0
    }

    [Fact]
    public async Task GetViewModelAsync_StackedItems_ShowsCorrectQuantity()
    {
        // Arrange
        var potion = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Health Potion",
            Weight = 100,
            Quality = QualityTier.Scavenged,
            ItemType = ItemType.Consumable,
            IsStackable = true,
            MaxStackSize = 10
        };
        var entry = new InventoryItem
        {
            Item = potion,
            Quantity = 5, // Stack of 5
            SlotPosition = 0,
            IsEquipped = false
        };

        _mockRepository
            .Setup(r => r.GetByCharacterIdAsync(_testCharacter.Id))
            .ReturnsAsync(new List<InventoryItem> { entry });
        _mockRepository
            .Setup(r => r.GetTotalWeightAsync(_testCharacter.Id))
            .ReturnsAsync(500); // 5 * 100g

        // Act
        var result = await _sut.GetViewModelAsync(_testCharacter);

        // Assert
        result.BackpackItems.Should().HaveCount(1);
        result.BackpackItems[0].Quantity.Should().Be(5);
        result.BackpackItems[0].WeightGrams.Should().Be(500); // Total stack weight
    }

    #endregion
}
