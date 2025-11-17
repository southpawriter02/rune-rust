namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.3: Interactive object type enumeration
/// Defines the four core categories of interactive objects
/// </summary>
public enum InteractiveObjectType
{
    /// <summary>
    /// Mechanisms (levers, switches, consoles, pressure plates)
    /// Interaction: Pull, Hack, Automatic trigger
    /// </summary>
    Mechanism,

    /// <summary>
    /// Containers (crates, chests, lockers)
    /// Interaction: Search, Open (possibly locked)
    /// </summary>
    Container,

    /// <summary>
    /// Investigatable objects (corpses, data slates, terminals)
    /// Interaction: Search, Read, Examine
    /// </summary>
    Investigatable,

    /// <summary>
    /// Barriers (doors, gates, hatches)
    /// Interaction: Open, Close (possibly locked or destructible)
    /// </summary>
    Barrier
}

/// <summary>
/// Interaction pattern types
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Pull/push action (levers, switches)
    /// </summary>
    Pull,

    /// <summary>
    /// Open/close action (doors, containers)
    /// </summary>
    Open,

    /// <summary>
    /// Search action (containers, corpses)
    /// </summary>
    Search,

    /// <summary>
    /// Read action (data slates, terminals)
    /// </summary>
    Read,

    /// <summary>
    /// Hack action (consoles, terminals)
    /// </summary>
    Hack,

    /// <summary>
    /// Automatic trigger (pressure plates)
    /// </summary>
    Automatic,

    /// <summary>
    /// Examine action (investigatable objects)
    /// </summary>
    Examine
}

/// <summary>
/// Consequence types for object interactions
/// </summary>
public enum ConsequenceType
{
    /// <summary>
    /// Unlock/open something (door, container, passage)
    /// </summary>
    Unlock,

    /// <summary>
    /// Trigger mechanism (lever consequence, hazard activation)
    /// </summary>
    Trigger,

    /// <summary>
    /// Spawn entities (enemies, NPCs, events)
    /// </summary>
    Spawn,

    /// <summary>
    /// Reveal information (quest hook, lore, hidden passage)
    /// </summary>
    Reveal,

    /// <summary>
    /// Grant loot (items, currency, quest items)
    /// </summary>
    Loot,

    /// <summary>
    /// Activate trap (damage, status effect)
    /// </summary>
    Trap,

    /// <summary>
    /// No consequence (informational only)
    /// </summary>
    None
}

/// <summary>
/// Check types for skill-based interactions
/// </summary>
public enum SkillCheckType
{
    /// <summary>
    /// No check required
    /// </summary>
    None,

    /// <summary>
    /// WITS check (hacking, puzzles, searching)
    /// </summary>
    WITS,

    /// <summary>
    /// MIGHT check (forcing doors, moving objects)
    /// </summary>
    MIGHT,

    /// <summary>
    /// Lockpicking check (locked containers, doors)
    /// </summary>
    Lockpicking,

    /// <summary>
    /// Hacking check (consoles, terminals)
    /// </summary>
    Hacking
}

/// <summary>
/// Loot tier for containers and corpses
/// </summary>
public enum LootTier
{
    /// <summary>
    /// No loot
    /// </summary>
    None,

    /// <summary>
    /// Common loot (basic items, small currency)
    /// </summary>
    Common,

    /// <summary>
    /// Uncommon loot (better items, moderate currency)
    /// </summary>
    Uncommon,

    /// <summary>
    /// Rare loot (valuable items, large currency, possible quest items)
    /// </summary>
    Rare,

    /// <summary>
    /// Random loot tier (determined at generation)
    /// </summary>
    Random
}
