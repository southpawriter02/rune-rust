// ------------------------------------------------------------------------------
// <copyright file="CantModifier.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the dice pool modifier applied based on a character's fluency in a
// cultural cant. Provides factory methods and integration with the SocialModifier system.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents the dice pool modifier applied based on a character's fluency in a cultural cant.
/// </summary>
/// <remarks>
/// <para>
/// This value object encapsulates the relationship between a <see cref="CantFluency"/> level
/// and its corresponding dice pool modifier. It provides a type-safe way to pass cant-based
/// modifiers through the social interaction system.
/// </para>
/// <para>
/// Cants are specialized dialects or coded languages used by different cultures in Aethelgard.
/// Each major culture has its own cant:
/// <list type="bullet">
///   <item><description>Dvergr Trade-Tongue: Precision language for craft and commerce</description></item>
///   <item><description>Utgard Veil-Speech: Layered communication where truth is hidden</description></item>
///   <item><description>Gorge-Maw Rumble: Resonant tonal language requiring patience</description></item>
///   <item><description>Rune-Lupin Pack-Speech: Semi-telepathic emotional communication</description></item>
///   <item><description>Iron-Bane Battle-Tongue: Martial terminology and honor codes</description></item>
/// </list>
/// </para>
/// <para>
/// The modifier values are:
/// <list type="bullet">
///   <item><description>Fluent: +1 die to the pool</description></item>
///   <item><description>Basic: No modification</description></item>
///   <item><description>None: -1 die from the pool</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Fluency">The character's fluency level in the target culture's cant.</param>
/// <param name="DiceModifier">The number of dice to add (positive) or remove (negative).</param>
/// <param name="CultureId">The culture this modifier applies to.</param>
/// <param name="CantName">The name of the cant (e.g., "Dvergr Trade-Tongue").</param>
/// <example>
/// <code>
/// // Create a modifier for a fluent speaker of Dvergr Trade-Tongue
/// var modifier = CantModifier.FromFluency(CantFluency.Fluent, "dvergr", "Dvergr Trade-Tongue");
/// Console.WriteLine(modifier.ToDisplayString()); // "Dvergr Trade-Tongue: +1d10 (Fluent)"
/// Console.WriteLine(modifier.IsBonus);           // True
///
/// // Create a neutral modifier for a culture without cant requirements
/// var neutral = CantModifier.Neutral("common");
/// Console.WriteLine(neutral.IsNeutral);          // True
/// </code>
/// </example>
public readonly record struct CantModifier(
    CantFluency Fluency,
    int DiceModifier,
    string CultureId,
    string CantName)
{
    /// <summary>
    /// Creates a <see cref="CantModifier"/> from a fluency level, automatically calculating the dice modifier.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <param name="cultureId">The culture identifier (e.g., "dvergr", "utgard").</param>
    /// <param name="cantName">The name of the cant (e.g., "Dvergr Trade-Tongue").</param>
    /// <returns>A new <see cref="CantModifier"/> with the appropriate dice modifier.</returns>
    /// <remarks>
    /// <para>
    /// This is the preferred factory method for creating CantModifiers as it automatically
    /// calculates the dice modifier based on the fluency level using
    /// <see cref="CantFluencyExtensions.GetDiceModifier"/>.
    /// </para>
    /// <para>
    /// The dice modifier is calculated as:
    /// <list type="bullet">
    ///   <item><description><see cref="CantFluency.Fluent"/>: +1</description></item>
    ///   <item><description><see cref="CantFluency.Basic"/>: 0</description></item>
    ///   <item><description><see cref="CantFluency.None"/>: -1</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var fluent = CantModifier.FromFluency(CantFluency.Fluent, "dvergr", "Dvergr Trade-Tongue");
    /// // fluent.DiceModifier == 1
    ///
    /// var none = CantModifier.FromFluency(CantFluency.None, "utgard", "Utgard Veil-Speech");
    /// // none.DiceModifier == -1
    /// </code>
    /// </example>
    public static CantModifier FromFluency(CantFluency fluency, string cultureId, string cantName) =>
        new(fluency, fluency.GetDiceModifier(), cultureId, cantName);

    /// <summary>
    /// Creates a neutral modifier (no effect) for cultures without cant requirements.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <returns>A <see cref="CantModifier"/> with zero effect.</returns>
    /// <remarks>
    /// <para>
    /// Use this factory method when interacting with cultures that do not have specific
    /// cant requirements, or when the character's fluency status is not relevant to
    /// the current interaction.
    /// </para>
    /// <para>
    /// The returned modifier has:
    /// <list type="bullet">
    ///   <item><description>Fluency: <see cref="CantFluency.Basic"/></description></item>
    ///   <item><description>DiceModifier: 0</description></item>
    ///   <item><description>CantName: "Common"</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var neutral = CantModifier.Neutral("common-folk");
    /// // neutral.DiceModifier == 0
    /// // neutral.IsNeutral == true
    /// </code>
    /// </example>
    public static CantModifier Neutral(string cultureId) =>
        new(CantFluency.Basic, 0, cultureId, "Common");

    /// <summary>
    /// Creates a penalty modifier for a character with no knowledge of the cant.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="cantName">The name of the cant.</param>
    /// <returns>A <see cref="CantModifier"/> with a -1 dice penalty.</returns>
    /// <remarks>
    /// This is a convenience method equivalent to calling
    /// <c>FromFluency(CantFluency.None, cultureId, cantName)</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var penalty = CantModifier.Unfamiliar("dvergr", "Dvergr Trade-Tongue");
    /// // penalty.DiceModifier == -1
    /// // penalty.IsPenalty == true
    /// </code>
    /// </example>
    public static CantModifier Unfamiliar(string cultureId, string cantName) =>
        FromFluency(CantFluency.None, cultureId, cantName);

    /// <summary>
    /// Creates a bonus modifier for a character fluent in the cant.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="cantName">The name of the cant.</param>
    /// <returns>A <see cref="CantModifier"/> with a +1 dice bonus.</returns>
    /// <remarks>
    /// This is a convenience method equivalent to calling
    /// <c>FromFluency(CantFluency.Fluent, cultureId, cantName)</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");
    /// // bonus.DiceModifier == 1
    /// // bonus.IsBonus == true
    /// </code>
    /// </example>
    public static CantModifier FluentSpeaker(string cultureId, string cantName) =>
        FromFluency(CantFluency.Fluent, cultureId, cantName);

    /// <summary>
    /// Gets a value indicating whether this modifier provides a bonus to the dice pool.
    /// </summary>
    /// <value>
    /// <c>true</c> if the dice modifier is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A bonus is granted only when the character has <see cref="CantFluency.Fluent"/> proficiency.
    /// </remarks>
    public bool IsBonus => DiceModifier > 0;

    /// <summary>
    /// Gets a value indicating whether this modifier applies a penalty to the dice pool.
    /// </summary>
    /// <value>
    /// <c>true</c> if the dice modifier is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A penalty is applied when the character has <see cref="CantFluency.None"/> proficiency.
    /// </remarks>
    public bool IsPenalty => DiceModifier < 0;

    /// <summary>
    /// Gets a value indicating whether this modifier has no effect on the dice pool.
    /// </summary>
    /// <value>
    /// <c>true</c> if the dice modifier is zero; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A neutral modifier occurs when the character has <see cref="CantFluency.Basic"/> proficiency.
    /// </remarks>
    public bool IsNeutral => DiceModifier == 0;

    /// <summary>
    /// Gets a value indicating whether the character can understand the cant.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character has at least basic understanding; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Characters with <see cref="CantFluency.None"/> cannot understand the cant but may
    /// still attempt communication with penalties.
    /// </remarks>
    public bool CanUnderstand => Fluency.CanUnderstand();

    /// <summary>
    /// Gets a value indicating whether the character can speak the cant.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character can speak the cant; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Characters with <see cref="CantFluency.None"/> cannot speak the cant but may
    /// attempt communication in Common with penalties.
    /// </remarks>
    public bool CanSpeak => Fluency.CanSpeak();

    /// <summary>
    /// Gets a value indicating whether the character has access to cultural dialogue options.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character is fluent and can access special dialogue; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only fluent speakers can access cultural-specific dialogue options that reference
    /// deep cultural knowledge, idioms, or insider references.
    /// </remarks>
    public bool HasCulturalDialogueAccess => Fluency.GrantsCulturalDialogueAccess();

    /// <summary>
    /// Creates a display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the cant name and modifier.</returns>
    /// <example>
    /// <code>
    /// var fluent = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");
    /// Console.WriteLine(fluent.ToDisplayString()); // "Dvergr Trade-Tongue: +1d10 (Fluent)"
    ///
    /// var none = CantModifier.Unfamiliar("utgard", "Utgard Veil-Speech");
    /// Console.WriteLine(none.ToDisplayString());   // "Utgard Veil-Speech: -1d10 (Unfamiliar)"
    /// </code>
    /// </example>
    public string ToDisplayString() => DiceModifier switch
    {
        > 0 => $"{CantName}: +{DiceModifier}d10 (Fluent)",
        < 0 => $"{CantName}: {DiceModifier}d10 (Unfamiliar)",
        _ => $"{CantName}: No modifier (Basic)"
    };

    /// <summary>
    /// Creates a short display string for compact UI elements.
    /// </summary>
    /// <returns>A compact string showing just the modifier value.</returns>
    /// <example>
    /// <code>
    /// var fluent = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");
    /// Console.WriteLine(fluent.ToShortDisplay()); // "+1d10"
    /// </code>
    /// </example>
    public string ToShortDisplay() => DiceModifier switch
    {
        > 0 => $"+{DiceModifier}d10",
        < 0 => $"{DiceModifier}d10",
        _ => "±0"
    };

    /// <summary>
    /// Converts this modifier to a <see cref="SocialModifier"/> for integration with the social context.
    /// </summary>
    /// <returns>A <see cref="SocialModifier"/> representing this cant bonus/penalty.</returns>
    /// <remarks>
    /// <para>
    /// This method enables seamless integration with the existing <see cref="SocialModifier"/> system
    /// from v0.15.3a. The returned modifier has:
    /// <list type="bullet">
    ///   <item><description>Source: "Cant: {CantName}"</description></item>
    ///   <item><description>Description: Fluency description (e.g., "Fluent (+1d10)")</description></item>
    ///   <item><description>DiceModifier: This modifier's dice value</description></item>
    ///   <item><description>DcModifier: 0 (cants affect dice pool, not DC)</description></item>
    ///   <item><description>ModifierType: <see cref="SocialModifierType.Cultural"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var cantModifier = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");
    /// var socialModifier = cantModifier.ToSocialModifier();
    /// // socialModifier.Source == "Cant: Dvergr Trade-Tongue"
    /// // socialModifier.DiceModifier == 1
    /// // socialModifier.ModifierType == SocialModifierType.Cultural
    /// </code>
    /// </example>
    public SocialModifier ToSocialModifier() => new(
        Source: $"Cant: {CantName}",
        Description: Fluency.GetDescription(),
        DiceModifier: DiceModifier,
        DcModifier: 0,
        ModifierType: SocialModifierType.Cultural,
        AppliesToInteractionTypes: null); // Applies to all social interaction types

    /// <summary>
    /// Gets a summary description suitable for logging or debugging.
    /// </summary>
    /// <returns>A detailed string representation of this modifier.</returns>
    public string GetSummary() =>
        $"CantModifier[Culture={CultureId}, Cant={CantName}, Fluency={Fluency}, Modifier={DiceModifier:+0;-0;0}]";

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}
