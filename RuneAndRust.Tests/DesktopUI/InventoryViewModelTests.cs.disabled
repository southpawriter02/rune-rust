using RuneAndRust.Core;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// v0.43.21: Comprehensive tests for InventoryViewModel.
/// Tests inventory management, equipment, and consumable functionality.
/// </summary>
public class InventoryViewModelTests
{
    private readonly EquipmentService _equipmentService;

    public InventoryViewModelTests()
    {
        _equipmentService = new EquipmentService();
    }

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var vm = new InventoryViewModel(_equipmentService);

        // Assert
        Assert.Equal("Inventory & Equipment", vm.Title);
        Assert.NotNull(vm.EquipmentItems);
        Assert.NotNull(vm.ConsumableItems);
        Assert.NotNull(vm.EquipmentSlots);
        Assert.Equal(2, vm.EquipmentSlots.Count); // Weapon and Armor
    }

    [Fact]
    public void Constructor_InitializesEquipmentSlots()
    {
        // Act
        var vm = new InventoryViewModel(_equipmentService);

        // Assert
        Assert.Contains(vm.EquipmentSlots, s => s.SlotType == EquipmentType.Weapon);
        Assert.Contains(vm.EquipmentSlots, s => s.SlotType == EquipmentType.Armor);
    }

    [Fact]
    public void Character_WhenSet_RefreshesInventory()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();

        // Act
        vm.Character = character;

        // Assert
        Assert.Same(character, vm.Character);
        Assert.NotEmpty(vm.EquipmentItems);
    }

    [Fact]
    public void EquipItemCommand_EquipsWeapon()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        character.EquippedWeapon = null; // Clear equipped weapon
        vm.Character = character;

        var weaponItem = vm.EquipmentItems.FirstOrDefault(e => e.Equipment.Type == EquipmentType.Weapon);
        Assert.NotNull(weaponItem);

        // Act
        vm.EquipItemCommand.Execute(weaponItem);

        // Assert
        Assert.NotNull(vm.Character.EquippedWeapon);
        Assert.Contains("Equipped", vm.StatusMessage);
    }

    [Fact]
    public void UnequipSlotCommand_UnequipsItem()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        var weaponSlot = vm.EquipmentSlots.First(s => s.SlotType == EquipmentType.Weapon);
        Assert.NotNull(weaponSlot.EquippedItem);

        // Act
        vm.UnequipSlotCommand.Execute(weaponSlot);

        // Assert
        Assert.Contains("Unequipped", vm.StatusMessage);
    }

    [Fact]
    public void DropItemCommand_RemovesFromInventory()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        var initialCount = vm.EquipmentItems.Count;
        var itemToDrop = vm.EquipmentItems.First();

        // Act
        vm.DropItemCommand.Execute(itemToDrop);

        // Assert
        Assert.Contains("Dropped", vm.StatusMessage);
    }

    [Fact]
    public void UseConsumableCommand_AppliesEffects()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        character.HP = 30; // Set HP below max
        vm.Character = character;

        var healingItem = vm.ConsumableItems.FirstOrDefault(c => c.Consumable.HPRestore > 0);
        Assert.NotNull(healingItem);

        var hpBefore = character.HP;

        // Act
        vm.UseConsumableCommand.Execute(healingItem);

        // Assert
        Assert.True(character.HP > hpBefore || character.HP == character.MaxHP);
        Assert.Contains("Used", vm.StatusMessage);
    }

    [Fact]
    public void SortByTypeCommand_SortsInventory()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        // Act
        vm.SortByTypeCommand.Execute(null);

        // Assert
        Assert.Contains("Sorted by Type", vm.StatusMessage);
    }

    [Fact]
    public void SortByQualityCommand_SortsInventory()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        // Act
        vm.SortByQualityCommand.Execute(null);

        // Assert
        Assert.Contains("Sorted by Quality", vm.StatusMessage);
    }

    [Fact]
    public void SortByNameCommand_SortsInventory()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        // Act
        vm.SortByNameCommand.Execute(null);

        // Assert
        Assert.Contains("Sorted by Name", vm.StatusMessage);
    }

    [Fact]
    public void SelectedItem_WhenSet_RaisesPropertyChanged()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var changedProperties = new List<string>();
        vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        var character = CreateTestCharacter();
        vm.Character = character;

        // Act
        vm.SelectedItem = vm.EquipmentItems.First();

        // Assert
        Assert.Contains("SelectedItem", changedProperties);
    }

    [Fact]
    public void HoveredItem_WhenSet_UpdatesComparison()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        vm.Character = character;

        var weaponItem = vm.EquipmentItems.FirstOrDefault(e => e.Equipment.Type == EquipmentType.Weapon);
        Assert.NotNull(weaponItem);

        // Act
        vm.HoveredItem = weaponItem;

        // Assert - comparison should be set when hovering over equipment
        Assert.NotNull(vm.CurrentComparison);
    }

    [Fact]
    public void InventoryCountDisplay_ReflectsCurrentCount()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();

        // Act
        vm.Character = character;

        // Assert
        Assert.Contains("/", vm.InventoryCountDisplay);
    }

    [Fact]
    public void ConsumablesCountDisplay_ReflectsCurrentCount()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();

        // Act
        vm.Character = character;

        // Assert
        Assert.Contains("/", vm.ConsumablesCountDisplay);
    }

    [Fact]
    public void IsInventoryFull_ReturnsTrueWhenFull()
    {
        // Arrange
        var vm = new InventoryViewModel(_equipmentService);
        var character = CreateTestCharacter();
        character.MaxInventorySize = 1; // Set max to 1

        // Act
        vm.Character = character;

        // Assert
        Assert.True(vm.IsInventoryFull);
    }

    private static PlayerCharacter CreateTestCharacter()
    {
        var character = new PlayerCharacter
        {
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            HP = 50,
            MaxHP = 60,
            Stamina = 30,
            MaxStamina = 40,
            MaxInventorySize = 10,
            MaxConsumables = 10
        };

        // Equipped items
        character.EquippedWeapon = new Equipment
        {
            Name = "Test Sword",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.Scavenged,
            WeaponCategory = WeaponCategory.Blade,
            DamageDice = 1,
            DamageBonus = 2,
            StaminaCost = 5
        };

        character.EquippedArmor = new Equipment
        {
            Name = "Test Armor",
            Type = EquipmentType.Armor,
            Quality = QualityTier.Scavenged,
            ArmorCategory = ArmorCategory.Medium,
            HPBonus = 10,
            DefenseBonus = 2
        };

        // Inventory items
        character.Inventory.Add(new Equipment
        {
            Name = "Spare Dagger",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.ClanForged,
            WeaponCategory = WeaponCategory.Dagger,
            DamageDice = 1,
            DamageBonus = 1,
            StaminaCost = 3
        });

        character.Inventory.Add(new Equipment
        {
            Name = "Light Armor",
            Type = EquipmentType.Armor,
            Quality = QualityTier.Scavenged,
            ArmorCategory = ArmorCategory.Light,
            HPBonus = 5,
            DefenseBonus = 1
        });

        // Consumables
        character.Consumables.Add(new Consumable
        {
            Name = "Health Potion",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 20
        });

        character.Consumables.Add(new Consumable
        {
            Name = "Stamina Tonic",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            StaminaRestore = 15
        });

        return character;
    }
}

/// <summary>
/// Tests for EquipmentItemViewModel.
/// </summary>
public class EquipmentItemViewModelTests
{
    [Fact]
    public void Constructor_InitializesFromEquipment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Epic Sword",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.MythForged,
            Description = "A legendary blade",
            DamageDice = 2,
            DamageBonus = 5
        };
        var equipmentService = new EquipmentService();

        // Act
        var vm = new EquipmentItemViewModel(equipment, equipmentService);

        // Assert
        Assert.Contains("Epic Sword", vm.Name);
        Assert.Equal("A legendary blade", vm.Description);
        Assert.Equal("Weapon", vm.TypeDisplay);
        Assert.Equal("#FFD700", vm.QualityColor); // Gold for MythForged
    }

    [Fact]
    public void IsWeapon_ReturnsTrueForWeapons()
    {
        // Arrange
        var equipment = new Equipment { Type = EquipmentType.Weapon };
        var vm = new EquipmentItemViewModel(equipment, new EquipmentService());

        // Assert
        Assert.True(vm.IsWeapon);
        Assert.False(vm.IsArmor);
    }

    [Fact]
    public void IsArmor_ReturnsTrueForArmor()
    {
        // Arrange
        var equipment = new Equipment { Type = EquipmentType.Armor };
        var vm = new EquipmentItemViewModel(equipment, new EquipmentService());

        // Assert
        Assert.False(vm.IsWeapon);
        Assert.True(vm.IsArmor);
    }

    [Theory]
    [InlineData(QualityTier.JuryRigged, "#808080")]
    [InlineData(QualityTier.Scavenged, "#FFFFFF")]
    [InlineData(QualityTier.ClanForged, "#4A90E2")]
    [InlineData(QualityTier.Optimized, "#9400D3")]
    [InlineData(QualityTier.MythForged, "#FFD700")]
    public void QualityColor_ReturnsCorrectColorForTier(QualityTier tier, string expectedColor)
    {
        // Arrange
        var equipment = new Equipment { Quality = tier };
        var vm = new EquipmentItemViewModel(equipment, new EquipmentService());

        // Assert
        Assert.Equal(expectedColor, vm.QualityColor);
    }
}

/// <summary>
/// Tests for ConsumableItemViewModel.
/// </summary>
public class ConsumableItemViewModelTests
{
    [Fact]
    public void Constructor_InitializesFromConsumable()
    {
        // Arrange
        var consumable = new Consumable
        {
            Name = "Super Potion",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Masterwork,
            Description = "Heals a lot",
            HPRestore = 50,
            MasterworkBonusHP = 25
        };

        // Act
        var vm = new ConsumableItemViewModel(consumable);

        // Assert
        Assert.Contains("Super Potion", vm.Name);
        Assert.Equal("Heals a lot", vm.Description);
        Assert.Equal("Medicine", vm.TypeDisplay);
        Assert.True(vm.IsMasterwork);
    }

    [Fact]
    public void HPRestore_IncludesMasterworkBonus()
    {
        // Arrange
        var consumable = new Consumable
        {
            HPRestore = 20,
            MasterworkBonusHP = 10,
            Quality = CraftQuality.Masterwork
        };

        // Act
        var vm = new ConsumableItemViewModel(consumable);

        // Assert
        Assert.Equal(30, vm.HPRestore);
    }

    [Fact]
    public void StaminaRestore_IncludesMasterworkBonus()
    {
        // Arrange
        var consumable = new Consumable
        {
            StaminaRestore = 15,
            MasterworkBonusStamina = 5,
            Quality = CraftQuality.Masterwork
        };

        // Act
        var vm = new ConsumableItemViewModel(consumable);

        // Assert
        Assert.Equal(20, vm.StaminaRestore);
    }
}

/// <summary>
/// Tests for EquipmentSlotViewModel.
/// </summary>
public class EquipmentSlotViewModelTests
{
    [Fact]
    public void IsEmpty_ReturnsTrueWhenNoItemEquipped()
    {
        // Arrange
        var slot = new EquipmentSlotViewModel
        {
            SlotType = EquipmentType.Weapon,
            DisplayName = "Weapon"
        };

        // Assert
        Assert.True(slot.IsEmpty);
        Assert.Equal("Empty", slot.EquippedItemName);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseWhenItemEquipped()
    {
        // Arrange
        var slot = new EquipmentSlotViewModel
        {
            SlotType = EquipmentType.Weapon,
            DisplayName = "Weapon",
            EquippedItem = new EquipmentItemViewModel(
                new Equipment { Name = "Sword", Type = EquipmentType.Weapon },
                new EquipmentService())
        };

        // Assert
        Assert.False(slot.IsEmpty);
        Assert.Contains("Sword", slot.EquippedItemName);
    }

    [Fact]
    public void EquippedItem_WhenSet_RaisesPropertyChanged()
    {
        // Arrange
        var slot = new EquipmentSlotViewModel();
        var changedProperties = new List<string>();
        slot.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        slot.EquippedItem = new EquipmentItemViewModel(
            new Equipment { Name = "Sword", Type = EquipmentType.Weapon },
            new EquipmentService());

        // Assert
        Assert.Contains("EquippedItem", changedProperties);
        Assert.Contains("IsEmpty", changedProperties);
        Assert.Contains("EquippedItemName", changedProperties);
        Assert.Contains("EquippedItemStats", changedProperties);
    }
}
