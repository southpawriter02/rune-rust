// ------------------------------------------------------------------------------
// <copyright file="CulturalProtocolOptions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Configuration options for cultural protocols and cant modifiers.
// Used with the IOptions pattern for dependency injection.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Configuration;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Configuration options for cultural protocols and cant modifiers.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for configuration binding via the IOptions pattern.
/// It specifies cultural protocol definitions, cant modifiers, and Veil-Speech rules.
/// </para>
/// <para>
/// Typical configuration in appsettings.json or cultural-protocols.json:
/// </para>
/// <code>
/// {
///   "CulturalProtocols": {
///     "Protocols": [
///       {
///         "CultureId": "dvergr",
///         "ProtocolName": "Logic-Chain",
///         "BaseDc": 18,
///         "CantName": "Dvergr Trade-Tongue",
///         "SpecialRuleType": "LogicChain"
///       }
///     ],
///     "CantModifiers": {
///       "None": -1,
///       "Basic": 0,
///       "Fluent": 1
///     }
///   }
/// }
/// </code>
/// <para>
/// Usage in Startup/Program.cs:
/// </para>
/// <code>
/// services.Configure&lt;CulturalProtocolOptions&gt;(
///     configuration.GetSection(CulturalProtocolOptions.SectionName));
/// </code>
/// </remarks>
public class CulturalProtocolOptions
{
    /// <summary>
    /// The configuration section name used for binding.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this constant when configuring options in DI:
    /// </para>
    /// <code>
    /// services.Configure&lt;CulturalProtocolOptions&gt;(
    ///     configuration.GetSection(CulturalProtocolOptions.SectionName));
    /// </code>
    /// </remarks>
    public const string SectionName = "CulturalProtocols";

    /// <summary>
    /// Gets or sets the list of cultural protocol configurations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each protocol defines the social interaction rules for a specific culture.
    /// If this list is empty, the service will use default protocols for all
    /// five major cultures (Dvergr, Utgard, Gorge-Maw, Rune-Lupin, Iron-Bane).
    /// </para>
    /// </remarks>
    public List<ProtocolConfig> Protocols { get; set; } = new();

    /// <summary>
    /// Gets or sets the cant modifier configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defines the dice pool modifiers for each fluency level.
    /// Default values are: None=-1, Basic=0, Fluent=+1.
    /// </para>
    /// </remarks>
    public CantModifierConfig CantModifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets the Veil-Speech rules configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defines the specific modifiers for Utgard Veil-Speech interactions.
    /// These values affect DC and dice pool when using deception, direct truth,
    /// or proper Veil-Speech with Utgard NPCs.
    /// </para>
    /// </remarks>
    public VeilSpeechRulesConfig VeilSpeechRules { get; set; } = new();
}

/// <summary>
/// Configuration for a single cultural protocol.
/// </summary>
/// <remarks>
/// <para>
/// Represents the complete definition of a culture's social protocol,
/// including base DC, requirements, consequences, and special rules.
/// </para>
/// </remarks>
public class ProtocolConfig
{
    /// <summary>
    /// Gets or sets the unique culture identifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Case-insensitive identifier used to look up the protocol.
    /// Examples: "dvergr", "utgard", "gorge-maw", "rune-lupin", "iron-bane".
    /// </para>
    /// </remarks>
    public string CultureId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the protocol.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Human-readable name shown to players.
    /// Examples: "Logic-Chain", "Veil-Speech", "Hospitality Rite".
    /// </para>
    /// </remarks>
    public string ProtocolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base difficulty class for social interactions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DC before any modifiers are applied.
    /// Typical values: Dvergr (18), Utgard (16), Gorge-Maw (14), Rune-Lupin (12), Iron-Bane (16).
    /// </para>
    /// </remarks>
    public int BaseDc { get; set; }

    /// <summary>
    /// Gets or sets the human-readable requirements description.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Text describing what the culture expects during social interaction.
    /// This is displayed to players to help them understand protocol rules.
    /// </para>
    /// </remarks>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the violation consequence description.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Text describing what happens when the protocol is violated.
    /// This is displayed to players after a violation occurs.
    /// </para>
    /// </remarks>
    public string ViolationConsequence { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the culture's cant (trade language).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The specialized dialect used by this culture.
    /// Examples: "Dvergr Trade-Tongue", "Utgard Shadow-Tongue", "Gorge-Maw Rumble".
    /// </para>
    /// </remarks>
    public string CantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the special protocol type, if any.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Identifies special rules that modify standard social mechanics.
    /// Nullable - most protocols use standard rules.
    /// </para>
    /// <list type="bullet">
    ///   <item><description>VeilSpeech: Deception respected, truth offends (Utgard)</description></item>
    ///   <item><description>LogicChain: Arguments must be logically consistent (Dvergr)</description></item>
    ///   <item><description>Telepathy: Mental openness required (Rune-Lupin)</description></item>
    ///   <item><description>BloodOath: Binding agreements with consequences (Iron-Bane)</description></item>
    ///   <item><description>HospitalityRite: Extended greeting ceremony required (Gorge-Maw)</description></item>
    /// </list>
    /// </remarks>
    public SpecialProtocolType? SpecialRuleType { get; set; }

    /// <summary>
    /// Gets or sets the detailed requirements list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Individual requirement items that can be checked during compliance evaluation.
    /// Each requirement can be mandatory or optional and has an associated violation severity.
    /// </para>
    /// </remarks>
    public List<DetailedRequirementConfig> DetailedRequirements { get; set; } = new();
}

/// <summary>
/// Configuration for a detailed protocol requirement.
/// </summary>
/// <remarks>
/// <para>
/// Represents a single checkable requirement within a cultural protocol.
/// Requirements can be behavioral, verbal, offering-based, mental, or ritual.
/// </para>
/// </remarks>
public class DetailedRequirementConfig
{
    /// <summary>
    /// Gets or sets the requirement description text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Human-readable description of what is required.
    /// Example: "Present premises before conclusions" (Dvergr Logic-Chain).
    /// </para>
    /// </remarks>
    public string RequirementText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requirement type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Categorizes the nature of the requirement:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Verbal: Related to speech and communication</description></item>
    ///   <item><description>Behavioral: Related to actions and conduct</description></item>
    ///   <item><description>Offering: Related to gifts or tributes</description></item>
    ///   <item><description>Mental: Related to mental state or telepathy</description></item>
    ///   <item><description>Ritual: Related to ceremonial actions</description></item>
    /// </list>
    /// </remarks>
    public string RequirementType { get; set; } = "Behavioral";

    /// <summary>
    /// Gets or sets whether the requirement is mandatory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mandatory requirements cause violations if not met.
    /// Optional requirements provide bonuses when fulfilled.
    /// </para>
    /// </remarks>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Gets or sets the violation severity if this requirement is not met.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only applies to mandatory requirements.
    /// Values: "Minor", "Moderate", "Severe", "Unforgivable".
    /// </para>
    /// </remarks>
    public string ViolationSeverity { get; set; } = "Minor";
}

/// <summary>
/// Configuration for cant modifier values.
/// </summary>
/// <remarks>
/// <para>
/// Defines the dice pool modifiers for each fluency level in a cultural cant.
/// These modifiers are applied to Rhetoric checks during social interactions
/// with NPCs of the associated culture.
/// </para>
/// </remarks>
public class CantModifierConfig
{
    /// <summary>
    /// Gets or sets the modifier for characters with no cant knowledge.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied when a character has <see cref="CantFluency.None"/>.
    /// Default is -1 (remove one die from the pool).
    /// </para>
    /// </remarks>
    public int None { get; set; } = -1;

    /// <summary>
    /// Gets or sets the modifier for characters with basic cant understanding.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied when a character has <see cref="CantFluency.Basic"/>.
    /// Default is 0 (no modification to the pool).
    /// </para>
    /// </remarks>
    public int Basic { get; set; } = 0;

    /// <summary>
    /// Gets or sets the modifier for characters fluent in the cant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied when a character has <see cref="CantFluency.Fluent"/>.
    /// Default is +1 (add one die to the pool).
    /// </para>
    /// </remarks>
    public int Fluent { get; set; } = 1;
}

/// <summary>
/// Configuration for Veil-Speech rules (Utgard-specific).
/// </summary>
/// <remarks>
/// <para>
/// Defines the specific modifiers for Utgard Veil-Speech interactions.
/// Veil-Speech is the cultural protocol where deception is respected and
/// direct truth-telling is considered offensive.
/// </para>
/// </remarks>
public class VeilSpeechRulesConfig
{
    /// <summary>
    /// Gets or sets the DC reduction for deception attempts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The amount by which the DC is reduced when using deception
    /// (lies within veil-speech are culturally accepted).
    /// Default is 4.
    /// </para>
    /// </remarks>
    public int DeceptionDcReduction { get; set; } = 4;

    /// <summary>
    /// Gets or sets the bonus dice for proper Veil-Speech.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The number of dice added to the pool when using proper
    /// Veil-Speech (layered, indirect communication).
    /// Default is +1.
    /// </para>
    /// </remarks>
    public int ProperVeilSpeechBonus { get; set; } = 1;

    /// <summary>
    /// Gets or sets the penalty for direct truth-telling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The number of dice removed from the pool when telling
    /// direct, unvarnished truth (offensive to Utgard).
    /// Default is -2 (stored as negative value).
    /// </para>
    /// </remarks>
    public int DirectTruthPenalty { get; set; } = -2;

    /// <summary>
    /// Gets or sets the number of offenses before escalating to DeepOffense.
    /// </summary>
    /// <remarks>
    /// <para>
    /// After this many direct truth offenses, the Veil-Speech state
    /// escalates from Offended to DeepOffense, which may block interaction.
    /// Default is 2.
    /// </para>
    /// </remarks>
    public int OffenseEscalationThreshold { get; set; } = 2;
}
