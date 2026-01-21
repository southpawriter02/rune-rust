// ═══════════════════════════════════════════════════════════════════════════════
// NodeState.cs
// Enumeration for ability tree node unlock states.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Enums;

/// <summary>
/// Represents the current unlock state of an ability tree node.
/// </summary>
/// <remarks>
/// <para>
/// Node states are determined by the <see cref="UI.TreeNodeRenderer"/> based on
/// player progress, prerequisites, and talent point availability.
/// </para>
/// <para>
/// Each state maps to specific visual indicators and colors defined in
/// <see cref="Configuration.NodeStateDisplayConfig"/>.
/// </para>
/// </remarks>
public enum NodeState
{
    /// <summary>
    /// The node is locked and cannot be unlocked.
    /// </summary>
    /// <remarks>
    /// <para>Prerequisites are not met, or the player lacks required level.</para>
    /// <para>Displayed with the locked indicator ("[L]") and locked color (DarkGray).</para>
    /// </remarks>
    Locked = 0,

    /// <summary>
    /// The node is available for unlock.
    /// </summary>
    /// <remarks>
    /// <para>Prerequisites are met and the player has sufficient talent points.</para>
    /// <para>Displayed with the available indicator ("( )") and available color (Yellow).</para>
    /// </remarks>
    Available = 1,

    /// <summary>
    /// The node has been unlocked by the player.
    /// </summary>
    /// <remarks>
    /// <para>At least one rank has been invested in this node.</para>
    /// <para>Displayed with the unlocked indicator ("[x]") and unlocked color (Green).</para>
    /// </remarks>
    Unlocked = 2
}
