# v0.42.1: Tactical Decision-Making & Target Selection

Type: Mechanic
Description: Core AI decision-making framework enabling intelligent threat assessment and target selection. Delivers TacticalDecisionService architecture, threat assessment algorithms, target selection logic per AI archetype, situational analysis system, and database schema for AI configuration weights.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1 (Combat System), v0.20-v0.20.5 (Tactical Grid)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.42: Enemy AI Improvements & Behavior Polish (v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Spec:** [v0.42: Enemy AI Improvements & Behavior Polish](v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)

**Status:** Design Phase

**Timeline:** 5-8 hours (1 week part-time)

**Focus:** Core AI decision-making framework that enables intelligent threat assessment and target selection

---

## I. Overview

v0.42.1 implements the **foundational decision-making infrastructure** that enables enemies to intelligently evaluate battlefield conditions, assess threats, and select optimal targets.

**What This Delivers:**

- TacticalDecisionService architecture
- Threat assessment algorithms (damage × priority × vulnerability)
- Target selection logic per AI archetype
- Situational analysis system
- Database schema for AI configuration weights

**Success Metric:**

Enemies consistently choose logical targets based on threat level, not random selection. AI decisions are explainable and debuggable through structured logging.

---

## II. Functional Requirements

### FR1: Threat Assessment System

**Requirement:**

Implement a threat scoring algorithm that evaluates each player character's danger level based on multiple factors.

**Threat Score Formula:**

```
ThreatScore = (DamageOutput × 0.4) + (CurrentHP × -0.2) + (Positioning × 0.2) + (Abilities × 0.15) + (StatusEffects × 0.05)

Where:
- DamageOutput: Average damage per turn (0-100)
- CurrentHP: Inverse of current HP % (high HP = low threat component)
- Positioning: Tactical advantage score (0-20)
- Abilities: Threat from available abilities (0-30)
- StatusEffects: Buff/debuff modifier (-10 to +10)
```

**Example C# Implementation:**

```csharp
public class ThreatAssessmentService : IThreatAssessmentService
{
    private readonly ILogger<ThreatAssessmentService> _logger;
    private readonly IAIConfigurationRepository _configRepo;

    public async Task<ThreatAssessment> AssessThreatAsync(
        Enemy assessor, 
        Character target, 
        BattlefieldState state)
    {
        var weights = await _configRepo.GetThreatWeightsAsync(assessor.AIArchetype);
        
        var factors = new Dictionary<ThreatFactor, float>();
        
        // Factor 1: Damage Output (highest weight)
        var damageHistory = await GetRecentDamageHistoryAsync(target, turns: 3);
        var avgDamage = damageHistory.Average();
        factors[ThreatFactor.DamageOutput] = avgDamage * weights.DamageWeight;
        
        // Factor 2: Current HP (inverse - low HP = easier kill = higher threat)
        var hpPercent = (float)target.CurrentHP / target.MaxHP;
        var hpThreat = (1.0f - hpPercent) * 20f;  // Low HP enemies are priority targets
        factors[ThreatFactor.CurrentHP] = hpThreat * weights.HPWeight;
        
        // Factor 3: Positioning (flanked? elevated? in cover?)
        var positionScore = EvaluatePositionThreat(target, state.Grid);
        factors[ThreatFactor.Positioning] = positionScore * weights.PositionWeight;
        
        // Factor 4: Abilities (does target have dangerous abilities ready?)
        var abilityThreat = await EvaluateAbilityThreatAsync(target);
        factors[ThreatFactor.Abilities] = abilityThreat * weights.AbilityWeight;
        
        // Factor 5: Status Effects (buffed? debuffed?)
        var statusThreat = EvaluateStatusEffectThreat(target);
        factors[ThreatFactor.StatusEffects] = statusThreat * weights.StatusWeight;
        
        var totalScore = factors.Values.Sum();
        
        _logger.Information(
            "Threat assessment: Enemy {EnemyId} evaluates {TargetName} = {TotalScore} " +
            "(Damage={Dmg}, HP={HP}, Position={Pos}, Abilities={Abil}, Status={Status})",
            assessor.EnemyId, [target.Name](http://target.Name), totalScore,
            factors[ThreatFactor.DamageOutput],
            factors[ThreatFactor.CurrentHP],
            factors[ThreatFactor.Positioning],
            factors[ThreatFactor.Abilities],
            factors[ThreatFactor.StatusEffects]);
        
        return new ThreatAssessment
        {
            Target = target,
            TotalThreatScore = totalScore,
            FactorScores = factors,
            Reasoning = GenerateReasoningText(factors)
        };
    }

    private float EvaluatePositionThreat(Character target, TacticalGrid grid)
    {
        float score = 0f;
        
        // Elevated targets are harder to reach
        if (grid.GetElevation(target.GridPosition) > 0)
            score -= 5f;
        
        // Targets in cover are lower priority
        var coverLevel = grid.GetCoverLevel(target.GridPosition);
        score -= (int)coverLevel * 3f;
        
        // Isolated targets are higher priority
        var nearbyAllies = grid.GetAlliesInRange(target.GridPosition, range: 2);
        if (nearbyAllies.Count == 0)
            score += 8f;
        
        return score;
    }

    private async Task<float> EvaluateAbilityThreatAsync(Character target)
    {
        var abilities = target.Abilities.Where(a => !a.IsOnCooldown).ToList();
        
        float threat = 0f;
        
        foreach (var ability in abilities)
        {
            // High damage abilities are threatening
            if (ability.BaseDamage > 30)
                threat += 10f;
            
            // CC abilities are threatening
            if (ability.AppliesStatusEffect && ability.StatusEffect.IsCC)
                threat += 8f;
            
            // AOE abilities are very threatening
            if (ability.IsAOE)
                threat += 12f;
        }
        
        return Math.Min(threat, 30f);  // Cap at 30
    }

    private float EvaluateStatusEffectThreat(Character target)
    {
        float threat = 0f;
        
        foreach (var effect in target.ActiveStatusEffects)
        {
            if (effect.IsBuff)
                threat += 5f;  // Buffed enemies are more dangerous
            else if (effect.IsDebuff)
                threat -= 3f;  // Debuffed enemies are less dangerous
        }
        
        return Math.Clamp(threat, -10f, 10f);
    }

    private string GenerateReasoningText(Dictionary<ThreatFactor, float> factors)
    {
        var primary = factors.OrderByDescending(kvp => Math.Abs(kvp.Value)).First();
        return $"Primary threat factor: {primary.Key} ({primary.Value:F1})";
    }
}
```

**Archetype-Specific Threat Weights:**

| Archetype | Damage Weight | HP Weight | Position Weight | Ability Weight | Status Weight |
| --- | --- | --- | --- | --- | --- |
| Aggressive | 0.5 | 0.1 | 0.1 | 0.2 | 0.1 |
| Defensive | 0.2 | 0.3 | 0.3 | 0.1 | 0.1 |
| Cautious | 0.3 | 0.2 | 0.3 | 0.1 | 0.1 |
| Reckless | 0.4 | 0.0 | 0.0 | 0.4 | 0.2 |
| Tactical | 0.3 | 0.25 | 0.25 | 0.15 | 0.05 |
| Support | 0.2 | 0.4 | 0.2 | 0.1 | 0.1 |
| Control | 0.25 | 0.15 | 0.2 | 0.3 | 0.1 |
| Ambusher | 0.3 | 0.3 | 0.3 | 0.05 | 0.05 |

**Why Different Weights:**

- **Aggressive:** Prioritizes damage dealers over everything
- **Defensive:** Cares about protecting vulnerable allies (low HP)
- **Cautious:** Worried about position (will I get flanked if I attack this target?)
- **Reckless:** Only cares about damage potential, ignores safety
- **Tactical:** Balanced assessment of all factors
- **Support:** Prioritizes healing injured allies (high HP weight)
- **Control:** Focuses on ability threat (who needs to be disabled?)
- **Ambusher:** Looks for isolated, low HP targets

---

### FR2: Target Selection Logic

**Requirement:**

Implement target selection that chooses the optimal target based on threat assessment and AI archetype behavior.

**Target Selection Algorithm:**

```
1. Assess threat for all valid targets
2. Apply archetype-specific modifiers
3. Filter out invalid targets (out of range, untargetable, etc.)
4. Select highest-scoring target
5. Log decision reasoning
```

**C# Implementation:**

```csharp
public class TargetSelectionService : ITargetSelectionService
{
    private readonly IThreatAssessmentService _threatService;
    private readonly IBehaviorPatternService _behaviorService;
    private readonly ILogger<TargetSelectionService> _logger;

    public async Task<Character> SelectTargetAsync(
        Enemy enemy, 
        List<Character> potentialTargets, 
        BattlefieldState state)
    {
        var archetype = await _behaviorService.GetArchetypeAsync(enemy);
        
        // Step 1: Assess threat for all targets
        var assessments = new List<ThreatAssessment>();
        foreach (var target in potentialTargets)
        {
            var assessment = await _threatService.AssessThreatAsync(enemy, target, state);
            assessments.Add(assessment);
        }
        
        // Step 2: Apply archetype modifiers
        var modifiedScores = await ApplyArchetypeModifiersAsync(archetype, assessments, state);
        
        // Step 3: Filter invalid targets
        var validTargets = modifiedScores
            .Where(kvp => IsValidTarget(enemy, kvp.Key, state))
            .ToList();
        
        if (!validTargets.Any())
        {
            _logger.Warning("No valid targets for enemy {EnemyId}", enemy.EnemyId);
            return null;
        }
        
        // Step 4: Select highest-scoring target
        var selected = validTargets.OrderByDescending(kvp => kvp.Value).First();
        
        _logger.Information(
            "Target selected: Enemy {EnemyId} ({Archetype}) targets {TargetName} (score={Score})",
            enemy.EnemyId, archetype, [selected.Key.Name](http://selected.Key.Name), selected.Value);
        
        return selected.Key;
    }

    private async Task<Dictionary<Character, float>> ApplyArchetypeModifiersAsync(
        AIArchetype archetype,
        List<ThreatAssessment> assessments,
        BattlefieldState state)
    {
        var scores = new Dictionary<Character, float>();
        
        foreach (var assessment in assessments)
        {
            float score = assessment.TotalThreatScore;
            
            // Apply archetype-specific logic
            switch (archetype)
            {
                case AIArchetype.Aggressive:
                    // Prioritize high-damage dealers even more
                    if (assessment.FactorScores[ThreatFactor.DamageOutput] > 30)
                        score *= 1.3f;
                    break;
                    
                case AIArchetype.Defensive:
                    // Deprioritize if it means leaving allies undefended
                    var nearbyAllies = state.Grid.GetAlliesInRange(
                        [assessment.Target](http://assessment.Target).GridPosition, range: 3);
                    if (nearbyAllies.Any(a => a.CurrentHP < a.MaxHP * 0.5f))
                        score *= 0.7f;  // Lower priority if allies need help
                    break;
                    
                case AIArchetype.Cautious:
                    // Avoid risky targets
                    if (assessment.FactorScores[ThreatFactor.Positioning] < 0)
                        score *= 0.6f;  // Target is in strong position, avoid
                    break;
                    
                case AIArchetype.Reckless:
                    // Just target highest damage dealer, ignore everything else
                    score = assessment.FactorScores[ThreatFactor.DamageOutput];
                    break;
                    
                case AIArchetype.Tactical:
                    // No modifiers - use pure threat assessment
                    break;
                    
                case [AIArchetype.Support](http://AIArchetype.Support):
                    // Actually targeting allies for heals, invert logic
                    // (This is handled separately in healing logic)
                    break;
                    
                case AIArchetype.Control:
                    // Prioritize targets with dangerous abilities
                    if (assessment.FactorScores[ThreatFactor.Abilities] > 15)
                        score *= 1.4f;
                    break;
                    
                case AIArchetype.Ambusher:
                    // Heavily prioritize isolated, low-HP targets
                    if (assessment.FactorScores[ThreatFactor.CurrentHP] > 10 &&
                        assessment.FactorScores[ThreatFactor.Positioning] > 5)
                        score *= 1.5f;
                    break;
            }
            
            scores[[assessment.Target](http://assessment.Target)] = score;
        }
        
        return scores;
    }

    private bool IsValidTarget(Enemy enemy, Character target, BattlefieldState state)
    {
        // Check basic validity
        if (target.IsDead)
            return false;
        
        if (target.IsUntargetable)
            return false;
        
        // Check range (for ranged enemies)
        if (enemy.IsRanged)
        {
            var distance = state.Grid.GetDistance(enemy.GridPosition, target.GridPosition);
            if (distance > enemy.AttackRange)
                return false;
        }
        
        return true;
    }
}
```

---

### FR3: Situational Analysis System

**Requirement:**

Implement a system that evaluates the current battlefield state and provides context for AI decision-making.

**Situational Factors:**

- **Ally Count:** How many allies are still alive?
- **Enemy Count:** How many enemies are still alive?
- **HP Status:** Is AI at low health? Are allies injured?
- **Turn Number:** Early game vs late game?
- **Tactical Position:** Does AI have advantage or disadvantage?

**C# Implementation:**

```csharp
public class SituationalAnalysisService : ISituationalAnalysisService
{
    public SituationalContext AnalyzeSituation(Enemy actor, BattlefieldState state)
    {
        var context = new SituationalContext();
        
        // Ally/Enemy count analysis
        var livingAllies = state.Enemies.Count(e => !e.IsDead);
        var livingEnemies = state.PlayerCharacters.Count(c => !c.IsDead);
        
        context.AllyCount = livingAllies;
        context.EnemyCount = livingEnemies;
        context.IsOutnumbered = livingEnemies > livingAllies;
        
        // HP status
        context.SelfHPPercent = (float)actor.CurrentHP / actor.MaxHP;
        context.IsLowHP = context.SelfHPPercent < 0.3f;
        context.IsCriticalHP = context.SelfHPPercent < 0.15f;
        
        // Ally HP analysis
        var allyHPPercentages = state.Enemies
            .Where(e => !e.IsDead)
            .Select(e => (float)e.CurrentHP / e.MaxHP)
            .ToList();
        
        context.AverageAllyHP = allyHPPercentages.Any() ? allyHPPercentages.Average() : 0f;
        context.HasCriticalAllies = allyHPPercentages.Any(hp => hp < 0.2f);
        
        // Tactical position
        context.IsFlanked = IsFlanked(actor, state.Grid);
        context.HasHighGround = HasHighGround(actor, state.Grid, state.PlayerCharacters);
        context.IsInCover = state.Grid.GetCoverLevel(actor.GridPosition) != CoverLevel.None;
        
        // Combat phase
        context.TurnNumber = state.CurrentTurn;
        context.IsEarlyGame = context.TurnNumber <= 3;
        context.IsMidGame = context.TurnNumber > 3 && context.TurnNumber <= 8;
        context.IsLateGame = context.TurnNumber > 8;
        
        // Overall assessment
        context.Advantage = CalculateAdvantage(context);
        
        return context;
    }

    private bool IsFlanked(Enemy actor, TacticalGrid grid)
    {
        // Check if enemies are on multiple sides
        var adjacentPositions = grid.GetAdjacentPositions(actor.GridPosition);
        var enemyDirections = adjacentPositions
            .Where(pos => grid.HasEnemyAt(pos))
            .Select(pos => GetDirection(actor.GridPosition, pos))
            .ToList();
        
        // Flanked if enemies on opposite sides
        return enemyDirections.Any(d => 
            enemyDirections.Contains(GetOppositeDirection(d)));
    }

    private bool HasHighGround(Enemy actor, TacticalGrid grid, List<Character> enemies)
    {
        var actorElevation = grid.GetElevation(actor.GridPosition);
        var enemyElevations = enemies
            .Where(e => !e.IsDead)
            .Select(e => grid.GetElevation(e.GridPosition));
        
        return enemyElevations.All(e => actorElevation > e);
    }

    private TacticalAdvantage CalculateAdvantage(SituationalContext context)
    {
        int advantageScore = 0;
        
        // Numerical advantage
        if (!context.IsOutnumbered) advantageScore += 2;
        else advantageScore -= 2;
        
        // HP advantage
        if (context.SelfHPPercent > 0.7f) advantageScore += 1;
        if (context.SelfHPPercent < 0.3f) advantageScore -= 2;
        
        // Positional advantage
        if (context.HasHighGround) advantageScore += 1;
        if (context.IsFlanked) advantageScore -= 2;
        if (context.IsInCover) advantageScore += 1;
        
        if (advantageScore >= 3) return TacticalAdvantage.Strong;
        if (advantageScore >= 1) return TacticalAdvantage.Slight;
        if (advantageScore >= -1) return TacticalAdvantage.Neutral;
        return TacticalAdvantage.Disadvantaged;
    }
}

public class SituationalContext
{
    public int AllyCount { get; set; }
    public int EnemyCount { get; set; }
    public bool IsOutnumbered { get; set; }
    
    public float SelfHPPercent { get; set; }
    public bool IsLowHP { get; set; }
    public bool IsCriticalHP { get; set; }
    
    public float AverageAllyHP { get; set; }
    public bool HasCriticalAllies { get; set; }
    
    public bool IsFlanked { get; set; }
    public bool HasHighGround { get; set; }
    public bool IsInCover { get; set; }
    
    public int TurnNumber { get; set; }
    public bool IsEarlyGame { get; set; }
    public bool IsMidGame { get; set; }
    public bool IsLateGame { get; set; }
    
    public TacticalAdvantage Advantage { get; set; }
}

public enum TacticalAdvantage
{
    Strong,
    Slight,
    Neutral,
    Disadvantaged
}
```

---

## III. Database Schema

```sql
-- AI threat weight configuration per archetype
CREATE TABLE AIThreatWeights (
    ArchetypeId INT PRIMARY KEY,
    ArchetypeName VARCHAR(50) NOT NULL,
    DamageWeight DECIMAL(3,2) NOT NULL,  -- 0.00 to 1.00
    HPWeight DECIMAL(3,2) NOT NULL,
    PositionWeight DECIMAL(3,2) NOT NULL,
    AbilityWeight DECIMAL(3,2) NOT NULL,
    StatusWeight DECIMAL(3,2) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Threat assessment log (for debugging and analysis)
CREATE TABLE AIThreatAssessmentLog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    CombatEncounterId UNIQUEIDENTIFIER NOT NULL,
    AssessorEnemyId UNIQUEIDENTIFIER NOT NULL,
    TargetCharacterId UNIQUEIDENTIFIER NOT NULL,
    TotalThreatScore DECIMAL(10,2) NOT NULL,
    DamageScore DECIMAL(10,2) NOT NULL,
    HPScore DECIMAL(10,2) NOT NULL,
    PositionScore DECIMAL(10,2) NOT NULL,
    AbilityScore DECIMAL(10,2) NOT NULL,
    StatusScore DECIMAL(10,2) NOT NULL,
    WasSelected BIT NOT NULL,
    Reasoning NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_ThreatLog_GameSession (GameSessionId),
    INDEX IX_ThreatLog_Combat (CombatEncounterId)
);

-- Target selection log (for debugging)
CREATE TABLE AITargetSelectionLog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    CombatEncounterId UNIQUEIDENTIFIER NOT NULL,
    EnemyId UNIQUEIDENTIFIER NOT NULL,
    EnemyArchetype INT NOT NULL,
    SelectedTargetId UNIQUEIDENTIFIER NOT NULL,
    FinalScore DECIMAL(10,2) NOT NULL,
    TurnNumber INT NOT NULL,
    DecisionTimeMs INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_TargetLog_GameSession (GameSessionId),
    INDEX IX_TargetLog_Combat (CombatEncounterId)
);

-- Situational analysis log
CREATE TABLE AISituationalAnalysisLog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    CombatEncounterId UNIQUEIDENTIFIER NOT NULL,
    EnemyId UNIQUEIDENTIFIER NOT NULL,
    TurnNumber INT NOT NULL,
    AllyCount INT NOT NULL,
    EnemyCount INT NOT NULL,
    IsOutnumbered BIT NOT NULL,
    SelfHPPercent DECIMAL(5,2) NOT NULL,
    IsLowHP BIT NOT NULL,
    IsFlanked BIT NOT NULL,
    HasHighGround BIT NOT NULL,
    TacticalAdvantage VARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_SituationalLog_Combat (CombatEncounterId)
);
```

**Seed Data:**

```sql
INSERT INTO AIThreatWeights (ArchetypeId, ArchetypeName, DamageWeight, HPWeight, PositionWeight, AbilityWeight, StatusWeight)
VALUES
    (1, 'Aggressive', 0.50, 0.10, 0.10, 0.20, 0.10),
    (2, 'Defensive', 0.20, 0.30, 0.30, 0.10, 0.10),
    (3, 'Cautious', 0.30, 0.20, 0.30, 0.10, 0.10),
    (4, 'Reckless', 0.40, 0.00, 0.00, 0.40, 0.20),
    (5, 'Tactical', 0.30, 0.25, 0.25, 0.15, 0.05),
    (6, 'Support', 0.20, 0.40, 0.20, 0.10, 0.10),
    (7, 'Control', 0.25, 0.15, 0.20, 0.30, 0.10),
    (8, 'Ambusher', 0.30, 0.30, 0.30, 0.05, 0.05);
```

---

## IV. Integration Points

### Combat System Integration

```csharp
// In CombatService.ProcessEnemyTurnAsync

public async Task ProcessEnemyTurnAsync(Enemy enemy)
{
    var state = await BuildBattlefieldStateAsync();
    
    // NEW: Analyze situation
    var situation = _situationalAnalysisService.AnalyzeSituation(enemy, state);
    
    // NEW: Select target intelligently
    var target = await _targetSelectionService.SelectTargetAsync(
        enemy, 
        state.PlayerCharacters.Where(c => !c.IsDead).ToList(), 
        state);
    
    if (target == null)
    {
        // No valid targets, skip turn
        return;
    }
    
    // Continue with ability selection (v0.42.2)
    // ...
}
```

---

## V. Testing Requirements

**Unit Tests (Target: 80%+ coverage):**

```csharp
[TestClass]
public class ThreatAssessmentServiceTests
{
    [TestMethod]
    public async Task AssessThreat_AggressiveArchetype_PrioritizesDamage()
    {
        // Arrange: Two targets, one high damage, one low HP
        var highDamageTarget = CreateCharacter(damage: 50, hp: 100);
        var lowHPTarget = CreateCharacter(damage: 10, hp: 20);
        var aggressiveEnemy = CreateEnemy(AIArchetype.Aggressive);
        
        // Act
        var threat1 = await _service.AssessThreatAsync(aggressiveEnemy, highDamageTarget, state);
        var threat2 = await _service.AssessThreatAsync(aggressiveEnemy, lowHPTarget, state);
        
        // Assert: Aggressive should prioritize high damage target
        Assert.IsTrue(threat1.TotalThreatScore > threat2.TotalThreatScore);
    }
    
    [TestMethod]
    public async Task AssessThreat_SupportArchetype_PrioritizesLowHP()
    {
        // Arrange
        var highHPAlly = CreateEnemy(hp: 100, maxHP: 100);
        var lowHPAlly = CreateEnemy(hp: 20, maxHP: 100);
        var supportEnemy = CreateEnemy([AIArchetype.Support](http://AIArchetype.Support));
        
        // Act
        var threat1 = await _service.AssessThreatAsync(supportEnemy, highHPAlly, state);
        var threat2 = await _service.AssessThreatAsync(supportEnemy, lowHPAlly, state);
        
        // Assert: Support should prioritize healing low HP ally
        Assert.IsTrue(threat2.TotalThreatScore > threat1.TotalThreatScore);
    }
}

[TestClass]
public class TargetSelectionServiceTests
{
    [TestMethod]
    public async Task SelectTarget_ChoosesHighestThreat()
    {
        // Arrange: 3 targets with different threat levels
        var targets = new List<Character>
        {
            CreateCharacter("Low", threat: 10),
            CreateCharacter("High", threat: 50),
            CreateCharacter("Medium", threat: 25)
        };
        
        // Act
        var selected = await _service.SelectTargetAsync(enemy, targets, state);
        
        // Assert
        Assert.AreEqual("High", [selected.Name](http://selected.Name));
    }
    
    [TestMethod]
    public async Task SelectTarget_FiltersDeadTargets()
    {
        // Arrange
        var targets = new List<Character>
        {
            CreateCharacter("Alive", isDead: false),
            CreateCharacter("Dead", isDead: true)
        };
        
        // Act
        var selected = await _service.SelectTargetAsync(enemy, targets, state);
        
        // Assert
        Assert.AreEqual("Alive", [selected.Name](http://selected.Name));
    }
}

[TestClass]
public class SituationalAnalysisServiceTests
{
    [TestMethod]
    public void AnalyzeSituation_Outnumbered_SetsFlag()
    {
        // Arrange: 1 ally vs 3 enemies
        var state = CreateBattlefieldState(allyCount: 1, enemyCount: 3);
        
        // Act
        var context = _service.AnalyzeSituation(enemy, state);
        
        // Assert
        Assert.IsTrue(context.IsOutnumbered);
    }
    
    [TestMethod]
    public void AnalyzeSituation_LowHP_SetsLowHPFlag()
    {
        // Arrange
        var lowHPEnemy = CreateEnemy(hp: 20, maxHP: 100);
        
        // Act
        var context = _service.AnalyzeSituation(lowHPEnemy, state);
        
        // Assert
        Assert.IsTrue(context.IsLowHP);
        Assert.AreEqual(0.2f, context.SelfHPPercent);
    }
}
```

---

## VI. Success Criteria

**v0.42.1 is DONE when:**

- [ ]  ThreatAssessmentService calculates accurate threat scores
- [ ]  All 8 AI archetypes have proper threat weight configuration
- [ ]  TargetSelectionService chooses logical targets
- [ ]  SituationalAnalysisService provides battlefield context
- [ ]  Database schema created and seeded
- [ ]  All unit tests pass (80%+ coverage)
- [ ]  Structured logging captures all AI decisions
- [ ]  Performance: Threat assessment <10ms per target
- [ ]  Performance: Target selection <25ms total per enemy turn

---

## VII. Implementation Notes

**Logging Strategy:**

All AI decisions must be logged with structured data for debugging:

```csharp
_logger.Information(
    "AI Decision: {EnemyId} ({Archetype}) | Target: {Target} ({Score}) | " +
    "Factors: Dmg={Dmg} HP={HP} Pos={Pos} | Situation: {Advantage}",
    enemy.EnemyId, enemy.AIArchetype, [target.Name](http://target.Name), finalScore,
    damageScore, hpScore, positionScore, situation.Advantage);
```

**Performance Considerations:**

- Cache threat weights (don't query DB every turn)
- Batch threat assessments when possible
- Use parallel processing for multiple enemies
- Profile and optimize hotspots

**Testing Focus:**

- Each archetype behaves distinctly
- Threat scores are consistent and logical
- Target selection handles edge cases (no valid targets, all targets dead, etc.)
- Situational analysis accurately reflects battlefield state

---

**Ready to build intelligent threat assessment and target selection.**