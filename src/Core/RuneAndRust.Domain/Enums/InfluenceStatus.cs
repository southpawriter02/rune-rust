// ------------------------------------------------------------------------------
// <copyright file="InfluenceStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the possible states of an extended influence attempt.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current state of an extended influence attempt.
/// </summary>
/// <remarks>
/// <para>
/// Extended influence tracking persists across multiple game sessions, allowing
/// players to work on changing NPC beliefs over time. The status reflects the
/// current progress and whether the influence attempt can continue.
/// </para>
/// <para>
/// State transitions:
/// <list type="bullet">
///   <item><description>Active → Successful: Influence pool reaches conviction threshold</description></item>
///   <item><description>Active → Failed: Maximum resistance reached or quest failure</description></item>
///   <item><description>Active → Stalled: Resistance too high, NPC needs time or evidence</description></item>
///   <item><description>Stalled → Active: External event resolves stall condition</description></item>
/// </list>
/// </para>
/// <para>
/// Terminal states (Successful, Failed) cannot transition to other states.
/// The Stalled state is recoverable with appropriate actions or passage of time.
/// </para>
/// </remarks>
public enum InfluenceStatus
{
    /// <summary>
    /// The influence attempt is in progress and the player can continue making attempts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default state for ongoing influence efforts. The influence pool continues
    /// accumulating with successful rhetoric checks, and resistance may increase on
    /// failures depending on the conviction level.
    /// </para>
    /// <para>
    /// While Active, the player can:
    /// <list type="bullet">
    ///   <item><description>Make additional influence attempts to add to the pool</description></item>
    ///   <item><description>Check current progress toward the threshold</description></item>
    ///   <item><description>View accumulated resistance and effective DC</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Active = 0,

    /// <summary>
    /// The influence succeeded and the NPC's belief has been changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal success state indicating the conviction threshold was reached.
    /// The NPC now holds a different belief on this topic, which may unlock
    /// new dialogue options, quest paths, or faction relationship changes.
    /// </para>
    /// <para>
    /// Once successful, this influence tracking instance is complete. The belief
    /// change is recorded with a timestamp and may trigger:
    /// <list type="bullet">
    ///   <item><description>New dialogue options reflecting the changed belief</description></item>
    ///   <item><description>Faction reputation changes if the belief was faction-related</description></item>
    ///   <item><description>Quest state updates or new quest availability</description></item>
    ///   <item><description>Changes in NPC behavior patterns</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Successful = 1,

    /// <summary>
    /// The influence failed and the NPC is completely resistant to further attempts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal failure state reached when resistance becomes too high or specific
    /// failure conditions are triggered. The NPC may never change their mind on
    /// this topic through normal influence mechanics.
    /// </para>
    /// <para>
    /// This state can occur when:
    /// <list type="bullet">
    ///   <item><description>Maximum resistance modifier (6) is reached with insufficient pool progress</description></item>
    ///   <item><description>A related quest fails, making the belief change impossible</description></item>
    ///   <item><description>The NPC dies or becomes permanently unavailable</description></item>
    ///   <item><description>A critical fumble during influence creates an unrecoverable situation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Failed influence may still be recoverable through extraordinary
    /// narrative means (major story events), but not through the standard
    /// influence mechanics.
    /// </para>
    /// </remarks>
    Failed = 2,

    /// <summary>
    /// The influence is stalled, awaiting an external event or condition to continue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Paused state where no progress can be made through conversation alone.
    /// The NPC has become too guarded or requires something beyond words to
    /// reconsider their position.
    /// </para>
    /// <para>
    /// Stall conditions and their resolutions:
    /// <list type="bullet">
    ///   <item><description>Resistance ≥ 4: Wait 24+ hours game time ("I need time to think.")</description></item>
    ///   <item><description>Fumble consequence: Complete a related quest ("Prove yourself first.")</description></item>
    ///   <item><description>Contradicted by evidence: Find counter-evidence ("Show me the records.")</description></item>
    ///   <item><description>NPC emotional state: Wait for mood change ("I can't discuss this now.")</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When resumed, resistance is typically reduced by 2 points, representing
    /// the NPC's renewed openness after the stall condition is resolved.
    /// </para>
    /// </remarks>
    Stalled = 3
}
