// ------------------------------------------------------------------------------
// <copyright file="InterrogationMethod.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the five interrogation methods available during an interrogation session.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The five interrogation methods available during an interrogation session.
/// </summary>
/// <remarks>
/// <para>
/// Each method has different DCs, reliability percentages, costs, and side effects.
/// Players choose their method based on the subject's resistance level, available
/// resources, and acceptable consequences.
/// </para>
/// <para>
/// Method selection affects information reliability - torture yields unreliable
/// information (the subject will say anything to stop the pain), while building
/// rapport through GoodCop provides highly reliable intelligence.
/// </para>
/// <para>
/// Integration with underlying systems:
/// <list type="bullet">
///   <item><description>GoodCop: Uses the Persuasion system</description></item>
///   <item><description>BadCop: Uses the Intimidation system</description></item>
///   <item><description>Deception: Uses the Deception system</description></item>
///   <item><description>Bribery: Uses the Persuasion system with resource cost</description></item>
///   <item><description>Torture: Raw attribute check, no underlying system</description></item>
/// </list>
/// </para>
/// </remarks>
public enum InterrogationMethod
{
    /// <summary>
    /// Sympathetic approach - build rapport and encourage cooperation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WILL + Rhetoric vs DC 14. Highest reliability (95%) but slowest.
    /// Best for subjects who might be willing to cooperate given the right approach.
    /// </para>
    /// <para>
    /// Mechanical details:
    /// <list type="bullet">
    ///   <item><description>Base DC: 14</description></item>
    ///   <item><description>Duration: 30 minutes per round</description></item>
    ///   <item><description>Reliability: 95%</description></item>
    ///   <item><description>Disposition change: 0 (neutral)</description></item>
    ///   <item><description>Underlying system: Persuasion</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fumble consequence: [Trust Shattered] - Subject refuses to cooperate further,
    /// all future GoodCop attempts auto-fail.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I understand your situation. Help me help you."
    /// "No one has to know we talked. This stays between us."
    /// </example>
    GoodCop = 0,

    /// <summary>
    /// Aggressive approach - intimidate and threaten to extract information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses MIGHT or WILL + Rhetoric vs DC 12. Lower reliability (80%) but faster.
    /// Causes disposition damage (-5 per round). Effective against cowardly subjects.
    /// </para>
    /// <para>
    /// Mechanical details:
    /// <list type="bullet">
    ///   <item><description>Base DC: 12</description></item>
    ///   <item><description>Duration: 15 minutes per round</description></item>
    ///   <item><description>Reliability: 80%</description></item>
    ///   <item><description>Disposition change: -5 per round</description></item>
    ///   <item><description>Underlying system: Intimidation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fumble consequence: [Challenge Accepted] - Subject defies interrogator,
    /// all future Intimidation-based methods have +4 DC penalty.
    /// </para>
    /// </remarks>
    /// <example>
    /// "We can do this the easy way or the hard way."
    /// "Your friends already talked. Your silence is pointless."
    /// </example>
    BadCop = 1,

    /// <summary>
    /// Deceptive approach - trick the subject into revealing information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WILL + Rhetoric vs DC 16 (opposed by subject's WITS). Moderate
    /// reliability (70%) due to risk of counter-deception. Slight disposition
    /// damage (-2 per round) if subject becomes suspicious.
    /// </para>
    /// <para>
    /// Mechanical details:
    /// <list type="bullet">
    ///   <item><description>Base DC: 16 (subject's WITS opposed)</description></item>
    ///   <item><description>Duration: 20 minutes per round</description></item>
    ///   <item><description>Reliability: 70%</description></item>
    ///   <item><description>Disposition change: -2 per round</description></item>
    ///   <item><description>Underlying system: Deception</description></item>
    ///   <item><description>Special: Counter-deception risk, Liar's Burden stress</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fumble consequence: [Lie Exposed] - Subject sees through deception,
    /// all future Deception attempts have -2d10 penalty with this subject.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Your accomplice already confessed everything. Confirm the details for me."
    /// "The guards intercepted your message. We know more than you think."
    /// </example>
    Deception = 2,

    /// <summary>
    /// Financial approach - offer payment for information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WILL + Rhetoric vs DC 10. High reliability (90%) with no disposition
    /// damage, but costs gold. Cost scales with subject's resistance level.
    /// </para>
    /// <para>
    /// Mechanical details:
    /// <list type="bullet">
    ///   <item><description>Base DC: 10</description></item>
    ///   <item><description>Duration: 10 minutes per round</description></item>
    ///   <item><description>Reliability: 90%</description></item>
    ///   <item><description>Disposition change: 0 (may even improve)</description></item>
    ///   <item><description>Underlying system: Persuasion</description></item>
    ///   <item><description>Requires: Gold (amount based on resistance level)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Bribery costs by resistance level:
    /// <list type="bullet">
    ///   <item><description>Minimal: 10-25 gold</description></item>
    ///   <item><description>Low: 25-50 gold</description></item>
    ///   <item><description>Moderate: 50-100 gold</description></item>
    ///   <item><description>High: 100-200 gold</description></item>
    ///   <item><description>Extreme: 200-500 gold</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fumble consequence: [Trust Shattered] - Offer insulted the subject,
    /// Bribery no longer works and disposition drops.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Everyone has a price. Name yours."
    /// "Fifty gold could change your circumstances."
    /// </example>
    Bribery = 3,

    /// <summary>
    /// Extreme approach - physical coercion to extract information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses raw MIGHT or WILL vs DC = Subject's WILL × 2. Lowest reliability
    /// (50%) because subjects will say anything to stop the pain. Severe
    /// consequences including -30 faction reputation, subject trauma, and
    /// potential death on fumble.
    /// </para>
    /// <para>
    /// Mechanical details:
    /// <list type="bullet">
    ///   <item><description>Base DC: Subject's WILL × 2</description></item>
    ///   <item><description>Duration: 60 minutes per round</description></item>
    ///   <item><description>Reliability: 50% (CAPPED - never higher)</description></item>
    ///   <item><description>Disposition change: -20 per round</description></item>
    ///   <item><description>Reputation cost: -30 per session</description></item>
    ///   <item><description>No underlying system (raw attribute check)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Special rules:
    /// <list type="bullet">
    ///   <item><description>Using Torture at ANY point caps reliability at 60%</description></item>
    ///   <item><description>Subject gains [Traumatized] condition permanently</description></item>
    ///   <item><description>Some factions become permanently hostile</description></item>
    ///   <item><description>Companions may leave or refuse orders</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fumble consequence: [Subject Broken] - Subject dies, goes insane, or
    /// becomes catatonic. No information can ever be extracted. Additional
    /// -20 reputation loss (total -50).
    /// </para>
    /// </remarks>
    /// <example>
    /// "This can stop whenever you decide to talk."
    /// [No dialogue - physical coercion]
    /// </example>
    Torture = 4
}
