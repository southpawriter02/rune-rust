---
id: SPEC-UI-GUI-ADAPTER
title: "GUI Adapter — Avalonia Implementation Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.DesktopUI/"
    status: Planned
---

# GUI Adapter — Avalonia Implementation Specification

---

## 1. Overview

The GUI adapter implements the 4 core presentation interfaces using Avalonia UI with ReactiveUI.

| Interface | GUI Implementation | Pattern |
|-----------|-------------------|---------|
| `IPresenter` | `GuiPresenter` | Toast notifications, log panels |
| `IInputHandler` | `GuiInputHandler` | ReactiveCommand, keybindings |
| `IRenderTarget` | View + ViewModel | XAML + ReactiveUI |
| `IMapRenderer` | `MinimapControl` | Custom SkiaSharp control |

**Technology Stack:**
- Avalonia UI 11.x
- ReactiveUI 19.5+
- SkiaSharp (2D rendering)
- .NET 8.0

---

## 2. Project Structure

```
RuneAndRust.DesktopUI/
├── App.axaml                    ← Application entry
├── App.axaml.cs
├── Program.cs
├── ViewLocator.cs               ← ViewModel → View resolution
├── Services/
│   ├── GuiPresenter.cs          ← IPresenter implementation
│   ├── GuiInputHandler.cs       ← IInputHandler implementation
│   ├── SpriteService.cs         ← Sprite loading/caching
│   └── NavigationService.cs     ← View navigation
├── ViewModels/
│   ├── ViewModelBase.cs         ← ReactiveUI base
│   ├── MainWindowViewModel.cs
│   ├── CombatViewModel.cs
│   ├── CharacterSheetViewModel.cs
│   ├── DungeonExplorationViewModel.cs
│   ├── InventoryViewModel.cs
│   ├── DialogueViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   ├── CombatView.axaml
│   ├── CharacterSheetView.axaml
│   ├── DungeonExplorationView.axaml
│   ├── InventoryView.axaml
│   ├── DialogueView.axaml
│   └── SettingsView.axaml
├── Controls/
│   ├── CombatGridControl.cs     ← Custom SkiaSharp grid
│   ├── MinimapControl.cs        ← Dungeon minimap
│   ├── ResourceBar.axaml        ← HP/Stamina bars
│   ├── SmartCommandPanel.axaml  ← Action buttons
│   └── StatusEffectIcon.axaml
└── Resources/
    ├── Sprites/
    │   ├── player_sprites.json
    │   └── enemy_sprites.json
    ├── Styles/
    │   ├── Colors.axaml
    │   └── Controls.axaml
    └── Fonts/
```

---

## 3. GuiPresenter Implementation

### 3.1 Interface Implementation

```csharp
public class GuiPresenter : IPresenter
{
    private readonly INotificationService _notifications;
    private readonly ICombatLogService _combatLog;
    
    public void ShowMessage(string message, MessageType type)
    {
        _notifications.ShowToast(message, ToTypeToast(type));
    }
    
    public void ShowCombatLog(CombatLogEntry entry)
    {
        _combatLog.AddEntry(entry);
    }
    
    public void UpdateResource(ResourceType type, int current, int max)
    {
        // Publishes event for resource bar bindings
        MessageBus.Current.SendMessage(new ResourceUpdateMessage(type, current, max));
    }
    
    public void ShowStatusEffect(string effectName, StatusChange change)
    {
        _notifications.ShowStatusToast(effectName, change);
    }
    
    public async Task<int> PromptChoiceAsync(string prompt, IReadOnlyList<string> options)
    {
        return await _dialogService.ShowChoiceDialogAsync(prompt, options);
    }
    
    public async Task<string> PromptTextAsync(string prompt)
    {
        return await _dialogService.ShowInputDialogAsync(prompt);
    }
    
    public async Task<bool> ConfirmAsync(string message)
    {
        return await _dialogService.ShowConfirmDialogAsync(message);
    }
}
```

### 3.2 Toast Notifications

| Toast Type | Duration | Color | Icon |
|------------|----------|-------|------|
| Info | 3s | White | ℹ️ |
| Success | 3s | Green | ✓ |
| Warning | 5s | Yellow | ⚠️ |
| Error | 5s | Red | ✗ |
| Combat | 2s | Cyan | ⚔️ |
| Loot | 4s | Magenta | 💎 |
| Quest | 4s | Blue | 📜 |

---

## 4. GuiInputHandler Implementation

### 4.1 Interface Implementation

```csharp
public class GuiInputHandler : IInputHandler
{
    private readonly Dictionary<string, Action<string[]>> _commands = new();
    private readonly Dictionary<string, Action> _hotkeys = new();
    private InputContext _context = InputContext.MainMenu;
    
    public event Action<PlayerCommand>? OnCommandReceived;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // GUI doesn't need polling — events are reactive
        return Task.CompletedTask;
    }
    
    public void Stop() { }
    
    public void SetContext(InputContext context)
    {
        _context = context;
        // Update which hotkeys are active
        RefreshHotkeyBindings();
    }
    
    public void RegisterCommand(string verb, Action<string[]> handler)
    {
        _commands[verb.ToLowerInvariant()] = handler;
    }
    
    public void RegisterHotkey(string key, Action handler)
    {
        _hotkeys[key] = handler;
    }
}
```

### 4.2 Global Keyboard Shortcuts

| Key | Context | Action |
|-----|---------|--------|
| `Escape` | Any | Back/Cancel/Pause |
| `I` | Exploration | Open Inventory |
| `C` | Exploration | Open Character |
| `J` | Exploration | Open Journal |
| `M` | Exploration | Toggle Minimap |
| `1-9` | Combat | Execute Smart Command |
| `Space` | Combat | End Turn |
| `Tab` | Combat | Next Target |
| `F5` | Non-combat | Quick Save |
| `F9` | Non-combat | Quick Load |

---

## 5. ViewModelBase

### 5.1 Base Class

```csharp
public class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    
    protected ViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnActivated(disposables);
        });
    }
    
    protected virtual void OnActivated(CompositeDisposable disposables) { }
}
```

### 5.2 Observable Properties Pattern

```csharp
public class CombatViewModel : ViewModelBase
{
    private CombatDisplay? _combatDisplay;
    public CombatDisplay? CombatDisplay
    {
        get => _combatDisplay;
        private set => this.RaiseAndSetIfChanged(ref _combatDisplay, value);
    }
    
    private CombatantDisplay? _selectedUnit;
    public CombatantDisplay? SelectedUnit
    {
        get => _selectedUnit;
        set => this.RaiseAndSetIfChanged(ref _selectedUnit, value);
    }
    
    // Commands
    public ReactiveCommand<Unit, Unit> AttackCommand { get; }
    public ReactiveCommand<Unit, Unit> DefendCommand { get; }
    public ReactiveCommand<GridPosition, Unit> CellClickedCommand { get; }
}
```

---

## 6. CombatViewModel

### 6.1 Properties

| Property | Type | Description |
|----------|------|-------------|
| `CombatDisplay` | `CombatDisplay` | Full combat state |
| `SelectedUnit` | `CombatantDisplay?` | Currently selected |
| `TargetingMode` | `TargetingMode` | None/Attack/Move/Ability |
| `StatusMessage` | `string` | Feedback message |
| `IsPlayerTurn` | `bool` | Can player act |

### 6.2 Commands

| Command | Parameter | Behavior |
|---------|-----------|----------|
| `AttackCommand` | — | Enter attack targeting |
| `DefendCommand` | — | Take defensive stance |
| `UseAbilityCommand` | `string` abilityId | Use specific ability |
| `MoveCommand` | — | Enter movement targeting |
| `FleeCommand` | — | Attempt escape |
| `EndTurnCommand` | — | Pass turn |
| `CellClickedCommand` | `GridPosition` | Select/confirm target |
| `SmartCommand` | `int` index | Execute smart command |

### 6.3 Event Subscriptions

```csharp
protected override void OnActivated(CompositeDisposable disposables)
{
    _eventBus.Subscribe<CombatStateChangedEvent>(OnCombatStateChanged)
        .DisposeWith(disposables);
    
    _eventBus.Subscribe<TurnAdvancedEvent>(OnTurnAdvanced)
        .DisposeWith(disposables);
    
    _eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt)
        .DisposeWith(disposables);
}

private void OnCombatStateChanged(CombatStateChangedEvent evt)
{
    CombatDisplay = _displayFactory.CreateCombatDisplay(evt.State);
}
```

---

## 7. Combat Grid Control

### 7.1 Custom Control

```csharp
public class CombatGridControl : Control
{
    // Dependency Properties
    public static readonly StyledProperty<GridDisplay?> GridProperty;
    public static readonly StyledProperty<GridPosition?> SelectedCellProperty;
    
    // Events
    public event EventHandler<GridPosition>? CellClicked;
    public event EventHandler<GridPosition>? CellHovered;
    
    public override void Render(DrawingContext context)
    {
        if (Grid == null) return;
        
        RenderCells(context);
        RenderOccupants(context);
        RenderHighlights(context);
        RenderSelection(context);
    }
}
```

### 7.2 Visual Specifications

| Element | Size | Color |
|---------|------|-------|
| Cell | 80×80 px | Checkerboard pattern |
| Player Zone | Rows 0-1 | Blue gradient |
| Enemy Zone | Rows 2-3 | Red gradient |
| Selected Border | 3px | Gold (#FFD700) |
| Hovered Border | 2px | Light Blue (#87CEEB) |
| Valid Target | Fill | Green (#4CAF50) 30% |
| Attack Target | Fill | Red (#DC143C) 30% |

### 7.3 Sprite Rendering

```csharp
private void RenderOccupant(DrawingContext context, GridCellDisplay cell, CombatantDisplay occupant)
{
    var sprite = _spriteService.GetSprite(occupant.SpriteKey);
    var rect = GetCellRect(cell.Position);
    
    // Render sprite at 3x scale (48x48 from 16x16)
    var spriteRect = new Rect(
        rect.X + (rect.Width - 48) / 2,
        rect.Y + 8,
        48, 48
    );
    context.DrawImage(sprite, spriteRect);
    
    // Render HP bar below sprite
    RenderHPBar(context, rect, occupant);
    
    // Render status effect icons
    RenderStatusIcons(context, rect, occupant.StatusEffects);
}
```

---

## 8. Resource Bars

### 8.1 ResourceBar Control

```xml
<UserControl x:Class="RuneAndRust.DesktopUI.Controls.ResourceBar">
    <Grid>
        <Border Background="{Binding BackgroundColor}" 
                CornerRadius="3" />
        <Border Background="{Binding FillColor}"
                Width="{Binding FillWidth}"
                HorizontalAlignment="Left"
                CornerRadius="3" />
        <TextBlock Text="{Binding Display}"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center" />
    </Grid>
</UserControl>
```

### 8.2 Resource Bar Colors

| Resource | Background | Fill | Low (<25%) |
|----------|------------|------|------------|
| HP | #3D0000 | #DC143C | Pulse animation |
| Stamina | #003D00 | #4CAF50 | — |
| Aether | #1A0033 | #9400D3 | — |
| Stress | #3D1A00 | #FF8C00 | Shake effect |

---

## 9. Smart Command Panel

### 9.1 Panel Layout

```xml
<ItemsControl ItemsSource="{Binding AvailableActions}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Button Command="{Binding ExecuteCommand}"
                    Classes="smart-command">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding HotkeyDisplay}" 
                               Classes="hotkey-badge" />
                    <TextBlock Text="{Binding DisplayText}" />
                    <TextBlock Text="{Binding SkillCheckDisplay}"
                               Classes="skill-hint" />
                </StackPanel>
            </Button>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 9.2 Command Styling

| Element | Style |
|---------|-------|
| Hotkey Badge | Blue circle with number |
| Command Text | White, 14pt |
| Skill Hint | Attribute color, italic |
| Cost Display | Yellow if Stamina, Purple if AP |
| Risk Warning | Red ⚠ icon if fumble >20% |

---

## 10. Minimap Control

### 10.1 Rendering

```csharp
public class MinimapControl : Control
{
    public static readonly StyledProperty<MinimapDisplay?> MapProperty;
    
    public override void Render(DrawingContext context)
    {
        foreach (var room in Map.Rooms)
        {
            var rect = GetRoomRect(room.Position);
            var color = room.IsCurrentRoom ? CurrentRoomColor
                      : room.IsVisited ? VisitedRoomColor
                      : FogColor;
            
            context.DrawRectangle(color, null, rect);
            
            // Draw connections
            foreach (var exit in room.Exits)
            {
                DrawConnection(context, room.Position, exit.Direction);
            }
        }
    }
}
```

### 10.2 Minimap Colors

| State | Color |
|-------|-------|
| Current Room | Gold (#FFD700) |
| Visited | Gray (#4A4A4A) |
| Unvisited | Black (fog) |
| Connections | Dark Gray (#2A2A2A) |
| Vertical Connection | Blue (#4A90E2) |

---

## 11. Theming & Styling

### 11.1 Color Palette

```xml
<!-- Colors.axaml -->
<Color x:Key="BackgroundDark">#1C1C1C</Color>
<Color x:Key="BackgroundMedium">#2D2D2D</Color>
<Color x:Key="BackgroundLight">#3D3D3D</Color>
<Color x:Key="AccentPrimary">#4A90E2</Color>
<Color x:Key="AccentSecondary">#FFD700</Color>
<Color x:Key="TextPrimary">#FFFFFF</Color>
<Color x:Key="TextSecondary">#B0B0B0</Color>
<Color x:Key="DangerRed">#DC143C</Color>
<Color x:Key="SuccessGreen">#4CAF50</Color>
<Color x:Key="WarningYellow">#FFD700</Color>
```

### 11.2 Typography

| Usage | Font | Size | Weight |
|-------|------|------|--------|
| Title | Default | 24pt | Bold |
| Header | Default | 18pt | SemiBold |
| Body | Default | 14pt | Normal |
| Label | Default | 12pt | Normal |
| Hotkey | Monospace | 12pt | Bold |

---

## 12. Navigation

### 12.1 View Navigation

```csharp
public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _mainWindow;
    
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = Locator.Current.GetService<TViewModel>();
        _mainWindow.CurrentView = vm;
    }
    
    public void NavigateBack()
    {
        _navigationStack.Pop();
        _mainWindow.CurrentView = _navigationStack.Peek();
    }
}
```

### 12.2 View Flow

```
MainMenu
  ├── NewGame → CharacterCreation → DungeonExploration
  ├── Continue → DungeonExploration
  ├── LoadGame → SaveLoadView → DungeonExploration
  └── Settings → SettingsView

DungeonExploration
  ├── Combat (trigger) → CombatView → Victory/Defeat → DungeonExploration
  ├── NPC (talk) → DialogueView → DungeonExploration
  ├── Character (C) → CharacterSheetView
  ├── Inventory (I) → InventoryView
  └── Minimap (M) → Toggle overlay
```

---

## 13. Accessibility

### 13.1 Colorblind Modes

| Mode | Original | Replacement |
|------|----------|-------------|
| Protanopia | Red | Orange/Yellow |
| Deuteranopia | Green | Blue/Cyan |
| Tritanopia | Blue | Pink/Magenta |
| High Contrast | All | Black/White/Yellow |

### 13.2 Screen Reader Support

- All interactive elements have `AutomationProperties.Name`
- Combat log entries announced
- Focus indicators visible
- Keyboard navigation complete

---

## 14. Implementation Status

| Component | File Path | Status |
|-----------|-----------|--------|
| GuiPresenter | `Services/GuiPresenter.cs` | ❌ Planned |
| GuiInputHandler | `Services/GuiInputHandler.cs` | ❌ Planned |
| CombatViewModel | `ViewModels/CombatViewModel.cs` | ❌ Planned |
| CombatView | `Views/CombatView.axaml` | ❌ Planned |
| CombatGridControl | `Controls/CombatGridControl.cs` | ❌ Planned |
| MinimapControl | `Controls/MinimapControl.cs` | ❌ Planned |
| SpriteService | `Services/SpriteService.cs` | ❌ Planned |
| NavigationService | `Services/NavigationService.cs` | ❌ Planned |
