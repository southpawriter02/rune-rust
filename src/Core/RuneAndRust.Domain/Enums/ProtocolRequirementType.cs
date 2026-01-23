// ------------------------------------------------------------------------------
// <copyright file="ProtocolRequirementType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes the types of requirements within cultural protocols.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the types of requirements within cultural protocols.
/// </summary>
/// <remarks>
/// <para>
/// Each cultural protocol may contain multiple requirements of different types.
/// These categories help organize protocol requirements and determine what
/// actions or behaviors the character must demonstrate.
/// </para>
/// <para>
/// Requirement types by culture (typical examples):
/// <list type="bullet">
///   <item><description>Dvergr: Verbal (logical structure), Behavioral (precision)</description></item>
///   <item><description>Utgard: Verbal (veil-speech), Behavioral (accepting deception)</description></item>
///   <item><description>Gorge-Maw: Behavioral (patience), Offering (shared food)</description></item>
///   <item><description>Rune-Lupin: Mental (openness), Behavioral (suppressing hostility)</description></item>
///   <item><description>Iron-Bane: Offering (martial tribute), Behavioral (showing strength)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ProtocolRequirementType
{
    /// <summary>
    /// Actions or behaviors required during the interaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Behavioral requirements demand specific physical actions or demeanor:
    /// <list type="bullet">
    ///   <item><description>Maintaining proper posture or stance</description></item>
    ///   <item><description>Demonstrating patience during extended discourse</description></item>
    ///   <item><description>Following movement protocols (who enters first, seating order)</description></item>
    ///   <item><description>Controlling emotional displays</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Gorge-Maw: Listen patiently to extended rumbling discourse without interruption</description></item>
    ///   <item><description>Rune-Lupin: Suppress hostile emotions to allow telepathic contact</description></item>
    ///   <item><description>Iron-Bane: Maintain martial bearing, do not show weakness</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Behavioral = 0,

    /// <summary>
    /// Speech patterns or verbal formulas required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Verbal requirements demand specific speech patterns or phrases:
    /// <list type="bullet">
    ///   <item><description>Using correct honorific forms</description></item>
    ///   <item><description>Following greeting and farewell formulas</description></item>
    ///   <item><description>Maintaining logical argument structure</description></item>
    ///   <item><description>Using the appropriate cant or dialect</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Dvergr: Present arguments in precise, non-contradictory chains</description></item>
    ///   <item><description>Utgard: Layer truth within acceptable deception (Veil-Speech)</description></item>
    ///   <item><description>Iron-Bane: State martial achievements before making requests</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Verbal = 1,

    /// <summary>
    /// Physical offerings or gifts required before or during interaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Offering requirements demand tangible gifts or tributes:
    /// <list type="bullet">
    ///   <item><description>Material gifts appropriate to the culture</description></item>
    ///   <item><description>Symbolic items demonstrating respect</description></item>
    ///   <item><description>Shared resources (food, drink, materials)</description></item>
    ///   <item><description>Tributes acknowledging status or achievement</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Iron-Bane: Present martial tribute (weapon, armor piece, trophy)</description></item>
    ///   <item><description>Gorge-Maw: Share food before discussing business</description></item>
    ///   <item><description>Dvergr: Offer items of fine craftsmanship</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Failure to provide required offerings may prevent interaction entirely
    /// or result in severe protocol violations.
    /// </para>
    /// </remarks>
    Offering = 2,

    /// <summary>
    /// Mental state or psychic openness required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mental requirements demand specific psychological states:
    /// <list type="bullet">
    ///   <item><description>Opening the mind to telepathic contact</description></item>
    ///   <item><description>Suppressing aggressive or hostile thoughts</description></item>
    ///   <item><description>Maintaining emotional sincerity</description></item>
    ///   <item><description>Achieving meditative calm</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Rune-Lupin: Open mind to pack telepathy, share emotional state</description></item>
    ///   <item><description>Dvergr: Maintain logical clarity, suppress emotional reasoning</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Mental requirements may require skill checks (typically WILL-based)
    /// to meet, especially for characters unfamiliar with the practice.
    /// </para>
    /// </remarks>
    Mental = 3,

    /// <summary>
    /// A specific sequence of actions or events required in order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ritual requirements demand completion of multi-step ceremonies:
    /// <list type="bullet">
    ///   <item><description>Formal greeting sequences with specific timing</description></item>
    ///   <item><description>Ceremonial exchanges before business discussion</description></item>
    ///   <item><description>Purification or preparation rites</description></item>
    ///   <item><description>Oath-taking or binding agreements</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Gorge-Maw: Complete the full hospitality greeting (may take 30+ minutes)</description></item>
    ///   <item><description>Iron-Bane: Blood oath ceremony for serious agreements</description></item>
    ///   <item><description>Dvergr: Formal introduction sequence with titles and lineage</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ritual requirements are often the most complex and may combine
    /// elements of Behavioral, Verbal, and Offering requirements.
    /// </para>
    /// </remarks>
    Ritual = 4,

    /// <summary>
    /// Acknowledgment of status, hierarchy, or authority within the culture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Status requirements demand recognition of social position:
    /// <list type="bullet">
    ///   <item><description>Acknowledging rank or title before speaking</description></item>
    ///   <item><description>Deferring to elders or leaders</description></item>
    ///   <item><description>Following proper order of address</description></item>
    ///   <item><description>Recognizing achievements or lineage</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Culture-specific examples:
    /// <list type="bullet">
    ///   <item><description>Rune-Lupin: Acknowledge pack hierarchy, defer to alpha</description></item>
    ///   <item><description>Dvergr: Recognize craft mastery rankings</description></item>
    ///   <item><description>Iron-Bane: Acknowledge martial achievements and veteran status</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ignoring status requirements is often treated as a moderate or severe
    /// violation, as it directly insults the individual's standing.
    /// </para>
    /// </remarks>
    StatusAcknowledgment = 5
}
