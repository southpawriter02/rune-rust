namespace RuneAndRust.Core;

/// <summary>
/// v0.21.3: Canonical v2.0 status effect definitions
/// Defines the base properties and behaviors of each status effect type
/// </summary>
public class StatusEffectDefinition
{
    public string EffectType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public StatusEffectCategory Category { get; set; }
    public bool CanStack { get; set; }
    public int MaxStacks { get; set; } = 1;
    public int DefaultDuration { get; set; } = 1;
    public bool IgnoresSoak { get; set; } = false;
    public string? DamageBase { get; set; } // "1d6", "1d4", etc.
    public string Description { get; set; } = string.Empty;
    public string V2QuoteSource { get; set; } = string.Empty; // v2.0 specification quote

    /// <summary>
    /// v2.0 Canonical Control Effects
    /// </summary>
    public static readonly StatusEffectDefinition[] ControlEffects = new[]
    {
        new StatusEffectDefinition
        {
            EffectType = "Stunned",
            DisplayName = "[Stunned]",
            Category = StatusEffectCategory.ControlDebuff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 1,
            Description = "Cannot take any actions (move/attack/ability). Most potent 'hard control' debuff.",
            V2QuoteSource = "Character suffering critical, temporary system crash"
        },
        new StatusEffectDefinition
        {
            EffectType = "Rooted",
            DisplayName = "[Rooted]",
            Category = StatusEffectCategory.ControlDebuff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 2,
            Description = "Cannot move, can still attack/use abilities."
        },
        new StatusEffectDefinition
        {
            EffectType = "Feared",
            DisplayName = "[Feared]",
            Category = StatusEffectCategory.ControlDebuff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 3,
            Description = "Must flee from source, cannot willingly approach."
        },
        new StatusEffectDefinition
        {
            EffectType = "Disoriented",
            DisplayName = "[Disoriented]",
            Category = StatusEffectCategory.ControlDebuff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 2,
            Description = "-2 to hit, cannot use complex abilities. Can convert to Stunned with 2 applications."
        },
        new StatusEffectDefinition
        {
            EffectType = "Slowed",
            DisplayName = "[Slowed]",
            Category = StatusEffectCategory.ControlDebuff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 3,
            Description = "Movement speed halved, -1 AP per turn."
        }
    };

    /// <summary>
    /// v2.0 Canonical Damage Over Time Effects
    /// </summary>
    public static readonly StatusEffectDefinition[] DamageOverTimeEffects = new[]
    {
        new StatusEffectDefinition
        {
            EffectType = "Bleeding",
            DisplayName = "[Bleeding]",
            Category = StatusEffectCategory.DamageOverTime,
            CanStack = true,
            MaxStacks = 5,
            DefaultDuration = 5,
            IgnoresSoak = true,
            DamageBase = "1d6",
            Description = "1d6 damage per stack at start of turn. Ignores Soak. Physical system crash.",
            V2QuoteSource = "Catastrophic breach in target's physical 'hardware'"
        },
        new StatusEffectDefinition
        {
            EffectType = "Poisoned",
            DisplayName = "[Poisoned]",
            Category = StatusEffectCategory.DamageOverTime,
            CanStack = true,
            MaxStacks = 3,
            DefaultDuration = 4,
            DamageBase = "1d4",
            Description = "1d4 damage per stack at start of turn. Reduces healing received by 50%."
        },
        new StatusEffectDefinition
        {
            EffectType = "Corroded",
            DisplayName = "[Corroded]",
            Category = StatusEffectCategory.DamageOverTime,
            CanStack = true,
            MaxStacks = 5,
            DefaultDuration = -1, // Permanent until cleansed
            DamageBase = "1d4",
            Description = "-1 Soak per stack. 1d4 damage per stack at end of turn. Permanent until cleansed."
        }
    };

    /// <summary>
    /// v2.0 Canonical Stat Modification Effects
    /// </summary>
    public static readonly StatusEffectDefinition[] StatModificationEffects = new[]
    {
        new StatusEffectDefinition
        {
            EffectType = "Vulnerable",
            DisplayName = "[Vulnerable]",
            Category = StatusEffectCategory.StatModification,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 3,
            Description = "+25% damage taken from all sources."
        },
        new StatusEffectDefinition
        {
            EffectType = "Analyzed",
            DisplayName = "[Analyzed]",
            Category = StatusEffectCategory.StatModification,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 4,
            Description = "All attackers gain +2 accuracy dice against this target."
        }
    };

    /// <summary>
    /// Buff Effects
    /// </summary>
    public static readonly StatusEffectDefinition[] BuffEffects = new[]
    {
        new StatusEffectDefinition
        {
            EffectType = "Hasted",
            DisplayName = "[Hasted]",
            Category = StatusEffectCategory.Buff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 3,
            Description = "Movement speed doubled, +1 AP per turn."
        },
        new StatusEffectDefinition
        {
            EffectType = "Inspired",
            DisplayName = "[Inspired]",
            Category = StatusEffectCategory.Buff,
            CanStack = false,
            MaxStacks = 1,
            DefaultDuration = 3,
            Description = "+3 damage dice before rolling."
        }
    };

    /// <summary>
    /// Get all canonical status effect definitions
    /// </summary>
    public static IEnumerable<StatusEffectDefinition> GetAllDefinitions()
    {
        return ControlEffects
            .Concat(DamageOverTimeEffects)
            .Concat(StatModificationEffects)
            .Concat(BuffEffects);
    }

    /// <summary>
    /// Get definition by effect type
    /// </summary>
    public static StatusEffectDefinition? GetDefinition(string effectType)
    {
        return GetAllDefinitions()
            .FirstOrDefault(d => d.EffectType.Equals(effectType, StringComparison.OrdinalIgnoreCase));
    }
}
