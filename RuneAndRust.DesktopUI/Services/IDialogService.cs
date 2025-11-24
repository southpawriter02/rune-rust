using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for displaying modal dialogs and user prompts.
/// Full implementation in v0.43.18.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an informational message dialog.
    /// </summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a custom dialog with a view model.
    /// </summary>
    Task<T?> ShowDialogAsync<T>(object viewModel);
}
