namespace RuneAndRust.Presentation.Gui.Enums;

/// <summary>
/// Defines the type of damage popup to display during combat.
/// </summary>
/// <remarks>
/// Each type has associated visual styling including color and text format.
/// Used by <see cref="Controls.DamagePopupControl"/> to determine display.
/// </remarks>
public enum DamagePopupType
{
    /// <summary>
    /// Standard damage dealt to a target.
    /// Displayed as negative value (e.g., "-12") in white or gold (critical).
    /// </summary>
    Damage,

    /// <summary>
    /// Healing received by a target.
    /// Displayed as positive value (e.g., "+15") in green.
    /// </summary>
    Healing,

    /// <summary>
    /// Attack missed the target.
    /// Displayed as "MISS" in gray.
    /// </summary>
    Miss,

    /// <summary>
    /// Attack was blocked by the target.
    /// Displayed as "BLOCKED" in steel blue.
    /// </summary>
    Block
}
