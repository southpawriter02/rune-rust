// ------------------------------------------------------------------------------
// <copyright file="IntimidationTarget.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes NPC resistance to intimidation based on resolve and position.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes NPC resistance to intimidation based on their resolve,
/// experience, and position. Higher tiers represent individuals who
/// are harder to frighten due to battle experience, exceptional willpower,
/// or positions that demand fearlessness.
/// </summary>
/// <remarks>
/// <para>
/// The base DC for intimidation is determined solely by the target's
/// resistance tier. Other factors (relative strength, reputation, artifacts)
/// affect dice pools rather than DC.
/// </para>
/// <para>
/// Intimidation Target Tiers:
/// </para>
/// <list type="bullet">
///   <item><description>Coward (DC 8): Timid individuals who frighten easily</description></item>
///   <item><description>Common (DC 12): Average resolve, neither brave nor cowardly</description></item>
///   <item><description>Veteran (DC 16): Battle-hardened, has faced death before</description></item>
///   <item><description>Elite (DC 20): Exceptional willpower, rarely intimidated</description></item>
///   <item><description>FactionLeader (DC 24): Position demands fearlessness</description></item>
/// </list>
/// </remarks>
public enum IntimidationTarget
{
    /// <summary>
    /// Timid individuals who frighten easily. Nervous merchants,
    /// sheltered nobles, inexperienced youth, those already visibly afraid.
    /// </summary>
    /// <remarks>
    /// Base DC: 8. Typical response: Immediate compliance, visible trembling,
    /// may flee if given the chance. Low risk of [Challenge Accepted] fumble.
    /// </remarks>
    Coward = 0,

    /// <summary>
    /// Average individuals with normal resolve. Standard guards,
    /// craftsmen, innkeepers, travelers, commoners.
    /// </summary>
    /// <remarks>
    /// Base DC: 12. Typical response: Reluctant compliance, seeks help if
    /// possible, remembers the threat. Standard fumble risk.
    /// </remarks>
    Common = 1,

    /// <summary>
    /// Battle-hardened individuals who have faced death and survived.
    /// Experienced soldiers, seasoned mercenaries, veteran guards,
    /// criminals with violent history.
    /// </summary>
    /// <remarks>
    /// Base DC: 16. Typical response: Assesses threat carefully, complies
    /// but with dignity intact. More likely to trigger [Challenge Accepted]
    /// on fumble.
    /// </remarks>
    Veteran = 2,

    /// <summary>
    /// Exceptional individuals with ironclad willpower. Champions,
    /// elite commanders, notorious crime lords, fanatical zealots.
    /// </summary>
    /// <remarks>
    /// Base DC: 20. Typical response: May counter-threaten before yielding,
    /// never forgets the insult. Very likely to trigger [Challenge Accepted]
    /// on fumble. Usually requires exceptional circumstances to intimidate.
    /// </remarks>
    Elite = 3,

    /// <summary>
    /// Leaders whose position demands fearlessness. Jarls, guild masters,
    /// clan chieftains, high priests, war leaders. Showing fear would
    /// undermine their authority.
    /// </summary>
    /// <remarks>
    /// Base DC: 24. Typical response: Outraged, likely retribution regardless
    /// of outcome. Almost certain to trigger [Challenge Accepted] on fumble.
    /// Success extremely rare; failure has severe consequences.
    /// </remarks>
    FactionLeader = 4
}
