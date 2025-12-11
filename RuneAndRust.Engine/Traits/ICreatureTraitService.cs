using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits;

/// <summary>
/// Service interface for processing creature traits during combat.
/// Coordinates trait effects, stat modifiers, and combat event hooks.
/// </summary>
public interface ICreatureTraitService
{
    // ========================================
    // Combat Event Hooks
    // ========================================

    /// <summary>
    /// Called when combat begins. Initializes trait state and applies combat-start effects.
    /// Examples: ShieldGenerator applies temp HP, MirrorImage creates duplicates.
    /// </summary>
    void OnCombatStart(Enemy enemy, CombatState state);

    /// <summary>
    /// Called at the start of an enemy's turn.
    /// Examples: Regeneration heals HP, ForlornAura applies stress.
    /// </summary>
    void OnTurnStart(Enemy enemy, CombatState state);

    /// <summary>
    /// Called at the end of an enemy's turn.
    /// Examples: SelfRepair heals if didn't attack, Whispers applies stress.
    /// </summary>
    void OnTurnEnd(Enemy enemy, CombatState state);

    /// <summary>
    /// Called when an enemy moves.
    /// Examples: ChronoDistortion applies stress to nearby characters.
    /// </summary>
    void OnMovement(Enemy enemy, GridPosition from, GridPosition to, CombatState state);

    /// <summary>
    /// Called when an enemy dies.
    /// Examples: PowerSurge deals damage, SplitOnDeath spawns creatures, Explosive detonates.
    /// </summary>
    void OnDeath(Enemy enemy, CombatState state);

    /// <summary>
    /// Called at the end of a combat round (after all participants have acted).
    /// Examples: Whispers targets lowest-WILL character.
    /// </summary>
    void OnRoundEnd(CombatState state);

    // ========================================
    // Stat Modifiers
    // ========================================

    /// <summary>
    /// Gets total evasion modifier from all traits.
    /// Examples: TemporalPrescience adds evasion, Flight adds vs melee.
    /// </summary>
    int GetEvasionModifier(Enemy enemy, CombatState? state = null);

    /// <summary>
    /// Gets total accuracy modifier from all traits against a specific target.
    /// Examples: Networked adds per ally, PredatorInstinct adds vs isolated targets.
    /// </summary>
    int GetAccuracyModifier(Enemy enemy, object? target, CombatState? state = null);

    /// <summary>
    /// Gets total damage modifier from all traits against a specific target.
    /// Examples: Executioner adds vs low HP, Enrage adds when wounded.
    /// </summary>
    int GetDamageModifier(Enemy enemy, object? target, CombatState? state = null);

    /// <summary>
    /// Gets total soak (armor) modifier from all traits.
    /// Examples: ArmoredPlating adds flat soak, AdaptiveArmor adds vs adapted types.
    /// </summary>
    int GetSoakModifier(Enemy enemy, CombatState? state = null);

    /// <summary>
    /// Gets total defense modifier from all traits.
    /// Examples: Flight adds vs melee, Territorial adds in starting zone.
    /// </summary>
    int GetDefenseModifier(Enemy enemy, CombatState? state = null);

    /// <summary>
    /// Gets movement speed bonus from traits.
    /// Examples: Swiftness adds tiles per turn.
    /// </summary>
    int GetMovementBonus(Enemy enemy);

    // ========================================
    // Movement Queries
    // ========================================

    /// <summary>
    /// Checks if an enemy ignores attacks of opportunity.
    /// Examples: TemporalPhase, Phasing, HitAndRun (after attacking).
    /// </summary>
    bool IgnoresAttacksOfOpportunity(Enemy enemy);

    /// <summary>
    /// Checks if an enemy can move through a specific position.
    /// Examples: Phasing allows moving through obstacles, Flight ignores ground hazards.
    /// </summary>
    bool CanMoveThrough(Enemy enemy, GridPosition position, CombatState state);

    /// <summary>
    /// Checks if an enemy cannot be forcibly moved.
    /// Examples: Anchored prevents knockback/pull.
    /// </summary>
    bool IsImmobileToForce(Enemy enemy);

    // ========================================
    // Status/Damage Immunities
    // ========================================

    /// <summary>
    /// Checks if an enemy is immune to a specific status effect.
    /// Examples: IronHeart immune to Bleeding/Poison, Unstoppable immune to Stun/Root.
    /// </summary>
    bool IsImmuneToStatus(Enemy enemy, string statusType);

    /// <summary>
    /// Gets damage multiplier for a specific damage type (1.0 = normal, 0.5 = resistant, 1.5 = vulnerable).
    /// Examples: FireResistant = 0.5 for Fire, HolyVulnerable = 2.0 for Holy.
    /// </summary>
    float GetDamageTypeMultiplier(Enemy enemy, string damageType);

    /// <summary>
    /// Checks if damage of a type should be absorbed as healing.
    /// Examples: ElementalAbsorption, StormBorn (Lightning).
    /// </summary>
    bool AbsorbsDamageAsHealing(Enemy enemy, string damageType);

    // ========================================
    // Attack Modifiers
    // ========================================

    /// <summary>
    /// Gets the critical damage multiplier for an enemy.
    /// Default is 2.0 (double damage), Brutal makes it 3.0.
    /// </summary>
    float GetCriticalMultiplier(Enemy enemy);

    /// <summary>
    /// Gets the armor penetration value (ignores X points of target soak).
    /// Examples: ArmorPiercing.
    /// </summary>
    int GetArmorPenetration(Enemy enemy);

    /// <summary>
    /// Checks if attacks ignore soak entirely.
    /// Examples: MindSpike (Psychic damage).
    /// </summary>
    bool AttacksIgnoreSoak(Enemy enemy);

    /// <summary>
    /// Gets attack range modifier.
    /// Examples: Reach adds 1 tile to melee range.
    /// </summary>
    int GetRangeModifier(Enemy enemy);

    /// <summary>
    /// Checks if the enemy can attack multiple times.
    /// Examples: Relentless allows double attack.
    /// </summary>
    (bool canMultiAttack, int attackCount, int accuracyPenalty) GetMultiAttackInfo(Enemy enemy);

    // ========================================
    // Special Mechanics
    // ========================================

    /// <summary>
    /// Calculates damage reflection amount.
    /// Examples: Reflective returns 20% of damage to attacker.
    /// </summary>
    int CalculateReflectedDamage(Enemy enemy, int incomingDamage);

    /// <summary>
    /// Calculates lifesteal healing amount.
    /// Examples: Vampiric heals 50% of damage dealt.
    /// </summary>
    int CalculateLifesteal(Enemy enemy, int damageDealt);

    /// <summary>
    /// Checks if damage should be reduced by threshold mechanics.
    /// Examples: DamageThreshold ignores attacks below X damage.
    /// </summary>
    bool DamageIsBelowThreshold(Enemy enemy, int damage);

    /// <summary>
    /// Applies first-hit reduction if applicable.
    /// Examples: ModularConstruction takes 50% damage on first hit.
    /// </summary>
    int ApplyFirstHitReduction(Enemy enemy, int damage);

    /// <summary>
    /// Checks if an enemy should resurrect on death.
    /// Examples: TimeLoop (25% chance), Resurrection (if ally alive).
    /// </summary>
    (bool shouldResurrect, int hpPercent, int delayTurns) CheckResurrection(Enemy enemy, CombatState state);

    // ========================================
    // Utility
    // ========================================

    /// <summary>
    /// Gets all active trait configurations for an enemy.
    /// </summary>
    IEnumerable<TraitConfiguration> GetActiveTraits(Enemy enemy);

    /// <summary>
    /// Validates an enemy's trait combination for conflicts.
    /// Returns true if valid, false with conflict descriptions if invalid.
    /// </summary>
    (bool isValid, List<string> conflicts) ValidateTraitCombination(Enemy enemy);

    /// <summary>
    /// Calculates total trait points for balance validation.
    /// </summary>
    int CalculateTotalTraitPoints(Enemy enemy);
}
