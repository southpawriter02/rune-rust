using System;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Implementation of dialog service.
/// Stub implementation for v0.43.1, full implementation in v0.43.18.
/// </summary>
public class DialogService : IDialogService
{
    /// <inheritdoc/>
    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        // Stub implementation - will be replaced with actual Avalonia dialog in v0.43.18
        Console.WriteLine($"[CONFIRMATION] {title}: {message}");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task ShowMessageAsync(string title, string message)
    {
        // Stub implementation - will be replaced with actual Avalonia dialog in v0.43.18
        Console.WriteLine($"[MESSAGE] {title}: {message}");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<T?> ShowDialogAsync<T>(object viewModel)
    {
        // Stub implementation - will be replaced with actual Avalonia dialog in v0.43.18
        throw new NotImplementedException("Custom dialogs will be implemented in v0.43.18");
    }
}
