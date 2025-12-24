using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Models.Analysis;

/// <summary>
/// Accumulates statistics from multiple loot generation cycles for Monte Carlo analysis.
/// Used by the LootAuditService to track drop rates and compare against expected probabilities.
/// </summary>
public class LootStatistics
{
    #region Aggregate Counts

    /// <summary>
    /// Gets the total number of loot generation iterations recorded.
    /// </summary>
    public int TotalIterations { get; private set; }

    /// <summary>
    /// Gets the total number of individual items dropped across all iterations.
    /// </summary>
    public int TotalItemsDropped { get; private set; }

    /// <summary>
    /// Gets the cumulative Scrip value of all items dropped.
    /// </summary>
    public long TotalScripValue { get; private set; }

    /// <summary>
    /// Gets the cumulative weight in grams of all items dropped.
    /// </summary>
    public long TotalWeight { get; private set; }

    #endregion

    #region Distribution Tracking

    /// <summary>
    /// Gets the count of items dropped per quality tier.
    /// </summary>
    public Dictionary<QualityTier, int> QualityTierCounts { get; } = new();

    /// <summary>
    /// Gets the count of items dropped per item type.
    /// </summary>
    public Dictionary<ItemType, int> ItemTypeCounts { get; } = new();

    #endregion

    #region Derived Metrics

    /// <summary>
    /// Gets the average Scrip value per iteration.
    /// </summary>
    public double AverageScripPerIteration =>
        TotalIterations > 0 ? (double)TotalScripValue / TotalIterations : 0;

    /// <summary>
    /// Gets the average number of items per iteration.
    /// </summary>
    public double AverageItemsPerIteration =>
        TotalIterations > 0 ? (double)TotalItemsDropped / TotalIterations : 0;

    /// <summary>
    /// Gets the number of iterations that produced at least one item.
    /// </summary>
    public int SuccessfulIterations { get; private set; }

    /// <summary>
    /// Gets the percentage of iterations that produced loot.
    /// </summary>
    public double SuccessRate =>
        TotalIterations > 0 ? (double)SuccessfulIterations / TotalIterations * 100 : 0;

    #endregion

    #region Recording

    /// <summary>
    /// Records the result of a single loot generation cycle.
    /// </summary>
    /// <param name="result">The loot result to record.</param>
    public void Record(LootResult result)
    {
        TotalIterations++;

        if (!result.Success || result.Items.Count == 0)
            return;

        SuccessfulIterations++;

        foreach (var item in result.Items)
        {
            TotalItemsDropped++;
            TotalScripValue += item.Value;
            TotalWeight += item.Weight;

            // Track quality tier distribution
            if (!QualityTierCounts.ContainsKey(item.Quality))
                QualityTierCounts[item.Quality] = 0;
            QualityTierCounts[item.Quality]++;

            // Track item type distribution
            if (!ItemTypeCounts.ContainsKey(item.ItemType))
                ItemTypeCounts[item.ItemType] = 0;
            ItemTypeCounts[item.ItemType]++;
        }
    }

    #endregion

    #region Percentage Calculations

    /// <summary>
    /// Calculates the percentage of items in a given quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to calculate.</param>
    /// <returns>The percentage (0-100) of items in this tier.</returns>
    public double GetQualityTierPercent(QualityTier tier)
    {
        if (TotalItemsDropped == 0)
            return 0;

        var count = QualityTierCounts.GetValueOrDefault(tier, 0);
        return (double)count / TotalItemsDropped * 100;
    }

    /// <summary>
    /// Calculates the percentage of items of a given type.
    /// </summary>
    /// <param name="type">The item type to calculate.</param>
    /// <returns>The percentage (0-100) of items of this type.</returns>
    public double GetItemTypePercent(ItemType type)
    {
        if (TotalItemsDropped == 0)
            return 0;

        var count = ItemTypeCounts.GetValueOrDefault(type, 0);
        return (double)count / TotalItemsDropped * 100;
    }

    #endregion
}
