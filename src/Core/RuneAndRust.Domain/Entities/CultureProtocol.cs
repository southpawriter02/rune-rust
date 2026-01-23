// ------------------------------------------------------------------------------
// <copyright file="CultureProtocol.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a cultural protocol - the formal rules and requirements for social
// interaction with members of a specific culture in Aethelgard.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a cultural protocol - the formal rules and requirements for social
/// interaction with members of a specific culture in Aethelgard.
/// </summary>
/// <remarks>
/// <para>
/// Each major culture has unique social protocols that characters must observe to
/// interact successfully. Failure to observe these protocols results in social penalties
/// ranging from minor embarrassment to permanent relationship damage.
/// </para>
/// <para>
/// Defined protocols and their characteristics:
/// <list type="bullet">
///   <item><description><b>Dvergr Logic-Chain (DC 18)</b>: Precise, non-contradictory argument sequences. Emotional appeals are penalized; technical precision is rewarded.</description></item>
///   <item><description><b>Utgard Veil-Speech (DC 16)</b>: Layer truth within acceptable deception. Direct truth offends; deception is respected.</description></item>
///   <item><description><b>Gorge-Maw Patience (DC 14)</b>: Extended hospitality rites and patient listening. Interruption is a serious offense.</description></item>
///   <item><description><b>Rune-Lupin Telepathy (DC 12)</b>: Open mind to pack telepathy and suppress hostility. Closed minds are distrusted.</description></item>
///   <item><description><b>Iron-Bane Tribute (DC 16)</b>: Martial tribute before negotiation. Strength and martial achievement must be demonstrated.</description></item>
/// </list>
/// </para>
/// <para>
/// Each protocol has:
/// <list type="bullet">
///   <item><description>A base DC for protocol compliance checks</description></item>
///   <item><description>One or more requirements (behavioral, verbal, offering, etc.)</description></item>
///   <item><description>Optionally, special rules that modify standard mechanics</description></item>
///   <item><description>An associated cant (dialect/language) that affects checks</description></item>
///   <item><description>Violation consequences and recovery options</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create the Dvergr Logic-Chain protocol
/// var dvergr = CultureProtocol.Create(
///     cultureId: "dvergr",
///     protocolName: "Logic-Chain",
///     baseDc: 18,
///     requirements: "Present arguments in precise, non-contradictory chains. " +
///                   "Emotional appeals are considered weakness.",
///     violationConsequence: "The Dvergr's expression hardens. 'Your logic is flawed. " +
///                           "Perhaps you should return when your thoughts are ordered.'",
///     cantName: "Dvergr Trade-Tongue",
///     specialRuleType: SpecialProtocolType.LogicChain);
///
/// Console.WriteLine(dvergr.IsLogicChain);  // True
/// Console.WriteLine(dvergr.BaseDc);        // 18
/// </code>
/// </example>
public sealed class CultureProtocol
{
    /// <summary>
    /// The detailed requirements for this protocol.
    /// </summary>
    private readonly List<ProtocolRequirement> _requirements = new();

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private CultureProtocol()
    {
    }

    /// <summary>
    /// Gets the unique identifier for the culture (e.g., "dvergr", "utgard").
    /// </summary>
    /// <remarks>
    /// This ID is used to look up protocols and should match NPC culture assignments.
    /// </remarks>
    public string CultureId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name for the culture (e.g., "Dvergr", "Utgard").
    /// </summary>
    public string CultureName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the name of this protocol (e.g., "Logic-Chain", "Veil-Speech").
    /// </summary>
    /// <remarks>
    /// The protocol name is used for display and logging purposes.
    /// </remarks>
    public string ProtocolName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the base difficulty class for successfully observing this protocol.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The base DC represents the minimum difficulty of correctly following
    /// the protocol. Modifiers from cant fluency, prior violations, and other
    /// factors may adjust the effective DC.
    /// </para>
    /// <para>
    /// Culture base DCs:
    /// <list type="bullet">
    ///   <item><description>Dvergr: 18 (most demanding)</description></item>
    ///   <item><description>Utgard: 16</description></item>
    ///   <item><description>Iron-Bane: 16</description></item>
    ///   <item><description>Gorge-Maw: 14</description></item>
    ///   <item><description>Rune-Lupin: 12 (most accessible)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int BaseDc { get; private set; }

    /// <summary>
    /// Gets the human-readable description of what the protocol requires.
    /// </summary>
    /// <remarks>
    /// This is a summary description suitable for display to players before
    /// initiating social interaction.
    /// </remarks>
    public string Requirements { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of consequences for violating this protocol.
    /// </summary>
    /// <remarks>
    /// This is narrative text displayed when a violation occurs, helping
    /// players understand the social impact of their actions.
    /// </remarks>
    public string ViolationConsequence { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the name of the cant (dialect/language) associated with this culture.
    /// </summary>
    /// <remarks>
    /// Fluency in the cant affects dice pools for social checks:
    /// <list type="bullet">
    ///   <item><description>Fluent: +1d10</description></item>
    ///   <item><description>Basic: +0</description></item>
    ///   <item><description>None: -1d10</description></item>
    /// </list>
    /// </remarks>
    public string CantName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this protocol has special rules.
    /// </summary>
    /// <value>
    /// <c>true</c> if special rules apply; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Special rules fundamentally change how certain interaction mechanics work,
    /// such as Veil-Speech inverting the value of deception.
    /// </remarks>
    public bool HasSpecialRules { get; private set; }

    /// <summary>
    /// Gets the type of special rules, if any.
    /// </summary>
    /// <value>
    /// The <see cref="SpecialProtocolType"/> if special rules apply;
    /// otherwise, <c>null</c>.
    /// </value>
    public SpecialProtocolType? SpecialRuleType { get; private set; }

    /// <summary>
    /// Gets the detailed requirements for this protocol.
    /// </summary>
    /// <remarks>
    /// Each requirement specifies what the character must do (or avoid doing)
    /// to comply with the protocol. Mandatory requirements must be met;
    /// optional requirements provide additional bonuses.
    /// </remarks>
    public IReadOnlyList<ProtocolRequirement> DetailedRequirements => _requirements.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this is the Utgard Veil-Speech protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Veil-Speech protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Veil-Speech has unique mechanics where deception is respected and truth offends.
    /// </remarks>
    public bool IsVeilSpeech => SpecialRuleType == SpecialProtocolType.VeilSpeech;

    /// <summary>
    /// Gets a value indicating whether this is the Dvergr Logic-Chain protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Logic-Chain protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Logic-Chain requires consistent, non-contradictory arguments.
    /// </remarks>
    public bool IsLogicChain => SpecialRuleType == SpecialProtocolType.LogicChain;

    /// <summary>
    /// Gets a value indicating whether this is the Rune-Lupin Telepathy protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Telepathy protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Telepathy requires mental openness and suppression of hostile thoughts.
    /// </remarks>
    public bool IsTelepathy => SpecialRuleType == SpecialProtocolType.Telepathy;

    /// <summary>
    /// Gets a value indicating whether this is the Iron-Bane Blood Oath protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Blood Oath protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Blood Oath creates magically binding agreements with supernatural consequences.
    /// </remarks>
    public bool IsBloodOath => SpecialRuleType == SpecialProtocolType.BloodOath;

    /// <summary>
    /// Gets a value indicating whether this is the Gorge-Maw Hospitality Rite protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Hospitality Rite protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Hospitality Rite requires extended greeting ceremonies and patience.
    /// </remarks>
    public bool IsHospitalityRite => SpecialRuleType == SpecialProtocolType.HospitalityRite;

    /// <summary>
    /// Gets a value indicating whether this is the Iron-Bane Martial Tribute protocol.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the Martial Tribute protocol; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Martial Tribute requires demonstration of martial status through gifts or achievements.
    /// </remarks>
    public bool IsMartialTribute => SpecialRuleType == SpecialProtocolType.MartialTribute;

    /// <summary>
    /// Gets a value indicating whether this protocol has mandatory requirements.
    /// </summary>
    /// <value>
    /// <c>true</c> if any requirement is mandatory; otherwise, <c>false</c>.
    /// </value>
    public bool HasMandatoryRequirements => _requirements.Any(r => r.IsMandatory);

    /// <summary>
    /// Gets the count of mandatory requirements.
    /// </summary>
    public int MandatoryRequirementCount => _requirements.Count(r => r.IsMandatory);

    /// <summary>
    /// Gets the count of optional requirements.
    /// </summary>
    public int OptionalRequirementCount => _requirements.Count(r => r.IsOptional);

    /// <summary>
    /// Creates a new <see cref="CultureProtocol"/> with the specified parameters.
    /// </summary>
    /// <param name="cultureId">Unique identifier for the culture.</param>
    /// <param name="cultureName">Display name for the culture.</param>
    /// <param name="protocolName">Name of the protocol.</param>
    /// <param name="baseDc">Base difficulty class for protocol compliance.</param>
    /// <param name="requirements">Human-readable requirements description.</param>
    /// <param name="violationConsequence">Description of violation consequences.</param>
    /// <param name="cantName">Name of the associated cant.</param>
    /// <param name="specialRuleType">Optional special rule type.</param>
    /// <param name="detailedRequirements">Optional detailed requirements list.</param>
    /// <returns>A new <see cref="CultureProtocol"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="cultureId"/> or <paramref name="protocolName"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="baseDc"/> is less than 1.
    /// </exception>
    public static CultureProtocol Create(
        string cultureId,
        string cultureName,
        string protocolName,
        int baseDc,
        string requirements,
        string violationConsequence,
        string cantName,
        SpecialProtocolType? specialRuleType = null,
        IReadOnlyList<ProtocolRequirement>? detailedRequirements = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));
        ArgumentException.ThrowIfNullOrWhiteSpace(protocolName, nameof(protocolName));
        ArgumentOutOfRangeException.ThrowIfLessThan(baseDc, 1, nameof(baseDc));

        var protocol = new CultureProtocol
        {
            CultureId = cultureId.ToLowerInvariant(),
            CultureName = cultureName,
            ProtocolName = protocolName,
            BaseDc = baseDc,
            Requirements = requirements ?? string.Empty,
            ViolationConsequence = violationConsequence ?? string.Empty,
            CantName = cantName ?? "Common",
            HasSpecialRules = specialRuleType.HasValue && specialRuleType != SpecialProtocolType.None,
            SpecialRuleType = specialRuleType
        };

        if (detailedRequirements != null)
        {
            protocol._requirements.AddRange(detailedRequirements);
        }

        return protocol;
    }

    /// <summary>
    /// Creates the Dvergr Logic-Chain protocol.
    /// </summary>
    /// <returns>A <see cref="CultureProtocol"/> for Dvergr culture.</returns>
    public static CultureProtocol Dvergr() => Create(
        cultureId: "dvergr",
        cultureName: "Dvergr",
        protocolName: "Logic-Chain",
        baseDc: 18,
        requirements: "Present arguments in precise, non-contradictory logical chains. " +
                      "Emotional appeals are considered weakness; technical precision is valued.",
        violationConsequence: "The Dvergr's expression hardens with barely concealed disdain. " +
                              "'Your logic is... imprecise. Return when your thoughts are properly ordered.'",
        cantName: "Dvergr Trade-Tongue",
        specialRuleType: SpecialProtocolType.LogicChain,
        detailedRequirements: new[]
        {
            ProtocolRequirement.Verbal(
                "Present arguments in logical sequence without contradiction",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate),
            ProtocolRequirement.Verbal(
                "Avoid emotional appeals or passionate rhetoric",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Minor),
            ProtocolRequirement.Behavioral(
                "Maintain measured, precise speech patterns",
                mandatory: false)
        });

    /// <summary>
    /// Creates the Utgard Veil-Speech protocol.
    /// </summary>
    /// <returns>A <see cref="CultureProtocol"/> for Utgard culture.</returns>
    public static CultureProtocol Utgard() => Create(
        cultureId: "utgard",
        cultureName: "Utgard",
        protocolName: "Veil-Speech",
        baseDc: 16,
        requirements: "Layer truth within acceptable deception. Direct truth-telling is " +
                      "considered offensive; deception is a sign of respect and sophistication.",
        violationConsequence: "The Utgard's eyes narrow coldly. 'You speak as a child speaks - " +
                              "without art or subtlety. I had expected better from one who seeks my attention.'",
        cantName: "Utgard Veil-Speech",
        specialRuleType: SpecialProtocolType.VeilSpeech,
        detailedRequirements: new[]
        {
            ProtocolRequirement.Verbal(
                "Layer truth within acceptable misdirection (proper Veil-Speech)",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate),
            ProtocolRequirement.Behavioral(
                "Never speak direct, unvarnished truth",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Severe),
            ProtocolRequirement.Verbal(
                "Demonstrate wit through elegant deception",
                mandatory: false)
        });

    /// <summary>
    /// Creates the Gorge-Maw Hospitality Rite protocol.
    /// </summary>
    /// <returns>A <see cref="CultureProtocol"/> for Gorge-Maw culture.</returns>
    public static CultureProtocol GorgeMaw() => Create(
        cultureId: "gorge-maw",
        cultureName: "Gorge-Maw",
        protocolName: "Hospitality Rite",
        baseDc: 14,
        requirements: "Complete the full hospitality greeting, share food before business, " +
                      "and listen patiently to extended rumbling discourse without interruption.",
        violationConsequence: "The Gorge-Maw falls silent, their rumbling chant cut short. " +
                              "The offense is palpable - to interrupt hospitality is to reject kinship itself.",
        cantName: "Gorge-Maw Rumble",
        specialRuleType: SpecialProtocolType.HospitalityRite,
        detailedRequirements: new[]
        {
            ProtocolRequirement.Ritual(
                "Complete the full hospitality greeting (30+ minutes)",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Severe),
            ProtocolRequirement.Offering(
                "Share food before discussing any business",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate),
            ProtocolRequirement.Behavioral(
                "Listen patiently without interruption",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate)
        });

    /// <summary>
    /// Creates the Rune-Lupin Telepathy protocol.
    /// </summary>
    /// <returns>A <see cref="CultureProtocol"/> for Rune-Lupin culture.</returns>
    public static CultureProtocol RuneLupin() => Create(
        cultureId: "rune-lupin",
        cultureName: "Rune-Lupin",
        protocolName: "Pack Telepathy",
        baseDc: 12,
        requirements: "Open your mind to the pack's telepathic contact and suppress hostile emotions. " +
                      "Closed minds are distrusted; deception is nearly impossible.",
        violationConsequence: "The Rune-Lupin's hackles rise as they sense resistance. " +
                              "'Your mind is closed. What do you hide from the pack?'",
        cantName: "Rune-Lupin Pack-Speech",
        specialRuleType: SpecialProtocolType.Telepathy,
        detailedRequirements: new[]
        {
            ProtocolRequirement.WithSkillCheck(
                "Open your mind to pack telepathy",
                ProtocolRequirementType.Mental,
                skillId: "will",
                dc: 14,
                failureConsequence: "Mind remains closed; perceived as hostile or deceptive",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Severe),
            ProtocolRequirement.Mental(
                "Suppress hostile thoughts and emotions",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Severe),
            ProtocolRequirement.StatusAcknowledgment(
                "Acknowledge pack hierarchy and defer to alpha",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate)
        });

    /// <summary>
    /// Creates the Iron-Bane Martial Tribute protocol.
    /// </summary>
    /// <returns>A <see cref="CultureProtocol"/> for Iron-Bane culture.</returns>
    public static CultureProtocol IronBane() => Create(
        cultureId: "iron-bane",
        cultureName: "Iron-Bane",
        protocolName: "Martial Tribute",
        baseDc: 16,
        requirements: "Present martial tribute (weapon, armor piece, or battle trophy) before negotiation. " +
                      "Demonstrate martial achievement through recounting of deeds.",
        violationConsequence: "The Iron-Bane warrior regards you with contempt. " +
                              "'You come with neither blade nor tale of blood. What word has one who has never shed it?'",
        cantName: "Iron-Bane Battle-Tongue",
        specialRuleType: SpecialProtocolType.MartialTribute,
        detailedRequirements: new[]
        {
            ProtocolRequirement.Offering(
                "Present a martial tribute (weapon, armor, or trophy)",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Moderate),
            ProtocolRequirement.Verbal(
                "State your martial achievements before making requests",
                mandatory: true,
                violationSeverity: ProtocolViolationType.Minor),
            ProtocolRequirement.Behavioral(
                "Maintain martial bearing; do not show weakness",
                mandatory: false)
        });

    /// <summary>
    /// Adds a requirement to this protocol.
    /// </summary>
    /// <param name="requirement">The requirement to add.</param>
    /// <remarks>
    /// This method is primarily used during protocol construction or when
    /// loading protocols from configuration.
    /// </remarks>
    public void AddRequirement(ProtocolRequirement requirement)
    {
        _requirements.Add(requirement);
    }

    /// <summary>
    /// Adds multiple requirements to this protocol.
    /// </summary>
    /// <param name="requirements">The requirements to add.</param>
    public void AddRequirements(IEnumerable<ProtocolRequirement> requirements)
    {
        _requirements.AddRange(requirements);
    }

    /// <summary>
    /// Gets the mandatory requirements only.
    /// </summary>
    /// <returns>A list of mandatory requirements.</returns>
    public IReadOnlyList<ProtocolRequirement> GetMandatoryRequirements() =>
        _requirements.Where(r => r.IsMandatory).ToList().AsReadOnly();

    /// <summary>
    /// Gets the optional requirements only.
    /// </summary>
    /// <returns>A list of optional requirements.</returns>
    public IReadOnlyList<ProtocolRequirement> GetOptionalRequirements() =>
        _requirements.Where(r => r.IsOptional).ToList().AsReadOnly();

    /// <summary>
    /// Gets requirements of a specific type.
    /// </summary>
    /// <param name="type">The requirement type to filter by.</param>
    /// <returns>A list of requirements of the specified type.</returns>
    public IReadOnlyList<ProtocolRequirement> GetRequirementsByType(ProtocolRequirementType type) =>
        _requirements.Where(r => r.RequirementType == type).ToList().AsReadOnly();

    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the protocol details.</returns>
    public string ToDisplayString() =>
        $"{CultureName} {ProtocolName} (DC {BaseDc})";

    /// <summary>
    /// Gets a detailed display string including requirements.
    /// </summary>
    /// <returns>A multi-line string with full protocol details.</returns>
    public string ToDetailedDisplay()
    {
        var lines = new List<string>
        {
            $"{CultureName} {ProtocolName}",
            $"Base DC: {BaseDc}",
            $"Cant: {CantName}",
            string.Empty,
            "Requirements:",
        };

        foreach (var req in _requirements)
        {
            lines.Add($"  {req.ToShortDisplay()}");
        }

        if (HasSpecialRules)
        {
            lines.Add(string.Empty);
            lines.Add($"Special Rules: {SpecialRuleType}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a summary suitable for logging.
    /// </summary>
    /// <returns>A detailed string representation for debugging.</returns>
    public string GetSummary() =>
        $"CultureProtocol[Id={CultureId}, Name={ProtocolName}, DC={BaseDc}, " +
        $"Cant={CantName}, Special={SpecialRuleType}, Requirements={_requirements.Count}]";

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}
