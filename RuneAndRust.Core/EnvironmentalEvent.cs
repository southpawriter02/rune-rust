namespace RuneAndRust.Core;

/// <summary>
/// v0.22: Environmental Combat System - Environmental Events
/// Tracks environmental interactions for analytics, replay, and statistics.
/// Records object destruction, hazard triggers, environmental kills, and manipulations.
/// </summary>
public class EnvironmentalEvent
{
    // Identity
    public int EventId { get; set; }
    public int CombatInstanceId { get; set; }
    public int TurnNumber { get; set; }

    // Event details
    public EnvironmentalEventType EventType { get; set; }
    public int? ObjectId { get; set; } // Environmental object involved
    public int? ActorId { get; set; } // Character who triggered the event (null for automatic hazards)
    public List<int> Targets { get; set; } = new(); // Affected character IDs

    // Damage/effects
    public int DamageDealt { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public string? StatusEffectApplied { get; set; } // "[Burning]", "[Poisoned]", etc.

    // Context
    public string? Description { get; set; } // Narrative description of the event
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a combat log entry for this event
    /// </summary>
    public string GenerateLogEntry()
    {
        return EventType switch
        {
            EnvironmentalEventType.ObjectDestroyed =>
                $"[STRUCTURE DESTROYED] {Description ?? "Environmental object destroyed"}",
            EnvironmentalEventType.HazardTriggered =>
                $"💀 {Description ?? "Environmental hazard activated"} → {DamageDealt} damage",
            EnvironmentalEventType.EnvironmentalKill =>
                $"☠️ ENVIRONMENTAL KILL! {Description ?? "Enemy killed by environment"}",
            EnvironmentalEventType.CoverPlaced =>
                $"🛡️ Cover established: {Description}",
            EnvironmentalEventType.CeilingCollapse =>
                $"⚠️ STRUCTURAL FAILURE! {Description ?? "Ceiling collapsed"} → {DamageDealt} damage",
            EnvironmentalEventType.PushIntoHazard =>
                $"⚡ {Description ?? "Enemy pushed into hazard"} → {DamageDealt} damage",
            EnvironmentalEventType.InteractionTriggered =>
                $"🔧 {Description ?? "Environmental interaction"}",
            EnvironmentalEventType.AmbientDamage =>
                $"☠️ {Description ?? "Ambient condition damage"} → {DamageDealt} damage",
            _ => Description ?? "Environmental event occurred"
        };
    }
}

/// <summary>
/// Type of environmental event
/// </summary>
public enum EnvironmentalEventType
{
    ObjectDestroyed,        // Environmental object was destroyed
    HazardTriggered,        // Static or dynamic hazard dealt damage
    EnvironmentalKill,      // Character killed by environmental damage
    CoverPlaced,            // Cover was deployed/established
    CoverDestroyed,         // Cover was destroyed (subset of ObjectDestroyed)
    CeilingCollapse,        // Controlled collapse triggered
    PushIntoHazard,         // Character pushed/pulled into hazard
    InteractionTriggered,   // Interactive object activated
    AmbientDamage,          // Ambient condition dealt damage
    WeatherEffectApplied,   // Weather effect modified combat
    ChainReaction           // Destruction caused cascade effect
}

/// <summary>
/// Result of environmental object destruction (v0.22.1 enhanced)
/// </summary>
public class DestructionResult
{
    public bool Success { get; set; } = true;
    public bool ObjectDestroyed { get; set; }
    public int ObjectId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string? DestructionMethod { get; set; }
    public int DamageDealt { get; set; }
    public int RemainingDurability { get; set; }
    public List<int> SecondaryTargets { get; set; } = new(); // Character IDs hit by destruction
    public string? TerrainCreated { get; set; }
    public List<DestructionResult> ChainReactions { get; set; } = new(); // Cascading effects
    public string Message { get; set; } = string.Empty;

    // Legacy compatibility
    public bool WasDestroyed => ObjectDestroyed;
    public string LogMessage => Message;
}

/// <summary>
/// Result of hazard trigger (v0.22.1 enhanced)
/// </summary>
public class HazardResult
{
    public bool WasTriggered { get; set; }
    public int TotalDamage { get; set; }
    public List<int> AffectedCharacters { get; set; } = new();
    public string? StatusEffectApplied { get; set; }
    public string Description { get; set; } = string.Empty;
    public string HazardName { get; set; } = string.Empty;

    // Legacy compatibility - make settable for backward compatibility
    public int DamageDealt
    {
        get => TotalDamage;
        set => TotalDamage = value;
    }
    public string LogMessage
    {
        get => Description;
        set => Description = value;
    }
}

/// <summary>
/// Result of push/pull manipulation
/// </summary>
public class PushResult
{
    public bool Success { get; set; }
    public string? NewPosition { get; set; }
    public List<int> HazardsEncountered { get; set; } = new(); // Hazard ObjectIds
    public int TotalDamage { get; set; }
    public string LogMessage { get; set; } = string.Empty;
}

/// <summary>
/// Result of controlled collapse
/// </summary>
public class CollapseResult
{
    public bool Success { get; set; }
    public List<int> AffectedCharacters { get; set; } = new();
    public int DamageDealt { get; set; }
    public string? TerrainCreated { get; set; } // "Rubble", "Difficult Terrain"
    public string LogMessage { get; set; } = string.Empty;
}

/// <summary>
/// Result of environmental combo detection
/// </summary>
public class ComboResult
{
    public bool ComboDetected { get; set; }
    public string? ComboName { get; set; }
    public int BonusDamage { get; set; }
    public string LogMessage { get; set; } = string.Empty;
}

/// <summary>
/// Cover data for position-based cover calculation
/// </summary>
public class CoverData
{
    public CoverQuality Quality { get; set; }
    public int DefenseBonus { get; set; }
    public int? RemainingDurability { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Result of interaction attempt
/// </summary>
public class InteractionResult
{
    public bool Success { get; set; }
    public int StaminaCost { get; set; }
    public string? EffectDescription { get; set; }
    public int DamageDealt { get; set; } = 0;
    public List<int> AffectedCharacters { get; set; } = new();
    public string LogMessage { get; set; } = string.Empty;
}
