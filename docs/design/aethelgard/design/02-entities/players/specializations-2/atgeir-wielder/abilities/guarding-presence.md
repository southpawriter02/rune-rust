---
id: SPEC-ABILITY-GUARDING-PRESENCE
title: "Guarding Presence"
ability-id: 1206
tier: 2
type: Passive
---

# Guarding Presence

> *"Your disciplined presence inspires fortitude in those around you."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1206 |
| Display | `Guarding Presence II/III` |
| Tier | 2 (Advanced) |
| Type | Passive (Aura) |
| PP Cost | 4 PP |
| Prerequisite | 8 PP in tree |
| Resource | None |
| Requirement | Must be in Front Row |
| Target | Adjacent Front-Row Allies |

---

## Ranks

### Guarding Presence II
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Aura Soak | +2 |
| Resolve vs [Fear] | +1 die |

**Tooltip**: `Guarding Presence II: Front row aura: +2 Soak, +1 die vs Fear`

---

### Guarding Presence III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Aura Soak | +3 |
| Resolve vs [Fear] | +1 die |
| Stamina Regen | +3 for allies in aura |

**Tooltip**: `Guarding Presence III: Front row aura: +3 Soak, +1 die vs Fear, +3 Stamina regen.`

---

## Formula

```csharp
// Requirement
Requires: Self.Position == FrontRow;

// Rank II
Aura(AdjacentFrontRowAllies) {
    Soak += 2;
    ResolveBonusVsFear += 1;
}

// Rank III
Aura(AdjacentFrontRowAllies) {
    Soak += 3;
    StaminaRegen += 3;
}
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Budget | Effect Value |
|------|--------|--------------|
| II | Advanced | +2 Soak Aura (AOE x3 = 6), +1 Fear Resist (5) = 11 |
| III | Mastery | +3 Soak Aura (AOE x3 = 9), +1 Fear, +3 Regen (AOE) = 20 |

### 7.2 Tactical Value
- **Front-Line Anchor:** Encourages formation play ("Phalanx" style).
- **Fear Resistance:** Critical against Horror enemies.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Aura**: Create `GuardingPresenceAura` status.
- [ ] **Check**: Hook `TurnStart` to scan adjacent allies.

### Phase 2: Logic Integration
- [ ] **Conditional**: `Target.Row == Front && Target.IsAdjacent(Source)`.
- [ ] **Stacking**: Ensure multiple Atgeir-Wielders don't double-dip (highest rank applies).

### Phase 3: Visuals
- [ ] **VFX**: Golden shield overlay on adjacent allies.
- [ ] **Tooltip**: "Protected by Guarding Presence (+3 Soak)".

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Range**: Ally is Adjacent -> Gets Buff. Ally moves away -> Loses Buff.
- [ ] **Row**: Ally in Back Row -> No Buff.
- [ ] **Soak**: Ally takes damage -> Reduced by 2/3.
- [ ] **Regen**: Rank III -> Ally gains +3 Stamina.

### 9.2 Integration Tests
- [ ] **Death**: Source dies -> Aura fades immediately.
- [ ] **Stun**: Source stunned -> Aura persists (usually YES for Passives, unless channeled).

### 9.3 Manual QA
- [ ] **Grid**: Verify adjacency check works on hexagonal/square grid (checking 2 neighbors).

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Apply | Debug | "{Target} gains Guarding Presence from {Source}." | `Target`, `Source` |
| Effect | Info | "{Target}'s armor is bolstered (+3)." | `Target` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/feared.md) | Fear resistance |
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Adjacency rules |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
