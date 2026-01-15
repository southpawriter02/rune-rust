namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;

/// <summary>
/// Panel control displaying the player's status.
/// </summary>
/// <remarks>
/// This control displays:
/// - Character name, class, and level
/// - HP, MP, XP resource bars with color thresholds
/// - Six core stats with modifiers
/// - Equipment slots
/// - Gold amount
/// - Current location
/// </remarks>
public partial class PlayerStatusPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerStatusPanel"/> class.
    /// </summary>
    public PlayerStatusPanel()
    {
        InitializeComponent();
    }
}
