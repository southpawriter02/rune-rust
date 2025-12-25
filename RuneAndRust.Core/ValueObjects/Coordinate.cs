namespace RuneAndRust.Core.ValueObjects;

/// <summary>
/// Represents a 3D coordinate position in the game world.
/// Uses readonly record struct for stack allocation and value equality semantics (v0.3.18a).
/// The Z-axis supports vertical navigation (The Deep, Canopy Sea, etc.).
/// </summary>
/// <param name="X">East-West position (East is positive).</param>
/// <param name="Y">North-South position (North is positive).</param>
/// <param name="Z">Vertical position (Up is positive).</param>
public readonly record struct Coordinate(int X, int Y, int Z)
{
    /// <summary>
    /// Gets the origin coordinate (0, 0, 0).
    /// </summary>
    public static Coordinate Origin => new(0, 0, 0);

    /// <summary>
    /// Returns a human-readable string representation.
    /// </summary>
    public override string ToString() => $"({X}, {Y}, {Z})";

    /// <summary>
    /// Creates a new coordinate offset by the specified amounts.
    /// </summary>
    /// <param name="deltaX">X offset.</param>
    /// <param name="deltaY">Y offset.</param>
    /// <param name="deltaZ">Z offset.</param>
    /// <returns>A new coordinate at the offset position.</returns>
    public Coordinate Offset(int deltaX, int deltaY, int deltaZ) =>
        new(X + deltaX, Y + deltaY, Z + deltaZ);
}
