using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a hostile creature in the dungeon that can engage in combat with players.
/// </summary>
/// <remarks>
/// Monsters have their own health, attack, and defense stats. They can take damage during
/// combat and are defeated when their health reaches zero.
/// </remarks>
public class Monster : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this monster.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this monster.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of this monster shown to players.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the monster's current health points.
    /// </summary>
    public int Health { get; private set; }

    /// <summary>
    /// Gets the monster's maximum health points.
    /// </summary>
    public int MaxHealth { get; private set; }

    /// <summary>
    /// Gets the monster's combat statistics (max health, attack, defense).
    /// </summary>
    public Stats Stats { get; private set; }

    /// <summary>
    /// Gets the experience points awarded when this monster is defeated.
    /// </summary>
    /// <remarks>
    /// This value is added to the player's experience upon defeating the monster.
    /// Higher-level or more difficult monsters award more experience.
    /// </remarks>
    public int ExperienceValue { get; private set; }

    /// <summary>
    /// Gets the initiative modifier for combat turn order.
    /// </summary>
    /// <remarks>
    /// Higher values mean the monster acts earlier in combat.
    /// Typically based on creature agility/reflexes.
    /// Default is 0 for average creatures.
    /// </remarks>
    public int InitiativeModifier { get; private set; } = 0;

    /// <summary>
    /// Gets the definition ID this monster was created from.
    /// </summary>
    /// <remarks>
    /// Used for grouping duplicate monsters and loading configuration.
    /// Null for dynamically created monsters.
    /// </remarks>
    public string? MonsterDefinitionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this monster has been defeated (health is zero or less).
    /// </summary>
    public bool IsDefeated => Health <= 0;

    /// <summary>
    /// Gets a value indicating whether this monster is still alive (health greater than zero).
    /// </summary>
    public bool IsAlive => Health > 0;

    // ===== AI Behavior Properties (v0.0.6b) =====

    /// <summary>
    /// Gets the AI behavior pattern for this monster.
    /// </summary>
    /// <remarks>
    /// Determines how the monster makes decisions during combat.
    /// Defaults to <see cref="AIBehavior.Aggressive"/> if not set.
    /// </remarks>
    public AIBehavior Behavior { get; private set; } = AIBehavior.Aggressive;

    /// <summary>
    /// Gets whether this monster can heal itself or allies.
    /// </summary>
    public bool CanHeal { get; private set; }

    /// <summary>
    /// Gets the heal amount if the monster can heal.
    /// </summary>
    public int? HealAmount { get; private set; }

    /// <summary>
    /// Gets the damage resistances for this monster instance.
    /// </summary>
    /// <remarks>
    /// Copied from MonsterDefinition.BaseResistances on spawn.
    /// May be modified by traits or temporary effects.
    /// </remarks>
    public DamageResistances Resistances { get; private set; } = DamageResistances.None;

    // ===== Tier & Trait Properties (v0.0.9c) =====

    /// <summary>
    /// Gets the tier ID this monster was spawned with.
    /// </summary>
    /// <remarks>
    /// References a TierDefinition by ID (e.g., "common", "elite", "boss").
    /// Determines stat multipliers and display properties.
    /// </remarks>
    public string TierId { get; private set; } = "common";

    /// <summary>
    /// Gets the list of trait IDs this monster has.
    /// </summary>
    /// <remarks>
    /// References MonsterTrait definitions by ID.
    /// Traits provide special abilities and behaviors.
    /// </remarks>
    public IReadOnlyList<string> TraitIds { get; private set; } = [];

    /// <summary>
    /// Gets the display name including tier prefix or generated name.
    /// </summary>
    /// <remarks>
    /// For common monsters: "Goblin"
    /// For elite monsters: "Elite Goblin"
    /// For named monsters: "Grok the Goblin"
    /// </remarks>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this monster is a Named tier with a unique name.
    /// </summary>
    public bool IsNamed { get; private set; } = false;

    /// <summary>
    /// Gets the color for displaying this monster (from tier).
    /// </summary>
    /// <remarks>
    /// Should be a valid Spectre.Console color name.
    /// Used by the renderer for colored monster names.
    /// </remarks>
    public string DisplayColor { get; private set; } = "white";

    /// <summary>
    /// Gets the loot multiplier from the monster's tier.
    /// </summary>
    /// <remarks>
    /// Used by LootService in v0.0.9d to scale drops and currency.
    /// </remarks>
    public float LootMultiplier { get; private set; } = 1.0f;

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Monster()
    {
        Name = null!;
        Description = null!;
        DisplayName = null!;
    }

    /// <summary>
    /// Creates a new monster with the specified properties.
    /// </summary>
    /// <param name="name">The display name of the monster.</param>
    /// <param name="description">The description shown to players.</param>
    /// <param name="maxHealth">The maximum health points of the monster.</param>
    /// <param name="stats">The combat statistics for the monster.</param>
    /// <param name="initiativeModifier">The initiative modifier (default 0).</param>
    /// <param name="monsterDefinitionId">The definition ID for grouping (optional).</param>
    /// <param name="experienceValue">The XP awarded when defeated (default 0).</param>
    /// <param name="resistances">The damage resistances for this monster (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxHealth is not positive.</exception>
    public Monster(
        string name,
        string description,
        int maxHealth,
        Stats stats,
        int initiativeModifier = 0,
        string? monsterDefinitionId = null,
        int experienceValue = 0,
        DamageResistances? resistances = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MaxHealth = maxHealth > 0 ? maxHealth : throw new ArgumentOutOfRangeException(nameof(maxHealth));
        Health = maxHealth;
        Stats = stats;
        InitiativeModifier = initiativeModifier;
        MonsterDefinitionId = monsterDefinitionId;
        ExperienceValue = Math.Max(0, experienceValue);
        Resistances = resistances ?? DamageResistances.None;
        DisplayName = name; // Default display name to base name for backwards compatibility
    }

    // ===== Tier & Trait Methods (v0.0.9c) =====

    /// <summary>
    /// Sets the tier information for this monster.
    /// </summary>
    /// <param name="tierId">The tier definition ID.</param>
    /// <param name="displayName">The formatted display name (may include tier prefix or generated name).</param>
    /// <param name="displayColor">The Spectre.Console color name for rendering.</param>
    /// <param name="lootMultiplier">The loot drop multiplier from the tier.</param>
    /// <param name="isNamed">Whether this monster has a unique generated name.</param>
    internal void SetTierInfo(
        string tierId,
        string displayName,
        string displayColor,
        float lootMultiplier,
        bool isNamed = false)
    {
        TierId = tierId ?? throw new ArgumentNullException(nameof(tierId));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        DisplayColor = displayColor ?? "white";
        LootMultiplier = lootMultiplier > 0 ? lootMultiplier : 1.0f;
        IsNamed = isNamed;
    }

    /// <summary>
    /// Sets the traits for this monster.
    /// </summary>
    /// <param name="traitIds">The list of trait definition IDs.</param>
    internal void SetTraits(IReadOnlyList<string> traitIds)
    {
        TraitIds = traitIds ?? [];
    }

    /// <summary>
    /// Applies a defense bonus from traits (e.g., Armored trait).
    /// </summary>
    /// <param name="defenseBonus">The defense bonus to add.</param>
    internal void ApplyDefenseBonus(int defenseBonus)
    {
        if (defenseBonus > 0)
        {
            Stats = new Stats(Stats.MaxHealth, Stats.Attack, Stats.Defense + defenseBonus);
        }
    }

    /// <summary>
    /// Applies damage to the monster, reduced by their defense stat.
    /// </summary>
    /// <param name="damage">The raw damage amount before defense reduction.</param>
    /// <returns>The actual damage dealt after defense calculation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when damage is negative.</exception>
    /// <remarks>
    /// Damage is reduced by the monster's defense stat (minimum 0 damage taken).
    /// The monster's health cannot go below zero.
    /// </remarks>
    public int TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");

        var actualDamage = Math.Max(0, damage - Stats.Defense);
        Health = Math.Max(0, Health - actualDamage);
        return actualDamage;
    }

    // ===== AI Behavior Methods (v0.0.6b) =====

    /// <summary>
    /// Sets the AI behavior for this monster.
    /// </summary>
    /// <param name="behavior">The behavior pattern to use.</param>
    public void SetBehavior(AIBehavior behavior)
    {
        Behavior = behavior;
    }

    /// <summary>
    /// Enables healing capability for this monster.
    /// </summary>
    /// <param name="healAmount">The amount of HP restored per heal action.</param>
    public void EnableHealing(int healAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(healAmount);
        CanHeal = true;
        HealAmount = healAmount;
    }

    /// <summary>
    /// Restores health to this monster (used for healing).
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    /// <returns>The actual amount healed (capped at MaxHealth).</returns>
    public int Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        var actualHeal = Math.Min(amount, MaxHealth - Health);
        Health += actualHeal;
        return actualHeal;
    }

    /// <summary>
    /// Factory method to create a basic goblin enemy with Cowardly behavior.
    /// </summary>
    /// <returns>A new goblin monster with 25 XP value.</returns>
    [Obsolete("Use IMonsterService.SpawnMonster(\"goblin\") instead. This method will be removed in a future version.")]
    public static Monster CreateGoblin()
    {
        return new(
            "Goblin",
            "A small, green creature with sharp teeth and beady eyes. It looks hostile.",
            30,
            new Stats(30, 8, 2),
            initiativeModifier: 1,
            monsterDefinitionId: "goblin",
            experienceValue: 25)
        {
            Behavior = AIBehavior.Cowardly
        };
    }

    /// <summary>
    /// Factory method to create a skeleton enemy with Aggressive behavior.
    /// </summary>
    /// <returns>A new skeleton monster with 20 XP value.</returns>
    [Obsolete("Use IMonsterService.SpawnMonster(\"skeleton\") instead. This method will be removed in a future version.")]
    public static Monster CreateSkeleton()
    {
        return new(
            "Skeleton",
            "An animated pile of bones held together by dark magic.",
            25,
            new Stats(25, 6, 3),
            initiativeModifier: 0,
            monsterDefinitionId: "skeleton",
            experienceValue: 20)
        {
            Behavior = AIBehavior.Aggressive
        };
    }

    /// <summary>
    /// Factory method to create an orc enemy with Aggressive behavior.
    /// </summary>
    /// <returns>A new orc monster with 40 XP value.</returns>
    [Obsolete("Use IMonsterService.SpawnMonster(\"orc\") instead. This method will be removed in a future version.")]
    public static Monster CreateOrc()
    {
        return new(
            "Orc",
            "A large, brutish creature with green skin and tusks. It wields a crude axe.",
            45,
            new Stats(45, 12, 4),
            initiativeModifier: -1,
            monsterDefinitionId: "orc",
            experienceValue: 40)
        {
            Behavior = AIBehavior.Aggressive
        };
    }

    /// <summary>
    /// Factory method to create a goblin shaman with Support behavior.
    /// </summary>
    /// <returns>A new goblin shaman monster with 30 XP value.</returns>
    [Obsolete("Use IMonsterService.SpawnMonster(\"goblin_shaman\") instead. This method will be removed in a future version.")]
    public static Monster CreateGoblinShaman()
    {
        var shaman = new Monster(
            "Goblin Shaman",
            "A goblin adorned with crude fetishes and glowing runes.",
            25,
            new Stats(25, 6, 1),
            initiativeModifier: 2,
            monsterDefinitionId: "goblin_shaman",
            experienceValue: 30)
        {
            Behavior = AIBehavior.Support,
            CanHeal = true,
            HealAmount = 10
        };
        return shaman;
    }

    /// <summary>
    /// Factory method to create a slime enemy with Chaotic behavior.
    /// </summary>
    /// <returns>A new slime monster with 15 XP value.</returns>
    [Obsolete("Use IMonsterService.SpawnMonster(\"slime\") instead. This method will be removed in a future version.")]
    public static Monster CreateSlime()
    {
        return new(
            "Slime",
            "A gelatinous blob that oozes across the floor.",
            40,
            new Stats(40, 5, 5),
            initiativeModifier: -2,
            monsterDefinitionId: "slime",
            experienceValue: 15)
        {
            Behavior = AIBehavior.Chaotic
        };
    }

    // ===== Vision & Light Properties (v0.4.3b) =====

    /// <summary>
    /// Gets the type of vision this monster has.
    /// </summary>
    /// <remarks>
    /// Affects how the monster perceives light levels.
    /// DarkVision allows seeing in darkness as dim light.
    /// </remarks>
    public VisionType VisionType { get; private set; } = VisionType.Normal;

    /// <summary>
    /// Gets whether this monster is sensitive to bright light.
    /// </summary>
    /// <remarks>
    /// Light-sensitive creatures suffer penalties in bright conditions.
    /// Common for underground or nocturnal creatures.
    /// </remarks>
    public bool LightSensitivity { get; private set; }

    /// <summary>
    /// Gets the accuracy penalty when in bright light (for light-sensitive creatures).
    /// </summary>
    public int LightSensitivityPenalty { get; private set; } = -2;

    /// <summary>
    /// Sets the vision type for this monster.
    /// </summary>
    /// <param name="visionType">The vision type to set.</param>
    public void SetVisionType(VisionType visionType) => VisionType = visionType;

    /// <summary>
    /// Configures light sensitivity for this monster.
    /// </summary>
    /// <param name="sensitive">Whether the creature is light-sensitive.</param>
    /// <param name="penalty">The accuracy penalty in bright light (default -2).</param>
    public void SetLightSensitivity(bool sensitive, int penalty = -2)
    {
        LightSensitivity = sensitive;
        LightSensitivityPenalty = penalty;
    }

    /// <summary>
    /// Gets the effective light level considering vision type.
    /// </summary>
    /// <param name="room">The room containing this monster.</param>
    /// <returns>The perceived light level.</returns>
    /// <remarks>
    /// DarkVision: Dark → Dim
    /// TrueSight: All → Bright
    /// </remarks>
    public LightLevel GetEffectiveLightLevel(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        var roomLight = room.CurrentLightLevel;

        return VisionType switch
        {
            VisionType.TrueSight => LightLevel.Bright,
            VisionType.DarkVision when roomLight == LightLevel.Dark => LightLevel.Dim,
            _ => roomLight
        };
    }

    /// <summary>
    /// Returns a string representation of this monster.
    /// </summary>
    /// <returns>A string containing the monster name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{MaxHealth})";
}
