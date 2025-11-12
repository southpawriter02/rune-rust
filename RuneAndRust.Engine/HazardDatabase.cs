using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.16 Hazard Database
/// Provides pre-configured dynamic hazard definitions
/// </summary>
public class HazardDatabase
{
    private readonly Dictionary<string, DynamicHazard> _hazards = new();

    public HazardDatabase()
    {
        InitializeV016Hazards();
    }

    private void InitializeV016Hazards()
    {
        // 1. Spore Cloud
        AddHazard("spore_cloud", new DynamicHazard
        {
            HazardId = "spore_cloud",
            Name = "[Spore Cloud]",
            Description = "Thick clouds of Symbiotic Plate spores drift through the air. Vision obscured. Breathing difficult.",
            Type = DynamicHazardType.SporeCloud,
            ActivationChance = 1.0f,
            Trigger = HazardTrigger.Automatic,
            DamageDice = 0,
            DamageType = "Corruption",
            AreaSize = 5, // 5ft radius
            AffectsAllCombatants = true,
            AppliesStatusEffect = "Infected",
            StatusEffectChance = 0.3f,
            IsOneTime = false
        });

        // 2. Automated Turret
        AddHazard("automated_turret", new DynamicHazard
        {
            HazardId = "automated_turret",
            Name = "[Automated Turret]",
            Description = "Pre-Glitch security system. Red targeting laser sweeps the room. Still active after 800 years.",
            Type = DynamicHazardType.AutomatedTurret,
            ActivationChance = 0.8f, // 80% chance per turn
            Trigger = HazardTrigger.OnMovement,
            DamageDice = 2, // 2d6 damage
            DamageType = "Kinetic",
            AreaSize = 1,
            AffectsAllCombatants = false,
            IsOneTime = false,
            RequiresProximity = false
        });

        // 3. Collapsing Ceiling
        AddHazard("collapsing_ceiling", new DynamicHazard
        {
            HazardId = "collapsing_ceiling",
            Name = "[Collapsing Ceiling]",
            Description = "800 years of decay. The ceiling groans. Chunks of concrete rain down.",
            Type = DynamicHazardType.CollapsingCeiling,
            ActivationChance = 0.3f, // 30% chance per turn
            Trigger = HazardTrigger.Automatic,
            DamageDice = 3, // 3d8 ≈ 3d6+3
            DamageType = "Physical",
            AreaSize = 3, // 3x3 area
            AffectsAllCombatants = true,
            IsOneTime = true,
            AppliesStatusEffect = "Difficult Terrain",
            StatusEffectChance = 1.0f
        });

        // 4. Data Stream
        AddHazard("data_stream", new DynamicHazard
        {
            HazardId = "data_stream",
            Name = "[Data Stream]",
            Description = "Jötun-Reader information overflow. Alien thought pouring into your mind. Too much. Too fast.",
            Type = DynamicHazardType.DataStream,
            ActivationChance = 1.0f,
            Trigger = HazardTrigger.OnProximity,
            DamageDice = 0,
            DamageType = "Psychic",
            AreaSize = 2,
            AffectsAllCombatants = false,
            RequiresProximity = true,
            ProximityRange = 2,
            AppliesStatusEffect = "Stunned",
            StatusEffectChance = 0.5f,
            IsOneTime = false
        });

        // 5. Fungal Growth
        AddHazard("fungal_growth", new DynamicHazard
        {
            HazardId = "fungal_growth",
            Name = "[Fungal Growth]",
            Description = "Dense Symbiotic Plate barrier. Blocks movement. Attacks spread spores.",
            Type = DynamicHazardType.FungalGrowth,
            ActivationChance = 1.0f,
            Trigger = HazardTrigger.Manual,
            DamageDice = 0,
            DamageType = "Corruption",
            AreaSize = 1,
            AffectsAllCombatants = false,
            AppliesStatusEffect = "Blocking Terrain",
            StatusEffectChance = 1.0f,
            IsOneTime = false
        });

        // 6. Unstable Grating
        AddHazard("unstable_grating", new DynamicHazard
        {
            HazardId = "unstable_grating",
            Name = "[Unstable Grating]",
            Description = "Corroded floor grating. Fragile. Waiting to collapse under your weight.",
            Type = DynamicHazardType.UnstableGrating,
            ActivationChance = 1.0f,
            Trigger = HazardTrigger.OnMovement,
            DamageDice = 2, // 2d10 ≈ 2d6+4
            DamageType = "Physical",
            AreaSize = 1,
            AffectsAllCombatants = false,
            IsOneTime = true,
            AppliesStatusEffect = "Prone",
            StatusEffectChance = 1.0f
        });

        // 7. Psychic Echo
        AddHazard("psychic_echo", new DynamicHazard
        {
            HazardId = "psychic_echo",
            Name = "[Psychic Echo]",
            Description = "The Great Silence left echoes. Moments of death, terror, despair - replaying endlessly.",
            Type = DynamicHazardType.PsychicEcho,
            ActivationChance = 0.4f, // 40% chance
            Trigger = HazardTrigger.OnProximity,
            DamageDice = 0,
            DamageType = "Psychic",
            AreaSize = 2,
            AffectsAllCombatants = true,
            RequiresProximity = true,
            ProximityRange = 3,
            AppliesStatusEffect = "Shaken",
            StatusEffectChance = 1.0f,
            IsOneTime = false
        });

        // 8. Radiation Source
        AddHazard("radiation_source", new DynamicHazard
        {
            HazardId = "radiation_source",
            Name = "[Radiation Leak]",
            Description = "Damaged reactor core. Hazmat zone. Your dosimeter screams warnings you can't ignore.",
            Type = DynamicHazardType.RadiationSource,
            ActivationChance = 1.0f,
            Trigger = HazardTrigger.Automatic,
            DamageDice = 1, // 1d6 per turn
            DamageType = "Radiation",
            AreaSize = 4, // 4x4 area
            AffectsAllCombatants = true,
            AppliesStatusEffect = "Irradiated",
            StatusEffectChance = 1.0f,
            IsOneTime = false
        });
    }

    private void AddHazard(string key, DynamicHazard hazard)
    {
        _hazards[key] = hazard;
    }

    /// <summary>
    /// Get a hazard by ID
    /// </summary>
    public DynamicHazard? GetHazard(string hazardId)
    {
        return _hazards.GetValueOrDefault(hazardId);
    }

    /// <summary>
    /// Get all hazards in the database
    /// </summary>
    public List<DynamicHazard> GetAllHazards()
    {
        return _hazards.Values.ToList();
    }

    /// <summary>
    /// Get hazards by type
    /// </summary>
    public List<DynamicHazard> GetHazardsByType(DynamicHazardType type)
    {
        return _hazards.Values
            .Where(h => h.Type == type)
            .ToList();
    }

    /// <summary>
    /// Create a new instance of a hazard (for spawning multiples)
    /// </summary>
    public DynamicHazard? CreateHazardInstance(string hazardId, string instanceId)
    {
        var template = GetHazard(hazardId);
        if (template == null) return null;

        return new DynamicHazard
        {
            HazardId = instanceId,
            Name = template.Name,
            Description = template.Description,
            Type = template.Type,
            ActivationChance = template.ActivationChance,
            Trigger = template.Trigger,
            DamageDice = template.DamageDice,
            DamageDieSize = template.DamageDieSize,
            DamageType = template.DamageType,
            AreaSize = template.AreaSize,
            AffectsAllCombatants = template.AffectsAllCombatants,
            IsOneTime = template.IsOneTime,
            RequiresProximity = template.RequiresProximity,
            ProximityRange = template.ProximityRange,
            AppliesStatusEffect = template.AppliesStatusEffect,
            StatusEffectChance = template.StatusEffectChance,
            EnhancedByCondition = template.EnhancedByCondition,
            EnhancementMultiplier = template.EnhancementMultiplier,
            IsActive = true,
            HasActivatedThisTurn = false
        };
    }
}
