namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Descriptor pools for biome-themed room descriptions.
/// </summary>
public class BiomeDescriptors
{
    /// <summary>
    /// Atmospheric descriptors (general mood/feeling).
    /// </summary>
    public IReadOnlyList<string> Atmospheric { get; init; } = [];

    /// <summary>
    /// Visual descriptors (what you see).
    /// </summary>
    public IReadOnlyList<string> SensoryVisual { get; init; } = [];

    /// <summary>
    /// Audio descriptors (what you hear).
    /// </summary>
    public IReadOnlyList<string> SensoryAudio { get; init; } = [];

    /// <summary>
    /// Smell descriptors (what you smell).
    /// </summary>
    public IReadOnlyList<string> SensorySmell { get; init; } = [];

    /// <summary>
    /// Surface descriptors (floor, walls, etc.).
    /// </summary>
    public IReadOnlyList<string> Surfaces { get; init; } = [];

    /// <summary>
    /// Lighting descriptors (illumination).
    /// </summary>
    public IReadOnlyList<string> Lighting { get; init; } = [];

    /// <summary>
    /// Gets a random descriptor from the specified category.
    /// </summary>
    public string? GetRandom(string category, Random random)
    {
        var pool = category.ToLowerInvariant() switch
        {
            "atmospheric" => Atmospheric,
            "sensoryvisual" or "visual" => SensoryVisual,
            "sensoryaudio" or "audio" => SensoryAudio,
            "sensorysmell" or "smell" => SensorySmell,
            "surfaces" => Surfaces,
            "lighting" => Lighting,
            _ => null
        };

        if (pool == null || pool.Count == 0)
            return null;

        return pool[random.Next(pool.Count)];
    }

    /// <summary>
    /// Creates empty descriptors.
    /// </summary>
    public static BiomeDescriptors Empty => new();
}
