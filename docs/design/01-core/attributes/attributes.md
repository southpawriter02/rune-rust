---
id: SPEC-CORE-ATTRIBUTES
title: "Attributes System — Overview & Shared Mechanics"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/StatCalculationService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Models/CharacterAttributes.cs"
    status: Planned
---

# Attributes System — Overview & Shared Mechanics

The foundational character parameters that define capabilities across all game systems.

> [!NOTE]
> Each attribute has its own detailed specification. This document covers shared mechanics only.

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification (split from combined attributes.md) |

---

## 1. Overview

### 1.1 The Five Attributes

| Attribute | Role | Spec Link |
|-----------|------|-----------|
| **MIGHT** | Brute-Force Processor | [might.md](./might.md) |
| **FINESSE** | Agile Co-Processor | [finesse.md](./finesse.md) |
| **STURDINESS** | Resilient Hardware | [sturdiness.md](./sturdiness.md) |
| **WITS** | Analytical Processor | [wits.md](./wits.md) |
| **WILL** | Psychic Firewall | [will.md](./will.md) |

### 1.2 Design Philosophy

The five Core Attributes are the **fundamental parameters of a character's operating system**—raw, innate potential that determines how they interface with the broken world of Aethelgard.

**Design Pillars:**

- **Meaningful Specialization**: Every attribute compelling for at least one Archetype
- **Balanced Distribution**: No single attribute dominates all play aspects
- **Clear Identity**: Each attribute solves problems in fundamentally different ways
- **Universal Relevance**: Every character needs at least two attributes to survive
- **Progression Foundation**: Attributes are the primary long-term investment

---

## 2. Value Ranges

### 2.1 Numerical Scale

| Range | Description | Typical Character |
|-------|-------------|-------------------|
| 5 | Base/Minimum | Untrained human |
| 6-7 | Below Average | Minor investment |
| 8-9 | Average | Competent adventurer |
| 10-12 | Above Average | Specialist focus |
| 13-15 | Exceptional | Elite professionals |
| 16-20 | Legendary | Late-game heroes |
| 20+ | Mythic | Campaign-ending power |

### 2.2 Attribute Role by Archetype

| Archetype | Primary Attribute | Secondary |
|-----------|-------------------|-----------|
| **Warrior** | MIGHT or STURDINESS | WILL |
| **Skirmisher** | FINESSE | WITS |
| **Adept** | WITS | STURDINESS |
| **Mystic** | WILL (always) | WITS |

---

## 3. Character Creation

### 3.1 Point-Buy System

**Starting Allocation:**
- All five attributes begin at **5** (base minimum)
- Players receive **15 points** to distribute
- Scaling cost incentivizes specialization

**Cost Table:**

| Target Value | Cost per Point |
|--------------|----------------|
| 5 → 6 | 1 point |
| 6 → 7 | 1 point |
| 7 → 8 | 1 point |
| 8 → 9 | 2 points |
| 9 → 10 | 2 points |

**Maximum at Creation:** 10 (requires 7 points in one attribute)

### 3.2 Example Distributions

#### Balanced Warrior (5 points remaining)
```
MIGHT: 8 (+3pts)  FINESSE: 7 (+2pts)  STURDINESS: 8 (+3pts)
WITS: 5 (+0pts)   WILL: 7 (+2pts)
```
*Reliable frontline fighter with decent mental defense.*

#### Specialized Mystic (0 points remaining)
```
MIGHT: 5 (+0pts)  FINESSE: 6 (+1pt)  STURDINESS: 6 (+1pt)
WITS: 7 (+2pts)   WILL: 10 (+7pts: 3 + 4)
```
*Maximum spiritual power, physically fragile.*

#### High-FINESSE Skirmisher (3 points remaining)
```
MIGHT: 5 (+0pts)  FINESSE: 10 (+7pts)  STURDINESS: 6 (+1pt)
WITS: 6 (+1pt)    WILL: 8 (+3pts)
```
*Hard to hit, relies on evasion over HP.*

### 3.3 UI Requirements

**Character Creation Screen Must Display:**
- [ ] Remaining allocation points (prominent)
- [ ] Cost for next level (per attribute)
- [ ] Live preview of derived stats (HP, Defense, etc.)
- [ ] Warning if WILL < 6 ("Trauma Vulnerability")
- [ ] Archetype recommendations

---

## 4. Progression (Saga System)

### 4.1 PP Cost Formula

```
PP Cost = New Attribute Level × 2
```

| Current → New | PP Cost |
|---------------|---------|
| 8 → 9 | 18 PP |
| 9 → 10 | 20 PP |
| 10 → 11 | 22 PP |
| 15 → 16 | 32 PP |

### 4.2 Progression Philosophy

| Game Phase | PP Priority |
|------------|-------------|
| **Early** | Specialization abilities (unlock core kit) |
| **Mid** | Balance: abilities + key attributes |
| **Late** | Primarily attributes (incremental power) |

### 4.3 No Hard Caps

- Attributes can increase **indefinitely**
- **Organic cap**: Finite PP earnable in campaign
- At ~50 milestones: Primary attribute might reach 20+

### 4.4 Respec Policy

> [!WARNING]
> **Current Design: No Respec**

Attribute allocation is **permanent** to encourage meaningful choices.

---

## 5. Derived Statistics Summary

| Stat | Formula | Primary Attribute |
|------|---------|-------------------|
| Max HP | `50 + (STURDINESS × 10) + Gear − Corruption%` | STURDINESS |
| Max Stamina | `50 + (STURDINESS × 5) + Gear` | STURDINESS |
| Max AP | `Base + (WILL × 10) + Gear − Corruption%` | WILL |
| Defense | `10 + FINESSE − StressPenalty + Armor` | FINESSE |
| Vigilance | `FINESSE + WITS` | FINESSE, WITS |
| Physical Resolve | `STURDINESS` dice | STURDINESS |
| Mental Resolve | `WILL` dice | WILL |
| Carry Capacity | `25 + (STURDINESS × 5)` kg | STURDINESS |
| Passive Phys. Perception | `10 + WITS` | WITS |
| Passive Psych. Perception | `10 + WILL` | WILL |

> [!TIP]
> See individual attribute specs for detailed breakdown and examples.

---

## 6. Balancing Overview

### 6.1 Single-Attribute Dominance Prevention

Each attribute has designed **limitations**:

| Attribute | Limitation |
|-----------|------------|
| MIGHT | No accuracy boost, no survivability |
| FINESSE | Countered by AoE, penalized by Stress |
| STURDINESS | Purely defensive, no offensive value |
| WITS | No direct combat damage contribution |
| WILL | Offensive only for Mystics |

### 6.2 Known Balance Considerations

- **WILL as God Stat**: Mitigated by gear alternatives & party support
- **FINESSE Dodge Tank**: Countered by Stress penalties & AoE

---

## 7. Technical Implementation

### 7.1 C# Entity

```csharp
public class CharacterAttributes
{
    public int Might { get; private set; } = 5;
    public int Finesse { get; private set; } = 5;
    public int Sturdiness { get; private set; } = 5;
    public int Wits { get; private set; } = 5;
    public int Will { get; private set; } = 5;
    
    public int GetAttribute(AttributeType type) => type switch
    {
        AttributeType.Might => Might,
        AttributeType.Finesse => Finesse,
        AttributeType.Sturdiness => Sturdiness,
        AttributeType.Wits => Wits,
        AttributeType.Will => Will,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}

public enum AttributeType { Might, Finesse, Sturdiness, Wits, Will }
```

### 7.2 Database Schema

```sql
CREATE TABLE character_attributes (
    character_id UUID PRIMARY KEY REFERENCES characters(id) ON DELETE CASCADE,
    might INT NOT NULL DEFAULT 5 CHECK (might >= 5),
    finesse INT NOT NULL DEFAULT 5 CHECK (finesse >= 5),
    sturdiness INT NOT NULL DEFAULT 5 CHECK (sturdiness >= 5),
    wits INT NOT NULL DEFAULT 5 CHECK (wits >= 5),
    will INT NOT NULL DEFAULT 5 CHECK (will >= 5),
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

---

## 8. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Data Model**: Implement `CharacterAttributes` class and enum.
- [ ] **Formulas**: Implement derived stat calculation logic (HP, AP, Defense).
- [ ] **Services**: Create `AttributeService` for handling increases/checks.

### Phase 2: Persistence
- [ ] **Schema**: Implement `character_attributes` table.
- [ ] **Repo**: Update `CharacterRepository` to include attributes in load.
- [ ] **Tests**: Verify persistence of allocated points.

### Phase 3: Integration
- [ ] **Character Creation**: Hook up point-buy logic to `CharacterCreationService`.
- [ ] **Saga System**: Connect PP spending to attribute increases.
- [ ] **Combat**: Ensure Combat Engine uses derived stats (not hardcoded).

### Phase 4: UI
- [ ] **Character Sheet**: Display attributes and derived stats clearly.
- [ ] **Level Up**: Implement attribute increase UI in Saga Menu.
- [ ] **Tooltips**: Show what each attribute affects on hover.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Bounds**: Verify attributes cannot go below 5.
- [ ] **Cost Calculation**: Verify Point Buy cost (1 for 5->6, 2 for 8->9).
- [ ] **Derived Stats**: Verify HP = 50 + (Sturdiness * 10).
- [ ] **Switch Expression**: Verify `GetAttribute` returns correct value for enum.

### 9.2 Integration Tests
- [ ] **Persistence**: Save attributes -> Load -> Verify equality.
- [ ] **Creation**: Allocate 15 points -> Save -> Verify total points correct.
- [ ] **Progression**: Spend PP -> Verify attribute increases and PP decreases.

### 9.3 Manual QA
- [ ] **Creation UI**: Try to spend more than 15 points (blocked).
- [ ] **Saga UI**: Try to buy upgrade without PP (blocked).

---

## 10. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 10.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Attribute Check | Verbose | "{Character} rolled {Attribute} check: {Result}" | `Character`, `Attribute`, `Result` |
| Attribute Increase | Information | "{Character} increased {Attribute} to {Value}" | `Character`, `Attribute`, `Value` |
| Point Allocation | Information | "Character creation attributes finalized for {Character}" | `Character` |

---

## 11. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-DICE` | Attributes provide base dice pools |
| `SPEC-CORE-RESOURCES` | HP, Stamina, AP derived from attributes |
| `SPEC-COMBAT-ENGINE` | Combat uses attribute-based pools |
| `SPEC-SKILLS` | Skills add to attribute-based dice pools |
| `SPEC-CMD-DIALOGUE` | MIGHT/WITS/WILL used for dialogue checks |
| `SPEC-CMD-CRAFTING` | WITS governs all crafting trades |
| `SPEC-CMD-REST-CAMP` | Saga menu for attribute progression |

---

## 12. Command Integration Summary

Attributes are used across game commands as follows:

| Command | Primary Attribute | Usage |
|---------|-------------------|-------|
| **Dialogue** (`talk`, `say`) | MIGHT, WITS, WILL | Intimidation, Insight, Persuasion checks |
| **Crafting** (`craft`, `brew`, `forge`, `repair`) | WITS | Crafting pool = WITS + Skill Rank |
| **Rest** (`rest`, `camp`) | STURDINESS, WILL | Recovery rates, Stress resistance |
| **Inventory** (`equip`) | MIGHT, STURDINESS | Equipment requirements |
| **Combat** | All | Accuracy (FINESSE), Damage (MIGHT/FINESSE), Defense (FINESSE) |
