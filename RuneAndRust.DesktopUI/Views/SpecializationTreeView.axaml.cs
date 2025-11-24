using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RuneAndRust.DesktopUI.Views;

/// <summary>
/// View for displaying the specialization ability tree.
/// Shows abilities organized by tier with unlock/rank up functionality.
/// </summary>
public partial class SpecializationTreeView : UserControl
{
    public SpecializationTreeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
