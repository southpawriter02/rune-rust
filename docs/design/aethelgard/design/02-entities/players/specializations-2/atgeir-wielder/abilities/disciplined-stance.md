---
id: SPEC-ABILITY-DISCIPLINED-STANCE
title: "Disciplined Stance"
ability-id: 1203
tier: 1
type: Active
---

# Disciplined Stance

> *"You plant your feet, becoming an anchor of stability."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1203 |
| Display | `Disciplined Stance I/II/III` |
| Tier | 1 (Foundation) |
| Type | Active (Stance) |
| PP Cost | 3 PP |
| Resource | 30 Stamina |
| Cooldown | 3 turns |
| Action | Bonus Action |

---

## Ranks

### Disciplined Stance I
*Base — included with ability unlock*

| Effect | Value |
|--------|-------|
| Duration | 2 turns |
| Soak | +4 |
| Resist Push/Pull | +3 dice |
| Movement | Cannot move |

**Tooltip**: `Disciplined Stance I: 2 turns: +4 Soak, +3 dice vs Push/Pull. Cannot move. Cost: 30 Stamina`

---

### Disciplined Stance II
*Upgrade Cost: +2 PP*

| Effect | Value |
|--------|-------|
| Duration | 3 turns |
| Soak | +6 |
| Resist Push/Pull | +4 dice |
| Movement | Cannot move |

**Tooltip**: `Disciplined Stance II: 3 turns: +6 Soak, +4 dice vs Push/Pull. Cannot move.`

---

### Disciplined Stance III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Duration | 3 turns |
| Soak | +8 |
| Resist Push/Pull | **IMMUNE** |
| Stamina Regen | +5 while in stance |

**Tooltip**: `Disciplined Stance III: +8 Soak. IMMUNE to Push/Pull. +5 Stamina regen while in stance.`

---

## Formula

```csharp
Self.AddStatus("DisciplinedStance", Duration: duration);

// Rank I
Soak += 4;
ResistForcedMovement += 3;

// Rank II
Soak += 6;
ResistForcedMovement += 4;

// Rank III
Soak += 8;
ImmuneToForcedMovement = true;
StaminaRegen += 5;
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Effect Value | Total Budget |
|------|------|--------------|--------------|
| I | 30 Stamina | +4 Soak (8), Resist (5) | 13 |
| II | 30 Stamina | +6 Soak (12), Resist (7) | 19 |
| III | 30 Stamina | +8 Soak (16), Immune (10), Regen (5) | 31 (Capstone) |

### 7.2 Trade-offs
- **Immobility:** High cost for positioning-dependent classes.
- **Stamina:** High upkeep if recast frequently.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Status**: Create `DisciplinedStanceStatus` class.
- [ ] **Modifiers**: Implement Soak bonus and Movement Lock.

### Phase 2: Logic Integration
- [ ] **Push/Pull**: Hook into `CombatResolution.EvaluateForcedMovement` to check for Immunity/Resistance dice.
- [ ] **Regen**: Hook into `TurnStart` for Stamina regeneration (Rank III).

### Phase 3: Visuals
- [ ] **VFX**: "Grid Anchor" graphical effect.
- [ ] **Tooltip**: Show "Immovable" state in UI.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Soak**: Taking damage -> Reduced by 4/6/8.
- [ ] **Movement**: TryMove() -> Returns False.
- [ ] **Push**: Apply Push -> Resisted/Immune.
- [ ] **Regen**: New turn -> Stamina increases (Rank III).

### 9.2 Integration Tests
- [ ] **Combat**: Player anchors, Enemy tries to Push, fails.
- [ ] **Traversal**: Cannot use "Dash" ability while in Stance.

### 9.3 Manual QA
- [ ] **Log**: "Resisted push due to Disciplined Stance".

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Activation | Info | "{Character} enters a disciplined stance." | `Character` |
| Resisted | Info | "{Character} holds their ground against the force." | `Character` |
| Expire | Debug | "{Character} loosens their stance." | `Character` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Forced Movement rules |
| [Damage formulas](../../../03-combat/damage-formulas.md) | Soak calculation |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
