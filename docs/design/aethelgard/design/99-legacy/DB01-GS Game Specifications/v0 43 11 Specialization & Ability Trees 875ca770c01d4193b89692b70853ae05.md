# v0.43.11: Specialization & Ability Trees

Type: UI
Description: Specialization tree UI: tree visualization with Foundation/Core/Advanced/Mastery tiers, ability nodes with prerequisites, PP spending interface, ability rank-up buttons, and detailed ability tooltips. 5-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.11-v0.14 (Specializations)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.11-v0.14 (Specializations)

**Estimated Time:** 5-8 hours

**Group:** Character Management

**Deliverable:** Specialization tree visualization and ability management

---

## Executive Summary

v0.43.11 implements the specialization tree UI, displaying the progression paths from v0.11-v0.14, showing available abilities, allowing PP spending, and visualizing the tree structure with unlocked/locked indicators.

**What This Delivers:**

- Specialization tree visualization
- Ability nodes with prerequisites
- PP (Progression Points) spending interface
- Ability rank-up buttons
- Ability tooltips with full details
- Tree layout (Foundation → Core → Advanced → Mastery)
- Integration with v0.11-v0.14 progression

**Success Metric:** Can view entire specialization tree and spend PP to unlock/upgrade abilities.

---

## Service Implementation

### SpecializationTreeViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Abilities;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SpecializationTreeViewModel : ViewModelBase
{
    private readonly ICharacterProgressionService _progressionService;
    private PlayerCharacter? _character;
    private AbilityNodeViewModel? _selectedNode;
    
    public ObservableCollection<AbilityNodeViewModel> FoundationTier { get; } = new();
    public ObservableCollection<AbilityNodeViewModel> CoreTier { get; } = new();
    public ObservableCollection<AbilityNodeViewModel> AdvancedTier { get; } = new();
    public ObservableCollection<AbilityNodeViewModel> MasteryTier { get; } = new();
    
    public int AvailablePP => _character?.ProgressionPoints ?? 0;
    
    public AbilityNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
    }
    
    public ICommand UnlockAbilityCommand { get; }
    public ICommand RankUpAbilityCommand { get; }
    
    public SpecializationTreeViewModel(ICharacterProgressionService progressionService)
    {
        _progressionService = progressionService;
        
        UnlockAbilityCommand = ReactiveCommand.Create<AbilityNodeViewModel>(
            UnlockAbility,
            this.WhenAnyValue(x => x.SelectedNode, x => x.AvailablePP,
                (node, pp) => node != null && !node.IsUnlocked && pp >= node.UnlockCost));
        
        RankUpAbilityCommand = ReactiveCommand.Create<AbilityNodeViewModel>(
            RankUpAbility,
            this.WhenAnyValue(x => x.SelectedNode, x => x.AvailablePP,
                (node, pp) => node != null && node.IsUnlocked && node.CurrentRank < node.MaxRank && pp >= node.RankUpCost));
    }
    
    public void LoadCharacter(PlayerCharacter character)
    {
        _character = character;
        LoadAbilityTree();
        this.RaisePropertyChanged(nameof(AvailablePP));
    }
    
    private void LoadAbilityTree()
    {
        FoundationTier.Clear();
        CoreTier.Clear();
        AdvancedTier.Clear();
        MasteryTier.Clear();
        
        var specialization = _character!.Specialization;
        
        foreach (var ability in specialization.Abilities)
        {
            var node = new AbilityNodeViewModel(ability, _character);
            
            switch (ability.Tier)
            {
                case [AbilityTier.Foundation](http://AbilityTier.Foundation):
                    FoundationTier.Add(node);
                    break;
                case AbilityTier.Core:
                    CoreTier.Add(node);
                    break;
                case AbilityTier.Advanced:
                    AdvancedTier.Add(node);
                    break;
                case AbilityTier.Mastery:
                    MasteryTier.Add(node);
                    break;
            }
        }
    }
    
    private void UnlockAbility(AbilityNodeViewModel node)
    {
        var result = _progressionService.UnlockAbility(_character!, node.Ability);
        if (result.Success)
        {
            _character!.ProgressionPoints -= node.UnlockCost;
            LoadAbilityTree();
            this.RaisePropertyChanged(nameof(AvailablePP));
        }
    }
    
    private void RankUpAbility(AbilityNodeViewModel node)
    {
        var result = _progressionService.RankUpAbility(_character!, node.Ability);
        if (result.Success)
        {
            _character!.ProgressionPoints -= node.RankUpCost;
            LoadAbilityTree();
            this.RaisePropertyChanged(nameof(AvailablePP));
        }
    }
}

public class AbilityNodeViewModel : ViewModelBase
{
    public Ability Ability { get; }
    private readonly PlayerCharacter _character;
    
    public string Name => [Ability.Name](http://Ability.Name);
    public string Description => Ability.Description;
    public AbilityTier Tier => Ability.Tier;
    public bool IsUnlocked => _character.HasAbility([Ability.Id](http://Ability.Id));
    public int CurrentRank => _character.GetAbilityRank([Ability.Id](http://Ability.Id));
    public int MaxRank => Ability.MaxRank;
    public int UnlockCost => Ability.UnlockCost;
    public int RankUpCost => Ability.RankUpCost;
    
    public bool CanUnlock
    {
        get
        {
            if (IsUnlocked) return false;
            if (_character.ProgressionPoints < UnlockCost) return false;
            
            // Check prerequisites
            foreach (var prereqId in Ability.PrerequisiteAbilityIds)
            {
                if (!_character.HasAbility(prereqId)) return false;
            }
            
            return true;
        }
    }
    
    public bool CanRankUp
    {
        get
        {
            if (!IsUnlocked) return false;
            if (CurrentRank >= MaxRank) return false;
            if (_character.ProgressionPoints < RankUpCost) return false;
            return true;
        }
    }
    
    public string PrerequisiteText
    {
        get
        {
            if (Ability.PrerequisiteAbilityIds.Count == 0)
                return "No prerequisites";
            
            var prereqNames = Ability.PrerequisiteAbilityIds
                .Select(id => _character.GetAbilityName(id))
                .ToList();
            
            return $"Requires: {string.Join(", ", prereqNames)}";
        }
    }
    
    public AbilityNodeViewModel(Ability ability, PlayerCharacter character)
    {
        Ability = ability;
        _character = character;
    }
}
```

---

## SpecializationTreeView XAML

```xml
<UserControl xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.SpecializationTreeView"
             x:DataType="vm:SpecializationTreeViewModel">
    
    <Grid RowDefinitions="Auto,*" Margin="20">
        <!-- Header -->
        <Border Grid.Row="0" 
                Background="#2C2C2C" 
                Padding="15" 
                Margin="0,0,0,10"
                CornerRadius="5">
            <Grid ColumnDefinitions="*,Auto">
                <StackPanel>
                    <TextBlock Text="{Binding Character.SpecializationName}"
                               FontSize="24"
                               FontWeight="Bold"
                               Foreground="#FFD700"/>
                    <TextBlock Text="{Binding Character.SpecializationDescription}"
                               FontSize="12"
                               Foreground="#CCCCCC"
                               TextWrapping="Wrap"/>
                </StackPanel>
                
                <StackPanel Grid.Column="1" HorizontalAlignment="Right">
                    <TextBlock Text="Progression Points"
                               Foreground="#CCCCCC"
                               HorizontalAlignment="Center"/>
                    <TextBlock Text="{Binding AvailablePP}"
                               FontSize="32"
                               FontWeight="Bold"
                               Foreground="#4A90E2"
                               HorizontalAlignment="Center"/>
                </StackPanel>
            </Grid>
        </Border>
        
        <!-- Ability Tree -->
        <ScrollViewer Grid.Row="1">
            <StackPanel Spacing="20">
                <!-- Foundation Tier -->
                <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                    <StackPanel>
                        <TextBlock Text="Foundation"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#4A90E2"
                                   Margin="0,0,0,10"/>
                        
                        <ItemsControl ItemsSource="{Binding FoundationTier}">
                            <ItemsControl.ItemsPanel>
                                <ItemsPanelTemplate>
                                    <WrapPanel/>
                                </ItemsPanelTemplate>
                            </ItemsControl.ItemsPanel>
                            
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="{Binding IsUnlocked, Converter={StaticResource UnlockedBackgroundConverter}}"
                                            BorderBrush="{Binding CanUnlock, Converter={StaticResource CanUnlockBorderConverter}}"
                                            BorderThickness="2"
                                            Width="200"
                                            Margin="10"
                                            Padding="10"
                                            CornerRadius="5"
                                            Cursor="Hand"
                                            Tapped="OnAbilityNodeTapped">
                                        <StackPanel>
                                            <TextBlock Text="{Binding Name}"
                                                       FontWeight="Bold"
                                                       FontSize="14"/>
                                            
                                            <TextBlock Text="{Binding Description}"
                                                       FontSize="11"
                                                       Foreground="#CCCCCC"
                                                       TextWrapping="Wrap"
                                                       Margin="0,5"/>
                                            
                                            <StackPanel Orientation="Horizontal" Margin="0,5,0,0">
                                                <TextBlock Text="Rank: "/>
                                                <TextBlock Text="{Binding CurrentRank}"/>
                                                <TextBlock Text=" / "/>
                                                <TextBlock Text="{Binding MaxRank}"/>
                                            </StackPanel>
                                            
                                            <TextBlock Text="{Binding PrerequisiteText}"
                                                       FontSize="10"
                                                       FontStyle="Italic"
                                                       Foreground="#888888"
                                                       Margin="0,5,0,0"/>
                                            
                                            <Button Content="{Binding IsUnlocked, Converter={StaticResource UnlockRankUpTextConverter}}"
                                                    Command="{Binding $parent[UserControl].DataContext.UnlockAbilityCommand}"
                                                    CommandParameter="{Binding}"
                                                    IsVisible="{Binding CanUnlock}"
                                                    Margin="0,5,0,0"/>
                                            
                                            <Button Content="Rank Up"
                                                    Command="{Binding $parent[UserControl].DataContext.RankUpAbilityCommand}"
                                                    CommandParameter="{Binding}"
                                                    IsVisible="{Binding CanRankUp}"
                                                    Margin="0,5,0,0"/>
                                        </StackPanel>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </StackPanel>
                </Border>
                
                <!-- Core Tier -->
                <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                    <StackPanel>
                        <TextBlock Text="Core"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#FFD700"
                                   Margin="0,0,0,10"/>
                        
                        <ItemsControl ItemsSource="{Binding CoreTier}">
                            <!-- Same template as Foundation -->
                        </ItemsControl>
                    </StackPanel>
                </Border>
                
                <!-- Advanced Tier -->
                <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                    <StackPanel>
                        <TextBlock Text="Advanced"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#9400D3"
                                   Margin="0,0,0,10"/>
                        
                        <ItemsControl ItemsSource="{Binding AdvancedTier}">
                            <!-- Same template -->
                        </ItemsControl>
                    </StackPanel>
                </Border>
                
                <!-- Mastery Tier -->
                <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                    <StackPanel>
                        <TextBlock Text="Mastery"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#DC143C"
                                   Margin="0,0,0,10"/>
                        
                        <ItemsControl ItemsSource="{Binding MasteryTier}">
                            <!-- Same template -->
                        </ItemsControl>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.11-v0.14 (Specializations):**

- Displays all specialization trees
- Shows ability prerequisites
- Validates PP spending
- Updates ability ranks

**With v0.43.9 (Character Sheet):**

- PP count synced
- Legend level affects available tiers

---

## Success Criteria

**v0.43.11 is DONE when:**

### ✅ Tree Display

- [ ]  All 4 tiers visible
- [ ]  Abilities organized by tier
- [ ]  Locked/unlocked states clear
- [ ]  Prerequisites shown

### ✅ PP Spending

- [ ]  Can unlock abilities
- [ ]  Can rank up abilities
- [ ]  PP cost validation
- [ ]  Prerequisite validation

### ✅ Visual Design

- [ ]  Color-coded tiers
- [ ]  Clear hierarchy
- [ ]  Responsive layout
- [ ]  Ability tooltips detailed

---

**Character management complete! Ready for dungeon exploration in v0.43.12.**