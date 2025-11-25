using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Threading.Tasks;
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
    private readonly ISaveGameService _saveGameService;
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
    /// Command to navigate to specialization tree.
    /// </summary>
    public ICommand NavigateToSpecializationTreeCommand { get; }

    /// <summary>
    /// Command to navigate back.
    /// </summary>
    public ICommand NavigateBackCommand { get; }

    /// <summary>
    /// Command to navigate to save/load view.
    /// </summary>
    public ICommand NavigateToSaveLoadCommand { get; }

    /// <summary>
    /// Command to perform quick save (F5).
    /// </summary>
    public ICommand QuickSaveCommand { get; }

    /// <summary>
    /// Command to perform quick load (F9).
    /// </summary>
    public ICommand QuickLoadCommand { get; }

    /// <summary>
    /// Command to navigate to help view (F1).
    /// </summary>
    public ICommand NavigateToHelpCommand { get; }

    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel(
        INavigationService navigationService,
        IKeyboardShortcutService keyboardShortcutService,
        ISaveGameService saveGameService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _keyboardShortcutService = keyboardShortcutService ?? throw new ArgumentNullException(nameof(keyboardShortcutService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));

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

        NavigateToSpecializationTreeCommand = ReactiveCommand.Create(() =>
            _navigationService.NavigateTo<SpecializationTreeViewModel>());

        NavigateBackCommand = ReactiveCommand.Create(
            () => _navigationService.NavigateBack(),
            this.WhenAnyValue(x => x._navigationService.CanNavigateBack));

        NavigateToSaveLoadCommand = ReactiveCommand.Create(() =>
            _navigationService.NavigateTo<SaveLoadViewModel>());

        QuickSaveCommand = ReactiveCommand.CreateFromTask(QuickSaveAsync);

        QuickLoadCommand = ReactiveCommand.CreateFromTask(QuickLoadAsync);

        NavigateToHelpCommand = ReactiveCommand.Create(() =>
            _navigationService.NavigateTo<HelpViewModel>());

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

        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<SpecializationTreeViewModel>());

        // v0.43.15: Meta-Progression
        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<MetaProgressionViewModel>());

        // v0.43.16: Endgame Mode Selection
        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<EndgameModeViewModel>());

        // v0.43.18: Settings
        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<SettingsViewModel>());

        // v0.43.19: Save/Load
        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<SaveLoadViewModel>());

        // v0.43.20: Help
        _navigationService.RegisterViewModelFactory(() =>
            serviceProvider.GetRequiredService<HelpViewModel>());
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
            () => NavigateToHelpCommand.Execute(null),
            "Help");

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
            Avalonia.Input.Key.T,
            () => NavigateToSpecializationTreeCommand.Execute(null),
            "Specialization Tree");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.Back,
            () => NavigateBackCommand.Execute(null),
            "Navigate Back");

        // v0.43.19: Quick Save/Load shortcuts
        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.F5,
            () => QuickSaveCommand.Execute(null),
            "Quick Save");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.F9,
            () => QuickLoadCommand.Execute(null),
            "Quick Load");

        _keyboardShortcutService.RegisterShortcut(
            Avalonia.Input.Key.L,
            () => NavigateToSaveLoadCommand.Execute(null),
            "Save/Load Menu");
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

    /// <summary>
    /// Performs a quick save operation.
    /// </summary>
    private async Task QuickSaveAsync()
    {
        try
        {
            await _saveGameService.QuickSaveAsync();
            Console.WriteLine("[MAINWINDOW] Quick save completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MAINWINDOW] Quick save failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs a quick load operation.
    /// </summary>
    private async Task QuickLoadAsync()
    {
        try
        {
            var success = await _saveGameService.QuickLoadAsync();
            if (success)
            {
                Console.WriteLine("[MAINWINDOW] Quick load completed");
            }
            else
            {
                Console.WriteLine("[MAINWINDOW] Quick load failed - no quick save found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MAINWINDOW] Quick load failed: {ex.Message}");
        }
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
