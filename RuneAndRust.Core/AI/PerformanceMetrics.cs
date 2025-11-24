namespace RuneAndRust.Core.AI;

/// <summary>
/// Performance metrics for AI operations.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Total number of calls to this operation.
    /// </summary>
    public int TotalCalls { get; set; }

    /// <summary>
    /// Total milliseconds spent in this operation.
    /// </summary>
    public long TotalMs { get; set; }

    /// <summary>
    /// Average milliseconds per call.
    /// </summary>
    public double AverageMs { get; set; }

    /// <summary>
    /// Maximum milliseconds for a single call.
    /// </summary>
    public long MaxMs { get; set; }

    /// <summary>
    /// Minimum milliseconds for a single call.
    /// </summary>
    public long MinMs { get; set; }

    /// <summary>
    /// Last recorded timestamp.
    /// </summary>
    public DateTime LastRecorded { get; set; } = DateTime.UtcNow;
}
