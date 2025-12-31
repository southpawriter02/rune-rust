namespace RuneAndRust.Core.Enums;

/// <summary>
/// Severity levels for magical backlash when casting in high-Flux environments.
/// Backlash occurs when Flux exceeds the Critical threshold (50) and the caster fails a risk roll.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// Severity is determined by the fail margin (RiskChance - Roll):
/// - Minor: 1-10 margin
/// - Major: 11-25 margin
/// - Catastrophic: 26+ margin
/// </remarks>
public enum BacklashSeverity
{
    /// <summary>
    /// No backlash occurred - safe casting.
    /// Either Flux was below critical threshold or risk roll succeeded.
    /// </summary>
    None = 0,

    /// <summary>
    /// Minor backlash: 1d6 damage to caster.
    /// Triggered when fail margin is 1-10.
    /// The weave snaps back, stinging the caster.
    /// </summary>
    Minor = 1,

    /// <summary>
    /// Major backlash: 2d6 damage + Aether Sickness (2 turns).
    /// Triggered when fail margin is 11-25.
    /// Arcane energies tear through the caster, leaving them reeling.
    /// </summary>
    Major = 2,

    /// <summary>
    /// Catastrophic backlash: 3d6 damage + Aether Sickness (5 turns) + 1 Corruption.
    /// Triggered when fail margin is 26+.
    /// Reality itself rejects the spell. Corruption takes root in the soul.
    /// </summary>
    Catastrophic = 3
}
