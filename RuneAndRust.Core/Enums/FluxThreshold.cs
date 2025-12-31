namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the current threshold level of environmental flux (aetheric volatility).
/// Each threshold tier corresponds to a range of flux values and carries increasing risk.
/// </summary>
/// <remarks>
/// Thresholds (v0.4.3a - The Aether):
/// - Safe: 0-24 (stable aetheric field)
/// - Elevated: 25-49 (minor instability detected)
/// - Critical: 50-74 (significant instability)
/// - Overload: 75-100 (maximum danger, reality fraying)
/// </remarks>
public enum FluxThreshold
{
    /// <summary>
    /// Flux level 0-24. Stable aetheric field with no adverse effects.
    /// </summary>
    Safe = 0,

    /// <summary>
    /// Flux level 25-49. Minor instability detected; caution advised.
    /// </summary>
    Elevated = 1,

    /// <summary>
    /// Flux level 50-74. Significant instability; magical effects may behave erratically.
    /// </summary>
    Critical = 2,

    /// <summary>
    /// Flux level 75-100. Maximum danger; the boundary between realms grows thin.
    /// </summary>
    Overload = 3
}
