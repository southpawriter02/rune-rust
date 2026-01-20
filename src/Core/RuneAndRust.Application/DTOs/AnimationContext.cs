namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Context for animation template variable substitution.
/// </summary>
/// <param name="ActorName">Name of the acting entity.</param>
/// <param name="TargetName">Name of the target entity.</param>
/// <param name="Value">Numeric value (damage, healing, etc.).</param>
/// <param name="AbilityName">Name of ability being used.</param>
/// <param name="DamageType">Type of damage dealt.</param>
/// <param name="StatusName">Name of status effect.</param>
public record AnimationContext(
    string ActorName,
    string? TargetName = null,
    int? Value = null,
    string? AbilityName = null,
    string? DamageType = null,
    string? StatusName = null);
