// ═══════════════════════════════════════════════════════════════════════════════
// ShadowPosition.cs
// Immutable value object representing a position with light level data for
// Shadow Step targeting and validation.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a grid position with associated light level information,
/// used for Shadow Step targeting and validation.
/// </summary>
/// <remarks>
/// <para>
/// ShadowPosition combines spatial coordinates with environmental light
/// data to determine whether a position is a valid target for shadow
/// abilities like Shadow Step.
/// </para>
/// <para>
/// A valid shadow target must satisfy all conditions:
/// </para>
/// <list type="bullet">
///   <item><description>Light level is Darkness or DimLight</description></item>
///   <item><description>Position is not occupied by another entity</description></item>
///   <item><description>Position is within the ability's range (6 spaces for Shadow Step)</description></item>
/// </list>
/// <example>
/// <code>
/// var target = ShadowPosition.Create(20, 15, LightLevelType.Darkness);
/// var origin = ShadowPosition.Create(10, 10, LightLevelType.NormalLight);
///
/// target.IsValidShadowTarget() // true (Darkness, unoccupied)
/// target.DistanceTo(origin)    // ~11.18 spaces
/// target.IsWithinRange(origin, 6) // false (too far)
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="LightLevelType"/>
/// <seealso cref="MyrkgengrAbilityId"/>
public sealed record ShadowPosition
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// X coordinate on the combat grid.
    /// </summary>
    public int X { get; private init; }

    /// <summary>
    /// Y coordinate on the combat grid.
    /// </summary>
    public int Y { get; private init; }

    /// <summary>
    /// Light level at this position.
    /// </summary>
    public LightLevelType LightLevel { get; private init; }

    /// <summary>
    /// Whether this position is occupied by another entity.
    /// </summary>
    public bool IsOccupied { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a ShadowPosition with the specified coordinates and light level.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="lightLevel">Light level at this position.</param>
    /// <param name="isOccupied">Whether the position is occupied. Defaults to false.</param>
    /// <returns>A new ShadowPosition instance.</returns>
    public static ShadowPosition Create(
        int x,
        int y,
        LightLevelType lightLevel,
        bool isOccupied = false) => new()
    {
        X = x,
        Y = y,
        LightLevel = lightLevel,
        IsOccupied = isOccupied
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines if this position is a valid Shadow Step target.
    /// A valid target must be in shadow (Darkness or DimLight) and unoccupied.
    /// </summary>
    /// <returns><c>true</c> if the position is a valid shadow target.</returns>
    public bool IsValidShadowTarget() =>
        LightLevel <= LightLevelType.DimLight && !IsOccupied;

    /// <summary>
    /// Calculates the Euclidean distance from this position to another.
    /// </summary>
    /// <param name="other">The other position.</param>
    /// <returns>Distance in grid spaces (Euclidean).</returns>
    public double DistanceTo(ShadowPosition other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Checks whether this position is within the specified range of an origin.
    /// </summary>
    /// <param name="origin">The origin position for the range check.</param>
    /// <param name="range">Maximum range in grid spaces.</param>
    /// <returns><c>true</c> if within range.</returns>
    public bool IsWithinRange(ShadowPosition origin, int range)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentOutOfRangeException.ThrowIfLessThan(range, 0);
        return DistanceTo(origin) <= range;
    }

    /// <summary>
    /// Returns a human-readable representation of this position.
    /// </summary>
    public override string ToString() =>
        $"ShadowPos({X}, {Y}, {LightLevel}, Occupied={IsOccupied})";
}
