namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels.Settings;

/// <summary>
/// Settings dialog window.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsWindowViewModel();
    }

    /// <summary>
    /// Initializes with existing settings.
    /// </summary>
    /// <param name="settings">Current game settings.</param>
    public SettingsWindow(GameSettings settings) : this()
    {
        var vm = new SettingsWindowViewModel(settings, Close);
        DataContext = vm;
    }
}
