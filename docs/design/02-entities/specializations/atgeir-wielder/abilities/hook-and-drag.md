---
id: SPEC-ABILITY-HOOK-AND-DRAG
title: "Hook and Drag"
ability-id: 1204
tier: 2
type: Active
---

# Hook and Drag

> *"Using your weapon's hooked blade, you violently yank a priority target out of position."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1204 |
| Display | `Hook and Drag II/III` |
| Tier | 2 (Advanced) |
| Type | Active |
| PP Cost | 4 PP |
| Prerequisite | 8 PP in tree |
| Resource | 45 Stamina |
| Cooldown | 4 turns |
| Target | Single Enemy (Back Row) |
| Keywords | [Pull] |

---

## Ranks

### Hook and Drag II
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Damage | 3d8 Physical |
| Pull Bonus | +2 dice |
| On Pull | Target [Slowed] 1 turn |

**Tooltip**: `Hook and Drag II: 3d8 Physical. [Pull] back→front (+2). Pulled: [Slowed] 1 turn. Cost: 45 Stamina`

---

### Hook and Drag III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Damage | 4d8 Physical |
| Pull Bonus | +3 dice |
| On Pull | Target [Slowed] + **[Stunned]** 1 turn |

**Tooltip**: `Hook and Drag III: 4d8 Physical. [Pull] (+3). [Slowed] + [Stunned] on Pull.`

---

## Formula

```csharp
// Rank II
Damage = Roll(3d8);
PullCheck = Roll(MIGHT + 2) vs Target.Roll(STURDINESS);
if (PullCheck.Success) {
    Target.MoveToFrontRow();
    Target.AddStatus("Slowed", Duration: 1);
}

// Rank III
Damage = Roll(4d8);
PullCheck = Roll(MIGHT + 3) vs Target.Roll(STURDINESS);
if (PullCheck.Success) {
    Target.AddStatus("Stunned", Duration: 1);
}
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Damage | Effect |
|------|------|--------|--------|
| II | 45 Stamina | 3d8 (13.5) | [Pull] Back w/ +2 dice, Slow (Low) |
| III | 45 Stamina | 4d8 (18) | [Pull] Back w/ +3 dice, Slow+Stun (High) |

### 7.2 Use Cases
- **Anti-Caster:** Yank squishy casters from safety into melee range.
- **Formation Disruption:** Pull shield bearers out of position.

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Maneuver**: Implement `Pull` combat maneuver logic.
- [ ] **Targeting**: Restrict to `BackRow` enemies only? Or any? (Spec says Back Row).

### Phase 2: Logic Integration
- [ ] **Check**: Implement Opposed Roll (MIGHT vs STURDINESS).
- [ ] **Movement**: Update `Position` on success.
- [ ] **Status**: Apply [Slowed] and/or [Stunned] based on Rank.

### Phase 3: Visuals
- [ ] **Animation**: Hook pull animation.
- [ ] **Feedback**: "Yanked!" text floater.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Damage**: Deals correct 3d8/4d8 damage.
- [ ] **Checks**: MIGHT + 2 vs STURDINESS.
- [ ] **Success**: Target Row changes Back -> Front.
- [ ] **Failure**: Target stays, but takes damage? (Usually maneuvers deal damage separate or contingent? Formula implies damage is unconditional, Pull is conditional? No, Formula looks like damage is unconditional, pull is checked. Need to verify implemented logic). *Correction: Formula shows Damage happens, then Pull check.*

### 9.2 Integration Tests
- [ ] **Obstruction**: Pull target into occupied slot? (Swap or displace?)
- [ ] **Immunity**: Pull vs Large/Boss (Immune to Forced Movement).

### 9.3 Manual QA
- [ ] **Log**: "Hooked {Target} from the shadows!" (Flavor).

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cast | Info | "{Character} hooks {Target}!" | `Character`, `Target` |
| Pull | Info | "{Target} is dragged into the fray!" | `Target` |
| Fail | Info | "{Target} resists the pull." | `Target` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/slowed.md) | Slowed spec |
| [Status Effects](../../../04-systems/status-effects/stunned.md) | Stunned spec |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
