using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Mechanical Hazards",
        "Constructed traps and mechanisms from Pre-Glitch security systems or deliberate sabotage. Pressure plates, dart launchers, and collapsing floors threaten the unwary. Careful observation may reveal trigger mechanisms.")]
    Mechanical = 0,

    /// <summary>
    /// Natural or elemental hazards. Steam vents, lava pools, freezing winds.
    /// </summary>
    [GameDocument(
        "Environmental Hazards",
        "Natural or elemental dangers arising from the environment itself. Steam vents, lava pools, and freezing winds exist independent of any creator. These hazards follow predictable patterns once understood.")]
    Environmental = 1,

    /// <summary>
    /// Living or organic hazards. Spore pods, acid plants, corrupted growths.
    /// </summary>
    [GameDocument(
        "Biological Hazards",
        "Living or organic threats spawned by Blight corruption or natural predation. Spore pods, acid-secreting plants, and corrupted growths react to proximity. Fire often proves effective against organic hazards.")]
    Biological = 2
}
