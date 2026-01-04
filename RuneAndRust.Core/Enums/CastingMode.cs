namespace RuneAndRust.Core.Enums;

/// <summary>
/// Determines how a Mystic casts a spell, affecting resonance gain and cast time.
/// </summary>
public enum CastingMode
{
    /// <summary>
    /// Instant cast as bonus action. +15 Resonance, +5 Flux.
    /// </summary>
    Quick = 0,

    /// <summary>
    /// Standard single-turn cast. +10 Resonance, normal Flux.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Extended two-turn cast. +5 Resonance, -5 Flux.
    /// </summary>
    Channeled = 2,

    /// <summary>
    /// Out-of-combat ritual. +0 Resonance, -10 Flux. Cannot be used in combat.
    /// </summary>
    Ritual = 3
}
