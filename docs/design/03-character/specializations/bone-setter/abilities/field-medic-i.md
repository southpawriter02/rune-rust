---
id: SPEC-ABILITY-2001
title: "Field Medic I"
parent: bone-setter
tier: 1
type: passive
version: 1.0
---

# Field Medic I

**Ability ID:** 2001 | **Tier:** 1 | **Type:** Passive | **PP Cost:** 3

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisite** | Bone-Setter specialization |

---

## 2. Description

> The Bone-Setter is an expert at preparing medical supplies, ensuring their kit is always ready and effective.

---

## 3. Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- +1d10 to all Field Medicine crafting checks
- Start each expedition with 3× [Standard Healing Poultice]

**Formula:**
```
CraftingBonus = 1d10
StartingPoultices = 3
```

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- +2d10 to Field Medicine crafting
- Start with 4× [Standard Healing Poultice]
- **NEW:** Start with 1× [Soot-Stained Bandage]

**Formula:**
```
CraftingBonus = 2d10
StartingPoultices = 4
StartingBandages = 1
```

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +3d10 to Field Medicine crafting
- Start with 5× [Standard Healing Poultice]
- Start with 2× [Soot-Stained Bandage]
- **NEW:** Start with 1× [Common Antidote]

**Formula:**
```
CraftingBonus = 3d10
StartingPoultices = 5
StartingBandages = 2
StartingAntidotes = 1
```

---

## 4. Synergies

| Combination | Effect |
|-------------|--------|
| + Full medical kit | Stacking crafting bonuses |
| + Alchemist's Lab | Access to all recipes |
| + Wasteland Survival | Better ingredient foraging |

---

## 5. Example: Crafting with Field Medic

> **Rank 2 Bone-Setter crafts Healing Poultice:**
> - WITS: 6
> - Field Medic I (Rank 2): +2d10
> - Bone-Setter specialization: -2 DC
> - Base DC: 12 → 10
> - Pool: 8d10
>
> **Roll:** [8, 7, 9, 4, 7, 8, 3, 9] = 6 successes ✓
> **Result:** Masterwork poultice (+50% healing)

---

## 6. Balance Data

### 6.1 Economy
- **Crafting:** +1d10 to +3d10 allows crafting higher tier items earlier.
- **Supplies:** Starting with 5 Poultices (Rank 3) saves ~100 Hacksilver per run.

---

## 7. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **State**: `ExpeditionStart` listener.
- [ ] **Inventory**: Add items to `Player.Inventory` on start.

### Phase 2: Logic Integration
- [ ] **Dice**: Hook `GetCraftingBonus(Trade)` -> Return +Xd10.

### Phase 3: Visuals
- [ ] **UI**: Popup on Expedition start: "Field Medic Kit prepared: 5x Poultices."

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Start**: Start Expedition -> Inventory has 3 Poultices (Rank 1).
- [ ] **Bonus**: Roll Crafting -> Dice pool includes Bonus dice.

### 8.2 Integration Tests
- [ ] **Stacking**: Do these free items stack with bought items? (Yes).

### 8.3 Manual QA
- [ ] **Log**: "Field Medic supplies added."

---

## 9. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Supply | Info | "Medical supplies restocked. (+{Count} items)" | `Count` |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [Field Medicine](../../../../04-systems/crafting/field-medicine.md) | Crafting trade |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
