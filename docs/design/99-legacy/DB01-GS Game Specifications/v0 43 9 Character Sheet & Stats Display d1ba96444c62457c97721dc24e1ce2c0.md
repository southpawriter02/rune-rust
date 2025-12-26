# v0.43.9: Character Sheet & Stats Display

Type: UI
Description: Character sheet view displaying all attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS), derived stats (HP, Stamina, Speed, Accuracy), trauma meters (Psychic Stress, Corruption), and Legend/XP progression. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.6-v0.10 (Character Stats)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.6-v0.10 (Character Stats)

**Estimated Time:** 6-8 hours

**Group:** Character Management

**Deliverable:** Complete character sheet UI with all stats

---

## Executive Summary

v0.43.9 implements the character sheet view, displaying all attributes, derived stats, trauma meters, and progression information from v0.6-v0.10 in a clear, organized layout.

**What This Delivers:**

- Character sheet view layout
- Core attributes display (MIGHT, FINESSE, WITS, WILL, STURDINESS)
- Derived stats (HP, Stamina, Speed, Accuracy, etc.)
- Trauma meters (Psychic Stress, Corruption)
- Legend/XP progression bar
- Stat tooltips with explanations

**Success Metric:** All character statistics visible and understandable at a glance.

---

## Service Implementation

### CharacterSheetViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Characters;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);

namespace RuneAndRust.DesktopUI.ViewModels;

public class CharacterSheetViewModel : ViewModelBase
{
    private readonly ICharacterProgressionService _progressionService;
    private PlayerCharacter? _character;
    
    public PlayerCharacter? Character
    {
        get => _character;
        set
        {
            this.RaiseAndSetIfChanged(ref _character, value);
            UpdateAllStats();
        }
    }
    
    // Core Attributes
    public int Might => Character?.Might ?? 0;
    public int Finesse => Character?.Finesse ?? 0;
    public int Wits => Character?.Wits ?? 0;
    public int Will => Character?.Will ?? 0;
    public int Sturdiness => Character?.Sturdiness ?? 0;
    
    // Derived Stats
    public int MaxHP => Character?.MaxHP ?? 0;
    public int CurrentHP => Character?.CurrentHP ?? 0;
    public int MaxStamina => Character?.MaxStamina ?? 0;
    public int CurrentStamina => Character?.CurrentStamina ?? 0;
    public int Speed => Character?.Speed ?? 0;
    public int Accuracy => Character?.Accuracy ?? 0;
    public int Evasion => Character?.Evasion ?? 0;
    public int CritChance => Character?.CritChance ?? 0;
    
    // Combat Stats
    public int PhysicalDefense => Character?.PhysicalDefense ?? 0;
    public int MetaphysicalDefense => Character?.MetaphysicalDefense ?? 0;
    public int AttackPower => Character?.AttackPower ?? 0;
    
    // Trauma
    public int PsychicStress => Character?.PsychicStress ?? 0;
    public int MaxPsychicStress => Character?.MaxPsychicStress ?? 0;
    public int Corruption => Character?.Corruption ?? 0;
    public int MaxCorruption => Character?.MaxCorruption ?? 0;
    
    // Progression
    public int Legend => Character?.Legend ?? 0;
    public int CurrentXP => Character?.CurrentXP ?? 0;
    public int XPToNextLevel => Character?.XPToNextLevel ?? 0;
    public float XPProgress => XPToNextLevel > 0 ? (float)CurrentXP / XPToNextLevel : 0f;
    
    public CharacterSheetViewModel(ICharacterProgressionService progressionService)
    {
        _progressionService = progressionService;
    }
    
    private void UpdateAllStats()
    {
        this.RaisePropertyChanged(nameof(Might));
        this.RaisePropertyChanged(nameof(Finesse));
        this.RaisePropertyChanged(nameof(Wits));
        this.RaisePropertyChanged(nameof(Will));
        this.RaisePropertyChanged(nameof(Sturdiness));
        this.RaisePropertyChanged(nameof(MaxHP));
        this.RaisePropertyChanged(nameof(CurrentHP));
        this.RaisePropertyChanged(nameof(MaxStamina));
        this.RaisePropertyChanged(nameof(CurrentStamina));
        this.RaisePropertyChanged(nameof(Speed));
        this.RaisePropertyChanged(nameof(Accuracy));
        this.RaisePropertyChanged(nameof(Evasion));
        this.RaisePropertyChanged(nameof(CritChance));
        this.RaisePropertyChanged(nameof(PhysicalDefense));
        this.RaisePropertyChanged(nameof(MetaphysicalDefense));
        this.RaisePropertyChanged(nameof(AttackPower));
        this.RaisePropertyChanged(nameof(PsychicStress));
        this.RaisePropertyChanged(nameof(Corruption));
        this.RaisePropertyChanged(nameof(Legend));
        this.RaisePropertyChanged(nameof(XPProgress));
    }
}
```

---

## CharacterSheetView XAML

```xml
<UserControl xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.CharacterSheetView"
             x:DataType="vm:CharacterSheetViewModel">
    
    <ScrollViewer>
        <StackPanel Margin="20" Spacing="20">
            <!-- Header -->
            <TextBlock Text="{Binding [Character.Name](http://Character.Name)}"
                       FontSize="32"
                       FontWeight="Bold"
                       Foreground="#FFD700"/>
            
            <TextBlock Text="{Binding Character.SpecializationName}"
                       FontSize="18"
                       Foreground="#CCCCCC"/>
            
            <!-- Core Attributes -->
            <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                <StackPanel Spacing="10">
                    <TextBlock Text="Core Attributes"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto">
                        <!-- MIGHT -->
                        <StackPanel Grid.Row="0" Grid.Column="0" Margin="5">
                            <TextBlock Text="MIGHT" Foreground="#DC143C"/>
                            <TextBlock Text="{Binding Might}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="Physical power, melee damage" 
                                       FontSize="10" 
                                       Foreground="#888888"/>
                        </StackPanel>
                        
                        <!-- FINESSE -->
                        <StackPanel Grid.Row="0" Grid.Column="1" Margin="5">
                            <TextBlock Text="FINESSE" Foreground="#4A90E2"/>
                            <TextBlock Text="{Binding Finesse}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="Dexterity, ranged accuracy" 
                                       FontSize="10" 
                                       Foreground="#888888"/>
                        </StackPanel>
                        
                        <!-- WITS -->
                        <StackPanel Grid.Row="1" Grid.Column="0" Margin="5">
                            <TextBlock Text="WITS" Foreground="#FFD700"/>
                            <TextBlock Text="{Binding Wits}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="Perception, initiative" 
                                       FontSize="10" 
                                       Foreground="#888888"/>
                        </StackPanel>
                        
                        <!-- WILL -->
                        <StackPanel Grid.Row="1" Grid.Column="1" Margin="5">
                            <TextBlock Text="WILL" Foreground="#9400D3"/>
                            <TextBlock Text="{Binding Will}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="Mental fortitude, magic power" 
                                       FontSize="10" 
                                       Foreground="#888888"/>
                        </StackPanel>
                        
                        <!-- STURDINESS -->
                        <StackPanel Grid.Row="2" Grid.Column="0" Margin="5">
                            <TextBlock Text="STURDINESS" Foreground="#228B22"/>
                            <TextBlock Text="{Binding Sturdiness}" FontSize="24" FontWeight="Bold"/>
                            <TextBlock Text="HP, defense, endurance" 
                                       FontSize="10" 
                                       Foreground="#888888"/>
                        </StackPanel>
                    </Grid>
                </StackPanel>
            </Border>
            
            <!-- Derived Stats -->
            <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                <StackPanel Spacing="10">
                    <TextBlock Text="Combat Statistics"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto,Auto,Auto">
                        <!-- HP -->
                        <StackPanel Grid.Row="0" Grid.Column="0" Margin="5">
                            <TextBlock Text="Hit Points"/>
                            <TextBlock FontSize="18">
                                <Run Text="{Binding CurrentHP}" Foreground="#228B22"/>
                                <Run Text=" / "/>
                                <Run Text="{Binding MaxHP}"/>
                            </TextBlock>
                        </StackPanel>
                        
                        <!-- Stamina -->
                        <StackPanel Grid.Row="0" Grid.Column="1" Margin="5">
                            <TextBlock Text="Stamina"/>
                            <TextBlock FontSize="18">
                                <Run Text="{Binding CurrentStamina}" Foreground="#4A90E2"/>
                                <Run Text=" / "/>
                                <Run Text="{Binding MaxStamina}"/>
                            </TextBlock>
                        </StackPanel>
                        
                        <!-- Speed -->
                        <StackPanel Grid.Row="0" Grid.Column="2" Margin="5">
                            <TextBlock Text="Speed"/>
                            <TextBlock Text="{Binding Speed}" FontSize="18"/>
                        </StackPanel>
                        
                        <!-- Accuracy -->
                        <StackPanel Grid.Row="1" Grid.Column="0" Margin="5">
                            <TextBlock Text="Accuracy"/>
                            <TextBlock Text="{Binding Accuracy}" FontSize="18"/>
                        </StackPanel>
                        
                        <!-- Evasion -->
                        <StackPanel Grid.Row="1" Grid.Column="1" Margin="5">
                            <TextBlock Text="Evasion"/>
                            <TextBlock Text="{Binding Evasion}" FontSize="18"/>
                        </StackPanel>
                        
                        <!-- Crit Chance -->
                        <StackPanel Grid.Row="1" Grid.Column="2" Margin="5">
                            <TextBlock Text="Critical Chance"/>
                            <TextBlock FontSize="18">
                                <Run Text="{Binding CritChance}"/>
                                <Run Text="%"/>
                            </TextBlock>
                        </StackPanel>
                        
                        <!-- Attack Power -->
                        <StackPanel Grid.Row="2" Grid.Column="0" Margin="5">
                            <TextBlock Text="Attack Power"/>
                            <TextBlock Text="{Binding AttackPower}" FontSize="18"/>
                        </StackPanel>
                        
                        <!-- Physical Defense -->
                        <StackPanel Grid.Row="2" Grid.Column="1" Margin="5">
                            <TextBlock Text="Physical Defense"/>
                            <TextBlock Text="{Binding PhysicalDefense}" FontSize="18"/>
                        </StackPanel>
                        
                        <!-- Metaphysical Defense -->
                        <StackPanel Grid.Row="2" Grid.Column="2" Margin="5">
                            <TextBlock Text="Metaphysical Defense"/>
                            <TextBlock Text="{Binding MetaphysicalDefense}" FontSize="18"/>
                        </StackPanel>
                    </Grid>
                </StackPanel>
            </Border>
            
            <!-- Trauma Meters -->
            <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                <StackPanel Spacing="10">
                    <TextBlock Text="Trauma & Corruption"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <!-- Psychic Stress -->
                    <StackPanel>
                        <Grid ColumnDefinitions="*,Auto">
                            <TextBlock Text="Psychic Stress" Foreground="#9400D3"/>
                            <TextBlock Grid.Column="1">
                                <Run Text="{Binding PsychicStress}"/>
                                <Run Text=" / "/>
                                <Run Text="{Binding MaxPsychicStress}"/>
                            </TextBlock>
                        </Grid>
                        <ProgressBar Value="{Binding PsychicStress}"
                                     Maximum="{Binding MaxPsychicStress}"
                                     Height="20"
                                     Foreground="#9400D3"
                                     Margin="0,5"/>
                    </StackPanel>
                    
                    <!-- Corruption -->
                    <StackPanel>
                        <Grid ColumnDefinitions="*,Auto">
                            <TextBlock Text="Corruption" Foreground="#8B0000"/>
                            <TextBlock Grid.Column="1">
                                <Run Text="{Binding Corruption}"/>
                                <Run Text=" / "/>
                                <Run Text="{Binding MaxCorruption}"/>
                            </TextBlock>
                        </Grid>
                        <ProgressBar Value="{Binding Corruption}"
                                     Maximum="{Binding MaxCorruption}"
                                     Height="20"
                                     Foreground="#8B0000"
                                     Margin="0,5"/>
                    </StackPanel>
                </StackPanel>
            </Border>
            
            <!-- Progression -->
            <Border Background="#2C2C2C" Padding="15" CornerRadius="5">
                <StackPanel Spacing="10">
                    <TextBlock Text="Progression"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <Grid ColumnDefinitions="*,Auto">
                        <TextBlock Text="Legend Level" Foreground="#FFD700"/>
                        <TextBlock Grid.Column="1" 
                                   Text="{Binding Legend}" 
                                   FontSize="24" 
                                   FontWeight="Bold"
                                   Foreground="#FFD700"/>
                    </Grid>
                    
                    <StackPanel>
                        <Grid ColumnDefinitions="*,Auto">
                            <TextBlock Text="Experience"/>
                            <TextBlock Grid.Column="1">
                                <Run Text="{Binding CurrentXP}"/>
                                <Run Text=" / "/>
                                <Run Text="{Binding XPToNextLevel}"/>
                            </TextBlock>
                        </Grid>
                        <ProgressBar Value="{Binding XPProgress}"
                                     Maximum="1.0"
                                     Height="15"
                                     Foreground="#4A90E2"
                                     Margin="0,5"/>
                    </StackPanel>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

---

## Integration Points

**With v0.6-v0.10 (Character Stats):**

- Displays all core attributes
- Shows derived stats calculations
- Trauma meters from v0.9
- Legend progression from v0.10

**With v0.43.3 (Navigation):**

- Accessible via 'C' keyboard shortcut
- Shows in navigation sidebar

---

## Success Criteria

**v0.43.9 is DONE when:**

### ✅ Attributes Display

- [ ]  All 5 core attributes visible
- [ ]  Color-coded by type
- [ ]  Tooltips explain each stat

### ✅ Derived Stats

- [ ]  HP/Stamina with current/max
- [ ]  All combat stats shown
- [ ]  Accurate calculations

### ✅ Trauma Meters

- [ ]  Progress bars for stress/corruption
- [ ]  Current/max values
- [ ]  Visual warnings at high levels

### ✅ Progression

- [ ]  Legend level prominent
- [ ]  XP bar shows progress
- [ ]  Clear next level indication

---

**Character sheet complete. Ready for inventory in v0.43.10.**