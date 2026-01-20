namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a status effect is applied to a combatant.
/// </summary>
/// <param name="TargetId">The target entity ID.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="SourceId">Optional source entity ID.</param>
public record EffectAppliedEvent(Guid TargetId, string EffectId, Guid? SourceId);

/// <summary>
/// Published when a status effect is removed from a combatant.
/// </summary>
/// <param name="TargetId">The target entity ID.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="Reason">The reason for removal.</param>
public record EffectRemovedEvent(Guid TargetId, string EffectId, EffectRemovalReason Reason);

/// <summary>
/// Published when a status effect's stack count changes.
/// </summary>
/// <param name="TargetId">The target entity ID.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="NewStackCount">The new stack count.</param>
public record EffectStackedEvent(Guid TargetId, string EffectId, int NewStackCount);

/// <summary>
/// Published when a status effect's duration is refreshed.
/// </summary>
/// <param name="TargetId">The target entity ID.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="NewDuration">The new duration in turns.</param>
public record EffectRefreshedEvent(Guid TargetId, string EffectId, int NewDuration);

/// <summary>
/// Published when a status effect ticks (DoT/HoT).
/// </summary>
/// <param name="TargetId">The target entity ID.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="Amount">The damage or healing amount.</param>
/// <param name="IsHealing">True if healing, false if damage.</param>
public record EffectTickedEvent(Guid TargetId, string EffectId, int Amount, bool IsHealing);

/// <summary>
/// Reason why an effect was removed.
/// </summary>
public enum EffectRemovalReason
{
    /// <summary>Duration expired.</summary>
    Expired,

    /// <summary>Manually removed.</summary>
    Manual,

    /// <summary>Cleansed by an ability.</summary>
    Cleansed,

    /// <summary>Dispelled by an ability.</summary>
    Dispelled,

    /// <summary>Target died.</summary>
    Death,

    /// <summary>Replaced by new application.</summary>
    Replaced
}
