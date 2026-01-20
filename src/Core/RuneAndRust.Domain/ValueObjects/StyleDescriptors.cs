namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Descriptors for architectural style elements.
/// </summary>
public class StyleDescriptors
{
    /// <summary>
    /// Gets wall descriptors.
    /// </summary>
    public IReadOnlyList<string> Walls { get; init; } = [];

    /// <summary>
    /// Gets floor descriptors.
    /// </summary>
    public IReadOnlyList<string> Floors { get; init; } = [];

    /// <summary>
    /// Gets ceiling descriptors.
    /// </summary>
    public IReadOnlyList<string> Ceilings { get; init; } = [];

    /// <summary>
    /// Gets passage/doorway descriptors.
    /// </summary>
    public IReadOnlyList<string> Passages { get; init; } = [];

    /// <summary>
    /// Gets decoration descriptors.
    /// </summary>
    public IReadOnlyList<string> Decorations { get; init; } = [];

    /// <summary>
    /// Gets a random descriptor from the specified category.
    /// </summary>
    public string? GetRandom(string category, Random random)
    {
        var pool = category.ToLowerInvariant() switch
        {
            "walls" => Walls,
            "floors" => Floors,
            "ceilings" => Ceilings,
            "passages" => Passages,
            "decorations" => Decorations,
            _ => []
        };

        return pool.Count > 0 ? pool[random.Next(pool.Count)] : null;
    }

    /// <summary>
    /// Default empty descriptors.
    /// </summary>
    public static StyleDescriptors Empty => new();

    /// <summary>
    /// Creates rough-hewn stone style descriptors.
    /// </summary>
    public static StyleDescriptors RoughHewn => new()
    {
        Walls = ["rough-cut stone walls", "crudely carved rock face", "uneven stone surfaces"],
        Floors = ["uneven flagstones", "packed earth with stone fragments", "worn cobblestones"],
        Ceilings = ["low rough ceiling", "jagged rock overhead", "supported by crude pillars"],
        Passages = ["narrow opening", "rough-hewn archway", "cramped tunnel"],
        Decorations = ["crude torch sconce", "piled rubble", "ancient tool marks"]
    };

    /// <summary>
    /// Creates ornate temple style descriptors.
    /// </summary>
    public static StyleDescriptors OrnateTemple => new()
    {
        Walls = ["carved relief panels", "polished marble columns", "gilded inscriptions"],
        Floors = ["intricate mosaic tiles", "polished granite slabs", "ceremonial patterns"],
        Ceilings = ["vaulted arches", "painted celestial scenes", "golden chandeliers"],
        Passages = ["grand archway", "ornate double doors", "ceremonial gate"],
        Decorations = ["sacred altar", "devotional statue", "eternal flame brazier"]
    };
}
