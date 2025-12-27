---
id: SPEC-MYRK-GENGR-24002
title: "Myrk-Gengr (Shadow-Walker)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Myrk-Gengr (Shadow-Walker)

**Archetype:** Skirmisher | **Path:** Heretical | **Role:** Stealth Assassin / Alpha Strike Specialist

> *"The Ghost in the Machine"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Myrk-Gengr (Shadow-Walker) |
| **Archetype** | Skirmisher |
| **Path Type** | Heretical |
| **Role** | Stealth Assassin / Alpha Strike Specialist |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | WILL |
| **Resource** | Stamina + Focus |
| **Trauma Risk** | High |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 5 |
| **Corruption** | Any (0-100) |
| **Prerequisites** | None |
| **Exclusive With** | None |

---

## Design Philosophy

### Tagline
*"You are the ghost in the machine"*

### Core Fantasy
You've learned to wrap yourself in the world's psychic static, becoming a blind spot in enemy perception. Your attacks from stealth don't just deal physical damage—they inflict psychological terror, shattering minds alongside bodies.

Your capstone ability lets you become a living glitch, a reality-warping violation of causality. You are the predator who strikes from places the mind insists are empty.

### Mechanical Identity
1. **[Hidden] State**: Core stealth mechanic—enemies cannot target you directly
2. **Psychic Resonance Zones**: Areas of psychic static that enhance stealth
3. **Terror Strikes**: First attack from stealth inflicts massive Psychic Stress and Fear
4. **Stealth Persistence**: Chance to remain Hidden after attacking
5. **Self-Corruption Risk**: Capstone inflicts significant self-Corruption

### Gameplay Feel
Patient, predatory, devastating. You wait for the perfect moment, then delete a target before they can react. High risk, high reward assassination gameplay with psychological warfare elements.

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
                    [MYRK-GENGR SHADOW MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ ONE WITH THE    │ │ ENTER THE VOID  │ │ SHADOW STRIKE   │
│ STATIC I        │ │                 │ │                 │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ Stealth bonus   │ │ Enter [Hidden]  │ │ Crit from hide  │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ THROAT-CUTTER   │ │ SENSORY         │ │ MIND OF         │
│                 │ │ SCRAMBLE        │ │ STILLNESS       │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Active          │ │ Active          │ │ Passive         │
│ Silence target  │ │ Create zone     │ │ Regen while     │
│                 │ │                 │ │ hidden          │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ TERROR FROM     GHOSTLY     │
              │ THE VOID        FORM        │
              │ [Tier 3]        [Tier 3]    │
              │ First strike    Persist     │
              │ terror          stealth     │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │        LIVING GLITCH        │
              │          [Capstone]         │
              │    Become a Reality Glitch  │
              │      +15 Self-Corruption    │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 24010 | [One with the Static I](abilities/one-with-the-static-i.md) | 1 | Passive | 3 | +Stealth dice, enhanced in Resonance zones |
| 24011 | [Enter the Void](abilities/enter-the-void.md) | 1 | Active | 3 | Enter [Hidden] state |
| 24012 | [Shadow Strike](abilities/shadow-strike.md) | 1 | Active | 3 | Guaranteed crit from [Hidden] |
| 24013 | [Throat-Cutter](abilities/throat-cutter.md) | 2 | Active | 4 | Damage + [Silenced] from flank/Hidden |
| 24014 | [Sensory Scramble](abilities/sensory-scramble.md) | 2 | Active | 4 | Create [Psychic Resonance] zone |
| 24015 | [Mind of Stillness](abilities/mind-of-stillness.md) | 2 | Passive | 4 | Regen Stamina/remove Stress while Hidden |
| 24016 | [Terror from the Void](abilities/terror-from-the-void.md) | 3 | Passive | 5 | First strike: +Stress, +[Feared] |
| 24017 | [Ghostly Form](abilities/ghostly-form.md) | 3 | Passive | 5 | +Defense, stealth persistence chance |
| 24018 | [Living Glitch](abilities/living-glitch.md) | 4 | Active | 6 | Ultimate assassination, +15 self-Corruption |

---

## Core Mechanics

### [Hidden] State

The foundation of Shadow-Walker gameplay:

```
[Hidden] State Properties:
- Enemies cannot target you directly
- Breaks when: you attack (unless Ghostly Form) or fail stealth check
- Visual: Character becomes translucent/static effect
- Duration: Until broken
```

### Psychic Resonance Zones

Created by Sensory Scramble:
- Enemy debuff: -1d10 Perception
- Your bonus: +2d10 to Enter the Void
- Synergy: One with the Static provides additional +2d10

### Terror Strike Flow

First Shadow Strike each combat triggers:
1. Shadow Strike executes (guaranteed crit)
2. Terror from the Void triggers: +15 Psychic Stress, 85% [Feared]
3. Ghostly Form persistence check (65% stay hidden)
4. Stealth state resolved

---

## Situational Power Profile

### Optimal Conditions
- Single high-priority target
- Darkness or Resonance zones
- First strike opportunity
- Target isolation possible

### Weakness Conditions
- Multiple enemies aware of you
- Sustained combat (no re-stealth opportunity)
- Flying enemies (harder to reach)
- Anti-stealth abilities

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Skald** | Distracts while you position |
| **Gorge-Maw Ascetic** | Creates chaos for stealth entry |
| **Hlekkr-Master** | Controls enemies you're targeting |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **AoE-focused builds** | May break your stealth |
| **Aggressive front-liners** | Draw attention before you strike |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Basic stealth + crit online |
| 6-10 | High | Mind of Stillness sustain |
| 11-15 | Very High | Terror from the Void + Ghostly Form |
| 16+ | Extreme | Living Glitch deletes targets |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 10/10 | Best single-target burst |
| Survivability | 4/10 | If caught, very fragile |
| Support | 3/10 | Minimal team utility |
| Control | 5/10 | [Feared], [Silenced] |
| Utility | 6/10 | Scouting, assassination |

---

## Voice Guidance

### Tone Profile
- Cold, patient, clinical
- Speaks of targets as "problems to solve"
- Detached from violence emotionally

### Example Quotes (NPC Flavor Text)
- *"You never saw me. You never will."*
- *"The void between heartbeats—that's where I live."*
- *"They fear the dark. They should fear what's in it."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement [Hidden] state system
- [ ] Add stealth check mechanics
- [ ] Implement Focus resource

### Phase 2: Core Abilities
- [ ] Implement Enter the Void stealth entry
- [ ] Implement Shadow Strike guaranteed crit
- [ ] Add [Bleeding] status effect

### Phase 3: Advanced Systems
- [ ] Implement [Psychic Resonance] zone
- [ ] Implement Terror from the Void trigger
- [ ] Add Ghostly Form persistence

### Phase 4: Capstone
- [ ] Implement Living Glitch
- [ ] Add self-Corruption mechanic
- [ ] Implement guaranteed hit system

### Phase 5: Polish
- [ ] Add stealth visual indicators
- [ ] Implement zone visual effects
- [ ] Test persistence probability

---

## Testing Requirements

### Unit Tests
- [Hidden] state targeting immunity
- Stealth check bonus calculation
- Ghostly Form persistence probability
- Living Glitch self-Corruption

### Integration Tests
- Full assassination sequence
- Terror from the Void trigger
- Psychic Resonance zone effects

### Manual QA
- Verify stealth feels responsive
- Test assassination satisfaction
- Confirm corruption risk is meaningful

---

## Logging Requirements

### Event Templates

```
OnEnterHidden:
  "[Shadow-Walker] vanishes into the psychic static"

OnShadowStrike:
  "SHADOW STRIKE! {Damage} critical damage from stealth!"

OnTerrorFromVoid:
  "Terror from the Void: +15 Psychic Stress, {Target} is [Feared]!"

OnGhostlyFormPersist:
  "Ghostly Form: Stealth maintained! (65% success)"

OnLivingGlitch:
  "LIVING GLITCH! {Damage} unblockable damage. +15 Self-Corruption."
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skirmisher Archetype](../../archetypes/skirmisher.md) | Parent archetype |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Stress System](../../../01-core/resources/stress.md) | Psychic Stress mechanics |
| [Feared Status](../../../04-systems/status-effects/feared.md) | Status effect |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
