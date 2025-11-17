using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.2: Feature mechanics definition for static terrain
/// Contains all mechanical properties for cover, obstacles, elevation
/// </summary>
public class FeatureMechanics
{
    // Structural Properties
    public int HP { get; set; }
    public int Soak { get; set; }
    public bool Destructible { get; set; }

    // Cover Properties
    public string? CoverQuality { get; set; }
    public int CoverBonus { get; set; }
    public bool BlocksLoS { get; set; }

    // Obstacle Properties
    public bool Impassable { get; set; }
    public string? FallDamage { get; set; }
    public string? DamageType { get; set; }
    public bool RequiresAcrobatics { get; set; }

    // Elevation Properties
    public string? ElevationBonus { get; set; }
    public string? AppliesTo { get; set; }
    public int ClimbCost { get; set; }
    public bool RequiresCheck { get; set; }

    // Movement Properties
    public int MovementCostModifier { get; set; }

    // Spatial Properties
    public int TilesOccupied { get; set; }
    public int TilesWidth { get; set; }

    // Tactical Properties
    public bool TacticalDivider { get; set; }
    public bool ProvidesCover { get; set; }

    /// <summary>
    /// Creates a deep copy of the mechanics
    /// </summary>
    public FeatureMechanics Clone()
    {
        return new FeatureMechanics
        {
            HP = this.HP,
            Soak = this.Soak,
            Destructible = this.Destructible,
            CoverQuality = this.CoverQuality,
            CoverBonus = this.CoverBonus,
            BlocksLoS = this.BlocksLoS,
            Impassable = this.Impassable,
            FallDamage = this.FallDamage,
            DamageType = this.DamageType,
            RequiresAcrobatics = this.RequiresAcrobatics,
            ElevationBonus = this.ElevationBonus,
            AppliesTo = this.AppliesTo,
            ClimbCost = this.ClimbCost,
            RequiresCheck = this.RequiresCheck,
            MovementCostModifier = this.MovementCostModifier,
            TilesOccupied = this.TilesOccupied,
            TilesWidth = this.TilesWidth,
            TacticalDivider = this.TacticalDivider,
            ProvidesCover = this.ProvidesCover
        };
    }

    /// <summary>
    /// Serializes mechanics to JSON string
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserializes mechanics from JSON string
    /// </summary>
    public static FeatureMechanics? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<FeatureMechanics>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
    }
}

/// <summary>
/// v0.38.2: Hazard mechanics definition for dynamic hazards
/// Contains all mechanical properties for damage, activation, status effects
/// </summary>
public class HazardMechanics
{
    // Damage Properties
    public string Damage { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;

    // Activation Properties
    public int ActivationFrequency { get; set; }
    public string? ActivationType { get; set; }
    public int ActivationRange { get; set; }
    public string? ActivationTiming { get; set; }

    // Area Properties
    public string? AreaPattern { get; set; }
    public int TilesAffected { get; set; }

    // Status Effect Properties
    public List<object>? StatusEffect { get; set; }
    public float StatusEffectChance { get; set; }

    // Special Properties
    public bool WarningTurn { get; set; }
    public bool OneTime { get; set; }
    public string? CreatesTerrain { get; set; }
    public List<string>? Triggers { get; set; }
    public List<string>? EnhancedBy { get; set; }

    // Persistent Properties
    public float SpreadChance { get; set; }
    public int AccuracyPenalty { get; set; }
    public bool Stacks { get; set; }

    // Special Modifiers
    public int AmbientHeatRange { get; set; }
    public string? AmbientHeatDamage { get; set; }
    public int ProximityStress { get; set; }
    public int ProximityRange { get; set; }
    public bool Unstable { get; set; }

    /// <summary>
    /// Creates a deep copy of the mechanics
    /// </summary>
    public HazardMechanics Clone()
    {
        return new HazardMechanics
        {
            Damage = this.Damage,
            DamageType = this.DamageType,
            ActivationFrequency = this.ActivationFrequency,
            ActivationType = this.ActivationType,
            ActivationRange = this.ActivationRange,
            ActivationTiming = this.ActivationTiming,
            AreaPattern = this.AreaPattern,
            TilesAffected = this.TilesAffected,
            StatusEffect = this.StatusEffect != null ? new List<object>(this.StatusEffect) : null,
            StatusEffectChance = this.StatusEffectChance,
            WarningTurn = this.WarningTurn,
            OneTime = this.OneTime,
            CreatesTerrain = this.CreatesTerrain,
            Triggers = this.Triggers != null ? new List<string>(this.Triggers) : null,
            EnhancedBy = this.EnhancedBy != null ? new List<string>(this.EnhancedBy) : null,
            SpreadChance = this.SpreadChance,
            AccuracyPenalty = this.AccuracyPenalty,
            Stacks = this.Stacks,
            AmbientHeatRange = this.AmbientHeatRange,
            AmbientHeatDamage = this.AmbientHeatDamage,
            ProximityStress = this.ProximityStress,
            ProximityRange = this.ProximityRange,
            Unstable = this.Unstable
        };
    }

    /// <summary>
    /// Serializes mechanics to JSON string
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserializes mechanics from JSON string
    /// </summary>
    public static HazardMechanics? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<HazardMechanics>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
    }
}
