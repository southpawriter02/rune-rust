namespace RuneAndRust.Core.Enums;

/// <summary>
/// Threat classification for enemy scaling and encounter balancing.
/// Determines base stat multipliers when enemies are created from templates.
/// </summary>
public enum ThreatTier
{
    /// <summary>
    /// Weak fodder enemies. 0.6x stat multiplier.
    /// Examples: Utility Servitors, Scrap-Mites.
    /// </summary>
    Minion = 0,

    /// <summary>
    /// Normal combat threats. 1.0x stat multiplier (baseline).
    /// Examples: Rusted Draugr, Ash-Vargr, Rust-Clan Scav.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Enhanced enemies with traits (v0.2.2c). 1.5x stat multiplier.
    /// Examples: Iron-Bound Draugr, Alpha Vargr.
    /// </summary>
    Elite = 2,

    /// <summary>
    /// Fixed high stats, multi-phase encounters. 2.5x fixed multiplier.
    /// Examples: Haugbui Warlord, Corrupted Overseer.
    /// </summary>
    Boss = 3
}
