using RuneAndRust.DesktopUI.ViewModels;
using System;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Implementation of navigation service.
/// Basic functionality for v0.43.1, full implementation in v0.43.3.
/// </summary>
public class NavigationService : INavigationService
{
    private ViewModelBase? _currentView;

    /// <inheritdoc/>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView != value)
            {
                _currentView = value;
                if (value != null)
                {
                    CurrentViewChanged?.Invoke(this, value);
                }
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ViewModelBase>? CurrentViewChanged;

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        CurrentView = viewModel;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        throw new NotImplementedException("View model type registration will be implemented in v0.43.3");
    }
}
