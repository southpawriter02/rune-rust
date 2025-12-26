---
id: SPEC-ABILITY-BRACE-FOR-CHARGE
title: "Brace for Charge"
ability-id: 1207
tier: 3
type: Active
---

# Brace for Charge

> *"You set your weapon with expert precision. They will run onto your spear and break."*

---

## Identity

| Property | Value |
|----------|-------|
| Ability ID | 1207 |
| Display | `Brace for Charge II/III` |
| Tier | 3 (Mastery) |
| Type | Active (Defensive Stance) |
| PP Cost | 5 PP |
| Prerequisite | 16 PP in tree |
| Resource | 40 Stamina |
| Cooldown | Once per combat |
| Trigger | When hit by melee attack |

---

## Ranks

### Brace for Charge II
*Starting Rank: When ability is learned*

| Effect | Value |
|--------|-------|
| Duration | 1 turn |
| Soak | +10 |
| Immunity | [Knocked Down] |
| Counter Damage | 5d8 Physical |
| Stun DC | 15 WILL save or [Stunned] 1 turn |

**Tooltip**: `Brace for Charge II: 1 turn: +10 Soak, immune Knockdown. Counter: 5d8, DC 15 or Stunned. Cost: 40 Stamina`

---

### Brace for Charge III
*Upgrade Cost: +3 PP, requires Rank 2*

| Effect | Value |
|--------|-------|
| Counter Damage | 6d8 Physical |
| Stun DC | 18 WILL save or [Stunned] |
| **Special** | Mechanical/Undying auto-Stunned (no save) |

**Tooltip**: `Brace for Charge III: Counter: 6d8, DC 18 or Stunned. Mech/Undying auto-Stunned.`

---

## Formula

```csharp
Self.AddStatus("Braced", Duration: 1);

OnMeleeHit {
    Soak += 10;
    ImmuneToKnockdown = true;
    
    // Rank II
    Attacker.TakeDamage(Roll(5d8), "Physical");
    if (Attacker.WillSave < 15)
        Attacker.AddStatus("Stunned", Duration: 1);
    
    // Rank III
    Attacker.TakeDamage(Roll(6d8), "Physical");
    if (Attacker.Type == "Mechanical" || Attacker.Type == "Undying")
        Attacker.AddStatus("Stunned", Duration: 1); // No save
    else if (Attacker.WillSave < 18)
        Attacker.AddStatus("Stunned", Duration: 1);
}
```

---

## 7. Balance Data

### 7.1 Power Budget
| Rank | Cost | Damage | Effect Value | Total Budget |
|------|------|--------|--------------|--------------|
| II | 40 Stamina | 5d8 (22.5) | +10 Soak, Stun (15) | 50 (Tier 3) |
| III | 40 Stamina | 6d8 (27) | Counter Stun (18/Auto) | 60 (Capstone) |

### 7.2 Effectiveness
- **Vs Swarm:** Low (Single target counter)
- **Vs Brute:** High (Stun stops big hits)
- **Vs Boss:** Very High (Auto-stun mechanicals)

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Stance**: Create `BracedStatus` (+10 Soak).
- [ ] **Trigger**: Hook into `OnBeingHit` event for Atgeir-Wielder.

### Phase 2: Logic Integration
- [ ] **Counter**: Implement attack interruption and retribution damage.
- [ ] **Stun**: Implement WILL save vs [Stunned] logic.
- [ ] **Type Check**: Add specific check for `Mechanical` or `Undying` archetype.

### Phase 3: Visuals
- [ ] **VFX**: "Shield Wall" effect on activation.
- [ ] **Animation**: Spear set animation.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Trigger**: Hit by melee -> Triggers counter.
- [ ] **Damage**: Dealt 5d8/6d8 accurately.
- [ ] **Status**: Mechanical enemy -> Stunned without save.
- [ ] **Cooldown**: Cannot be used twice in same combat.

### 9.2 Integration Tests
- [ ] **Combat**: Enemy attacks, takes damage, gets Stunned, Attack is effectively nullified? (Or does it happen *after* damage? "When hit" implies after).
- [ ] **Interaction**: Brace vs Unstoppable Force (Edge case).

### 9.3 Manual QA
- [ ] **Log**: Verify "Counter-attacked for X damage" appears.

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Activation | Info | "{Character} braces for impact!" | `Character` |
| Counter | Info | "{Character} counters {Attacker} for {Damage}." | `Character`, `Attacker`, `Damage` |
| Stun | Info | "{Attacker} is stunned by the impact!" | `Attacker` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../04-systems/status-effects/stunned.md) | Stunned condition |
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Counter-attack timing |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
