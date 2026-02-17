---
id: SPEC-SPECIALIZATION-RUIN-STALKER
title: "Ruin-Stalker (System Anomaly Hunter)"
version: 1.0
status: implemented
last-updated: 2025-12-08
---

# Ruin-Stalker (System Anomaly Hunter)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Ruin-Stalker |
| **Translation** | "System Anomaly Hunter" |
| **Archetype** | Skirmisher |
| **Path Type** | Coherent |
| **Mechanical Role** | Scout / Utility |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina |
| **Trauma Risk** | Medium (CPS exposure) |
| **Icon** | 🔍 |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | |
| **Minimum Legend** | 2 | Early game |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | |

---

## 3. Design Philosophy

**Tagline:** "Preparation defeats danger. Your saga is written in mapped corridors, bypassed hazards, and teammates who survived because you entered first."

**Core Fantasy:** You are the master of dead places. Where others see crumbling Old World facilities as death traps, you see charted harvest routes, documented anomaly patterns, and survivable extraction protocols. You don't explore ruins — you systematically dismantle their threats.

**Mechanical Identity:**
1. **Survey Specialist** — Detect anomalies, traps, and hazards before advancing
2. **Hazard Navigator** — Guide party safely through dangerous environments
3. **Trap Master** — Disarm enemy traps and deploy your own
4. **CPS Manager** — Resist and mitigate Cognitive Paradox Syndrome
5. **Situational Power** — Invaluable in ruins, less impactful in open combat

**Gameplay Feel:** Methodical utility specialist. Your loop: Survey → Map → Disable → Extract. Success comes from preparation, not speed.

---

## 4. Core Mechanics

### 4.1 Anomaly Detection & Classification

Ruin-Stalkers perceive and classify Old World anomalies:

| Anomaly Type | Description | Counter-Protocol |
|--------------|-------------|------------------|
| **Echo Surface** | Reflective materials copying movement | Veil with anti-pattern cloth |
| **Singing Vent** | Tones pulling breath to match | Ear baffles, off-rhythm steps |
| **Animated Script** | Drifting runes responding to gaze | Cover immediately, call Rúnasmiðr |
| **Loop Corridor** | Architecture returning in short cycles | Mark and exit |
| **Frame Residue** | J.T.N. work-prints felt in teeth | Record, return with specialists |

### 4.2 CPS Hygiene Protocols

**Cognitive Paradox Syndrome** is the occupational hazard of ruin work:

| Protocol | Requirement |
|----------|-------------|
| **Entry Windows** | 15-30 min maximum first entry |
| **Rotation Cadence** | Rotate lead observer every 12 min |
| **Abort Criteria** | Any 2 Stage 1 precursors |
| **Silent Room** | Dim • Dampen • De-pattern • Document |

### 4.3 Stage 1 CPS Precursors

- Auditory paresthesia
- Semantic drift
- Looped prosody
- Fixation behaviors

---

## 5. Rank Progression

### 5.1 Rank Unlock Rules

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 | Rank 2 (2× Tier 2) | Capstone |
| **Tier 2** | Rank 2 | Rank 3 (Capstone) | Capstone |
| **Tier 3** | Rank 2 | Rank 3 (Capstone) | Capstone |
| **Capstone** | Rank 1 | Rank 2→3 (tree) | Full tree |

### 5.2 Total PP Investment

| Milestone | PP Spent | Tier 1 Rank |
|-----------|----------|-------------|
| Unlock Specialization | 10 PP | - |
| All Tier 1 | 19 PP | Rank 1 |
| 2× Tier 2 | 27 PP | **Rank 2** |
| All Tier 2 | 31 PP | Rank 2 |
| All Tier 3 | 41 PP | Rank 2 |
| Capstone | 47 PP | **Rank 3** |

---

## 6. Ability Tree

### 6.1 Visual Structure

```
                    TIER 1: FOUNDATIONAL SURVEY (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Anomaly Sense I]   [Careful Advance]     [Disarm
   (Passive)           (Active)          Mechanism]
    │                     │                (Active)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED NAVIGATION (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Hazard Mapping]   [Anomaly             [CPS
   (Active)      Exploitation]       Resistance]
                    (Active)           (Passive)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
[Safe Extraction       [Master Trapsmith]
    Protocol]           (Passive + Active)
    (Active)
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
                [Corridor Maker]
                (Passive + Active)
```

### 6.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 4001 | Anomaly Sense I | 1 | Passive | 3 | [anomaly-sense-i.md](abilities/anomaly-sense-i.md) |
| 4002 | Careful Advance | 1 | Active | 3 | [careful-advance.md](abilities/careful-advance.md) |
| 4003 | Disarm Mechanism | 1 | Active | 3 | [disarm-mechanism.md](abilities/disarm-mechanism.md) |
| 4004 | Hazard Mapping | 2 | Active | 4 | [hazard-mapping.md](abilities/hazard-mapping.md) |
| 4005 | Anomaly Exploitation | 2 | Active | 4 | [anomaly-exploitation.md](abilities/anomaly-exploitation.md) |
| 4006 | CPS Resistance | 2 | Passive | 4 | [cps-resistance.md](abilities/cps-resistance.md) |
| 4007 | Safe Extraction Protocol | 3 | Active | 5 | [safe-extraction-protocol.md](abilities/safe-extraction-protocol.md) |
| 4008 | Master Trapsmith | 3 | Passive + Active | 5 | [master-trapsmith.md](abilities/master-trapsmith.md) |
| 4009 | Corridor Maker | 4 | Passive + Active | 6 | [corridor-maker.md](abilities/corridor-maker.md) |

---

## 7. Situational Power Profile

### 7.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| Old World ruins | Full toolkit applies |
| Anomaly-dense zones | Detection + exploitation |
| Trap-heavy environments | Disarm + salvage |
| Exploration sessions | Survey + mapping |

### 7.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Open wilderness | No traps/anomalies |
| Direct combat | Utility, not damage |
| Time-pressured scenarios | Survey requires time |
| Anomaly-free zones | Kit doesn't apply |

---

## 8. Party Synergies

### 8.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Jötun-Reader** | Survey + translation = full comprehension |
| **Bone-Setter** | CPS management, stress healing |
| **Rúnasmiðr** | You find inscriptions, they work them |
| **Skjaldmær** | Hold thresholds while you survey |

### 8.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| Aggressive parties | Don't respect survey cadence |
| Impatient playstyles | Value is in preparation |

---

## 9. Integration Points

| System | Integration |
|--------|-------------|
| **Trap System** | Detection, disarm, deployment |
| **Anomaly System** | Classification, exploitation |
| **Stress/CPS** | Resistance, management |
| **Exploration** | Survey, mapping, extraction |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Mobility | Utility | Combat Power |
|--------|----------|---------|--------------|
| 1-3 | High | Medium | Low |
| 4-6 | Very High | High | Medium |
| 7-10 | Maximum | Very High | High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★★☆☆☆ | Relies on traps/ambush |
| AoE DPS | ★★★☆☆ | Trap detonation |
| Tanking | ★☆☆☆☆ | Avoidance tanking only |
| Scouting | ★★★★★ | Unmatched |
| Utility | ★★★★☆ | Trap removal & safe paths |

---

## 11. Voice Guidance

**Reference:** [npc-flavor.md](../../../.templates/flavor-text/npc-flavor.md)

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Quiet, observant, pragmatic |
| **Key Words** | Pattern, loop, trace, echo, breach |
| **Sentence Style** | Observations, warnings, precise directions |

### 11.2 Example Voice

> **Activation:** "Pattern identified. Don't blink."
> **Effect:** "The corridor shifts. Follow my steps exactly."
> **Failure:** "Anomaly spike! It's de-synchronizing!"

---

## 12. Phased Implementation Guide

### Phase 1: Logic & Detection
- [ ] **Factory**: Register `RuinStalkerSpecialization`.
- [ ] **Perception**: Implement `TrapDetectionChance` and `AnomalySense` modifiers.

### Phase 2: Core Abilities
- [ ] **Traps**: Implement `DisarmMechanism` and `DeployTrap` systems.
- [ ] **Survey**: Implement `HazardMapping` revealing map tiles.

### Phase 3: Environment Integration
- [ ] **Room Engine**: Hook `CorridorMaker` into dungeon generation/modification.
- [ ] **CPS**: Implement `CPSResistance` logic in `TraumaService`.

### Phase 4: UI & Feedback
- [ ] **Visuals**: Highlight traps for Stalkers.
- [ ] **Minimap**: Show hazards on minimap after mapping.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Sense**: Anomaly detection roll includes WITS bonus.
- [ ] **Disarm**: Disarm check vs Trap DC.
- [ ] **CPS**: Verify CPS accumulation is reduced by Resistance.
- [ ] **Stealth**: Calculate stealth duration and break conditions.

### 13.2 Integration Tests
- [ ] **Mapping**: Use Hazard Mapping -> Hidden traps become visible.
- [ ] **Traps**: Walk into own trap -> Does not trigger (if Master).
- [ ] **Corridor**: Activate Corridor Maker -> New exit appears.

### 13.3 Manual QA
- [ ] **Visual**: Trap overlay renders correctly.
- [ ] **Audio**: Warning cue plays near anomaly.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Ruin-Stalker for {Character}." | `Character` |
| Trap Detect | Info | "{Character} detected a {TrapType}." | `Character`, `TrapType` |
| Disarm | Info | "{Character} disarmed {Trap} (Result: {Result})." | `Character`, `Trap`, `Result` |
| Anomaly | Info | "{Character} identified anomaly: {Anomaly}." | `Character`, `Anomaly` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Stress](../../01-core/resources/stress.md) | CPS mechanics |
| [Disoriented](../../04-systems/status-effects/disoriented.md) | Anomaly effect |
| [Stunned](../../04-systems/status-effects/stunned.md) | Trap effect |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-08 | Initial specification |
| 1.1 | 2025-12-13 | Standardized with Phased Guide, Logging, and Testing sections |
