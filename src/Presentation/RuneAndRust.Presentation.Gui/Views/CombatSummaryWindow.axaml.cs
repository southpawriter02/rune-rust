namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;

/// <summary>
/// Code-behind for the combat summary window.
/// </summary>
/// <remarks>
/// Handles keyboard shortcuts for common actions.
/// </remarks>
public partial class CombatSummaryWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombatSummaryWindow"/> class.
    /// </summary>
    public CombatSummaryWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is ViewModels.CombatSummaryViewModel viewModel)
        {
            switch (e.Key)
            {
                case Key.C when viewModel.IsVictory && viewModel.HasLoot:
                    viewModel.CollectAllCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Enter when viewModel.IsVictory:
                    viewModel.ContinueCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (viewModel.IsVictory)
                        viewModel.ContinueCommand.Execute(null);
                    else
                        viewModel.ReturnToMenuCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
