# v0.31.3: Enemy Definitions & Spawn System

Type: Technical
Description: Defines 5 Alfheim enemy types (Aether-Vulture, Energy Elemental, Forlorn Echo, Crystalline Construct, All-Rune's Echo boss), Energy/Aetheric resistance patterns.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.31.1-v0.31.2 (Database, Environmental)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.31: Alfheim Biome Implementation (v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.31.3-ENEMIES

**Parent Specification:** v0.31 Alfheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 10-15 hours

**Prerequisites:** v0.31.1 (Database), v0.31.2 (Environmental systems), v0.30.3 (Niflheim enemies as reference)

---

## I. Overview

This specification defines all enemy types for the Alfheim biome, including 5 distinct enemy types with complete mechanical definitions, spawn weights, Energy/Aetheric resistance patterns, and the All-Rune's Echo boss framework.

### Core Deliverables

- **5 Enemy Type Definitions** with complete stat blocks
- **Biome_EnemySpawns** weighted spawn system
- **Energy/Aetheric Resistance** patterns (high resistance standard)
- **Psychic Damage** mechanics for Forlorn Echo
- **All-Rune's Echo** boss framework (full fight in v0.35)
- **Spawn weight system** integrated with BiomeGenerationService

---

## II. Enemy Type Definitions

### A. Aether-Vulture

**Conceptual Identity:**

Energy-based aerial predators that evolved in the high-Aetheric environment. Not natural birds, but creatures whose biology has adapted to pure energy consumption over 800 years.

**Mechanical Definition:**

**Stat Block:**

```yaml
Enemy: Aether-Vulture
Archetype: Aerial Predator
Tier: 3 (Mid-game threat)
HP: 45
Defense: 12
Movement: 6 tiles (Flight)
Initiative: +4

Resistances:
  Energy: 75%
  Physical: 0%
  Psychic: 25%
  Fire: 0%
  Cold: 0%

Abilities:
  - Energy Dive (2 Stamina):
      Range: 4 tiles
      Damage: 2d8 + 4 Energy damage
      Effect: Aether-Vulture dives at target, dealing Energy damage. If target has active Mystic buffs, +50% damage.
      Cooldown: 2 turns
  
  - Aetheric Screech (3 Stamina):
      Range: 3 tile cone
      Damage: 1d6 Energy damage
      Effect: All targets in cone take damage and lose 5 Aether Pool (Mystics only).
      Cooldown: 3 turns
  
  - Opportunistic Strike (Passive):
      Effect: Gains +2 attack vs. characters with <50% HP.

Loot Table:
  - Aetheric Residue (80% chance)
  - Energy Crystal Shard (40% chance)
  - Vulture Talon (crafting component) (15% chance)

Spawn Weight: 1.5 (Common)
```

**v5.0 Narrative Frame:**

"Scavenger species that evolved to feed on Aetheric energy leakage. Their biology has been transformed by centuries of exposure - they're barely recognizable as birds anymore. They hunt Mystics preferentially, drawn to active Aether manipulation."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, movement_range,
    initiative_bonus, spawn_weight, description
)
VALUES (
    6,
    'Aether-Vulture',
    'Aerial_Predator',
    3,
    45,
    12,
    6,
    4,
    1.5,
    'Energy-adapted aerial predator. Evolved to feed on Aetheric energy leakage over 800 years. Prioritizes Mystics and targets with active energy buffs. Flight allows superior positioning. 75% Energy Resistance.'
);

-- Resistances
INSERT INTO Enemy_Resistances (enemy_name, damage_type, resistance_percentage)
VALUES 
    ('Aether-Vulture', 'Energy', 75),
    ('Aether-Vulture', 'Psychic', 25);

-- Abilities
INSERT INTO Enemy_Abilities (enemy_name, ability_name, stamina_cost, range, damage_dice, damage_type, cooldown, description)
VALUES
    ('Aether-Vulture', 'Energy Dive', 2, 4, '2d8+4', 'Energy', 2, 'Diving attack. +50% damage vs. targets with active Mystic buffs.'),
    ('Aether-Vulture', 'Aetheric Screech', 3, 3, '1d6', 'Energy', 3, 'Cone attack. Targets lose 5 Aether Pool (Mystics only).');
```

---

### B. Energy Elemental

**Conceptual Identity:**

Aetheric manifestations from ruptured containment fields. Not sentient beings, but emergent behavior from chaotic energy systems. Think "walking reactor breach" not "elemental spirit."

**Mechanical Definition:**

**Stat Block:**

```yaml
Enemy: Energy Elemental
Archetype: Aetheric Manifestation
Tier: 4 (High-level threat)
HP: 60
Defense: 10
Movement: 4 tiles (Hovering)
Initiative: +2

Resistances:
  Energy: 90% (Near-immunity)
  Physical: 50% (Partially incorporeal)
  Psychic: 0%
  Fire: 25%
  Cold: 25%

Abilities:
  - Discharge Pulse (2 Stamina):
      Range: 2 tiles (AoE around self)
      Damage: 2d10 Energy damage
      Effect: All adjacent characters take Energy damage. Energy Elemental loses 10 HP (self-damage).
      Cooldown: 1 turn
  
  - Energy Beam (3 Stamina):
      Range: 6 tiles (Line)
      Damage: 3d8 Energy damage
      Effect: Fires concentrated beam. Ignores cover.
      Cooldown: 2 turns
  
  - Unstable Core (Passive):
      Effect: When Energy Elemental dies, explodes for 2d12 Energy damage in 2 tile radius.
  
  - Aether Absorption (Reaction):
      Effect: When hit by Mystic ability, absorbs 50% of damage as healing instead.

Loot Table:
  - Aetheric Residue (100% chance)
  - Energy Crystal Shard (60% chance)
  - Unstable Aether Sample (30% chance)
  - Pure Aether Shard (5% chance)

Spawn Weight: 1.0 (Uncommon)
```

**v5.0 Narrative Frame:**

"Emergent phenomenon from ruptured Aetheric containment. Not alive in any meaningful sense - more like a walking feedback loop. Extremely dangerous. Mystic attacks feed it. Explodes catastrophically when containment fails (death)."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, movement_range,
    initiative_bonus, spawn_weight, description
)
VALUES (
    6,
    'Energy Elemental',
    'Aetheric_Manifestation',
    4,
    60,
    10,
    4,
    2,
    1.0,
    'Manifestation from ruptured Aetheric containment. Walking energy feedback loop. 90% Energy Resistance, absorbs 50% of Mystic damage as healing. Explodes on death (2d12 Energy damage, 2 tile radius). Do not engage with Mystic abilities.'
);

-- Resistances
INSERT INTO Enemy_Resistances (enemy_name, damage_type, resistance_percentage)
VALUES 
    ('Energy Elemental', 'Energy', 90),
    ('Energy Elemental', 'Physical', 50),
    ('Energy Elemental', 'Fire', 25),
    ('Energy Elemental', 'Cold', 25);

-- Abilities
INSERT INTO Enemy_Abilities (enemy_name, ability_name, stamina_cost, range, damage_dice, damage_type, cooldown, description)
VALUES
    ('Energy Elemental', 'Discharge Pulse', 2, 2, '2d10', 'Energy', 1, 'AoE around self. Energy Elemental takes 10 HP self-damage.'),
    ('Energy Elemental', 'Energy Beam', 3, 6, '3d8', 'Energy', 2, 'Line attack. Ignores cover.'),
    ('Energy Elemental', 'Unstable Core', 0, 0, '2d12', 'Energy', 0, 'Passive. On death, explodes in 2 tile radius.');
```

---

### C. Forlorn Echo

**Conceptual Identity:**

The Original Dead - the most powerful psychic echoes. These are the consciousness-fragments of the scientists who compiled the All-Rune and died in the Great Silence. Older, stronger, and more coherent than any other Forlorn.

**Mechanical Definition:**

**Stat Block:**

```yaml
Enemy: Forlorn Echo
Archetype: Psychic Entity (Elite)
Tier: 4 (High-level threat)
HP: 55
Defense: 11
Movement: 5 tiles (Hovering)
Initiative: +5

Resistances:
  Energy: 50%
  Physical: 75% (Mostly incorporeal)
  Psychic: 100% (Immune)
  Fire: 50%
  Cold: 50%

Abilities:
  - Memory Fragment Strike (2 Stamina):
      Range: 4 tiles
      Damage: 2d8 Psychic damage
      Effect: Target gains +8 Psychic Stress. If target reaches Stress threshold during combat, Forlorn Echo heals 15 HP.
      Cooldown: 1 turn
  
  - Reality Echo (3 Stamina):
      Range: 3 tiles
      Damage: 3d6 Psychic damage
      Effect: Target is teleported 1d4 tiles in random direction (Reality Tear effect). Target gains +5 Corruption.
      Cooldown: 3 turns
  
  - Psychic Resonance Amplification (Passive):
      Effect: All party members gain +3 Psychic Stress per turn while Forlorn Echo is alive.
  
  - Last Moments (Passive):
      Effect: Forlorn Echo cannot be reduced below 1 HP until all other enemies are defeated.

Loot Table:
  - Holographic Data Fragment (70% chance)
  - Psychic Residue (crafting component) (40% chance)
  - Paradox-Touched Component (20% chance)
  - Fragment of the All-Rune (1% chance - extremely rare)

Spawn Weight: 0.6 (Rare, elite)
```

**v5.0 Narrative Frame:**

"The Original Dead. Psychic fragments of the researchers who crashed reality. More coherent and powerful than any other Forlorn - they remember what they did. They're reliving their final moments in an endless loop. The psychic pressure from their presence is overwhelming."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, movement_range,
    initiative_bonus, spawn_weight, description
)
VALUES (
    6,
    'Forlorn Echo',
    'Psychic_Entity_Elite',
    4,
    55,
    11,
    5,
    5,
    0.6,
    'The Original Dead - psychic fragments of All-Rune researchers. Most powerful Forlorn variants. Deal Psychic damage, apply Stress and Corruption. Amplify [Psychic Resonance] (+3 Stress per turn). Cannot die until other enemies defeated. 100% Psychic Immunity, 75% Physical Resistance.'
);

-- Resistances
INSERT INTO Enemy_Resistances (enemy_name, damage_type, resistance_percentage)
VALUES 
    ('Forlorn Echo', 'Energy', 50),
    ('Forlorn Echo', 'Physical', 75),
    ('Forlorn Echo', 'Psychic', 100),
    ('Forlorn Echo', 'Fire', 50),
    ('Forlorn Echo', 'Cold', 50);

-- Abilities
INSERT INTO Enemy_Abilities (enemy_name, ability_name, stamina_cost, range, damage_dice, damage_type, cooldown, description)
VALUES
    ('Forlorn Echo', 'Memory Fragment Strike', 2, 4, '2d8', 'Psychic', 1, 'Psychic attack. +8 Stress. Heals Forlorn 15 HP if target reaches Stress threshold.'),
    ('Forlorn Echo', 'Reality Echo', 3, 3, '3d6', 'Psychic', 3, 'Teleports target 1d4 tiles randomly. +5 Corruption.');
```

---

### D. Crystalline Construct

**Conceptual Identity:**

Pre-Glitch architectural structures animated by Aetheric Blight. Not golems or magical constructs - these are load-bearing columns and support beams that have gained hostile agency through 800 years of Blight exposure.

**Mechanical Definition:**

**Stat Block:**

```yaml
Enemy: Crystalline Construct
Archetype: Animated Structure
Tier: 3 (Mid-game threat)
HP: 80
Defense: 15 (Heavy armor)
Movement: 3 tiles (Slow)
Initiative: +0

Resistances:
  Energy: 50%
  Physical: 25%
  Psychic: 100% (Immune - not sentient)
  Fire: -25% (Vulnerable)
  Cold: 0%

Abilities:
  - Crystal Slam (2 Stamina):
      Range: Melee
      Damage: 3d8 + 6 Physical damage
      Effect: Target is knocked back 2 tiles. If target collides with wall/obstacle, +1d8 damage.
      Cooldown: 1 turn
  
  - Shard Spray (3 Stamina):
      Range: 4 tiles (Cone)
      Damage: 2d6 Physical damage
      Effect: All targets in cone take damage. Targets bleed for 1d4 damage per turn for 3 turns.
      Cooldown: 3 turns
  
  - Reflective Surface (Passive):
      Effect: Reflects 25% of Energy damage back at attacker.
  
  - Structural Integrity (Passive):
      Effect: Takes half damage from attacks that deal <10 damage (armor too thick).

Loot Table:
  - Energy Crystal Shard (90% chance)
  - Crystallized Aether (15% chance)
  - Structural Component (crafting) (50% chance)
  - Pure Aether Shard (8% chance)

Spawn Weight: 1.2 (Common)
```

**v5.0 Narrative Frame:**

"Pre-Glitch architecture animated by Aetheric Blight. These aren't creatures - they're support columns and structural elements that have gained hostile agency. Slow but incredibly durable. The Blight has fused with the crystal lattice structure."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, movement_range,
    initiative_bonus, spawn_weight, description
)
VALUES (
    6,
    'Crystalline Construct',
    'Animated_Structure',
    3,
    80,
    15,
    3,
    0,
    1.2,
    'Pre-Glitch architecture animated by Aetheric Blight. Not creatures, but load-bearing structures with hostile agency. High HP, high Defense, slow movement. Reflects 25% Energy damage. Vulnerable to Fire (-25%). Takes half damage from weak attacks (<10 damage).'
);

-- Resistances
INSERT INTO Enemy_Resistances (enemy_name, damage_type, resistance_percentage)
VALUES 
    ('Crystalline Construct', 'Energy', 50),
    ('Crystalline Construct', 'Physical', 25),
    ('Crystalline Construct', 'Psychic', 100),
    ('Crystalline Construct', 'Fire', -25);

-- Abilities
INSERT INTO Enemy_Abilities (enemy_name, ability_name, stamina_cost, range, damage_dice, damage_type, cooldown, description)
VALUES
    ('Crystalline Construct', 'Crystal Slam', 2, 1, '3d8+6', 'Physical', 1, 'Melee attack. Knockback 2 tiles. +1d8 damage if target collides with obstacle.'),
    ('Crystalline Construct', 'Shard Spray', 3, 4, '2d6', 'Physical', 3, 'Cone attack. Applies Bleed (1d4 per turn for 3 turns).');
```

---

### E. All-Rune's Echo (Boss Framework)

**Conceptual Identity:**

The sentient Reality Glitch at the heart of Alfheim. Not the All-Rune itself (which was compiled and crashed reality), but the **echo** - the persistent error in reality's operating system. This is the closest thing to a "demon" in the setting, but it's explicitly technological failure, not supernatural.

**Mechanical Definition (Framework Only - Full Fight in v0.35):**

**Stat Block:**

```yaml
Enemy: All-Rune's Echo
Archetype: Reality Glitch (Boss)
Tier: 5 (Boss encounter)
HP: 200 (Phase 1), 150 (Phase 2)
Defense: 14
Movement: 5 tiles (Hovering)
Initiative: +6

Resistances:
  Energy: 80%
  Physical: 60%
  Psychic: 75%
  Fire: 40%
  Cold: 40%

Phase 1 Abilities:
  - Paradox Weave (3 Stamina):
      Range: 5 tiles
      Damage: 3d10 Energy damage
      Effect: Target and all adjacent targets take damage. Applies [Reality Distortion] (-2 to all rolls for 2 turns).
      Cooldown: 2 turns
  
  - Compile Error (4 Stamina):
      Range: 4 tiles (AoE)
      Damage: 4d8 Psychic damage
      Effect: All Mystics in range lose 10 Aether Pool. All non-Mystics gain +10 Stress.
      Cooldown: 3 turns
  
  - Reality Recursion (Passive):
      Effect: At 50% HP, triggers Phase 2. Heals to full HP, summons 2 Reality Fragments (minions).

Phase 2 Abilities:
  - All-Rune Manifestation (5 Stamina):
      Range: 6 tiles
      Damage: 5d10 mixed (Energy + Psychic)
      Effect: Ignores all resistances. Target gains +10 Corruption.
      Cooldown: 4 turns
  
  - System Crash (Ultimate):
      Range: Full battlefield
      Damage: 6d8 to all characters
      Effect: Used at <25% HP. All Reality Tears activate simultaneously. All characters teleported randomly.
      Cooldown: Once per fight

Loot Table:
  - Fragment of the All-Rune (100% chance)
  - Reality Anchor Core (50% chance)
  - Crystallized Aether (80% chance)
  - Unique boss loot (TBD in v0.35)

Spawn Weight: 0.0 (Scripted encounter only)
```

**v5.0 Narrative Frame:**

"The sentient error at reality's core. When researchers compiled the paradoxical All-Rune, it created a permanent Logic Bomb in the universe's operating system. This is the Echo - the persistent manifestation of that crash. It doesn't want anything. It simply is. And its existence is agony for reality itself."

**Database Entry:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type, tier, hp, defense, movement_range,
    initiative_bonus, spawn_weight, description
)
VALUES (
    6,
    'All-Rune''s Echo',
    'Reality_Glitch_Boss',
    5,
    200,
    14,
    5,
    6,
    0.0, -- Scripted encounter only
    'BOSS: Sentient Reality Glitch. The persistent error from compiling the paradoxical All-Rune. Two-phase fight (200 HP → 150 HP). High resistances across the board. Deals mixed Energy/Psychic damage, applies Corruption, teleports party. Ultimate "System Crash" at <25% HP. Scripted encounter in All-Rune Proving Ground.'
);

-- Resistances
INSERT INTO Enemy_Resistances (enemy_name, damage_type, resistance_percentage)
VALUES 
    ('All-Rune''s Echo', 'Energy', 80),
    ('All-Rune''s Echo', 'Physical', 60),
    ('All-Rune''s Echo', 'Psychic', 75),
    ('All-Rune''s Echo', 'Fire', 40),
    ('All-Rune''s Echo', 'Cold', 40);

-- Abilities (Phase 1)
INSERT INTO Enemy_Abilities (enemy_name, ability_name, stamina_cost, range, damage_dice, damage_type, cooldown, description)
VALUES
    ('All-Rune''s Echo', 'Paradox Weave', 3, 5, '3d10', 'Energy', 2, 'AoE attack. Applies [Reality Distortion] (-2 to all rolls for 2 turns).'),
    ('All-Rune''s Echo', 'Compile Error', 4, 4, '4d8', 'Psychic', 3, 'Mystics lose 10 Aether Pool. Non-Mystics gain +10 Stress.'),
    ('All-Rune''s Echo', 'All-Rune Manifestation', 5, 6, '5d10', 'Mixed', 4, 'Phase 2 only. Ignores resistances. +10 Corruption.'),
    ('All-Rune''s Echo', 'System Crash', 10, 99, '6d8', 'Mixed', 0, 'Ultimate. <25% HP. Teleports all characters, activates all Reality Tears.');
```

---

## III. Spawn Weight System

### Weighted Spawn Distribution

**Total Spawn Weights:**

- Aether-Vulture: 1.5 (Common)
- Energy Elemental: 1.0 (Uncommon)
- Forlorn Echo: 0.6 (Rare, elite)
- Crystalline Construct: 1.2 (Common)
- All-Rune's Echo: 0.0 (Scripted only)

**Total Weight: 4.3**

**Spawn Probability:**

- Aether-Vulture: ~35%
- Crystalline Construct: ~28%
- Energy Elemental: ~23%
- Forlorn Echo: ~14%

### Difficulty Scaling

**Enemy Count by Difficulty:**

```yaml
Easy:
  Total Enemies: 2-3
  Max Elite: 0
  
Normal:
  Total Enemies: 3-4
  Max Elite: 1
  
Hard:
  Total Enemies: 4-5
  Max Elite: 2
  
Deadly:
  Total Enemies: 5-6
  Max Elite: 2-3
```

**Service Implementation:**

```csharp
public List<Enemy> GenerateAlfheimEnemyGroup(int difficulty)
{
    var weights = new Dictionary<string, double>
    {
        { "Aether-Vulture", 1.5 },
        { "Energy Elemental", 1.0 },
        { "Forlorn Echo", 0.6 },
        { "Crystalline Construct", 1.2 }
    };
    
    int enemyCount = difficulty switch
    {
        1 => _diceService.Roll(2, 3), // Easy
        2 => _diceService.Roll(3, 4), // Normal
        3 => _diceService.Roll(4, 5), // Hard
        4 => _diceService.Roll(5, 6), // Deadly
        _ => 3
    };
    
    int maxElite = difficulty switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        4 => 3,
        _ => 1
    };
    
    var enemies = new List<Enemy>();
    int eliteCount = 0;
    
    for (int i = 0; i < enemyCount; i++)
    {
        // Roll weighted spawn
        var enemyType = _diceService.RollWeighted(weights);
        
        // Limit elite spawns
        if (enemyType == "Forlorn Echo")
        {
            if (eliteCount >= maxElite)
            {
                enemyType = "Crystalline Construct"; // Replace with common
            }
            else
            {
                eliteCount++;
            }
        }
        
        enemies.Add(CreateEnemy(enemyType));
    }
    
    return enemies;
}
```

---

## IV. Energy Resistance Patterns

### Design Philosophy

Alfheim enemies have **universally high Energy Resistance** because they evolved/manifested in high-Aetheric environment:

**Resistance Tiers:**

- Energy Elemental: 90% (near-immune)
- Aether-Vulture: 75% (high)
- All-Rune's Echo: 80% (boss)
- Crystalline Construct: 50% (moderate)
- Forlorn Echo: 50% (moderate)

**Design Intent:**

- Mystics are challenged but not invalidated
- Energy-based attacks less effective than usual
- Encourages tactical diversity (Physical damage, conditions, positioning)
- Thematic: creatures adapted to Aetheric environment

**Counter-Balance:**

- Mystics gain +10% Aether Pool from [Runic Instability]
- Wild Magic Surges can increase damage (+50% surge)
- Mystic utility (buffs, debuffs, positioning) still valuable

---

## V. v5.0 Setting Compliance

### Technology, Not Magic

**All enemy descriptions emphasize:**

- **Aether-Vulture:** "Evolved to feed on energy" (biology), not "magical creature"
- **Energy Elemental:** "Ruptured containment manifestation" (industrial failure), not "summoned elemental"
- **Forlorn Echo:** "Psychic fragment of researcher" (consciousness echo), not "ghost"
- **Crystalline Construct:** "Animated by Aetheric Blight" (infection), not "golem"
- **All-Rune's Echo:** "Sentient Reality Glitch" (system error), not "demon"

### Voice Layer Examples

**❌ v2.0 Language:**

- "Summoned by dark magic"
- "Cursed spirit"
- "Elemental being from another plane"

**✅ v5.0 Language:**

- "Emerged from containment breach"
- "Psychic echo of pre-Glitch researcher"
- "Manifestation of system failure"

---

## VI. Testing Requirements

### Unit Tests

**Test 1: Spawn Weight Distribution**

```csharp
[Fact]
public void GenerateAlfheimEnemyGroup_1000Spawns_MatchesExpectedDistribution()
{
    var counts = new Dictionary<string, int>();
    
    for (int i = 0; i < 1000; i++)
    {
        var enemies = _biomeService.GenerateAlfheimEnemyGroup(difficulty: 2);
        foreach (var enemy in enemies)
        {
            if (!counts.ContainsKey([enemy.Name](http://enemy.Name)))
                counts[[enemy.Name](http://enemy.Name)] = 0;
            counts[[enemy.Name](http://enemy.Name)]++;
        }
    }
    
    // Expected distribution (approximate):
    // Aether-Vulture: ~35% (1050)
    // Crystalline Construct: ~28% (840)
    // Energy Elemental: ~23% (690)
    // Forlorn Echo: ~14% (420)
    
    Assert.InRange(counts["Aether-Vulture"], 900, 1200);
    Assert.InRange(counts["Crystalline Construct"], 700, 1000);
    Assert.InRange(counts["Energy Elemental"], 550, 850);
    Assert.InRange(counts["Forlorn Echo"], 300, 550);
}
```

**Test 2: Energy Resistance Application**

```csharp
[Fact]
public void EnergyElemental_TakesEnergyDamage_Applies90PercentResistance()
{
    var elemental = CreateEnemyByName("Energy Elemental");
    var damage = 100;
    
    var actualDamage = elemental.TakeDamage(damage, "Energy");
    
    Assert.Equal(10, actualDamage); // 90% resisted
}
```

**Test 3: Forlorn Echo Elite Status**

```csharp
[Fact]
public void GenerateAlfheimEnemyGroup_NormalDifficulty_LimitsEliteCount()
{
    var enemies = _biomeService.GenerateAlfheimEnemyGroup(difficulty: 2);
    
    var eliteCount = enemies.Count(e => [e.Name](http://e.Name) == "Forlorn Echo");
    
    Assert.InRange(eliteCount, 0, 1); // Max 1 elite at Normal
}
```

**Test 4: Boss Framework**

```csharp
[Fact]
public void AllRuneEcho_Phase1To Phase2_TriggersAt50PercentHP()
{
    var boss = CreateBoss("All-Rune's Echo");
    
    // Reduce to 50% HP
    boss.TakeDamage(100, "Physical");
    
    Assert.True(boss.CurrentPhase == 2);
    Assert.Equal(150, boss.HP); // Healed to Phase 2 HP
}
```

---

## VII. Success Criteria

### Functional Requirements

- [ ]  All 5 enemy types spawn correctly in Alfheim
- [ ]  Spawn weights match expected distribution
- [ ]  Energy resistances apply correctly
- [ ]  Aether-Vulture prioritizes Mystics
- [ ]  Energy Elemental absorbs Mystic damage
- [ ]  Forlorn Echo amplifies Psychic Resonance
- [ ]  Crystalline Construct reflects Energy damage
- [ ]  All-Rune's Echo transitions to Phase 2 at 50% HP
- [ ]  Elite spawn count respects difficulty limits

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  All enemy definitions use v5.0 voice
- [ ]  ASCII-only entity names
- [ ]  Loot tables have correct probabilities
- [ ]  Boss framework ready for v0.35 expansion

### Balance Validation

- [ ]  Energy resistance doesn't invalidate Mystics
- [ ]  Enemy variety creates tactical decisions
- [ ]  Forlorn Echo feels elite/threatening
- [ ]  Crystalline Construct is durable tank
- [ ]  Energy Elemental death explosion is impactful
- [ ]  Aether-Vulture mobility is advantageous

---

## VIII. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.31.3_enemy_definitions.sql
```

### Step 2: Compile Enemy Services

```bash
dotnet build Services/Enemies/AetherVultureAI.cs
dotnet build Services/Enemies/EnergyElementalAI.cs
dotnet build Services/Enemies/ForlornEchoAI.cs
dotnet build Services/Enemies/CrystallineConstructAI.cs
dotnet build Services/Enemies/AllRuneEchoBoss.cs
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Alfheim.EnemyTests"
```

### Step 4: Manual Verification

- Generate Alfheim sector
- Verify enemy spawns match expected distribution
- Engage each enemy type
- Verify resistances apply correctly
- Test Energy Elemental absorption
- Test Forlorn Echo Stress amplification
- Verify boss can be spawned (scripted)

---

## IX. Next Steps

Once v0.31.3 is complete:

**Proceed to v0.31.4:** Service Implementation & Testing

- AlfheimService complete implementation
- BiomeGenerationService.GenerateAlfheimSector()
- RunicInstabilityService integration
- RealityTearService integration
- Comprehensive unit test suite
- End-to-end integration testing

---

## X. Related Documents

**Parent Specification:**

- v0.31: Alfheim Biome Implementation[[1]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)

**Reference Implementation:**

- v0.29.3: Muspelheim Enemies[[2]](v0%2029%203%20Enemy%20Definitions%20&%20Spawn%20System%208f34dab3be0e4869bd99f792215abd8a.md)
- v0.30.3: Niflheim Enemies[[3]](v0%2030%203%20Enemy%20Definitions%20&%20Spawn%20System%2098ff5d3b26b44f7db6b8bdf0843c77fe.md)

**Prerequisites:**

- v0.31.1: Database Schema[[4]](v0%2031%201%20Database%20Schema%20&%20Room%20Templates%20b3f608de386941b5a8a45ddfa962641a.md)
- v0.31.2: Environmental Hazards[[5]](v0%2031%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%20babad7a1095a4acea8314f2e9162344c.md)

---

**Enemy definitions complete. Proceed to service implementation (v0.31.4).**