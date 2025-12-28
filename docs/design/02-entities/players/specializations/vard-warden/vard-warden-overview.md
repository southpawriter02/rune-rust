---
id: SPEC-VARD-WARDEN-28001
title: "Vard-Warden"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Vard-Warden

**Archetype:** Mystic | **Path:** Coherent | **Role:** Defensive Caster / Battlefield Controller

> *"Firewall Architect"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Vard-Warden |
| **Archetype** | Mystic |
| **Path Type** | Coherent |
| **Role** | Defensive Caster / Battlefield Controller |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource** | Aether Pool (AP) |
| **Trauma Risk** | None |

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
*"Not while I stand."*

### Core Fantasy
You are the firewall architect who creates pockets of stable reality in a corrupted world. Where chaos spreads, you inscribe barriers of solidified Aether. Where Blight advances, you consecrate ground. You don't just protect—you control the battlefield itself, forcing enemies to fight on your terms.

### Mechanical Identity
1. **Physical Barriers**: Create destructible walls that block movement and line-of-sight
2. **Zone Control**: Consecrate areas that heal allies and damage Blighted/Undying
3. **Ally Protection**: Buff allies with shields and stress resistance
4. **Reaction Defense**: Ultimate ability prevents fatal damage as a reaction

### Gameplay Feel
Methodical, protective, controlling. You shape the battlefield before enemies can act. Combat becomes a puzzle of positioning where your walls and zones dictate the flow of battle.

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
                    [VARD-WARDEN PROTECTION MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ SANCTIFIED      │ │ RUNIC BARRIER   │ │ CONSECRATE      │
│ RESOLVE I       │ │                 │ │ GROUND          │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ NO RANKS        │ │ Create wall     │ │ Create zone     │
│ +Push/Pull res  │ │ (30-50 HP)      │ │ heal + damage   │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ RUNE OF         │ │ REINFORCE       │ │ WARDEN'S        │
│ SHIELDING       │ │ WARD            │ │ VIGIL           │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Active          │ │ Active          │ │ Passive         │
│ Buff ally       │ │ Heal barrier/   │ │ NO RANKS        │
│ +Soak +corrupt  │ │ boost zone      │ │ Row Stress res  │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ GLYPH OF      AEGIS OF      │
              │ SANCTUARY     SANCTITY      │
              │ [Tier 3]      [Tier 3]      │
              │ Party TempHP  Barrier       │
              │ + Stress Imm  reflection    │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │      INDOMITABLE BASTION    │
              │          [Capstone]         │
              │   Reaction: Negate Fatal    │
              │       Once Per Expedition   │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------|
| 28010 | [Sanctified Resolve I](abilities/sanctified-resolve-i.md) | 1 | Passive | 3 | +1d10 WILL vs Push/Pull (NO RANKS) |
| 28011 | [Runic Barrier](abilities/runic-barrier.md) | 1 | Active | 3 | Create wall (30-50 HP) |
| 28012 | [Consecrate Ground](abilities/consecrate-ground.md) | 1 | Active | 3 | Create healing/damage zone |
| 28013 | [Rune of Shielding](abilities/rune-of-shielding.md) | 2 | Active | 4 | Buff ally +Soak +corruption resist |
| 28014 | [Reinforce Ward](abilities/reinforce-ward.md) | 2 | Active | 4 | Heal barrier or boost zone |
| 28015 | [Warden's Vigil](abilities/wardens-vigil.md) | 2 | Passive | 4 | Row-wide Stress resistance (NO RANKS) |
| 28016 | [Glyph of Sanctuary](abilities/glyph-of-sanctuary.md) | 3 | Active | 5 | Party temp HP + Stress immunity |
| 28017 | [Aegis of Sanctity](abilities/aegis-of-sanctity.md) | 3 | Passive | 5 | Barrier reflection + zone cleanse |
| 28018 | [Indomitable Bastion](abilities/indomitable-bastion.md) | 4 | Reaction | 6 | Negate fatal damage, create barrier |

---

## Core Mechanics

### Runic Barriers

Physical walls of solidified Aether:

```
Barrier Properties:
- HP: 30/40/50 (by rank)
- Duration: 2/3/4 turns (by rank)
- Blocks: Movement, line-of-sight, projectiles
- Destruction: Rank 3 deals 2d6 Arcane AoE
```

### Sanctified Ground Zones

Consecrated areas that affect friend and foe:

| Target | Effect |
|--------|--------|
| Allies | Heal 1d6-2d6 HP per turn |
| Blighted/Undying | Take 1d6-2d6 Arcane damage per turn |
| All Allies (R3) | +1d10 Resolve while in zone |

### Reaction System

Indomitable Bastion uses a **reaction** trigger:
- Activates when ally would take fatal damage
- No action cost (immediate response)
- Once per expedition limitation

---

## Situational Power Profile

### Optimal Conditions
- Defensive scenarios
- Chokepoint control
- Protecting key allies
- Fighting Blighted/Undying enemies

### Weakness Conditions
- Highly mobile enemies
- Barrier-bypassing abilities
- Offensive pressure requirements
- Solo combat

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | Keep them alive through rage |
| **Bone-Setter** | Combined healing power |
| **Atgeir-Wielder** | Barrier + phalanx control |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Mobility-focused** | Barriers may block allies |
| **Aggressive pushers** | May want enemies close |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Basic barrier and zone |
| 6-10 | High | Rune of Shielding protection |
| 11-15 | Very High | Aegis reflection + cleanse |
| 16+ | Extreme | Indomitable Bastion saves lives |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 3/10 | Zone damage only |
| Survivability | 10/10 | Ultimate defensive caster |
| Support | 9/10 | Healing zones, ally buffs |
| Control | 8/10 | Barrier positioning |
| Utility | 7/10 | Corruption resistance, cleansing |

---

## Voice Guidance

### Tone Profile
- Calm, resolute, protective
- Speaks of walls and wards
- Unshakeable confidence

### Example Quotes (NPC Flavor Text)
- *"This ground is mine. You will not pass."*
- *"The ward holds. It always holds."*
- *"Behind my barrier, there is only safety. Before it? Only death."*

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement barrier HP tracking
- [ ] Add barrier line-of-sight blocking
- [ ] Implement zone area effects

### Phase 2: Core Abilities
- [ ] Implement Runic Barrier creation
- [ ] Implement Consecrate Ground zones
- [ ] Add Soak buff system

### Phase 3: Advanced Systems
- [ ] Implement reaction trigger system
- [ ] Add barrier reflection (Aegis)
- [ ] Implement zone cleansing

### Phase 4: Capstone
- [ ] Implement Indomitable Bastion reaction
- [ ] Add fatal damage interception
- [ ] Implement once-per-expedition tracking

### Phase 5: Polish
- [ ] Add barrier visual indicators
- [ ] Implement zone ground effects
- [ ] Test barrier/zone interactions

---

## Testing Requirements

### Unit Tests
- Barrier HP and destruction
- Zone healing/damage per turn
- Reaction trigger conditions
- Once-per-expedition limitation

### Integration Tests
- Full combat with barrier blocking
- Zone stacking behavior
- Indomitable Bastion save

### Manual QA
- Verify barrier feels protective
- Test zone visual clarity
- Confirm capstone feels heroic

---

## Logging Requirements

### Event Templates

```
OnBarrierCreate:
  "Runic Barrier created ({HP} HP, {Duration} turns)"

OnBarrierDamage:
  "Runic Barrier takes {Damage} damage ({Remaining}/{Max} HP)"

OnZoneHeal:
  "[Sanctified Ground] heals {Ally} for {Amount} HP"

OnIndomitableBastion:
  "INDOMITABLE BASTION! Fatal damage negated on {Ally}!"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Mystic Archetype](../../archetypes/mystic.md) | Parent archetype |
| [Aether Resource](../../../01-core/resources/aether.md) | Primary resource |
| [Stress System](../../../01-core/resources/stress.md) | Stress immunity mechanics |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
