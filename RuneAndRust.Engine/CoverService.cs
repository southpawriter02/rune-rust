using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.2: Cover System Service
/// Handles physical and metaphysical cover mechanics in tactical combat.
///
/// Philosophy: Environment provides protection from attacks and psychic threats.
/// - Physical Cover: Blocks ranged attacks (+4 Defense vs ranged)
/// - Metaphysical Cover: Blocks Psychic Stress (+4 WILL for Resolve Checks)
/// - Cover is destructible (physical only, 20 HP default)
/// - Cover only applies when attacking from opposing zones
/// </summary>
public class CoverService
{
    private static readonly ILogger _log = Log.ForContext<CoverService>();

    /// <summary>
    /// Calculates cover bonuses for a combatant being targeted.
    /// Cover only applies to ranged/psychic attacks from opposing zones.
    /// </summary>
    /// <param name="targetPosition">Position of the target combatant</param>
    /// <param name="attackerPosition">Position of the attacker (null for environmental effects)</param>
    /// <param name="attackType">Type of attack (Melee, Ranged, Psychic)</param>
    /// <param name="grid">The battlefield grid</param>
    /// <returns>Cover bonus with defense and resolve bonuses</returns>
    public CoverBonus CalculateCoverBonus(GridPosition? targetPosition, GridPosition? attackerPosition, AttackType attackType, BattlefieldGrid grid)
    {
        // No cover if no position data
        if (targetPosition == null || grid == null)
        {
            return CoverBonus.None();
        }

        var targetTile = grid.GetTile(targetPosition);
        if (targetTile == null)
        {
            return CoverBonus.None();
        }

        var coverBonus = new CoverBonus();

        _log.Debug("Cover calculation: TargetPosition={TargetPosition}, AttackerPosition={AttackerPosition}, AttackType={AttackType}, TileCover={CoverType}",
            targetPosition, attackerPosition, attackType, targetTile.Cover);

        // Check if cover applies to this attack
        if (!CoverAppliesToAttack(targetPosition, attackerPosition, attackType))
        {
            _log.Debug("Cover does not apply: same zone or melee attack");
            return CoverBonus.None();
        }

        // Apply cover bonuses based on type
        switch (targetTile.Cover)
        {
            case CoverType.Physical:
                if (attackType == AttackType.Ranged)
                {
                    coverBonus.DefenseBonus = 4;
                    _log.Information("Physical cover applied: TargetPosition={Position}, DefenseBonus={Bonus}",
                        targetPosition, coverBonus.DefenseBonus);
                }
                break;

            case CoverType.Metaphysical:
                if (attackType == AttackType.Psychic)
                {
                    coverBonus.ResolveBonus = 4;
                    _log.Information("Metaphysical cover applied: TargetPosition={Position}, ResolveBonus={Bonus}",
                        targetPosition, coverBonus.ResolveBonus);
                }
                break;

            case CoverType.Both:
                if (attackType == AttackType.Ranged)
                    coverBonus.DefenseBonus = 4;
                if (attackType == AttackType.Psychic)
                    coverBonus.ResolveBonus = 4;
                _log.Information("Full cover applied: TargetPosition={Position}, DefenseBonus={DefenseBonus}, ResolveBonus={ResolveBonus}",
                    targetPosition, coverBonus.DefenseBonus, coverBonus.ResolveBonus);
                break;
        }

        return coverBonus;
    }

    /// <summary>
    /// Determines if cover applies to a given attack.
    /// Cover only works against ranged/psychic attacks from opposing zones.
    /// </summary>
    private bool CoverAppliesToAttack(GridPosition? targetPosition, GridPosition? attackerPosition, AttackType attackType)
    {
        // Cover only applies if we know both positions
        if (targetPosition == null || attackerPosition == null)
        {
            return false;
        }

        // Cover only works against ranged attacks from opposing zone
        if (attackerPosition.Zone == targetPosition.Zone)
        {
            return false;  // Same zone, no cover
        }

        // Melee attacks ignore cover (close quarters combat)
        if (attackType == AttackType.Melee)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Applies damage to physical cover, potentially destroying it.
    /// Cover takes 25% of the damage dealt to the protected target.
    /// </summary>
    /// <param name="tile">The tile with cover</param>
    /// <param name="damageToTarget">The damage dealt to the target behind cover</param>
    /// <returns>Combat log message if cover was damaged/destroyed, null otherwise</returns>
    public string? DamageCover(BattlefieldTile tile, int damageToTarget)
    {
        if (tile == null || tile.Cover == CoverType.None)
        {
            return null;
        }

        // Only physical cover can be damaged
        if (tile.Cover != CoverType.Physical && tile.Cover != CoverType.Both)
        {
            return null;
        }

        // Initialize cover health if not set
        if (!tile.CoverHealth.HasValue)
        {
            tile.CoverHealth = 20;  // Default cover HP
        }

        // Cover takes 25% of damage dealt
        int coverDamage = Math.Max(1, damageToTarget / 4);
        tile.CoverHealth -= coverDamage;

        _log.Information("Cover damaged: Position={Position}, Damage={Damage}, RemainingHP={HP}, CoverType={CoverType}",
            tile.Position, coverDamage, tile.CoverHealth, tile.Cover);

        if (tile.CoverHealth <= 0)
        {
            return DestroyCover(tile);
        }

        return $"The {tile.CoverDescription ?? "cover"} at {tile.Position} absorbs some damage! ({tile.CoverHealth} HP remaining)";
    }

    /// <summary>
    /// Destroys physical cover on a tile.
    /// If tile has Both types, preserves metaphysical cover.
    /// </summary>
    private string DestroyCover(BattlefieldTile tile)
    {
        var previousCoverType = tile.Cover;
        var description = tile.CoverDescription ?? "cover";

        _log.Warning("Cover destroyed: Position={Position}, PreviousCoverType={CoverType}, Description={Description}",
            tile.Position, previousCoverType, description);

        // Preserve metaphysical cover if it was Both
        if (tile.Cover == CoverType.Both)
        {
            tile.Cover = CoverType.Metaphysical;
            tile.CoverDescription = "Runic Anchor"; // Metaphysical cover remains
        }
        else
        {
            tile.Cover = CoverType.None;
            tile.CoverDescription = null;
        }

        tile.CoverHealth = null;

        // Layer 2 diagnostic voice for destruction
        return $"[COVER DESTROYED] The {description} at {tile.Position} has been obliterated! Structural integrity compromised.";
    }

    /// <summary>
    /// Places cover on a tile with specified type and health.
    /// </summary>
    /// <param name="tile">Tile to place cover on</param>
    /// <param name="coverType">Type of cover to place</param>
    /// <param name="description">Optional description (auto-generated if null)</param>
    /// <param name="coverHealth">Optional health (defaults to 20 for physical cover)</param>
    public void PlaceCover(BattlefieldTile tile, CoverType coverType, string? description = null, int? coverHealth = null)
    {
        if (tile == null || coverType == CoverType.None)
        {
            return;
        }

        tile.Cover = coverType;

        // Set cover health for physical cover types
        if (coverType == CoverType.Physical || coverType == CoverType.Both)
        {
            tile.CoverHealth = coverHealth ?? 20;  // Default 20 HP
            tile.CoverDescription = description ?? SelectPhysicalCoverDescription();
        }

        // Set description for metaphysical cover
        if (coverType == CoverType.Metaphysical || coverType == CoverType.Both)
        {
            if (coverType == CoverType.Both && description == null)
            {
                tile.CoverDescription = "Sanctified Barricade"; // Both types
            }
            else if (coverType == CoverType.Metaphysical)
            {
                tile.CoverDescription = description ?? "Runic Anchor";
            }
        }

        _log.Information("Cover placed: Position={Position}, Type={CoverType}, Description={Description}, HP={HP}",
            tile.Position, coverType, tile.CoverDescription, tile.CoverHealth);
    }

    /// <summary>
    /// Selects a random physical cover description for flavor.
    /// </summary>
    private string SelectPhysicalCoverDescription()
    {
        var descriptions = new[] { "Pillar", "Crate", "Debris", "Wall Section", "Console", "Barrel" };
        return descriptions[new Random().Next(descriptions.Length)];
    }
}

/// <summary>
/// Represents bonuses granted by cover to a combatant.
/// </summary>
public class CoverBonus
{
    public int DefenseBonus { get; set; }      // Additional defense dice vs ranged attacks
    public int ResolveBonus { get; set; }      // Additional WILL dice vs psychic stress

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
/// Types of attacks for cover calculation.
/// </summary>
public enum AttackType
{
    Melee,      // Close-quarters combat (ignores cover)
    Ranged,     // Projectile/energy attacks (blocked by physical cover)
    Psychic     // Mental/spiritual attacks (blocked by metaphysical cover)
}
