---
id: SPEC-EINBUI-27002
title: "Einbui"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Einbui

**Archetype:** Adept | **Path:** Coherent | **Role:** Survival Specialist / Resource Generation

> *"Loner - Master of Radical Self-Reliance"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Einbui |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Role** | Survival Specialist / Resource Generation / Exploration Support |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource** | Stamina |
| **Trauma Risk** | None |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 10 PP |
| **Minimum Legend** | 5 |
| **Prerequisites** | None |
| **Exclusive With** | None |

---

## Design Philosophy

### Tagline
*"The reason parties survive deep wilderness"*

### Core Fantasy
The ultimate survivalist embodying radical self-reliance through mastery of survival fundamentals. You are not a specialist but a grim generalist—you have learned a little about everything out of necessity.

Where others focus on single complex disciplines, you master tracking, brewing, trapping, navigating, foraging, and camp craft. You are never unprepared, never lacking the right tool. You are the reason parties survive deep wilderness.

### Mechanical Identity
1. **Zero Combat Power**: Entire value lies in exploration, survival, and logistics
2. **Field Crafting**: Create healing items and traps from gathered resources
3. **Resource Location**: Find hidden materials, water, and food
4. **Safe Rest Creation**: The ultimate expression is Blight Haven—guaranteed safe rest

### Gameplay Feel
Methodical, prepared, self-sufficient. Every expedition feels more manageable with you present. While others fight, you ensure the party can survive long enough to reach their destination.

---

## Rank Progression

### Tree-Based Advancement
Abilities unlock through **prerequisite chains**, not PP purchase:

| Tier | PP Cost | Starting Rank | Rank Upgrades |
|------|---------|---------------|---------------|
| Tier 1 | 3 PP each | Rank 1 | → Rank 2 → Rank 3 |
| Tier 2 | 4 PP each | Rank 2 | → Rank 3 |
| Tier 3 | 5 PP each | No ranks | Full power when unlocked |
| Capstone | 6 PP | No ranks | Upgrades all Tier 1 & 2 to Rank 3 |

### Rank Unlock Requirements

| Rank | Requirement |
|------|-------------|
| Rank 2 | Unlock any Tier 2 ability in this specialization |
| Rank 3 | Unlock the Capstone ability |

---

## Ability Tree

```
                    [EINBUI SURVIVAL MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ RADICAL SELF-   │ │ IMPROVISED TRAP │ │ BASIC           │
│ RELIANCE I      │ │                 │ │ CONCOCTION      │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ Tracking bonus  │ │ Place trap      │ │ Field crafting  │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ RADICAL SELF-   │ │ RESOURCEFUL EYE │ │ WASTELAND       │
│ RELIANCE II     │ │                 │ │ WANDERER        │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Passive         │ │ Active          │ │ Passive         │
│ Stealth bonus   │ │ Reveal hidden   │ │ Hazard resist   │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ MASTER         LIVE OFF    │
              │ IMPROVISER     THE LAND    │
              │ [Tier 3]       [Tier 3]    │
              │ Auto-upgrade   No rations  │
              └──────────────┬─────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │    THE ULTIMATE SURVIVOR    │
              │         [Capstone]          │
              │    Universal Competence     │
              │       + Blight Haven        │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 27010 | [Radical Self-Reliance I](abilities/radical-self-reliance-i.md) | 1 | Passive | 3 | +dice to Tracking/Foraging |
| 27011 | [Improvised Trap](abilities/improvised-trap.md) | 1 | Active | 3 | Place trap that roots enemies |
| 27012 | [Basic Concoction](abilities/basic-concoction.md) | 1 | Active | 3 | Field craft healing/stamina items |
| 27013 | [Resourceful Eye](abilities/resourceful-eye.md) | 2 | Active | 4 | Reveal hidden resources in room |
| 27014 | [Radical Self-Reliance II](abilities/radical-self-reliance-ii.md) | 2 | Passive | 4 | +dice to Stealth/Climbing |
| 27015 | [Wasteland Wanderer](abilities/wasteland-wanderer.md) | 2 | Passive | 4 | Resist environmental hazards |
| 27016 | [Master Improviser](abilities/master-improviser.md) | 3 | Passive | 5 | Auto-upgrade crafting to Rank 3 |
| 27017 | [Live off the Land](abilities/live-off-the-land.md) | 3 | Passive | 5 | Reduce party ration consumption |
| 27018 | [The Ultimate Survivor](abilities/the-ultimate-survivor.md) | 4 | Passive+Active | 6 | Universal competence + Blight Haven |

---

## Core Mechanics

### Zero Combat Paradigm

The Einbui provides **no combat abilities**. All value comes from:
- Exploration efficiency
- Resource generation
- Party sustainability
- Safe rest creation

### Field Crafting System

```
CraftableItems:
  - Poultices (HP restoration)
  - Stimulants (Stamina restoration)
  - Improvised Traps (Rooted effect)

MaterialRequirements:
  Trap: 1 [Scrap Metal] + 1 [Tough Leather]
  Concoction: 1 [Common Herb] + 1 [Clean Cloth]
```

### Crafting Materials

| Material | Source | Used For |
|----------|--------|----------|
| [Scrap Metal] | Salvage | Traps |
| [Tough Leather] | Creature drops | Traps |
| [Common Herb] | Foraging | Concoctions |
| [Clean Cloth] | Salvage/loot | Concoctions |

### Blight Haven (Capstone)

The ultimate survival ability:
- Designate cleared room as **[Hidden Camp]**
- 0% Ambush Chance
- All Wilderness Rest benefits
- +10/20/30 to party recovery rolls
- Protected from environmental Psychic Stress

---

## Situational Power Profile

### Optimal Conditions
- Deep wilderness expeditions
- Low-resource environments
- Extended exploration runs
- Parties lacking healers

### Weakness Conditions
- Combat-heavy scenarios
- Short dungeon runs
- Urban environments
- Parties with abundant supplies

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Ruin-Stalker** | Both excel in exploration; combined scouting |
| **Bone-Setter** | Supplements healing with field medicine |
| **Berserkr** | You keep them alive between rages |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Combat-focused builds** | You contribute nothing in fights |
| **Scrap-Tinker** | Overlapping resource competition |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Low | Basic survival bonuses |
| 6-10 | Medium | Field crafting online |
| 11-15 | High | Master Improviser multiplicative value |
| 16+ | Very High | Blight Haven enables deep expeditions |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 0/10 | Zero combat abilities |
| Survivability | 6/10 | Personal survival high |
| Support | 9/10 | Party sustainability |
| Exploration | 10/10 | Designed for this |
| Utility | 8/10 | Crafting, detection |

---

## Voice Guidance

### Tone Profile
- Practical, laconic, unsentimental
- Speaks from hard-won experience
- Values preparation over bravado

### Example Quotes (NPC Flavor Text)
- *"You brought three days of rations for a week's journey? I'll show you what's edible out here."*
- *"This room. We rest here. I've made it safe."*
- *"The wasteland doesn't care how strong you are. It cares how prepared you are."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement crafting material inventory
- [ ] Create basic crafting recipes
- [ ] Add skill check bonuses

### Phase 2: Core Abilities
- [ ] Implement Improvised Trap placement
- [ ] Implement Basic Concoction crafting
- [ ] Implement Resourceful Eye scanning

### Phase 3: Advanced Systems
- [ ] Implement environmental hazard resistance
- [ ] Implement ration consumption reduction
- [ ] Add hidden resource node system

### Phase 4: Capstone
- [ ] Implement Universal Competence passive
- [ ] Implement Blight Haven room designation
- [ ] Implement [Hidden Camp] status

### Phase 5: Polish
- [ ] Add rank progression bonuses
- [ ] Balance crafting costs
- [ ] Test expedition sustainability

---

## Testing Requirements

### Unit Tests
- Crafting material consumption
- Skill check bonus application
- Trap placement validation
- Ration reduction calculation

### Integration Tests
- Full expedition with Einbui support
- Blight Haven rest cycle
- Resource node revelation

### Manual QA
- Verify crafting feels impactful
- Test party sustainability improvement
- Confirm zero combat contribution

---

## Logging Requirements

### Event Templates

```
OnCraft:
  "[Einbui] crafts {ItemName} using {Materials}"

OnTrapPlaced:
  "[Einbui] places {TrapType} at tile ({X},{Y})"

OnResourceReveal:
  "Resourceful Eye reveals {Count} hidden nodes in {RoomName}"

OnBlightHaven:
  "BLIGHT HAVEN established in {RoomName}! Party has safe haven."
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Adept Archetype](../../archetypes/adept.md) | Parent archetype |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Wasteland Survival Skill](../../../01-core/skills/wasteland-survival.md) | Key skill |
| [Crafting Overview](../../../04-systems/crafting/crafting-overview.md) | Crafting system |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
