using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the BodgingService class.
/// Validates repair and salvage mechanics including WITS-based rolls,
/// Scrap consumption, and durability changes.
/// </summary>
public class BodgingServiceTests
{
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ILogger<BodgingService>> _mockLogger;
    private readonly BodgingService _sut;

    private readonly Character _testCharacter;
    private readonly Equipment _damagedSword;
    private readonly Equipment _undamagedArmor;
    private readonly Item _scrap;
    private readonly Item _nonEquipment;

    public BodgingServiceTests()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockItemRepository = new Mock<IItemRepository>();
        _mockLogger = new Mock<ILogger<BodgingService>>();

        _sut = new BodgingService(
            _mockDiceService.Object,
            _mockInventoryService.Object,
            _mockItemRepository.Object,
            _mockLogger.Object);

        // Set up test character with WITS 5
        _testCharacter = new Character
        {
            Name = "Test Tinkerer",
            Wits = 5
        };

        // Set up test equipment - damaged sword (50/100 durability = 50 damage)
        _damagedSword = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Damaged Sword",
            ItemType = ItemType.Weapon,
            MaxDurability = 100,
            CurrentDurability = 50,
            Weight = 1000, // 1kg
            Quality = QualityTier.ClanForged
        };

        // Set up undamaged armor
        _undamagedArmor = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Pristine Armor",
            ItemType = ItemType.Armor,
            MaxDurability = 100,
            CurrentDurability = 100,
            Weight = 5000, // 5kg
            Quality = QualityTier.Optimized
        };

        // Set up scrap item
        _scrap = new Item
        {
            Id = Guid.NewGuid(),
            Name = "scrap",
            ItemType = ItemType.Material,
            IsStackable = true
        };

        // Set up non-equipment item
        _nonEquipment = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Potion",
            ItemType = ItemType.Consumable
        };
    }

    #region RepairItemAsync - Validation Tests

    [Fact]
    public async Task RepairItem_WithNonEquipment_ReturnsFailure()
    {
        // Arrange
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _nonEquipment, Quantity = 1 }
        };

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _nonEquipment.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("not equipment");
    }

    [Fact]
    public async Task RepairItem_WithUndamagedItem_ReturnsFailure()
    {
        // Arrange
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _undamagedArmor, Quantity = 1 }
        };

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _undamagedArmor.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("good condition");
    }

    [Fact]
    public async Task RepairItem_WithInsufficientScrap_ReturnsFailure()
    {
        // Arrange - Has damaged sword but no scrap
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _damagedSword, Quantity = 1 }
            // No scrap in inventory
        };

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("Insufficient Scrap");
    }

    [Fact]
    public async Task RepairItem_WithUnknownItem_ReturnsFailure()
    {
        // Arrange - Empty inventory
        _testCharacter.Inventory = new List<InventoryItem>();

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region RepairItemAsync - Outcome Tests

    [Fact]
    public async Task RepairItem_RollMeetsDc_RestoresDurability()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20); // Plenty of scrap
        // 50 damage / 5 = DC 8 + 10 = 18 for base DC
        // DC = 8 + (50 / 5) = 8 + 10 = 18
        SetupSuccessfulRepairRoll(netSuccesses: 18);
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(CraftingOutcome.Success);
        result.DurabilityRestored.Should().BeGreaterThan(0);
        _damagedSword.CurrentDurability.Should().BeGreaterThan(50);
    }

    [Fact]
    public async Task RepairItem_RollExceedsDcBy5_FullyRestores()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20);
        // DC = 8 + 10 = 18, Masterwork threshold = 18 + 5 = 23
        SetupSuccessfulRepairRoll(netSuccesses: 23);
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(CraftingOutcome.Masterwork);
        _damagedSword.CurrentDurability.Should().Be(_damagedSword.MaxDurability);
        result.Message.Should().Contain("MASTERWORK");
    }

    [Fact]
    public async Task RepairItem_RollBelowDc_ReturnsFailure()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20);
        SetupSuccessfulRepairRoll(netSuccesses: 5); // Below DC of 18
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.DurabilityRestored.Should().Be(0);
        _damagedSword.CurrentDurability.Should().Be(50); // Unchanged
    }

    [Fact]
    public async Task RepairItem_NetNegative_ReducesMaxDurability()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20);
        SetupCatastropheRepairRoll(); // Net < 0
        SetupInventoryServiceSuccess();

        var originalMax = _damagedSword.MaxDurability;

        // Act
        var result = await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Catastrophe);
        result.MaxDurabilityLost.Should().Be(10);
        _damagedSword.MaxDurability.Should().Be(originalMax - 10);
        result.Message.Should().Contain("CATASTROPHE");
    }

    [Fact]
    public async Task RepairItem_ConsumesScrap()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20);
        SetupSuccessfulRepairRoll(netSuccesses: 18);
        SetupInventoryServiceSuccess();

        // 50 damage / 5 = 10 Scrap required
        var expectedScrapCost = 10;

        // Act
        await _sut.RepairItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        _mockInventoryService.Verify(
            s => s.RemoveItemAsync(_testCharacter, "scrap", expectedScrapCost),
            Times.Once);
    }

    #endregion

    #region SalvageItemAsync Tests

    [Fact]
    public async Task SalvageItem_WithValidEquipment_YieldsScrap()
    {
        // Arrange
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _damagedSword, Quantity = 1 }
        };
        SetupInventoryServiceSuccess();
        SetupItemRepository();

        // Act
        var result = await _sut.SalvageItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ScrapYield.Should().BeGreaterThan(0);
        result.Message.Should().Contain("Salvaged");
    }

    [Fact]
    public async Task SalvageItem_WithNonEquipment_ReturnsFailure()
    {
        // Arrange
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _nonEquipment, Quantity = 1 }
        };

        // Act
        var result = await _sut.SalvageItemAsync(_testCharacter, _nonEquipment.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ScrapYield.Should().Be(0);
        result.Message.Should().Contain("not equipment");
    }

    [Fact]
    public async Task SalvageItem_RemovesItemFromInventory()
    {
        // Arrange
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _damagedSword, Quantity = 1 }
        };
        SetupInventoryServiceSuccess();
        SetupItemRepository();

        // Act
        await _sut.SalvageItemAsync(_testCharacter, _damagedSword.Id);

        // Assert
        _mockInventoryService.Verify(
            s => s.RemoveItemAsync(_testCharacter, _damagedSword.Name, 1),
            Times.Once);
    }

    [Fact]
    public void SalvageItem_HighQualityYieldsMoreScrap()
    {
        // Arrange
        var lowQualityEquip = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Junk Sword",
            ItemType = ItemType.Weapon,
            MaxDurability = 100,
            CurrentDurability = 100,
            Weight = 1000,
            Quality = QualityTier.JuryRigged // Quality modifier 0
        };

        var highQualityEquip = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Fine Sword",
            ItemType = ItemType.Weapon,
            MaxDurability = 100,
            CurrentDurability = 100,
            Weight = 1000,
            Quality = QualityTier.MythForged // Quality modifier 4
        };

        // Act
        var lowQualityYield = _sut.CalculateSalvageYield(lowQualityEquip);
        var highQualityYield = _sut.CalculateSalvageYield(highQualityEquip);

        // Assert
        highQualityYield.Should().BeGreaterThan(lowQualityYield);
    }

    [Fact]
    public void SalvageItem_HeavyItemYieldsMoreScrap()
    {
        // Arrange
        var lightEquip = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Dagger",
            ItemType = ItemType.Weapon,
            MaxDurability = 100,
            CurrentDurability = 100,
            Weight = 200, // 200g
            Quality = QualityTier.ClanForged
        };

        var heavyEquip = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Greataxe",
            ItemType = ItemType.Weapon,
            MaxDurability = 100,
            CurrentDurability = 100,
            Weight = 3000, // 3kg
            Quality = QualityTier.ClanForged
        };

        // Act
        var lightYield = _sut.CalculateSalvageYield(lightEquip);
        var heavyYield = _sut.CalculateSalvageYield(heavyEquip);

        // Assert
        heavyYield.Should().BeGreaterThan(lightYield);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public void CalculateRepairCost_ReturnsCorrectValue()
    {
        // Arrange - 50 damage on sword
        // Cost = Ceiling(50 / 5) = 10

        // Act
        var cost = _sut.CalculateRepairCost(_damagedSword);

        // Assert
        cost.Should().Be(10);
    }

    [Fact]
    public void CalculateRepairCost_MinimumOne()
    {
        // Arrange - Equipment with only 1 damage
        var slightlyDamaged = new Equipment
        {
            MaxDurability = 100,
            CurrentDurability = 99
        };

        // Act
        var cost = _sut.CalculateRepairCost(slightlyDamaged);

        // Assert
        cost.Should().Be(1);
    }

    [Fact]
    public void CalculateSalvageYield_ReturnsCorrectValue()
    {
        // Arrange
        // _damagedSword: Weight = 1000g, Quality = ClanForged (modifier 2)
        // Yield = (1000 / 100) * (2 + 1) = 10 * 3 = 30

        // Act
        var yield = _sut.CalculateSalvageYield(_damagedSword);

        // Assert
        yield.Should().Be(30);
    }

    [Fact]
    public void CalculateSalvageYield_MinimumOne()
    {
        // Arrange - Very light, low quality equipment
        var lightJunk = new Equipment
        {
            Weight = 50, // Only 50g
            Quality = QualityTier.JuryRigged // Modifier 0
        };
        // Yield = (50 / 100) * (0 + 1) = 0 * 1 = 0, but minimum is 1

        // Act
        var yield = _sut.CalculateSalvageYield(lightJunk);

        // Assert
        yield.Should().Be(1);
    }

    [Fact]
    public void CanRepair_WithSufficientScrap_ReturnsTrue()
    {
        // Arrange
        SetupCharacterWithDamagedSwordAndScrap(20);

        // Act
        var result = _sut.CanRepair(_testCharacter, _damagedSword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanRepair_WithInsufficientScrap_ReturnsFalse()
    {
        // Arrange - Only 5 scrap but need 10
        SetupCharacterWithDamagedSwordAndScrap(5);

        // Act
        var result = _sut.CanRepair(_testCharacter, _damagedSword);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private void SetupCharacterWithDamagedSwordAndScrap(int scrapQuantity)
    {
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = _damagedSword, Quantity = 1 },
            new InventoryItem { Item = _scrap, Quantity = scrapQuantity }
        };
    }

    private void SetupSuccessfulRepairRoll(int netSuccesses)
    {
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(netSuccesses, 0, new List<int> { 8, 8, 8, 8, 8 }));
    }

    private void SetupCatastropheRepairRoll()
    {
        // Net negative: more botches than successes
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 3, new List<int> { 8, 1, 1, 1, 4 }));
    }

    private void SetupInventoryServiceSuccess()
    {
        _mockInventoryService
            .Setup(s => s.RemoveItemAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Removed"));
        _mockInventoryService
            .Setup(s => s.AddItemAsync(It.IsAny<Character>(), It.IsAny<Item>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Added"));
    }

    private void SetupItemRepository()
    {
        _mockItemRepository
            .Setup(r => r.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(_scrap);
    }

    #endregion
}
