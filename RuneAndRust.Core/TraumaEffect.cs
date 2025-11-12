namespace RuneAndRust.Core;

/// <summary>
/// Base class for Trauma effects
/// v0.15: Trauma Economy
/// </summary>
public abstract class TraumaEffect
{
    /// <summary>
    /// Type identifier for serialization
    /// </summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>
    /// Apply the effect to a character
    /// </summary>
    public abstract void Apply(PlayerCharacter character);

    /// <summary>
    /// Remove the effect from a character
    /// </summary>
    public abstract void Remove(PlayerCharacter character);

    /// <summary>
    /// Get a description of this effect for display
    /// </summary>
    public abstract string GetDescription();
}

/// <summary>
/// Reduces an attribute value
/// </summary>
public class AttributePenaltyEffect : TraumaEffect
{
    public string AttributeName { get; set; } = string.Empty;
    public int Penalty { get; set; }
    public string? Condition { get; set; } // Optional: "in_darkness", "when_alone", etc.

    public AttributePenaltyEffect()
    {
        EffectType = "AttributePenalty";
    }

    public override void Apply(PlayerCharacter character)
    {
        // Note: Actual attribute penalties will be calculated dynamically
        // when attributes are read, based on active traumas
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cleanup if needed
    }

    public override string GetDescription()
    {
        var desc = $"-{Penalty} {AttributeName}";
        if (!string.IsNullOrEmpty(Condition))
        {
            desc += $" ({Condition})";
        }
        return desc;
    }
}

/// <summary>
/// Multiplies stress gain in certain situations
/// </summary>
public class StressMultiplierEffect : TraumaEffect
{
    public float Multiplier { get; set; }  // 1.2 = 20% more stress
    public string? TriggerCondition { get; set; }  // "in_darkness", "enemy_nearby", "when_alone"

    public StressMultiplierEffect()
    {
        EffectType = "StressMultiplier";
    }

    public override void Apply(PlayerCharacter character)
    {
        // Applied dynamically when stress is added
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cleanup if needed
    }

    public override string GetDescription()
    {
        var percentIncrease = (int)((Multiplier - 1.0f) * 100);
        var desc = $"+{percentIncrease}% Stress gain";
        if (!string.IsNullOrEmpty(TriggerCondition))
        {
            desc += $" ({TriggerCondition})";
        }
        return desc;
    }
}

/// <summary>
/// Adds flat stress per turn in certain conditions
/// </summary>
public class PassiveStressEffect : TraumaEffect
{
    public int StressPerTurn { get; set; }
    public string? Condition { get; set; }  // "in_large_rooms", "in_small_rooms", "when_alone"

    public PassiveStressEffect()
    {
        EffectType = "PassiveStress";
    }

    public override void Apply(PlayerCharacter character)
    {
        // Applied dynamically each turn based on conditions
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cleanup if needed
    }

    public override string GetDescription()
    {
        var desc = $"+{StressPerTurn} Stress per turn";
        if (!string.IsNullOrEmpty(Condition))
        {
            desc += $" ({Condition})";
        }
        return desc;
    }
}

/// <summary>
/// Prevents or limits resting
/// </summary>
public class RestRestrictionEffect : TraumaEffect
{
    public string RestrictionType { get; set; } = string.Empty; // "no_rest_multiple_exits", "reduced_effectiveness"
    public float? EffectivenessMultiplier { get; set; }  // 0.5 = half effectiveness
    public string? BlockReason { get; set; }  // Reason shown to player

    public RestRestrictionEffect()
    {
        EffectType = "RestRestriction";
    }

    public override void Apply(PlayerCharacter character)
    {
        // Applied when rest is attempted
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cleanup if needed
    }

    public override string GetDescription()
    {
        if (EffectivenessMultiplier.HasValue)
        {
            var percent = (int)(EffectivenessMultiplier.Value * 100);
            return $"Rest effectiveness reduced to {percent}%";
        }
        return BlockReason ?? "Rest restrictions apply";
    }
}

/// <summary>
/// Restricts certain actions or behaviors
/// </summary>
public class BehaviorRestrictionEffect : TraumaEffect
{
    public string RestrictionType { get; set; } = string.Empty; // "cannot_drop_common_items", "must_carry_light", etc.
    public string Description { get; set; } = string.Empty;

    public BehaviorRestrictionEffect()
    {
        EffectType = "BehaviorRestriction";
    }

    public override void Apply(PlayerCharacter character)
    {
        // Applied when actions are attempted
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cleanup if needed
    }

    public override string GetDescription()
    {
        return Description;
    }
}

/// <summary>
/// Adds immediate corruption
/// </summary>
public class ImmediateCorruptionEffect : TraumaEffect
{
    public int CorruptionAmount { get; set; }

    public ImmediateCorruptionEffect()
    {
        EffectType = "ImmediateCorruption";
    }

    public override void Apply(PlayerCharacter character)
    {
        character.Corruption = Math.Min(100, character.Corruption + CorruptionAmount);
    }

    public override void Remove(PlayerCharacter character)
    {
        // Cannot remove corruption once applied
    }

    public override string GetDescription()
    {
        return $"+{CorruptionAmount} Corruption (immediate)";
    }
}
