---
id: SPEC-ABILITY-FORMAL-TRAINING
title: "Formal Training"
ability-id: 1201
tier: 1
type: Passive
---

# Formal Training

> *"Your formal training instills deep physical and mental discipline."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1201 |
| Display | `Formal Training I/II/III` |
| Tier | 1 (Foundation) |
| Type | Passive |
| PP Cost | 3 PP |
| Resource | None |

---

## Ranks

### Formal Training I
*Base — included with ability unlock*

| Effect | Value |
|--------|-------|
| Stamina Regen | +5 per turn |
| Resolve Bonus | +1d10 vs [Stagger] |

**Tooltip**: `Formal Training I: +5 Stamina regen. +1d10 vs [Stagger]`

---

### Formal Training II
*Upgrade Cost: +2 PP*

| Effect | Value |
|--------|-------|
| Stamina Regen | +7 per turn |
| Resolve Bonus | +1d10 vs [Stagger], [Disoriented] |

**Tooltip**: `Formal Training II: +7 Stamina regen. +1d10 vs [Stagger] and [Disoriented]`

---

### Formal Training III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Stamina Regen | +10 per turn |
| Resolve Bonus | +2d10 vs [Stagger], [Disoriented] |
| **Permanent** | +1 WITS |

**Tooltip**: `Formal Training III: +10 Stamina regen. +2d10 vs disruption. +1 WITS.`

---

## Formula

```csharp
// Rank I
StaminaRegen += 5;
ResolveBonus += 1d10; // vs Stagger

// Rank II
StaminaRegen += 7;
ResolveBonus += 1d10; // vs Stagger, Disoriented

// Rank III
StaminaRegen += 10;
ResolveBonus += 2d10;
WITS += 1;
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Budget | Effect Value |
|------|--------|--------------|
| I | Standard | +5 Regen (5), +1d10 Resolve (5) = 10 |
| II | Advanced | +7 Regen (7), +1d10 x2 (8) = 15 |
| III | Mastery | +10 Regen (10), +2d10 (12), +1 WITS (10) = 32 |

### 7.2 Value Proposition
- **Sustained Combat:** Very high value due to continuous Stamina.
- **Resistance:** Mitigates the most dangerous CC (Stagger).

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Regen**: Hook `StaminaSystem` to add base flat regen.
- [ ] **Resolve**: Hook `DiceSystem` to add dice on Resolve checks.

### Phase 2: Logic Integration
- [ ] **Conditionals**: Ensure Resolve bonus triggers *only* for [Stagger] (Rank I) and [Stagger/Disoriented] (Rank II).
- [ ] **Attribute**: Add permanent +1 WITS modifier (Rank III).

### Phase 3: Visuals
- [ ] **Character Sheet**: Show green "+5" next to Stamina Regen.
- [ ] **Log**: Highlight "Formal Training Bonus" in roll details.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Regen**: EndTurn -> Stamina +5/+7/+10.
- [ ] **Roll**: Resolve check vs Stagger -> Adds 1d10/2d10.
- [ ] **Roll**: Resolve check vs Fear -> No bonus (unless specified).
- [ ] **Stat**: Rank III -> WITS increases by 1.

### 9.2 Integration Tests
- [ ] **Combat**: Long combat (10 rounds) -> Verify total stamina gained matches regen rate.
- [ ] **Save/Load**: WITS bonus persists correctly.

### 9.3 Manual QA
- [ ] **Tooltip**: Verify regen values displayed correctly.

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Regen | Debug | "{Character} regenerates {Amount} Stamina (Formal Training)." | `Character`, `Amount` |
| Bonus | Info | "Formal Training grants +{Dice}d10 to resolve." | `Dice` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/disoriented.md) | Disoriented spec |
| [Resources](../../../01-core/resources/stamina.md) | Stamina system |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
