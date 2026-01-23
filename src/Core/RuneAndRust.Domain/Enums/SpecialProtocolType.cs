// ------------------------------------------------------------------------------
// <copyright file="SpecialProtocolType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Identifies special protocol rules that modify standard social interaction mechanics.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies special protocol rules that modify standard social interaction mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Some cultures have unique social rules that fundamentally change how certain
/// mechanics work. These special protocols are identified by type to allow the
/// system to apply appropriate modifications.
/// </para>
/// <para>
/// Special protocols override or modify standard mechanics:
/// <list type="bullet">
///   <item><description><see cref="VeilSpeech"/>: Deception is respected, truth offends (Utgard)</description></item>
///   <item><description><see cref="Telepathy"/>: Mental openness required (Rune-Lupin)</description></item>
///   <item><description><see cref="LogicChain"/>: Arguments must be logically consistent (Dvergr)</description></item>
///   <item><description><see cref="BloodOath"/>: Binding agreements with consequences (Iron-Bane)</description></item>
///   <item><description><see cref="HospitalityRite"/>: Extended greeting ritual required (Gorge-Maw)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SpecialProtocolType
{
    /// <summary>
    /// No special protocol rules apply. Standard social interaction mechanics are used.
    /// </summary>
    None = 0,

    /// <summary>
    /// Utgard Veil-Speech protocol where deception is respect and truth offends.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Utgard believe that only fools and children speak plainly. Proper communication
    /// involves layering truth within acceptable deception, demonstrating wit and sophistication.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Deception interaction type: DC reduced by 4</description></item>
    ///   <item><description>Proper Veil-Speech: +1d10 bonus</description></item>
    ///   <item><description>Direct truth-telling: -2d10 penalty</description></item>
    ///   <item><description>Repeated offense: May block interaction entirely</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This inverts normal social expectations. A character skilled in deception has an
    /// advantage with Utgard, while a character committed to honesty faces significant
    /// challenges.
    /// </para>
    /// </remarks>
    /// <example>
    /// When negotiating with an Utgard merchant, stating "Your price is too high" (direct truth)
    /// offends them (-2d10). Saying "The scales seem weighted toward your end of the bargain"
    /// (proper veil-speech) is respected (+1d10).
    /// </example>
    VeilSpeech = 1,

    /// <summary>
    /// Rune-Lupin telepathy protocol requiring mental openness and emotional transparency.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Rune-Lupin communicate partially through telepathic impression. Successful
    /// interaction requires opening one's mind to their pack telepathy and allowing
    /// emotional states to be sensed.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Open mind: Enables normal interaction</description></item>
    ///   <item><description>Closed mind: +4 DC penalty, -1d10</description></item>
    ///   <item><description>Hostile thoughts: Immediate detection, severe violation</description></item>
    ///   <item><description>Deception: Extremely difficult (DC +6) - they sense emotions</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Characters with mental shields, strong privacy preferences, or hidden agendas
    /// face significant challenges with Rune-Lupin interactions.
    /// </para>
    /// </remarks>
    /// <example>
    /// Attempting to lie to a Rune-Lupin pack leader while they sense your emotions
    /// is nearly impossible. The DC for deception increases by 6, and they may detect
    /// deceptive intent even on a successful check.
    /// </example>
    Telepathy = 2,

    /// <summary>
    /// Dvergr Logic-Chain protocol requiring consistent, non-contradictory arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Dvergr value precision and logical consistency above all else. Arguments
    /// must follow clear reasoning chains, and contradictions are considered either
    /// insulting or evidence of poor intellect.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Logical arguments: Normal DC</description></item>
    ///   <item><description>Emotional appeals: +4 DC penalty</description></item>
    ///   <item><description>Contradictory statements: Immediate moderate violation</description></item>
    ///   <item><description>Precise technical language: +1d10 bonus</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Characters with high WITS or scholarly backgrounds often excel at Logic-Chain
    /// interactions, while those who rely on charm or emotion face difficulties.
    /// </para>
    /// </remarks>
    /// <example>
    /// When persuading a Dvergr craftmaster, saying "This would honor your family"
    /// (emotional appeal) is less effective than "This commission increases guild
    /// prestige by 15% while requiring only 8% additional material cost."
    /// </example>
    LogicChain = 3,

    /// <summary>
    /// Iron-Bane Blood Oath protocol for binding serious agreements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Iron-Bane seal significant agreements with blood oaths - rituals that
    /// bind both parties to their word with supernatural consequences for betrayal.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Oath sealed: Agreement is magically binding</description></item>
    ///   <item><description>Breaking oath: Severe supernatural consequences</description></item>
    ///   <item><description>Refusing oath: May be seen as lack of commitment (-2d10)</description></item>
    ///   <item><description>Completing oath terms: Significant reputation gain</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Blood oaths are not required for all interactions, but for serious agreements
    /// (alliances, major trades, promises of service), Iron-Bane may insist on them.
    /// </para>
    /// </remarks>
    /// <example>
    /// The Iron-Bane chieftain offers alliance against the common enemy. "Seal it in
    /// blood, outlander. Words are wind - blood is binding." Accepting the oath creates
    /// a genuine supernatural bond; refusing may end negotiations.
    /// </example>
    BloodOath = 4,

    /// <summary>
    /// Gorge-Maw Hospitality Rite protocol requiring extended greeting ceremonies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Gorge-Maw consider hospitality sacred. Business cannot be discussed until
    /// proper hospitality rites are completed, including shared food, extended greetings,
    /// and patient listening.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Completing rite: Enables normal interaction, +1d10 bonus</description></item>
    ///   <item><description>Skipping rite: +6 DC, -2d10, possible hostility</description></item>
    ///   <item><description>Patience displayed: Additional +1d10 bonus</description></item>
    ///   <item><description>Impatience shown: Immediate moderate violation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The hospitality rite takes significant time (30+ minutes in-world) but creates
    /// a foundation of trust that benefits all subsequent interactions.
    /// </para>
    /// </remarks>
    /// <example>
    /// The Gorge-Maw elder begins the hospitality rite with a rumbling chant about
    /// ancestral stone and shared earth. Attempting to interrupt with "Yes, but about
    /// the trade agreement..." results in deeply offended silence.
    /// </example>
    HospitalityRite = 5,

    /// <summary>
    /// Martial tribute protocol requiring demonstration of martial status before negotiation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Iron-Bane respect martial achievement. Before serious discussion can occur,
    /// visitors must demonstrate their martial status through tribute (offering a weapon
    /// or trophy) or by recounting their martial achievements.
    /// </para>
    /// <para>
    /// Mechanical modifications:
    /// <list type="bullet">
    ///   <item><description>Martial tribute given: Normal DC</description></item>
    ///   <item><description>No tribute: +4 DC, character viewed as civilian/non-combatant</description></item>
    ///   <item><description>Impressive tribute: +2d10 bonus</description></item>
    ///   <item><description>Insulting tribute (poor quality): Moderate violation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Characters without martial background can substitute by bringing a quality weapon
    /// as gift or by having a martial ally introduce them.
    /// </para>
    /// </remarks>
    /// <example>
    /// At the Iron-Bane warband's camp, presenting a fine blade as tribute opens
    /// negotiation. Arriving empty-handed leads to dismissive treatment: "What word
    /// has one who has never shed blood?"
    /// </example>
    MartialTribute = 6
}
