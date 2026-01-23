namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Encapsulates all factors affecting a social interaction check.
/// </summary>
/// <remarks>
/// <para>
/// The social context extends the skill context pattern from v0.15.1a with
/// additional factors specific to social interactions, including target
/// disposition, faction relationships, and cultural protocols.
/// </para>
/// <para>
/// This is an immutable value object created via the SocialContextBuilder service.
/// All modifiers are aggregated and applied to the final skill check.
/// </para>
/// </remarks>

/// <param name="InteractionType">The type of social interaction.</param>
/// <param name="TargetId">The ID of the NPC being interacted with.</param>
/// <param name="TargetDisposition">The NPC's current disposition toward the player.</param>
/// <param name="TargetFactionId">The faction the NPC belongs to, if any.</param>
/// <param name="CultureId">The cultural protocol to use, if applicable.</param>
/// <param name="BaseDc">The base difficulty class before modifiers.</param>
/// <param name="SocialModifiers">All social-specific modifiers applied to the check.</param>
/// <param name="EquipmentModifiers">Equipment modifiers (evidence, documents, etc.).</param>
/// <param name="SituationalModifiers">Situational modifiers (time pressure, etc.).</param>
/// <param name="AppliedStatuses">Status effect IDs currently affecting the check.</param>
public readonly record struct SocialContext(
    SocialInteractionType InteractionType,
    string TargetId,
    DispositionLevel TargetDisposition,
    string? TargetFactionId,
    string? CultureId,
    int BaseDc,
    IReadOnlyList<SocialModifier> SocialModifiers,
    IReadOnlyList<EquipmentModifier> EquipmentModifiers,
    IReadOnlyList<SituationalModifier> SituationalModifiers,
    IReadOnlyList<string> AppliedStatuses)
{
    /// <summary>
    /// Gets the disposition category for the target.
    /// </summary>
    public NpcDisposition DispositionCategory => TargetDisposition.Category;

    /// <summary>
    /// Gets the dice modifier from disposition.
    /// </summary>
    /// <remarks>
    /// Range: +3d10 (Ally) to -2d10 (Hostile).
    /// </remarks>
    public int DispositionDiceModifier => TargetDisposition.DiceModifier;

    /// <summary>
    /// Gets the total dice modifier from all social-specific modifiers.
    /// </summary>
    public int SocialDiceModifier => SocialModifiers.Sum(m => m.DiceModifier);

    /// <summary>
    /// Gets the total DC modifier from all social-specific modifiers.
    /// </summary>
    public int SocialDcModifier => SocialModifiers.Sum(m => m.DcModifier);

    /// <summary>
    /// Gets the total dice modifier from equipment.
    /// </summary>
    public int EquipmentDiceModifier => EquipmentModifiers.Sum(m => m.DiceModifier);

    /// <summary>
    /// Gets the total DC modifier from equipment.
    /// </summary>
    public int EquipmentDcModifier => EquipmentModifiers.Sum(m => m.DcModifier);

    /// <summary>
    /// Gets the total dice modifier from situational factors.
    /// </summary>
    public int SituationalDiceModifier => SituationalModifiers.Sum(m => m.DiceModifier);

    /// <summary>
    /// Gets the total DC modifier from situational factors.
    /// </summary>
    public int SituationalDcModifier => SituationalModifiers.Sum(m => m.DcModifier);

    /// <summary>
    /// Gets the total dice modifier combining all sources.
    /// </summary>
    /// <remarks>
    /// Includes disposition, social, equipment, and situational modifiers.
    /// </remarks>
    public int TotalDiceModifier =>
        DispositionDiceModifier +
        SocialDiceModifier +
        EquipmentDiceModifier +
        SituationalDiceModifier;

    /// <summary>
    /// Gets the total DC modifier combining all sources.
    /// </summary>
    public int TotalDcModifier =>
        SocialDcModifier +
        EquipmentDcModifier +
        SituationalDcModifier;

    /// <summary>
    /// Gets the effective DC after all modifiers.
    /// </summary>
    /// <remarks>
    /// The effective DC is clamped to a minimum of 1.
    /// </remarks>
    public int EffectiveDc => Math.Max(1, BaseDc + TotalDcModifier);

    /// <summary>
    /// Gets whether this interaction type uses opposed rolls.
    /// </summary>
    /// <remarks>
    /// Deception is opposed by WITS, Interrogation by WILL.
    /// </remarks>
    public bool IsOpposed => InteractionType.IsOpposed();

    /// <summary>
    /// Gets whether this interaction incurs stress cost.
    /// </summary>
    /// <remarks>
    /// Deception incurs Psychic Stress due to the Liar's Burden mechanic.
    /// </remarks>
    public bool HasStressCost => InteractionType.HasStressCost();

    /// <summary>
    /// Gets whether this interaction always costs reputation.
    /// </summary>
    /// <remarks>
    /// Intimidation always carries a reputation cost even on success.
    /// </remarks>
    public bool AlwaysCostsReputation => InteractionType.AlwaysCostsReputation();

    /// <summary>
    /// Gets whether this is a multi-round interaction.
    /// </summary>
    /// <remarks>
    /// Negotiation and Interrogation span multiple rounds.
    /// </remarks>
    public bool IsMultiRound => InteractionType.IsMultiRound();

    /// <summary>
    /// Gets whether there are any modifiers applied.
    /// </summary>
    public bool HasModifiers =>
        SocialModifiers.Count > 0 ||
        EquipmentModifiers.Count > 0 ||
        SituationalModifiers.Count > 0 ||
        DispositionDiceModifier != 0;

    /// <summary>
    /// Gets the total count of all modifiers.
    /// </summary>
    public int ModifierCount =>
        SocialModifiers.Count +
        EquipmentModifiers.Count +
        SituationalModifiers.Count +
        (DispositionDiceModifier != 0 ? 1 : 0);

    /// <summary>
    /// Gets the fumble type for this interaction.
    /// </summary>
    public FumbleType FumbleType => InteractionType.GetFumbleType();

    /// <summary>
    /// Gets potential stress cost on success for this interaction type.
    /// </summary>
    public int PotentialSuccessStressCost => InteractionType.GetSuccessStressCost();

    /// <summary>
    /// Gets potential stress cost on failure for this interaction type.
    /// </summary>
    public int PotentialFailureStressCost => InteractionType.GetFailureStressCost();

    /// <summary>
    /// Gets potential stress cost on fumble for this interaction type.
    /// </summary>
    public int PotentialFumbleStressCost => InteractionType.GetFumbleStressCost();

    /// <summary>
    /// Creates an empty social context for testing.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>A minimal social context.</returns>
    public static SocialContext CreateMinimal(
        string targetId,
        SocialInteractionType interactionType = SocialInteractionType.Persuasion)
    {
        return new SocialContext(
            InteractionType: interactionType,
            TargetId: targetId,
            TargetDisposition: DispositionLevel.CreateNeutral(),
            TargetFactionId: null,
            CultureId: null,
            BaseDc: 12,
            SocialModifiers: Array.Empty<SocialModifier>(),
            EquipmentModifiers: Array.Empty<EquipmentModifier>(),
            SituationalModifiers: Array.Empty<SituationalModifier>(),
            AppliedStatuses: Array.Empty<string>());
    }

    /// <summary>
    /// Builds a detailed modifier breakdown for display.
    /// </summary>
    /// <returns>A multi-line string showing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Social Check: {InteractionType.GetDescription()}",
            $"Target: {TargetId} ({TargetDisposition.Category.GetDescription()})",
            $"Base DC: {BaseDc}"
        };

        if (DispositionDiceModifier != 0)
        {
            var sign = DispositionDiceModifier > 0 ? "+" : "";
            lines.Add($"  Disposition: {sign}{DispositionDiceModifier}d10");
        }

        foreach (var mod in SocialModifiers)
        {
            lines.Add($"  {mod.ToShortDescription()}");
        }

        foreach (var mod in EquipmentModifiers)
        {
            lines.Add($"  {mod.ToShortDescription()}");
        }

        foreach (var mod in SituationalModifiers)
        {
            lines.Add($"  {mod.ToShortDescription()}");
        }

        var totalDice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        var totalDc = TotalDcModifier != 0
            ? (TotalDcModifier > 0 ? $", DC +{TotalDcModifier}" : $", DC {TotalDcModifier}")
            : "";

        lines.Add($"Total: {totalDice}{totalDc}");
        lines.Add($"Effective DC: {EffectiveDc}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!HasModifiers)
            return $"{InteractionType} vs {TargetId} (DC {BaseDc})";

        var dice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        var dc = TotalDcModifier != 0
            ? (TotalDcModifier > 0 ? $", DC +{TotalDcModifier}" : $", DC {TotalDcModifier}")
            : "";

        return $"{InteractionType} vs {TargetId}: {dice}{dc} ({ModifierCount} modifiers)";
    }
}
