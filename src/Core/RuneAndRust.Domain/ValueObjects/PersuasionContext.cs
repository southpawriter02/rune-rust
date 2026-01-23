// ------------------------------------------------------------------------------
// <copyright file="PersuasionContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Encapsulates all factors affecting a persuasion attempt.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Encapsulates all factors affecting a persuasion attempt.
/// </summary>
/// <remarks>
/// <para>
/// The persuasion context contains request complexity, argument alignment,
/// evidence support, NPC status, faction standing, and attempt history.
/// </para>
/// <para>
/// This is an immutable value object created via the PersuasionService.
/// </para>
/// </remarks>
/// <param name="RequestType">The complexity tier of the request.</param>
/// <param name="TargetId">The NPC being persuaded.</param>
/// <param name="TargetDisposition">The NPC's current disposition.</param>
/// <param name="TargetFactionId">The faction the NPC belongs to.</param>
/// <param name="ArgumentAlignment">How well the argument aligns with NPC values.</param>
/// <param name="EvidenceProvided">Whether supporting evidence was presented.</param>
/// <param name="EvidenceDescription">Description of the evidence, if any.</param>
/// <param name="NpcStressed">Whether the NPC has [Stressed] status.</param>
/// <param name="NpcFeared">Whether the NPC has [Feared] status.</param>
/// <param name="NpcGrateful">Whether the NPC has [Grateful] status.</param>
/// <param name="PreviousAttempts">Number of previous failed attempts this conversation.</param>
/// <param name="SameArgumentUsed">Whether this exact argument was used before.</param>
/// <param name="PlayerFactionStanding">Player's reputation with NPC's faction.</param>
public readonly record struct PersuasionContext(
    PersuasionRequest RequestType,
    string TargetId,
    DispositionLevel TargetDisposition,
    string? TargetFactionId,
    ArgumentAlignment ArgumentAlignment,
    bool EvidenceProvided,
    string? EvidenceDescription,
    bool NpcStressed,
    bool NpcFeared,
    bool NpcGrateful,
    int PreviousAttempts,
    bool SameArgumentUsed,
    int PlayerFactionStanding)
{
    /// <summary>
    /// Gets the base DC from request complexity.
    /// </summary>
    public int BaseDc => RequestType.GetBaseDc();

    /// <summary>
    /// Gets the disposition dice modifier.
    /// </summary>
    public int DispositionModifier => TargetDisposition.DiceModifier;

    /// <summary>
    /// Gets the argument alignment dice modifier.
    /// </summary>
    public int ArgumentModifier => ArgumentAlignment.DiceModifier;

    /// <summary>
    /// Gets the evidence dice modifier (+2d10 if evidence provided).
    /// </summary>
    public int EvidenceModifier => EvidenceProvided ? 2 : 0;

    /// <summary>
    /// Gets the faction standing dice modifier.
    /// </summary>
    /// <remarks>
    /// +1d10 for Honored (≥75), 0 for Neutral (-24 to +74), -1d10 for Unfriendly (-74 to -25), -2d10 for Hostile (≤-75).
    /// </remarks>
    public int FactionModifier => PlayerFactionStanding switch
    {
        >= 75 => 1,   // Honored
        >= -24 => 0,  // Neutral range
        >= -74 => -1, // Unfriendly
        _ => -2       // Hostile
    };

    /// <summary>
    /// Gets the NPC status modifier (positive = player bonus, negative = harder to persuade).
    /// </summary>
    /// <remarks>
    /// Stressed and Feared NPCs are harder to persuade (-1d10 each).
    /// Grateful NPCs are easier to persuade (+1d10).
    /// </remarks>
    public int NpcStatusModifier
    {
        get
        {
            var modifier = 0;
            if (NpcStressed) modifier -= 1; // Harder to persuade stressed NPC
            if (NpcFeared) modifier -= 1;   // Harder to persuade fearful NPC
            if (NpcGrateful) modifier += 1; // Easier to persuade grateful NPC
            return modifier;
        }
    }

    /// <summary>
    /// Gets the previous attempt DC modifier.
    /// </summary>
    /// <remarks>
    /// Second attempt: +2 DC, third+ attempt: +4 DC.
    /// Same argument reuse: additional +2 DC.
    /// </remarks>
    public int PreviousAttemptDcModifier
    {
        get
        {
            var modifier = PreviousAttempts switch
            {
                0 => 0,
                1 => 2,
                _ => 4
            };
            if (SameArgumentUsed) modifier += 2;
            return modifier;
        }
    }

    /// <summary>
    /// Gets the total dice pool modifier from all sources.
    /// </summary>
    public int TotalDiceModifier =>
        DispositionModifier +
        ArgumentModifier +
        EvidenceModifier +
        FactionModifier +
        NpcStatusModifier;

    /// <summary>
    /// Gets the total DC modifier from previous attempts.
    /// </summary>
    public int TotalDcModifier => PreviousAttemptDcModifier;

    /// <summary>
    /// Gets the effective DC after all modifiers.
    /// </summary>
    public int EffectiveDc => Math.Max(1, BaseDc + TotalDcModifier);

    /// <summary>
    /// Gets whether persuasion is likely to fail due to disposition.
    /// </summary>
    public bool IsUnlikelyDueToDisposition =>
        TargetDisposition.Category < RequestType.GetMinimumDisposition();

    /// <summary>
    /// Gets whether this request may be refused outright.
    /// </summary>
    public bool MayBeImpossible => RequestType.MayBeImpossible();

    /// <summary>
    /// Builds a detailed modifier breakdown for display.
    /// </summary>
    /// <returns>A multi-line string showing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Persuasion: {RequestType.GetDescription()}",
            $"Target: {TargetId} ({TargetDisposition.Category.GetDescription()})"
        };

        if (DispositionModifier != 0)
        {
            var sign = DispositionModifier > 0 ? "+" : "";
            lines.Add($"  Disposition: {sign}{DispositionModifier}d10");
        }

        if (ArgumentModifier != 0)
        {
            var sign = ArgumentModifier > 0 ? "+" : "";
            var desc = ArgumentAlignment.IsAligned ? "aligns with values" : "contradicts values";
            lines.Add($"  Argument ({desc}): {sign}{ArgumentModifier}d10");
        }

        if (EvidenceProvided)
        {
            lines.Add($"  Evidence ({EvidenceDescription ?? "provided"}): +2d10");
        }

        if (FactionModifier != 0)
        {
            var sign = FactionModifier > 0 ? "+" : "";
            lines.Add($"  Faction standing: {sign}{FactionModifier}d10");
        }

        if (NpcStatusModifier != 0)
        {
            var sign = NpcStatusModifier > 0 ? "+" : "";
            var statuses = new List<string>();
            if (NpcStressed) statuses.Add("[Stressed]");
            if (NpcFeared) statuses.Add("[Feared]");
            if (NpcGrateful) statuses.Add("[Grateful]");
            lines.Add($"  NPC status ({string.Join(", ", statuses)}): {sign}{NpcStatusModifier}d10");
        }

        if (PreviousAttemptDcModifier != 0)
        {
            lines.Add($"  Previous attempts: DC +{PreviousAttemptDcModifier}");
        }

        var totalDice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        lines.Add($"Total dice: {totalDice}");
        lines.Add($"Effective DC: {EffectiveDc}");

        if (IsUnlikelyDueToDisposition)
        {
            lines.Add("WARNING: NPC disposition too low for this request type");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var dice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        return $"Persuade {TargetId}: {RequestType} (DC {EffectiveDc}, {dice})";
    }
}
