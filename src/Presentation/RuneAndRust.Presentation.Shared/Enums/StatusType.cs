// ═══════════════════════════════════════════════════════════════════════════════
// StatusType.cs
// Status type enum for status indicator icons.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Represents the type of status indicator for visual feedback.
/// </summary>
/// <remarks>
/// Used by <see cref="IconUtilities.GetStatusIcon"/> to return appropriate
/// status icons for various UI states.
/// </remarks>
public enum StatusType
{
    /// <summary>Operation completed successfully (✓).</summary>
    Success,

    /// <summary>Operation failed (✗).</summary>
    Failure,

    /// <summary>Warning or caution state (⚠).</summary>
    Warning,

    /// <summary>Informational status (ℹ).</summary>
    Info,

    /// <summary>Pending operation, not yet started (◌).</summary>
    Pending,

    /// <summary>Operation in progress (◐).</summary>
    InProgress
}
