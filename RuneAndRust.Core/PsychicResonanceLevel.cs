namespace RuneAndRust.Core;

/// <summary>
/// Psychic Resonance levels for environmental stress zones
/// </summary>
public enum PsychicResonanceLevel
{
    None = 0,       // No stress accumulation
    Light = 5,      // +5 Stress per turn
    Moderate = 10,  // +10 Stress per turn
    Heavy = 15,     // +15 Stress per turn
    Overwhelming = 20  // +20 Stress per turn (extreme psychic zones)
}
