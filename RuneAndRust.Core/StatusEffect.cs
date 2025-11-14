namespace RuneAndRust.Core;

/// <summary>
/// v0.21.3: Advanced Status Effect System
/// Represents a single active status effect instance on a character
/// </summary>
public class StatusEffect
{
    public int EffectInstanceID { get; set; }
    public int TargetID { get; set; } // Character or Enemy ID
    public string EffectType { get; set; } = string.Empty; // "Bleeding", "Stunned", etc.
    public int StackCount { get; set; } = 1;
    public int DurationRemaining { get; set; }
    public int AppliedBy { get; set; } // Who applied this effect
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// v2.0 Canonical effect categories
    /// </summary>
    public StatusEffectCategory Category { get; set; }

    /// <summary>
    /// Whether this effect can stack with itself
    /// </summary>
    public bool CanStack { get; set; }

    /// <summary>
    /// Maximum stacks allowed
    /// </summary>
    public int MaxStacks { get; set; } = 1;

    /// <summary>
    /// Whether this effect ignores Soak (armor)
    /// </summary>
    public bool IgnoresSoak { get; set; } = false;

    /// <summary>
    /// Base damage dice for DoT effects (e.g., "1d6" for Bleeding)
    /// </summary>
    public string? DamageBase { get; set; }

    /// <summary>
    /// Metadata for effect-specific behavior
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// v2.0 Canonical status effect categories
/// </summary>
public enum StatusEffectCategory
{
    ControlDebuff,      // Stunned, Rooted, Feared, Disoriented, Slowed
    DamageOverTime,     // Bleeding, Poisoned, Corroded, Burning
    StatModification,   // Vulnerable, Analyzed, Weakened, Fortified
    Buff                // Hasted, Shielded, Inspired
}

/// <summary>
/// Status effect interaction types
/// </summary>
public enum StatusInteractionType
{
    Conversion,      // Multiple applications convert to different effect (Disoriented → Stunned)
    Amplification,   // One effect amplifies another (Bleeding + Corroded)
    Suppression      // Effects cancel each other (Slowed + Hasted)
}

/// <summary>
/// Defines how status effects interact with each other
/// </summary>
public class StatusInteraction
{
    public int InteractionID { get; set; }
    public StatusInteractionType InteractionType { get; set; }
    public string PrimaryEffect { get; set; } = string.Empty;
    public string? SecondaryEffect { get; set; }
    public int RequiredApplications { get; set; } = 2; // For conversions
    public string? ResultEffect { get; set; } // For conversions
    public int ResultDuration { get; set; } = 1; // For conversions
    public float Multiplier { get; set; } = 1.0f; // For amplifications
    public string Resolution { get; set; } = string.Empty; // For suppressions: "Cancel", "PrimaryWins"
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Result of applying a status effect
/// </summary>
public class StatusApplicationResult
{
    public bool Success { get; set; }
    public string EffectType { get; set; } = string.Empty;
    public int CurrentStacks { get; set; }
    public bool ConversionTriggered { get; set; }
    public string? ConvertedTo { get; set; }
    public List<string> ActiveInteractions { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
