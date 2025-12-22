namespace RuneAndRust.Core.Entities;

/// <summary>
/// Tracks what elements have been spawned in a room during population.
/// Used by ElementSpawnEvaluator to enforce dependency-based spawn rules.
/// </summary>
public class SpawnContext
{
    /// <summary>
    /// Gets or sets the list of enemy types spawned in the current room.
    /// Example: ["RustHorror", "RustedServitor"]
    /// </summary>
    public List<string> SpawnedEnemyTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of hazard types spawned in the current room.
    /// Example: ["SteamVent", "UnstableCeiling"]
    /// </summary>
    public List<string> SpawnedHazardTypes { get; set; } = new();
}
