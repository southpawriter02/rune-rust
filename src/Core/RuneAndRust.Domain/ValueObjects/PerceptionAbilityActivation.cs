namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Records when and how a perception ability was activated.
/// Used for display and tracking purposes.
/// </summary>
/// <remarks>
/// This is specifically for specialization perception ability activations,
/// distinct from the combat/survival AbilityActivation type.
/// </remarks>
public sealed record PerceptionAbilityActivation
{
    /// <summary>
    /// The ability that was activated.
    /// </summary>
    public required PerceptionAbility Ability { get; init; }

    /// <summary>
    /// The character who used the ability.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The target that triggered the activation.
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// Timestamp of activation.
    /// </summary>
    public required DateTime ActivatedAt { get; init; }

    /// <summary>
    /// The specific effect that was applied.
    /// </summary>
    public required string EffectApplied { get; init; }

    /// <summary>
    /// Gets the ability ID.
    /// </summary>
    public string AbilityId => Ability.AbilityId;

    /// <summary>
    /// Gets the ability name.
    /// </summary>
    public string AbilityName => Ability.AbilityName;

    /// <summary>
    /// Creates a perception ability activation record.
    /// </summary>
    public static PerceptionAbilityActivation Create(
        PerceptionAbility ability,
        string characterId,
        string targetId,
        string effectApplied) =>
        new()
        {
            Ability = ability,
            CharacterId = characterId,
            TargetId = targetId,
            ActivatedAt = DateTime.UtcNow,
            EffectApplied = effectApplied
        };
}
