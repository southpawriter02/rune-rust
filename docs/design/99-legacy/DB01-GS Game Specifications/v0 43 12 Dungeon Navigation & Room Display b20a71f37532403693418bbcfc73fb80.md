# v0.43.12: Dungeon Navigation & Room Display

Type: UI
Description: Dungeon exploration UI: DungeonExplorationView layout, room description display, exit indicators (N/S/E/W/U/D), room action buttons (Search, Rest, Move), biome-specific backgrounds. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.4 (Dungeon Generation), v0.5 (Vertical Progression)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.4 (Dungeon Generation), v0.5 (Vertical Progression)

**Estimated Time:** 6-8 hours

**Group:** Exploration

**Deliverable:** Dungeon exploration interface with room display

---

## Executive Summary

v0.43.12 implements the dungeon exploration UI, displaying room descriptions, available exits, features, and actions. Integrates with v0.4 dungeon generation and provides the main exploration loop interface.

**What This Delivers:**

- DungeonExplorationView layout
- Room description display
- Exit indicators (N/S/E/W/U/D)
- Room action buttons (Search, Rest, Move)
- Biome-specific backgrounds
- Room feature visualization
- Integration with v0.4-v0.5 systems

**Success Metric:** Can navigate dungeon, see all room information, and perform all exploration actions.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### DungeonExplorationViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Dungeon;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class DungeonExplorationViewModel : ViewModelBase
{
    private readonly IDungeonGenerator _dungeonGenerator;
    private readonly INavigationService _navigationService;
    private DungeonState? _currentDungeon;
    private Room? _currentRoom;
    
    public Room? CurrentRoom
    {
        get => _currentRoom;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentRoom, value);
            UpdateRoomDisplay();
        }
    }
    
    public string RoomDescription => CurrentRoom?.Description ?? "";
    public string BiomeName => CurrentRoom?.[Biome.Name](http://Biome.Name) ?? "";
    public ObservableCollection<ExitViewModel> AvailableExits { get; } = new();
    public ObservableCollection<string> RoomFeatures { get; } = new();
    
    public bool CanMoveNorth => CurrentRoom?.HasExit(Direction.North) ?? false;
    public bool CanMoveSouth => CurrentRoom?.HasExit(Direction.South) ?? false;
    public bool CanMoveEast => CurrentRoom?.HasExit(Direction.East) ?? false;
    public bool CanMoveWest => CurrentRoom?.HasExit(Direction.West) ?? false;
    public bool CanMoveUp => CurrentRoom?.HasExit(Direction.Up) ?? false;
    public bool CanMoveDown => CurrentRoom?.HasExit(Direction.Down) ?? false;
    
    // Commands
    public ICommand MoveCommand { get; }
    public ICommand SearchRoomCommand { get; }
    public ICommand RestCommand { get; }
    public ICommand ViewCharacterCommand { get; }
    public ICommand ViewInventoryCommand { get; }
    
    public DungeonExplorationViewModel(
        IDungeonGenerator dungeonGenerator,
        INavigationService navigationService)
    {
        _dungeonGenerator = dungeonGenerator;
        _navigationService = navigationService;
        
        MoveCommand = ReactiveCommand.CreateFromTask<Direction>(MoveAsync);
        SearchRoomCommand = ReactiveCommand.CreateFromTask(SearchRoomAsync);
        RestCommand = ReactiveCommand.CreateFromTask(RestAsync);
        ViewCharacterCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<CharacterSheetViewModel>());
        ViewInventoryCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<InventoryViewModel>());
    }
    
    public void LoadDungeon(DungeonState dungeon, Room startingRoom)
    {
        _currentDungeon = dungeon;
        CurrentRoom = startingRoom;
    }
    
    private async Task MoveAsync(Direction direction)
    {
        if (CurrentRoom == null || !CurrentRoom.HasExit(direction))
            return;
        
        var nextRoom = _currentDungeon!.GetAdjacentRoom([CurrentRoom.Id](http://CurrentRoom.Id), direction);
        if (nextRoom != null)
        {
            CurrentRoom = nextRoom;
            
            // Check for random encounters
            if (ShouldTriggerEncounter())
            {
                await TriggerCombatEncounterAsync();
            }
        }
    }
    
    private async Task SearchRoomAsync()
    {
        if (CurrentRoom == null || CurrentRoom.IsSearched)
            return;
        
        var result = await _dungeonGenerator.SearchRoomAsync(CurrentRoom);
        
        if (result.FoundLoot)
        {
            // Show loot dialog
        }
        
        if (result.TriggeredEncounter)
        {
            await TriggerCombatEncounterAsync();
        }
        
        CurrentRoom.IsSearched = true;
        UpdateRoomDisplay();
    }
    
    private async Task RestAsync()
    {
        // Restore HP/Stamina but risk Psychic Stress
        var confirm = await ShowConfirmDialog(
            "Rest",
            "Rest to recover HP and Stamina? May increase Psychic Stress.");
        
        if (confirm)
        {
            // Apply rest effects
        }
    }
    
    private async Task TriggerCombatEncounterAsync()
    {
        // Generate enemies and start combat
        _navigationService.NavigateTo<CombatViewModel>();
    }
    
    private void UpdateRoomDisplay()
    {
        AvailableExits.Clear();
        RoomFeatures.Clear();
        
        if (CurrentRoom == null) return;
        
        // Update exits
        foreach (var direction in Enum.GetValues<Direction>())
        {
            if (CurrentRoom.HasExit(direction))
            {
                AvailableExits.Add(new ExitViewModel
                {
                    Direction = direction,
                    DisplayName = GetDirectionDisplayName(direction)
                });
            }
        }
        
        // Update features
        foreach (var feature in CurrentRoom.Features)
        {
            RoomFeatures.Add(feature.Description);
        }
        
        this.RaisePropertyChanged(nameof(RoomDescription));
        this.RaisePropertyChanged(nameof(BiomeName));
        this.RaisePropertyChanged(nameof(CanMoveNorth));
        this.RaisePropertyChanged(nameof(CanMoveSouth));
        this.RaisePropertyChanged(nameof(CanMoveEast));
        this.RaisePropertyChanged(nameof(CanMoveWest));
        this.RaisePropertyChanged(nameof(CanMoveUp));
        this.RaisePropertyChanged(nameof(CanMoveDown));
    }
    
    private string GetDirectionDisplayName(Direction direction)
    {
        return direction switch
        {
            Direction.North => "⬆️ North",
            Direction.South => "⬇️ South",
            Direction.East => "➡️ East",
            Direction.West => "⬅️ West",
            Direction.Up => "🔼 Up (Stairs/Ladder)",
            Direction.Down => "🔽 Down (Stairs/Shaft)",
            _ => direction.ToString()
        };
    }
    
    private bool ShouldTriggerEncounter()
    {
        // Random encounter chance logic
        return Random.Shared.NextDouble() < 0.15; // 15% chance
    }
}

public class ExitViewModel
{
    public Direction Direction { get; set; }
    public string DisplayName { get; set; } = "";
}
```

---

## DungeonExplorationView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.DungeonExplorationView"
             x:DataType="vm:DungeonExplorationViewModel">
    
    <Grid ColumnDefinitions="*,300" Margin="20">
        <!-- Main Room Display -->
        <Border Grid.Column="0" 
                Background="#2C2C2C" 
                Padding="20" 
                Margin="0,0,10,0"
                CornerRadius="5">
            <ScrollViewer>
                <StackPanel Spacing="15">
                    <!-- Biome Header -->
                    <TextBlock Text="{Binding BiomeName}"
                               FontSize="14"
                               FontWeight="Bold"
                               Foreground="#FFD700"/>
                    
                    <!-- Room Description -->
                    <TextBlock Text="{Binding RoomDescription}"
                               FontSize="16"
                               TextWrapping="Wrap"
                               Foreground="White"
                               LineHeight="24"/>
                    
                    <!-- Room Features -->
                    <Border Background="#1C1C1C" 
                            Padding="10" 
                            CornerRadius="5"
                            IsVisible="{Binding RoomFeatures.Count}">
                        <StackPanel>
                            <TextBlock Text="Features:"
                                       FontWeight="Bold"
                                       Foreground="#4A90E2"
                                       Margin="0,0,0,5"/>
                            
                            <ItemsControl ItemsSource="{Binding RoomFeatures}">
                                <ItemsControl.ItemTemplate>
                                    <DataTemplate>
                                        <TextBlock Text="{Binding}"
                                                   Foreground="#CCCCCC"
                                                   Margin="10,2"/>
                                    </DataTemplate>
                                </ItemsControl.ItemTemplate>
                            </ItemsControl>
                        </StackPanel>
                    </Border>
                    
                    <!-- Available Exits -->
                    <Border Background="#1C1C1C" 
                            Padding="10" 
                            CornerRadius="5">
                        <StackPanel>
                            <TextBlock Text="Exits:"
                                       FontWeight="Bold"
                                       Foreground="#4A90E2"
                                       Margin="0,0,0,10"/>
                            
                            <ItemsControl ItemsSource="{Binding AvailableExits}">
                                <ItemsControl.ItemTemplate>
                                    <DataTemplate>
                                        <Button Content="{Binding DisplayName}"
                                                Command="{Binding $parent[UserControl].DataContext.MoveCommand}"
                                                CommandParameter="{Binding Direction}"
                                                Margin="0,2"
                                                HorizontalAlignment="Left"/>
                                    </DataTemplate>
                                </ItemsControl.ItemTemplate>
                            </ItemsControl>
                        </StackPanel>
                    </Border>
                </StackPanel>
            </ScrollViewer>
        </Border>
        
        <!-- Actions Sidebar -->
        <Border Grid.Column="1" 
                Background="#2C2C2C" 
                Padding="15"
                CornerRadius="5">
            <StackPanel Spacing="10">
                <TextBlock Text="Actions"
                           FontSize="18"
                           FontWeight="Bold"
                           Foreground="White"
                           Margin="0,0,0,10"/>
                
                <!-- Search Room -->
                <Button Content="🔍 Search Room"
                        Command="{Binding SearchRoomCommand}"
                        HotKey="S"
                        Height="50"/>
                
                <!-- Rest -->
                <Button Content="😴 Rest"
                        Command="{Binding RestCommand}"
                        HotKey="R"
                        Height="50"/>
                
                <Separator Background="#3C3C3C" Margin="0,10"/>
                
                <!-- Character Sheet -->
                <Button Content="📊 Character"
                        Command="{Binding ViewCharacterCommand}"
                        HotKey="C"
                        Height="40"/>
                
                <!-- Inventory -->
                <Button Content="🎒 Inventory"
                        Command="{Binding ViewInventoryCommand}"
                        HotKey="I"
                        Height="40"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.4 (Dungeon Generation):**

- Displays generated rooms
- Shows biome information
- Uses room features and descriptions

**With v0.5 (Vertical Progression):**

- Handles Up/Down exits
- Shows vertical layer information

**With v0.43.5 (Combat):**

- Triggers combat encounters
- Navigates to combat view

---

## Success Criteria

**v0.43.12 is DONE when:**

### ✅ Room Display

- [ ]  Description shows clearly
- [ ]  Biome name visible
- [ ]  Features listed
- [ ]  Atmospheric text

### ✅ Navigation

- [ ]  All 6 directions supported
- [ ]  Exit buttons functional
- [ ]  Keyboard shortcuts work
- [ ]  Room transitions smooth

### ✅ Actions

- [ ]  Search room works
- [ ]  Rest functionality
- [ ]  Quick access to character/inventory
- [ ]  Random encounters trigger

---

**Dungeon navigation complete. Ready for minimap in v0.43.13.**