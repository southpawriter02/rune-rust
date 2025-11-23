namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Difficulty tier for content density scaling
/// Affects global budget calculations and enemy population
/// </summary>
public enum DifficultyTier
{
    /// <summary>
    /// Easy difficulty - reduced enemy counts (0.8× multiplier)
    /// Suitable for new players or story-focused playthroughs
    /// </summary>
    Easy,

    /// <summary>
    /// Normal difficulty - baseline experience (1.0× multiplier)
    /// Balanced challenge suitable for most players
    /// </summary>
    Normal,

    /// <summary>
    /// Hard difficulty - increased challenge (1.3× multiplier)
    /// For experienced players seeking tougher encounters
    /// </summary>
    Hard,

    /// <summary>
    /// Lethal difficulty - brutal combat (1.6× multiplier)
    /// Maximum challenge for veteran players
    /// </summary>
    Lethal
}
