namespace RuneAndRust.Core.Enums;

/// <summary>
/// Corruption thresholds representing the caster's soul degradation.
/// Corruption accumulates from Catastrophic backlash and never heals naturally.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// Corruption is the long-term cost of magical failure. Each Catastrophic backlash
/// adds +1 corruption, slowly pushing the caster toward being "Lost".
/// </remarks>
public enum CorruptionLevel
{
    /// <summary>
    /// 0-9 corruption. No penalties. Soul remains pristine.
    /// "Your soul remains untouched by the weave's corruption."
    /// </summary>
    Untouched = 0,

    /// <summary>
    /// 10-24 corruption. -1 MaxAP (10% reduction).
    /// "A faint darkness lingers at the edge of your vision."
    /// </summary>
    Tainted = 1,

    /// <summary>
    /// 25-49 corruption. -2 MaxAP (20% reduction), -1 Will.
    /// "The corruption whispers in quiet moments."
    /// </summary>
    Afflicted = 2,

    /// <summary>
    /// 50-74 corruption. -3 MaxAP (30% reduction), -2 Will, -1 Wits.
    /// "Your flesh bears the marks of arcane scarring."
    /// </summary>
    Blighted = 3,

    /// <summary>
    /// 75-100 corruption. Cannot cast spells. Soul consumed.
    /// "Your soul has been consumed. Magic no longer answers."
    /// </summary>
    Lost = 4
}
