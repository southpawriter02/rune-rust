# Einbui Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-EINBUI
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Einbui specialization, including design philosophy, all 9 abilities with rank details, and implementation status.

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/EinbuiSeeder.cs` | Implemented |
| Tests | N/A | Not Yet Implemented |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from EinbuiSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | Einbui |
| **Display Name** | Einbui |
| **Specialization ID** | 27002 |
| **Archetype** | Adept (ArchetypeID = 2) |
| **Path Type** | Coherent |
| **Mechanical Role** | Survival Specialist / Resource Generation / Exploration Support |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina |
| **Trauma Risk** | None |
| **Icon** | :camping: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher cost (powerful exploration utility) |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "Loner - Master of Radical Self-Reliance"

**Core Fantasy**: The ultimate survivalist embodying radical self-reliance through mastery of survival fundamentals. You are not a specialist but a grim generalist—you have learned a little about everything out of necessity.

Where others focus on single complex disciplines, you master tracking, brewing, trapping, navigating, foraging, and camp craft. You are never unprepared, never lacking the right tool. You are the reason parties survive deep wilderness.

**Mechanical Identity**:
1. **Zero Combat Power**: Entire value lies in exploration, survival, and logistics
2. **Field Crafting**: Create healing items and traps from gathered resources
3. **Resource Location**: Find hidden materials, water, and food
4. **Safe Rest Creation**: The ultimate expression is Blight Haven—guaranteed safe rest

### 1.4 Specialization Description (Full Text)

> The ultimate survivalist embodying radical self-reliance through mastery of survival fundamentals. You are not a specialist but a grim generalist—you have learned a little about everything out of necessity.
>
> Your entire value lies in exploration, survival, and logistics. You provide zero combat power but make extended expeditions possible through field crafting, resource location, and creating guaranteed-safe rest locations.

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Key Effect |
|----|--------------|------|------|------------|
| 27010 | Radical Self-Reliance I | 1 | Passive | +dice to Tracking/Foraging |
| 27011 | Improvised Trap | 1 | Active | Place trap that roots enemies |
| 27012 | Basic Concoction | 1 | Active | Field craft healing/stamina items |
| 27013 | Resourceful Eye | 2 | Active | Reveal hidden resources in room |
| 27014 | Radical Self-Reliance II | 2 | Passive | +dice to Stealth/Climbing |
| 27015 | Wasteland Wanderer | 2 | Passive | Resist environmental hazards |
| 27016 | Master Improviser | 3 | Passive | Auto-upgrade crafting to Rank 3 |
| 27017 | Live off the Land | 3 | Passive | Reduce party ration consumption |
| 27018 | The Ultimate Survivor | 4 | Passive+Active | Universal competence + Blight Haven |

---

## 3. Tier 1 Abilities

### 3.1 Radical Self-Reliance I (ID: 27010)

**Type**: Passive | **PP Cost**: 3

| Rank | Effect |
|------|--------|
| 1 | +1 die to Wasteland Survival (Tracking) and (Foraging) checks |
| 2 | +2 dice to both checks |
| 3 | +2 dice + auto-succeed on Easy (DC 10) survival checks |

---

### 3.2 Improvised Trap (ID: 27011)

**Type**: Active | **Action**: Standard Action | **Cost**: 15 Stamina + 1 [Scrap Metal] + 1 [Tough Leather]

| Rank | Effect |
|------|--------|
| 1 | Place trap within 2 tiles. First enemy: [Rooted] 1 turn |
| 2 | [Rooted] 2 turns |
| 3 | [Rooted] 2 turns + [Bleeding] 1d4/turn for 3 turns |

---

### 3.3 Basic Concoction (ID: 27012)

**Type**: Active | **Action**: Standard Action | **Cost**: 10 Stamina + 1 [Common Herb] + 1 [Clean Cloth]

| Rank | Effect |
|------|--------|
| 1 | Create: [Crude Poultice] (2d6 HP) OR [Weak Stimulant] (15 Stamina). Hold max 3 |
| 2 | Create: [Refined Poultice] (4d6 HP) OR [Potent Stimulant] (30 Stamina) |
| 3 | Create: [Superior Poultice] (6d6 HP + remove [Bleeding]) OR [Exceptional Stimulant] (45 Stamina + remove [Exhausted]) |

---

## 4. Tier 2 Abilities

### 4.1 Resourceful Eye (ID: 27013)

**Type**: Active | **Action**: Standard Action (out of combat) | **Cost**: 20 Stamina

| Rank | Effect |
|------|--------|
| 1 | WITS + Wasteland Survival check (DC 12). Success: Reveal all hidden Resource Nodes in room |
| 2 | DC reduced to 10 |
| 3 | DC 10 + also reveals hidden passages and traps |

---

### 4.2 Radical Self-Reliance II (ID: 27014)

**Type**: Passive | **PP Cost**: 4

| Rank | Effect |
|------|--------|
| 1 | +1 die to Acrobatics (Stealth) and (Climbing) checks |
| 2 | +2 dice to both checks |
| 3 | +2 dice + climb at full movement speed (no penalty) |

---

### 4.3 Wasteland Wanderer (ID: 27015)

**Type**: Passive | **PP Cost**: 4

| Rank | Effect |
|------|--------|
| 1 | Half damage from environmental hazards. +1 die to resist [Poisoned] and [Disease] |
| 2 | +2 dice to all environmental resistances |
| 3 | +2 dice + can rest safely in mildly hazardous environments |

---

## 5. Tier 3 Abilities

### 5.1 Master Improviser (ID: 27016)

**Type**: Passive | **PP Cost**: 5

| Rank | Effect |
|------|--------|
| 1 | Improvised Trap and Basic Concoction automatically use Rank 3 effects |
| 2 | Field crafting costs -5 Stamina (min 5) |
| 3 | -5 Stamina + can craft 2 items per Standard Action |

---

### 5.2 Live off the Land (ID: 27017)

**Type**: Passive | **PP Cost**: 5

| Rank | Effect |
|------|--------|
| 1 | No [Rations] needed for Wilderness Rest. Auto-find 1d3 [Common Herbs] when foraging. -25% party Ration consumption |
| 2 | Find 2d3 [Common Herbs]. -40% Rations |
| 3 | Find 3d3 [Common Herbs]. -50% Rations + no Ration consumption during Wilderness Rest if Einbui present |

---

## 6. Capstone Ability

### 6.1 The Ultimate Survivor (ID: 27018)

**Type**: Passive + Active | **PP Cost**: 6

**Passive - Universal Competence**:
| Rank | Effect |
|------|--------|
| 1 | +1 die to ALL non-combat skill checks you're not proficient in |
| 2 | +2 dice |
| 3 | +2 dice |

**Active - Blight Haven** (Once per expedition):
| Rank | Effect |
|------|--------|
| 1 | Designate cleared room as [Hidden Camp]: 0% Ambush Chance, all Wilderness Rest benefits, +10 to party recovery rolls, protected from environmental Psychic Stress |
| 2 | +20 to recovery rolls |
| 3 | +30 to recovery rolls + party can perform advanced crafting without station |

---

## 7. Status Effect Definitions

### 7.1 [Rooted]
- Cannot move
- Duration: 1-2 turns

### 7.2 [Bleeding]
- Takes 1d4 Physical damage per turn
- Duration: 3 turns

### 7.3 [Hidden Camp] (Blight Haven)
- 0% Ambush Chance
- All Wilderness Rest benefits apply
- Bonus to recovery rolls
- Protected from environmental Psychic Stress

---

## 8. Implementation Priority

### Phase 1: Resource System
1. Implement crafting material inventory ([Scrap Metal], [Tough Leather], [Common Herb], [Clean Cloth])
2. Implement field-craftable consumables (Poultices, Stimulants)
3. Implement trap placement system

### Phase 2: Exploration Integration
4. Implement hidden Resource Node system
5. Implement resource reveal mechanic
6. Implement environmental hazard resistance

### Phase 3: Rest & Expedition
7. Implement Ration consumption reduction
8. Implement Wilderness Rest integration
9. Implement Blight Haven safe room designation

### Phase 4: Polish
10. Add rank-specific skill check bonuses
11. Implement Universal Competence
12. Add [Hidden Camp] status tracking

---

**End of Specification**
