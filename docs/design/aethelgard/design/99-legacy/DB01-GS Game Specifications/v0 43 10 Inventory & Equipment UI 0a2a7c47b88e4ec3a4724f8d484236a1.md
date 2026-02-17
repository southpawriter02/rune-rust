# v0.43.10: Inventory & Equipment UI

Type: UI
Description: Inventory and equipment UI: inventory grid display, equipment slot visualization, drag-and-drop equipping, item tooltips with full stats, stat comparison (equipped vs hover), item sorting and filtering. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.15 (Equipment System)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.15 (Equipment System)

**Estimated Time:** 6-8 hours

**Group:** Character Management

**Deliverable:** Inventory management and equipment interface

---

## Executive Summary

v0.43.10 implements the inventory and equipment UI, allowing players to manage items, equip/unequip gear, compare stats, and organize their inventory through drag-and-drop.

**What This Delivers:**

- Inventory grid display
- Equipment slot visualization
- Drag-and-drop equipping
- Item tooltips with full stats
- Stat comparison (equipped vs hover)
- Item sorting and filtering
- Integration with v0.15 equipment system

**Success Metric:** Can manage full inventory and equipment with intuitive drag-and-drop interface.

---

## Service Implementation

### InventoryViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Items;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class InventoryViewModel : ViewModelBase
{
    private readonly IEquipmentService _equipmentService;
    private readonly IItemService _itemService;
    private PlayerCharacter? _character;
    private ItemViewModel? _selectedItem;
    private ItemViewModel? _hoveredItem;
    
    public ObservableCollection<ItemViewModel> InventoryItems { get; } = new();
    public ObservableCollection<EquipmentSlotViewModel> EquipmentSlots { get; } = new();
    
    public ItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }
    
    public ItemViewModel? HoveredItem
    {
        get => _hoveredItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _hoveredItem, value);
            UpdateStatComparison();
        }
    }
    
    // Commands
    public ICommand EquipItemCommand { get; }
    public ICommand UnequipItemCommand { get; }
    public ICommand DropItemCommand { get; }
    public ICommand SortInventoryCommand { get; }
    
    public InventoryViewModel(
        IEquipmentService equipmentService,
        IItemService itemService)
    {
        _equipmentService = equipmentService;
        _itemService = itemService;
        
        EquipItemCommand = ReactiveCommand.Create<ItemViewModel>(EquipItem);
        UnequipItemCommand = ReactiveCommand.Create<EquipmentSlotViewModel>(UnequipSlot);
        DropItemCommand = ReactiveCommand.Create<ItemViewModel>(DropItem);
        SortInventoryCommand = ReactiveCommand.Create(SortInventory);
        
        InitializeEquipmentSlots();
    }
    
    public void LoadCharacter(PlayerCharacter character)
    {
        _character = character;
        LoadInventory();
        LoadEquipment();
    }
    
    private void LoadInventory()
    {
        InventoryItems.Clear();
        foreach (var item in _character!.Inventory)
        {
            InventoryItems.Add(new ItemViewModel(item));
        }
    }
    
    private void LoadEquipment()
    {
        // Update equipped items in slots
        var weapon = _character!.EquippedWeapon;
        var armor = _character.EquippedArmor;
        var accessory = _character.EquippedAccessory;
        
        UpdateSlot(EquipmentSlotType.Weapon, weapon);
        UpdateSlot(EquipmentSlotType.Armor, armor);
        UpdateSlot(EquipmentSlotType.Accessory, accessory);
    }
    
    private void InitializeEquipmentSlots()
    {
        EquipmentSlots.Add(new EquipmentSlotViewModel
        {
            SlotType = EquipmentSlotType.Weapon,
            DisplayName = "Weapon",
            IconName = "slot_weapon"
        });
        
        EquipmentSlots.Add(new EquipmentSlotViewModel
        {
            SlotType = EquipmentSlotType.Armor,
            DisplayName = "Armor",
            IconName = "slot_armor"
        });
        
        EquipmentSlots.Add(new EquipmentSlotViewModel
        {
            SlotType = EquipmentSlotType.Accessory,
            DisplayName = "Accessory",
            IconName = "slot_accessory"
        });
    }
    
    private void EquipItem(ItemViewModel itemVM)
    {
        if (itemVM.Item is not Equipment equipment) return;
        
        var result = _equipmentService.EquipItem(_character!, equipment);
        if (result.Success)
        {
            LoadInventory();
            LoadEquipment();
        }
    }
    
    private void UnequipSlot(EquipmentSlotViewModel slot)
    {
        if (slot.EquippedItem == null) return;
        
        var result = _equipmentService.UnequipItem(_character!, slot.SlotType);
        if (result.Success)
        {
            LoadInventory();
            LoadEquipment();
        }
    }
    
    private void DropItem(ItemViewModel itemVM)
    {
        _character!.Inventory.Remove(itemVM.Item);
        LoadInventory();
    }
    
    private void SortInventory()
    {
        var sorted = _character!.Inventory
            .OrderBy(i => i.ItemType)
            .ThenBy(i => [i.Name](http://i.Name))
            .ToList();
        
        _character.Inventory.Clear();
        foreach (var item in sorted)
        {
            _character.Inventory.Add(item);
        }
        
        LoadInventory();
    }
    
    private void UpdateSlot(EquipmentSlotType slotType, Equipment? item)
    {
        var slot = EquipmentSlots.FirstOrDefault(s => s.SlotType == slotType);
        if (slot != null)
        {
            slot.EquippedItem = item != null ? new ItemViewModel(item) : null;
        }
    }
    
    private void UpdateStatComparison()
    {
        // Compare hovered item with equipped item in same slot
        // Update UI to show stat differences
    }
}

public class ItemViewModel : ViewModelBase
{
    public Item Item { get; }
    
    public string Name => [Item.Name](http://Item.Name);
    public string Description => Item.Description;
    public string SpriteName => $"item_{Item.ItemType.ToString().ToLower()}_{[Item.Id](http://Item.Id)}";
    public bool IsEquipment => Item is Equipment;
    
    public ItemViewModel(Item item)
    {
        Item = item;
    }
}

public class EquipmentSlotViewModel : ViewModelBase
{
    private ItemViewModel? _equippedItem;
    
    public EquipmentSlotType SlotType { get; set; }
    public string DisplayName { get; set; } = "";
    public string IconName { get; set; } = "";
    
    public ItemViewModel? EquippedItem
    {
        get => _equippedItem;
        set => this.RaiseAndSetIfChanged(ref _equippedItem, value);
    }
    
    public bool IsEmpty => EquippedItem == null;
}
```

---

## InventoryView XAML

```xml
<UserControl xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.InventoryView"
             x:DataType="vm:InventoryViewModel">
    
    <Grid ColumnDefinitions="300,*" Margin="20">
        <!-- Equipment Slots -->
        <Border Grid.Column="0" 
                Background="#2C2C2C" 
                Padding="15" 
                Margin="0,0,10,0"
                CornerRadius="5">
            <StackPanel Spacing="15">
                <TextBlock Text="Equipment"
                           FontSize="20"
                           FontWeight="Bold"
                           Foreground="White"/>
                
                <ItemsControl ItemsSource="{Binding EquipmentSlots}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="#1C1C1C"
                                    BorderBrush="#4A4A4A"
                                    BorderThickness="2"
                                    Padding="10"
                                    Margin="0,5"
                                    CornerRadius="5"
                                    Height="80"
                                    AllowDrop="True"
                                    DragOver="OnSlotDragOver"
                                    Drop="OnSlotDrop">
                                <Grid ColumnDefinitions="60,*">
                                    <!-- Slot Icon -->
                                    <Border Grid.Column="0"
                                            Background="#3C3C3C"
                                            Width="60"
                                            Height="60"
                                            CornerRadius="5">
                                        <TextBlock Text="{Binding DisplayName, Converter={StaticResource FirstCharConverter}}"
                                                   FontSize="32"
                                                   HorizontalAlignment="Center"
                                                   VerticalAlignment="Center"
                                                   Foreground="#888888"/>
                                    </Border>
                                    
                                    <!-- Equipped Item or Empty -->
                                    <StackPanel Grid.Column="1" Margin="10,0,0,0">
                                        <TextBlock Text="{Binding DisplayName}"
                                                   FontWeight="Bold"
                                                   Foreground="#CCCCCC"/>
                                        
                                        <TextBlock Text="{Binding [EquippedItem.Name](http://EquippedItem.Name)}"
                                                   Foreground="White"
                                                   Margin="0,5,0,0"
                                                   IsVisible="{Binding !IsEmpty}"/>
                                        
                                        <TextBlock Text="Empty"
                                                   Foreground="#666666"
                                                   FontStyle="Italic"
                                                   IsVisible="{Binding IsEmpty}"/>
                                        
                                        <Button Content="Unequip"
                                                Command="{Binding $parent[UserControl].DataContext.UnequipItemCommand}"
                                                CommandParameter="{Binding}"
                                                IsVisible="{Binding !IsEmpty}"
                                                Margin="0,5,0,0"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </Border>
        
        <!-- Inventory Grid -->
        <Border Grid.Column="1" 
                Background="#2C2C2C" 
                Padding="15"
                CornerRadius="5">
            <StackPanel>
                <!-- Header -->
                <Grid ColumnDefinitions="*,Auto" Margin="0,0,0,10">
                    <TextBlock Text="Inventory"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <Button Grid.Column="1"
                            Content="Sort"
                            Command="{Binding SortInventoryCommand}"/>
                </Grid>
                
                <!-- Items Grid -->
                <ScrollViewer Height="500">
                    <ItemsControl ItemsSource="{Binding InventoryItems}">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <WrapPanel/>
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="#1C1C1C"
                                        BorderBrush="#4A4A4A"
                                        BorderThickness="2"
                                        Width="70"
                                        Height="70"
                                        Margin="5"
                                        CornerRadius="5"
                                        Cursor="Hand"
                                        DragStarted="OnItemDragStarted"
                                        PointerEntered="OnItemPointerEntered"
                                        PointerExited="OnItemPointerExited"
                                        Tapped="OnItemTapped">
                                    
                                    <Grid>
                                        <!-- Item Icon (from sprite) -->
                                        <Image Source="{Binding SpriteName, Converter={StaticResource SpriteToImageConverter}}"
                                               Stretch="Uniform"
                                               Margin="5"/>
                                        
                                        <!-- Tooltip -->
                                        <ToolTip.Tip>
                                            <StackPanel MaxWidth="300">
                                                <TextBlock Text="{Binding Name}"
                                                           FontWeight="Bold"
                                                           FontSize="14"/>
                                                <TextBlock Text="{Binding Description}"
                                                           TextWrapping="Wrap"
                                                           Margin="0,5"/>
                                                <!-- Equipment stats would go here -->
                                            </StackPanel>
                                        </ToolTip.Tip>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.15 (Equipment System):**

- Calls `EquipmentService.EquipItem()`
- Uses equipment stat bonuses
- Validates equipment slots

**With v0.43.2 (Sprite System):**

- Loads item icons from sprites
- Equipment slot icons

---

## Success Criteria

**v0.43.10 is DONE when:**

### ✅ Inventory Display

- [ ]  Grid shows all items
- [ ]  Item icons from sprites
- [ ]  Tooltips with full info
- [ ]  Sorting works

### ✅ Equipment Slots

- [ ]  3 slots visible (Weapon, Armor, Accessory)
- [ ]  Shows equipped items
- [ ]  Empty slots indicated
- [ ]  Unequip button works

### ✅ Drag-and-Drop

- [ ]  Can drag items from inventory
- [ ]  Drop on equipment slots to equip
- [ ]  Visual feedback during drag
- [ ]  Invalid drops rejected

### ✅ Stat Comparison

- [ ]  Hover shows stat differences
- [ ]  Green/red for better/worse
- [ ]  Clear comparison display

---

**Inventory complete. Ready for specializations in v0.43.11.**