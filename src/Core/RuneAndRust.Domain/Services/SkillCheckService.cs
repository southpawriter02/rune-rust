namespace RuneAndRust.Domain.Services;

/// <summary>
/// Result of a skill check (d20 + attribute vs DC).
/// </summary>
public record SkillCheckResult(
    int RollResult,
    int AttributeValue,
    int TotalResult,
    int TargetDC,
    bool IsSuccess,
    int SuccessMargin
);

/// <summary>
/// Service for performing d20-based skill checks.
/// </summary>
public class SkillCheckService
{
    private readonly Random _random;

    public SkillCheckService(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Performs a d20 + attribute roll against a DC.
    /// </summary>
    /// <param name="attributeValue">The attribute modifier to add to the roll.</param>
    /// <param name="targetDC">The difficulty class to beat.</param>
    /// <returns>Result containing roll details and success status.</returns>
    public SkillCheckResult PerformCheck(int attributeValue, int targetDC)
    {
        var roll = _random.Next(1, 21); // 1-20
        var total = roll + attributeValue;
        var success = total >= targetDC;
        var margin = total - targetDC;

        return new SkillCheckResult(
            RollResult: roll,
            AttributeValue: attributeValue,
            TotalResult: total,
            TargetDC: targetDC,
            IsSuccess: success,
            SuccessMargin: margin
        );
    }

    /// <summary>
    /// Performs a WITS check against DC 12.
    /// </summary>
    public SkillCheckResult PerformDetailedCheck(int witsValue) =>
        PerformCheck(witsValue, 12);

    /// <summary>
    /// Performs a WITS check against DC 18.
    /// </summary>
    public SkillCheckResult PerformExpertCheck(int witsValue) =>
        PerformCheck(witsValue, 18);

    /// <summary>
    /// Determines the highest examination layer unlocked based on a single WITS roll.
    /// Layer 1: Always available (no check)
    /// Layer 2: DC 12
    /// Layer 3: DC 18
    /// </summary>
    /// <param name="witsValue">The player's WITS attribute value.</param>
    /// <returns>Tuple containing highest layer (1-3) and the check result.</returns>
    public (int HighestLayer, SkillCheckResult CheckResult) DetermineExaminationLayer(int witsValue)
    {
        // Single roll determines which layer is unlocked
        var checkResult = PerformCheck(witsValue, 0); // Roll with no DC for raw result

        // Check against layer thresholds
        if (checkResult.TotalResult >= 18)
            return (3, checkResult with { TargetDC = 18, IsSuccess = true, SuccessMargin = checkResult.TotalResult - 18 });

        if (checkResult.TotalResult >= 12)
            return (2, checkResult with { TargetDC = 12, IsSuccess = true, SuccessMargin = checkResult.TotalResult - 12 });

        return (1, checkResult with { TargetDC = 12, IsSuccess = false, SuccessMargin = checkResult.TotalResult - 12 });
    }
}
