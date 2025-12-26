---
id: SPEC-ABILITY-SKEWER
title: "Skewer"
ability-id: 1202
tier: 1
type: Active
---

# Skewer

> *"A precise, powerful thrust designed to exploit your weapon's length."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1202 |
| Display | `Skewer I/II/III` |
| Tier | 1 (Foundation) |
| Type | Active |
| PP Cost | 3 PP |
| Resource | 40 Stamina (35 at Rank II+) |
| Attribute | MIGHT |
| Damage Type | Physical |
| Keywords | [Reach] |

---

## Ranks

### Skewer I
*Base — included with ability unlock*

| Effect | Value |
|--------|-------|
| Damage | 2d8 Physical |
| Cost | 40 Stamina |
| Special | [Reach]: Attack front row from back row |

**Tooltip**: `Skewer I: 2d8 Physical. [Reach]. Cost: 40 Stamina`

---

### Skewer II
*Upgrade Cost: +2 PP*

| Effect | Value |
|--------|-------|
| Damage | 2d8 + 1d10 Physical |
| Cost | 35 Stamina |
| Special | [Reach] |

**Tooltip**: `Skewer II: 2d8+1d10 Physical. [Reach]. Cost: 35 Stamina`

---

### Skewer III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Damage | 2d8 + 2d10 Physical |
| Cost | 35 Stamina |
| Special | [Reach], **Crit: [Bleeding] 2 turns** |

**Tooltip**: `Skewer III: 2d8+2d10 Physical. [Reach]. Crit: [Bleeding] 2 turns. Cost: 35 Stamina`

---

## Formula

```csharp
// All Ranks
AttackRoll = Roll(MIGHT + 2);

// Rank I
Damage = Roll(2d8);
Cost = 40;

// Rank II
Damage = Roll(2d8) + Roll(1d10);
Cost = 35;

// Rank III
Damage = Roll(2d8) + Roll(2d10);
Cost = 35;
if (Critical) Target.AddStatus("Bleeding", Duration: 2);
```

---

## [Reach] Targeting

| Your Position | Valid Targets |
|---------------|---------------|
| Front Row | Front + Back Row |
| Back Row | Front Row only |

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Damage | Effect |
|------|------|--------|--------|
| I | 40 Stamina | 2d8 (9) | Reach (High value) |
| II | 35 Stamina | 2d8+1d10 (14.5) | Cost reduced (-5) |
| III | 35 Stamina | 2d8+2d10 (20) | Crit Bleed (High) |

### 7.2 Efficiency
- **DPE (Damage Per Energy):** Rank III = 20/35 = ~0.57 (Good for reach).
- **Utility:** Ability to snipe backline without moving is tactical gold.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Targeting**: Implement `[Reach]` logic (Ignore Row penalty).
- [ ] **Damage**: Implement scaling dice tiers.

### Phase 2: Logic Integration
- [ ] **Rank Checks**: Ensure correct dice/cost per Rank.
- [ ] **Effect**: Implement Critical Hit -> Bleed Trigger.

### Phase 3: Visuals
- [ ] **Animation**: Thrust animation.
- [ ] **VFX**: Blood spray on Crit.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Targeting**: Front -> Back (Valid). Back -> Front (Valid).
- [ ] **Damage**: 2d8 / 2d8+1d10 / 2d8+2d10 ranges correct.
- [ ] **Cost**: 40 -> 35 reduction verified.
- [ ] **Crit**: Critical Hit -> Applies Bleed.

### 9.2 Integration Tests
- [ ] **Combat**: Player uses Skewer to kill low HP status caster in back row.

### 9.3 Manual QA
- [ ] **Tooltip**: "Reach" keyword description correct.
- [ ] **Log**: "Skewered {Target} for {Damage}!"

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cast | Info | "{Character} skewers {Target}!" | `Character`, `Target` |
| Crit | Info | "Critical Hit! {Target} begins bleeding." | `Target` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/bleeding.md) | Bleeding spec |
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Reach rules |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
