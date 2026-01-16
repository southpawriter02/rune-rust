namespace RuneAndRust.Application.Events;

/// <summary>Published when DoT effects deal damage during a tick.</summary>
public record DoTDamageDealtEvent(
    Guid TargetId,
    int TotalDamage,
    Dictionary<string, int> DamageByEffect);

/// <summary>Published when HoT effects heal during a tick.</summary>
public record HoTHealingDoneEvent(
    Guid TargetId,
    int TotalHealing,
    Dictionary<string, int> HealingByEffect);

/// <summary>Published when effects are cleansed from a target.</summary>
public record EffectsCleanedEvent(
    Guid TargetId,
    IReadOnlyList<string> CleanedEffectIds);

/// <summary>Published when effects are dispelled from a target.</summary>
public record EffectsDispelledEvent(
    Guid TargetId,
    IReadOnlyList<string> DispelledEffectIds);

/// <summary>Published when a custom trigger fires.</summary>
public record CustomTriggerEvent(
    Guid TargetId,
    string EffectId,
    string TriggerValue);
