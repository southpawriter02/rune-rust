using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.21: Service for displaying modal dialogs and user prompts.
/// Provides confirmation dialogs, message boxes, error dialogs, and input dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Confirmation message.</param>
    /// <returns>True if user clicked Yes, false otherwise.</returns>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an informational message dialog with OK button.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Message to display.</param>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a custom dialog with a view model.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="viewModel">The ViewModel to display in the dialog.</param>
    /// <returns>The dialog result, or default if cancelled.</returns>
    Task<T?> ShowDialogAsync<T>(object viewModel);

    /// <summary>
    /// Shows an error dialog with a red accent.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Error message to display.</param>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows an input dialog and returns the user's input.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="prompt">Prompt text to display.</param>
    /// <param name="defaultValue">Default value for the input field.</param>
    /// <returns>The user's input, or null if cancelled.</returns>
    Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "");
}
