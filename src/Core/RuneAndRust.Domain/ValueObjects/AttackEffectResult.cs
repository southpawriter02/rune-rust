namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of processing effects triggered by an attack.
/// </summary>
/// <param name="AppliedEffects">Effects that were applied to the target.</param>
/// <param name="Interactions">Effect interactions that triggered.</param>
/// <param name="BonusDamage">Additional damage from interactions.</param>
public readonly record struct AttackEffectResult(
    IReadOnlyList<EffectApplicationResult> AppliedEffects,
    IReadOnlyList<EffectInteractionResult> Interactions,
    int BonusDamage)
{
    /// <summary>Creates an empty result (no effects or interactions).</summary>
    public static AttackEffectResult None() =>
        new(Array.Empty<EffectApplicationResult>(),
            Array.Empty<EffectInteractionResult>(), 0);

    /// <summary>Whether any effects were applied.</summary>
    public bool HadAppliedEffects => AppliedEffects.Count > 0;

    /// <summary>Whether any interactions triggered.</summary>
    public bool HadInteractions => Interactions.Count > 0;

    /// <summary>Gets all effect IDs that were successfully applied.</summary>
    public IEnumerable<string> GetAppliedEffectIds() =>
        AppliedEffects.Where(e => e.Applied).Select(e => e.EffectId);
}
