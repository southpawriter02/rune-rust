---
id: SPEC-SKAR-HORDE-ASPIRANT-29001
title: "Skar-Horde Aspirant"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Skar-Horde Aspirant

## Identity

| Property | Value |
|----------|-------|
| **Name** | Skar-Horde Aspirant |
| **Archetype** | Warrior |
| **Path Type** | Heretical |
| **Role** | Melee DPS / Armor-Breaker |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | None |
| **Resource** | Stamina + Savagery (0-100) |
| **Trauma Risk** | Extreme |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 5 |
| **Maximum Corruption** | 100 |
| **Required Quest** | None |

---

## Design Philosophy

**Tagline:** "The Warrior Who Bleeds for Power"

**Core Fantasy:** The Skar-Horde Aspirant embodies the heretical philosophy of achieving power through savage, willful self-mutilation. You have ritualistically replaced your hand with a modular weapon-stump augment, trading humanity for devastating combat prowess.

Build Savagery by fighting in melee, then unleash armor-bypassing attacks that ignore all defenses. Every strike pushes you closer to madness, but power is worth any price. You are no longer human. You are a weapon.

**Mechanical Identity:**
1. **Augmentation System** - Replaced hand with modular weapon-stump
2. **Savagery Resource** - Builds through combat, required for abilities
3. **Armor Bypass** - Specializes in damage that ignores defenses
4. **High-Risk Trauma** - Extreme psychic cost for power

**Gameplay Feel:** Berserker-style melee fighter who builds momentum through combat, then unleashes devastating armor-piercing attacks. High risk, high reward playstyle.

---

## Core Mechanics

### Savagery Resource System

| Property | Value |
|----------|-------|
| **Range** | 0-100 |
| **Generation** | Hits, damage taken, causing Fear |
| **Decay** | Slowly out of combat |
| **Use** | Required for Tier 2+ abilities |

**Generation Sources:**

| Source | Base Savagery |
|--------|---------------|
| Savage Strike hit | 15-25 (by rank) |
| Taking damage | 10-20% of damage |
| Causing Fear | +5 |
| Kill (varies) | Ability-dependent |

### Augmentation System

The Skar-Horde Aspirant uses an **[Augmentation] slot** instead of standard weapon:

| Augment Type | Damage Type | Special |
|--------------|-------------|---------|
| Piercing Spike | Piercing | Required for Impaling Spike |
| Blunt Piston | Bludgeoning | Required for Overcharged Piston Slam |
| Slashing Blade | Slashing | Standard damage |
| Flame Emitter | Fire | Bonus vs organic |

---

## Rank Progression

### Tier Unlock Requirements

| Tier | PP Cost | Starting Rank | Max Rank | Progression |
|------|---------|---------------|----------|-------------|
| Tier 1 | 0 PP (free) | Rank 1 | Rank 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| Tier 2 | 4 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Tier 3 | 5 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Capstone | 6 PP | Rank 1 | Rank 3 | 1→2→3 (tree-based) |

### Total Investment: 31 PP (3 unlock + 28 abilities)

---

## Ability Tree

```
                    TIER 1: FOUNDATION (Free with Spec)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Heretical           [Savage Strike]      [Horrific Form]
 Augmentation]         (Active)            (Passive)
   (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Grievous Wound]   [Impaling Spike]   [Pain Fuels Savagery]
    (Active)           (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
   [Overcharged           [The Price of Power]
    Piston Slam]               (Passive)
      (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
              [Monstrous Apotheosis]
                    (Active)
```

### Ability Index

| ID | Name | Tier | Type | PP | Key Effect |
|----|------|------|------|-----|------------|
| 29001 | Heretical Augmentation | 1 | Passive | 0 | Unlocks [Augmentation] slot |
| 29002 | Savage Strike | 1 | Active | 0 | Basic attack + Savagery gen |
| 29003 | Horrific Form | 1 | Passive | 0 | Fear melee attackers |
| 29004 | Grievous Wound | 2 | Active | 4 | DoT that bypasses Soak |
| 29005 | Impaling Spike | 2 | Active | 4 | Root enemy (Piercing) |
| 29006 | Pain Fuels Savagery | 2 | Passive | 4 | Damage → Savagery |
| 29007 | Overcharged Piston Slam | 3 | Active | 5 | Massive damage + Stun |
| 29008 | The Price of Power | 3 | Passive | 5 | +Savagery gen, +Stress |
| 29009 | Monstrous Apotheosis | 4 | Active | 6 | Transform state |

---

## Situational Power Profile

### Optimal Conditions
- Extended melee combat (Savagery building)
- Multiple enemies to Fear (Horrific Form procs)
- Heavily armored targets (armor bypass)
- Parties with healing support
- Boss fights requiring burst damage

### Weakness Conditions
- Ranged-heavy encounters (can't build Savagery)
- Fear-immune enemies
- High ambient Stress (self-Stress generation)
- Short fights (no time to build momentum)
- Solo without support

---

## Party Synergies

### Positive Synergies
- **Bone-Setter** - Essential Stress management
- **Skald** - Saga of Einherjar + Apotheosis combo
- **Tanks** - Draw aggro while Aspirant builds Savagery
- **Controllers** - Keep enemies in melee range

### Negative Synergies
- **Other high-Stress builds** - Competing for Stress threshold
- **Stealth compositions** - Horrific Form draws attention
- **Parties without healing** - Self-damage unsustainable

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 5-7 | Medium | Learning Savagery management |
| 8-12 | High | Armor bypass mastery |
| 13-17 | Very High | Apotheosis timing |
| 18+ | Extreme | Full berserker power |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★★★★ | Primary strength |
| AoE Damage | ★★☆☆☆ | Limited spread |
| Control | ★★★☆☆ | Fear, Root, Stun |
| Support | ★☆☆☆☆ | Pure offensive |
| Survivability | ★★☆☆☆ | High risk |

---

## Voice Guidance

### Tone Profile
- Barely contained violence
- Dismissive of pain
- Pride in transformation
- Contempt for weakness

### Example Quotes
- "Pain is just fuel. Watch me burn."
- "Your armor means nothing. I'll carve through it."
- "I gave up my hand. What have YOU sacrificed for power?"
- "I am no longer human. I am a WEAPON."

---

## Phased Implementation Guide

### Phase 1: Core Framework
- [ ] Implement Savagery resource system (0-100)
- [ ] Implement [Augmentation] equipment slot
- [ ] Create augment item types
- [ ] Rank calculation based on tree progression

### Phase 2: Combat Integration
- [ ] Route Skar-Horde abilities through CombatEngine
- [ ] Implement Savagery generation from hits/damage
- [ ] Add Savagery bar to CombatView
- [ ] Implement augment type requirements

### Phase 3: Status Effects
- [ ] Implement [Grievous Wound] with Soak bypass
- [ ] Implement [Rooted] status
- [ ] Implement [Apotheosis] transformation state
- [ ] Implement immunity system

### Phase 4: Trauma Integration
- [ ] Implement Stress generation from The Price of Power
- [ ] Implement post-Apotheosis Stress penalty
- [ ] Add trauma warning indicators

### Phase 5: Polish
- [ ] Rank-specific icons (Bronze/Silver/Gold)
- [ ] Rank-up notifications
- [ ] Apotheosis transformation visual

---

## Testing Requirements

### Unit Tests
- Savagery generation calculations
- Augment damage type routing
- Soak bypass mechanics
- Transformation state effects

### Integration Tests
- Combat flow with Savagery building
- Augment swap action economy
- Apotheosis end-state Stress
- Fear trigger on melee hits

### Manual QA Checklist
- [ ] Savagery bar updates correctly
- [ ] Augment swap works in 1-2 actions
- [ ] Grievous Wound bypasses Soak
- [ ] Apotheosis grants listed immunities
- [ ] Stress applies after Apotheosis

---

## Logging Requirements

### Combat Events
```
"Savage Strike: {damage} damage to {target}. +{savagery} Savagery"
"Horrific Form: [Enemy] recoils in horror! [Feared] for 1 turn"
"Grievous Wound: {damage} damage + [Grievous Wound] (bypasses Soak)"
"[Grievous Wound] tick: {target} takes {damage} (ignores Soak)"
"Impaling Spike: {target} is [Rooted] for 3 turns!"
"Pain Fuels Savagery: +{savagery} from {damage} damage taken"
"The Price of Power: +{savagery} Savagery, +{stress} Psychic Stress"
"MONSTROUS APOTHEOSIS! You are no longer human. You are a WEAPON."
"Apotheosis ends. +{stress} Psychic Stress"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Warrior Archetype](../../archetypes/warrior.md) | Parent archetype |
| [Stress System](../../../01-core/resources/stress.md) | Psychic Stress mechanics |
| [Trauma Economy](../../../01-core/trauma-economy.md) | Risk/reward framework |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
