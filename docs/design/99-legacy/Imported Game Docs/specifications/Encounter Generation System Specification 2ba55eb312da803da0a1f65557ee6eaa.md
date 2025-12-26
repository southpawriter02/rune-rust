# Encounter Generation System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-SYSTEM-007
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Combat Designer
- **Design**: Encounter composition, difficulty scaling
- **Balance**: Budget allocation, player challenge
- **Implementation**: EncounterService.cs, BudgetDistributionService.cs
- **QA/Testing**: Encounter validity, difficulty testing

---

## Executive Summary

### Purpose Statement

The Encounter Generation System creates balanced combat encounters by composing enemy groups based on budget allocation, player milestone, room type, and faction constraints.

### Scope

**In Scope**:

- Encounter budget calculation
- Enemy group composition
- Difficulty scaling by milestone
- Boss phase mechanics
- Faction-based encounters
- Room population

**Out of Scope**:

- Enemy AI behavior → `SPEC-SYSTEM-005`
- Enemy stat definitions → `SPEC-COMBAT-003`
- Loot generation → `SPEC-ECONOMY-001`
- Room generation → `SPEC-SYSTEM-008`

---

## Design Philosophy

### Design Pillars

1. **Budget-Based Balance**
    - **Rationale**: Encounter difficulty controlled via point budget
    - **Examples**: Milestone 3 = 15 budget points, distributed among enemies
2. **Composition Variety**
    - **Rationale**: Encounters should feel varied
    - **Examples**: Mix of frontline + support, not just 5 identical enemies
3. **Faction Identity**
    - **Rationale**: Each faction should fight distinctly
    - **Examples**: Draugr swarm, Jotun use heavy hitters

---

## Functional Requirements

### FR-001: Calculate Encounter Budget

**Priority**: Critical
**Status**: Implemented

**Formula**:

```
Budget = BaseBudget + (Milestone × BudgetPerMilestone) + RoomModifier

Example:
  BaseBudget = 5
  Milestone = 3
  BudgetPerMilestone = 3
  RoomModifier = +2 (Elite room)

  Budget = 5 + (3 × 3) + 2 = 16 points

```

### FR-002: Compose Enemy Group

**Priority**: Critical
**Status**: Implemented

**Description**:
System must select enemies to fill budget while maintaining composition rules.

**Composition Rules**:

- Max 6 enemies per encounter
- At least 1 frontline enemy
- Support enemies require frontline
- Boss encounters are 1 boss + potential adds

### FR-003: Scale Difficulty by Milestone

**Priority**: High
**Status**: Implemented

**Scaling Table**:

| Milestone | Budget | Max Enemies | Boss HP Modifier |
| --- | --- | --- | --- |
| 0 | 5 | 3 | N/A |
| 1 | 8 | 4 | 1.0× |
| 2 | 12 | 5 | 1.2× |
| 3+ | 15+ | 6 | 1.5× |

### FR-004: Generate Boss Encounters

**Priority**: High
**Status**: Implemented

**Boss Phase Structure**:

- Phase 1 (100-70% HP): Normal abilities
- Phase 2 (70-30% HP): Enhanced abilities, adds spawn
- Phase 3 (30-0% HP): Enraged, signature abilities

---

## System Mechanics

### Mechanic 1: Budget Allocation

**Enemy Costs**:

| Enemy Type | Budget Cost | Example |
| --- | --- | --- |
| Fodder | 1 | Rust Mite |
| Standard | 2 | Draugr |
| Elite | 4 | Champion |
| Support | 3 | Healer |
| Boss | Special | Named Boss |

**Allocation Algorithm**:

```
1. Determine total budget
2. Reserve minimum for frontline (2 points)
3. Roll for composition style (swarm, balanced, elite)
4. Fill remaining budget with appropriate enemies
5. Validate composition rules

```

### Mechanic 2: Faction Encounters

**Faction Composition Templates**:

| Faction | Style | Typical Composition |
| --- | --- | --- |
| Draugr | Swarm | 4-5 Fodder + 1 Elite |
| Jotun | Heavy | 1-2 Elite + 1 Support |
| Corrupted | Mixed | 2 Standard + 2 Corrupted |
| Blight | Chaotic | Random mix |

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Impact |
| --- | --- | --- | --- |
| BaseBudget | EncounterService | 5 | Starting difficulty |
| BudgetPerMilestone | EncounterService | 3 | Scaling rate |
| MaxEnemies | EncounterService | 6 | Combat complexity |

---

**End of Specification**