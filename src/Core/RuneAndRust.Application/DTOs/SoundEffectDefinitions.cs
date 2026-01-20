namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Definition of a single sound effect.
/// </summary>
/// <remarks>
/// Each effect can have multiple files for variation.
/// </remarks>
public class SoundEffectDefinition
{
    /// <summary>
    /// Gets or sets the effect ID (e.g., "attack-hit").
    /// </summary>
    public string EffectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category (e.g., "combat").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of audio file paths.
    /// </summary>
    public List<string> Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the volume multiplier (0.0-1.0).
    /// </summary>
    public float Volume { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether to randomize file selection.
    /// </summary>
    public bool Randomize { get; set; }
}

/// <summary>
/// Global sound effect settings.
/// </summary>
public class SoundEffectSettings
{
    /// <summary>
    /// Gets or sets the maximum simultaneous sounds.
    /// </summary>
    public int MaxSimultaneous { get; set; } = 8;

    /// <summary>
    /// Gets or sets the default volume.
    /// </summary>
    public float DefaultVolume { get; set; } = 0.8f;
}

/// <summary>
/// Root configuration for sound-effects.json.
/// </summary>
public class SoundEffectsConfig
{
    /// <summary>
    /// Gets or sets the categories dictionary.
    /// </summary>
    public Dictionary<string, CategoryDefinition> Categories { get; set; } = new();

    /// <summary>
    /// Gets or sets the global settings.
    /// </summary>
    public SoundEffectSettings Settings { get; set; } = new();
}

/// <summary>
/// Category with its effects.
/// </summary>
public class CategoryDefinition
{
    /// <summary>
    /// Gets or sets the effects dictionary.
    /// </summary>
    public Dictionary<string, EffectFileConfig> Effects { get; set; } = new();
}

/// <summary>
/// Raw effect config from JSON.
/// </summary>
public class EffectFileConfig
{
    /// <summary>
    /// Gets or sets the list of audio file paths.
    /// </summary>
    public List<string> Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the volume multiplier.
    /// </summary>
    public float Volume { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether to randomize file selection.
    /// </summary>
    public bool Randomize { get; set; }
}
