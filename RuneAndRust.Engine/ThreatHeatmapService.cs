using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.3: Threat heatmap generator
/// Creates visualization data for debugging and balance analysis
/// </summary>
public class ThreatHeatmapService
{
    private static readonly ILogger _log = Log.ForContext<ThreatHeatmapService>();

    /// <summary>
    /// Generates threat heatmap from a sector and its population plan
    /// </summary>
    /// <param name="dungeon">Dungeon/sector to analyze</param>
    /// <param name="plan">Population plan with allocations</param>
    /// <returns>Threat heatmap with intensity data</returns>
    public ThreatHeatmap GenerateHeatmap(Dungeon dungeon, SectorPopulationPlan plan)
    {
        if (dungeon == null || plan == null)
        {
            _log.Warning("Cannot generate heatmap: dungeon or plan is null");
            return new ThreatHeatmap();
        }

        var heatmap = new ThreatHeatmap();

        foreach (var room in dungeon.Rooms.Values)
        {
            if (!plan.RoomAllocations.TryGetValue(room.RoomId, out var allocation))
            {
                _log.Debug("Room {RoomId} has no allocation, skipping", room.RoomId);
                continue;
            }

            var threatLevel = allocation.AllocatedEnemies + allocation.AllocatedHazards;

            heatmap.RoomThreatLevels[room.RoomId] = new RoomThreatData
            {
                RoomId = room.RoomId,
                Position = room.Position,
                TotalThreats = threatLevel,
                Enemies = allocation.AllocatedEnemies,
                Hazards = allocation.AllocatedHazards,
                Intensity = CalculateIntensity(threatLevel)
            };
        }

        // Calculate statistics
        if (heatmap.RoomThreatLevels.Count > 0)
        {
            heatmap.AverageThreatLevel = (float)heatmap.RoomThreatLevels.Values
                .Average(t => t.TotalThreats);

            heatmap.MaxThreatLevel = heatmap.RoomThreatLevels.Values
                .Max(t => t.TotalThreats);
        }

        _log.Information(
            "Threat heatmap generated: {Rooms} rooms, avg={Average:F2} threats/room, max={Max} threats",
            heatmap.RoomThreatLevels.Count,
            heatmap.AverageThreatLevel,
            heatmap.MaxThreatLevel);

        return heatmap;
    }

    /// <summary>
    /// Calculates threat intensity from threat count
    /// </summary>
    private ThreatIntensity CalculateIntensity(int threatCount)
    {
        return threatCount switch
        {
            0 => ThreatIntensity.None,
            1 or 2 => ThreatIntensity.Low,
            3 or 4 => ThreatIntensity.Medium,
            5 or 6 or 7 => ThreatIntensity.High,
            _ => ThreatIntensity.Extreme
        };
    }

    /// <summary>
    /// Logs heatmap statistics for debugging
    /// </summary>
    public void LogHeatmapStatistics(ThreatHeatmap heatmap, string sectorId)
    {
        if (heatmap == null || heatmap.RoomThreatLevels.Count == 0)
        {
            _log.Warning("Cannot log heatmap: no data");
            return;
        }

        var intensityDist = heatmap.GetIntensityDistribution();
        var intensityPct = heatmap.GetIntensityPercentages();

        _log.Information(
            "Threat Heatmap for Sector {SectorId}:\n" +
            "  Average Threat Level: {Average:F2} threats/room\n" +
            "  Maximum Threat Level: {Max} threats\n" +
            "  Intensity Distribution:\n" +
            "    - None (0 threats):      {None,3} rooms ({NonePct,5:F1}%)\n" +
            "    - Low (1-2 threats):     {Low,3} rooms ({LowPct,5:F1}%)\n" +
            "    - Medium (3-4 threats):  {Medium,3} rooms ({MediumPct,5:F1}%)\n" +
            "    - High (5-7 threats):    {High,3} rooms ({HighPct,5:F1}%)\n" +
            "    - Extreme (8+ threats):  {Extreme,3} rooms ({ExtremePct,5:F1}%)",
            sectorId,
            heatmap.AverageThreatLevel,
            heatmap.MaxThreatLevel,
            intensityDist.GetValueOrDefault(ThreatIntensity.None, 0),
            intensityPct.GetValueOrDefault(ThreatIntensity.None, 0),
            intensityDist.GetValueOrDefault(ThreatIntensity.Low, 0),
            intensityPct.GetValueOrDefault(ThreatIntensity.Low, 0),
            intensityDist.GetValueOrDefault(ThreatIntensity.Medium, 0),
            intensityPct.GetValueOrDefault(ThreatIntensity.Medium, 0),
            intensityDist.GetValueOrDefault(ThreatIntensity.High, 0),
            intensityPct.GetValueOrDefault(ThreatIntensity.High, 0),
            intensityDist.GetValueOrDefault(ThreatIntensity.Extreme, 0),
            intensityPct.GetValueOrDefault(ThreatIntensity.Extreme, 0));
    }
}
