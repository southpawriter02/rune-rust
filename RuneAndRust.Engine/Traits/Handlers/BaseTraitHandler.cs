using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits.Handlers;

/// <summary>
/// Abstract base class for trait handlers.
/// Provides no-op implementations for all interface methods.
/// Derived classes override only the methods relevant to their traits.
/// </summary>
public abstract class BaseTraitHandler : ITraitHandler
{
    public abstract TraitCategory Category { get; }

    public virtual bool CanHandle(CreatureTrait trait)
    {
        return trait.GetCategory() == Category;
    }

    // ========================================
    // Combat Event Hooks - Default no-ops
    // ========================================

    public virtual void OnCombatStart(Enemy enemy, TraitConfiguration config, CombatState state) { }
    public virtual void OnTurnStart(Enemy enemy, TraitConfiguration config, CombatState state) { }
    public virtual void OnTurnEnd(Enemy enemy, TraitConfiguration config, CombatState state) { }
    public virtual void OnMovement(Enemy enemy, TraitConfiguration config, GridPosition from, GridPosition to, CombatState state) { }
    public virtual void OnDeath(Enemy enemy, TraitConfiguration config, CombatState state) { }

    // ========================================
    // Stat Modifiers - Default zero
    // ========================================

    public virtual int GetEvasionModifier(Enemy enemy, TraitConfiguration config, CombatState? state) => 0;
    public virtual int GetAccuracyModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state) => 0;
    public virtual int GetDamageModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state) => 0;
    public virtual int GetSoakModifier(Enemy enemy, TraitConfiguration config, CombatState? state) => 0;
    public virtual int GetDefenseModifier(Enemy enemy, TraitConfiguration config, CombatState? state) => 0;
    public virtual int GetMovementBonus(TraitConfiguration config) => 0;

    // ========================================
    // Movement Queries - Default null (doesn't apply)
    // ========================================

    public virtual bool? IgnoresAttacksOfOpportunity(TraitConfiguration config) => null;
    public virtual bool? CanMoveThrough(Enemy enemy, TraitConfiguration config, GridPosition position, CombatState state) => null;
    public virtual bool? IsImmobileToForce(TraitConfiguration config) => null;

    // ========================================
    // Status/Damage Immunities - Default null (doesn't apply)
    // ========================================

    public virtual bool? IsImmuneToStatus(TraitConfiguration config, string statusType) => null;
    public virtual float? GetDamageTypeMultiplier(TraitConfiguration config, string damageType) => null;
    public virtual bool? AbsorbsDamageAsHealing(TraitConfiguration config, string damageType) => null;

    // ========================================
    // Attack Modifiers - Default null/zero
    // ========================================

    public virtual float? GetCriticalMultiplier(TraitConfiguration config) => null;
    public virtual int GetArmorPenetration(TraitConfiguration config) => 0;
    public virtual bool? AttacksIgnoreSoak(TraitConfiguration config) => null;
    public virtual int GetRangeModifier(TraitConfiguration config) => 0;
    public virtual (bool canMultiAttack, int attackCount, int accuracyPenalty)? GetMultiAttackInfo(TraitConfiguration config) => null;

    // ========================================
    // Special Mechanics - Default zero/null
    // ========================================

    public virtual float GetReflectionPercentage(TraitConfiguration config) => 0f;
    public virtual float GetLifestealPercentage(TraitConfiguration config) => 0f;
    public virtual int GetDamageThreshold(TraitConfiguration config) => 0;
    public virtual float GetFirstHitReduction(TraitConfiguration config) => 0f;
    public virtual (bool shouldResurrect, int hpPercent, int delayTurns)? CheckResurrection(Enemy enemy, TraitConfiguration config, CombatState state) => null;

    // ========================================
    // Helper Methods for Derived Classes
    // ========================================

    /// <summary>
    /// Safely gets the player from combat state.
    /// </summary>
    protected PlayerCharacter? GetPlayer(CombatState? state)
    {
        return state?.Player;
    }

    /// <summary>
    /// Calculates distance between two grid positions.
    /// Uses Zone/Row/Column structure for tactical grid.
    /// </summary>
    protected int GetDistance(GridPosition? from, GridPosition? to)
    {
        if (from == null || to == null) return int.MaxValue;

        var fromPos = from.Value;
        var toPos = to.Value;

        // Calculate zone distance (0 if same zone, 2 if different)
        int zoneDistance = fromPos.Zone == toPos.Zone ? 0 : 2;

        // Calculate row distance (0 if same row, 1 if different)
        int rowDistance = fromPos.Row == toPos.Row ? 0 : 1;

        // Calculate column distance
        int columnDistance = Math.Abs(fromPos.Column - toPos.Column);

        return zoneDistance + rowDistance + columnDistance;
    }

    /// <summary>
    /// Gets enemies within range of a position.
    /// </summary>
    protected IEnumerable<Enemy> GetEnemiesInRange(CombatState state, GridPosition center, int range)
    {
        return state.Enemies
            .Where(e => e.IsAlive && e.Position != null)
            .Where(e => GetDistance(center, e.Position) <= range);
    }

    /// <summary>
    /// Checks if target is below HP threshold (as percentage).
    /// </summary>
    protected bool IsTargetBelowHpThreshold(object? target, float threshold)
    {
        if (target is Enemy enemy && enemy.MaxHP > 0)
        {
            return (float)enemy.HP / enemy.MaxHP < threshold;
        }
        if (target is PlayerCharacter player && player.MaxHP > 0)
        {
            return (float)player.HP / player.MaxHP < threshold;
        }
        return false;
    }

    /// <summary>
    /// Checks if the enemy is below HP threshold (as percentage).
    /// </summary>
    protected bool IsEnemyBelowHpThreshold(Enemy enemy, float threshold)
    {
        if (enemy.MaxHP <= 0) return false;
        return (float)enemy.HP / enemy.MaxHP < threshold;
    }

    /// <summary>
    /// Adds a log entry to combat state.
    /// </summary>
    protected void Log(CombatState state, string message)
    {
        state.AddLogEntry(message);
    }
}
