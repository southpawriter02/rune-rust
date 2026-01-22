# v0.42.2: Ability Usage & Behavior Patterns

Type: Mechanic
Description: Ability prioritization system and implementation of 8 distinct AI behavior archetypes. Delivers ability scoring algorithm (damage vs utility vs cost), 8 fully implemented AI archetypes (Aggressive, Defensive, Cautious, Reckless, Tactical, Support, Control, Ambusher), cooldown and resource management.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.42.1 (Tactical Decision-Making)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.42: Enemy AI Improvements & Behavior Polish (v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Spec:** [v0.42: Enemy AI Improvements & Behavior Polish](v0%2042%20Enemy%20AI%20Improvements%20&%20Behavior%20Polish%206f38ca58428f4c77a98cfbea08a5542a.md)

**Status:** Design Phase

**Timeline:** 5-8 hours (1 week part-time)

**Focus:** Ability prioritization system and implementation of 8 distinct AI behavior archetypes

---

## I. Overview

v0.42.2 implements **ability prioritization** and **behavior pattern** systems that enable enemies to choose optimal abilities and exhibit distinct personalities through their AI archetype.

**What This Delivers:**

- Ability scoring algorithm (damage vs utility vs cost)
- 8 fully implemented AI behavior archetypes
- Cooldown and resource management
- Archetype assignment system
- Boss ability rotation framework

**Success Metric:**

Enemies use abilities intelligently (not randomly) and each archetype feels distinct in gameplay. Players can identify enemy behavior patterns and adapt their strategy accordingly.

---

## II. Functional Requirements

### FR1: Ability Prioritization System

**Requirement:**

Implement a scoring system that evaluates each available ability and selects the most appropriate one for the current situation.

**Ability Score Formula:**

```jsx
AbilityScore = (Damage × 0.4) + (Utility × 0.3) + (Efficiency × 0.2) + (Situation × 0.1)

Where:
- Damage: Expected damage output (0-100)
- Utility: Non-damage value (CC, buffs, debuffs) (0-50)
- Efficiency: Resource cost vs benefit ratio (0-30)
- Situation: Contextual value for current battlefield state (0-20)
```

**C# Implementation:**

```csharp
public class AbilityPrioritizationService : IAbilityPrioritizationService
{
    private readonly ILogger<AbilityPrioritizationService> _logger;
    private readonly IBehaviorPatternService _behaviorService;

    public async Task<Ability> SelectOptimalAbilityAsync(
        Enemy enemy,
        Character target,
        BattlefieldState state)
    {
        var availableAbilities = await GetAvailableAbilitiesAsync(enemy);
        
        if (!availableAbilities.Any())
        {
            _logger.Warning("Enemy {EnemyId} has no available abilities", enemy.EnemyId);
            return enemy.BasicAttack;
        }
        
        var scores = new Dictionary<Ability, float>();
        var archetype = await _behaviorService.GetArchetypeAsync(enemy);
        
        foreach (var ability in availableAbilities)
        {
            var score = await ScoreAbilityAsync(ability, enemy, target, state, archetype);
            scores[ability] = score;
        }
        
        var selected = scores.OrderByDescending(kvp => kvp.Value).First();
        
        _logger.Information(
            "Ability selected: {EnemyId} ({Archetype}) uses {AbilityName} on {Target} (score={Score})",
            enemy.EnemyId, archetype, [selected.Key.Name](http://selected.Key.Name), [target.Name](http://target.Name), selected.Value);
        
        return selected.Key;
    }
    
    private async Task<float> ScoreAbilityAsync(
        Ability ability,
        Enemy enemy,
        Character target,
        BattlefieldState state,
        AIArchetype archetype)
    {
        float damageScore = CalculateDamageScore(ability, target);
        float utilityScore = CalculateUtilityScore(ability, target, state);
        float efficiencyScore = CalculateEfficiencyScore(ability, enemy);
        float situationScore = CalculateSituationScore(ability, state);
        
        // Apply archetype modifiers
        var archetypeModifiers = await GetArchetypeModifiersAsync(archetype);
        
        float totalScore = 
            (damageScore * archetypeModifiers.DamageWeight * 0.4f) +
            (utilityScore * archetypeModifiers.UtilityWeight * 0.3f) +
            (efficiencyScore * 0.2f) +
            (situationScore * 0.1f);
        
        return totalScore;
    }
}
```

---

### FR2: Behavior Pattern Implementation

**Requirement:**

Implement the 8 AI archetypes as distinct behavior patterns that modify decision-making.

**C# Implementation:**

```csharp
public class BehaviorPatternService : IBehaviorPatternService
{
    private readonly IAIConfigurationRepository _configRepo;
    private readonly ILogger<BehaviorPatternService> _logger;

    public async Task<AIArchetype> GetArchetypeAsync(Enemy enemy)
    {
        return enemy.AIArchetype;
    }

    public async Task ApplyArchetypeBehaviorAsync(Enemy enemy, EnemyAction action, BattlefieldState state)
    {
        var archetype = enemy.AIArchetype;
        
        switch (archetype)
        {
            case AIArchetype.Aggressive:
                await ApplyAggressiveBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Defensive:
                await ApplyDefensiveBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Cautious:
                await ApplyCautiousBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Reckless:
                await ApplyRecklessBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Tactical:
                await ApplyTacticalBehaviorAsync(enemy, action, state);
                break;
            case [AIArchetype.Support](http://AIArchetype.Support):
                await ApplySupportBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Control:
                await ApplyControlBehaviorAsync(enemy, action, state);
                break;
            case AIArchetype.Ambusher:
                await ApplyAmbusherBehaviorAsync(enemy, action, state);
                break;
        }
    }
}
```

**Note:** Full behavior implementations are defined in the parent v0.42 spec.

---

## III. Database Schema

```sql
-- AI archetype configuration
CREATE TABLE AIArchetypeConfiguration (
    ArchetypeId INT PRIMARY KEY,
    ArchetypeName VARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    DamageAbilityModifier DECIMAL(3,2) NOT NULL,
    UtilityAbilityModifier DECIMAL(3,2) NOT NULL,
    DefensiveAbilityModifier DECIMAL(3,2) NOT NULL,
    AggressionLevel INT NOT NULL,
    RetreatThresholdHP DECIMAL(3,2),
    PreferredRange VARCHAR(20),
    UsesCoordination BIT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Enemy archetype assignment
CREATE TABLE EnemyArchetypeAssignment (
    EnemyTypeId INT PRIMARY KEY,
    EnemyTypeName VARCHAR(100) NOT NULL,
    AssignedArchetype INT NOT NULL,
    OverrideRetreatThreshold DECIMAL(3,2),
    OverrideAggressionLevel INT,
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (AssignedArchetype) REFERENCES AIArchetypeConfiguration(ArchetypeId)
);

-- Ability usage log
CREATE TABLE AIAbilityUsageLog (
    LogId BIGINT PRIMARY KEY IDENTITY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL,
    CombatEncounterId UNIQUEIDENTIFIER NOT NULL,
    EnemyId UNIQUEIDENTIFIER NOT NULL,
    AbilityId INT NOT NULL,
    TargetId UNIQUEIDENTIFIER NOT NULL,
    AbilityScore DECIMAL(10,2) NOT NULL,
    DamageScore DECIMAL(10,2) NOT NULL,
    UtilityScore DECIMAL(10,2) NOT NULL,
    EfficiencyScore DECIMAL(10,2) NOT NULL,
    SituationScore DECIMAL(10,2) NOT NULL,
    ArchetypeModifier DECIMAL(3,2) NOT NULL,
    WasSuccessful BIT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_AbilityLog_Combat (CombatEncounterId),
    INDEX IX_AbilityLog_Enemy (EnemyId)
);
```

**Seed Data:**

```sql
INSERT INTO AIArchetypeConfiguration 
    (ArchetypeId, ArchetypeName, Description, DamageAbilityModifier, UtilityAbilityModifier, DefensiveAbilityModifier, AggressionLevel, RetreatThresholdHP, PreferredRange, UsesCoordination)
VALUES
    (1, 'Aggressive', 'Rushes enemies, prioritizes damage', 1.40, 0.50, 0.30, 5, NULL, 'Melee', 0),
    (2, 'Defensive', 'Protects allies, uses defensive abilities', 0.70, 1.00, 1.50, 2, 0.30, 'Medium', 1),
    (3, 'Cautious', 'Retreats when low, uses cover', 1.00, 1.00, 1.20, 3, 0.50, 'Long', 0),
    (4, 'Reckless', 'Charges in, ignores danger', 1.60, 0.40, 0.20, 5, NULL, 'Melee', 0),
    (5, 'Tactical', 'Balanced, coordinates with allies', 1.00, 1.10, 1.00, 3, 0.25, 'Medium', 1),
    (6, 'Support', 'Heals allies, stays at range', 0.30, 2.00, 1.50, 1, 0.40, 'Long', 1),
    (7, 'Control', 'Uses CC, disables threats', 0.60, 1.80, 0.80, 3, 0.35, 'Long', 1),
    (8, 'Ambusher', 'Waits for opportunity, burst damage', 1.50, 0.70, 0.60, 4, 0.40, 'Medium', 0);

-- Example enemy assignments
INSERT INTO EnemyArchetypeAssignment (EnemyTypeId, EnemyTypeName, AssignedArchetype)
VALUES
    (1, 'Skar-Horde Berserker', 1),
    (2, 'Rusted Warden', 2),
    (3, 'Undying Scout', 3),
    (4, 'Corrupted Thrall', 4),
    (5, 'Forge-Master', 5),
    (6, 'Bone-Mender', 6),
    (7, 'Frost-Weaver', 7),
    (8, 'Rust-Stalker', 8);
```

---

## IV. Testing Requirements

```csharp
[TestClass]
public class AbilityPrioritizationServiceTests
{
    [TestMethod]
    public async Task SelectAbility_AggressiveArchetype_FavorsDamage()
    {
        // Arrange
        var highDamageAbility = CreateAbility(damage: 50, utility: 0);
        var lowDamageAbility = CreateAbility(damage: 10, utility: 30);
        var aggressiveEnemy = CreateEnemy(AIArchetype.Aggressive, abilities: new[] { highDamageAbility, lowDamageAbility });
        
        // Act
        var selected = await _service.SelectOptimalAbilityAsync(aggressiveEnemy, target, state);
        
        // Assert
        Assert.AreEqual(highDamageAbility, selected);
    }
    
    [TestMethod]
    public async Task SelectAbility_SupportArchetype_FavorsHealing()
    {
        // Arrange
        var healAbility = CreateAbility(isHeal: true);
        var damageAbility = CreateAbility(damage: 40);
        var supportEnemy = CreateEnemy([AIArchetype.Support](http://AIArchetype.Support), abilities: new[] { healAbility, damageAbility });
        var injuredAlly = CreateEnemy(hp: 20, maxHP: 100);
        
        // Act
        var selected = await _service.SelectOptimalAbilityAsync(supportEnemy, injuredAlly, state);
        
        // Assert
        Assert.AreEqual(healAbility, selected);
    }
    
    [TestMethod]
    public async Task SelectAbility_OnCooldown_UsesFallback()
    {
        // Arrange
        var primaryAbility = CreateAbility(damage: 50, isOnCooldown: true);
        var fallbackAbility = CreateAbility(damage: 30);
        var enemy = CreateEnemy(abilities: new[] { primaryAbility, fallbackAbility });
        
        // Act
        var selected = await _service.SelectOptimalAbilityAsync(enemy, target, state);
        
        // Assert
        Assert.AreEqual(fallbackAbility, selected);
    }
}
```

---

## V. Success Criteria

**v0.42.2 is DONE when:**

- [ ]  Ability prioritization selects logical abilities
- [ ]  All 8 archetypes implemented and tested
- [ ]  Each archetype exhibits distinct behavior in playtests
- [ ]  Cooldown and resource management functional
- [ ]  Database schema created and seeded
- [ ]  80%+ unit test coverage
- [ ]  Ability selection <15ms per enemy turn
- [ ]  Players can identify archetype behaviors

---

**Ready to implement intelligent ability usage and distinct AI personalities.**