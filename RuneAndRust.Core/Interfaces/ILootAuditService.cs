using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Analysis;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service interface for running Monte Carlo simulations of loot generation.
/// Used to validate drop rates and detect economy imbalances.
/// </summary>
public interface ILootAuditService
{
    /// <summary>
    /// Runs a batch loot generation simulation and produces an analysis report.
    /// </summary>
    /// <param name="config">The audit configuration specifying iterations and context.</param>
    /// <returns>A report containing statistics, markdown output, and variance flags.</returns>
    Task<LootAuditReport> RunAuditAsync(LootAuditConfiguration config);
}

/// <summary>
/// Configuration for a loot audit simulation run.
/// </summary>
/// <param name="Iterations">Number of loot generation cycles to run.</param>
/// <param name="Biome">The biome type affecting item type distribution.</param>
/// <param name="DangerLevel">The danger level affecting quality tier weights.</param>
/// <param name="WitsBonus">Optional character WITS bonus for quality upgrade chances.</param>
public record LootAuditConfiguration(
    int Iterations,
    BiomeType Biome,
    DangerLevel DangerLevel,
    int WitsBonus = 0)
{
    /// <summary>
    /// Creates a default configuration for quick testing.
    /// </summary>
    public static LootAuditConfiguration Default => new(10000, BiomeType.Ruin, DangerLevel.Safe);
}

/// <summary>
/// The complete result of a loot audit simulation.
/// </summary>
/// <param name="Statistics">Accumulated statistics from all iterations.</param>
/// <param name="MarkdownReport">Human-readable markdown report for documentation.</param>
/// <param name="Flags">List of variance flags indicating deviations from expected rates.</param>
public record LootAuditReport(
    LootStatistics Statistics,
    string MarkdownReport,
    IReadOnlyList<VarianceFlag> Flags);

/// <summary>
/// Represents a deviation between actual and expected drop rates.
/// </summary>
/// <param name="Category">The category being measured (e.g., "QualityTier", "ItemType").</param>
/// <param name="ItemName">The specific tier or type name.</param>
/// <param name="ActualPercent">The actual observed percentage.</param>
/// <param name="ExpectedPercent">The expected percentage from LootTables.</param>
/// <param name="Severity">The severity level based on deviation magnitude.</param>
public record VarianceFlag(
    string Category,
    string ItemName,
    double ActualPercent,
    double ExpectedPercent,
    VarianceSeverity Severity)
{
    /// <summary>
    /// Gets the variance (actual - expected) as a percentage.
    /// </summary>
    public double Variance => ActualPercent - ExpectedPercent;

    /// <summary>
    /// Gets the absolute variance magnitude.
    /// </summary>
    public double AbsoluteVariance => Math.Abs(Variance);
}

/// <summary>
/// Severity levels for variance flagging based on deviation magnitude.
/// </summary>
public enum VarianceSeverity
{
    /// <summary>
    /// Variance is within acceptable bounds (less than 1%).
    /// </summary>
    Ok,

    /// <summary>
    /// Variance is notable but not critical (1-5%).
    /// </summary>
    Warning,

    /// <summary>
    /// Variance is significant and requires attention (greater than 5%).
    /// </summary>
    Critical
}
