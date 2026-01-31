namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements trauma economy integration for skill checks.
/// </summary>
/// <remarks>
/// <para>
/// Calculates psychic stress from skill checks based on corruption exposure
/// and fumble outcomes. This service bridges the skill system with the
/// trauma economy defined in the Rune &amp; Rust design specifications.
/// </para>
/// <para>
/// Stress accumulation follows these rules:
/// <list type="bullet">
///   <item>Corruption tier determines base stress (0-10)</item>
///   <item>Fumbles add bonus stress (1-8 based on tier)</item>
///   <item>Breaking point threshold is 8 total stress</item>
/// </list>
/// </para>
/// </remarks>
public class TraumaIntegrationService : ITraumaIntegrationService
{
    private readonly ILogger<TraumaIntegrationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaIntegrationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public TraumaIntegrationService(ILogger<TraumaIntegrationService> logger)
    {
        _logger = logger;
        _logger.LogDebug("TraumaIntegrationService initialized");
    }

    /// <inheritdoc/>
    public SkillStressResult CalculateSkillStress(
        SkillContext context,
        OutcomeDetails outcomeDetails)
    {
        // Extract corruption tier from context
        var corruptionTier = ExtractCorruptionTier(context);

        _logger.LogTrace(
            "Calculating skill stress: CorruptionTier={Tier}, IsFumble={IsFumble}",
            corruptionTier,
            outcomeDetails.IsFumble);

        // No stress in normal areas without fumble
        if (corruptionTier == CorruptionTier.Normal && !outcomeDetails.IsFumble)
        {
            _logger.LogTrace("No stress: normal area, no fumble");
            return SkillStressResult.None();
        }

        // Calculate stress based on fumble status
        var result = outcomeDetails.IsFumble
            ? SkillStressResult.FromFumble(corruptionTier)
            : SkillStressResult.FromCorruption(corruptionTier);

        if (result.HasStress)
        {
            _logger.LogDebug(
                "Skill stress calculated: TotalStress={TotalStress} (Corruption={Corruption}, Fumble={Fumble})",
                result.TotalStress,
                result.CorruptionStress,
                result.FumbleStress);
        }

        if (result.TriggersBreakingPoint)
        {
            _logger.LogInformation(
                "Breaking point threshold reached: {TotalStress} stress from {Source}",
                result.TotalStress,
                result.Source);
        }

        return result;
    }

    /// <inheritdoc/>
    public SkillStressResult CalculateObjectInteractionStress(
        CorruptionTier objectCorruptionTier,
        bool isFumble)
    {
        _logger.LogTrace(
            "Calculating object interaction stress: Tier={Tier}, IsFumble={IsFumble}",
            objectCorruptionTier,
            isFumble);

        var corruptionStress = GetCorruptionStressCost(objectCorruptionTier);
        var fumbleStress = isFumble ? GetFumbleStressCost(objectCorruptionTier) : 0;
        var totalStress = corruptionStress + fumbleStress;

        var source = isFumble
            ? StressSource.Combat
            : objectCorruptionTier != CorruptionTier.Normal
                ? StressSource.Exploration
                : StressSource.Exploration;

        var result = new SkillStressResult(
            TotalStress: totalStress,
            CorruptionStress: corruptionStress,
            FumbleStress: fumbleStress,
            Source: source,
            TriggersBreakingPoint: totalStress >= SkillStressResult.BreakingPointThreshold,
            CorruptionTier: objectCorruptionTier);

        _logger.LogDebug(
            "Object interaction stress: TotalStress={TotalStress} (Tier={Tier}, Fumble={IsFumble})",
            result.TotalStress,
            objectCorruptionTier,
            isFumble);

        return result;
    }

    /// <inheritdoc/>
    public SkillStressResult CalculateExtendedCheckStress(
        SkillContext context,
        int stepCount,
        int fumbleCount)
    {
        var corruptionTier = ExtractCorruptionTier(context);

        _logger.LogTrace(
            "Calculating extended check stress: Tier={Tier}, Steps={Steps}, Fumbles={Fumbles}",
            corruptionTier,
            stepCount,
            fumbleCount);

        // Each step in a corrupted area incurs stress
        var perStepStress = GetCorruptionStressCost(corruptionTier);
        var accumulatedCorruptionStress = perStepStress * stepCount;

        // Fumbles add bonus stress
        var perFumbleStress = GetFumbleStressCost(corruptionTier);
        var accumulatedFumbleStress = perFumbleStress * fumbleCount;

        var totalStress = accumulatedCorruptionStress + accumulatedFumbleStress;

        var source = fumbleCount > 0
            ? StressSource.Combat
            : corruptionTier != CorruptionTier.Normal
                ? StressSource.Environmental
                : StressSource.Exploration;

        var result = new SkillStressResult(
            TotalStress: totalStress,
            CorruptionStress: accumulatedCorruptionStress,
            FumbleStress: accumulatedFumbleStress,
            Source: source,
            TriggersBreakingPoint: totalStress >= SkillStressResult.BreakingPointThreshold,
            CorruptionTier: corruptionTier);

        _logger.LogDebug(
            "Extended check stress: TotalStress={TotalStress} ({Steps} steps, {Fumbles} fumbles in {Tier})",
            result.TotalStress,
            stepCount,
            fumbleCount,
            corruptionTier);

        if (result.TriggersBreakingPoint)
        {
            _logger.LogInformation(
                "Extended check triggers breaking point: {TotalStress} total stress",
                result.TotalStress);
        }

        return result;
    }

    /// <inheritdoc/>
    public int GetCorruptionStressCost(CorruptionTier tier)
    {
        return SkillStressResult.GetCorruptionStress(tier);
    }

    /// <inheritdoc/>
    public int GetFumbleStressCost(CorruptionTier tier)
    {
        return SkillStressResult.GetFumbleStress(tier);
    }

    /// <summary>
    /// Extracts the corruption tier from a skill context.
    /// </summary>
    /// <param name="context">The skill context to examine.</param>
    /// <returns>The highest corruption tier found in environment modifiers.</returns>
    private static CorruptionTier ExtractCorruptionTier(SkillContext context)
    {
        // Find the highest corruption tier from environment modifiers
        var highestTier = CorruptionTier.Normal;

        foreach (var modifier in context.EnvironmentModifiers)
        {
            if (modifier.CorruptionTier.HasValue &&
                modifier.CorruptionTier.Value > highestTier)
            {
                highestTier = modifier.CorruptionTier.Value;
            }
        }

        return highestTier;
    }
}
