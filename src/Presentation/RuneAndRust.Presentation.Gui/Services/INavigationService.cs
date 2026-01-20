namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Controls;

/// <summary>
/// Handles navigation between windows in the application.
/// </summary>
/// <remarks>
/// Provides methods for window replacement, modal dialogs,
/// file picker dialogs, and application lifecycle control.
/// </remarks>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently active main window.
    /// </summary>
    /// <value>The current window, or null if no window is active.</value>
    Window? CurrentWindow { get; }

    /// <summary>
    /// Navigates to a window by replacing the current main window.
    /// </summary>
    /// <typeparam name="TWindow">The type of window to navigate to.</typeparam>
    /// <remarks>
    /// The previous main window is closed after the new window is shown.
    /// </remarks>
    void NavigateTo<TWindow>() where TWindow : Window, new();

    /// <summary>
    /// Shows a modal dialog window.
    /// </summary>
    /// <typeparam name="TWindow">The type of dialog window to show.</typeparam>
    /// <remarks>
    /// The dialog is displayed modally over the current window.
    /// </remarks>
    void ShowDialog<TWindow>() where TWindow : Window, new();

    /// <summary>
    /// Shows a file open dialog for loading save files.
    /// </summary>
    /// <returns>
    /// The selected file path, or null if the user cancelled the dialog.
    /// </returns>
    Task<string?> ShowLoadDialogAsync();

    /// <summary>
    /// Shows a file save dialog for saving game data.
    /// </summary>
    /// <returns>
    /// The selected file path, or null if the user cancelled the dialog.
    /// </returns>
    Task<string?> ShowSaveDialogAsync();

    /// <summary>
    /// Returns to the main menu by navigating to the MainMenuWindow.
    /// </summary>
    void ReturnToMainMenu();

    /// <summary>
    /// Quits the application gracefully.
    /// </summary>
    void Quit();
}
