// ═══════════════════════════════════════════════════════════════════════════════
// RenderErrorContext.cs
// Provides diagnostic context information for render errors.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.Models;

/// <summary>
/// Provides diagnostic context information for a render error occurrence.
/// </summary>
/// <remarks>
/// <para>
/// This record captures all relevant information about a render error for
/// diagnostic purposes, including the component that failed, the exception
/// that occurred, the timestamp, and the classified severity.
/// </para>
/// <para>
/// Used by the SafeRender pattern to pass error context to the
/// <c>OnRenderError</c> virtual hook, allowing components to implement
/// custom error handling behavior.
/// </para>
/// </remarks>
/// <param name="ComponentName">The name of the component that encountered the error.</param>
/// <param name="Exception">The exception that was thrown during rendering.</param>
/// <param name="Timestamp">The UTC timestamp when the error occurred.</param>
/// <param name="Severity">The classified severity of the error.</param>
/// <param name="IsRecoverable">Whether the error allows continued operation with fallback content.</param>
/// <example>
/// <code>
/// // Creating an error context for diagnostic purposes
/// var context = new RenderErrorContext(
///     ComponentName: "HealthBar",
///     Exception: new FormatException("Invalid health value"),
///     Timestamp: DateTimeOffset.UtcNow,
///     Severity: ErrorSeverity.Recoverable,
///     IsRecoverable: true);
///     
/// // Accessing context properties
/// Logger.LogWarning("Error in {Component}: {Message}, Recoverable: {IsRecoverable}",
///     context.ComponentName,
///     context.Exception.Message,
///     context.IsRecoverable);
/// </code>
/// </example>
public sealed record RenderErrorContext(
    string ComponentName,
    Exception Exception,
    DateTimeOffset Timestamp,
    ErrorSeverity Severity,
    bool IsRecoverable)
{
    /// <summary>
    /// Gets the elapsed time since the error occurred.
    /// </summary>
    /// <remarks>
    /// Useful for tracking how long ago an error happened when reviewing
    /// multiple errors in sequence.
    /// </remarks>
    public TimeSpan ElapsedSinceError => DateTimeOffset.UtcNow - Timestamp;

    /// <summary>
    /// Gets a short description of the error suitable for logging.
    /// </summary>
    /// <remarks>
    /// Returns a formatted string combining component name and exception message.
    /// </remarks>
    public string ShortDescription => $"{ComponentName}: {Exception.Message}";

    /// <summary>
    /// Returns a string representation of the error context for debugging.
    /// </summary>
    /// <returns>A formatted string containing all context information.</returns>
    public override string ToString() =>
        $"[{Severity}] {ComponentName} at {Timestamp:HH:mm:ss.fff}: {Exception.Message} (Recoverable: {IsRecoverable})";
}
