---
id: SPEC-ABILITY-UNSTOPPABLE-PHALANX
title: "Unstoppable Phalanx"
ability-id: 1208
tier: 3
type: Active
---

# Unstoppable Phalanx

> *"Your polearm punches through armor and flesh, impaling one target and striking another."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1208 |
| Display | `Unstoppable Phalanx II/III` |
| Tier | 3 (Mastery) |
| Type | Active |
| PP Cost | 5 PP |
| Prerequisite | 16 PP in tree |
| Resource | 60 Stamina |
| Cooldown | 4 turns |
| Target | Single Enemy + Enemy Behind |
| Special | Line-piercing attack |

---

## Ranks

### Unstoppable Phalanx II
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Primary Damage | 7d10 Physical |
| Secondary Damage | 5d10 Physical |
| Status | Both targets [Off-Balance] 1 turn |

**Tooltip**: `Unstoppable Phalanx II: 7d10 primary, 5d10 to enemy behind. Both [Off-Balance]. Cost: 60 Stamina`

---

### Unstoppable Phalanx III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Primary Damage | 8d10 Physical |
| Secondary Damage | 6d10 Physical |
| **Overkill** | If primary dies: secondary takes 2× damage |

**Tooltip**: `Unstoppable Phalanx III: 8d10 primary, 6d10 secondary. If primary dies: 2x secondary damage.`

---

## Formula

```csharp
AttackRoll = Roll(MIGHT + 3);

// Rank II
if (AttackHits) {
    PrimaryDamage = Roll(7d10);
    SecondaryTarget = GetEnemyBehind(PrimaryTarget);
    SecondaryDamage = Roll(5d10);
    PrimaryTarget.AddStatus("Off-Balance", Duration: 1);
    SecondaryTarget.AddStatus("Off-Balance", Duration: 1);
}

// Rank III
if (AttackHits) {
    PrimaryDamage = Roll(8d10);
    SecondaryDamage = Roll(6d10);
    if (PrimaryTarget.HP <= 0)
        SecondaryDamage *= 2;
}
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Damage | Effect |
|------|------|--------|--------|
| II | 60 Stamina | 7d10 (38.5) + 5d10 (27.5) = 66 | Off-Balance (Dual) |
| III | 60 Stamina | 8d10 (44) + 6d10 (33) = 77 | Overkill Mechanic |

### 7.2 Efficiency
- **Burst:** Highest single-action damage potential in the tree.
- **Cost:** Very high stamina (60), basically an "Ultimate".

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Targeting**: Select Primary. Calculate `EnemyBehind`.
- [ ] **Damage**: Apply separate damage events to both targets.

### Phase 2: Logic Integration
- [ ] **Conditionals**: Check `PrimaryTarget.IsDead` *after* primary damage but *before* secondary damage (for Rank III check).
- [ ] **Status**: Apply [Off-Balance] to both.

### Phase 3: Visuals
- [ ] **VFX**: Piercing beam or spear trail through both targets.
- [ ] **Sound**: Crunch-Crunch double impact.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Targeting**: Select Front Row -> Hits Back Row directly behind.
- [ ] **Targeting**: No enemy behind -> Only hits primary.
- [ ] **Damage**: 7d10/8d10 primary, 5d10/6d10 secondary.
- [ ] **Overkill**: Kill primary -> Verify secondary takes 2x damage (Rank III).

### 9.2 Integration Tests
- [ ] **Status**: Both targets get Off-Balance (Rank II).
- [ ] **Logs**: Two separate damage log entries generated.

### 9.3 Manual QA
- [ ] **Visual**: Verify correct enemies highlight during targeting.

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cast | Info | "{Character} unleashes the Phalanx!" | `Character` |
| Primary | Info | "{Target} takes {Damage} (Impaled)." | `Target`, `Damage` |
| Secondary | Info | "The spear continues into {Target} for {Damage}!" | `Target`, `Damage` |
| Overkill | Info | "Target destroyed! Impact doubles on {Secondary}!" | `Secondary` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/disoriented.md) | Off-Balance spec |
| [Damage formulas](../../../03-combat/damage-formulas.md) | Damage calculation |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
