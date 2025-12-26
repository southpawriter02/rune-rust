# Skald Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-SKALD
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Skald specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Service Implementation | `RuneAndRust.Engine/SkaldService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/SkaldSeeder.cs` | Implemented |
| Specialization Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | Partial |
| Tests | N/A | Not Yet Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from SkaldSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | Skald |
| **Display Name** | Skald |
| **Specialization ID** | 27001 |
| **Archetype** | Adept (ArchetypeID = 2) |
| **Path Type** | Coherent |
| **Mechanical Role** | Performance Buffer / Trauma Economy Support |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina |
| **Trauma Risk** | Low |
| **Icon** | :scroll: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |

### 1.3 Design Philosophy

**Tagline**: "Chronicler of Coherence"

**Core Fantasy**: The keeper of coherent narratives in a world whose story has shattered. You are a warrior-poet who wields structured verse as both weapon and shield, creating 'narrative firewalls'—pockets of logic and meaning that fortify allies' minds against psychic static or break enemies' morale with the weight of foreseen doom.

**Mechanical Identity**:

1. **Performance System**: Most abilities are [Performance] type - channeled abilities that provide ongoing effects while maintained
2. **Narrative Firewall**: Creates pockets of coherence that protect allies from mental effects
3. **Morale Manipulation**: Breaks enemy morale through the narrative weight of doom
4. **Trauma Economy Support**: Manages party Psychic Stress through structured verse

### 1.4 The Performance System

**Performance Mechanics**:

- [Performance] abilities are channeled - they require ongoing concentration
- While performing, you cannot take other Standard Actions
- Duration is based on WILL score in rounds
- Can be interrupted by [Stunned], [Feared], or [Silenced]
- Some abilities are NOT performances (e.g., Rousing Verse) and work as instant effects

### 1.5 Specialization Description (Full Text)

> The keeper of coherent narratives in a world whose story has shattered. You are a warrior-poet who wields structured verse as both weapon and shield, creating 'narrative firewalls'—pockets of logic and meaning that fortify allies' minds against psychic static or break enemies' morale with the weight of foreseen doom.
> 
> 
> You are not a mystic but a coherence-keeper, proving that in a glitching reality, a well-told story is tangible power. Your performances channel battlefield-wide effects, steadying allies' resolve and unnerving intelligent foes through the sheer narrative weight of sagas and dirges.
> 
> The ultimate expression is Saga of the Einherjar—a masterpiece performance that elevates allies to legendary status, granting massive temporary power at the cost of psychic exhaustion when the saga ends.
> 

---

## 2. Rank Progression System

### 2.1 Ability Structure by Tier

| Tier | Abilities | PP Cost | Starting Rank | Max Rank |
| --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 3 PP each | 1 | 3 |
| **Tier 2** | 3 | 4 PP each | 2 | 3 |
| **Tier 3** | 2 | 5 PP each | N/A | N/A |
| **Capstone** | 1 | 6 PP | N/A | N/A |

---

## 3. Ability Tree Overview

### 3.1 Ability Summary Table

| ID | Ability Name | Tier | Type | Performance? | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 27001 | Oral Tradition | 1 | Passive | No | +dice to Rhetoric/lore |
| 27002 | Saga of Courage | 1 | Active | Yes | Fear immunity + Stress resistance |
| 27003 | Dirge of Defeat | 1 | Active | Yes | Enemy accuracy/damage penalty |
| 27004 | Rousing Verse | 2 | Active | No | Restore ally Stamina |
| 27005 | Song of Silence | 2 | Active | No | Silence enemy caster |
| 27006 | Enduring Performance | 2 | Passive | No | +Performance duration |
| 27007 | Lay of the Iron Wall | 3 | Active | Yes | Front Row +Soak |
| 27008 | Heart of the Clan | 3 | Passive | No | Aura: +dice to Resolve |
| 27009 | Saga of the Einherjar | 4 | Active | Yes | Massive buffs + Stress cost |

---

## 4. Tier 1 Abilities

### 4.1 Oral Tradition (ID: 27001)

**Type**: Passive | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost** | 3 PP |
| **Ranks** | 3 |
| **Performance** | No |

### Description

The great sagas are part of the Skald's very being, carried in verse and cadence.

### Rank Details

**Rank 1**: +1 die to Rhetoric checks and investigate checks for historical lore

**Rank 2**: +2 dice to above checks

**Rank 3**: +2 dice + can recall any historical fact with DC 15 WITS check

### Implementation Status

- [x]  Data seeded in `SkaldSeeder.SeedSkaldTier1()`
- [ ]  GUI: Passive indicator
- [ ]  Combat: Skill check integration

---

### 4.2 Saga of Courage (ID: 27002)

**Type**: Active [Performance] | **Target**: All Allies (Aura)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |

### Description

Rousing chant of a hero who stood against overwhelming odds. Creates pocket of coherence steadying allies' resolve.

### Rank Details

**Rank 1**: While performing, all allies IMMUNE to [Feared] + +1 die to WILL Resolve vs Psychic Stress

**Rank 2**: +2 dice to Stress resistance

**Rank 3**: +2 dice + allies also gain +1 die to resist [Disoriented]

**Formulas**:

```
Duration = Skald.WILL (rounds)
While Performing:
    For each Ally:
        Ally.AddImmunity("Feared")
        Ally.ResolveBonus_Stress += Rank  // 1/2/2
        If (Rank >= 3):
            Ally.ResolveBonus_Disoriented += 1

```

### GUI Display

- Performance indicator showing duration remaining
- Aura effect on all allies
- Cannot use other Standard Actions warning

---

### 4.3 Dirge of Defeat (ID: 27003)

**Type**: Active [Performance] | **Target**: All Intelligent Enemies (Aura)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |

### Description

Sorrowful dirge recounting doom of a great army. Narrative weight unnerves intelligent foes.

### Rank Details

**Rank 1**: All intelligent enemies suffer -1 die penalty to Accuracy and damage. Does not affect mindless/Undying.

**Rank 2**: -2 dice penalty

**Rank 3**: -2 dice + intelligent enemies take 1d4 Psychic damage per turn from narrative weight

**Note**: Does NOT affect mindless enemies or Undying constructs

**Formulas**:

```
Duration = Skald.WILL (rounds)
While Performing:
    For each IntelligentEnemy:
        Enemy.AccuracyPenalty += Rank  // 1/2/2
        Enemy.DamagePenalty += Rank
        If (Rank >= 3):
            Enemy.TakeDamage(Roll(1d4), "Psychic")  // per turn

```

---

## 5. Tier 2 Abilities

### 5.1 Rousing Verse (ID: 27004)

**Type**: Active (NOT Performance) | **Target**: Single Ally

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 PP |
| **Ranks** | 2→3 |
| **Resource Cost** | 35 Stamina |
| **Performance** | **No** (instant effect) |

### Description

Quick verse from saga about tireless warrior, banishing fatigue through structured recollection.

### Rank Details

**Rank 2**: Restore 15 + (WILL × 2) Stamina to single ally

**Rank 3**: Restore 25 + (WILL × 3) Stamina + remove [Exhausted] status

**Formulas**:

```
Rank 2: StaminaRestored = 15 + (WILL * 2)
Rank 3: StaminaRestored = 25 + (WILL * 3)
        Target.RemoveStatus("Exhausted")

```

---

### 5.2 Song of Silence (ID: 27005)

**Type**: Active (NOT Performance) | **Target**: Single Intelligent Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 PP |
| **Ranks** | 2→3 |
| **Resource Cost** | 45 Stamina |
| **Opposed Check** | WILL + Rhetoric vs enemy WILL |

### Description

Counter-resonant chant designed to disrupt hostile vocalizations and choke caster's words.

### Rank Details

**Rank 2**: Apply [Silenced] for 3 rounds

**Rank 3**: [Silenced] 3 rounds + target takes 2d6 Psychic damage from vocal disruption

**[Silenced] Effect**:

- Cannot cast spells
- Cannot use abilities requiring speech
- Cannot use [Performance] abilities

---

### 5.3 Enduring Performance (ID: 27006)

**Type**: Passive | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 PP |
| **Ranks** | 2→3 |

### Description

Honed vocal endurance allows maintaining powerful performances longer.

### Rank Details

**Rank 2**: All Performance durations increased by +3 rounds

**Rank 3**: +4 rounds + can maintain 2 Performances simultaneously (costs both Stamina costs, requires 2 Standard Actions to initiate)

**Multiple Performance Note**: At Rank 3, Skald can maintain two different performances at once, but:

- Must pay both Stamina costs
- Must spend 2 Standard Actions to initiate both
- Both can be interrupted independently

---

## 6. Tier 3 Abilities (No Ranks)

### 6.1 Lay of the Iron Wall (ID: 27007)

**Type**: Active [Performance] | **Target**: All Front Row Allies

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 PP |
| **Ranks** | None |
| **Resource Cost** | 55 Stamina |
| **Performance** | Yes |

### Description

Story of unbreakable shield wall at Battle of Black Pass. Narrative imposes structural coherence on formation.

### Mechanical Effect

- All Front Row allies gain +4 Soak (damage reduction)
- Resistance to Push/Pull effects
- Duration: WILL rounds

**Formulas**:

```
Duration = Skald.WILL (rounds)
While Performing:
    For each FrontRowAlly:
        Ally.Soak += 4
        Ally.AddResistance("Push")
        Ally.AddResistance("Pull")

```

---

### 6.2 Heart of the Clan (ID: 27008)

**Type**: Passive | **Target**: Allies in Same Row

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 PP |
| **Ranks** | None |

### Description

The Skald is the living heart of their clan. Their presence is a source of unshakeable resolve.

### Mechanical Effect

- All allies in same row as Skald gain +2 dice to defensive Resolve Checks (Fear, Disoriented)
- Inactive if Skald is [Stunned], [Feared], or [Silenced]
- Aura extends to adjacent row

**Formulas**:

```
If (Skald.Status NOT IN ["Stunned", "Feared", "Silenced"]):
    For each Ally in SameRow OR AdjacentRow:
        Ally.ResolveBonus_Fear += 2
        Ally.ResolveBonus_Disoriented += 2

```

---

## 7. Capstone Ability

### 7.1 Saga of the Einherjar (ID: 27009)

**Type**: Active [Performance] | **Target**: All Allies

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 4 (Capstone) |
| **PP Cost** | 6 PP |
| **Ranks** | None |
| **Resource Cost** | 75 Stamina |
| **Performance** | Yes |
| **Cooldown** | Once Per Combat |
| **Special** | Training this ability upgrades all Tier 1 & Tier 2 abilities to Rank 3 |

### Description

Masterpiece saga of greatest heroes. Allies believe themselves elevated to legendary state.

### Mechanical Effect

- All allies gain [Inspired] (+5 dice to damage)
- All allies gain 40 temporary HP
- All allies immune to [Feared] and [Stunned] during performance
- **Cost on End**: When performance ends, all affected allies take 6 Psychic Stress
- Duration: WILL rounds

**Formulas**:

```
Duration = Skald.WILL (rounds)
While Performing:
    For each Ally:
        Ally.AddStatus("Inspired", BonusDice: 5)
        Ally.TempHP += 40
        Ally.AddImmunity("Feared")
        Ally.AddImmunity("Stunned")
OnPerformanceEnd:
    For each AffectedAlly:
        Ally.PsychicStress += 6

```

### GUI Display - CAPSTONE NOTIFICATION

```
┌─────────────────────────────────────────────┐
│       SAGA OF THE EINHERJAR BEGINS!         │
├─────────────────────────────────────────────┤
│                                             │
│  "Rise, warriors! Be as the Einherjar!"     │
│                                             │
│  All allies gain:                           │
│  • [Inspired] +5 damage dice                │
│  • 40 Temporary HP                          │
│  • Immune to Fear and Stun                  │
│                                             │
│  WARNING: 6 Psychic Stress when saga ends   │
│                                             │
└─────────────────────────────────────────────┘

```

### Combat Log Examples

- "SAGA OF THE EINHERJAR! All allies become legendary warriors!"
- "[Ally] gains Inspired (+5 damage dice) and 40 temp HP"
- "Saga ends... [Ally] suffers 6 Psychic Stress"

---

## 8. Status Effect Definitions

### 8.1 [Inspired]

| Property | Value |
| --- | --- |
| **Applied By** | Saga of the Einherjar |
| **Duration** | Performance duration |
| **Icon** | Glowing warrior |
| **Color** | Gold |

**Effects**:

- +5 dice to all damage rolls
- Visual: Golden glow on character

### 8.2 [Silenced]

| Property | Value |
| --- | --- |
| **Applied By** | Song of Silence |
| **Duration** | 3 rounds |
| **Icon** | Crossed-out mouth |
| **Color** | Purple |

**Effects**:

- Cannot cast spells
- Cannot use speech-based abilities
- Cannot use [Performance] abilities

---

## 9. GUI Requirements

### 9.1 Performance Indicator

```
┌─────────────────────────────────────────┐
│  [Performance] SAGA OF COURAGE          │
│  Duration: ████████░░ 6/8 rounds        │
│  Effect: Fear Immune + Stress Resist    │
│  ⚠ Cannot use Standard Actions          │
└─────────────────────────────────────────┘

```

- Shows active performance name
- Duration bar
- Effect summary
- Warning about action restriction

### 9.2 Dual Performance Display (Rank 3 Enduring Performance)

```
┌─────────────────────────────────────────┐
│  [Performance 1] SAGA OF COURAGE        │
│  Duration: ████████░░ 6/8 rounds        │
├─────────────────────────────────────────┤
│  [Performance 2] LAY OF THE IRON WALL   │
│  Duration: ██████░░░░ 4/7 rounds        │
│  ⚠ Maintaining dual performances        │
└─────────────────────────────────────────┘

```

---

## 10. Implementation Priority

### Phase 1: Performance System

1. **Implement Performance mechanic** - channeled abilities with duration
2. **Implement WILL-based duration** calculation
3. **Implement action restriction** while performing

### Phase 2: Ally Buffs

1. **Implement aura system** - affects allies in range
2. **Implement [Inspired] status** with damage bonus
3. **Implement Stamina restoration** (Rousing Verse)

### Phase 3: Enemy Debuffs

1. **Implement intelligent enemy detection** - exclude mindless/Undying
2. **Implement [Silenced] status** - prevent casting
3. **Implement accuracy/damage penalties**

### Phase 4: Capstone

1. **Implement Saga of the Einherjar** with end-cost
2. **Implement dual performance** (Rank 3 Enduring Performance)
3. **Add performance visual effects**

---

**End of Specification**