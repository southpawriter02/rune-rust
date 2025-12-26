---
id: SPEC-CORE-ATTR-WITS
title: "WITS Attribute — Complete Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/PerceptionService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Services/CraftingService.cs"
    status: Planned
---

# WITS — The Analytical Processor

> *"Intelligence, perception, and analytical reasoning. The sharpness of your eye and the speed of your thought. WITS governs your ability to observe and analyze the environment, your skill in crafting and technical tasks, and the speed at which you can react to changing situations."*

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-CORE-ATTR-WITS` |
| Category | Core Attribute |
| Parent Spec | `SPEC-CORE-ATTRIBUTES` |
| Primary Archetypes | Adepts (Jötun-Reader, Scrap-Tinker, Ruin-Stalker, Brewmaster) |

### 1.2 Core Philosophy

WITS represents intelligence, perception, and analytical reasoning. It is the parameter for the **analytical processor**—the ability to perceive the world's glitches, decode its paradoxical logic, understand its broken systems, and exploit that understanding for advantage.

A high-WITS character does not ignore the complexity of Aethelgard; they **comprehend it**. They see patterns in chaos, solutions in corrupted code, and opportunities where others see only noise.

---

## 2. Primary Functions

### 2.1 Passive Physical Perception

WITS determines the threshold for automatically noticing hidden physical elements.

**Formula:**
```
Passive Physical Perception = 10 + WITS
```

**Automatic Detection:**
- Hidden objects, traps, or secrets with DC ≤ Passive Perception are **automatically revealed**
- No roll required; this occurs on entering the area

**Examples:**

| WITS | Passive Perception | Auto-Detects |
|------|-------------------|--------------|
| 5 | 15 | Obvious traps (DC 15) |
| 8 | 18 | Hidden doors (DC 18) |
| 10 | 20 | Concealed switches (DC 20) |
| 12 | 22 | Well-hidden caches (DC 22) |
| 15 | 25 | Masterfully hidden secrets (DC 25) |

### 2.2 Active Perception Checks

For deliberate searching beyond passive awareness.

**Formula:**
```
Active Perception Pool = WITS + Perception Skill Rank + Modifiers
```

**Applications:**

| Task | Pool | DC Range |
|------|------|----------|
| Search room for loot | `WITS + Perception` | 1-3 |
| Find hidden passage | `WITS + Perception` | 2-4 |
| Detect ambush | `WITS + Perception` | 2-4 |
| Read enemy weakness | `WITS + Analysis` | 3-5 |
| Identify magical effect | `WITS + Arcana` | 3-5 |

### 2.3 Crafting (All Disciplines)

WITS is the universal crafting attribute.

**Formula:**
```
Crafting Pool = WITS + Craft Skill Rank + Tool Quality + Modifiers
```

**Quality Determination:**

| Net Successes | Result |
|---------------|--------|
| 0-1 | Standard quality |
| 2-4 | Masterwork quality (+1 to relevant stat) |
| 5+ | Legendary quality (+2 and special property) |

**Crafting Disciplines:**

| Discipline | Applications | Key Skill |
|------------|--------------|-----------|
| Weaponsmithing | Weapons, repairs | `WITS + Smithing` |
| Armorsmithing | Armor, shields | `WITS + Smithing` |
| Alchemy | Potions, salves, bombs | `WITS + Alchemy` |
| Runecraft | Inscriptions, enchantments | `WITS + Runecraft` |
| Engineering | Traps, mechanisms | `WITS + Engineering` |
| Cooking | Food buffs | `WITS + Cooking` |

### 2.4 System Bypass

WITS governs technical interaction with locks, traps, and machines.

**Formula:**
```
System Bypass Pool = WITS + System Bypass Skill + Tools
```

**Applications:**

| Task | DC | Notes |
|------|----|-------|
| Simple lock | 1 | Common chests |
| Standard lock | 2 | Secured doors |
| Complex lock | 3 | Vaults |
| Mechanical trap (disarm) | 3 | Pressure plates, tripwires |
| Runic lock | 4 | Requires Runecraft |
| Terminal hack | 4 | Corrupted systems |
| Master lock | 5 | Legendary security |

### 2.5 Initiative (Vigilance)

WITS contributes to reaction speed via tactical awareness.

**Formula:**
```
Vigilance = FINESSE + WITS
```

High WITS allows characters to **anticipate threats** even without peak reflexes.

---

## 3. Derived Statistics

| Stat | Contribution | Formula |
|------|--------------|---------|
| Passive Phys. Perception | **Primary** | `10 + WITS` |
| Vigilance | **Co-Primary** | `FINESSE + WITS` |
| Crafting Quality | **Primary** | `WITS + Skill = pool` |
| Analysis Speed | **Primary** | Varies by ability |

---

## 4. Combat Integration

### 4.1 Tactical Actions

High-WITS characters can use unique combat options:

| Action | Pool | Effect |
|--------|------|--------|
| **Analyze Enemy** | `WITS + Analysis` | Reveal weakness, grant party +1 damage |
| **Feint** | `WITS + Skill` vs `WITS` | Set up ally for advantage |
| **Called Shot** | Accuracy −2, `WITS + Skill` | Target specific body part |
| **Identify Ability** | `WITS + Arcana` | Learn enemy ability details |

### 4.2 Trap Interaction

In combat, traps can be weaponized:

| Action | Pool | Effect |
|--------|------|--------|
| **Trigger Trap on Enemy** | `WITS + Engineering` | Redirect trap effect |
| **Quick Disarm** | `WITS + System Bypass` (−2) | Disarm as bonus action |
| **Improvise Trap** | `WITS + Engineering` | Create hazard from environment |

### 4.3 Environmental Awareness

WITS allows reading the battlefield:

| Perception | Benefit |
|------------|---------|
| Notice cover opportunities | +2 Defense positioning |
| Spot hazardous terrain | Avoid environmental damage |
| Identify flanking routes | Enable tactical movement |

---

## 5. Skill Integration

### 5.1 WITS-Governed Skills

| Skill | Pool | Applications |
|-------|------|--------------|
| **Perception** | `WITS + Perception` | Notice details, spot traps |
| **System Bypass** | `WITS + System Bypass` | Locks, traps, hacking |
| **All Crafting** | `WITS + Craft Skill` | Create/repair items |
| **Analysis** | `WITS + Analysis` | Identify enemies, lore |
| **Navigation** | `WITS + Navigation` | Dungeon mapping, pathfinding |

### 5.2 Knowledge Checks

| Check Type | Pool | DC Examples |
|------------|------|-------------|
| Historical lore | `WITS + History` | DC 2: Common, DC 4: Obscure |
| Enemy identification | `WITS + Bestiary` | DC 1: Common, DC 3: Rare |
| Runic interpretation | `WITS + Runecraft` | DC 2: Standard, DC 5: Ancient |
| Engineering analysis | `WITS + Engineering` | DC 2: Simple, DC 4: Complex |

---

## 6. Exploration Integration

### 6.1 Room Exploration

High WITS significantly affects exploration efficiency:

| WITS | Exploration Benefit |
|------|---------------------|
| 5-7 | Standard search times |
| 8-10 | −1 turn to search rooms |
| 11-13 | −2 turns, auto-find common secrets |
| 14+ | −3 turns, auto-find rare secrets |

### 6.2 Puzzle Advantage

WITS provides **hints** for puzzles:

| WITS | Hint Level |
|------|------------|
| < 8 | No hints |
| 8-10 | General direction hint |
| 11-13 | Specific mechanism hint |
| 14+ | Near-complete solution hint |

> [!NOTE]
> Hints reduce XP reward but prevent being stuck.

---

## 7. Specialization Synergies

### 7.1 Primary WITS Specializations

| Specialization | WITS Role | Key Synergy |
|----------------|-----------|-------------|
| **Jötun-Reader** | Ancient lore access | Decode god-fragment scripts |
| **Scrap-Tinker** | Crafting bonus | Build combat automatons |
| **Ruin-Stalker** | Trap expertise | Detect/set traps with bonus |
| **Brewmaster** | Alchemy mastery | Superior potion effects |

### 7.2 Secondary WITS Uses

| Specialization | WITS Use |
|----------------|----------|
| Veiðimaðr | Tracking, trap awareness |
| Seiðkona | Ritual interpretation |
| All Mystics | Spell component identification |

---

## 8. Puzzle Integration

WITS enables **analytical puzzle solutions**:

| Puzzle Type | WITS Solution | Advantage |
|-------------|---------------|-----------|
| Logic puzzle | Deduce pattern | Optimal solution |
| Decoding | Translate symbols | Complete information |
| Mechanism | Understand function | No trial-and-error |
| Pattern matching | Identify sequence | Faster completion |
| Debugging | Fix corrupted data | Access hidden content |

WITS-based solutions are typically:
- Most efficient (least resources spent)
- Most complete (full information gained)
- Quietest (no noise from brute force)

---

## 9. Balancing Considerations

### 9.1 Designed Limitations

| Limitation | Reasoning |
|------------|-----------|
| No direct damage contribution | Information advantage, not combat power |
| No HP or Defense | Requires party to capitalize on intelligence |
| Support-focused | High WITS alone cannot win fights |
| Requires party synergy | Analysis bonuses need allies to use them |

### 9.2 Force Multiplier Role

WITS is a **force multiplier**, not a direct power:

```
High WITS Character Contribution:
- Reveal enemy weakness (+1 to +3 party damage)
- Prevent ambush (party acts first)
- Craft superior gear (permanent bonuses)
- Find secrets (extra resources)
```

---

## 10. Narrative Descriptions by Value

| WITS | Mental Description | Capabilities |
|------|-------------------|--------------|
| 5 | Average observation | Notices obvious |
| 6-7 | Attentive | Catches subtle clues |
| 8-9 | Sharp investigator | Decodes basic systems |
| 10-11 | Brilliant analyst | Sees hidden patterns |
| 12-14 | Genius intellect | Understands paradox logic |
| 15+ | Legendary sage | Comprehends god-fragments |

**Flavor Text Examples:**
- *"You notice the hairline crack in the mechanism—a fatal flaw."*
- *"The pattern clicks into place. You see the solution clearly."*
- *"Their fighting style reveals itself to you: a half-second tell before each strike."*

---

## 11. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Formulas**: Implement `CalculatePassivePerception` and `CalculateCraftingPool`.
- [ ] **Data**: Define DC tables for Perception and System Bypass.

### Phase 2: World Integration
- [ ] **Room Engine**: Hook Passive Perception to Room Entry logic (Auto-detect hidden).
- [ ] **Interaction**: Implement `AttemptHack` and `AttemptDisarm` services.

### Phase 3: Combat Integration
- [ ] **Analysis**: Implement `AnalyzeEnemy` action (Query Bestiary DB for weaknesses).
- [ ] **Traps**: Implement Logic for triggering traps on enemies.

### Phase 4: UI & Feedback
- [ ] **Notification**: "Secret Detected" toast when Passive Perception triggers.
- [ ] **Crafting UI**: Show Quality prediction based on Wits pool.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Passive Perception**: `10 + WITS`.
- [ ] **Vigilance**: `FINESSE + WITS`.
- [ ] **Crafting Pool**: `WITS + Skill + Tools`.
- [ ] **Auto-detection**: Verify objects with DC <= Perception are flagged as Visible.

### 12.2 Integration Tests
- [ ] **Entry Trigger**: Enter Room -> Secrets Revealed log generated.
- [ ] **Analysis**: Analyze Enemy -> Weakness applies to next attack.

### 12.3 Manual QA
- [ ] **Crafting**: Attempt craft with infinite material -> Verify Masterwork rate matches math.
- [ ] **Secrets**: Walk past hidden door (DC 15) with WITS 5 (Total 15) -> auto-opens.

---

## 13. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 13.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Passive Percept | Info | "{Character} auto-detected {Object} (DC {DC})" | `Character`, `Object`, `DC` |
| Analyze | Info | "{Character} analyzed {Enemy}: Revealed {Weakness}" | `Character`, `Enemy`, `Weakness` |
| Craft Result | Info | "{Character} crafted {Item} (Quality: {Quality})" | `Character`, `Item`, `Quality` |

---

## 14. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-ATTRIBUTES` | Parent overview spec |
| `SPEC-CORE-DICE` | Uses dice pool system |
| `SPEC-CRAFTING-ENGINE` | Crafting mechanics detail |
| `SPEC-EXPLORATION-PERCEPTION` | Perception subsystem |
