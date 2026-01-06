namespace RuneAndRust.Presentation.Shared.Abstractions;

public interface INavigationService
{
    Task NavigateToAsync<TView>() where TView : class;
    Task NavigateBackAsync();
    Task<TResult?> ShowDialogAsync<TDialog, TResult>(object? parameter = null)
        where TDialog : class
        where TResult : class;
}
