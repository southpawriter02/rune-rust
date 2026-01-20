namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;

/// <summary>
/// Main game window containing all panel regions.
/// </summary>
/// <remarks>
/// This window provides the primary game interface with regions for:
/// - Main content (room display)
/// - Bottom content (message log)
/// - Right sidebar (player status, inventory)
/// - Input (command entry)
/// - Combat overlay (tactical grid)
/// </remarks>
public partial class GameWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameWindow"/> class.
    /// </summary>
    public GameWindow()
    {
        InitializeComponent();
    }
}
