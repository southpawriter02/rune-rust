---
id: SPEC-HLEKKR-MASTER-25002
title: "Hlekkr-Master (Chain-Master)"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Hlekkr-Master (Chain-Master)

**Archetype:** Skirmisher | **Path:** Coherent | **Role:** Battlefield Controller / Formation Breaker

> *"The Battlefield Puppeteer"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Hlekkr-Master (Chain-Master) |
| **Archetype** | Skirmisher |
| **Path Type** | Coherent |
| **Role** | Battlefield Controller / Formation Breaker |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | MIGHT |
| **Resource** | Stamina |
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
*"Dance, puppets. Dance."*

### Core Fantasy
The Hlekkr-master is the battlefield puppeteer who exploits glitching physics to drag enemies into kill zones and lock them down. You use chains, hooks, and nets to control positioning and punish helplessness. Your chains don't just lock down—they make enemies die faster.

### Mechanical Identity
1. **Battlefield Control**: Specializes in Pull, Push, Root, and Slow effects
2. **Corruption Exploitation**: Control effects more effective vs corrupted enemies
3. **Formation Breaking**: Drags enemies out of position
4. **Punish the Helpless**: Massive bonus damage vs controlled enemies

### Gameplay Feel
Tactical, predatory, methodical. You set up kills by controlling where enemies stand. Corrupted enemies are particularly vulnerable to your chains—the more corrupt, the easier they are to manipulate.

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
                    [HLEKKR-MASTER CHAIN MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ PRAGMATIC       │ │ NETTING SHOT    │ │ GRAPPLING       │
│ PREPARATION I   │ │                 │ │ HOOK TOSS       │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ Trap + control  │ │ Root enemies    │ │ Pull to front   │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ SNAG THE        │ │ UNYIELDING      │ │ PUNISH THE      │
│ GLITCH          │ │ GRIP            │ │ HELPLESS        │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Passive         │ │ Active          │ │ Passive         │
│ Corruption      │ │ [Seized] vs     │ │ +100% damage    │
│ scaling         │ │ machines        │ │ vs controlled   │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ CHAIN          CORRUPTION   │
              │ SCYTHE         SIPHON CHAIN │
              │ [Tier 3]       [Tier 3]     │
              │ Row AoE        Stun + purge │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │       MASTER OF PUPPETS     │
              │          [Capstone]         │
              │   Pull/Push = Vulnerable    │
              │      + Corruption Bomb      │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 25010 | [Pragmatic Preparation I](abilities/pragmatic-preparation-i.md) | 1 | Passive | 3 | +dice to traps, control +1 turn |
| 25011 | [Netting Shot](abilities/netting-shot.md) | 1 | Active | 3 | Low damage + [Rooted] |
| 25012 | [Grappling Hook Toss](abilities/grappling-hook-toss.md) | 1 | Active | 3 | Pull Back→Front + [Disoriented] |
| 25013 | [Snag the Glitch](abilities/snag-the-glitch.md) | 2 | Passive | 4 | Control scales with corruption |
| 25014 | [Unyielding Grip](abilities/unyielding-grip.md) | 2 | Active | 4 | [Seized] vs machines |
| 25015 | [Punish the Helpless](abilities/punish-the-helpless.md) | 2 | Passive | 4 | +100% damage vs controlled |
| 25016 | [Chain Scythe](abilities/chain-scythe.md) | 3 | Active | 5 | Row AoE + [Slowed]/[Knocked Down] |
| 25017 | [Corruption Siphon Chain](abilities/corruption-siphon-chain.md) | 3 | Active | 5 | [Stunned] scales with corruption |
| 25018 | [Master of Puppets](abilities/master-of-puppets.md) | 4 | Hybrid | 6 | Pull/Push = [Vulnerable] + Corruption Bomb |

---

## Core Mechanics

### Control Effect Synergy

The Hlekkr-Master specializes in **stacking control effects** to enable massive damage:

```
ControlFlow:
  1. Apply [Rooted] or [Slowed] (Netting Shot, Chain Scythe)
  2. Apply additional effects ([Disoriented], [Seized])
  3. Punish the Helpless activates (+100% damage)
  4. Pull/Push triggers [Vulnerable] (Master of Puppets)
  5. Execute with doubled damage on Vulnerable targets
```

### Corruption Scaling

Control effects scale with target corruption:

| Corruption Level | Range | Control Bonus |
|------------------|-------|---------------|
| Low | 1-29 | +20% success |
| Medium | 30-59 | +40% success |
| High | 60-89 | +80% success |
| Extreme | 90+ | +100% (cannot miss) |

### Punish the Helpless Triggers

| Status Effect | Triggers Bonus? |
|---------------|-----------------|
| [Rooted] | Yes |
| [Slowed] | Yes |
| [Stunned] | Yes |
| [Seized] | Yes |
| [Disoriented] | Yes |
| [Knocked Down] | Yes |

---

## Situational Power Profile

### Optimal Conditions
- Fighting corrupted enemies
- Mechanical/Undying enemy types (for [Seized])
- Formation-based encounters
- Supporting heavy damage dealers

### Weakness Conditions
- Single target bosses (less control value)
- Immune-to-control enemies
- Low-corruption enemies
- Solo combat situations

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | You control, they devastate Vulnerable targets |
| **Atgeir-Wielder** | Combined positioning control |
| **Rust-Witch** | Corruption exploitation synergy |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Solo damage builds** | Less value from control setup |
| **Fear-based builds** | Enemies may flee before controlled |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Basic control online |
| 6-10 | High | Corruption scaling kicks in |
| 11-15 | Very High | Punish the Helpless multiplier |
| 16+ | Extreme | Master of Puppets + Corruption Bomb |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 6/10 | Through multipliers, not base damage |
| Survivability | 5/10 | Mid-range fighter |
| Support | 8/10 | Excellent setup for allies |
| Control | 10/10 | Best-in-class positioning control |
| Utility | 6/10 | Trap expertise |

---

## Voice Guidance

### Tone Profile
- Cold, calculating, predatory
- Speaks of enemies as "pieces" or "puppets"
- Darkly satisfied when control works

### Example Quotes (NPC Flavor Text)
- *"You think you choose where to stand? That's adorable."*
- *"The more corrupt they are, the easier they dance."*
- *"Hold still. This works better when you can't move."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement [Rooted] status effect
- [ ] Implement [Slowed] status effect
- [ ] Add Pull/Push positioning mechanics

### Phase 2: Core Abilities
- [ ] Implement Netting Shot root application
- [ ] Implement Grappling Hook Toss pull
- [ ] Add [Disoriented] status effect

### Phase 3: Advanced Systems
- [ ] Implement corruption scaling for control
- [ ] Implement [Seized] status (machine-specific)
- [ ] Add Punish the Helpless damage multiplier

### Phase 4: Capstone
- [ ] Implement [Vulnerable] on displacement
- [ ] Implement Corruption Bomb AoE
- [ ] Add corruption purge mechanic

### Phase 5: Polish
- [ ] Test control stacking interactions
- [ ] Balance corruption scaling values
- [ ] Verify damage multiplier combinations

---

## Testing Requirements

### Unit Tests
- Control effect duration with Pragmatic Preparation
- Corruption scaling success rates
- Punish the Helpless damage calculation
- [Seized] application rates

### Integration Tests
- Full combat with control chains
- Party synergy with damage dealers
- Corruption Bomb targeting and damage

### Manual QA
- Verify control feels satisfying
- Test positioning manipulation
- Confirm corruption scaling is noticeable

---

## Logging Requirements

### Event Templates

```
OnControlApply:
  "[Hlekkr-Master] applies {Status} to {Enemy} for {Duration} turns"

OnCorruptionBonus:
  "Snag the Glitch: +{Bonus}% control success vs {CorruptionLevel} corruption"

OnPunishDamage:
  "Punish the Helpless: Damage doubled ({Base} → {Final})"

OnCorruptionBomb:
  "CORRUPTION BOMB! {Damage} Psychic damage to all enemies!"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skirmisher Archetype](../../archetypes/skirmisher.md) | Parent archetype |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Rooted Status](../../../04-systems/status-effects/rooted.md) | Control effect |
| [Slowed Status](../../../04-systems/status-effects/slowed.md) | Control effect |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
