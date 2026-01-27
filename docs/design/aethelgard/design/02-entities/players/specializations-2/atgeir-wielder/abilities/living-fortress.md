---
id: SPEC-ABILITY-LIVING-FORTRESS
title: "Living Fortress"
ability-id: 1209
tier: 4
type: Passive
---

# Living Fortress

> *"You have become the absolute master of your domain. A living fortress around which battles are won."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1209 |
| Display | `Living Fortress I/II/III` |
| Tier | 4 (Capstone) |
| Type | Passive |
| PP Cost | 6 PP |
| Prerequisite | 24 PP + both Tier 3 abilities |
| Resource | None |
| Requirement | Must be in Front Row |

> [!IMPORTANT]
> **Training this ability upgrades ALL Tier 1, 2, & 3 abilities to Rank III**

---

## Ranks

### Living Fortress I
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Immunity | [Push], [Pull] |
| Brace for Charge | Can use as Reaction (1×/combat) |

**Tooltip**: `Living Fortress I: Immune to Push/Pull. Brace as Reaction 1x/combat.`

---

### Living Fortress II
*Unlocked: Tree progression*

| Effect | Value |
|--------|-------|
| Aura | Allies +3 dice to resist [Push]/[Pull] |
| Skewer | +1 row range (hit back row from front) |

**Tooltip**: `Living Fortress II: Allies +3 dice vs Push/Pull. Skewer +1 range.`

---

### Living Fortress III
*Unlocked: Full tree completion*

| Effect | Value |
|--------|-------|
| **Zone of Control** | Enemies in front row opposite you: -1 hit, restricted movement |
| Brace for Charge | Reactive triggers 2×/combat |

**Tooltip**: `Living Fortress III: Zone of Control: Enemies -1 hit, restricted movement. Brace as Reaction 2x/combat.`

---

## Formula

```csharp
// Requirement
Requires: Self.Position == FrontRow;

// Rank I
ImmuneToForcedMovement = true;
BraceForCharge.CanUseAsReaction = true;
BraceForCharge.ReactiveUsesPerCombat = 1;

// Rank II
Aura(AdjacentAllies) {
    ResistForcedMovement += 3;
}
Skewer.Range += 1;

// Rank III
ZoneOfControl(OpposingFrontRow) {
    HitPenalty = -1;
    MovementRestricted = true;
}
BraceForCharge.ReactiveUsesPerCombat = 2;
```

---

## Zone of Control

When Living Fortress III is active:

| Affected | Effect |
|----------|--------|
| Enemies in front row opposite you | -1 to hit |
| Movement | Requires check to move freely |

---

## 10. Balance Data

### 10.1 Power Budget
| Rank | Budget | Effect Value |
|------|--------|--------------|
| I | Capstone Start | Immune (10), React x1 (10) = 20 |
| II | Progression | Aura +3 (9), Skewer +1 Range (5) = 14 (Cumulative 34) |
| III | Ultimate | ZoC (-1 Hit, Move) (15), React x2 (10) = 25 (Cumulative 59) |

### 10.2 Value Proposition
- **Ultimate Tanking:** Immunity to displacement + Zone of Control makes the character an immovable object that dictates enemy movement.
- **Damage Scaling:** Reaction attacks significantly increase DPS output on enemy turns.

---

## 11. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Reaction**: Implement `Reaction` system (Action taken on enemy turn).
- [ ] **Trigger**: Hook `BraceForCharge` to Reaction pool.

### Phase 2: Logic Integration
- [ ] **ZoC**: Implement `ZoneOfControl` aura logic (Opposing Front Row).
- [ ] **Immunity**: Set `ImmuneToForcedMovement` flag.
- [ ] **Skewer**: Modify `Skewer` range dynamically based on skill rank.

### Phase 3: Visuals
- [ ] **VFX**: "Iron Fortress" aura/shader.
- [ ] **UI**: Show available Reaction count.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Immunity**: Apply Push -> No effect.
- [ ] **Reaction**: Enters Brace -> Enemy attacks -> Counter triggers (Cost: 0 AP, 1 Reaction).
- [ ] **ZoC**: Enemy in opposing Front Row -> -1 Hit penalty applied.
- [ ] **Range**: Skewer targeting -> Can hit Back Row from Front Row (Range 2+1=3?). Spec says "+1 row range", implying Range 2->3 or just consistency.

### 12.2 Integration Tests
- [ ] **Combat**: Character stands in front, blocks movement, kills enemies with Reactions.
- [ ] **Stacking**: Multiple fortresses? ZoC penalties stack? (Usually No).

### 12.3 Manual QA
- [ ] **Log**: "Reacted with Brace!"

---

## 13. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 13.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Reaction | Info | "{Character} reacts with Brace for Charge!" | `Character` |
| ZoC | Debug | "{Enemy} is hindered by the Living Fortress." | `Enemy` |

---

## 14. Related Specifications
| Document | Purpose |
|----------|---------|
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Reaction system |
| [Brace for Charge](brace-for-charge.md) | The reactive ability |

---

## 15. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
