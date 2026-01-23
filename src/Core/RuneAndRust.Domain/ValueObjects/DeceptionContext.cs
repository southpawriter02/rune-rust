// ------------------------------------------------------------------------------
// <copyright file="DeceptionContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Encapsulates all factors affecting a deception attempt.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Encapsulates all factors affecting a deception attempt.
/// </summary>
/// <remarks>
/// <para>
/// The deception context captures the complexity of the lie, the NPC's
/// state of alertness/trust, any contradicting evidence, and the quality
/// of the player's cover story. Unlike persuasion, deception uses an
/// opposed roll system where the NPC actively contests the player's check.
/// </para>
/// <para>
/// This is an immutable value object created via the DeceptionService.
/// </para>
/// </remarks>
/// <param name="LieComplexity">The plausibility tier of the lie.</param>
/// <param name="TargetId">The NPC being deceived.</param>
/// <param name="TargetWits">The NPC's WITS attribute for opposed roll.</param>
/// <param name="TargetDisposition">The NPC's current disposition toward player.</param>
/// <param name="NpcSuspicious">Whether the NPC has [Suspicious] status (+4 DC).</param>
/// <param name="NpcTrusting">Whether the NPC has [Trusting] status (-4 DC).</param>
/// <param name="NpcTrainedObserver">Whether the NPC is a guard, investigator, etc. (+1d10 NPC).</param>
/// <param name="NpcPreviouslyFooled">Whether this NPC was deceived before by player (+1d10 NPC).</param>
/// <param name="NpcHasAlert">Whether the NPC has [Alert] status (+1d10 NPC).</param>
/// <param name="NpcHasFatigued">Whether the NPC has [Fatigued] status (-1d10 NPC).</param>
/// <param name="NpcKnowsPlayer">Whether the NPC knows the player personally (+1d10 NPC).</param>
/// <param name="EvidenceContradicts">Whether physical evidence contradicts the lie (+6 DC).</param>
/// <param name="ContradictingEvidenceDescription">Description of contradicting evidence.</param>
/// <param name="CoverStoryQuality">Quality of prepared cover story (-0 to -4 DC).</param>
/// <param name="HasForgedDocuments">Whether player has forged documents (+1-2d10).</param>
/// <param name="ForgedDocumentQuality">Quality of forged documents (0=none, 1=basic +1d10, 2=high +2d10).</param>
/// <param name="LieContainsTruth">Whether the lie incorporates some genuine truth (+1d10).</param>
/// <param name="NpcIsDistracted">Whether NPC is distracted by other events (+1d10).</param>
/// <param name="NpcIsIntoxicated">Whether NPC is intoxicated (+1d10).</param>
/// <param name="PlayerHasUntrustworthy">Whether player has [Untrustworthy] flag with this NPC (+3 DC).</param>
/// <param name="PlayerFactionStanding">Player's reputation with NPC's faction.</param>
public readonly record struct DeceptionContext(
    LieComplexity LieComplexity,
    string TargetId,
    int TargetWits,
    DispositionLevel TargetDisposition,
    bool NpcSuspicious,
    bool NpcTrusting,
    bool NpcTrainedObserver,
    bool NpcPreviouslyFooled,
    bool NpcHasAlert,
    bool NpcHasFatigued,
    bool NpcKnowsPlayer,
    bool EvidenceContradicts,
    string? ContradictingEvidenceDescription,
    CoverStoryQuality CoverStoryQuality,
    bool HasForgedDocuments,
    int ForgedDocumentQuality,
    bool LieContainsTruth,
    bool NpcIsDistracted,
    bool NpcIsIntoxicated,
    bool PlayerHasUntrustworthy,
    int PlayerFactionStanding)
{
    /// <summary>
    /// Gets the base DC from lie complexity.
    /// </summary>
    public int BaseDc => LieComplexity.GetBaseDc();

    /// <summary>
    /// Gets the NPC suspicion DC modifier (+4 if suspicious).
    /// </summary>
    public int SuspicionDcModifier => NpcSuspicious ? 4 : 0;

    /// <summary>
    /// Gets the NPC trust DC modifier (-4 if trusting).
    /// </summary>
    public int TrustDcModifier => NpcTrusting ? -4 : 0;

    /// <summary>
    /// Gets the contradicting evidence DC modifier (+6 if evidence contradicts).
    /// </summary>
    public int ContradictingEvidenceDcModifier => EvidenceContradicts ? 6 : 0;

    /// <summary>
    /// Gets the [Untrustworthy] flag DC modifier (+3 if player has flag).
    /// </summary>
    public int UntrustworthyDcModifier => PlayerHasUntrustworthy ? 3 : 0;

    /// <summary>
    /// Gets the cover story DC modifier.
    /// </summary>
    public int CoverStoryDcModifier => CoverStoryQuality.GetDcModifier();

    /// <summary>
    /// Gets the total DC modifier from all sources.
    /// </summary>
    public int TotalDcModifier =>
        SuspicionDcModifier +
        TrustDcModifier +
        ContradictingEvidenceDcModifier +
        UntrustworthyDcModifier +
        CoverStoryDcModifier;

    /// <summary>
    /// Gets the effective DC after all modifiers.
    /// </summary>
    public int EffectiveDc => Math.Max(1, BaseDc + TotalDcModifier);

    /// <summary>
    /// Gets the player's dice pool modifier from documents and circumstance.
    /// </summary>
    public int PlayerDiceModifier
    {
        get
        {
            var modifier = 0;

            // Forged documents
            if (HasForgedDocuments)
            {
                modifier += ForgedDocumentQuality >= 2 ? 2 : 1;
            }

            // Lie contains truth
            if (LieContainsTruth) modifier += 1;

            // NPC state bonuses
            if (NpcIsDistracted) modifier += 1;
            if (NpcIsIntoxicated) modifier += 1;

            // Faction reputation bonus
            if (PlayerFactionStanding >= 75) modifier += 1; // Honored

            return modifier;
        }
    }

    /// <summary>
    /// Gets the NPC's dice pool modifier for the opposed roll.
    /// </summary>
    public int NpcDiceModifier
    {
        get
        {
            var modifier = 0;

            if (NpcTrainedObserver) modifier += 1;
            if (NpcPreviouslyFooled) modifier += 1;
            if (NpcHasAlert) modifier += 1;
            if (NpcKnowsPlayer) modifier += 1;
            if (NpcHasFatigued) modifier -= 1;

            return modifier;
        }
    }

    /// <summary>
    /// Gets the NPC's total dice pool for the opposed roll.
    /// </summary>
    public int NpcDicePool => Math.Max(1, TargetWits + NpcDiceModifier);

    /// <summary>
    /// Gets the detection severity if the lie is exposed.
    /// </summary>
    public int DetectionSeverity => LieComplexity.GetDetectionSeverity();

    /// <summary>
    /// Gets whether this deception attempt is particularly risky.
    /// </summary>
    public bool IsHighRisk =>
        LieComplexity >= LieComplexity.Unlikely ||
        NpcSuspicious ||
        EvidenceContradicts ||
        PlayerHasUntrustworthy;

    /// <summary>
    /// Builds a detailed modifier breakdown for display.
    /// </summary>
    /// <returns>A multi-line string showing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Deception: {LieComplexity.GetDescription()}",
            $"Target: {TargetId} (WITS {TargetWits})"
        };

        // DC modifiers
        lines.Add("DC Modifiers:");
        if (SuspicionDcModifier != 0)
            lines.Add($"  [Suspicious]: +{SuspicionDcModifier} DC");
        if (TrustDcModifier != 0)
            lines.Add($"  [Trusting]: {TrustDcModifier} DC");
        if (ContradictingEvidenceDcModifier != 0)
            lines.Add($"  Evidence Contradicts: +{ContradictingEvidenceDcModifier} DC");
        if (UntrustworthyDcModifier != 0)
            lines.Add($"  [Untrustworthy]: +{UntrustworthyDcModifier} DC");
        if (CoverStoryDcModifier != 0)
            lines.Add($"  Cover Story ({CoverStoryQuality.GetShortName()}): {CoverStoryDcModifier} DC");

        lines.Add($"  Effective DC: {EffectiveDc}");

        // Player dice modifiers
        if (PlayerDiceModifier != 0)
        {
            lines.Add("Player Bonuses:");
            if (HasForgedDocuments)
            {
                var quality = ForgedDocumentQuality >= 2 ? "High-Quality" : "Basic";
                var bonus = ForgedDocumentQuality >= 2 ? 2 : 1;
                lines.Add($"  {quality} Forged Documents: +{bonus}d10");
            }
            if (LieContainsTruth)
                lines.Add("  Lie Contains Truth: +1d10");
            if (NpcIsDistracted)
                lines.Add("  NPC Distracted: +1d10");
            if (NpcIsIntoxicated)
                lines.Add("  NPC Intoxicated: +1d10");
            if (PlayerFactionStanding >= 75)
                lines.Add("  [Honored] Reputation: +1d10");
        }

        // NPC dice modifiers
        if (NpcDiceModifier != 0)
        {
            lines.Add("NPC Perception Modifiers:");
            if (NpcTrainedObserver)
                lines.Add("  Trained Observer: +1d10");
            if (NpcPreviouslyFooled)
                lines.Add("  Previously Fooled: +1d10");
            if (NpcHasAlert)
                lines.Add("  [Alert]: +1d10");
            if (NpcKnowsPlayer)
                lines.Add("  Knows Player: +1d10");
            if (NpcHasFatigued)
                lines.Add("  [Fatigued]: -1d10");
        }

        lines.Add($"NPC Opposed Pool: {NpcDicePool}d10");

        if (IsHighRisk)
        {
            lines.Add("WARNING: High-risk deception attempt");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var dice = PlayerDiceModifier >= 0 ? $"+{PlayerDiceModifier}d10" : $"{PlayerDiceModifier}d10";
        return $"Deceive {TargetId}: {LieComplexity.GetShortName()} (DC {EffectiveDc}, {dice} vs {NpcDicePool}d10)";
    }
}
