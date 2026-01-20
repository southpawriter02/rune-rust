namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Dialogue Window for NPC conversations.
/// </summary>
public partial class DialogueWindow : Window
{
    /// <summary>Creates a new dialogue window.</summary>
    public DialogueWindow()
    {
        InitializeComponent();
        DataContext = new DialogueWindowViewModel(Close);
        Loaded += async (_, _) =>
        {
            if (DataContext is DialogueWindowViewModel vm)
                await vm.StartDialogueAsync("Merchant Marcus", "🧙");
        };
        KeyDown += OnKeyDown;
    }

    private void OnDialogueTextPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is DialogueWindowViewModel vm)
            vm.ClickTextCommand.Execute(null);
    }

    private void OnChoicePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            DataContext is DialogueWindowViewModel vm &&
            border.DataContext is DialogueChoiceViewModel choice)
        {
            vm.SelectChoiceCommand.Execute(choice);
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not DialogueWindowViewModel vm) return;

        // Number keys 1-9 select choices
        if (e.Key >= Key.D1 && e.Key <= Key.D9)
        {
            await vm.HandleKeyPress(e.Key - Key.D1 + 1);
            e.Handled = true;
        }
        else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
        {
            await vm.HandleKeyPress(e.Key - Key.NumPad1 + 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            vm.ClickTextCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
