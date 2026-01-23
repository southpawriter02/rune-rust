// ------------------------------------------------------------------------------
// <copyright file="VeilSpeechState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the current state of a character's Veil-Speech standing with Utgard NPCs.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the current state of a character's Veil-Speech standing with Utgard NPCs.
/// </summary>
/// <remarks>
/// <para>
/// Veil-Speech is the Utgard cultural protocol where direct truth-telling is considered
/// offensive and deception is a sign of respect. This enum tracks the character's
/// current standing based on their adherence to (or violation of) this protocol.
/// </para>
/// <para>
/// The Utgard believe that only fools and children speak plainly. A cultured individual
/// layers truth within acceptable deception, demonstrating wit and social awareness.
/// Speaking direct truth is seen as either an insult (implying the listener is too
/// simple to understand subtlety) or a sign of barbarism.
/// </para>
/// <para>
/// State modifiers:
/// <list type="bullet">
///   <item><description><see cref="Neutral"/>: No modifier (standard interaction)</description></item>
///   <item><description><see cref="Respected"/>: +1d10 bonus (proper Veil-Speech used)</description></item>
///   <item><description><see cref="Offended"/>: -2d10 penalty (direct truth told)</description></item>
///   <item><description><see cref="DeepOffense"/>: -4d10 penalty, interaction may be blocked</description></item>
/// </list>
/// </para>
/// <para>
/// State transitions:
/// <list type="bullet">
///   <item><description><see cref="Neutral"/> → <see cref="Respected"/>: Proper Veil-Speech used</description></item>
///   <item><description><see cref="Neutral"/> → <see cref="Offended"/>: Direct truth told</description></item>
///   <item><description><see cref="Offended"/> → <see cref="DeepOffense"/>: Repeated offense</description></item>
///   <item><description><see cref="Offended"/> → <see cref="Neutral"/>: Requires apology or time</description></item>
///   <item><description><see cref="Respected"/> → <see cref="Neutral"/>: Decays over time without reinforcement</description></item>
/// </list>
/// </para>
/// <para>
/// Special rule: When using the Deception interaction type with Utgard NPCs, the DC is
/// reduced by 4 because deception is culturally respected.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var state = VeilSpeechState.Respected;
/// var modifier = state.GetDiceModifier(); // Returns +1
/// var description = state.GetDescription(); // Returns "Respected (+1d10)"
/// </code>
/// </example>
public enum VeilSpeechState
{
    /// <summary>
    /// Default state. The character has neither impressed nor offended the Utgard NPC
    /// with their speech patterns. Standard interaction rules apply.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the starting state for all characters interacting with Utgard NPCs.
    /// No modifier is applied to dice pools or DCs.
    /// </para>
    /// <para>
    /// Characters in the Neutral state:
    /// <list type="bullet">
    ///   <item><description>Can attempt normal social interactions</description></item>
    ///   <item><description>Are evaluated on a per-interaction basis</description></item>
    ///   <item><description>Can improve to Respected by using proper Veil-Speech</description></item>
    ///   <item><description>Risk falling to Offended by speaking direct truth</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Neutral = 0,

    /// <summary>
    /// The character has demonstrated proper Veil-Speech, layering truth within
    /// acceptable deception. This grants a +1d10 bonus on the current check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state is typically temporary, applying to the current interaction and
    /// potentially extending to subsequent interactions if the character continues
    /// to demonstrate Veil-Speech proficiency.
    /// </para>
    /// <para>
    /// Characters in the Respected state:
    /// <list type="bullet">
    ///   <item><description>Receive +1d10 on all Rhetoric checks with this NPC</description></item>
    ///   <item><description>May receive preferential treatment in negotiations</description></item>
    ///   <item><description>Are viewed as culturally sophisticated</description></item>
    ///   <item><description>Can access Utgard-specific dialogue options</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The Respected state decays to Neutral if the character does not continue
    /// to use proper Veil-Speech in subsequent interactions.
    /// </para>
    /// </remarks>
    /// <example>
    /// "The merchant layers their offer within polite misdirection, speaking of
    /// 'fair exchanges' when they mean 'maximum profit.' The Utgard nods approvingly."
    /// </example>
    Respected = 1,

    /// <summary>
    /// The character has offended the Utgard NPC by speaking direct truth without
    /// the expected layers of deception. This applies a -2d10 penalty on future checks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state persists until the character makes amends or sufficient time passes.
    /// The penalty applies to all future Rhetoric checks with this specific NPC.
    /// </para>
    /// <para>
    /// Characters in the Offended state:
    /// <list type="bullet">
    ///   <item><description>Suffer -2d10 on all Rhetoric checks with this NPC</description></item>
    ///   <item><description>May be denied certain services or information</description></item>
    ///   <item><description>Are viewed as uncivilized or insulting</description></item>
    ///   <item><description>Risk escalating to DeepOffense with another violation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recovery from Offended state requires:
    /// <list type="bullet">
    ///   <item><description>A formal apology using proper Veil-Speech</description></item>
    ///   <item><description>A gift or favor appropriate to the offense</description></item>
    ///   <item><description>Sufficient time for the offense to be forgotten</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// "You bluntly state the price is too high. The Utgard's expression hardens.
    /// 'How... direct,' they say coldly. 'Perhaps you would be more comfortable
    /// dealing with the simpler folk in the outer market.'"
    /// </example>
    Offended = 2,

    /// <summary>
    /// The character has deeply offended the Utgard NPC through repeated or severe
    /// protocol violations. Social interaction may be impossible until resolved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This state represents a serious breach of cultural protocol that has severely
    /// damaged the relationship. The NPC may refuse all further interaction.
    /// </para>
    /// <para>
    /// Characters in the DeepOffense state:
    /// <list type="bullet">
    ///   <item><description>Suffer -4d10 on all Rhetoric checks with this NPC (if allowed)</description></item>
    ///   <item><description>May be completely blocked from interaction</description></item>
    ///   <item><description>Risk the NPC becoming hostile or calling guards</description></item>
    ///   <item><description>May trigger faction reputation loss with Utgard</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recovery from DeepOffense requires significant effort:
    /// <list type="bullet">
    ///   <item><description>Intervention by a respected third party</description></item>
    ///   <item><description>Completion of a quest or significant favor</description></item>
    ///   <item><description>Substantial gifts and formal apology</description></item>
    ///   <item><description>Some NPCs may never forgive this level of offense</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// "After your second blunt statement, the Utgard noble rises abruptly.
    /// 'This creature speaks as if truth were coin to be spent carelessly.
    /// I will not dignify such barbarism with further audience.'"
    /// </example>
    DeepOffense = 3
}
