using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Adapter that wraps a Character or Enemy for use during combat.
/// Holds combat-volatile state (initiative, current HP) without modifying the source entity.
/// </summary>
/// <remarks>
/// See: SPEC-COMBAT-001 for Combat System design.
/// See: SPEC-INTENT-001 for Enemy Intent & Telegraph System design (PlannedAction, IsIntentRevealed).
/// </remarks>
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
    /// Current psychic stress during combat. Copied from source at combat start.
    /// Stress 0-100 with tiered status effects. Affects defense calculation.
    /// </summary>
    public int CurrentStress { get; set; } = 0;

    /// <summary>
    /// Maximum stress threshold. Fixed at 100 for all combatants.
    /// Reaching 100 triggers a Breaking Point event.
    /// </summary>
    public int MaxStress { get; set; } = 100;

    /// <summary>
    /// Current Runic Blight corruption. Copied from source at combat start.
    /// Corruption 0-100 with tiered penalties. Affects Max AP and attributes.
    /// Unlike Stress, Corruption is permanent and not mitigated by WILL.
    /// </summary>
    public int CurrentCorruption { get; set; } = 0;

    /// <summary>
    /// Maximum corruption threshold. Fixed at 100 for all combatants.
    /// Reaching 100 triggers Terminal Error (character becomes a Forlorn).
    /// </summary>
    public int MaxCorruption { get; set; } = 100;

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

    #region Row System (v0.3.6a)

    /// <summary>
    /// The combatant's row position on the battlefield (v0.3.6a).
    /// Affects melee targeting - Back Row is protected by Front Row.
    /// </summary>
    public RowPosition Row { get; set; } = RowPosition.Front;

    /// <summary>
    /// Whether this combatant is currently being targeted (v0.3.6a).
    /// Used for visual highlighting in the combat grid.
    /// </summary>
    public bool IsTargeted { get; set; } = false;

    #endregion

    #region Spatial Positioning (v0.3.18b)

    /// <summary>
    /// The combatant's position on the combat grid (v0.3.18b - The Hot Path).
    /// Used by SpatialHashGrid for O(1) occupancy checks and A* pathfinding.
    /// </summary>
    public Coordinate Position { get; set; } = Coordinate.Origin;

    #endregion

    #region Intent System (v0.3.6c)

    /// <summary>
    /// The action the AI intends to take this round (v0.3.6c).
    /// Calculated at round start and on state changes (HP, status).
    /// Null for player combatants.
    /// </summary>
    public CombatAction? PlannedAction { get; set; }

    /// <summary>
    /// Whether the player has successfully perceived this enemy's intent (v0.3.6c).
    /// Set by CombatService based on WITS check or Analyzed status.
    /// Always false for player combatants.
    /// </summary>
    public bool IsIntentRevealed { get; set; } = false;

    #endregion

    #region Telegraphed Ability System (v0.2.4c)

    /// <summary>
    /// The ability being channeled during Chanting state (v0.2.4c).
    /// Set when charge begins, cleared on release or interruption.
    /// Null when not channeling.
    /// </summary>
    public Guid? ChanneledAbilityId { get; set; }

    /// <summary>
    /// The spell being channeled during Chanting state (v0.4.3c).
    /// Set when charge begins, cleared on release or interruption.
    /// Null when not channeling a spell.
    /// </summary>
    public Guid? ChanneledSpellId { get; set; }

    #endregion

    #region Ambient Condition Modifiers (v0.3.3b)

    /// <summary>
    /// The active ambient condition type affecting this combatant, if any.
    /// Set at combat start based on room condition.
    /// </summary>
    public ConditionType? ActiveCondition { get; set; }

    /// <summary>
    /// Condition-based modifier to Sturdiness (v0.3.3b). Applied at combat start.
    /// </summary>
    public int ConditionSturdinessModifier { get; set; } = 0;

    /// <summary>
    /// Condition-based modifier to Finesse (v0.3.3b). Applied at combat start.
    /// </summary>
    public int ConditionFinesseModifier { get; set; } = 0;

    /// <summary>
    /// Condition-based modifier to Wits (v0.3.3b). Applied at combat start.
    /// </summary>
    public int ConditionWitsModifier { get; set; } = 0;

    /// <summary>
    /// Condition-based modifier to Will (v0.3.3b). Applied at combat start.
    /// </summary>
    public int ConditionWillModifier { get; set; } = 0;

    #endregion

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

    /// <summary>
    /// List of abilities available to this combatant during combat.
    /// Loaded from the database based on archetype at combat start.
    /// </summary>
    public List<ActiveAbility> Abilities { get; set; } = new();

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
    /// <param name="abilities">Optional list of abilities to assign to this combatant.</param>
    /// <returns>A new Combatant wrapping the Character with equipment stats cached.</returns>
    public static Combatant FromCharacter(CharacterEntity c, IEnumerable<ActiveAbility>? abilities = null)
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
            // Psychic Stress (v0.3.0a)
            CurrentStress = c.PsychicStress,
            MaxStress = 100,
            // Runic Blight Corruption (v0.3.0b)
            CurrentCorruption = c.Corruption,
            MaxCorruption = 100,
            // Equipment snapshot
            WeaponDamageDie = weapon?.DamageDie ?? 4,
            WeaponAccuracyBonus = 0, // Future: derive from weapon attributes
            ArmorSoak = totalSoak,
            WeaponName = weapon?.Name ?? "Fists",
            // Abilities (v0.2.3c)
            Abilities = abilities?.ToList() ?? new List<ActiveAbility>()
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
        // Psychic Stress (v0.3.0a) - enemies start at 0
        CurrentStress = 0,
        MaxStress = 100,
        // Runic Blight Corruption (v0.3.0b) - enemies start at 0
        CurrentCorruption = 0,
        MaxCorruption = 100,
        // Enemy equipment stats
        WeaponDamageDie = e.WeaponDamageDie,
        WeaponAccuracyBonus = e.WeaponAccuracyBonus,
        ArmorSoak = e.ArmorSoak,
        WeaponName = e.WeaponName,
        // Template/AI properties (v0.2.2a)
        Archetype = e.Archetype,
        Tags = new List<string>(e.Tags),
        // Trait properties (v0.2.2c)
        ActiveTraits = new List<CreatureTraitType>(e.ActiveTraits),
        // Abilities (v0.2.4a)
        Abilities = e.Abilities?.ToList() ?? new List<ActiveAbility>()
    };
}
