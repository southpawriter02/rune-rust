namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the type of rest being performed.
/// Determines recovery amounts and resource requirements.
/// </summary>
/// <remarks>See: SPEC-REST-001, Section "Rest Types".</remarks>
public enum RestType
{
    /// <summary>
    /// Resting in the wilderness without the protection of a Runic Anchor.
    /// Requires supplies (Ration + Water). Provides partial recovery.
    /// Subject to ambush risk (v0.3.2b).
    /// </summary>
    Wilderness = 0,

    /// <summary>
    /// Resting at a Runic Anchor (Sanctuary).
    /// Safe, full recovery, no supplies required.
    /// Unlocks progression features (saga command).
    /// </summary>
    Sanctuary = 1
}
