namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the quality tier of equipment items.
/// </summary>
/// <remarks>
/// <para>
/// Quality tiers determine the base stat ranges, drop rarity, and visual
/// presentation of equipment. Higher tiers have better stats but are rarer.
/// </para>
/// <para>
/// The five tiers follow Norse-inspired naming conventions:
/// - Jury-Rigged: Makeshift equipment cobbled together from scraps
/// - Scavenged: Common finds, functional but unremarkable
/// - Clan-Forged: Quality craftsmanship from regional smiths
/// - Optimized: Exceptional gear fine-tuned for combat
/// - Myth-Forged: Legendary items of mythic origin
/// </para>
/// <para>
/// Tier values are explicitly assigned (0-4) to ensure stable serialization
/// and database storage. New tiers should be added at the end if needed.
/// </para>
/// </remarks>
public enum QualityTier
{
    /// <summary>
    /// Tier 0: Makeshift equipment cobbled together from available materials.
    /// </summary>
    /// <remarks>
    /// Jury-rigged items have the lowest stats and no special properties.
    /// They are common in early game and serve as placeholders until
    /// better equipment is found.
    /// </remarks>
    JuryRigged = 0,

    /// <summary>
    /// Tier 1: Common equipment scavenged from the environment.
    /// </summary>
    /// <remarks>
    /// Scavenged items are functional but unremarkable. They represent
    /// standard equipment found on defeated enemies or in basic containers.
    /// </remarks>
    Scavenged = 1,

    /// <summary>
    /// Tier 2: Quality craftsmanship from regional clan smiths.
    /// </summary>
    /// <remarks>
    /// Clan-forged items show skilled craftsmanship and often bear clan
    /// markings. They have noticeably better stats than scavenged gear.
    /// </remarks>
    ClanForged = 2,

    /// <summary>
    /// Tier 3: Exceptional gear optimized for combat effectiveness.
    /// </summary>
    /// <remarks>
    /// Optimized items have been refined for maximum performance. They
    /// represent the best non-legendary equipment available and may have
    /// minor special properties.
    /// </remarks>
    Optimized = 3,

    /// <summary>
    /// Tier 4: Legendary items of mythic origin with unique properties.
    /// </summary>
    /// <remarks>
    /// Myth-forged items are unique artifacts with special effects that
    /// can define a playstyle. Only one of each myth-forged item can
    /// drop per run to maintain their legendary status.
    /// </remarks>
    MythForged = 4
}
