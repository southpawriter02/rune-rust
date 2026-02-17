# Loot Generation (ROG) System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-ECONOMY-001
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Economy Designer
- **Design**: Drop rates, quality distribution, loot tables
- **Balance**: Progression pacing, rarity value
- **Implementation**: LootService.cs, BossLootService.cs
- **QA/Testing**: Drop rate validation, quality distribution

---

## Executive Summary

### Purpose Statement

The Loot Generation System provides procedural random object generation (ROG) for equipment, consumables, crafting components, and currency drops based on enemy type, difficulty, and player progression.

### Scope

**In Scope**:

- Equipment generation with quality tiers
- Consumable drops
- Crafting component drops
- Currency (Cogs) drops
- Loot table management
- Boss-specific loot
- Rarity distribution

**Out of Scope**:

- Equipment stat definitions → `SPEC-SYSTEM-002`
- Shop inventory → `SPEC-ECONOMY-002`
- Crafting recipes → `SPEC-SYSTEM-004`
- Quest rewards → Quest specifications

### Success Criteria

- **Player Experience**: Loot feels rewarding and progression-appropriate
- **Technical**: Loot generation completes in <50ms
- **Design**: Quality distribution creates excitement for rare drops
- **Balance**: Progression pacing feels natural, not grindy

---

## Related Documentation

### Dependencies

**Depends On**:

- Equipment System: Equipment model → `SPEC-SYSTEM-002`
- Enemy System: Enemy difficulty ratings → `SPEC-COMBAT-003`
- Crafting System: Component types → `SPEC-SYSTEM-004`

**Depended Upon By**:

- Combat System: Victory rewards → `SPEC-COMBAT-001`
- Room System: Room loot spawns → `SPEC-SYSTEM-008`
- Economy System: Currency flow → `SPEC-ECONOMY-002`

### Code References

- **Primary Service**: `RuneAndRust.Engine/LootService.cs`
- **Boss Loot**: `RuneAndRust.Engine/BossLootService.cs`
- **Spawner**: `RuneAndRust.Engine/LootSpawner.cs`
- **Seeder**: `RuneAndRust.Persistence/BossLootSeeder.cs`

---

## Design Philosophy

### Design Pillars

1. **Risk = Reward**
    - **Rationale**: Harder enemies should drop better loot
    - **Examples**: Boss enemies guarantee high-tier drops
2. **Rarity Creates Excitement**
    - **Rationale**: Rare drops should feel special
    - **Examples**: Myth-Forged items are memorable finds
3. **Progression Pacing**
    - **Rationale**: Loot should match player milestone
    - **Examples**: Early game = common, Late game = rare possible

---

## Functional Requirements

### FR-001: Generate Enemy Loot

**Priority**: Critical
**Status**: Implemented

**Description**:
System must generate appropriate loot drops when enemies are defeated.

**Formula/Logic**:

```
DropChance = BaseDropChance × EnemyDifficulty × MilestoneModifier

QualityTier = WeightedRandom(QualityWeights[EnemyTier])

Example:
  Enemy Tier 2 (Elite) defeated at Milestone 3
  BaseDropChance = 0.6 (60%)
  EnemyDifficulty = 1.5
  MilestoneModifier = 1.1

  DropChance = 0.6 × 1.5 × 1.1 = 0.99 (99%)

  Quality Weights (Elite):
    Scavenged: 30%
    Clan-Forged: 45%
    Optimized: 20%
    Myth-Forged: 5%

```

---

### FR-002: Generate Boss Loot

**Priority**: High
**Status**: Implemented

**Description**:
System must generate guaranteed high-quality loot for boss defeats with unique drop tables.

**Acceptance Criteria**:

- [x]  Boss always drops equipment
- [x]  Boss drops from unique loot table
- [x]  Quality tier minimum = Clan-Forged
- [x]  Chance for Myth-Forged signature item

---

### FR-003: Generate Currency Drops

**Priority**: High
**Status**: Implemented

**Description**:
System must generate appropriate Cogs (currency) drops.

**Formula**:

```
CogsDrop = BaseCogs × EnemyTier × (1 + Random(0, 0.3))

Example:
  BaseCogs = 10
  EnemyTier = 2 (Elite)
  RandomVariance = 0.15

  CogsDrop = 10 × 2 × 1.15 = 23 Cogs

```

---

### FR-004: Generate Component Drops

**Priority**: High
**Status**: Implemented

**Description**:
System must drop crafting components based on enemy type and biome.

**Drop Logic**:

- Enemy type determines possible components
- Biome influences component pool
- Rarity affects quality

---

## System Mechanics

### Mechanic 1: Quality Distribution

**Quality Weights by Enemy Tier**:

| Enemy Tier | Jury-Rigged | Scavenged | Clan-Forged | Optimized | Myth-Forged |
| --- | --- | --- | --- | --- | --- |
| Fodder | 50% | 40% | 10% | 0% | 0% |
| Standard | 20% | 50% | 25% | 5% | 0% |
| Elite | 0% | 30% | 45% | 20% | 5% |
| Boss | 0% | 0% | 30% | 50% | 20% |

### Mechanic 2: Loot Tables

**Loot Table Structure**:

```
LootTable:
  - TableId: "elite_warrior"
  - Entries:
    - Item: "Iron Axe", Weight: 30
    - Item: "Steel Sword", Weight: 25
    - Item: "Health Potion", Weight: 20
    - Item: "ScrapMetal x2", Weight: 15
    - Item: "Nothing", Weight: 10

```

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Impact |
| --- | --- | --- | --- |
| BaseDropChance | LootService | 0.6 | Drop frequency |
| MythForgedChance | LootService | 0.02 | Rare drop excitement |
| BaseCogs | LootService | 10 | Currency flow |

---

## Appendix

### Appendix A: Enemy Tier Definitions

| Tier | Name | Examples | Drop Quality |
| --- | --- | --- | --- |
| 0 | Fodder | Rust Mites, Debris | Poor |
| 1 | Standard | Draugr, Scavengers | Average |
| 2 | Elite | Champions, Veterans | Good |
| 3 | Boss | Named Bosses | Excellent |

---

**End of Specification**