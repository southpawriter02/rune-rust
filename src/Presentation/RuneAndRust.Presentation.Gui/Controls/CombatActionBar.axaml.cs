namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;

/// <summary>
/// Action bar with combat buttons.
/// </summary>
/// <remarks>
/// Displays turn info (round, action points) and action buttons
/// for Attack, Move, Ability, Defend, Item, Wait, and End Turn.
/// </remarks>
public partial class CombatActionBar : UserControl
{
    /// <summary>
    /// Initializes a new instance of the CombatActionBar.
    /// </summary>
    public CombatActionBar()
    {
        InitializeComponent();
    }
}
