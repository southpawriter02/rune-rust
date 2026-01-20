// ═══════════════════════════════════════════════════════════════════════════════
// ComboStepState.cs
// Enum representing the visual state of a combo step.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Enums;

/// <summary>
/// Represents the visual state of a combo step for UI rendering.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="DTOs.ComboStepDisplayDto"/> to determine how each step
/// in a combo chain should be displayed.
/// </para>
/// <para>
/// Step states map to visual symbols and colors:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Completed"/> - Green checkmark (✓)</description></item>
///   <item><description><see cref="Current"/> - Yellow arrow (&gt;)</description></item>
///   <item><description><see cref="Pending"/> - Gray question mark (?)</description></item>
/// </list>
/// </remarks>
public enum ComboStepState
{
    /// <summary>
    /// Step has been successfully executed.
    /// </summary>
    /// <remarks>
    /// Displayed with a checkmark (✓) in green.
    /// </remarks>
    Completed,

    /// <summary>
    /// Step is the next one to execute.
    /// </summary>
    /// <remarks>
    /// Displayed with an arrow (&gt;) in yellow.
    /// This is the player's current action target.
    /// </remarks>
    Current,

    /// <summary>
    /// Step is a future step in the sequence.
    /// </summary>
    /// <remarks>
    /// Displayed with a question mark (?) in gray.
    /// These steps become Current as the combo progresses.
    /// </remarks>
    Pending
}
