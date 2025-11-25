namespace RuneAndRust.Core.Population;

/// <summary>
/// Represents an enemy spawn point for procedural generation
/// </summary>
public class EnemySpawn
{
    public string EnemyId { get; set; } = string.Empty;
    public string EnemyName { get; set; } = string.Empty;
    public EnemyType EnemyType { get; set; } = EnemyType.CorruptedServitor;
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Low;
    public Vector2? Position { get; set; }
    public bool IsChampion { get; set; } = false;
    public string? BehaviorNote { get; set; }
}
