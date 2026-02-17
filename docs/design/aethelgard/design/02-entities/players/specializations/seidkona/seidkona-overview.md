---
id: SPEC-SEIDKONA-27001
title: "Seiðkona"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Seiðkona

## Identity

| Property | Value |
|----------|-------|
| **Name** | Seiðkona |
| **Archetype** | Mystic |
| **Path Type** | Corrupted |
| **Role** | Psychic Archaeologist / Trauma Economy High Risk |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource** | Aether Pool |
| **Trauma Risk** | High |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 10 PP |
| **Minimum Legend** | 5 |
| **Maximum Corruption** | 100 |
| **Required Quest** | None |

---

## Design Philosophy

**Tagline:** "Psychic Archaeologist"

**Core Fantasy:** You are the psychic archaeologist who communes with fragmented echoes of a crashed reality. Not a nature shaman but an interpreter of corrupted data-logs, traumatic memories, and fragmented consciousness left by the Great Silence.

Your magic, Seiðr, is not command but interpretation—sifting through the eternal psychic scream for coherent whispers of truth. You are a medium between living and dead code, a translator of corruption, trading sanity for knowledge that could save or doom your party.

**Mechanical Identity:**
1. **Spirit Bargain System** - Abilities have chance to trigger bonus effects
2. **Trauma Economy** - Trade Psychic Stress for powerful effects
3. **Support/Debuff Focus** - Healing, curses, and protective wards
4. **Moment of Clarity** - Ultimate state where all bargains succeed

**Gameplay Feel:** High-risk support mystic who gambles with probability and sanity. Masters of chance-based bonus effects that become guaranteed during their ultimate state.

---

## Core Mechanics

### Spirit Bargain System

Many Seiðkona abilities have [Spirit Bargain] effects—bonus outcomes that trigger on chance:

| Base Chance | With Fickle Fortune |
|-------------|-------------------|
| 25% | 40-50% |
| 30% | 45-55% |
| 35% | 50-60% |

During **Moment of Clarity:** All Spirit Bargains are **100% guaranteed**.

### Trauma Economy Integration

| Ability | Psychic Stress Effect |
|---------|----------------------|
| Forlorn Communion | +10-15 (unavoidable) |
| Spiritual Anchor | -20-30 (removal) |
| Moment of Clarity (aftermath) | +10-20 |
| Ride the Echoes | +2 Corruption |

**Risk/Reward:** The Seiðkona trades mental stability for powerful effects. Managing Psychic Stress is essential.

---

## Rank Progression

### Tier Unlock Requirements

| Tier | PP Cost | Starting Rank | Max Rank | Progression |
|------|---------|---------------|----------|-------------|
| Tier 1 | 3 PP each | Rank 1 | Rank 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| Tier 2 | 4 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Tier 3 | 5 PP each | — | — | No ranks |
| Capstone | 6 PP | — | — | No ranks (triggers Rank 3) |

### Total Investment: 40 PP (10 unlock + 30 abilities)

---

## Ability Tree

```
                    TIER 1: FOUNDATION
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Spiritual          [Echo of Vigor]      [Echo of Misfortune]
 Attunement I]         (Active)              (Active)
  (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Forlorn Communion] [Spiritual Anchor]   [Fickle Fortune]
     (Active)            (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
    [Spirit Ward]              [Ride the Echoes]
       (Active)                    (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
               [Moment of Clarity]
                    (Active)
```

### Ability Index

| ID | Name | Tier | Type | PP | Key Effect |
|----|------|------|------|-----|------------|
| 27001 | Spiritual Attunement I | 1 | Passive | 3 | Perceive magical/psychic phenomena |
| 27002 | Echo of Vigor | 1 | Active | 3 | Healing + Spirit Bargain debuff cleanse |
| 27003 | Echo of Misfortune | 1 | Active | 3 | Apply [Cursed] + Spirit Bargain spread |
| 27004 | Forlorn Communion | 2 | Active | 4 | Knowledge for Psychic Stress cost |
| 27005 | Spiritual Anchor | 2 | Active | 4 | Remove Psychic Stress from self |
| 27006 | Fickle Fortune | 2 | Passive | 4 | Increase Spirit Bargain chances |
| 27007 | Spirit Ward | 3 | Active | 5 | Protective ward vs psychic damage |
| 27008 | Ride the Echoes | 3 | Active | 5 | Teleport for Corruption cost |
| 27009 | Moment of Clarity | 4 | Active | 6 | Guaranteed Spirit Bargains |

---

## Situational Power Profile

### Optimal Conditions
- [Psychic Resonance] zones (+15% bargain chance)
- Extended combats allowing bargain opportunities
- Parties with Stress mitigation support
- Intelligence-gathering missions
- Fighting psychic or Forlorn enemies

### Weakness Conditions
- Quick encounters (bargains can't proc)
- High ambient Stress environments
- Anti-magic zones
- Parties without Stress recovery
- Solo operation (support-focused kit)

---

## Party Synergies

### Positive Synergies
- **Skald** - Stress management synergy
- **Bone-Setter** - Can heal Stress damage
- **Tanks** - Protect fragile Seiðkona
- **Burst damage dealers** - [Cursed] setup for alpha strikes

### Negative Synergies
- **Other high-Stress builds** - Competing for Stress threshold
- **Speed compositions** - Bargains need time
- **Corruption-weak parties** - Ride the Echoes adds Corruption

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 5-7 | Medium | Learning bargain management |
| 8-12 | High | Fickle Fortune online |
| 13-17 | Very High | Moment of Clarity mastery |
| 18+ | Extreme | Full Spirit control |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★★☆☆ | [Cursed] focus |
| AoE Damage | ★★☆☆☆ | Limited spread |
| Control | ★★★★☆ | [Cursed], wards |
| Support | ★★★★★ | Healing, Stress management |
| Survivability | ★★☆☆☆ | Fragile, Stress vulnerable |

---

## Voice Guidance

### Tone Profile
- Distant, dreamlike observations
- Speaks of spirits as colleagues
- Acceptance of mental burden
- Cryptic insights from beyond

### Example Quotes
- "The echoes remember what the living have forgotten."
- "I trade pieces of my mind for glimpses of truth."
- "The spirits are... restless today. They have much to share."
- "Clarity comes at a price. I have paid it gladly."

---

## Phased Implementation Guide

### Phase 1: Spirit Bargain System
- [ ] Implement Spirit Bargain chance calculation
- [ ] Implement bonus effect triggers
- [ ] Implement Fickle Fortune modifiers
- [ ] Spirit Bargain UI indicators

### Phase 2: Core Healing/Debuff
- [ ] Implement Echo of Vigor healing + cleanse
- [ ] Implement Echo of Misfortune [Cursed] status
- [ ] Implement Spirit Ward row protection
- [ ] Status effect spreading mechanics

### Phase 3: Trauma Integration
- [ ] Implement Psychic Stress costs for Forlorn Communion
- [ ] Implement Spiritual Anchor stress removal
- [ ] Implement Corruption cost for Ride the Echoes
- [ ] Stress threshold warnings

### Phase 4: Moment of Clarity
- [ ] Implement [Clarity] state with 100% bargains
- [ ] Implement enhanced ability effects during Clarity
- [ ] Implement aftermath Psychic Stress
- [ ] Clarity visual effects

### Phase 5: Polish
- [ ] Psychic Resonance zone detection
- [ ] Teleportation (Ride the Echoes)
- [ ] Spirit Bargain trigger animations
- [ ] Clarity state UI overlay

---

## Testing Requirements

### Unit Tests
- Spirit Bargain probability calculations
- Stress cost/removal math
- [Cursed] effect application
- Fickle Fortune modifier stacking

### Integration Tests
- Bargain triggers in combat flow
- Stress accumulation over encounters
- Moment of Clarity state transitions
- Ward protection vs psychic damage

### Manual QA Checklist
- [ ] Spirit Bargain triggers display correctly
- [ ] Stress costs apply and show warnings
- [ ] Moment of Clarity enhances all abilities
- [ ] Teleportation respects line-of-sight rules
- [ ] [Cursed] spreads on critical hits

---

## Logging Requirements

### Combat Events
```
"Echo of Vigor: Healed {target} for {amount} HP"
"Spirit Bargain TRIGGERED: Cleansed {debuff} from {target}!"
"Echo of Misfortune: {target} is [Cursed] for {duration} turns"
"Spirit Bargain: [Cursed] spreads to {adjacent_target}!"
"Forlorn Communion: Knowledge gained. +{stress} Psychic Stress"
"Spiritual Anchor: Removed {amount} Psychic Stress"
"Moment of Clarity ACTIVATED: All Spirit Bargains guaranteed!"
"Moment of Clarity ended. +{stress} Psychic Stress (aftermath)"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Mystic Archetype](../../archetypes/mystic.md) | Parent archetype |
| [Stress System](../../../01-core/resources/stress.md) | Psychic Stress mechanics |
| [Trauma Economy](../../../01-core/trauma-economy.md) | Risk/reward framework |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
