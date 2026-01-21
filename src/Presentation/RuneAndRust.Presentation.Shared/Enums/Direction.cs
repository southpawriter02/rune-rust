// ═══════════════════════════════════════════════════════════════════════════════
// Direction.cs
// Direction enum for grid coordinate calculations.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Represents cardinal and intercardinal directions for grid-based navigation.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="GridUtilities.GetDirection"/> to indicate the direction
/// from one grid position to another.
/// </para>
/// </remarks>
public enum Direction
{
    /// <summary>No direction (same position).</summary>
    None,

    /// <summary>Up / North (Y decreases).</summary>
    North,

    /// <summary>Up-Right / North-East diagonal.</summary>
    NorthEast,

    /// <summary>Right / East (X increases).</summary>
    East,

    /// <summary>Down-Right / South-East diagonal.</summary>
    SouthEast,

    /// <summary>Down / South (Y increases).</summary>
    South,

    /// <summary>Down-Left / South-West diagonal.</summary>
    SouthWest,

    /// <summary>Left / West (X decreases).</summary>
    West,

    /// <summary>Up-Left / North-West diagonal.</summary>
    NorthWest
}
