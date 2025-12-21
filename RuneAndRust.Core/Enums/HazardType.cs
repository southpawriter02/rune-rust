namespace RuneAndRust.Core.Enums;

/// <summary>
/// Classification of dynamic hazards for visual/thematic grouping (v0.3.3a).
/// Informs renderer styling and AI behavior.
/// </summary>
public enum HazardType
{
    /// <summary>
    /// Constructed traps and mechanisms. Pressure plates, dart traps, collapsing floors.
    /// </summary>
    Mechanical = 0,

    /// <summary>
    /// Natural or elemental hazards. Steam vents, lava pools, freezing winds.
    /// </summary>
    Environmental = 1,

    /// <summary>
    /// Living or organic hazards. Spore pods, acid plants, corrupted growths.
    /// </summary>
    Biological = 2
}
