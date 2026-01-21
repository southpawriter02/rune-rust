// ═══════════════════════════════════════════════════════════════════════════════
// LineDirection.cs
// Represents the direction of a line segment for prerequisite connection lines.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Enums;

/// <summary>
/// Represents the direction of a line segment for prerequisite connection lines.
/// </summary>
/// <remarks>
/// <para>Line directions determine which character to use for rendering:</para>
/// <list type="bullet">
///   <item><description><see cref="Horizontal"/>: ─ (horizontal line)</description></item>
///   <item><description><see cref="Vertical"/>: │ (vertical line)</description></item>
///   <item><description>Corner variants for path routing</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var direction = start.Y == end.Y 
///     ? LineDirection.Horizontal 
///     : LineDirection.Vertical;
/// var character = lineRenderer.GetLineCharacter(direction);
/// </code>
/// </example>
public enum LineDirection
{
    /// <summary>
    /// Horizontal line segment (─).
    /// </summary>
    /// <remarks>
    /// Used for drawing lines between nodes at the same Y position.
    /// </remarks>
    Horizontal,

    /// <summary>
    /// Vertical line segment (│).
    /// </summary>
    /// <remarks>
    /// Used for drawing vertical portions of L-shaped or Z-shaped paths.
    /// </remarks>
    Vertical,

    /// <summary>
    /// Corner turning from horizontal (left) to down (┐).
    /// </summary>
    /// <remarks>
    /// Box drawing character: Down and Left corner.
    /// </remarks>
    CornerTopRight,

    /// <summary>
    /// Corner turning from up to horizontal (right) (┘).
    /// </summary>
    /// <remarks>
    /// Box drawing character: Up and Left corner.
    /// </remarks>
    CornerBottomRight,

    /// <summary>
    /// Corner turning from horizontal (right) to down (┌).
    /// </summary>
    /// <remarks>
    /// Box drawing character: Down and Right corner.
    /// </remarks>
    CornerTopLeft,

    /// <summary>
    /// Corner turning from up to horizontal (left) (└).
    /// </summary>
    /// <remarks>
    /// Box drawing character: Up and Right corner.
    /// </remarks>
    CornerBottomLeft
}
