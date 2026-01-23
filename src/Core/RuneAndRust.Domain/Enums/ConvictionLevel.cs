// ------------------------------------------------------------------------------
// <copyright file="ConvictionLevel.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines how deeply an NPC holds a particular belief, determining the
// difficulty and effort required to change their conviction.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents how deeply an NPC holds a particular belief.
/// </summary>
/// <remarks>
/// <para>
/// Conviction levels determine the mechanical parameters for extended influence:
/// <list type="bullet">
///   <item><description>Base DC for rhetoric checks when attempting influence</description></item>
///   <item><description>Pool threshold required to change the belief</description></item>
///   <item><description>Resistance accumulation rate on failed attempts</description></item>
///   <item><description>Narrative weight of successfully changing the belief</description></item>
/// </list>
/// </para>
/// <para>
/// Higher conviction levels represent beliefs that are more fundamental to the NPC's
/// identity and worldview. These require sustained effort over multiple interactions
/// to shift. Fanatical beliefs may additionally require life-changing events before
/// the NPC will even consider alternative viewpoints.
/// </para>
/// <para>
/// The relationship between conviction level and influence mechanics:
/// <list type="bullet">
///   <item><description>WeakOpinion: DC 10, 5 pool needed, no resistance accumulation</description></item>
///   <item><description>ModerateBelief: DC 12, 10 pool needed, no resistance accumulation</description></item>
///   <item><description>StrongConviction: DC 14, 15 pool needed, +1 resistance per 2 failures</description></item>
///   <item><description>CoreBelief: DC 16, 20 pool needed, +1 resistance per failure</description></item>
///   <item><description>Fanatical: DC 18, 25 pool needed, +2 resistance per failure</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ConvictionLevel
{
    /// <summary>
    /// Surface-level opinion that is easily changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 10, Pool Threshold: 5 points.
    /// </para>
    /// <para>
    /// Weak opinions are casual positions the NPC hasn't invested much thought into.
    /// They can be swayed with minimal effort and basic arguments. Failed influence
    /// attempts do not increase the NPC's resistance.
    /// </para>
    /// <para>
    /// Example beliefs:
    /// <list type="bullet">
    ///   <item><description>"I've heard the Combine is powerful."</description></item>
    ///   <item><description>"I don't know much about the Dvergr."</description></item>
    ///   <item><description>"The old tunnels are probably dangerous."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    WeakOpinion = 0,

    /// <summary>
    /// Casually held belief that can be swayed with good arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 12, Pool Threshold: 10 points.
    /// </para>
    /// <para>
    /// Moderate beliefs represent positions the NPC holds with some conviction but
    /// without deep personal investment. They require more substantial arguments
    /// to change but are still accessible to persistent persuasion. Failed influence
    /// attempts do not increase the NPC's resistance.
    /// </para>
    /// <para>
    /// Example beliefs:
    /// <list type="bullet">
    ///   <item><description>"The Combine keeps things orderly."</description></item>
    ///   <item><description>"Magic seems dangerous to me."</description></item>
    ///   <item><description>"Outsiders can't be trusted."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ModerateBelief = 1,

    /// <summary>
    /// Firmly held conviction that requires compelling evidence to shift.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 14, Pool Threshold: 15 points.
    /// Resistance increases by +1 for every 2 failed attempts.
    /// </para>
    /// <para>
    /// Strong convictions are beliefs the NPC has adopted through experience or
    /// reasoning. They won't change their mind easily and become more resistant
    /// to influence after repeated failed attempts. Successful influence typically
    /// requires evidence or compelling personal testimony.
    /// </para>
    /// <para>
    /// Example beliefs:
    /// <list type="bullet">
    ///   <item><description>"The Combine's rules saved my family during the Drought."</description></item>
    ///   <item><description>"The old ways are better than this new chaos."</description></item>
    ///   <item><description>"The Dvergr are the only ones who understand the machines."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    StrongConviction = 2,

    /// <summary>
    /// Deeply held core belief that is fundamental to the NPC's worldview.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 16, Pool Threshold: 20 points.
    /// Resistance increases by +1 for every failed attempt.
    /// </para>
    /// <para>
    /// Core beliefs are fundamental to how the NPC understands the world. These
    /// beliefs have been reinforced over years of experience and form the foundation
    /// for many of their decisions. Changing a core belief requires sustained effort
    /// and may trigger significant behavioral changes in the NPC.
    /// </para>
    /// <para>
    /// Example beliefs:
    /// <list type="bullet">
    ///   <item><description>"The Combine IS civilization. Without them, we'd all be dead."</description></item>
    ///   <item><description>"Magic corrupts absolutely. No exceptions."</description></item>
    ///   <item><description>"Blood is thicker than water. Family comes first, always."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    CoreBelief = 3,

    /// <summary>
    /// Identity-defining belief that is part of who the NPC fundamentally is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 18, Pool Threshold: 25 points.
    /// Resistance increases by +2 for every failed attempt.
    /// May require life-changing events before influence attempts can succeed.
    /// </para>
    /// <para>
    /// Fanatical beliefs are inseparable from the NPC's sense of self. These are
    /// not merely opinions but core aspects of their identity. Challenging these
    /// beliefs is perceived as an attack on who they are. Successfully changing
    /// a fanatical belief often requires the NPC to witness something that directly
    /// contradicts their worldview or experience a transformative life event.
    /// </para>
    /// <para>
    /// Example beliefs:
    /// <list type="bullet">
    ///   <item><description>"I am Combine. The Overseer's will is my will."</description></item>
    ///   <item><description>"The runes speak through me. I am their vessel."</description></item>
    ///   <item><description>"My clan's honor is my honor. I would die before betraying them."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Fanatical = 4
}
