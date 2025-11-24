using Serilog;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Core;

/// <summary>
/// v0.13: Base class for destructible environmental elements.
/// Handles HP tracking, damage application, and destruction events.
/// </summary>
public abstract class DestructibleElement
{
    private static readonly ILogger _log = Log.ForContext<DestructibleElement>();

    public string ElementId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Destructibility
    public bool IsDestructible { get; set; } = false;
    public int MaxHP { get; set; } = 0;
    public int CurrentHP { get; set; } = 0;

    public bool IsDestroyed => CurrentHP <= 0;

    // Narrative
    public string DestructionNarrative { get; set; } = string.Empty;
    public string PartialDamageNarrative { get; set; } = string.Empty;

    // Rubble spawning
    public bool SpawnsRubbleOnDestruction { get; set; } = true;

    /// <summary>
    /// Initialize HP for a destructible element
    /// </summary>
    public void Initialize()
    {
        if (IsDestructible && MaxHP > 0)
        {
            CurrentHP = MaxHP;
            _log.Debug("Initialized destructible element: {ElementId}, HP={MaxHP}", ElementId, MaxHP);
        }
    }

    /// <summary>
    /// Apply damage to this element
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="source">Source of the damage (for logging)</param>
    /// <returns>True if element was destroyed by this damage</returns>
    public bool TakeDamage(int damage, string source)
    {
        if (!IsDestructible)
        {
            _log.Debug("Attempted to damage non-destructible element: {ElementId}", ElementId);
            return false;
        }

        var previousHP = CurrentHP;
        CurrentHP = Math.Max(0, CurrentHP - damage);

        _log.Information("Destructible element damaged: Element={ElementId}, Damage={Damage}, " +
            "HP={CurrentHP}/{MaxHP}, Source={Source}",
            ElementId, damage, CurrentHP, MaxHP, source);

        var wasDestroyed = previousHP > 0 && CurrentHP <= 0;

        if (wasDestroyed)
        {
            OnDestroyed(source);
        }

        return wasDestroyed;
    }

    /// <summary>
    /// Called when this element is destroyed
    /// </summary>
    protected virtual void OnDestroyed(string source)
    {
        _log.Information("Destructible element destroyed: Element={ElementId}, Source={Source}",
            ElementId, source);
    }

    /// <summary>
    /// Get narrative description of current damage state
    /// </summary>
    public string GetDamageStateDescription()
    {
        if (!IsDestructible || MaxHP == 0)
            return string.Empty;

        var healthPercent = (float)CurrentHP / MaxHP;

        return healthPercent switch
        {
            >= 0.75f => "It appears mostly intact.",
            >= 0.50f => "It shows signs of damage.",
            >= 0.25f => "It is heavily damaged and unstable.",
            > 0.00f => "It is on the verge of total collapse.",
            _ => "It has been destroyed."
        };
    }
}

/// <summary>
/// v0.13: Extensions to StaticTerrain to support destruction
/// </summary>
public static class StaticTerrainExtensions
{
    /// <summary>
    /// Initialize HP for destructible static terrain
    /// </summary>
    public static void InitializeHP(this StaticTerrain terrain)
    {
        if (terrain.IsDestructible && terrain.HP > 0)
        {
            // For StaticTerrain, HP field serves as both max and current
            // We'll track damage in a separate system
        }
    }

    /// <summary>
    /// Get default HP for terrain type
    /// </summary>
    public static int GetDefaultHP(this StaticTerrainType type)
    {
        return type switch
        {
            StaticTerrainType.CollapsedPillar => 30,
            StaticTerrainType.RustedBulkhead => 40,
            StaticTerrainType.RubblePile => 15,
            StaticTerrainType.CorrodedGrating => 10,
            StaticTerrainType.SteelBarricade => 35,
            StaticTerrainType.ExposedConduit => 20,
            StaticTerrainType.BrokenGantry => 25,
            _ => 0
        };
    }

    /// <summary>
    /// Check if terrain type should spawn rubble on destruction
    /// </summary>
    public static bool ShouldSpawnRubble(this StaticTerrainType type)
    {
        return type switch
        {
            StaticTerrainType.CollapsedPillar => true,
            StaticTerrainType.RustedBulkhead => true,
            StaticTerrainType.SteelBarricade => true,
            StaticTerrainType.BrokenGantry => true,
            StaticTerrainType.CorrodedGrating => false, // Falls into chasm
            StaticTerrainType.RubblePile => false,      // Already rubble
            _ => false
        };
    }

    /// <summary>
    /// Get destruction narrative for terrain type
    /// </summary>
    public static string GetDestructionNarrative(this StaticTerrainType type)
    {
        return type switch
        {
            StaticTerrainType.CollapsedPillar =>
                "The corroded pillar groans under centuries of stress. Your assault is the final strain—it collapses in a cloud of rust and debris.",
            StaticTerrainType.RustedBulkhead =>
                "The rusted bulkhead crumples under your assault, screeching as metal tears and buckles, revealing the passage beyond.",
            StaticTerrainType.RubblePile =>
                "You heave the debris aside, scattering it across the floor. The path is now clear.",
            StaticTerrainType.CorrodedGrating =>
                "The weakened grating gives way with a metallic shriek, plunging into the darkness below.",
            StaticTerrainType.SteelBarricade =>
                "The steel barricade, weakened by 800 years of decay, finally yields. It crashes to the ground with a resounding clang.",
            StaticTerrainType.BrokenGantry =>
                "The gantry's remaining supports fail. The structure collapses into twisted metal.",
            _ => $"The {type} is destroyed."
        };
    }
}

/// <summary>
/// v0.13: Extensions to DynamicHazard to support destruction
/// </summary>
public static class DynamicHazardExtensions
{
    /// <summary>
    /// Get default HP for hazard type
    /// </summary>
    public static int GetDefaultHP(this DynamicHazardType type)
    {
        return type switch
        {
            DynamicHazardType.SteamVent => 20,
            DynamicHazardType.LivePowerConduit => 15,
            DynamicHazardType.UnstableCeiling => 0,  // Non-destructible (one-time)
            DynamicHazardType.ToxicSporeCloud => 0,   // Non-destructible (environmental)
            DynamicHazardType.CorrodedGrating => 10,
            DynamicHazardType.LeakingCoolant => 12,
            DynamicHazardType.RadiationLeak => 0,     // Non-destructible (radiation source)
            DynamicHazardType.PressurizedPipe => 18,
            _ => 0
        };
    }

    /// <summary>
    /// Check if hazard type is destructible
    /// </summary>
    public static bool IsDestructibleHazard(this DynamicHazardType type)
    {
        return type switch
        {
            DynamicHazardType.SteamVent => true,
            DynamicHazardType.LivePowerConduit => true,
            DynamicHazardType.CorrodedGrating => true,
            DynamicHazardType.LeakingCoolant => true,
            DynamicHazardType.PressurizedPipe => true,
            _ => false
        };
    }

    /// <summary>
    /// Get destruction narrative for hazard type
    /// </summary>
    public static string GetDestructionNarrative(this DynamicHazardType type)
    {
        return type switch
        {
            DynamicHazardType.SteamVent =>
                "You jam debris into the vent, sealing the escaping steam. The hissing fades to silence.",
            DynamicHazardType.LivePowerConduit =>
                "You sever the conduit with a decisive strike. Sparks fly, then darkness. The electrical hazard is neutralized.",
            DynamicHazardType.CorrodedGrating =>
                "The grating collapses completely, leaving a gaping hole in the floor.",
            DynamicHazardType.LeakingCoolant =>
                "You rupture the coolant line completely. The chemical spill drains away through floor grates.",
            DynamicHazardType.PressurizedPipe =>
                "The pipe explodes in a violent decompression! Once the chaos settles, the hazard is gone.",
            _ => $"The {type} is destroyed."
        };
    }

    /// <summary>
    /// Check if destroying this hazard causes secondary effects
    /// </summary>
    public static bool CausesSecondaryEffect(this DynamicHazardType type)
    {
        return type switch
        {
            DynamicHazardType.PressurizedPipe => true,  // Explosion
            DynamicHazardType.LivePowerConduit => true, // Sparks/electrical discharge
            _ => false
        };
    }
}
