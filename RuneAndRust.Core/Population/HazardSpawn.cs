namespace RuneAndRust.Core.Population;

/// <summary>
/// Represents a hazard spawn point for procedural generation
/// </summary>
public class HazardSpawn
{
    public string HazardId { get; set; } = string.Empty;
    public string HazardName { get; set; } = string.Empty;
    public DynamicHazardType HazardType { get; set; } = DynamicHazardType.SteamVent;
    public Vector2? Position { get; set; }
    public int DamageDice { get; set; } = 1;
    public int DamageFlat { get; set; } = 0;
    public float Range { get; set; } = 1.0f;
    public string Description { get; set; } = string.Empty;
}
