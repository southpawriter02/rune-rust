namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents position coordinates for UI layout of ability tree nodes.
/// </summary>
/// <remarks>
/// <para>
/// NodePosition is a simple immutable value object that holds X and Y coordinates
/// for positioning nodes within the ability tree visualization.
/// </para>
/// <para>
/// The coordinate system:
/// <list type="bullet">
///   <item><description>X represents the horizontal position (column) within a branch</description></item>
///   <item><description>Y represents the vertical position (tier/row) within a branch</description></item>
/// </list>
/// </para>
/// <para>
/// Typical layout:
/// <code>
///   Y=0: Root/placeholder
///   Y=1: Tier 1 nodes
///   Y=2: Tier 2 nodes
///   etc.
/// </code>
/// </para>
/// </remarks>
public readonly record struct NodePosition
{
    /// <summary>
    /// Gets the X coordinate (horizontal position/column).
    /// </summary>
    /// <value>The horizontal position within the tree branch layout.</value>
    public int X { get; }

    /// <summary>
    /// Gets the Y coordinate (vertical position/row).
    /// </summary>
    /// <value>The vertical position, typically corresponding to the node's tier.</value>
    public int Y { get; }

    /// <summary>
    /// Creates a new NodePosition with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position (column).</param>
    /// <param name="y">The vertical position (row/tier).</param>
    public NodePosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the origin position (0, 0).
    /// </summary>
    public static NodePosition Origin => new(0, 0);

    /// <summary>
    /// Creates a NodePosition from the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position.</param>
    /// <param name="y">The vertical position.</param>
    /// <returns>A new NodePosition instance.</returns>
    public static NodePosition At(int x, int y) => new(x, y);

    /// <summary>
    /// Returns a string representation of this position in (X, Y) format.
    /// </summary>
    /// <returns>A string in the format "(X, Y)".</returns>
    public override string ToString() => $"({X}, {Y})";
}
