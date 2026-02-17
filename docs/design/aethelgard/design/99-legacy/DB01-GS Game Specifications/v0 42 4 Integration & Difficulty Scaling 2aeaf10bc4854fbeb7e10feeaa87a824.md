# v0.42.4: Integration & Difficulty Scaling

Type: Mechanic
Description: Endgame integration, difficulty scaling, performance optimization, and comprehensive testing. Delivers NG+ AI intelligence scaling (0-5 tiers), Challenge Sector AI adaptation, Boss Gauntlet progression, Endless Mode wave-based scaling, and performance optimization (<50ms per enemy decision).
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.42.3 (Boss AI), v0.40 (Endgame Content)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.42: Enemy AI Improvements & Behavior Polish (v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Spec:** [v0.42: Enemy AI Improvements & Behavior Polish](v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)

**Status:** Design Phase

**Timeline:** 5-8 hours (1 week part-time)

**Focus:** Endgame integration, difficulty scaling, performance optimization, and comprehensive testing

---

## I. Overview

v0.42.4 completes the AI system by integrating with endgame modes (NG+, Challenge Sectors, Boss Gauntlet, Endless), implementing difficulty-based intelligence scaling, and ensuring production-ready performance.

**What This Delivers:**

- NG+ AI intelligence scaling (0-5 tiers)
- Challenge Sector AI adaptation
- Boss Gauntlet progression system
- Endless Mode wave-based scaling
- Performance optimization (<50ms per enemy decision)
- Comprehensive testing suite
- AI debugging tools

**Success Metric:**

NG+5 enemies feel genuinely smarter than NG+0. Performance is consistently under 50ms per enemy turn. Players report that higher difficulties require tactical skill, not just better gear.

---

## II. Functional Requirements

### FR1: NG+ Intelligence Scaling

**Requirement:**

Implement AI intelligence scaling across NG+ tiers where enemies become progressively smarter (not just tankier).

**Intelligence Levels:**

```
Level 0 (NG+0 - Normal):
- Basic threat assessment
- Occasional tactical errors (15% chance)
- Simple ability priorities
- No group coordination

Level 1-2 (NG+1-2):
- Improved threat assessment
- Fewer errors (8% chance)
- Better ability timing
- Basic group tactics

Level 3-4 (NG+3-4):
- Advanced threat calculation
- Rare errors (3% chance)
- Near-optimal ability usage
- Strong group coordination

Level 5 (NG+5):
- Perfect threat assessment
- No tactical errors
- Optimal ability usage
- Maximum coordination
- Exploits player weaknesses
```

**C# Implementation:**

```csharp
public class DifficultyScalingService : IDifficultyScalingService
{
    private readonly ILogger<DifficultyScalingService> _logger;
    private readonly IGameStateService _gameStateService;

    public async Task<int> GetAIIntelligenceLevelAsync()
    {
        var ngPlusTier = await _gameStateService.GetCurrentNGPlusTierAsync();
        var endlessWave = await _gameStateService.GetCurrentEndlessWaveAsync();
        var isChallengeSector = await _gameStateService.IsInChallengeSectorAsync();
        var isBossGauntlet = await _gameStateService.IsInBossGauntletAsync();
        
        // Endless Mode has special scaling
        if (endlessWave.HasValue)
        {
            return CalculateEndlessIntelligence(endlessWave.Value);
        }
        
        // Boss Gauntlet escalates per boss
        if (isBossGauntlet)
        {
            var bossNumber = await _gameStateService.GetCurrentBossGauntletNumberAsync();
            return CalculateBossGauntletIntelligence(bossNumber);
        }
        
        // Challenge Sectors use base + modifier
        if (isChallengeSector)
        {
            var baseTier = Math.Min(ngPlusTier, 5);
            return Math.Min(baseTier + 1, 5);  // Challenge Sectors = +1 intelligence
        }
        
        // Standard NG+ scaling
        return Math.Min(ngPlusTier, 5);
    }

    private int CalculateEndlessIntelligence(int wave)
    {
        // Gradual scaling in Endless Mode
        if (wave <= 10) return 1;
        if (wave <= 20) return 2;
        if (wave <= 30) return 3;
        if (wave <= 40) return 4;
        return 5;  // Wave 40+: max intelligence
    }

    private int CalculateBossGauntletIntelligence(int bossNumber)
    {
        // Each boss in gauntlet is smarter
        // Boss 1-2: Intelligence 2
        // Boss 3-4: Intelligence 3
        // Boss 5-6: Intelligence 4
        // Boss 7-8: Intelligence 5
        return Math.Min((bossNumber + 1) / 2, 5);
    }

    public async Task<EnemyAction> ApplyIntelligenceScalingAsync(
        EnemyAction action,
        int intelligenceLevel,
        BattlefieldState state)
    {
        // Low intelligence: introduce intentional mistakes
        if (intelligenceLevel < 3)
        {
            var errorChance = CalculateErrorChance(intelligenceLevel);
            
            if (Random.NextDouble() < errorChance)
            {
                _logger.Debug(
                    "AI {EnemyId} making tactical error (intelligence={Level}, errorChance={Chance})",
                    [action.Actor](http://action.Actor).EnemyId, intelligenceLevel, errorChance);
                
                // Make a suboptimal decision
                action = await MakeSuboptimalDecisionAsync(action, state);
            }
        }
        
        // High intelligence: advanced tactics
        if (intelligenceLevel >= 4)
        {
            await ApplyAdvancedTacticsAsync(action, state);
        }
        
        // Max intelligence: exploit player weaknesses
        if (intelligenceLevel == 5)
        {
            await ExploitPlayerWeaknessesAsync(action, state);
        }
        
        return action;
    }

    private double CalculateErrorChance(int intelligenceLevel)
    {
        return intelligenceLevel switch
        {
            0 => 0.15,  // 15% error rate
            1 => 0.10,  // 10% error rate
            2 => 0.08,  // 8% error rate
            3 => 0.03,  // 3% error rate
            4 => 0.01,  // 1% error rate
            5 => 0.00,  // No errors
            _ => 0.15
        };
    }

    private async Task<EnemyAction> MakeSuboptimalDecisionAsync(
        EnemyAction action,
        BattlefieldState state)
    {
        // Types of mistakes:
        // 1. Target suboptimal enemy (not highest threat)
        // 2. Use basic attack instead of better ability
        // 3. Poor positioning choice
        
        var mistakeType = [Random.Next](http://Random.Next)(1, 4);
        
        switch (mistakeType)
        {
            case 1:  // Wrong target
                var targets = state.PlayerCharacters.Where(c => !c.IsDead).ToList();
                if (targets.Any())
                {
                    [action.Target](http://action.Target) = targets[[Random.Next](http://Random.Next)(targets.Count)];
                    _logger.Debug("AI mistake: Wrong target selected");
                }
                break;
                
            case 2:  // Use basic attack
                action.SelectedAbility = [action.Actor](http://action.Actor).BasicAttack;
                _logger.Debug("AI mistake: Used basic attack instead of better ability");
                break;
                
            case 3:  // Poor positioning
                // Don't move (even if should)
                action.MoveTo = null;
                _logger.Debug("AI mistake: Failed to reposition");
                break;
        }
        
        return action;
    }

    private async Task ApplyAdvancedTacticsAsync(EnemyAction action, BattlefieldState state)
    {
        // Advanced tactics for intelligence 4+:
        
        // 1. Focus fire (all enemies attack same target)
        var mostCommonTarget = state.Enemies
            .Where(e => !e.IsDead && e.CurrentTarget != null)
            .GroupBy(e => e.CurrentTarget)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;
        
        if (mostCommonTarget != null)
        {
            [action.Target](http://action.Target) = mostCommonTarget;
            _logger.Debug("Advanced tactic: Focus fire on {TargetId}", mostCommonTarget.CharacterId);
        }
        
        // 2. Coordinated CC chain
        var recentlyStunnedTargets = state.PlayerCharacters
            .Where(c => c.ActiveStatusEffects.Any(e => e.IsCC && e.RemainingDuration <= 1))
            .ToList();
        
        if (recentlyStunnedTargets.Any() && action.SelectedAbility.AppliesStatusEffect)
        {
            // Chain CC on same target
            [action.Target](http://action.Target) = recentlyStunnedTargets.First();
            _logger.Debug("Advanced tactic: CC chain");
        }
        
        // 3. Save powerful abilities for optimal moment
        if (action.SelectedAbility.Cooldown >= 5)  // Powerful ability
        {
            // Only use if target is vulnerable or it's critical moment
            var isOptimalMoment = 
                [action.Target](http://action.Target).CurrentHP < [action.Target](http://action.Target).MaxHP * 0.3f ||  // Low HP target
                state.Enemies.Count(e => !e.IsDead) < 3;  // Losing, desperate
            
            if (!isOptimalMoment)
            {
                // Save the big ability, use something else
                var alternateAbility = [action.Actor](http://action.Actor).Abilities
                    .Where(a => !a.IsOnCooldown && a.Cooldown < 5)
                    .OrderByDescending(a => a.BaseDamage)
                    .FirstOrDefault();
                
                if (alternateAbility != null)
                {
                    action.SelectedAbility = alternateAbility;
                    _logger.Debug("Advanced tactic: Saving powerful ability");
                }
            }
        }
    }

    private async Task ExploitPlayerWeaknessesAsync(EnemyAction action, BattlefieldState state)
    {
        // Max intelligence: identify and exploit weaknesses
        
        // Find player's weakest character (lowest HP%)
        var weakestPlayer = state.PlayerCharacters
            .Where(c => !c.IsDead)
            .OrderBy(c => (float)c.CurrentHP / c.MaxHP)
            .First();
        
        var hpPercent = (float)weakestPlayer.CurrentHP / weakestPlayer.MaxHP;
        
        // If player is weak, focus them down for the kill
        if (hpPercent < 0.4f)
        {
            [action.Target](http://action.Target) = weakestPlayer;
            _logger.Debug("Max intelligence: Exploiting weak player at {HP}% HP", 
                (int)(hpPercent * 100));
            
            // Use execute abilities if available
            var executeAbility = [action.Actor](http://action.Actor).Abilities
                .Where(a => !a.IsOnCooldown && a.HasExecuteMechanic)
                .FirstOrDefault();
            
            if (executeAbility != null)
            {
                action.SelectedAbility = executeAbility;
                _logger.Debug("Max intelligence: Using execute ability");
            }
        }
        
        // Identify player's resource state
        var lowManaPlayers = state.PlayerCharacters
            .Where(c => !c.IsDead && c.CurrentMana < c.MaxMana * 0.2f)
            .ToList();
        
        if (lowManaPlayers.Any())
        {
            // Player is low on resources, pressure them
            [action.Target](http://action.Target) = lowManaPlayers
                .OrderByDescending(c => c.ThreatLevel)
                .First();
            
            _logger.Debug("Max intelligence: Pressuring low-mana player");
        }
        
        // Identify if player lacks mobility
        var immobilePlayers = state.PlayerCharacters
            .Where(c => !c.IsDead && c.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Immobilize))
            .ToList();
        
        if (immobilePlayers.Any())
        {
            // Easy target, focus them
            [action.Target](http://action.Target) = immobilePlayers.First();
            _logger.Debug("Max intelligence: Targeting immobilized player");
        }
    }
}
```

---

### FR2: Challenge Sector AI Adaptation

**Requirement:**

AI adapts tactics based on Challenge Sector modifiers.

**C# Implementation:**

```csharp
public class ChallengeSectorAIService : IChallengeSectorAIService
{
    public async Task AdaptToSectorModifiersAsync(
        Enemy enemy,
        EnemyAction action,
        List<ChallengeSectorModifier> modifiers,
        BattlefieldState state)
    {
        foreach (var modifier in modifiers)
        {
            switch (modifier.Type)
            {
                case SectorModifierType.NoHealing:
                    // AI focuses on burst damage (no attrition allowed)
                    await PrioritizeBurstDamageAsync(enemy, action, state);
                    break;
                    
                case SectorModifierType.DoubleSpeed:
                    // AI uses abilities more aggressively (shorter fight)
                    action.AggressionModifier += 0.3f;
                    break;
                    
                case SectorModifierType.OneHP:
                    // AI plays extremely cautiously (one hit = death)
                    await PrioritizeDefenseAsync(enemy, action, state);
                    break;
                    
                case SectorModifierType.NoAbilities:
                    // Player has no abilities, AI can play more recklessly
                    action.AggressionModifier += 0.5f;
                    break;
                    
                case SectorModifierType.HalfDamage:
                    // Fight will be longer, AI conserves resources
                    await ConserveResourcesAsync(enemy, action, state);
                    break;
                    
                case SectorModifierType.Permadeath:
                    // Player is cautious, AI can be aggressive
                    action.AggressionModifier += 0.4f;
                    break;
            }
        }
    }

    private async Task PrioritizeBurstDamageAsync(
        Enemy enemy,
        EnemyAction action,
        BattlefieldState state)
    {
        // In [No Healing] sectors, use highest damage abilities
        var burstAbility = enemy.Abilities
            .Where(a => !a.IsOnCooldown)
            .OrderByDescending(a => a.BaseDamage)
            .FirstOrDefault();
        
        if (burstAbility != null && burstAbility.BaseDamage > action.SelectedAbility.BaseDamage)
        {
            action.SelectedAbility = burstAbility;
            _logger.Debug("Sector adaptation: Using burst damage (No Healing sector)");
        }
    }

    private async Task PrioritizeDefenseAsync(
        Enemy enemy,
        EnemyAction action,
        BattlefieldState state)
    {
        // In dangerous sectors, prioritize survival
        var defensiveAbility = enemy.Abilities
            .Where(a => !a.IsOnCooldown && (a.IsBuff || a.IsHeal))
            .FirstOrDefault();
        
        if (defensiveAbility != null)
        {
            action.SelectedAbility = defensiveAbility;
            _logger.Debug("Sector adaptation: Using defensive ability");
        }
    }

    private async Task ConserveResourcesAsync(
        Enemy enemy,
        EnemyAction action,
        BattlefieldState state)
    {
        // Avoid expensive abilities if fight will be long
        if (action.SelectedAbility.ManaCost > enemy.MaxMana * 0.3f)
        {
            // Too expensive, use cheaper alternative
            var cheapAbility = enemy.Abilities
                .Where(a => !a.IsOnCooldown && a.ManaCost < enemy.MaxMana * 0.15f)
                .OrderByDescending(a => a.BaseDamage)
                .FirstOrDefault();
            
            if (cheapAbility != null)
            {
                action.SelectedAbility = cheapAbility;
                _logger.Debug("Sector adaptation: Conserving resources");
            }
        }
    }
}
```

---

### FR3: Performance Optimization

**Requirement:**

Ensure AI decision-making is consistently under 50ms per enemy turn.

**C# Implementation:**

```csharp
public class AIPerformanceMonitor : IAIPerformanceMonitor
{
    private readonly ILogger<AIPerformanceMonitor> _logger;
    private readonly Dictionary<string, PerformanceMetrics> _metrics = new();

    public async Task<T> MonitorPerformanceAsync<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await operation();
            stopwatch.Stop();
            
            RecordMetric(operationName, stopwatch.ElapsedMilliseconds);
            
            if (stopwatch.ElapsedMilliseconds > 50)
            {
                _logger.Warning(
                    "AI operation {Operation} took {Ms}ms (threshold: 50ms)",
                    operationName, stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "AI operation {Operation} failed after {Ms}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void RecordMetric(string operationName, long milliseconds)
    {
        if (!_metrics.ContainsKey(operationName))
        {
            _metrics[operationName] = new PerformanceMetrics();
        }
        
        var metric = _metrics[operationName];
        metric.TotalCalls++;
        metric.TotalMs += milliseconds;
        metric.MaxMs = Math.Max(metric.MaxMs, milliseconds);
        metric.MinMs = metric.MinMs == 0 ? milliseconds : Math.Min(metric.MinMs, milliseconds);
    }

    public Dictionary<string, PerformanceMetrics> GetMetrics()
    {
        return _metrics.ToDictionary(
            kvp => kvp.Key,
            kvp => new PerformanceMetrics
            {
                TotalCalls = kvp.Value.TotalCalls,
                TotalMs = kvp.Value.TotalMs,
                AverageMs = kvp.Value.TotalCalls > 0 ? kvp.Value.TotalMs / kvp.Value.TotalCalls : 0,
                MaxMs = kvp.Value.MaxMs,
                MinMs = kvp.Value.MinMs
            });
    }
}

public class PerformanceMetrics
{
    public int TotalCalls { get; set; }
    public long TotalMs { get; set; }
    public double AverageMs { get; set; }
    public long MaxMs { get; set; }
    public long MinMs { get; set; }
}

// Usage:
public async Task<EnemyAction> DecideActionAsync(Enemy enemy, BattlefieldState state)
{
    return await _performanceMonitor.MonitorPerformanceAsync(
        "DecideAction",
        async () =>
        {
            // AI decision logic here
            var action = await PerformAIDecisionAsync(enemy, state);
            return action;
        });
}
```

**Performance Optimization Techniques:**

1. **Caching:**

```csharp
// Cache threat weights (don't query DB every turn)
private readonly ConcurrentDictionary<AIArchetype, AIThreatWeights> _weightsCache = new();

public async Task<AIThreatWeights> GetThreatWeightsAsync(AIArchetype archetype)
{
    return _weightsCache.GetOrAdd(archetype, async key =>
    {
        return await _configRepo.GetThreatWeightsAsync(key);
    });
}
```

1. **Parallel Processing:**

```csharp
// Assess threats for multiple targets in parallel
var assessmentTasks = potentialTargets
    .Select(target => _threatService.AssessThreatAsync(enemy, target, state))
    .ToList();

var assessments = await Task.WhenAll(assessmentTasks);
```

1. **Early Exit:**

```csharp
// Don't evaluate all abilities if we find a perfect match
foreach (var ability in abilities)
{
    var score = await ScoreAbilityAsync(ability, enemy, target, state);
    
    if (score >= 95.0f)  // Near-perfect score
    {
        return ability;  // Early exit, don't check remaining abilities
    }
}
```

---

### FR4: AI Debugging Tools

**Requirement:**

Provide tools to debug and visualize AI decision-making.

**C# Implementation:**

```csharp
public class AIDebugService : IAIDebugService
{
    private readonly ILogger<AIDebugService> _logger;
    private bool _debugMode = false;

    public void EnableDebugMode()
    {
        _debugMode = true;
        _logger.Information("AI Debug Mode ENABLED");
    }

    public void DisableDebugMode()
    {
        _debugMode = false;
        _logger.Information("AI Debug Mode DISABLED");
    }

    public void LogDecision(Enemy enemy, EnemyAction action, DecisionContext context)
    {
        if (!_debugMode) return;
        
        var debugInfo = new
        {
            Enemy = new { enemy.EnemyId, [enemy.Name](http://enemy.Name), enemy.AIArchetype },
            Decision = new
            {
                Target = [action.Target](http://action.Target)?.Name,
                Ability = action.SelectedAbility?.Name,
                MoveTo = action.MoveTo
            },
            Context = new
            {
                IntelligenceLevel = context.IntelligenceLevel,
                ThreatScores = context.ThreatAssessments
                    .Select(t => new { [t.Target.Name](http://t.Target.Name), t.TotalThreatScore })
                    .ToList(),
                AvailableAbilities = context.AvailableAbilities
                    .Select(a => new { [a.Name](http://a.Name), a.IsOnCooldown })
                    .ToList()
            },
            Reasoning = context.Reasoning
        };
        
        _logger.Information(
            "[AI DEBUG] {Enemy} decision: {Decision}",
            [enemy.Name](http://enemy.Name),
            JsonSerializer.Serialize(debugInfo, new JsonSerializerOptions { WriteIndented = true }));
    }

    public AIDecisionReport GenerateDecisionReport(Guid combatEncounterId)
    {
        // Generate comprehensive report of all AI decisions in an encounter
        var logs = GetAILogsForEncounter(combatEncounterId);
        
        var report = new AIDecisionReport
        {
            EncounterId = combatEncounterId,
            TotalDecisions = logs.Count,
            AverageDecisionTimeMs = logs.Average(l => l.DecisionTimeMs),
            DecisionsByArchetype = logs
                .GroupBy(l => l.Archetype)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostCommonTargets = logs
                .GroupBy(l => l.TargetName)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count()),
            AbilityUsageFrequency = logs
                .GroupBy(l => l.AbilityName)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count())
        };
        
        return report;
    }
}

public class DecisionContext
{
    public int IntelligenceLevel { get; set; }
    public List<ThreatAssessment> ThreatAssessments { get; set; }
    public List<Ability> AvailableAbilities { get; set; }
    public string Reasoning { get; set; }
}

public class AIDecisionReport
{
    public Guid EncounterId { get; set; }
    public int TotalDecisions { get; set; }
    public double AverageDecisionTimeMs { get; set; }
    public Dictionary<AIArchetype, int> DecisionsByArchetype { get; set; }
    public Dictionary<string, int> MostCommonTargets { get; set; }
    public Dictionary<string, int> AbilityUsageFrequency { get; set; }
}
```

---

## III. Database Schema

```sql
-- AI performance metrics
CREATE TABLE AIPerformanceMetrics (
    MetricId BIGINT PRIMARY KEY IDENTITY,
    OperationName VARCHAR(100) NOT NULL,
    DurationMs INT NOT NULL,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    CombatEncounterId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_PerfMetrics_Operation (OperationName),
    INDEX IX_PerfMetrics_Session (GameSessionId)
);

-- Difficulty scaling log
CREATE TABLE AIDifficultyScalingLog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    EnemyId UNIQUEIDENTIFIER NOT NULL,
    IntelligenceLevel INT NOT NULL,
    NGPlusTier INT NOT NULL,
    EndlessWave INT,
    IsChallengeSector BIT NOT NULL,
    IsBossGauntlet BIT NOT NULL,
    MadeIntentionalError BIT NOT NULL,
    ErrorType VARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_DifficultyLog_Session (GameSessionId)
);
```

---

## IV. Integration Testing

```csharp
[TestClass]
public class DifficultyScalingIntegrationTests
{
    [TestMethod]
    public async Task NG5Enemy_NoTacticalErrors()
    {
        // Arrange: Simulate 100 enemy turns at NG+5
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var state = CreateBattlefieldState();
        ConfigureNG5Difficulty();
        
        int errorCount = 0;
        
        // Act: Run 100 turns
        for (int i = 0; i < 100; i++)
        {
            var action = await _aiService.DecideActionAsync(enemy, state);
            
            if (IsSuboptimalDecision(action, state))
            {
                errorCount++;
            }
        }
        
        // Assert: NG+5 should have 0% error rate
        Assert.AreEqual(0, errorCount, "NG+5 AI should make no tactical errors");
    }
    
    [TestMethod]
    public async Task EndlessMode_IntelligenceScalesWithWave()
    {
        // Arrange
        ConfigureEndlessMode();
        
        // Act & Assert
        SetEndlessWave(5);
        var intel5 = await _scalingService.GetAIIntelligenceLevelAsync();
        Assert.AreEqual(1, intel5);
        
        SetEndlessWave(15);
        var intel15 = await _scalingService.GetAIIntelligenceLevelAsync();
        Assert.AreEqual(2, intel15);
        
        SetEndlessWave(45);
        var intel45 = await _scalingService.GetAIIntelligenceLevelAsync();
        Assert.AreEqual(5, intel45);
    }
}

[TestClass]
public class PerformanceTests
{
    [TestMethod]
    public async Task AIDecision_CompletesUnder50ms()
    {
        // Arrange
        var enemy = CreateEnemy();
        var state = CreateBattlefieldState(playerCount: 4, enemyCount: 6);
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var action = await _aiService.DecideActionAsync(enemy, state);
        stopwatch.Stop();
        
        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50, 
            $"AI decision took {stopwatch.ElapsedMilliseconds}ms (threshold: 50ms)");
    }
    
    [TestMethod]
    public async Task FullCombatRound_AllEnemies_CompletesUnder500ms()
    {
        // Arrange: 10 enemies making decisions
        var enemies = Enumerable.Range(0, 10).Select(_ => CreateEnemy()).ToList();
        var state = CreateBattlefieldState();
        var stopwatch = Stopwatch.StartNew();
        
        // Act: All enemies decide
        var tasks = [enemies.Select](http://enemies.Select)(e => _aiService.DecideActionAsync(e, state));
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert: 10 enemies should complete in < 500ms
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"10 enemy decisions took {stopwatch.ElapsedMilliseconds}ms (threshold: 500ms)");
    }
}
```

---

## V. Success Criteria

**v0.42.4 is DONE when:**

- [ ]  NG+ intelligence scaling functional (0-5 tiers)
- [ ]  Challenge Sector AI adaptation working
- [ ]  Boss Gauntlet intelligence progression functional
- [ ]  Endless Mode wave scaling operational
- [ ]  All AI decisions complete under 50ms
- [ ]  Performance monitoring implemented
- [ ]  AI debugging tools functional
- [ ]  80%+ unit test coverage
- [ ]  Integration tests pass
- [ ]  NG+5 playtests confirm increased difficulty through intelligence

---

## VI. Final Deliverables

### Code Quality

- [ ]  All services implement interfaces
- [ ]  Comprehensive XML documentation
- [ ]  Structured logging throughout
- [ ]  No hardcoded values (use configuration)

### Testing

- [ ]  Unit tests for all services
- [ ]  Integration tests for difficulty scaling
- [ ]  Performance benchmarks
- [ ]  Playtest validation

### Documentation

- [ ]  API documentation complete
- [ ]  Configuration guide for designers
- [ ]  Debugging guide
- [ ]  Performance tuning guide

---

**Ready to complete the AI system with endgame integration and optimization.**