using RuneAndRust.DesktopUI.ViewModels;
using System;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for managing view navigation throughout the application.
/// Supports forward navigation, back navigation with stack, and view factory registration.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently displayed view model.
    /// </summary>
    ViewModelBase? CurrentView { get; }

    /// <summary>
    /// Gets whether back navigation is available.
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Navigates to a specific view model instance.
    /// </summary>
    void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a view model type (resolved from registered factory).
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates back to the previous view in the navigation stack.
    /// </summary>
    void NavigateBack();

    /// <summary>
    /// Registers a factory function for creating instances of a view model type.
    /// </summary>
    void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory) where TViewModel : ViewModelBase;

    /// <summary>
    /// Event raised when the current view changes.
    /// </summary>
    event EventHandler<ViewModelBase>? CurrentViewChanged;
}
