using RuneAndRust.Core.Entities;
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
    /// </summary>
    /// <param name="c">The source Character.</param>
    /// <returns>A new Combatant wrapping the Character.</returns>
    public static Combatant FromCharacter(CharacterEntity c) => new()
    {
        Name = c.Name,
        IsPlayer = true,
        CharacterSource = c,
        CurrentHp = c.CurrentHP,
        MaxHp = c.MaxHP,
        CurrentStamina = c.CurrentStamina,
        MaxStamina = c.MaxStamina
    };

    /// <summary>
    /// Creates a Combatant from an Enemy entity.
    /// </summary>
    /// <param name="e">The source Enemy.</param>
    /// <returns>A new Combatant wrapping the Enemy.</returns>
    public static Combatant FromEnemy(Enemy e) => new()
    {
        Name = e.Name,
        IsPlayer = false,
        EnemySource = e,
        CurrentHp = e.CurrentHp,
        MaxHp = e.MaxHp,
        CurrentStamina = e.CurrentStamina,
        MaxStamina = e.MaxStamina
    };
}
