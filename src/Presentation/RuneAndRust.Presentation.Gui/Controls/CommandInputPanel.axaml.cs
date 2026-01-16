namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Panel control for command input with history and tab completion.
/// </summary>
/// <remarks>
/// Provides a text input field with:
/// - Command prompt indicator (>)
/// - History navigation (up/down arrows)
/// - Tab completion with popup suggestions
/// - Visual history indicator when navigating
/// </remarks>
public partial class CommandInputPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandInputPanel"/> class.
    /// </summary>
    public CommandInputPanel()
    {
        InitializeComponent();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CommandInputViewModel vm)
            return;

        // Let the ViewModel handle the key
        var handled = vm.HandleKeyDown(e.Key);

        if (handled)
        {
            e.Handled = true;
        }
    }

    private void OnCompletionAccepted(object? sender, string suggestion)
    {
        if (DataContext is CommandInputViewModel vm)
        {
            vm.ApplyCompletion(suggestion);

            // Refocus the input box
            InputBox.Focus();
            InputBox.CaretIndex = InputBox.Text?.Length ?? 0;
        }
    }

    /// <summary>
    /// Sets focus to the input box.
    /// </summary>
    public void FocusInput()
    {
        InputBox.Focus();
    }
}
