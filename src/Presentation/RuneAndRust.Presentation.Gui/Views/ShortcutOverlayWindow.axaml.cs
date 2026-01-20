namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.Services;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Shortcut overlay window that closes on any key press.
/// </summary>
public partial class ShortcutOverlayWindow : Window
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public ShortcutOverlayWindow()
    {
        InitializeComponent();
        DataContext = new ShortcutOverlayViewModel();
    }

    /// <summary>
    /// Initializes with a shortcut service.
    /// </summary>
    /// <param name="shortcutService">The keyboard shortcut service.</param>
    public ShortcutOverlayWindow(IKeyboardShortcutService shortcutService)
    {
        InitializeComponent();
        DataContext = new ShortcutOverlayViewModel(shortcutService, Close);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Close on any key press
        Close();
        e.Handled = true;
    }
}
