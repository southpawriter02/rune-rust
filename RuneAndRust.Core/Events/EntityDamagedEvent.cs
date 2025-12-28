using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when an entity takes damage during combat (v0.3.19b).
/// Consumed by AudioEventListener to trigger appropriate sound cues.
/// </summary>
/// <param name="TargetId">The unique identifier of the damaged entity.</param>
/// <param name="TargetName">The display name of the damaged entity.</param>
/// <param name="Amount">The amount of damage dealt (after mitigation).</param>
/// <param name="IsCritical">Whether the attack was a critical hit.</param>
/// <param name="DamageType">The type of damage (Physical, Magical, etc.).</param>
/// <param name="TargetHpAfter">The target's HP after damage was applied.</param>
public record EntityDamagedEvent(
    Guid TargetId,
    string TargetName,
    int Amount,
    bool IsCritical,
    DamageType DamageType,
    int TargetHpAfter)
{
    /// <summary>
    /// Returns true if this damage was lethal (reduced target to 0 or below HP).
    /// </summary>
    public bool IsLethal => TargetHpAfter <= 0;
}
