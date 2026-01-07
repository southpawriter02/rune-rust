using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for ability definition information display.
/// </summary>
public record AbilityDefinitionDto(
    string Id,
    string Name,
    string Description,
    string CostResourceTypeId,
    int CostAmount,
    int Cooldown,
    AbilityTargetType TargetType,
    int UnlockLevel,
    IReadOnlyList<string> Tags,
    IReadOnlyList<AbilityEffectDto> Effects);

/// <summary>
/// DTO for ability effect information display.
/// </summary>
public record AbilityEffectDto(
    AbilityEffectType EffectType,
    int Value,
    int Duration,
    string? StatusEffect,
    string? StatModifier,
    double Chance,
    string? ScalingStat,
    double ScalingMultiplier,
    string? Description);

/// <summary>
/// DTO for player's ability instance information display.
/// </summary>
public record PlayerAbilityDto(
    string AbilityId,
    string Name,
    string Description,
    string CostResourceTypeId,
    int CostAmount,
    int Cooldown,
    int CurrentCooldown,
    AbilityTargetType TargetType,
    int UnlockLevel,
    bool IsUnlocked,
    bool IsReady,
    bool IsOnCooldown,
    bool CanAfford,
    int TimesUsed,
    IReadOnlyList<string> Tags);
