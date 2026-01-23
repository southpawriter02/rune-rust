// ------------------------------------------------------------------------------
// <copyright file="RhetoricSpecializationAbility.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Identifies rhetoric-focused abilities granted by archetype specializations.
// Part of v0.15.3i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies rhetoric-focused abilities granted by archetype specializations.
/// </summary>
/// <remarks>
/// <para>
/// Each archetype has two rhetoric specialization abilities:
/// <list type="bullet">
///   <item><description>Thul: Voice of Reason, Scholarly Authority</description></item>
///   <item><description>Skald: Inspiring Words, Saga of Heroes</description></item>
///   <item><description>Kupmaðr: Silver Tongue, Sniff Out Lies</description></item>
///   <item><description>Myrk-gengr: Maintain Cover, Forge Documents</description></item>
/// </list>
/// </para>
/// <para>
/// These abilities represent the Specialization Bonus Hook Pattern for Rhetoric skills,
/// providing conditional ability activation, outcome modification, dice pool augmentation,
/// and party-wide effects based on character archetype.
/// </para>
/// <para>
/// The ability types include:
/// <list type="bullet">
///   <item><description>Outcome Modification: Voice of Reason (prevent option lock)</description></item>
///   <item><description>Dice Bonus: Scholarly Authority, Sniff Out Lies, Maintain Cover</description></item>
///   <item><description>Auto-Success: Silver Tongue (DC ≤ 12 negotiation)</description></item>
///   <item><description>Party Buff: Inspiring Words (ally dice bonus)</description></item>
///   <item><description>Stress Relief: Saga of Heroes (party stress reduction)</description></item>
///   <item><description>Asset Creation: Forge Documents (create forged credentials)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum RhetoricSpecializationAbility
{
    /// <summary>
    /// Thul ability: Failed persuasion doesn't lock dialogue options.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Thul's measured, logical approach to discourse means that even when they
    /// fail to convince someone, they don't alienate them. Where other characters
    /// might push too hard and permanently close dialogue options, the Thul's
    /// respectful reasoning leaves the door open for future attempts.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Thul</description></item>
    ///   <item><description>Social interaction type is Persuasion</description></item>
    ///   <item><description>Check result is Failure (not Fumble/Critical Failure)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Limitations:
    /// <list type="bullet">
    ///   <item><description>Does NOT prevent fumble consequences (Trust Shattered still locks)</description></item>
    ///   <item><description>Only applies to Persuasion, not Deception or Intimidation</description></item>
    ///   <item><description>Requires waiting for next conversation (not immediate retry)</description></item>
    ///   <item><description>Does NOT prevent disposition loss from failure</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    VoiceOfReason = 0,

    /// <summary>
    /// Thul ability: +2d10 when discussing lore/history topics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the topic of conversation involves academic knowledge, historical events,
    /// ancient lore, or scholarly matters, the Thul's reputation and expertise grants
    /// them significant social leverage.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Thul</description></item>
    ///   <item><description>Conversation topic is tagged as Lore, History, Academic, or Scholarly</description></item>
    ///   <item><description>Any social interaction type (Persuasion, Deception, Intimidation, etc.)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example topics that trigger this ability:
    /// <list type="bullet">
    ///   <item><description>Historical events ("The Fall of the Old Kingdom")</description></item>
    ///   <item><description>Ancient lore ("The Rune-Singers of Mímir")</description></item>
    ///   <item><description>Academic knowledge ("The principles of æthercraft")</description></item>
    ///   <item><description>Written records ("As documented in the Archives")</description></item>
    ///   <item><description>Cultural traditions ("According to Dvergr custom")</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ScholarlyAuthority = 1,

    /// <summary>
    /// Skald ability: Grant allies +1d10 on social checks through oratory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Skald can inspire their companions before a critical social encounter,
    /// giving them the confidence and presence to perform better in the upcoming
    /// interaction.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Skald</description></item>
    ///   <item><description>Player declares intention to inspire before ally's check</description></item>
    ///   <item><description>Skald succeeds DC 12 Rhetoric check</description></item>
    ///   <item><description>Ability not on cooldown (once per scene)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Effect:
    /// <list type="bullet">
    ///   <item><description>On success: Target ally gains +1d10 on their next social check</description></item>
    ///   <item><description>On exceptional success (Net ≥ 3): All party members gain +1d10</description></item>
    ///   <item><description>Duration: One check (within next 10 minutes)</description></item>
    ///   <item><description>Stacks: No (cannot stack multiple inspirations)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    InspiringWords = 2,

    /// <summary>
    /// Skald ability: Reduce party Stress through storytelling during rest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During downtime or rest, the Skald can tell tales of heroic deeds, legendary
    /// figures, and inspiring triumphs. This storytelling helps the party process
    /// their experiences and reduces accumulated psychic stress.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Skald</description></item>
    ///   <item><description>Party is in rest/downtime phase</description></item>
    ///   <item><description>At least one party member has Stress > 0</description></item>
    ///   <item><description>Ability not on cooldown (once per rest)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Stress reduction based on success tier:
    /// <list type="bullet">
    ///   <item><description>Marginal Success (DC exact): -1 Stress to all listeners</description></item>
    ///   <item><description>Full Success (DC +1-2): -2 Stress to all listeners</description></item>
    ///   <item><description>Exceptional (DC +3-4): -3 Stress to all listeners</description></item>
    ///   <item><description>Critical (DC +5+): -4 Stress to all listeners</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    SagaOfHeroes = 3,

    /// <summary>
    /// Kupmaðr ability: Auto-succeed negotiation checks with DC ≤ 12.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Kupmaðr has conducted so many deals and haggled in so many markets that
    /// routine negotiations are effortless. Low-difficulty negotiations are automatically
    /// successful, representing the merchant's instinctive mastery of the bargaining process.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Kupmaðr</description></item>
    ///   <item><description>Social interaction type is Negotiation</description></item>
    ///   <item><description>Final DC (after all modifiers) ≤ 12</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples of DC ≤ 12 negotiations:
    /// <list type="bullet">
    ///   <item><description>Fair trade exchanges (DC 10)</description></item>
    ///   <item><description>Slight advantage in common goods (DC 12)</description></item>
    ///   <item><description>Standard bartering with neutral merchants (DC 10-12)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This mirrors the Master Ability pattern from v0.15.1c where auto-succeed
    /// abilities bypass checks below a threshold.
    /// </para>
    /// </remarks>
    SilverTongue = 4,

    /// <summary>
    /// Kupmaðr ability: +2d10 when detecting NPC deception.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Years of dealing with unscrupulous merchants, con artists, and competitors
    /// have given the Kupmaðr a keen sense for when someone is being dishonest.
    /// They receive bonus dice when attempting to detect lies or deception.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Kupmaðr</description></item>
    ///   <item><description>Player is target of deception OR actively detecting lies</description></item>
    ///   <item><description>Check type is Insight/Deception Detection</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Detection scenarios:
    /// <list type="bullet">
    ///   <item><description>NPC attempts to deceive player (passive detection)</description></item>
    ///   <item><description>Player actively questions NPC's honesty</description></item>
    ///   <item><description>GM-initiated detection when NPC lies</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Cooldown: None (always active when detecting deception).
    /// </para>
    /// </remarks>
    SniffOutLies = 5,

    /// <summary>
    /// Myrk-gengr ability: +2d10 and reduced stress when cover is challenged.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an infiltrator's cover identity is challenged—whether through pointed
    /// questions, suspicious NPCs, or stressful situations—the Myrk-gengr can draw
    /// on their training to remain calm and maintain the deception.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Myrk-gengr</description></item>
    ///   <item><description>Player has an active cover identity</description></item>
    ///   <item><description>Cover identity is being challenged (questions, suspicion, stress)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Effect:
    /// <list type="bullet">
    ///   <item><description>+2d10 to deception/composure checks when maintaining cover</description></item>
    ///   <item><description>Stress from cover challenges reduced by 1</description></item>
    ///   <item><description>Fumble consequences are downgraded one tier (Lie Exposed → normal failure)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Limitation: Only applies when maintaining an established cover identity,
    /// not for initial deception attempts.
    /// </para>
    /// </remarks>
    MaintainCover = 6,

    /// <summary>
    /// Myrk-gengr ability: Create forged documents as evidence/credentials.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Myrk-gengr can create forged documents, fake credentials, and other
    /// false proof to support their cover stories and deceptions. These forgeries
    /// provide bonuses to deception checks or can be used as evidence in
    /// persuasion/negotiation.
    /// </para>
    /// <para>
    /// Activation Conditions:
    /// <list type="bullet">
    ///   <item><description>Player archetype is Myrk-gengr</description></item>
    ///   <item><description>Player has materials and time for forgery</description></item>
    ///   <item><description>Player is not in combat or stressful situation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Document types and DCs:
    /// <list type="bullet">
    ///   <item><description>Simple note: DC 10, 10 minutes</description></item>
    ///   <item><description>Travel papers: DC 12, 30 minutes</description></item>
    ///   <item><description>Guild credentials: DC 14, 1 hour</description></item>
    ///   <item><description>Official orders: DC 16, 2 hours</description></item>
    ///   <item><description>Faction authorization: DC 18, 4 hours</description></item>
    ///   <item><description>Rare/complex document: DC 20+, 8+ hours</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Forgery quality determines detection DC reduction:
    /// <list type="bullet">
    ///   <item><description>Passable: -2 DC to detect</description></item>
    ///   <item><description>Good: -4 DC to detect</description></item>
    ///   <item><description>Excellent: -6 DC to detect</description></item>
    ///   <item><description>Masterwork: Requires expertise to detect</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ForgeDocuments = 7
}
