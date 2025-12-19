using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for InventoryItem entity persistence.
/// Uses InMemory database to test InventoryRepository operations.
/// </summary>
public class InventoryPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly InventoryRepository _inventoryRepository;
    private readonly Character _testCharacter;
    private readonly Item _testItem;
    private readonly Equipment _testEquipment;

    public InventoryPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var loggerMock = new Mock<ILogger<InventoryRepository>>();
        _inventoryRepository = new InventoryRepository(_context, loggerMock.Object);

        // Create test entities
        _testCharacter = new Character { Name = "Test Hero" };
        _testItem = new Item { Name = "Health Potion", Weight = 100, IsStackable = true, MaxStackSize = 10 };
        _testEquipment = new Equipment { Name = "Iron Sword", Slot = EquipmentSlot.MainHand, Weight = 1500 };

        // Add to context
        _context.Characters.Add(_testCharacter);
        _context.Items.Add(_testItem);
        _context.Items.Add(_testEquipment);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistInventoryEntry()
    {
        // Arrange
        var entry = CreateInventoryEntry(_testItem, 1);

        // Act
        await _inventoryRepository.AddAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _inventoryRepository.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllProperties()
    {
        // Arrange
        var entry = new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = _testItem.Id,
            Quantity = 5,
            SlotPosition = 3,
            IsEquipped = false
        };

        // Act
        await _inventoryRepository.AddAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _inventoryRepository.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Quantity.Should().Be(5);
        retrieved.SlotPosition.Should().Be(3);
        retrieved.IsEquipped.Should().BeFalse();
    }

    #endregion

    #region GetByCharacterIdAsync Tests

    [Fact]
    public async Task GetByCharacterIdAsync_ReturnsAllInventoryItems()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 2);
        await AddInventoryEntry(_testEquipment, 1);

        // Act
        var result = await _inventoryRepository.GetByCharacterIdAsync(_testCharacter.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCharacterIdAsync_IncludesItemNavigation()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 1);

        // Act
        var result = await _inventoryRepository.GetByCharacterIdAsync(_testCharacter.Id);

        // Assert
        result.First().Item.Should().NotBeNull();
        result.First().Item.Name.Should().Be("Health Potion");
    }

    [Fact]
    public async Task GetByCharacterIdAsync_ReturnsOrderedBySlotPosition()
    {
        // Arrange
        var entry1 = CreateInventoryEntry(_testItem, 1);
        entry1.SlotPosition = 2;
        var entry2 = CreateInventoryEntry(_testEquipment, 1);
        entry2.SlotPosition = 1;

        await _inventoryRepository.AddAsync(entry1);
        await _inventoryRepository.AddAsync(entry2);
        await _inventoryRepository.SaveChangesAsync();

        // Act
        var result = await _inventoryRepository.GetByCharacterIdAsync(_testCharacter.Id);

        // Assert
        result.First().SlotPosition.Should().Be(1);
        result.Last().SlotPosition.Should().Be(2);
    }

    [Fact]
    public async Task GetByCharacterIdAsync_EmptyInventory_ReturnsEmptyList()
    {
        // Act
        var result = await _inventoryRepository.GetByCharacterIdAsync(_testCharacter.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetEquippedItemsAsync Tests

    [Fact]
    public async Task GetEquippedItemsAsync_ReturnsOnlyEquipped()
    {
        // Arrange
        var equipped = CreateInventoryEntry(_testEquipment, 1);
        equipped.IsEquipped = true;
        var unequipped = CreateInventoryEntry(_testItem, 3);
        unequipped.IsEquipped = false;

        await _inventoryRepository.AddAsync(equipped);
        await _inventoryRepository.AddAsync(unequipped);
        await _inventoryRepository.SaveChangesAsync();

        // Act
        var result = await _inventoryRepository.GetEquippedItemsAsync(_testCharacter.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Item.Name.Should().Be("Iron Sword");
    }

    #endregion

    #region GetEquippedInSlotAsync Tests

    [Fact]
    public async Task GetEquippedInSlotAsync_ReturnsEquippedItem()
    {
        // Arrange
        var equipped = CreateInventoryEntry(_testEquipment, 1);
        equipped.IsEquipped = true;
        await _inventoryRepository.AddAsync(equipped);
        await _inventoryRepository.SaveChangesAsync();

        // Act
        var result = await _inventoryRepository.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.MainHand);

        // Assert
        result.Should().NotBeNull();
        result!.Item.Name.Should().Be("Iron Sword");
    }

    [Fact]
    public async Task GetEquippedInSlotAsync_EmptySlot_ReturnsNull()
    {
        // Act
        var result = await _inventoryRepository.GetEquippedInSlotAsync(_testCharacter.Id, EquipmentSlot.Head);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region FindByItemNameAsync Tests

    [Fact]
    public async Task FindByItemNameAsync_FindsItem()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 5);

        // Act
        var result = await _inventoryRepository.FindByItemNameAsync(_testCharacter.Id, "Health Potion");

        // Assert
        result.Should().NotBeNull();
        result!.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task FindByItemNameAsync_CaseInsensitive()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 1);

        // Act
        var result = await _inventoryRepository.FindByItemNameAsync(_testCharacter.Id, "HEALTH POTION");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FindByItemNameAsync_NotFound_ReturnsNull()
    {
        // Act
        var result = await _inventoryRepository.FindByItemNameAsync(_testCharacter.Id, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTotalWeightAsync Tests

    [Fact]
    public async Task GetTotalWeightAsync_CalculatesTotalWeight()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 3); // 100g * 3 = 300g
        await AddInventoryEntry(_testEquipment, 1); // 1500g * 1 = 1500g

        // Act
        var result = await _inventoryRepository.GetTotalWeightAsync(_testCharacter.Id);

        // Assert
        result.Should().Be(1800);
    }

    [Fact]
    public async Task GetTotalWeightAsync_EmptyInventory_ReturnsZero()
    {
        // Act
        var result = await _inventoryRepository.GetTotalWeightAsync(_testCharacter.Id);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region GetItemCountAsync Tests

    [Fact]
    public async Task GetItemCountAsync_CountsSlots()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 10);
        await AddInventoryEntry(_testEquipment, 1);

        // Act
        var result = await _inventoryRepository.GetItemCountAsync(_testCharacter.Id);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldModifyEntry()
    {
        // Arrange
        var entry = CreateInventoryEntry(_testItem, 1);
        await _inventoryRepository.AddAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        // Act
        entry.Quantity = 5;
        await _inventoryRepository.UpdateAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _inventoryRepository.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id);
        retrieved!.Quantity.Should().Be(5);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_ShouldRemoveEntry()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 1);

        // Act
        await _inventoryRepository.RemoveAsync(_testCharacter.Id, _testItem.Id);
        await _inventoryRepository.SaveChangesAsync();

        // Assert
        var retrieved = await _inventoryRepository.GetByCharacterAndItemAsync(_testCharacter.Id, _testItem.Id);
        retrieved.Should().BeNull();
    }

    #endregion

    #region ClearInventoryAsync Tests

    [Fact]
    public async Task ClearInventoryAsync_RemovesAllEntries()
    {
        // Arrange
        await AddInventoryEntry(_testItem, 3);
        await AddInventoryEntry(_testEquipment, 1);

        // Act
        await _inventoryRepository.ClearInventoryAsync(_testCharacter.Id);

        // Assert
        var result = await _inventoryRepository.GetByCharacterIdAsync(_testCharacter.Id);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearInventoryAsync_DoesNotAffectOtherCharacters()
    {
        // Arrange
        var otherCharacter = new Character { Name = "Other Hero" };
        _context.Characters.Add(otherCharacter);
        await _context.SaveChangesAsync();

        await AddInventoryEntry(_testItem, 1);
        var otherEntry = new InventoryItem
        {
            CharacterId = otherCharacter.Id,
            ItemId = _testItem.Id,
            Quantity = 2
        };
        await _inventoryRepository.AddAsync(otherEntry);
        await _inventoryRepository.SaveChangesAsync();

        // Act
        await _inventoryRepository.ClearInventoryAsync(_testCharacter.Id);

        // Assert
        var otherInventory = await _inventoryRepository.GetByCharacterIdAsync(otherCharacter.Id);
        otherInventory.Should().HaveCount(1);
    }

    #endregion

    #region Helper Methods

    private InventoryItem CreateInventoryEntry(Item item, int quantity)
    {
        return new InventoryItem
        {
            CharacterId = _testCharacter.Id,
            ItemId = item.Id,
            Quantity = quantity,
            IsEquipped = false
        };
    }

    private async Task AddInventoryEntry(Item item, int quantity)
    {
        var entry = CreateInventoryEntry(item, quantity);
        await _inventoryRepository.AddAsync(entry);
        await _inventoryRepository.SaveChangesAsync();
    }

    #endregion
}
