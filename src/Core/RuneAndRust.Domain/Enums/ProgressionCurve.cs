namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type of progression curve for XP requirements.
/// </summary>
public enum ProgressionCurve
{
    /// <summary>
    /// Linear progression: each level requires the same additional XP.
    /// Formula: (level - 1) * baseXpRequirement
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential progression: each level requires more XP than the last.
    /// Formula: cumulative sum of (baseXp * multiplier^(level-2))
    /// </summary>
    Exponential,

    /// <summary>
    /// Custom progression: XP requirements are defined per-level in LevelOverrides.
    /// Falls back to linear for undefined levels.
    /// </summary>
    Custom
}
