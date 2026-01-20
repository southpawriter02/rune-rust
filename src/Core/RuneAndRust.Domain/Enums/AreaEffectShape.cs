namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the geometric shapes for area effect abilities.
/// </summary>
public enum AreaEffectShape
{
    /// <summary>No area effect - single target ability.</summary>
    None = 0,

    /// <summary>Circle centered on target point, expands outward by radius.</summary>
    Circle = 1,

    /// <summary>Cone spreading from caster in a direction with angle.</summary>
    Cone = 2,

    /// <summary>Line from caster to target point with optional width.</summary>
    Line = 3,

    /// <summary>Square centered on target point.</summary>
    Square = 4
}
