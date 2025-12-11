using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits.Handlers;

/// <summary>
/// Handles Defensive traits (600-699): Regeneration, DamageThreshold, Reflective,
/// ShieldGenerator, LastStand, AdaptiveArmor, Camouflage, Unstoppable.
/// </summary>
public class DefensiveTraitHandler : BaseTraitHandler
{
    public override TraitCategory Category => TraitCategory.Defensive;

    // ========================================
    // Combat Event Hooks
    // ========================================

    public override void OnCombatStart(Enemy enemy, TraitConfiguration config, CombatState state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.ShieldGenerator:
                // Starts combat with X temporary HP that doesn't regenerate
                var tempHP = config.PrimaryValue > 0 ? config.PrimaryValue : 15;
                enemy.TemporaryHP = tempHP;
                Log(state, $"[ShieldGenerator] {enemy.Name}'s energy shield activates (+{tempHP} temporary HP).");
                break;

            case CreatureTrait.LastStand:
                // Cannot be reduced below 1 HP for first 2 turns
                config.StateCounter = config.PrimaryValue > 0 ? config.PrimaryValue : 2;
                Log(state, $"[LastStand] {enemy.Name} enters a desperate stance (immortal for {config.StateCounter} turns).");
                break;
        }
    }

    public override void OnTurnStart(Enemy enemy, TraitConfiguration config, CombatState state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Regeneration:
                // Heals X HP at start of turn
                if (enemy.HP < enemy.MaxHP)
                {
                    var healAmount = config.PrimaryValue > 0 ? config.PrimaryValue : 5;
                    var actualHeal = Math.Min(healAmount, enemy.MaxHP - enemy.HP);
                    enemy.HP += actualHeal;
                    Log(state, $"[Regeneration] {enemy.Name} regenerates {actualHeal} HP ({enemy.HP}/{enemy.MaxHP}).");
                }
                break;

            case CreatureTrait.LastStand:
                // Decrement turn counter
                if (config.StateCounter > 0)
                {
                    config.StateCounter--;
                    if (config.StateCounter == 0)
                    {
                        Log(state, $"[LastStand] {enemy.Name}'s desperate stance fades.");
                    }
                }
                break;
        }
    }

    // ========================================
    // Stat Modifiers
    // ========================================

    public override int GetSoakModifier(Enemy enemy, TraitConfiguration config, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.ArmoredPlating:
                // +X Soak (flat damage reduction)
                return config.PrimaryValue > 0 ? config.PrimaryValue : 4;

            case CreatureTrait.AdaptiveArmor:
                // After taking damage of a type, gains +2 Soak vs that type
                // Note: This is tracked per damage type in enemy.AdaptedDamageTypes
                // The actual soak bonus is applied during damage calculation
                return 0;

            default:
                return 0;
        }
    }

    public override int GetEvasionModifier(Enemy enemy, TraitConfiguration config, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Camouflage:
                // -2 Accuracy for ranged attacks (effectively +2 evasion vs ranged)
                // Note: This should only apply vs ranged attacks - full implementation
                // would need attack type context
                return 0; // Handled specially in GetDefenseModifier for ranged
            default:
                return 0;
        }
    }

    public override int GetDefenseModifier(Enemy enemy, TraitConfiguration config, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Camouflage:
                // -2 Accuracy for ranged attacks = +2 Defense vs ranged
                // Note: Full implementation needs attack type context
                var bonus = config.PrimaryValue > 0 ? config.PrimaryValue : 2;
                return bonus; // Applied generally; full context would check if ranged
            default:
                return 0;
        }
    }

    // ========================================
    // Status/Damage Immunities
    // ========================================

    public override bool? IsImmuneToStatus(TraitConfiguration config, string statusType)
    {
        if (config.Trait == CreatureTrait.Unstoppable)
        {
            // Immune to Stun, Root, and Slow effects
            var immuneStatuses = new[] { "Stun", "Stunned", "Root", "Rooted", "Slow", "Slowed" };
            if (immuneStatuses.Contains(statusType, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return null;
    }

    // ========================================
    // Special Mechanics
    // ========================================

    public override float GetReflectionPercentage(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Reflective)
        {
            // 20% of damage taken is dealt back to attacker
            return config.Percentage > 0 ? config.Percentage : 0.20f;
        }
        return 0f;
    }

    public override int GetDamageThreshold(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.DamageThreshold)
        {
            // Ignores attacks dealing less than X damage
            return config.PrimaryValue > 0 ? config.PrimaryValue : 4;
        }
        return 0;
    }

    /// <summary>
    /// Checks if LastStand should prevent death.
    /// Call this when enemy would be reduced to 0 HP.
    /// </summary>
    public bool ShouldPreventDeath(Enemy enemy, TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.LastStand && config.StateCounter > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles AdaptiveArmor learning a new damage type.
    /// Call this after enemy takes damage of a specific type.
    /// </summary>
    public void LearnDamageType(Enemy enemy, string damageType, CombatState state)
    {
        if (enemy.HasTrait(CreatureTrait.AdaptiveArmor) && !enemy.AdaptedDamageTypes.Contains(damageType))
        {
            enemy.AdaptedDamageTypes.Add(damageType);
            var config = enemy.GetTraitConfig(CreatureTrait.AdaptiveArmor);
            var soakBonus = config?.PrimaryValue ?? 2;
            Log(state, $"[AdaptiveArmor] {enemy.Name} adapts to {damageType} damage (+{soakBonus} Soak vs {damageType}).");
        }
    }

    /// <summary>
    /// Gets AdaptiveArmor bonus against a specific damage type.
    /// </summary>
    public int GetAdaptiveArmorBonus(Enemy enemy, string damageType)
    {
        if (enemy.HasTrait(CreatureTrait.AdaptiveArmor) && enemy.AdaptedDamageTypes.Contains(damageType))
        {
            var config = enemy.GetTraitConfig(CreatureTrait.AdaptiveArmor);
            return config?.PrimaryValue ?? 2;
        }
        return 0;
    }
}
