// ═══════════════════════════════════════════════════════════════════════════════
// FallbackType.cs
// Defines types of fallback content that can be rendered when errors occur.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Defines the types of fallback content that can be rendered when a
/// component encounters an error.
/// </summary>
/// <remarks>
/// <para>Different fallback types are appropriate for different scenarios:</para>
/// <list type="bullet">
///   <item><description><see cref="Empty"/>: Hides the component entirely.</description></item>
///   <item><description><see cref="Placeholder"/>: Shows neutral placeholder text.</description></item>
///   <item><description><see cref="ErrorMessage"/>: Shows error indicator for debugging.</description></item>
///   <item><description><see cref="LastKnown"/>: Shows the last successfully rendered content.</description></item>
/// </list>
/// </remarks>
public enum FallbackType
{
    /// <summary>
    /// Render empty/hidden content.
    /// </summary>
    /// <remarks>
    /// The component becomes invisible. Useful for optional decorative
    /// elements where absence is acceptable.
    /// </remarks>
    Empty = 0,

    /// <summary>
    /// Render generic placeholder text.
    /// </summary>
    /// <remarks>
    /// Shows a neutral placeholder like "[ComponentName]" to indicate
    /// the component's location while avoiding misleading data.
    /// This is the default fallback type.
    /// </remarks>
    Placeholder = 1,

    /// <summary>
    /// Render error indicator.
    /// </summary>
    /// <remarks>
    /// Shows an error indicator like "[!ComponentName]" with error styling.
    /// Useful for development to clearly identify failed components.
    /// </remarks>
    ErrorMessage = 2,

    /// <summary>
    /// Render the last known valid content.
    /// </summary>
    /// <remarks>
    /// Shows cached content from the last successful render. Useful for
    /// transient errors where stale data is preferable to no data.
    /// Requires the component to cache its last valid output.
    /// </remarks>
    LastKnown = 3
}
