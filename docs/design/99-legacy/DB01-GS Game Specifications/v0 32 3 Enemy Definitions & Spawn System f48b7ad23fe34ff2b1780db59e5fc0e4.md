# v0.32.3: Enemy Definitions & Spawn System

Type: Technical
Description: Defines 6 Jötunheim enemy types (Rusted Servitor, Rusted Warden, Draugr Juggernaut, God-Sleeper Cultist, Scrap-Tinker, Iron-Husked Boar). Undying-heavy spawns.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.32.1-v0.32.2 (Database, Environmental)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.32: Jötunheim Biome Implementation (v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.32.3-ENEMIES

**Parent Specification:** v0.32 Jötunheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 10-16 hours

**Prerequisites:** v0.32.1 (Database), v0.32.2 (Environmental systems), v0.31.3 (Enemy reference)

---

## I. Overview

This specification defines all enemy types for the Jötunheim biome, including 6 distinct enemy types with complete mechanical definitions, spawn weights, Undying Physical Soak patterns, and armor-shredding tactical requirements.

### Core Deliverables

- **6 Enemy Type Definitions** with complete stat blocks
- **4 Undying variants** (janitorial/security/heavy)
- **2 Humanoid types** (God-Sleeper Cultist, Scrap-Tinker)
- **1 Beast type** (Iron-Husked Boar)
- **Undying-heavy spawn tables** (matches v2.0 "graveyard" theme)
- **Physical Soak patterns** (high armor standard)
- **Draugr Juggernaut** teaches armor-shredding

---

## II. Enemy Type Definitions

### A. Rusted Servitor (Undying - Common)

**Conceptual Identity:**

The most common threat - mindless janitorial constructs still executing maintenance protocols after 800 years.

**Stat Block:**

```yaml
Enemy: Rusted Servitor
Archetype: Undying (Janitorial)
Tier: 2 (Early-mid game)
HP: 35
Defense: 10
Physical Soak: 4 (moderate armor)
Movement: 4 tiles
Initiative: +1

Resistances:
  Physical: 25%
  Energy: 0%
  Fire: -25% (Vulnerable - rust/corrosion)
  Psychic: 100% (Immune)

Abilities:
  - Maintenance Protocol (1 Stamina):
      Range: Melee
      Damage: 1d8 + 2 Physical damage
      Effect: Basic attack. Servitor attempts to "fix" targets by disassembling them.
      Cooldown: None
  
  - Sweep Mode (2 Stamina):
      Range: Melee (cone, 2 adjacent tiles)
      Damage: 1d6 Physical damage
      Effect: Attacks 2 adjacent targets simultaneously.
      Cooldown: 2 turns
  
  - Undying Resilience (Passive):
      Effect: Cannot be reduced below 1 HP by single attack. Physical Soak reduces damage before HP loss.

Loot Table:
  - Rusted Scrap Metal (90% chance)
  - Intact Servomotor (30% chance)
  - Ball Bearings (20% chance)

Spawn Weight: 2.0 (Very common)
```

**v5.0 Narrative Frame:**

"Corroded maintenance unit following 800-year-old cleaning protocols. Not aggressive by design - but anything living registers as 'debris to be removed.' The automation never stopped."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'Rusted Servitor',
    'Undying_Janitorial',
    2,
    35,
    10,
    4, -- Moderate armor
    4,
    1,
    2.0, -- Very common
    'Corroded janitorial construct. Executes maintenance protocols after 800 years. Physical Soak 4 (moderate armor). 25% Physical Resistance. Vulnerable to Fire (-25%). Basic attacks and cone sweep. Most common Undying in Jötunheim.'
);
```

---

### B. Rusted Warden (Undying - Medium)

**Conceptual Identity:**

Security forces - tougher and more resilient than Servitors. Still executing perimeter defense protocols.

**Stat Block:**

```yaml
Enemy: Rusted Warden
Archetype: Undying (Security)
Tier: 3 (Mid-game)
HP: 50
Defense: 12
Physical Soak: 6 (high armor)
Movement: 4 tiles
Initiative: +2

Resistances:
  Physical: 35%
  Energy: 0%
  Fire: -20% (Vulnerable)
  Psychic: 100% (Immune)

Abilities:
  - Security Strike (2 Stamina):
      Range: Melee
      Damage: 2d6 + 3 Physical damage
      Effect: Heavy melee attack. If target has <50% HP, apply [Stunned] for 1 turn.
      Cooldown: 1 turn
  
  - Defensive Stance (2 Stamina):
      Range: Self
      Effect: Gain +4 Defense for 2 turns. Cannot move while in stance.
      Cooldown: 3 turns
  
  - Threat Detection (Passive):
      Effect: Cannot be surprised. Always acts first when combat begins.

Loot Table:
  - Rusted Scrap Metal (70% chance)
  - Intact Servomotor (50% chance)
  - Hydraulic Cylinder (25% chance)
  - Power Relay Circuit (15% chance)

Spawn Weight: 1.5 (Common)
```

**v5.0 Narrative Frame:**

"Security unit with heavier armor plating. Still follows perimeter defense protocols - hostile to all unregistered personnel. Physical Soak makes it resistant to weak attacks. Requires focused fire or armor-shredding."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'Rusted Warden',
    'Undying_Security',
    3,
    50,
    12,
    6, -- High armor
    4,
    2,
    1.5, -- Common
    'Security construct with heavy armor plating. Physical Soak 6 (high armor). 35% Physical Resistance. Can enter defensive stance (+4 Defense). Applies [Stunned] to low-HP targets. Fire Vulnerable (-20%). Requires armor-shredding or focused fire.'
);
```

---

### C. Draugr Juggernaut (Undying - Rare/Elite)

**Conceptual Identity:**

The armor-shredding teaching enemy - massively armored Undying that **requires** armor-shredding tactics to defeat efficiently.

**Stat Block:**

```yaml
Enemy: Draugr Juggernaut
Archetype: Undying (Heavy Assault)
Tier: 4 (Elite threat)
HP: 90
Defense: 14
Physical Soak: 10 (massive armor)
Movement: 3 tiles (Slow)
Initiative: +0

Resistances:
  Physical: 50%
  Energy: 0%
  Fire: -15% (Vulnerable)
  Psychic: 100% (Immune)

Abilities:
  - Juggernaut Slam (3 Stamina):
      Range: Melee
      Damage: 3d8 + 6 Physical damage
      Effect: Devastating melee attack. Target knocked back 2 tiles. If collision, +1d8 damage.
      Cooldown: 2 turns
  
  - Armored Advance (2 Stamina):
      Range: Self
      Effect: Move 3 tiles and gain +4 Defense for 1 turn. Can move through difficult terrain.
      Cooldown: 2 turns
  
  - Fortress Protocol (Passive):
      Effect: Takes HALF damage from attacks dealing <15 damage. Physical Soak 10 reduces damage significantly. TEACHING MECHANIC: Players must use armor-shredding or high-damage attacks.
  
  - Undying Endurance (Passive):
      Effect: When reduced to 0 HP, revives at 20 HP once per combat (unless Fire damage killed it).

Loot Table:
  - Intact Servomotor (80% chance)
  - Hydraulic Cylinder (60% chance)
  - Unblemished Jötun Plating (15% chance)
  - Industrial Servo Actuator (10% chance)

Spawn Weight: 0.6 (Rare, elite)
```

**v5.0 Narrative Frame:**

"Heavy assault construct with extreme armor plating. The Juggernaut is a teaching enemy - it REQUIRES armor-shredding tactics. Attacks below 15 damage are halved. Physical Soak 10 absorbs most attacks. Iron-Banes and Rust-Witches excel here."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'Draugr Juggernaut',
    'Undying_Heavy_Assault',
    4,
    90,
    14,
    10, -- Massive armor (TEACHING MECHANIC)
    3,
    0,
    0.6, -- Rare elite
    'ELITE: Heavy assault construct. Physical Soak 10, 50% Physical Resistance. Takes HALF damage from attacks <15 damage. Revives once at 20 HP. Slow but devastating. TEACHING ENEMY: Requires armor-shredding (Iron-Bane, Rust-Witch) or high-damage focus fire. Fire Vulnerable (-15%).'
);
```

---

### D. God-Sleeper Cultist (Humanoid - Medium)

**Conceptual Identity:**

Fanatics who worship dormant Jötun-Forged as sleeping gods. Gain power near Jötun corpse terrain.

**Stat Block:**

```yaml
Enemy: God-Sleeper Cultist
Archetype: Humanoid (Fanatic)
Tier: 3 (Mid-game)
HP: 40
Defense: 11
Physical Soak: 2 (light armor)
Movement: 5 tiles
Initiative: +3

Resistances:
  Physical: 0%
  Energy: 15%
  Psychic: 50% (Fanatical devotion)
  Fire: 0%

Abilities:
  - Salvaged Weapon Strike (2 Stamina):
      Range: Melee
      Damage: 2d6 + 2 Physical damage
      Effect: Attack with improvised industrial weapon.
      Cooldown: None
  
  - Devotional Chant (3 Stamina):
      Range: 4 tiles (allies)
      Effect: All allies within range gain +2 Defense and +3 HP for 2 turns.
      Cooldown: 4 turns
  
  - Jötun Attunement (Passive):
      Effect: When in [Jötun Corpse Terrain] or within 3 tiles of dormant Jötun-Forged, gain +4 to all rolls and +10 HP. Represents cargo cult power boost.
  
  - Fanatical Zeal (Passive):
      Effect: Immune to [Fear] effects. When ally dies, gain +2 to next attack roll.

Loot Table:
  - Rusted Scrap Metal (60% chance)
  - Salvaged Weapon Parts (40% chance)
  - Cultist Robe (crafting material) (20% chance)

Spawn Weight: 1.3 (Medium)
```

**v5.0 Narrative Frame:**

"Cargo cultists worshipping Jötun-Forged as sleeping gods. Not delusional - the psychic broadcast from corrupted logic cores DOES grant them power. Dangerous near their 'temples' (Jötun corpses)."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'God-Sleeper Cultist',
    'Humanoid_Fanatic',
    3,
    40,
    11,
    2,
    5,
    3,
    1.3, -- Medium spawn
    'Cargo cultist worshipping Jötun-Forged. Gains power near Jötun Corpse Terrain (+4 to rolls, +10 HP). Provides buff to allies (Devotional Chant). 50% Psychic Resistance (fanatical devotion). Immune to [Fear]. Fast and mobile.'
);
```

---

### E. Scrap-Tinker (Humanoid - Rare)

**Conceptual Identity:**

Rival scavengers seeking high-quality mechanical components. Use improvised gadgets and traps.

**Stat Block:**

```yaml
Enemy: Scrap-Tinker
Archetype: Humanoid (Scavenger)
Tier: 3 (Mid-game)
HP: 35
Defense: 10
Physical Soak: 1 (minimal armor)
Movement: 5 tiles
Initiative: +4

Resistances:
  Physical: 0%
  Energy: 25% (improvised shielding)
  Psychic: 0%
  Fire: -10% (Vulnerable - carries volatile materials)

Abilities:
  - Improvised Crossbow (2 Stamina):
      Range: 6 tiles
      Damage: 2d6 Physical damage
      Effect: Ranged attack with makeshift crossbow.
      Cooldown: 1 turn
  
  - Deploy Trap (3 Stamina):
      Range: Adjacent tile
      Effect: Places [Scrap Trap]. First character entering tile takes 2d8 damage and is [Snared] for 1 turn.
      Cooldown: 4 turns
  
  - Smoke Bomb (2 Stamina):
      Range: 3 tiles
      Effect: Creates [Smoke] effect in 2x2 area. Obscures vision for 2 turns. Tinker can use to escape.
      Cooldown: 5 turns
  
  - Salvage Expertise (Passive):
      Effect: When killed near [Scrap Heap] or industrial debris, guaranteed to drop Tier 2-3 component.

Loot Table:
  - Intact Servomotor (70% chance)
  - Power Relay Circuit (50% chance)
  - Ball Bearings (40% chance)
  - Industrial Servo Actuator (8% chance)

Spawn Weight: 0.7 (Uncommon)
```

**v5.0 Narrative Frame:**

"Rival scavenger with improvised tech. Not evil - just competition for resources. Uses traps and ranged attacks. Prefers to avoid direct combat. Valuable loot if defeated."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'Scrap-Tinker',
    'Humanoid_Scavenger',
    3,
    35,
    10,
    1,
    5,
    4,
    0.7, -- Uncommon
    'Rival scavenger with improvised tech. Ranged crossbow attacks (6 tiles). Deploys [Scrap Trap] (2d8 + Snare). Smoke bomb for escape. 25% Energy Resistance. Fire Vulnerable (-10%). Guaranteed Tier 2-3 loot drop. High initiative.'
);
```

---

### F. Iron-Husked Boar (Beast - Low)

**Conceptual Identity:**

Mutated Svin-fylking that have incorporated scrap metal into their biology through Blight exposure.

**Stat Block:**

```yaml
Enemy: Iron-Husked Boar
Archetype: Beast (Mutated)
Tier: 2 (Early-mid game)
HP: 45
Defense: 9
Physical Soak: 5 (metal-infused hide)
Movement: 6 tiles
Initiative: +2

Resistances:
  Physical: 30% (metal hide)
  Energy: 0%
  Psychic: 0%
  Fire: 0%

Abilities:
  - Gore Charge (2 Stamina):
      Range: 4 tiles (charge)
      Damage: 2d8 + 4 Physical damage
      Effect: Charges in straight line. Targets in path take damage and are knocked back 1 tile.
      Cooldown: 2 turns
  
  - Thrashing Strike (1 Stamina):
      Range: Melee
      Damage: 1d10 + 2 Physical damage
      Effect: Wild thrashing attack.
      Cooldown: None
  
  - Metal Hide (Passive):
      Effect: Physical Soak 5. Scrap metal has fused with hide through Blight exposure. Reduces most attacks significantly.
  
  - Feral Rage (Passive):
      Effect: When reduced below 50% HP, gains +2 to attack rolls and +1 movement.

Loot Table:
  - Rusted Scrap Metal (80% chance)
  - Iron-Husked Hide (crafting material) (50% chance)
  - Beast Meat (survival) (30% chance)

Spawn Weight: 0.8 (Low)
```

**v5.0 Narrative Frame:**

"Mutated beast that has incorporated scrap metal into its biology. Not Undying - still organic, but Blight-corrupted. Makes dens in scrap heaps. Physical Soak from metal-infused hide."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, physical_soak,
    movement_range, initiative_bonus, spawn_weight, description
)
VALUES (
    7,
    'Iron-Husked Boar',
    'Beast_Mutated',
    2,
    45,
    9,
    5, -- Metal-infused hide
    6,
    2,
    0.8, -- Low spawn
    'Mutated Svin-fylking with scrap metal fused to hide. Physical Soak 5 (metal hide). 30% Physical Resistance. Charge attack (4 tiles, knockback). Feral Rage at <50% HP (+2 attack, +1 movement). Fast and aggressive. Not Undying - still organic.'
);
```

---

## III. Spawn Weight System

### Weighted Spawn Distribution

**Total Spawn Weights:**

- Rusted Servitor: 2.0 (Very common)
- Rusted Warden: 1.5 (Common)
- God-Sleeper Cultist: 1.3 (Medium)
- Iron-Husked Boar: 0.8 (Low)
- Scrap-Tinker: 0.7 (Uncommon)
- Draugr Juggernaut: 0.6 (Rare elite)

**Total Weight: 6.9**

**Spawn Probability:**

- Rusted Servitor: ∼29% (Undying)
- Rusted Warden: ∼22% (Undying)
- God-Sleeper Cultist: ∼19% (Humanoid)
- Iron-Husked Boar: ∼12% (Beast)
- Scrap-Tinker: ∼10% (Humanoid)
- Draugr Juggernaut: ∼9% (Undying Elite)

**Undying Dominance:**

- Total Undying weight: 4.1 / 6.9 = ∼60%
- Matches v2.0 "graveyard of giants" theme
- Humanoid: ∼29%, Beast: ∼12%

---

## IV. Physical Soak Patterns

### Design Philosophy

Jötunheim enemies have **universally high Physical Soak** because they are industrial constructs or scrap-adapted creatures:

**Soak Tiers:**

- Draugr Juggernaut: 10 (massive armor - TEACHING MECHANIC)
- Rusted Warden: 6 (high armor)
- Iron-Husked Boar: 5 (metal-infused hide)
- Rusted Servitor: 4 (moderate armor)
- God-Sleeper Cultist: 2 (light armor)
- Scrap-Tinker: 1 (minimal armor)

**Design Intent:**

- **Teaches armor-shredding tactics** (Draugr Juggernaut is centerpiece)
- Physical Soak makes brute force ineffective
- Encourages Iron-Bane (+2d6 vs. Corrupted/Constructs)
- Encourages Rust-Witch (armor degradation)
- Fire Vulnerability provides alternative path

**Counter-Balance:**

- All Undying are Fire Vulnerable (-15% to -25%)
- Low HP pools relative to armor (can be burst down)
- Positioning and environmental hazards still effective

---

## V. v5.0 Setting Compliance

### Technology, Not Supernatural

**All enemy descriptions emphasize:**

- **Rusted Servitor:** "Maintenance unit," not "undead janitor"
- **Rusted Warden:** "Security construct," not "guardian spirit"
- **Draugr Juggernaut:** "Heavy assault construct," not "armored revenant"
- **God-Sleeper Cultist:** "Cargo cultist," not "priest of metal gods"
- **Iron-Husked Boar:** "Blight-mutated beast," not "cursed creature"

### Voice Layer Examples

**❌ v2.0 Language:**

- "Risen from the dead"
- "Cursed by the fall of the titans"
- "Animated by dark magic"

**✅ v5.0 Language:**

- "Executing corrupted protocols"
- "Following 800-year-old automation"
- "Blight-corrupted industrial unit"

---

## VI. Testing Requirements

### Unit Tests

**Test 1: Spawn Weight Distribution**

```csharp
[Fact]
public void GenerateJotunheimEnemyGroup_1000Spawns_MatchesExpectedDistribution()
{
    var counts = new Dictionary<string, int>();
    
    for (int i = 0; i < 1000; i++)
    {
        var enemies = _biomeService.GenerateJotunheimEnemyGroup(difficulty: 2);
        foreach (var enemy in enemies)
        {
            if (!counts.ContainsKey([enemy.Name](http://enemy.Name)))
                counts[[enemy.Name](http://enemy.Name)] = 0;
            counts[[enemy.Name](http://enemy.Name)]++;
        }
    }
    
    // Expected: Undying dominance (~60%)
    var undyingCount = counts["Rusted Servitor"] + counts["Rusted Warden"] + counts["Draugr Juggernaut"];
    var totalCount = counts.Values.Sum();
    
    var undyingPercentage = (double)undyingCount / totalCount;
    Assert.InRange(undyingPercentage, 0.55, 0.65); // ~60% Undying
}
```

**Test 2: Physical Soak Application**

```csharp
[Fact]
public void DraugrJuggernaut_TakesDamage_PhysicalSoak10Applies()
{
    var juggernaut = CreateEnemyByName("Draugr Juggernaut");
    var damage = 20;
    
    var actualDamage = juggernaut.TakeDamage(damage, "Physical");
    
    // Physical Soak 10 + 50% Resistance
    // 20 - 10 (Soak) = 10
    // 10 * 0.5 (Resistance) = 5 actual damage
    Assert.Equal(5, actualDamage);
}
```

**Test 3: Fortress Protocol (Draugr Teaching Mechanic)**

```csharp
[Fact]
public void DraugrJuggernaut_LowDamageAttack_TakesHalfDamage()
{
    var juggernaut = CreateEnemyByName("Draugr Juggernaut");
    var lowDamage = 12; // Below 15 threshold
    
    var actualDamage = juggernaut.TakeDamage(lowDamage, "Physical");
    
    // Should be halved by Fortress Protocol
    // Then Physical Soak and Resistance apply
    Assert.InRange(actualDamage, 0, 2); // Heavily reduced
}
```

**Test 4: God-Sleeper Jötun Attunement**

```csharp
[Fact]
public void GodSleeperCultist_OnJotunCorpseTerrain_GainsPowerBoost()
{
    var cultist = CreateEnemyByName("God-Sleeper Cultist");
    var terrain = CreateJotunCorpseTerrain();
    
    var initialHP = cultist.HP;
    var initialRollBonus = cultist.GetRollBonus();
    
    _jotunheimService.ApplyJotunAttunement(cultist, terrain);
    
    Assert.Equal(initialHP + 10, cultist.HP);
    Assert.Equal(initialRollBonus + 4, cultist.GetRollBonus());
}
```

---

## VII. Success Criteria

### Functional Requirements

- [ ]  All 6 enemy types spawn correctly in Jötunheim
- [ ]  Spawn weights match expected distribution (∼60% Undying)
- [ ]  Physical Soak applies correctly for all enemies
- [ ]  Draugr Juggernaut Fortress Protocol works (<15 damage halved)
- [ ]  Fire Vulnerability applies to all Undying
- [ ]  God-Sleeper Cultist Jötun Attunement triggers on corpse terrain
- [ ]  Scrap-Tinker traps deploy correctly
- [ ]  Iron-Husked Boar Feral Rage triggers at <50% HP

### Quality Gates

- [ ]  85%+ unit test coverage
- [ ]  All enemy definitions use v5.0 voice
- [ ]  ASCII-only entity names
- [ ]  Loot tables have correct probabilities
- [ ]  Physical Soak values balance combat

### Balance Validation

- [ ]  Physical Soak creates need for armor-shredding
- [ ]  Draugr Juggernaut feels like teaching enemy (not unfair)
- [ ]  Fire Vulnerability provides viable alternative path
- [ ]  Undying dominance feels thematic (industrial graveyard)
- [ ]  Enemy variety creates tactical decisions
- [ ]  God-Sleeper Cultists dangerous near Jötun corpses

---

## VIII. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.32.3_enemy_definitions.sql
```

### Step 2: Compile Enemy Services

```bash
dotnet build Services/Enemies/RustedServitorAI.cs
dotnet build Services/Enemies/RustedWardenAI.cs
dotnet build Services/Enemies/DraugrJuggernautAI.cs
dotnet build Services/Enemies/GodSleeperCultistAI.cs
dotnet build Services/Enemies/ScrapTinkerAI.cs
dotnet build Services/Enemies/IronHuskedBoarAI.cs
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Jotunheim.EnemyTests"
```

### Step 4: Manual Verification

- Generate Jötunheim sector
- Verify enemy spawns match distribution (∼60% Undying)
- Engage Draugr Juggernaut
- Verify Physical Soak and Fortress Protocol
- Test God-Sleeper Cultist on Jötun corpse terrain
- Verify Fire Vulnerability on all Undying

---

## IX. Next Steps

Once v0.32.3 is complete:

**Proceed to v0.32.4:** Service Implementation & Testing

- JotunheimService complete implementation
- BiomeGenerationService.GenerateJotunheimSector()
- JotunCorpseTerrainService
- PowerConduitService integration
- Comprehensive unit test suite
- End-to-end integration testing

---

## X. Related Documents

**Parent Specification:**

- v0.32: Jötunheim Biome Implementation[[1]](v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)

**Reference Implementation:**

- v0.29.3: Muspelheim Enemies[[2]](v0%2029%203%20Enemy%20Definitions%20&%20Spawn%20System%208f34dab3be0e4869bd99f792215abd8a.md)
- v0.30.3: Niflheim Enemies[[3]](v0%2030%203%20Enemy%20Definitions%20&%20Spawn%20System%2098ff5d3b26b44f7db6b8bdf0843c77fe.md)
- v0.31.3: Alfheim Enemies[[4]](v0%2031%203%20Enemy%20Definitions%20&%20Spawn%20System%2014b78664d0ba43b8ac6ae5df815c2cce.md)

**Prerequisites:**

- v0.32.1: Database Schema[[5]](v0%2032%201%20Database%20Schema%20&%20Room%20Templates%20ffc37b6b82c1421bb1a599bdb61194d3.md)
- v0.32.2: Environmental Hazards[[6]](v0%2032%202%20Environmental%20Hazards%20&%20Industrial%20Terrain%2008ebb5b9a68843d6b1b607cfb4736edf.md)
- v0.21: Undying Enemy System[[7]](https://www.notion.so/v0-20-4-Advanced-Movement-Abilities-35e0ee82ed4344ad824d914d31de7e0a?pvs=21)

---

**Enemy definitions complete. Proceed to service implementation (v0.32.4).**