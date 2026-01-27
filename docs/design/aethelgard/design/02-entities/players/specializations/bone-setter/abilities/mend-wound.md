---
id: SPEC-ABILITY-2002
title: "Mend Wound"
parent: bone-setter
tier: 1
type: active
version: 1.0
---

# Mend Wound

**Ability ID:** 2002 | **Tier:** 1 | **Type:** Active | **PP Cost:** 3

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single ally |
| **Resource Cost** | 1× [Healing Poultice] |
| **Range** | Adjacent (touch) |
| **Prerequisite** | Bone-Setter specialization |

---

## 2. Description

> The Bone-Setter quickly and efficiently dresses a wound, applying a prepared poultice to begin the healing process.

---

## 3. Mechanical Effects

### 3.1 Healing Formula

```
Healing = Poultice Base + WITS score
```

| Poultice Quality | Base Healing | + WITS 6 |
|------------------|--------------|----------|
| Weak | 1d6 | 1d6 + 6 (7-12) |
| Standard | 2d6 | 2d6 + 6 (8-18) |
| Potent | 2d6 + 25% | ~2d6 + 8 |
| Masterwork | 3d6 | 3d6 + 6 (9-24) |

---

## 4. Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- Apply [Healing Poultice] to restore HP
- Formula: Poultice base + WITS
- Cost: 1× poultice consumed

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- All Rank 1 effects
- **NEW:** +2 flat bonus to all healing

**Formula:**
```
Healing = Poultice Base + WITS + 2
```

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- All Rank 2 effects
- **NEW:** Healing over time (+1d6 at start of next turn)

**Formula:**
```
Healing = Poultice Base + WITS + 2
NextTurnBonus = 1d6
```

---

## 5. Mend Wound Workflow

```mermaid
flowchart TD
    START[Use Mend Wound] --> TARGET[Select wounded ally]
    TARGET --> POULTICE{Have poultice?}
    
    POULTICE --> |No| FAIL[Cannot use ability]
    POULTICE --> |Yes| APPLY[Apply poultice]
    
    APPLY --> CONSUME[Poultice consumed]
    CONSUME --> HEAL[Calculate healing]
    HEAL --> TRIAGE{Target [Bloodied]?}
    
    TRIAGE --> |Yes| BONUS[+25% from Triage passive]
    TRIAGE --> |No| NORMAL[Normal healing]
    
    BONUS --> COMPLETE[HP restored]
    NORMAL --> COMPLETE
```

---

## 6. Synergies

| Combination | Effect |
|-------------|--------|
| + Triage passive | +25% when target below 50% HP |
| + "First, Do No Harm" | +2 Defense after healing |
| + Masterwork poultices | +50% healing |

---

## 7. Balance Data

### 7.1 Healing Per Resource
- **Weak:** ~9 HP average / Cheapest poultice.
- **Standard:** ~13 HP average / Common.
- **Masterwork:** ~17 HP average / Expensive but worth it.
- **Comparison:** An HP potion self-heals ~8 HP. Mend Wound is more efficient but requires Bone-Setter.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Action**: Create `MendWound` ability.
- [ ] **Cost**: Consume 1 Healing Poultice.
- [ ] **Effect**: `Target.Heal(Poultice.Base + Caster.WITS)`.

### Phase 2: Logic Integration
- [ ] **Rank 2**: Flat +2 added to formula.
- [ ] **Rank 3**: Add HoT buff (1d6 next turn).

### Phase 3: Visuals
- [ ] **Anim**: Bandaging animation.
- [ ] **VFX**: Green "+" particles rising from target.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Cost**: Poultice consumed.
- [ ] **Heal**: Target HP increases by correct formula.
- [ ] **Rank 3**: HoT applies 1d6 at start of next turn.

### 9.2 Integration Tests
- [ ] **Triage**: Verify Triage bonus stacks correctly.
- [ ] **First Do No Harm**: Verify Caster gets Defense buff.

### 9.3 Manual QA
- [ ] **Log**: "Kára's wounds begin to close."

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Heal | Info | "{Character} mends {Target}'s wounds (+{HP} HP)." | `Character`, `Target`, `HP` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Triage](triage.md) | Healing multiplier |
| [Field Medicine](../../../../04-systems/crafting/field-medicine.md) | Poultice crafting |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
