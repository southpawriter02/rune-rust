namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for descriptor pools loaded from JSON files.
/// </summary>
public class DescriptorConfiguration
{
    /// <summary>
    /// Category of this descriptor set (e.g., "environmental", "combat").
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pools keyed by pool ID.
    /// </summary>
    public IReadOnlyDictionary<string, DescriptorPool> Pools { get; init; } =
        new Dictionary<string, DescriptorPool>();
}

/// <summary>
/// A pool of descriptors with weighted selection.
/// </summary>
public class DescriptorPool
{
    /// <summary>
    /// Pool identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Available descriptors in this pool.
    /// </summary>
    public IReadOnlyList<Descriptor> Descriptors { get; init; } = [];
}

/// <summary>
/// A single descriptor entry with text and selection metadata.
/// </summary>
public class Descriptor
{
    /// <summary>
    /// Descriptor identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The descriptor text to display.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Selection weight (higher = more likely to be chosen).
    /// </summary>
    public int Weight { get; init; } = 10;

    /// <summary>
    /// Tags for filtering (e.g., ["dungeon", "danger"]).
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Themes this descriptor is suitable for.
    /// Empty list means all themes.
    /// </summary>
    public IReadOnlyList<string> Themes { get; init; } = [];

    /// <summary>
    /// Minimum damage percentage for damage-based descriptors.
    /// </summary>
    public double? MinDamagePercent { get; init; }

    /// <summary>
    /// Maximum damage percentage for damage-based descriptors.
    /// </summary>
    public double? MaxDamagePercent { get; init; }

    /// <summary>
    /// Effective weight after theme adjustments (runtime only).
    /// </summary>
    public int? EffectiveWeight { get; set; }

    // ===== v0.0.11b additions =====

    /// <summary>
    /// Weapon types this descriptor applies to (e.g., "sword", "axe", "bow").
    /// Empty list means all weapon types.
    /// </summary>
    public IReadOnlyList<string> WeaponTypes { get; init; } = [];

    /// <summary>
    /// Damage types this descriptor applies to (e.g., "fire", "ice", "physical").
    /// Empty list means all damage types.
    /// </summary>
    public IReadOnlyList<string> DamageTypes { get; init; } = [];

    /// <summary>
    /// Creature categories this descriptor applies to (e.g., "humanoid", "undead").
    /// Empty list means all categories.
    /// </summary>
    public IReadOnlyList<string> CreatureCategories { get; init; } = [];

    /// <summary>
    /// Whether this descriptor is for critical hits only.
    /// </summary>
    public bool CriticalOnly { get; init; }

    /// <summary>
    /// Whether this descriptor is for misses/fumbles only.
    /// </summary>
    public bool MissOnly { get; init; }
}
