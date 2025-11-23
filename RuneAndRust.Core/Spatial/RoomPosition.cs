namespace RuneAndRust.Core.Spatial;

/// <summary>
/// v0.39.1: Represents a room's position in 3D coordinate space
/// Origin (0, 0, 0) is always the Entry Hall at Ground Level
/// </summary>
public struct RoomPosition : IEquatable<RoomPosition>
{
    /// <summary>
    /// X-Axis: East (+) / West (-)
    /// Unit: 1 = one room space (variable meters based on room size)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y-Axis: North (+) / South (-)
    /// Unit: 1 = one room space
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Z-Axis: Up (+) / Down (-)
    /// Unit: 1 = one vertical layer (~100 meters)
    /// Range: -3 (Deep Roots) to +3 (Canopy)
    /// </summary>
    public int Z { get; set; }

    /// <summary>
    /// Origin point: Entry Hall always at (0, 0, 0)
    /// </summary>
    public static readonly RoomPosition Origin = new RoomPosition(0, 0, 0);

    public RoomPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Calculates Manhattan distance between two positions (ignoring Z)
    /// </summary>
    public int ManhattanDistanceHorizontal(RoomPosition other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }

    /// <summary>
    /// Calculates full 3D Manhattan distance
    /// </summary>
    public int ManhattanDistance3D(RoomPosition other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }

    /// <summary>
    /// Checks if this position is directly above or below another position
    /// </summary>
    public bool IsDirectlyAboveOrBelow(RoomPosition other)
    {
        return X == other.X && Y == other.Y && Z != other.Z;
    }

    /// <summary>
    /// Checks if this position is on the same horizontal plane (same Z)
    /// </summary>
    public bool IsOnSameLevel(RoomPosition other)
    {
        return Z == other.Z;
    }

    /// <summary>
    /// Gets the vertical distance (Z difference) to another position
    /// </summary>
    public int VerticalDistanceTo(RoomPosition other)
    {
        return Math.Abs(Z - other.Z);
    }

    /// <summary>
    /// Checks if this position is adjacent (horizontally) to another position
    /// </summary>
    public bool IsAdjacentHorizontal(RoomPosition other)
    {
        if (Z != other.Z) return false;
        return ManhattanDistanceHorizontal(other) == 1;
    }

    #region Equality and Hashing

    public bool Equals(RoomPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoomPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(RoomPosition left, RoomPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoomPosition left, RoomPosition right)
    {
        return !left.Equals(right);
    }

    #endregion

    #region String Representation

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public string ToCompassString()
    {
        var xDir = X > 0 ? $"{X}E" : X < 0 ? $"{Math.Abs(X)}W" : "";
        var yDir = Y > 0 ? $"{Y}N" : Y < 0 ? $"{Math.Abs(Y)}S" : "";
        var zDir = Z > 0 ? $"{Z}↑" : Z < 0 ? $"{Math.Abs(Z)}↓" : "Ground";

        if (string.IsNullOrEmpty(xDir) && string.IsNullOrEmpty(yDir))
        {
            return zDir;
        }

        return $"{xDir}{yDir} {zDir}".Trim();
    }

    #endregion
}
