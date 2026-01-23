// ------------------------------------------------------------------------------
// <copyright file="ProtocolRequirement.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Encapsulates the requirements for observing a cultural protocol, including
// the requirement type, mandatory status, and optional skill check.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the requirements for observing a cultural protocol.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="Entities.CultureProtocol"/> may contain multiple requirements that
/// the character must meet for successful social interaction. Requirements are
/// categorized by type and may be mandatory or optional.
/// </para>
/// <para>
/// Requirement types include:
/// <list type="bullet">
///   <item><description><see cref="ProtocolRequirementType.Behavioral"/>: Actions or behaviors (e.g., patience, posture)</description></item>
///   <item><description><see cref="ProtocolRequirementType.Verbal"/>: Speech patterns or formulas (e.g., honorifics, logical structure)</description></item>
///   <item><description><see cref="ProtocolRequirementType.Offering"/>: Physical gifts or tributes</description></item>
///   <item><description><see cref="ProtocolRequirementType.Mental"/>: Psychological states (e.g., telepathic openness)</description></item>
///   <item><description><see cref="ProtocolRequirementType.Ritual"/>: Ceremonial sequences</description></item>
///   <item><description><see cref="ProtocolRequirementType.StatusAcknowledgment"/>: Recognition of hierarchy or rank</description></item>
/// </list>
/// </para>
/// <para>
/// Mandatory requirements, when failed, trigger a protocol violation. Optional
/// requirements may provide bonuses when met but do not cause violations when skipped.
/// </para>
/// </remarks>
/// <param name="RequirementText">Human-readable description of what the protocol requires.</param>
/// <param name="RequirementType">The category of requirement (behavioral, verbal, offering, etc.).</param>
/// <param name="IsMandatory">Whether failure to meet this requirement triggers a violation.</param>
/// <param name="SkillCheck">Optional skill check required to fulfill the requirement.</param>
/// <param name="ViolationSeverity">The severity of violation if this mandatory requirement is not met.</param>
/// <example>
/// <code>
/// // Create a mandatory behavioral requirement for the Gorge-Maw
/// var patience = ProtocolRequirement.Behavioral(
///     "Listen patiently to extended rumbling discourse without interruption",
///     mandatory: true,
///     violationSeverity: ProtocolViolationType.Moderate);
///
/// // Create an optional offering requirement for the Dvergr
/// var craftGift = ProtocolRequirement.Offering(
///     "Present an item of fine craftsmanship as a gesture of respect",
///     mandatory: false);
/// </code>
/// </example>
public readonly record struct ProtocolRequirement(
    string RequirementText,
    ProtocolRequirementType RequirementType,
    bool IsMandatory,
    SkillCheckRequirement? SkillCheck,
    ProtocolViolationType ViolationSeverity = ProtocolViolationType.Minor)
{
    /// <summary>
    /// Creates a behavioral requirement (e.g., "Listen patiently", "Maintain posture").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new behavioral <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Behavioral requirements demand specific physical actions or demeanor:
    /// <list type="bullet">
    ///   <item><description>Maintaining proper posture or stance</description></item>
    ///   <item><description>Demonstrating patience during extended discourse</description></item>
    ///   <item><description>Following movement protocols</description></item>
    ///   <item><description>Controlling emotional displays</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var patience = ProtocolRequirement.Behavioral(
    ///     "Listen without interruption for the full greeting (30+ minutes)",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Moderate);
    /// </code>
    /// </example>
    public static ProtocolRequirement Behavioral(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor) =>
        new(text, ProtocolRequirementType.Behavioral, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates a verbal requirement (e.g., "Use formal address", "Present logical argument").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new verbal <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Verbal requirements demand specific speech patterns or phrases:
    /// <list type="bullet">
    ///   <item><description>Using correct honorific forms</description></item>
    ///   <item><description>Following greeting and farewell formulas</description></item>
    ///   <item><description>Maintaining logical argument structure</description></item>
    ///   <item><description>Using the appropriate cant or dialect</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var logicChain = ProtocolRequirement.Verbal(
    ///     "Present arguments in precise, non-contradictory logical chains",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Moderate);
    /// </code>
    /// </example>
    public static ProtocolRequirement Verbal(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor) =>
        new(text, ProtocolRequirementType.Verbal, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates an offering requirement (e.g., "Present martial tribute", "Share food").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new offering <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Offering requirements demand tangible gifts or tributes:
    /// <list type="bullet">
    ///   <item><description>Material gifts appropriate to the culture</description></item>
    ///   <item><description>Symbolic items demonstrating respect</description></item>
    ///   <item><description>Shared resources (food, drink, materials)</description></item>
    ///   <item><description>Tributes acknowledging status or achievement</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Failure to provide required offerings may prevent interaction entirely.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tribute = ProtocolRequirement.Offering(
    ///     "Present a martial tribute (weapon, armor piece, or battle trophy)",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Severe);
    /// </code>
    /// </example>
    public static ProtocolRequirement Offering(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor) =>
        new(text, ProtocolRequirementType.Offering, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates a mental requirement (e.g., "Open mind to telepathy", "Suppress hostility").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new mental <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Mental requirements demand specific psychological states:
    /// <list type="bullet">
    ///   <item><description>Opening the mind to telepathic contact</description></item>
    ///   <item><description>Suppressing aggressive or hostile thoughts</description></item>
    ///   <item><description>Maintaining emotional sincerity</description></item>
    ///   <item><description>Achieving meditative calm</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Mental requirements may require skill checks (typically WILL-based).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var openMind = ProtocolRequirement.Mental(
    ///     "Open mind to pack telepathy, share emotional state without deception",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Severe);
    /// </code>
    /// </example>
    public static ProtocolRequirement Mental(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor) =>
        new(text, ProtocolRequirementType.Mental, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates a ritual requirement (e.g., "Complete greeting ceremony", "Blood oath rite").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new ritual <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Ritual requirements demand completion of multi-step ceremonies:
    /// <list type="bullet">
    ///   <item><description>Formal greeting sequences with specific timing</description></item>
    ///   <item><description>Ceremonial exchanges before business discussion</description></item>
    ///   <item><description>Purification or preparation rites</description></item>
    ///   <item><description>Oath-taking or binding agreements</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ritual requirements are often the most complex and may combine elements
    /// of Behavioral, Verbal, and Offering requirements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bloodOath = ProtocolRequirement.Ritual(
    ///     "Complete the blood oath ceremony (exchange of blood, spoken vow, witnessed by third party)",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Unforgivable);
    /// </code>
    /// </example>
    public static ProtocolRequirement Ritual(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor) =>
        new(text, ProtocolRequirementType.Ritual, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates a status acknowledgment requirement (e.g., "Acknowledge pack hierarchy", "Recognize titles").
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new status acknowledgment <see cref="ProtocolRequirement"/>.</returns>
    /// <remarks>
    /// <para>
    /// Status requirements demand recognition of social position:
    /// <list type="bullet">
    ///   <item><description>Acknowledging rank or title before speaking</description></item>
    ///   <item><description>Deferring to elders or leaders</description></item>
    ///   <item><description>Following proper order of address</description></item>
    ///   <item><description>Recognizing achievements or lineage</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ignoring status requirements is often treated as a moderate or severe
    /// violation, as it directly insults the individual's standing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var hierarchy = ProtocolRequirement.StatusAcknowledgment(
    ///     "Acknowledge pack hierarchy, defer to the alpha before addressing others",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Moderate);
    /// </code>
    /// </example>
    public static ProtocolRequirement StatusAcknowledgment(
        string text,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Moderate) =>
        new(text, ProtocolRequirementType.StatusAcknowledgment, mandatory, null, violationSeverity);

    /// <summary>
    /// Creates a requirement with an associated skill check.
    /// </summary>
    /// <param name="text">The requirement description.</param>
    /// <param name="requirementType">The category of requirement.</param>
    /// <param name="skillId">The skill identifier for the check.</param>
    /// <param name="dc">The difficulty class for the skill check.</param>
    /// <param name="failureConsequence">Description of what happens on failed check.</param>
    /// <param name="mandatory">Whether failure triggers a violation.</param>
    /// <param name="violationSeverity">The severity if this requirement is violated.</param>
    /// <returns>A new <see cref="ProtocolRequirement"/> with skill check.</returns>
    /// <example>
    /// <code>
    /// var telepathy = ProtocolRequirement.WithSkillCheck(
    ///     text: "Open your mind to the pack's telepathic contact",
    ///     requirementType: ProtocolRequirementType.Mental,
    ///     skillId: "will",
    ///     dc: 14,
    ///     failureConsequence: "Mind remains closed; Rune-Lupin perceives hostility or deception",
    ///     mandatory: true,
    ///     violationSeverity: ProtocolViolationType.Severe);
    /// </code>
    /// </example>
    public static ProtocolRequirement WithSkillCheck(
        string text,
        ProtocolRequirementType requirementType,
        string skillId,
        int dc,
        string failureConsequence,
        bool mandatory = true,
        ProtocolViolationType violationSeverity = ProtocolViolationType.Minor)
    {
        var skillCheck = new SkillCheckRequirement(skillId, dc, failureConsequence);
        return new ProtocolRequirement(text, requirementType, mandatory, skillCheck, violationSeverity);
    }

    /// <summary>
    /// Gets a value indicating whether this requirement has an associated skill check.
    /// </summary>
    /// <value>
    /// <c>true</c> if a skill check is required; otherwise, <c>false</c>.
    /// </value>
    public bool HasSkillCheck => SkillCheck.HasValue;

    /// <summary>
    /// Gets a value indicating whether failure to meet this requirement is significant.
    /// </summary>
    /// <value>
    /// <c>true</c> if the violation severity is Moderate or higher; otherwise, <c>false</c>.
    /// </value>
    public bool IsSignificantViolation => ViolationSeverity >= ProtocolViolationType.Moderate;

    /// <summary>
    /// Gets a value indicating whether this is an optional requirement.
    /// </summary>
    /// <value>
    /// <c>true</c> if the requirement is optional (not mandatory); otherwise, <c>false</c>.
    /// </value>
    public bool IsOptional => !IsMandatory;

    /// <summary>
    /// Gets the display name for the requirement type.
    /// </summary>
    /// <returns>A human-readable type name.</returns>
    public string GetTypeDisplayName() => RequirementType switch
    {
        ProtocolRequirementType.Behavioral => "Behavior",
        ProtocolRequirementType.Verbal => "Speech",
        ProtocolRequirementType.Offering => "Offering",
        ProtocolRequirementType.Mental => "Mental",
        ProtocolRequirementType.Ritual => "Ritual",
        ProtocolRequirementType.StatusAcknowledgment => "Status",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the requirement.</returns>
    /// <example>
    /// <code>
    /// var req = ProtocolRequirement.Behavioral("Listen patiently", mandatory: true);
    /// Console.WriteLine(req.ToDisplayString()); // "[Required] Behavior: Listen patiently"
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var mandatoryMarker = IsMandatory ? "[Required]" : "[Optional]";
        var skillCheckNote = HasSkillCheck ? $" (DC {SkillCheck!.Value.Dc})" : "";
        return $"{mandatoryMarker} {GetTypeDisplayName()}: {RequirementText}{skillCheckNote}";
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A compact representation of the requirement.</returns>
    public string ToShortDisplay()
    {
        var marker = IsMandatory ? "•" : "○";
        return $"{marker} {RequirementText}";
    }

    /// <summary>
    /// Gets a summary suitable for logging.
    /// </summary>
    /// <returns>A detailed string representation.</returns>
    public string GetSummary() =>
        $"Requirement[Type={RequirementType}, Mandatory={IsMandatory}, " +
        $"Severity={ViolationSeverity}, HasCheck={HasSkillCheck}]: {RequirementText}";

    /// <inheritdoc/>
    public override string ToString() => ToShortDisplay();
}

/// <summary>
/// Represents an optional skill check associated with a protocol requirement.
/// </summary>
/// <remarks>
/// <para>
/// Some protocol requirements may require a skill check to fulfill. For example,
/// a Rune-Lupin mental openness requirement might require a WILL check to
/// successfully open the mind to telepathic contact.
/// </para>
/// <para>
/// If the skill check fails, the <see cref="FailureConsequence"/> describes what happens,
/// which may include a protocol violation if the parent requirement is mandatory.
/// </para>
/// </remarks>
/// <param name="SkillId">The skill identifier (e.g., "will", "wits", "rhetoric").</param>
/// <param name="Dc">The difficulty class for the skill check.</param>
/// <param name="FailureConsequence">Description of what happens if the check fails.</param>
/// <example>
/// <code>
/// var check = new SkillCheckRequirement(
///     SkillId: "will",
///     Dc: 14,
///     FailureConsequence: "Mind remains closed; perceived as hostile");
/// </code>
/// </example>
public readonly record struct SkillCheckRequirement(
    string SkillId,
    int Dc,
    string FailureConsequence)
{
    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the skill check details.</returns>
    public string ToDisplayString() => $"{SkillId.ToUpperInvariant()} DC {Dc}";

    /// <summary>
    /// Gets a detailed description including failure consequence.
    /// </summary>
    /// <returns>A detailed string with failure information.</returns>
    public string ToDetailedDisplay() => $"{ToDisplayString()} - On failure: {FailureConsequence}";

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}
