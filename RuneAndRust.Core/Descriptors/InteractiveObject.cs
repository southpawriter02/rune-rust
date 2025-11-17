namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.3: Interactive object model
/// Represents player-interactable environmental objects
/// Generated from Descriptor_Base_Templates + Thematic_Modifiers + Function_Variants
/// </summary>
public class InteractiveObject
{
    /// <summary>
    /// Unique identifier for this object instance
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Room ID where this object is located
    /// </summary>
    public int RoomId { get; set; }

    /// <summary>
    /// Composite descriptor ID (if generated from composite)
    /// </summary>
    public int? CompositeDescriptorId { get; set; }

    /// <summary>
    /// Base template name (e.g., "Lever_Base")
    /// </summary>
    public string? BaseTemplateName { get; set; }

    /// <summary>
    /// Modifier name (e.g., "Rusted")
    /// </summary>
    public string? ModifierName { get; set; }

    /// <summary>
    /// Object name (e.g., "Corroded Door Lever")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive text for the object
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Object type (Mechanism, Container, Investigatable, Barrier)
    /// </summary>
    public InteractiveObjectType ObjectType { get; set; }

    // Interaction Properties
    /// <summary>
    /// Interaction type (Pull, Search, Read, Hack, Open, Automatic)
    /// </summary>
    public InteractionType InteractionType { get; set; }

    /// <summary>
    /// Whether interaction requires a skill check
    /// </summary>
    public bool RequiresCheck { get; set; }

    /// <summary>
    /// Check type (WITS, MIGHT, Lockpicking, Hacking)
    /// </summary>
    public SkillCheckType CheckType { get; set; }

    /// <summary>
    /// Check difficulty class
    /// </summary>
    public int CheckDC { get; set; }

    /// <summary>
    /// Number of attempts allowed (for consoles, hacking)
    /// </summary>
    public int AttemptsAllowed { get; set; } = 1;

    /// <summary>
    /// Attempts remaining
    /// </summary>
    public int AttemptsRemaining { get; set; } = 1;

    // State Management
    /// <summary>
    /// Current state ("Up"/"Down", "Open"/"Closed", "Locked"/"Unlocked")
    /// </summary>
    public string? CurrentState { get; set; }

    /// <summary>
    /// Available states for this object
    /// </summary>
    public List<string> States { get; set; } = new();

    /// <summary>
    /// Whether this object has been interacted with
    /// </summary>
    public bool Interacted { get; set; }

    /// <summary>
    /// Whether this object's state can be reversed (lever up/down)
    /// </summary>
    public bool Reversible { get; set; }

    // Lock Properties
    /// <summary>
    /// Whether this object is locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// What is required to unlock (e.g., "Keycard", "Console_Override")
    /// </summary>
    public string? Requires { get; set; }

    // Destructible Properties
    /// <summary>
    /// Whether this object can be destroyed
    /// </summary>
    public bool IsDestructible { get; set; }

    /// <summary>
    /// Hit points (for destructible objects)
    /// </summary>
    public int HP { get; set; }

    /// <summary>
    /// Current HP
    /// </summary>
    public int CurrentHP { get; set; }

    /// <summary>
    /// Damage reduction (soak)
    /// </summary>
    public int Soak { get; set; }

    /// <summary>
    /// Whether this object blocks line of sight
    /// </summary>
    public bool BlocksLoS { get; set; }

    // Loot Properties (for containers, corpses)
    /// <summary>
    /// Loot table ID reference
    /// </summary>
    public int? LootTableId { get; set; }

    /// <summary>
    /// Loot tier (Common, Uncommon, Rare)
    /// </summary>
    public LootTier LootTier { get; set; }

    /// <summary>
    /// Whether loot has been taken
    /// </summary>
    public bool LootTaken { get; set; }

    // Trap Properties
    /// <summary>
    /// Whether this is a trapped object
    /// </summary>
    public bool IsTrap { get; set; }

    /// <summary>
    /// Trap damage (e.g., "2d6")
    /// </summary>
    public string? TrapDamage { get; set; }

    /// <summary>
    /// Whether trap has been detected
    /// </summary>
    public bool TrapDetected { get; set; }

    /// <summary>
    /// Whether trap has been disarmed
    /// </summary>
    public bool TrapDisarmed { get; set; }

    /// <summary>
    /// Trap chance (0.0-1.0) for containers
    /// </summary>
    public float TrapChance { get; set; }

    // Consequence Properties
    /// <summary>
    /// Consequence type (Unlock, Trigger, Spawn, Reveal, Loot, Trap)
    /// </summary>
    public ConsequenceType ConsequenceType { get; set; }

    /// <summary>
    /// Consequence data (JSON)
    /// </summary>
    public string? ConsequenceData { get; set; }

    /// <summary>
    /// Failure consequence (for failed checks)
    /// </summary>
    public string? FailureConsequence { get; set; }

    /// <summary>
    /// Biome restriction (if any)
    /// </summary>
    public string? BiomeRestriction { get; set; }

    /// <summary>
    /// Tags for filtering and classification
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Validates that this object has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Name)) return false;
        if (RoomId <= 0) return false;

        return true;
    }

    /// <summary>
    /// Gets a summary of this object for display
    /// </summary>
    public string GetDisplaySummary()
    {
        var summary = new List<string>();

        summary.Add($"Type: {ObjectType}");
        summary.Add($"Interaction: {InteractionType}");

        if (RequiresCheck)
        {
            summary.Add($"Requires: {CheckType} Check (DC {CheckDC})");
        }

        if (IsLocked && !string.IsNullOrEmpty(Requires))
        {
            summary.Add($"Locked (Requires: {Requires})");
        }
        else if (IsLocked)
        {
            summary.Add("Locked");
        }

        if (CurrentState != null)
        {
            summary.Add($"State: {CurrentState}");
        }

        if (Interacted && !Reversible)
        {
            summary.Add("Already used");
        }

        return string.Join(" | ", summary);
    }

    /// <summary>
    /// Toggles state between available states
    /// </summary>
    public void ToggleState()
    {
        if (!Reversible || States.Count < 2 || CurrentState == null)
            return;

        var currentIndex = States.IndexOf(CurrentState);
        if (currentIndex == -1)
            return;

        var nextIndex = (currentIndex + 1) % States.Count;
        CurrentState = States[nextIndex];
    }

    /// <summary>
    /// Checks if object can be interacted with
    /// </summary>
    public bool CanInteract()
    {
        // Already interacted and not reversible
        if (Interacted && !Reversible)
            return false;

        // No attempts remaining
        if (AttemptsRemaining <= 0)
            return false;

        // Destroyed
        if (IsDestructible && CurrentHP <= 0)
            return false;

        return true;
    }
}
