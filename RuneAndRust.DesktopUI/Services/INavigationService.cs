using RuneAndRust.DesktopUI.ViewModels;
using System;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for managing view navigation throughout the application.
/// Full implementation in v0.43.3.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently displayed view model.
    /// </summary>
    ViewModelBase? CurrentView { get; }

    /// <summary>
    /// Navigates to a specific view model instance.
    /// </summary>
    void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a view model type (resolved from DI container).
    /// Full implementation in v0.43.3.
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Event raised when the current view changes.
    /// </summary>
    event EventHandler<ViewModelBase>? CurrentViewChanged;
}
