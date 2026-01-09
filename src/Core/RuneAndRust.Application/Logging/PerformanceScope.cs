using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RuneAndRust.Application.Logging;

/// <summary>
/// Provides automatic performance logging for operations.
/// Logs elapsed time on disposal and warns when threshold is exceeded.
/// </summary>
public sealed class PerformanceScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private readonly LogLevel _logLevel;
    private readonly long _warningThresholdMs;

    /// <summary>
    /// Creates a new performance scope.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="operationName">The name of the operation being measured.</param>
    /// <param name="logLevel">The log level for normal completion (default: Debug).</param>
    /// <param name="warningThresholdMs">Threshold in ms above which a warning is logged (default: 1000).</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or operationName is null.</exception>
    public PerformanceScope(
        ILogger logger,
        string operationName,
        LogLevel logLevel = LogLevel.Debug,
        long warningThresholdMs = 1000)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _logLevel = logLevel;
        _warningThresholdMs = warningThresholdMs;
        _stopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Starting operation: {Operation}", operationName);
    }

    /// <summary>
    /// Gets the elapsed milliseconds since the scope was created.
    /// </summary>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    /// <inheritdoc />
    public void Dispose()
    {
        _stopwatch.Stop();
        var elapsed = _stopwatch.ElapsedMilliseconds;

        if (elapsed > _warningThresholdMs)
        {
            _logger.LogWarning(
                "Operation {Operation} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                _operationName, elapsed, _warningThresholdMs);
        }
        else
        {
            _logger.Log(_logLevel,
                "Operation {Operation} completed in {ElapsedMs}ms",
                _operationName, elapsed);
        }
    }
}
