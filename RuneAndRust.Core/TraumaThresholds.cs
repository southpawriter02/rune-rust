namespace RuneAndRust.Core;

/// <summary>
/// Psychic Stress thresholds for UI color coding and warnings
/// </summary>
public enum StressThreshold
{
    Safe = 0,      // 0-25: No effects (green)
    Strained = 1,  // 26-50: Minor warnings (yellow)
    Severe = 2,    // 51-75: Serious warnings (orange)
    Critical = 3   // 76-100: Breaking Point imminent (red, pulsing)
}

/// <summary>
/// Runic Blight Corruption thresholds for UI color coding and warnings
/// </summary>
public enum CorruptionThreshold
{
    Minimal = 0,   // 0-20: Baseline (green)
    Low = 1,       // 21-40: Minor warnings (yellow)
    Moderate = 2,  // 41-60: Serious warnings (orange)
    High = 3,      // 61-80: Critical warnings (red)
    Extreme = 4    // 81-100: Terminal Error imminent (red, pulsing)
}
