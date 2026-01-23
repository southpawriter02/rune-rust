// ------------------------------------------------------------------------------
// <copyright file="CantFluency.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the level of proficiency a character has with a specific cultural cant.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the level of proficiency a character has with a specific cultural cant (dialect/language).
/// </summary>
/// <remarks>
/// <para>
/// Cants are specialized dialects or coded languages used by different cultures in Aethelgard.
/// A character's fluency level directly affects their ability to communicate effectively
/// and navigate social interactions with members of that culture.
/// </para>
/// <para>
/// Fluency levels provide dice pool modifiers:
/// <list type="bullet">
///   <item><description><see cref="None"/>: -1d10 penalty (unfamiliar with the cant)</description></item>
///   <item><description><see cref="Basic"/>: No modifier (functional understanding)</description></item>
///   <item><description><see cref="Fluent"/>: +1d10 bonus (native-level proficiency)</description></item>
/// </list>
/// </para>
/// <para>
/// Integration with Cultural Protocols:
/// <list type="bullet">
///   <item><description>Dvergr Trade-Tongue: Precision language for craft and commerce</description></item>
///   <item><description>Utgard Veil-Speech: Layered communication where truth is hidden</description></item>
///   <item><description>Gorge-Maw Rumble: Resonant tonal language requiring patience</description></item>
///   <item><description>Rune-Lupin Pack-Speech: Semi-telepathic emotional communication</description></item>
///   <item><description>Iron-Bane Battle-Tongue: Martial terminology and honor codes</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var fluency = CantFluency.Fluent;
/// var modifier = fluency.GetDiceModifier(); // Returns +1
/// </code>
/// </example>
public enum CantFluency
{
    /// <summary>
    /// No knowledge of the cant. The character cannot understand or speak the dialect,
    /// resulting in communication difficulties and social penalties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies a -1d10 penalty to all Rhetoric checks with members of this culture.
    /// The character may misinterpret idioms, miss cultural references, or accidentally
    /// cause offense through linguistic ignorance.
    /// </para>
    /// <para>
    /// In gameplay terms, characters with no cant knowledge:
    /// <list type="bullet">
    ///   <item><description>Cannot understand idioms or cultural references</description></item>
    ///   <item><description>May accidentally cause minor protocol violations</description></item>
    ///   <item><description>Are clearly identified as outsiders</description></item>
    ///   <item><description>Cannot access certain cultural-specific dialogue options</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// A human merchant attempting to negotiate with Dvergr without knowing Trade-Tongue
    /// suffers -1d10 on persuasion checks and may misunderstand technical terms.
    /// </example>
    None = 0,

    /// <summary>
    /// Basic understanding of the cant. The character can communicate functional ideas
    /// but lacks nuance and may miss subtle meanings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// No modifier applied. The character can conduct basic social interactions but
    /// won't benefit from cultural linguistic advantages. Suitable for travelers
    /// who have spent limited time with the culture.
    /// </para>
    /// <para>
    /// Characters with basic fluency:
    /// <list type="bullet">
    ///   <item><description>Can conduct routine transactions and conversations</description></item>
    ///   <item><description>Understand literal meanings but miss subtext</description></item>
    ///   <item><description>Are recognized as learners, which may earn respect or patience</description></item>
    ///   <item><description>Can access standard dialogue options</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// A character who has traveled through Dvergr lands can speak basic Trade-Tongue,
    /// sufficient for commerce but not for understanding technical craft discussions.
    /// </example>
    Basic = 1,

    /// <summary>
    /// Fluent in the cant. The character speaks with native-level proficiency,
    /// understanding idioms, cultural references, and subtle implications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies a +1d10 bonus to all Rhetoric checks with members of this culture.
    /// The character can navigate complex social situations, use appropriate
    /// honorifics, and demonstrate cultural respect through language.
    /// </para>
    /// <para>
    /// Characters with fluent knowledge:
    /// <list type="bullet">
    ///   <item><description>Can understand and use idioms, metaphors, and cultural references</description></item>
    ///   <item><description>Are treated as cultural insiders (or impressive outsiders)</description></item>
    ///   <item><description>Can detect deception or subtext in conversations</description></item>
    ///   <item><description>Can access exclusive dialogue options and cultural knowledge</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Fluency can be acquired through:
    /// <list type="bullet">
    ///   <item><description>Extended time living among the culture</description></item>
    ///   <item><description>Formal study with cultural teachers</description></item>
    ///   <item><description>The Polyglot master ability (v0.15.3i)</description></item>
    ///   <item><description>Cultural background during character creation</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// A Kupmaðr (merchant) specialization character fluent in Dvergr Trade-Tongue
    /// gains +1d10 on all negotiations with Dvergr and can discuss technical
    /// craftsmanship with proper terminology.
    /// </example>
    Fluent = 2
}
