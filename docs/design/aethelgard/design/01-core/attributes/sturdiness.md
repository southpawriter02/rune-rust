---
id: SPEC-CORE-ATTR-STURDINESS
title: "STURDINESS Attribute — Complete Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/ResourceService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Services/ResolveCheckService.cs"
    status: Planned
---

# STURDINESS — The Resilient Hardware

> *"Physical toughness, endurance, and resilience. The capacity of your body to withstand punishment and keep functioning. STURDINESS governs your maximum health, your ability to resist poison and disease, your endurance in prolonged physical exertion, and your capacity to block incoming blows."*

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-CORE-ATTR-STURDINESS` |
| Category | Core Attribute |
| Parent Spec | `SPEC-CORE-ATTRIBUTES` |
| Primary Archetypes | Tanks (Skjaldmær, Vargr-Born tank builds) |

### 1.2 Core Philosophy

STURDINESS represents physical toughness, endurance, and resilience. It is the parameter for **resilient hardware**—the measure of a character's ability to withstand physical damage, disease, environmental hardship, and prolonged exertion without system failure.

A high-STURDINESS character does not avoid damage; they **absorb it**. They are the stable, well-maintained system that refuses to crash, the immovable object that endures where others would fall.

---

## 2. Primary Functions

### 2.1 Health Pool (Primary Driver)

STURDINESS is the **most significant** contributor to maximum HP.

**Formula:**
```
Max HP = 50 + (STURDINESS × 10) + Gear/Ability Bonuses − Corruption Penalty
```

**Corruption Penalty:**
```
Corruption Penalty = floor(Corruption / 10) × 5% of Base Max HP
```

**HP by STURDINESS:**

| STURDINESS | Base Max HP | With +20 Gear | At 30 Corruption |
|------------|-------------|---------------|------------------|
| 5 | 100 | 120 | 85 |
| 8 | 130 | 150 | 111 |
| 10 | 150 | 170 | 128 |
| 12 | 170 | 190 | 145 |
| 15 | 200 | 220 | 170 |

### 2.2 Stamina Pool

STURDINESS affects maximum Stamina for physical actions.

**Formula:**
```
Max Stamina = 50 + (STURDINESS × 5) + Gear/Ability Bonuses
```

**Stamina by STURDINESS:**

| STURDINESS | Base Max Stamina | With +15 Gear |
|------------|------------------|---------------|
| 5 | 75 | 90 |
| 8 | 90 | 105 |
| 10 | 100 | 115 |
| 12 | 110 | 125 |
| 15 | 125 | 140 |

**Stamina Regeneration (Combat):**
```
Regen per Turn = Max Stamina × 0.25
```

### 2.3 Physical Resolve Checks

STURDINESS provides the dice pool for resisting physical effects.

**Pool:**
```
Physical Resolve = STURDINESS (dice)
```

**Resists:**

| Effect Type | Opposed Roll |
|-------------|--------------|
| Poison | `STURDINESS` vs Poison DC |
| Disease | `STURDINESS` vs Disease DC |
| Knockdown | `STURDINESS` vs Attack Net |
| Exhaustion | `STURDINESS` vs Environment DC |
| Bleeding | `STURDINESS` vs Wound Severity |

**Outcome Table:**

| Net Successes | Result |
|---------------|--------|
| 0 | Full effect applies |
| 1 | Partial resist (half duration/intensity) |
| 2-3 | Strong resist (minimal effect) |
| 4+ | Complete resist (negate entirely) |

### 2.4 Block Pool

STURDINESS enables active damage mitigation via blocking.

**Formula:**
```
Block Pool = STURDINESS + Shield Rating + Relevant Skill
```

**Example Block Pools:**

| Character | STURDINESS | Shield | Skill | Total Pool |
|-----------|------------|--------|-------|------------|
| Light Shield User | 8 | +2 | +1 | 11d10 |
| Heavy Shield Tank | 12 | +4 | +3 | 19d10 |
| Tower Shield Master | 15 | +6 | +4 | 25d10 |

**Block Mechanic:**
- Block is declared as a **reaction**
- Roll Block Pool; each net success **reduces** incoming damage by 1

### 2.5 Carrying Capacity

**Formula:**
```
Carrying Capacity = 25 + (STURDINESS × 5) kg
```

**Encumbrance Thresholds:**

| Load % | Effect |
|--------|--------|
| 0-100% | No penalty |
| 100-150% | −2 Defense, movement halved |
| 150%+ | Cannot move without dropping items |

---

## 3. Derived Statistics

| Stat | Contribution | Formula |
|------|--------------|---------|
| Max HP | **Primary** | `50 + (STURDINESS × 10) + Gear − Corruption%` |
| Max Stamina | **Primary** | `50 + (STURDINESS × 5) + Gear` |
| Physical Resolve | **Primary** | `STURDINESS` dice pool |
| Block Pool | **Primary** | `STURDINESS + Shield + Skill` |
| Carry Capacity | **Primary** | `25 + (STURDINESS × 5)` kg |

---

## 4. Combat Integration

### 4.1 Defensive Actions

| Action | Pool | Effect |
|--------|------|--------|
| **Block (Reaction)** | `STURDINESS + Shield + Skill` | Reduce damage by net successes |
| **Brace** | `STURDINESS` | Halve next impact damage, cannot move |
| **Endure** | `STURDINESS` | Reduce status effect duration |

### 4.2 Status Effect Conditions

STURDINESS determines HP-based status thresholds:

| Condition | Trigger | Effect |
|-----------|---------|--------|
| **[Bloodied]** | HP < 50% Max | Some enemy abilities trigger |
| **[Critical]** | HP < 25% Max | Death save required on further damage |
| **[System Crashing]** | HP = 0 | Incapacitated, death timer starts |

### 4.3 Armor Synergy

Heavy armor synergizes with STURDINESS:

| Armor Type | Requirement | Bonus |
|------------|-------------|-------|
| Light Armor | None | +1-2 Defense |
| Medium Armor | STURDINESS 7 | +3-4 Defense, −1 Movement |
| Heavy Armor | STURDINESS 9 | +5-6 Defense, −2 Movement |
| Plate Armor | STURDINESS 12 | +7-8 Defense, −3 Movement |

**Below Requirement Penalty:** −2 Defense, movement halved

---

## 5. Skill Integration

### 5.1 STURDINESS-Governed Checks

| Check Type | Pool | Applications |
|------------|------|--------------|
| Endurance | `STURDINESS` | Prolonged exertion, forced march |
| Recovery | `STURDINESS` | Natural healing rate |
| Toxin Resistance | `STURDINESS` | Resist poison/venom |
| Environmental Survival | `STURDINESS` | Extreme heat/cold/pressure |

### 5.2 Extended Endurance Checks

For sustained physical trials:

| Trial | Threshold | Pool | Failure Result |
|-------|-----------|------|----------------|
| Forced march (24h) | 8 | `STURDINESS` | Gain [Exhausted] |
| Hold position under fire | 5 | `STURDINESS` | Forced to retreat |
| Resist interrogation | 10 | `STURDINESS` | Physical breakdown |

---

## 6. Resource Integration

### 6.1 HP Recovery

**Natural Healing (Rest):**

| Rest Type | HP Recovery |
|-----------|-------------|
| Short Rest (1 hour) | `STURDINESS` × 2 |
| Long Rest (8 hours) | `STURDINESS` × 5 |
| Sanctuary Rest (safe location) | Full HP restored |

### 6.2 Stamina Economy

**Stamina Costs (Examples):**

| Action | Cost |
|--------|------|
| Power Attack | 10 Stamina |
| Sprint (1 round) | 5 Stamina |
| Dodge (reaction) | 8 Stamina |
| Block (reaction) | 5 Stamina |

High STURDINESS = larger Stamina pool = more actions per encounter.

---

## 7. Specialization Synergies

### 7.1 Primary STURDINESS Specializations

| Specialization | STURDINESS Role | Key Synergy |
|----------------|-----------------|-------------|
| **Skjaldmær** | Shield tanking | Block abilities scale with STURDINESS |
| **Vargr-Born (Tank)** | Damage absorption | Transform form gains HP from STURDINESS |

### 7.2 Secondary STURDINESS Uses

| Specialization | STURDINESS Use |
|----------------|----------------|
| Berserker | HP pool for rage sustainability |
| Atgeir-wielder | Holding formation positions |
| Gorge-Maw Ascetic | Unarmed damage absorption |

---

## 8. Puzzle Integration

STURDINESS enables **endurance puzzle solutions**:

| Puzzle Type | STURDINESS Solution | Trade-off |
|-------------|---------------------|-----------|
| Acid pool | Wade through | Takes damage but reaches goal |
| Pressure trap | Tank the hit | Damage instead of avoidance |
| Collapsing structure | Hold it up | Ties up character physically |
| Environmental hazard | Endure exposure | Time-limited solution |

---

## 9. Balancing Considerations

### 9.1 Designed Limitations

| Limitation | Reasoning |
|------------|-----------|
| No offensive contribution | Must invest in MIGHT/FINESSE for damage |
| No accuracy | Can hit hard but must connect first |
| No mental defense | Vulnerable to psychic attacks |
| Cannot evade | Must absorb, not avoid |

### 9.2 Synergy Requirements

STURDINESS alone is insufficient because:
- High HP means nothing without damage output
- Blocking requires threat generation (taunt mechanics)
- Recovery means little without combat ending
- Must pair with MIGHT (damage) or WILL (threat control)

---

## 10. Narrative Descriptions by Value

| STURDINESS | Physical Description | Capabilities |
|------------|---------------------|--------------|
| 5 | Average constitution | Normal injury recovery |
| 6-7 | Hale, rarely sick | Shrugs off minor wounds |
| 8-9 | Hardy laborer | Works through injuries |
| 10-11 | Iron constitution | Barely slowed by wounds |
| 12-14 | Legendary endurance | Fights on despite critical injury |
| 15+ | Near-immortal toughness | Survives what should kill |

**Flavor Text Examples:**
- *"The blow that would fell an ox merely staggers you."*
- *"Blood flows freely, but your stance never wavers."*
- *"You've survived worse. Much worse."*

---

## 11. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Formulas**: Implement `CalculateMaxHP`, `CalculateMaxStamina`.
- [ ] **Pools**: Implement `PhysicalResolve` and `BlockPool` properties.
- [ ] **Penalty Logic**: Implement Corruption Penalty to MaxHP.

### Phase 2: Combat Integration
- [ ] **Block Reaction**: Implement `AttemptBlock` logic (Damage Reduction).
- [ ] **Status Triggers**: Hook logic to apply [Bloodied] and [Critical] flags automatically.
- [ ] **Armor**: Implement Strength Requirement checks for Heavy Armor.

### Phase 3: World Interaction
- [ ] **Encumbrance**: Implement `CalculateCarryCapacity` and Movement Penalty logic.
- [ ] **Regen**: Implement Natural Healing rates based on Sturdiness.

### Phase 4: UI & Feedback
- [ ] **HP Bar**: Color code HP bar based on Sturdiness thresholds (Green -> Yellow -> Red).
- [ ] **Inventory**: Show "Overencumbered" warning when near capacity.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Max HP**: `50 + (STURDINESS * 10) + Gear - Corruption%`.
- [ ] **Stamina**: `50 + (STURDINESS * 5) + Gear`.
- [ ] **Block Pool**: `STURDINESS + Shield + Skill`.
- [ ] **Encumbrance**: Verify capacity limit and penalties above 100%.

### 12.2 Integration Tests
- [ ] **Combat Damage**: Take damage -> Verify Block reduces it -> Verify [Bloodied] applies at 50%.
- [ ] **Rest**: Short Rest -> Verify HP recovers by `Sturdiness * 2`.

### 12.3 Manual QA
- [ ] **Inventory**: Add heavy items until overencumbered -> Verify movement speed slows.
- [ ] **Armor**: Equip Plate with low Sturdiness -> Verify Defense penalty.

---

## 13. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 13.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| HP Calc | Verbose | "Max HP recalculated for {Character}: {Value} (Penalty: {Penalty})" | `Character`, `Value`, `Penalty` |
| Block | Information | "{Character} blocked {Damage} damage (Rolled {Successes})" | `Character`, `Damage`, `Successes` |
| Encumbrance | Warning | "{Character} is Overencumbered ({Current}/{Max})" | `Character`, `Current`, `Max` |
| Status Change | Information | "{Character} status changed to {Status}" | `Character`, `Status` |

---

## 14. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-ATTRIBUTES` | Parent overview spec |
| `SPEC-CORE-RESOURCES` | HP, Stamina detailed mechanics |
| `SPEC-COMBAT-BLOCKING` | Block system details |
| `SPEC-COMBAT-STATUS` | Status effect definitions |
