// ------------------------------------------------------------------------------
// <copyright file="IntimidationResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the outcome of an intimidation attempt.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Captures the outcome of an intimidation attempt.
/// </summary>
/// <remarks>
/// <para>
/// The intimidation result includes compliance level, mandatory reputation cost,
/// potential combat initiation on fumble, disposition changes, and unlocked
/// dialogue options.
/// </para>
/// <para>
/// Key characteristics:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>Mandatory reputation cost regardless of outcome (Cost of Fear)</description>
///   </item>
///   <item>
///     <description>No stress cost except on fumble (+5 Psychic Stress)</description>
///   </item>
///   <item>
///     <description>Fumble triggers [Challenge Accepted] with combat initiation</description>
///   </item>
///   <item>
///     <description>NPC gains [Furious] buff on fumble (+2d10 damage)</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="Outcome">The skill outcome classification.</param>
/// <param name="TargetCompliance">The level of compliance achieved from the target.</param>
/// <param name="ReputationCost">The mandatory reputation cost (Cost of Fear).</param>
/// <param name="PlayerSuccesses">Number of successes the player rolled.</param>
/// <param name="Dc">The difficulty class that was rolled against.</param>
/// <param name="Margin">The difference between successes and DC.</param>
/// <param name="DispositionChange">Change to NPC disposition toward player.</param>
/// <param name="StressCost">Psychic stress incurred (0 on success, +5 on fumble).</param>
/// <param name="UnlockedOptions">Dialogue options unlocked by successful intimidation.</param>
/// <param name="FumbleConsequence">The [Challenge Accepted] fumble consequence if applicable.</param>
/// <param name="CombatInitiated">Whether combat was initiated (fumble only).</param>
/// <param name="NpcGainsFurious">Whether NPC gains [Furious] buff (fumble only).</param>
/// <param name="NpcInitiativeBonus">NPC initiative bonus from fumble rage.</param>
/// <param name="NpcWillAcceptSurrender">Whether NPC will accept surrender (false on fumble).</param>
/// <param name="NarrativeText">Flavor text describing the outcome.</param>
public readonly record struct IntimidationResult(
    SkillOutcome Outcome,
    TargetCompliance TargetCompliance,
    CostOfFear ReputationCost,
    int PlayerSuccesses,
    int Dc,
    int Margin,
    int DispositionChange,
    int StressCost,
    IReadOnlyList<string> UnlockedOptions,
    FumbleConsequence? FumbleConsequence,
    bool CombatInitiated,
    bool NpcGainsFurious,
    int NpcInitiativeBonus,
    bool NpcWillAcceptSurrender,
    string? NarrativeText)
{
    /// <summary>
    /// Psychic stress cost on fumble.
    /// </summary>
    public const int FumbleStressCost = 5;

    /// <summary>
    /// NPC initiative bonus on fumble (fueled by rage).
    /// </summary>
    public const int FumbleInitiativeBonus = 2;

    /// <summary>
    /// Disposition penalty on fumble.
    /// </summary>
    public const int FumbleDispositionPenalty = -30;

    /// <summary>
    /// Disposition penalty on failure.
    /// </summary>
    public const int FailureDispositionPenalty = -15;

    /// <summary>
    /// Disposition penalty on success (intimidation always reduces disposition).
    /// </summary>
    public const int SuccessDispositionPenalty = -5;

    /// <summary>
    /// Gets whether this was a successful intimidation (any success tier).
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether this was a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether this result had severe consequences.
    /// </summary>
    public bool HasSevereConsequences => CombatInitiated || NpcGainsFurious;

    /// <summary>
    /// Gets whether the target provided any compliance.
    /// </summary>
    public bool TargetComplied => TargetCompliance > TargetCompliance.None;

    /// <summary>
    /// Creates a successful intimidation result.
    /// </summary>
    /// <param name="outcome">The success outcome tier.</param>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="unlockedOptions">Dialogue options unlocked.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A successful IntimidationResult.</returns>
    public static IntimidationResult Success(
        SkillOutcome outcome,
        string factionId,
        int playerSuccesses,
        int dc,
        IReadOnlyList<string>? unlockedOptions = null,
        string? narrativeText = null)
    {
        var compliance = GetComplianceForOutcome(outcome);
        var costOfFear = new CostOfFear(outcome, factionId);

        return new IntimidationResult(
            Outcome: outcome,
            TargetCompliance: compliance,
            ReputationCost: costOfFear,
            PlayerSuccesses: playerSuccesses,
            Dc: dc,
            Margin: playerSuccesses - dc,
            DispositionChange: SuccessDispositionPenalty,
            StressCost: 0, // No stress on success
            UnlockedOptions: unlockedOptions ?? Array.Empty<string>(),
            FumbleConsequence: null,
            CombatInitiated: false,
            NpcGainsFurious: false,
            NpcInitiativeBonus: 0,
            NpcWillAcceptSurrender: true,
            NarrativeText: narrativeText ?? GetDefaultSuccessNarrative(outcome, compliance));
    }

    /// <summary>
    /// Creates a failed intimidation result (not fumble).
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A failed IntimidationResult.</returns>
    public static IntimidationResult Failure(
        string factionId,
        int playerSuccesses,
        int dc,
        string? narrativeText = null)
    {
        var costOfFear = CostOfFear.ForFailure(factionId);

        return new IntimidationResult(
            Outcome: SkillOutcome.Failure,
            TargetCompliance: TargetCompliance.None,
            ReputationCost: costOfFear,
            PlayerSuccesses: playerSuccesses,
            Dc: dc,
            Margin: playerSuccesses - dc,
            DispositionChange: FailureDispositionPenalty,
            StressCost: 0, // No stress on failure (only fumble)
            UnlockedOptions: Array.Empty<string>(),
            FumbleConsequence: null,
            CombatInitiated: false,
            NpcGainsFurious: false,
            NpcInitiativeBonus: 0,
            NpcWillAcceptSurrender: true,
            NarrativeText: narrativeText ?? "The target refuses to be intimidated. Your threat falls flat.");
    }

    /// <summary>
    /// Creates a fumble result with [Challenge Accepted] consequences.
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="fumbleConsequence">The created fumble consequence entity.</param>
    /// <param name="narrativeText">Flavor text for the fumble.</param>
    /// <returns>A fumble IntimidationResult with [Challenge Accepted] consequences.</returns>
    public static IntimidationResult Fumble(
        string factionId,
        int playerSuccesses,
        int dc,
        FumbleConsequence fumbleConsequence,
        string? narrativeText = null)
    {
        var costOfFear = CostOfFear.ForFumble(factionId);

        return new IntimidationResult(
            Outcome: SkillOutcome.CriticalFailure,
            TargetCompliance: TargetCompliance.None,
            ReputationCost: costOfFear,
            PlayerSuccesses: playerSuccesses,
            Dc: dc,
            Margin: playerSuccesses - dc,
            DispositionChange: FumbleDispositionPenalty,
            StressCost: FumbleStressCost, // +5 Psychic Stress on fumble
            UnlockedOptions: Array.Empty<string>(),
            FumbleConsequence: fumbleConsequence,
            CombatInitiated: true,
            NpcGainsFurious: true,
            NpcInitiativeBonus: FumbleInitiativeBonus,
            NpcWillAcceptSurrender: false, // NPC will not accept surrender
            NarrativeText: narrativeText ?? GetDefaultFumbleNarrative());
    }

    /// <summary>
    /// Gets the compliance level for a given outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>The corresponding target compliance level.</returns>
    private static TargetCompliance GetComplianceForOutcome(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess => TargetCompliance.Complete,
            SkillOutcome.ExceptionalSuccess => TargetCompliance.Full,
            SkillOutcome.FullSuccess => TargetCompliance.Reluctant,
            SkillOutcome.MarginalSuccess => TargetCompliance.Minimal,
            _ => TargetCompliance.None
        };
    }

    private static string GetDefaultSuccessNarrative(SkillOutcome outcome, TargetCompliance compliance)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                "The target is thoroughly cowed. They will do whatever you ask and may even offer additional assistance to avoid further confrontation.",
            SkillOutcome.ExceptionalSuccess =>
                "The target visibly trembles and complies fully with your demands. They will not forget this moment.",
            SkillOutcome.FullSuccess =>
                "The target reluctantly yields to your threat, complying but maintaining their dignity.",
            SkillOutcome.MarginalSuccess =>
                "The target provides minimal cooperation, doing only what is absolutely necessary to avoid conflict.",
            _ => "The target complies with your demands."
        };
    }

    private static string GetDefaultFumbleNarrative()
    {
        return "[Challenge Accepted] Your threat backfires catastrophically. The target's fear transforms " +
               "into rage as they decide they've had enough. 'You think you can threaten ME?' Combat begins " +
               "immediately. The target gains [Furious] (+2d10 damage) and will not accept surrender.";
    }

    /// <summary>
    /// Gets a summary of the result for display.
    /// </summary>
    /// <returns>A multi-line string summarizing the intimidation outcome.</returns>
    public string ToSummary()
    {
        var lines = new List<string>
        {
            $"Intimidation: {Outcome}"
        };

        if (TargetComplied)
        {
            lines.Add($"Compliance: {TargetCompliance}");
        }
        else
        {
            lines.Add("Target REFUSED");
        }

        lines.Add($"Roll: {PlayerSuccesses} vs DC {Dc} (Margin: {Margin:+#;-#;0})");
        lines.Add(ReputationCost.GetNarrativeDescription());

        if (DispositionChange != 0)
        {
            var sign = DispositionChange > 0 ? "+" : "";
            lines.Add($"Disposition: {sign}{DispositionChange}");
        }

        if (StressCost > 0)
        {
            lines.Add($"Psychic Stress: +{StressCost}");
        }

        if (CombatInitiated)
        {
            lines.Add("COMBAT INITIATED!");
            lines.Add($"[Challenge Accepted] triggered");
        }

        if (NpcGainsFurious)
        {
            lines.Add("NPC gains [Furious] (+2d10 damage)");
        }

        if (!NpcWillAcceptSurrender && CombatInitiated)
        {
            lines.Add("NPC will NOT accept surrender");
        }

        if (NpcInitiativeBonus > 0)
        {
            lines.Add($"NPC Initiative: +{NpcInitiativeBonus}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
