using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Main window view model that manages the top-level application shell.
/// Coordinates navigation between different views and registers view factories.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IKeyboardShortcutService _keyboardShortcutService;
    private ViewModelBase? _currentView;

    /// <summary>
    /// Gets or sets the currently displayed view model.
    /// </summary>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title => "Rune & Rust";

    /// <summary>
    /// Command to navigate to the main menu.
    /// </summary>
    public ICommand NavigateToMenuCommand { get; }

    /// <summary>
    /// Command to navigate to combat view.
    /// </summary>
    public ICommand NavigateToCombatCommand { get; }

    /// <summary>
    /// Command to navigate to character sheet.
    /// </summary>
    public ICommand NavigateToCharacterCommand { get; }

    /// <summary>
    /// Command to navigate to dungeon exploration.
    /// </summary>
    public ICommand NavigateToDungeonCommand { get; }

    /// <summary>
    /// Command to navigate to inventory.
    /// </summary>
    public ICommand NavigateToInventoryCommand { get; }

    /// <summary>
    /// Command to navigate to sprite demo.
    /// </summary>
    public ICommand NavigateToSpriteDemoCommand { get; }

    /// <summary>
    /// Command to navigate back.
    /// </summary>
    public ICommand NavigateBackCommand { get; }

    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel(
        INavigationService navigationService,
        IKeyboardShortcutService keyboardShortcutService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _keyboardShortcutService = keyboardShortcutService ?? throw new ArgumentNullException(nameof(keyboardShortcutService));

        // Subscribe to navigation changes
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;

        // Register view model factories
        RegisterViewModelFactories(serviceProvider);

        // Set up navigation commands
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

        NavigateToSpriteDemoCommand = ReactiveCommand.Create(() =>
            _navigationService.NavigateTo<SpriteDemoViewModel>());

        NavigateBackCommand = ReactiveCommand.Create(
            () => _navigationService.NavigateBack(),
            this.WhenAnyValue(x => x._navigationService.CanNavigateBack));

        // Register keyboard shortcuts
        RegisterKeyboardShortcuts();

        // Start with the menu view
        _navigationService.NavigateTo<MenuViewModel>();
    }

    /// <summary>
    /// Registers view model factories with the navigation service.
    /// </summary>
    private void RegisterViewModelFactories(IServiceProvider serviceProvider)
    {
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

        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<SpriteDemoViewModel>());
    }

    /// <summary>
    /// Registers keyboard shortcuts for navigation.
    /// </summary>
    private void RegisterKeyboardShortcuts()
    {
        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.Escape,
            () => NavigateToMenuCommand.Execute(null),
            "Main Menu");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.F1,
            () => NavigateToCombatCommand.Execute(null),
            "Combat View");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.M,
            () => NavigateToDungeonCommand.Execute(null),
            "Dungeon Map");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.C,
            () => NavigateToCharacterCommand.Execute(null),
            "Character Sheet");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.I,
            () => NavigateToInventoryCommand.Execute(null),
            "Inventory");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.Back,
            () => NavigateBackCommand.Execute(null),
            "Navigate Back");
    }

    /// <summary>
    /// Handles keyboard input for shortcuts.
    /// </summary>
    public bool HandleKeyPress(Avalonia.Input.Key key, Avalonia.Input.KeyModifiers modifiers)
    {
        return _keyboardShortcutService.HandleKeyPress(key, modifiers);
    }

    private void OnCurrentViewChanged(object? sender, ViewModelBase view)
    {
        CurrentView = view;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigationService.CurrentViewChanged -= OnCurrentViewChanged;
        }
        base.Dispose(disposing);
    }
}
