---
id: SPEC-SKALD-28001
title: "Skald"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Skald

## Identity

| Property | Value |
|----------|-------|
| **Name** | Skald |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Role** | Performance Buffer / Trauma Economy Support |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource** | Stamina |
| **Trauma Risk** | Low |

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

**Tagline:** "Chronicler of Coherence"

**Core Fantasy:** The keeper of coherent narratives in a world whose story has shattered. You are a warrior-poet who wields structured verse as both weapon and shield, creating 'narrative firewalls'—pockets of logic and meaning that fortify allies' minds against psychic static or break enemies' morale with the weight of foreseen doom.

You are not a mystic but a coherence-keeper, proving that in a glitching reality, a well-told story is tangible power.

**Mechanical Identity:**
1. **Performance System** - Channeled abilities providing ongoing effects
2. **Narrative Firewall** - Creates coherence pockets protecting allies
3. **Morale Manipulation** - Breaks enemy morale through narrative weight
4. **Trauma Economy Support** - Manages party Psychic Stress

**Gameplay Feel:** Battlefield conductor who maintains powerful auras through continuous performance. High uptime on buffs/debuffs but restricted to single actions while performing.

---

## Core Mechanics

### Performance System

**[Performance] Mechanics:**
- Channeled abilities requiring ongoing concentration
- While performing, cannot take other Standard Actions
- Duration based on WILL score in rounds
- Can be interrupted by [Stunned], [Feared], or [Silenced]
- Some abilities are NOT performances (instant effects)

**Performance Duration:**
```
Duration = Skald.WILL (in rounds)
With Enduring Performance: Duration += 3-4 rounds
```

### Interruption Conditions

| Status | Effect |
|--------|--------|
| [Stunned] | Performance ends immediately |
| [Feared] | Performance ends immediately |
| [Silenced] | Performance ends immediately |
| Damage | No interruption (maintain through pain) |

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
[Oral Tradition]   [Saga of Courage]    [Dirge of Defeat]
   (Passive)        (Performance)        (Performance)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Rousing Verse]    [Song of Silence]    [Enduring Performance]
   (Active)            (Active)            (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
[Lay of the Iron Wall]         [Heart of the Clan]
    (Performance)                  (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
            [Saga of the Einherjar]
               (Performance)
```

### Ability Index

| ID | Name | Tier | Type | PP | Performance? | Key Effect |
|----|------|------|------|-----|--------------|------------|
| 28001 | Oral Tradition | 1 | Passive | 3 | No | +dice to Rhetoric/lore |
| 28002 | Saga of Courage | 1 | Active | 3 | Yes | Fear immunity + Stress resist |
| 28003 | Dirge of Defeat | 1 | Active | 3 | Yes | Enemy accuracy/damage penalty |
| 28004 | Rousing Verse | 2 | Active | 4 | No | Restore ally Stamina |
| 28005 | Song of Silence | 2 | Active | 4 | No | Silence enemy caster |
| 28006 | Enduring Performance | 2 | Passive | 4 | No | +Performance duration |
| 28007 | Lay of the Iron Wall | 3 | Active | 5 | Yes | Front Row +Soak |
| 28008 | Heart of the Clan | 3 | Passive | 5 | No | Aura: +dice to Resolve |
| 28009 | Saga of the Einherjar | 4 | Active | 6 | Yes | Massive buffs + Stress cost |

---

## Situational Power Profile

### Optimal Conditions
- Extended combats (Performance value)
- Parties with frontline fighters (Iron Wall, Einherjar)
- Intelligent enemy presence (Dirge of Defeat)
- High-Stress environments (Courage, Heart of Clan)
- Support-oriented compositions

### Weakness Conditions
- Short skirmishes (Performance startup cost)
- Mindless/Undying enemies (immune to Dirge)
- [Silenced] enemy abilities
- Solo combat (auras need allies)
- Ambush situations (no setup time)

---

## Party Synergies

### Positive Synergies
- **Warriors** - Iron Wall Soak, Einherjar damage boost
- **Seiðkona** - Combined Stress management
- **Tanks** - Heart of the Clan Resolve aura
- **Burst damage** - Rousing Verse Stamina recovery

### Negative Synergies
- **Other support builds** - Competing for party slots
- **Stealth compositions** - Performances are not subtle
- **High-mobility builds** - Skald locked in position while performing

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 5-7 | Medium | Learning Performance timing |
| 8-12 | High | Enduring Performance online |
| 13-17 | Very High | Dual performance potential |
| 18+ | Extreme | Einherjar mastery |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★☆☆☆ | Not specialized |
| AoE Damage | ★★☆☆☆ | Dirge psychic damage |
| Control | ★★★★☆ | Silence, Fear immunity |
| Support | ★★★★★ | Primary role |
| Survivability | ★★★☆☆ | Vulnerable while performing |

---

## Voice Guidance

### Tone Profile
- Dramatic, rhythmic speech patterns
- References to ancient sagas
- Confident in the power of narrative
- Inspiring presence

### Example Quotes
- "Let me tell you a story of those who stood against impossible odds."
- "The dirge I sing is your doom written in ancient verse."
- "Coherence is my weapon. Stories are my shield."
- "Rise! Be as the Einherjar of legend!"

---

## Phased Implementation Guide

### Phase 1: Performance System
- [ ] Implement Performance mechanic (channeled abilities)
- [ ] Implement WILL-based duration calculation
- [ ] Implement action restriction while performing
- [ ] Interruption handling (Stunned/Feared/Silenced)

### Phase 2: Ally Buffs
- [ ] Implement aura system (affects allies in range)
- [ ] Implement [Inspired] status with damage bonus
- [ ] Implement Stamina restoration (Rousing Verse)
- [ ] Implement Fear/Stun immunity during performances

### Phase 3: Enemy Debuffs
- [ ] Implement intelligent enemy detection
- [ ] Implement [Silenced] status (prevent casting)
- [ ] Implement accuracy/damage penalties
- [ ] Psychic damage at Rank 3 Dirge

### Phase 4: Capstone
- [ ] Implement Saga of the Einherjar
- [ ] Implement temporary HP system
- [ ] Implement end-cost Stress mechanic
- [ ] Dual performance at Rank 3 Enduring

### Phase 5: Polish
- [ ] Performance duration bar UI
- [ ] Aura visual effects
- [ ] Dual performance display
- [ ] Interruption notifications

---

## Testing Requirements

### Unit Tests
- Performance duration calculations
- WILL-based scaling
- Aura range detection
- Interruption triggers

### Integration Tests
- Performance + combat action flow
- Multi-ally aura application
- Saga of Einherjar end-cost
- Dual performance stacking

### Manual QA Checklist
- [ ] Performance duration shows correctly
- [ ] Cannot take other actions while performing
- [ ] Interruption ends performance properly
- [ ] Einherjar Stress applies on end
- [ ] Dual performance works at Rank 3

---

## Logging Requirements

### Combat Events
```
"Saga of Courage begins! All allies immune to Fear."
"Performance duration: {WILL} rounds (+{bonus} from Enduring Performance)"
"Dirge of Defeat: {count} intelligent enemies suffer -{penalty}d10 accuracy/damage"
"Rousing Verse: {target} restored {amount} Stamina"
"Song of Silence: {target} is [Silenced] for 3 rounds!"
"Saga of Courage interrupted by [Stunned]!"
"SAGA OF THE EINHERJAR! All allies become legendary warriors!"
"Saga ends... {count} allies suffer 6 Psychic Stress"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Adept Archetype](../../archetypes/adept.md) | Parent archetype |
| [Stress System](../../../01-core/resources/stress.md) | Psychic Stress mechanics |
| [Inspired Status](../../../04-systems/status-effects/inspired.md) | Status effect details |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
