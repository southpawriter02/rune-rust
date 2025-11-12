namespace RuneAndRust.Core.Population;

/// <summary>
/// Represents an enemy spawn point in procedurally generated Sectors (v0.11)
/// "Dormant Process" = Aethelgard's corrupted automatons awaiting activation
/// </summary>
public class DormantProcess
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProcessType { get; set; } = string.Empty; // "rusted_servitor", "rust_horror", etc.
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Low;

    // Spawn Properties
    public Vector2? SpawnPosition { get; set; } = null; // Tactical positioning
    public bool IsChampion { get; set; } = false; // Elite variant with enhanced stats

    // Behavioral Notes (v0.12 Coherent Glitch)
    public string? BehaviorNote { get; set; } = null; // Environmental storytelling context

    // Enemy Data (instantiated at runtime)
    public EnemyType? AssociatedEnemyType { get; set; } = null;
}

/// <summary>
/// Threat level classification for enemies (v0.11)
/// </summary>
public enum ThreatLevel
{
    Minimal = 0,    // Tutorial/decorative enemies
    Low = 1,        // Tier 0: Minions (Rusted Servitors, Scrap Hounds)
    Medium = 2,     // Tier 1: Standard threats (Rust Horrors, Construction Haulers)
    High = 3,       // Tier 2: Champions/Elites
    Boss = 4        // Boss encounters
}

/// <summary>
/// Simple 2D vector for tactical positioning (v0.12)
/// </summary>
public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float DistanceTo(Vector2 other)
    {
        float dx = other.X - X;
        float dy = other.Y - Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
}
