# v0.43.15: Meta-Progression & Achievements UI

Type: UI
Description: Meta-progression UI: achievement tracking display, meta-progression unlock browser, account-level statistics, cosmetic unlock gallery, progress tracking for hidden achievements. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.41 (Meta-Progression)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.41 (Meta-Progression)

**Estimated Time:** 5-7 hours

**Group:** Endgame & Meta

**Deliverable:** Achievement tracking and meta-progression display

---

## Executive Summary

v0.43.15 implements the meta-progression UI, displaying achievements, account-level statistics, cosmetic unlocks, and persistent progression that carries across runs.

**What This Delivers:**

- Achievement tracking display
- Meta-progression unlock browser
- Account-level statistics
- Cosmetic unlock gallery
- Progress tracking for hidden achievements
- Integration with v0.41 meta-progression

**Success Metric:** All achievements and meta unlocks visible with clear progress indicators.

---

## Service Implementation

### MetaProgressionViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.MetaProgression;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using System.Linq;

namespace RuneAndRust.DesktopUI.ViewModels;

public class MetaProgressionViewModel : ViewModelBase
{
    private readonly IMetaProgressionService _metaService;
    private MetaProgressionState? _metaState;
    private AchievementViewModel? _selectedAchievement;
    private string _filterCategory = "All";
    
    public ObservableCollection<AchievementViewModel> Achievements { get; } = new();
    public ObservableCollection<UnlockViewModel> Unlocks { get; } = new();
    public ObservableCollection<string> Categories { get; } = new() { "All", "Combat", "Exploration", "Character", "Endgame", "Hidden" };
    
    public string FilterCategory
    {
        get => _filterCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterCategory, value);
            FilterAchievements();
        }
    }
    
    public AchievementViewModel? SelectedAchievement
    {
        get => _selectedAchievement;
        set => this.RaiseAndSetIfChanged(ref _selectedAchievement, value);
    }
    
    // Statistics
    public int TotalAchievements => _metaState?.TotalAchievements ?? 0;
    public int UnlockedAchievements => _metaState?.UnlockedAchievements ?? 0;
    public float CompletionPercentage => TotalAchievements > 0 ? (float)UnlockedAchievements / TotalAchievements : 0f;
    
    public int TotalRuns => _metaState?.TotalRuns ?? 0;
    public int SuccessfulRuns => _metaState?.SuccessfulRuns ?? 0;
    public int HighestLegend => _metaState?.HighestLegend ?? 0;
    public TimeSpan TotalPlayTime => _metaState?.TotalPlayTime ?? [TimeSpan.Zero](http://TimeSpan.Zero);
    
    public MetaProgressionViewModel(IMetaProgressionService metaService)
    {
        _metaService = metaService;
    }
    
    public void LoadMetaProgression()
    {
        _metaState = _metaService.LoadMetaProgression();
        LoadAchievements();
        LoadUnlocks();
        UpdateStatistics();
    }
    
    private void LoadAchievements()
    {
        Achievements.Clear();
        
        foreach (var achievement in _metaState!.Achievements)
        {
            Achievements.Add(new AchievementViewModel(achievement));
        }
        
        FilterAchievements();
    }
    
    private void LoadUnlocks()
    {
        Unlocks.Clear();
        
        foreach (var unlock in _metaState!.Unlocks)
        {
            if (unlock.IsUnlocked)
            {
                Unlocks.Add(new UnlockViewModel(unlock));
            }
        }
    }
    
    private void FilterAchievements()
    {
        var filtered = FilterCategory == "All" 
            ? _metaState!.Achievements
            : _metaState!.Achievements.Where(a => a.Category == FilterCategory);
        
        Achievements.Clear();
        foreach (var achievement in filtered)
        {
            Achievements.Add(new AchievementViewModel(achievement));
        }
    }
    
    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(TotalAchievements));
        this.RaisePropertyChanged(nameof(UnlockedAchievements));
        this.RaisePropertyChanged(nameof(CompletionPercentage));
        this.RaisePropertyChanged(nameof(TotalRuns));
        this.RaisePropertyChanged(nameof(SuccessfulRuns));
        this.RaisePropertyChanged(nameof(HighestLegend));
        this.RaisePropertyChanged(nameof(TotalPlayTime));
    }
}

public class AchievementViewModel
{
    public Achievement Achievement { get; }
    
    public string Name => [Achievement.Name](http://Achievement.Name);
    public string Description => Achievement.Description;
    public string Category => Achievement.Category;
    public bool IsUnlocked => Achievement.IsUnlocked;
    public bool IsHidden => Achievement.IsHidden;
    public float Progress => Achievement.Progress;
    public int CurrentValue => Achievement.CurrentValue;
    public int TargetValue => Achievement.TargetValue;
    public string RewardDescription => Achievement.RewardDescription;
    
    public string DisplayName => IsHidden && !IsUnlocked ? "???" : Name;
    public string DisplayDescription => IsHidden && !IsUnlocked ? "Hidden achievement" : Description;
    
    public AchievementViewModel(Achievement achievement)
    {
        Achievement = achievement;
    }
}

public class UnlockViewModel
{
    public MetaUnlock Unlock { get; }
    
    public string Name => [Unlock.Name](http://Unlock.Name);
    public string Description => Unlock.Description;
    public UnlockType Type => Unlock.Type;
    public bool IsCosmetic => Type == UnlockType.Cosmetic;
    
    public UnlockViewModel(MetaUnlock unlock)
    {
        Unlock = unlock;
    }
}
```

---

## MetaProgressionView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.MetaProgressionView"
             x:DataType="vm:MetaProgressionViewModel">
    
    <Grid RowDefinitions="Auto,*" Margin="20">
        <!-- Statistics Header -->
        <Border Grid.Row="0" 
                Background="#2C2C2C" 
                Padding="20" 
                Margin="0,0,0,10"
                CornerRadius="5">
            <Grid ColumnDefinitions="*,*,*,*">
                <!-- Total Play Time -->
                <StackPanel Grid.Column="0">
                    <TextBlock Text="Total Play Time"
                               Foreground="#CCCCCC"
                               HorizontalAlignment="Center"/>
                    <TextBlock Text="{Binding TotalPlayTime, StringFormat='{}{0:hh\\:mm\\:ss}'}"
                               FontSize="24"
                               FontWeight="Bold"
                               Foreground="#4A90E2"
                               HorizontalAlignment="Center"/>
                </StackPanel>
                
                <!-- Total Runs -->
                <StackPanel Grid.Column="1">
                    <TextBlock Text="Total Runs"
                               Foreground="#CCCCCC"
                               HorizontalAlignment="Center"/>
                    <TextBlock FontSize="24" FontWeight="Bold" HorizontalAlignment="Center">
                        <Run Text="{Binding SuccessfulRuns}" Foreground="#228B22"/>
                        <Run Text=" / " Foreground="White"/>
                        <Run Text="{Binding TotalRuns}" Foreground="White"/>
                    </TextBlock>
                </StackPanel>
                
                <!-- Highest Legend -->
                <StackPanel Grid.Column="2">
                    <TextBlock Text="Highest Legend"
                               Foreground="#CCCCCC"
                               HorizontalAlignment="Center"/>
                    <TextBlock Text="{Binding HighestLegend}"
                               FontSize="24"
                               FontWeight="Bold"
                               Foreground="#FFD700"
                               HorizontalAlignment="Center"/>
                </StackPanel>
                
                <!-- Achievement Progress -->
                <StackPanel Grid.Column="3">
                    <TextBlock Text="Achievements"
                               Foreground="#CCCCCC"
                               HorizontalAlignment="Center"/>
                    <TextBlock FontSize="24" FontWeight="Bold" HorizontalAlignment="Center">
                        <Run Text="{Binding UnlockedAchievements}" Foreground="#FFD700"/>
                        <Run Text=" / " Foreground="White"/>
                        <Run Text="{Binding TotalAchievements}" Foreground="White"/>
                    </TextBlock>
                    <ProgressBar Value="{Binding CompletionPercentage}"
                                 Maximum="1.0"
                                 Height="8"
                                 Foreground="#FFD700"
                                 Margin="10,5,10,0"/>
                </StackPanel>
            </Grid>
        </Border>
        
        <!-- Achievements and Unlocks -->
        <Grid Grid.Row="1" ColumnDefinitions="*,300">
            <!-- Achievements List -->
            <Border Grid.Column="0" 
                    Background="#2C2C2C" 
                    Padding="15" 
                    Margin="0,0,10,0"
                    CornerRadius="5">
                <StackPanel>
                    <!-- Category Filter -->
                    <Grid ColumnDefinitions="Auto,*" Margin="0,0,0,10">
                        <TextBlock Grid.Column="0" 
                                   Text="Filter:" 
                                   VerticalAlignment="Center"
                                   Margin="0,0,10,0"/>
                        <ComboBox Grid.Column="1" 
                                  ItemsSource="{Binding Categories}"
                                  SelectedItem="{Binding FilterCategory}"
                                  Width="150"
                                  HorizontalAlignment="Left"/>
                    </Grid>
                    
                    <!-- Achievements -->
                    <ScrollViewer Height="600">
                        <ItemsControl ItemsSource="{Binding Achievements}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="{Binding IsUnlocked, Converter={StaticResource UnlockedBackgroundConverter}}"
                                            BorderBrush="{Binding IsUnlocked, Converter={StaticResource UnlockedBorderConverter}}"
                                            BorderThickness="2"
                                            Padding="15"
                                            Margin="0,5"
                                            CornerRadius="5"
                                            Cursor="Hand"
                                            Tapped="OnAchievementTapped">
                                        <Grid ColumnDefinitions="50,*">
                                            <!-- Icon -->
                                            <TextBlock Grid.Column="0"
                                                       Text="{Binding IsUnlocked, Converter={StaticResource AchievementIconConverter}}"
                                                       FontSize="32"
                                                       VerticalAlignment="Center"/>
                                            
                                            <!-- Info -->
                                            <StackPanel Grid.Column="1" Margin="10,0,0,0">
                                                <TextBlock Text="{Binding DisplayName}"
                                                           FontWeight="Bold"
                                                           FontSize="14"/>
                                                <TextBlock Text="{Binding DisplayDescription}"
                                                           FontSize="11"
                                                           Foreground="#CCCCCC"
                                                           TextWrapping="Wrap"
                                                           Margin="0,3,0,0"/>
                                                
                                                <!-- Progress Bar (if not unlocked) -->
                                                <StackPanel IsVisible="{Binding !IsUnlocked}" Margin="0,5,0,0">
                                                    <TextBlock FontSize="10">
                                                        <Run Text="{Binding CurrentValue}"/>
                                                        <Run Text=" / "/>
                                                        <Run Text="{Binding TargetValue}"/>
                                                    </TextBlock>
                                                    <ProgressBar Value="{Binding Progress}"
                                                                 Maximum="1.0"
                                                                 Height="6"
                                                                 Foreground="#4A90E2"/>
                                                </StackPanel>
                                                
                                                <!-- Reward (if unlocked) -->
                                                <TextBlock Text="{Binding RewardDescription}"
                                                           FontSize="10"
                                                           Foreground="#FFD700"
                                                           IsVisible="{Binding IsUnlocked}"
                                                           Margin="0,3,0,0"/>
                                            </StackPanel>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </ScrollViewer>
                </StackPanel>
            </Border>
            
            <!-- Unlocks Sidebar -->
            <Border Grid.Column="1" 
                    Background="#2C2C2C" 
                    Padding="15"
                    CornerRadius="5">
                <StackPanel>
                    <TextBlock Text="Unlocks"
                               FontSize="18"
                               FontWeight="Bold"
                               Foreground="White"
                               Margin="0,0,0,10"/>
                    
                    <ScrollViewer Height="600">
                        <ItemsControl ItemsSource="{Binding Unlocks}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="#1C1C1C"
                                            Padding="10"
                                            Margin="0,5"
                                            CornerRadius="5">
                                        <StackPanel>
                                            <TextBlock Text="{Binding Name}"
                                                       FontWeight="Bold"
                                                       Foreground="#FFD700"/>
                                            <TextBlock Text="{Binding Description}"
                                                       FontSize="11"
                                                       Foreground="#CCCCCC"
                                                       TextWrapping="Wrap"
                                                       Margin="0,3,0,0"/>
                                        </StackPanel>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </ScrollViewer>
                </StackPanel>
            </Border>
        </Grid>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.41 (Meta-Progression):**

- Loads all achievements
- Shows unlock progress
- Displays account statistics

**With v0.43.3 (Navigation):**

- Accessible from main menu
- Persistent across runs

---

## Success Criteria

**v0.43.15 is DONE when:**

### ✅ Achievements Display

- [ ]  All achievements listed
- [ ]  Progress bars for incomplete
- [ ]  Hidden achievements obscured
- [ ]  Category filtering works

### ✅ Statistics

- [ ]  Total play time accurate
- [ ]  Run count displayed
- [ ]  Highest legend shown
- [ ]  Completion percentage

### ✅ Unlocks

- [ ]  All earned unlocks visible
- [ ]  Clear descriptions
- [ ]  Organized by type

---

**Meta-progression UI complete. Ready for endgame modes in v0.43.16.**