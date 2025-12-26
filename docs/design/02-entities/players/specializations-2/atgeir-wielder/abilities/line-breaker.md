---
id: SPEC-ABILITY-LINE-BREAKER
title: "Line Breaker"
ability-id: 1205
tier: 2
type: Active
---

# Line Breaker

> *"A wide, sweeping strike that shatters enemy formations."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1205 |
| Display | `Line Breaker II/III` |
| Tier | 2 (Advanced) |
| Type | Active |
| PP Cost | 4 PP |
| Prerequisite | 8 PP in tree |
| Resource | 50 Stamina |
| Cooldown | 5 turns |
| Target | All Enemies (Front Row) |
| Keywords | [Push] |

---

## Ranks

### Line Breaker II
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Damage | 4d10 Physical (AoE) |
| Push Bonus | +1 dice |
| On Push | +1d10 bonus damage |

**Tooltip**: `Line Breaker II: 4d10 Physical to Front Row. [Push] to Back (+1). +1d10 on Push. Cost: 50 Stamina`

---

### Line Breaker III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Damage | 5d10 Physical (AoE) |
| Push Bonus | +2 dice |
| On Push | Target **[Off-Balance]** (-2 hit) 1 turn |

**Tooltip**: `Line Breaker III: 5d10 Physical. [Push] (+2). Pushed: [Off-Balance] -2 hit, 1 turn.`

---

## Formula

```csharp
// Rank II
foreach (Enemy in FrontRow) {
    Damage = Roll(4d10);
    PushCheck = Roll(MIGHT + 1) vs Target.Roll(STURDINESS);
    if (PushCheck.Success) {
        Target.MoveToBackRow();
        BonusDamage = Roll(1d10);
    }
}

// Rank III
foreach (Enemy in FrontRow) {
    Damage = Roll(5d10);
    PushCheck = Roll(MIGHT + 2) vs Target.Roll(STURDINESS);
    if (PushCheck.Success) {
        Target.AddStatus("Off-Balance", HitPenalty: -2, Duration: 1);
    }
}
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Damage | Effect |
|------|------|--------|--------|
| II | 50 Stamina | 4d10 (22) AoE | [Push] Front→Back w/ +1 dice, +1d10 dmg |
| III | 50 Stamina | 5d10 (27.5) AoE | [Push] Front→Back w/ +2 dice, Off-Balance |

### 7.2 Efficiency
- **Cost:** High (50 Stamina), requires huge commitment.
- **Payoff:** Can completely reset enemy frontline, exposing backline.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Targeting**: Iterate `AllEnemies` where `Row == Front`.
- [ ] **Damage**: Apply base physical damage.

### Phase 2: Logic Integration
- [ ] **Push**: Run Opposed Check for *each* target.
- [ ] **Effect**: If Push succeeds -> Move to Back -> Apply Bonus Damage/Status.

### Phase 3: Visuals
- [ ] **VFX**: Wide sweeping arc effect across entire front row.
- [ ] **Sound**: Heavy metal impact / scattering shields.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Targeting**: Hits 3 enemies in front row, ignores 2 in back.
- [ ] **Damage**: Dealt 4d10 to all 3.
- [ ] **Push**: Target A succeeds check (Stays), Target B fails (Moves).
- [ ] **Bonus**: Target B takes extra 1d10 or Off-Balance status.

### 9.2 Integration Tests
- [ ] **Crowding**: Back row full? Pushed unit collides? (Check Combat Resolution rules for "Full Row Push" - usually bounce/extra damage).

### 9.3 Manual QA
- [ ] **Log**: "Swept the line! 2 Pushed, 1 Resisted."

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cast | Info | "{Character} sweeps the enemy line!" | `Character` |
| Push | Info | "{Target} is thrown back!" | `Target` |
| Resist | Info | "{Target} holds the line." | `Target` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/disoriented.md) | Off-Balance (Disoriented variant) |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
