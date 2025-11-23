using RuneAndRust.Core.Spatial;

namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Threat level data for a single room in the heatmap
/// Used for visualization and balance analysis
/// </summary>
public class ThreatLevel
{
    /// <summary>
    /// Room identifier
    /// </summary>
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// 3D position of the room
    /// </summary>
    public RoomPosition Position { get; set; } = RoomPosition.Origin;

    /// <summary>
    /// Total threat count (enemies + hazards)
    /// </summary>
    public int TotalThreats { get; set; } = 0;

    /// <summary>
    /// Number of enemies in this room
    /// </summary>
    public int Enemies { get; set; } = 0;

    /// <summary>
    /// Number of hazards in this room
    /// </summary>
    public int Hazards { get; set; } = 0;

    /// <summary>
    /// Threat intensity classification
    /// </summary>
    public ThreatIntensity Intensity { get; set; } = ThreatIntensity.None;
}
