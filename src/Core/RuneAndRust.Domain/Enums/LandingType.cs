namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies the type of landing surface or target for a leap.
/// </summary>
/// <remarks>
/// <para>
/// Landing type affects the difficulty of a leap. Precision landings on small
/// targets or corrupted surfaces increase the DC, while normal landings have
/// no modifier.
/// </para>
/// </remarks>
public enum LandingType
{
    /// <summary>
    /// Normal landing on a standard surface (5+ feet wide).
    /// DC modifier: +0.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Precision landing on a small target (less than 5 feet wide).
    /// DC modifier: +1.
    /// </summary>
    /// <remarks>
    /// Examples: narrow ledge, small platform, single beam.
    /// </remarks>
    Precision = 1,

    /// <summary>
    /// Landing on a corruption-affected [Glitched] surface.
    /// DC modifier: +2.
    /// </summary>
    /// <remarks>
    /// The landing zone is unstable due to corruption effects.
    /// Distances may fluctuate, surfaces may shift.
    /// </remarks>
    Glitched = 2,

    /// <summary>
    /// Precision landing on a Glitched surface (combines both penalties).
    /// DC modifier: +3 (Precision +1, Glitched +2).
    /// </summary>
    PrecisionGlitched = 3,

    /// <summary>
    /// Landing at a lower elevation (jumping down).
    /// DC modifier: -1.
    /// </summary>
    /// <remarks>
    /// Gravity assists the leap. Still triggers fall damage if landing zone missed.
    /// </remarks>
    Downward = 4
}
