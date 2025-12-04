using RuneAndRust.Core;
using RuneAndRust.Core.Traits;
using RuneAndRust.Engine.Traits;
using RuneAndRust.Engine.Traits.Handlers;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.45: Creature Traits System integration for CombatEngine.
/// Partial class that adds trait processing hooks to combat flow.
/// </summary>
public partial class CombatEngine
{
    private ICreatureTraitService? _traitService;

    /// <summary>
    /// Initializes the creature trait service with all handlers.
    /// Call this after constructing the CombatEngine.
    /// </summary>
    public void InitializeTraitService()
    {
        var service = new CreatureTraitService();

        // Register all trait handlers
        service.RegisterHandler(new DefensiveTraitHandler());
        service.RegisterHandler(new OffensiveTraitHandler());
        service.RegisterHandler(new MobilityTraitHandler());
        // TODO: Register additional handlers as they are implemented
        // service.RegisterHandler(new TemporalTraitHandler());
        // service.RegisterHandler(new CorruptionTraitHandler());
        // service.RegisterHandler(new MechanicalTraitHandler());
        // service.RegisterHandler(new PsychicTraitHandler());
        // service.RegisterHandler(new UniqueTraitHandler());
        // service.RegisterHandler(new ResistanceTraitHandler());
        // service.RegisterHandler(new StrategyTraitHandler());
        // service.RegisterHandler(new SensoryTraitHandler());
        // service.RegisterHandler(new CombatConditionTraitHandler());

        _traitService = service;
        _log.Information("Creature Trait Service initialized with {HandlerCount} handlers", 3);
    }

    /// <summary>
    /// Gets the trait service, initializing if needed.
    /// </summary>
    public ICreatureTraitService GetTraitService()
    {
        if (_traitService == null)
        {
            InitializeTraitService();
        }
        return _traitService!;
    }

    // ========================================
    // Trait Hook Methods
    // ========================================

    /// <summary>
    /// Process trait effects at combat start for all enemies.
    /// Call this after InitializeCombat.
    /// </summary>
    public void ProcessTraitCombatStart(CombatState state)
    {
        var service = GetTraitService();
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Traits.Count > 0)
            {
                service.OnCombatStart(enemy, state);
            }
        }
    }

    /// <summary>
    /// Process trait effects at turn start for an enemy.
    /// </summary>
    public void ProcessTraitTurnStart(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count > 0)
        {
            GetTraitService().OnTurnStart(enemy, state);
        }
    }

    /// <summary>
    /// Process trait effects at turn end for an enemy.
    /// </summary>
    public void ProcessTraitTurnEnd(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count > 0)
        {
            GetTraitService().OnTurnEnd(enemy, state);
        }
    }

    /// <summary>
    /// Process trait effects when an enemy moves.
    /// </summary>
    public void ProcessTraitMovement(Enemy enemy, GridPosition from, GridPosition to, CombatState state)
    {
        if (enemy.Traits.Count > 0)
        {
            GetTraitService().OnMovement(enemy, from, to, state);
        }
    }

    /// <summary>
    /// Process trait effects when an enemy dies.
    /// </summary>
    public void ProcessTraitDeath(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count > 0)
        {
            GetTraitService().OnDeath(enemy, state);
        }
    }

    /// <summary>
    /// Process trait effects at round end.
    /// </summary>
    public void ProcessTraitRoundEnd(CombatState state)
    {
        GetTraitService().OnRoundEnd(state);
    }

    // ========================================
    // Stat Modifier Methods
    // ========================================

    /// <summary>
    /// Gets total evasion bonus from traits.
    /// </summary>
    public int GetTraitEvasionBonus(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetEvasionModifier(enemy, state);
    }

    /// <summary>
    /// Gets total accuracy bonus from traits against a target.
    /// </summary>
    public int GetTraitAccuracyBonus(Enemy enemy, object target, CombatState state)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetAccuracyModifier(enemy, target, state);
    }

    /// <summary>
    /// Gets total damage bonus from traits against a target.
    /// </summary>
    public int GetTraitDamageBonus(Enemy enemy, object target, CombatState state)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetDamageModifier(enemy, target, state);
    }

    /// <summary>
    /// Gets total soak bonus from traits.
    /// </summary>
    public int GetTraitSoakBonus(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetSoakModifier(enemy, state);
    }

    /// <summary>
    /// Gets total defense bonus from traits.
    /// </summary>
    public int GetTraitDefenseBonus(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetDefenseModifier(enemy, state);
    }

    /// <summary>
    /// Gets movement bonus from traits.
    /// </summary>
    public int GetTraitMovementBonus(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetMovementBonus(enemy);
    }

    // ========================================
    // Movement Query Methods
    // ========================================

    /// <summary>
    /// Checks if enemy ignores attacks of opportunity.
    /// </summary>
    public bool TraitIgnoresAoO(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().IgnoresAttacksOfOpportunity(enemy);
    }

    /// <summary>
    /// Checks if enemy can move through a position.
    /// </summary>
    public bool TraitCanMoveThrough(Enemy enemy, GridPosition position, CombatState state)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().CanMoveThrough(enemy, position, state);
    }

    /// <summary>
    /// Checks if enemy is immune to forced movement.
    /// </summary>
    public bool TraitIsImmobileToForce(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().IsImmobileToForce(enemy);
    }

    // ========================================
    // Damage Processing Methods
    // ========================================

    /// <summary>
    /// Gets damage type multiplier from traits.
    /// </summary>
    public float GetTraitDamageMultiplier(Enemy enemy, string damageType)
    {
        if (enemy.Traits.Count == 0) return 1.0f;
        return GetTraitService().GetDamageTypeMultiplier(enemy, damageType);
    }

    /// <summary>
    /// Checks if enemy absorbs damage type as healing.
    /// </summary>
    public bool TraitAbsorbsDamage(Enemy enemy, string damageType)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().AbsorbsDamageAsHealing(enemy, damageType);
    }

    /// <summary>
    /// Checks if damage is below damage threshold.
    /// </summary>
    public bool TraitBlocksDamage(Enemy enemy, int damage)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().DamageIsBelowThreshold(enemy, damage);
    }

    /// <summary>
    /// Applies first-hit damage reduction if applicable.
    /// </summary>
    public int ApplyTraitFirstHitReduction(Enemy enemy, int damage)
    {
        if (enemy.Traits.Count == 0) return damage;
        return GetTraitService().ApplyFirstHitReduction(enemy, damage);
    }

    /// <summary>
    /// Calculates damage reflection amount.
    /// </summary>
    public int CalculateTraitReflection(Enemy enemy, int damage)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().CalculateReflectedDamage(enemy, damage);
    }

    /// <summary>
    /// Calculates lifesteal healing.
    /// </summary>
    public int CalculateTraitLifesteal(Enemy enemy, int damageDealt)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().CalculateLifesteal(enemy, damageDealt);
    }

    // ========================================
    // Attack Modifier Methods
    // ========================================

    /// <summary>
    /// Gets critical damage multiplier from traits.
    /// </summary>
    public float GetTraitCritMultiplier(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return 2.0f;
        return GetTraitService().GetCriticalMultiplier(enemy);
    }

    /// <summary>
    /// Gets armor penetration from traits.
    /// </summary>
    public int GetTraitArmorPenetration(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetArmorPenetration(enemy);
    }

    /// <summary>
    /// Checks if attacks ignore soak.
    /// </summary>
    public bool TraitIgnoresSoak(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().AttacksIgnoreSoak(enemy);
    }

    /// <summary>
    /// Gets attack range modifier.
    /// </summary>
    public int GetTraitRangeModifier(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return 0;
        return GetTraitService().GetRangeModifier(enemy);
    }

    /// <summary>
    /// Gets multi-attack info from traits.
    /// </summary>
    public (bool canMultiAttack, int attackCount, int accuracyPenalty) GetTraitMultiAttack(Enemy enemy)
    {
        if (enemy.Traits.Count == 0) return (false, 1, 0);
        return GetTraitService().GetMultiAttackInfo(enemy);
    }

    // ========================================
    // Status Immunity Methods
    // ========================================

    /// <summary>
    /// Checks if enemy is immune to a status effect.
    /// </summary>
    public bool TraitIsImmuneToStatus(Enemy enemy, string statusType)
    {
        if (enemy.Traits.Count == 0) return false;
        return GetTraitService().IsImmuneToStatus(enemy, statusType);
    }

    // ========================================
    // Resurrection Methods
    // ========================================

    /// <summary>
    /// Checks if enemy should resurrect on death.
    /// </summary>
    public (bool shouldResurrect, int hpPercent, int delayTurns) CheckTraitResurrection(Enemy enemy, CombatState state)
    {
        if (enemy.Traits.Count == 0) return (false, 0, 0);
        return GetTraitService().CheckResurrection(enemy, state);
    }
}
