---
id: SPEC-SPECIALIZATION-JOTUNREADER
title: "Jötun-Reader (Forensic Pathologist of the Apocalypse)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Jötun-Reader (Forensic Pathologist of the Apocalypse)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Jötun-Reader |
| **Translation** | "Forensic Pathologist of the Apocalypse" |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Mechanical Role** | Controller / Utility Specialist |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina + Psychic Stress |
| **Trauma Risk** | High |
| **Icon** | :mag: |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard unlock cost |
| **Minimum Legend** | 3 | Early-mid game |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | No prerequisite |

---

## 3. Design Philosophy

**Tagline:** "You read the crash logs of a dead civilization."

**Core Fantasy:** You are the scholar-pathologist who reads the crash logs of a dead civilization. Where others see chaos, you see patterns. You translate error messages carved in ancient stone, identify structural flaws in corrupted war-machines, and speak fragments of command-line code that freeze enemies in logic conflicts.

**Mechanical Identity:**
1. **Information Warfare** — Analyze weaknesses, expose enemy defenses to allies
2. **Linguistic Archaeology** — Translate ancient Jötun inscriptions, decipher error codes
3. **Force Multiplier** — Zero direct damage, but increases party effectiveness by 30-40%
4. **Trauma Economy Integration** — Most abilities cost Psychic Stress (high-risk/high-reward)
5. **Jötun-Forged Specialist** — Ultimate power against machine-type enemies

**Gameplay Feel:** A tactical support role that rewards careful observation and party coordination. Your knowledge is your weapon, turning analysis into overwhelming advantage.

---

## 4. Core Mechanics

### 4.1 Psychic Stress Costs

Many Jötun-Reader abilities inflict Psychic Stress on the user:

| Ability | Stress Cost (by Rank) |
|---------|----------------------|
| Analyze Weakness | 5 → 3 → 0 |
| Architect of the Silence | 10-15 (Capstone) |

### 4.2 Target Type Bonuses

The Jötun-Reader excels against specific enemy types:

| Enemy Type | Bonus |
|------------|-------|
| **Jötun-Forged** | Capstone auto-analysis, [Seized] effect |
| **Undying** | Capstone auto-analysis, [Seized] effect |

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
[Scholarly          [Analyze             [Runic
 Acumen I]          Weakness]          Linguistics]
 (Passive)           (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Exploit           [Navigational       [Structural
 Design Flaw]        Bypass]            Insight]
  (Active)          (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Calculated            [The Unspoken
     Triage]                  Truth]
    (Passive)                (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
             [Architect of the Silence]
                     (Active)
```

### 6.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 201 | Scholarly Acumen I | 1 | Passive | 3 | [scholarly-acumen-i.md](abilities/scholarly-acumen-i.md) |
| 202 | Analyze Weakness | 1 | Active | 3 | [analyze-weakness.md](abilities/analyze-weakness.md) |
| 203 | Runic Linguistics | 1 | Passive | 3 | [runic-linguistics.md](abilities/runic-linguistics.md) |
| 204 | Exploit Design Flaw | 2 | Active | 4 | [exploit-design-flaw.md](abilities/exploit-design-flaw.md) |
| 205 | Navigational Bypass | 2 | Active | 4 | [navigational-bypass.md](abilities/navigational-bypass.md) |
| 206 | Structural Insight | 2 | Passive | 4 | [structural-insight.md](abilities/structural-insight.md) |
| 207 | Calculated Triage | 3 | Passive | 5 | [calculated-triage.md](abilities/calculated-triage.md) |
| 208 | The Unspoken Truth | 3 | Active | 5 | [the-unspoken-truth.md](abilities/the-unspoken-truth.md) |
| 209 | Architect of the Silence | 4 | Active | 6 | [architect-of-the-silence.md](abilities/architect-of-the-silence.md) |

---

## 7. Situational Power Profile

### 7.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| Party with DPS characters | +10-15% party hit rate via [Analyzed] |
| Jötun-Forged enemies | Capstone provides complete shutdown |
| Exploration-heavy dungeons | Runic Linguistics unlocks 30% of content |
| Bone-Setter in party | Stress management sustains abilities |

### 7.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Solo scenarios | Zero direct damage output |
| Combat-only dungeons | Exploration abilities wasted |
| No healer support | Psychic Stress costs unsustainable |
| Non-machine enemies | Capstone cannot be used |

---

## 8. Party Synergies

### 8.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Bone-Setter** | Essential for Psychic Stress management |
| **Skjaldmær** | Tank benefits from +Accuracy; Reader safe behind shield |
| **Any DPS** | +10-15% hit rate dramatically increases damage output |
| **Skald** | Combined party buffs create overwhelming advantage |

### 8.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| Other support-only builds | Not enough damage dealers |
| Solo compositions | Information warfare needs targets to enhance |

---

## 9. Integration Points

| System | Integration |
|--------|-------------|
| **Trauma Economy** | High Psychic Stress costs |
| **Status Effects** | [Analyzed], [Disoriented], [Shaken], [Fixated], [Seized] |
| **Exploration** | Translation system, trap detection |
| **Combat** | Party-wide accuracy buffs, enemy analysis |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Support Value | Survivability | Utility |
|--------|---------------|---------------|---------|
| 1-3 | Medium | Low | High |
| 4-6 | High | Low | Very High |
| 7-10 | Very High | Medium | Very High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ☆☆☆☆☆ | Zero direct damage |
| AoE DPS | ☆☆☆☆☆ | Zero direct damage |
| Tanking | ★☆☆☆☆ | Can redirect aggro via Fixated |
| Healing | ★★★☆☆ | +50-75% healing amplification |
| Utility | ★★★★★ | Best-in-class support |

### 10.3 Party Impact Analysis

| Metric | Value | Notes |
|--------|-------|-------|
| Party hit rate increase | +10-15% | Via [Analyzed] debuff |
| Healing amplification | +50-75% | Via Calculated Triage |
| Exploration unlock | 20-30% | Via Runic Linguistics |
| Machine shutdown | 100% | Via Capstone |

---

## 11. Voice Guidance

**Reference:** [/docs/.templates/flavor-text/specialization-abilities.md]

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Clinical, precise, scholarly |
| **Key Words** | Analyze, pattern, code, syntax, protocol |
| **Sentence Style** | Technical, diagnostic, matter-of-fact |

### 11.2 Example Voice

> **Activation:** "Pattern recognized. Initiating analysis protocol."
> **Effect:** "Structural weakness identified—strike the left actuator joint."
> **Failure:** "Data corrupted. Analysis inconclusive."

---

## 12. Phased Implementation Guide

### Phase 1: Foundation
- [ ] **Define Status Effects**: Implement [Analyzed], [Disoriented], [Shaken], [Fixated], [Seized].
- [ ] **Enemy Type Tagging**: Add Jötun-Forged and Undying type tags to enemies.
- [ ] **Psychic Stress Costs**: Hook abilities to Stress system.

### Phase 2: Core Abilities
- [ ] **Analyze Weakness**: Implement enemy stat revelation system.
- [ ] **Exploit Design Flaw**: Implement party-wide accuracy modifier.
- [ ] **Runic Linguistics**: Implement translation system for exploration.

### Phase 3: Advanced Abilities
- [ ] **Calculated Triage**: Implement healing amplification aura.
- [ ] **The Unspoken Truth**: Implement opposed WITS vs WILL check.
- [ ] **Structural Insight**: Implement environmental hazard detection.

### Phase 4: Capstone
- [ ] **Architect of the Silence**: Implement target restriction (Jötun-Forged/Undying).
- [ ] **[Seized] Effect**: Implement complete paralysis status.
- [ ] **Auto-Analysis Passive**: Implement combat-start auto-reveal for machines.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Stress Costs**: Verify abilities deduct correct Psychic Stress per rank.
- [ ] **[Analyzed] Duration**: Verify 2-4 turn duration by rank.
- [ ] **Capstone Target Restriction**: Verify only Jötun-Forged/Undying can be targeted.

### 13.2 Integration Tests
- [ ] **Party Accuracy Buff**: Verify all party members receive +Accuracy vs [Analyzed].
- [ ] **Healing Amplification**: Verify +50% healing within Calculated Triage range.
- [ ] **Translation System**: Verify Runic Linguistics unlocks appropriate content.

### 13.3 Manual QA
- [ ] **UI Check**: [Analyzed] debuff icon appears on enemy.
- [ ] **Stress Display**: Psychic Stress costs shown before ability use.
- [ ] **Capstone Feedback**: [Seized] visual effect displays correctly.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Jötun-Reader for {Character}." | `Character` |
| Analysis Complete | Info | "{Character} analyzed {Target}: {Weaknesses} revealed." | `Character`, `Target`, `Weaknesses` |
| Capstone Used | Info | "{Character} used Architect of the Silence on {Target}." | `Character`, `Target` |
| [Seized] Applied | Info | "{Target} is [Seized] for {Duration} rounds." | `Target`, `Duration` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Disoriented](../../../04-systems/status-effects/disoriented.md) | The Unspoken Truth effect |
| [Psychic Stress](../../../01-core/resources/stress.md) | Ability costs |
| [Enemy Types](../../../03-combat/creature-traits.md) | Jötun-Forged/Undying definitions |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration from legacy spec |
