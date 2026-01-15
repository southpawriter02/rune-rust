namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Panel control displaying the player's inventory.
/// </summary>
/// <remarks>
/// Displays items in a grid layout with:
/// - Click-to-select items
/// - Keyboard navigation (arrow keys)
/// - Selected item details display
/// - Capacity and weight indicators
/// </remarks>
public partial class InventoryPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryPanel"/> class.
    /// </summary>
    public InventoryPanel()
    {
        InitializeComponent();
        
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not InventoryPanelViewModel vm)
            return;

        switch (e.Key)
        {
            case Key.Left:
                vm.NavigateCommand.Execute("left");
                e.Handled = true;
                break;
            case Key.Right:
                vm.NavigateCommand.Execute("right");
                e.Handled = true;
                break;
            case Key.Up:
                vm.NavigateCommand.Execute("up");
                e.Handled = true;
                break;
            case Key.Down:
                vm.NavigateCommand.Execute("down");
                e.Handled = true;
                break;
            case Key.Escape:
                vm.ClearSelectionCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
