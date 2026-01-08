namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of an effect interaction being triggered.
/// </summary>
/// <param name="InteractionId">The interaction that triggered.</param>
/// <param name="Message">Display message for the interaction.</param>
/// <param name="BonusDamage">Additional damage dealt.</param>
/// <param name="AppliedEffects">Effect IDs that were applied.</param>
/// <param name="RemovedEffects">Effect IDs that were removed.</param>
public readonly record struct EffectInteractionResult(
    string InteractionId,
    string Message,
    int BonusDamage,
    IReadOnlyList<string> AppliedEffects,
    IReadOnlyList<string> RemovedEffects)
{
    /// <summary>Creates a result with only bonus damage.</summary>
    public static EffectInteractionResult WithBonusDamage(
        string interactionId,
        string message,
        int damage) =>
        new(interactionId, message, damage,
            Array.Empty<string>(), Array.Empty<string>());

    /// <summary>Creates a result with effect changes.</summary>
    public static EffectInteractionResult WithEffectChanges(
        string interactionId,
        string message,
        IReadOnlyList<string> applied,
        IReadOnlyList<string> removed) =>
        new(interactionId, message, 0, applied, removed);

    /// <summary>Creates a full result.</summary>
    public static EffectInteractionResult Full(
        string interactionId,
        string message,
        int bonusDamage,
        IReadOnlyList<string> applied,
        IReadOnlyList<string> removed) =>
        new(interactionId, message, bonusDamage, applied, removed);
}
