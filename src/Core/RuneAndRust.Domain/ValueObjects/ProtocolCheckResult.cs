// ------------------------------------------------------------------------------
// <copyright file="ProtocolCheckResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the result of evaluating protocol compliance for a social interaction,
// including violation type, applied modifiers, and consequences.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Captures the result of evaluating protocol compliance for a social interaction.
/// </summary>
/// <remarks>
/// <para>
/// When a character initiates a social interaction with an NPC from a specific culture,
/// their adherence to that culture's protocol is evaluated. This value object captures
/// the outcome of that evaluation, including:
/// <list type="bullet">
///   <item><description>Whether the character is compliant with the protocol</description></item>
///   <item><description>The type and severity of any violation</description></item>
///   <item><description>All modifiers to be applied (dice pool and DC adjustments)</description></item>
///   <item><description>Disposition changes resulting from the interaction</description></item>
/// </list>
/// </para>
/// <para>
/// Compliance outcomes range from full compliance (no penalties) to unforgivable
/// violations that block further interaction entirely.
/// </para>
/// </remarks>
/// <param name="IsCompliant">Whether the character met the protocol requirements.</param>
/// <param name="ViolationType">The type of violation if non-compliant.</param>
/// <param name="CultureId">The culture whose protocol was evaluated.</param>
/// <param name="ProtocolName">The name of the protocol (e.g., "Logic-Chain", "Veil-Speech").</param>
/// <param name="AppliedModifiers">All modifiers applied from protocol evaluation.</param>
/// <param name="ConsequenceDescription">Human-readable description of consequences.</param>
/// <param name="DispositionChange">Change to NPC disposition from this check.</param>
/// <param name="DcAdjustment">Adjustment to the social check DC (positive increases difficulty).</param>
/// <param name="FailedRequirements">List of requirements that were not met.</param>
/// <example>
/// <code>
/// // Create a compliant result
/// var compliant = ProtocolCheckResult.Compliant("dvergr", "Logic-Chain");
/// Console.WriteLine(compliant.IsCompliant);         // True
/// Console.WriteLine(compliant.BlocksInteraction);   // False
///
/// // Create a violation result
/// var violation = ProtocolCheckResult.Violation(
///     "dvergr", "Logic-Chain",
///     ProtocolViolationType.Moderate,
///     failedRequirements: new[] { "Present arguments in logical sequence" });
/// Console.WriteLine(violation.DcAdjustment);        // 4
/// Console.WriteLine(violation.TotalDiceModifier);   // -1
/// </code>
/// </example>
public readonly record struct ProtocolCheckResult(
    bool IsCompliant,
    ProtocolViolationType ViolationType,
    string CultureId,
    string ProtocolName,
    IReadOnlyList<SocialModifier> AppliedModifiers,
    string ConsequenceDescription,
    int DispositionChange,
    int DcAdjustment,
    IReadOnlyList<string> FailedRequirements)
{
    /// <summary>
    /// Creates a successful compliance result.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="protocolName">The protocol name.</param>
    /// <param name="additionalModifiers">Optional additional modifiers to apply.</param>
    /// <returns>A <see cref="ProtocolCheckResult"/> indicating successful compliance.</returns>
    /// <remarks>
    /// Compliance means the character has met all mandatory requirements of the protocol.
    /// No penalties are applied, and the interaction proceeds normally.
    /// </remarks>
    public static ProtocolCheckResult Compliant(
        string cultureId,
        string protocolName,
        IReadOnlyList<SocialModifier>? additionalModifiers = null) =>
        new(
            IsCompliant: true,
            ViolationType: ProtocolViolationType.None,
            CultureId: cultureId,
            ProtocolName: protocolName,
            AppliedModifiers: additionalModifiers ?? Array.Empty<SocialModifier>(),
            ConsequenceDescription: "Protocol observed correctly. The NPC responds favorably to your cultural awareness.",
            DispositionChange: 0,
            DcAdjustment: 0,
            FailedRequirements: Array.Empty<string>());

    /// <summary>
    /// Creates a compliance result with a bonus for exceptional protocol adherence.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="protocolName">The protocol name.</param>
    /// <param name="bonusDescription">Description of why the bonus was earned.</param>
    /// <param name="diceBonus">The dice bonus earned (positive number).</param>
    /// <returns>A <see cref="ProtocolCheckResult"/> with bonus modifiers.</returns>
    /// <remarks>
    /// Exceptional compliance (going beyond minimum requirements) can earn additional
    /// dice pool bonuses, such as when using proper Veil-Speech with Utgard.
    /// </remarks>
    public static ProtocolCheckResult CompliantWithBonus(
        string cultureId,
        string protocolName,
        string bonusDescription,
        int diceBonus)
    {
        var modifier = new SocialModifier(
            Source: $"Protocol: {protocolName}",
            Description: bonusDescription,
            DiceModifier: diceBonus,
            DcModifier: 0,
            ModifierType: SocialModifierType.Cultural);

        return new ProtocolCheckResult(
            IsCompliant: true,
            ViolationType: ProtocolViolationType.None,
            CultureId: cultureId,
            ProtocolName: protocolName,
            AppliedModifiers: new[] { modifier },
            ConsequenceDescription: $"Exceptional protocol adherence. {bonusDescription}",
            DispositionChange: 5, // Small positive disposition change
            DcAdjustment: 0,
            FailedRequirements: Array.Empty<string>());
    }

    /// <summary>
    /// Creates a violation result with appropriate consequences.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="protocolName">The protocol name.</param>
    /// <param name="violationType">The type of violation that occurred.</param>
    /// <param name="failedRequirements">List of requirements that were not met.</param>
    /// <param name="additionalModifiers">Optional additional modifiers beyond violation penalties.</param>
    /// <returns>A <see cref="ProtocolCheckResult"/> indicating a violation.</returns>
    /// <remarks>
    /// <para>
    /// Violations apply escalating penalties based on severity:
    /// <list type="bullet">
    ///   <item><description>Minor: +2 DC</description></item>
    ///   <item><description>Moderate: +4 DC, -1d10</description></item>
    ///   <item><description>Severe: +6 DC, -2d10</description></item>
    ///   <item><description>Unforgivable: Interaction blocked</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static ProtocolCheckResult Violation(
        string cultureId,
        string protocolName,
        ProtocolViolationType violationType,
        IReadOnlyList<string>? failedRequirements = null,
        IReadOnlyList<SocialModifier>? additionalModifiers = null)
    {
        var modifiers = new List<SocialModifier>();

        // Add violation penalty modifier
        var dicePenalty = violationType.GetDicePenalty();
        if (dicePenalty > 0)
        {
            modifiers.Add(new SocialModifier(
                Source: $"Protocol Violation: {protocolName}",
                Description: $"{violationType.GetDisplayName()} violation",
                DiceModifier: -dicePenalty,
                DcModifier: 0,
                ModifierType: SocialModifierType.Cultural));
        }

        // Add any additional modifiers
        if (additionalModifiers != null)
        {
            modifiers.AddRange(additionalModifiers);
        }

        return new ProtocolCheckResult(
            IsCompliant: false,
            ViolationType: violationType,
            CultureId: cultureId,
            ProtocolName: protocolName,
            AppliedModifiers: modifiers,
            ConsequenceDescription: violationType.GetConsequenceDescription(),
            DispositionChange: violationType.GetDispositionChange(),
            DcAdjustment: violationType.GetDcIncrease(),
            FailedRequirements: failedRequirements ?? Array.Empty<string>());
    }

    /// <summary>
    /// Creates a result for interactions with no protocol requirements.
    /// </summary>
    /// <returns>A <see cref="ProtocolCheckResult"/> indicating no protocol applies.</returns>
    /// <remarks>
    /// Use this when the NPC does not belong to a culture with defined protocols,
    /// or when the interaction type does not require protocol compliance.
    /// </remarks>
    public static ProtocolCheckResult NoProtocol() =>
        new(
            IsCompliant: true,
            ViolationType: ProtocolViolationType.None,
            CultureId: string.Empty,
            ProtocolName: string.Empty,
            AppliedModifiers: Array.Empty<SocialModifier>(),
            ConsequenceDescription: "No cultural protocol required for this interaction.",
            DispositionChange: 0,
            DcAdjustment: 0,
            FailedRequirements: Array.Empty<string>());

    /// <summary>
    /// Creates a result for interactions blocked due to prior violations.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="protocolName">The protocol name.</param>
    /// <param name="reason">The reason interaction is blocked.</param>
    /// <returns>A <see cref="ProtocolCheckResult"/> indicating blocked interaction.</returns>
    /// <remarks>
    /// This should be used when prior unforgivable violations or deep offense states
    /// prevent any social interaction with the NPC.
    /// </remarks>
    public static ProtocolCheckResult Blocked(
        string cultureId,
        string protocolName,
        string reason) =>
        new(
            IsCompliant: false,
            ViolationType: ProtocolViolationType.Unforgivable,
            CultureId: cultureId,
            ProtocolName: protocolName,
            AppliedModifiers: Array.Empty<SocialModifier>(),
            ConsequenceDescription: reason,
            DispositionChange: 0, // No additional change - already at worst state
            DcAdjustment: int.MaxValue,
            FailedRequirements: new[] { "Prior unforgivable violation" });

    /// <summary>
    /// Gets a value indicating whether this result blocks further interaction.
    /// </summary>
    /// <value>
    /// <c>true</c> if the violation type blocks interaction; otherwise, <c>false</c>.
    /// </value>
    public bool BlocksInteraction => ViolationType.BlocksInteraction();

    /// <summary>
    /// Gets a value indicating whether the violation can be recovered from.
    /// </summary>
    /// <value>
    /// <c>true</c> if recovery is possible; otherwise, <c>false</c>.
    /// </value>
    public bool IsRecoverable => ViolationType.IsRecoverable();

    /// <summary>
    /// Gets a value indicating whether this result may trigger NPC hostility.
    /// </summary>
    /// <value>
    /// <c>true</c> if the violation may cause hostility; otherwise, <c>false</c>.
    /// </value>
    public bool MayTriggerHostility => ViolationType.MayTriggerHostility();

    /// <summary>
    /// Gets a value indicating whether this result affects faction reputation.
    /// </summary>
    /// <value>
    /// <c>true</c> if faction reputation is affected; otherwise, <c>false</c>.
    /// </value>
    public bool AffectsFactionReputation => ViolationType.AffectsFactionReputation();

    /// <summary>
    /// Gets a value indicating whether any protocol was evaluated.
    /// </summary>
    /// <value>
    /// <c>true</c> if a protocol was evaluated; otherwise, <c>false</c>.
    /// </value>
    public bool HasProtocol => !string.IsNullOrEmpty(CultureId);

    /// <summary>
    /// Gets a value indicating whether there are failed requirements.
    /// </summary>
    /// <value>
    /// <c>true</c> if one or more requirements were not met; otherwise, <c>false</c>.
    /// </value>
    public bool HasFailedRequirements => FailedRequirements.Count > 0;

    /// <summary>
    /// Gets the total dice modifier from all applied modifiers.
    /// </summary>
    /// <value>
    /// The sum of all dice modifiers (positive or negative).
    /// </value>
    public int TotalDiceModifier => AppliedModifiers.Sum(m => m.DiceModifier);

    /// <summary>
    /// Gets the total DC modifier from all applied modifiers plus the violation DC adjustment.
    /// </summary>
    /// <value>
    /// The effective DC change for the social check.
    /// </value>
    public int TotalDcModifier => DcAdjustment + AppliedModifiers.Sum(m => m.DcModifier);

    /// <summary>
    /// Gets the recovery description for the current violation.
    /// </summary>
    /// <returns>Text describing how to recover from the violation.</returns>
    public string GetRecoveryDescription() => ViolationType.GetRecoveryDescription();

    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the result.</returns>
    public string ToDisplayString()
    {
        if (!HasProtocol)
            return "No protocol required";

        if (IsCompliant)
        {
            var bonus = TotalDiceModifier > 0 ? $" (+{TotalDiceModifier}d10)" : "";
            return $"✓ {ProtocolName} protocol observed{bonus}";
        }

        var parts = new List<string>
        {
            $"✗ {ProtocolName}: {ViolationType.GetDisplayName()}"
        };

        if (DcAdjustment > 0 && DcAdjustment < int.MaxValue)
            parts.Add($"DC +{DcAdjustment}");

        if (TotalDiceModifier < 0)
            parts.Add($"{TotalDiceModifier}d10");

        if (BlocksInteraction)
            parts.Add("[BLOCKED]");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A compact representation of the result.</returns>
    public string ToShortDisplay()
    {
        if (!HasProtocol)
            return "—";

        if (IsCompliant)
            return TotalDiceModifier > 0 ? $"✓ +{TotalDiceModifier}d10" : "✓";

        if (BlocksInteraction)
            return "✗ BLOCKED";

        return $"✗ DC+{DcAdjustment}";
    }

    /// <summary>
    /// Gets a summary suitable for logging.
    /// </summary>
    /// <returns>A detailed string representation for debugging.</returns>
    public string GetSummary()
    {
        if (!HasProtocol)
            return "ProtocolCheckResult[NoProtocol]";

        return $"ProtocolCheckResult[Culture={CultureId}, Protocol={ProtocolName}, " +
               $"Compliant={IsCompliant}, Violation={ViolationType}, " +
               $"DC+{DcAdjustment}, Dice={TotalDiceModifier:+0;-0}, " +
               $"Disposition={DispositionChange:+0;-0}, Blocked={BlocksInteraction}]";
    }

    /// <summary>
    /// Combines this result with another, taking the worst outcome.
    /// </summary>
    /// <param name="other">The other result to combine with.</param>
    /// <returns>A combined <see cref="ProtocolCheckResult"/> with accumulated effects.</returns>
    /// <remarks>
    /// This is useful when multiple protocol checks apply to a single interaction.
    /// The result takes the more severe violation type and accumulates all modifiers.
    /// </remarks>
    public ProtocolCheckResult CombineWith(ProtocolCheckResult other)
    {
        // Take the more severe violation
        var worstViolation = ViolationType.IsMoreSevereThan(other.ViolationType)
            ? ViolationType
            : other.ViolationType;

        // Combine modifiers
        var combinedModifiers = AppliedModifiers.Concat(other.AppliedModifiers).ToList();

        // Combine failed requirements
        var combinedFailures = FailedRequirements.Concat(other.FailedRequirements).Distinct().ToList();

        // Use the primary culture/protocol, but note both in description
        var combinedDescription = HasProtocol && other.HasProtocol
            ? $"{ConsequenceDescription} Additionally: {other.ConsequenceDescription}"
            : ConsequenceDescription + other.ConsequenceDescription;

        return new ProtocolCheckResult(
            IsCompliant: IsCompliant && other.IsCompliant,
            ViolationType: worstViolation,
            CultureId: CultureId,
            ProtocolName: ProtocolName,
            AppliedModifiers: combinedModifiers,
            ConsequenceDescription: combinedDescription,
            DispositionChange: DispositionChange + other.DispositionChange,
            DcAdjustment: Math.Max(DcAdjustment, other.DcAdjustment), // Take worst DC
            FailedRequirements: combinedFailures);
    }

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}
