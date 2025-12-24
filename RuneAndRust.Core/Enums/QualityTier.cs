using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Jury-Rigged Quality",
        "Hastily repaired or barely functional equipment held together by hope and scrap wire. Common in Safe zones where proper materials are scarce. Prone to breakage at the worst moments.")]
    JuryRigged = 0,

    /// <summary>
    /// Standard salvaged goods recovered from ruins.
    /// The baseline tier found throughout the world.
    /// </summary>
    [GameDocument(
        "Scavenged Quality",
        "Standard salvaged goods recovered from ruins and wreckage. The baseline quality found throughout the wastes. Functional without distinction, neither exceptional nor terrible.")]
    Scavenged = 1,

    /// <summary>
    /// Properly crafted by Dvergr smiths or equivalent artisans.
    /// Reliable quality with consistent performance.
    /// </summary>
    [GameDocument(
        "Clan-Forged Quality",
        "Properly crafted by Dvergr smiths or equivalent artisans. Reliable quality with consistent performance. Each piece bears the mark of genuine craftsmanship and professional standards.")]
    ClanForged = 2,

    /// <summary>
    /// Pre-Glitch technology or masterwork craftsmanship.
    /// Rare finds in dangerous areas. Superior stats.
    /// </summary>
    [GameDocument(
        "Optimized Quality",
        "Pre-Glitch technology or masterwork craftsmanship surpassing modern capabilities. Rare finds in dangerous areas where few dare venture. Superior statistics justify the risk of acquisition.")]
    Optimized = 3,

    /// <summary>
    /// Legendary artifacts of unknown origin.
    /// Extraordinarily rare. Found only in Lethal zones.
    /// </summary>
    [GameDocument(
        "Myth-Forged Quality",
        "Legendary artifacts of unknown origin, their crafting methods lost to time. Extraordinarily rare and found only in Lethal zones. The stories surrounding such items may be as valuable as the items themselves.")]
    MythForged = 4
}
