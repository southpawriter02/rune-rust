---
id: SPEC-ECHO-CALLER-28002
title: "Echo-Caller"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo-Caller

## Identity

| Property | Value |
|----------|-------|
| **Name** | Echo-Caller |
| **Archetype** | Mystic |
| **Path Type** | Coherent |
| **Role** | Psychic Artillery / Crowd Control |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource** | Aether Pool |
| **Trauma Risk** | Medium |

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

**Tagline:** "Psychic Artillery"

**Core Fantasy:** You are the reality manipulator who weaponizes the Great Silence's eternal psychic scream. Where others commune with echoes, you command them. You are psychic artillery—projecting traumatic memories as weapons, creating cascading fear, and implanting phantom sensations in enemy minds.

**Mechanical Identity:**
1. **[Echo] Tag System** - Most abilities have [Echo] tag enabling chain effects
2. **Fear Manipulation** - Specializes in applying and exploiting [Feared] status
3. **Echo Chain** - Abilities spread to adjacent enemies for reduced effect
4. **Terror Feedback** - Gains resources from applying Fear

**Gameplay Feel:** AoE psychic damage dealer who chains effects between enemies. Masters of [Feared] status with bonus damage against terrified targets.

---

## Core Mechanics

### Echo System

**[Echo] Tag Mechanics:**
- Abilities tagged [Echo] can trigger **Echo Chain**
- **Echo Chain**: 50-80% damage/effect spreads to 1-2 adjacent enemies
- Echo Cascade passive enhances chain range and damage
- Some abilities gain bonus effects vs [Feared] targets

**Echo Chain Scaling:**

| Rank | Chain Range | Chain Damage | Targets |
|------|-------------|--------------|---------|
| Base | 1 tile | 50% | 1 |
| With Echo Cascade (Rank 2) | 2 tiles | 70% | 1 |
| With Echo Cascade (Rank 3) | 3 tiles | 80% | 2 |

### Terror Feedback Loop

| Trigger | Effect |
|---------|--------|
| Apply [Feared] | +15-20 Aether |
| Hit [Feared] target | +1d8 - +2d8 bonus damage |
| Fear Cascade success | +[Empowered] (+2 dice damage) |

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
[Echo Attunement]   [Scream of Silence]  [Phantom Menace]
    (Passive)           (Active)            (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Echo Cascade]      [Reality Fracture]   [Terror Feedback]
   (Passive)            (Active)            (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
    [Fear Cascade]            [Echo Displacement]
       (Active)                    (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
              [Silence Made Weapon]
                    (Active)
```

### Ability Index

| ID | Name | Tier | Type | PP | Key Effect |
|----|------|------|------|-----|------------|
| 28010 | Echo Attunement | 1 | Passive | 3 | -Aether cost, +psychic resist |
| 28011 | Scream of Silence | 1 | Active | 3 | [Echo] Psychic damage, bonus vs Feared |
| 28012 | Phantom Menace | 1 | Active | 3 | [Echo] Apply [Feared] |
| 28013 | Echo Cascade | 2 | Passive | 4 | Echo Chain +range +damage |
| 28014 | Reality Fracture | 2 | Active | 4 | [Echo] Damage + [Disoriented] + Push |
| 28015 | Terror Feedback | 2 | Passive | 4 | Restore Aether when applying Fear |
| 28016 | Fear Cascade | 3 | Active | 5 | [Echo] AoE Fear spread |
| 28017 | Echo Displacement | 3 | Active | 5 | [Echo] Forced teleportation |
| 28018 | Silence Made Weapon | 4 | Active | 6 | Ultimate AoE scaling with Fear |

---

## Situational Power Profile

### Optimal Conditions
- Multiple clustered enemies (Echo Chain value)
- Enemies susceptible to Fear
- Extended combats allowing chain buildup
- Intelligent enemies (Fear affects behavior)
- [Psychic Resonance] zones (+15% Echo Chain chance)

### Weakness Conditions
- Single target encounters (chains wasted)
- Fear-immune enemies (Undying, Mindless)
- Spread out enemy formations
- Short fights
- Low Aether situations

---

## Party Synergies

### Positive Synergies
- **Seiðkona** - Combined psychic assault
- **Hlekkr-Master** - Fear enables control bonuses
- **Tanks** - Hold enemies together for chains
- **Skald** - Stress management for medium trauma risk

### Negative Synergies
- **Other AoE builds** - Competing for clustered targets
- **Fear-based builds** - Diminishing returns on Fear
- **High-mobility compositions** - Enemies scatter

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 5-7 | Medium | Learning chain mechanics |
| 8-12 | High | Echo Cascade online |
| 13-17 | Very High | Fear Cascade mastery |
| 18+ | Extreme | Silence Made Weapon |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★★☆☆ | Bonus vs Feared helps |
| AoE Damage | ★★★★★ | Primary strength |
| Control | ★★★★★ | Fear specialist |
| Support | ★☆☆☆☆ | Purely offensive |
| Survivability | ★★☆☆☆ | Fragile caster |

---

## Voice Guidance

### Tone Profile
- Unsettling calm
- Speaks of silence as a presence
- Detached observation of terror
- Matter-of-fact about psychic violence

### Example Quotes
- "The silence speaks. Can you hear it screaming?"
- "Your fear is not weakness—it is truth."
- "I merely show them what the world has become."
- "The echoes remember everything. Everything."

---

## Phased Implementation Guide

### Phase 1: Echo System
- [ ] Implement [Echo] tag on abilities
- [ ] Implement Echo Chain spread mechanic
- [ ] Implement adjacency detection for chains
- [ ] Chain damage calculation

### Phase 2: Fear Mechanics
- [ ] Implement [Feared] status (flee behavior)
- [ ] Implement bonus damage vs Feared
- [ ] Implement Terror Feedback resource restoration
- [ ] Fear chance calculations

### Phase 3: AoE Processing
- [ ] Fear Cascade area targeting
- [ ] Silence Made Weapon battlefield targeting
- [ ] Damage scaling with debuff count
- [ ] Multiple target processing

### Phase 4: Capstone
- [ ] Implement scaling damage (base + per debuffed)
- [ ] Implement mass Fear application
- [ ] Implement Psychic Stress cost
- [ ] Twice-per-combat tracking

### Phase 5: Polish
- [ ] Echo Chain visual effects
- [ ] Fear cascade animations
- [ ] Psychic damage visual indicators
- [ ] Aether regeneration notifications

---

## Testing Requirements

### Unit Tests
- Echo Chain spread calculations
- Fear chance rolls
- Bonus damage vs Feared
- Aether cost reduction

### Integration Tests
- Multi-enemy chain processing
- Fear behavior (flee mechanics)
- Capstone damage scaling
- Echo Cascade range detection

### Manual QA Checklist
- [ ] Echo Chain visually connects targets
- [ ] Feared enemies flee correctly
- [ ] Terror Feedback restores Aether on Fear
- [ ] Silence Made Weapon scales with debuffs
- [ ] Psychic Stress costs apply

---

## Logging Requirements

### Combat Events
```
"Scream of Silence: {damage} Psychic damage to {target}"
"ECHO CHAIN: {chain_damage} spreads to {adjacent_target}!"
"Phantom Menace: {target} is [Feared] for {duration} turns"
"Terror Feedback: +{aether} Aether (Fear applied)"
"Fear Cascade: {count} enemies must resist or become [Feared]"
"SILENCE MADE WEAPON: {total_damage} damage to {count} enemies! (Base + {bonus}d10 scaling)"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Mystic Archetype](../../archetypes/mystic.md) | Parent archetype |
| [Feared Status](../../../04-systems/status-effects/feared.md) | Status effect details |
| [Stress System](../../../01-core/resources/stress.md) | Psychic Stress mechanics |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
