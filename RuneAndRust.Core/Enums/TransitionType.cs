namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines screen transition animation types (v0.3.14b).
/// Used by IScreenTransitionService to play visual effects between game phases.
/// </summary>
/// <remarks>See: SPEC-TRANSITION-001 for Screen Transition System design.</remarks>
public enum TransitionType
{
    /// <summary>No transition - instant screen clear.</summary>
    None = 0,

    /// <summary>Screen shatters into noise - Combat start.</summary>
    Shatter = 1,

    /// <summary>Screen fades/dissolves out - Combat victory.</summary>
    Dissolve = 2,

    /// <summary>Text decays to garbage then black - Game Over (quit after death).</summary>
    GlitchDecay = 3
}
