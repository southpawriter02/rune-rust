namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when an entity dies during combat (v0.3.19b).
/// Consumed by AudioEventListener to trigger death sound cues.
/// </summary>
/// <param name="DeceasedId">The unique identifier of the deceased entity.</param>
/// <param name="DeceasedName">The display name of the deceased entity.</param>
/// <param name="IsPlayer">Whether the deceased was the player character.</param>
/// <param name="KilledByName">The display name of the entity that dealt the killing blow.</param>
public record EntityDeathEvent(
    Guid DeceasedId,
    string DeceasedName,
    bool IsPlayer,
    string KilledByName);
