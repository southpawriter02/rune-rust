// ═══════════════════════════════════════════════════════════════════════════════
// ErrorSeverity.cs
// Defines severity levels for render errors to determine recovery strategy.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Defines severity levels for render errors to determine the appropriate
/// recovery strategy.
/// </summary>
/// <remarks>
/// <para>The severity level impacts how the SafeRender pattern handles errors:</para>
/// <list type="bullet">
///   <item><description><see cref="Transient"/>: May self-resolve; retry immediately.</description></item>
///   <item><description><see cref="Recoverable"/>: Use fallback content; may retry later.</description></item>
///   <item><description><see cref="Permanent"/>: Use fallback content; don't retry automatically.</description></item>
///   <item><description><see cref="Critical"/>: Propagate exception to caller.</description></item>
/// </list>
/// </remarks>
public enum ErrorSeverity
{
    /// <summary>
    /// Transient error that may self-resolve.
    /// </summary>
    /// <remarks>
    /// Examples include temporary resource contention, brief network timeouts,
    /// or timing-related issues. The SafeRender pattern will retry once
    /// before falling back.
    /// </remarks>
    Transient = 0,

    /// <summary>
    /// Recoverable error where fallback content can be used.
    /// </summary>
    /// <remarks>
    /// Examples include format errors, missing optional data, or
    /// out-of-range values that can be clamped. The component will
    /// render fallback content but may recover on the next update.
    /// </remarks>
    Recoverable = 1,

    /// <summary>
    /// Permanent error that won't self-resolve without external intervention.
    /// </summary>
    /// <remarks>
    /// Examples include null data required for rendering, invalid operations,
    /// or configuration errors. The component will render fallback content
    /// and remain in degraded state until data is corrected.
    /// </remarks>
    Permanent = 2,

    /// <summary>
    /// Critical error that must be propagated to the caller.
    /// </summary>
    /// <remarks>
    /// Examples include out-of-memory conditions, stack overflows, or
    /// disposed object access. These errors cannot be handled gracefully
    /// and must propagate for higher-level handling (e.g., graceful shutdown).
    /// </remarks>
    Critical = 3
}
