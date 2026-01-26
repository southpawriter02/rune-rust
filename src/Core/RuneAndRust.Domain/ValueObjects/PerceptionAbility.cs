namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a specialization-specific perception ability that modifies
/// examination, passive perception, or investigation mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Perception abilities are granted by character specializations and provide
/// bonuses or automatic successes in specific domains.
/// </para>
/// </remarks>
public sealed record PerceptionAbility
{
    /// <summary>
    /// Unique identifier for this ability.
    /// </summary>
    public required string AbilityId { get; init; }

    /// <summary>
    /// Display name for the ability (e.g., "[Deep Scan]").
    /// </summary>
    public required string AbilityName { get; init; }

    /// <summary>
    /// The specialization that grants this ability.
    /// </summary>
    public required string SpecializationId { get; init; }

    /// <summary>
    /// The type of perception this ability affects.
    /// </summary>
    public required PerceptionAbilityType AbilityType { get; init; }

    /// <summary>
    /// Description of what this ability does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Conditions required for this ability to activate.
    /// </summary>
    public required AbilityCondition Condition { get; init; }

    /// <summary>
    /// The effect applied when the ability activates.
    /// </summary>
    public required PerceptionAbilityEffect Effect { get; init; }

    /// <summary>
    /// Flavor text displayed when the ability activates.
    /// </summary>
    public string? ActivationText { get; init; }

    /// <summary>
    /// Whether this ability is passive (always active when conditions met)
    /// or requires explicit activation.
    /// </summary>
    public bool IsPassive { get; init; } = true;

    /// <summary>
    /// Creates a perception ability for testing.
    /// </summary>
    public static PerceptionAbility Create(
        string abilityId,
        string abilityName,
        string specializationId,
        PerceptionAbilityType abilityType,
        string description,
        AbilityCondition condition,
        PerceptionAbilityEffect effect) =>
        new()
        {
            AbilityId = abilityId,
            AbilityName = abilityName,
            SpecializationId = specializationId,
            AbilityType = abilityType,
            Description = description,
            Condition = condition,
            Effect = effect
        };
}
