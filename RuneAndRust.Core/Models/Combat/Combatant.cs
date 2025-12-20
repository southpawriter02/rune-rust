using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Adapter that wraps a Character or Enemy for use during combat.
/// Holds combat-volatile state (initiative, current HP) without modifying the source entity.
/// </summary>
public class Combatant
{
    /// <summary>
    /// Unique identifier for this combatant instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for this combatant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this combatant is the player character.
    /// </summary>
    public bool IsPlayer { get; set; }

    #region Combat-Volatile State

    /// <summary>
    /// Initiative roll result for turn order determination.
    /// </summary>
    public int Initiative { get; set; }

    /// <summary>
    /// Current HP during combat. Copied from source at combat start.
    /// </summary>
    public int CurrentHp { get; set; }

    /// <summary>
    /// Maximum HP for this combatant.
    /// </summary>
    public int MaxHp { get; set; }

    /// <summary>
    /// Current stamina during combat. Copied from source at combat start.
    /// </summary>
    public int CurrentStamina { get; set; }

    /// <summary>
    /// Maximum stamina for this combatant.
    /// </summary>
    public int MaxStamina { get; set; }

    /// <summary>
    /// Current Aether Points (magical energy pool) during combat.
    /// Copied from source at combat start. Does NOT regenerate.
    /// </summary>
    public int CurrentAp { get; set; }

    /// <summary>
    /// Maximum Aether Points for this combatant.
    /// Non-zero for Mystic archetype characters.
    /// </summary>
    public int MaxAp { get; set; }

    /// <summary>
    /// Active status effects on this combatant during combat.
    /// Combat-volatile: cleared when combat ends.
    /// </summary>
    public List<ActiveStatusEffect> StatusEffects { get; set; } = new();

    /// <summary>
    /// Combat archetype for AI behavior selection (enemies only).
    /// Used by enemy AI in v0.2.2b to determine attack patterns.
    /// </summary>
    public EnemyArchetype Archetype { get; set; } = EnemyArchetype.DPS;

    /// <summary>
    /// Tags for status effect interactions and special handling.
    /// Copied from source entity. Examples: "Mechanical", "Undying", "Beast".
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Active creature traits copied from source enemy.
    /// Used for runtime trait trigger checks in CombatService.
    /// </summary>
    public List<CreatureTraitType> ActiveTraits { get; set; } = new();

    /// <summary>
    /// Whether this combatant is currently defending (temporary soak bonus).
    /// Reset at the start of their next turn.
    /// </summary>
    public bool IsDefending { get; set; } = false;

    /// <summary>
    /// Ability cooldowns tracked per-ability. Key = AbilityId, Value = turns remaining.
    /// Combat-volatile: cleared when combat ends.
    /// </summary>
    public Dictionary<Guid, int> Cooldowns { get; set; } = new();

    #endregion

    #region Equipment Snapshot

    /// <summary>
    /// The damage die size for the combatant's weapon (e.g., 6 for d6).
    /// Defaults to 4 (unarmed d4) if no weapon is equipped.
    /// </summary>
    public int WeaponDamageDie { get; set; } = 4;

    /// <summary>
    /// Accuracy bonus from the combatant's weapon.
    /// </summary>
    public int WeaponAccuracyBonus { get; set; } = 0;

    /// <summary>
    /// Total armor soak value from all equipped armor pieces.
    /// </summary>
    public int ArmorSoak { get; set; } = 0;

    /// <summary>
    /// Display name for the combatant's weapon.
    /// </summary>
    public string WeaponName { get; set; } = "Fists";

    #endregion

    #region Source References

    /// <summary>
    /// Reference to the source Character entity if this is a player combatant.
    /// </summary>
    public CharacterEntity? CharacterSource { get; set; }

    /// <summary>
    /// Reference to the source Enemy entity if this is an enemy combatant.
    /// </summary>
    public Enemy? EnemySource { get; set; }

    #endregion

    /// <summary>
    /// Gets the effective attribute value for the specified attribute.
    /// Uses the source entity's attribute system.
    /// </summary>
    /// <param name="attr">The attribute to retrieve.</param>
    /// <returns>The effective attribute value, or 0 if no source is set.</returns>
    public int GetAttribute(CharacterAttribute attr)
    {
        if (CharacterSource != null) return CharacterSource.GetEffectiveAttribute(attr);
        if (EnemySource != null) return EnemySource.GetAttribute(attr);
        return 0;
    }

    /// <summary>
    /// Creates a Combatant from a Character entity.
    /// Snapshots equipment stats at combat start to avoid DB access during combat.
    /// </summary>
    /// <param name="c">The source Character.</param>
    /// <returns>A new Combatant wrapping the Character with equipment stats cached.</returns>
    public static Combatant FromCharacter(CharacterEntity c)
    {
        // Find equipped weapon (MainHand slot)
        var weapon = c.Inventory?
            .Where(i => i.IsEquipped && i.Item is Equipment eq && eq.Slot == EquipmentSlot.MainHand)
            .Select(i => i.Item as Equipment)
            .FirstOrDefault();

        // Calculate total soak from all equipped armor pieces
        var totalSoak = c.Inventory?
            .Where(i => i.IsEquipped && i.Item is Equipment)
            .Sum(i => ((Equipment)i.Item!).SoakBonus) ?? 0;

        return new Combatant
        {
            Name = c.Name,
            IsPlayer = true,
            CharacterSource = c,
            CurrentHp = c.CurrentHP,
            MaxHp = c.MaxHP,
            CurrentStamina = c.CurrentStamina,
            MaxStamina = c.MaxStamina,
            // Aether Pool (v0.2.3a)
            CurrentAp = c.CurrentAp,
            MaxAp = c.MaxAp,
            // Equipment snapshot
            WeaponDamageDie = weapon?.DamageDie ?? 4,
            WeaponAccuracyBonus = 0, // Future: derive from weapon attributes
            ArmorSoak = totalSoak,
            WeaponName = weapon?.Name ?? "Fists"
        };
    }

    /// <summary>
    /// Creates a Combatant from an Enemy entity.
    /// Copies enemy equipment stats for use during combat.
    /// </summary>
    /// <param name="e">The source Enemy.</param>
    /// <returns>A new Combatant wrapping the Enemy with equipment stats.</returns>
    public static Combatant FromEnemy(Enemy e) => new()
    {
        Name = e.Name,
        IsPlayer = false,
        EnemySource = e,
        CurrentHp = e.CurrentHp,
        MaxHp = e.MaxHp,
        CurrentStamina = e.CurrentStamina,
        MaxStamina = e.MaxStamina,
        // Enemy equipment stats
        WeaponDamageDie = e.WeaponDamageDie,
        WeaponAccuracyBonus = e.WeaponAccuracyBonus,
        ArmorSoak = e.ArmorSoak,
        WeaponName = e.WeaponName,
        // Template/AI properties (v0.2.2a)
        Archetype = e.Archetype,
        Tags = new List<string>(e.Tags),
        // Trait properties (v0.2.2c)
        ActiveTraits = new List<CreatureTraitType>(e.ActiveTraits)
    };
}
