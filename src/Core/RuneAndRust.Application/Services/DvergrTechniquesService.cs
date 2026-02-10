using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implements the Dvergr Techniques passive ability by providing 20% crafting cost
/// and time reduction for players who have unlocked the ability.
/// </summary>
/// <remarks>
/// <para>Dvergr Techniques is a Tier 2 Rúnasmiðr passive ability (v0.20.2b):</para>
/// <list type="bullet">
/// <item>Reduces all crafting material costs by 20%</item>
/// <item>Reduces all crafting time by 20%</item>
/// <item>Does NOT affect Rune Charge costs or other special resources</item>
/// <item>Applies to consumables and equipment equally</item>
/// <item>Stacks additively with other crafting bonuses</item>
/// </list>
/// </remarks>
public class DvergrTechniquesService : ICraftingCostModifier
{
    /// <summary>
    /// The percentage reduction applied by Dvergr Techniques (20%).
    /// </summary>
    public const decimal ReductionPercentage = 0.20m;

    /// <summary>
    /// The multiplier applied to costs (1 - ReductionPercentage = 0.80).
    /// </summary>
    public const decimal CostMultiplier = 1.0m - ReductionPercentage;

    private readonly ILogger<DvergrTechniquesService> _logger;

    /// <summary>
    /// Creates a new DvergrTechniquesService.
    /// </summary>
    /// <param name="logger">Logger for tracking cost modifications.</param>
    public DvergrTechniquesService(ILogger<DvergrTechniquesService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Applies Dvergr Techniques 20% material cost reduction if the player has the ability unlocked.
    /// </summary>
    /// <param name="player">The player performing the craft.</param>
    /// <param name="baseCost">The original material cost.</param>
    /// <returns>Reduced cost if applicable, otherwise the original cost. Never less than 1.</returns>
    public int ModifyMaterialCost(Player player, int baseCost)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegative(baseCost);

        if (baseCost == 0)
            return 0;

        if (!HasDvergrTechniques(player))
            return baseCost;

        var reducedCost = Math.Max(1, (int)(baseCost * CostMultiplier));

        _logger.LogInformation(
            "Dvergr Techniques applied to material cost for player {PlayerName}. " +
            "Cost reduced from {OriginalCost} to {ReducedCost} ({Percentage}% reduction)",
            player.Name, baseCost, reducedCost, (int)(ReductionPercentage * 100));

        return reducedCost;
    }

    /// <summary>
    /// Applies Dvergr Techniques 20% crafting time reduction if the player has the ability unlocked.
    /// </summary>
    /// <param name="player">The player performing the craft.</param>
    /// <param name="baseTimeMinutes">The original crafting time in minutes.</param>
    /// <returns>Reduced time if applicable, otherwise the original time. Never less than 1.</returns>
    public int ModifyCraftingTime(Player player, int baseTimeMinutes)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegative(baseTimeMinutes);

        if (baseTimeMinutes == 0)
            return 0;

        if (!HasDvergrTechniques(player))
            return baseTimeMinutes;

        var reducedTime = Math.Max(1, (int)(baseTimeMinutes * CostMultiplier));

        _logger.LogInformation(
            "Dvergr Techniques applied to crafting time for player {PlayerName}. " +
            "Time reduced from {OriginalTime}m to {ReducedTime}m ({Percentage}% reduction)",
            player.Name, baseTimeMinutes, reducedTime, (int)(ReductionPercentage * 100));

        return reducedTime;
    }

    /// <inheritdoc />
    public decimal GetCostReductionPercentage() => ReductionPercentage;

    /// <summary>
    /// Checks whether the player has the Dvergr Techniques ability unlocked.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has Dvergr Techniques unlocked.</returns>
    private static bool HasDvergrTechniques(Player player) =>
        player.HasDvergrTechniques;
}
