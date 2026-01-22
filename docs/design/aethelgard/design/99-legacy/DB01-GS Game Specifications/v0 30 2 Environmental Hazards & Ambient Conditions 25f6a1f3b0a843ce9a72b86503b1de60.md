# v0.30.2: Environmental Hazards & Ambient Conditions

Type: Technical
Description: Implements [Frigid Cold] ambient condition (+50% Ice vulnerability, critical slow), [Slippery Terrain] (70% coverage, FINESSE DC 12), [Brittle] debuff, and 9 environmental hazards.
Priority: Must-Have
Status: Implemented
Target Version: Alpha
Dependencies: v0.30.1 (Database Schema)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.30: Niflheim Biome Implementation (v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.30.2-ENVIRONMENTAL

**Parent Specification:** v0.30 Niflheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.30.1 (Database complete), v0.29.2 (Muspelheim hazards as reference)

---

## ✅ IMPLEMENTATION COMPLETE (2025-11-16)

**Status:** Implementation complete, all deliverables verified

**Actual Time:** ~8 hours

**File:** `Data/v0.30.2_environmental_hazards.sql`

**Conditions Created:**

- ✅ **[Frigid Cold]** (condition_id: 105): +50% Ice vulnerability, critical slow, +5 Psychic Stress
- ✅ **[Brittle]** (condition_id: 106): +50% Physical vulnerability

**9 Environmental Hazards Implemented:**

1. ✅ **Slippery Terrain** (70% coverage) - FINESSE DC 12, knockdown risk
2. ✅ **Unstable Ceiling (Icicles)** - 2d8 Ice AoE, destructible
3. ✅ **Frozen Machinery** - +4 Defense cover, 50 HP
4. ✅ **Ice Boulders** - Blocking obstacles, 30 HP
5. ✅ **Cryo-Vent** - 3d6 Ice periodic damage (1 on/2 off cycles)
6. ✅ **Brittle Ice Bridge** - STURDINESS DC 10, weight limit
7. ✅ **Frozen Corpse** - Interactive, WITS DC 15 search
8. ✅ **Cryogenic Fog** - Visibility reduction to 3 tiles
9. ✅ **Flash-Frozen Terminal** - Fire thaw + data extraction

**Integration Complete:**

- ✅ ConditionService integration for [Frigid Cold] and [Brittle]
- ✅ Environmental hazard mechanics defined
- ✅ v5.0 compliance (cryogenic system failures, not magic)

---

## I. Overview

This specification defines environmental hazards and ambient conditions for the Niflheim biome, including [Frigid Cold] ambient condition, [Slippery Terrain] mechanics, 8+ hazard types, terrain features, and integration with existing combat and movement systems.

### Core Deliverables

- **[Frigid Cold]** ambient condition (universal Ice vulnerability)
- **[Slippery Terrain]** dominant floor type (knockdown risk)
- **8+ Hazard Types** with mechanical definitions
- **Brittleness mechanic** (Ice → [Brittle] → Physical vulnerability)
- **Integration** with ConditionService, MovementService, CombatService
- **Serilog structured logging** for all hazard interactions

---

## II. Ambient Condition: [Frigid Cold]

### A. Mechanical Definition

**[Frigid Cold]** is a biome-wide ambient condition that affects all combatants in Niflheim.

**Core Effects:**

1. **Universal Ice Vulnerability:** All characters are Vulnerable to Ice damage (take +50% Ice damage)
2. **Critical Hit Slow:** When critically hit by any attack while in [Frigid Cold], apply [Slowed] status for 2 turns
3. **Psychic Stress Accumulation:** +5 Psychic Stress per combat encounter (environmental anxiety)

**v5.0 Narrative Frame:**

- "Your thermal regulation system is failing. The cold bypasses all insulation."
- "Critical system alert: Heat loss exceeding safe parameters."
- "Extremity response slowed. Motor function compromised by cryogenic exposure."

### B. Database Schema

**Conditions Table Entry:**

```sql
INSERT INTO Conditions (condition_id, condition_name, condition_type, description, is_ambient)
VALUES (
    105, -- condition_id for [Frigid Cold]
    'Frigid Cold',
    'Environmental',
    'Biome-wide cryogenic exposure. All combatants are Vulnerable to Ice damage (+50%). Critical hits inflict [Slowed] for 2 turns. The absolute zero temperature bypasses all standard thermal protection.',
    1 -- is_ambient flag
);
```

**Condition_Effects Table:**

```sql
-- Effect 1: Universal Ice Vulnerability
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'Damage_Vulnerability',
    50, -- +50% Ice damage taken
    'Ice vulnerability: All characters take +50% damage from Ice attacks.'
);

-- Effect 2: Critical Hit Slow
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'On_Critical_Hit',
    0, -- No numeric value, triggers status effect
    'When critically hit, apply [Slowed] status for 2 turns.'
);

-- Effect 3: Stress Accumulation
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'Psychic_Stress',
    5, -- +5 Stress per combat
    'Environmental anxiety from extreme cold: +5 Psychic Stress per encounter.'
);
```

**Update Biomes Table:**

```sql
UPDATE Biomes 
SET ambient_condition_id = 105 
WHERE biome_id = 5;
```

### C. Service Implementation

**FrigidColdService.cs:**

```csharp
public class FrigidColdService
{
    private readonly ILogger<FrigidColdService> _logger;
    private readonly ConditionService _conditionService;
    private readonly CombatService _combatService;
    
    public FrigidColdService(
        ILogger<FrigidColdService> logger,
        ConditionService conditionService,
        CombatService combatService)
    {
        _logger = logger;
        _conditionService = conditionService;
        _combatService = combatService;
    }
    
    /// <summary>
    /// Apply Frigid Cold ambient condition to all combatants in Niflheim.
    /// Called at combat start.
    /// </summary>
    public void ApplyFrigidCold(CombatInstance combat)
    {
        if (combat.Biome?.BiomeId != 5) return; // Not Niflheim
        
        _logger.Information("Applying [Frigid Cold] ambient condition to combat {CombatId}",
            combat.CombatId);
        
        // Apply to all combatants
        foreach (var combatant in combat.AllCombatants)
        {
            ApplyFrigidColdToCombatant(combatant);
        }
    }
    
    private void ApplyFrigidColdToCombatant(Combatant combatant)
    {
        // Apply Ice Vulnerability
        combatant.DamageVulnerabilities["Ice"] = 50; // +50% Ice damage
        
        _logger.Debug("Applied [Frigid Cold] to {CombatantName}: Ice Vulnerability +50%",
            [combatant.Name](http://combatant.Name));
    }
    
    /// <summary>
    /// Process critical hit slow effect.
    /// Called after critical hit damage is applied.
    /// </summary>
    public void ProcessCriticalHitSlow(Combatant target, AttackResult attackResult)
    {
        if (!attackResult.IsCriticalHit) return;
        if (target.CurrentBiome?.BiomeId != 5) return; // Not in Niflheim
        
        _logger.Information("Critical hit in [Frigid Cold]: Applying [Slowed] to {Target}",
            [target.Name](http://target.Name));
        
        _conditionService.ApplyCondition(target, "Slowed", duration: 2);
    }
    
    /// <summary>
    /// Apply Psychic Stress from extreme cold exposure.
    /// Called at combat end.
    /// </summary>
    public void ApplyEnvironmentalStress(Character character, CombatInstance combat)
    {
        if (combat.Biome?.BiomeId != 5) return;
        
        const int stressAmount = 5;
        character.PsychicStress += stressAmount;
        
        _logger.Information("[Frigid Cold] environmental stress: {Character} +{Stress} Psychic Stress",
            [character.Name](http://character.Name), stressAmount);
    }
}
```

---

## III. Terrain Type: [Slippery Terrain]

### A. Mechanical Definition

**[Slippery Terrain]** is the dominant floor type in Niflheim (60-80% of all tiles).

**Core Effects:**

1. **Movement Risk:** Moving through slippery tiles requires FINESSE check (DC 12)
2. **Knockdown on Failure:** Failed check = [Knocked Down] status
3. **Fall Damage:** [Knocked Down] on ice takes 1d4 Physical damage
4. **Forced Movement Amplification:** Being pushed/pulled on ice adds +1 tile distance
5. **Knockdown Immunity Bypass:** Characters immune to [Knocked Down] ignore this terrain

**v5.0 Narrative Frame:**

- "The floor is a sheet of ice. Every step risks a fall."
- "Your boots struggle for purchase on the frozen surface."
- "You slip on the ice and crash hard to the ground."

### B. Database Schema

**Biome_EnvironmentalFeatures Table:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, 
    dc_value, damage_dice, damage_type, description
)
VALUES (
    5,
    'Slippery Terrain',
    'Terrain',
    4.0, -- Very high spawn weight, dominant floor type
    12, -- FINESSE DC
    '1d4', -- Fall damage
    'Physical',
    'Ice sheet covering the floor. Moving requires FINESSE check (DC 12) or risk [Knocked Down] with 1d4 Physical damage. Forced movement effects gain +1 tile distance. Characters immune to [Knocked Down] ignore this terrain.'
);
```

### C. Service Implementation

**SlipperyTerrainService.cs:**

```csharp
public class SlipperyTerrainService
{
    private readonly ILogger<SlipperyTerrainService> _logger;
    private readonly DiceService _diceService;
    private readonly ConditionService _conditionService;
    
    private const int SLIPPERY_TERRAIN_DC = 12;
    private const string FALL_DAMAGE_DICE = "1d4";
    
    /// <summary>
    /// Process movement through slippery terrain.
    /// Returns true if movement succeeds, false if knocked down.
    /// </summary>
    public bool ProcessSlipperyMovement(
        Combatant combatant, 
        RoomTile fromTile, 
        RoomTile toTile)
    {
        // Check if target tile is slippery
        if (!toTile.HasFeature("Slippery Terrain")) return true;
        
        // Characters immune to knockdown bypass slippery terrain
        if (combatant.HasEffect("Knockdown_Immunity"))
        {
            _logger.Debug("{Combatant} immune to knockdown, bypasses slippery terrain",
                [combatant.Name](http://combatant.Name));
            return true;
        }
        
        // FINESSE check to avoid knockdown
        int finesses = combatant.GetAttributeValue("FINESSE");
        int roll = _diceService.Roll(1, 20);
        int total = roll + finesse;
        
        bool success = total >= SLIPPERY_TERRAIN_DC;
        
        _logger.Information(
            "Slippery terrain check: {Combatant} rolled {Roll}+{Finesse}={Total} vs DC {DC}: {Result}",
            [combatant.Name](http://combatant.Name), roll, finesse, total, SLIPPERY_TERRAIN_DC,
            success ? "SUCCESS" : "KNOCKED DOWN");
        
        if (!success)
        {
            // Apply knockdown and fall damage
            _conditionService.ApplyCondition(combatant, "Knocked Down", duration: 1);
            
            int fallDamage = _diceService.RollDice(FALL_DAMAGE_DICE);
            combatant.TakeDamage(fallDamage, "Physical");
            
            _logger.Information(
                "{Combatant} slipped on ice: [Knocked Down] + {Damage} Physical damage",
                [combatant.Name](http://combatant.Name), fallDamage);
        }
        
        return success;
    }
    
    /// <summary>
    /// Amplify forced movement distance on slippery terrain.
    /// </summary>
    public int AmplifyForcedMovement(RoomTile targetTile, int baseDistance)
    {
        if (!targetTile.HasFeature("Slippery Terrain")) return baseDistance;
        
        int amplifiedDistance = baseDistance + 1;
        
        _logger.Debug("Slippery terrain amplifies forced movement: {Base} → {Amplified}",
            baseDistance, amplifiedDistance);
        
        return amplifiedDistance;
    }
}
```

---

## IV. Environmental Hazards (8 Types)

### 1. [Unstable Ceiling] (Icicle Hazards)

**Mechanical Definition:**

- Ceiling-mounted icicles can be shattered by attacks or explosions
- When triggered, deals 2d8 Ice damage to all combatants in 2-tile radius
- Can be used tactically (shoot icicles to trigger)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, aoe_radius, description
)
VALUES (
    5,
    'Unstable Ceiling (Icicles)',
    'Dynamic_Hazard',
    1.5,
    '2d8',
    'Ice',
    2, -- 2-tile radius
    'Massive icicles hanging from ceiling. Can be shattered by attacks or explosions. Deals 2d8 Ice damage to all in 2-tile radius. Tactical opportunity for area damage.'
);
```

### 2. [Frozen Machinery] (Cover)

**Mechanical Definition:**

- Excellent cover (+4 Defense)
- Blocks line of sight
- Destructible (50 HP)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    defense_bonus, hp_value, description
)
VALUES (
    5,
    'Frozen Machinery',
    'Cover',
    2.0, -- Common
    4, -- +4 Defense
    50, -- 50 HP
    'Pre-Glitch equipment encased in ice. Provides excellent cover (+4 Defense). Blocks line of sight. Destructible (50 HP).'
);
```

### 3. [Ice Boulders] (Destructible Obstacles)

**Mechanical Definition:**

- Blocks movement and line of sight
- Destructible (30 HP)
- When destroyed, creates slippery terrain in adjacent tiles

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    hp_value, description
)
VALUES (
    5,
    'Ice Boulders',
    'Destructible_Obstacle',
    1.2,
    30,
    'Large blocks of solid ice. Blocks movement and line of sight. Destructible (30 HP). When destroyed, creates [Slippery Terrain] in adjacent tiles.'
);
```

### 4. [Cryo-Vent] (Liquid Nitrogen Jets)

**Mechanical Definition:**

- Intermittent jets of liquid nitrogen
- Active 1 turn, inactive 2 turns (33% uptime)
- When active: 3d6 Ice damage to any combatant entering or starting turn in tile

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    5,
    'Cryo-Vent',
    'Dynamic_Hazard',
    1.0,
    '3d6',
    'Ice',
    'Ruptured coolant line sprays liquid nitrogen jets. Active 1 turn, inactive 2 turns. Deals 3d6 Ice damage to anyone entering or starting turn in spray area. Timing pattern is predictable.'
);
```

### 5. [Brittle Ice Bridge] (Conditional Chasm)

**Mechanical Definition:**

- Crossing requires STURDINESS check (DC 10)
- Weight limit: 2 combatants maximum
- Failure = fall into chasm below (instant death or major damage)
- Success = bridge remains passable

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    dc_value, description
)
VALUES (
    5,
    'Brittle Ice Bridge',
    'Conditional_Obstacle',
    0.6,
    10, -- STURDINESS DC
    'Thin ice spanning a chasm. Crossing requires STURDINESS check (DC 10). Weight limit: 2 combatants. Failure = fall into chasm (instant death or 10d10 damage if bottom visible). Success = bridge holds.'
);
```

### 6. [Frozen Corpse] (Storytelling Feature)

**Mechanical Definition:**

- No mechanical effect
- Can be searched (WITS check DC 15) for resources
- Environmental storytelling (Pre-Glitch victims)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    dc_value, description
)
VALUES (
    5,
    'Frozen Corpse',
    'Interactive_Object',
    1.5,
    15, -- WITS DC to search
    'Pre-Glitch victim perfectly preserved in ice. No mechanical threat. Can be searched (WITS DC 15) for resources. Environmental storytelling: frozen at moment of death, expressions of panic preserved.'
);
```

### 7. [Cryogenic Fog] (Visibility Reduction)

**Mechanical Definition:**

- Reduces visibility to 3 tiles
- Ranged attacks beyond 3 tiles have Disadvantage
- Does not affect melee or adjacent attacks

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    description
)
VALUES (
    5,
    'Cryogenic Fog',
    'Ambient_Effect',
    0.8,
    'Dense fog from sublimating ice. Reduces visibility to 3 tiles. Ranged attacks beyond 3 tiles have Disadvantage. Melee and adjacent attacks unaffected.'
);
```

### 8. [Flash-Frozen Terminal] (Interactive Object)

**Mechanical Definition:**

- Can be thawed with Fire damage (10+ Fire damage)
- Thawed terminal reveals data (WITS check DC 14)
- Success = gain Cryogenic Data-Slate resource

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    dc_value, description
)
VALUES (
    5,
    'Flash-Frozen Terminal',
    'Interactive_Object',
    0.7,
    14, -- WITS DC to extract data
    'Pre-Glitch data terminal frozen solid. Must be thawed with Fire damage (10+). Once thawed, WITS check (DC 14) to extract data. Success = gain Cryogenic Data-Slate resource (Tier 4).'
);
```

---

## V. Brittleness Mechanic Integration

### Concept

**Inverse of Muspelheim:** Ice-resistant enemies become vulnerable to Physical damage after Ice attack.

**Mechanic:**

1. Enemy has Ice Resistance (50-75%)
2. Hit with Ice damage → Apply [Brittle] debuff (1 turn)
3. While [Brittle]: Vulnerable to Physical damage (+50%)

### Database Schema

**[Brittle] Condition:**

```sql
INSERT INTO Conditions (condition_id, condition_name, condition_type, description)
VALUES (
    106,
    'Brittle',
    'Debuff',
    'Target has been supercooled by Ice damage. Physical attacks exploit structural weakness. Vulnerable to Physical damage (+50%) for 1 turn.'
);

INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    106,
    'Damage_Vulnerability',
    50,
    'Physical vulnerability: Take +50% Physical damage.'
);
```

### Service Implementation

**BrittlenessService.cs:**

```csharp
public class BrittlenessService
{
    private readonly ILogger<BrittlenessService> _logger;
    private readonly ConditionService _conditionService;
    
    /// <summary>
    /// Check if Ice damage should apply [Brittle] debuff.
    /// Only applies to enemies with Ice Resistance.
    /// </summary>
    public void ProcessBrittlenessCheck(Combatant target, AttackResult result)
    {
        // Only apply to Ice damage
        if (result.DamageType != "Ice") return;
        
        // Only apply to Ice-resistant targets
        if (!target.HasResistance("Ice")) return;
        
        // Apply [Brittle] debuff
        _conditionService.ApplyCondition(target, "Brittle", duration: 1);
        
        _logger.Information(
            "Ice attack on Ice-resistant {Target}: Applied [Brittle] (Physical Vuln +50%)",
            [target.Name](http://target.Name));
    }
}
```

---

## VI. Integration Points

### A. ConditionService Integration

**Apply [Frigid Cold] at combat start:**

```csharp
public void InitializeCombat(CombatInstance combat)
{
    // Existing initialization...
    
    // Apply biome ambient condition
    if (combat.Biome?.AmbientConditionId != null)
    {
        ApplyAmbientCondition(combat, combat.Biome.AmbientConditionId);
    }
}
```

### B. MovementService Integration

**Check slippery terrain on movement:**

```csharp
public bool TryMove(Combatant combatant, RoomTile targetTile)
{
    // Existing passability checks...
    
    // Check slippery terrain
    if (targetTile.HasFeature("Slippery Terrain"))
    {
        bool success = _slipperyTerrainService.ProcessSlipperyMovement(
            combatant, combatant.CurrentTile, targetTile);
        
        if (!success)
        {
            // Knocked down, movement fails
            return false;
        }
    }
    
    // Complete movement...
}
```

### C. CombatService Integration

**Apply brittleness on Ice damage:**

```csharp
public AttackResult ProcessAttack(Combatant attacker, Combatant target, Ability ability)
{
    var result = CalculateAttack(attacker, target, ability);
    
    // Apply damage...
    
    // Check for brittleness (Ice damage on Ice-resistant targets)
    if (result.DamageType == "Ice")
    {
        _brittlenessService.ProcessBrittlenessCheck(target, result);
    }
    
    // Check for critical hit slow (Frigid Cold)
    if (result.IsCriticalHit && target.CurrentBiome?.BiomeId == 5)
    {
        _frigidColdService.ProcessCriticalHitSlow(target, result);
    }
    
    return result;
}
```

---

## VII. Testing Requirements

### Unit Tests

**Test 1: Frigid Cold Application**

```csharp
[Fact]
public void ApplyFrigidCold_AppliesIceVulnerabilityToAllCombatants()
{
    var combat = CreateNiflheimCombat();
    _frigidColdService.ApplyFrigidCold(combat);
    
    foreach (var combatant in combat.AllCombatants)
    {
        Assert.Equal(50, combatant.DamageVulnerabilities["Ice"]);
    }
}
```

**Test 2: Slippery Terrain Movement**

```csharp
[Fact]
public void ProcessSlipperyMovement_FailedCheck_AppliesKnockdownAndDamage()
{
    var combatant = CreateTestCombatant(finesse: 8);
    var tile = CreateSlipperyTile();
    
    _diceService.SetNextRoll(1); // Ensure failure
    
    bool success = _slipperyTerrainService.ProcessSlipperyMovement(
        combatant, combatant.CurrentTile, tile);
    
    Assert.False(success);
    Assert.True(combatant.HasCondition("Knocked Down"));
    Assert.True(combatant.CurrentHP < combatant.MaxHP); // Took fall damage
}
```

**Test 3: Brittleness Application**

```csharp
[Fact]
public void ProcessBrittlenessCheck_IceDamageOnIceResistantTarget_AppliesBrittle()
{
    var target = CreateIceResistantEnemy();
    var result = new AttackResult { DamageType = "Ice", DamageDone = 20 };
    
    _brittlenessService.ProcessBrittlenessCheck(target, result);
    
    Assert.True(target.HasCondition("Brittle"));
    Assert.Equal(50, target.DamageVulnerabilities["Physical"]);
}
```

**Test 4: Critical Hit Slow**

```csharp
[Fact]
public void ProcessCriticalHitSlow_InFrigidCold_AppliesSlowed()
{
    var target = CreateTestCombatant();
    target.CurrentBiome = CreateNiflheimBiome();
    var result = new AttackResult { IsCriticalHit = true };
    
    _frigidColdService.ProcessCriticalHitSlow(target, result);
    
    Assert.True(target.HasCondition("Slowed"));
    Assert.Equal(2, target.GetConditionDuration("Slowed"));
}
```

### Integration Tests

**Test 5: Full Niflheim Combat Scenario**

```csharp
[Fact]
public void NiflheimCombat_FullScenario_AllMechanicsWork()
{
    // Arrange
    var combat = CreateNiflheimCombat();
    var player = combat.PlayerParty[0];
    var enemy = combat.Enemies[0]; // Ice-resistant enemy
    
    // Act: Start combat (applies Frigid Cold)
    _combatService.InitializeCombat(combat);
    
    // Assert: Frigid Cold applied
    Assert.Equal(50, player.DamageVulnerabilities["Ice"]);
    
    // Act: Player uses Ice ability on Ice-resistant enemy
    var iceAttack = _combatService.ProcessAttack(player, enemy, iceAbility);
    
    // Assert: Brittle applied
    Assert.True(enemy.HasCondition("Brittle"));
    
    // Act: Player follows up with Physical attack
    var physicalAttack = _combatService.ProcessAttack(player, enemy, physicalAbility);
    
    // Assert: Increased damage from Brittle
    Assert.True(physicalAttack.DamageDone > physicalAbility.BaseDamage);
    
    // Act: Enemy critical hits player
    var critAttack = _combatService.ProcessAttack(enemy, player, enemyAbility);
    critAttack.IsCriticalHit = true;
    
    // Assert: Slowed applied
    Assert.True(player.HasCondition("Slowed"));
}
```

---

## VIII. Success Criteria

### Functional Requirements

- [ ]  [Frigid Cold] applies to all combatants in Niflheim
- [ ]  Ice vulnerability +50% calculated correctly
- [ ]  Critical hits apply [Slowed] in Frigid Cold
- [ ]  [Slippery Terrain] triggers FINESSE checks
- [ ]  Failed checks apply [Knocked Down] + fall damage
- [ ]  Knockdown immunity bypasses slippery terrain
- [ ]  [Brittle] applies on Ice damage to Ice-resistant targets
- [ ]  [Brittle] grants +50% Physical vulnerability
- [ ]  All 8 hazard types spawn correctly
- [ ]  Icicles trigger area damage when shattered

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass for full combat scenarios
- [ ]  v5.0 voice compliance (technology, not magic)
- [ ]  Performance acceptable (< 100ms per hazard check)

### Balance Validation

- [ ]  Slippery terrain feels dangerous but not unfair
- [ ]  Brittleness combo is discoverable and rewarding
- [ ]  Fire damage provides meaningful advantage
- [ ]  Knockdown immunity characters feel valuable
- [ ]  Hazards create tactical opportunities, not frustration

---

## IX. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.30.2_environmental_hazards.sql
```

### Step 2: Compile Services

```bash
dotnet build Services/FrigidColdService.cs
dotnet build Services/SlipperyTerrainService.cs
dotnet build Services/BrittlenessService.cs
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Niflheim.EnvironmentalTests"
```

### Step 4: Run Integration Tests

```bash
dotnet test --filter "FullyQualifiedName~Niflheim.IntegrationTests"
```

### Step 5: Manual Verification

- Start combat in Niflheim
- Verify [Frigid Cold] displays in combat log
- Move through slippery terrain
- Verify FINESSE checks and knockdown mechanics
- Use Ice ability on Ice-resistant enemy
- Verify [Brittle] application
- Follow up with Physical attack
- Verify increased damage

---

## X. Next Steps

Once v0.30.2 is complete:

**Proceed to v0.30.3:** Enemy Definitions & Spawn System

- 5 enemy type definitions
- Spawn weight system
- Ice Resistance + Fire Vulnerability patterns
- Frost-Giant boss framework

---

## XI. Related Documents

**Parent Specification:**

- v0.30: Niflheim Biome Implementation[[1]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)

**Reference Implementation:**

- v0.29.2: Muspelheim Environmental Hazards[[2]](v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md)

**Prerequisites:**

- v0.30.1: Database Schema[[1]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
- v0.20: Tactical Grid System[[3]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)
- v0.22: Environmental Combat[[4]](https://www.notion.so/v0-22-Environmental-Combat-System-f2f10fecac364272a084cd0655b10998?pvs=21)

---

**Environmental systems complete. Proceed to enemy definitions (v0.30.3).**