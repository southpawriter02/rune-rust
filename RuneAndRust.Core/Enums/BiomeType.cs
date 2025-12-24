using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the environmental atmosphere type for rooms and areas.
/// Influences description modifiers and object spawning.
/// </summary>
public enum BiomeType
{
    /// <summary>
    /// Collapsed structures and debris. Stone, dust, ancient decay.
    /// </summary>
    [GameDocument(
        "Ruin Biome",
        "Collapsed structures and ancient debris define these areas. Stone walls crumble into dust, and the weight of ages presses down upon every surface. Pre-Glitch architecture slowly returns to rubble.")]
    Ruin = 0,

    /// <summary>
    /// Pipes, machinery, rust, and mechanical remnants.
    /// </summary>
    [GameDocument(
        "Industrial Biome",
        "Pipes, machinery, and rust dominate these spaces. The mechanical remnants of Pre-Glitch industry groan and drip with unknown fluids. Salvage opportunities abound, but so do mechanical hazards.")]
    Industrial = 1,

    /// <summary>
    /// Overgrown areas with fungal growth and corrupted life.
    /// </summary>
    [GameDocument(
        "Organic Biome",
        "Corrupted life reclaims these territories. Fungal growths carpet every surface, and Blight-touched vegetation pulses with unwholesome vitality. The air hangs thick with spores and decay.")]
    Organic = 2,

    /// <summary>
    /// Empty, echoing spaces filled with darkness and absence.
    /// </summary>
    [GameDocument(
        "Void Biome",
        "Empty spaces where something essential has been drained away. Light behaves strangely here, and sound seems muffled. Survivors report a sense of profound absence, as if reality itself thins.")]
    Void = 3
}
