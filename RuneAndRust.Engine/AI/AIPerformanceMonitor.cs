using Microsoft.Extensions.Logging;
using RuneAndRust.Core.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for monitoring and tracking AI performance metrics.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class AIPerformanceMonitor : IAIPerformanceMonitor
{
    private readonly ILogger<AIPerformanceMonitor> _logger;
    private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
    private const long PerformanceThresholdMs = 50;

    public AIPerformanceMonitor(ILogger<AIPerformanceMonitor> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T> MonitorPerformanceAsync<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();
            stopwatch.Stop();

            RecordMetric(operationName, stopwatch.ElapsedMilliseconds);

            if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMs)
            {
                _logger.LogWarning(
                    "AI operation {Operation} took {Ms}ms (threshold: {Threshold}ms)",
                    operationName, stopwatch.ElapsedMilliseconds, PerformanceThresholdMs);
            }
            else
            {
                _logger.LogDebug(
                    "AI operation {Operation} completed in {Ms}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "AI operation {Operation} failed after {Ms}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <inheritdoc/>
    public void RecordMetric(string operationName, long milliseconds)
    {
        var metric = _metrics.GetOrAdd(operationName, _ => new PerformanceMetrics
        {
            MinMs = long.MaxValue
        });

        lock (metric)
        {
            metric.TotalCalls++;
            metric.TotalMs += milliseconds;
            metric.MaxMs = Math.Max(metric.MaxMs, milliseconds);
            metric.MinMs = Math.Min(metric.MinMs, milliseconds);
            metric.AverageMs = (double)metric.TotalMs / metric.TotalCalls;
            metric.LastRecorded = DateTime.UtcNow;
        }

        _logger.LogTrace(
            "Recorded metric for {Operation}: {Ms}ms (avg: {Avg:F1}ms, calls: {Calls})",
            operationName, milliseconds, metric.AverageMs, metric.TotalCalls);
    }

    /// <inheritdoc/>
    public Dictionary<string, PerformanceMetrics> GetMetrics()
    {
        return _metrics.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                lock (kvp.Value)
                {
                    return new PerformanceMetrics
                    {
                        TotalCalls = kvp.Value.TotalCalls,
                        TotalMs = kvp.Value.TotalMs,
                        AverageMs = kvp.Value.AverageMs,
                        MaxMs = kvp.Value.MaxMs,
                        MinMs = kvp.Value.MinMs == long.MaxValue ? 0 : kvp.Value.MinMs,
                        LastRecorded = kvp.Value.LastRecorded
                    };
                }
            });
    }

    /// <inheritdoc/>
    public void ResetMetrics()
    {
        _metrics.Clear();
        _logger.LogInformation("All AI performance metrics reset");
    }

    /// <inheritdoc/>
    public PerformanceMetrics? GetMetricsForOperation(string operationName)
    {
        if (_metrics.TryGetValue(operationName, out var metric))
        {
            lock (metric)
            {
                return new PerformanceMetrics
                {
                    TotalCalls = metric.TotalCalls,
                    TotalMs = metric.TotalMs,
                    AverageMs = metric.AverageMs,
                    MaxMs = metric.MaxMs,
                    MinMs = metric.MinMs == long.MaxValue ? 0 : metric.MinMs,
                    LastRecorded = metric.LastRecorded
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Generates a performance summary report.
    /// </summary>
    public string GeneratePerformanceSummary()
    {
        var metrics = GetMetrics();

        if (!metrics.Any())
        {
            return "No performance metrics recorded.";
        }

        var summary = new System.Text.StringBuilder();
        summary.AppendLine("=== AI Performance Summary ===");
        summary.AppendLine();

        foreach (var (operation, metric) in metrics.OrderByDescending(m => m.Value.AverageMs))
        {
            summary.AppendLine($"Operation: {operation}");
            summary.AppendLine($"  Total Calls: {metric.TotalCalls}");
            summary.AppendLine($"  Average: {metric.AverageMs:F2}ms");
            summary.AppendLine($"  Min: {metric.MinMs}ms");
            summary.AppendLine($"  Max: {metric.MaxMs}ms");
            summary.AppendLine($"  Total Time: {metric.TotalMs}ms");
            summary.AppendLine($"  Last Recorded: {metric.LastRecorded:yyyy-MM-dd HH:mm:ss}");

            if (metric.AverageMs > PerformanceThresholdMs)
            {
                summary.AppendLine($"  ⚠ WARNING: Average exceeds threshold ({PerformanceThresholdMs}ms)");
            }

            summary.AppendLine();
        }

        return summary.ToString();
    }
}
