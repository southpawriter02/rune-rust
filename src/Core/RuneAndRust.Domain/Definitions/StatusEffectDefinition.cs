using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a status effect's properties and behavior.
/// </summary>
/// <remarks>
/// <para>StatusEffectDefinition is an immutable configuration entity loaded from JSON.</para>
/// <para>Instances are shared across all active effects of the same type.</para>
/// </remarks>
public class StatusEffectDefinition
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Unique identifier for this effect (e.g., "bleeding", "hasted").</summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>Display name for this effect.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Description of what this effect does.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Category of this effect (Debuff, Buff, Environmental).</summary>
    public EffectCategory Category { get; private set; }

    /// <summary>Icon identifier for UI display.</summary>
    public string? IconId { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // DURATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>How duration is tracked for this effect.</summary>
    public DurationType DurationType { get; private set; }

    /// <summary>Base duration in turns (if DurationType is Turns).</summary>
    public int BaseDuration { get; private set; }

    /// <summary>Event that removes this effect (if DurationType is Triggered).</summary>
    public string? RemovalTrigger { get; private set; }

    /// <summary>Resource pool amount (if DurationType is ResourceBased).</summary>
    public int? ResourcePool { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // STACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>How this effect behaves when reapplied.</summary>
    public StackingRule StackingRule { get; private set; }

    /// <summary>Maximum stack count (if StackingRule is Stack).</summary>
    public int MaxStacks { get; private set; } = 1;

    // ═══════════════════════════════════════════════════════════════
    // EFFECTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Stat modifications applied by this effect.</summary>
    public IReadOnlyList<StatModifier> StatModifiers => _statModifiers.AsReadOnly();
    private readonly List<StatModifier> _statModifiers = new();

    /// <summary>Damage dealt per turn (DoT).</summary>
    public int? DamagePerTurn { get; private set; }

    /// <summary>Damage type for DoT (e.g., "physical", "fire", "poison").</summary>
    public string? DamageType { get; private set; }

    /// <summary>Healing applied per turn (HoT).</summary>
    public int? HealingPerTurn { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // BEHAVIORAL FLAGS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Whether this effect prevents all actions.</summary>
    public bool PreventsActions { get; private set; }

    /// <summary>Whether this effect prevents movement.</summary>
    public bool PreventsMovement { get; private set; }

    /// <summary>Whether this effect prevents ability use.</summary>
    public bool PreventsAbilities { get; private set; }

    /// <summary>Whether this effect prevents attacking.</summary>
    public bool PreventsAttacking { get; private set; }

    /// <summary>Whether this effect is hidden from the target.</summary>
    public bool IsHidden { get; private set; }

    /// <summary>Whether this effect is beneficial (buff).</summary>
    public bool IsBeneficial => Category == EffectCategory.Buff;

    /// <summary>Whether this effect is harmful (debuff).</summary>
    public bool IsHarmful => Category == EffectCategory.Debuff;

    // ═══════════════════════════════════════════════════════════════
    // INTERACTIONS (metadata for v0.0.6d)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Effect IDs that this effect grants immunity to.</summary>
    public IReadOnlyList<string> GrantsImmunityTo => _grantsImmunityTo.AsReadOnly();
    private readonly List<string> _grantsImmunityTo = new();

    /// <summary>Damage types that this effect grants resistance to.</summary>
    public IReadOnlyList<string> ResistsDamageTypes => _resistsDamageTypes.AsReadOnly();
    private readonly List<string> _resistsDamageTypes = new();

    /// <summary>Damage types that this effect grants vulnerability to.</summary>
    public IReadOnlyList<string> VulnerableToDamageTypes => _vulnerableToDamageTypes.AsReadOnly();
    private readonly List<string> _vulnerableToDamageTypes = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Private constructor for controlled creation.</summary>
    private StatusEffectDefinition() { }

    /// <summary>
    /// Creates a status effect definition with required properties.
    /// </summary>
    public static StatusEffectDefinition Create(
        string id,
        string name,
        string description,
        EffectCategory category,
        DurationType durationType,
        int baseDuration = 0,
        StackingRule stackingRule = StackingRule.RefreshDuration,
        int maxStacks = 1,
        string? iconId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new StatusEffectDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Category = category,
            DurationType = durationType,
            BaseDuration = baseDuration,
            StackingRule = stackingRule,
            MaxStacks = Math.Max(1, maxStacks),
            IconId = iconId
        };
    }

    /// <summary>Adds a stat modifier to this effect.</summary>
    public StatusEffectDefinition WithStatModifier(StatModifier modifier)
    {
        _statModifiers.Add(modifier);
        return this;
    }

    /// <summary>Adds multiple stat modifiers to this effect.</summary>
    public StatusEffectDefinition WithStatModifiers(IEnumerable<StatModifier> modifiers)
    {
        _statModifiers.AddRange(modifiers);
        return this;
    }

    /// <summary>Sets damage over time for this effect.</summary>
    public StatusEffectDefinition WithDamageOverTime(int damage, string damageType)
    {
        DamagePerTurn = damage;
        DamageType = damageType;
        return this;
    }

    /// <summary>Sets healing over time for this effect.</summary>
    public StatusEffectDefinition WithHealingOverTime(int healing)
    {
        HealingPerTurn = healing;
        return this;
    }

    /// <summary>Sets action prevention flags.</summary>
    public StatusEffectDefinition WithActionPrevention(
        bool preventsActions = false,
        bool preventsMovement = false,
        bool preventsAbilities = false,
        bool preventsAttacking = false)
    {
        PreventsActions = preventsActions;
        PreventsMovement = preventsMovement;
        PreventsAbilities = preventsAbilities;
        PreventsAttacking = preventsAttacking;
        return this;
    }

    /// <summary>Sets triggered removal condition.</summary>
    public StatusEffectDefinition WithRemovalTrigger(string trigger)
    {
        RemovalTrigger = trigger;
        return this;
    }

    /// <summary>Sets resource pool for resource-based effects.</summary>
    public StatusEffectDefinition WithResourcePool(int amount)
    {
        ResourcePool = amount;
        return this;
    }

    /// <summary>Adds immunity grants to this effect.</summary>
    public StatusEffectDefinition WithImmunityTo(params string[] effectIds)
    {
        _grantsImmunityTo.AddRange(effectIds);
        return this;
    }

    /// <summary>Adds damage resistance to this effect.</summary>
    public StatusEffectDefinition WithDamageResistance(params string[] damageTypes)
    {
        _resistsDamageTypes.AddRange(damageTypes);
        return this;
    }

    /// <summary>Adds damage vulnerability to this effect.</summary>
    public StatusEffectDefinition WithDamageVulnerability(params string[] damageTypes)
    {
        _vulnerableToDamageTypes.AddRange(damageTypes);
        return this;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({Id})";
}
