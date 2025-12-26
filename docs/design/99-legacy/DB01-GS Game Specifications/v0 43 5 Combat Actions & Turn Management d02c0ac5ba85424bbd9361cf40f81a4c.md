# v0.43.5: Combat Actions & Turn Management

Type: UI
Description: Combat action system: action menu panel with buttons, target selection workflow, turn order display, combat log panel, action execution through CombatEngine, and end turn functionality. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.4, v0.1 (Combat System), v0.15 (Action System)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.4, v0.1 (Combat System), v0.15 (Action System)

**Estimated Time:** 6-8 hours

**Group:** Combat UI

**Deliverable:** Complete combat action menu and turn management UI

---

## Executive Summary

v0.43.5 implements the combat action system, allowing players to select and execute actions (Attack, Defend, Ability, Item, Move) through the GUI. Integrates with the existing CombatEngine from v0.1 and displays turn order.

**What This Delivers:**

- Action menu panel with buttons
- Target selection workflow
- Turn order display
- Combat log panel
- Action execution through CombatEngine
- End turn functionality

**Success Metric:** Can execute all combat action types through GUI with proper targeting and feedback.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### Enhanced CombatViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Combat;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class CombatViewModel : ViewModelBase
{
    private readonly ICombatEngine _combatEngine;
    private readonly IDialogService _dialogService;
    private CombatState? _currentCombat;
    private BattlefieldGrid? _grid;
    private ObservableCollection<GridPosition> _highlightedCells = new();
    private ObservableCollection<string> _combatLog = new();
    private TargetingMode _targetingMode = TargetingMode.None;
    private CombatAction? _pendingAction;
    
    public BattlefieldGrid? Grid
    {
        get => _grid;
        private set => this.RaiseAndSetIfChanged(ref _grid, value);
    }
    
    public ObservableCollection<GridPosition> HighlightedCells
    {
        get => _highlightedCells;
        set => this.RaiseAndSetIfChanged(ref _highlightedCells, value);
    }
    
    public ObservableCollection<string> CombatLog
    {
        get => _combatLog;
        set => this.RaiseAndSetIfChanged(ref _combatLog, value);
    }
    
    public ObservableCollection<TurnOrderEntry> TurnOrder { get; } = new();
    
    public bool IsPlayerTurn => _currentCombat?.CurrentPhase == CombatPhase.PlayerTurn;
    
    // Commands
    public ICommand AttackCommand { get; }
    public ICommand DefendCommand { get; }
    public ICommand UseAbilityCommand { get; }
    public ICommand UseItemCommand { get; }
    public ICommand MoveCommand { get; }
    public ICommand EndTurnCommand { get; }
    public ICommand FleeCommand { get; }
    
    public CombatViewModel(
        ICombatEngine combatEngine,
        IDialogService dialogService)
    {
        _combatEngine = combatEngine;
        _dialogService = dialogService;
        
        // Initialize commands
        AttackCommand = ReactiveCommand.CreateFromTask(AttackAsync,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        DefendCommand = ReactiveCommand.Create(Defend,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        UseAbilityCommand = ReactiveCommand.CreateFromTask(UseAbilityAsync,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        UseItemCommand = ReactiveCommand.CreateFromTask(UseItemAsync,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        MoveCommand = ReactiveCommand.Create(StartMove,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        EndTurnCommand = ReactiveCommand.CreateFromTask(EndTurnAsync,
            this.WhenAnyValue(x => x.IsPlayerTurn));
        FleeCommand = ReactiveCommand.CreateFromTask(FleeAsync,
            this.WhenAnyValue(x => x.IsPlayerTurn));
    }
    
    public void LoadCombatState(CombatState state)
    {
        _currentCombat = state;
        Grid = state.BattlefieldGrid;
        UpdateTurnOrder();
        this.RaisePropertyChanged(nameof(IsPlayerTurn));
    }
    
    public async Task HandleCellClickAsync(GridPosition position)
    {
        if (_targetingMode == TargetingMode.None) return;
        
        switch (_targetingMode)
        {
            case TargetingMode.AttackTarget:
                await ExecuteAttackAsync(position);
                break;
            case TargetingMode.MovementTarget:
                await ExecuteMoveAsync(position);
                break;
            case TargetingMode.AbilityTarget:
                await ExecuteAbilityAsync(position);
                break;
        }
        
        ClearTargeting();
    }
    
    private async Task AttackAsync()
    {
        _targetingMode = TargetingMode.AttackTarget;
        var player = _currentCombat!.PlayerCharacter;
        var attackRange = _combatEngine.GetAttackRange(player);
        
        HighlightedCells.Clear();
        foreach (var pos in attackRange)
        {
            HighlightedCells.Add(pos);
        }
        
        AddToCombatLog("Select target to attack...");
    }
    
    private async Task ExecuteAttackAsync(GridPosition targetPos)
    {
        var player = _currentCombat!.PlayerCharacter;
        var target = Grid!.GetTileAt(targetPos)?.Occupant;
        
        if (target == null)
        {
            AddToCombatLog("No target at that position.");
            return;
        }
        
        var action = new AttackAction
        {
            ActorId = [player.Id](http://player.Id),
            TargetId = [target.Id](http://target.Id)
        };
        
        var result = await _combatEngine.ProcessPlayerAction(action);
        AddToCombatLog(result.Message);
        
        await ProcessCombatResult(result);
    }
    
    private void Defend()
    {
        var player = _currentCombat!.PlayerCharacter;
        var action = new DefendAction { ActorId = [player.Id](http://player.Id) };
        
        var result = _combatEngine.ProcessPlayerAction(action).Result;
        AddToCombatLog(result.Message);
    }
    
    private void StartMove()
    {
        _targetingMode = TargetingMode.MovementTarget;
        var player = _currentCombat!.PlayerCharacter;
        var moveRange = _combatEngine.GetValidMovementPositions(player);
        
        HighlightedCells.Clear();
        foreach (var pos in moveRange)
        {
            HighlightedCells.Add(pos);
        }
        
        AddToCombatLog("Select position to move to...");
    }
    
    private async Task ExecuteMoveAsync(GridPosition targetPos)
    {
        var player = _currentCombat!.PlayerCharacter;
        var action = new MoveAction
        {
            ActorId = [player.Id](http://player.Id),
            TargetPosition = targetPos
        };
        
        var result = await _combatEngine.ProcessPlayerAction(action);
        AddToCombatLog(result.Message);
        
        await ProcessCombatResult(result);
    }
    
    private async Task UseAbilityAsync()
    {
        // Show ability selection dialog
        var player = _currentCombat!.PlayerCharacter;
        var abilities = player.Abilities;
        
        // TODO: Ability selection dialog in v0.43.6
        AddToCombatLog("Ability selection not yet implemented.");
    }
    
    private async Task UseItemAsync()
    {
        // Show item selection dialog
        // TODO: Item selection in v0.43.10
        AddToCombatLog("Item usage not yet implemented.");
    }
    
    private async Task EndTurnAsync()
    {
        AddToCombatLog("Ending turn...");
        await _combatEngine.EndPlayerTurnAsync();
        await ProcessEnemyTurnsAsync();
    }
    
    private async Task FleeAsync()
    {
        var confirm = await _dialogService.ShowConfirmationAsync(
            "Flee Combat",
            "Are you sure you want to flee? You may take damage.");
        
        if (confirm)
        {
            var result = await _combatEngine.AttemptFleeAsync();
            AddToCombatLog(result.Message);
        }
    }
    
    private async Task ProcessEnemyTurnsAsync()
    {
        while (_currentCombat!.CurrentPhase == CombatPhase.EnemyTurn)
        {
            var result = await _combatEngine.ProcessEnemyTurnAsync();
            AddToCombatLog(result.Message);
            
            // Delay for visual feedback
            await Task.Delay(500);
        }
        
        this.RaisePropertyChanged(nameof(IsPlayerTurn));
    }
    
    private async Task ProcessCombatResult(CombatActionResult result)
    {
        if (result.CombatEnded)
        {
            await HandleCombatEndAsync(result);
        }
        else
        {
            LoadCombatState(_currentCombat!);
        }
    }
    
    private async Task HandleCombatEndAsync(CombatActionResult result)
    {
        if (result.PlayerVictory)
        {
            await _dialogService.ShowMessageAsync("Victory!", "You have defeated your enemies.");
        }
        else
        {
            await _dialogService.ShowMessageAsync("Defeat", "You have been defeated.");
        }
        
        // Return to dungeon exploration
    }
    
    private void UpdateTurnOrder()
    {
        TurnOrder.Clear();
        if (_currentCombat == null) return;
        
        var ordered = _currentCombat.GetTurnOrder();
        foreach (var combatant in ordered)
        {
            TurnOrder.Add(new TurnOrderEntry
            {
                Name = [combatant.Name](http://combatant.Name),
                IsPlayer = combatant is PlayerCharacter,
                IsActive = combatant == _currentCombat.CurrentActor
            });
        }
    }
    
    private void ClearTargeting()
    {
        _targetingMode = TargetingMode.None;
        HighlightedCells.Clear();
    }
    
    private void AddToCombatLog(string message)
    {
        CombatLog.Insert(0, $"[{[DateTime.Now](http://DateTime.Now):HH:mm:ss}] {message}");
        if (CombatLog.Count > 100)
            CombatLog.RemoveAt(CombatLog.Count - 1);
    }
}

public enum TargetingMode
{
    None,
    AttackTarget,
    MovementTarget,
    AbilityTarget,
    ItemTarget
}

public class TurnOrderEntry
{
    public string Name { get; set; } = "";
    public bool IsPlayer { get; set; }
    public bool IsActive { get; set; }
}
```

---

## CombatView XAML

```xml
<UserControl xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             xmlns:controls="using:RuneAndRust.DesktopUI.Controls"
             x:Class="RuneAndRust.DesktopUI.Views.CombatView"
             x:DataType="vm:CombatViewModel">
    
    <Grid ColumnDefinitions="*,250" RowDefinitions="*,200">
        <!-- Combat Grid -->
        <controls:CombatGridControl Grid.Row="0" Grid.Column="0"
                                    Grid="{Binding Grid}"
                                    HighlightedCells="{Binding HighlightedCells}"
                                    CellClicked="OnCellClicked"
                                    Margin="20"/>
        
        <!-- Turn Order -->
        <Border Grid.Row="0" Grid.Column="1"
                Background="#1C1C1C"
                Margin="0,20,20,10"
                Padding="10">
            <StackPanel>
                <TextBlock Text="Turn Order"
                           FontSize="16"
                           FontWeight="Bold"
                           Foreground="White"
                           Margin="0,0,0,10"/>
                
                <ItemsControl ItemsSource="{Binding TurnOrder}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="{Binding IsActive, Converter={StaticResource ActiveTurnBackgroundConverter}}"
                                    Padding="5"
                                    Margin="0,2">
                                <TextBlock Text="{Binding Name}"
                                           Foreground="{Binding IsPlayer, Converter={StaticResource PlayerColorConverter}}"/>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </Border>
        
        <!-- Action Menu -->
        <Border Grid.Row="1" Grid.Column="0"
                Background="#1C1C1C"
                Margin="20,10,10,20"
                Padding="10">
            <StackPanel Orientation="Horizontal" Spacing="10">
                <Button Command="{Binding AttackCommand}"
                        Content="⚔️ Attack"
                        HotKey="D1"
                        Width="120"/>
                
                <Button Command="{Binding DefendCommand}"
                        Content="🛡️ Defend"
                        HotKey="D2"
                        Width="120"/>
                
                <Button Command="{Binding UseAbilityCommand}"
                        Content="✨ Ability"
                        HotKey="D3"
                        Width="120"/>
                
                <Button Command="{Binding UseItemCommand}"
                        Content="🧪 Item"
                        HotKey="D4"
                        Width="120"/>
                
                <Button Command="{Binding MoveCommand}"
                        Content="👣 Move"
                        HotKey="D5"
                        Width="120"/>
                
                <Button Command="{Binding EndTurnCommand}"
                        Content="⏭️ End Turn"
                        HotKey="Space"
                        Width="120"/>
                
                <Button Command="{Binding FleeCommand}"
                        Content="🏃 Flee"
                        HotKey="F"
                        Width="120"/>
            </StackPanel>
        </Border>
        
        <!-- Combat Log -->
        <Border Grid.Row="1" Grid.Column="1"
                Background="#1C1C1C"
                Margin="0,10,20,20"
                Padding="10">
            <ScrollViewer>
                <ItemsControl ItemsSource="{Binding CombatLog}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <TextBlock Text="{Binding}"
                                       Foreground="#CCCCCC"
                                       TextWrapping="Wrap"
                                       Margin="0,2"/>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </Border>
    </Grid>
</UserControl>
```

---

## Integration Points

**With v0.1 (Combat System):**

- Calls `CombatEngine.ProcessPlayerAction()`
- Uses `CombatActionResult` for feedback
- Respects combat phases

**With v0.15 (Action System):**

- Creates `AttackAction`, `DefendAction`, `MoveAction`
- Uses action validation from Engine

**With v0.43.4 (Grid Control):**

- Handles cell click events
- Updates highlighted cells for targeting

---

## Success Criteria

**v0.43.5 is DONE when:**

### ✅ Action Menu

- [ ]  All 7 actions have buttons
- [ ]  Buttons enabled only on player turn
- [ ]  Keyboard shortcuts work
- [ ]  Visual feedback on click

### ✅ Action Execution

- [ ]  Attack action with targeting
- [ ]  Defend action immediate
- [ ]  Move action with targeting
- [ ]  End turn progresses combat
- [ ]  Flee with confirmation

### ✅ Combat Flow

- [ ]  Player turn → Enemy turn cycles
- [ ]  Turn order displays correctly
- [ ]  Combat log shows messages
- [ ]  Victory/defeat handled

---

**Combat actions complete. Ready for status effects in v0.43.6.**