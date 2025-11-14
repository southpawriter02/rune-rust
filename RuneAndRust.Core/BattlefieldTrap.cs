namespace RuneAndRust.Core;

/// <summary>
/// Represents a trap placed on the battlefield by a combatant (typically Rúnasmiðr).
/// Traps are invisible to enemies and trigger based on specific conditions.
/// </summary>
public class BattlefieldTrap
{
    public string TrapId { get; set; }
    public string TrapName { get; set; }
    public GridPosition Position { get; set; }
    public string OwnerId { get; set; }                    // Combatant who placed the trap
    public int TurnsRemaining { get; set; }                // 3 turns by default
    public bool IsVisible { get; set; }                    // False = invisible to enemies

    // Trap effect
    public TrapEffectType EffectType { get; set; }
    public Dictionary<string, object> EffectData { get; set; }

    // Trigger condition
    public TrapTriggerType TriggerType { get; set; }       // OnEnter, OnExit, Manual

    public BattlefieldTrap(string trapId, string trapName, string ownerId)
    {
        TrapId = trapId;
        TrapName = trapName;
        OwnerId = ownerId;
        TurnsRemaining = 3;
        IsVisible = false;
        EffectData = new Dictionary<string, object>();
        TriggerType = TrapTriggerType.OnEnter;
    }

    public override string ToString()
    {
        return $"{TrapName} @ {Position} (Owner:{OwnerId}, Turns:{TurnsRemaining})";
    }
}

/// <summary>
/// Type of effect the trap produces when triggered
/// </summary>
public enum TrapEffectType
{
    Damage,         // Direct HP damage
    Status,         // Apply status effect (Rooted, Disoriented, etc.)
    Debuff,         // Apply attribute/stat debuff
    AreaEffect      // Multi-tile area of effect
}

/// <summary>
/// Condition that triggers the trap
/// </summary>
public enum TrapTriggerType
{
    OnEnter,        // Triggered when combatant enters the tile
    OnExit,         // Triggered when combatant leaves the tile
    Manual          // Triggered manually by trap owner
}
