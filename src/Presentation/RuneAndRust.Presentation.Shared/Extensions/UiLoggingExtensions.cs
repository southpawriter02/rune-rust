// ═══════════════════════════════════════════════════════════════════════════════
// UiLoggingExtensions.cs
// Extension methods for consistent UI component logging patterns.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="ILogger"/> that provide consistent
/// logging patterns for UI components.
/// </summary>
/// <remarks>
/// <para>
/// This static class provides standardized logging methods for common UI
/// operations including render cycles, state changes, component events,
/// and fallback triggers. All methods use structured logging with
/// consistent message templates.
/// </para>
/// <para>
/// Logging levels follow these guidelines:
/// <list type="bullet">
///   <item><description>Trace: Render cycles, fine-grained events</description></item>
///   <item><description>Debug: State changes, lifecycle events, component init</description></item>
///   <item><description>Warning: Recoverable errors, fallback rendering triggered</description></item>
///   <item><description>Error: Unrecoverable errors, component failure</description></item>
/// </list>
/// </para>
/// </remarks>
public static class UiLoggingExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER LOGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs the start of a render operation and returns a disposable scope.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component being rendered.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that tracks the render duration.
    /// Dispose is called automatically when the render completes or fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Logs at Trace level: "Render starting: {Component}"
    /// </para>
    /// <para>
    /// The returned disposable tracks timing but does NOT automatically log
    /// completion. Call <see cref="LogRenderComplete"/> explicitly on success.
    /// This allows the caller to handle success and failure differently.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using (Logger.LogRenderStart("HealthBar"))
    /// {
    ///     RenderHealthBar();
    ///     Logger.LogRenderComplete("HealthBar", stopwatch.Elapsed);
    /// }
    /// </code>
    /// </example>
    public static IDisposable LogRenderStart(this ILogger logger, string componentName)
    {
        // Null check for safety in edge cases where logger might be null
        if (logger is null)
        {
            return NullRenderScope.Instance;
        }

        logger.LogTrace("Render starting: {Component}", componentName);
        return new RenderScope(logger, componentName, Stopwatch.StartNew());
    }

    /// <summary>
    /// Logs the successful completion of a render operation.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component that was rendered.</param>
    /// <param name="duration">The time taken to complete the render.</param>
    /// <remarks>
    /// Logs at Trace level: "Render complete: {Component} ({Duration:F2}ms)"
    /// </remarks>
    public static void LogRenderComplete(
        this ILogger logger,
        string componentName,
        TimeSpan duration)
    {
        logger?.LogTrace(
            "Render complete: {Component} ({Duration:F2}ms)",
            componentName,
            duration.TotalMilliseconds);
    }

    /// <summary>
    /// Logs a render error with exception details.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component that failed.</param>
    /// <param name="exception">The exception that occurred during rendering.</param>
    /// <remarks>
    /// Logs at Warning level: "Render failed: {Component} - {Error}"
    /// The exception is included for stack trace capture.
    /// </remarks>
    public static void LogRenderError(
        this ILogger logger,
        string componentName,
        Exception exception)
    {
        logger?.LogWarning(
            exception,
            "Render failed: {Component} - {Error}",
            componentName,
            exception.Message);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE CHANGE LOGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs a state change with old and new values.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component with the state change.</param>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="oldValue">The previous value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    /// <remarks>
    /// Logs at Debug level: "State change: {Component}.{Property}: {Old} -> {New}"
    /// </remarks>
    /// <example>
    /// <code>
    /// set
    /// {
    ///     if (_health != value)
    ///     {
    ///         Logger.LogStateChange(ComponentName, nameof(Health), _health, value);
    ///         _health = value;
    ///     }
    /// }
    /// </code>
    /// </example>
    public static void LogStateChange<T>(
        this ILogger logger,
        string componentName,
        string propertyName,
        T oldValue,
        T newValue)
    {
        logger?.LogDebug(
            "State change: {Component}.{Property}: {Old} -> {New}",
            componentName,
            propertyName,
            oldValue,
            newValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT EVENT LOGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs a component event.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <remarks>
    /// Logs at Debug level: "Component event: {Component} - {Event}"
    /// </remarks>
    public static void LogComponentEvent(
        this ILogger logger,
        string componentName,
        string eventName)
    {
        logger?.LogDebug(
            "Component event: {Component} - {Event}",
            componentName,
            eventName);
    }

    /// <summary>
    /// Logs a component event with additional details.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="details">Additional details about the event.</param>
    /// <remarks>
    /// Logs at Debug level: "Component event: {Component} - {Event}: {Details}"
    /// </remarks>
    public static void LogComponentEvent(
        this ILogger logger,
        string componentName,
        string eventName,
        object details)
    {
        logger?.LogDebug(
            "Component event: {Component} - {Event}: {Details}",
            componentName,
            eventName,
            details);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FALLBACK LOGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs when fallback rendering is triggered.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component using fallback.</param>
    /// <param name="fallbackType">The type of fallback being used.</param>
    /// <remarks>
    /// Logs at Warning level: "Fallback triggered: {Component} using {FallbackType}"
    /// </remarks>
    public static void LogFallbackTriggered(
        this ILogger logger,
        string componentName,
        FallbackType fallbackType)
    {
        logger?.LogWarning(
            "Fallback triggered: {Component} using {FallbackType}",
            componentName,
            fallbackType);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR CLASSIFICATION LOGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs an error classification result based on severity.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="componentName">The name of the component with the error.</param>
    /// <param name="exception">The exception that was classified.</param>
    /// <param name="severity">The classified severity level.</param>
    /// <remarks>
    /// <para>Logging levels vary by severity:</para>
    /// <list type="bullet">
    ///   <item><description>Transient: Debug level - "Transient error in {Component}, will retry"</description></item>
    ///   <item><description>Recoverable: Warning level - "Recoverable error in {Component}, using fallback"</description></item>
    ///   <item><description>Permanent: Warning level - "Permanent error in {Component}, fallback only"</description></item>
    ///   <item><description>Critical: Error level - "Critical error in {Component}, propagating"</description></item>
    /// </list>
    /// </remarks>
    public static void LogErrorClassification(
        this ILogger logger,
        string componentName,
        Exception exception,
        ErrorSeverity severity)
    {
        if (logger is null) return;

        switch (severity)
        {
            case ErrorSeverity.Transient:
                logger.LogDebug(
                    exception,
                    "Transient error in {Component}, will retry",
                    componentName);
                break;

            case ErrorSeverity.Recoverable:
                logger.LogWarning(
                    exception,
                    "Recoverable error in {Component}, using fallback",
                    componentName);
                break;

            case ErrorSeverity.Permanent:
                logger.LogWarning(
                    exception,
                    "Permanent error in {Component}, fallback only",
                    componentName);
                break;

            case ErrorSeverity.Critical:
                logger.LogError(
                    exception,
                    "Critical error in {Component}, propagating",
                    componentName);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER CLASSES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Disposable scope that tracks render duration for timing purposes.
    /// </summary>
    /// <remarks>
    /// This class is used internally by <see cref="LogRenderStart"/> to
    /// provide a disposable scope. It does NOT automatically log completion
    /// on dispose; the caller must call <see cref="LogRenderComplete"/>
    /// explicitly for successful renders.
    /// </remarks>
    private sealed class RenderScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _componentName;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderScope"/> class.
        /// </summary>
        /// <param name="logger">The logger for this scope.</param>
        /// <param name="componentName">The component name for logging.</param>
        /// <param name="stopwatch">The stopwatch tracking duration.</param>
        public RenderScope(ILogger logger, string componentName, Stopwatch stopwatch)
        {
            _logger = logger;
            _componentName = componentName;
            _stopwatch = stopwatch;
        }

        /// <summary>
        /// Disposes the render scope, stopping the stopwatch.
        /// </summary>
        /// <remarks>
        /// Note: This does NOT log render completion. The caller must
        /// call <see cref="LogRenderComplete"/> explicitly on success.
        /// This allows error paths to log differently than success paths.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// Null object pattern for render scope when logger is null.
    /// </summary>
    private sealed class NullRenderScope : IDisposable
    {
        /// <summary>
        /// Singleton instance of the null render scope.
        /// </summary>
        public static readonly NullRenderScope Instance = new();

        /// <summary>
        /// Does nothing - null object pattern.
        /// </summary>
        public void Dispose()
        {
            // No-op
        }
    }
}
