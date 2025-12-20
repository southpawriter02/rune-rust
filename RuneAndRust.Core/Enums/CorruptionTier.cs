namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the corruption tier of a character.
/// Corruption is a permanent, accumulating consequence of exposure to the Runic Blight.
/// Unlike Stress, Corruption does not heal naturally and imposes permanent penalties.
/// </summary>
public enum CorruptionTier
{
    /// <summary>
    /// Corruption 0-20. No mechanical penalties. Soul remains untainted.
    /// </summary>
    Pristine = 0,

    /// <summary>
    /// Corruption 21-40. Minor visual glitches in UI. The Blight's mark is visible.
    /// </summary>
    Tainted = 1,

    /// <summary>
    /// Corruption 41-60. -10% Max AP. The corruption interferes with Aetheric channeling.
    /// </summary>
    Corrupted = 2,

    /// <summary>
    /// Corruption 61-80. -20% Max AP, -1 WILL. The soul begins to fracture.
    /// </summary>
    Blighted = 3,

    /// <summary>
    /// Corruption 81-99. -40% Max AP, -2 WILL, -1 WITS. Near total dissolution.
    /// </summary>
    Fractured = 4,

    /// <summary>
    /// Corruption 100. Terminal state. Character becomes a Forlorn (unplayable).
    /// </summary>
    Terminal = 5
}
