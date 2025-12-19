namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the craftsmanship quality of items in the world.
/// Higher tiers provide better stats but are exponentially rarer.
/// </summary>
public enum QualityTier
{
    /// <summary>
    /// Hastily repaired or barely functional equipment.
    /// Common in Safe zones. Lowest stats, prone to breakage.
    /// </summary>
    JuryRigged = 0,

    /// <summary>
    /// Standard salvaged goods recovered from ruins.
    /// The baseline tier found throughout the world.
    /// </summary>
    Scavenged = 1,

    /// <summary>
    /// Properly crafted by Dvergr smiths or equivalent artisans.
    /// Reliable quality with consistent performance.
    /// </summary>
    ClanForged = 2,

    /// <summary>
    /// Pre-Glitch technology or masterwork craftsmanship.
    /// Rare finds in dangerous areas. Superior stats.
    /// </summary>
    Optimized = 3,

    /// <summary>
    /// Legendary artifacts of unknown origin.
    /// Extraordinarily rare. Found only in Lethal zones.
    /// </summary>
    MythForged = 4
}
