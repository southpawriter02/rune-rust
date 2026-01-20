namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines an interaction between an existing effect and a trigger (damage type or new effect).
/// </summary>
/// <remarks>
/// <para>Effect interactions create tactical depth through elemental combinations:</para>
/// <list type="bullet">
/// <item>Wet + Lightning → +50% damage, apply Stunned</item>
/// <item>Wet + Ice → Apply Frozen, remove Wet</item>
/// <item>Burning + Wet → Remove Burning</item>
/// </list>
/// </remarks>
/// <param name="Id">Unique identifier for this interaction.</param>
/// <param name="TriggerEffectId">The existing effect on the target.</param>
/// <param name="WithTrigger">The incoming trigger (damage type or effect ID).</param>
/// <param name="BonusDamagePercent">Additional damage as percentage of base (0-100+).</param>
/// <param name="BonusDamageType">Damage type for bonus damage (if any).</param>
/// <param name="ApplyEffect">Effect ID to apply as result (if any).</param>
/// <param name="RemoveEffect">Effect ID to remove as result (if any).</param>
/// <param name="Message">Display message when interaction triggers.</param>
public readonly record struct EffectInteraction(
    string Id,
    string TriggerEffectId,
    string WithTrigger,
    int BonusDamagePercent,
    string? BonusDamageType,
    string? ApplyEffect,
    string? RemoveEffect,
    string Message)
{
    /// <summary>Whether this interaction deals bonus damage.</summary>
    public bool HasBonusDamage => BonusDamagePercent > 0;

    /// <summary>Whether this interaction applies a new effect.</summary>
    public bool AppliesEffect => !string.IsNullOrEmpty(ApplyEffect);

    /// <summary>Whether this interaction removes an effect.</summary>
    public bool RemovesEffect => !string.IsNullOrEmpty(RemoveEffect);
}
