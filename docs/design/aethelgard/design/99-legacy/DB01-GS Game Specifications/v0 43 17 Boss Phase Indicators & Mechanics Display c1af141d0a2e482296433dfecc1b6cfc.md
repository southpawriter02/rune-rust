# v0.43.17: Boss Phase Indicators & Mechanics Display

Type: UI
Description: Specialized boss encounter UI: phase indicators, phase transition animations, mechanic warning system, enrage timer display, boss ability telegraph visualization, health bar segmentation. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.4-v0.43.8, v0.42 (Boss Enhancements)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.4-v0.43.8, v0.42 (Boss Enhancements)

**Estimated Time:** 5-7 hours

**Group:** Endgame & Meta

**Deliverable:** Boss-specific UI elements and phase visualization

---

## Executive Summary

v0.43.17 implements specialized UI for boss encounters, including phase indicators, mechanic warnings, enrage timers, and unique boss ability visualizations.

**What This Delivers:**

- Boss phase indicator UI
- Phase transition animations
- Mechanic warning system
- Enrage timer display
- Boss ability telegraph visualization
- Health bar segmentation for phases
- Integration with v0.42 boss systems

**Success Metric:** Boss mechanics clearly communicated with visual warnings and phase tracking.

---

## Service Implementation

### BossCombatViewModel (Enhancement)

```csharp
// Add to CombatViewModel from v0.43.5

using RuneAndRust.Core.Bosses;

private BossEnemy? _currentBoss;
private int _currentPhase = 1;
private float _enrageProgress = 0f;
private ObservableCollection<BossMechanicWarning> _activeWarnings = new();

public bool IsBossFight => _currentBoss != null;

public BossEnemy? CurrentBoss
{
    get => _currentBoss;
    set
    {
        this.RaiseAndSetIfChanged(ref _currentBoss, value);
        this.RaisePropertyChanged(nameof(IsBossFight));
        UpdateBossPhases();
    }
}

public int CurrentPhase
{
    get => _currentPhase;
    set => this.RaiseAndSetIfChanged(ref _currentPhase, value);
}

public int TotalPhases => _currentBoss?.TotalPhases ?? 1;

public float EnrageProgress
{
    get => _enrageProgress;
    set => this.RaiseAndSetIfChanged(ref _enrageProgress, Math.Clamp(value, 0f, 1f));
}

public ObservableCollection<BossMechanicWarning> ActiveWarnings
{
    get => _activeWarnings;
    set => this.RaiseAndSetIfChanged(ref _activeWarnings, value);
}

public ObservableCollection<PhaseHealthSegment> PhaseSegments { get; } = new();

private void UpdateBossPhases()
{
    if (_currentBoss == null) return;
    
    PhaseSegments.Clear();
    
    for (int i = 1; i <= _currentBoss.TotalPhases; i++)
    {
        PhaseSegments.Add(new PhaseHealthSegment
        {
            PhaseNumber = i,
            IsCurrentPhase = i == CurrentPhase,
            IsCompleted = i < CurrentPhase,
            HealthThreshold = _currentBoss.GetPhaseThreshold(i)
        });
    }
}

public void OnBossPhaseTransition(int newPhase)
{
    CurrentPhase = newPhase;
    UpdateBossPhases();
    
    // Trigger phase transition animation
    ShowPhaseTransitionEffect(newPhase);
}

public void OnBossMechanicTriggered(BossMechanic mechanic)
{
    var warning = new BossMechanicWarning
    {
        MechanicName = [mechanic.Name](http://mechanic.Name),
        Description = mechanic.Description,
        WarningTime = mechanic.TelegraphDuration,
        DangerLevel = mechanic.DangerLevel
    };
    
    ActiveWarnings.Add(warning);
    
    // Remove after duration
    Task.Delay(warning.WarningTime * 1000).ContinueWith(_ =>
    {
        [Dispatcher.UIThread.Post](http://Dispatcher.UIThread.Post)(() => ActiveWarnings.Remove(warning));
    });
}

public void UpdateEnrageTimer(float progress)
{
    EnrageProgress = progress;
}

private async Task ShowPhaseTransitionEffect(int phase)
{
    // Trigger screen flash and phase announcement
    await _animationService.PlayPhaseTransitionAsync(phase);
}

public class BossMechanicWarning : ViewModelBase
{
    public string MechanicName { get; set; } = "";
    public string Description { get; set; } = "";
    public float WarningTime { get; set; }
    public DangerLevel DangerLevel { get; set; }
    
    public Color WarningColor => DangerLevel switch
    {
        DangerLevel.Low => Colors.Yellow,
        DangerLevel.Medium => [Colors.Orange](http://Colors.Orange),
        DangerLevel.High => [Colors.Red](http://Colors.Red),
        DangerLevel.Lethal => Colors.DarkRed,
        _ => Colors.White
    };
}

public class PhaseHealthSegment
{
    public int PhaseNumber { get; set; }
    public bool IsCurrentPhase { get; set; }
    public bool IsCompleted { get; set; }
    public float HealthThreshold { get; set; }
}
```

### BossUIOverlay Component

```xml
<!-- Add to CombatView when IsBossFight is true -->

<Grid IsVisible="{Binding IsBossFight}" Grid.RowSpan="2" Grid.ColumnSpan="2">
    <!-- Boss Name & Title -->
    <Border Background="#E0000000"
            Height="80"
            VerticalAlignment="Top"
            Padding="20">
        <StackPanel VerticalAlignment="Center">
            <TextBlock Text="{Binding [CurrentBoss.Name](http://CurrentBoss.Name)}"
                       FontSize="32"
                       FontWeight="Bold"
                       Foreground="#DC143C"
                       HorizontalAlignment="Center"/>
            <TextBlock Text="{Binding CurrentBoss.Title}"
                       FontSize="14"
                       Foreground="#FFD700"
                       HorizontalAlignment="Center"/>
        </StackPanel>
    </Border>
    
    <!-- Phase Indicator -->
    <Border Background="#E0000000"
            Width="300"
            Height="60"
            VerticalAlignment="Top"
            HorizontalAlignment="Right"
            Margin="0,90,20,0"
            Padding="15"
            CornerRadius="10">
        <StackPanel>
            <TextBlock Text="Boss Phase"
                       Foreground="White"
                       FontSize="12"
                       HorizontalAlignment="Center"/>
            <Grid ColumnDefinitions="*,*,*,*" Height="30" Margin="0,5,0,0">
                <ItemsControl ItemsSource="{Binding PhaseSegments}">
                    <ItemsControl.ItemsPanel>
                        <ItemsPanelTemplate>
                            <UniformGrid Columns="{Binding TotalPhases}"/>
                        </ItemsPanelTemplate>
                    </ItemsControl.ItemsPanel>
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="{Binding IsCompleted, Converter={StaticResource PhaseSegmentColorConverter}}"
                                    BorderBrush="{Binding IsCurrentPhase, Converter={StaticResource CurrentPhaseBorderConverter}}"
                                    BorderThickness="{Binding IsCurrentPhase, Converter={StaticResource CurrentPhaseBorderThicknessConverter}}"
                                    Margin="2"
                                    CornerRadius="5">
                                <TextBlock Text="{Binding PhaseNumber}"
                                           Foreground="White"
                                           FontWeight="Bold"
                                           HorizontalAlignment="Center"
                                           VerticalAlignment="Center"/>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </Grid>
        </StackPanel>
    </Border>
    
    <!-- Enrage Timer -->
    <Border Background="#E08B0000"
            Width="250"
            Height="50"
            VerticalAlignment="Top"
            HorizontalAlignment="Left"
            Margin="20,90,0,0"
            Padding="10"
            CornerRadius="10"
            IsVisible="{Binding EnrageProgress, Converter={StaticResource GreaterThanZeroConverter}}">
        <StackPanel>
            <TextBlock Text="⚠️ ENRAGE WARNING"
                       Foreground="White"
                       FontWeight="Bold"
                       FontSize="12"
                       HorizontalAlignment="Center"/>
            <ProgressBar Value="{Binding EnrageProgress}"
                         Maximum="1.0"
                         Height="15"
                         Foreground="#DC143C"
                         Margin="0,5,0,0"/>
        </StackPanel>
    </Border>
    
    <!-- Mechanic Warnings -->
    <ItemsControl ItemsSource="{Binding ActiveWarnings}"
                  VerticalAlignment="Center"
                  HorizontalAlignment="Center"
                  Margin="0,0,0,100">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border Background="{Binding WarningColor, Converter={StaticResource ColorToSolidBrushConverter}}"
                        BorderBrush="White"
                        BorderThickness="3"
                        Padding="20"
                        Margin="0,10"
                        CornerRadius="10"
                        Width="400">
                    <StackPanel>
                        <TextBlock Text="{Binding MechanicName}"
                                   FontSize="24"
                                   FontWeight="Bold"
                                   Foreground="White"
                                   HorizontalAlignment="Center"/>
                        <TextBlock Text="{Binding Description}"
                                   FontSize="14"
                                   Foreground="White"
                                   TextWrapping="Wrap"
                                   HorizontalAlignment="Center"
                                   Margin="0,5,0,0"/>
                    </StackPanel>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
    
    <!-- Boss Health Bar (Segmented) -->
    <Border Background="#E0000000"
            Height="40"
            VerticalAlignment="Top"
            Margin="100,150,100,0"
            Padding="5"
            CornerRadius="10">
        <Grid>
            <!-- Background segments -->
            <ItemsControl ItemsSource="{Binding PhaseSegments}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <UniformGrid Columns="{Binding TotalPhases}"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border Background="#3C3C3C"
                                Margin="2"
                                CornerRadius="5"/>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
            
            <!-- Actual health bar -->
            <ProgressBar Value="{Binding CurrentBoss.CurrentHP}"
                         Maximum="{Binding CurrentBoss.MaxHP}"
                         Height="30"
                         Foreground="#DC143C"/>
        </Grid>
    </Border>
</Grid>
```

---

## Integration Points

**With v0.42 (Boss Enhancements):**

- Displays boss phases
- Shows mechanic warnings
- Tracks enrage timer

**With v0.43.4-v0.43.8 (Combat UI):**

- Overlays on combat grid
- Uses animation system

---

## Success Criteria

**v0.43.17 is DONE when:**

### ✅ Phase Display

- [ ]  Boss phases clearly indicated
- [ ]  Current phase highlighted
- [ ]  Phase transitions animated
- [ ]  Health segmented by phase

### ✅ Mechanic Warnings

- [ ]  Warnings appear before mechanics
- [ ]  Color-coded by danger
- [ ]  Clear descriptions
- [ ]  Auto-dismiss after duration

### ✅ Enrage Timer

- [ ]  Shows when enrage approaching
- [ ]  Visual warning
- [ ]  Progress bar accurate

---

**Boss UI complete. Ready for settings in v0.43.18.**