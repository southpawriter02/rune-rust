using ReactiveUI;
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Complete implementation of navigation service with view factory registration and navigation stack.
/// </summary>
public class NavigationService : ReactiveObject, INavigationService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Type, Func<ViewModelBase>> _viewModelFactories = new();
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private ViewModelBase? _currentView;
    private bool _canNavigateBack;

    /// <inheritdoc/>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView != value)
            {
                _currentView = value;
                this.RaisePropertyChanged();
                if (value != null)
                {
                    CurrentViewChanged?.Invoke(this, value);
                    _logger.Information("Navigated to {ViewModelType}", value.GetType().Name);
                }
            }
        }
    }

    /// <inheritdoc/>
    public bool CanNavigateBack
    {
        get => _canNavigateBack;
        private set => this.RaiseAndSetIfChanged(ref _canNavigateBack, value);
    }

    /// <inheritdoc/>
    public event EventHandler<ViewModelBase>? CurrentViewChanged;

    public NavigationService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory)
        where TViewModel : ViewModelBase
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _viewModelFactories[typeof(TViewModel)] = () => factory();
        _logger.Debug("Registered factory for {ViewModelType}", typeof(TViewModel).Name);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        // Push current view onto stack if it exists
        if (CurrentView != null)
        {
            _navigationStack.Push(CurrentView);
            CanNavigateBack = true;
        }

        CurrentView = viewModel;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        if (!_viewModelFactories.TryGetValue(typeof(TViewModel), out var factory))
        {
            var errorMessage = $"No factory registered for {typeof(TViewModel).Name}. " +
                             $"Call RegisterViewModelFactory<{typeof(TViewModel).Name}>() first.";
            _logger.Error(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        var viewModel = factory();
        NavigateTo((TViewModel)viewModel);
    }

    /// <inheritdoc/>
    public void NavigateBack()
    {
        if (!CanNavigateBack)
        {
            _logger.Warning("Cannot navigate back: navigation stack is empty");
            return;
        }

        CurrentView = _navigationStack.Pop();
        CanNavigateBack = _navigationStack.Count > 0;

        _logger.Information("Navigated back to {ViewModelType}", CurrentView?.GetType().Name);
    }
}
