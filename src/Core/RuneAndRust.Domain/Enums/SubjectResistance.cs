// ------------------------------------------------------------------------------
// <copyright file="SubjectResistance.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes how many successful interrogation checks are required
// to break a subject's will and extract information.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes how many successful interrogation checks are required
/// to break a subject's will and extract information.
/// </summary>
/// <remarks>
/// <para>
/// Resistance level is primarily determined by the subject's WILL attribute,
/// modified by training, loyalty, and circumstances. Higher resistance means
/// more successful checks are needed to break the subject.
/// </para>
/// <para>
/// Resistance is not a binary state - each successful check reduces the
/// subject's resistance counter. The checks required varies within the
/// range for each level based on modifiers.
/// </para>
/// <para>
/// WILL to Resistance mapping:
/// <list type="bullet">
///   <item><description>WILL 1-2: Minimal (1 check)</description></item>
///   <item><description>WILL 3-4: Low (2-3 checks)</description></item>
///   <item><description>WILL 5-6: Moderate (4-5 checks)</description></item>
///   <item><description>WILL 7-8: High (6-8 checks)</description></item>
///   <item><description>WILL 9+: Extreme (10+ checks)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SubjectResistance
{
    /// <summary>
    /// Frightened civilian, coward, or already broken individual.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires only 1 successful check to break. These subjects have
    /// virtually no resistance - they may even volunteer information
    /// without much prompting.
    /// </para>
    /// <para>
    /// Typical subjects:
    /// <list type="bullet">
    ///   <item><description>Terrified civilians caught up in events</description></item>
    ///   <item><description>Cowards who value self-preservation above all</description></item>
    ///   <item><description>Individuals already traumatized from prior interrogation</description></item>
    ///   <item><description>Children or elderly with low will to resist</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Minimal = 0,

    /// <summary>
    /// Hired muscle, petty criminal, or untrained individual.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires 2-3 successful checks to break. These subjects have some
    /// resistance but will eventually crack under pressure. They typically
    /// lack training in resisting interrogation.
    /// </para>
    /// <para>
    /// Typical subjects:
    /// <list type="bullet">
    ///   <item><description>Hired thugs and mercenaries</description></item>
    ///   <item><description>Petty criminals and street-level operatives</description></item>
    ///   <item><description>Opportunistic informants looking for the best deal</description></item>
    ///   <item><description>Common soldiers without specialized training</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Low = 1,

    /// <summary>
    /// Loyal soldier, guild member, or moderately committed individual.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires 4-5 successful checks to break. These subjects have
    /// genuine loyalty or conviction that makes them more difficult
    /// to break. They may require a combination of methods.
    /// </para>
    /// <para>
    /// Typical subjects:
    /// <list type="bullet">
    ///   <item><description>Soldiers loyal to their commander</description></item>
    ///   <item><description>Guild members protecting trade secrets</description></item>
    ///   <item><description>Devoted servants or retainers</description></item>
    ///   <item><description>Religious believers with moderate conviction</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Moderate = 2,

    /// <summary>
    /// Trained operative, sworn knight, or highly committed individual.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires 6-8 successful checks to break. These subjects have
    /// either formal training in resistance techniques or deep personal
    /// commitment that strengthens their will. Breaking them is difficult
    /// and time-consuming.
    /// </para>
    /// <para>
    /// Typical subjects:
    /// <list type="bullet">
    ///   <item><description>Trained spies and intelligence operatives</description></item>
    ///   <item><description>Sworn knights bound by sacred oaths</description></item>
    ///   <item><description>Professional assassins protecting employers</description></item>
    ///   <item><description>Veteran soldiers with resistance training</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    High = 3,

    /// <summary>
    /// Fanatical believer, elite spy, or extraordinarily committed individual.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires 10+ successful checks to break. These subjects represent
    /// the most difficult interrogation targets. Their will is nearly
    /// unbreakable through conventional means, and even Torture may be
    /// insufficient to extract reliable information.
    /// </para>
    /// <para>
    /// Typical subjects:
    /// <list type="bullet">
    ///   <item><description>Fanatical cult members believing death is preferable</description></item>
    ///   <item><description>Elite assassins trained to resist all methods</description></item>
    ///   <item><description>Devoted cultists protecting forbidden secrets</description></item>
    ///   <item><description>True believers willing to die for their cause</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Breaking such subjects may not be possible through normal interrogation.
    /// Magical means or extreme measures may be required.
    /// </para>
    /// </remarks>
    Extreme = 4
}
