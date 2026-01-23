// ------------------------------------------------------------------------------
// <copyright file="ProtocolViolationType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes the types of cultural protocol violations and their severity.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the types of cultural protocol violations and their severity.
/// </summary>
/// <remarks>
/// <para>
/// Protocol violations occur when a character fails to observe the required
/// cultural formalities during social interactions. The type of violation
/// determines the consequences and recovery options.
/// </para>
/// <para>
/// Violation severity escalates from minor faux pas to severe cultural offenses:
/// <list type="bullet">
///   <item><description><see cref="None"/>: No violation occurred</description></item>
///   <item><description><see cref="Minor"/>: Small mistake (+2 DC), easily forgiven</description></item>
///   <item><description><see cref="Moderate"/>: Noticeable breach (+4 DC, -1d10), requires acknowledgment</description></item>
///   <item><description><see cref="Severe"/>: Serious offense (+6 DC, -2d10), lasting consequences</description></item>
///   <item><description><see cref="Unforgivable"/>: Permanent damage, interaction blocked</description></item>
/// </list>
/// </para>
/// <para>
/// Common causes of violations by severity:
/// <list type="bullet">
///   <item><description>Minor: Wrong honorific, minor timing error, slight formality breach</description></item>
///   <item><description>Moderate: Skipping required ritual, showing impatience, ignoring status</description></item>
///   <item><description>Severe: Insulting cultural values, refusing sacred offerings, public disrespect</description></item>
///   <item><description>Unforgivable: Desecration, betrayal, violence against protected individuals</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var violation = ProtocolViolationType.Moderate;
/// var dcIncrease = violation.GetDcIncrease(); // Returns 4
/// var dicePenalty = violation.GetDicePenalty(); // Returns 1
/// var blocked = violation.BlocksInteraction(); // Returns false
/// </code>
/// </example>
public enum ProtocolViolationType
{
    /// <summary>
    /// No violation occurred. The character successfully observed the protocol
    /// or the interaction did not require protocol compliance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the default state when:
    /// <list type="bullet">
    ///   <item><description>All protocol requirements were met</description></item>
    ///   <item><description>The target NPC has no cultural protocol</description></item>
    ///   <item><description>The interaction type doesn't require protocol compliance</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// No penalties or consequences apply.
    /// </para>
    /// </remarks>
    None = 0,

    /// <summary>
    /// A minor violation such as incorrect honorifics or slight timing errors.
    /// Easily overlooked or forgiven with a brief apology.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>DC increase: +2 on next check with this NPC</description></item>
    ///   <item><description>Dice penalty: None</description></item>
    ///   <item><description>Disposition change: -5</description></item>
    ///   <item><description>Recovery: Automatic after one successful interaction</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples of minor violations:
    /// <list type="bullet">
    ///   <item><description>Using informal address when formal is expected</description></item>
    ///   <item><description>Speaking before the proper time in a greeting ritual</description></item>
    ///   <item><description>Offering the wrong hand during a handshake</description></item>
    ///   <item><description>Mispronouncing honorific titles</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recovery: A simple apology or showing proper form in the next exchange
    /// typically resolves minor violations.
    /// </para>
    /// </remarks>
    /// <example>
    /// "You accidentally address the guild master without their proper title.
    /// They raise an eyebrow but continue the conversation."
    /// </example>
    Minor = 1,

    /// <summary>
    /// A moderate violation such as skipping required formalities or showing
    /// impatience during required rituals.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>DC increase: +4 on future checks with this NPC</description></item>
    ///   <item><description>Dice penalty: -1d10 on future checks</description></item>
    ///   <item><description>Disposition change: -15</description></item>
    ///   <item><description>Recovery: Requires explicit acknowledgment or small gesture</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples of moderate violations:
    /// <list type="bullet">
    ///   <item><description>Interrupting a formal speech or ceremony</description></item>
    ///   <item><description>Refusing a customary offering without explanation</description></item>
    ///   <item><description>Showing visible impatience during extended discourse</description></item>
    ///   <item><description>Failing to acknowledge status hierarchy</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recovery: Explicit acknowledgment of the error and a gesture of respect
    /// (such as a small gift or formal apology) are required.
    /// </para>
    /// </remarks>
    /// <example>
    /// "You interrupt the Gorge-Maw elder's extended greeting. They fall silent,
    /// rumbling displeasure. 'The young ones are always in such haste...'"
    /// </example>
    Moderate = 2,

    /// <summary>
    /// A severe violation such as direct insult to cultural values or
    /// complete disregard for sacred protocols.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>DC increase: +6 on future checks with this NPC</description></item>
    ///   <item><description>Dice penalty: -2d10 on future checks</description></item>
    ///   <item><description>Disposition change: -30</description></item>
    ///   <item><description>Recovery: Requires significant effort, gifts, or intermediary</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples of severe violations:
    /// <list type="bullet">
    ///   <item><description>Openly mocking cultural traditions or beliefs</description></item>
    ///   <item><description>Refusing to participate in mandatory rituals</description></item>
    ///   <item><description>Showing disrespect to sacred objects or spaces</description></item>
    ///   <item><description>Public accusations without proper form</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recovery requires one or more of:
    /// <list type="bullet">
    ///   <item><description>Substantial gift appropriate to the culture</description></item>
    ///   <item><description>Formal apology before witnesses</description></item>
    ///   <item><description>Completing a task set by the offended party</description></item>
    ///   <item><description>Intervention by a respected third party</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Severe violations may cause the NPC to become hostile until resolved.
    /// </para>
    /// </remarks>
    /// <example>
    /// "You refuse the Iron-Bane's request for martial tribute. The warrior's
    /// hand moves to their weapon. 'You come to our hall empty-handed and
    /// expect to be heard? Leave, before I forget the laws of hospitality.'"
    /// </example>
    Severe = 3,

    /// <summary>
    /// An unforgivable violation that permanently damages the relationship.
    /// Examples include desecration of sacred objects or betrayal of trust.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>DC increase: Interaction blocked entirely</description></item>
    ///   <item><description>Dice penalty: N/A (cannot attempt interaction)</description></item>
    ///   <item><description>Disposition change: -100 (minimum/hostile)</description></item>
    ///   <item><description>Recovery: Generally impossible with this NPC</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples of unforgivable violations:
    /// <list type="bullet">
    ///   <item><description>Destroying or defiling sacred objects</description></item>
    ///   <item><description>Violence against protected individuals (children, elders, diplomats)</description></item>
    ///   <item><description>Betraying information given under oath</description></item>
    ///   <item><description>Violating the sanctity of hospitality rites</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Consequences may extend beyond the individual NPC:
    /// <list type="bullet">
    ///   <item><description>Faction reputation loss with the entire culture</description></item>
    ///   <item><description>Bounties or warrants in cultural territory</description></item>
    ///   <item><description>Loss of access to cultural services and locations</description></item>
    ///   <item><description>Other NPCs of the culture become hostile</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// "You strike the Rune-Lupin pack leader during negotiations. The entire pack
    /// turns as one, their eyes burning with fury. There will be no more words today,
    /// only the hunt."
    /// </example>
    Unforgivable = 4
}
