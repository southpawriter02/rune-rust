using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for debugging and visualizing AI decision-making.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public interface IAIDebugService
{
    /// <summary>
    /// Enables AI debug mode (verbose logging).
    /// </summary>
    void EnableDebugMode();

    /// <summary>
    /// Disables AI debug mode.
    /// </summary>
    void DisableDebugMode();

    /// <summary>
    /// Checks if debug mode is enabled.
    /// </summary>
    /// <returns>True if debug mode is active.</returns>
    bool IsDebugModeEnabled();

    /// <summary>
    /// Logs an AI decision with full context.
    /// </summary>
    /// <param name="enemy">The enemy making the decision.</param>
    /// <param name="action">The action decided.</param>
    /// <param name="context">Decision context.</param>
    void LogDecision(Enemy enemy, EnemyAction action, DecisionContext context);

    /// <summary>
    /// Generates a comprehensive decision report for a combat encounter.
    /// </summary>
    /// <param name="encounterId">Combat encounter ID.</param>
    /// <returns>Decision report with statistics.</returns>
    AIDecisionReport GenerateDecisionReport(Guid encounterId);

    /// <summary>
    /// Logs a performance warning if decision took too long.
    /// </summary>
    /// <param name="enemy">The enemy.</param>
    /// <param name="durationMs">Decision duration in milliseconds.</param>
    void LogPerformanceWarning(Enemy enemy, long durationMs);
}
