namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for ambient event pools.
/// </summary>
public class AmbientEventConfiguration
{
    /// <summary>
    /// Available event pools keyed by pool ID.
    /// </summary>
    public IReadOnlyDictionary<string, AmbientEventPool> EventPools { get; init; } =
        new Dictionary<string, AmbientEventPool>();

    /// <summary>
    /// Base probability for events to trigger (0.0 to 1.0).
    /// </summary>
    public float BaseProbability { get; init; } = 0.15f;

    /// <summary>
    /// Minimum seconds between events.
    /// </summary>
    public int CooldownSeconds { get; init; } = 30;
}

/// <summary>
/// A pool of ambient events.
/// </summary>
public class AmbientEventPool
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
    /// Biomes where this pool is valid.
    /// </summary>
    public IReadOnlyList<string> ValidBiomes { get; init; } = [];

    /// <summary>
    /// Trigger types that can activate this pool.
    /// </summary>
    public IReadOnlyList<string> ValidTriggers { get; init; } = [];
}
