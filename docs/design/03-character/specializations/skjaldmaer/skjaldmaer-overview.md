---
id: SPEC-SPECIALIZATION-SKJALDMAER
title: "Skjaldmær (The Bastion of Coherence)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Skjaldmær (The Bastion of Coherence)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Skjaldmær (Shieldmaiden) |
| **Translation** | "The Bastion of Coherence" |
| **Archetype** | Warrior |
| **Path Type** | Coherent |
| **Mechanical Role** | Tank / Psychic Stress Mitigation |
| **Primary Attribute** | STURDINESS |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina |
| **Trauma Risk** | Low |
| **Icon** | :shield: |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | High-impact role |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | No prerequisite |

---

## 3. Design Philosophy

**Tagline:** "Her shield is a grounding rod against the psychic scream of the Great Silence."

**Core Fantasy:** The Skjaldmær is a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, she shields not just bodies but sanity itself. Her power comes from indomitable WILL channeled into protection, transforming the tank role from 'meat shield' to 'reality anchor.'

**Mechanical Identity:**
1. **Dual Protection** — Shields both HP and Psychic Stress simultaneously
2. **Trauma Economy Anchor** — Actively mitigates party Psychic Stress through abilities and auras
3. **WILL-Based Tanking** — Taunt system draws aggro with WILL-based projection of coherence
4. **Ultimate Sacrifice** — Capstone ability allows absorbing permanent Trauma to save allies
5. **Reaction Specialist** — Key abilities trigger as reactions to protect allies

**Gameplay Feel:** A stalwart guardian who provides unshakeable stability. Position carefully, react decisively, and be willing to sacrifice for the team.

---

## 4. Rank Progression

### 4.1 Rank Unlock Rules

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 | Rank 2 (2× Tier 2) | Capstone trained |
| **Tier 2** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Tier 3** | No ranks | N/A | N/A |
| **Capstone** | No ranks | N/A | N/A |

### 4.2 Total PP Investment

| Milestone | PP Spent | Tier 1 Rank | Notes |
|-----------|----------|-------------|-------|
| Unlock Specialization | 10 PP | - | |
| All Tier 1 | 19 PP | Rank 1 | 3 abilities |
| 2× Tier 2 | 27 PP | **Rank 2** | |
| All Tier 2 | 31 PP | Rank 2 | |
| All Tier 3 | 41 PP | Rank 2 | |
| Capstone | 47 PP | **Rank 3** | Full tree |

---

## 5. Ability Tree

### 5.1 Visual Structure

```
                    TIER 1: FOUNDATION (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Sanctified        [Shield Bash]       [Oath of the
 Resolve]                               Protector]
 (Passive)          (Active)             (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Guardian's        [Shield Wall]      [Interposing
  Taunt]                                Shield]
  (Active)          (Active)          (Reaction)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Implacable             [Aegis of the
    Defense]                   Clan]
    (Active)               (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
               [Bastion of Sanity]
               (Passive + Reaction)
```

### 5.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 26019 | Sanctified Resolve | 1 | Passive | 3 | [sanctified-resolve.md](abilities/sanctified-resolve.md) |
| 26020 | Shield Bash | 1 | Active | 3 | [shield-bash.md](abilities/shield-bash.md) |
| 26021 | Oath of the Protector | 1 | Active | 3 | [oath-of-the-protector.md](abilities/oath-of-the-protector.md) |
| 26022 | Guardian's Taunt | 2 | Active | 4 | [guardians-taunt.md](abilities/guardians-taunt.md) |
| 26023 | Shield Wall | 2 | Active | 4 | [shield-wall.md](abilities/shield-wall.md) |
| 26024 | Interposing Shield | 2 | Reaction | 4 | [interposing-shield.md](abilities/interposing-shield.md) |
| 26025 | Implacable Defense | 3 | Active | 5 | [implacable-defense.md](abilities/implacable-defense.md) |
| 26026 | Aegis of the Clan | 3 | Passive | 5 | [aegis-of-the-clan.md](abilities/aegis-of-the-clan.md) |
| 26027 | Bastion of Sanity | 4 | Passive+Reaction | 6 | [bastion-of-sanity.md](abilities/bastion-of-sanity.md) |

---

## 6. Situational Power Profile

### 6.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| Psychic-heavy encounters | Dual physical/mental protection |
| Party with fragile members | Multiple protection abilities |
| Long attrition fights | Sustained mitigation scales |
| Front-line positioning | Taunt and Wall abilities excel |

### 6.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Solo scenarios | Protection abilities need targets |
| Ranged-heavy enemies | Cannot intercept ranged attacks |
| Mobility-focused fights | Rooted defensive style |
| Damage-race encounters | Low personal damage output |

---

## 7. Party Synergies

### 7.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Bone-Setter** | Heals while Skjaldmær mitigates |
| **Berserkr** | Tank absorbs what Berserkr cannot |
| **Jötun-Reader** | Reader safe behind shield wall |
| **Any DPS** | Holds line for damage dealers |

### 7.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| Other tank builds | Redundant protection |
| Full-ranged parties | Less value from melee protection |

---

## 8. Integration Points

| System | Integration |
|--------|-------------|
| **Trauma Economy** | Psychic Stress mitigation |
| **Status Effects** | [Staggered], [Taunted], [Fortified], [Oath of the Protector] |
| **Reaction System** | Interposing Shield, Bastion of Sanity |
| **Combat Positioning** | Front Row tank mechanics |

---

## 9. Balance Data

### 9.1 Power Curve

| Legend | Damage Output | Survivability | Utility |
|--------|---------------|---------------|---------|
| 1-3 | Low | Very High | High |
| 4-6 | Low | Extreme | Very High |
| 7-10 | Medium | Extreme | Very High |

### 9.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★★☆☆☆ | Shield Bash only |
| AoE DPS | ★☆☆☆☆ | Minimal |
| Tanking | ★★★★★ | Best-in-class |
| Healing | ★★☆☆☆ | Stress mitigation only |
| Utility | ★★★★☆ | Taunt, positioning control |

### 9.3 Protection Analysis

| Metric | Value | Notes |
|--------|-------|-------|
| Avg Soak provided | +2 to +4 | Via Oath/Wall |
| Stress mitigation | 10-20% reduction | Via various passives |
| Taunt duration | 2 rounds | Redirects all attacks |
| Reaction triggers | 1/round | Interposing Shield |

---

## 10. Voice Guidance

**Reference:** [/docs/.templates/flavor-text/specialization-abilities.md]

### 10.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Steadfast, protective, sacrificial |
| **Key Words** | Shield, protect, stand, hold, oath |
| **Sentence Style** | Declarative, resolute, short |

### 10.2 Example Voice

> **Activation:** "I am the wall. They shall not pass."
> **Effect:** "My shield holds. My oath is unbroken."
> **Failure:** "I... cannot protect everyone."

---

## 11. Phased Implementation Guide

### Phase 1: Foundation
- [ ] **Define Status Effects**: Implement [Staggered], [Taunted], [Fortified].
- [ ] **Soak System**: Ensure damage reduction stacks correctly.
- [ ] **Position Tracking**: Implement Front Row detection.

### Phase 2: Core Abilities
- [ ] **Shield Bash**: Implement damage + stagger.
- [ ] **Oath of the Protector**: Implement targeted buff with duration.
- [ ] **Guardian's Taunt**: Implement forced targeting.

### Phase 3: Reaction System
- [ ] **Interposing Shield**: Implement critical hit detection trigger.
- [ ] **Reaction Prompt UI**: Build player decision interface.
- [ ] **Damage Redirect**: Implement damage transfer mechanics.

### Phase 4: Capstone
- [ ] **Bastion of Sanity Aura**: Implement passive WILL buff.
- [ ] **Trauma Absorption**: Implement Trauma redirect with Corruption cost.
- [ ] **Breaking Point Detection**: Hook into Trauma system.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Soak Stacking**: Verify multiple Soak sources stack.
- [ ] **Taunt Duration**: Verify 2-round duration.
- [ ] **Reaction Trigger**: Verify critical hit detection.

### 12.2 Integration Tests
- [ ] **Party Protection**: Verify buffs apply to correct targets.
- [ ] **Trauma Absorption**: Verify Corruption cost applied.
- [ ] **Position Requirements**: Verify Front Row abilities fail from Back Row.

### 12.3 Manual QA
- [ ] **Reaction Prompt**: UI appears at correct timing.
- [ ] **Buff Icons**: Display correctly on protected allies.
- [ ] **Aura Indicator**: Visual feedback for Bastion of Sanity.

---

## 13. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 13.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Skjaldmær for {Character}." | `Character` |
| Taunt Applied | Info | "{Character} taunted {Count} enemies for {Duration} rounds." | `Character`, `Count`, `Duration` |
| Interpose | Info | "{Character} intercepted critical hit meant for {Ally}." | `Character`, `Ally` |
| Trauma Absorbed | Warning | "{Character} absorbed Trauma from {Ally}. Gained {Corruption} Corruption." | `Character`, `Ally`, `Corruption` |

---

## 14. Related Documentation

| Document | Purpose |
|----------|---------|
| [Stunned](../../../04-systems/status-effects/stunned.md) | Shield Wall immunity |
| [Feared](../../../04-systems/status-effects/feared.md) | Implacable Defense immunity |
| [Stress](../../../01-core/resources/stress.md) | Psychic Stress mitigation |

---

## 15. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration from legacy spec |
