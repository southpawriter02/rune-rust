// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderAbilityId.cs
// Strongly-typed identifiers for all Jötun-Reader specialization abilities,
// organized by tier.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies all abilities available to the Jötun-Reader specialization.
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
public enum JotunReaderAbilityId
{
    // ═══════ Tier 1: Knowledge Foundation (Free, Rank 1) ═══════

    /// <summary>
    /// Active: adds +2d10 to examine machinery, terminals, or Jötun technology.
    /// Costs 2 AP. Generates Lore Insight on success (1 normal, 2 on Master).
    /// </summary>
    DeepScan = 1,

    /// <summary>
    /// Passive: automatically succeeds on Layer 2 examinations for all Jötun
    /// technology. Generates +1 Lore Insight on first examination of any
    /// Jötun tech object.
    /// </summary>
    PatternRecognition = 2,

    /// <summary>
    /// Passive: grants comprehension of Jötun Formal, Jötun Technical,
    /// Dvergr Standard, and Dvergr Runic scripts. No translation check
    /// required; understanding is instantaneous and complete.
    /// </summary>
    AncientTongues = 3,

    // ═══════ Tier 2: Applied Knowledge (4 PP each, Rank 2) ═══════

    /// <summary>
    /// Passive: perfectly recalls all previous examination results.
    /// Allows re-reading without re-examination.
    /// Implemented in v0.20.3b.
    /// </summary>
    TechnicalMemory = 4,

    /// <summary>
    /// Active: identifies and targets weak points in examined machinery.
    /// Grants bonus damage to previously examined targets.
    /// Implemented in v0.20.3b.
    /// </summary>
    ExploitWeakness = 5,

    /// <summary>
    /// Active: recovers data from damaged or corrupted terminals.
    /// Implemented in v0.20.3b.
    /// </summary>
    DataRecovery = 6,

    // ═══════ Tier 3: Master Scholar (5 PP each, Rank 3) ═══════

    /// <summary>
    /// Passive: increases Lore Insight maximum and generation rates.
    /// Implemented in v0.20.3c.
    /// </summary>
    LoreKeeper = 7,

    /// <summary>
    /// Active: applies accumulated knowledge for powerful effects.
    /// Implemented in v0.20.3c.
    /// </summary>
    AncientKnowledge = 8,

    // ═══════ Capstone: Ultimate (6 PP, Rank 4) ═══════

    /// <summary>
    /// Ultimate ability channeling the collective wisdom of the Jötun.
    /// Implemented in v0.20.3c.
    /// </summary>
    VoiceOfTheGiants = 9
}
