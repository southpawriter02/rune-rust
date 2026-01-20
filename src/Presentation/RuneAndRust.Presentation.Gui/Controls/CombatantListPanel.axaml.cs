namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;

/// <summary>
/// Panel displaying the list of combatants in turn order.
/// </summary>
/// <remarks>
/// Shows each combatant with HP bar, status effects, and turn indicator.
/// The current turn combatant is highlighted.
/// </remarks>
public partial class CombatantListPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the CombatantListPanel.
    /// </summary>
    public CombatantListPanel()
    {
        InitializeComponent();
    }
}
