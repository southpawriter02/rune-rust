using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using RuneAndRust.DesktopUI.ViewModels;

namespace RuneAndRust.DesktopUI.Views;

/// <summary>
/// v0.43.19: View for save/load game management.
/// Allows users to save, load, and manage save files.
/// </summary>
public partial class SaveLoadView : UserControl
{
    public SaveLoadView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles pointer press on save file items for selection.
    /// </summary>
    private void OnSaveFilePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is SaveFileViewModel saveVm)
        {
            if (DataContext is SaveLoadViewModel vm)
            {
                vm.SelectSave(saveVm);
            }
        }
    }
}
