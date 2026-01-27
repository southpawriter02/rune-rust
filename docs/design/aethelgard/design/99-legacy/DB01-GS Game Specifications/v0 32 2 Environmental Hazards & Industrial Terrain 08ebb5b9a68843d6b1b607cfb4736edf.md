# v0.32.2: Environmental Hazards & Industrial Terrain

Type: Technical
Description: Implements [Live Power Conduit] signature hazard, [High-Pressure Steam Vent], [Unstable Ceiling/Wall] collapse, 10+ industrial terrain types and Jötun corpse terrain.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.32.1 (Database Schema)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.32: Jötunheim Biome Implementation (v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.32.2-ENVIRONMENTAL

**Parent Specification:** v0.32 Jötunheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 10-14 hours

**Prerequisites:** v0.32.1 (Database complete), v0.31.2 (Environmental hazards reference)

---

## I. Overview

This specification defines environmental hazards and industrial terrain for the Jötunheim biome, including [Live Power Conduit] signature hazard, [High-Pressure Steam Vent], [Unstable Ceiling/Wall], and 10+ terrain/hazard types. Jötunheim hazards are **physical and technological**, not metaphysical.

### Core Deliverables

- **[Live Power Conduit]** signature hazard (interacts with [Flooded])
- **[High-Pressure Steam Vent]** eruption mechanic
- **[Unstable Ceiling/Wall]** structural collapse system
- **10+ Hazard/Terrain Types** with mechanical definitions
- **No Ambient Condition** (physical threats only)
- **Integration** with Environmental Combat and Tactical Grid
- **Serilog structured logging** for all hazard interactions

---

## II. Signature Hazard: [Live Power Conduit]

### A. Mechanical Definition

**[Live Power Conduit]** is the defining environmental hazard of Jötunheim - failing power grid cables that deal electrical damage.

**Core Effects:**

1. **Proximity Damage:** Characters adjacent to conduit take 1d8 Energy damage per turn
2. **Flooded Amplification:** If conduit is in [Flooded] terrain, damage becomes 2d10 and affects 2-tile radius
3. **Destructible:** Conduit can be destroyed (15 HP) to stop damage
4. **Forced Movement Risk:** Characters pushed/pulled into conduit take double damage

**v5.0 Narrative Frame:**

- "The failing power grid arcs with deadly electrical current."
- "Coolant fluid conducts the electricity - stay out of the water."
- "The cable sparks violently. Direct contact would be instantly lethal."

### B. Database Entry

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, hp_value, description
)
VALUES (
    7,
    'Live Power Conduit',
    'Dynamic_Hazard',
    2.5, -- Very high spawn weight (signature hazard)
    '1d8',
    'Energy',
    15,
    'Failing power grid cable arcing with electrical current. Deals 1d8 Energy damage per turn to adjacent characters. AMPLIFIED IN FLOODED TERRAIN: 2d10 damage in 2-tile radius. Destructible (15 HP). Represents 800 years of failing infrastructure.'
);
```

### C. Service Implementation

**PowerConduitService.cs:**

```csharp
public class PowerConduitService
{
    private readonly ILogger<PowerConduitService> _logger;
    private readonly DiceService _diceService;
    
    private const string BASE_DAMAGE = "1d8";
    private const string FLOODED_DAMAGE = "2d10";
    private const int FLOODED_RADIUS = 2;
    
    /// <summary>
    /// Process Live Power Conduit damage each turn.
    /// Checks for flooded terrain amplification.
    /// </summary>
    public void ProcessPowerConduits(BattlefieldGrid grid, List<Combatant> combatants)
    {
        var conduitTiles = grid.Tiles.Where(t => t.HasFeature("Live Power Conduit")).ToList();
        
        foreach (var conduit in conduitTiles)
        {
            var isFlooded = conduit.HasTerrain("Flooded");
            
            if (isFlooded)
            {
                ProcessFloodedConduit(conduit, grid, combatants);
            }
            else
            {
                ProcessStandardConduit(conduit, grid, combatants);
            }
        }
    }
    
    private void ProcessStandardConduit(
        RoomTile conduit,
        BattlefieldGrid grid,
        List<Combatant> combatants)
    {
        var adjacentCombatants = grid.GetAdjacentCombatants(conduit);
        
        foreach (var combatant in adjacentCombatants)
        {
            var damage = _diceService.RollDice(BASE_DAMAGE);
            combatant.TakeDamage(damage, "Energy");
            
            _logger.Information(
                "{Combatant} takes {Damage} Energy damage from Live Power Conduit at ({X}, {Y})",
                [combatant.Name](http://combatant.Name), damage, conduit.X, conduit.Y);
        }
    }
    
    private void ProcessFloodedConduit(
        RoomTile conduit,
        BattlefieldGrid grid,
        List<Combatant> combatants)
    {
        var affectedCombatants = grid.GetCombatantsInRadius(conduit, FLOODED_RADIUS);
        
        foreach (var combatant in affectedCombatants)
        {
            var damage = _diceService.RollDice(FLOODED_DAMAGE);
            combatant.TakeDamage(damage, "Energy");
            
            _logger.Warning(
                "AMPLIFIED: {Combatant} takes {Damage} Energy damage from flooded power conduit at ({X}, {Y})",
                [combatant.Name](http://combatant.Name), damage, conduit.X, conduit.Y);
        }
    }
}
```

---

## III. Hazard: [High-Pressure Steam Vent]

### A. Mechanical Definition

**Core Effects:**

1. **Scheduled Eruption:** Erupts every 3 turns (predictable)
2. **Area Effect:** 3-tile cone
3. **Damage:** 2d6 Fire damage to all in cone
4. **Knockback:** Targets pushed back 1 tile
5. **Warning:** Vents hiss loudly 1 turn before eruption

**v5.0 Narrative Frame:**

- "The steam vent hisses - pressure is building."
- "Superheated steam erupts with explosive force."
- "The faulty pipe vents centuries of built-up pressure."

### B. Database Entry

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    7,
    'High-Pressure Steam Vent',
    'Dynamic_Hazard',
    1.5, -- Medium-high spawn
    '2d6',
    'Fire',
    'Faulty industrial piping. Erupts every 3 turns in 3-tile cone. Deals 2d6 Fire damage and knocks targets back 1 tile. Hisses loudly 1 turn before eruption (predictable). Represents failing coolant system pressure management.'
);
```

---

## IV. Hazard: [Unstable Ceiling/Wall]

### A. Mechanical Definition

**Core Effects:**

1. **Triggered Collapse:** Heavy impacts (10+ damage in 1 attack) trigger collapse
2. **Area Effect:** 2x2 tile area
3. **Damage:** 3d8 Physical damage
4. **Terrain Change:** Creates [Debris Pile] (difficult terrain + cover)
5. **One-Time:** Cannot collapse twice

**v5.0 Narrative Frame:**

- "The ceiling groans ominously. Structural integrity is compromised."
- "Rusted support beams finally give way after 800 years."
- "The wall collapses in a shower of concrete and twisted metal."

### B. Database Entry

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    7,
    'Unstable Ceiling/Wall',
    'Triggered_Hazard',
    1.3, -- Medium spawn
    '3d8',
    'Physical',
    'Structurally compromised section. Collapses when heavy impact (10+ damage) occurs nearby. Deals 3d8 Physical damage in 2x2 area. Creates [Debris Pile] terrain (difficult terrain + cover). One-time hazard. 800 years of decay finally catching up.'
);
```

---

## V. Terrain Type: [Flooded] (Coolant Spills)

### A. Mechanical Definition

**Core Effects:**

1. **Movement Penalty:** Costs +1 Stamina to enter (wading through fluid)
2. **Conductivity:** Amplifies Energy damage (power conduit interaction)
3. **Toxic Exposure:** Characters ending turn in flooded terrain take 1 Poison damage
4. **Visibility:** Difficult to see submerged hazards

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    7,
    'Flooded (Coolant)',
    'Terrain_Type',
    1.8, -- High spawn in Coolant Reservoir rooms
    'Ankle-deep coolant fluid. Movement costs +1 Stamina (wading). Conductive - amplifies Live Power Conduit damage (1d8 → 2d10, radius 1 → 2). Characters ending turn in fluid take 1 Poison damage. Rainbow-sheened surface obscures submerged hazards.'
);
```

---

## VI. Additional Environmental Features (6 More Types)

### 1. [Cover] (Shipping Containers, Engine Blocks)

**Mechanical Definition:**

- Provides +3 Defense
- Destructible (25-40 HP depending on size)
- Very high spawn weight (industrial debris everywhere)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    defense_bonus, hp_value, description
)
VALUES (
    7,
    'Cover (Industrial)',
    'Cover',
    2.5, -- Very high
    3, -- +3 Defense
    30, -- Average HP
    'Industrial debris provides excellent cover. Shipping containers, engine blocks, and structural components litter the factory floor. Provides +3 Defense. Destructible (25-40 HP depending on size). Some containers can be opened for salvage.'
);
```

### 2. [Jötun Corpse Terrain]

**Mechanical Definition:**

- Unique terrain type (walking on/in dormant Ju00f6tun-Forged)
- Creates extreme verticality (limbs = bridges, hull = platforms)
- Passive Stress (+2 per turn from psychic broadcast)
- God-Sleeper Cultist power amplification (when relevant)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    7,
    'Jotun Corpse Terrain',
    'Special_Terrain',
    0.8, -- Rare but iconic
    'Terrain formed by dormant Jötun-Forged chassis. Hull sections = platforms, limbs = bridges, interior = cave. Creates extreme verticality. Characters on corpse terrain gain +2 Psychic Stress per turn (corrupted logic core broadcast). God-Sleeper Cultists gain power here.'
);
```

### 3. [Assembly Line] (Conveyor Hazard)

**Mechanical Definition:**

- Active conveyor belt (still powered after 800 years)
- Forces movement: Characters on belt move 2 tiles per turn (direction depends on belt)
- Can push characters into hazards
- Can be shut down with Engineering check

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    7,
    'Assembly Line (Active)',
    'Dynamic_Hazard',
    1.0,
    'Still-functional conveyor belt. Characters standing on belt are moved 2 tiles per turn in belt direction. Can push targets into hazards or off platforms. Shutting down requires Engineering check (DC 15). The automation protocols never stopped - even after 800 years.'
);
```

### 4. [Scrap Heap]

**Mechanical Definition:**

- Difficult terrain (costs +1 Stamina to enter)
- Salvageable: Can search for resources (takes 1 action)
- Unstable: Movement through heap is loud (alerts enemies)
- Mixed quality: Find anything from Tier 1-3 components

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    7,
    'Scrap Heap',
    'Interactive_Terrain',
    1.6, -- Common
    'Mountain of compacted scrap metal. Difficult terrain (+1 Stamina movement). Salvageable: 1 action to search, roll 1d20 vs. DC 12 for Tier 1-3 component. Unstable: Movement alerts enemies. Valuable components mixed with worthless debris.'
);
```

### 5. [Toxic Haze] (in Coolant Sectors)

**Mechanical Definition:**

- Zone effect (4x4 tiles)
- Obscures vision (ranged attacks at Disadvantage)
- Poison damage: 1d4 per turn spent in haze
- Dissipates slowly (1 tile per turn from edges)

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight,
    damage_dice, damage_type, description
)
VALUES (
    7,
    'Toxic Haze',
    'Ambient_Effect',
    0.7, -- Uncommon, specific to coolant areas
    '1d4',
    'Poison',
    'Chemical vapor from leaking coolant. Obscures vision in 4x4 zone - ranged attacks at Disadvantage. Deals 1d4 Poison damage per turn. Dissipates slowly (1 tile/turn from edges). Prolonged exposure causes respiratory damage. Found near coolant reservoirs.'
);
```

### 6. [Gantry Platform]

**Mechanical Definition:**

- Elevated platform (creates verticality)
- Accessible via ladder/stairs (costs 2 Stamina to climb)
- Provides height advantage (+1 to ranged attacks)
- Can be climbed by Gantry-Runner for free

**Database Entry:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, spawn_weight, description
)
VALUES (
    7,
    'Gantry Platform',
    'Elevation',
    1.4,
    'Elevated maintenance platform. Accessible via ladder (costs 2 Stamina to climb). Provides height advantage: +1 to ranged attacks from platform. Gantry-Runner specialization climbs for free. Creates tactical verticality in factory floor combat.'
);
```

---

## VII. No Ambient Condition

**Important Design Decision:**

Jötunheim has **NO ambient condition**. This is intentional and canonical from v2.0.

**Rationale:**

- Threats are physical/technological, not metaphysical
- Contrasts with Alfheim ([Runic Instability]), Muspelheim ([Intense Heat]), Niflheim ([Frigid Cold])
- Creates distinct biome identity: "industrial hazards, not magical curses"
- Focuses gameplay on tactical positioning and environmental awareness

**Implementation Note:**

The `ambient_condition_id` field in Biomes table is `NULL` for Jötunheim (biome_id: 7).

---

## VIII. Integration Points

### A. Environmental Combat Integration

**Power Conduit + Flooded Combo:**

```csharp
public void CheckFloodedConduitInteraction(RoomTile tile)
{
    if (tile.HasFeature("Live Power Conduit") && tile.HasTerrain("Flooded"))
    {
        _logger.Warning(
            "DANGEROUS COMBO: Live Power Conduit in flooded terrain at ({X}, {Y})",
            tile.X, tile.Y);
        
        // Amplify damage and radius
        tile.SetDamage("2d10"); // vs standard 1d8
        tile.SetRadius(2); // vs standard 1
    }
}
```

### B. Tactical Grid Integration

**Forced Movement into Hazards:**

```csharp
public void ProcessForcedMovement(Combatant target, GridPosition destination)
{
    // Existing forced movement logic...
    
    var destTile = _grid.GetTile(destination);
    
    // Check for power conduit
    if (destTile.HasFeature("Live Power Conduit"))
    {
        var damage = _diceService.RollDice("1d8") * 2; // Double damage from forced movement
        target.TakeDamage(damage, "Energy");
        
        _logger.Information(
            "{Target} forced into Live Power Conduit - takes {Damage} Energy damage (doubled)",
            [target.Name](http://target.Name), damage);
    }
}
```

### C. Trauma Economy Integration

**Jötun Proximity Stress:**

```csharp
public void ProcessJotunProximityStress(CombatInstance combat, int currentTurn)
{
    foreach (var character in combat.PlayerParty)
    {
        var currentTile = _grid.GetTile(character.CurrentPosition);
        
        if (currentTile.HasTerrain("Jotun Corpse Terrain"))
        {
            _traumaService.ApplyStress(character, 2,
                $"Jötun proximity - turn {currentTurn}");
            
            _logger.Debug(
                "{Character} on Jötun corpse terrain - applied +2 Psychic Stress",
                [character.Name](http://character.Name));
        }
    }
}
```

---

## IX. Testing Requirements

### Unit Tests

**Test 1: Power Conduit Standard Damage**

```csharp
[Fact]
public void ProcessPowerConduits_StandardTerrain_Deals1d8Damage()
{
    var character = CreateTestCharacter();
    var conduitTile = CreateTileWithFeature("Live Power Conduit");
    var grid = CreateTestGrid();
    grid.PlaceCombatant(character, adjacentTo: conduitTile);
    
    var initialHP = character.HP;
    
    _powerConduitService.ProcessPowerConduits(grid, new List<Combatant> { character });
    
    // Should deal 1-8 damage
    Assert.InRange(initialHP - character.HP, 1, 8);
}
```

**Test 2: Power Conduit Flooded Amplification**

```csharp
[Fact]
public void ProcessPowerConduits_FloodedTerrain_AmplifiedDamageAndRadius()
{
    var characters = CreateTestParty();
    var conduitTile = CreateTileWithFeature("Live Power Conduit");
    conduitTile.AddTerrain("Flooded");
    var grid = CreateTestGrid();
    
    // Place characters at varying distances
    grid.PlaceCombatant(characters[0], distance: 1, from: conduitTile);
    grid.PlaceCombatant(characters[1], distance: 2, from: conduitTile);
    grid.PlaceCombatant(characters[2], distance: 3, from: conduitTile);
    
    _powerConduitService.ProcessPowerConduits(grid, characters);
    
    // Characters within 2 tiles should take damage
    Assert.True(characters[0].HP < characters[0].MaxHP); // Distance 1
    Assert.True(characters[1].HP < characters[1].MaxHP); // Distance 2
    Assert.Equal(characters[2].MaxHP, characters[2].HP); // Distance 3 - no damage
}
```

**Test 3: Steam Vent Eruption Timing**

```csharp
[Fact]
public void ProcessSteamVent_EruptsEvery3Turns()
{
    var combat = CreateTestCombat();
    var vent = CreateSteamVent();
    
    for (int turn = 1; turn <= 9; turn++)
    {
        var erupted = _jotunheimService.ProcessSteamVent(vent, combat, turn);
        
        if (turn % 3 == 0)
        {
            Assert.True(erupted, $"Should erupt on turn {turn}");
        }
        else
        {
            Assert.False(erupted, $"Should not erupt on turn {turn}");
        }
    }
}
```

**Test 4: Unstable Ceiling Trigger**

```csharp
[Fact]
public void ProcessUnstableCeiling_TriggersOn10PlusDamage()
{
    var ceiling = CreateUnstableCeiling();
    
    // 9 damage - should not trigger
    var triggered1 = _jotunheimService.CheckCeilingCollapse(ceiling, damage: 9);
    Assert.False(triggered1);
    
    // 10 damage - should trigger
    var triggered2 = _jotunheimService.CheckCeilingCollapse(ceiling, damage: 10);
    Assert.True(triggered2);
}
```

---

## X. Success Criteria

### Functional Requirements

- [ ]  [Live Power Conduit] deals 1d8 Energy damage per turn
- [ ]  [Live Power Conduit] + [Flooded] amplification works (2d10, 2-tile radius)
- [ ]  [High-Pressure Steam Vent] erupts every 3 turns
- [ ]  [Unstable Ceiling/Wall] triggers on 10+ damage
- [ ]  [Flooded] terrain applies movement penalty and Poison damage
- [ ]  [Jötun Corpse Terrain] applies Psychic Stress correctly
- [ ]  All 10 hazard/terrain types spawn correctly
- [ ]  No ambient condition applies (NULL in database)

### Quality Gates

- [ ]  85%+ unit test coverage
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass for hazard combinations
- [ ]  v5.0 voice compliance (industrial, not supernatural)
- [ ]  Performance acceptable (<100ms per hazard check)

### Balance Validation

- [ ]  Power conduits feel dangerous but predictable
- [ ]  Flooded + conduit combo creates tactical decisions
- [ ]  Steam vents create positioning challenges without frustration
- [ ]  Structural collapses feel cinematic
- [ ]  Jötun proximity Stress is noticeable but manageable
- [ ]  Overall hazard density feels appropriate for mid-game

---

## XI. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.32.2_environmental_hazards.sql
```

### Step 2: Compile Services

```bash
dotnet build Services/PowerConduitService.cs
dotnet build Services/Biomes/JotunheimService.cs
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Jotunheim.EnvironmentalTests"
```

### Step 4: Manual Verification

- Start combat in Jötunheim
- Verify power conduits deal damage
- Test flooded + conduit combo
- Observe steam vent eruptions (every 3 turns)
- Trigger ceiling collapse with heavy damage
- Verify no ambient condition applies

---

## XII. Next Steps

Once v0.32.2 is complete:

**Proceed to v0.32.3:** Enemy Definitions & Spawn System

- 6 enemy type definitions (4 Undying, 2 Humanoid, 1 Beast)
- Undying Physical Soak patterns
- Armor-shredding tactical requirement
- Draugr Juggernaut as teaching enemy

---

## XIII. Related Documents

**Parent Specification:**

- v0.32: Jötunheim Biome Implementation[[1]](v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)

**Reference Implementation:**

- v0.29.2: Muspelheim Environmental Hazards[[2]](v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md)
- v0.30.2: Niflheim Environmental Hazards[[3]](v0%2030%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%2025f6a1f3b0a843ce9a72b86503b1de60.md)
- v0.31.2: Alfheim Environmental Hazards[[4]](v0%2031%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%20babad7a1095a4acea8314f2e9162344c.md)

**Prerequisites:**

- v0.32.1: Database Schema[[5]](v0%2032%201%20Database%20Schema%20&%20Room%20Templates%20ffc37b6b82c1421bb1a599bdb61194d3.md)
- v0.22: Environmental Combat[[6]](https://www.notion.so/v0-22-Environmental-Combat-System-f2f10fecac364272a084cd0655b10998?pvs=21)
- v0.20: Tactical Grid[[7]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)

---

**Environmental systems complete. Proceed to enemy definitions (v0.32.3).**