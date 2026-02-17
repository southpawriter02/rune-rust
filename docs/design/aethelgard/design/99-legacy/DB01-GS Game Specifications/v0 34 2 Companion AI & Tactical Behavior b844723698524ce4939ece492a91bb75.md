# v0.34.2: Companion AI & Tactical Behavior

Type: Technical
Description: CompanionAIService implementation, stance-based behavior (Aggressive/Defensive/Passive), tactical grid integration, target selection algorithms, ability usage logic. 8-12 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.34.1 (Database), v0.20 (Tactical Grid), v0.21 (Stance System)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.34: NPC Companion System (v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.34.2-COMPANION-AI

**Status:** Implementation Complete — Ready for Integration

**Timeline:** 8-12 hours

**Prerequisites:** v0.34.1 (Database), v0.20 (Tactical Grid), v0.21 (Stance System)

**Parent Specification:** v0.34 NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

## I. Executive Summary

### The Deliverable

This specification defines the **Companion AI engine** that controls companion behavior in tactical combat:

- **CompanionAIService** — Decision-making system for companion actions
- **Stance-based behavior** — Aggressive/Defensive/Passive AI modes
- **Tactical grid integration** — Positioning, flanking, cover usage
- **Target selection algorithms** — Smart enemy prioritization
- **Ability usage logic** — When to use which ability

Companion AI must make **intelligent tactical decisions** while respecting player-set stances and feeling distinct from player control.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.34.2)

**CompanionAIService Implementation:**

- SelectAction() — Choose action based on stance and battlefield state
- SelectTarget() — Pick optimal enemy target
- EvaluateThreat() — Assess danger to allies
- FindOptimalPosition() — Movement for tactical advantage
- ShouldUseAbility() — Ability vs. basic attack decision

**Stance Behavior:**

- **Aggressive:** Maximize damage, prioritize wounded enemies
- **Defensive:** Protect allies, stay near player, conservative positioning
- **Passive:** No autonomous action (manual control only)

**Tactical Grid Integration:**

- Use existing TacticalGridService for positioning
- Respect flanking bonuses
- Use cover when available (Defensive stance)
- Avoid hazards

**Testing:**

- Unit tests for target selection
- Stance behavior validation
- Movement algorithm tests

### ❌ Explicitly Out of Scope

- Command verb parsing (defer to v0.34.4)
- Companion progression (defer to v0.34.3)
- Recruitment logic (defer to v0.34.3)
- Advanced AI behaviors (formations, combo abilities) (defer to v2.0+)
- Machine learning AI (simple heuristic-based)
- Companion dialogue/barks during combat (defer to polish)

---

## III. Service Architecture

### CompanionAIService.cs

**Location:** `RuneAndRust.Engine/Services/CompanionAIService.cs`

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System;
using System.Collections.Generic;
using System.Linq;

namespace [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services)
{
    public class CompanionAIService
    {
        private readonly TacticalGridService _tacticalGridService;
        private readonly ILogger _logger;

        public CompanionAIService(
            TacticalGridService tacticalGridService,
            ILogger logger)
        {
            _tacticalGridService = tacticalGridService;
            _logger = logger.ForContext<CompanionAIService>();
        }

        /// <summary>
        /// Select companion action based on stance and battlefield state
        /// </summary>
        public CompanionAction SelectAction(
            Companion companion,
            Character player,
            List<Enemy> enemies,
            CombatState combatState)
        {
            _logger.Information(
                "SelectAction for {CompanionName} with stance {Stance}",
                companion.CompanionName,
                companion.CurrentStance);

            // Passive stance: No autonomous action
            if (companion.CurrentStance == "Passive")
            {
                _logger.Debug("Companion in Passive stance, no action");
                return new CompanionAction
                {
                    ActionType = "Wait",
                    Reason = "Passive stance"
                };
            }

            // Check if companion needs to move to safety (low HP)
            if (ShouldRetreat(companion, enemies))
            {
                var safePosition = FindSafePosition(companion, player, enemies);
                return new CompanionAction
                {
                    ActionType = "Move",
                    TargetPosition = safePosition,
                    Reason = "Retreat to safety (low HP)"
                };
            }

            // Defensive stance: Protect allies first
            if (companion.CurrentStance == "Defensive")
            {
                var threatenedAlly = FindThreatenedAlly(player, enemies, combatState);
                if (threatenedAlly != null)
                {
                    var protectAction = PlanProtectAction(
                        companion, threatenedAlly, enemies);
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
                _logger.Warning("No valid target found");
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
        public Enemy SelectTarget(
            Companion companion,
            List<Enemy> enemies,
            Character player)
        {
            var validTargets = enemies
                .Where(e => e.CurrentHitPoints > 0)
                .ToList();

            if (!validTargets.Any())
            {
                return null;
            }

            _logger.Debug(
                "SelectTarget for {CompanionName}, {Count} valid enemies",
                companion.CompanionName,
                validTargets.Count);

            switch (companion.CurrentStance)
            {
                case "Aggressive":
                    return SelectAggressiveTarget(companion, validTargets);

                case "Defensive":
                    return SelectDefensiveTarget(companion, validTargets, player);

                default:
                    return validTargets.First(); // Fallback
            }
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
                .Where(e => e.CurrentHitPoints < e.MaxHitPoints * 0.5)
                .OrderBy(e => e.CurrentHitPoints)
                .ToList();

            if (wounded.Any())
            {
                _logger.Debug("Aggressive: Targeting wounded enemy {EnemyName}",
                    wounded.First().Name);
                return wounded.First();
            }

            // Prioritize high-threat enemies
            var highThreat = enemies
                .OrderByDescending(e => EvaluateThreat(e))
                .First();

            _logger.Debug("Aggressive: Targeting high-threat enemy {EnemyName}",
                [highThreat.Name](http://highThreat.Name));
            return highThreat;
        }

        private Enemy SelectDefensiveTarget(
            Companion companion,
            List<Enemy> enemies,
            Character player)
        {
            // Defensive priority:
            // 1. Enemies threatening player
            // 2. Enemies closest to player
            // 3. Weakest enemies (reliable takedowns)

            var playerPosition = player.CurrentPosition;
            var threateningPlayer = enemies
                .Where(e => IsAdjacentTo(e.CurrentPosition, playerPosition))
                .OrderByDescending(e => EvaluateThreat(e))
                .ToList();

            if (threateningPlayer.Any())
            {
                _logger.Debug("Defensive: Targeting enemy threatening player {EnemyName}",
                    threateningPlayer.First().Name);
                return threateningPlayer.First();
            }

            // Target closest to player
            var closestToPlayer = enemies
                .OrderBy(e => DistanceTo(e.CurrentPosition, playerPosition))
                .First();

            _logger.Debug("Defensive: Targeting enemy closest to player {EnemyName}",
                [closestToPlayer.Name](http://closestToPlayer.Name));
            return closestToPlayer;
        }

        /// <summary>
        /// Evaluate threat level of an enemy
        /// </summary>
        public int EvaluateThreat(Enemy enemy)
        {
            int threat = 0;

            // Base threat from damage output
            threat += enemy.BaseDamage * 10;

            // Additional threat from special abilities
            if (enemy.HasAbility("Stun") || enemy.HasAbility("Disable"))
            {
                threat += 20;
            }

            // Ranged enemies more threatening (can attack from safety)
            if (enemy.AttackRange > 1)
            {
                threat += 15;
            }

            // Low HP enemies less threatening
            if (enemy.CurrentHitPoints < enemy.MaxHitPoints * 0.3)
            {
                threat = (int)(threat * 0.5);
            }

            _logger.Debug("Threat evaluation for {EnemyName}: {Threat}",
                [enemy.Name](http://enemy.Name), threat);

            return threat;
        }

        /// <summary>
        /// Find optimal position for tactical advantage
        /// </summary>
        public Position FindOptimalPosition(
            Companion companion,
            Character player,
            List<Enemy> enemies,
            Enemy target)
        {
            var validPositions = _tacticalGridService
                .GetValidMovementPositions(companion.CurrentPosition, companion.MovementRange);

            if (!validPositions.Any())
            {
                _logger.Warning("No valid movement positions found");
                return companion.CurrentPosition;
            }

            // Score each position based on tactical factors
            var scoredPositions = validPositions
                .Select(pos => new
                {
                    Position = pos,
                    Score = ScorePosition(pos, companion, player, enemies, target)
                })
                .OrderByDescending(p => p.Score)
                .ToList();

            var bestPosition = scoredPositions.First().Position;

            _logger.Information(
                "Optimal position for {CompanionName}: {X},{Y} (score {Score})",
                companion.CompanionName,
                bestPosition.X,
                bestPosition.Y,
                scoredPositions.First().Score);

            return bestPosition;
        }

        private int ScorePosition(
            Position position,
            Companion companion,
            Character player,
            List<Enemy> enemies,
            Enemy target)
        {
            int score = 0;

            // Flanking bonus
            if (HasFlankingOn(position, target.CurrentPosition, player.CurrentPosition))
            {
                score += 30;
            }

            // Cover bonus (Defensive stance)
            if (companion.CurrentStance == "Defensive")
            {
                if (_tacticalGridService.HasCover(position))
                {
                    score += 20;
                }

                // Stay near player
                var distanceToPlayer = DistanceTo(position, player.CurrentPosition);
                score += (10 - distanceToPlayer) * 5; // Closer = better
            }

            // Aggressive stance: Get closer to enemies
            if (companion.CurrentStance == "Aggressive")
            {
                var distanceToTarget = DistanceTo(position, target.CurrentPosition);
                score += (10 - distanceToTarget) * 3; // Closer = better
            }

            // Avoid hazards
            if (_tacticalGridService.IsHazard(position))
            {
                score -= 50;
            }

            // Avoid clustering with other allies
            if (IsAdjacentTo(position, player.CurrentPosition))
            {
                score -= 10; // Penalty for blocking player
            }

            return score;
        }

        /// <summary>
        /// Decide if companion should use ability vs. basic attack
        /// </summary>
        public CompanionAction ShouldUseAbility(
            Companion companion,
            Enemy target,
            List<Enemy> enemies)
        {
            var availableAbilities = companion.Abilities
                .Where(a => CanUseAbility(companion, a))
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
                    var clusteredEnemies = enemies
                        .Where(e => DistanceTo(e.CurrentPosition, target.CurrentPosition) <= ability.AOERadius)
                        .Count();

                    if (clusteredEnemies >= 2)
                    {
                        _logger.Information(
                            "Using AOE ability {AbilityName} on {Count} enemies",
                            [ability.Name](http://ability.Name),
                            clusteredEnemies);

                        return new CompanionAction
                        {
                            ActionType = "UseAbility",
                            AbilityName = [ability.Name](http://ability.Name),
                            TargetEnemy = target,
                            Reason = $"AOE ability hits {clusteredEnemies} enemies"
                        };
                    }
                }

                // Healing abilities: Use if ally below 50% HP
                if (ability.AbilityType == "Heal")
                {
                    // Check companion HP or player HP
                    if (companion.CurrentHitPoints < companion.MaxHitPoints * 0.5)
                    {
                        _logger.Information(
                            "Using heal ability {AbilityName} on self",
                            [ability.Name](http://ability.Name));

                        return new CompanionAction
                        {
                            ActionType = "UseAbility",
                            AbilityName = [ability.Name](http://ability.Name),
                            TargetSelf = true,
                            Reason = "Self-heal (below 50% HP)"
                        };
                    }
                }

                // Buff abilities: Use at combat start
                if (ability.AbilityType == "Buff" && !companion.HasActiveBuff([ability.Name](http://ability.Name)))
                {
                    _logger.Information(
                        "Using buff ability {AbilityName}",
                        [ability.Name](http://ability.Name));

                    return new CompanionAction
                    {
                        ActionType = "UseAbility",
                        AbilityName = [ability.Name](http://ability.Name),
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
                        _logger.Information(
                            "Using high-damage ability {AbilityName} on high-threat target",
                            [ability.Name](http://ability.Name));

                        return new CompanionAction
                        {
                            ActionType = "UseAbility",
                            AbilityName = [ability.Name](http://ability.Name),
                            TargetEnemy = target,
                            Reason = "High-damage ability on high-threat target"
                        };
                    }
                }
            }

            // No compelling ability use case
            return null;
        }

        private bool CanUseAbility(Companion companion, CompanionAbility ability)
        {
            // Check resource cost
            if (ability.StaminaCost > 0 && companion.CurrentStamina < ability.StaminaCost)
            {
                return false;
            }

            if (ability.AetherCost > 0 && companion.CurrentAether < ability.AetherCost)
            {
                return false;
            }

            // Check cooldown
            if (companion.AbilityOnCooldown([ability.Name](http://ability.Name)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if companion should retreat to safety
        /// </summary>
        private bool ShouldRetreat(Companion companion, List<Enemy> enemies)
        {
            // Retreat if below 30% HP and enemies adjacent
            if (companion.CurrentHitPoints < companion.MaxHitPoints * 0.3)
            {
                var adjacentEnemies = enemies
                    .Where(e => IsAdjacentTo(e.CurrentPosition, companion.CurrentPosition))
                    .Count();

                return adjacentEnemies > 0;
            }

            return false;
        }

        private Position FindSafePosition(
            Companion companion,
            Character player,
            List<Enemy> enemies)
        {
            var validPositions = _tacticalGridService
                .GetValidMovementPositions(companion.CurrentPosition, companion.MovementRange);

            // Find position with no adjacent enemies and closest to player
            var safePositions = validPositions
                .Where(pos => !enemies.Any(e => IsAdjacentTo(e.CurrentPosition, pos)))
                .OrderBy(pos => DistanceTo(pos, player.CurrentPosition))
                .ToList();

            return safePositions.FirstOrDefault() ?? companion.CurrentPosition;
        }

        private Character FindThreatenedAlly(
            Character player,
            List<Enemy> enemies,
            CombatState combatState)
        {
            // Check if player is threatened (low HP + adjacent enemies)
            if (player.CurrentHitPoints < player.MaxHitPoints * 0.4)
            {
                var adjacentEnemies = enemies
                    .Where(e => IsAdjacentTo(e.CurrentPosition, player.CurrentPosition))
                    .Count();

                if (adjacentEnemies > 0)
                {
                    return player;
                }
            }

            // TODO: Check other companions in party
            return null;
        }

        private CompanionAction PlanProtectAction(
            Companion companion,
            Character threatenedAlly,
            List<Enemy> enemies)
        {
            // Find enemy threatening ally
            var threat = enemies
                .Where(e => IsAdjacentTo(e.CurrentPosition, threatenedAlly.CurrentPosition))
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
                Reason = $"Protect {[threatenedAlly.Name](http://threatenedAlly.Name)} from {[threat.Name](http://threat.Name)}"
            };
        }

        // Helper methods
        private bool IsAdjacentTo(Position pos1, Position pos2)
        {
            return Math.Abs(pos1.X - pos2.X) <= 1 &&
                   Math.Abs(pos1.Y - pos2.Y) <= 1;
        }

        private int DistanceTo(Position pos1, Position pos2)
        {
            return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
        }

        private bool HasFlankingOn(Position attackerPos, Position targetPos, Position allyPos)
        {
            // Simplified flanking: Attacker and ally on opposite sides of target
            var attackerDirection = new Position
            {
                X = targetPos.X - attackerPos.X,
                Y = targetPos.Y - attackerPos.Y
            };

            var allyDirection = new Position
            {
                X = targetPos.X - allyPos.X,
                Y = targetPos.Y - allyPos.Y
            };

            // Opposite directions indicate flanking
            return (attackerDirection.X * allyDirection.X < 0) ||
                   (attackerDirection.Y * allyDirection.Y < 0);
        }
    }

    // Supporting models
    public class CompanionAction
    {
        public string ActionType { get; set; } // "Attack", "UseAbility", "Move", "Wait"
        public Enemy TargetEnemy { get; set; }
        public Position TargetPosition { get; set; }
        public string AbilityName { get; set; }
        public bool TargetSelf { get; set; }
        public string Reason { get; set; }
    }
}
```

---

## IV. Stance Behavior Definitions

### Aggressive Stance

**Philosophy:** Maximize damage output, finish wounded enemies

**Behavior:**

- **Target Selection:** Wounded enemies first, then high-threat targets
- **Positioning:** Move toward enemies, prioritize attack range
- **Ability Usage:** Use high-damage abilities liberally, AOE when available
- **Risk Tolerance:** Willing to take damage to secure kills

**Code Path:**

```csharp
if (companion.CurrentStance == "Aggressive")
{
    // SelectAggressiveTarget()
    // - Prioritize wounded enemies
    // - Then high-damage threats
    
    // ScorePosition()
    // - Bonus for proximity to target
    // - Less weight on cover/safety
}
```

### Defensive Stance

**Philosophy:** Protect allies, conservative positioning, survivability

**Behavior:**

- **Target Selection:** Enemies threatening player/allies first
- **Positioning:** Stay near player, use cover, avoid overextension
- **Ability Usage:** Use defensive/support abilities, healing when needed
- **Risk Tolerance:** Retreat if low HP, avoid unnecessary risks

**Code Path:**

```csharp
if (companion.CurrentStance == "Defensive")
{
    // Check for threatened allies first
    var threatenedAlly = FindThreatenedAlly(...);
    if (threatenedAlly != null)
    {
        return PlanProtectAction(...);
    }
    
    // SelectDefensiveTarget()
    // - Enemies threatening player
    // - Enemies closest to player
    
    // ScorePosition()
    // - High bonus for cover
    // - Stay near player
}
```

### Passive Stance

**Philosophy:** No autonomous action, manual control only

**Behavior:**

- **No AI decisions:** Companion does nothing unless commanded
- **Use Case:** Keep companion safe, precise tactical control
- **Command Required:** Player must use `command` verb for action

**Code Path:**

```csharp
if (companion.CurrentStance == "Passive")
{
    return new CompanionAction
    {
        ActionType = "Wait",
        Reason = "Passive stance"
    };
}
```

---

## V. Tactical Grid Integration

### TacticalGridService Integration

**Existing Service (v0.20):**

```csharp
// CompanionAIService uses TacticalGridService for:
var validPositions = _tacticalGridService
    .GetValidMovementPositions(currentPos, movementRange);

bool hasCover = _tacticalGridService.HasCover(position);

bool isHazard = _tacticalGridService.IsHazard(position);
```

### Flanking Detection

**Flanking Bonus:** +2 to attack rolls when ally and attacker on opposite sides

```csharp
private bool HasFlankingOn(Position attackerPos, Position targetPos, Position allyPos)
{
    // Attacker and ally on opposite sides of target
    var attackerDirection = new Position
    {
        X = targetPos.X - attackerPos.X,
        Y = targetPos.Y - attackerPos.Y
    };

    var allyDirection = new Position
    {
        X = targetPos.X - allyPos.X,
        Y = targetPos.Y - allyPos.Y
    };

    // Opposite directions = flanking
    return (attackerDirection.X * allyDirection.X < 0) ||
           (attackerDirection.Y * allyDirection.Y < 0);
}
```

### Cover Usage

**Cover Bonus:** +2 Defense when in cover

**Defensive Stance Behavior:**

- Prioritize positions with cover
- +20 score bonus for cover positions
- Avoid leaving cover unless necessary

```csharp
// In ScorePosition()
if (companion.CurrentStance == "Defensive")
{
    if (_tacticalGridService.HasCover(position))
    {
        score += 20; // Significant bonus
    }
}
```

### Hazard Avoidance

**All Stances:** Avoid hazards unless no other option

```csharp
// In ScorePosition()
if (_tacticalGridService.IsHazard(position))
{
    score -= 50; // Heavy penalty
}
```

---

## VI. Testing Strategy

### Unit Tests

**File:** `RuneAndRust.Tests/Services/CompanionAIServiceTests.cs`

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using RuneAndRust.Core.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace [RuneAndRust.Tests.Services](http://RuneAndRust.Tests.Services)
{
    [TestClass]
    public class CompanionAIServiceTests
    {
        private CompanionAIService _aiService;
        private TacticalGridService _gridService;
        private ILogger _logger;

        [TestInitialize]
        public void Setup()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            _gridService = new TacticalGridService(_logger);
            _aiService = new CompanionAIService(_gridService, _logger);
        }

        [TestMethod]
        public void SelectTarget_Aggressive_PrioritizesWounded()
        {
            // Arrange
            var companion = CreateCompanion("Kara", "Aggressive");
            var player = CreatePlayer();

            var enemies = new List<Enemy>
            {
                CreateEnemy("Warden A", 50, 100), // Wounded
                CreateEnemy("Warden B", 100, 100) // Full HP
            };

            // Act
            var target = _aiService.SelectTarget(companion, enemies, player);

            // Assert
            Assert.AreEqual("Warden A", [target.Name](http://target.Name));
            Assert.AreEqual(50, target.CurrentHitPoints);
        }

        [TestMethod]
        public void SelectTarget_Defensive_PrioritizesThreatsToPlayer()
        {
            // Arrange
            var companion = CreateCompanion("Runa", "Defensive");
            var player = CreatePlayer();
            player.CurrentPosition = new Position { X = 5, Y = 5 };

            var enemies = new List<Enemy>
            {
                CreateEnemy("Cultist", 80, 100, new Position { X = 8, Y = 8 }), // Far from player
                CreateEnemy("Draugr", 100, 100, new Position { X = 5, Y = 6 }) // Adjacent to player
            };

            // Act
            var target = _aiService.SelectTarget(companion, enemies, player);

            // Assert
            Assert.AreEqual("Draugr", [target.Name](http://target.Name));
        }

        [TestMethod]
        public void SelectAction_PassiveStance_ReturnsWait()
        {
            // Arrange
            var companion = CreateCompanion("Valdis", "Passive");
            var player = CreatePlayer();
            var enemies = new List<Enemy> { CreateEnemy("Warden", 100, 100) };
            var combatState = new CombatState();

            // Act
            var action = _aiService.SelectAction(companion, player, enemies, combatState);

            // Assert
            Assert.AreEqual("Wait", action.ActionType);
            Assert.AreEqual("Passive stance", action.Reason);
        }

        [TestMethod]
        public void EvaluateThreat_StunAbility_IncreasedThreat()
        {
            // Arrange
            var enemy = CreateEnemy("Warden", 100, 100);
            enemy.Abilities.Add(new EnemyAbility { Name = "Stun" });

            // Act
            var threat = _aiService.EvaluateThreat(enemy);

            // Assert
            Assert.IsTrue(threat > 20); // Base + Stun bonus
        }

        [TestMethod]
        public void ShouldUseAbility_AOE_MultipleClustered_UsesAOE()
        {
            // Arrange
            var companion = CreateCompanion("Bjorn", "Aggressive");
            companion.CurrentStamina = 20;
            companion.Abilities.Add(new CompanionAbility
            {
                Name = "Scrap Grenade",
                IsAOE = true,
                AOERadius = 2,
                StaminaCost = 10
            });

            var target = CreateEnemy("Cultist A", 80, 100, new Position { X = 5, Y = 5 });
            var enemies = new List<Enemy>
            {
                target,
                CreateEnemy("Cultist B", 80, 100, new Position { X = 5, Y = 6 }), // Adjacent
                CreateEnemy("Cultist C", 80, 100, new Position { X = 6, Y = 5 })  // Adjacent
            };

            // Act
            var action = _aiService.ShouldUseAbility(companion, target, enemies);

            // Assert
            Assert.IsNotNull(action);
            Assert.AreEqual("UseAbility", action.ActionType);
            Assert.AreEqual("Scrap Grenade", action.AbilityName);
        }

        [TestMethod]
        public void ShouldUseAbility_InsufficientStamina_ReturnsNull()
        {
            // Arrange
            var companion = CreateCompanion("Kara", "Aggressive");
            companion.CurrentStamina = 5; // Not enough
            companion.Abilities.Add(new CompanionAbility
            {
                Name = "Shield Bash",
                StaminaCost = 10
            });

            var target = CreateEnemy("Warden", 100, 100);
            var enemies = new List<Enemy> { target };

            // Act
            var action = _aiService.ShouldUseAbility(companion, target, enemies);

            // Assert
            Assert.IsNull(action); // Should use basic attack instead
        }

        [TestMethod]
        public void FindOptimalPosition_DefensiveStance_PrefersCover()
        {
            // Arrange
            var companion = CreateCompanion("Runa", "Defensive");
            companion.CurrentPosition = new Position { X = 3, Y = 3 };
            companion.MovementRange = 3;

            var player = CreatePlayer();
            player.CurrentPosition = new Position { X = 4, Y = 4 };

            var target = CreateEnemy("Draugr", 100, 100, new Position { X = 6, Y = 6 });
            var enemies = new List<Enemy> { target };

            // Mock cover at (4, 3)
            // (This requires TacticalGridService mock setup)

            // Act
            var position = _aiService.FindOptimalPosition(
                companion, player, enemies, target);

            // Assert
            Assert.IsNotNull(position);
            // Verify position is near player and has cover
        }

        [TestMethod]
        public void SelectAction_LowHP_RetreatsToSafety()
        {
            // Arrange
            var companion = CreateCompanion("Valdis", "Defensive");
            companion.CurrentHitPoints = 15; // 30% of 50 max
            companion.MaxHitPoints = 50;
            companion.CurrentPosition = new Position { X = 5, Y = 5 };

            var player = CreatePlayer();
            player.CurrentPosition = new Position { X = 3, Y = 3 };

            var enemies = new List<Enemy>
            {
                CreateEnemy("Draugr", 100, 100, new Position { X = 5, Y = 6 }) // Adjacent
            };

            var combatState = new CombatState();

            // Act
            var action = _aiService.SelectAction(companion, player, enemies, combatState);

            // Assert
            Assert.AreEqual("Move", action.ActionType);
            Assert.IsTrue(action.Reason.Contains("Retreat"));
        }

        // Helper methods
        private Companion CreateCompanion(string name, string stance)
        {
            return new Companion
            {
                CompanionName = name,
                CurrentStance = stance,
                CurrentHitPoints = 50,
                MaxHitPoints = 50,
                CurrentStamina = 20,
                MaxStamina = 20,
                CurrentAether = 10,
                MaxAether = 10,
                MovementRange = 3,
                Abilities = new List<CompanionAbility>(),
                CurrentPosition = new Position { X = 3, Y = 3 }
            };
        }

        private Character CreatePlayer()
        {
            return new Character
            {
                Name = "Player",
                CurrentHitPoints = 80,
                MaxHitPoints = 100,
                CurrentPosition = new Position { X = 5, Y = 5 }
            };
        }

        private Enemy CreateEnemy(string name, int hp, int maxHp,
            Position position = null)
        {
            return new Enemy
            {
                Name = name,
                CurrentHitPoints = hp,
                MaxHitPoints = maxHp,
                BaseDamage = 10,
                AttackRange = 1,
                CurrentPosition = position ?? new Position { X = 7, Y = 7 },
                Abilities = new List<EnemyAbility>()
            };
        }
    }
}
```

### Test Coverage Target

**85%+ Coverage:**

- ✅ Target selection (Aggressive/Defensive)
- ✅ Stance behavior (Passive/Aggressive/Defensive)
- ✅ Threat evaluation
- ✅ Ability usage decisions
- ✅ Position scoring
- ✅ Retreat logic
- ✅ Resource checking

---

## VII. Integration with Existing Systems

### TacticalGridService (v0.20)

```csharp
// Existing methods used by CompanionAIService:
_tacticalGridService.GetValidMovementPositions(position, range);
_tacticalGridService.HasCover(position);
_tacticalGridService.IsHazard(position);
```

**No changes required to TacticalGridService**

### Stance System (v0.21)

```csharp
// Companions use existing stance mechanics:
companion.CurrentStance = "Aggressive"; // Or "Defensive", "Passive"

// Stance affects:
// - Target selection algorithm
// - Position scoring priorities
// - Risk tolerance
```

### Combat System (v0.10)

```csharp
// CompanionService (v0.34.4) calls CompanionAIService during combat:
public void ProcessCompanionTurn(Companion companion, CombatState state)
{
    var action = _companionAIService.SelectAction(
        companion, state.Player, state.Enemies, state);
    
    // Execute action...
}
```

---

## VIII. Deployment Instructions

### Step 1: Create Service File

1. Create `RuneAndRust.Engine/Services/CompanionAIService.cs`
2. Copy service implementation from Section III

### Step 2: Register in DI

```csharp
// In Startup.cs or ServiceConfiguration.cs
services.AddSingleton<CompanionAIService>();
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter CompanionAIServiceTests
```

**Expected Results:**

- 8 tests pass
- 85%+ code coverage
- No warnings or errors

### Step 4: Integration Testing

Manual test scenarios:

**Scenario 1: Aggressive Stance**

```
Setup: Companion in Aggressive, multiple enemies (1 wounded)
Expected: Targets wounded enemy, uses high-damage ability if available
```

**Scenario 2: Defensive Stance**

```
Setup: Companion in Defensive, enemy adjacent to player
Expected: Targets enemy threatening player, stays near player
```

**Scenario 3: Passive Stance**

```
Setup: Companion in Passive, enemies present
Expected: Takes no action (Wait)
```

**Scenario 4: Low HP Retreat**

```
Setup: Companion at 25% HP, enemy adjacent
Expected: Moves away from enemies toward player
```

---

## IX. Success Criteria

### Functional Requirements

- [ ]  Aggressive stance prioritizes wounded enemies
- [ ]  Defensive stance protects player/allies
- [ ]  Passive stance takes no action
- [ ]  Threat evaluation accounts for abilities, damage, HP
- [ ]  Optimal positioning considers flanking, cover, hazards
- [ ]  Ability usage decisions based on situation (AOE, heal, buff, high-damage)
- [ ]  Retreat logic when companion low HP
- [ ]  Resource checking (Stamina/Aether) before ability use

### Quality Gates

- [ ]  85%+ unit test coverage
- [ ]  All 8+ unit tests pass
- [ ]  Serilog logging on all decisions
- [ ]  No hardcoded magic numbers (constants defined)
- [ ]  Tactical grid integration functional

### Balance Validation

- [ ]  AI makes reasonable decisions 90%+ of time
- [ ]  No obvious exploits (AI doesn't do stupid things)
- [ ]  Stances feel distinct in play
- [ ]  Companions feel helpful, not burdensome
- [ ]  Manual command override always works (Passive stance)

---

## X. Design Notes

### AI Philosophy

**Heuristic-Based, Not ML:**

- Simple scoring algorithms
- Transparent decision-making (logged reasons)
- Predictable behavior
- Easily tunable parameters

**Goal:** Companions feel helpful without being optimal. Players should still make better decisions.

### Stance Design Intent

**Aggressive:**

- For players who want maximum damage
- Risk: Companion may overextend and die
- Reward: Faster combat resolution

**Defensive:**

- For players who value survivability
- Risk: Slower combat, less damage
- Reward: Companion survives more reliably

**Passive:**

- For players who want precise control
- Risk: Requires more player attention
- Reward: Perfect tactical execution

### Performance Considerations

**Computational Cost:**

- `SelectAction()` called once per companion per turn
- Position scoring: ~5-10 positions evaluated per turn
- Target selection: ~3-5 enemies evaluated per turn

**Optimization:**

- Cache tactical grid data when possible
- Limit position search radius
- Early-exit on clear decisions

---

## XI. Known Limitations & Future Work

### Current Limitations

**No Formation AI:**

- Companions don't coordinate positioning with each other
- Defer to v2.0+

**No Combo Abilities:**

- Companions don't combo abilities with player or each other
- Defer to v2.0+

**Simple Threat Model:**

- Threat evaluation is heuristic-based
- Doesn't learn from player behavior

**No Dialogue:**

- Companions don't comment on actions
- Defer to narrative polish pass

### Future Enhancements (v2.0+)

**Advanced AI:**

- Formation tactics (shield wall, flanking maneuvers)
- Combo ability recognition
- Learning player preferences
- Adaptive difficulty (AI gets smarter as player levels)

**Personality-Driven AI:**

- Kara plays more aggressively
- Finnr plays more cautiously
- Valdis acts unpredictably (Forlorn influence)

**Strategic Layer:**

- Pre-combat planning
- Companion suggestions ("I should use AOE here")
- Risk assessment warnings

---

## XII. Related Documents

**Prerequisites:**

- v0.34.1: Database Schema & Companion Definitions[[2]](v0%2034%201%20Database%20Schema%20&%20Companion%20Definitions%200d9bf4c187e94d2dbebf7d73e81ded97.md)
- v0.20: Tactical Combat Grid System[[3]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.21: Stance System[[4]](316)

**Next Steps:**

- v0.34.3: Recruitment & Progression Systems[[5]](v0%2034%203%20Recruitment%20&%20Progression%20Systems%20c234daa5f1074be8b7323d6137cf70b3.md)
- v0.34.4: Service Implementation & Testing[[6]](v0%2034%204%20Service%20Implementation%20&%20Testing%2090fcbf84cb89413bbf6b49fc459e4cac.md)

**Parent Specification:**

- v0.34: NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

**Implementation-ready AI service with stance-based behavior, tactical grid integration, and comprehensive testing strategy complete.**