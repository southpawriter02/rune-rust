using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.DesktopUI.ViewModels;

namespace RuneAndRust.DesktopUI.Views;

/// <summary>
/// v0.43.20: View for the help browser.
/// Allows users to search and browse help documentation.
/// </summary>
public partial class HelpView : UserControl
{
    public HelpView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles pointer press on topic items for selection.
    /// </summary>
    private void OnTopicPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is HelpTopicViewModel topicVm)
        {
            if (DataContext is HelpViewModel vm)
            {
                vm.SelectTopic(topicVm);
            }
        }
    }
}
