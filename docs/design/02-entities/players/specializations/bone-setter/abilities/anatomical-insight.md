---
id: SPEC-ABILITY-2004
title: "Anatomical Insight"
parent: bone-setter
tier: 2
type: active
version: 1.0
---

# Anatomical Insight

**Ability ID:** 2004 | **Tier:** 2 | **Type:** Active | **PP Cost:** 4

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single organic enemy |
| **Resource Cost** | 20 Stamina |
| **Range** | 30 ft (visual range) |
| **Prerequisite** | 8 PP in Bone-Setter tree |
| **Starting Rank** | 2 |

---

## 2. Description

> The Bone-Setter's knowledge is not limited to healing. They can observe a living creature and instantly recognize its anatomical weaknesses.

> [!NOTE]
> This is the Bone-Setter's **offensive utility** ability — their only direct contribution to damage.

---

## 3. Mechanical Effects

### 3.1 Primary Effect

```
Target: Single organic (non-mechanical) enemy
Check: WITS vs target WILL
Success: Apply [Vulnerable] for 2 turns
Effect: Target takes bonus damage from Physical attacks
```

### 3.2 Targeting Restriction

| Entity Type | Valid Target? |
|-------------|---------------|
| Organic creatures | ✓ Yes |
| Undying (mechanical) | ✗ No |
| Constructs | ✗ No |
| Incorporeal | ✗ No |

---

## 4. Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- WITS check vs target WILL
- Apply [Vulnerable] for 2 turns
- Physical damage bonus: +25%
- Cost: 20 Stamina

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- All Rank 2 effects
- **NEW:** Duration 3 turns
- **NEW:** Party can see highlighted weak points (visual indicator)
- **NEW:** Physical damage bonus: +35%

---

## 5. Anatomical Insight Workflow

```mermaid
flowchart TD
    START[Use Anatomical Insight] --> CHECK_TYPE{Target organic?}
    
    CHECK_TYPE --> |No| FAIL[Cannot target - wrong type]
    CHECK_TYPE --> |Yes| CONTEST[WITS vs WILL]
    
    CONTEST --> RESULT{Success?}
    RESULT --> |No| RESIST[Target resists]
    RESULT --> |Yes| APPLY[Apply [Vulnerable]]
    
    APPLY --> PARTY[Party informed of weak points]
    PARTY --> DURATION[2-3 turns based on rank]
```

---

## 6. Tactical Applications

| Situation | Application |
|-----------|-------------|
| **Boss fight** | Mark boss for party burst |
| **Priority target** | Focus fire with bonus |
| **Supporting DPS** | Your contribution to damage |

---

## 7. Balance Data

### 7.1 Damage Amplification
- **Value:** +25% (Rank 2) / +35% (Rank 3).
- **Impact:** On a boss with 500 HP, +25% damage from a full 4-man party for 2 rounds represents ~100-150 extra damage.
- **Cost:** 20 Stamina is very cheap for a "Raid Buff". Balanced by the Check requirement (WITS vs WILL) which might fail on high-will bosses.

### 7.2 Targeting
- **Limitation:** "Organic Only" is a hard counter. Useless against Constructs/Undead (unless zombies count as organic? Spec says "Organic vs Construct/Undying". Usually Undead are Undying).

---

## 8. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Targeting**: Validate `Target.Type == Organic`.
- [ ] **Check**: `DiceSystem.ContestedRoll(WITS, WILL)`.
- [ ] **Effect**: Apply `Vulnerable` status with `Source=Insight`.

### Phase 2: Logic Integration
- [ ] **Rank 3**: Duration = 3. Damage Mod = 1.35x.
- [ ] **UI**: Add "Critical Spots" visual overlay on target model.

### Phase 3: Visuals
- [ ] **VFX**: Target glows red at weak points.
- [ ] **Anim**: Character points/calls out target.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Target**: Select Construct -> Returns InvalidTarget.
- [ ] **Check**: WITS > WILL -> Success -> Vulnerable applied.
- [ ] **Bonus**: Attack Vulnerable target -> Damage * 1.25.

### 9.2 Integration Tests
- [ ] **Stacking**: Does it stack with other Vulnerable sources? (Usually highest wins or add duration).
- [ ] **Party**: Verify ALL party members get the bonus damage.

### 9.3 Manual QA
- [ ] **Log**: "Identified weakness in the Beast's armor!"

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Insight | Info | "{Character} exposes a flaw in {Target}'s defense!" | `Character`, `Target` |
| Fail | Info | "{Target}'s anatomy is too alien/complex." | `Target` |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [Status Effects](../../../../04-systems/status-effects/vulnerable.md) | Vulnerable effect |
| [WITS Attribute](../../../../01-core/attributes/wits.md) | Attribute definition |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
