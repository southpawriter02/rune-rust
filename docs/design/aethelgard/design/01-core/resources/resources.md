---
id: SPEC-CORE-RESOURCES
title: "Resource Systems — Overview & Shared Mechanics"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/ResourceService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Models/CharacterResources.cs"
    status: Planned
---

# Resource Systems — Overview & Shared Mechanics

The foundational expendable resources that govern character survival and action economy.

> [!NOTE]
> Each resource has its own detailed specification. This document covers shared mechanics only.

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification (migrated from legacy v5.0) |

---

## 1. Overview

### 1.1 The Resource Hierarchy

| Resource Type | Role | Availability | Spec Link |
|---------------|------|--------------|-----------|
| **Health Pool (HP)** | System Integrity | Universal | [hp.md](./hp.md) |
| **Stamina** | Action Readiness | Universal | [stamina.md](./stamina.md) |
| **Aether Pool (AP)** | Mystic Power Cache | Mystics Only | [aether.md](./aether.md) |
| **Psychic Stress** | Trauma Accumulator | Universal | [stress.md](./stress.md) |

### 1.2 Specialization Resources

Some specializations have **unique secondary resources**:

| Resource | Specialization | Spec Link |
|----------|----------------|-----------|
| **Rage** | Berserker | [rage.md](./rage.md) |
| **Momentum** | Skirmisher archetypes | [momentum.md](./momentum.md) |
| **Coherence** | Skald, Thul | [coherence.md](./coherence.md) |

### 1.3 Design Philosophy

**The Three Pillars of System Capacity:**

- **HP (Health Pool)**: Measures **System Integrity** — physical form coherence against data-loss
- **Stamina**: Measures **Action Readiness** — fuel for coherent physical exertion
- **Aether Pool (AP)**: Measures **Aetheric Cache Capacity** — holding tainted Aether without corruption

**Design Pillars:**

- **Universal Baseline + Archetype Divergence**: All characters have HP and Stamina; only Mystics have AP
- **Attribute-Driven Scaling**: Each resource scales from specific attributes
- **Corruption as Universal Penalty**: Runic Blight penalizes HP and AP (not Stamina)
- **Regeneration Asymmetry**: Different resources regenerate at different rates
- **Tactical Economy**: Resources are the currency of the action economy

---

## 2. Attribute Integration

### 2.1 Primary Attribute Mapping

| Resource | Primary Attribute | Scaling Formula |
|----------|-------------------|-----------------|
| HP | STURDINESS | `50 + (STURDINESS × 10)` |
| Stamina | STURDINESS + FINESSE | `50 + (STURDINESS × 5) + (FINESSE × 2)` |
| Aether Pool | WILL | `50 + (WILL × 10)` (Mystics only) |
| Stress Resistance | WILL | `WILL` dice for resist checks |

### 2.2 Corruption Penalties

The Runic Blight Corruption system applies penalties to certain resources:

| Resource | Corruption Effect | Formula |
|----------|-------------------|---------|
| HP | Reduces max | `−5% per 10 Corruption` |
| AP | Reduces max | `−5% per 10 Corruption` |
| Stamina | **No effect** | — |

> [!IMPORTANT]
> Stamina is **not** penalized by Corruption to prevent unwinnable combat states.

---

## 3. Regeneration Rates

### 3.1 Combat Regeneration

| Resource | Combat Regen | Timing |
|----------|--------------|--------|
| HP | **None** | Requires healing abilities |
| Stamina | **25% of Max per turn** | Start of character's turn |
| AP | **None** | Requires focus abilities or sanctuary |
| Stress | **None** | Sanctuary rest only |

### 3.2 Rest & Recovery

| Rest Type | HP | Stamina | AP | Stress |
|-----------|----|---------|----|--------|
| **Short Rest** (1 hr) | `STURDINESS × 2` | Full | Partial | `WILL × 2` |
| **Long Rest** (8 hr) | `STURDINESS × 5` | Full | Full | `WILL × 5` |
| **Sanctuary** | Full | Full | Full | Full |
| **Milestone** | Full | Full | Full | Partial |

---

## 4. Resource Thresholds

### 4.1 HP Status Triggers

| HP Level | Status | Effects |
|----------|--------|---------|
| 100% | Healthy | Normal operation |
| < 50% | `[Bloodied]` | Some enemy abilities trigger |
| < 25% | `[Critical]` | Death save on damage |
| 0 | `[System Crashing]` | Incapacitated, death timer |

### 4.2 Stamina Thresholds

| Stamina Level | Effect |
|---------------|--------|
| > 50% | Normal operation |
| 25-50% | Some high-cost abilities unavailable |
| 0 | Cannot use abilities; basic actions only |

### 4.3 Stress Thresholds

| Stress Level | Defense Penalty | Additional Effects |
|--------------|-----------------|-------------------|
| 0-19 | 0 | — |
| 20-39 | −1 | — |
| 40-59 | −2 | — |
| 60-79 | −3 | — |
| 80-99 | −4 | Skill check disadvantage |
| 100 | −5 | **Trauma Check triggered** |

---

## 5. UI Display Requirements

### 5.1 Vitals Panel

```
┌─────────────────────────────────────────┐
│  HP:  ████████████░░░░░░░░  75/100      │
│  STA: ███████████████████░  95/100      │
│  AP:  █████████░░░░░░░░░░░  45/100      │
│  ψ:   ░░░░░░░░░░░░░░░░░░░░  12/100      │
└─────────────────────────────────────────┘
```

### 5.2 Color Coding

| Resource | Normal | Warning | Critical |
|----------|--------|---------|----------|
| HP | Green | Yellow (<50%) | Red (<25%) |
| Stamina | Cyan | Yellow (<25%) | Red (0) |
| AP | Blue/Purple | Yellow (<25%) | Red (0) |
| Stress | Gray | Orange (>60) | Red (>80) |

### 5.3 Feedback Events

| Event | Display |
|-------|---------|
| Resource deduction | `"[Ability] costs 15 Stamina. (45 → 30)"` |
| Regeneration | `"[Character] regenerates 25 Stamina. (30 → 55)"` |
| Corruption penalty | `"Max HP and AP reduced by 15%."` |
| Threshold crossed | `"[Character] is BLOODIED!"` |

---

## 6. Technical Implementation

### 6.1 Data Model

```csharp
public class CharacterResources
{
    // Universal Resources
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    
    // Mystic-Only Resource
    public int CurrentAp { get; set; }
    public int MaxAp { get; set; }
    
    // Trauma Resource
    public int PsychicStress { get; set; }
    
    // Computed Properties
    public bool IsAlive => CurrentHp > 0;
    public bool IsBloodied => CurrentHp < MaxHp * 0.5;
    public bool IsCritical => CurrentHp < MaxHp * 0.25;
    public int StressPenalty => PsychicStress / 20;
}
```

### 6.2 Resource Service Interface

```csharp
public interface IResourceService
{
    // Calculation
    int CalculateMaxHp(Character character);
    int CalculateMaxStamina(Character character);
    int CalculateMaxAp(Character character);
    
    // Modification
    ResourceResult DeductHp(Character character, int amount);
    ResourceResult DeductStamina(Character character, int cost);
    ResourceResult DeductAp(Character character, int cost);
    ResourceResult ApplyHealing(Character character, int amount);
    ResourceResult ApplyStress(Character character, int amount);
    
    // Regeneration
    int RegenerateStamina(Character character);  // Returns amount regenerated
    void RestoreOnRest(Character character, RestType restType);
}

public record ResourceResult(bool Success, int OldValue, int NewValue, string? Message);
```

### 6.3 Database Schema

```sql
CREATE TABLE character_resources (
    character_id UUID PRIMARY KEY REFERENCES characters(id) ON DELETE CASCADE,
    current_hp INT NOT NULL DEFAULT 50,
    max_hp INT NOT NULL DEFAULT 50,
    current_stamina INT NOT NULL DEFAULT 50,
    max_stamina INT NOT NULL DEFAULT 50,
    current_ap INT NOT NULL DEFAULT 0,
    max_ap INT NOT NULL DEFAULT 0,
    psychic_stress INT NOT NULL DEFAULT 0,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_hp CHECK (current_hp >= 0 AND current_hp <= max_hp),
    CONSTRAINT chk_stamina CHECK (current_stamina >= 0 AND current_stamina <= max_stamina),
    CONSTRAINT chk_ap CHECK (current_ap >= 0 AND current_ap <= max_ap),
    CONSTRAINT chk_stress CHECK (psychic_stress >= 0 AND psychic_stress <= 100)
);

CREATE INDEX idx_character_resources_id ON character_resources(character_id);
```

---

## 7. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Data Model**: Implement `CharacterResources` class with computed properties.
- [ ] **Formulas**: Implement `CalculateMaxHp`, `MaxStamina`, `MaxAp`.
- [ ] **Service**: Implement `IResourceService` stub.

### Phase 2: Persistence
- [ ] **Schema**: Implement `character_resources` table with constraints.
- [ ] **Repo**: Update `CharacterRepository` load/save logic.
- [ ] **Validation**: Ensure persistence prevents invalid states (e.g. Current > Max).

### Phase 3: Integration
- [ ] **Combat**: Hook Stamina deduction to turn actions.
- [ ] **Rest**: Implement `RestoreOnRest` logic based on rest type.
- [ ] **Attributes**: Ensure changes to `Sturdiness` update MaxHP immediately.

### Phase 4: UI & Feedback
- [ ] **HUD**: Implement Vitals Panel with color coding.
- [ ] **Events**: Trigger events on [Bloodied] or [Critical] state change.
- [ ] **Logs**: format resource changes clearly ("HP 45->30").

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Calculations**: MaxHP = `50 + (Sturdiness * 10)`.
- [ ] **Constraints**: Deducting HP below 0 sets it to 0.
- [ ] **Regen**: Stamina regen = 25% max.
- [ ] **Stress**: Stress limit = 100.
- [ ] **Computed**: `IsBloodied` is true when HP < 50%.

### 8.2 Integration Tests
- [ ] **Persistence**: Save Resources -> Load -> Verify values.
- [ ] **Rest**: Sleep Long Rest -> Verify Full Recovery (except Stress).
- [ ] **Attribute Change**: Increase Sturdiness -> Verify MaxHP increases.

### 8.3 Manual QA
- [ ] **HUD**: Take damage -> Verify bar turns yellow then red.
- [ ] **Cost**: Attempt ability with insufficient stamina -> Verify blocked.

---

## 9. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 9.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Resource Change | Verbose | "{Character} {Resource}: {Old} -> {New} ({Reason})" | `Character`, `Resource`, `Old`, `New`, `Reason` |
| Threshold | Warning | "{Character} is {Status}!" | `Character`, `Status` |
| Regen | Debug | "{Character} regenerated {Amount} {Resource}" | `Character`, `Amount`, `Resource` |

---

## 10. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-ATTRIBUTES` | Resources scale from attributes |
| `SPEC-CORE-DICE` | Stress checks use dice system |
| `SPEC-COMBAT-DAMAGE` | HP reduction mechanics |
| `SPEC-CORE-TRAUMA` | Stress/Trauma integration |
