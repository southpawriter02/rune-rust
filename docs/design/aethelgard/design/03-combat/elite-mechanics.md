---
id: SPEC-COMBAT-019
title: "Elite & Champion Mechanics"
version: 1.0
status: draft
last-updated: 2025-12-14
parent-specification: SPEC-COMBAT-016
related-files:
  - path: "RuneAndRust.Engine/Services/EliteService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Services/ChampionAffixService.cs"
    status: Planned
  - path: "docs/03-combat/creature-traits.md"
    status: Implemented
  - path: "docs/03-combat/encounter-generation.md"
    status: Implemented
  - path: "docs/03-combat/spawn-scaling.md"
    status: Implemented
---

# Elite & Champion Mechanics

> "Some things in the dark have names. Names whispered by survivors. Names that mean 'run.'"

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-COMBAT-019` |
| Category | Combat System |
| Priority | Must-Have |
| Status | Draft |
| Parent | [SPEC-COMBAT-016](encounter-generation.md) |

### 1.2 Core Philosophy

The **Elite & Champion Mechanics** system elevates standard enemies into memorable threats through procedural enhancement. Elites are harder versions of standard enemies with bonus stats, while **Champions** are named elites with unique affix combinations that create distinct combat puzzles.

This system ensures that even familiar enemy types can surprise veterans, while providing meaningful loot incentives for engaging these enhanced threats rather than avoiding them.

**Design Pillars:**
- **Emergent Challenge**: Champions combine traits to create unique tactical problems
- **Risk-Reward**: Elite enemies offer significantly better loot for increased danger
- **Narrative Weight**: Named champions can become recurring threats with bounties
- **Readability**: Players can identify elite status and affixes through visual cues

---

## 2. Elite Hierarchy

### 2.1 Elite vs Champion vs Boss

| Type | Stat Bonus | Affixes | Naming | Spawn Context | Cost |
|------|-----------|---------|--------|---------------|------|
| **Standard** | +0% | 0 | None | Budget filler | 3 |
| **Elite** | +50% HP/Dmg | 0 | None | Rare spawn, pack leader | 6 |
| **Champion** | +50% HP/Dmg | 1-3 | Procedural title | Very rare, solo threat | 8-12 |
| **Boss** | Custom | Custom | Scripted | End of dungeon | 15+ |

### 2.2 Elite Stat Scaling

Elites apply a flat multiplier on top of TDR-scaled base stats:

```
Elite HP = Base HP × 1.5
Elite Damage = Base Damage × 1.5
Elite Defense = Base Defense + 2
Elite Soak = Base Soak + 1
Elite Legend Value = Base Legend × 2.0
```

**Example:**
```
Standard Skrap-Troll (TDR 40):
  HP: 45, Damage: 3d10+4, Defense: 3, Soak: 4

Elite Skrap-Troll (TDR 40):
  HP: 68, Damage: 4d10+6, Defense: 5, Soak: 5
  Legend: 80 (vs 40 base)
```

---

## 3. Champion System

### 3.1 Champion Generation

Champions are elites with procedurally assigned affixes from the [Creature Traits System](creature-traits.md).

**Generation Formula:**
```
Affix Count = 1 + floor(Depth / 3)
Max Affixes = 3

Affix Selection:
1. Roll affix category (weighted by biome)
2. Roll specific affix from category
3. Check for conflicts with existing affixes
4. If conflict, reroll (max 3 attempts, then skip)
```

### 3.2 Champion Affixes

Champions draw from a curated subset of traits designed for elite enhancement:

#### 3.2.1 Offensive Affixes

| Affix | Effect | Visual Cue | Counter-Play |
|-------|--------|------------|--------------|
| **Brutal** | Crits deal 3× damage (vs 2×) | Blood-red glow | Don't let it crit (high Defense) |
| **Relentless** | Attacks twice per turn (-2 Accuracy on second) | Blurred afterimages | CC effects, kiting |
| **Executioner** | +50% damage vs targets <25% HP | Skull icon over wounded | Keep HP above 25% |
| **Vampiric** | Heals 50% of damage dealt | Green life drain VFX | Burst damage, healing reduction |
| **ArmorPiercing** | Ignores 3 Soak | Glowing weapon edge | High HP pool over Soak |
| **Sweeping** | Basic attacks hit all adjacent enemies | Wide stance | Spread positioning |

#### 3.2.2 Defensive Affixes

| Affix | Effect | Visual Cue | Counter-Play |
|-------|--------|------------|--------------|
| **Regeneration** | Heals 5 HP/turn at start of turn | Green particles | Burst damage, healing reduction |
| **ShieldGenerator** | Starts with 15 temp HP (doesn't regen) | Blue energy shield | Focus fire early |
| **Reflective** | 20% of damage reflected to attacker | Mirror-like skin | Ranged attacks, DoTs |
| **AdaptiveArmor** | +2 Soak vs damage type after first hit | Shifting plates | Vary damage types |
| **Unstoppable** | Immune to Stun, Root, Slow | Heavy footsteps | Raw damage, positioning |
| **LastStand** | Cannot drop below 1 HP for first 2 turns | Golden glow | Patience, sustained damage |

#### 3.2.3 Mobility Affixes

| Affix | Effect | Visual Cue | Counter-Play |
|-------|--------|------------|--------------|
| **Swiftness** | +2 movement per turn | Speed lines | Zone control, roots |
| **Hit-and-Run** | Free 1-tile disengage after attack | Retreating stance | Chase, ranged attacks |
| **Phasing** | Can move through occupied tiles/obstacles | Translucent body | Area denial ineffective |
| **Burrowing** | Can move through impassable terrain | Dirt particles | Tremorsense, patience |
| **Flight** | +2 Defense vs melee, ignores ground hazards | Hovering | Ranged attacks, forced grounding |
| **RandomBlink** | Teleports to random valid tile each turn | Flickering | Predictive positioning |

#### 3.2.4 Special Affixes

| Affix | Effect | Visual Cue | Counter-Play |
|-------|--------|------------|--------------|
| **Explosive** | On death, 3d6 damage to all within 2 tiles | Pulsing red core | Kill at range, spread out |
| **SplitOnDeath** | Spawns 2 copies at 50% HP/damage | Segmented body | AoE, save resources |
| **MirrorImage** | Starts with 2 illusory duplicates (1 HP each) | Multiple overlapping | AoE reveals real target |
| **Resurrection** | If ally alive, returns at 50% HP after 2 turns | Ghostly tether | Kill adds first |
| **SymbioticLink** | Shares damage 50/50 with linked ally | Energy tether visible | AoE, or focus linked |
| **TimeLoop** | 25% chance to resurrect at 25% HP (once) | Temporal distortion | Overkill, finish fast |

### 3.3 Affix Conflicts

Certain affixes cannot appear together:

| Affix A | Affix B | Reason |
|---------|---------|--------|
| Anchored | RandomBlink | Contradictory movement |
| Flight | Burrowing | Contradictory terrain |
| Unstoppable | any CC-inflicting affix | Self-immunity irrelevant |
| Blind | any targeting bonus | No targeting benefit |
| LastStand | Explosive | Delays death trigger |

### 3.4 Affix Weights by Biome

| Biome | Offensive | Defensive | Mobility | Special |
|-------|-----------|-----------|----------|---------|
| Rust Wastes | 40% | 30% | 20% | 10% |
| Frozen Depths | 25% | 40% | 25% | 10% |
| Blight Marshes | 30% | 20% | 30% | 20% |
| Temporal Rift | 20% | 20% | 40% | 20% |
| Mechanical Hive | 35% | 35% | 15% | 15% |

---

## 4. Naming System

### 4.1 Procedural Title Generation

Champions receive generated names following the pattern:
```
[Title] [Base Name] [Epithet]
```

**Title Pool (by Affix Count):**
| Affix Count | Titles |
|-------------|--------|
| 1 | the Scarred, the Twisted, the Marked |
| 2 | the Dreaded, the Vicious, the Cursed |
| 3 | the Abomination, the Scourge, the Terror |

**Epithet Pool (by Primary Affix):**
| Affix Category | Epithets |
|----------------|----------|
| Offensive | "of Slaughter", "the Flayer", "Bloodied" |
| Defensive | "the Unyielding", "Ironhide", "the Eternal" |
| Mobility | "the Swift", "Shadowstep", "the Untouchable" |
| Special | "Deathless", "the Splitting", "Mirror-born" |

**Example Names:**
- "The Dreaded Skrap-Troll, Bloodied" (2 affixes, offensive focus)
- "The Scourge Rust-Wraith, Shadowstep" (3 affixes, mobility focus)
- "The Twisted Forlorn Shambler, Ironhide" (1 affix, defensive focus)

### 4.2 Name Persistence

Champion names are tracked per-session for narrative consistency:

```csharp
public class ChampionRecord {
    public string GeneratedName { get; set; }
    public string BaseEnemyType { get; set; }
    public List<ChampionAffix> Affixes { get; set; }
    public int TimesEncountered { get; set; }
    public int TimesDefeated { get; set; }
    public int TimesEscaped { get; set; }
    public bool HasBounty { get; set; }
}
```

---

## 5. Pack Leader Mechanics

### 5.1 Pack Leader Role

One Elite or Champion per encounter can be designated as **Pack Leader**, providing buffs to nearby allies.

**Pack Leader Aura (3-tile radius):**
- Allied Minions: +1 Accuracy, +1 Defense
- Allied Standards: +1 Accuracy

**Pack Leader Behavior:**
- Acts last in initiative order (coordinates pack)
- Prioritizes targets threatening minions
- Retreats if all pack members die

### 5.2 Pack Composition

| Pack Leader Type | Typical Composition |
|------------------|---------------------|
| Elite Standard | 2-4 Minions |
| Champion | 1 Elite + 2-3 Minions |
| Alpha Champion | 2 Elites + 4-6 Minions |

**Pack Bonus:**
```
Pack Legend Bonus = PackLeaderLegend × 0.25 × PackSize
```

---

## 6. Spawn Probability

### 6.1 Elite Spawn Chance

Elites have a base spawn chance modified by depth and biome danger:

```
EliteChance = BaseChance + (Depth × 2) + (BiomeDanger × 5)
ChampionChance = EliteChance × 0.3

Caps:
- Elite: Max 40%
- Champion: Max 12%
```

**Base Chances by Encounter Type:**
| Encounter Type | Elite Base | Champion Base |
|----------------|------------|---------------|
| Standard | 10% | 3% |
| Ambush | 15% | 5% |
| Patrol | 20% | 6% |
| Lair | 30% | 10% |
| Boss Room | 0% | 0% (boss only) |

### 6.2 Guaranteed Spawns

Certain conditions guarantee elite/champion presence:

| Condition | Result |
|-----------|--------|
| Treasure room | 1 Elite minimum |
| Named NPC bounty | 1 Champion (specific) |
| Cleared room re-entry | +20% Champion chance |
| Deadly difficulty ratio | 1 Elite minimum |

---

## 7. Loot Bonuses

### 7.1 Elite Loot Modifier

Elites and Champions have improved drop rates:

| Enemy Type | Legend Mult | Loot Quality Bonus | Extra Drops |
|------------|-------------|-------------------|-------------|
| Standard | 1.0× | +0 | 0 |
| Elite | 2.0× | +1 tier | +1 drop |
| Champion (1 affix) | 2.5× | +1 tier | +2 drops |
| Champion (2 affixes) | 3.0× | +2 tiers | +2 drops |
| Champion (3 affixes) | 4.0× | +2 tiers | +3 drops |

### 7.2 Affix-Specific Drops

Certain affixes guarantee thematic drops:

| Affix | Guaranteed Drop |
|-------|-----------------|
| Vampiric | Blood Vial (consumable) |
| ShieldGenerator | Shield Core (crafting) |
| Explosive | Volatile Core (crafting) |
| SplitOnDeath | Biomass Sample (crafting) |
| TimeLoop | Temporal Shard (rare crafting) |

---

## 8. Bounty System

### 8.1 Bounty Generation

Champions that escape combat (player flees or champion retreats) may become bounty targets:

**Bounty Trigger:**
```
If Champion.HP > 50% when party flees:
  BountyChance = 60%

If Champion escapes (Cowardly trait or low HP):
  BountyChance = 40%
```

### 8.2 Bounty Properties

| Property | Value |
|----------|-------|
| Spawn Zone | Same biome, random room |
| Stat Bonus | +10% HP/Damage (remembers the fight) |
| Affix Addition | May gain 1 new affix (25% chance) |
| Reward Bonus | +50% Legend, guaranteed unique drop |

### 8.3 Bounty Tracking

```sql
CREATE TABLE Champion_Bounties (
    bounty_id UUID PRIMARY KEY,
    champion_name TEXT NOT NULL,
    base_enemy_type TEXT NOT NULL,
    affixes JSONB NOT NULL,
    biome_id INTEGER NOT NULL,
    times_escaped INTEGER DEFAULT 1,
    legend_reward INTEGER NOT NULL,
    is_claimed BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_biome FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);
```

---

## 9. UI Requirements

### 9.1 Elite Indicators

```
Standard Enemy:        Elite Enemy:           Champion Enemy:
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│ Skrap-Troll  │       │ ⬥ Skrap-Troll│       │ ★ The Dreaded│
│ HP: ████░░░░ │       │ HP: ████████ │       │   Skrap-Troll│
│              │       │ [ELITE]      │       │ HP: ████████ │
└──────────────┘       └──────────────┘       │ [CHAMPION]   │
                                              │ ⚔ Brutal     │
                                              │ 🛡 Reflective│
                                              └──────────────┘
```

### 9.2 Color Coding

| Enemy Type | Name Color | Border Color |
|------------|------------|--------------|
| Standard | White | Gray |
| Elite | Yellow | Gold |
| Champion | Orange | Red |
| Bounty Target | Purple | Magenta |

### 9.3 Affix Icons

Each affix has a distinct icon displayed on the enemy frame:
- Offensive: ⚔ (red)
- Defensive: 🛡 (blue)
- Mobility: 💨 (green)
- Special: ✦ (purple)

---

## 10. Integration Points

### 10.1 Dependencies

| System | Dependency Type |
|--------|-----------------|
| [SPEC-COMBAT-016](encounter-generation.md) | Parent - spawn budget integration |
| [SPEC-COMBAT-017](spawn-scaling.md) | Reads - base stats to scale |
| [SPEC-COMBAT-015](creature-traits.md) | Uses - affix trait definitions |
| Loot System | Writes - enhanced drop tables |

### 10.2 Triggered By

| Trigger | Source |
|---------|--------|
| Enemy spawn | Encounter Generation rolls elite/champion |
| Bounty check | Combat Resolution on party flee |
| Name generation | Champion first spawn |

### 10.3 Modifies

| Target | Modification |
|--------|--------------|
| Enemy stats | Elite/Champion multipliers |
| Loot tables | Bonus drops and quality |
| AI behavior | Pack Leader coordination |

---

## 11. Service Architecture

### 11.1 Service Interface

```csharp
public interface IEliteService
{
    // Generation
    bool RollEliteSpawn(SpawnContext context);
    bool RollChampionSpawn(SpawnContext context);
    Champion GenerateChampion(EnemyType baseType, int depth, Biome biome);

    // Affixes
    List<ChampionAffix> SelectAffixes(int count, Biome biome);
    bool CheckAffixConflict(ChampionAffix a, ChampionAffix b);
    void ApplyAffixes(Champion champion, List<ChampionAffix> affixes);

    // Naming
    string GenerateChampionName(EnemyType baseType, List<ChampionAffix> affixes);

    // Pack Leadership
    void AssignPackLeader(Encounter encounter);
    void ApplyPackLeaderAura(Enemy leader, List<Enemy> pack);

    // Bounties
    void CheckBountyTrigger(Champion champion, CombatOutcome outcome);
    List<Bounty> GetActiveBounties(Biome biome);
}
```

### 11.2 Data Model

```csharp
public class Champion : Enemy
{
    public string GeneratedName { get; set; }
    public List<ChampionAffix> Affixes { get; set; } = new();
    public bool IsPackLeader { get; set; }
    public Guid? BountyId { get; set; }

    public float LegendMultiplier => 2.5f + (Affixes.Count * 0.5f);
    public int BonusDrops => 1 + Affixes.Count;
}

public enum ChampionAffix
{
    // Offensive
    Brutal, Relentless, Executioner, Vampiric, ArmorPiercing, Sweeping,

    // Defensive
    Regeneration, ShieldGenerator, Reflective, AdaptiveArmor, Unstoppable, LastStand,

    // Mobility
    Swiftness, HitAndRun, Phasing, Burrowing, Flight, RandomBlink,

    // Special
    Explosive, SplitOnDeath, MirrorImage, Resurrection, SymbioticLink, TimeLoop
}
```

---

## 12. Balance Data

### 12.1 Design Intent

Elite and Champion enemies should feel like "mini-boss" encounters within regular exploration. A Champion with 3 affixes should approach (but not exceed) the threat level of an actual boss encounter.

### 12.2 Power Budget

| Enemy Type | Effective TDR Multiplier | Expected Party Response |
|------------|-------------------------|-------------------------|
| Standard | 1.0× | Normal engagement |
| Elite | 1.5× | Focus fire, use resources |
| Champion (1) | 1.8× | Prioritize, tactical retreat acceptable |
| Champion (2) | 2.0× | Major threat, full commitment |
| Champion (3) | 2.3× | Near-boss threat, may flee if unprepared |

### 12.3 Encounter Budget Impact

| Enemy Type | Budget Cost | Max Per Encounter |
|------------|-------------|-------------------|
| Elite | 6 | 2 |
| Champion (1 affix) | 8 | 1 |
| Champion (2 affixes) | 10 | 1 |
| Champion (3 affixes) | 12 | 1 |

---

## 13. Voice Guidance

**Reference:** [npc-flavor.md](../../.templates/flavor-text/npc-flavor.md)

### 13.1 System Tone

| Context | Tone |
|---------|------|
| Elite Spawn | Warning, elevated threat |
| Champion Spawn | Dread, named threat |
| Bounty Target | Recognition, old enemy |

### 13.2 Feedback Text Examples

| Event | Text |
|-------|------|
| Elite detected | "Threat assessment elevated. This one has survived longer than most." |
| Champion appears | "ALERT: Named entity detected. {ChampionName}. Threat signatures indicate: {AffixList}." |
| Bounty target | "Warning: This entity matches a prior engagement record. It remembers you." |
| Champion defeated | "Named threat neutralized. {ChampionName} has been deleted from the system." |
| Champion escapes | "{ChampionName} has fled. Expect retaliation. Bounty marker created." |

---

## 14. Logging Requirements

**Reference:** [logging.md](../01-core/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Elite spawned | Information | "Elite {EnemyType} spawned in {Room}" | `EnemyType`, `Room`, `Stats` |
| Champion generated | Information | "Champion generated: {Name} with affixes {Affixes}" | `Name`, `BaseType`, `Affixes` |
| Affix applied | Debug | "Applied affix {Affix} to {Enemy}" | `Affix`, `Enemy`, `Effect` |
| Pack leader assigned | Debug | "Pack leader {Leader} assigned to pack of {Size}" | `Leader`, `Size`, `Members` |
| Bounty created | Information | "Bounty created for {ChampionName} in {Biome}" | `ChampionName`, `Biome`, `Reward` |

### 14.2 Example Implementation

```csharp
public Champion GenerateChampion(EnemyType baseType, int depth, Biome biome)
{
    using (LogContext.PushProperty("ChampionGeneration", Guid.NewGuid()))
    {
        _logger.Information("Generating champion from {BaseType} at depth {Depth}",
            baseType, depth);

        var affixCount = Math.Min(3, 1 + depth / 3);
        var affixes = SelectAffixes(affixCount, biome);

        _logger.Debug("Selected {Count} affixes: {Affixes}",
            affixes.Count, string.Join(", ", affixes));

        var champion = new Champion
        {
            BaseType = baseType,
            Affixes = affixes,
            GeneratedName = GenerateChampionName(baseType, affixes)
        };

        ApplyEliteScaling(champion);
        ApplyAffixes(champion, affixes);

        _logger.Information("Champion generated: {Name} with affixes {Affixes}",
            champion.GeneratedName, champion.Affixes);

        return champion;
    }
}
```

---

## 15. Testing Requirements

### 15.1 Unit Test Coverage

| Area | Priority | Test Cases |
|------|----------|------------|
| Elite scaling | High | Stat multipliers apply correctly |
| Affix selection | High | Conflict checking, weight distribution |
| Name generation | Medium | No empty names, proper formatting |
| Pack leader aura | Medium | Buff applies within range only |
| Bounty triggers | Medium | Correct conditions create bounties |

### 15.2 Key Test Cases

```csharp
[TestMethod]
public void EliteScaling_AppliesCorrectMultipliers()
{
    var standard = new Enemy { HP = 50, Damage = 10 };
    var elite = _eliteService.ApplyEliteScaling(standard.Clone());

    Assert.AreEqual(75, elite.HP);  // 50 × 1.5
    Assert.AreEqual(15, elite.Damage);  // 10 × 1.5
}

[TestMethod]
public void ChampionAffixes_ConflictsAreRejected()
{
    var affixes = new List<ChampionAffix> { ChampionAffix.Anchored };
    var candidate = ChampionAffix.RandomBlink;

    var hasConflict = _eliteService.CheckAffixConflict(affixes, candidate);

    Assert.IsTrue(hasConflict);
}

[TestMethod]
public void BountyCreation_TriggersOnChampionEscape()
{
    var champion = GenerateTestChampion(affixCount: 2);
    champion.CurrentHP = champion.MaxHP * 0.6f;  // Above 50%

    _eliteService.CheckBountyTrigger(champion, CombatOutcome.PartyFled);

    var bounties = _eliteService.GetActiveBounties(champion.Biome);
    Assert.IsTrue(bounties.Any(b => b.ChampionName == champion.GeneratedName));
}
```

### 15.3 QA Checklist

- [ ] Elite enemies display correct visual indicators
- [ ] Champion names are unique and readable
- [ ] Affix effects trigger correctly in combat
- [ ] Pack leader aura visually shows range
- [ ] Bounty targets appear in expected zones
- [ ] Loot bonuses reflect affix count

---

## 16. Related Specifications

| Spec | Relationship |
|------|--------------|
| [SPEC-COMBAT-016](encounter-generation.md) | Parent - integrates with spawn budget |
| [SPEC-COMBAT-017](spawn-scaling.md) | Sibling - base stat scaling |
| [SPEC-COMBAT-015](creature-traits.md) | Uses - trait definitions for affixes |
| [SPEC-COMBAT-018](boss-mechanics.md) | Sibling - boss-tier threats |
| [SPEC-COMBAT-020](impossible-encounters.md) | Sibling - threat assessment |

---

## 17. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
