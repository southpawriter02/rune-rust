namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.4: Atmospheric category types
/// Five sensory categories for environmental atmosphere
/// </summary>
public enum AtmosphericCategory
{
    Lighting,           // Visual brightness, color, quality
    Sound,              // Auditory environment
    Smell,              // Olfactory environment
    Temperature,        // Thermal/tactile sensations
    PsychicPresence    // Runic/metaphysical atmosphere
}

/// <summary>
/// v0.38.4: Atmospheric intensity levels
/// Determines how prominent/overwhelming the sensory details are
/// </summary>
public enum AtmosphericIntensity
{
    Subtle,      // Barely noticeable, background detail
    Moderate,    // Clearly present, affects mood
    Oppressive   // Overwhelming, dominates experience
}
