using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of validating whether an ability can be used.
/// </summary>
/// <param name="IsValid">Whether the ability can be used.</param>
/// <param name="FailureReason">Reason for failure if not valid.</param>
public record AbilityValidationResult(bool IsValid, string? FailureReason = null)
{
    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static AbilityValidationResult Success => new(true);

    /// <summary>
    /// Creates a failed validation result with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for failure.</param>
    /// <returns>A failed validation result.</returns>
    public static AbilityValidationResult Fail(string reason) => new(false, reason);

    /// <summary>Gets a result indicating the ability was not found.</summary>
    public static AbilityValidationResult AbilityNotFound => Fail("Ability not found");

    /// <summary>Gets a result indicating the player hasn't learned this ability.</summary>
    public static AbilityValidationResult AbilityNotLearned => Fail("You haven't learned this ability");

    /// <summary>
    /// Gets a result indicating the ability is locked due to level requirement.
    /// </summary>
    /// <param name="requiredLevel">The level required to unlock the ability.</param>
    /// <returns>A failed validation result with level requirement message.</returns>
    public static AbilityValidationResult AbilityLocked(int requiredLevel) =>
        Fail($"Requires level {requiredLevel}");

    /// <summary>
    /// Gets a result indicating the ability is on cooldown.
    /// </summary>
    /// <param name="turnsRemaining">The number of turns remaining on cooldown.</param>
    /// <returns>A failed validation result with cooldown message.</returns>
    public static AbilityValidationResult OnCooldown(int turnsRemaining) =>
        Fail($"On cooldown ({turnsRemaining} turn{(turnsRemaining != 1 ? "s" : "")} remaining)");

    /// <summary>
    /// Gets a result indicating insufficient resources.
    /// </summary>
    /// <param name="resource">The resource type name.</param>
    /// <param name="have">The amount the player has.</param>
    /// <param name="need">The amount required.</param>
    /// <returns>A failed validation result with resource requirement message.</returns>
    public static AbilityValidationResult InsufficientResource(string resource, int have, int need) =>
        Fail($"Insufficient {resource}: have {have}, need {need}");

    /// <summary>Gets a result indicating an invalid target.</summary>
    public static AbilityValidationResult InvalidTarget => Fail("Invalid target for this ability");

    /// <summary>Gets a result indicating the ability can only be used in combat.</summary>
    public static AbilityValidationResult NotInCombat => Fail("Can only use this ability in combat");
}

/// <summary>
/// Result of using an ability.
/// </summary>
/// <param name="Success">Whether the ability was used successfully.</param>
/// <param name="Message">Description of what happened.</param>
/// <param name="EffectsApplied">List of effects that were applied.</param>
/// <param name="ResourceSpent">Resource change from the ability cost, if any.</param>
public record AbilityResult(
    bool Success,
    string Message,
    IReadOnlyList<AppliedEffect> EffectsApplied,
    ResourceChange? ResourceSpent = null)
{
    /// <summary>
    /// Creates a failed ability result with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for failure.</param>
    /// <returns>A failed ability result.</returns>
    public static AbilityResult Failed(string reason) =>
        new(false, reason, Array.Empty<AppliedEffect>());

    /// <summary>Gets whether any effects were applied.</summary>
    public bool HasEffects => EffectsApplied.Count > 0;

    /// <summary>Gets the total damage dealt by this ability use.</summary>
    public int TotalDamageDealt => EffectsApplied
        .Where(e => e.EffectType == AbilityEffectType.Damage && !e.WasResisted)
        .Sum(e => e.Value);

    /// <summary>Gets the total healing done by this ability use.</summary>
    public int TotalHealingDone => EffectsApplied
        .Where(e => e.EffectType == AbilityEffectType.Heal && !e.WasResisted)
        .Sum(e => e.Value);
}

/// <summary>
/// Represents an effect that was applied during ability execution.
/// </summary>
/// <param name="EffectType">The type of effect applied.</param>
/// <param name="Value">The actual value applied (damage dealt, health restored, etc.).</param>
/// <param name="TargetName">Name of the target affected.</param>
/// <param name="WasResisted">Whether the effect was resisted by the target.</param>
/// <param name="StatusApplied">Status effect applied, if any.</param>
public record AppliedEffect(
    AbilityEffectType EffectType,
    int Value,
    string TargetName,
    bool WasResisted = false,
    string? StatusApplied = null)
{
    /// <summary>
    /// Gets a human-readable description of this applied effect.
    /// </summary>
    /// <returns>A formatted description of the effect.</returns>
    public string GetDescription()
    {
        if (WasResisted)
            return $"{TargetName} resisted the effect";

        return EffectType switch
        {
            AbilityEffectType.Damage => $"Deals {Value} damage to {TargetName}",
            AbilityEffectType.Heal => $"Heals {TargetName} for {Value}",
            AbilityEffectType.Buff => $"Applies buff to {TargetName}",
            AbilityEffectType.Debuff => StatusApplied != null
                ? $"Applies {StatusApplied} to {TargetName}"
                : $"Applies debuff to {TargetName}",
            AbilityEffectType.DamageOverTime => $"Applies {StatusApplied ?? "DoT"} to {TargetName} ({Value}/turn)",
            AbilityEffectType.HealOverTime => $"Applies regeneration to {TargetName} ({Value}/turn)",
            AbilityEffectType.Shield => $"Shields {TargetName} for {Value}",
            AbilityEffectType.Stun => $"Stuns {TargetName}",
            AbilityEffectType.Taunt => $"Taunts {TargetName}",
            AbilityEffectType.ResourceGain => $"Gains {Value} resource",
            AbilityEffectType.CooldownReset => $"Resets cooldown",
            _ => $"Applies {EffectType} to {TargetName}"
        };
    }
}
