using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.1: Service responsible for detecting flanking and calculating tactical positioning bonuses
///
/// Philosophy: Flanking represents targeting algorithm overload - a combatant's threat-assessment
/// subroutines are overwhelmed by contradictory attack vectors from multiple spatial coordinates.
/// </summary>
public class FlankingService
{
    private static readonly ILogger _log = Log.ForContext<FlankingService>();

    /// <summary>
    /// Calculates whether a target is flanked by checking for threats from multiple angles
    /// Flanking requires: 2+ threats, from different columns, with 90°+ angular separation
    /// </summary>
    public FlankingResult CalculateFlanking(object target, BattlefieldGrid grid)
    {
        var (targetPosition, targetId, targetName, isPlayer) = target switch
        {
            PlayerCharacter player => (player.Position, "player", player.Name, true),
            Enemy enemy => (enemy.Position, enemy.Id, enemy.Name, false),
            _ => throw new ArgumentException("Invalid target type")
        };

        if (targetPosition == null)
        {
            return FlankingResult.NotFlanked();
        }

        _log.Information("Flanking calculation: Target={TargetName}, Position={Position}",
            targetName, targetPosition);

        var threats = GetThreateningEnemies(target, grid);

        if (threats.Count < 2)
        {
            _log.Debug("Insufficient threats for flanking: Target={TargetName}, ThreatCount={ThreatCount}",
                targetName, threats.Count);
            return FlankingResult.NotFlanked();
        }

        // Check if threats are from different columns
        var uniqueColumns = threats.Select(t => GetPosition(t).Column).Distinct().Count();

        if (uniqueColumns < 2)
        {
            _log.Debug("Threats from same column, no flanking: Target={TargetName}",
                targetName);
            return FlankingResult.NotFlanked();
        }

        // Calculate angular separation
        var angles = CalculateThreatAngles(targetPosition.Value, threats);
        bool significantSeparation = angles.Max() - angles.Min() >= 90.0;

        if (!significantSeparation)
        {
            _log.Debug("Insufficient angular separation: Target={TargetName}, AngleRange={AngleRange}",
                targetName, angles.Max() - angles.Min());
            return FlankingResult.NotFlanked();
        }

        _log.Information("Flanking detected: Target={TargetName}, Threats={ThreatCount}, Columns={UniqueColumns}, AngleSeparation={AngleSeparation}",
            targetName, threats.Count, uniqueColumns, angles.Max() - angles.Min());

        return FlankingResult.Flanked(threats);
    }

    /// <summary>
    /// Gets all combatants that are threatening the target
    /// A threat must be: alive, conscious, enemy faction, and in range
    /// </summary>
    private List<object> GetThreateningEnemies(object target, BattlefieldGrid grid)
    {
        var threats = new List<object>();
        var (_, _, _, isPlayerTarget) = target switch
        {
            PlayerCharacter _ => (null, null, null, true),
            Enemy _ => (null, null, null, false),
            _ => throw new ArgumentException("Invalid target type")
        };

        // For player targets, check all enemies
        if (isPlayerTarget && target is PlayerCharacter player)
        {
            // Get all enemies from grid tiles
            foreach (var tile in grid.Tiles.Values.Where(t => t.IsOccupied))
            {
                if (tile.OccupantId != "player" && tile.OccupantId != null)
                {
                    // Find the enemy by position
                    // We need to search through a list of enemies - this will be passed from combat state
                    // For now, we'll use a simpler approach by checking the tile's zone
                    if (tile.Position.Zone != player.Position?.Zone)
                    {
                        // This is an enemy tile - we'll need the actual enemy object
                        // This will be provided through the grid or combat state
                        // For now, skip as we need enemy list from combat state
                    }
                }
            }
        }

        return threats;
    }

    /// <summary>
    /// Improved version that takes a list of all combatants
    /// </summary>
    public List<object> GetThreateningCombatants(object target, List<object> allCombatants, BattlefieldGrid grid)
    {
        var threats = new List<object>();
        var isPlayerTarget = target is PlayerCharacter;

        foreach (var combatant in allCombatants)
        {
            // Skip if same faction
            bool combatantIsPlayer = combatant is PlayerCharacter;
            if (isPlayerTarget == combatantIsPlayer)
                continue;

            // Skip self
            if (combatant == target)
                continue;

            if (CanThreaten(combatant, target, grid))
            {
                threats.Add(combatant);
            }
        }

        return threats;
    }

    /// <summary>
    /// Checks if an attacker can threaten a target (is in range to attack)
    /// </summary>
    private bool CanThreaten(object attacker, object target, BattlefieldGrid grid)
    {
        // Extract attacker properties
        var (attackerPosition, attackerHP, attackerIncapacitated, attackerHasReach) = attacker switch
        {
            PlayerCharacter player => (player.Position, player.HP, false, HasReachWeapon(player)),
            Enemy enemy => (enemy.Position, enemy.HP, enemy.IsStunned, false),
            _ => throw new ArgumentException("Invalid attacker type")
        };

        // Extract target properties
        var targetPosition = target switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => throw new ArgumentException("Invalid target type")
        };

        // Must be conscious
        if (attackerHP <= 0 || attackerIncapacitated)
            return false;

        // Must have position
        if (attackerPosition == null || targetPosition == null)
            return false;

        // Must be in range (melee or reach)
        if (IsInMeleeRange(attackerPosition.Value, targetPosition.Value))
        {
            return true;
        }

        if (attackerHasReach && IsInReachRange(attackerPosition.Value, targetPosition.Value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if two positions are in melee range (Front row to Front row across zones, or same zone/row)
    /// </summary>
    private bool IsInMeleeRange(GridPosition attacker, GridPosition target)
    {
        // Same zone: must be in same row and adjacent column (or same tile)
        if (attacker.Zone == target.Zone)
        {
            return attacker.Row == target.Row && Math.Abs(attacker.Column - target.Column) <= 1;
        }

        // Different zones: both must be in Front row and same column
        return attacker.Row == Row.Front && target.Row == Row.Front && attacker.Column == target.Column;
    }

    /// <summary>
    /// Checks if two positions are in reach range (Back row to Front row across zones)
    /// </summary>
    private bool IsInReachRange(GridPosition attacker, GridPosition target)
    {
        // Reach allows attacking from back row to enemy front row
        if (attacker.Zone != target.Zone)
        {
            return (attacker.Row == Row.Back && target.Row == Row.Front) ||
                   (attacker.Row == Row.Front && target.Row == Row.Back);
        }

        // Within same zone, reach allows back row to attack front row (same or adjacent column)
        if (attacker.Zone == target.Zone && attacker.Row == Row.Back && target.Row == Row.Front)
        {
            return Math.Abs(attacker.Column - target.Column) <= 1;
        }

        return false;
    }

    /// <summary>
    /// Checks if a player character has a reach weapon equipped
    /// </summary>
    private bool HasReachWeapon(PlayerCharacter player)
    {
        // Check for Atgeir or other reach weapons (Spear category or Atgeir in name)
        if (player.EquippedWeapon != null)
        {
            return player.EquippedWeapon.Name.Contains("Atgeir", StringComparison.OrdinalIgnoreCase) ||
                   player.EquippedWeapon.WeaponCategory == WeaponCategory.Spear;
        }

        return false;
    }

    /// <summary>
    /// Calculates the angles of all threats relative to the target
    /// Simplified to use column positions as angular approximation
    /// </summary>
    private List<double> CalculateThreatAngles(GridPosition targetPosition, List<object> threats)
    {
        var angles = new List<double>();

        foreach (var threat in threats)
        {
            var threatPosition = GetPosition(threat);

            // Calculate angle based on relative position
            // Front vs Back = 180 degrees
            // Left vs Right column = approximate angular offset

            double angle = 0.0;

            // Base angle from row difference
            if (threatPosition.Row != targetPosition.Row)
            {
                angle += 90.0; // Row difference adds significant angle
            }

            // Add column-based angular offset
            int columnDiff = threatPosition.Column - targetPosition.Column;
            angle += columnDiff * 30.0; // Each column adds ~30 degrees

            // Zone difference (opposite sides)
            if (threatPosition.Zone != targetPosition.Zone)
            {
                angle += 180.0;
            }

            angles.Add(Math.Abs(angle));
        }

        return angles;
    }

    /// <summary>
    /// Calculates the flanking bonus for an attack
    /// Returns: +2 Accuracy, +10% Crit, -2 Enemy Defense if target is flanked
    /// </summary>
    public FlankingBonus CalculateFlankingBonus(object attacker, object target, List<object> allCombatants, BattlefieldGrid grid)
    {
        var flankingResult = CalculateFlanking(target, grid);

        // Check if we need to use the combatants list
        if (flankingResult.Threats.Count == 0)
        {
            flankingResult = new FlankingResult
            {
                IsFlanked = false,
                Threats = GetThreateningCombatants(target, allCombatants, grid)
            };

            // Recalculate if we have threats now
            if (flankingResult.Threats.Count >= 2)
            {
                var targetPosition = GetPosition(target);
                var uniqueColumns = flankingResult.Threats.Select(t => GetPosition(t).Column).Distinct().Count();

                if (uniqueColumns >= 2)
                {
                    var angles = CalculateThreatAngles(targetPosition, flankingResult.Threats);
                    if (angles.Max() - angles.Min() >= 90.0)
                    {
                        flankingResult.IsFlanked = true;
                    }
                }
            }
        }

        if (!flankingResult.IsFlanked)
        {
            return FlankingBonus.None();
        }

        var attackerName = attacker switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Name,
            _ => "Unknown"
        };

        var targetName = target switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Name,
            _ => "Unknown"
        };

        var bonus = new FlankingBonus
        {
            AccuracyBonus = 2,           // +2 dice to Accuracy Pool
            CriticalHitBonus = 0.10f,    // +10% crit chance
            DefensePenalty = 2           // -2 dice to target's Defense
        };

        _log.Information("Flanking bonus applied: Attacker={AttackerId}, Target={TargetId}, AccuracyBonus={AccuracyBonus}, DefensePenalty={DefensePenalty}, CritBonus={CritBonus}",
            attackerName, targetName, bonus.AccuracyBonus, bonus.DefensePenalty, bonus.CriticalHitBonus);

        return bonus;
    }

    /// <summary>
    /// Helper to get position from any combatant type
    /// </summary>
    private GridPosition GetPosition(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.Position ?? new GridPosition(Zone.Player, Row.Front, 0),
            Enemy enemy => enemy.Position ?? new GridPosition(Zone.Enemy, Row.Front, 0),
            _ => new GridPosition(Zone.Player, Row.Front, 0)
        };
    }
}

/// <summary>
/// Result of a flanking calculation
/// </summary>
public class FlankingResult
{
    public bool IsFlanked { get; set; }
    public List<object> Threats { get; set; } = new();

    public static FlankingResult Flanked(List<object> threats)
    {
        return new FlankingResult { IsFlanked = true, Threats = threats };
    }

    public static FlankingResult NotFlanked()
    {
        return new FlankingResult { IsFlanked = false, Threats = new List<object>() };
    }
}

/// <summary>
/// Bonuses applied when attacking a flanked target
/// </summary>
public class FlankingBonus
{
    public int AccuracyBonus { get; set; }          // +2 dice to Accuracy Pool
    public float CriticalHitBonus { get; set; }     // +10% crit chance
    public int DefensePenalty { get; set; }         // -2 dice to target's Defense

    public static FlankingBonus None()
    {
        return new FlankingBonus { AccuracyBonus = 0, CriticalHitBonus = 0f, DefensePenalty = 0 };
    }
}
