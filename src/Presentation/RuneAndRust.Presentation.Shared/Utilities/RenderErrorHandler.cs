// ═══════════════════════════════════════════════════════════════════════════════
// RenderErrorHandler.cs
// Centralized utility for handling and classifying render errors.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Models;

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides centralized error handling and classification utilities for
/// UI component render errors.
/// </summary>
/// <remarks>
/// <para>
/// This static utility class provides methods for:
/// <list type="bullet">
///   <item><description>Classifying exceptions by severity</description></item>
///   <item><description>Determining if errors should trigger retry</description></item>
///   <item><description>Generating fallback content for failed components</description></item>
///   <item><description>Creating diagnostic error contexts</description></item>
/// </list>
/// </para>
/// <para>
/// The error classification follows these guidelines:
/// <list type="table">
///   <listheader>
///     <term>Severity</term>
///     <description>Exception Types</description>
///   </listheader>
///   <item>
///     <term>Transient</term>
///     <description>TimeoutException, temporary I/O errors</description>
///   </item>
///   <item>
///     <term>Recoverable</term>
///     <description>FormatException, ArgumentOutOfRangeException, KeyNotFoundException</description>
///   </item>
///   <item>
///     <term>Permanent</term>
///     <description>ArgumentNullException, NullReferenceException, InvalidOperationException</description>
///   </item>
///   <item>
///     <term>Critical</term>
///     <description>OutOfMemoryException, StackOverflowException, ObjectDisposedException</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public static class RenderErrorHandler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR HANDLING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles a render error by logging with appropriate severity.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="componentName">The name of the component that failed.</param>
    /// <param name="logger">The logger to use for recording the error.</param>
    /// <remarks>
    /// <para>
    /// This method classifies the exception and logs it at the appropriate level:
    /// <list type="bullet">
    ///   <item><description>Transient: Debug level - will retry</description></item>
    ///   <item><description>Recoverable: Warning level - using fallback</description></item>
    ///   <item><description>Permanent: Warning level - fallback only</description></item>
    ///   <item><description>Critical: Error level - propagating</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static void HandleRenderError(
        Exception exception,
        string componentName,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(componentName);

        var severity = GetErrorSeverity(exception);

        // Log based on severity
        switch (severity)
        {
            case ErrorSeverity.Transient:
                logger?.LogDebug(
                    exception,
                    "Transient error in {Component}, will retry",
                    componentName);
                break;

            case ErrorSeverity.Recoverable:
                logger?.LogWarning(
                    exception,
                    "Recoverable error in {Component}, using fallback",
                    componentName);
                break;

            case ErrorSeverity.Permanent:
                logger?.LogWarning(
                    exception,
                    "Permanent error in {Component}, fallback only",
                    componentName);
                break;

            case ErrorSeverity.Critical:
                logger?.LogError(
                    exception,
                    "Critical error in {Component}, propagating",
                    componentName);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FALLBACK CONTENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets fallback content text for a failed component.
    /// </summary>
    /// <param name="componentName">The name of the component that failed.</param>
    /// <param name="fallbackType">The type of fallback content to generate.</param>
    /// <returns>A fallback string appropriate for the requested type.</returns>
    /// <remarks>
    /// <para>Fallback content by type:</para>
    /// <list type="bullet">
    ///   <item><description><see cref="FallbackType.Empty"/>: Returns empty string</description></item>
    ///   <item><description><see cref="FallbackType.Placeholder"/>: Returns "[ComponentName]"</description></item>
    ///   <item><description><see cref="FallbackType.ErrorMessage"/>: Returns "[!ComponentName]"</description></item>
    ///   <item><description><see cref="FallbackType.LastKnown"/>: Returns "[~ComponentName]"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var fallback = RenderErrorHandler.GetFallbackContent("HealthBar", FallbackType.Placeholder);
    /// // Returns: "[HealthBar]"
    /// </code>
    /// </example>
    public static string GetFallbackContent(string componentName, FallbackType fallbackType)
    {
        return fallbackType switch
        {
            FallbackType.Empty => string.Empty,
            FallbackType.Placeholder => $"[{componentName}]",
            FallbackType.ErrorMessage => $"[!{componentName}]",
            FallbackType.LastKnown => $"[~{componentName}]",
            _ => $"[{componentName}]"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR CLASSIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if an error should trigger an automatic retry.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the exception is transient and may self-resolve;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>Exceptions that qualify for retry:</para>
    /// <list type="bullet">
    ///   <item><description><see cref="TimeoutException"/>: May be temporary contention</description></item>
    ///   <item><description><see cref="IOException"/>: If message indicates temporary condition</description></item>
    /// </list>
    /// <para>Exceptions that should NOT retry:</para>
    /// <list type="bullet">
    ///   <item><description><see cref="ArgumentException"/>: Logic error, won't self-resolve</description></item>
    ///   <item><description><see cref="NullReferenceException"/>: Data error, needs fix</description></item>
    ///   <item><description><see cref="InvalidOperationException"/>: State error, needs intervention</description></item>
    ///   <item><description><see cref="ObjectDisposedException"/>: Cannot recover</description></item>
    /// </list>
    /// </remarks>
    public static bool ShouldRetry(Exception exception)
    {
        return exception switch
        {
            // Transient errors that may self-resolve
            TimeoutException => true,
            IOException io when IsTransientIO(io) => true,

            // Never retry for these - logic/data errors
            // Note: More specific types checked before falling through to defaults
            ObjectDisposedException => false,
            NullReferenceException => false,
            InvalidOperationException => false,
            ArgumentException => false, // Base type last in this group

            // Default: don't retry
            _ => false
        };
    }

    /// <summary>
    /// Gets the severity level of an exception for classification purposes.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>The <see cref="ErrorSeverity"/> classification for the exception.</returns>
    /// <remarks>
    /// <para>Classification logic:</para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Severity</term>
    ///     <description>Exception Types</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Transient</term>
    ///     <description>TimeoutException - may self-resolve</description>
    ///   </item>
    ///   <item>
    ///     <term>Recoverable</term>
    ///     <description>FormatException, ArgumentOutOfRangeException, KeyNotFoundException, DivideByZeroException</description>
    ///   </item>
    ///   <item>
    ///     <term>Permanent</term>
    ///     <description>ArgumentNullException, NullReferenceException, InvalidOperationException</description>
    ///   </item>
    ///   <item>
    ///     <term>Critical</term>
    ///     <description>OutOfMemoryException, StackOverflowException, ObjectDisposedException, AccessViolationException</description>
    ///   </item>
    /// </list>
    /// <para>Unknown exceptions default to <see cref="ErrorSeverity.Recoverable"/>.</para>
    /// </remarks>
    public static ErrorSeverity GetErrorSeverity(Exception exception)
    {
        return exception switch
        {
            // Critical - must propagate, cannot handle gracefully
            // Note: Check critical types first as they need immediate propagation
            OutOfMemoryException => ErrorSeverity.Critical,
            StackOverflowException => ErrorSeverity.Critical,
            AccessViolationException => ErrorSeverity.Critical,
            ObjectDisposedException => ErrorSeverity.Critical,

            // Transient - may self-resolve with retry
            TimeoutException => ErrorSeverity.Transient,

            // Recoverable - fallback works, may fix on next update
            // Note: More specific ArgumentException types before base type
            ArgumentOutOfRangeException => ErrorSeverity.Recoverable,
            FormatException => ErrorSeverity.Recoverable,
            KeyNotFoundException => ErrorSeverity.Recoverable,
            DivideByZeroException => ErrorSeverity.Recoverable,
            IndexOutOfRangeException => ErrorSeverity.Recoverable,

            // Permanent - won't self-resolve, needs code/data fix
            // Note: ArgumentNullException inherits from ArgumentException, so check it first
            ArgumentNullException => ErrorSeverity.Permanent,
            InvalidOperationException => ErrorSeverity.Permanent,
            NullReferenceException => ErrorSeverity.Permanent,
            NotSupportedException => ErrorSeverity.Permanent,
            NotImplementedException => ErrorSeverity.Permanent,

            // Default to recoverable for unknown exceptions
            _ => ErrorSeverity.Recoverable
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR CONTEXT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an error context record for diagnostic purposes.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="componentName">The name of the component that failed.</param>
    /// <returns>A <see cref="RenderErrorContext"/> containing diagnostic information.</returns>
    /// <remarks>
    /// <para>
    /// The created context includes:
    /// <list type="bullet">
    ///   <item><description>Component name for identification</description></item>
    ///   <item><description>Exception for stack trace and details</description></item>
    ///   <item><description>UTC timestamp for correlation</description></item>
    ///   <item><description>Classified severity</description></item>
    ///   <item><description>Recoverability flag</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// catch (Exception ex)
    /// {
    ///     var context = RenderErrorHandler.CreateErrorContext(ex, ComponentName);
    ///     OnRenderError(context);
    /// }
    /// </code>
    /// </example>
    public static RenderErrorContext CreateErrorContext(
        Exception exception,
        string componentName)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(componentName);

        var severity = GetErrorSeverity(exception);

        return new RenderErrorContext(
            ComponentName: componentName,
            Exception: exception,
            Timestamp: DateTimeOffset.UtcNow,
            Severity: severity,
            IsRecoverable: severity != ErrorSeverity.Critical);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if an I/O exception is transient and may self-resolve.
    /// </summary>
    /// <param name="exception">The IOException to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the exception message indicates a temporary condition;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Checks for keywords in the exception message that indicate temporary
    /// conditions, such as "temporarily", "busy", or "locked".
    /// </remarks>
    private static bool IsTransientIO(IOException exception)
    {
        // Check for transient I/O conditions in the message
        var message = exception.Message.ToLowerInvariant();
        return message.Contains("temporarily") ||
               message.Contains("busy") ||
               message.Contains("locked") ||
               message.Contains("in use");
    }
}
