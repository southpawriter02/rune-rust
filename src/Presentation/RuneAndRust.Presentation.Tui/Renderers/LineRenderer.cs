// ═══════════════════════════════════════════════════════════════════════════════
// LineRenderer.cs
// Renders prerequisite lines with state-specific styling.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders prerequisite lines with state-specific styling.
/// </summary>
/// <remarks>
/// <para>Provides line characters, colors, and path calculation for
/// drawing connection lines between ability tree nodes.</para>
/// <para>Configuration is loaded from <c>config/prerequisite-lines.json</c>.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new LineRenderer(config, logger);
/// var color = renderer.GetLineColor(LineState.Satisfied); // Returns Green
/// var path = renderer.CalculatePath(start, end);
/// </code>
/// </example>
public class LineRenderer
{
    private readonly PrerequisiteLinesConfig _config;
    private readonly ILogger<LineRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new LineRenderer with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration for line display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public LineRenderer(
        IOptions<PrerequisiteLinesConfig>? config = null,
        ILogger<LineRenderer>? logger = null)
    {
        _config = config?.Value ?? PrerequisiteLinesConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "LineRenderer initialized with satisfied color {SatisfiedColor}, unsatisfied color {UnsatisfiedColor}",
            _config.SatisfiedLineColor,
            _config.UnsatisfiedLineColor);
    }

    // ═══════════════════════════════════════════════════════════════
    // LINE CHARACTER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the line character for a given direction.
    /// </summary>
    /// <param name="direction">The line direction.</param>
    /// <returns>The character to use for the line segment.</returns>
    /// <remarks>
    /// <para>Default characters:</para>
    /// <list type="bullet">
    ///   <item><description>Horizontal: ─</description></item>
    ///   <item><description>Vertical: │</description></item>
    ///   <item><description>Corners: ┐ ┘ ┌ └</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var character = renderer.GetLineCharacter(LineDirection.Horizontal);
    /// // Returns: '─'
    /// </code>
    /// </example>
    public char GetLineCharacter(LineDirection direction)
    {
        var character = direction switch
        {
            LineDirection.Horizontal => _config.HorizontalLineCharacter,
            LineDirection.Vertical => _config.VerticalLineCharacter,
            LineDirection.CornerTopRight => _config.CornerTopRightCharacter,
            LineDirection.CornerBottomRight => _config.CornerBottomRightCharacter,
            LineDirection.CornerTopLeft => _config.CornerTopLeftCharacter,
            LineDirection.CornerBottomLeft => _config.CornerBottomLeftCharacter,
            _ => _config.HorizontalLineCharacter
        };

        _logger?.LogDebug(
            "GetLineCharacter for direction {Direction}: '{Character}'",
            direction, character);

        return character;
    }

    // ═══════════════════════════════════════════════════════════════
    // LINE COLOR METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the color for a line state.
    /// </summary>
    /// <param name="state">The line state.</param>
    /// <returns>The console color for the line.</returns>
    /// <remarks>
    /// <para>Default colors:</para>
    /// <list type="bullet">
    ///   <item><description>Satisfied: Green</description></item>
    ///   <item><description>Unsatisfied: DarkGray</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = renderer.GetLineColor(LineState.Satisfied);
    /// // Returns: ConsoleColor.Green
    /// </code>
    /// </example>
    public ConsoleColor GetLineColor(LineState state)
    {
        var color = state switch
        {
            LineState.Satisfied => _config.SatisfiedLineColor,
            LineState.Unsatisfied => _config.UnsatisfiedLineColor,
            _ => _config.UnsatisfiedLineColor
        };

        _logger?.LogDebug(
            "GetLineColor for state {State}: {Color}",
            state, color);

        return color;
    }

    // ═══════════════════════════════════════════════════════════════
    // PATH CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the path between two points.
    /// </summary>
    /// <param name="start">The starting point.</param>
    /// <param name="end">The ending point.</param>
    /// <returns>A sequence of line segments forming the path.</returns>
    /// <remarks>
    /// <para>For nodes at the same Y position, draws a straight horizontal line.</para>
    /// <para>For nodes at different Y positions, draws a path with a vertical segment.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var start = new LinePoint(10, 5);
    /// var end = new LinePoint(20, 5);
    /// var path = renderer.CalculatePath(start, end);
    /// // Returns 10 horizontal segments
    /// </code>
    /// </example>
    public IEnumerable<LineSegmentDto> CalculatePath(LinePoint start, LinePoint end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var segments = new List<LineSegmentDto>();
        var index = 0;

        _logger?.LogDebug(
            "CalculatePath from ({StartX}, {StartY}) to ({EndX}, {EndY})",
            start.X, start.Y, end.X, end.Y);

        if (start.Y == end.Y)
        {
            // Simple horizontal line
            for (var x = start.X; x < end.X; x++)
            {
                segments.Add(new LineSegmentDto(
                    X: x,
                    Y: start.Y,
                    Direction: LineDirection.Horizontal,
                    Index: index++));
            }

            _logger?.LogDebug(
                "Created straight horizontal path with {Count} segments",
                segments.Count);
        }
        else
        {
            // Path with vertical segment
            var midX = start.X + ((end.X - start.X) / 2);

            // First horizontal segment
            for (var x = start.X; x < midX; x++)
            {
                segments.Add(new LineSegmentDto(
                    X: x,
                    Y: start.Y,
                    Direction: LineDirection.Horizontal,
                    Index: index++));
            }

            // Corner from horizontal to vertical
            var verticalDirection = end.Y > start.Y
                ? LineDirection.CornerBottomRight
                : LineDirection.CornerTopRight;

            segments.Add(new LineSegmentDto(
                X: midX,
                Y: start.Y,
                Direction: verticalDirection,
                Index: index++));

            // Vertical segment
            var minY = Math.Min(start.Y, end.Y);
            var maxY = Math.Max(start.Y, end.Y);

            for (var y = minY + 1; y < maxY; y++)
            {
                segments.Add(new LineSegmentDto(
                    X: midX,
                    Y: y,
                    Direction: LineDirection.Vertical,
                    Index: index++));
            }

            // Corner from vertical to horizontal
            var cornerDirection = end.Y > start.Y
                ? LineDirection.CornerBottomLeft
                : LineDirection.CornerTopLeft;

            segments.Add(new LineSegmentDto(
                X: midX,
                Y: end.Y,
                Direction: cornerDirection,
                Index: index++));

            // Final horizontal segment
            for (var x = midX + 1; x < end.X; x++)
            {
                segments.Add(new LineSegmentDto(
                    X: x,
                    Y: end.Y,
                    Direction: LineDirection.Horizontal,
                    Index: index++));
            }

            _logger?.LogDebug(
                "Created L-shaped path with {Count} segments (vertical from Y={MinY} to Y={MaxY})",
                segments.Count, minY, maxY);
        }

        return segments;
    }

    /// <summary>
    /// Formats a connection point character for a node edge.
    /// </summary>
    /// <param name="direction">The direction the line exits/enters.</param>
    /// <returns>The connection point character.</returns>
    public string FormatConnectionPoint(LineDirection direction)
    {
        return _config.ConnectionPointCharacter.ToString();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The prerequisite lines configuration.</returns>
    public PrerequisiteLinesConfig GetConfig() => _config;

    /// <summary>
    /// Gets the character for satisfied lines.
    /// </summary>
    public char GetSatisfiedLineCharacter() => _config.SatisfiedLineCharacter;

    /// <summary>
    /// Gets the character for unsatisfied lines.
    /// </summary>
    public char GetUnsatisfiedLineCharacter() => _config.UnsatisfiedLineCharacter;
}
