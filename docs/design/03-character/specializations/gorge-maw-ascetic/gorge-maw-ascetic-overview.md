---
id: SPEC-GORGE-MAW-ASCETIC-26002
title: "Gorge-Maw Ascetic"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Gorge-Maw Ascetic

**Archetype:** Warrior | **Path:** Coherent | **Role:** Control Fighter / Seismic Monk

> *"The Seismic Monk"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Gorge-Maw Ascetic |
| **Archetype** | Warrior |
| **Path Type** | Coherent |
| **Role** | Control Fighter / Seismic Monk |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WILL |
| **Resource** | Stamina |
| **Trauma Risk** | None |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 10 PP |
| **Minimum Legend** | 5 |
| **Prerequisites** | None |
| **Exclusive With** | None |

---

## Design Philosophy

### Tagline
*"Perceive through vibration, strike through earth"*

### Core Fantasy
The Gorge-Maw Ascetic embodies the warrior-philosopher who perceives the world through vibrations in the earth rather than sight. Through disciplined meditation near colossal Gorge-Maws, they have mastered Tremorsense—a seismic perception that makes them immune to darkness and blindness but completely vulnerable to flying enemies.

### Mechanical Identity
1. **Tremorsense**: Immune to visual impairment, auto-detect ground-based enemies, blind to flying enemies
2. **Unarmed Combat**: Specialized in earth-channeling strikes and shockwaves
3. **Battlefield Control**: Emphasis on Push, Stun, Root, and Difficult Terrain effects
4. **Mental Fortress**: Exceptional resistance to Fear and mental effects, with aura protection for allies

### Gameplay Feel
Methodical, grounded, tactical. You control space and positioning while remaining steadfast against mental assault. Your vulnerability to flying enemies creates unique tactical considerations.

---

## Rank Progression

### Tree-Based Advancement
Abilities unlock through **prerequisite chains**, not PP purchase:

| Tier | PP Cost | Starting Rank | Rank Upgrades |
|------|---------|---------------|---------------|
| Tier 1 | 3 PP each | Rank 1 | → Rank 2 → Rank 3 |
| Tier 2 | 4 PP each | Rank 2 | → Rank 3 |
| Tier 3 | 5 PP each | No ranks | Full power when unlocked |
| Capstone | 6 PP | No ranks | Upgrades all Tier 1 & 2 to Rank 3 |

### Rank Unlock Requirements

| Rank | Requirement |
|------|-------------|
| Rank 2 | Unlock any Tier 2 ability in this specialization |
| Rank 3 | Unlock the Capstone ability |

---

## Ability Tree

```
                    [GORGE-MAW ASCETIC MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ TREMORSENSE     │ │ STONE FIST      │ │ CONCUSSIVE      │
│                 │ │                 │ │ PULSE           │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ NO RANKS        │ │ Unarmed strike  │ │ Push + damage   │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ SENSORY         │ │ SHATTERING      │ │ RESONANT        │
│ DISCIPLINE      │ │ WAVE            │ │ TREMOR          │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Passive         │ │ Active          │ │ Active          │
│ Mental resist   │ │ Ranged stun     │ │ Terrain zone    │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ EARTHEN         INNER       │
              │ GRASP           STILLNESS   │
              │ [Tier 3]        [Tier 3]    │
              │ AoE Root        Immunity    │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │         EARTHSHAKER         │
              │          [Capstone]         │
              │    Battlefield Earthquake   │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 26010 | [Tremorsense](abilities/tremorsense.md) | 1 | Passive | 3 | Seismic perception (NO RANKS) |
| 26011 | [Stone Fist](abilities/stone-fist.md) | 1 | Active | 3 | Unarmed strike + MIGHT |
| 26012 | [Concussive Pulse](abilities/concussive-pulse.md) | 1 | Active | 3 | Push Front Row + damage |
| 26013 | [Sensory Discipline](abilities/sensory-discipline.md) | 2 | Passive | 4 | +dice vs Fear/Disoriented |
| 26014 | [Shattering Wave](abilities/shattering-wave.md) | 2 | Active | 4 | Single target stun at range |
| 26015 | [Resonant Tremor](abilities/resonant-tremor.md) | 2 | Active | 4 | Create Difficult Terrain zone |
| 26016 | [Earthen Grasp](abilities/earthen-grasp.md) | 3 | Active | 5 | AoE [Root] + damage |
| 26017 | [Inner Stillness](abilities/inner-stillness.md) | 3 | Passive | 5 | Mental immunity + ally aura |
| 26018 | [Earthshaker](abilities/earthshaker.md) | 4 | Active | 6 | Battlefield earthquake + terrain |

---

## Core Mechanics

### Tremorsense System

The foundational mechanic that defines the specialization:

**Immunities:**
- IMMUNE to [Blinded]
- IMMUNE to [Thick Fog]
- IMMUNE to [Absolute Darkness]

**Auto-Detection:**
- Automatically detect all ground-based enemies
- Detects Hidden and Stealth enemies touching the ground
- Cannot be surprised by ground-based ambushes

**Flying Vulnerability:**
- 50% miss chance vs flying enemies
- 0 Defense vs flying enemy attacks
- Flying enemies invisible on minimap

```
If Target.IsFlying:
    AttackRoll = Roll() * 0.5  // 50% miss chance
    Defense = 0
Else:
    AutoDetect(Target)
    IgnoreVisualImpairment = true
```

### Control Effects

| Effect | Source | Duration |
|--------|--------|----------|
| [Rooted] | Earthen Grasp | 3 turns |
| [Stunned] | Shattering Wave | 1-2 turns |
| [Staggered] | Stone Fist, Concussive Pulse | 1 turn |
| [Knocked Down] | Earthshaker | 2 turns |
| [Difficult Terrain] | Resonant Tremor, Earthshaker | 3-4 turns / Permanent |

---

## Situational Power Profile

### Optimal Conditions
- Ground-based enemy encounters
- Darkness or fog environments
- Need for battlefield control
- Protecting mentally vulnerable allies

### Weakness Conditions
- Flying enemies present
- Wide-open aerial battlefields
- Enemies with ranged superiority
- Tremorsense-immune creatures

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | You control, they damage |
| **Skald** | Mental aura stacks with songs |
| **Atgeir-Wielder** | Combined zone control |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Ranged specialists** | Your control may not protect them |
| **Flying-focused builds** | Your weakness is their strength |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Tremorsense + basic control |
| 6-10 | High | Zone control comes online |
| 11-15 | Very High | Mental immunity + ally protection |
| 16+ | Extreme | Earthshaker battlefield dominance |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 5/10 | Control-focused, not damage |
| Survivability | 7/10 | Mental immunity helps |
| Support | 7/10 | Aura and control protect allies |
| Control | 10/10 | Best-in-class battlefield control |
| Utility | 6/10 | Specialized tremorsense |

---

## Voice Guidance

### Tone Profile
- Calm, philosophical, grounded
- Speaks in earth metaphors
- Patient and deliberate

### Example Quotes (NPC Flavor Text)
- *"The earth speaks to those who listen. I have learned its language."*
- *"Flying things... they exist in a world I cannot touch. A humbling reminder."*
- *"Still your mind. Feel the vibrations. The ground never lies."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement Tremorsense system
- [ ] Add visual impairment immunity
- [ ] Implement flying enemy penalties

### Phase 2: Core Abilities
- [ ] Implement Stone Fist melee attack
- [ ] Implement Concussive Pulse push mechanics
- [ ] Implement row-based positioning

### Phase 3: Advanced Systems
- [ ] Implement zone creation (Difficult Terrain)
- [ ] Implement Stun/Stagger status effects
- [ ] Add mental resistance bonuses

### Phase 4: Capstone
- [ ] Implement Inner Stillness immunity + aura
- [ ] Implement Earthshaker battlefield effect
- [ ] Add permanent terrain modification

### Phase 5: Polish
- [ ] Add Tremorsense visual indicators
- [ ] Implement flying enemy warnings
- [ ] Test control effect interactions

---

## Testing Requirements

### Unit Tests
- Tremorsense immunity verification
- Flying enemy penalty calculation
- Zone creation and duration
- Status effect application

### Integration Tests
- Full combat with mixed enemy types
- Darkness/fog environment behavior
- Ally aura proximity detection

### Manual QA
- Verify control feels impactful
- Test flying vulnerability balance
- Confirm mental immunity satisfaction

---

## Logging Requirements

### Event Templates

```
OnTremorsenseDetect:
  "Tremorsense detects {EnemyName} at ({X},{Y})!"

OnFlyingPenalty:
  "Attack vs {FlyingEnemy} has 50% miss chance (Tremorsense blind)"

OnZoneCreation:
  "Resonant Tremor creates Difficult Terrain ({Size}) for {Duration} turns"

OnEarthshaker:
  "EARTHSHAKER! {Count} ground enemies knocked down!"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Warrior Archetype](../../archetypes/warrior.md) | Parent archetype |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Stunned Status](../../../04-systems/status-effects/stunned.md) | Status effect |
| [Rooted Status](../../../04-systems/status-effects/rooted.md) | Status effect |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
