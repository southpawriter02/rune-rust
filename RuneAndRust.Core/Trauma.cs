namespace RuneAndRust.Core;

/// <summary>
/// Represents a permanent psychological Trauma acquired at a Breaking Point
/// v0.15: Trauma Economy
/// </summary>
public class Trauma
{
    // Identity
    public string TraumaId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Acquisition
    public TraumaCategory Category { get; set; }
    public string AcquisitionSource { get; set; } = string.Empty;  // What caused it
    public DateTime AcquiredAt { get; set; }

    // Progression
    public TraumaSeverity Severity { get; set; }
    public int ProgressionLevel { get; set; } = 1;  // 1-3, worsens over time

    // Mechanical effects
    public List<TraumaEffect> Effects { get; set; } = new();

    // Narrative
    public string? FlavorText { get; set; }  // Occasional intrusive thoughts

    // Management
    public bool IsManagedThisSession { get; set; }  // Has player addressed it today?
    public int DaysSinceManagement { get; set; }

    /// <summary>
    /// Checks if this trauma blocks rest in the given conditions
    /// </summary>
    public bool BlocksRest(Room? room)
    {
        // Check rest restriction effects
        foreach (var effect in Effects)
        {
            if (effect is RestRestrictionEffect restEffect)
            {
                // Check specific conditions
                if (restEffect.RestrictionType == "no_rest_multiple_exits" && room != null)
                {
                    // Count exits
                    int exitCount = 0;
                    if (room.NorthExit != null) exitCount++;
                    if (room.SouthExit != null) exitCount++;
                    if (room.EastExit != null) exitCount++;
                    if (room.WestExit != null) exitCount++;

                    if (exitCount > 1)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the rest block reason for display
    /// </summary>
    public string GetRestBlockReason()
    {
        foreach (var effect in Effects)
        {
            if (effect is RestRestrictionEffect restEffect && !string.IsNullOrEmpty(restEffect.BlockReason))
            {
                return restEffect.BlockReason;
            }
        }

        return $"{Name} prevents rest here";
    }

    /// <summary>
    /// Calculates the rest effectiveness multiplier
    /// </summary>
    public float GetRestEffectivenessMultiplier()
    {
        float multiplier = 1.0f;

        foreach (var effect in Effects)
        {
            if (effect is RestRestrictionEffect restEffect && restEffect.EffectivenessMultiplier.HasValue)
            {
                multiplier *= restEffect.EffectivenessMultiplier.Value;
            }
        }

        return multiplier;
    }

    /// <summary>
    /// Gets stress multiplier for the given condition
    /// </summary>
    public float GetStressMultiplier(string condition)
    {
        float multiplier = 1.0f;

        foreach (var effect in Effects)
        {
            if (effect is StressMultiplierEffect stressEffect)
            {
                // If no trigger condition, always applies
                if (string.IsNullOrEmpty(stressEffect.TriggerCondition))
                {
                    multiplier *= stressEffect.Multiplier;
                }
                // If trigger matches current condition, apply
                else if (stressEffect.TriggerCondition == condition)
                {
                    multiplier *= stressEffect.Multiplier;
                }
            }
        }

        return multiplier;
    }

    /// <summary>
    /// Gets passive stress per turn for the given condition
    /// </summary>
    public int GetPassiveStress(string condition)
    {
        int stress = 0;

        foreach (var effect in Effects)
        {
            if (effect is PassiveStressEffect passiveEffect)
            {
                // If no condition, always applies
                if (string.IsNullOrEmpty(passiveEffect.Condition))
                {
                    stress += passiveEffect.StressPerTurn;
                }
                // If condition matches, apply
                else if (passiveEffect.Condition == condition)
                {
                    stress += passiveEffect.StressPerTurn;
                }
            }
        }

        return stress;
    }

    /// <summary>
    /// Gets attribute penalty for the given attribute and condition
    /// </summary>
    public int GetAttributePenalty(string attributeName, string? condition = null)
    {
        int penalty = 0;

        foreach (var effect in Effects)
        {
            if (effect is AttributePenaltyEffect attrEffect &&
                attrEffect.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            {
                // If no condition, always applies
                if (string.IsNullOrEmpty(attrEffect.Condition))
                {
                    penalty += attrEffect.Penalty;
                }
                // If condition matches, apply
                else if (attrEffect.Condition == condition)
                {
                    penalty += attrEffect.Penalty;
                }
            }
        }

        return penalty;
    }
}
