namespace RuneAndRust.Engine.Telemetry;

/// <summary>
/// Telemetry data for dungeon generation (v0.12)
/// Tracks performance, rule application, and generation statistics
/// </summary>
public class GenerationTelemetry
{
    // Generation Metadata
    public int Seed { get; set; } = 0;
    public int DungeonId { get; set; } = 0;
    public string BiomeId { get; set; } = string.Empty;
    public DateTime GenerationStartTime { get; set; } = DateTime.UtcNow;
    public DateTime GenerationEndTime { get; set; } = DateTime.UtcNow;

    // Performance Metrics
    public TimeSpan TotalGenerationTime => GenerationEndTime - GenerationStartTime;
    public TimeSpan LayoutGenerationTime { get; set; } = TimeSpan.Zero;
    public TimeSpan PopulationTime { get; set; } = TimeSpan.Zero;
    public TimeSpan CoherentGlitchTime { get; set; } = TimeSpan.Zero;

    // Structure Statistics
    public int RoomCount { get; set; } = 0;
    public int EdgeCount { get; set; } = 0;
    public int BranchNodes { get; set; } = 0;
    public int SecretNodes { get; set; } = 0;

    // Population Statistics
    public int TotalEnemies { get; set; } = 0;
    public int TotalMinions { get; set; } = 0;
    public int TotalChampions { get; set; } = 0;
    public int TotalBosses { get; set; } = 0;
    public int TotalHazards { get; set; } = 0;
    public int TotalStaticTerrain { get; set; } = 0;
    public int TotalLootNodes { get; set; } = 0;
    public int EstimatedTotalCogsValue { get; set; } = 0;

    // Coherent Glitch Statistics
    public int TotalRulesFired { get; set; } = 0;
    public Dictionary<string, int> RuleFireCounts { get; set; } = new Dictionary<string, int>();

    // Warnings and Errors
    public List<string> Warnings { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Was generation successful?
    /// </summary>
    public bool IsSuccessful => Errors.Count == 0 && RoomCount > 0;

    /// <summary>
    /// Did generation meet performance targets? (< 700ms acceptable)
    /// </summary>
    public bool MeetsPerformanceTarget => TotalGenerationTime.TotalMilliseconds < 700;

    /// <summary>
    /// Did generation meet strict performance targets? (< 500ms ideal)
    /// </summary>
    public bool MeetsStrictPerformanceTarget => TotalGenerationTime.TotalMilliseconds < 500;

    /// <summary>
    /// Average enemies per room
    /// </summary>
    public double AverageEnemiesPerRoom => RoomCount > 0 ? (double)TotalEnemies / RoomCount : 0;

    /// <summary>
    /// Champion to minion ratio
    /// </summary>
    public double ChampionToMinionRatio => TotalMinions > 0 ? (double)TotalChampions / TotalMinions : 0;

    /// <summary>
    /// Average Cogs value per room
    /// </summary>
    public double AverageCogsPerRoom => RoomCount > 0 ? (double)EstimatedTotalCogsValue / RoomCount : 0;

    /// <summary>
    /// Rule application rate (% of rooms with at least one rule applied)
    /// </summary>
    public double RuleApplicationRate => RoomCount > 0 ? (double)TotalRulesFired / RoomCount : 0;
}
