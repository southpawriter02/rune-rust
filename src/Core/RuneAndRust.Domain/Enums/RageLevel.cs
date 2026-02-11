// ═══════════════════════════════════════════════════════════════════════════════
// RageLevel.cs
// Classification levels for the Berserkr's Rage resource, mapping Rage point
// ranges to named states with mechanical effects.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies the current Rage level of a Berserkr into named tiers.
/// </summary>
/// <remarks>
/// <para>
/// Each level corresponds to a specific Rage point range and carries
/// increasing mechanical effects. Rage at 80+ (Enraged/Berserk)
/// triggers Corruption risk on ability usage, reflecting the Heretical
/// nature of the Berserkr path.
/// </para>
/// <list type="bullet">
///   <item><description><b>Calm (0–19):</b> No bonuses, no risk</description></item>
///   <item><description><b>Irritated (20–39):</b> Minor combat awareness</description></item>
///   <item><description><b>Angry (40–59):</b> Growing aggression, minor bonuses</description></item>
///   <item><description><b>Furious (60–79):</b> Significant combat bonuses</description></item>
///   <item><description><b>Enraged (80–99):</b> Peak power, Corruption risk begins</description></item>
///   <item><description><b>Berserk (100):</b> Maximum power, highest Corruption risk</description></item>
/// </list>
/// </remarks>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="BerserkrCorruptionTrigger"/>
public enum RageLevel
{
    /// <summary>
    /// Rage 0–19. No mechanical bonuses or penalties.
    /// The Berserkr is controlled and composed.
    /// </summary>
    Calm = 0,

    /// <summary>
    /// Rage 20–39. Minor combat awareness.
    /// The Berserkr begins to feel the stirrings of battle fury.
    /// </summary>
    Irritated = 1,

    /// <summary>
    /// Rage 40–59. Growing aggression with minor bonuses.
    /// The Berserkr's strikes carry mounting force.
    /// </summary>
    Angry = 2,

    /// <summary>
    /// Rage 60–79. Significant combat bonuses.
    /// The Berserkr fights with reckless intensity.
    /// </summary>
    Furious = 3,

    /// <summary>
    /// Rage 80–99. Peak combat power. Corruption risk begins.
    /// Abilities used at this level may trigger +1 Corruption.
    /// Entering combat at this level triggers Corruption risk.
    /// </summary>
    Enraged = 4,

    /// <summary>
    /// Rage 100 (maximum). Highest combat power and Corruption risk.
    /// The Berserkr is consumed by fury, gaining maximum bonuses
    /// but incurring the greatest risk of Corruption.
    /// </summary>
    Berserk = 5
}
