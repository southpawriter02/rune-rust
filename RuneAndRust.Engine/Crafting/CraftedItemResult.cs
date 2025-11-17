namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Result of a crafting operation using the Advanced Crafting System
/// </summary>
public class CraftedItemResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CraftedItemId { get; set; }
    public string? CraftedItemName { get; set; }
    public int FinalQuality { get; set; } = 1; // 1-4
    public int SkillCheckRoll { get; set; } = 0;
    public int SkillCheckDC { get; set; } = 0;
    public bool SkillCheckPassed { get; set; } = false;

    /// <summary>
    /// Components that were consumed in the crafting attempt
    /// </summary>
    public List<ConsumedComponent> ConsumedComponents { get; set; } = new();

    /// <summary>
    /// Calculation breakdown for transparency
    /// </summary>
    public QualityCalculation? QualityCalculation { get; set; }

    /// <summary>
    /// Create a failure result
    /// </summary>
    public static CraftedItemResult FailureResult(string message)
    {
        return new CraftedItemResult
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Represents a component that was consumed during crafting
/// </summary>
public class ConsumedComponent
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QualityTier { get; set; }
}

/// <summary>
/// Breakdown of quality calculation for transparency
/// </summary>
public class QualityCalculation
{
    public int LowestComponentQuality { get; set; }
    public int StationMaxQuality { get; set; }
    public int RecipeQualityBonus { get; set; }
    public int BaseQuality { get; set; } // min(LowestComponentQuality, StationMaxQuality)
    public int FinalQuality { get; set; } // BaseQuality + RecipeBonus, clamped to 1-4

    public override string ToString()
    {
        return $"Quality Calculation: min({LowestComponentQuality}, {StationMaxQuality}) + {RecipeQualityBonus} = {FinalQuality}";
    }
}
