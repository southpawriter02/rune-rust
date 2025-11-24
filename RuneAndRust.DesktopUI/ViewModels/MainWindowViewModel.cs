using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Main window view model that manages the top-level application shell.
/// Coordinates navigation between different views.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
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
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    /// <param name="navigationService">The navigation service for managing views.</param>
    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Subscribe to navigation changes
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;

        // Start with the menu view
        var menuViewModel = new MenuViewModel();
        _navigationService.NavigateTo(menuViewModel);
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
