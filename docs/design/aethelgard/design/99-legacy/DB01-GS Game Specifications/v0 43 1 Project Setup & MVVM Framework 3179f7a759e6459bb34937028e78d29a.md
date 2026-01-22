# v0.43.1: Project Setup & MVVM Framework

Type: Technical
Description: Establishes foundational infrastructure for Avalonia desktop UI: project structure, DI container configuration, MVVM pattern with ReactiveUI, and basic application lifecycle. Deliverable: Working Avalonia application with DI and navigation. 5-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1-v0.42 (All core systems)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.1-v0.42 (All core systems)

**Estimated Time:** 5-8 hours

**Group:** Foundation

**Deliverable:** Working Avalonia application with DI and navigation

---

## Executive Summary

v0.43.1 establishes the foundational infrastructure for the Avalonia desktop UI by creating the project structure, configuring dependency injection, implementing the MVVM pattern with ReactiveUI, and setting up the basic application lifecycle.

**What This Delivers:**

- `RuneAndRust.DesktopUI` Avalonia project
- Dependency injection container configured
- `ViewModelBase` with ReactiveUI
- Main application window
- Service registration for existing Engine services
- Application launches successfully

**Success Metric:** Application launches, displays a window, and DI container resolves existing game services.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### ViewModelBase

```csharp
using ReactiveUI;
using System.Reactive.Disposables;

namespace RuneAndRust.DesktopUI.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    
    protected CompositeDisposable Disposables { get; } = new();
    
    protected ViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            // Add activations/deactivations here
            Disposable.Create(() => OnDeactivated())
                .DisposeWith(disposables);
            
            OnActivated();
        });
    }
    
    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disposables.Dispose();
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

### App.axaml.cs (Application Entry Point)

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.DesktopUI.Views;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.DesktopUI;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/desktopui-.log", rollingInterval: [RollingInterval.Day](http://RollingInterval.Day))
            .WriteTo.Console()
            .CreateLogger();
        
        // Build service container
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddSingleton<ILogger>(Log.Logger);
        
        // Persistence (existing)
        services.AddSingleton<ISaveRepository, SaveRepository>();
        
        // Engine services (existing v0.1-v0.42)
        services.AddSingleton<ICombatEngine, CombatEngine>();
        services.AddSingleton<IDungeonGenerator, DungeonGenerator>();
        services.AddSingleton<ICharacterProgressionService, CharacterProgressionService>();
        services.AddSingleton<IEnemyAIService, EnemyAIService>();
        services.AddSingleton<IEquipmentService, EquipmentService>();
        services.AddSingleton<IStatusEffectService, StatusEffectService>();
        // ... (register all existing Engine services)
        
        // UI Services (new)
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IDialogService, DialogService>();
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
    }
}
```

### INavigationService (Stub for v0.43.3)

```csharp
namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface INavigationService
{
    ViewModelBase? CurrentView { get; }
    void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    event EventHandler<ViewModelBase>? CurrentViewChanged;
}

public class NavigationService : INavigationService
{
    private ViewModelBase? _currentView;
    
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke(this, value!);
        }
    }
    
    public event EventHandler<ViewModelBase>? CurrentViewChanged;
    
    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        CurrentView = viewModel;
    }
    
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        throw new NotImplementedException("View registration in v0.43.3");
    }
}
```

### MainWindowViewModel

```csharp
using ReactiveUI;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

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
    
    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        // Start with menu
        CurrentView = new MenuViewModel();
    }
    
    private void OnCurrentViewChanged(object? sender, ViewModelBase view)
    {
        CurrentView = view;
    }
}
```

---

## Integration Points

**With Existing Engine Services:**

- All v0.1-v0.42 services registered in DI container
- No changes to existing service implementations
- UI project references `RuneAndRust.Engine`
- UI project references `RuneAndRust.Core`
- UI project references `RuneAndRust.Persistence`

**With Future Specs:**

- v0.43.2: Sprite service registration
- v0.43.3: Navigation service implementation complete
- v0.43.4+: ViewModels for combat, character, etc.

---

## Functional Requirements

### FR1: Project Structure

**Requirement:** Create Avalonia project with proper structure.

**Directory Structure:**

```
RuneAndRust.DesktopUI/
├── App.axaml
├── App.axaml.cs
├── Program.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── MainWindowViewModel.cs
│   └── MenuViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   ├── MainWindow.axaml.cs
│   └── MenuView.axaml
├── Services/
│   ├── NavigationService.cs
│   ├── ConfigurationService.cs
│   └── DialogService.cs
├── Models/ (UI-specific models)
└── Assets/ (sprites, fonts, etc.)
```

**Validation:**

```csharp
[Fact]
public void Project_HasCorrectStructure()
{
    var projectPath = "RuneAndRust.DesktopUI";
    Assert.True(Directory.Exists(Path.Combine(projectPath, "ViewModels")));
    Assert.True(Directory.Exists(Path.Combine(projectPath, "Views")));
    Assert.True(Directory.Exists(Path.Combine(projectPath, "Services")));
    Assert.True(File.Exists(Path.Combine(projectPath, "App.axaml")));
}
```

### FR2: Dependency Injection Configuration

**Requirement:** Configure DI container with all existing services.

**Test:**

```csharp
[Fact]
public void DI_ResolvesEngineServices()
{
    var app = new App();
    app.Initialize();
    
    var services = [app.Services](http://app.Services);
    Assert.NotNull(services);
    
    // Test existing service resolution
    var combatEngine = services.GetService<ICombatEngine>();
    Assert.NotNull(combatEngine);
    
    var dungeonGen = services.GetService<IDungeonGenerator>();
    Assert.NotNull(dungeonGen);
    
    var aiService = services.GetService<IEnemyAIService>();
    Assert.NotNull(aiService);
}

[Fact]
public void DI_ResolvesUIServices()
{
    var app = new App();
    app.Initialize();
    
    var services = [app.Services](http://app.Services);
    
    var navService = services.GetService<INavigationService>();
    Assert.NotNull(navService);
    
    var mainViewModel = services.GetService<MainWindowViewModel>();
    Assert.NotNull(mainViewModel);
}
```

### FR3: MVVM Foundation

**Requirement:** Implement ViewModelBase with ReactiveUI.

**Test:**

```csharp
[Fact]
public void ViewModelBase_SupportsReactiveProperties()
{
    var vm = new TestViewModel();
    
    string? changedProperty = null;
    vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;
    
    vm.TestProperty = "New Value";
    
    Assert.Equal("TestProperty", changedProperty);
}

[Fact]
public void ViewModelBase_SupportsActivation()
{
    var vm = new TestViewModel();
    
    Assert.False(vm.WasActivated);
    
    // Activate
    vm.Activator.Activate();
    Assert.True(vm.WasActivated);
    
    // Deactivate
    vm.Activator.Deactivate();
    Assert.True(vm.WasDeactivated);
}

private class TestViewModel : ViewModelBase
{
    private string _testProperty = "";
    public string TestProperty
    {
        get => _testProperty;
        set => this.RaiseAndSetIfChanged(ref _testProperty, value);
    }
    
    public bool WasActivated { get; private set; }
    public bool WasDeactivated { get; private set; }
    
    protected override void OnActivated() => WasActivated = true;
    protected override void OnDeactivated() => WasDeactivated = true;
}
```

### FR4: Application Lifecycle

**Requirement:** Application starts, shows window, and shuts down cleanly.

**Manual Test Checklist:**

- [ ]  Run application
- [ ]  Window appears
- [ ]  Window has title "Rune & Rust"
- [ ]  Window can be resized
- [ ]  Window can be closed
- [ ]  No exceptions in logs
- [ ]  Application exits cleanly

**Automated Test:**

```csharp
[Fact]
public void Application_Initializes()
{
    var app = new App();
    app.Initialize();
    
    Assert.NotNull([app.Services](http://app.Services));
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**ViewModelBase Tests:**

- Property change notifications
- Activation/deactivation lifecycle
- Disposal behavior

**DI Configuration Tests:**

- All Engine services resolve
- All UI services resolve
- Transient vs Singleton lifetimes correct

**Navigation Service Tests:**

- Current view tracking
- View change events

### Integration Tests

**Application Startup:**

```csharp
[Fact]
public async Task Application_StartsAndShowsWindow()
{
    // This test requires UI testing framework
    // Implementation in v0.43.21
}
```

---

## Success Criteria

**v0.43.1 is DONE when:**

### ✅ Project Setup

- [ ]  Avalonia project created
- [ ]  Project references all required assemblies
- [ ]  Directory structure follows conventions
- [ ]  `.csproj` file configured correctly

### ✅ Dependency Injection

- [ ]  DI container configured
- [ ]  All Engine services registered
- [ ]  All UI services registered
- [ ]  Service resolution tested

### ✅ MVVM Foundation

- [ ]  ViewModelBase implemented
- [ ]  ReactiveUI integrated
- [ ]  Property change notifications work
- [ ]  Activation lifecycle works

### ✅ Application Lifecycle

- [ ]  Application launches
- [ ]  MainWindow displays
- [ ]  Navigation service wired up
- [ ]  Application closes cleanly
- [ ]  No memory leaks

### ✅ Testing

- [ ]  Unit tests written (80%+ coverage)
- [ ]  All tests pass
- [ ]  Manual testing checklist complete

---

## Implementation Notes

**NuGet Packages Required:**

```xml
<PackageReference Include="Avalonia" Version="11.0.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
```

**Target Framework:** .NET 8.0

**Logging Configuration:**

- File logging to `logs/desktopui-{Date}.log`
- Console logging for development
- Minimum level: Debug

**Platform Support:**

- Windows 10/11 (primary development platform)
- macOS 10.15+ (tested weekly)
- Ubuntu 20.04+ (tested weekly)

---

**Foundation established. Ready for sprite system in v0.43.2.**