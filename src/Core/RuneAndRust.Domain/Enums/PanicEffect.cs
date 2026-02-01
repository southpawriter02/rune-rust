// ═══════════════════════════════════════════════════════════════════════════════
// PanicEffect.cs
// Represents the possible effects from a Panic Table roll during Ruin Madness.
// When a character in the RuinMadness CPS stage experiences a stress-inducing
// event (combat start, horror encounter), they roll d10 on the Panic Table
// to determine the involuntary psychological response.
// Version: 0.18.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the possible effects from a Panic Table roll.
/// </summary>
/// <remarks>
/// <para>
/// The Panic Table is triggered when a character in RuinMadness stage
/// experiences a stress-inducing event (combat start, horror encounter).
/// Roll d10 to determine the effect.
/// </para>
/// <para>
/// Panic Table (d10):
/// <list type="bullet">
///   <item><description>1: Frozen — Cannot act for 1 round</description></item>
///   <item><description>2: Scream — Involuntary scream alerts enemies</description></item>
///   <item><description>3: Flee — Must attempt to flee combat</description></item>
///   <item><description>4: Fetal — Drop prone, disadvantage until recovered</description></item>
///   <item><description>5: Blackout — Fall unconscious for 1d4 rounds</description></item>
///   <item><description>6: Denial — Cannot perceive the threatening entity</description></item>
///   <item><description>7: Violence — Attack nearest creature (friend or foe)</description></item>
///   <item><description>8: Catatonia — Stunned until taking damage</description></item>
///   <item><description>9: Dissociation — Take random action</description></item>
///   <item><description>10: None — Lucky break, no panic effect</description></item>
/// </list>
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-9) for stable serialization.
/// The ordering places None at 0 for default/fallback, with panic effects 1-9
/// matching the d10 roll values for direct mapping.
/// </para>
/// </remarks>
/// <seealso cref="CpsStage"/>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.CpsState"/>
public enum PanicEffect
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PANIC TABLE EFFECTS (d10 roll results)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// No panic effect (rolled 10 on Panic Table).
    /// </summary>
    /// <remarks>
    /// A lucky break — the character maintains composure despite their
    /// fractured mental state. No forced action or status effect.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Character freezes and cannot act for 1 round.
    /// </summary>
    /// <remarks>
    /// The character's mind locks up. They lose their next turn
    /// but can still be moved by allies.
    /// </remarks>
    Frozen = 1,

    /// <summary>
    /// Character screams involuntarily, alerting nearby enemies.
    /// </summary>
    /// <remarks>
    /// Any unaware enemies within hearing range become alerted.
    /// May trigger additional encounters in exploration mode.
    /// </remarks>
    Scream = 2,

    /// <summary>
    /// Character must attempt to flee combat immediately.
    /// </summary>
    /// <remarks>
    /// The character must use their action to Disengage and move
    /// away from all threats. Lasts until they leave combat or
    /// pass a WILL check (DC 2) at start of their turn.
    /// </remarks>
    Flee = 3,

    /// <summary>
    /// Character drops prone and has disadvantage until recovered.
    /// </summary>
    /// <remarks>
    /// The character curls into fetal position. They are Prone
    /// and have disadvantage on all checks until they use an
    /// action to stand and compose themselves.
    /// </remarks>
    Fetal = 4,

    /// <summary>
    /// Character falls unconscious for 1d4 rounds.
    /// </summary>
    /// <remarks>
    /// The mind shuts down temporarily. Character is unconscious
    /// and cannot be woken by normal means during this duration.
    /// </remarks>
    Blackout = 5,

    /// <summary>
    /// Character cannot perceive the threatening entity.
    /// </summary>
    /// <remarks>
    /// The mind refuses to acknowledge the threat. Character cannot
    /// target or react to the triggering entity for 1d4 rounds.
    /// They may still be harmed by it.
    /// </remarks>
    Denial = 6,

    /// <summary>
    /// Character attacks the nearest creature regardless of allegiance.
    /// </summary>
    /// <remarks>
    /// Panic-induced violence. On their next turn, the character must
    /// attack the nearest creature (friend or foe) with their best
    /// available attack.
    /// </remarks>
    Violence = 7,

    /// <summary>
    /// Character is stunned until they take damage.
    /// </summary>
    /// <remarks>
    /// Complete mental shutdown. Character is Stunned and remains
    /// so until they take any amount of damage (waking them up).
    /// </remarks>
    Catatonia = 8,

    /// <summary>
    /// Character takes a random action determined by GM/system.
    /// </summary>
    /// <remarks>
    /// Mind and body disconnect. On their turn, the character takes
    /// a random action (move in random direction, use random ability,
    /// speak nonsense). Duration: 1 round.
    /// </remarks>
    Dissociation = 9
}
