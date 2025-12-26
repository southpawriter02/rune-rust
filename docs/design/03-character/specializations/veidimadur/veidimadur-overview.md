---
id: SPEC-SPECIALIZATION-VEIDIMADUR
title: "Veiðimaðr (The Hunter Who Tracks the Blight)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Veiðimaðr (The Hunter Who Tracks the Blight)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Veiðimaðr (Hunter) |
| **Translation** | "The Hunter Who Tracks the Blight" |
| **Archetype** | Skirmisher |
| **Path Type** | Coherent |
| **Mechanical Role** | Ranged DPS / Corruption Tracker |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina + Focus |
| **Trauma Risk** | Medium |
| **Icon** | :bow_and_arrow: |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | No prerequisite |

---

## 3. Design Philosophy

**Tagline:** "You've learned to read the invisible signs of the Runic Blight."

**Core Fantasy:** You are the patient predator of a corrupted world. You've learned to read the invisible signs of the Runic Blight—the subtle tells that reveal a creature's corruption level. You mark high-priority targets, exploit their weaknesses, and deliver devastating shots from the safety of the back row.

**Mechanical Identity:**
1. **Corruption Tracking** — Specializes in detecting and exploiting enemy corruption levels
2. **Mark System** — Mark for Death provides significant damage bonuses
3. **Back Row Safety** — Gains defensive bonuses when positioned in back row
4. **Corruption Purging** — Can purge corruption from heavily Blighted foes for bonus damage
5. **Trap Specialist** — Set snares to control enemy movement

**Gameplay Feel:** A calculating predator who marks targets, controls space with traps, and delivers devastating precision shots from safety.

---

## 4. The Focus System

### 4.1 Focus Mechanics

| Property | Value |
|----------|-------|
| **Range** | 0-100 |
| **Decay** | Slow decay out of combat |
| **Generation** | Maintain back row position, mark targets |

### 4.2 Focus Spending

| Cost | Abilities |
|------|-----------|
| 30 Focus | Heartseeker Shot |

---

## 5. Rank Progression

### 5.1 Rank Unlock Rules

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 | Rank 2 (2× Tier 2) | Capstone trained |
| **Tier 2** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Tier 3** | No ranks | N/A | N/A |
| **Capstone** | No ranks | N/A | N/A |

### 5.2 Total PP Investment

| Milestone | PP Spent | Tier 1 Rank | Notes |
|-----------|----------|-------------|-------|
| Unlock Specialization | 3 PP | - | |
| All Tier 1 | 12 PP | Rank 1 | 3 abilities |
| 2× Tier 2 | 20 PP | **Rank 2** | |
| All Tier 2 | 24 PP | Rank 2 | |
| All Tier 3 | 34 PP | Rank 2 | |
| Capstone | 40 PP | **Rank 3** | Full tree |

---

## 6. Ability Tree

### 6.1 Visual Structure

```
                    TIER 1: FOUNDATION (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Wilderness         [Aimed Shot]         [Set Snare]
Acclimation I]
  (Passive)          (Active)            (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Mark for          [Blight-Tipped      [Predator's
  Death]              Arrow]             Focus]
  (Active)          (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
    [Exploit             [Heartseeker
   Corruption]              Shot]
    (Passive)              (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
             [Stalker of the Unseen]
                    (Hybrid)
```

### 6.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 24001 | Wilderness Acclimation I | 1 | Passive | 3 | [wilderness-acclimation-i.md](abilities/wilderness-acclimation-i.md) |
| 24002 | Aimed Shot | 1 | Active | 3 | [aimed-shot.md](abilities/aimed-shot.md) |
| 24003 | Set Snare | 1 | Active | 3 | [set-snare.md](abilities/set-snare.md) |
| 24004 | Mark for Death | 2 | Active | 4 | [mark-for-death.md](abilities/mark-for-death.md) |
| 24005 | Blight-Tipped Arrow | 2 | Active | 4 | [blight-tipped-arrow.md](abilities/blight-tipped-arrow.md) |
| 24006 | Predator's Focus | 2 | Passive | 4 | [predators-focus.md](abilities/predators-focus.md) |
| 24007 | Exploit Corruption | 3 | Passive | 5 | [exploit-corruption.md](abilities/exploit-corruption.md) |
| 24008 | Heartseeker Shot | 3 | Active | 5 | [heartseeker-shot.md](abilities/heartseeker-shot.md) |
| 24009 | Stalker of the Unseen | 4 | Hybrid | 6 | [stalker-of-the-unseen.md](abilities/stalker-of-the-unseen.md) |

---

## 7. Situational Power Profile

### 7.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| High-corruption enemies | Massive crit bonuses |
| Back row positioning | Defensive bonuses + Focus |
| Single priority targets | Mark for Death excels |
| Exploration scenarios | Trap placement, tracking |

### 7.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Melee-forced fights | Loses back row benefits |
| Low-corruption enemies | Reduced kit effectiveness |
| Multiple equal threats | Mark system favors single target |
| No setup time | Traps need pre-placement |

---

## 8. Party Synergies

### 8.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Skjaldmær** | Tank maintains frontline, Hunter safe in back |
| **Jötun-Reader** | Combined analysis + mark for devastation |
| **Iron-Bane** | Both excel vs corrupted targets |
| **Ruin-Stalker** | Combined trap control |

### 8.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| Other ranged-only builds | No frontline protection |
| Aggro-less compositions | Hunter draws attacks |

---

## 9. Integration Points

| System | Integration |
|--------|-------------|
| **Corruption System** | Corruption level detection |
| **Status Effects** | [Marked], [Rooted], [Blighted Toxin], [Glitch], [Bleeding] |
| **Trap System** | Set Snare placement |
| **Combat Positioning** | Back Row bonuses |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Damage Output | Survivability | Utility |
|--------|---------------|---------------|---------|
| 1-3 | Medium | Medium | High |
| 4-6 | High | Medium | High |
| 7-10 | Very High | Medium | Very High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★★★★★ | Mark + Heartseeker |
| AoE DPS | ★★☆☆☆ | Limited AoE |
| Tanking | ★☆☆☆☆ | Back row specialist |
| Healing | ☆☆☆☆☆ | None |
| Utility | ★★★★☆ | Traps, tracking, marking |

### 10.3 Corruption Bonus Analysis

| Corruption Level | Crit Bonus | Notes |
|------------------|------------|-------|
| Low (1-29) | +10% | Minimal benefit |
| Medium (30-59) | +20% | Solid advantage |
| High (60-89) | +30% | Major bonus |
| Extreme (90+) | +40% | Devastating |

---

## 11. Voice Guidance

**Reference:** [/docs/.templates/flavor-text/specialization-abilities.md]

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Patient, calculating, predatory |
| **Key Words** | Track, mark, hunt, prey, corruption |
| **Sentence Style** | Measured, observational |

### 11.2 Example Voice

> **Activation:** "I see you. I know what you are."
> **Effect:** "The corruption betrays your weakness."
> **Failure:** "The trail has gone cold."

---

## 12. Phased Implementation Guide

### Phase 1: Foundation
- [ ] **Define Status Effects**: Implement [Marked], [Rooted], [Blighted Toxin], [Glitch].
- [ ] **Corruption Detection**: Implement corruption level reading.
- [ ] **Position Tracking**: Implement Back Row detection for bonuses.

### Phase 2: Core Abilities
- [ ] **Mark for Death**: Implement targeted debuff with damage bonus.
- [ ] **Set Snare**: Implement trap placement system.
- [ ] **Aimed Shot**: Implement basic ranged attack with rank scaling.

### Phase 3: Advanced Abilities
- [ ] **Blight-Tipped Arrow**: Implement DoT + [Glitch] vs corrupted.
- [ ] **Heartseeker Shot**: Implement charged shot + corruption purge.
- [ ] **Exploit Corruption**: Implement crit scaling by corruption level.

### Phase 4: Capstone
- [ ] **Stalker of the Unseen**: Implement stance toggle.
- [ ] **Vulnerability Detection**: Implement auto-learn on Mark.
- [ ] **Visual Immunity**: Implement blindness/obscure immunity.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Corruption Detection**: Verify correct level categorization.
- [ ] **Mark Duration**: Verify 3-4 turn duration by rank.
- [ ] **Crit Scaling**: Verify +10/20/30/40% by corruption level.

### 13.2 Integration Tests
- [ ] **Trap Trigger**: Verify snare activates on enemy movement.
- [ ] **Charged Shot**: Verify Heartseeker requires full turn charge.
- [ ] **Corruption Purge**: Verify +2 damage per corruption purged.

### 13.3 Manual QA
- [ ] **Mark Icon**: Display correctly on marked target.
- [ ] **Trap Placement**: Visual feedback for snare location.
- [ ] **Stance Toggle**: Clear UI for Blight-Stalker's Stance.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Veiðimaðr for {Character}." | `Character` |
| Target Marked | Info | "{Character} marked {Target} for death." | `Character`, `Target` |
| Trap Triggered | Info | "{Target} triggered snare! [Rooted] for {Duration} turns." | `Target`, `Duration` |
| Corruption Purged | Info | "{Character} purged {Amount} corruption from {Target}." | `Character`, `Amount`, `Target` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Rooted](../../../04-systems/status-effects/rooted.md) | Set Snare effect |
| [Bleeding](../../../04-systems/status-effects/bleeding.md) | Aimed Shot crit effect |
| [Corruption](../../../01-core/resources/coherence.md) | Corruption level system |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration from legacy spec |
