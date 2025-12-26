---
id: SPEC-ABILITY-2007
title: "Cognitive Realignment"
parent: bone-setter
tier: 3
type: active
version: 1.0
---

# Cognitive Realignment

**Ability ID:** 2007 | **Tier:** 3 | **Type:** Active | **PP Cost:** 5

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single ally |
| **Resource Cost** | 1× [Stabilizing Draught] |
| **Range** | Adjacent (touch) |
| **Prerequisite** | 20 PP in Bone-Setter tree |
| **Starting Rank** | 2 |

---

## 2. Description

> The Bone-Setter uses a combination of calming techniques, pressure points, and alchemical smelling salts to "reboot" a panicked or disoriented mind, forcing a moment of clarity.

> [!IMPORTANT]
> This is the **premier sanity-restoring ability** in the game. The Bone-Setter is essential for Trauma Economy management.

---

## 3. Mechanical Effects

### 3.1 Primary Effect

```
Effect 1: Removes [Feared] from ally
Effect 2: Removes [Disoriented] from ally
Effect 3: Removes large amount of Psychic Stress
Cost: 1× [Stabilizing Draught]
```

---

## 4. Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- Remove [Feared] from ally
- Remove [Disoriented] from ally
- Remove 3 Stress
- Cost: 1× [Stabilizing Draught]

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- All Rank 2 effects
- **NEW:** Also removes [Silenced]
- **NEW:** Remove 5 Stress
- **NEW:** +2d10 WILL saves for 2 rounds

---

## 5. Cognitive Realignment Workflow

```mermaid
flowchart TD
    START[Use Cognitive Realignment] --> CHECK{Have Stabilizing Draught?}
    
    CHECK --> |No| FAIL[Cannot use]
    CHECK --> |Yes| TARGET[Select affected ally]
    
    TARGET --> CONSUME[Draught consumed]
    CONSUME --> MENTAL[Remove mental effects]
    
    MENTAL --> FEARED{Has [Feared]?}
    FEARED --> |Yes| REMOVE_FEAR[Remove [Feared]]
    FEARED --> |No| DISORIENTED{Has [Disoriented]?}
    
    REMOVE_FEAR --> DISORIENTED
    DISORIENTED --> |Yes| REMOVE_DIS[Remove [Disoriented]]
    DISORIENTED --> |No| STRESS[Remove Stress]
    
    REMOVE_DIS --> STRESS
    STRESS --> DONE[Ally stabilized]
```

---

## 6. Trauma Economy Role

| Role | Effect |
|------|--------|
| **Reactive healing** | Removes Stress after accumulation |
| **Mental cleanse** | Removes fear/disorientation |
| **Complements Skald** | Skald prevents, Bone-Setter heals |

---

## 7. Balance Data

### 7.1 Trauma Economics
- **Input:** 1 Stabilizing Draught (~25 Hacksilver) + 5 PP.
- **Output:** Removes 3-5 Stress.
- **Value:** High. 5 Stress = 100 Stamina worth of "Trauma" if left unchecked (Assuming Trauma check failure leads to major debuff). Preventing Trauma is cheaper than curing it.

### 7.2 Comparisons
- **Vs Skald:** Skald reduces *incoming* stress. Bone-Setter removes *accumulated* stress. They work best together.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Action**: Create `CognitiveRealignment` ability.
- [ ] **Cost**: Consumption of `StabilizingDraught`.
- [ ] **Effect**: `Target.Stress -= 3`. `Target.RemoveStatus(Feared|Disoriented)`.

### Phase 2: Logic Integration
- [ ] **Rank 3**: Stress removal = 5. Add `Silenced` to removal list.
- [ ] **Buff**: Add `WillSaveBonus` (+2d10) for 2 rounds.

### Phase 3: Visuals
- [ ] **Anim**: "Smelling Salts" animation (Snap fingers/Wave vial).
- [ ] **VFX**: Blue/Clear shockwave around target head.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Cost**: Inventory -1 Draught.
- [ ] **Effect**: Target Stress 50 -> 47 (Rank 2).
- [ ] **Cleanse**: Target has Feared -> Removed.

### 9.2 Integration Tests
- [ ] **Max Stress**: If Target Stress is 99 (Near break), verify this reduces it to 94 (Safe zone).
- [ ] **Silenced**: Ensure Rank 3 cleanses Silenced (Magic blocker).

### 9.3 Manual QA
- [ ] **Log**: "Jarl snaps out of his panic."

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Sanity | Info | "{Target}'s mind clears. (-{Amount} Stress)" | `Target`, `Amount` |
| Cleanse | Info | "{Target} is no longer {Status}." | `Target`, `Status` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Stress](../../../../01-core/resources/stress.md) | Stress mechanic |
| [Alchemy](../../../../04-systems/crafting/alchemy.md) | Draught source |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
