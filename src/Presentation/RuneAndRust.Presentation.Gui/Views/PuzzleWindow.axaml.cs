namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Puzzle Window for interactive puzzles.
/// </summary>
public partial class PuzzleWindow : Window
{
    /// <summary>Creates a new puzzle window.</summary>
    public PuzzleWindow()
    {
        InitializeComponent();
        DataContext = new PuzzleWindowViewModel(Close);
    }

    private void OnElementPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            DataContext is PuzzleWindowViewModel vm &&
            border.DataContext is PuzzleElementViewModel element)
        {
            if (vm.PuzzleType == PuzzleType.Lever)
                vm.ToggleElementCommand.Execute(element);
            else
                vm.SelectElementCommand.Execute(element);
        }
    }
}
