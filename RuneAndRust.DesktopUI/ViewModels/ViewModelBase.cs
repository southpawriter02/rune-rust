using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides ReactiveUI support, activation lifecycle, and disposable pattern.
/// </summary>
public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel, IDisposable
{
    /// <summary>
    /// Gets the activator for managing view model lifecycle.
    /// </summary>
    public ViewModelActivator Activator { get; } = new();

    /// <summary>
    /// Composite disposable for managing subscriptions and resources.
    /// </summary>
    protected CompositeDisposable Disposables { get; } = new();

    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of ViewModelBase and sets up activation lifecycle.
    /// </summary>
    protected ViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            // Call OnActivated when the view model is activated
            OnActivated();

            // Call OnDeactivated when disposed
            Disposable.Create(() => OnDeactivated())
                .DisposeWith(disposables);
        });
    }

    /// <summary>
    /// Called when the view model is activated.
    /// Override to set up subscriptions and reactive bindings.
    /// </summary>
    protected virtual void OnActivated() { }

    /// <summary>
    /// Called when the view model is deactivated.
    /// Override to clean up resources.
    /// </summary>
    protected virtual void OnDeactivated() { }

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Disposables.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Disposes of the view model.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
