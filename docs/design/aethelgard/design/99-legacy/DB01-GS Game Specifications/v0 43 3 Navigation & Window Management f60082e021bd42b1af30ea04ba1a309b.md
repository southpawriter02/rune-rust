# v0.43.3: Navigation & Window Management

Type: Technical
Description: Completes foundation layer: NavigationService implementation, view registration system, MainWindow layout with sidebar, keyboard shortcut system, and view transitions. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1, v0.43.2
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1, v0.43.2

**Estimated Time:** 5-7 hours

**Group:** Foundation

**Deliverable:** Complete navigation system with keyboard shortcuts

---

## Executive Summary

v0.43.3 completes the foundation layer by implementing the navigation system, creating the main window layout with sidebar navigation, setting up view registration and resolution, and adding comprehensive keyboard shortcut handling.

**What This Delivers:**

- Complete `NavigationService` implementation
- View registration system
- MainWindow layout with sidebar
- Keyboard shortcut system
- View transitions with animations (optional)
- Main menu implementation

**Success Metric:** Can navigate between all major views using mouse or keyboard shortcuts.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### Complete NavigationService

```csharp
using ReactiveUI;
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;
using System.Collections.Concurrent;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface INavigationService
{
    ViewModelBase? CurrentView { get; }
    void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateBack();
    bool CanNavigateBack { get; }
    event EventHandler<ViewModelBase>? CurrentViewChanged;
    void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory) where TViewModel : ViewModelBase;
}

public class NavigationService : ReactiveObject, INavigationService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Type, Func<ViewModelBase>> _viewModelFactories = new();
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private ViewModelBase? _currentView;
    
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView != value)
            {
                _currentView = value;
                this.RaisePropertyChanged();
                CurrentViewChanged?.Invoke(this, value!);
                _logger.Information("Navigated to {ViewModelType}", value?.GetType().Name);
            }
        }
    }
    
    public bool CanNavigateBack => _navigationStack.Count > 0;
    
    public event EventHandler<ViewModelBase>? CurrentViewChanged;
    
    public NavigationService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory) 
        where TViewModel : ViewModelBase
    {
        _viewModelFactories[typeof(TViewModel)] = () => factory();
    }
    
    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        if (CurrentView != null)
        {
            _navigationStack.Push(CurrentView);
        }
        
        CurrentView = viewModel;
    }
    
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        if (!_viewModelFactories.TryGetValue(typeof(TViewModel), out var factory))
        {
            throw new InvalidOperationException(
                $"No factory registered for {typeof(TViewModel).Name}");
        }
        
        var viewModel = factory();
        NavigateTo((TViewModel)viewModel);
    }
    
    public void NavigateBack()
    {
        if (!CanNavigateBack)
        {
            _logger.Warning("Cannot navigate back: navigation stack is empty");
            return;
        }
        
        CurrentView = _navigationStack.Pop();
    }
}
```

### MainWindowViewModel (Enhanced)

```csharp
using ReactiveUI;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private ViewModelBase? _currentView;
    
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }
    
    public ICommand NavigateToMenuCommand { get; }
    public ICommand NavigateToCombatCommand { get; }
    public ICommand NavigateToCharacterCommand { get; }
    public ICommand NavigateToDungeonCommand { get; }
    public ICommand NavigateToInventoryCommand { get; }
    public ICommand NavigateToMapCommand { get; }
    public ICommand NavigateBackCommand { get; }
    
    public MainWindowViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        // Register view factories
        _navigationService.RegisterViewModelFactory(() => 
            serviceProvider.GetRequiredService<MenuViewModel>());
        _navigationService.RegisterViewModelFactory(() => 
            serviceProvider.GetRequiredService<CombatViewModel>());
        _navigationService.RegisterViewModelFactory(() => 
            serviceProvider.GetRequiredService<CharacterSheetViewModel>());
        _navigationService.RegisterViewModelFactory(() => 
            serviceProvider.GetRequiredService<DungeonExplorationViewModel>());
        _navigationService.RegisterViewModelFactory(() => 
            serviceProvider.GetRequiredService<InventoryViewModel>());
        
        // Navigation commands
        NavigateToMenuCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<MenuViewModel>());
        NavigateToCombatCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<CombatViewModel>());
        NavigateToCharacterCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<CharacterSheetViewModel>());
        NavigateToDungeonCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<DungeonExplorationViewModel>());
        NavigateToInventoryCommand = ReactiveCommand.Create(() => 
            _navigationService.NavigateTo<InventoryViewModel>());
        NavigateBackCommand = ReactiveCommand.Create(
            () => _navigationService.NavigateBack(),
            this.WhenAnyValue(x => x._navigationService.CanNavigateBack));
        
        // Start at menu
        _navigationService.NavigateTo<MenuViewModel>();
    }
    
    private void OnCurrentViewChanged(object? sender, ViewModelBase view)
    {
        CurrentView = view;
    }
}
```

### KeyboardShortcutService

```csharp
using Avalonia.Input;
using Serilog;
using System.Collections.Concurrent;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface IKeyboardShortcutService
{
    void RegisterShortcut(Key key, KeyModifiers modifiers, Action action, string description);
    void RegisterShortcut(Key key, Action action, string description);
    bool HandleKeyPress(Key key, KeyModifiers modifiers);
    IEnumerable<(Key Key, KeyModifiers Modifiers, string Description)> GetRegisteredShortcuts();
}

public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<(Key, KeyModifiers), (Action Action, string Description)> _shortcuts = new();
    
    public KeyboardShortcutService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void RegisterShortcut(Key key, KeyModifiers modifiers, Action action, string description)
    {
        _shortcuts[(key, modifiers)] = (action, description);
        _logger.Debug("Registered shortcut: {Key}+{Modifiers} - {Description}", 
            key, modifiers, description);
    }
    
    public void RegisterShortcut(Key key, Action action, string description)
    {
        RegisterShortcut(key, KeyModifiers.None, action, description);
    }
    
    public bool HandleKeyPress(Key key, KeyModifiers modifiers)
    {
        if (_shortcuts.TryGetValue((key, modifiers), out var shortcut))
        {
            _logger.Debug("Executing shortcut: {Key}+{Modifiers}", key, modifiers);
            shortcut.Action();
            return true;
        }
        
        return false;
    }
    
    public IEnumerable<(Key Key, KeyModifiers Modifiers, string Description)> GetRegisteredShortcuts()
    {
        return _[shortcuts.Select](http://shortcuts.Select)(kvp => (kvp.Key.Item1, kvp.Key.Item2, kvp.Value.Description));
    }
}
```

---

## MainWindow XAML Layout

```xml
<Window xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
        xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
        xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
        xmlns:views="using:RuneAndRust.DesktopUI.Views"
        x:Class="RuneAndRust.DesktopUI.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Icon="/Assets/icon.ico"
        Title="Rune & Rust"
        Width="1280"
        Height="720"
        MinWidth="1024"
        MinHeight="576">
    
    <Grid ColumnDefinitions="200,*">
        <!-- Sidebar Navigation -->
        <Border Grid.Column="0" 
                Background="#2C2C2C"
                BorderBrush="#3C3C3C"
                BorderThickness="0,0,1,0">
            <StackPanel Margin="10">
                <TextBlock Text="RUNE &amp; RUST" 
                           FontSize="18"
                           FontWeight="Bold"
                           Foreground="#FFD700"
                           HorizontalAlignment="Center"
                           Margin="0,10"/>
                
                <Separator Margin="0,10" Background="#3C3C3C"/>
                
                <!-- Navigation Buttons -->
                <Button Content="📜 Main Menu" 
                        Command="{Binding NavigateToMenuCommand}"
                        HotKey="Escape"
                        Margin="0,5"/>
                
                <Button Content="⚔️ Combat" 
                        Command="{Binding NavigateToCombatCommand}"
                        HotKey="F1"
                        Margin="0,5"/>
                
                <Button Content="🗺️ Dungeon" 
                        Command="{Binding NavigateToDungeonCommand}"
                        HotKey="M"
                        Margin="0,5"/>
                
                <Button Content="📊 Character" 
                        Command="{Binding NavigateToCharacterCommand}"
                        HotKey="C"
                        Margin="0,5"/>
                
                <Button Content="🎒 Inventory" 
                        Command="{Binding NavigateToInventoryCommand}"
                        HotKey="I"
                        Margin="0,5"/>
                
                <Separator Margin="0,10" Background="#3C3C3C"/>
                
                <Button Content="⬅️ Back" 
                        Command="{Binding NavigateBackCommand}"
                        HotKey="Backspace"
                        Margin="0,5"/>
            </StackPanel>
        </Border>
        
        <!-- Content Area -->
        <ContentControl Grid.Column="1"
                        Content="{Binding CurrentView}">
            <ContentControl.DataTemplates>
                <DataTemplate DataType="vm:MenuViewModel">
                    <views:MenuView/>
                </DataTemplate>
                <DataTemplate DataType="vm:CombatViewModel">
                    <views:CombatView/>
                </DataTemplate>
                <DataTemplate DataType="vm:CharacterSheetViewModel">
                    <views:CharacterSheetView/>
                </DataTemplate>
                <DataTemplate DataType="vm:DungeonExplorationViewModel">
                    <views:DungeonExplorationView/>
                </DataTemplate>
                <DataTemplate DataType="vm:InventoryViewModel">
                    <views:InventoryView/>
                </DataTemplate>
            </ContentControl.DataTemplates>
        </ContentControl>
    </Grid>
</Window>
```

---

## Integration Points

**With v0.43.1:**

- Uses DI container for ViewModel resolution
- Extends ViewModelBase

**With v0.43.2:**

- Sidebar icons can use sprite system
- Menu backgrounds can use sprite system

**With v0.43.4+ (All Future Views):**

- Register new ViewModels in MainWindowViewModel
- Add DataTemplate for new views
- Add navigation buttons if needed

---

## Functional Requirements

### FR1: View Registration

**Requirement:** Register ViewModels with factory functions.

**Test:**

```csharp
[Fact]
public void NavigationService_RegistersViewModelFactory()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new NavigationService(logger);
    
    service.RegisterViewModelFactory(() => new TestViewModel());
    
    service.NavigateTo<TestViewModel>();
    
    Assert.NotNull(service.CurrentView);
    Assert.IsType<TestViewModel>(service.CurrentView);
}
```

### FR2: Navigation

**Requirement:** Navigate between views programmatically.

**Test:**

```csharp
[Fact]
public void NavigationService_NavigatesToView()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new NavigationService(logger);
    
    var viewModel1 = new TestViewModel();
    var viewModel2 = new TestViewModel();
    
    service.NavigateTo(viewModel1);
    Assert.Same(viewModel1, service.CurrentView);
    
    service.NavigateTo(viewModel2);
    Assert.Same(viewModel2, service.CurrentView);
}

[Fact]
public void NavigationService_FiresCurrentViewChangedEvent()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new NavigationService(logger);
    
    ViewModelBase? changedView = null;
    service.CurrentViewChanged += (s, vm) => changedView = vm;
    
    var viewModel = new TestViewModel();
    service.NavigateTo(viewModel);
    
    Assert.Same(viewModel, changedView);
}
```

### FR3: Navigation Stack

**Requirement:** Support back navigation with stack.

**Test:**

```csharp
[Fact]
public void NavigationService_SupportsBackNavigation()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new NavigationService(logger);
    
    var view1 = new TestViewModel();
    var view2 = new TestViewModel();
    var view3 = new TestViewModel();
    
    service.NavigateTo(view1);
    service.NavigateTo(view2);
    service.NavigateTo(view3);
    
    Assert.Same(view3, service.CurrentView);
    Assert.True(service.CanNavigateBack);
    
    service.NavigateBack();
    Assert.Same(view2, service.CurrentView);
    
    service.NavigateBack();
    Assert.Same(view1, service.CurrentView);
    
    Assert.False(service.CanNavigateBack);
}
```

### FR4: Keyboard Shortcuts

**Requirement:** Handle keyboard shortcuts for navigation.

**Test:**

```csharp
[Fact]
public void KeyboardShortcutService_RegistersAndExecutesShortcuts()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new KeyboardShortcutService(logger);
    
    bool executed = false;
    service.RegisterShortcut(Key.C, () => executed = true, "Character sheet");
    
    var handled = service.HandleKeyPress(Key.C, KeyModifiers.None);
    
    Assert.True(handled);
    Assert.True(executed);
}

[Fact]
public void KeyboardShortcutService_HandlesModifiers()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new KeyboardShortcutService(logger);
    
    bool ctrlCExecuted = false;
    bool cExecuted = false;
    
    service.RegisterShortcut(Key.C, KeyModifiers.Control, () => ctrlCExecuted = true, "Copy");
    service.RegisterShortcut(Key.C, () => cExecuted = true, "Character");
    
    service.HandleKeyPress(Key.C, KeyModifiers.Control);
    Assert.True(ctrlCExecuted);
    Assert.False(cExecuted);
    
    service.HandleKeyPress(Key.C, KeyModifiers.None);
    Assert.True(cExecuted);
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**NavigationService Tests:**

- View registration
- Navigation between views
- Back navigation with stack
- Event firing
- Error handling for unregistered views

**KeyboardShortcutService Tests:**

- Shortcut registration
- Shortcut execution
- Modifier handling
- Multiple shortcuts for same key with different modifiers

### Integration Tests

**End-to-End Navigation:**

```csharp
[Fact]
public async Task MainWindow_NavigatesBetweenViews()
{
    // Requires Avalonia UI testing framework
    // Implementation in v0.43.21
}
```

---

## Success Criteria

**v0.43.3 is DONE when:**

### ✅ Navigation System

- [ ]  NavigationService complete
- [ ]  View registration works
- [ ]  Navigation between views works
- [ ]  Back navigation works
- [ ]  Events fire correctly

### ✅ Window Layout

- [ ]  MainWindow layout complete
- [ ]  Sidebar navigation renders
- [ ]  Content area displays views
- [ ]  Window resizes correctly
- [ ]  Responsive layout

### ✅ Keyboard Shortcuts

- [ ]  KeyboardShortcutService implemented
- [ ]  Shortcuts registered for all views
- [ ]  Shortcuts execute correctly
- [ ]  Modifier keys work
- [ ]  Help display shows shortcuts

### ✅ Testing

- [ ]  Unit tests written (80%+ coverage)
- [ ]  All tests pass
- [ ]  Manual navigation testing complete

---

## Default Keyboard Shortcuts

| Key | Action |
| --- | --- |
| **Escape** | Main Menu |
| **F1** | Combat View |
| **M** | Map/Dungeon View |
| **C** | Character Sheet |
| **I** | Inventory |
| **Backspace** | Back |
| **F5** | Quick Save |
| **F9** | Quick Load |
| **F11** | Toggle Fullscreen |
| **Alt+F4** | Quit |

---

**Foundation phase complete! Ready for combat UI in v0.43.4.**