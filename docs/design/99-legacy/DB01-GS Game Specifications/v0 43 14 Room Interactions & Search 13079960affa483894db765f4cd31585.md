# v0.43.14: Room Interactions & Search

Type: UI
Description: Room interaction system: search action with results display, loot panel with item collection, rest/camp confirmation dialog, environmental storytelling UI, interactive room features. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.12-v0.43.13, v0.4 (Room Features)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.12-v0.43.13, v0.4 (Room Features)

**Estimated Time:** 5-7 hours

**Group:** Exploration

**Deliverable:** Room interaction UI and search results display

---

## Executive Summary

v0.43.14 implements the room interaction system, including search UI, loot display, rest/camp confirmation, and environmental storytelling elements.

**What This Delivers:**

- Search action with results display
- Loot panel with item collection
- Rest/camp confirmation dialog
- Environmental storytelling UI
- Interactive room features
- Search cooldown indicator
- Encounter warnings

**Success Metric:** All room interactions accessible and visually clear.

---

## Service Implementation

### Enhanced DungeonExplorationViewModel (Room Interactions)

```csharp
// Add to DungeonExplorationViewModel from v0.43.12

private bool _isSearching;
private SearchResultViewModel? _searchResult;

public bool IsSearching
{
    get => _isSearching;
    set => this.RaiseAndSetIfChanged(ref _isSearching, value);
}

public SearchResultViewModel? SearchResult
{
    get => _searchResult;
    set => this.RaiseAndSetIfChanged(ref _searchResult, value);
}

public bool CanSearch => CurrentRoom != null && !CurrentRoom.IsSearched && !IsSearching;

private async Task SearchRoomAsync()
{
    if (!CanSearch) return;
    
    IsSearching = true;
    
    // Simulate search delay
    await Task.Delay(500);
    
    var result = await _dungeonGenerator.SearchRoomAsync(CurrentRoom!);
    
    SearchResult = new SearchResultViewModel
    {
        FoundLoot = result.FoundLoot,
        LootItems = [result.LootItems.Select](http://result.LootItems.Select)(i => new ItemViewModel(i)).ToList(),
        DiscoveredSecrets = result.DiscoveredSecrets,
        TriggeredEncounter = result.TriggeredEncounter,
        EncounterDescription = result.EncounterDescription
    };
    
    CurrentRoom!.IsSearched = true;
    IsSearching = false;
    
    if (result.TriggeredEncounter)
    {
        // Show encounter warning, then navigate to combat
        await Task.Delay(2000); // Let player read
        await TriggerCombatEncounterAsync();
    }
}

public void CollectLoot()
{
    if (SearchResult == null || !SearchResult.FoundLoot) return;
    
    foreach (var item in SearchResult.LootItems)
    {
        _character!.Inventory.Add(item.Item);
    }
    
    SearchResult = null;
}

private async Task<bool> ShowConfirmDialog(string title, string message)
{
    return await _dialogService.ShowConfirmationAsync(title, message);
}
```

### SearchResultViewModel

```csharp
using ReactiveUI;
using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SearchResultViewModel : ViewModelBase
{
    public bool FoundLoot { get; set; }
    public List<ItemViewModel> LootItems { get; set; } = new();
    public List<string> DiscoveredSecrets { get; set; } = new();
    public bool TriggeredEncounter { get; set; }
    public string EncounterDescription { get; set; } = "";
}
```

---

## Room Interaction UI Components

### Search Result Panel (Added to DungeonExplorationView)

```xml
<!-- Add to DungeonExplorationView -->

<!-- Search Result Overlay -->
<Border IsVisible="{Binding SearchResult, Converter={StaticResource NotNullConverter}}"
        Background="#E0000000"
        Grid.ColumnSpan="2">
    <Border Background="#2C2C2C"
            BorderBrush="#FFD700"
            BorderThickness="3"
            Padding="20"
            CornerRadius="10"
            MaxWidth="500"
            VerticalAlignment="Center"
            HorizontalAlignment="Center">
        <StackPanel Spacing="15">
            <TextBlock Text="Search Results"
                       FontSize="24"
                       FontWeight="Bold"
                       Foreground="#FFD700"
                       HorizontalAlignment="Center"/>
            
            <!-- Found Loot -->
            <StackPanel IsVisible="{Binding SearchResult.FoundLoot}">
                <TextBlock Text="Found Items:"
                           FontSize="16"
                           FontWeight="Bold"
                           Foreground="White"
                           Margin="0,0,0,10"/>
                
                <ItemsControl ItemsSource="{Binding SearchResult.LootItems}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="#1C1C1C"
                                    Padding="10"
                                    Margin="0,5"
                                    CornerRadius="5">
                                <Grid ColumnDefinitions="50,*">
                                    <Image Grid.Column="0"
                                           Source="{Binding SpriteName, Converter={StaticResource SpriteConverter}}"
                                           Width="40"
                                           Height="40"/>
                                    
                                    <StackPanel Grid.Column="1" Margin="10,0,0,0">
                                        <TextBlock Text="{Binding Name}"
                                                   FontWeight="Bold"
                                                   Foreground="White"/>
                                        <TextBlock Text="{Binding Description}"
                                                   FontSize="11"
                                                   Foreground="#CCCCCC"
                                                   TextWrapping="Wrap"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
            
            <!-- Discovered Secrets -->
            <StackPanel IsVisible="{Binding SearchResult.DiscoveredSecrets.Count}">
                <TextBlock Text="Discovered:"
                           FontSize="16"
                           FontWeight="Bold"
                           Foreground="#9400D3"
                           Margin="0,10,0,5"/>
                
                <ItemsControl ItemsSource="{Binding SearchResult.DiscoveredSecrets}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <TextBlock Text="{Binding}"
                                       Foreground="#CCCCCC"
                                       FontStyle="Italic"
                                       TextWrapping="Wrap"
                                       Margin="10,2"/>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
            
            <!-- Encounter Warning -->
            <Border IsVisible="{Binding SearchResult.TriggeredEncounter}"
                    Background="#8B0000"
                    Padding="10"
                    CornerRadius="5"
                    Margin="0,10,0,0">
                <StackPanel>
                    <TextBlock Text="⚠️ COMBAT ENCOUNTER!"
                               FontSize="18"
                               FontWeight="Bold"
                               Foreground="White"
                               HorizontalAlignment="Center"/>
                    <TextBlock Text="{Binding SearchResult.EncounterDescription}"
                               FontSize="12"
                               Foreground="White"
                               TextWrapping="Wrap"
                               Margin="0,5,0,0"/>
                </StackPanel>
            </Border>
            
            <!-- Nothing Found -->
            <TextBlock Text="You find nothing of interest."
                       Foreground="#888888"
                       FontStyle="Italic"
                       HorizontalAlignment="Center"
                       IsVisible="{Binding SearchResult, Converter={StaticResource NothingFoundConverter}}"/>
            
            <!-- Collect Button -->
            <Button Content="Collect Loot"
                    Command="{Binding CollectLootCommand}"
                    IsVisible="{Binding SearchResult.FoundLoot}"
                    HorizontalAlignment="Center"
                    Margin="0,10,0,0"/>
            
            <!-- Close Button -->
            <Button Content="Close"
                    Click="OnCloseSearchResult"
                    IsVisible="{Binding !SearchResult.FoundLoot}"
                    HorizontalAlignment="Center"
                    Margin="0,10,0,0"/>
        </StackPanel>
    </Border>
</Border>
```

### Rest Confirmation Dialog

```xml
<!-- Rest Dialog Component -->
<Border x:Name="RestDialog"
        IsVisible="False"
        Background="#E0000000"
        Grid.ColumnSpan="2">
    <Border Background="#2C2C2C"
            BorderBrush="#4A90E2"
            BorderThickness="2"
            Padding="20"
            CornerRadius="10"
            MaxWidth="400"
            VerticalAlignment="Center"
            HorizontalAlignment="Center">
        <StackPanel Spacing="15">
            <TextBlock Text="Rest"
                       FontSize="20"
                       FontWeight="Bold"
                       Foreground="White"/>
            
            <TextBlock Text="Rest to recover HP and Stamina?"
                       TextWrapping="Wrap"
                       Foreground="#CCCCCC"/>
            
            <Border Background="#8B4513"
                    Padding="10"
                    CornerRadius="5">
                <StackPanel>
                    <TextBlock Text="⚠️ Warning:"
                               FontWeight="Bold"
                               Foreground="White"/>
                    <TextBlock Text="Resting may increase Psychic Stress"
                               Foreground="White"
                               FontSize="12"/>
                    <TextBlock Text="Random encounters can occur"
                               Foreground="White"
                               FontSize="12"/>
                </StackPanel>
            </Border>
            
            <TextBlock Text="Recovery:"
                       FontWeight="Bold"
                       Foreground="#4A90E2"/>
            <TextBlock Text="• +50% Max HP"
                       Foreground="#228B22"
                       Margin="10,0"/>
            <TextBlock Text="• +50% Max Stamina"
                       Foreground="#228B22"
                       Margin="10,0"/>
            
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Center"
                        Spacing="10"
                        Margin="0,10,0,0">
                <Button Content="Rest"
                        Click="OnConfirmRest"
                        Width="100"/>
                <Button Content="Cancel"
                        Click="OnCancelRest"
                        Width="100"/>
            </StackPanel>
        </StackPanel>
    </Border>
</Border>
```

---

## Integration Points

**With v0.4 (Room Features):**

- Displays search results
- Shows loot from rooms
- Environmental storytelling

**With v0.43.12 (Navigation):**

- Adds interaction layer
- Completes exploration loop

**With v0.43.10 (Inventory):**

- Collected items go to inventory
- Item display consistent

---

## Success Criteria

**v0.43.14 is DONE when:**

### ✅ Search System

- [ ]  Search action works
- [ ]  Results display in overlay
- [ ]  Loot collection functional
- [ ]  Encounter warnings clear

### ✅ Rest System

- [ ]  Confirmation dialog shows
- [ ]  Recovery amounts clear
- [ ]  Warnings displayed
- [ ]  Psychic Stress risk communicated

### ✅ Interaction Flow

- [ ]  Smooth transitions
- [ ]  Clear feedback
- [ ]  No softlocks
- [ ]  Intuitive UX

---

**Exploration complete! Ready for endgame UI in v0.43.15.**