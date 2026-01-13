namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of interactive objects that can be described in the environment.
/// </summary>
public enum InteractiveObjectType
{
    /// <summary>A door or gate.</summary>
    Door,

    /// <summary>A container that can be opened.</summary>
    Chest,

    /// <summary>A lever or switch mechanism.</summary>
    Lever,

    /// <summary>A decorative or magical statue.</summary>
    Statue,

    /// <summary>A religious or magical altar.</summary>
    Altar,

    /// <summary>A source of illumination.</summary>
    LightSource,

    /// <summary>A written message or plaque.</summary>
    Inscription,

    /// <summary>A water feature (fountain, pool, well).</summary>
    WaterFeature
}
