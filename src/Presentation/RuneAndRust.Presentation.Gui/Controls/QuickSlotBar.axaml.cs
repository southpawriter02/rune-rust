namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Code-behind for the quick-slot bar control.
/// </summary>
/// <remarks>
/// Handles keyboard input for slots 1-8 and delegates to the ViewModel.
/// </remarks>
public partial class QuickSlotBar : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuickSlotBar"/> class.
    /// </summary>
    public QuickSlotBar()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is QuickSlotBarViewModel viewModel)
        {
            viewModel.HandleKeyPress(e.Key);
        }
    }
}
