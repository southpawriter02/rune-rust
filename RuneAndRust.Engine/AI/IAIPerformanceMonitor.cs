using RuneAndRust.Core.AI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for monitoring and tracking AI performance metrics.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public interface IAIPerformanceMonitor
{
    /// <summary>
    /// Monitors the performance of an AI operation.
    /// Logs warning if operation exceeds 50ms threshold.
    /// </summary>
    /// <typeparam name="T">Return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation being monitored.</param>
    /// <param name="operation">The async operation to monitor.</param>
    /// <returns>Result of the operation.</returns>
    Task<T> MonitorPerformanceAsync<T>(string operationName, Func<Task<T>> operation);

    /// <summary>
    /// Records a performance metric.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    void RecordMetric(string operationName, long milliseconds);

    /// <summary>
    /// Gets all collected performance metrics.
    /// </summary>
    /// <returns>Dictionary of operation names to metrics.</returns>
    Dictionary<string, PerformanceMetrics> GetMetrics();

    /// <summary>
    /// Resets all performance metrics.
    /// </summary>
    void ResetMetrics();

    /// <summary>
    /// Gets metrics for a specific operation.
    /// </summary>
    /// <param name="operationName">Operation name.</param>
    /// <returns>Metrics for the operation, or null if not found.</returns>
    PerformanceMetrics? GetMetricsForOperation(string operationName);
}
