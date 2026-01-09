namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Smell intensity levels.
/// </summary>
public enum SmellIntensity
{
    /// <summary>Barely noticeable.</summary>
    Faint,

    /// <summary>Clearly present.</summary>
    Noticeable,

    /// <summary>Dominant odor.</summary>
    Strong,

    /// <summary>Nearly unbearable.</summary>
    Overwhelming
}

/// <summary>
/// Lighting and visibility description.
/// </summary>
/// <param name="Description">The lighting description text.</param>
/// <param name="LightSource">The type of light source (torch, crystal, etc.).</param>
/// <param name="DarknessLevel">The darkness level (dim, bright, etc.).</param>
public readonly record struct LightingDescription(
    string Description,
    string? LightSource,
    string? DarknessLevel);

/// <summary>
/// Layered sound description.
/// </summary>
public readonly record struct SoundDescription
{
    /// <summary>
    /// Distant background sounds (echoes, wind, thunder).
    /// </summary>
    public string? Distant { get; init; }

    /// <summary>
    /// Nearby room-level sounds (dripping, creaking, breathing).
    /// </summary>
    public string? Nearby { get; init; }

    /// <summary>
    /// Immediate close sounds (your heartbeat, nearby creatures).
    /// </summary>
    public string? Immediate { get; init; }

    /// <summary>
    /// Gets the primary sound (first non-null layer).
    /// </summary>
    public string? Primary => Nearby ?? Distant ?? Immediate;

    /// <summary>
    /// Returns true if any sound layers are present.
    /// </summary>
    public bool HasSounds =>
        !string.IsNullOrEmpty(Distant) ||
        !string.IsNullOrEmpty(Nearby) ||
        !string.IsNullOrEmpty(Immediate);

    /// <summary>
    /// Combines all sound layers into a single description.
    /// </summary>
    public string ToFullDescription()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(Distant)) parts.Add(Distant);
        if (!string.IsNullOrEmpty(Nearby)) parts.Add(Nearby);
        if (!string.IsNullOrEmpty(Immediate)) parts.Add(Immediate);
        return string.Join(" ", parts);
    }
}

/// <summary>
/// Smell description with intensity.
/// </summary>
/// <param name="Description">The formatted smell description.</param>
/// <param name="Intensity">The smell intensity level.</param>
/// <param name="SmellType">The type of smell (decay, sulfur, etc.).</param>
public readonly record struct SmellDescription(
    string Description,
    SmellIntensity Intensity,
    string? SmellType);

/// <summary>
/// A complete multi-sense description of an environment.
/// </summary>
/// <remarks>
/// Combines visual, auditory, olfactory, and tactile sensations into
/// a coherent atmospheric description. Each sense can have multiple
/// descriptors that work together to create immersion.
/// </remarks>
public readonly record struct SensoryDescription
{
    /// <summary>
    /// Visual description including lighting and visibility.
    /// </summary>
    public LightingDescription Lighting { get; init; }

    /// <summary>
    /// Layered sound descriptions by distance.
    /// </summary>
    public SoundDescription Sounds { get; init; }

    /// <summary>
    /// Smell description with intensity.
    /// </summary>
    public SmellDescription Smell { get; init; }

    /// <summary>
    /// Temperature sensation description.
    /// </summary>
    public string Temperature { get; init; }

    /// <summary>
    /// Weather condition description if applicable.
    /// </summary>
    public string? Weather { get; init; }

    /// <summary>
    /// Time of day effects if applicable.
    /// </summary>
    public string? TimeOfDay { get; init; }

    /// <summary>
    /// Returns true if any sensory descriptions are present.
    /// </summary>
    public bool HasDescriptions =>
        !string.IsNullOrEmpty(Lighting.Description) ||
        Sounds.HasSounds ||
        !string.IsNullOrEmpty(Smell.Description) ||
        !string.IsNullOrEmpty(Temperature);

    /// <summary>
    /// Combines all sensory descriptions into a narrative paragraph.
    /// </summary>
    /// <param name="maxSenses">Maximum number of senses to include (default 4).</param>
    /// <returns>A combined atmospheric description.</returns>
    public string ToNarrative(int maxSenses = 4)
    {
        var parts = new List<string>();

        // Add lighting first (sets the visual scene)
        if (!string.IsNullOrEmpty(Lighting.Description))
            parts.Add(Lighting.Description);

        // Add primary sound
        if (!string.IsNullOrEmpty(Sounds.Primary))
            parts.Add(Sounds.Primary);

        // Add smell
        if (!string.IsNullOrEmpty(Smell.Description))
            parts.Add(Smell.Description);

        // Add temperature
        if (!string.IsNullOrEmpty(Temperature))
            parts.Add(Temperature);

        // Add weather if present
        if (!string.IsNullOrEmpty(Weather))
            parts.Add(Weather);

        // Limit to max senses
        var selected = parts.Take(maxSenses);

        return string.Join(" ", selected);
    }
}
