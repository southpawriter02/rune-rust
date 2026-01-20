namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;

/// <summary>
/// Panel control displaying the current room's state.
/// </summary>
/// <remarks>
/// This control displays:
/// - Room name as header
/// - ASCII art representation (when available)
/// - Art legend (when available)
/// - Room description
/// - Available exits as clickable buttons
/// - Visible monsters and items
/// </remarks>
public partial class RoomDisplayPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomDisplayPanel"/> class.
    /// </summary>
    public RoomDisplayPanel()
    {
        InitializeComponent();
    }
}
