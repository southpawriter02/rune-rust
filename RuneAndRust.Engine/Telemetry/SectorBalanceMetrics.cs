using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.Telemetry;

/// <summary>
/// Balance validation metrics for generated Sectors (v0.12)
/// Used for playtesting and balance tuning
/// </summary>
public class SectorBalanceMetrics
{
    // Enemy Density
    public double AverageEnemiesPerRoom { get; set; } = 0;
    public int TotalMinions { get; set; } = 0;
    public int TotalChampions { get; set; } = 0;
    public double ChampionToMinionRatio { get; set; } = 0;

    // Hazard Frequency
    public double RoomsWithHazardsPercent { get; set; } = 0;
    public int TotalHazards { get; set; } = 0;
    public Dictionary<string, int> HazardTypeDistribution { get; set; } = new Dictionary<string, int>();

    // Loot Economy
    public int TotalLootNodes { get; set; } = 0;
    public int EstimatedCogsValue { get; set; } = 0;
    public double AverageCogsPerRoom { get; set; } = 0;

    // Difficulty Curve
    public List<double> DifficultyByDepth { get; set; } = new List<double>();

    // Coherent Glitch Effectiveness
    public int TotalRulesFired { get; set; } = 0;
    public Dictionary<string, int> RuleFireCounts { get; set; } = new Dictionary<string, int>();

    // Player Feedback (manual testing)
    public TimeSpan? AverageClearTime { get; set; } = null;
    public int? PlayerDeaths { get; set; } = null;
    public double? FunRating { get; set; } = null; // Subjective 1-10

    /// <summary>
    /// Calculates balance metrics from a generated dungeon
    /// </summary>
    public static SectorBalanceMetrics CalculateFromDungeon(Dungeon dungeon)
    {
        var metrics = new SectorBalanceMetrics();

        int roomCount = dungeon.TotalRoomCount;
        int totalEnemies = 0;
        int totalHazards = 0;
        int totalLoot = 0;
        int estimatedCogs = 0;
        int roomsWithHazards = 0;

        foreach (var room in dungeon.Rooms.Values)
        {
            // Count enemies
            int roomEnemies = room.DormantProcesses.Count;
            totalEnemies += roomEnemies;

            metrics.TotalMinions += room.DormantProcesses
                .Count(e => e.ThreatLevel <= ThreatLevel.Medium && !e.IsChampion);

            metrics.TotalChampions += room.DormantProcesses
                .Count(e => e.ThreatLevel >= ThreatLevel.High || e.IsChampion);

            // Count hazards
            int roomHazards = room.DynamicHazards.Count;
            totalHazards += roomHazards;

            if (roomHazards > 0)
                roomsWithHazards++;

            foreach (var hazard in room.DynamicHazards)
            {
                string hazardType = hazard.GetType().Name;
                if (!metrics.HazardTypeDistribution.ContainsKey(hazardType))
                {
                    metrics.HazardTypeDistribution[hazardType] = 0;
                }
                metrics.HazardTypeDistribution[hazardType]++;
            }

            // Count loot
            totalLoot += room.LootNodes.Count;
            estimatedCogs += room.LootNodes.Sum(l => l.EstimatedCogsValue);

            // Track Coherent Glitch rules
            metrics.TotalRulesFired += room.CoherentGlitchRulesFired;
        }

        // Calculate averages
        metrics.AverageEnemiesPerRoom = roomCount > 0 ? (double)totalEnemies / roomCount : 0;
        metrics.ChampionToMinionRatio = metrics.TotalMinions > 0 ?
            (double)metrics.TotalChampions / metrics.TotalMinions : 0;

        metrics.TotalHazards = totalHazards;
        metrics.RoomsWithHazardsPercent = roomCount > 0 ?
            (double)roomsWithHazards / roomCount * 100 : 0;

        metrics.TotalLootNodes = totalLoot;
        metrics.EstimatedCogsValue = estimatedCogs;
        metrics.AverageCogsPerRoom = roomCount > 0 ? (double)estimatedCogs / roomCount : 0;

        return metrics;
    }

    /// <summary>
    /// Validates that metrics are within acceptable balance ranges
    /// </summary>
    public List<string> ValidateBalance()
    {
        var issues = new List<string>();

        // Enemy density (target: 2.0-4.0 enemies per room)
        if (AverageEnemiesPerRoom < 2.0)
            issues.Add($"Enemy density too low: {AverageEnemiesPerRoom:F2} < 2.0");
        if (AverageEnemiesPerRoom > 4.0)
            issues.Add($"Enemy density too high: {AverageEnemiesPerRoom:F2} > 4.0");

        // Champion ratio (target: 0.15-0.30)
        if (ChampionToMinionRatio < 0.15)
            issues.Add($"Champion ratio too low: {ChampionToMinionRatio:F2} < 0.15");
        if (ChampionToMinionRatio > 0.30)
            issues.Add($"Champion ratio too high: {ChampionToMinionRatio:F2} > 0.30");

        // Loot economy (target: 50-150 Cogs per room)
        if (AverageCogsPerRoom < 50)
            issues.Add($"Loot economy too poor: {AverageCogsPerRoom:F0} < 50 Cogs/room");
        if (AverageCogsPerRoom > 150)
            issues.Add($"Loot economy too rich: {AverageCogsPerRoom:F0} > 150 Cogs/room");

        return issues;
    }
}
