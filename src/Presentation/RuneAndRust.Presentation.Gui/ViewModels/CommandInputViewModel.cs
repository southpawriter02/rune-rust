namespace RuneAndRust.Presentation.Gui.ViewModels;

using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the command input panel.
/// </summary>
/// <remarks>
/// Provides text input with command history navigation (up/down arrows)
/// and tab completion with popup suggestions.
/// </remarks>
public partial class CommandInputViewModel : ViewModelBase
{
    private readonly ICommandHistoryService? _history;
    private readonly ITabCompletionService? _completion;

    /// <summary>
    /// Gets or sets the current input text.
    /// </summary>
    [ObservableProperty]
    private string _inputText = string.Empty;

    /// <summary>
    /// Gets or sets whether the completion popup is visible.
    /// </summary>
    [ObservableProperty]
    private bool _isCompletionVisible;

    /// <summary>
    /// Gets or sets the completion suggestions.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _suggestions = [];

    /// <summary>
    /// Gets or sets the selected suggestion index.
    /// </summary>
    [ObservableProperty]
    private int _selectedSuggestionIndex = -1;

    /// <summary>
    /// Gets or sets whether currently navigating history.
    /// </summary>
    [ObservableProperty]
    private bool _isNavigatingHistory;

    private string _inputBeforeHistory = string.Empty;

    /// <summary>
    /// Event raised when a command should be executed.
    /// </summary>
    public event Action<string>? CommandSubmitted;

    /// <summary>
    /// Creates a new CommandInputViewModel with services.
    /// </summary>
    /// <param name="history">The command history service.</param>
    /// <param name="completion">The tab completion service.</param>
    public CommandInputViewModel(
        ICommandHistoryService history,
        ITabCompletionService completion)
    {
        _history = history;
        _completion = completion;
        Log.Debug("CommandInputViewModel created with services");
    }

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public CommandInputViewModel()
    {
        Log.Debug("CommandInputViewModel created (design-time)");
    }

    /// <summary>
    /// Handles key press events.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>True if the key was handled, false otherwise.</returns>
    public bool HandleKeyDown(Key key)
    {
        switch (key)
        {
            case Key.Enter:
                if (IsCompletionVisible && SelectedSuggestionIndex >= 0)
                {
                    AcceptCompletion();
                }
                else
                {
                    SubmitCommand();
                }
                return true;

            case Key.Up:
                if (IsCompletionVisible)
                {
                    MoveSuggestionSelection(-1);
                }
                else
                {
                    NavigateHistoryBack();
                }
                return true;

            case Key.Down:
                if (IsCompletionVisible)
                {
                    MoveSuggestionSelection(1);
                }
                else
                {
                    NavigateHistoryForward();
                }
                return true;

            case Key.Tab:
                TriggerCompletion();
                return true;

            case Key.Escape:
                if (IsCompletionVisible)
                {
                    HideCompletion();
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Applies a completion suggestion to the input.
    /// </summary>
    /// <param name="completion">The completion to apply.</param>
    public void ApplyCompletion(string completion)
    {
        // Replace current word with completion
        var words = InputText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            words[^1] = completion;
            InputText = string.Join(' ', words) + " ";
        }
        else
        {
            InputText = completion + " ";
        }

        HideCompletion();
        Log.Debug("Applied completion: {Completion}", completion);
    }

    private void SubmitCommand()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        var command = InputText.Trim();

        Log.Information("Submitting command: {Command}", command);

        _history?.Add(command);
        CommandSubmitted?.Invoke(command);

        InputText = string.Empty;
        IsNavigatingHistory = false;
        _history?.ResetNavigation();
    }

    private void NavigateHistoryBack()
    {
        if (_history is null)
            return;

        if (!IsNavigatingHistory)
        {
            _history.SaveCurrentInput(InputText);
            _inputBeforeHistory = InputText;
            IsNavigatingHistory = true;
        }

        var previous = _history.GetPrevious();
        if (previous is not null)
        {
            InputText = previous;
            Log.Debug("Navigated to previous history: {Command}", previous);
        }
    }

    private void NavigateHistoryForward()
    {
        if (_history is null)
            return;

        var next = _history.GetNext();
        if (next is not null)
        {
            InputText = next;
            Log.Debug("Navigated to next history: {Command}", next);
        }
        else
        {
            // Return to original input
            InputText = _inputBeforeHistory;
            IsNavigatingHistory = false;
        }
    }

    private void TriggerCompletion()
    {
        if (_completion is null || string.IsNullOrWhiteSpace(InputText))
            return;

        var context = CompletionContext.FromInput(InputText);
        var completions = _completion.GetCompletions(InputText, context);

        if (completions.Count == 0)
        {
            HideCompletion();
            Log.Debug("No completions found for: {Input}", InputText);
            return;
        }

        if (completions.Count == 1)
        {
            // Single match - complete inline
            ApplyCompletion(completions[0]);
            Log.Debug("Single completion applied: {Completion}", completions[0]);
        }
        else
        {
            // Multiple matches - show popup
            Suggestions.Clear();
            foreach (var c in completions)
            {
                Suggestions.Add(c);
            }

            SelectedSuggestionIndex = 0;
            IsCompletionVisible = true;
            Log.Debug("Showing {Count} completions", completions.Count);
        }
    }

    private void AcceptCompletion()
    {
        if (SelectedSuggestionIndex >= 0 && SelectedSuggestionIndex < Suggestions.Count)
        {
            ApplyCompletion(Suggestions[SelectedSuggestionIndex]);
        }
        HideCompletion();
    }

    private void MoveSuggestionSelection(int delta)
    {
        if (Suggestions.Count == 0)
            return;

        var newIndex = SelectedSuggestionIndex + delta;

        // Wrap around
        if (newIndex < 0)
            newIndex = Suggestions.Count - 1;
        else if (newIndex >= Suggestions.Count)
            newIndex = 0;

        SelectedSuggestionIndex = newIndex;
    }

    private void HideCompletion()
    {
        IsCompletionVisible = false;
        Suggestions.Clear();
        SelectedSuggestionIndex = -1;
    }
}
