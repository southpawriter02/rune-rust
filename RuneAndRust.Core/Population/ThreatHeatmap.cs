namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Threat heatmap for sector-wide density visualization
/// Provides debugging and balance analysis tools
/// </summary>
public class ThreatHeatmap
{
    /// <summary>
    /// Threat levels indexed by room ID
    /// </summary>
    public Dictionary<string, RoomThreatData> RoomThreatLevels { get; set; } = new();

    /// <summary>
    /// Average threat level across all rooms
    /// </summary>
    public float AverageThreatLevel { get; set; } = 0f;

    /// <summary>
    /// Maximum threat level in any single room
    /// </summary>
    public int MaxThreatLevel { get; set; } = 0;

    /// <summary>
    /// Gets statistics about room density distribution
    /// </summary>
    public Dictionary<ThreatIntensity, int> GetIntensityDistribution()
    {
        return RoomThreatLevels.Values
            .GroupBy(t => t.Intensity)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets percentage of rooms at each intensity level
    /// </summary>
    public Dictionary<ThreatIntensity, float> GetIntensityPercentages()
    {
        var total = RoomThreatLevels.Count;
        if (total == 0) return new Dictionary<ThreatIntensity, float>();

        return GetIntensityDistribution()
            .ToDictionary(kvp => kvp.Key, kvp => (float)kvp.Value / total * 100f);
    }
}
