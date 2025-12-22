namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of visual effects that can be triggered during gameplay (v0.3.9a).
/// Used by VisualEffectService to determine border flash colors and animations.
/// </summary>
public enum VisualEffectType
{
    /// <summary>
    /// No visual effect.
    /// </summary>
    None = 0,

    /// <summary>
    /// Red border flash when player or enemy takes damage.
    /// </summary>
    DamageFlash = 1,

    /// <summary>
    /// Gold border flash on critical hit.
    /// </summary>
    CriticalFlash = 2,

    /// <summary>
    /// Green border flash when healing occurs.
    /// </summary>
    HealFlash = 3,

    /// <summary>
    /// Purple border flash on trauma or stress events.
    /// </summary>
    TraumaFlash = 4,

    /// <summary>
    /// Bright gold flash on combat victory.
    /// </summary>
    VictoryFlash = 5
}
