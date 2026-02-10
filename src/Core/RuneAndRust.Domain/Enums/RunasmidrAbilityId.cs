// ═══════════════════════════════════════════════════════════════════════════════
// RunasmidrAbilityId.cs
// Strongly-typed identifiers for all Rúnasmiðr specialization abilities,
// organized by tier.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies all abilities available to the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Abilities are organized across four tiers, each requiring increasing
/// Proficiency Point (PP) investment to unlock:
/// </para>
/// <list type="bullet">
///   <item><description><b>Tier 1 (1–3):</b> Free, unlocked immediately</description></item>
///   <item><description><b>Tier 2 (4–6):</b> 4 PP each, requires 8 PP invested</description></item>
///   <item><description><b>Tier 3 (7–8):</b> 5 PP each, requires 16 PP invested</description></item>
///   <item><description><b>Capstone (9):</b> 6 PP, requires 24 PP invested</description></item>
/// </list>
/// </remarks>
/// <seealso cref="SpecializationId"/>
public enum RunasmidrAbilityId
{
    // ═══════ Tier 1: Foundational Inscription (Free, Rank 1) ═══════

    /// <summary>
    /// Inscribes a rune on a weapon (+2 damage) or armor (+1 Defense) for 10 turns.
    /// Costs 3 AP and 1 Rune Charge. Only one rune per item at a time.
    /// </summary>
    InscribeRune = 1,

    /// <summary>
    /// Passive: automatically identifies Jötun technology when examined.
    /// Bypasses Perception/Lore checks and reveals function, origin, and lore.
    /// </summary>
    ReadTheMarks = 2,

    /// <summary>
    /// Creates a protective ward that absorbs up to 10 damage.
    /// Costs 2 AP and 1 Rune Charge. Maximum 1 active ward per character.
    /// </summary>
    RunestoneWard = 3,

    // ═══════ Tier 2: Advanced Techniques (4 PP each, Rank 2) ═══════

    /// <summary>
    /// Enhanced inscription that adds elemental damage to weapons.
    /// Implemented in v0.20.2b.
    /// </summary>
    EmpoweredInscription = 4,

    /// <summary>
    /// Places a runic trap on a surface that triggers on enemy contact.
    /// Implemented in v0.20.2b.
    /// </summary>
    RunicTrap = 5,

    /// <summary>
    /// Passive: reduces material costs for all crafting by 25%.
    /// Implemented in v0.20.2b.
    /// </summary>
    DvergrTechniques = 6,

    // ═══════ Tier 3: Master Inscription (5 PP each, Rank 3) ═══════

    /// <summary>
    /// Passive: doubles the duration of all inscribed runes.
    /// Implemented in v0.20.2c.
    /// </summary>
    MasterScrivener = 7,

    /// <summary>
    /// Creates animated rune constructs that fight alongside the character.
    /// Implemented in v0.20.2c.
    /// </summary>
    LivingRunes = 8,

    // ═══════ Capstone: Ultimate (6 PP, Rank 4) ═══════

    /// <summary>
    /// Dispels all active effects in an area, destroying enemy enchantments.
    /// Implemented in v0.20.2c.
    /// </summary>
    WordOfUnmaking = 9
}
