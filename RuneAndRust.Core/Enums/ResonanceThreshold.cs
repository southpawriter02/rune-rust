namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the intensity level of a Mystic's Aetheric Resonance.
/// Higher thresholds grant greater spell potency but increase Paradox risk.
/// </summary>
public enum ResonanceThreshold
{
    /// <summary>
    /// 0-24: Minimal attunement. -10% spell potency. Safe casting.
    /// </summary>
    Dim = 0,

    /// <summary>
    /// 25-49: Balanced attunement. Normal spell potency. Minor risk.
    /// </summary>
    Steady = 1,

    /// <summary>
    /// 50-74: Elevated attunement. +15% spell potency. Moderate risk.
    /// </summary>
    Bright = 2,

    /// <summary>
    /// 75-99: Intense attunement. +30% spell potency. High risk.
    /// </summary>
    Blazing = 3,

    /// <summary>
    /// 100: Critical attunement. +50% potency for 1 turn, then forced discharge.
    /// </summary>
    Overflow = 4
}
