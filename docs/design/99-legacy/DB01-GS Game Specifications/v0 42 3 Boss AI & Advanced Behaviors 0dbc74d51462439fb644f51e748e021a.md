# v0.42.3: Boss AI & Advanced Behaviors

Type: Mechanic
Description: Boss-specific AI enhancements and complex encounter mechanics. Delivers phase transition system (3-phase boss encounters), ability rotation framework, add management and coordination, adaptive difficulty (boss responds to player strategy), and boss AI configuration database.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.42.2 (Ability Usage & Behavior Patterns), v0.23 (Boss Encounters)
Implementation Difficulty: Very Complex
Balance Validated: No
Parent item: v0.42: Enemy AI Improvements & Behavior Polish (v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Spec:** [v0.42: Enemy AI Improvements & Behavior Polish](v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)

**Status:** Design Phase

**Timeline:** 5-8 hours (1 week part-time)

**Focus:** Boss-specific AI enhancements and complex encounter mechanics

---

## I. Overview

v0.42.3 implements **advanced boss AI** that creates memorable, skill-testing encounters through phase-aware behavior, intelligent add management, and adaptive difficulty.

**What This Delivers:**

- Phase transition system (3-phase boss encounters)
- Ability rotation framework
- Add management and coordination
- Adaptive difficulty (boss responds to player strategy)
- Boss AI configuration database

**Success Metric:**

Boss encounters feel like puzzles to solve, not DPS races. Players report memorable boss fights and attribute victories to learning boss patterns, not luck.

---

## II. Functional Requirements

### FR1: Phase-Aware Boss Behavior

**Requirement:**

Implement a phase system where bosses change behavior at specific HP thresholds.

**Phase Structure:**

```
Phase 1 (100-66% HP): Teaching Phase
- Standard abilities
- Predictable patterns
- Gives player time to learn mechanics

Phase 2 (66-33% HP): Escalation Phase
- Increased aggression
- New abilities unlocked
- More complex patterns
- Summons adds

Phase 3 (33-0% HP): Desperation Phase
- All abilities available
- Fastest rotation
- Maximum lethality
- Enrage mechanics
```

**C# Implementation:**

```csharp
public class BossAIService : IBossAIService
{
    private readonly ILogger<BossAIService> _logger;
    private readonly IBossConfigurationRepository _bossConfigRepo;
    private readonly IAbilityRotationService _rotationService;

    public async Task<BossAction> DecideBossActionAsync(Boss boss, BattlefieldState state)
    {
        // Determine current phase
        var currentPhase = DeterminePhase(boss);
        
        // Check for phase transition
        if (ShouldTransitionPhase(boss, currentPhase))
        {
            await ExecutePhaseTransitionAsync(boss, currentPhase);
        }
        
        // Get phase-specific rotation
        var rotation = await _rotationService.GetPhaseRotationAsync(boss, currentPhase);
        
        // Select next ability in rotation
        var ability = await SelectNextAbilityInRotationAsync(boss, rotation, state);
        
        // Check if should summon adds
        var shouldSummonAdds = await ShouldSummonAddsAsync(boss, currentPhase, state);
        
        var action = new BossAction
        {
            Boss = boss,
            SelectedAbility = ability,
            Target = await SelectBossTargetAsync(boss, ability, state),
            Phase = currentPhase,
            SummonAdds = shouldSummonAdds
        };
        
        // Apply adaptive difficulty adjustments
        await ApplyAdaptiveDifficultyAsync(boss, action, state);
        
        return action;
    }

    private BossPhase DeterminePhase(Boss boss)
    {
        var hpPercent = (float)boss.CurrentHP / boss.MaxHP;
        
        if (hpPercent > 0.66f)
            return BossPhase.Phase1;
        else if (hpPercent > 0.33f)
            return BossPhase.Phase2;
        else
            return BossPhase.Phase3;
    }

    private bool ShouldTransitionPhase(Boss boss, BossPhase currentPhase)
    {
        // Check if boss has already transitioned to this phase
        if (boss.CurrentPhase == currentPhase)
            return false;
        
        // Boss is entering a new phase
        return boss.CurrentPhase != currentPhase;
    }

    private async Task ExecutePhaseTransitionAsync(Boss boss, BossPhase newPhase)
    {
        _logger.Information(
            "Boss {BossId} transitioning from {OldPhase} to {NewPhase} at {HP}% HP",
            boss.BossId, boss.CurrentPhase, newPhase, 
            (int)((float)boss.CurrentHP / boss.MaxHP * 100));
        
        boss.CurrentPhase = newPhase;
        
        // Get phase transition configuration
        var config = await _bossConfigRepo.GetPhaseTransitionAsync(boss.BossTypeId, newPhase);
        
        if (config != null)
        {
            // Execute transition effects
            if (!string.IsNullOrEmpty(config.TransitionDialogue))
            {
                await DisplayDialogueAsync(boss, config.TransitionDialogue);
            }
            
            if (config.TransitionAbilityId.HasValue)
            {
                // Execute special transition ability (e.g., AOE knockback)
                var transitionAbility = boss.Abilities.First(a => a.AbilityId == config.TransitionAbilityId.Value);
                await ExecuteTransitionAbilityAsync(boss, transitionAbility);
            }
            
            // Apply phase buffs
            foreach (var buff in config.PhaseBonuses)
            {
                boss.ApplyStatusEffect(buff);
            }
        }
    }

    public async Task<AbilityRotation> GetPhaseRotationAsync(Boss boss, int currentPhase)
    {
        var rotation = await _bossConfigRepo.GetAbilityRotationAsync(boss.BossTypeId, currentPhase);
        
        if (rotation == null)
        {
            _logger.Warning("No rotation found for boss {BossId} phase {Phase}", 
                boss.BossId, currentPhase);
            return CreateDefaultRotation(boss);
        }
        
        return rotation;
    }
}

public enum BossPhase
{
    Phase1 = 1,  // 100-66% HP
    Phase2 = 2,  // 66-33% HP
    Phase3 = 3   // 33-0% HP
}

public class BossAction : EnemyAction
{
    public Boss Boss { get; set; }
    public BossPhase Phase { get; set; }
    public bool SummonAdds { get; set; }
    public List<EnemyType> AddsToSummon { get; set; }
    public int AddCount { get; set; }
}
```

**Example Boss Configuration:**

**Forge-Master Thrain (Fire Boss):**

**Phase 1 (100-66% HP):**

- Transition: "The forge ignites!"
- Ability Rotation: Basic Attack → Basic Attack → Lava Torrent → Molten Grasp → Basic Attack
- Summon Adds: 2x Forge-Hardened Warriors at start of phase
- Strategy: Teaches players to manage adds while avoiding fire AOE

**Phase 2 (66-33% HP):**

- Transition: "The Forge awakens!" + Room-wide fire pulse (moderate damage)
- Ability Rotation: Meteor Strike → Flame Breath → Basic Attack → Molten Armor (self-buff)
- Summon Adds: 3x adds (1 Bone-Mender, 2 Forge-Hardened) when phase starts
- Buffs: +25% damage, immunity to fire
- Strategy: Players must prioritize killing healer while avoiding increased damage

**Phase 3 (33-0% HP):**

- Transition: "THE FORGE CONSUMES ALL!" + Massive fire explosion
- Ability Rotation: Molten Crash → Meteor Strike → Flame Breath → Basic Attack (faster rotation)
- Summon Adds: None (pure DPS race)
- Buffs: +50% damage, +30% attack speed, Enraged (all abilities deal fire damage)
- Unique Mechanic: Every 5 turns, uses "Inferno" (room-wide heavy damage, must use defensive abilities)
- Strategy: Burn boss down before Inferno kills everyone

---

### FR2: Ability Rotation System

**Requirement:**

Implement a rotation system where bosses use abilities in intelligent, predictable sequences rather than randomly.

**C# Implementation:**

```csharp
public class AbilityRotationService : IAbilityRotationService
{
    private readonly ILogger<AbilityRotationService> _logger;

    public async Task<Ability> SelectNextAbilityInRotationAsync(
        Boss boss,
        AbilityRotation rotation,
        BattlefieldState state)
    {
        // Get current step in rotation
        var currentStep = boss.CurrentRotationStep;
        
        // Check if we need to reset rotation
        if (currentStep >= rotation.Steps.Count)
        {
            currentStep = 0;
            boss.CurrentRotationStep = 0;
        }
        
        var rotationStep = rotation.Steps[currentStep];
        var ability = boss.Abilities.FirstOrDefault(a => a.AbilityId == rotationStep.AbilityId);
        
        if (ability == null)
        {
            _logger.Error("Rotation step references invalid ability {AbilityId}", rotationStep.AbilityId);
            return boss.BasicAttack;
        }
        
        // Check if ability is available (not on cooldown, enough resources)
        if (!IsAbilityAvailable(boss, ability))
        {
            // Ability not ready, use fallback or skip
            if (rotationStep.FallbackAbilityId.HasValue)
            {
                var fallback = boss.Abilities.FirstOrDefault(a => a.AbilityId == rotationStep.FallbackAbilityId.Value);
                if (fallback != null && IsAbilityAvailable(boss, fallback))
                {
                    _logger.Debug("Boss {BossId} using fallback ability {FallbackId}", 
                        boss.BossId, fallback.AbilityId);
                    return fallback;
                }
            }
            
            // No fallback, use basic attack and don't advance rotation
            _logger.Debug("Boss {BossId} ability not ready, using basic attack", boss.BossId);
            return boss.BasicAttack;
        }
        
        // Ability is ready, advance rotation
        boss.CurrentRotationStep++;
        
        _logger.Information(
            "Boss {BossId} using rotation ability: {AbilityName} (step {Step}/{Total})",
            boss.BossId, [ability.Name](http://ability.Name), currentStep + 1, rotation.Steps.Count);
        
        return ability;
    }

    private bool IsAbilityAvailable(Boss boss, Ability ability)
    {
        // Check cooldown
        if (ability.IsOnCooldown)
            return false;
        
        // Check resource cost
        if (ability.ManaCost > boss.CurrentMana)
            return false;
        
        // Check special conditions
        if (ability.RequiresMinHP.HasValue && boss.CurrentHP < ability.RequiresMinHP.Value)
            return false;
        
        return true;
    }
}

public class AbilityRotation
{
    public int BossTypeId { get; set; }
    public int Phase { get; set; }
    public List<RotationStep> Steps { get; set; }
}

public class RotationStep
{
    public int StepNumber { get; set; }
    public int AbilityId { get; set; }
    public int? FallbackAbilityId { get; set; }  // If primary not available
    public string Notes { get; set; }  // Designer notes
}
```

**Example Rotations:**

**Boss: Frost-Tyrant Ymir (Ice Boss)**

**Phase 1 Rotation:**

1. Ice Shard (single-target damage)
2. Ice Shard
3. Frost Nova (AOE around boss)
4. Basic Attack
5. Glacial Armor (defensive buff)
6. REPEAT

**Phase 2 Rotation:**

1. Blizzard (room-wide DOT)
2. Ice Shard → Ice Shard (rapid attacks)
3. Frozen Tomb (CC on highest threat)
4. Frost Nova
5. Basic Attack
6. REPEAT

**Phase 3 Rotation:**

1. Absolute Zero (massive AOE, must be interrupted or players die)
2. Ice Shard → Ice Shard → Ice Shard (triple cast)
3. Frozen Tomb on healer
4. Blizzard
5. REPEAT (faster, no basic attacks)

---

### FR3: Add Management System

**Requirement:**

Implement intelligent add summoning and coordination between boss and adds.

**C# Implementation:**

```csharp
public class AddManagementService : IAddManagementService
{
    private readonly IEnemySpawnService _spawnService;
    private readonly ILogger<AddManagementService> _logger;

    public async Task ManageAddsAsync(Boss boss, BattlefieldState state)
    {
        var config = await GetAddManagementConfigAsync(boss, boss.CurrentPhase);
        
        if (config == null)
            return;  // No adds for this boss/phase
        
        // Check if should summon adds
        if (ShouldSummonAdds(boss, config, state))
        {
            await SummonAddsAsync(boss, config, state);
        }
        
        // Coordinate with existing adds
        await CoordinateWithAddsAsync(boss, state);
    }

    private bool ShouldSummonAdds(Boss boss, AddManagementConfig config, BattlefieldState state)
    {
        // Check if already at max adds
        var currentAdds = state.Enemies.Count(e => e.IsSummonedBy == boss.BossId && !e.IsDead);
        if (currentAdds >= config.MaxSimultaneousAdds)
            return false;
        
        // Check turn-based trigger
        if (config.SummonEveryNTurns.HasValue)
        {
            var turnsSinceLastSummon = state.CurrentTurn - boss.LastAddSummonTurn;
            if (turnsSinceLastSummon >= config.SummonEveryNTurns.Value)
                return true;
        }
        
        // Check HP-based trigger
        if (config.SummonAtHPThresholds != null)
        {
            var hpPercent = (float)boss.CurrentHP / boss.MaxHP;
            foreach (var threshold in config.SummonAtHPThresholds)
            {
                if (hpPercent <= threshold && !boss.TriggeredHPThresholds.Contains(threshold))
                {
                    boss.TriggeredHPThresholds.Add(threshold);
                    return true;
                }
            }
        }
        
        // Check if all adds died (re-summon)
        if (config.ResummonIfAllAddsDie && currentAdds == 0 && boss.HasSummonedAdds)
        {
            return true;
        }
        
        return false;
    }

    private async Task SummonAddsAsync(Boss boss, AddManagementConfig config, BattlefieldState state)
    {
        _logger.Information("Boss {BossId} summoning adds", boss.BossId);
        
        // Display summon dialogue
        if (!string.IsNullOrEmpty(config.SummonDialogue))
        {
            await DisplayDialogueAsync(boss, config.SummonDialogue);
        }
        
        // Spawn adds
        foreach (var addType in config.AddTypes)
        {
            for (int i = 0; i < addType.Count; i++)
            {
                var spawnPosition = FindAddSpawnPositionAsync(boss, state.Grid);
                var add = await _spawnService.SpawnEnemyAsync(
                    addType.EnemyTypeId, 
                    spawnPosition, 
                    boss.BossId);
                
                // Apply add modifiers
                if (config.AddHPMultiplier != 1.0f)
                {
                    add.MaxHP = (int)(add.MaxHP * config.AddHPMultiplier);
                    add.CurrentHP = add.MaxHP;
                }
                
                if (config.AddDamageMultiplier != 1.0f)
                {
                    add.BaseDamage = (int)(add.BaseDamage * config.AddDamageMultiplier);
                }
            }
        }
        
        boss.LastAddSummonTurn = state.CurrentTurn;
        boss.HasSummonedAdds = true;
    }

    private async Task CoordinateWithAddsAsync(Boss boss, BattlefieldState state)
    {
        var adds = state.Enemies.Where(e => e.IsSummonedBy == boss.BossId && !e.IsDead).ToList();
        
        if (!adds.Any())
            return;
        
        // Boss protects adds if they're in danger
        var endangeredAdds = adds.Where(a => (float)a.CurrentHP / a.MaxHP < 0.3f).ToList();
        
        if (endangeredAdds.Any())
        {
            // Boss uses protective abilities or taunts to draw aggro
            var protectAbility = boss.Abilities.FirstOrDefault(a => a.IsProtective);
            if (protectAbility != null && !protectAbility.IsOnCooldown)
            {
                _logger.Debug("Boss {BossId} protecting endangered adds", boss.BossId);
                // Use protective ability on add
            }
        }
        
        // Adds coordinate targeting with boss
        var bossTarget = boss.CurrentTarget;
        if (bossTarget != null)
        {
            // Tell adds to focus same target
            foreach (var add in adds)
            {
                if (add.AIArchetype == AIArchetype.Tactical)
                {
                    add.PreferredTarget = bossTarget;  // Tactical adds follow boss's lead
                }
            }
        }
    }
}

public class AddManagementConfig
{
    public int BossTypeId { get; set; }
    public int Phase { get; set; }
    public List<AddType> AddTypes { get; set; }  // What adds to summon
    public int MaxSimultaneousAdds { get; set; }
    public int? SummonEveryNTurns { get; set; }  // Time-based trigger
    public List<float> SummonAtHPThresholds { get; set; }  // HP-based triggers
    public bool ResummonIfAllAddsDie { get; set; }
    public string SummonDialogue { get; set; }
    public float AddHPMultiplier { get; set; } = 1.0f;
    public float AddDamageMultiplier { get; set; } = 1.0f;
}

public class AddType
{
    public int EnemyTypeId { get; set; }
    public string EnemyTypeName { get; set; }
    public int Count { get; set; }
}
```

---

### FR4: Adaptive Difficulty System

**Requirement:**

Implement a system where bosses recognize and adapt to player strategies.

**C# Implementation:**

```csharp
public class AdaptiveDifficultyService : IAdaptiveDifficultyService
{
    private readonly ILogger<AdaptiveDifficultyService> _logger;

    public async Task ApplyAdaptiveDifficultyAsync(
        Boss boss,
        BossAction action,
        BattlefieldState state)
    {
        // Track player behavior
        var playerStrategy = AnalyzePlayerStrategyAsync(state);
        
        // Apply counter-strategies
        await ApplyCounterStrategiesAsync(boss, action, playerStrategy, state);
    }

    private PlayerStrategy AnalyzePlayerStrategyAsync(BattlefieldState state)
    {
        var strategy = new PlayerStrategy();
        
        // Check if player is camping in one location
        var playerPositions = state.PlayerCharacters
            .Select(c => c.PositionHistory.TakeLast(5))
            .ToList();
        
        var isStationary = playerPositions.All(history => 
            history.Distinct().Count() <= 2);  // Player hasn't moved much
        
        strategy.IsCamping = isStationary;
        
        // Check if player is healing a lot
        var recentHealing = state.CombatLog
            .Where(log => log.TurnNumber > state.CurrentTurn - 3)
            .Where(log => log.Action == CombatAction.Heal)
            .Sum(log => log.Amount);
        
        strategy.IsHealingHeavily = recentHealing > 100;
        
        // Check if player is using ranged attacks exclusively
        var recentAttacks = state.CombatLog
            .Where(log => log.TurnNumber > state.CurrentTurn - 5)
            .Where(log => log.Action == CombatAction.Attack)
            .ToList();
        
        var rangedAttacks = recentAttacks.Count(a => a.IsRangedAttack);
        strategy.IsKiting = rangedAttacks > recentAttacks.Count * 0.8f;
        
        // Check if player is burning down adds quickly
        var addDeaths = state.Enemies
            .Where(e => e.IsSummonedBy != null && e.IsDead)
            .Count(e => e.DeathTurn >= state.CurrentTurn - 2);
        
        strategy.IsPrioritizingAdds = addDeaths >= 2;
        
        return strategy;
    }

    private async Task ApplyCounterStrategiesAsync(
        Boss boss,
        BossAction action,
        PlayerStrategy strategy,
        BattlefieldState state)
    {
        if (strategy.IsCamping)
        {
            // Force player to move with AOE at their location
            var aoeAbility = boss.Abilities.FirstOrDefault(a => a.IsAOE && !a.IsOnCooldown);
            if (aoeAbility != null)
            {
                action.SelectedAbility = aoeAbility;
                [action.Target](http://action.Target) = state.PlayerCharacters
                    .OrderByDescending(c => c.PositionHistory.TakeLast(5).Distinct().Count())
                    .First();  // Target most stationary player
                
                _logger.Debug("Boss {BossId} countering camping with AOE", boss.BossId);
            }
        }
        
        if (strategy.IsHealingHeavily)
        {
            // Target the healer
            var healer = state.PlayerCharacters
                .OrderByDescending(c => c.RecentHealingDone)
                .FirstOrDefault();
            
            if (healer != null)
            {
                [action.Target](http://action.Target) = healer;
                _logger.Debug("Boss {BossId} targeting healer to counter healing", boss.BossId);
            }
        }
        
        if (strategy.IsKiting)
        {
            // Use gap closers or ranged abilities
            var gapCloser = boss.Abilities.FirstOrDefault(a => a.IsGapCloser && !a.IsOnCooldown);
            if (gapCloser != null)
            {
                action.SelectedAbility = gapCloser;
                _logger.Debug("Boss {BossId} using gap closer to counter kiting", boss.BossId);
            }
        }
        
        if (strategy.IsPrioritizingAdds)
        {
            // Boss becomes more aggressive when adds are killed
            boss.AggressionModifier += 0.2f;
            _logger.Debug("Boss {BossId} increasing aggression (adds killed)", boss.BossId);
        }
    }
}

public class PlayerStrategy
{
    public bool IsCamping { get; set; }
    public bool IsHealingHeavily { get; set; }
    public bool IsKiting { get; set; }
    public bool IsPrioritizingAdds { get; set; }
}
```

---

## III. Database Schema

```sql
-- Boss configuration
CREATE TABLE BossConfiguration (
    BossTypeId INT PRIMARY KEY,
    BossName VARCHAR(100) NOT NULL,
    HasPhases BIT NOT NULL DEFAULT 1,
    PhaseCount INT NOT NULL DEFAULT 3,
    UsesAdds BIT NOT NULL DEFAULT 1,
    UsesAdaptiveDifficulty BIT NOT NULL DEFAULT 1,
    BaseAggressionLevel INT NOT NULL DEFAULT 4,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Phase transitions
CREATE TABLE BossPhaseTransition (
    TransitionId INT PRIMARY KEY IDENTITY,
    BossTypeId INT NOT NULL,
    FromPhase INT NOT NULL,
    ToPhase INT NOT NULL,
    HPThreshold DECIMAL(5,2) NOT NULL,  -- HP % to trigger transition
    TransitionDialogue NVARCHAR(500),
    TransitionAbilityId INT,  -- Special ability used during transition
    PhaseBonuses NVARCHAR(MAX),  -- JSON array of buffs
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (BossTypeId) REFERENCES BossConfiguration(BossTypeId)
);

-- Ability rotations per phase
CREATE TABLE BossAbilityRotation (
    RotationId INT PRIMARY KEY IDENTITY,
    BossTypeId INT NOT NULL,
    Phase INT NOT NULL,
    StepNumber INT NOT NULL,
    AbilityId INT NOT NULL,
    FallbackAbilityId INT,
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (BossTypeId) REFERENCES BossConfiguration(BossTypeId),
    INDEX IX_Rotation_BossPhase (BossTypeId, Phase)
);

-- Add management configuration
CREATE TABLE BossAddManagement (
    ConfigId INT PRIMARY KEY IDENTITY,
    BossTypeId INT NOT NULL,
    Phase INT NOT NULL,
    EnemyTypeIdToSummon INT NOT NULL,
    SummonCount INT NOT NULL,
    MaxSimultaneousAdds INT NOT NULL,
    SummonEveryNTurns INT,
    SummonAtHPThreshold DECIMAL(5,2),
    ResummonIfAllDie BIT NOT NULL DEFAULT 0,
    SummonDialogue NVARCHAR(500),
    AddHPMultiplier DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    AddDamageMultiplier DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (BossTypeId) REFERENCES BossConfiguration(BossTypeId)
);

-- Boss AI log (for debugging)
CREATE TABLE BossAILog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    BossId UNIQUEIDENTIFIER NOT NULL,
    TurnNumber INT NOT NULL,
    Phase INT NOT NULL,
    SelectedAbilityId INT NOT NULL,
    TargetId UNIQUEIDENTIFIER,
    AddsSummoned INT NOT NULL DEFAULT 0,
    AdaptiveStrategyApplied VARCHAR(100),
    RotationStep INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_BossLog_GameSession (GameSessionId)
);
```

**Seed Data:**

```sql
-- Example: Forge-Master Thrain
INSERT INTO BossConfiguration (BossTypeId, BossName, HasPhases, PhaseCount, UsesAdds, UsesAdaptiveDifficulty, BaseAggressionLevel)
VALUES (1, 'Forge-Master Thrain', 1, 3, 1, 1, 4);

-- Phase transitions
INSERT INTO BossPhaseTransition (BossTypeId, FromPhase, ToPhase, HPThreshold, TransitionDialogue, TransitionAbilityId)
VALUES
    (1, 1, 2, 0.66, 'The Forge awakens!', 101),  -- Fire pulse
    (1, 2, 3, 0.33, 'THE FORGE CONSUMES ALL!', 102);  -- Massive explosion

-- Phase 1 rotation
INSERT INTO BossAbilityRotation (BossTypeId, Phase, StepNumber, AbilityId, FallbackAbilityId)
VALUES
    (1, 1, 1, 1, NULL),  -- Basic Attack
    (1, 1, 2, 1, NULL),  -- Basic Attack
    (1, 1, 3, 10, 1),    -- Lava Torrent (fallback: Basic Attack)
    (1, 1, 4, 11, 1),    -- Molten Grasp
    (1, 1, 5, 1, NULL);  -- Basic Attack

-- Phase 2 rotation
INSERT INTO BossAbilityRotation (BossTypeId, Phase, StepNumber, AbilityId, FallbackAbilityId)
VALUES
    (1, 2, 1, 20, 1),    -- Meteor Strike
    (1, 2, 2, 21, 1),    -- Flame Breath
    (1, 2, 3, 1, NULL),  -- Basic Attack
    (1, 2, 4, 22, NULL); -- Molten Armor (self-buff)

-- Add management
INSERT INTO BossAddManagement (BossTypeId, Phase, EnemyTypeIdToSummon, SummonCount, MaxSimultaneousAdds, SummonAtHPThreshold, SummonDialogue, AddHPMultiplier)
VALUES
    (1, 1, 10, 2, 2, 0.99, 'Forge-Hardened, to me!', 0.8),  -- Phase 1: 2 warriors at start
    (1, 2, 10, 2, 3, 0.66, NULL, 0.8),  -- Phase 2: 2 warriors
    (1, 2, 20, 1, 3, 0.66, 'Bone-Mender, sustain me!', 0.6);  -- Phase 2: 1 healer
```

---

## IV. Testing Requirements

```csharp
[TestClass]
public class BossAIServiceTests
{
    [TestMethod]
    public void DeterminePhase_FullHP_ReturnsPhase1()
    {
        // Arrange
        var boss = CreateBoss(hp: 1000, maxHP: 1000);
        
        // Act
        var phase = _service.DeterminePhase(boss);
        
        // Assert
        Assert.AreEqual(BossPhase.Phase1, phase);
    }
    
    [TestMethod]
    public void DeterminePhase_50HP_ReturnsPhase2()
    {
        // Arrange
        var boss = CreateBoss(hp: 500, maxHP: 1000);
        
        // Act
        var phase = _service.DeterminePhase(boss);
        
        // Assert
        Assert.AreEqual(BossPhase.Phase2, phase);
    }
    
    [TestMethod]
    public void DeterminePhase_20HP_ReturnsPhase3()
    {
        // Arrange
        var boss = CreateBoss(hp: 200, maxHP: 1000);
        
        // Act
        var phase = _service.DeterminePhase(boss);
        
        // Assert
        Assert.AreEqual(BossPhase.Phase3, phase);
    }
    
    [TestMethod]
    public async Task ExecutePhaseTransition_ShowsDialogue()
    {
        // Arrange
        var boss = CreateBoss();
        
        // Act
        await _service.ExecutePhaseTransitionAsync(boss, BossPhase.Phase2);
        
        // Assert
        // Verify dialogue was displayed
        _mockDialogueService.Verify(s => s.DisplayDialogueAsync(It.IsAny<string>()), Times.Once);
    }
}

[TestClass]
public class AbilityRotationServiceTests
{
    [TestMethod]
    public async Task SelectNextAbility_FollowsRotation()
    {
        // Arrange
        var ability1 = CreateAbility(id: 1, name: "Attack 1");
        var ability2 = CreateAbility(id: 2, name: "Attack 2");
        var rotation = CreateRotation(new[] { 1, 2, 1, 2 });
        var boss = CreateBoss(abilities: new[] { ability1, ability2 }, rotationStep: 0);
        
        // Act
        var selected1 = await _service.SelectNextAbilityInRotationAsync(boss, rotation, state);
        var selected2 = await _service.SelectNextAbilityInRotationAsync(boss, rotation, state);
        var selected3 = await _service.SelectNextAbilityInRotationAsync(boss, rotation, state);
        
        // Assert
        Assert.AreEqual(ability1, selected1);
        Assert.AreEqual(ability2, selected2);
        Assert.AreEqual(ability1, selected3);
    }
    
    [TestMethod]
    public async Task SelectNextAbility_AbilityOnCooldown_UsesFallback()
    {
        // Arrange
        var primaryAbility = CreateAbility(id: 1, isOnCooldown: true);
        var fallbackAbility = CreateAbility(id: 2);
        var rotation = CreateRotation(primaryId: 1, fallbackId: 2);
        var boss = CreateBoss(abilities: new[] { primaryAbility, fallbackAbility });
        
        // Act
        var selected = await _service.SelectNextAbilityInRotationAsync(boss, rotation, state);
        
        // Assert
        Assert.AreEqual(fallbackAbility, selected);
    }
}

[TestClass]
public class AddManagementServiceTests
{
    [TestMethod]
    public async Task ShouldSummonAdds_HPThreshold_ReturnsTrue()
    {
        // Arrange
        var boss = CreateBoss(hp: 650, maxHP: 1000);  // 65% HP
        var config = CreateAddConfig(summonAtHPThreshold: 0.66f);
        
        // Act
        var should = _service.ShouldSummonAdds(boss, config, state);
        
        // Assert
        Assert.IsTrue(should);
    }
    
    [TestMethod]
    public async Task SummonAdds_CreatesCorrectCount()
    {
        // Arrange
        var boss = CreateBoss();
        var config = CreateAddConfig(addCount: 3);
        
        // Act
        await _service.SummonAddsAsync(boss, config, state);
        
        // Assert
        var summonedAdds = state.Enemies.Count(e => e.IsSummonedBy == boss.BossId);
        Assert.AreEqual(3, summonedAdds);
    }
}

[TestClass]
public class AdaptiveDifficultyServiceTests
{
    [TestMethod]
    public void AnalyzePlayerStrategy_PlayerCamping_SetsCampingFlag()
    {
        // Arrange: Player hasn't moved in 5 turns
        var state = CreateBattlefieldState(playerPositions: new[] { (0,0), (0,0), (0,0), (0,0), (0,0) });
        
        // Act
        var strategy = _service.AnalyzePlayerStrategyAsync(state);
        
        // Assert
        Assert.IsTrue(strategy.IsCamping);
    }
    
    [TestMethod]
    public async Task ApplyCounterStrategies_PlayerCamping_UsesAOE()
    {
        // Arrange
        var aoeAbility = CreateAbility(isAOE: true);
        var boss = CreateBoss(abilities: new[] { aoeAbility });
        var action = new BossAction();
        var strategy = new PlayerStrategy { IsCamping = true };
        
        // Act
        await _service.ApplyCounterStrategiesAsync(boss, action, strategy, state);
        
        // Assert
        Assert.AreEqual(aoeAbility, action.SelectedAbility);
    }
}
```

---

## V. Success Criteria

**v0.42.3 is DONE when:**

- [ ]  Phase system triggers at correct HP thresholds
- [ ]  Phase transitions execute properly (dialogue, abilities, buffs)
- [ ]  Ability rotations function correctly
- [ ]  Add summoning works per configuration
- [ ]  Boss and adds coordinate targeting
- [ ]  Adaptive difficulty recognizes player strategies
- [ ]  Boss applies counter-strategies
- [ ]  Database schema created and seeded
- [ ]  80%+ unit test coverage
- [ ]  Boss encounters feel memorable in playtests

---

**Ready to create intelligent, memorable boss encounters.**