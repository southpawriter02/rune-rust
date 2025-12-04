using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits.Handlers;

/// <summary>
/// Interface for category-specific trait handlers.
/// Each handler processes traits within its category range.
/// </summary>
public interface ITraitHandler
{
    /// <summary>
    /// The trait category this handler manages.
    /// </summary>
    TraitCategory Category { get; }

    /// <summary>
    /// Checks if this handler can process the given trait.
    /// </summary>
    bool CanHandle(CreatureTrait trait);

    // ========================================
    // Combat Event Hooks
    // ========================================

    /// <summary>
    /// Called when combat begins for an enemy with traits in this category.
    /// </summary>
    void OnCombatStart(Enemy enemy, TraitConfiguration config, CombatState state);

    /// <summary>
    /// Called at the start of an enemy's turn.
    /// </summary>
    void OnTurnStart(Enemy enemy, TraitConfiguration config, CombatState state);

    /// <summary>
    /// Called at the end of an enemy's turn.
    /// </summary>
    void OnTurnEnd(Enemy enemy, TraitConfiguration config, CombatState state);

    /// <summary>
    /// Called when an enemy moves.
    /// </summary>
    void OnMovement(Enemy enemy, TraitConfiguration config, GridPosition from, GridPosition to, CombatState state);

    /// <summary>
    /// Called when an enemy dies.
    /// </summary>
    void OnDeath(Enemy enemy, TraitConfiguration config, CombatState state);

    // ========================================
    // Stat Modifiers
    // ========================================

    /// <summary>
    /// Gets evasion modifier from this trait.
    /// </summary>
    int GetEvasionModifier(Enemy enemy, TraitConfiguration config, CombatState? state);

    /// <summary>
    /// Gets accuracy modifier from this trait against a specific target.
    /// </summary>
    int GetAccuracyModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state);

    /// <summary>
    /// Gets damage modifier from this trait against a specific target.
    /// </summary>
    int GetDamageModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state);

    /// <summary>
    /// Gets soak (armor) modifier from this trait.
    /// </summary>
    int GetSoakModifier(Enemy enemy, TraitConfiguration config, CombatState? state);

    /// <summary>
    /// Gets defense modifier from this trait.
    /// </summary>
    int GetDefenseModifier(Enemy enemy, TraitConfiguration config, CombatState? state);

    /// <summary>
    /// Gets movement bonus from this trait.
    /// </summary>
    int GetMovementBonus(TraitConfiguration config);

    // ========================================
    // Movement Queries
    // ========================================

    /// <summary>
    /// Returns true if this trait allows ignoring attacks of opportunity, null otherwise.
    /// </summary>
    bool? IgnoresAttacksOfOpportunity(TraitConfiguration config);

    /// <summary>
    /// Returns true if this trait allows moving through the position, null otherwise.
    /// </summary>
    bool? CanMoveThrough(Enemy enemy, TraitConfiguration config, GridPosition position, CombatState state);

    /// <summary>
    /// Returns true if this trait prevents forced movement, null otherwise.
    /// </summary>
    bool? IsImmobileToForce(TraitConfiguration config);

    // ========================================
    // Status/Damage Immunities
    // ========================================

    /// <summary>
    /// Returns true if this trait grants immunity to the status, null otherwise.
    /// </summary>
    bool? IsImmuneToStatus(TraitConfiguration config, string statusType);

    /// <summary>
    /// Returns damage multiplier for the type, or null if this trait doesn't affect it.
    /// </summary>
    float? GetDamageTypeMultiplier(TraitConfiguration config, string damageType);

    /// <summary>
    /// Returns true if this trait absorbs the damage type as healing, null otherwise.
    /// </summary>
    bool? AbsorbsDamageAsHealing(TraitConfiguration config, string damageType);

    // ========================================
    // Attack Modifiers
    // ========================================

    /// <summary>
    /// Returns critical multiplier if this trait modifies it, null otherwise.
    /// </summary>
    float? GetCriticalMultiplier(TraitConfiguration config);

    /// <summary>
    /// Returns armor penetration value from this trait.
    /// </summary>
    int GetArmorPenetration(TraitConfiguration config);

    /// <summary>
    /// Returns true if this trait makes attacks ignore soak, null otherwise.
    /// </summary>
    bool? AttacksIgnoreSoak(TraitConfiguration config);

    /// <summary>
    /// Returns range modifier from this trait.
    /// </summary>
    int GetRangeModifier(TraitConfiguration config);

    /// <summary>
    /// Returns multi-attack info if this trait enables it, null otherwise.
    /// </summary>
    (bool canMultiAttack, int attackCount, int accuracyPenalty)? GetMultiAttackInfo(TraitConfiguration config);

    // ========================================
    // Special Mechanics
    // ========================================

    /// <summary>
    /// Returns damage reflection percentage (0-1) from this trait.
    /// </summary>
    float GetReflectionPercentage(TraitConfiguration config);

    /// <summary>
    /// Returns lifesteal percentage (0-1) from this trait.
    /// </summary>
    float GetLifestealPercentage(TraitConfiguration config);

    /// <summary>
    /// Returns damage threshold from this trait (damage below this is ignored).
    /// </summary>
    int GetDamageThreshold(TraitConfiguration config);

    /// <summary>
    /// Returns first-hit reduction percentage (0-1) from this trait.
    /// </summary>
    float GetFirstHitReduction(TraitConfiguration config);

    /// <summary>
    /// Returns resurrection info if this trait enables it, null otherwise.
    /// </summary>
    (bool shouldResurrect, int hpPercent, int delayTurns)? CheckResurrection(Enemy enemy, TraitConfiguration config, CombatState state);
}
