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
    Ruin = 0,

    /// <summary>
    /// Pipes, machinery, rust, and mechanical remnants.
    /// </summary>
    Industrial = 1,

    /// <summary>
    /// Overgrown areas with fungal growth and corrupted life.
    /// </summary>
    Organic = 2,

    /// <summary>
    /// Empty, echoing spaces filled with darkness and absence.
    /// </summary>
    Void = 3
}
