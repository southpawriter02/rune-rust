using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.2: Service responsible for managing cover mechanics in tactical combat
/// Handles physical cover (blocks ranged attacks) and metaphysical cover (blocks psychic stress)
/// </summary>
public class CoverService
{
    private static readonly ILogger _log = Log.ForContext<CoverService>();

    /// <summary>
    /// Calculates cover bonuses for a target being attacked
    /// </summary>
    /// <param name="target">The combatant being attacked</param>
    /// <param name="attacker">The combatant attacking (can be null for environmental effects)</param>
    /// <param name="attackType">Type of attack (Melee, Ranged, Psychic)</param>
    /// <param name="grid">The battlefield grid</param>
    /// <returns>Cover bonuses to apply to defense/resolve checks</returns>
    public CoverBonus CalculateCoverBonus(object target, object? attacker, AttackType attackType, BattlefieldGrid grid)
    {
        var targetPosition = GetPosition(target);
        if (targetPosition == null)
        {
            _log.Warning("Cover calculation failed: Target has no position");
            return CoverBonus.None();
        }

        var targetTile = grid.GetTile(targetPosition.Value);
        if (targetTile == null)
        {
            _log.Warning("Cover calculation failed: Invalid target position {Position}", targetPosition);
            return CoverBonus.None();
        }

        var targetId = GetCombatantId(target);
        var attackerId = attacker != null ? GetCombatantId(attacker) : "environment";

        _log.Information("Cover calculation: Target={TargetId}, Attacker={AttackerId}, AttackType={AttackType}, TileCover={CoverType}",
            targetId, attackerId, attackType, targetTile.Cover);

        // Check if cover applies to this attack
        if (attacker != null && !CoverAppliesToAttack(target, attacker, attackType))
        {
            _log.Debug("Cover does not apply: same zone or melee attack");
            return CoverBonus.None();
        }

        // Apply cover bonuses based on type
        var coverBonus = new CoverBonus();

        switch (targetTile.Cover)
        {
            case CoverType.Physical:
                if (attackType == AttackType.Ranged)
                {
                    coverBonus.DefenseBonus = 4;
                    _log.Information("Physical cover applied: Target={TargetId}, DefenseBonus={Bonus}, Cover={Description}",
                        targetId, coverBonus.DefenseBonus, targetTile.CoverDescription ?? "Unknown");
                }
                break;

            case CoverType.Metaphysical:
                if (attackType == AttackType.Psychic)
                {
                    coverBonus.ResolveBonus = 4;
                    _log.Information("Metaphysical cover applied: Target={TargetId}, ResolveBonus={Bonus}, Cover={Description}",
                        targetId, coverBonus.ResolveBonus, targetTile.CoverDescription ?? "Unknown");
                }
                break;

            case CoverType.Both:
                if (attackType == AttackType.Ranged)
                {
                    coverBonus.DefenseBonus = 4;
                }
                if (attackType == AttackType.Psychic)
                {
                    coverBonus.ResolveBonus = 4;
                }
                _log.Information("Full cover applied: Target={TargetId}, DefenseBonus={DefenseBonus}, ResolveBonus={ResolveBonus}, Cover={Description}",
                    targetId, coverBonus.DefenseBonus, coverBonus.ResolveBonus, targetTile.CoverDescription ?? "Unknown");
                break;

            case CoverType.None:
                // No cover, no bonuses
                break;
        }

        return coverBonus;
    }

    /// <summary>
    /// Determines if cover should apply to this attack based on positioning and attack type
    /// </summary>
    private bool CoverAppliesToAttack(object target, object attacker, AttackType attackType)
    {
        var targetPosition = GetPosition(target);
        var attackerPosition = GetPosition(attacker);

        if (targetPosition == null || attackerPosition == null)
        {
            return false;
        }

        // Cover only works against attacks from opposing zone
        if (attackerPosition.Value.Zone == targetPosition.Value.Zone)
        {
            _log.Debug("Cover bypassed: Same zone combat (Zone={Zone})", targetPosition.Value.Zone);
            return false;
        }

        // Melee attacks ignore cover (close quarters combat)
        if (attackType == AttackType.Melee)
        {
            _log.Debug("Cover bypassed: Melee attack ignores cover");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Applies damage to cover, potentially destroying it
    /// </summary>
    /// <param name="tile">The tile with cover</param>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="combatState">Combat state for logging messages</param>
    public void DamageCover(BattlefieldTile tile, int damage, CombatState combatState)
    {
        if (tile.Cover != CoverType.Physical && tile.Cover != CoverType.Both)
        {
            _log.Debug("Cover cannot be damaged: CoverType={CoverType}, Position={Position}",
                tile.Cover, tile.Position);
            return;  // Only physical cover can be damaged
        }

        // Initialize cover health if not set
        if (!tile.CoverHealth.HasValue)
        {
            tile.CoverHealth = 20;  // Default cover HP
            _log.Debug("Cover health initialized: Position={Position}, HP={HP}", tile.Position, tile.CoverHealth);
        }

        var previousHealth = tile.CoverHealth.Value;
        tile.CoverHealth -= damage;

        _log.Information("Cover damaged: Position={Position}, Damage={Damage}, RemainingHP={HP}, PreviousHP={PreviousHP}, Description={Description}",
            tile.Position, damage, tile.CoverHealth, previousHealth, tile.CoverDescription ?? "Unknown");

        if (tile.CoverHealth <= 0)
        {
            DestroyCover(tile, combatState);
        }
    }

    /// <summary>
    /// Destroys cover on a tile
    /// </summary>
    private void DestroyCover(BattlefieldTile tile, CombatState combatState)
    {
        var previousCoverType = tile.Cover;
        var coverDescription = tile.CoverDescription ?? "cover";

        _log.Warning("Cover destroyed: Position={Position}, PreviousCoverType={CoverType}, Description={Description}",
            tile.Position, previousCoverType, coverDescription);

        // Preserve metaphysical cover if it was Both
        if (tile.Cover == CoverType.Both)
        {
            tile.Cover = CoverType.Metaphysical;
            _log.Information("Physical cover destroyed, metaphysical cover remains: Position={Position}", tile.Position);
            combatState.CombatLog.Add($"[COVER DESTROYED] The {coverDescription} at {tile.Position} has been obliterated! Metaphysical protection remains.");
        }
        else
        {
            tile.Cover = CoverType.None;
            combatState.CombatLog.Add($"[COVER DESTROYED] The {coverDescription} at {tile.Position} has been obliterated!");
        }

        tile.CoverHealth = null;
    }

    /// <summary>
    /// Creates metaphysical cover on a tile (e.g., from Vard-Warden abilities)
    /// </summary>
    /// <param name="tile">The tile to add cover to</param>
    /// <param name="description">Description of the cover source</param>
    public void CreateMetaphysicalCover(BattlefieldTile tile, string description)
    {
        var previousCoverType = tile.Cover;

        // Upgrade cover type
        if (tile.Cover == CoverType.Physical)
        {
            tile.Cover = CoverType.Both;
        }
        else if (tile.Cover == CoverType.None)
        {
            tile.Cover = CoverType.Metaphysical;
        }

        tile.CoverDescription = description;

        _log.Information("Metaphysical cover created: Position={Position}, PreviousCoverType={PreviousCoverType}, NewCoverType={NewCoverType}, Description={Description}",
            tile.Position, previousCoverType, tile.Cover, description);
    }

    /// <summary>
    /// Helper method to get position from either PlayerCharacter or Enemy
    /// </summary>
    private GridPosition? GetPosition(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };
    }

    /// <summary>
    /// Helper method to get combatant ID for logging
    /// </summary>
    private string GetCombatantId(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => $"Player:{player.Name}",
            Enemy enemy => $"Enemy:{enemy.Name}({enemy.Id})",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Represents the defensive bonuses provided by cover
/// </summary>
public class CoverBonus
{
    public int DefenseBonus { get; set; }   // Added to Defense pool for physical attacks
    public int ResolveBonus { get; set; }   // Added to Resolve pool for psychic attacks

    public static CoverBonus None()
    {
        return new CoverBonus { DefenseBonus = 0, ResolveBonus = 0 };
    }

    public bool HasBonus()
    {
        return DefenseBonus > 0 || ResolveBonus > 0;
    }
}

/// <summary>
/// Type of attack for determining cover effectiveness
/// </summary>
public enum AttackType
{
    Melee,      // Close combat, ignores cover
    Ranged,     // Projectile/energy attacks, blocked by physical cover
    Psychic     // Psychic stress, blocked by metaphysical cover
}
