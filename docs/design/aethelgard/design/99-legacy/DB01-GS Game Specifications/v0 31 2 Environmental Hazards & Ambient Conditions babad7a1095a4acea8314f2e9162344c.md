# v0.31.2: Environmental Hazards & Ambient Conditions

Type: Technical
Description: Implements [Runic Instability] Wild Magic Surges, [Psychic Resonance] high-intensity Stress, Reality Tear positional warping, and 8+ environmental hazards.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.31.1 (Database Schema)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.31: Alfheim Biome Implementation (v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.31.2-ENVIRONMENTAL

**Parent Specification:** v0.31 Alfheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.31.1 (Database complete), v0.30.2 (Niflheim hazards as reference)

---

## I. Overview

This specification defines environmental hazards and ambient conditions for the Alfheim biome, including [Runic Instability] ambient condition (Wild Magic Surges), [Psychic Resonance] high-intensity variant, Reality Tear mechanic, 8+ hazard types, and integration with existing Mystic and Trauma Economy systems.

### Core Deliverables

- **[Runic Instability]** ambient condition (Wild Magic Surge system)
- **[Psychic Resonance]** high-intensity variant (extreme Stress)
- **Reality Tear mechanic** (positional warping + Corruption)
- **8+ Hazard Types** with mechanical definitions
- **Integration** with ConditionService, Trauma Economy, Mystic abilities
- **Serilog structured logging** for all hazard interactions

---

## II. Ambient Condition: [Runic Instability]

### A. Mechanical Definition

**[Runic Instability]** is a biome-wide ambient condition that affects all Mystic ability usage in Alfheim.

**Core Effects:**

1. **Wild Magic Surge:** Every Mystic ability use has 25% chance to trigger surge
2. **Surge Effects:** Random modification to spell (damage ±50%, range ±1 tile, targets ±1, duration ±50%)
3. **Amplified Power:** Mystics operating in Alfheim gain +10% base Aether Pool capacity
4. **Psychic Feedback:** Each Wild Magic Surge generates +5 Psychic Stress

**v5.0 Narrative Frame:**

- "Aetheric feedback loop detected. Spell casting parameters unstable."
- "Reality manipulation systems experiencing resonance cascade. Unpredictable effects likely."
- "Your runes flare uncontrollably - the ambient Aetheric field is amplifying and distorting your weaving."

### B. Database Schema

**Conditions Table Entry:**

```sql
INSERT INTO Conditions (condition_id, condition_name, condition_type, description, is_ambient)
VALUES (
    107, -- condition_id for [Runic Instability]
    'Runic Instability',
    'Environmental',
    'Biome-wide Aetheric feedback loop. Mystic abilities have 25% chance to trigger Wild Magic Surge (random modification). Mystics gain +10% Aether Pool capacity but each surge generates +5 Psychic Stress. The ambient energy amplifies power but makes control dangerous.',
    1 -- is_ambient flag
);
```

**Condition_Effects Table:**

```sql
-- Effect 1: Wild Magic Surge Chance
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Wild_Magic_Surge_Chance',
    25, -- 25% chance per Mystic ability use
    'Wild Magic Surge: 25% chance per Mystic ability to trigger random modification (damage, range, targets, or duration ±50%).'
);

-- Effect 2: Aether Pool Amplification
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Aether_Pool_Modifier',
    10, -- +10% Aether Pool
    'Amplified Aether: Mystics gain +10% base Aether Pool capacity in this biome.'
);

-- Effect 3: Psychic Feedback
INSERT INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Psychic_Stress_Per_Surge',
    5, -- +5 Stress per surge
    'Psychic Feedback: Each Wild Magic Surge generates +5 Psychic Stress.'
);
```

**Update Biomes Table:**

```sql
UPDATE Biomes 
SET ambient_condition_id = 107 
WHERE biome_id = 6;
```

### C. Service Implementation

**RunicInstabilityService.cs:**

```csharp
public class RunicInstabilityService
{
    private readonly ILogger<RunicInstabilityService> _logger;
    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaService;
    
    private const double SURGE_CHANCE = 0.25; // 25%
    private const int STRESS_PER_SURGE = 5;
    private const double AETHER_POOL_BONUS = 0.10; // +10%
    
    /// <summary>
    /// Check if Mystic ability triggers Wild Magic Surge.
    /// Returns surge result if triggered, null otherwise.
    /// </summary>
    public WildMagicSurgeResult TryTriggerWildMagicSurge(
        Character caster,
        Ability ability,
        CombatInstance combat)
    {
        // Only applies in Alfheim
        if (combat.Biome?.BiomeId != 6) return null;
        
        // Only affects Mystic abilities
        if (ability.ResourceType != "Aether") return null;
        
        // Roll for surge
        var roll = _diceService.RollPercentage();
        if (roll > SURGE_CHANCE) return null;
        
        _logger.Information(
            "Wild Magic Surge triggered: {Caster} using {Ability} (roll: {Roll})",
            [caster.Name](http://caster.Name), [ability.Name](http://ability.Name), roll);
        
        // Generate surge effect
        var surge = GenerateSurgeEffect(ability);
        
        // Apply Psychic Stress
        _traumaService.ApplyStress(caster, STRESS_PER_SURGE, 
            "Wild Magic Surge psychic feedback");
        
        _logger.Information(
            "Wild Magic Surge: {Effect} on {Ability} (+{Stress} Psychic Stress)",
            surge.EffectDescription, [ability.Name](http://ability.Name), STRESS_PER_SURGE);
        
        return surge;
    }
    
    /// <summary>
    /// Generate random surge effect for ability.
    /// </summary>
    private WildMagicSurgeResult GenerateSurgeEffect(Ability ability)
    {
        var roll = _diceService.Roll(1, 4); // 4 possible surge types
        
        return roll switch
        {
            1 => new WildMagicSurgeResult
            {
                Type = SurgeType.DamageModification,
                Modifier = _diceService.RollPercentage() < 0.5 ? -0.5 : +0.5,
                EffectDescription = _diceService.RollPercentage() < 0.5 
                    ? "Damage reduced by 50%" 
                    : "Damage increased by 50%"
            },
            2 => new WildMagicSurgeResult
            {
                Type = SurgeType.RangeModification,
                Modifier = _diceService.RollPercentage() < 0.5 ? -1 : +1,
                EffectDescription = _diceService.RollPercentage() < 0.5 
                    ? "Range decreased by 1 tile" 
                    : "Range increased by 1 tile"
            },
            3 => new WildMagicSurgeResult
            {
                Type = SurgeType.TargetModification,
                Modifier = _diceService.RollPercentage() < 0.5 ? -1 : +1,
                EffectDescription = _diceService.RollPercentage() < 0.5 
                    ? "Affects 1 fewer target" 
                    : "Affects 1 additional target"
            },
            4 => new WildMagicSurgeResult
            {
                Type = SurgeType.DurationModification,
                Modifier = _diceService.RollPercentage() < 0.5 ? -0.5 : +0.5,
                EffectDescription = _diceService.RollPercentage() < 0.5 
                    ? "Duration reduced by 50%" 
                    : "Duration increased by 50%"
            },
            _ => throw new InvalidOperationException("Invalid surge type roll")
        };
    }
    
    /// <summary>
    /// Apply Aether Pool amplification for Mystics in Alfheim.
    /// </summary>
    public void ApplyAetherPoolAmplification(Character character)
    {
        if (character.Archetype != "Mystic") return;
        
        var amplification = (int)(character.BaseAetherPool * AETHER_POOL_BONUS);
        character.CurrentAetherPool += amplification;
        character.MaxAetherPool += amplification;
        
        _logger.Information(
            "{Character} Aether Pool amplified: +{Bonus} (+10%)",
            [character.Name](http://character.Name), amplification);
    }
}

public class WildMagicSurgeResult
{
    public SurgeType Type { get; set; }
    public double Modifier { get; set; }
    public string EffectDescription { get; set; }
}

public enum SurgeType
{
    DamageModification,
    RangeModification,
    TargetModification,
    DurationModification
}
```

---

## III. Ambient Condition: [Psychic Resonance] (High-Intensity)

### A. Mechanical Definition

**[Psychic Resonance]** in Alfheim is the highest-intensity variant, representing the psychic "screaming" at ground zero of the Great Silence.

**Core Effects:**

1. **Extreme Stress Accumulation:** +10 Psychic Stress per combat encounter (double Helheim's +5)
2. **Passive Stress Generation:** +2 Psychic Stress per turn spent in combat
3. **Enhanced Forlorn Echoes:** Forlorn in Alfheim deal +50% Psychic damage

**v5.0 Narrative Frame:**

- "The psychic resonance is deafening - seven billion silenced minds screaming at once."
- "Your neural buffer is overloading. You re hearing error logs from dying consciousnesses."
- "This is ground zero. The place where thought itself crashed."

### B. Database Schema (Extends Existing)

**Alfheim-Specific Psychic Resonance values are handled in AlfheimService, not separate condition.**

No new database entries needed - intensity is biome-specific in service layer.

### C. Service Implementation

**AlfheimService.cs** (partial - Psychic Resonance handling):

```csharp
public partial class AlfheimService
{
    private const int STRESS_PER_ENCOUNTER = 10; // vs Helheim's 5
    private const int STRESS_PER_TURN = 2;
    
    /// <summary>
    /// Apply high-intensity Psychic Resonance at combat start.
    /// </summary>
    public void ApplyPsychicResonance(CombatInstance combat)
    {
        if (combat.Biome?.BiomeId != 6) return;
        
        _logger.Information(
            "Applying high-intensity [Psychic Resonance] to combat {CombatId}",
            combat.CombatId);
        
        foreach (var character in combat.PlayerParty)
        {
            _traumaService.ApplyStress(character, STRESS_PER_ENCOUNTER,
                "Alfheim Psychic Resonance (ground zero of Great Silence)");
        }
    }
    
    /// <summary>
    /// Apply per-turn Psychic Resonance stress.
    /// </summary>
    public void ProcessTurnStress(CombatInstance combat, int currentTurn)
    {
        if (combat.Biome?.BiomeId != 6) return;
        
        foreach (var character in combat.PlayerParty)
        {
            _traumaService.ApplyStress(character, STRESS_PER_TURN,
                $"Alfheim turn {currentTurn} exposure");
        }
        
        _logger.Debug(
            "Alfheim turn {Turn}: Applied +{Stress} Psychic Stress to all party members",
            currentTurn, STRESS_PER_TURN);
    }
}
```

---

## IV. Hazard: Reality Tear

### A. Mechanical Definition

**Reality Tears** are the signature hazard of Alfheim - localized physics failures that warp positioning.

**Core Effects:**

1. **Positional Warp:** Character entering Reality Tear is teleported to random valid tile (3-5 tiles away)
2. **Corruption Increase:** +5 Corruption per Reality Tear encounter
3. **Energy Damage:** 2d8 Energy damage on warp (represents quantum flux exposure)
4. **Disorientation:** Character is [Dazed] for 1 turn after warp

**v5.0 Narrative Frame:**

- "You step into a tear in spacetime fabric. Reality inverts."
- "The physics engine crashes around you. When coherence returns, you re elsewhere."
- "The Aetheric field collapses locally - you experience the sensation of being in two places simultaneously."

### B. Database Entry

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    6,
    'Reality Tear',
    'Dynamic_Hazard',
    2.0, -- High spawn weight
    '2d8',
    'Energy',
    'Localized physics failure. Entering Reality Tear warps character to random tile 3-5 spaces away, deals 2d8 Energy damage, applies +5 Corruption, and inflicts [Dazed] for 1 turn. Represents catastrophic spacetime fabric rupture.'
);
```

### C. Service Implementation

**RealityTearService.cs:**

```csharp
public class RealityTearService
{
    private readonly ILogger<RealityTearService> _logger;
    private readonly DiceService _diceService;
    private readonly PositioningService _positioningService;
    private readonly TraumaEconomyService _traumaService;
    private readonly ConditionService _conditionService;
    
    private const int CORRUPTION_PER_TEAR = 5;
    private const string DAMAGE_DICE = "2d8";
    
    /// <summary>
    /// Process character entering Reality Tear.
    /// </summary>
    public void ProcessRealityTearEncounter(
        Combatant combatant,
        RoomTile tearTile,
        BattlefieldGrid grid)
    {
        _logger.Information(
            "{Combatant} entered Reality Tear at ({X}, {Y})",
            [combatant.Name](http://combatant.Name), tearTile.X, tearTile.Y);
        
        // Apply Energy damage
        var damage = _diceService.RollDice(DAMAGE_DICE);
        combatant.TakeDamage(damage, "Energy");
        
        // Apply Corruption
        if (combatant is Character character)
        {
            _traumaService.ApplyCorruption(character, CORRUPTION_PER_TEAR,
                "Reality Tear exposure");
        }
        
        // Warp position
        var newPosition = SelectWarpDestination(tearTile, grid);
        _positioningService.TeleportTo(combatant, newPosition);
        
        // Apply Dazed
        _conditionService.ApplyCondition(combatant, "Dazed", duration: 1);
        
        _logger.Information(
            "Reality Tear: {Combatant} warped from ({OldX}, {OldY}) to ({NewX}, {NewY}), took {Damage} Energy damage, gained +{Corruption} Corruption",
            [combatant.Name](http://combatant.Name), tearTile.X, tearTile.Y, 
            newPosition.X, newPosition.Y, damage, CORRUPTION_PER_TEAR);
    }
    
    /// <summary>
    /// Select random valid tile for warp destination (3-5 tiles away).
    /// </summary>
    private GridPosition SelectWarpDestination(
        RoomTile originTile,
        BattlefieldGrid grid)
    {
        var validTiles = grid.Tiles
            .Where(t => t.IsPassable())
            .Where(t => !t.IsOccupied)
            .Where(t => 
            {
                var distance = Math.Abs(t.X - originTile.X) + Math.Abs(t.Y - originTile.Y);
                return distance >= 3 && distance <= 5;
            })
            .ToList();
        
        if (!validTiles.Any())
        {
            _logger.Warning("No valid warp destinations found, staying in place");
            return new GridPosition(originTile.X, originTile.Y);
        }
        
        var chosen = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
        return new GridPosition(chosen.X, chosen.Y);
    }
}
```

---

## V. Additional Environmental Hazards (6 More Types)

### 1. [Low Gravity Pocket]

**Mechanical Definition:**

- Zone of disrupted physics (4x4 tile area)
- All movement costs ½ Stamina (minimum 1)
- Leap distance doubled
- Forced movement effects amplified (+1 tile distance)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    6,
    'Low Gravity Pocket',
    'Ambient_Effect',
    1.2,
    'Physics disruption zone. Movement costs half Stamina (minimum 1), Leap distance doubled, forced movement amplified +1 tile. Represents localized gravity field collapse from failed anti-grav systems.'
);
```

### 2. [Unstable Platform]

**Mechanical Definition:**

- Flickers between solid and incorporeal every 2 turns
- While incorporeal: impassable, projectiles pass through
- While solid: normal terrain
- Pattern is predictable (players can time movement)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    6,
    'Unstable Platform',
    'Dynamic_Hazard',
    1.5,
    'Platform flickers between solid and incorporeal states every 2 turns. When incorporeal: impassable, projectiles pass through. Pattern is predictable - requires timing. Malfunctioning phase-shift technology from Pre-Glitch architecture.'
);
```

### 3. [Energy Conduit]

**Mechanical Definition:**

- Active conduit deals 1d10 Energy damage to adjacent characters per turn
- Characters with Energy Resistance 50%+ can "channel" conduit for +5 Aether Pool
- Can be destroyed (30 HP) to stop damage

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, hp_value, description
)
VALUES (
    6,
    'Energy Conduit',
    'Interactive_Hazard',
    1.8, -- Common
    '1d10',
    'Energy',
    30,
    'Active Aetheric conduit. Deals 1d10 Energy damage per turn to adjacent characters. Characters with 50%+ Energy Resistance can channel it for +5 Aether Pool. Destructible (30 HP). Ruptured energy distribution systems.'
);
```

### 4. [Aetheric Vortex]

**Mechanical Definition:**

- Pull effect: Pulls all characters within 3 tiles closer by 1 tile per turn
- Center deals 2d6 Energy damage per turn
- Mystics pulled in gain +10 Aether Pool but +5 Stress

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    6,
    'Aetheric Vortex',
    'Dynamic_Hazard',
    1.0,
    '2d6',
    'Energy',
    'Swirling Aetheric energy vortex. Pulls all characters within 3 tiles closer by 1 tile per turn. Center deals 2d6 Energy damage. Mystics pulled in gain +10 Aether Pool but +5 Psychic Stress. Represents feedback loop in energy systems.'
);
```

### 5. [Crystalline Spire] (Cover)

**Mechanical Definition:**

- Excellent cover (+4 Defense)
- Reflects 25% of Energy damage back at attacker
- Destructible (50 HP)
- When destroyed, creates 3x3 difficult terrain (crystal shards)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    defense_bonus, hp_value, description
)
VALUES (
    6,
    'Crystalline Spire',
    'Cover',
    2.0, -- Common
    4, -- +4 Defense
    50,
    'Solidified Aetheric energy crystal formation. Provides excellent cover (+4 Defense). Reflects 25% of Energy damage back at attacker. Destructible (50 HP). When destroyed, creates 3x3 difficult terrain from crystal shards.'
);
```

### 6. [Holographic Interference]

**Mechanical Definition:**

- Zone (3x3 tiles) filled with flickering holograms
- Obscures line of sight (ranged attacks at Disadvantage)
- Holograms can be "dispelled" with 10+ Energy damage
- Mystics can spend 5 Aether to clear zone for 2 turns

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    6,
    'Holographic Interference',
    'Ambient_Effect',
    0.9,
    'Malfunctioning holographic projectors create flickering interference (3x3 zone). Obscures line of sight - ranged attacks at Disadvantage. Can be dispelled with 10+ Energy damage or 5 Aether spent by Mystic (clears for 2 turns).'
);
```

---

## VI. Integration Points

### A. ConditionService Integration

**Apply [Runic Instability] at combat start:**

```csharp
public void InitializeCombat(CombatInstance combat)
{
    // Existing initialization...
    
    // Apply biome ambient condition
    if (combat.Biome?.BiomeId == 6) // Alfheim
    {
        ApplyAmbientCondition(combat, 107); // Runic Instability
        _alfheimService.ApplyPsychicResonance(combat);
    }
}
```

### B. Mystic Ability Integration

**Check Wild Magic Surge on ability use:**

```csharp
public AbilityResult UseAbility(
    Character caster,
    Ability ability,
    CombatInstance combat)
{
    // Existing ability logic...
    
    // Check for Wild Magic Surge (Alfheim only, Mystic only)
    if (combat.Biome?.BiomeId == 6 && ability.ResourceType == "Aether")
    {
        var surge = _runicInstabilityService.TryTriggerWildMagicSurge(
            caster, ability, combat);
        
        if (surge != null)
        {
            // Apply surge modification to ability result
            ApplySurgeModification(result, surge);
        }
    }
    
    return result;
}
```

### C. Movement Integration

**Check Reality Tear on movement:**

```csharp
public bool TryMove(Combatant combatant, RoomTile targetTile)
{
    // Existing passability checks...
    
    // Check Reality Tear
    if (targetTile.HasFeature("Reality Tear"))
    {
        _realityTearService.ProcessRealityTearEncounter(
            combatant, targetTile, combatant.CurrentGrid);
        
        // Movement succeeds but to different location
        return true;
    }
    
    // Complete movement...
}
```

---

## VII. Testing Requirements

### Unit Tests

**Test 1: Runic Instability Application**

```csharp
[Fact]
public void ApplyRunicInstability_Mystic_GainsAetherPoolBonus()
{
    var mystic = CreateTestMystic(baseAetherPool: 100);
    var combat = CreateAlfheimCombat();
    
    _runicInstabilityService.ApplyAetherPoolAmplification(mystic);
    
    Assert.Equal(110, mystic.MaxAetherPool); // +10%
}
```

**Test 2: Wild Magic Surge Triggering**

```csharp
[Fact]
public void TryTriggerWildMagicSurge_25PercentChance_TriggersCorrectly()
{
    var mystic = CreateTestMystic();
    var ability = CreateMysticAbility();
    var combat = CreateAlfheimCombat();
    
    int surgeCount = 0;
    for (int i = 0; i < 1000; i++)
    {
        var surge = _runicInstabilityService.TryTriggerWildMagicSurge(
            mystic, ability, combat);
        if (surge != null) surgeCount++;
    }
    
    // Should be ~250 ± 50 (25% of 1000)
    Assert.InRange(surgeCount, 200, 300);
}
```

**Test 3: Reality Tear Warping**

```csharp
[Fact]
public void ProcessRealityTearEncounter_WarpsPosition_AppliesEffects()
{
    var character = CreateTestCharacter();
    var tearTile = CreateRealityTearTile(x: 5, y: 5);
    var grid = CreateTestGrid();
    
    var originalPosition = character.CurrentPosition;
    var initialCorruption = character.Corruption;
    
    _realityTearService.ProcessRealityTearEncounter(
        character, tearTile, grid);
    
    // Position changed (3-5 tiles away)
    var distance = Math.Abs(character.CurrentPosition.X - originalPosition.X) + 
                   Math.Abs(character.CurrentPosition.Y - originalPosition.Y);
    Assert.InRange(distance, 3, 5);
    
    // Corruption applied
    Assert.Equal(initialCorruption + 5, character.Corruption);
    
    // Dazed applied
    Assert.True(character.HasCondition("Dazed"));
}
```

**Test 4: Psychic Resonance High Intensity**

```csharp
[Fact]
public void ApplyPsychicResonance_Alfheim_AppliesHighStress()
{
    var combat = CreateAlfheimCombat();
    var party = combat.PlayerParty;
    
    var initialStress = party[0].PsychicStress;
    
    _alfheimService.ApplyPsychicResonance(combat);
    
    // Should apply +10 per character
    Assert.Equal(initialStress + 10, party[0].PsychicStress);
}
```

---

## VIII. Success Criteria

### Functional Requirements

- [ ]  [Runic Instability] applies to all Alfheim combats
- [ ]  Wild Magic Surge triggers 25% of time on Mystic abilities
- [ ]  Surge effects modify abilities correctly
- [ ]  [Psychic Resonance] applies high Stress (+10 per encounter, +2 per turn)
- [ ]  Reality Tears warp character positions 3-5 tiles
- [ ]  Reality Tears apply Corruption and Dazed
- [ ]  All 8 hazard types spawn correctly
- [ ]  Energy Conduits deal damage and allow channeling
- [ ]  Crystalline Spires provide cover and reflect damage

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass for full combat scenarios
- [ ]  v5.0 voice compliance (technology, not magic)
- [ ]  Performance acceptable (< 100ms per hazard check)

### Balance Validation

- [ ]  Wild Magic Surges feel risky but exciting
- [ ]  Mystic power increase balances surge risk
- [ ]  Psychic Resonance is manageable with high WILL
- [ ]  Reality Tears create positioning chaos without frustration
- [ ]  Energy Conduits provide tactical choice
- [ ]  Alfheim overall feels like endgame challenge

---

## IX. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.31.2_environmental_hazards.sql
```

### Step 2: Compile Services

```bash
dotnet build Services/RunicInstabilityService.cs
dotnet build Services/RealityTearService.cs
dotnet build Services/Biomes/AlfheimService.cs
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Alfheim.EnvironmentalTests"
```

### Step 4: Manual Verification

- Start combat in Alfheim
- Verify [Runic Instability] displays in combat log
- Use Mystic abilities multiple times
- Verify Wild Magic Surges trigger ~25% of time
- Step into Reality Tear
- Verify position warp and Corruption application
- Observe Psychic Stress accumulation

---

## X. Next Steps

Once v0.31.2 is complete:

**Proceed to v0.31.3:** Enemy Definitions & Spawn System

- 5 enemy type definitions
- Energy/Aetheric resistance patterns
- Spawn weight system
- All-Rune's Echo boss framework

---

## XI. Related Documents

**Parent Specification:**

- v0.31: Alfheim Biome Implementation[[1]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)

**Reference Implementation:**

- v0.29.2: Muspelheim Environmental Hazards[[2]](v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md)
- v0.30.2: Niflheim Environmental Hazards[[3]](v0%2030%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%2025f6a1f3b0a843ce9a72b86503b1de60.md)

**Prerequisites:**

- v0.31.1: Database Schema[[1]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)
- v0.15: Trauma Economy[[4]](https://www.notion.so/v0-15-Trauma-Economy-Breaking-Points-Consequences-a1e59f904171485284d6754193af333b?pvs=21)
- v0.19: Mystic Specializations[[5]](https://www.notion.so/6e4966fa3e8c4317ad34df7c81f76789?pvs=21)

---

**Environmental systems complete. Proceed to enemy definitions (v0.31.3).**