---
id: SPEC-SCRAP-TINKER-26001
title: "Scrap-Tinker"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Scrap-Tinker

## Identity

| Property | Value |
|----------|-------|
| **Name** | Scrap-Tinker |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Role** | Crafter / Pet Controller |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource** | Stamina + Scrap Materials |
| **Trauma Risk** | None |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 3 |
| **Maximum Corruption** | 100 |
| **Required Quest** | None |

---

## Design Philosophy

**Tagline:** "Salvage and innovation — craft gadgets, deploy drones, modify weapons"

**Core Fantasy:** You are the scavenger-engineer who sees treasure in ruins. Where others see broken machines, you see repurposable parts. You salvage corrupted technology, reverse-engineer pre-Glitch devices, and cobble together functional gadgets from scrap.

You craft drones for reconnaissance, bombs for crowd control, and weapon mods for allies. You're the tinkerer who proves that in a crashed system, the best debugger is the one who can rebuild from the ground up.

**Mechanical Identity:**
1. **Scrap Material Economy** - Collect and spend Scrap Materials to craft gadgets
2. **Gadget Deployment** - Flash bombs, shock mines, and tactical devices
3. **Pet/Minion Control** - Scout Drones and the ultimate Scrap Golem
4. **Crafting Specialization** - Weapon modifications and superior quality items

**Gameplay Feel:** Technical support specialist with deployable pets and tactical gadgets. Masters of preparation and battlefield control.

---

## Core Mechanics

### Scrap Materials System

**Scrap Generation:**

| Source | Scrap Gained |
|--------|--------------|
| Defeated Mechanical enemies | +50-100% (by rank) |
| Loot containers | +50-100% (by rank) |
| Salvaging broken equipment | Variable |
| Automated Scavenging (post-combat) | 10-15 (by rank) |
| Scout Drone scavenging | +5 per combat |
| Scrap Golem scavenging | +10 per combat |
| Expedition start (Rank 3) | 20 Scrap |

**Scrap Costs:**

| Gadget | Base Cost | With Efficient Assembly |
|--------|-----------|------------------------|
| Flash Bomb | Standard | Reduced |
| Shock Mine | Standard | Reduced |
| Scout Drone | 15 Scrap | 8-15 Scrap |
| Weapon Modification | 20-25 Scrap | 10-18 Scrap |
| Scrap Golem | 50 Scrap | 25-50 Scrap |

### Quality Tiers

| Quality | Effect Bonus | Chance (by rank) |
|---------|--------------|------------------|
| **Standard** | Base effects | Default |
| **Masterwork** | Enhanced effects | 15% → 25% → 40% |
| **Prototype** | Superior effects | 0% → 0% → 10% |

---

## Rank Progression

### Tier Unlock Requirements

| Tier | PP Cost | Starting Rank | Max Rank | Progression |
|------|---------|---------------|----------|-------------|
| Tier 1 | 3 PP each | Rank 1 | Rank 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| Tier 2 | 4 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Tier 3 | 5 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Capstone | 6 PP | Rank 1 | Rank 3 | 1→2→3 (tree-based) |

### Total Investment: 40 PP (3 unlock + 37 abilities)

---

## Ability Tree

```
                    TIER 1: FOUNDATION
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Master Scavenger]  [Deploy Flash Bomb]  [Salvage Expertise]
    (Passive)           (Active)            (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Deploy Scout     [Deploy Shock Mine]  [Weapon Modification]
    Drone]             (Active)            (Active)
   (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
   [Automated             [Efficient Assembly]
    Scavenging]                (Passive)
     (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
               [Deploy Scrap Golem]
                    (Active)
```

### Ability Index

| ID | Name | Tier | Type | PP | Key Effect |
|----|------|------|------|-----|------------|
| 26001 | Master Scavenger | 1 | Passive | 3 | +Scrap from enemies/containers |
| 26002 | Deploy Flash Bomb | 1 | Active | 3 | AoE [Blinded] |
| 26003 | Salvage Expertise | 1 | Passive | 3 | +Crafting, Masterwork chance |
| 26004 | Deploy Scout Drone | 2 | Active | 4 | Recon drone pet |
| 26005 | Deploy Shock Mine | 2 | Active | 4 | Trap: damage + stun |
| 26006 | Weapon Modification | 2 | Active | 4 | Permanent weapon enhancements |
| 26007 | Automated Scavenging | 3 | Passive | 5 | Auto-collect Scrap post-combat |
| 26008 | Efficient Assembly | 3 | Passive | 5 | Reduced costs, faster crafting |
| 26009 | Deploy Scrap Golem | 4 | Active | 6 | Powerful combat pet |

---

## Situational Power Profile

### Optimal Conditions
- Pre-fight preparation time (placing mines, crafting)
- Multi-encounter dungeons (Scrap accumulation)
- Mechanical enemy presence (+Scrap, System weaknesses)
- Parties needing utility support
- Open battlefields for drone scouting

### Weakness Conditions
- Ambush situations (no prep time)
- Single-encounter missions (limited Scrap)
- Anti-construct enemies
- Cramped spaces limiting drone movement
- Resource-depleted early expeditions

---

## Party Synergies

### Positive Synergies
- **Warrior archetypes** - Weapon Modifications enhance their damage
- **Ranged specialists** - Scout Drone provides target marking
- **Tanks** - Scrap Golem adds to frontline presence
- **Damage dealers** - Flash Bomb sets up alpha strikes

### Negative Synergies
- **Stealth-focused parties** - Golem is not subtle
- **Speed compositions** - Setup time slows pace
- **Other resource-intensive builds** - Competing for Scrap equivalent

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 3-5 | Medium | Building Scrap economy |
| 6-10 | High | Full gadget deployment |
| 11-15 | Very High | Golem + optimized loadout |
| 16+ | Extreme | Economy mastery |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★☆☆☆ | Not specialized |
| AoE Damage | ★★★★☆ | Flash Bomb, Shock Mine |
| Control | ★★★★★ | Blind, Stun, Root |
| Support | ★★★★★ | Drone recon, weapon mods |
| Survivability | ★★★☆☆ | Golem bodyguard |

---

## Voice Guidance

### Tone Profile
- Enthusiastic about salvage and technology
- Practical problem-solver
- Sees potential in everything broken
- Confident in engineering skills

### Example Quotes
- "Broken? No, no—this is a learning opportunity."
- "With the right parts, I can build anything."
- "My golem may not be pretty, but it hits like a forge hammer."
- "Everyone underestimates junk. That's their mistake."

---

## Phased Implementation Guide

### Phase 1: Core Framework
- [ ] Implement Scrap Materials resource system
- [ ] Implement quality tiers (Standard, Masterwork, Prototype)
- [ ] Create gadget inventory system
- [ ] Basic crafting interface

### Phase 2: Gadget Deployment
- [ ] Flash Bomb AoE targeting
- [ ] Shock Mine trap placement
- [ ] Hidden object detection for mines
- [ ] Consumable item integration

### Phase 3: Pet System
- [ ] Scout Drone spawning and stats
- [ ] Drone movement and vision system
- [ ] Target marking mechanic
- [ ] Self-destruct ability

### Phase 4: Scrap Golem
- [ ] Golem spawning with full stat block
- [ ] Multi-ability pet commands (Slam, Defend, Repair, Detonate)
- [ ] Expedition-long duration tracking
- [ ] Pet AI for autonomous actions

### Phase 5: Polish
- [ ] Scrap Materials tracker UI
- [ ] Deployed pets panel
- [ ] Crafting quality indicators
- [ ] Rank visual indicators

---

## Testing Requirements

### Unit Tests
- Scrap generation calculations
- Quality tier chance rolls
- Gadget effect applications
- Pet stat calculations

### Integration Tests
- Post-combat Scrap collection
- Drone vision fog-of-war
- Mine trigger detection
- Golem action economy

### Manual QA Checklist
- [ ] Scrap counter updates correctly
- [ ] Flash Bomb blinds correct area
- [ ] Scout Drone reveals hidden enemies
- [ ] Shock Mine triggers on enemy movement
- [ ] Scrap Golem follows commands

---

## Logging Requirements

### Combat Events
```
"Master Scavenger: +{amount} Scrap from {source}"
"Deploy Flash Bomb: {count} enemies in 3x3 area. DC {dc} WILL save."
"[Enemy] is Blinded for {duration} turns"
"Deploy Scout Drone: 15 HP, 2 Armor. Vision radius: 7"
"Scout Drone: Marked {target}. Allies gain +1 to hit."
"Deploy Shock Mine: Trap placed at {location}"
"Shock Mine triggered! {damage} Lightning damage to {target}"
"Deploy Scrap Golem: 40 HP, 6 Armor. Ready for commands."
"Scrap Golem SLAM: {damage} Physical damage to {target}"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Adept Archetype](../../archetypes/adept.md) | Parent archetype |
| [Crafting System](../../../04-systems/crafting/crafting-overview.md) | Crafting mechanics |
| [Blinded Status](../../../04-systems/status-effects/status-effects-overview.md) | Status effect details |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
