// ═══════════════════════════════════════════════════════════════════════════════
// SkjaldmaerAbilityId.cs
// Strongly-typed identifiers for all Skjaldmær specialization abilities,
// organized by tier.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies all abilities available to the Skjaldmær specialization.
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
public enum SkjaldmaerAbilityId
{
    // ═══════ Tier 1: Foundational Defenses (Free, Rank 1) ═══════

    /// <summary>
    /// Stance that grants +3 Defense to self and +1 Defense to adjacent allies.
    /// Costs 2 AP. Requires a shield equipped.
    /// </summary>
    ShieldWall = 1,

    /// <summary>
    /// Reaction that redirects an attack targeting an ally within 2 spaces
    /// to the Skjaldmær instead. Costs 1 Block Charge.
    /// </summary>
    Intercept = 2,

    /// <summary>
    /// Passive that grants +5 Max HP per Block Charge held (max +15 HP).
    /// </summary>
    Bulwark = 3,

    // ═══════ Tier 2: Advanced Techniques (4 PP each, Rank 2) ═══════

    /// <summary>
    /// Active ability that prevents enemy movement through adjacent spaces.
    /// </summary>
    HoldTheLine = 4,

    /// <summary>
    /// Reaction that deals damage to an attacker when they are blocked.
    /// </summary>
    CounterShield = 5,

    /// <summary>
    /// Support ability that grants allies a bonus to saving throws.
    /// </summary>
    Rally = 6,

    // ═══════ Tier 3: Master Defenses (5 PP each, Rank 3) ═══════

    /// <summary>
    /// Passive that provides damage reduction on all incoming attacks.
    /// </summary>
    Unbreakable = 7,

    /// <summary>
    /// Reaction that absorbs damage intended for an adjacent ally.
    /// </summary>
    GuardiansSacrifice = 8,

    // ═══════ Capstone: Ultimate (6 PP, Rank 4) ═══════

    /// <summary>
    /// Once per rest, survive a lethal blow at 1 HP and grant allies temporary HP.
    /// </summary>
    TheWallLives = 9
}
