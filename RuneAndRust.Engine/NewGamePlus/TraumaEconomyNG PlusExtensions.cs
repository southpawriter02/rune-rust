using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.NewGamePlus;

/// <summary>
/// v0.40.1: New Game+ extensions for TraumaEconomyService
/// Applies NG+ corruption rate multipliers
/// </summary>
public static class TraumaEconomyNGPlusExtensions
{
    private static readonly ILogger _log = Log.ForContext(typeof(TraumaEconomyNGPlusExtensions));

    /// <summary>
    /// Add corruption with NG+ scaling applied
    /// </summary>
    public static (int corruptionGained, List<int> thresholdsCrossed) AddCorruptionWithNGPlus(
        this TraumaEconomyService traumaService,
        PlayerCharacter character,
        int baseAmount,
        int ngPlusTier,
        DifficultyScalingService scalingService,
        string source = "unknown")
    {
        if (ngPlusTier == 0)
        {
            // Normal mode - use standard corruption
            return traumaService.AddCorruption(character, baseAmount, source);
        }

        // Apply NG+ scaling to corruption amount
        var scaledAmount = scalingService.ApplyCorruptionScaling(baseAmount, ngPlusTier);

        _log.Debug(
            "Applying NG+{Tier} corruption scaling: {Base} → {Scaled} (×{Mult})",
            ngPlusTier, baseAmount, scaledAmount,
            scalingService.GetCorruptionRateMultiplier(ngPlusTier));

        // Use existing AddCorruption method with scaled amount
        return traumaService.AddCorruption(character, scaledAmount, source);
    }

    /// <summary>
    /// Get effective corruption rate for display purposes
    /// </summary>
    public static float GetEffectiveCorruptionRate(int ngPlusTier, DifficultyScalingService scalingService)
    {
        return scalingService.GetCorruptionRateMultiplier(ngPlusTier);
    }

    /// <summary>
    /// Get formatted corruption rate description
    /// </summary>
    public static string GetCorruptionRateDescription(int ngPlusTier, DifficultyScalingService scalingService)
    {
        if (ngPlusTier == 0)
        {
            return "Normal corruption rate (100%)";
        }

        var multiplier = scalingService.GetCorruptionRateMultiplier(ngPlusTier);
        return $"NG+{ngPlusTier} corruption rate ({(multiplier * 100):F0}%)";
    }
}
