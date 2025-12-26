# v0.43.20: Tooltips & Help System

Type: UI
Description: Tooltip and help system: enhanced tooltips for all UI elements, searchable help documentation browser, tutorial tooltip sequence, contextual help buttons, first-time user guidance. 4-6 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.19 (All UI)
Implementation Difficulty: Easy
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.21 (All UI)

**Estimated Time:** 4-6 hours

**Group:** Systems & Polish

**Deliverable:** Comprehensive tooltip and help documentation system

---

## Executive Summary

v0.43.20 implements the tooltip and help system, providing contextual information for all UI elements, a searchable help database, and tutorial tooltips for new players.

**What This Delivers:**

- Enhanced tooltip system
- Help documentation browser
- Tutorial tooltip sequence
- Contextual help buttons
- Searchable help index
- First-time user guidance
- Integration with all v0.43 UI

**Success Metric:** Every UI element has helpful tooltips, help system is searchable and comprehensive.

---

## Service Implementation

### TooltipService

```csharp
using Avalonia.Controls;
using System.Collections.Concurrent;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface ITooltipService
{
    ToolTip GetTooltip(string key);
    void RegisterTooltip(string key, string title, string description, string? detailedHelp = null);
    IEnumerable<ToolTip> SearchTooltips(string query);
}

public class TooltipService : ITooltipService
{
    private readonly ConcurrentDictionary<string, ToolTip> _tooltips = new();
    
    public TooltipService()
    {
        InitializeDefaultTooltips();
    }
    
    public ToolTip GetTooltip(string key)
    {
        return _tooltips.TryGetValue(key, out var tooltip) 
            ? tooltip 
            : new ToolTip { Title = key, Description = "No help available." };
    }
    
    public void RegisterTooltip(string key, string title, string description, string? detailedHelp = null)
    {
        _tooltips[key] = new ToolTip
        {
            Key = key,
            Title = title,
            Description = description,
            DetailedHelp = detailedHelp
        };
    }
    
    public IEnumerable<ToolTip> SearchTooltips(string query)
    {
        var lowerQuery = query.ToLower();
        return _tooltips.Values
            .Where(t => 
                t.Title.ToLower().Contains(lowerQuery) ||
                t.Description.ToLower().Contains(lowerQuery))
            .OrderBy(t => t.Title);
    }
    
    private void InitializeDefaultTooltips()
    {
        // Combat
        RegisterTooltip("combat.attack", "Attack", 
            "Deal damage to a target enemy within range.",
            "Choose an enemy within your weapon's range and attempt to hit them. Success depends on your Accuracy vs their Evasion. Critical hits deal double damage.");
        
        RegisterTooltip("combat.defend", "Defend", 
            "Take a defensive stance to reduce incoming damage.",
            "Until your next turn, gain +50% Physical Defense and +50% Metaphysical Defense. You cannot perform other actions while defending.");
        
        RegisterTooltip("combat.move", "Move", 
            "Reposition on the battlefield.",
            "Move to any valid adjacent cell. Moving through hazards will trigger their effects. Some abilities require specific positioning.");
        
        // Stats
        RegisterTooltip("stat.might", "MIGHT", 
            "Physical power and melee damage.",
            "MIGHT increases your base melee damage, carrying capacity, and physical skill effectiveness. Each point grants +2 melee damage and +10 carrying capacity.");
        
        RegisterTooltip("stat.finesse", "FINESSE", 
            "Dexterity and ranged accuracy.",
            "FINESSE increases ranged damage, accuracy, and evasion. Each point grants +2 ranged damage, +1 Accuracy, and +1 Evasion.");
        
        RegisterTooltip("stat.wits", "WITS", 
            "Perception and initiative.",
            "WITS increases your initiative (turn order), critical chance, and detection radius. Each point grants +2 initiative and +1% critical chance.");
        
        RegisterTooltip("stat.will", "WILL", 
            "Mental fortitude and magic power.",
            "WILL increases magic damage, Psychic Stress resistance, and maximum Stamina. Each point grants +2 magic damage, +5 max Stamina, and +2 max Psychic Stress.");
        
        RegisterTooltip("stat.sturdiness", "STURDINESS", 
            "HP and defense.",
            "STURDINESS increases maximum HP and both types of defense. Each point grants +10 max HP, +1 Physical Defense, and +1 Metaphysical Defense.");
        
        // Items
        RegisterTooltip("[inventory.equipment](http://inventory.equipment)", "Equipment", 
            "Drag items from inventory to equip them.",
            "You have 3 equipment slots: Weapon, Armor, and Accessory. Equipped items provide stat bonuses and may grant special abilities.");
        
        // Status Effects
        RegisterTooltip("status.bleeding", "Bleeding", 
            "Taking damage over time.",
            "Each turn, lose HP equal to the bleed magnitude. Can stack multiple times. Cured by healing items or resting.");
        
        RegisterTooltip("status.blessed", "Blessed", 
            "Divine protection.",
            "Increases all success chances by the magnitude percentage. Cannot coexist with Cursed.");
        
        // Progression
        RegisterTooltip("progression.legend", "Legend", 
            "Your character's power level.",
            "Legend increases through defeating powerful enemies and completing floors. Each Legend level grants 2 attribute points and 1 Progression Point (PP) to spend on abilities.");
        
        RegisterTooltip("progression.pp", "Progression Points (PP)", 
            "Currency for unlocking abilities.",
            "Spend PP to unlock new abilities or rank up existing ones in your specialization tree. Earned by leveling up Legend or completing achievements.");
    }
}

public class ToolTip
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? DetailedHelp { get; set; }
}
```

### HelpViewModel

```csharp
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace RuneAndRust.DesktopUI.ViewModels;

public class HelpViewModel : ViewModelBase
{
    private readonly ITooltipService _tooltipService;
    private string _searchQuery = "";
    private ToolTip? _selectedTopic;
    
    public ObservableCollection<ToolTip> HelpTopics { get; } = new();
    public ObservableCollection<string> Categories { get; } = new()
    {
        "All", "Combat", "Stats", "Items", "Abilities", "Dungeon", "Progression"
    };
    
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            PerformSearch();
        }
    }
    
    public ToolTip? SelectedTopic
    {
        get => _selectedTopic;
        set => this.RaiseAndSetIfChanged(ref _selectedTopic, value);
    }
    
    public HelpViewModel(ITooltipService tooltipService)
    {
        _tooltipService = tooltipService;
        LoadAllTopics();
    }
    
    private void LoadAllTopics()
    {
        HelpTopics.Clear();
        var topics = _tooltipService.SearchTooltips("");
        foreach (var topic in topics)
        {
            HelpTopics.Add(topic);
        }
    }
    
    private void PerformSearch()
    {
        HelpTopics.Clear();
        
        var results = string.IsNullOrWhiteSpace(SearchQuery)
            ? _tooltipService.SearchTooltips("")
            : _tooltipService.SearchTooltips(SearchQuery);
        
        foreach (var result in results)
        {
            HelpTopics.Add(result);
        }
    }
}
```

---

## HelpView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.HelpView"
             x:DataType="vm:HelpViewModel">
    
    <Grid ColumnDefinitions="300,*" Margin="20">
        <!-- Topics List -->
        <Border Grid.Column="0" 
                Background="#2C2C2C" 
                Padding="15" 
                Margin="0,0,10,0"
                CornerRadius="5">
            <StackPanel>
                <!-- Search -->
                <TextBox Text="{Binding SearchQuery}"
                         Watermark="🔍 Search help..."
                         Margin="0,0,0,15"/>
                
                <!-- Topics -->
                <ScrollViewer Height="600">
                    <ItemsControl ItemsSource="{Binding HelpTopics}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="{Binding IsSelected, Converter={StaticResource SelectedBackgroundConverter}}"
                                        Padding="10"
                                        Margin="0,2"
                                        CornerRadius="5"
                                        Cursor="Hand"
                                        Tapped="OnTopicTapped">
                                    <TextBlock Text="{Binding Title}"
                                               Foreground="White"/>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </StackPanel>
        </Border>
        
        <!-- Help Content -->
        <Border Grid.Column="1" 
                Background="#2C2C2C" 
                Padding="30"
                CornerRadius="5">
            <ScrollViewer IsVisible="{Binding SelectedTopic, Converter={StaticResource NotNullConverter}}">
                <StackPanel Spacing="15">
                    <TextBlock Text="{Binding SelectedTopic.Title}"
                               FontSize="28"
                               FontWeight="Bold"
                               Foreground="#FFD700"/>
                    
                    <TextBlock Text="{Binding SelectedTopic.Description}"
                               FontSize="16"
                               Foreground="White"
                               TextWrapping="Wrap"/>
                    
                    <Border Background="#1C1C1C"
                            Padding="15"
                            CornerRadius="5"
                            IsVisible="{Binding SelectedTopic.DetailedHelp, Converter={StaticResource NotNullConverter}}"
                            Margin="0,10,0,0">
                        <StackPanel>
                            <TextBlock Text="Details"
                                       FontSize="18"
                                       FontWeight="Bold"
                                       Foreground="#4A90E2"
                                       Margin="0,0,0,10"/>
                            
                            <TextBlock Text="{Binding SelectedTopic.DetailedHelp}"
                                       FontSize="14"
                                       Foreground="#CCCCCC"
                                       TextWrapping="Wrap"/>
                        </StackPanel>
                    </Border>
                </StackPanel>
            </ScrollViewer>
        </Border>
    </Grid>
</UserControl>
```

---

## Integration Points

**With All v0.43 UI:**

- Tooltips on all interactive elements
- Help buttons in complex views
- Tutorial tooltips for new players

---

## Success Criteria

**v0.43.20 is DONE when:**

### ✅ Tooltips

- [ ]  All buttons have tooltips
- [ ]  All stats explained
- [ ]  All abilities documented
- [ ]  Consistent formatting

### ✅ Help System

- [ ]  Searchable help database
- [ ]  All topics covered
- [ ]  Clear navigation
- [ ]  Detailed explanations

### ✅ First-Time Experience

- [ ]  Tutorial tooltips for new UI
- [ ]  Contextual help available
- [ ]  F1 opens help

---

**Help system complete. Ready for final polish in v0.43.21.**