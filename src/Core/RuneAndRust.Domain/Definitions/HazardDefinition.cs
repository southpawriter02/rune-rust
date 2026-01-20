using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration definition for a hazard type loaded from JSON.
/// </summary>
/// <remarks>
/// HazardDefinition is the configuration template used to create HazardZone instances.
/// Definitions are loaded from config/hazards.json and define the behavior of each hazard type.
/// </remarks>
public class HazardDefinition
{
    /// <summary>Gets or sets the unique identifier for this hazard definition (kebab-case).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name shown to players when the hazard is encountered.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description shown when examining the hazard.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of hazard for categorization and behavior.</summary>
    public HazardType HazardType { get; set; }

    /// <summary>Gets or sets the damage dealt each turn while in the hazard zone.</summary>
    public HazardDamageDefinition? DamagePerTurn { get; set; }

    /// <summary>Gets or sets the damage dealt when first entering the hazard zone.</summary>
    public HazardDamageDefinition? EntryDamage { get; set; }

    /// <summary>Gets or sets the status effects applied by this hazard.</summary>
    public List<string> StatusEffects { get; set; } = new();

    /// <summary>Gets or sets the duration of applied status effects in turns.</summary>
    public int StatusDuration { get; set; } = 3;

    /// <summary>Gets or sets the saving throw configuration for this hazard.</summary>
    public HazardSaveDefinition? Save { get; set; }

    /// <summary>Gets or sets whether this hazard affects the entire room (default: true).</summary>
    public bool AffectsWholeRoom { get; set; } = true;

    /// <summary>Gets or sets the duration in turns (-1 = permanent, positive = temporary).</summary>
    public int Duration { get; set; } = -1;

    /// <summary>Gets or sets the keywords used to identify this hazard in commands.</summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>Gets or sets the custom message displayed when the hazard effect triggers.</summary>
    public string? EffectMessage { get; set; }
}

/// <summary>
/// Configuration for hazard damage parameters.
/// </summary>
public class HazardDamageDefinition
{
    /// <summary>Gets or sets the dice notation for damage (e.g., "2d6", "1d8+2").</summary>
    public string Dice { get; set; } = string.Empty;

    /// <summary>Gets or sets the damage type identifier (e.g., "fire", "poison", "cold").</summary>
    public string DamageType { get; set; } = string.Empty;

    /// <summary>Gets or sets the flat bonus damage added to the roll.</summary>
    public int Bonus { get; set; }
}

/// <summary>
/// Configuration for hazard saving throw parameters.
/// </summary>
public class HazardSaveDefinition
{
    /// <summary>Gets or sets the attribute used for the save (e.g., "Fortitude", "Agility", "Will").</summary>
    public string Attribute { get; set; } = string.Empty;

    /// <summary>Gets or sets the Difficulty Class to beat.</summary>
    public int DC { get; set; }

    /// <summary>Gets or sets whether a successful save negates (true) or halves (false) the effect.</summary>
    public bool Negates { get; set; }
}
