using RuneAndRust.Core.Population;

namespace RuneAndRust.Core.Traits;

/// <summary>
/// Provides default configurations and point costs for creature traits.
/// Values are based on SPEC-COMBAT-015 balance guidelines.
/// </summary>
public static class TraitDefaults
{
    private static readonly Dictionary<CreatureTrait, TraitConfiguration> _defaults = new()
    {
        // ========================================
        // Temporal Traits (100-199)
        // ========================================
        [CreatureTrait.TemporalPhase] = new() { Trait = CreatureTrait.TemporalPhase, PointCost = 4 },
        [CreatureTrait.TemporalPrescience] = new() { Trait = CreatureTrait.TemporalPrescience, PrimaryValue = 3, PointCost = 2 },
        [CreatureTrait.ChronoDistortion] = new() { Trait = CreatureTrait.ChronoDistortion, PrimaryValue = 3, SecondaryValue = 2, PointCost = 3 },
        [CreatureTrait.RandomBlink] = new() { Trait = CreatureTrait.RandomBlink, Percentage = 0.30f, PointCost = 2 },
        [CreatureTrait.TimeLoop] = new() { Trait = CreatureTrait.TimeLoop, Percentage = 0.25f, PointCost = 4 },
        [CreatureTrait.CausalEcho] = new() { Trait = CreatureTrait.CausalEcho, Percentage = 0.20f, PointCost = 3 },
        [CreatureTrait.TemporalStasis] = new() { Trait = CreatureTrait.TemporalStasis, PointCost = 2 },
        [CreatureTrait.Rewind] = new() { Trait = CreatureTrait.Rewind, PointCost = 3 },

        // ========================================
        // Corruption Traits (200-299)
        // ========================================
        [CreatureTrait.BlightAura] = new() { Trait = CreatureTrait.BlightAura, PrimaryValue = 2, SecondaryValue = 2, PointCost = 4 },
        [CreatureTrait.CorrosiveTouch] = new() { Trait = CreatureTrait.CorrosiveTouch, PrimaryValue = 1, PointCost = 3 },
        [CreatureTrait.DataFragment] = new() { Trait = CreatureTrait.DataFragment, PrimaryValue = 5, SecondaryValue = 2, PointCost = 2 },
        [CreatureTrait.Glitched] = new() { Trait = CreatureTrait.Glitched, Percentage = 0.15f, PointCost = -1 },
        [CreatureTrait.Infectious] = new() { Trait = CreatureTrait.Infectious, PrimaryValue = 1, PointCost = 1 },
        [CreatureTrait.RealityTear] = new() { Trait = CreatureTrait.RealityTear, PrimaryValue = 3, PointCost = 3 },
        [CreatureTrait.Reforming] = new() { Trait = CreatureTrait.Reforming, PrimaryValue = 5, PointCost = 3 },
        [CreatureTrait.VoidTouched] = new() { Trait = CreatureTrait.VoidTouched, PointCost = 3 },

        // ========================================
        // Mechanical Traits (300-399)
        // ========================================
        [CreatureTrait.IronHeart] = new() { Trait = CreatureTrait.IronHeart, Percentage = 0.50f, PointCost = 4 },
        [CreatureTrait.ArmoredPlating] = new() { Trait = CreatureTrait.ArmoredPlating, PrimaryValue = 4, PointCost = 4 },
        [CreatureTrait.Overcharge] = new() { Trait = CreatureTrait.Overcharge, PrimaryValue = 10, SecondaryValue = 2, PointCost = 2 },
        [CreatureTrait.EmergencyProtocol] = new() { Trait = CreatureTrait.EmergencyProtocol, PrimaryValue = 2, SecondaryValue = 1, PointCost = 3 },
        [CreatureTrait.ModularConstruction] = new() { Trait = CreatureTrait.ModularConstruction, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.SelfRepair] = new() { Trait = CreatureTrait.SelfRepair, PrimaryValue = 5, PointCost = 3 },
        [CreatureTrait.PowerSurge] = new() { Trait = CreatureTrait.PowerSurge, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.Networked] = new() { Trait = CreatureTrait.Networked, PrimaryValue = 1, SecondaryValue = 3, PointCost = 2 },

        // ========================================
        // Psychic Traits (400-499)
        // ========================================
        [CreatureTrait.ForlornAura] = new() { Trait = CreatureTrait.ForlornAura, PrimaryValue = 4, SecondaryValue = 3, PointCost = 6 },
        [CreatureTrait.MindSpike] = new() { Trait = CreatureTrait.MindSpike, PointCost = 3 },
        [CreatureTrait.MemoryDrain] = new() { Trait = CreatureTrait.MemoryDrain, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.PsychicScream] = new() { Trait = CreatureTrait.PsychicScream, PrimaryValue = 1, SecondaryValue = 3, PointCost = 4 },
        [CreatureTrait.FearAura] = new() { Trait = CreatureTrait.FearAura, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.ThoughtLeech] = new() { Trait = CreatureTrait.ThoughtLeech, PointCost = 4 },
        [CreatureTrait.Hallucination] = new() { Trait = CreatureTrait.Hallucination, Percentage = 0.20f, PointCost = 3 },
        [CreatureTrait.Whispers] = new() { Trait = CreatureTrait.Whispers, PrimaryValue = 3, PointCost = 3 },

        // ========================================
        // Mobility Traits (500-599)
        // ========================================
        [CreatureTrait.Flight] = new() { Trait = CreatureTrait.Flight, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.Burrowing] = new() { Trait = CreatureTrait.Burrowing, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.Phasing] = new() { Trait = CreatureTrait.Phasing, PointCost = 2 },
        [CreatureTrait.Swiftness] = new() { Trait = CreatureTrait.Swiftness, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.Anchored] = new() { Trait = CreatureTrait.Anchored, PointCost = 2 },
        [CreatureTrait.Ambush] = new() { Trait = CreatureTrait.Ambush, PointCost = 3 },
        [CreatureTrait.HitAndRun] = new() { Trait = CreatureTrait.HitAndRun, PrimaryValue = 1, PointCost = 2 },
        [CreatureTrait.Territorial] = new() { Trait = CreatureTrait.Territorial, PrimaryValue = 2, SecondaryValue = 2, PointCost = 2 },

        // ========================================
        // Defensive Traits (600-699)
        // ========================================
        [CreatureTrait.Regeneration] = new() { Trait = CreatureTrait.Regeneration, PrimaryValue = 5, PointCost = 3 },
        [CreatureTrait.DamageThreshold] = new() { Trait = CreatureTrait.DamageThreshold, PrimaryValue = 4, PointCost = 3 },
        [CreatureTrait.Reflective] = new() { Trait = CreatureTrait.Reflective, Percentage = 0.20f, PointCost = 3 },
        [CreatureTrait.ShieldGenerator] = new() { Trait = CreatureTrait.ShieldGenerator, PrimaryValue = 15, PointCost = 3 },
        [CreatureTrait.LastStand] = new() { Trait = CreatureTrait.LastStand, PrimaryValue = 2, PointCost = 4 },
        [CreatureTrait.AdaptiveArmor] = new() { Trait = CreatureTrait.AdaptiveArmor, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.Camouflage] = new() { Trait = CreatureTrait.Camouflage, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.Unstoppable] = new() { Trait = CreatureTrait.Unstoppable, PointCost = 3 },

        // ========================================
        // Offensive Traits (700-799)
        // ========================================
        [CreatureTrait.Brutal] = new() { Trait = CreatureTrait.Brutal, PrimaryValue = 3, PointCost = 3 },
        [CreatureTrait.Relentless] = new() { Trait = CreatureTrait.Relentless, PrimaryValue = 2, PointCost = 5 },
        [CreatureTrait.Executioner] = new() { Trait = CreatureTrait.Executioner, Percentage = 0.50f, PointCost = 3 },
        [CreatureTrait.ArmorPiercing] = new() { Trait = CreatureTrait.ArmorPiercing, PrimaryValue = 3, PointCost = 3 },
        [CreatureTrait.Reach] = new() { Trait = CreatureTrait.Reach, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.Sweeping] = new() { Trait = CreatureTrait.Sweeping, PointCost = 3 },
        [CreatureTrait.Enrage] = new() { Trait = CreatureTrait.Enrage, PrimaryValue = 2, SecondaryValue = 2, PointCost = 2 },
        [CreatureTrait.PredatorInstinct] = new() { Trait = CreatureTrait.PredatorInstinct, PrimaryValue = 2, SecondaryValue = 2, PointCost = 2 },

        // ========================================
        // Unique Traits (800-899)
        // ========================================
        [CreatureTrait.SplitOnDeath] = new() { Trait = CreatureTrait.SplitOnDeath, PrimaryValue = 2, Percentage = 0.50f, PointCost = 4 },
        [CreatureTrait.MirrorImage] = new() { Trait = CreatureTrait.MirrorImage, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.Vampiric] = new() { Trait = CreatureTrait.Vampiric, Percentage = 0.50f, PointCost = 4 },
        [CreatureTrait.Explosive] = new() { Trait = CreatureTrait.Explosive, PrimaryValue = 3, SecondaryValue = 2, PointCost = 3 },
        [CreatureTrait.Resurrection] = new() { Trait = CreatureTrait.Resurrection, PrimaryValue = 2, Percentage = 0.50f, PointCost = 5 },
        [CreatureTrait.SymbioticLink] = new() { Trait = CreatureTrait.SymbioticLink, Percentage = 0.50f, PointCost = 3 },
        [CreatureTrait.Berserk] = new() { Trait = CreatureTrait.Berserk, PointCost = 1 },
        [CreatureTrait.PackTactics] = new() { Trait = CreatureTrait.PackTactics, PrimaryValue = 1, PointCost = 2 },

        // ========================================
        // Resistance Traits (900-999)
        // ========================================
        [CreatureTrait.FireResistant] = new() { Trait = CreatureTrait.FireResistant, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.ColdResistant] = new() { Trait = CreatureTrait.ColdResistant, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.LightningResistant] = new() { Trait = CreatureTrait.LightningResistant, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.PsychicResistant] = new() { Trait = CreatureTrait.PsychicResistant, Percentage = 0.50f, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.PhysicalResistant] = new() { Trait = CreatureTrait.PhysicalResistant, Percentage = 0.75f, PointCost = 3 },
        [CreatureTrait.AcidResistant] = new() { Trait = CreatureTrait.AcidResistant, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.FireVulnerable] = new() { Trait = CreatureTrait.FireVulnerable, Percentage = 1.50f, PointCost = -1 },
        [CreatureTrait.ColdVulnerable] = new() { Trait = CreatureTrait.ColdVulnerable, Percentage = 1.50f, PointCost = -1 },
        [CreatureTrait.LightningVulnerable] = new() { Trait = CreatureTrait.LightningVulnerable, Percentage = 1.50f, PointCost = -1 },
        [CreatureTrait.HolyVulnerable] = new() { Trait = CreatureTrait.HolyVulnerable, Percentage = 2.00f, PointCost = -2 },
        [CreatureTrait.ElementalAbsorption] = new() { Trait = CreatureTrait.ElementalAbsorption, PointCost = 5 },
        [CreatureTrait.AlloyedForm] = new() { Trait = CreatureTrait.AlloyedForm, PrimaryValue = 2, Percentage = 1.50f, PointCost = 3 },
        [CreatureTrait.OrganicBane] = new() { Trait = CreatureTrait.OrganicBane, Percentage = 0.50f, PointCost = 2 },
        [CreatureTrait.MechanicalBane] = new() { Trait = CreatureTrait.MechanicalBane, Percentage = 0.50f, PointCost = 2 },

        // ========================================
        // Strategy/AI Behavior Traits (1000-1099)
        // ========================================
        [CreatureTrait.Cowardly] = new() { Trait = CreatureTrait.Cowardly, PointCost = -1 },
        [CreatureTrait.Skittish] = new() { Trait = CreatureTrait.Skittish, Percentage = 0.40f, PointCost = -1 },
        [CreatureTrait.HealerHunter] = new() { Trait = CreatureTrait.HealerHunter, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.ThreatFocused] = new() { Trait = CreatureTrait.ThreatFocused, PointCost = 1 },
        [CreatureTrait.WeaknessSensor] = new() { Trait = CreatureTrait.WeaknessSensor, PrimaryValue = 1, PointCost = 2 },
        [CreatureTrait.ProtectorInstinct] = new() { Trait = CreatureTrait.ProtectorInstinct, PointCost = 2 },
        [CreatureTrait.Opportunist] = new() { Trait = CreatureTrait.Opportunist, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.CasterKiller] = new() { Trait = CreatureTrait.CasterKiller, Percentage = 0.25f, PointCost = 3 },
        [CreatureTrait.BerserkerAI] = new() { Trait = CreatureTrait.BerserkerAI, PrimaryValue = 1, PointCost = 2 },
        [CreatureTrait.PackLeader] = new() { Trait = CreatureTrait.PackLeader, PrimaryValue = 1, SecondaryValue = 3, PointCost = 3 },
        [CreatureTrait.AmbusherAI] = new() { Trait = CreatureTrait.AmbusherAI, PointCost = 1 },
        [CreatureTrait.TerritorialAI] = new() { Trait = CreatureTrait.TerritorialAI, PrimaryValue = 3, SecondaryValue = 3, PointCost = 3 },
        [CreatureTrait.HitAndFade] = new() { Trait = CreatureTrait.HitAndFade, PointCost = 2 },
        [CreatureTrait.Swarmer] = new() { Trait = CreatureTrait.Swarmer, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.SelfDestructive] = new() { Trait = CreatureTrait.SelfDestructive, PointCost = 1 },
        [CreatureTrait.Cautious] = new() { Trait = CreatureTrait.Cautious, PointCost = 0 },

        // ========================================
        // Sensory Traits (1100-1199)
        // ========================================
        [CreatureTrait.Blind] = new() { Trait = CreatureTrait.Blind, PointCost = -2 },
        [CreatureTrait.Blindsense] = new() { Trait = CreatureTrait.Blindsense, PrimaryValue = 3, PointCost = 2 },
        [CreatureTrait.SoundSensitive] = new() { Trait = CreatureTrait.SoundSensitive, PrimaryValue = 2, PointCost = 1 },
        [CreatureTrait.SoundBlind] = new() { Trait = CreatureTrait.SoundBlind, PrimaryValue = 2, PointCost = 0 },
        [CreatureTrait.Tremorsense] = new() { Trait = CreatureTrait.Tremorsense, PrimaryValue = 4, PointCost = 2 },
        [CreatureTrait.ThermalVision] = new() { Trait = CreatureTrait.ThermalVision, PrimaryValue = 2, SecondaryValue = 2, PointCost = 1 },
        [CreatureTrait.MotionDetection] = new() { Trait = CreatureTrait.MotionDetection, PrimaryValue = 3, SecondaryValue = 2, PointCost = 1 },
        [CreatureTrait.Darkvision] = new() { Trait = CreatureTrait.Darkvision, PointCost = 1 },
        [CreatureTrait.LightSensitive] = new() { Trait = CreatureTrait.LightSensitive, PrimaryValue = 2, PointCost = 0 },
        [CreatureTrait.PsychicSense] = new() { Trait = CreatureTrait.PsychicSense, PrimaryValue = 5, PointCost = 4 },
        [CreatureTrait.Eyeless] = new() { Trait = CreatureTrait.Eyeless, PointCost = 1 },
        [CreatureTrait.ScatterSense] = new() { Trait = CreatureTrait.ScatterSense, PrimaryValue = 2, PointCost = 2 },

        // ========================================
        // Combat Condition Traits (1200-1299)
        // ========================================
        [CreatureTrait.Amphibious] = new() { Trait = CreatureTrait.Amphibious, PrimaryValue = 2, PointCost = 1 },
        [CreatureTrait.HeatAdapted] = new() { Trait = CreatureTrait.HeatAdapted, PrimaryValue = 2, PointCost = 1 },
        [CreatureTrait.ColdAdapted] = new() { Trait = CreatureTrait.ColdAdapted, PrimaryValue = 2, PointCost = 1 },
        [CreatureTrait.ToxinImmune] = new() { Trait = CreatureTrait.ToxinImmune, PointCost = 2 },
        [CreatureTrait.RadiationImmune] = new() { Trait = CreatureTrait.RadiationImmune, PointCost = 1 },
        [CreatureTrait.VacuumSurvival] = new() { Trait = CreatureTrait.VacuumSurvival, PointCost = 1 },
        [CreatureTrait.GravityAdapted] = new() { Trait = CreatureTrait.GravityAdapted, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.Buoyant] = new() { Trait = CreatureTrait.Buoyant, PrimaryValue = 2, PointCost = 0 },
        [CreatureTrait.Grounded] = new() { Trait = CreatureTrait.Grounded, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.Ethereal] = new() { Trait = CreatureTrait.Ethereal, Percentage = 0.50f, PrimaryValue = 2, PointCost = 3 },
        [CreatureTrait.SunlightWeakness] = new() { Trait = CreatureTrait.SunlightWeakness, PrimaryValue = 2, PointCost = -1 },
        [CreatureTrait.MoonlightStrength] = new() { Trait = CreatureTrait.MoonlightStrength, PrimaryValue = 2, PointCost = 1 },
        [CreatureTrait.StormBorn] = new() { Trait = CreatureTrait.StormBorn, PrimaryValue = 3, SecondaryValue = 5, PointCost = 2 },
        [CreatureTrait.BloodScent] = new() { Trait = CreatureTrait.BloodScent, PrimaryValue = 2, SecondaryValue = 5, PointCost = 2 },
        [CreatureTrait.CorruptionSustained] = new() { Trait = CreatureTrait.CorruptionSustained, PrimaryValue = 2, PointCost = 2 },
        [CreatureTrait.HallowedBane] = new() { Trait = CreatureTrait.HallowedBane, PrimaryValue = 2, PointCost = -1 }
    };

    /// <summary>
    /// Gets the default configuration for a trait. Returns a new instance with default values.
    /// </summary>
    public static TraitConfiguration GetDefault(CreatureTrait trait)
    {
        if (_defaults.TryGetValue(trait, out var config))
        {
            // Return a copy to prevent mutation of defaults
            return new TraitConfiguration
            {
                Trait = config.Trait,
                PrimaryValue = config.PrimaryValue,
                SecondaryValue = config.SecondaryValue,
                Percentage = config.Percentage,
                PointCost = config.PointCost
            };
        }

        // Unknown trait - return basic config
        return new TraitConfiguration { Trait = trait };
    }

    /// <summary>
    /// Gets the point cost for a trait from defaults.
    /// </summary>
    public static int GetPointCost(CreatureTrait trait)
    {
        return _defaults.TryGetValue(trait, out var config) ? config.PointCost : 0;
    }

    /// <summary>
    /// Gets all traits in a given category.
    /// </summary>
    public static IEnumerable<CreatureTrait> GetTraitsInCategory(TraitCategory category)
    {
        var minValue = category.GetMinValue();
        var maxValue = category.GetMaxValue();

        return _defaults.Keys
            .Where(t => (int)t >= minValue && (int)t < maxValue)
            .OrderBy(t => (int)t);
    }

    /// <summary>
    /// Calculates total trait points for a collection of trait configurations.
    /// </summary>
    public static int CalculateTotalPoints(IEnumerable<TraitConfiguration> traits)
    {
        return traits.Sum(t => t.PointCost > 0 ? t.PointCost : GetPointCost(t.Trait));
    }

    /// <summary>
    /// Gets trait budget range for a threat tier.
    /// Minimal/Low = 2-6 points, Medium = 6-10, High = 10-14, Boss = 14-25
    /// </summary>
    public static (int Min, int Max) GetBudgetForTier(ThreatLevel tier)
    {
        return tier switch
        {
            ThreatLevel.Minimal => (2, 4),
            ThreatLevel.Low => (4, 6),
            ThreatLevel.Medium => (6, 10),
            ThreatLevel.High => (10, 14),
            ThreatLevel.Boss => (14, 25),
            _ => (0, 6)
        };
    }
}
