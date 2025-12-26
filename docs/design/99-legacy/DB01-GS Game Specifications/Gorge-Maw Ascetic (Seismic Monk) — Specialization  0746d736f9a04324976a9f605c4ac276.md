# Gorge-Maw Ascetic (Seismic Monk) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-GORGEMAWASCETIC-v5.0
Mechanical Role: Controller/Debuffer, Tank/Durability
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-Type: Combat
Sub-item: Rooted Stance (Rooted%20Stance%203886a13aa6d34278b2173682818ba0bf.md), Tremor Strike (Tremor%20Strike%208359853fad5c44aea4f9d7bc1caec442.md), Stone Skin (Stone%20Skin%20bb79e909f82847d1815ce8f6748b67d5.md), Earthshaker (Earthshaker%20cecb1efb016046679085c2118733dc1c.md), Fissure (Fissure%203a7d710bf6964c029d6d3d0bb1f98e6e.md), Seismic Sense (Seismic%20Sense%201d2886e847594ab0a292b20eee96bf22.md), Avalanche (Avalanche%200cf51506e5b849579c0a7a3b9a5bccc8.md), Mountain's Endurance (Mountain's%20Endurance%2014bd45fc9bb04d48bccd6e30d7850824.md), Tectonic Fury (Tectonic%20Fury%204e7fc6c1942f4ab09acf552657c37e12.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

# Gorge-Maw Ascetic (Seismic Monk) — Specialization Specification v5.0

## I. Core Philosophy: The Earth Remembers

The Gorge-Maw Ascetic is the Warrior specialization that channels the world's buried fury. They are seismic monks—disciplined practitioners who have learned to read the tremors beneath their feet and return them tenfold. Where others see solid ground, the Gorge-Maw sees potential energy waiting to be unleashed.

**The mountain does not strike twice. Once is enough.**

### Core Identity

| Attribute | Value |
| --- | --- |
| Archetype | Warrior |
| Path Type | Coherent |
| Mechanical Role | Controller/Debuffer + Tank/Durability |
| Primary Attribute | STURDINESS |
| Resource System | Stamina + Resonance |
| Trauma Economy Risk | None |

### Core Fantasy Delivered

You are the earthquake given form. In a world where the ground itself is fractured by the Glitch, you have learned to speak to those fractures—to amplify them, direct them, weaponize them. Your strikes don't just damage; they reshape the battlefield. Enemies find the earth itself rising against them, while you stand immovable at the center of the chaos you've created.

---

## II. Design Pillars

### A. Resonance Resource System

The Gorge-Maw builds **Resonance** through grounded combat, then releases it in devastating seismic effects.

**Resonance Mechanics:**

- **Maximum:** 100 Resonance
- **Generation:**
    - Melee attack while in [Rooted Stance]: +15
    - Taking damage while grounded: +10
    - Standing on [Rubble] or [Unstable Ground]: +5/turn
- **Spends:**
    - Tremor abilities: -20 to -40
    - Earthshaker: -50
    - Tectonic Fury: -80
- **Decay:** -5 at end of turn if not in contact with ground

**Grounding Requirement:** Most Gorge-Maw abilities require contact with solid ground. Flying, levitating, or standing on liquid surfaces breaks Resonance generation.

### B. Terrain Manipulation

The Gorge-Maw reshapes the battlefield:

- Creates [Rubble Piles] (difficult terrain, cover, God-Sleeper synergy)
- Creates [Unstable Ground] (triggers on movement)
- Creates [Fissures] (blocks movement, damages crossers)

### C. The Immovable Center

The Gorge-Maw fights from a position of absolute stability:

- High Soak and HP
- Resistance to [Push], [Pull], [Knocked Down]
- Low mobility but incredible staying power

---

## III. The Skill Tree: The Path of Stone

### Tier 1 (Foundational Grounding)

*Prerequisite: Unlock Gorge-Maw Ascetic Specialization (10 PP)*

**Rooted Stance (Passive)**

> *"Plant your feet as the mountain plants its roots; let no force move what has chosen to stand."*
> 
- **Cost:** 3 PP
- **Effect:** While you have not moved this turn, gain +2 Soak and resistance to [Push] and [Pull] effects.
- **Rank 2 (20 PP):** +3 Soak, immunity to [Knocked Down]
- **Rank 3 (Capstone):** +4 Soak, generate 10 Resonance at start of each turn you remain stationary

**Tremor Strike (Active)**

> *"Channel the earth's restless energy through your weapon; the ground shudders in sympathy."*
> 
- **Cost:** 3 PP | 35 Stamina | Standard Action
- **Target:** Single enemy (melee range)
- **Effect:** STURDINESS attack dealing 2d10 Physical damage. Target must make STURDINESS check (DC 12) or become [Staggered] for 1 round. Generates 15 Resonance.
- **Rank 2 (20 PP):** 3d10 damage, DC 14, affects all enemies adjacent to target
- **Rank 3 (Capstone):** 4d10 damage, DC 16, [Staggered] lasts 2 rounds

**Stone Skin (Passive)**

> *"The flesh learns from the stone; it hardens where the blows fall most."*
> 
- **Cost:** 3 PP
- **Effect:** Gain +1 base Soak. When you take Physical damage, gain an additional +1 Soak until end of your next turn (stacks up to +3).
- **Rank 2 (20 PP):** +2 base Soak, temporary stacks up to +4
- **Rank 3 (Capstone):** +3 base Soak, temporary stacks up to +5, stacks persist for 2 rounds

### Tier 2 (Advanced Seismics)

*Prerequisite: 8 PP spent in Gorge-Maw Ascetic tree*

**Earthshaker (Active)**

> *"Strike the fault line; let the earth speak your fury in a language of broken stone."*
> 
- **Cost:** 4 PP | 50 Stamina + 30 Resonance | Standard Action
- **Target:** 3x3 tile area within 4 tiles
- **Effect:** STURDINESS attack dealing 3d8 Physical damage to all enemies in area. Creates [Rubble Pile] terrain in the center tile. All enemies must make FINESSE check (DC 13) or be [Knocked Down].
- **Rank 2 (20 PP):** 4d8 damage, 4x4 area, creates 2 [Rubble Piles]
- **Rank 3 (Capstone):** 5d8 damage, DC 15, [Rubble Piles] count as [Scrap Piles] for God-Sleeper abilities

**Fissure (Active)**

> *"Open the earth's wounds; let them swallow the unwary."*
> 
- **Cost:** 4 PP | 45 Stamina + 20 Resonance | Standard Action
- **Target:** Line of tiles (up to 5 tiles long, 1 tile wide)
- **Effect:** Creates a [Fissure] along the line. Enemies in the line take 2d6 Physical damage. [Fissure] terrain blocks movement; enemies attempting to cross take 1d8 Physical damage and must make FINESSE check (DC 12) or fall in and become [Restrained] for 1 round.
- **Rank 2 (20 PP):** 3d6 initial damage, DC 14, line extends to 7 tiles
- **Rank 3 (Capstone):** 4d6 damage, [Fissure] lasts until end of combat, enemies who fall in take 2d8 damage

**Seismic Sense (Passive)**

> *"The ground whispers of footfalls yet to come; listen, and you will never be surprised."*
> 
- **Cost:** 4 PP
- **Effect:** You cannot be surprised while standing on solid ground. Gain +1 die to Initiative rolls.
- **Rank 2 (20 PP):** Detect invisible enemies within 4 tiles while grounded, +2 Initiative
- **Rank 3 (Capstone):** Automatically detect all enemies within 6 tiles (including through walls), +3 Initiative

### Tier 3 (Mastery of the Deep)

*Prerequisite: 16 PP spent in Gorge-Maw Ascetic tree*

**Avalanche (Active)**

> *"When the mountain moves, nothing stands before it."*
> 
- **Cost:** 5 PP | 55 Stamina + 40 Resonance | Standard Action
- **Target:** Cone (5 tiles long, 3 tiles wide at end)
- **Effect:** STURDINESS attack dealing 5d10 Physical damage to all enemies in cone. All enemies are [Pushed] 2 tiles away from you. Creates [Unstable Ground] throughout the affected area.
- **Rank 2 (20 PP):** 6d10 damage, [Push] 3 tiles, enemies who collide with obstacles take +2d6 damage
- **Rank 3 (Capstone):** 7d10 damage, enemies are also [Staggered] for 2 rounds

**Mountain's Endurance (Passive)**

> *"You have become the bedrock; you endure when all else crumbles."*
> 
- **Cost:** 5 PP
- **Effect:** Once per combat, when you would be reduced to 0 HP, instead drop to 1 HP and become immune to damage until end of your next turn. During this time, you cannot move.
- **Rank 2 (20 PP):** Triggers at 20% HP instead of 0 HP, heal 15 HP when immunity ends
- **Rank 3 (Capstone):** Can trigger twice per combat, heal 25 HP, generate 50 Resonance when triggered

### Capstone (Ultimate Expression)

*Prerequisite: 24 PP in tree + any Tier 3 ability*

**Tectonic Fury (Active)**

> *"You have learned to speak the old language—the tongue of fault lines and magma chambers. The earth answers your call with apocalyptic violence."*
> 
- **Cost:** 6 PP | 60 Stamina + 80 Resonance | Standard Action
- **Duration:** 3 rounds
- **Effect:** Enter [Tectonic Fury] state:
    - All tiles within 3 tiles of you become [Unstable Ground]
    - At start of each of your turns, all enemies on [Unstable Ground] take 3d6 Physical damage and must make STURDINESS check (DC 14) or be [Knocked Down]
    - Your Resonance generation is doubled
    - You cannot move, but gain +5 Soak
- **Rank 2 (20 PP):** 4d6 damage, DC 16, effect radius 4 tiles
- **Rank 3 (Capstone):** 5d6 damage, can move (slowly: 2 tiles/turn), [Unstable Ground] persists for 1 round after Tectonic Fury ends

---

## IV. Systemic Integration

### Strategic Role

**The Immovable Anchor:** The Gorge-Maw creates a zone of absolute control. Enemies cannot easily approach, cannot easily escape, and cannot ignore the ground beneath their feet. They complement mobile strikers by locking down the battlefield.

### Terrain Effects Reference

| Terrain Type | Effect |
| --- | --- |
| **[Rubble Pile]** | Difficult terrain, provides half cover, counts as [Scrap Pile] for God-Sleeper (Rank 3 Earthshaker) |
| **[Unstable Ground]** | Movement triggers FINESSE check (DC 12) or fall [Prone]; enemies starting turn here take 1d4 damage |
| **[Fissure]** | Blocks movement; crossing requires FINESSE check or [Restrained] + damage |

### Party Synergies

| Specialization | Synergy |
| --- | --- |
| **God-Sleeper Cultist** | Earthshaker creates [Rubble/Scrap Piles] for Animate Scrap |
| **Hólmgangr** | Fissures isolate duel targets |
| **Strandhögg** | Unstable Ground + Pace synergy (enemies slow, raider fast) |
| **Skjaldmær** | Dual anchor—two immovable points create impassable wall |
| **Ranged Specialists** | Terrain control keeps enemies at distance |

### Vulnerabilities

- **Mobility:** Slow, commits to positions
- **Aerial Enemies:** Flying enemies ignore terrain effects
- **Indoor/Artificial Terrain:** Some environments may not have "ground" for Resonance
- **Water/Liquid:** Cannot generate Resonance while in water

---

## V. Tactical Applications

### The Seismic Protocol

1. **Read the ground:** Identify fault lines, weak points, chokepoints
2. **Establish position:** Plant yourself where control matters most
3. **Build Resonance:** Tremor Strike and taking hits fuel your reserves
4. **Reshape terrain:** Earthshaker and Fissure create your battlefield
5. **Hold the line:** Mountain's Endurance ensures you outlast
6. **Unleash cataclysm:** Tectonic Fury when the moment demands

### Field Notes

- Do not chase. Let them come to you—then ensure they cannot leave.
- The best Fissure separates the dangerous from the vulnerable.
- Partner with mobile allies who can exploit the chaos you create.
- Earthshaker at the start of combat shapes the entire engagement.

---

## VI. Related Documents

- **Archetype:** Warrior Archetype Foundation
- **Key Synergy:** God-Sleeper Cultist ([Rubble Pile] → [Scrap Pile])
- **Related Specializations:** Skjaldmær (defensive anchor), Iron-Bane (anti-machine zone control)