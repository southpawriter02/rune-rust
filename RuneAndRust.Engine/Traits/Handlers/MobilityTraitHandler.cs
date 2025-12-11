using RuneAndRust.Core;
using RuneAndRust.Core.Traits;

namespace RuneAndRust.Engine.Traits.Handlers;

/// <summary>
/// Handles Mobility traits (500-599): Flight, Burrowing, Phasing, Swiftness,
/// Anchored, Ambush, HitAndRun, Territorial.
/// </summary>
public class MobilityTraitHandler : BaseTraitHandler
{
    public override TraitCategory Category => TraitCategory.Mobility;

    // ========================================
    // Combat Event Hooks
    // ========================================

    public override void OnCombatStart(Enemy enemy, TraitConfiguration config, CombatState state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Ambush:
                // If hidden at start of combat, first attack is automatic critical hit
                if (enemy.IsHidden || enemy.IsStealth)
                {
                    config.StateCounter = 1; // Mark that ambush is ready
                    Log(state, $"[Ambush] {enemy.Name} lies in wait, ready to strike...");
                }
                break;

            case CreatureTrait.Territorial:
                // Store starting position for territorial bonus
                if (enemy.Position != null)
                {
                    config.SetMetadata("StartZone", (int)enemy.Position.Value.Zone);
                    config.SetMetadata("StartRow", (int)enemy.Position.Value.Row);
                    config.SetMetadata("StartColumn", enemy.Position.Value.Column);
                }
                break;

            case CreatureTrait.Flight:
                // Mark as flying for other systems
                enemy.IsFlying = true;
                break;
        }
    }

    // ========================================
    // Stat Modifiers
    // ========================================

    public override int GetAccuracyModifier(Enemy enemy, TraitConfiguration config, object? target, CombatState? state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Burrowing:
                // Emerges with +2 Accuracy on next attack after burrowing
                if (config.StateCounter > 0)
                {
                    config.StateCounter = 0; // Use the bonus
                    return config.PrimaryValue > 0 ? config.PrimaryValue : 2;
                }
                break;

            case CreatureTrait.Territorial:
                // +2 Accuracy while in starting zone
                if (IsInStartingZone(enemy, config))
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
            case CreatureTrait.Flight:
                // +2 Defense vs melee attacks
                // Note: Full implementation would check attack type
                return config.PrimaryValue > 0 ? config.PrimaryValue : 2;

            case CreatureTrait.Territorial:
                // +2 Defense while in starting zone
                if (IsInStartingZone(enemy, config))
                {
                    return config.SecondaryValue > 0 ? config.SecondaryValue : 2;
                }
                break;
        }
        return 0;
    }

    public override int GetMovementBonus(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Swiftness)
        {
            // +X tiles movement per turn
            return config.PrimaryValue > 0 ? config.PrimaryValue : 2;
        }
        return 0;
    }

    // ========================================
    // Movement Queries
    // ========================================

    public override bool? IgnoresAttacksOfOpportunity(TraitConfiguration config)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Phasing:
                // Can move through occupied tiles and obstacles
                return true;

            case CreatureTrait.HitAndRun:
                // After attacking, may move 1 tile without provoking AoO
                // Note: This is conditional - full implementation tracks attack state
                return null; // Handled specially
        }
        return null;
    }

    public override bool? CanMoveThrough(Enemy enemy, TraitConfiguration config, GridPosition position, CombatState state)
    {
        switch (config.Trait)
        {
            case CreatureTrait.Phasing:
                // Can move through occupied tiles and obstacles
                return true;

            case CreatureTrait.Burrowing:
                // Can move through Impassable terrain
                // Mark that we emerged (for accuracy bonus)
                config.StateCounter = 1;
                return true;

            case CreatureTrait.Flight:
                // Ignores ground-based hazards
                // Note: Full implementation would check tile type
                return true;
        }
        return null;
    }

    public override bool? IsImmobileToForce(TraitConfiguration config)
    {
        if (config.Trait == CreatureTrait.Anchored)
        {
            // Cannot be forcibly moved; immune to knockback/pull effects
            return true;
        }
        return null;
    }

    // ========================================
    // Helper Methods
    // ========================================

    /// <summary>
    /// Checks if enemy is in their starting zone (same zone/row, within 2 columns of start).
    /// </summary>
    private bool IsInStartingZone(Enemy enemy, TraitConfiguration config)
    {
        if (enemy.Position == null)
            return false;

        if (config.Metadata == null)
            return true; // No start recorded, assume in zone

        var startZone = config.GetMetadata<int>("StartZone");
        var startRow = config.GetMetadata<int>("StartRow");
        var startColumn = config.GetMetadata<int>("StartColumn");

        var pos = enemy.Position.Value;

        // Must be in same zone and row
        if ((int)pos.Zone != startZone || (int)pos.Row != startRow)
            return false;

        // Within 2 columns of start
        return Math.Abs(pos.Column - startColumn) <= 2;
    }

    /// <summary>
    /// Checks if Ambush trait should grant auto-crit.
    /// Call this before an attack and consume the bonus.
    /// </summary>
    public bool ShouldAmbushCrit(Enemy enemy)
    {
        var config = enemy.GetTraitConfig(CreatureTrait.Ambush);
        if (config != null && config.StateCounter > 0)
        {
            config.StateCounter = 0; // Consume the ambush
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if HitAndRun free movement is available.
    /// </summary>
    public bool CanHitAndRunMove(Enemy enemy)
    {
        if (!enemy.HasTrait(CreatureTrait.HitAndRun))
            return false;

        var config = enemy.GetTraitConfig(CreatureTrait.HitAndRun);
        // StateCounter tracks if attack happened this turn
        return config?.StateCounter > 0;
    }

    /// <summary>
    /// Marks that enemy attacked this turn (for HitAndRun).
    /// </summary>
    public void MarkAttacked(Enemy enemy)
    {
        var config = enemy.GetTraitConfig(CreatureTrait.HitAndRun);
        if (config != null)
        {
            config.StateCounter = 1;
        }
    }

    /// <summary>
    /// Gets HitAndRun free movement distance.
    /// </summary>
    public int GetHitAndRunDistance(Enemy enemy)
    {
        var config = enemy.GetTraitConfig(CreatureTrait.HitAndRun);
        return config?.PrimaryValue ?? 1;
    }
}
