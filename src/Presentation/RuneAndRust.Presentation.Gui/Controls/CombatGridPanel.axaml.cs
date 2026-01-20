namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;

/// <summary>
/// Panel control displaying the combat grid.
/// </summary>
/// <remarks>
/// Displays an 8x8 tactical grid with:
/// - Coordinate labels (A-H rows, 1-8 columns)
/// - Terrain cells (floor, wall, water, hazard, cover)
/// - Entity tokens (player, monsters, allies)
/// - Legend explaining symbols
/// </remarks>
public partial class CombatGridPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombatGridPanel"/> class.
    /// </summary>
    public CombatGridPanel()
    {
        InitializeComponent();
    }
}
