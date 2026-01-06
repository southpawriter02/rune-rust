namespace RuneAndRust.Presentation.Shared.Abstractions;

/// <summary>
/// Defines the contract for view navigation in the application.
/// </summary>
/// <remarks>
/// Implementations of this interface handle navigation between views,
/// back navigation, and modal dialogs. This abstraction allows for
/// different navigation implementations across TUI and GUI presentations.
/// </remarks>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified view type.
    /// </summary>
    /// <typeparam name="TView">The type of view to navigate to.</typeparam>
    Task NavigateToAsync<TView>() where TView : class;

    /// <summary>
    /// Navigates back to the previous view in the navigation stack.
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// Shows a modal dialog and returns its result.
    /// </summary>
    /// <typeparam name="TDialog">The type of dialog to show.</typeparam>
    /// <typeparam name="TResult">The type of result the dialog returns.</typeparam>
    /// <param name="parameter">Optional parameter to pass to the dialog.</param>
    /// <returns>The dialog result, or null if cancelled.</returns>
    Task<TResult?> ShowDialogAsync<TDialog, TResult>(object? parameter = null)
        where TDialog : class
        where TResult : class;
}
