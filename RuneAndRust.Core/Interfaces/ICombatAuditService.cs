using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Analysis;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service interface for running Monte Carlo simulations of combat encounters.
/// Used to validate balance, generate TTK curves, and detect combat imbalances.
/// </summary>
public interface ICombatAuditService
{
    /// <summary>
    /// Runs a batch combat simulation and produces an analysis report.
    /// </summary>
    /// <param name="config">The audit configuration specifying iterations and combatants.</param>
    /// <returns>A report containing statistics, markdown output, and variance flags.</returns>
    Task<CombatAuditReport> RunAuditAsync(CombatAuditConfiguration config);
}

/// <summary>
/// Configuration for a combat audit simulation run.
/// </summary>
/// <param name="Iterations">Number of combat encounters to simulate.</param>
/// <param name="PlayerArchetype">The player archetype to simulate (Warrior, Skirmisher, etc.).</param>
/// <param name="EnemyTemplateId">The enemy template ID to fight against (e.g., "und_draugr_01").</param>
/// <param name="PlayerLevel">The player level for stat scaling (default: 1).</param>
/// <param name="Seed">Optional RNG seed for deterministic results.</param>
public record CombatAuditConfiguration(
    int Iterations,
    ArchetypeType PlayerArchetype,
    string EnemyTemplateId,
    int PlayerLevel = 1,
    int? Seed = null)
{
    /// <summary>
    /// Creates a default configuration for quick testing.
    /// </summary>
    public static CombatAuditConfiguration Default =>
        new(1000, ArchetypeType.Warrior, "und_draugr_01");
}

/// <summary>
/// The complete result of a combat audit simulation.
/// </summary>
/// <param name="Statistics">Accumulated statistics from all encounters.</param>
/// <param name="MarkdownReport">Human-readable markdown report for documentation.</param>
/// <param name="Flags">List of variance flags indicating deviations from expected metrics.</param>
public record CombatAuditReport(
    CombatStatistics Statistics,
    string MarkdownReport,
    IReadOnlyList<CombatVarianceFlag> Flags);

/// <summary>
/// Represents a deviation between actual and expected combat metrics.
/// </summary>
/// <param name="Metric">The metric being measured (e.g., "WinRate", "HitRate").</param>
/// <param name="ActualValue">The actual observed value.</param>
/// <param name="ExpectedMin">The minimum expected value.</param>
/// <param name="ExpectedMax">The maximum expected value.</param>
/// <param name="Severity">The severity level based on deviation magnitude.</param>
public record CombatVarianceFlag(
    string Metric,
    double ActualValue,
    double ExpectedMin,
    double ExpectedMax,
    VarianceSeverity Severity)
{
    /// <summary>
    /// Gets whether the actual value is within expected bounds.
    /// </summary>
    public bool IsWithinBounds => ActualValue >= ExpectedMin && ActualValue <= ExpectedMax;

    /// <summary>
    /// Gets the deviation from the nearest bound as a percentage.
    /// </summary>
    public double Deviation =>
        ActualValue < ExpectedMin
            ? ExpectedMin - ActualValue
            : ActualValue > ExpectedMax
                ? ActualValue - ExpectedMax
                : 0;
}
