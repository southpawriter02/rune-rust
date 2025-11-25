using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.34.2: Companion AI Service
/// Handles AI decision-making for companion NPCs in tactical combat.
/// Implements stance-based behavior (Aggressive/Defensive/Passive).
/// </summary>
public class CompanionAIService
{
    private readonly CoverService _coverService;
    private readonly FlankingService _flankingService;
    private readonly ILogger _log;

    public CompanionAIService(
        CoverService coverService,
        FlankingService flankingService,
        ILogger logger)
    {
        _coverService = coverService;
        _flankingService = flankingService;
        _log = logger.ForContext<CompanionAIService>();
    }

    /// <summary>
    /// Select companion action based on stance and battlefield state
    /// </summary>
    public CompanionAction SelectAction(
        Companion companion,
        PlayerCharacter player,
        List<Enemy> enemies,
        BattlefieldGrid? grid = null)
    {
        _log.Information(
            "SelectAction for {CompanionName} with stance {Stance}",
            companion.DisplayName,
            companion.CurrentStance);

        // Passive stance: No autonomous action
        if (companion.CurrentStance == "passive")
        {
            _log.Debug("Companion in Passive stance, no action");
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = "Passive stance"
            };
        }

        // Check if companion needs to retreat to safety (low HP)
        if (ShouldRetreat(companion, enemies))
        {
            var safePosition = FindSafePosition(companion, player, enemies, grid);
            return new CompanionAction
            {
                ActionType = "Move",
                TargetPosition = safePosition,
                Reason = "Retreat to safety (low HP)"
            };
        }

        // Defensive stance: Protect allies first
        if (companion.CurrentStance == "defensive")
        {
            var threatenedAlly = FindThreatenedAlly(player, enemies);
            if (threatenedAlly != null)
            {
                var protectAction = PlanProtectAction(companion, threatenedAlly, enemies);
                if (protectAction != null)
                {
                    return protectAction;
                }
            }
        }

        // Select target
        var target = SelectTarget(companion, enemies, player);
        if (target == null)
        {
            _log.Warning("No valid target found");
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = "No valid targets"
            };
        }

        // Decide: Ability or basic attack?
        var abilityAction = ShouldUseAbility(companion, target, enemies);
        if (abilityAction != null)
        {
            return abilityAction;
        }

        // Default: Basic attack
        return new CompanionAction
        {
            ActionType = "Attack",
            TargetEnemy = target,
            Reason = "Basic attack on optimal target"
        };
    }

    /// <summary>
    /// Select optimal enemy target based on stance and threat
    /// </summary>
    public Enemy? SelectTarget(
        Companion companion,
        List<Enemy> enemies,
        PlayerCharacter player)
    {
        var validTargets = enemies
            .Where(e => e.HP > 0)
            .ToList();

        if (!validTargets.Any())
        {
            return null;
        }

        _log.Debug(
            "SelectTarget for {CompanionName}, {Count} valid enemies",
            companion.DisplayName,
            validTargets.Count);

        return companion.CurrentStance switch
        {
            "aggressive" => SelectAggressiveTarget(companion, validTargets),
            "defensive" => SelectDefensiveTarget(companion, validTargets, player),
            _ => validTargets.First() // Fallback
        };
    }

    private Enemy SelectAggressiveTarget(
        Companion companion,
        List<Enemy> enemies)
    {
        // Aggressive priority:
        // 1. Wounded enemies (finish them off)
        // 2. High-damage threats
        // 3. Closest enemies

        var wounded = enemies
            .Where(e => e.HP < e.MaxHP * 0.5)
            .OrderBy(e => e.HP)
            .ToList();

        if (wounded.Any())
        {
            _log.Debug("Aggressive: Targeting wounded enemy {EnemyName}",
                wounded.First().Name);
            return wounded.First();
        }

        // Prioritize high-threat enemies
        var highThreat = enemies
            .OrderByDescending(e => EvaluateThreat(e))
            .First();

        _log.Debug("Aggressive: Targeting high-threat enemy {EnemyName}",
            highThreat.Name);
        return highThreat;
    }

    private Enemy SelectDefensiveTarget(
        Companion companion,
        List<Enemy> enemies,
        PlayerCharacter player)
    {
        // Defensive priority:
        // 1. Enemies threatening player
        // 2. Enemies closest to player
        // 3. Weakest enemies (reliable takedowns)

        var playerPosition = player.Position;
        if (playerPosition == null)
        {
            // Fallback to first enemy if player has no position
            return enemies.First();
        }

        var threateningPlayer = enemies
            .Where(e => e.Position != null && IsAdjacentTo(e.Position.Value, playerPosition.Value))
            .OrderByDescending(e => EvaluateThreat(e))
            .ToList();

        if (threateningPlayer.Any())
        {
            _log.Debug("Defensive: Targeting enemy threatening player {EnemyName}",
                threateningPlayer.First().Name);
            return threateningPlayer.First();
        }

        // Target closest to player
        var closestToPlayer = enemies
            .Where(e => e.Position != null)
            .OrderBy(e => DistanceTo(e.Position!.Value, playerPosition.Value))
            .FirstOrDefault() ?? enemies.First();

        _log.Debug("Defensive: Targeting enemy closest to player {EnemyName}",
            closestToPlayer.Name);
        return closestToPlayer;
    }

    /// <summary>
    /// Evaluate threat level of an enemy
    /// </summary>
    public int EvaluateThreat(Enemy enemy)
    {
        int threat = 0;

        // Base threat from damage output
        threat += enemy.BaseDamageDice * 10;

        // Additional threat from damage bonus
        threat += enemy.DamageBonus * 5;

        // Boss enemies are high threat
        if (enemy.IsBoss)
        {
            threat += 30;
        }

        // Champion enemies are higher threat
        if (enemy.IsChampion)
        {
            threat += 20;
        }

        // Forlorn enemies inflict psychic stress
        if (enemy.IsForlorn)
        {
            threat += 15;
        }

        // Low HP enemies less threatening
        if (enemy.HP < enemy.MaxHP * 0.3)
        {
            threat = (int)(threat * 0.5);
        }

        _log.Debug("Threat evaluation for {EnemyName}: {Threat}",
            enemy.Name, threat);

        return threat;
    }

    /// <summary>
    /// Find optimal position for tactical advantage
    /// </summary>
    public GridPosition? FindOptimalPosition(
        Companion companion,
        PlayerCharacter player,
        List<Enemy> enemies,
        Enemy target,
        BattlefieldGrid? grid = null)
    {
        var currentPos = companion.Position;
        if (currentPos == null)
        {
            _log.Warning("Companion has no position, cannot find optimal position");
            return null;
        }

        // Generate valid movement positions (simplified - would use positioning service)
        var validPositions = GetValidMovementPositions(currentPos.Value, companion.MovementRange);

        if (!validPositions.Any())
        {
            _log.Warning("No valid movement positions found");
            return currentPos;
        }

        // Score each position based on tactical factors
        var scoredPositions = validPositions
            .Select(pos => new
            {
                Position = pos,
                Score = ScorePosition(pos, companion, player, enemies, target, grid)
            })
            .OrderByDescending(p => p.Score)
            .ToList();

        var bestPosition = scoredPositions.First().Position;

        _log.Information(
            "Optimal position for {CompanionName}: {Position} (score {Score})",
            companion.DisplayName,
            bestPosition,
            scoredPositions.First().Score);

        return bestPosition;
    }

    private int ScorePosition(
        GridPosition position,
        Companion companion,
        PlayerCharacter player,
        List<Enemy> enemies,
        Enemy target,
        BattlefieldGrid? grid)
    {
        int score = 0;

        var targetPos = target.Position;
        var playerPos = player.Position;

        // Flanking bonus (if player and companion on opposite sides)
        if (targetPos != null && playerPos != null)
        {
            if (HasFlankingOn(position, targetPos.Value, playerPos.Value))
            {
                score += 30;
            }
        }

        // Cover bonus (Defensive stance)
        if (companion.CurrentStance == "defensive" && grid != null)
        {
            if (HasCover(position, grid))
            {
                score += 20;
            }

            // Stay near player
            if (playerPos != null)
            {
                var distanceToPlayer = DistanceTo(position, playerPos.Value);
                score += (10 - distanceToPlayer) * 5; // Closer = better
            }
        }

        // Aggressive stance: Get closer to enemies
        if (companion.CurrentStance == "aggressive" && targetPos != null)
        {
            var distanceToTarget = DistanceTo(position, targetPos.Value);
            score += (10 - distanceToTarget) * 3; // Closer = better
        }

        // Avoid hazards (would check grid for hazards)
        if (grid != null && IsHazard(position, grid))
        {
            score -= 50;
        }

        // Avoid clustering with other allies
        if (playerPos != null && IsAdjacentTo(position, playerPos.Value))
        {
            score -= 10; // Penalty for blocking player
        }

        return score;
    }

    /// <summary>
    /// Decide if companion should use ability vs. basic attack
    /// </summary>
    public CompanionAction? ShouldUseAbility(
        Companion companion,
        Enemy target,
        List<Enemy> enemies)
    {
        var availableAbilities = companion.Abilities
            .Where(a => companion.CanUseAbility(a))
            .ToList();

        if (!availableAbilities.Any())
        {
            return null; // Use basic attack
        }

        // Evaluate each ability
        foreach (var ability in availableAbilities)
        {
            // AOE abilities: Use if multiple enemies clustered
            if (ability.IsAOE)
            {
                var targetPos = target.Position;
                if (targetPos != null)
                {
                    var clusteredEnemies = enemies
                        .Where(e => e.Position != null &&
                                    DistanceTo(e.Position.Value, targetPos.Value) <= ability.AOERadius)
                        .Count();

                    if (clusteredEnemies >= 2)
                    {
                        _log.Information(
                            "Using AOE ability {AbilityName} on {Count} enemies",
                            ability.AbilityName,
                            clusteredEnemies);

                        return new CompanionAction
                        {
                            ActionType = "UseAbility",
                            AbilityName = ability.AbilityName,
                            TargetEnemy = target,
                            Reason = $"AOE ability hits {clusteredEnemies} enemies"
                        };
                    }
                }
            }

            // Healing abilities: Use if ally below 50% HP
            if (ability.IsHeal)
            {
                // Check companion HP or player HP
                if (companion.CurrentHitPoints < companion.MaxHitPoints * 0.5)
                {
                    _log.Information(
                        "Using heal ability {AbilityName} on self",
                        ability.AbilityName);

                    return new CompanionAction
                    {
                        ActionType = "UseAbility",
                        AbilityName = ability.AbilityName,
                        TargetSelf = true,
                        Reason = "Self-heal (below 50% HP)"
                    };
                }
            }

            // Buff abilities: Use at combat start
            if (ability.IsBuff && !companion.HasActiveBuff(ability.AbilityName))
            {
                _log.Information(
                    "Using buff ability {AbilityName}",
                    ability.AbilityName);

                return new CompanionAction
                {
                    ActionType = "UseAbility",
                    AbilityName = ability.AbilityName,
                    TargetSelf = true,
                    Reason = "Apply buff"
                };
            }

            // High-damage abilities: Use on high-threat targets
            if (ability.DamageMultiplier > 1.5)
            {
                var threatScore = EvaluateThreat(target);
                if (threatScore > 30)
                {
                    _log.Information(
                        "Using high-damage ability {AbilityName} on high-threat target",
                        ability.AbilityName);

                    return new CompanionAction
                    {
                        ActionType = "UseAbility",
                        AbilityName = ability.AbilityName,
                        TargetEnemy = target,
                        Reason = "High-damage ability on high-threat target"
                    };
                }
            }
        }

        // No compelling ability use case
        return null;
    }

    /// <summary>
    /// Check if companion should retreat to safety
    /// </summary>
    private bool ShouldRetreat(Companion companion, List<Enemy> enemies)
    {
        // Retreat if below 30% HP and enemies adjacent
        if (companion.CurrentHitPoints < companion.MaxHitPoints * 0.3)
        {
            var companionPos = companion.Position;
            if (companionPos != null)
            {
                var adjacentEnemies = enemies
                    .Where(e => e.Position != null && IsAdjacentTo(e.Position.Value, companionPos.Value))
                    .Count();

                return adjacentEnemies > 0;
            }
        }

        return false;
    }

    private GridPosition? FindSafePosition(
        Companion companion,
        PlayerCharacter player,
        List<Enemy> enemies,
        BattlefieldGrid? grid = null)
    {
        var currentPos = companion.Position;
        if (currentPos == null)
        {
            return null;
        }

        var validPositions = GetValidMovementPositions(currentPos.Value, companion.MovementRange);

        // Find position with no adjacent enemies and closest to player
        var playerPos = player.Position;
        if (playerPos == null)
        {
            return currentPos;
        }

        var safePositions = validPositions
            .Where(pos => !enemies.Any(e => e.Position != null && IsAdjacentTo(e.Position.Value, pos)))
            .OrderBy(pos => DistanceTo(pos, playerPos.Value))
            .ToList();

        return safePositions.FirstOrDefault() ?? currentPos;
    }

    private PlayerCharacter? FindThreatenedAlly(
        PlayerCharacter player,
        List<Enemy> enemies)
    {
        var playerPos = player.Position;
        if (playerPos == null)
        {
            return null;
        }

        // Check if player is threatened (low HP + adjacent enemies)
        if (player.HP < player.MaxHP * 0.4)
        {
            var adjacentEnemies = enemies
                .Where(e => e.Position != null && IsAdjacentTo(e.Position.Value, playerPos.Value))
                .Count();

            if (adjacentEnemies > 0)
            {
                return player;
            }
        }

        // TODO: Check other companions in party
        return null;
    }

    private CompanionAction? PlanProtectAction(
        Companion companion,
        PlayerCharacter threatenedAlly,
        List<Enemy> enemies)
    {
        var allyPos = threatenedAlly.Position;
        if (allyPos == null)
        {
            return null;
        }

        // Find enemy threatening ally
        var threat = enemies
            .Where(e => e.Position != null && IsAdjacentTo(e.Position.Value, allyPos.Value))
            .OrderByDescending(e => EvaluateThreat(e))
            .FirstOrDefault();

        if (threat == null)
        {
            return null;
        }

        // Attack threatening enemy
        return new CompanionAction
        {
            ActionType = "Attack",
            TargetEnemy = threat,
            Reason = $"Protect {threatenedAlly.Name} from {threat.Name}"
        };
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private bool IsAdjacentTo(GridPosition pos1, GridPosition pos2)
    {
        // Adjacent if same zone and (same row, column ±1) or (column same, row different)
        if (pos1.Zone != pos2.Zone)
            return false;

        if (pos1.Row == pos2.Row)
        {
            return Math.Abs(pos1.Column - pos2.Column) == 1;
        }

        // Different rows, same column
        return pos1.Column == pos2.Column;
    }

    private int DistanceTo(GridPosition pos1, GridPosition pos2)
    {
        // Manhattan distance (simplified for grid)
        if (pos1.Zone != pos2.Zone)
            return 10; // Different zones are far apart

        int rowDist = pos1.Row == pos2.Row ? 0 : 1;
        int colDist = Math.Abs(pos1.Column - pos2.Column);

        return rowDist + colDist;
    }

    private bool HasFlankingOn(GridPosition attackerPos, GridPosition targetPos, GridPosition allyPos)
    {
        // Simplified flanking: Attacker and ally on opposite sides of target
        // For grid-based system: check if they're in opposite columns or rows

        // Must all be in same zone
        if (attackerPos.Zone != targetPos.Zone || allyPos.Zone != targetPos.Zone)
            return false;

        // Check if attacker and ally are on opposite sides
        bool oppositeColumns = (attackerPos.Column < targetPos.Column && allyPos.Column > targetPos.Column) ||
                              (attackerPos.Column > targetPos.Column && allyPos.Column < targetPos.Column);

        bool oppositeRows = (attackerPos.Row != targetPos.Row && allyPos.Row != targetPos.Row &&
                            attackerPos.Row != allyPos.Row);

        return oppositeColumns || oppositeRows;
    }

    private bool HasCover(GridPosition position, BattlefieldGrid grid)
    {
        var tile = grid.GetTile(position);
        return tile?.Cover != null && tile.Cover != CoverType.None;
    }

    private bool IsHazard(GridPosition position, BattlefieldGrid grid)
    {
        var tile = grid.GetTile(position);
        return tile?.DynamicHazards?.Any() == true || tile?.StaticTerrain?.IsHazardous == true;
    }

    private List<GridPosition> GetValidMovementPositions(GridPosition currentPos, int movementRange)
    {
        // Generate valid positions within movement range
        // Simplified: positions in same zone, within column/row range
        var positions = new List<GridPosition>();

        for (int col = currentPos.Column - movementRange; col <= currentPos.Column + movementRange; col++)
        {
            if (col >= 0) // Assuming columns start at 0
            {
                // Same row
                positions.Add(new GridPosition(currentPos.Zone, currentPos.Row, col));

                // Different row (if within range)
                var otherRow = currentPos.Row == Row.Front ? Row.Back : Row.Front;
                if (Math.Abs(col - currentPos.Column) < movementRange)
                {
                    positions.Add(new GridPosition(currentPos.Zone, otherRow, col));
                }
            }
        }

        return positions.Where(p => p != currentPos).ToList();
    }
}
