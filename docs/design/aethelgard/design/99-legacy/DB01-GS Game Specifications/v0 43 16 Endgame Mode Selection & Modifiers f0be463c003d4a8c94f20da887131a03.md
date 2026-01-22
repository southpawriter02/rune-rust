# v0.43.16: Endgame Mode Selection & Modifiers

Type: UI
Description: Endgame mode selection UI: NG+ tier selection, Challenge Sector configuration panel, Boss Gauntlet mode selector, Endless Mode wave tracker, difficulty modifier display with rewards preview. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.40 (Endgame Content)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.40 (Endgame Content)

**Estimated Time:** 5-7 hours

**Group:** Endgame & Meta

**Deliverable:** Endgame mode selection and difficulty configuration UI

---

## Executive Summary

v0.43.16 implements the endgame mode selection UI, allowing players to choose NG+ tiers, configure Challenge Sectors, start Boss Gauntlet, and enter Endless Mode with appropriate difficulty modifiers.

**What This Delivers:**

- NG+ tier selection UI
- Challenge Sector configuration panel
- Boss Gauntlet mode selector
- Endless Mode wave tracker
- Difficulty modifier display and rewards preview
- Integration with v0.40 endgame systems

**Success Metric:** Can select and configure all endgame modes with clear difficulty/reward information.

---

## Service Implementation

### EndgameModeViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Endgame;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class EndgameModeViewModel : ViewModelBase
{
    private readonly IEndgameService _endgameService;
    private readonly INavigationService _navigationService;
    private EndgameMode _selectedMode = EndgameMode.NGPlus;
    private int _selectedNGPlusTier = 1;
    private ChallengeSectorConfig? _challengeConfig;
    
    public ObservableCollection<EndgameMode> AvailableModes { get; } = new();
    public ObservableCollection<DifficultyModifierViewModel> ActiveModifiers { get; } = new();
    public ObservableCollection<RewardMultiplierViewModel> RewardMultipliers { get; } = new();
    
    public EndgameMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMode, value);
            UpdateModeInfo();
        }
    }
    
    public int SelectedNGPlusTier
    {
        get => _selectedNGPlusTier;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedNGPlusTier, Math.Clamp(value, 1, 10));
            UpdateModifiers();
        }
    }
    
    public int MaxUnlockedNGPlusTier { get; private set; } = 1;
    public bool CanStartMode => ValidateMode();
    
    public ICommand StartModeCommand { get; }
    public ICommand IncrementTierCommand { get; }
    public ICommand DecrementTierCommand { get; }
    
    public EndgameModeViewModel(
        IEndgameService endgameService,
        INavigationService navigationService)
    {
        _endgameService = endgameService;
        _navigationService = navigationService;
        
        StartModeCommand = ReactiveCommand.CreateFromTask(StartModeAsync,
            this.WhenAnyValue(x => x.CanStartMode));
        IncrementTierCommand = ReactiveCommand.Create(() => SelectedNGPlusTier++);
        DecrementTierCommand = ReactiveCommand.Create(() => SelectedNGPlusTier--);
        
        LoadAvailableModes();
    }
    
    public void LoadPlayerProgress()
    {
        MaxUnlockedNGPlusTier = _endgameService.GetMaxUnlockedNGPlusTier();
        this.RaisePropertyChanged(nameof(MaxUnlockedNGPlusTier));
    }
    
    private void LoadAvailableModes()
    {
        AvailableModes.Clear();
        
        AvailableModes.Add(EndgameMode.NGPlus);
        
        if (_endgameService.IsChallengeSectorUnlocked())
            AvailableModes.Add(EndgameMode.ChallengeSector);
        
        if (_endgameService.IsBossGauntletUnlocked())
            AvailableModes.Add(EndgameMode.BossGauntlet);
        
        if (_endgameService.IsEndlessModeUnlocked())
            AvailableModes.Add(EndgameMode.EndlessMode);
    }
    
    private void UpdateModeInfo()
    {
        UpdateModifiers();
        UpdateRewards();
    }
    
    private void UpdateModifiers()
    {
        ActiveModifiers.Clear();
        
        var modifiers = SelectedMode switch
        {
            EndgameMode.NGPlus => _endgameService.GetNGPlusModifiers(SelectedNGPlusTier),
            EndgameMode.ChallengeSector => _endgameService.GetChallengeSectorModifiers(_challengeConfig),
            EndgameMode.BossGauntlet => _endgameService.GetBossGauntletModifiers(),
            EndgameMode.EndlessMode => _endgameService.GetEndlessModeModifiers(),
            _ => new List<DifficultyModifier>()
        };
        
        foreach (var modifier in modifiers)
        {
            ActiveModifiers.Add(new DifficultyModifierViewModel(modifier));
        }
    }
    
    private void UpdateRewards()
    {
        RewardMultipliers.Clear();
        
        var multipliers = _endgameService.GetRewardMultipliers(SelectedMode, SelectedNGPlusTier);
        
        foreach (var mult in multipliers)
        {
            RewardMultipliers.Add(new RewardMultiplierViewModel(mult));
        }
    }
    
    private async Task StartModeAsync()
    {
        var config = new EndgameModeConfig
        {
            Mode = SelectedMode,
            NGPlusTier = SelectedNGPlusTier,
            ChallengeSectorConfig = _challengeConfig
        };
        
        await _endgameService.StartEndgameModeAsync(config);
        
        // Navigate to dungeon exploration
        _navigationService.NavigateTo<DungeonExplorationViewModel>();
    }
    
    private bool ValidateMode()
    {
        return SelectedMode switch
        {
            EndgameMode.NGPlus => SelectedNGPlusTier <= MaxUnlockedNGPlusTier,
            EndgameMode.ChallengeSector => _challengeConfig != null,
            _ => true
        };
    }
}

public class DifficultyModifierViewModel
{
    public DifficultyModifier Modifier { get; }
    
    public string Name => [Modifier.Name](http://Modifier.Name);
    public string Description => Modifier.Description;
    public float Value => Modifier.Value;
    public bool IsPositive => Value > 0;
    
    public string DisplayValue
    {
        get
        {
            var sign = IsPositive ? "+" : "";
            return Modifier.Type switch
            {
                ModifierType.Percentage => $"{sign}{Value * 100:F0}%",
                ModifierType.Flat => $"{sign}{Value}",
                _ => Value.ToString()
            };
        }
    }
    
    public DifficultyModifierViewModel(DifficultyModifier modifier)
    {
        Modifier = modifier;
    }
}

public class RewardMultiplierViewModel
{
    public string Type { get; set; } = "";
    public float Multiplier { get; set; }
    
    public string DisplayText => $"{Type}: ×{Multiplier:F1}";
}
```

---

## EndgameModeView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.EndgameModeView"
             x:DataType="vm:EndgameModeViewModel">
    
    <Grid ColumnDefinitions="300,*" Margin="20">
        <!-- Mode Selection Sidebar -->
        <Border Grid.Column="0" 
                Background="#2C2C2C" 
                Padding="15" 
                Margin="0,0,10,0"
                CornerRadius="5">
            <StackPanel Spacing="10">
                <TextBlock Text="Endgame Modes"
                           FontSize="20"
                           FontWeight="Bold"
                           Foreground="White"
                           Margin="0,0,0,10"/>
                
                <ItemsControl ItemsSource="{Binding AvailableModes}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Button Content="{Binding Converter={StaticResource EndgameModeNameConverter}}"
                                    Command="{Binding $parent[UserControl].DataContext.SelectModeCommand}"
                                    CommandParameter="{Binding}"
                                    Height="50"
                                    Margin="0,5"
                                    HorizontalContentAlignment="Left"/>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </Border>
        
        <!-- Mode Configuration -->
        <ScrollViewer Grid.Column="1">
            <StackPanel>
                <!-- NG+ Configuration -->
                <Border Background="#2C2C2C" 
                        Padding="20" 
                        CornerRadius="5"
                        IsVisible="{Binding SelectedMode, Converter={StaticResource IsNGPlusConverter}}">
                    <StackPanel Spacing="15">
                        <TextBlock Text="New Game Plus"
                                   FontSize="24"
                                   FontWeight="Bold"
                                   Foreground="#FFD700"/>
                        
                        <TextBlock Text="Restart the dungeon with increased difficulty and better rewards."
                                   TextWrapping="Wrap"
                                   Foreground="#CCCCCC"/>
                        
                        <!-- Tier Selector -->
                        <Border Background="#1C1C1C" Padding="15" CornerRadius="5">
                            <Grid ColumnDefinitions="Auto,*,Auto">
                                <Button Grid.Column="0" 
                                        Content="◀"
                                        Command="{Binding DecrementTierCommand}"
                                        Width="50"
                                        Height="50"
                                        FontSize="24"/>
                                
                                <StackPanel Grid.Column="1" 
                                            VerticalAlignment="Center"
                                            HorizontalAlignment="Center">
                                    <TextBlock Text="NG+ Tier"
                                               Foreground="#CCCCCC"
                                               HorizontalAlignment="Center"/>
                                    <TextBlock FontSize="48" 
                                               FontWeight="Bold" 
                                               HorizontalAlignment="Center">
                                        <Run Text="{Binding SelectedNGPlusTier}" Foreground="#FFD700"/>
                                    </TextBlock>
                                    <TextBlock Foreground="#888888" HorizontalAlignment="Center">
                                        <Run Text="Max Unlocked: "/>
                                        <Run Text="{Binding MaxUnlockedNGPlusTier}"/>
                                    </TextBlock>
                                </StackPanel>
                                
                                <Button Grid.Column="2" 
                                        Content="▶"
                                        Command="{Binding IncrementTierCommand}"
                                        Width="50"
                                        Height="50"
                                        FontSize="24"/>
                            </Grid>
                        </Border>
                    </StackPanel>
                </Border>
                
                <!-- Difficulty Modifiers -->
                <Border Background="#2C2C2C" 
                        Padding="20" 
                        Margin="0,10,0,0"
                        CornerRadius="5">
                    <StackPanel Spacing="10">
                        <TextBlock Text="Difficulty Modifiers"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#DC143C"/>
                        
                        <ItemsControl ItemsSource="{Binding ActiveModifiers}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="#1C1C1C"
                                            Padding="10"
                                            Margin="0,5"
                                            CornerRadius="5">
                                        <Grid ColumnDefinitions="*,Auto">
                                            <StackPanel Grid.Column="0">
                                                <TextBlock Text="{Binding Name}"
                                                           FontWeight="Bold"
                                                           Foreground="White"/>
                                                <TextBlock Text="{Binding Description}"
                                                           FontSize="11"
                                                           Foreground="#CCCCCC"/>
                                            </StackPanel>
                                            <TextBlock Grid.Column="1"
                                                       Text="{Binding DisplayValue}"
                                                       FontSize="20"
                                                       FontWeight="Bold"
                                                       Foreground="{Binding IsPositive, Converter={StaticResource ModifierColorConverter}}"
                                                       VerticalAlignment="Center"/>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </StackPanel>
                </Border>
                
                <!-- Reward Multipliers -->
                <Border Background="#2C2C2C" 
                        Padding="20" 
                        Margin="0,10,0,0"
                        CornerRadius="5">
                    <StackPanel Spacing="10">
                        <TextBlock Text="Reward Multipliers"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#228B22"/>
                        
                        <ItemsControl ItemsSource="{Binding RewardMultipliers}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <TextBlock Text="{Binding DisplayText}"
                                               Foreground="#4A90E2"
                                               FontSize="14"
                                               Margin="10,2"/>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </StackPanel>
                </Border>
                
                <!-- Start Button -->
                <Button Content="Start Mode"
                        Command="{Binding StartModeCommand}"
                        Height="60"
                        FontSize="20"
                        FontWeight="Bold"
                        Margin="0,20,0,0"/>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.40 (Endgame Content):**

- Configures NG+ tiers
- Sets up Challenge Sectors
- Starts Boss Gauntlet
- Initiates Endless Mode

**With v0.43.12 (Dungeon):**

- Applies difficulty modifiers
- Adjusts reward multipliers

---

## Success Criteria

**v0.43.16 is DONE when:**

### ✅ Mode Selection

- [ ]  All unlocked modes visible
- [ ]  Clear mode descriptions
- [ ]  Easy switching between modes

### ✅ NG+ Configuration

- [ ]  Tier selector functional
- [ ]  Cannot exceed max unlocked
- [ ]  Clear tier progression

### ✅ Modifier Display

- [ ]  All modifiers listed
- [ ]  Clear value display
- [ ]  Reward multipliers shown
- [ ]  Visual distinction positive/negative

---

**Endgame mode selection complete. Ready for boss enhancements in v0.43.17.**