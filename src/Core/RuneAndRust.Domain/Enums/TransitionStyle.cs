namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Style of transition between biomes.
/// </summary>
public enum TransitionStyle
{
    /// <summary>
    /// Gradual blend over multiple rooms.
    /// </summary>
    Gradual,

    /// <summary>
    /// Abrupt change at a single boundary.
    /// </summary>
    Abrupt,

    /// <summary>
    /// Magical portal connecting distant biomes.
    /// </summary>
    Portal,

    /// <summary>
    /// Vertical transition (going deeper/higher).
    /// </summary>
    Vertical
}
