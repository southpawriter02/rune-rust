using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Defines a functional chamber variant with specialized purpose descriptors.
/// Room functions provide additional context and flavor for chamber-type rooms
/// (e.g., Pumping Station, Forge Hall, Research Lab).
/// </summary>
public class RoomFunction : IEntity
{
    public Guid Id { get; private set; }

    /// <summary>The function name (e.g., "Pumping Station").</summary>
    public string FunctionName { get; private set; }

    /// <summary>Detailed description of the room's function.</summary>
    public string FunctionDetail { get; private set; }

    /// <summary>Selection weight for random function selection.</summary>
    public int Weight { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private readonly List<Biome> _biomeAffinities = [];

    /// <summary>Biomes where this function is commonly found.</summary>
    public IReadOnlyList<Biome> BiomeAffinities => _biomeAffinities.AsReadOnly();

    private RoomFunction()
    {
        FunctionName = null!;
        FunctionDetail = null!;
    } // For EF Core

    public RoomFunction(
        string functionName,
        string functionDetail,
        IEnumerable<Biome>? biomeAffinities = null,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(functionName))
            throw new ArgumentException("Function name cannot be empty", nameof(functionName));
        if (string.IsNullOrWhiteSpace(functionDetail))
            throw new ArgumentException("Function detail cannot be empty", nameof(functionDetail));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        FunctionName = functionName;
        FunctionDetail = functionDetail;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;

        if (biomeAffinities != null)
        {
            _biomeAffinities.AddRange(biomeAffinities);
        }
    }

    /// <summary>
    /// Constructor for seeding with known Id.
    /// </summary>
    public RoomFunction(
        Guid id,
        string functionName,
        string functionDetail,
        IEnumerable<Biome>? biomeAffinities = null,
        int weight = 1)
        : this(functionName, functionDetail, biomeAffinities, weight)
    {
        Id = id;
    }

    /// <summary>
    /// Checks if this function has affinity for the given biome.
    /// Returns true if the biome is in the affinity list or if the list is empty (universal).
    /// </summary>
    public bool HasAffinityFor(Biome biome) =>
        _biomeAffinities.Count == 0 || _biomeAffinities.Contains(biome);

    /// <summary>
    /// Adds a biome affinity.
    /// </summary>
    public void AddBiomeAffinity(Biome biome)
    {
        if (!_biomeAffinities.Contains(biome))
            _biomeAffinities.Add(biome);
    }

    // Factory methods for common function types

    /// <summary>
    /// Creates a utility/infrastructure function.
    /// </summary>
    public static RoomFunction CreateUtility(
        string name,
        string detail,
        params Biome[] biomes) =>
        new(name, detail, biomes, weight: 2);

    /// <summary>
    /// Creates a combat-focused function.
    /// </summary>
    public static RoomFunction CreateCombat(
        string name,
        string detail,
        params Biome[] biomes) =>
        new(name, detail, biomes, weight: 1);

    /// <summary>
    /// Creates a universal function (any biome).
    /// </summary>
    public static RoomFunction CreateUniversal(
        string name,
        string detail) =>
        new(name, detail, null, weight: 1);

    public override string ToString() =>
        _biomeAffinities.Count > 0
            ? $"{FunctionName} ({string.Join(", ", _biomeAffinities)})"
            : $"{FunctionName} (Universal)";
}
