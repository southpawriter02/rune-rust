using RuneAndRust.Core;
using RuneAndRust.Core.Traits;
using RuneAndRust.Engine.Traits.Handlers;
using Serilog;

namespace RuneAndRust.Engine.Traits;

/// <summary>
/// Main service for processing creature traits during combat.
/// Coordinates trait handlers and aggregates their effects.
/// </summary>
public class CreatureTraitService : ICreatureTraitService
{
    private static readonly ILogger _log = Log.ForContext<CreatureTraitService>();
    private readonly Dictionary<TraitCategory, ITraitHandler> _handlers;
    private readonly Random _random;

    public CreatureTraitService()
    {
        _random = new Random();
        _handlers = new Dictionary<TraitCategory, ITraitHandler>();
    }

    /// <summary>
    /// Registers a trait handler for its category.
    /// </summary>
    public void RegisterHandler(ITraitHandler handler)
    {
        _handlers[handler.Category] = handler;
        _log.Debug("Registered trait handler for category {Category}", handler.Category);
    }

    /// <summary>
    /// Gets the handler for a specific trait.
    /// </summary>
    private ITraitHandler? GetHandler(CreatureTrait trait)
    {
        var category = trait.GetCategory();
        return _handlers.TryGetValue(category, out var handler) ? handler : null;
    }

    // ========================================
    // Combat Event Hooks
    // ========================================

    public void OnCombatStart(Enemy enemy, CombatState state)
    {
        enemy.ResetTraitCombatState();

        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                handler.OnCombatStart(enemy, config, state);
            }
        }
    }

    public void OnTurnStart(Enemy enemy, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                handler.OnTurnStart(enemy, config, state);
            }
        }
    }

    public void OnTurnEnd(Enemy enemy, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                handler.OnTurnEnd(enemy, config, state);
            }
        }
    }

    public void OnMovement(Enemy enemy, GridPosition from, GridPosition to, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                handler.OnMovement(enemy, config, from, to, state);
            }
        }
    }

    public void OnDeath(Enemy enemy, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                handler.OnDeath(enemy, config, state);
            }
        }
    }

    public void OnRoundEnd(CombatState state)
    {
        // Process round-end traits for all living enemies
        foreach (var enemy in state.Enemies.Where(e => e.IsAlive))
        {
            // Whispers trait targets lowest-WILL character at round end
            if (enemy.HasTrait(CreatureTrait.Whispers))
            {
                var config = enemy.GetTraitConfig(CreatureTrait.Whispers);
                if (config != null && state.Player != null)
                {
                    var stressAmount = config.PrimaryValue > 0 ? config.PrimaryValue : 3;
                    state.Player.PsychicStress += stressAmount;
                    state.AddLogEntry($"[Whispers] {enemy.Name}'s whispers inflict {stressAmount} Psychic Stress.");
                }
            }
        }
    }

    // ========================================
    // Stat Modifiers
    // ========================================

    public int GetEvasionModifier(Enemy enemy, CombatState? state = null)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetEvasionModifier(enemy, config, state);
            }
        }
        return total;
    }

    public int GetAccuracyModifier(Enemy enemy, object? target, CombatState? state = null)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetAccuracyModifier(enemy, config, target, state);
            }
        }
        return total;
    }

    public int GetDamageModifier(Enemy enemy, object? target, CombatState? state = null)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetDamageModifier(enemy, config, target, state);
            }
        }
        return total;
    }

    public int GetSoakModifier(Enemy enemy, CombatState? state = null)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetSoakModifier(enemy, config, state);
            }
        }
        return total;
    }

    public int GetDefenseModifier(Enemy enemy, CombatState? state = null)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetDefenseModifier(enemy, config, state);
            }
        }
        return total;
    }

    public int GetMovementBonus(Enemy enemy)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetMovementBonus(config);
            }
        }
        return total;
    }

    // ========================================
    // Movement Queries
    // ========================================

    public bool IgnoresAttacksOfOpportunity(Enemy enemy)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.IgnoresAttacksOfOpportunity(config);
                if (result == true) return true;
            }
        }
        return false;
    }

    public bool CanMoveThrough(Enemy enemy, GridPosition position, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.CanMoveThrough(enemy, config, position, state);
                if (result == true) return true;
            }
        }
        return false;
    }

    public bool IsImmobileToForce(Enemy enemy)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.IsImmobileToForce(config);
                if (result == true) return true;
            }
        }
        return false;
    }

    // ========================================
    // Status/Damage Immunities
    // ========================================

    public bool IsImmuneToStatus(Enemy enemy, string statusType)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.IsImmuneToStatus(config, statusType);
                if (result == true) return true;
            }
        }
        return false;
    }

    public float GetDamageTypeMultiplier(Enemy enemy, string damageType)
    {
        float multiplier = 1.0f;
        bool hasModifier = false;

        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.GetDamageTypeMultiplier(config, damageType);
                if (result.HasValue)
                {
                    // Multiplicative stacking for resistances/vulnerabilities
                    multiplier *= result.Value;
                    hasModifier = true;
                }
            }
        }

        return hasModifier ? multiplier : 1.0f;
    }

    public bool AbsorbsDamageAsHealing(Enemy enemy, string damageType)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.AbsorbsDamageAsHealing(config, damageType);
                if (result == true) return true;
            }
        }
        return false;
    }

    // ========================================
    // Attack Modifiers
    // ========================================

    public float GetCriticalMultiplier(Enemy enemy)
    {
        float multiplier = 2.0f; // Default critical multiplier

        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.GetCriticalMultiplier(config);
                if (result.HasValue && result.Value > multiplier)
                {
                    multiplier = result.Value;
                }
            }
        }

        return multiplier;
    }

    public int GetArmorPenetration(Enemy enemy)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetArmorPenetration(config);
            }
        }
        return total;
    }

    public bool AttacksIgnoreSoak(Enemy enemy)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.AttacksIgnoreSoak(config);
                if (result == true) return true;
            }
        }
        return false;
    }

    public int GetRangeModifier(Enemy enemy)
    {
        int total = 0;
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                total += handler.GetRangeModifier(config);
            }
        }
        return total;
    }

    public (bool canMultiAttack, int attackCount, int accuracyPenalty) GetMultiAttackInfo(Enemy enemy)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.GetMultiAttackInfo(config);
                if (result.HasValue && result.Value.canMultiAttack)
                {
                    return result.Value;
                }
            }
        }
        return (false, 1, 0);
    }

    // ========================================
    // Special Mechanics
    // ========================================

    public int CalculateReflectedDamage(Enemy enemy, int incomingDamage)
    {
        float totalReflection = 0f;

        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                totalReflection += handler.GetReflectionPercentage(config);
            }
        }

        return (int)(incomingDamage * totalReflection);
    }

    public int CalculateLifesteal(Enemy enemy, int damageDealt)
    {
        float totalLifesteal = 0f;

        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                totalLifesteal += handler.GetLifestealPercentage(config);
            }
        }

        return (int)(damageDealt * totalLifesteal);
    }

    public bool DamageIsBelowThreshold(Enemy enemy, int damage)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var threshold = handler.GetDamageThreshold(config);
                if (threshold > 0 && damage < threshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int ApplyFirstHitReduction(Enemy enemy, int damage)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var reduction = handler.GetFirstHitReduction(config);
                if (reduction > 0 && !config.HasBeenUsed)
                {
                    config.HasBeenUsed = true;
                    return (int)(damage * (1 - reduction));
                }
            }
        }
        return damage;
    }

    public (bool shouldResurrect, int hpPercent, int delayTurns) CheckResurrection(Enemy enemy, CombatState state)
    {
        foreach (var config in enemy.Traits)
        {
            var handler = GetHandler(config.Trait);
            if (handler != null)
            {
                var result = handler.CheckResurrection(enemy, config, state);
                if (result.HasValue && result.Value.shouldResurrect)
                {
                    return result.Value;
                }
            }
        }
        return (false, 0, 0);
    }

    // ========================================
    // Utility
    // ========================================

    public IEnumerable<TraitConfiguration> GetActiveTraits(Enemy enemy)
    {
        return enemy.Traits;
    }

    public (bool isValid, List<string> conflicts) ValidateTraitCombination(Enemy enemy)
    {
        var conflicts = new List<string>();

        // Check for known conflicting combinations
        if (enemy.HasTrait(CreatureTrait.Anchored) && enemy.HasTrait(CreatureTrait.RandomBlink))
        {
            conflicts.Add("Anchored + RandomBlink: Cannot be moved vs constantly moves");
        }

        if (enemy.HasTrait(CreatureTrait.IronHeart) && enemy.HasTrait(CreatureTrait.Vampiric))
        {
            conflicts.Add("IronHeart + Vampiric: Machine vs life-draining (thematic conflict)");
        }

        if (enemy.HasTrait(CreatureTrait.Flight) && enemy.HasTrait(CreatureTrait.Burrowing))
        {
            conflicts.Add("Flight + Burrowing: Aerial vs subterranean (pick one movement mode)");
        }

        if (enemy.HasTrait(CreatureTrait.Camouflage) && enemy.HasTrait(CreatureTrait.ForlornAura))
        {
            // Note: Per spec, ForlornAura overrides stealth - this is a warning, not a conflict
            _log.Warning("Camouflage + ForlornAura: ForlornAura's obvious presence overrides stealth benefit");
        }

        return (conflicts.Count == 0, conflicts);
    }

    public int CalculateTotalTraitPoints(Enemy enemy)
    {
        return enemy.GetTotalTraitPoints();
    }
}
