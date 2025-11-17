namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.4: Parameters for procedural generation influenced by territory control
/// Modifies encounter generation, loot, NPCs, and environment based on faction control
/// </summary>
public class SectorGenerationParams
{
    /// <summary>
    /// Filter enemies by faction (e.g., "Undying" for Iron-Banes controlled sectors)
    /// </summary>
    public string? EnemyFactionFilter { get; set; }

    /// <summary>
    /// Enemy density multiplier (1.0 = normal, 1.5 = +50%)
    /// </summary>
    public double EnemyDensityMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Enemy variety multiplier (1.0 = normal, 1.5 = more diverse)
    /// </summary>
    public double EnemyVarietyMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Hazard density multiplier (from HazardDensityModifier)
    /// </summary>
    public double HazardDensityMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Artifact spawn rate multiplier (1.0 = normal, 1.3 = +30%)
    /// </summary>
    public double ArtifactSpawnRate { get; set; } = 1.0;

    /// <summary>
    /// Salvage material spawn rate multiplier
    /// </summary>
    public double SalvageMaterialRate { get; set; } = 1.0;

    /// <summary>
    /// Loot table modifier (e.g., "Anti-Undying_Gear", "Pre-Glitch_Tech")
    /// </summary>
    public string? LootTableModifier { get; set; }

    /// <summary>
    /// Chance to spawn scholar NPCs (0.0 to 1.0)
    /// </summary>
    public double ScholarNPCChance { get; set; } = 0.0;

    /// <summary>
    /// Chance to spawn scavenger NPCs (0.0 to 1.0)
    /// </summary>
    public double ScavengerNPCChance { get; set; } = 0.0;

    /// <summary>
    /// Chance to spawn neutral/independent NPCs (0.0 to 1.0)
    /// </summary>
    public double NeutralNPCChance { get; set; } = 0.0;

    /// <summary>
    /// Merchant price modifier (0.85 = 15% discount, 1.25 = 25% markup)
    /// </summary>
    public double MerchantPriceModifier { get; set; } = 1.0;

    /// <summary>
    /// Environmental storytelling theme (e.g., "Pre-Glitch_History", "War_Torn")
    /// </summary>
    public string? EnvironmentalStorytelling { get; set; }

    /// <summary>
    /// Ambient description for the sector
    /// </summary>
    public string? AmbientDescription { get; set; }
}
