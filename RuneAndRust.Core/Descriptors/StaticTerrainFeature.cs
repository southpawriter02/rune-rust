namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.2: Static terrain feature model
/// Represents permanent environmental features (cover, obstacles, elevation)
/// Generated from Descriptor_Base_Templates + Thematic_Modifiers
/// </summary>
public class StaticTerrainFeature
{
    /// <summary>
    /// Unique identifier for this feature instance
    /// </summary>
    public int FeatureId { get; set; }

    /// <summary>
    /// Feature name (e.g., "Scorched Support Pillar")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive text for the feature
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base template name (e.g., "Pillar_Base")
    /// </summary>
    public string? BaseTemplateName { get; set; }

    /// <summary>
    /// Modifier name (e.g., "Scorched")
    /// </summary>
    public string? ModifierName { get; set; }

    /// <summary>
    /// Feature archetype (Cover, Obstacle, Tactical)
    /// </summary>
    public string Archetype { get; set; } = string.Empty;

    // Structural Properties
    /// <summary>
    /// Hit points (for destructible features)
    /// </summary>
    public int HP { get; set; }

    /// <summary>
    /// Damage reduction (soak)
    /// </summary>
    public int Soak { get; set; }

    /// <summary>
    /// Whether this feature can be destroyed
    /// </summary>
    public bool IsDestructible { get; set; }

    // Cover Properties
    /// <summary>
    /// Cover quality (None, Light, Heavy)
    /// </summary>
    public CoverQuality CoverQuality { get; set; }

    /// <summary>
    /// Dice penalty for attackers (-2 for Light, -4 for Heavy)
    /// </summary>
    public int CoverBonus { get; set; }

    /// <summary>
    /// Whether this feature blocks line of sight
    /// </summary>
    public bool BlocksLoS { get; set; }

    // Obstacle Properties
    /// <summary>
    /// Whether this feature cannot be crossed
    /// </summary>
    public bool IsImpassable { get; set; }

    /// <summary>
    /// Damage dealt if character falls/enters (e.g., "6d6")
    /// </summary>
    public string? FallDamage { get; set; }

    /// <summary>
    /// Damage type for fall damage (Physical, Fire, etc.)
    /// </summary>
    public string? DamageType { get; set; }

    // Elevation Properties
    /// <summary>
    /// Elevation bonus (e.g., "+1d" for ranged attacks)
    /// </summary>
    public string? ElevationBonus { get; set; }

    /// <summary>
    /// Movement cost to climb/reach this elevation
    /// </summary>
    public int ClimbCost { get; set; }

    // Movement Properties
    /// <summary>
    /// Additional movement cost modifier (for difficult terrain)
    /// </summary>
    public int MovementCostModifier { get; set; }

    // Spatial Properties
    /// <summary>
    /// Number of tiles this feature occupies
    /// </summary>
    public int TilesOccupied { get; set; }

    /// <summary>
    /// Width in tiles (for chasms, rivers, etc.)
    /// </summary>
    public int TilesWidth { get; set; }

    // Tactical Properties
    /// <summary>
    /// Whether this feature divides the battlefield into zones
    /// </summary>
    public bool IsTacticalDivider { get; set; }

    /// <summary>
    /// Biome restriction (if any)
    /// </summary>
    public string? BiomeRestriction { get; set; }

    /// <summary>
    /// Tags for filtering and classification
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Validates that this feature has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Name)) return false;
        if (string.IsNullOrEmpty(Archetype)) return false;
        if (TilesOccupied < 0) return false;

        return true;
    }

    /// <summary>
    /// Gets a tactical summary of this feature
    /// </summary>
    public string GetTacticalSummary()
    {
        var summary = new List<string>();

        if (CoverQuality != CoverQuality.None)
        {
            summary.Add($"{CoverQuality} Cover ({CoverBonus} dice penalty)");
        }

        if (IsImpassable)
        {
            summary.Add("Impassable");
        }

        if (MovementCostModifier > 0)
        {
            summary.Add($"Difficult Terrain (+{MovementCostModifier} movement cost)");
        }

        if (!string.IsNullOrEmpty(ElevationBonus))
        {
            summary.Add($"Elevation Bonus: {ElevationBonus}");
        }

        if (IsDestructible)
        {
            summary.Add($"Destructible (HP: {HP}, Soak: {Soak})");
        }

        return string.Join(", ", summary);
    }
}
