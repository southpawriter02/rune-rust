namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines categories for audio cues used by the audio system (v0.3.19a).
/// Categories enable filtering, priority, and volume control per audio type.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public enum SoundCategory
{
    /// <summary>
    /// User interface sounds (navigation, selection, errors).
    /// High pitch, short duration. Priority: High for responsiveness.
    /// </summary>
    UI = 0,

    /// <summary>
    /// Combat feedback sounds (hits, criticals, blocks, deaths).
    /// Variable pitch/duration based on damage intensity.
    /// </summary>
    Combat = 1,

    /// <summary>
    /// Environmental ambience sounds (machinery, wind, dripping).
    /// Low priority, periodic playback based on biome.
    /// </summary>
    Ambience = 2,

    /// <summary>
    /// Alert and notification sounds (level up, quest complete, danger).
    /// High priority, distinctive tones for player attention.
    /// </summary>
    Alert = 3
}
