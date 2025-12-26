# Seidkona Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-SEIDKONA
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Seidkona specialization, including design philosophy, all 9 abilities with rank details, and implementation status.

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Data Seeding | `RuneAndRust.Persistence/SeidkonaSeeder.cs` | Implemented |
| Tests | N/A | Not Yet Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from SeidkonaSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | Seidkona |
| **Display Name** | Seidkona |
| **Specialization ID** | 28001 |
| **Archetype** | Mystic (ArchetypeID = 5) |
| **Path Type** | Corrupted |
| **Mechanical Role** | Psychic Archaeologist / Trauma Economy High Risk |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource System** | Aether Pool |
| **Trauma Risk** | High |
| **Icon** | :crystal_ball: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 10 PP | Higher cost (powerful but risky) |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "Psychic Archaeologist"

**Core Fantasy**: You are the psychic archaeologist who communes with fragmented echoes of a crashed reality. Not a nature shaman but an interpreter of corrupted data-logs, traumatic memories, and fragmented consciousness left by the Great Silence.

Your magic, Seidr, is not command but interpretation—sifting through the eternal psychic scream for coherent whispers of truth. You are a medium between living and dead code, a translator of corruption, trading sanity for knowledge that could save or doom your party.

**Mechanical Identity**:

1. **Spirit Bargain System**: Abilities have chance to trigger bonus effects
2. **Trauma Economy**: Trade Psychic Stress for powerful effects
3. **Support/Debuff Focus**: Healing, curses, and protective wards
4. **Moment of Clarity**: Ultimate state where all bargains succeed

### 1.4 The Spirit Bargain System

Many Seidkona abilities have [Spirit Bargain] effects—bonus outcomes that trigger on chance:

| Base Chance | With Fickle Fortune |
| --- | --- |
| 25% | 40-50% |
| 30% | 45-55% |
| 35% | 50-60% |

During **Moment of Clarity**: All Spirit Bargains are 100% guaranteed.

### 1.5 Specialization Description (Full Text)

> You are the psychic archaeologist who communes with fragmented echoes of a crashed reality. Not a nature shaman but an interpreter of corrupted data-logs, traumatic memories, and fragmented consciousness left by the Great Silence.
> 
> 
> Your magic, Seidr, is not command but interpretation—sifting through the eternal psychic scream for coherent whispers of truth. You are a medium between living and dead code, a translator of corruption, trading sanity for knowledge that could save or doom your party.
> 

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Key Effect |
| --- | --- | --- | --- | --- |
| 28001 | Spiritual Attunement I | 1 | Passive | Perceive magical/psychic phenomena |
| 28002 | Echo of Vigor | 1 | Active | Healing with Spirit Bargain debuff cleanse |
| 28003 | Echo of Misfortune | 1 | Active | Apply [Cursed] with Spirit Bargain spread |
| 28004 | Forlorn Communion | 2 | Active | Knowledge for Psychic Stress cost |
| 28005 | Spiritual Anchor | 2 | Active | Remove Psychic Stress from self |
| 28006 | Fickle Fortune | 2 | Passive | Increase all Spirit Bargain chances |
| 28007 | Spirit Ward | 3 | Active | Protective ward vs psychic damage |
| 28008 | Ride the Echoes | 3 | Active | Teleport for Corruption cost |
| 28009 | Moment of Clarity | 4 | Active | Guaranteed Spirit Bargains for 2-3 turns |

---

## 3. Tier 1 Abilities

### 3.1 Spiritual Attunement I (ID: 28001)

**Type**: Passive | **PP Cost**: 3

| Rank | Effect |
| --- | --- |
| 1 | +1 die to WILL checks to perceive magical phenomena. See aura around [Psychic Resonance] zones and Forlorn entities |
| 2 | +2 dice |
| 3 | +2 dice + sense Forlorn through walls (30 ft radius) |

---

### 3.2 Echo of Vigor (ID: 28002)

**Type**: Active | **Action**: Standard Action | **Target**: Single Ally | **Cost**: Aether

| Rank | Effect |
| --- | --- |
| 1 | Restore 3d8 HP. [Spirit Bargain] 25%: Cleanse one minor physical debuff ([Bleeding], [Poisoned], [Disease]) |
| 2 | Restore 4d8 HP, bargain 30% |
| 3 | Restore 5d8 HP, bargain 35%, can target self |

---

### 3.3 Echo of Misfortune (ID: 28003)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy | **Cost**: Aether

| Rank | Effect |
| --- | --- |
| 1 | Apply [Cursed] 2 turns (25% miss chance, -1 die to all checks). [Spirit Bargain] On Crit: Spread to adjacent enemy |
| 2 | [Cursed] 3 turns, 30% miss chance |
| 3 | [Cursed] 3 turns, 30% miss + -2 dice penalty |

---

## 4. Tier 2 Abilities

### 4.1 Forlorn Communion (ID: 28004)

**Type**: Active | **Action**: Standard Action (out of combat) | **Target**: Forlorn/Psychic Resonance Zone

| Rank | Effect |
| --- | --- |
| 1 | WITS check (DC 12). Success: Reveal enemy weaknesses, puzzle solutions, lore, hidden paths. **COST: +15 Psychic Stress (unavoidable)** |
| 2 | DC 10, Stress +12 |
| 3 | DC 8, Stress +10, can ask 2 questions per communion |

---

### 4.2 Spiritual Anchor (ID: 28005)

**Type**: Active | **Action**: Standard Action | **Target**: Self | **Cost**: Aether

| Rank | Effect |
| --- | --- |
| 1 | Meditative state (no other actions). Remove 20 Psychic Stress from self |
| 2 | Remove 25 Stress |
| 3 | Remove 30 Stress + cleanse one mental debuff ([Fear], [Disoriented], [Charmed]) |

---

### 4.3 Fickle Fortune (ID: 28006)

**Type**: Passive | **PP Cost**: 4

| Rank | Effect |
| --- | --- |
| 1 | All [Spirit Bargain] trigger chances increased by +15% |
| 2 | +20% increase |
| 3 | +25% increase + once per combat, force a failed bargain to succeed |

---

## 5. Tier 3 Abilities

### 5.1 Spirit Ward (ID: 28007)

**Type**: Active | **Action**: Standard Action | **Target**: Row (All Allies) | **Cost**: Aether

| Rank | Effect |
| --- | --- |
| 1 | Place [Spirit Ward] on row for 3 turns. Allies: Negate environmental Psychic Stress, +1 die vs psychic attacks. [Spirit Bargain] 25%: Lasts 4 turns |
| 2 | +2 dice to resistance, bargain 30% |
| 3 | +2 dice, bargain 35%, can place on both rows (double cost) |

---

### 5.2 Ride the Echoes (ID: 28008)

**Type**: Active | **Action**: Standard Action | **Target**: Self | **Cost**: Aether

| Rank | Effect |
| --- | --- |
| 1 | Instantly teleport to any unoccupied tile. No line-of-sight required. **COST: +2 Runic Blight Corruption** |
| 2 | +1 Corruption (reduced) |
| 3 | +1 Corruption + can bring one adjacent ally (+1 Corruption for them) |

---

## 6. Capstone Ability

### 6.1 Moment of Clarity (ID: 28009)

**Type**: Active | **Action**: Standard Action | **Target**: Self | **Cost**: Aether | **Cooldown**: Once per combat

| Rank | Effect |
| --- | --- |
| 1 | Enter [Clarity] for 2 turns: ALL Spirit Bargains 100%. Echo of Vigor always cleanses. Echo of Misfortune always spreads. Spirit Ward always 4 turns. Forlorn Communion: 0 Aether, +7 Stress, auto-success. Can cast Spiritual Anchor on ally. **When ends: +20 Psychic Stress** |
| 2 | [Clarity] 3 turns, aftermath +15 Stress |
| 3 | 3 turns, aftermath +10 Stress, can use Moment of Clarity twice per combat |

---

## 7. Status Effect Definitions

### 7.1 [Cursed]

- 25-30% miss chance on attacks
- 1 to -2 dice penalty to all checks
- Duration: 2-3 turns

### 7.2 [Spirit Ward]

- Negate environmental Psychic Stress
- +1-2 dice to Resolve vs psychic attacks
- Protected from [Psychic Resonance] effects
- Duration: 3-4 turns

### 7.3 [Clarity]

- All Spirit Bargain effects guaranteed (100%)
- Enhanced ability effects
- Duration: 2-3 turns
- Aftermath: Psychic Stress penalty

### 7.4 [Psychic Resonance]

- Environmental zones with corrupted psychic energy
- Increases Seidkona Spirit Bargain chances (+15%)
- May cause ambient Psychic Stress gain

---

## 8. Trauma Economy Integration

The Seidkona is deeply integrated with the Trauma Economy:

| Ability | Psychic Stress Cost/Gain |
| --- | --- |
| Forlorn Communion | +10-15 (unavoidable) |
| Spiritual Anchor | -20-30 (removal) |
| Moment of Clarity (aftermath) | +10-20 |
| Ride the Echoes | +2 Corruption |

**Risk/Reward**: The Seidkona trades mental stability for powerful effects. Managing Psychic Stress is essential.

---

## 9. Implementation Priority

### Phase 1: Spirit Bargain System

1. Implement Spirit Bargain chance calculation
2. Implement bonus effect triggers
3. Implement Fickle Fortune modifiers

### Phase 2: Core Abilities

1. Implement Echo of Vigor healing with debuff cleanse
2. Implement Echo of Misfortune [Cursed] status
3. Implement Spirit Ward row protection

### Phase 3: Trauma Integration

1. Implement Psychic Stress costs for Forlorn Communion
2. Implement Spiritual Anchor stress removal
3. Implement Corruption cost for Ride the Echoes

### Phase 4: Moment of Clarity

1. Implement [Clarity] state with 100% bargains
2. Implement enhanced ability effects during Clarity
3. Implement aftermath Psychic Stress

### Phase 5: Polish

1. Add Psychic Resonance zone detection
2. Implement teleportation (Ride the Echoes)
3. Add visual indicators for Spirit Bargain triggers

---

**End of Specification**