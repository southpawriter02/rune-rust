---
id: SPEC-STRANDHOGG-25001
title: "Strandhogg (Glitch-Raider)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Strandhogg (Glitch-Raider)

**Archetype:** Skirmisher | **Path:** Coherent | **Role:** Mobile Burst DPS / Momentum Fighter

> *"The Momentum Striker"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Strandhogg (Glitch-Raider) |
| **Archetype** | Skirmisher |
| **Path Type** | Coherent |
| **Role** | Mobile Burst DPS / Momentum Fighter |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | MIGHT |
| **Resource** | Stamina + Momentum |
| **Trauma Risk** | Low |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 5 |
| **Prerequisites** | None |
| **Exclusive With** | None |

---

## Design Philosophy

### Tagline
*"Time stutters. Bodies fall."*

### Core Fantasy
The Strandhogg is the kinetic blur, the glitch-raider who exploits unstable physics to move impossibly fast. You build Momentum through movement and strikes, then spend it on devastating executions. You are the whirlwind that strikes from unexpected angles and vanishes before retaliation.

### Mechanical Identity
1. **Momentum Resource System**: Builds Momentum (0-100) through movement and strikes
2. **Hit-and-Run**: Attack and reposition in the same turn
3. **Debuff Exploitation**: Deals bonus damage and generates extra Momentum vs debuffed enemies
4. **Kinetic Violence**: Executes devastating finishers by spending accumulated Momentum

### Gameplay Feel
Fast, aggressive, flowing. Combat feels like a dance—strike, build momentum, reposition, execute. The longer the fight continues, the more powerful you become.

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
                    [STRANDHOGG MOMENTUM MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ HARRIER'S       │ │ REAVER'S        │ │ DREAD CHARGE    │
│ ALACRITY I      │ │ STRIKE          │ │                 │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ Start with      │ │ Attack + build  │ │ Move + attack   │
│ Momentum        │ │ Momentum        │ │ + Disorient     │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ TIDAL RUSH      │ │ HARRIER'S       │ │ VICIOUS FLANK   │
│                 │ │ WHIRLWIND       │ │                 │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Passive         │ │ Active          │ │ Active          │
│ Bonus vs        │ │ Attack + free   │ │ +50% damage     │
│ debuffed        │ │ reposition      │ │ vs debuffed     │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ NO QUARTER     SAVAGE       │
              │                HARVEST      │
              │ [Tier 3]       [Tier 3]     │
              │ Free move      Execution    │
              │ on kill        attack       │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │      RIPTIDE OF CARNAGE     │
              │          [Capstone]         │
              │     4 Attacks in One Turn   │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 25001 | [Harrier's Alacrity I](abilities/harriers-alacrity-i.md) | 1 | Passive | 3 | Start with Momentum + Vigilance bonus |
| 25002 | [Reaver's Strike](abilities/reavers-strike.md) | 1 | Active | 3 | Basic attack + 15 Momentum |
| 25003 | [Dread Charge](abilities/dread-charge.md) | 1 | Active | 3 | Move + attack + [Disoriented] |
| 25004 | [Tidal Rush](abilities/tidal-rush.md) | 2 | Passive | 4 | Bonus Momentum vs debuffed enemies |
| 25005 | [Harrier's Whirlwind](abilities/harriers-whirlwind.md) | 2 | Active | 4 | Attack + free reposition |
| 25006 | [Vicious Flank](abilities/vicious-flank.md) | 2 | Active | 4 | +50% damage vs debuffed |
| 25007 | [No Quarter](abilities/no-quarter.md) | 3 | Passive | 5 | Free move + Momentum on kill |
| 25008 | [Savage Harvest](abilities/savage-harvest.md) | 3 | Active | 5 | Massive execution + refund on kill |
| 25009 | [Riptide of Carnage](abilities/riptide-of-carnage.md) | 4 | Active | 6 | 4 attacks in one turn |

---

## Core Mechanics

### Momentum Resource System

| Property | Value |
|----------|-------|
| **Range** | 0-100 |
| **Starting Combat** | 20-30 (depends on Harrier's Alacrity rank) |
| **Generation** | Movement, attacks, kills |
| **Spending** | Powerful abilities consume Momentum |
| **Decay** | Slowly decays out of combat |

```
┌─────────────────────────────────────────┐
│  MOMENTUM [████████████░░░░░░] 65/100   │
│  ⚡ Movement and strikes build power    │
└─────────────────────────────────────────┘
```

### Momentum Generation

| Source | Momentum Gained |
|--------|-----------------|
| Reaver's Strike | +15 (25 vs debuffed) |
| Dread Charge | +10-15 |
| Harrier's Whirlwind (free move) | +5-10 |
| No Quarter (kill) | +10 |
| Tidal Rush (vs debuffed) | +10-15 bonus |

### Debuff Exploitation

Strandhogg gains bonuses against enemies with:
- Control debuffs: [Disoriented], [Stunned], [Feared], [Slowed], [Rooted]
- DoTs (Rank 3): [Bleeding], [Burning], [Poisoned]

---

## Situational Power Profile

### Optimal Conditions
- Multiple enemies (multi-target kills)
- Debuff-heavy team composition
- Extended combat (Momentum building)
- Hit-and-run terrain

### Weakness Conditions
- Single boss fights (less kill momentum)
- Locked-down positioning
- Short fights (no ramp time)
- Anti-mobility enemies

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Hlekkr-Master** | Applies debuffs for exploitation |
| **Echo-Caller** | [Feared] triggers Tidal Rush |
| **Rust-Witch** | DoTs trigger Tidal Rush (Rank 3) |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Static positioning builds** | Strandhogg needs to move |
| **Single-target focus** | Less kill opportunities |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Basic Momentum building |
| 6-10 | High | Tidal Rush amplification |
| 11-15 | Very High | No Quarter + Savage Harvest executions |
| 16+ | Extreme | Riptide of Carnage devastation |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 9/10 | Excellent burst, scales with kills |
| Survivability | 5/10 | Hit-and-run helps |
| Support | 3/10 | Minimal team utility |
| Control | 4/10 | [Disoriented] on Dread Charge |
| Utility | 5/10 | High mobility |

---

## Voice Guidance

### Tone Profile
- Aggressive, eager, kinetic
- Speaks in motion metaphors
- Thrives on chaos

### Example Quotes (NPC Flavor Text)
- *"Standing still is dying slow."*
- *"Watch the blur. That's the last thing you'll see."*
- *"Every kill feeds the tide. Every tide drowns another."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement Momentum resource (0-100)
- [ ] Add starting Momentum from Harrier's Alacrity
- [ ] Implement Momentum UI bar

### Phase 2: Core Abilities
- [ ] Implement Reaver's Strike Momentum generation
- [ ] Implement Dread Charge move + attack
- [ ] Add [Disoriented] status effect

### Phase 3: Advanced Systems
- [ ] Implement Tidal Rush debuff detection
- [ ] Implement free repositioning
- [ ] Add kill tracking for refunds

### Phase 4: Capstone
- [ ] Implement Riptide of Carnage multi-attack
- [ ] Add Psychic Stress cost
- [ ] Implement per-kill Momentum refund

### Phase 5: Polish
- [ ] Add Momentum generation visual feedback
- [ ] Test hit-and-run flow
- [ ] Balance Momentum costs

---

## Testing Requirements

### Unit Tests
- Momentum generation values
- Starting Momentum by rank
- Debuff detection for bonuses
- Kill refund calculation

### Integration Tests
- Full combat Momentum cycle
- Multi-target Riptide execution
- Party debuff synergy

### Manual QA
- Verify combat flow feels fast
- Test hit-and-run satisfaction
- Confirm Momentum bar readability

---

## Logging Requirements

### Event Templates

```
OnMomentumGain:
  "Momentum: +{Amount} ({Source}) [Now: {Current}/100]"

OnMomentumSpend:
  "Momentum spent: -{Amount} for {Ability}"

OnKillRefund:
  "No Quarter: +10 Momentum, free reposition available!"

OnRiptide:
  "RIPTIDE OF CARNAGE! 4 attacks incoming!"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skirmisher Archetype](../../archetypes/skirmisher.md) | Parent archetype |
| [Momentum Resource](../../../01-core/resources/momentum.md) | Momentum system |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Disoriented Status](../../../04-systems/status-effects/disoriented.md) | Status effect |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
