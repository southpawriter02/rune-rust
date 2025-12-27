---
id: SPEC-ABILITY-2005
title: "Administer Antidote"
parent: bone-setter
tier: 2
type: active
version: 1.0
---

# Administer Antidote

**Ability ID:** 2005 | **Tier:** 2 | **Type:** Active | **PP Cost:** 4

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single ally |
| **Resource Cost** | 1× [Common Antidote] |
| **Range** | Adjacent (touch) |
| **Prerequisite** | 8 PP in Bone-Setter tree |
| **Starting Rank** | 2 |

---

## 2. Description

> The Bone-Setter administers a carefully prepared antidote for the most common toxins.

---

## 3. Mechanical Effects

### 3.1 Primary Effect

```
Effect: Removes [Poisoned] or [Disease] from single ally
Cost: 1× [Common Antidote] consumed
```

---

## 4. Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- Remove [Poisoned] from ally (all stacks)
- Remove [Disease] from ally
- Consumes 1× [Common Antidote]

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- All Rank 2 effects
- **NEW:** Also removes [Corroded]
- **NEW:** +1d10 poison resistance for 3 rounds

---

## 5. Administer Antidote Workflow

```mermaid
flowchart TD
    START[Use Administer Antidote] --> CHECK{Have antidote?}
    
    CHECK --> |No| FAIL[Cannot use]
    CHECK --> |Yes| TARGET[Select poisoned ally]
    
    TARGET --> APPLY[Administer antidote]
    APPLY --> CONSUME[Antidote consumed]
    CONSUME --> REMOVE[Remove ALL [Poisoned] stacks]
    
    REMOVE --> RANK{Rank 3?}
    RANK --> |Yes| RESIST[+Poison resistance]
    RANK --> |No| DONE[Complete]
```

---

## 6. Comparison to Apply Tourniquet

| Aspect | Apply Tourniquet | Administer Antidote |
|--------|------------------|---------------------|
| Target | [Bleeding] | [Poisoned], [Disease] |
| Cost | Free | 1× Antidote |
| Cleanse Type | Physical | Alchemical |

---

## 7. Balance Data

### 7.1 Resource Economy
- **Input:** 1 Common Antidote (~15 Hacksilver).
- **Output:** Cleanses [Poisoned] (prevents ~3d4 damage) or [Disease] (prevents stat loss).
- **Efficiency:** Much cheaper than *field medic* supplies if crafting yourself.

### 7.2 Comparison
- **Vs Potion:** Drinking a potion is a Bonus Action (Self). This allows healing OTHERS, which potions usually cannot do without a specific "Feed Potion" action.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Action**: Create `AdministerAntidote` ability.
- [ ] **Cost**: Check inventory for `ItemId.CommonAntidote`. Consume 1.
- [ ] **Effect**: `Target.StatusEffects.RemoveAll(Poisoned | Disease)`.

### Phase 2: Logic Integration
- [ ] **Rank 3**: Add `Corroded` to removal list.
- [ ] **Buff**: Apply `StatusResistance` buff (+1d10 vs Poison) for 3 rounds.

### Phase 3: Visuals
- [ ] **Anim**: "Inject/Feed" animation.
- [ ] **VFX**: Green cleansing sparkles.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Cost**: Inventory -1 Antidote.
- [ ] **Cleanse**: Target has Poisoned -> Target has no Poisoned.
- [ ] **Rank 3**: Target has Corroded -> Cleanse success.

### 9.2 Integration Tests
- [ ] **Invalid Target**: Target has no status -> Ability fails? Or consumes item with no effect? (Spec implies check workflow).
- [ ] **Resistance**: Apply buf -> Attack with poison -> Check logic adds +1d10 defense.

### 9.3 Manual QA
- [ ] **Log**: "Administered antidote to Kára. Poison neutralized."

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cleanse | Info | "{Character} cures {Target} of toxins." | `Character`, `Target` |
| Resist | Debug | "Antidote resistance blocked poison application." | - |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../../04-systems/status-effects/poisoned.md) | Poison effect |
| [Alchemy](../../../../04-systems/crafting/alchemy.md) | Antidote source |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
