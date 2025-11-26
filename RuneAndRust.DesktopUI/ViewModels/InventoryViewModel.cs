using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.Engine;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the inventory and equipment management view.
/// Implements v0.43.10: Inventory & Equipment UI.
/// v0.44.5: Integrates with LootController for post-combat loot collection.
/// </summary>
public class InventoryViewModel : ViewModelBase
{
    private readonly EquipmentService _equipmentService;
    private readonly GameStateController? _gameStateController;
    private readonly LootController? _lootController;
    private PlayerCharacter? _character;
    private InventoryItemViewModel? _selectedItem;
    private InventoryItemViewModel? _hoveredItem;
    private EquipmentComparison? _currentComparison;
    private string _statusMessage = string.Empty;
    private bool _isInLootCollectionMode;

    /// <summary>
    /// Gets the view title.
    /// </summary>
    public string Title => "Inventory & Equipment";

    /// <summary>
    /// Gets or sets the character whose inventory is being displayed.
    /// </summary>
    public PlayerCharacter? Character
    {
        get => _character;
        set
        {
            this.RaiseAndSetIfChanged(ref _character, value);
            RefreshInventory();
        }
    }

    /// <summary>
    /// Gets the collection of equipment items in inventory.
    /// </summary>
    public ObservableCollection<EquipmentItemViewModel> EquipmentItems { get; } = new();

    /// <summary>
    /// Gets the collection of consumable items.
    /// </summary>
    public ObservableCollection<ConsumableItemViewModel> ConsumableItems { get; } = new();

    /// <summary>
    /// Gets the collection of equipment slots.
    /// </summary>
    public ObservableCollection<EquipmentSlotViewModel> EquipmentSlots { get; } = new();

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public InventoryItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
            UpdateComparison();
        }
    }

    /// <summary>
    /// Gets or sets the currently hovered item (for tooltip/comparison).
    /// </summary>
    public InventoryItemViewModel? HoveredItem
    {
        get => _hoveredItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _hoveredItem, value);
            UpdateComparison();
        }
    }

    /// <summary>
    /// Gets the current equipment comparison (when hovering over equipment).
    /// </summary>
    public EquipmentComparison? CurrentComparison
    {
        get => _currentComparison;
        private set => this.RaiseAndSetIfChanged(ref _currentComparison, value);
    }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    /// <summary>
    /// Gets the inventory count display string.
    /// </summary>
    public string InventoryCountDisplay => $"{EquipmentItems.Count} / {Character?.MaxInventorySize ?? 5}";

    /// <summary>
    /// Gets the consumables count display string.
    /// </summary>
    public string ConsumablesCountDisplay => $"{ConsumableItems.Count} / {Character?.MaxConsumables ?? 10}";

    /// <summary>
    /// Gets whether the inventory is full.
    /// </summary>
    public bool IsInventoryFull => EquipmentItems.Count >= (Character?.MaxInventorySize ?? 5);

    #region v0.44.5: Loot Collection Properties

    /// <summary>
    /// Gets whether the view is in loot collection mode.
    /// </summary>
    public bool IsInLootCollectionMode
    {
        get => _isInLootCollectionMode;
        private set => this.RaiseAndSetIfChanged(ref _isInLootCollectionMode, value);
    }

    /// <summary>
    /// Gets the pending loot items from combat.
    /// </summary>
    public ObservableCollection<EquipmentItemViewModel> PendingLootItems { get; } = new();

    /// <summary>
    /// Gets the loot summary text.
    /// </summary>
    public string LootSummary => _lootController?.GetLootSummary() ?? string.Empty;

    /// <summary>
    /// Gets whether there are pending loot items.
    /// </summary>
    public bool HasPendingLoot => _lootController?.PendingLoot.Count > 0;

    #endregion

    #region Commands

    /// <summary>
    /// Command to equip the selected item.
    /// </summary>
    public ICommand EquipItemCommand { get; }

    /// <summary>
    /// Command to unequip from a slot.
    /// </summary>
    public ICommand UnequipSlotCommand { get; }

    /// <summary>
    /// Command to drop an item.
    /// </summary>
    public ICommand DropItemCommand { get; }

    /// <summary>
    /// Command to use a consumable.
    /// </summary>
    public ICommand UseConsumableCommand { get; }

    /// <summary>
    /// Command to sort inventory by type.
    /// </summary>
    public ICommand SortByTypeCommand { get; }

    /// <summary>
    /// Command to sort inventory by quality.
    /// </summary>
    public ICommand SortByQualityCommand { get; }

    /// <summary>
    /// Command to sort inventory by name.
    /// </summary>
    public ICommand SortByNameCommand { get; }

    /// <summary>
    /// v0.44.5: Command to collect a loot item.
    /// </summary>
    public ICommand CollectLootItemCommand { get; }

    /// <summary>
    /// v0.44.5: Command to collect all loot items.
    /// </summary>
    public ICommand CollectAllLootCommand { get; }

    /// <summary>
    /// v0.44.5: Command to complete loot collection.
    /// </summary>
    public ICommand CompleteLootCollectionCommand { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of InventoryViewModel.
    /// v0.44.5: Now accepts optional GameStateController and LootController for game integration.
    /// </summary>
    public InventoryViewModel(
        EquipmentService equipmentService,
        GameStateController? gameStateController = null,
        LootController? lootController = null)
    {
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        _gameStateController = gameStateController;
        _lootController = lootController;

        // Initialize commands
        EquipItemCommand = ReactiveCommand.Create<EquipmentItemViewModel>(EquipItem);
        UnequipSlotCommand = ReactiveCommand.Create<EquipmentSlotViewModel>(UnequipSlot);
        DropItemCommand = ReactiveCommand.Create<InventoryItemViewModel>(DropItem);
        UseConsumableCommand = ReactiveCommand.Create<ConsumableItemViewModel>(UseConsumable);
        SortByTypeCommand = ReactiveCommand.Create(() => SortInventory(SortMode.Type));
        SortByQualityCommand = ReactiveCommand.Create(() => SortInventory(SortMode.Quality));
        SortByNameCommand = ReactiveCommand.Create(() => SortInventory(SortMode.Name));

        // v0.44.5: Loot collection commands
        CollectLootItemCommand = ReactiveCommand.Create<EquipmentItemViewModel>(CollectLootItem);
        CollectAllLootCommand = ReactiveCommand.Create(CollectAllLoot);
        CompleteLootCollectionCommand = ReactiveCommand.CreateFromTask(CompleteLootCollectionAsync);

        // Initialize equipment slots
        InitializeEquipmentSlots();

        // Load character from game state or use demo
        if (_gameStateController?.HasActiveGame == true)
        {
            Character = _gameStateController.CurrentGameState.Player;
            CheckLootCollectionMode();
        }
        else
        {
            // Load demo character for testing
            Character = CreateDemoCharacter();
        }
    }

    /// <summary>
    /// v0.44.5: Checks if we're in loot collection mode and loads pending loot.
    /// </summary>
    private void CheckLootCollectionMode()
    {
        if (_gameStateController?.CurrentGameState.CurrentPhase == Core.GamePhase.LootCollection && _lootController != null)
        {
            IsInLootCollectionMode = true;
            RefreshPendingLoot();
        }
        else
        {
            IsInLootCollectionMode = false;
        }
    }

    /// <summary>
    /// v0.44.5: Refreshes the pending loot list from LootController.
    /// </summary>
    private void RefreshPendingLoot()
    {
        PendingLootItems.Clear();

        if (_lootController?.PendingLoot != null)
        {
            foreach (var item in _lootController.PendingLoot)
            {
                PendingLootItems.Add(new EquipmentItemViewModel(item, _equipmentService));
            }
        }

        this.RaisePropertyChanged(nameof(LootSummary));
        this.RaisePropertyChanged(nameof(HasPendingLoot));
    }

    /// <summary>
    /// v0.44.5: Collects a single loot item.
    /// </summary>
    private void CollectLootItem(EquipmentItemViewModel? itemVM)
    {
        if (itemVM?.Equipment == null || _lootController == null) return;

        if (_lootController.CollectItem(itemVM.Equipment))
        {
            StatusMessage = $"Collected {itemVM.Name}";
            RefreshPendingLoot();
            RefreshInventory();
        }
        else
        {
            StatusMessage = "Inventory full - cannot collect";
        }
    }

    /// <summary>
    /// v0.44.5: Collects all pending loot items.
    /// </summary>
    private void CollectAllLoot()
    {
        if (_lootController == null) return;

        int collected = _lootController.CollectAllItems();
        StatusMessage = $"Collected {collected} items";

        RefreshPendingLoot();
        RefreshInventory();
    }

    /// <summary>
    /// v0.44.5: Completes loot collection and proceeds.
    /// </summary>
    private async Task CompleteLootCollectionAsync()
    {
        if (_lootController == null) return;

        await _lootController.CompleteLootCollectionAsync();
        IsInLootCollectionMode = false;
    }

    /// <summary>
    /// Initializes the equipment slot view models.
    /// </summary>
    private void InitializeEquipmentSlots()
    {
        EquipmentSlots.Add(new EquipmentSlotViewModel
        {
            SlotType = EquipmentType.Weapon,
            DisplayName = "Weapon",
            SlotIcon = "W"
        });

        EquipmentSlots.Add(new EquipmentSlotViewModel
        {
            SlotType = EquipmentType.Armor,
            DisplayName = "Armor",
            SlotIcon = "A"
        });
    }

    /// <summary>
    /// Refreshes the inventory display from the character data.
    /// </summary>
    private void RefreshInventory()
    {
        EquipmentItems.Clear();
        ConsumableItems.Clear();

        if (Character == null) return;

        // Load equipment items
        foreach (var equipment in Character.Inventory)
        {
            EquipmentItems.Add(new EquipmentItemViewModel(equipment, _equipmentService));
        }

        // Load consumables
        foreach (var consumable in Character.Consumables)
        {
            ConsumableItems.Add(new ConsumableItemViewModel(consumable));
        }

        // Update equipment slots
        UpdateEquipmentSlots();

        // Notify count changes
        this.RaisePropertyChanged(nameof(InventoryCountDisplay));
        this.RaisePropertyChanged(nameof(ConsumablesCountDisplay));
        this.RaisePropertyChanged(nameof(IsInventoryFull));
    }

    /// <summary>
    /// Updates the equipment slot displays.
    /// </summary>
    private void UpdateEquipmentSlots()
    {
        if (Character == null) return;

        foreach (var slot in EquipmentSlots)
        {
            slot.EquippedItem = slot.SlotType switch
            {
                EquipmentType.Weapon => Character.EquippedWeapon != null
                    ? new EquipmentItemViewModel(Character.EquippedWeapon, _equipmentService)
                    : null,
                EquipmentType.Armor => Character.EquippedArmor != null
                    ? new EquipmentItemViewModel(Character.EquippedArmor, _equipmentService)
                    : null,
                _ => null
            };
        }
    }

    /// <summary>
    /// Equips an item from inventory.
    /// </summary>
    private void EquipItem(EquipmentItemViewModel? itemVM)
    {
        if (itemVM?.Equipment == null || Character == null) return;

        bool success = itemVM.Equipment.Type switch
        {
            EquipmentType.Weapon => _equipmentService.EquipWeapon(Character, itemVM.Equipment),
            EquipmentType.Armor => _equipmentService.EquipArmor(Character, itemVM.Equipment),
            _ => false
        };

        if (success)
        {
            StatusMessage = $"Equipped {itemVM.Name}";
            RefreshInventory();
        }
        else
        {
            StatusMessage = $"Cannot equip {itemVM.Name}";
        }
    }

    /// <summary>
    /// Unequips an item from a slot.
    /// </summary>
    private void UnequipSlot(EquipmentSlotViewModel? slot)
    {
        if (slot?.EquippedItem == null || Character == null) return;

        bool success = slot.SlotType switch
        {
            EquipmentType.Weapon => _equipmentService.UnequipWeapon(Character),
            EquipmentType.Armor => _equipmentService.UnequipArmor(Character),
            _ => false
        };

        if (success)
        {
            StatusMessage = $"Unequipped {slot.EquippedItem.Name}";
            RefreshInventory();
        }
        else
        {
            StatusMessage = "Inventory full - cannot unequip";
        }
    }

    /// <summary>
    /// Drops an item from inventory.
    /// </summary>
    private void DropItem(InventoryItemViewModel? itemVM)
    {
        if (itemVM == null || Character == null) return;

        if (itemVM is EquipmentItemViewModel equipVM)
        {
            Character.Inventory.Remove(equipVM.Equipment);
            StatusMessage = $"Dropped {equipVM.Name}";
        }
        else if (itemVM is ConsumableItemViewModel consVM)
        {
            Character.Consumables.Remove(consVM.Consumable);
            StatusMessage = $"Dropped {consVM.Name}";
        }

        RefreshInventory();
    }

    /// <summary>
    /// Uses a consumable item.
    /// </summary>
    private void UseConsumable(ConsumableItemViewModel? itemVM)
    {
        if (itemVM?.Consumable == null || Character == null) return;

        var consumable = itemVM.Consumable;

        // Apply effects
        if (consumable.HPRestore > 0)
        {
            int healed = consumable.GetTotalHPRestore();
            Character.HP = Math.Min(Character.HP + healed, Character.MaxHP);
        }

        if (consumable.StaminaRestore > 0)
        {
            int restored = consumable.GetTotalStaminaRestore();
            Character.Stamina = Math.Min(Character.Stamina + restored, Character.MaxStamina);
        }

        if (consumable.StressRestore > 0)
        {
            Character.PsychicStress = Math.Max(0, Character.PsychicStress - consumable.StressRestore);
        }

        if (consumable.TempHPGrant > 0)
        {
            Character.TempHP += consumable.TempHPGrant;
        }

        // Remove from inventory
        Character.Consumables.Remove(consumable);
        StatusMessage = $"Used {consumable.Name}";
        RefreshInventory();
    }

    /// <summary>
    /// Sorts the inventory by the specified mode.
    /// </summary>
    private void SortInventory(SortMode mode)
    {
        if (Character == null) return;

        var sorted = mode switch
        {
            SortMode.Type => Character.Inventory.OrderBy(e => e.Type).ThenBy(e => e.Name).ToList(),
            SortMode.Quality => Character.Inventory.OrderByDescending(e => e.Quality).ThenBy(e => e.Name).ToList(),
            SortMode.Name => Character.Inventory.OrderBy(e => e.Name).ToList(),
            _ => Character.Inventory.ToList()
        };

        Character.Inventory.Clear();
        foreach (var item in sorted)
        {
            Character.Inventory.Add(item);
        }

        StatusMessage = $"Sorted by {mode}";
        RefreshInventory();
    }

    /// <summary>
    /// Updates the equipment comparison when hovering.
    /// </summary>
    private void UpdateComparison()
    {
        if (Character == null || HoveredItem is not EquipmentItemViewModel equipVM)
        {
            CurrentComparison = null;
            return;
        }

        var equipment = equipVM.Equipment;
        Equipment? currentEquipped = equipment.Type switch
        {
            EquipmentType.Weapon => Character.EquippedWeapon,
            EquipmentType.Armor => Character.EquippedArmor,
            _ => null
        };

        CurrentComparison = _equipmentService.CompareEquipment(currentEquipped, equipment);
    }

    /// <summary>
    /// Creates a demo character with sample inventory.
    /// </summary>
    private static PlayerCharacter CreateDemoCharacter()
    {
        var character = new PlayerCharacter
        {
            Name = "Ragnar the Bold",
            Class = CharacterClass.Warrior,
            HP = 45,
            MaxHP = 60,
            Stamina = 30,
            MaxStamina = 40,
            MaxInventorySize = 5,
            MaxConsumables = 10
        };

        // Equipped items
        character.EquippedWeapon = new Equipment
        {
            Name = "Rusted Greataxe",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.Scavenged,
            WeaponCategory = WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 1,
            StaminaCost = 8,
            Description = "A heavy axe covered in rust but still deadly."
        };

        character.EquippedArmor = new Equipment
        {
            Name = "Salvaged Plate",
            Type = EquipmentType.Armor,
            Quality = QualityTier.Scavenged,
            ArmorCategory = ArmorCategory.Heavy,
            HPBonus = 15,
            DefenseBonus = 3,
            Description = "Pieced together from pre-Glitch military armor."
        };

        // Inventory items
        character.Inventory.Add(new Equipment
        {
            Name = "Iron Shortsword",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.ClanForged,
            WeaponCategory = WeaponCategory.Blade,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 2,
            StaminaCost = 5,
            AccuracyBonus = 1,
            Description = "A well-crafted blade from the Northern Clans."
        });

        character.Inventory.Add(new Equipment
        {
            Name = "Leather Jerkin",
            Type = EquipmentType.Armor,
            Quality = QualityTier.Scavenged,
            ArmorCategory = ArmorCategory.Light,
            HPBonus = 5,
            DefenseBonus = 1,
            Description = "Light armor allowing quick movement.",
            Bonuses = new System.Collections.Generic.List<EquipmentBonus>
            {
                new() { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE" }
            }
        });

        character.Inventory.Add(new Equipment
        {
            Name = "Myth-Forged Dagger",
            Type = EquipmentType.Weapon,
            Quality = QualityTier.MythForged,
            WeaponCategory = WeaponCategory.Dagger,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 3,
            StaminaCost = 3,
            AccuracyBonus = 2,
            IgnoresArmor = true,
            SpecialEffect = "Ignores armor on critical hits",
            Description = "A legendary blade that phases through defenses."
        });

        // Consumables
        character.Consumables.Add(new Consumable
        {
            Name = "Healing Salve",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 20,
            Description = "A basic healing potion."
        });

        character.Consumables.Add(new Consumable
        {
            Name = "Masterwork Tonic",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Masterwork,
            HPRestore = 25,
            MasterworkBonusHP = 10,
            StaminaRestore = 15,
            MasterworkBonusStamina = 5,
            Description = "A potent healing mixture crafted by a master."
        });

        character.Consumables.Add(new Consumable
        {
            Name = "Antidote",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            ClearsPoison = true,
            Description = "Cures poison effects."
        });

        return character;
    }

    private enum SortMode
    {
        Type,
        Quality,
        Name
    }
}

/// <summary>
/// Base class for inventory item view models.
/// </summary>
public abstract class InventoryItemViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the item name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the item description.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the item type display string.
    /// </summary>
    public abstract string TypeDisplay { get; }

    /// <summary>
    /// Gets the quality color for display.
    /// </summary>
    public abstract string QualityColor { get; }
}

/// <summary>
/// View model for equipment items in inventory.
/// </summary>
public class EquipmentItemViewModel : InventoryItemViewModel
{
    private readonly EquipmentService _equipmentService;

    /// <summary>
    /// Gets the underlying equipment.
    /// </summary>
    public Equipment Equipment { get; }

    public override string Name => Equipment.GetDisplayName();
    public override string Description => Equipment.Description;
    public override string TypeDisplay => Equipment.Type.ToString();

    /// <summary>
    /// Gets the quality tier name.
    /// </summary>
    public string QualityName => Equipment.GetQualityName();

    public override string QualityColor => Equipment.Quality switch
    {
        QualityTier.JuryRigged => "#808080",   // Gray
        QualityTier.Scavenged => "#FFFFFF",    // White
        QualityTier.ClanForged => "#4A90E2",   // Blue
        QualityTier.Optimized => "#9400D3",    // Purple
        QualityTier.MythForged => "#FFD700",   // Gold
        _ => "#FFFFFF"
    };

    /// <summary>
    /// Gets the slot icon character.
    /// </summary>
    public string SlotIcon => Equipment.Type switch
    {
        EquipmentType.Weapon => "W",
        EquipmentType.Armor => "A",
        EquipmentType.Accessory => "R",
        _ => "?"
    };

    /// <summary>
    /// Gets the damage description for weapons.
    /// </summary>
    public string DamageDisplay => Equipment.Type == EquipmentType.Weapon
        ? Equipment.GetDamageDescription()
        : "N/A";

    /// <summary>
    /// Gets the defense bonus for armor.
    /// </summary>
    public string DefenseDisplay => Equipment.Type == EquipmentType.Armor
        ? $"+{Equipment.DefenseBonus} Def, +{Equipment.HPBonus} HP"
        : "N/A";

    /// <summary>
    /// Gets the stats summary.
    /// </summary>
    public string StatsSummary => Equipment.Type switch
    {
        EquipmentType.Weapon => $"{DamageDisplay} | {Equipment.StaminaCost} SP | {(Equipment.AccuracyBonus >= 0 ? "+" : "")}{Equipment.AccuracyBonus} Acc",
        EquipmentType.Armor => $"+{Equipment.HPBonus} HP | +{Equipment.DefenseBonus} Def",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the bonuses description.
    /// </summary>
    public string BonusesDisplay => Equipment.GetBonusesDescription();

    /// <summary>
    /// Gets whether this is a weapon.
    /// </summary>
    public bool IsWeapon => Equipment.Type == EquipmentType.Weapon;

    /// <summary>
    /// Gets whether this is armor.
    /// </summary>
    public bool IsArmor => Equipment.Type == EquipmentType.Armor;

    /// <summary>
    /// Gets whether this item has special effects.
    /// </summary>
    public bool HasSpecialEffect => !string.IsNullOrEmpty(Equipment.SpecialEffect);

    public EquipmentItemViewModel(Equipment equipment, EquipmentService equipmentService)
    {
        Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
    }
}

/// <summary>
/// View model for consumable items.
/// </summary>
public class ConsumableItemViewModel : InventoryItemViewModel
{
    /// <summary>
    /// Gets the underlying consumable.
    /// </summary>
    public Consumable Consumable { get; }

    public override string Name => Consumable.GetDisplayName();
    public override string Description => Consumable.Description;
    public override string TypeDisplay => Consumable.Type.ToString();

    public override string QualityColor => Consumable.Quality switch
    {
        CraftQuality.Standard => "#FFFFFF",    // White
        CraftQuality.Masterwork => "#FFD700",  // Gold
        _ => "#FFFFFF"
    };

    /// <summary>
    /// Gets whether this is a masterwork item.
    /// </summary>
    public bool IsMasterwork => Consumable.Quality == CraftQuality.Masterwork;

    /// <summary>
    /// Gets the effects description.
    /// </summary>
    public string EffectsDisplay => Consumable.GetEffectsDescription();

    /// <summary>
    /// Gets the HP restore amount.
    /// </summary>
    public int HPRestore => Consumable.GetTotalHPRestore();

    /// <summary>
    /// Gets the stamina restore amount.
    /// </summary>
    public int StaminaRestore => Consumable.GetTotalStaminaRestore();

    public ConsumableItemViewModel(Consumable consumable)
    {
        Consumable = consumable ?? throw new ArgumentNullException(nameof(consumable));
    }
}

/// <summary>
/// View model for equipment slots.
/// </summary>
public class EquipmentSlotViewModel : ViewModelBase
{
    private EquipmentItemViewModel? _equippedItem;

    /// <summary>
    /// Gets or sets the slot type.
    /// </summary>
    public EquipmentType SlotType { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slot icon character.
    /// </summary>
    public string SlotIcon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the equipped item (null if empty).
    /// </summary>
    public EquipmentItemViewModel? EquippedItem
    {
        get => _equippedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _equippedItem, value);
            this.RaisePropertyChanged(nameof(IsEmpty));
            this.RaisePropertyChanged(nameof(EquippedItemName));
            this.RaisePropertyChanged(nameof(EquippedItemStats));
        }
    }

    /// <summary>
    /// Gets whether the slot is empty.
    /// </summary>
    public bool IsEmpty => EquippedItem == null;

    /// <summary>
    /// Gets the equipped item name or "Empty".
    /// </summary>
    public string EquippedItemName => EquippedItem?.Name ?? "Empty";

    /// <summary>
    /// Gets the equipped item stats summary.
    /// </summary>
    public string EquippedItemStats => EquippedItem?.StatsSummary ?? "Drag item here to equip";
}
