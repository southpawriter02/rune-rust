using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits.Handlers;

/// <summary>
/// Handles Offensive traits (700-799): Brutal, Relentless, Executioner,
/// ArmorPiercing, Reach, Sweeping, Enrage, PredatorInstinct.
/// </summary>
public class OffensiveTraitHandler : BaseTraitHandler
{
    public override TraitCategory Category => TraitCategory.Offensive;

    // ========================================
    // Stat Modifiers
    // ========================================

    public override int GetAccuracyModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.PredatorInstinct:
                // +2 Accuracy against isolated targets (no allies within 2 tiles)
                if (state != null && target is PlayerCharacter player)
                {
                    if (IsTargetIsolated(player, state, config.SecondaryValue > 0 ? config.SecondaryValue : 2))
                    {
                        return config.PrimaryValue > 0 ? config.PrimaryValue : 2;
                    }
                }
                break;

            case CreatureTrait.Relentless:
                // Second attack at -2 Accuracy
                // Note: This is handled in GetMultiAttackInfo, not here
                break;
        }
        return 0;
    }

    public override int GetDamageModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Executioner:
                // +50% damage against targets below 25% HP
                if (IsTargetBelowHpThreshold(target, 0.25f))
                {
                    var percentage = config.Percentage > 0 ? config.Percentage : 0.50f;
                    // Return as flat bonus approximation (caller should multiply base damage)
                    return (int)(enemy.BaseDamageDice * 3 * percentage); // Approximate
                }
                break;

            case CreatureTrait.Enrage:
                // At <50% HP, +2 damage but -2 Defense
                if (IsEnemyBelowHpThreshold(enemy, 0.50f))
                {
                    return config.PrimaryValue > 0 ? config.PrimaryValue : 2;
                }
                break;
        }
        return 0;
    }

    public override int GetDefenseModifier(Enemy enemy, TraitConfiguration config, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Enrage:
                // At <50% HP, +2 damage but -2 Defense
                if (IsEnemyBelowHpThreshold(enemy, 0.50f))
                {
                    var penalty = config.SecondaryValue > 0 ? config.SecondaryValue : 2;
                    return -penalty;
                }
                break;
        }
        return 0;
    }

    // ========================================
    // Attack Modifiers
    // ========================================

    public override float? GetCriticalMultiplier(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Brutal)
        {
            // Critical hits deal triple damage instead of double
            return config.PrimaryValue > 0 ? config.PrimaryValue : 3.0f;
        }
        return null;
    }

    public override int GetArmorPenetration(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.ArmorPiercing)
        {
            // Attacks ignore X points of target's Soak
            return config.PrimaryValue > 0 ? config.PrimaryValue : 3;
        }
        return 0;
    }

    public override int GetRangeModifier(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Reach)
        {
            // Can attack targets 2 tiles away with melee attacks
            return config.PrimaryValue > 0 ? config.PrimaryValue : 1;
        }
        return 0;
    }

    public override (bool canMultiAttack, int attackCount, int accuracyPenalty)? GetMultiAttackInfo(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Relentless)
        {
            // Can attack twice per turn (second attack at -2 Accuracy)
            var penalty = config.PrimaryValue > 0 ? config.PrimaryValue : 2;
            return (true, 2, penalty);
        }
        return null;
    }

    // ========================================
    // Helper Methods
    // ========================================

    /// <summary>
    /// Checks if a player character is isolated (no companions within range).
    /// </summary>
    private bool IsTargetIsolated(PlayerCharacter player, CombatState state, int range)
    {
        // Check if any companions are within range of the player
        if (state.Companions == null || state.Companions.Count == 0)
        {
            return true; // No companions = isolated
        }

        // Would need companion positions to check properly
        // For now, assume not isolated if companions exist
        return false;
    }

    /// <summary>
    /// Checks if Sweeping trait should hit additional targets.
    /// Returns list of additional enemies that should be hit.
    /// </summary>
    public List<Enemy> GetSweepingTargets(Enemy attacker, Enemy primaryTarget, CombatState state)
    {
        var additionalTargets = new List<Enemy>();

        if (attacker.HasTrait(CreatureTrait.Sweeping) && attacker.Position != null)
        {
            // Get all enemies adjacent to the attacker (excluding primary target)
            foreach (var enemy in state.Enemies)
            {
                if (enemy != primaryTarget && enemy.IsAlive && enemy.Position != null)
                {
                    var distance = GetDistance(attacker.Position, enemy.Position);
                    if (distance == 1) // Adjacent
                    {
                        additionalTargets.Add(enemy);
                    }
                }
            }
        }

        return additionalTargets;
    }

    /// <summary>
    /// Checks if Executioner bonus damage applies.
    /// </summary>
    public float GetExecutionerMultiplier(Enemy attacker, object target)
    {
        if (!attacker.HasTrait(CreatureTrait.Executioner))
            return 1.0f;

        if (!IsTargetBelowHpThreshold(target, 0.25f))
            return 1.0f;

        var config = attacker.GetTraitConfig(CreatureTrait.Executioner);
        return 1.0f + (config?.Percentage ?? 0.50f);
    }
}
