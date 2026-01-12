namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks player progress and discoveries within biomes.
/// </summary>
public class BiomeProgress
{
    private readonly Dictionary<string, BiomeStats> _stats = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all discovered biome IDs.
    /// </summary>
    public IReadOnlyList<string> DiscoveredBiomes => _stats.Keys.ToList();

    /// <summary>
    /// Checks if a biome has been discovered.
    /// </summary>
    public bool HasDiscovered(string biomeId) => _stats.ContainsKey(biomeId);

    /// <summary>
    /// Gets the discovery time for a biome.
    /// </summary>
    public DateTime? GetDiscoveryTime(string biomeId) =>
        _stats.TryGetValue(biomeId, out var stats) ? stats.DiscoveredAt : null;

    /// <summary>
    /// Gets the rooms visited count for a biome.
    /// </summary>
    public int GetRoomsVisited(string biomeId) =>
        _stats.TryGetValue(biomeId, out var stats) ? stats.RoomsVisited : 0;

    /// <summary>
    /// Gets the monsters defeated count for a biome.
    /// </summary>
    public int GetMonstersDefeated(string biomeId) =>
        _stats.TryGetValue(biomeId, out var stats) ? stats.MonstersDefeated : 0;

    /// <summary>
    /// Gets the deepest depth reached in a biome.
    /// </summary>
    public int GetDeepestDepth(string biomeId) =>
        _stats.TryGetValue(biomeId, out var stats) ? stats.DeepestDepth : 0;

    /// <summary>
    /// Discovers a biome, returning true if it's a new discovery.
    /// </summary>
    public bool DiscoverBiome(string biomeId)
    {
        if (_stats.ContainsKey(biomeId))
            return false;

        _stats[biomeId] = new BiomeStats { DiscoveredAt = DateTime.UtcNow };
        return true;
    }

    /// <summary>
    /// Records a room visit in a biome, discovering it if necessary.
    /// </summary>
    public bool RecordRoomVisit(string biomeId, int depth)
    {
        var isNew = DiscoverBiome(biomeId);
        var stats = _stats[biomeId];
        stats.RoomsVisited++;
        stats.DeepestDepth = Math.Max(stats.DeepestDepth, depth);
        return isNew;
    }

    /// <summary>
    /// Records a monster defeat in a biome.
    /// </summary>
    public void RecordMonsterDefeat(string biomeId)
    {
        if (!_stats.ContainsKey(biomeId))
            DiscoverBiome(biomeId);
        _stats[biomeId].MonstersDefeated++;
    }

    private class BiomeStats
    {
        public DateTime DiscoveredAt { get; set; }
        public int RoomsVisited { get; set; }
        public int MonstersDefeated { get; set; }
        public int DeepestDepth { get; set; }
    }
}
