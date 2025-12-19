using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for Item and Equipment entity persistence.
/// Uses InMemory database to test ItemRepository operations with TPH inheritance.
/// </summary>
public class ItemPersistenceTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly ItemRepository _repository;

    public ItemPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);

        var genericLoggerMock = new Mock<ILogger<GenericRepository<Item>>>();
        var itemLoggerMock = new Mock<ILogger<ItemRepository>>();

        _repository = new ItemRepository(_context, genericLoggerMock.Object, itemLoggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Item CRUD Tests

    [Fact]
    public async Task AddAsync_ShouldPersistItem()
    {
        // Arrange
        var item = CreateTestItem("Health Potion");

        // Act
        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(item.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Health Potion");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllItemProperties()
    {
        // Arrange
        var item = new Item
        {
            Name = "Full Item",
            ItemType = ItemType.Consumable,
            Description = "A fully configured item.",
            DetailedDescription = "Detailed description.",
            Weight = 500,
            Value = 100,
            Quality = QualityTier.ClanForged,
            IsStackable = true,
            MaxStackSize = 20
        };

        // Act
        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(item.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Full Item");
        retrieved.ItemType.Should().Be(ItemType.Consumable);
        retrieved.Description.Should().Be("A fully configured item.");
        retrieved.DetailedDescription.Should().Be("Detailed description.");
        retrieved.Weight.Should().Be(500);
        retrieved.Value.Should().Be(100);
        retrieved.Quality.Should().Be(QualityTier.ClanForged);
        retrieved.IsStackable.Should().BeTrue();
        retrieved.MaxStackSize.Should().Be(20);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyItem()
    {
        // Arrange
        var item = CreateTestItem("Original Name");
        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        // Act
        item.Name = "Updated Name";
        item.Quality = QualityTier.Optimized;
        await _repository.UpdateAsync(item);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(item.Id);
        retrieved!.Name.Should().Be("Updated Name");
        retrieved.Quality.Should().Be(QualityTier.Optimized);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        // Arrange
        var item = CreateTestItem("To Delete");
        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(item.Id);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(item.Id);
        retrieved.Should().BeNull();
    }

    #endregion

    #region GetByQualityAsync Tests

    [Fact]
    public async Task GetByQualityAsync_ReturnsItemsOfQuality()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Item 1", QualityTier.Scavenged));
        await _repository.AddAsync(CreateTestItem("Item 2", QualityTier.Scavenged));
        await _repository.AddAsync(CreateTestItem("Item 3", QualityTier.ClanForged));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByQualityAsync(QualityTier.Scavenged);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.Quality == QualityTier.Scavenged);
    }

    [Fact]
    public async Task GetByQualityAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Item 1", QualityTier.Scavenged));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByQualityAsync(QualityTier.MythForged);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsItemsOfType()
    {
        // Arrange
        var consumable = CreateTestItem("Potion");
        consumable.ItemType = ItemType.Consumable;
        var material = CreateTestItem("Ore");
        material.ItemType = ItemType.Material;

        await _repository.AddAsync(consumable);
        await _repository.AddAsync(material);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(ItemType.Consumable);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Potion");
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_FindsExactMatch()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Iron Sword"));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Iron Sword");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Iron Sword");
    }

    [Fact]
    public async Task GetByNameAsync_CaseInsensitive()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Iron Sword"));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("iron sword");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByNameAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Iron Sword"));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Steel Sword");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region TPH Equipment Tests

    [Fact]
    public async Task AddAsync_ShouldPersistEquipment()
    {
        // Arrange
        var equipment = CreateTestEquipment("Iron Helm");

        // Act
        await _repository.AddAsync(equipment);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(equipment.Id);
        retrieved.Should().NotBeNull();
        retrieved.Should().BeOfType<Equipment>();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEquipmentWithBonuses()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Enchanted Gauntlets",
            Slot = EquipmentSlot.Hands,
            ItemType = ItemType.Armor,
            Description = "Gauntlets that enhance strength.",
            SoakBonus = 1,
            AttributeBonuses = new Dictionary<CharacterAttribute, int>
            {
                [CharacterAttribute.Might] = 2,
                [CharacterAttribute.Finesse] = 1
            }
        };

        // Act
        await _repository.AddAsync(equipment);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(equipment.Id) as Equipment;
        retrieved.Should().NotBeNull();
        retrieved!.Slot.Should().Be(EquipmentSlot.Hands);
        retrieved.SoakBonus.Should().Be(1);
        retrieved.AttributeBonuses.Should().HaveCount(2);
        retrieved.GetAttributeBonus(CharacterAttribute.Might).Should().Be(2);
    }

    [Fact]
    public async Task GetAllEquipmentAsync_ReturnsOnlyEquipment()
    {
        // Arrange
        await _repository.AddAsync(CreateTestItem("Health Potion"));
        await _repository.AddAsync(CreateTestEquipment("Iron Sword"));
        await _repository.AddAsync(CreateTestEquipment("Iron Helm"));
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllEquipmentAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<Equipment>();
    }

    [Fact]
    public async Task GetEquipmentBySlotAsync_ReturnsEquipmentForSlot()
    {
        // Arrange
        var weapon = CreateTestEquipment("Sword");
        weapon.Slot = EquipmentSlot.MainHand;
        var helm = CreateTestEquipment("Helm");
        helm.Slot = EquipmentSlot.Head;

        await _repository.AddAsync(weapon);
        await _repository.AddAsync(helm);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetEquipmentBySlotAsync(EquipmentSlot.MainHand);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Sword");
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleItems()
    {
        // Arrange
        var items = new[]
        {
            CreateTestItem("Item 1"),
            CreateTestItem("Item 2"),
            CreateTestEquipment("Equipment 1")
        };

        // Act
        await _repository.AddRangeAsync(items);
        await _repository.SaveChangesAsync();

        // Assert
        var allItems = await _repository.GetAllAsync();
        allItems.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private Item CreateTestItem(string name, QualityTier quality = QualityTier.Scavenged)
    {
        return new Item
        {
            Name = name,
            ItemType = ItemType.Junk,
            Description = $"A {name.ToLower()}.",
            Quality = quality,
            Weight = 100,
            Value = 10
        };
    }

    private Equipment CreateTestEquipment(string name)
    {
        return new Equipment
        {
            Name = name,
            ItemType = ItemType.Weapon,
            Description = $"A {name.ToLower()}.",
            Slot = EquipmentSlot.MainHand,
            Quality = QualityTier.Scavenged,
            Weight = 1500,
            Value = 100,
            DamageDie = 6
        };
    }

    #endregion
}
