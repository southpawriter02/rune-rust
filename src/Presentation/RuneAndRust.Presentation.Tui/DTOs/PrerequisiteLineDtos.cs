// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteLineDtos.cs
// Data transfer objects for prerequisite connection lines.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for a prerequisite connection line between nodes.
/// </summary>
/// <param name="SourceNodeId">The prerequisite (source) node ID.</param>
/// <param name="TargetNodeId">The dependent (target) node ID.</param>
/// <param name="State">The current line state (Satisfied/Unsatisfied).</param>
/// <param name="Path">The calculated path segments forming the line.</param>
/// <remarks>
/// <para>Represents a single prerequisite connection line:</para>
/// <list type="bullet">
///   <item><description>Source is the prerequisite node (must be unlocked first)</description></item>
///   <item><description>Target is the dependent node (requires the source)</description></item>
///   <item><description>State determines visual styling (solid/dashed)</description></item>
///   <item><description>Path contains segments for clearing/redrawing</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var line = new PrerequisiteLineDto(
///     SourceNodeId: "power-strike",
///     TargetNodeId: "cleave",
///     State: LineState.Satisfied,
///     Path: calculatedSegments);
/// </code>
/// </example>
public record PrerequisiteLineDto(
    string SourceNodeId,
    string TargetNodeId,
    LineState State,
    IReadOnlyList<LineSegmentDto> Path);

/// <summary>
/// Data transfer object for a single line segment.
/// </summary>
/// <param name="X">The X coordinate (column) of the segment.</param>
/// <param name="Y">The Y coordinate (row) of the segment.</param>
/// <param name="Direction">The direction of this segment.</param>
/// <param name="Index">The index in the path (used for dashed pattern).</param>
/// <remarks>
/// <para>Represents a single character position in the line path:</para>
/// <list type="bullet">
///   <item><description>X/Y: Screen coordinates for terminal output</description></item>
///   <item><description>Direction: Determines character to render</description></item>
///   <item><description>Index: Used for dashed pattern (odd indices are skipped)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var segment = new LineSegmentDto(
///     X: 15,
///     Y: 10,
///     Direction: LineDirection.Horizontal,
///     Index: 0);
/// </code>
/// </example>
public record LineSegmentDto(
    int X,
    int Y,
    LineDirection Direction,
    int Index);

/// <summary>
/// Represents a point for line path calculations.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
/// <remarks>
/// Value object for path calculation inputs and outputs.
/// </remarks>
/// <example>
/// <code>
/// var start = new LinePoint(10, 5);
/// var end = new LinePoint(20, 5);
/// var path = lineRenderer.CalculatePath(start, end);
/// </code>
/// </example>
public record LinePoint(int X, int Y);

/// <summary>
/// Represents the screen bounds of a rendered node.
/// </summary>
/// <param name="X">The left X coordinate.</param>
/// <param name="Y">The top Y coordinate.</param>
/// <param name="Width">The width in characters.</param>
/// <param name="Height">The height in rows.</param>
/// <remarks>
/// Used for calculating line endpoints at node edges.
/// </remarks>
public record NodeBounds(int X, int Y, int Width, int Height);
