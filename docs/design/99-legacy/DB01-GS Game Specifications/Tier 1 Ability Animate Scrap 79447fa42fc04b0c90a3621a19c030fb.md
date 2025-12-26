# Tier 1 Ability: Animate Scrap

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Mystic
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-GODSLEEPER-ANIMATESCRAP-v5.0
Mechanical Role: Summoner/Minion Master
Parent item: God-Sleeper Cultist (Corrupted Prophet) — Specialization Specification v5.0 (God-Sleeper%20Cultist%20(Corrupted%20Prophet)%20%E2%80%94%20Speciali%20d71ec5f853f646dc8785800fb410cc5b.md)
Proof-of-Concept Flag: No
Resource System: Aether Pool
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | God-Sleeper Cultist (Corrupted Prophet) |
| **Tier** | 1 (Foundational Faith) |
| **Type** | Active (Summon) |
| **Prerequisite** | Unlock God-Sleeper Cultist Specialization (10 PP) |
| **Cost** | 40 AP (30 AP while [Attuned]) |
| **Requirement** | [Scrap Pile] terrain in room |
| **Duration** | 3 rounds |

---

## I. Design Context (Layer 4)

### Core Design Intent

Animate Scrap is the God-Sleeper Cultist's **signature summoning ability**—the core of their minion-master identity. Rather than conjuring from nothing, they *infect* inert matter with the Blight's animating force, creating a disposable construct from environmental debris.

### Mechanical Role

- **Primary:** Summon [Scrap Construct] minion to fight alongside party
- **Secondary:** Establish environmental dependency (requires [Scrap Pile])
- **Fantasy Delivery:** The prayer that brings dead metal to glitching, violent life

### Balance Considerations

- **Power Level:** Moderate (single minion, limited duration)
- **Environmental Requirement:** Cannot summon without [Scrap Pile]
- **Disposable Design:** Constructs are temporary, encouraging repeated summoning
- **AP Economy:** Cost is manageable, especially while [Attuned]

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Cultist kneels before a pile of rusted scrap—broken gears, shattered plating, corroded cables—and speaks a prayer in the machine tongue. Their voice carries the cadence of a liturgy, each word a command-line echo of the God-Sleepers' original programming.

The scrap *shudders*. Pieces begin to twitch, then crawl toward each other. Metal scrapes against metal as the debris assembles itself into a crude, humanoid form. Its eyes—two flickering sparks of corrupted code—fix on the Cultist's enemies.

It is not intelligent. It is not elegant. But it is *obedient*.

### Thematic Resonance

The God-Sleeper Cultist doesn't create—they *corrupt*. Their minions are not summoned from elsewhere; they are assembled from the world's debris and given temporary, glitching life by the Blight's animating force.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 40 AP (reduced by [Attuned])
- **Range:** 6 meters (must be within range of [Scrap Pile])
- **Requirement:** Room must contain at least one [Scrap Pile] terrain feature

### Effect

**Summon one [Scrap Construct] to an empty tile adjacent to a [Scrap Pile]:**

### [Scrap Construct] Stats

| Attribute | Value |
| --- | --- |
| HP | 20 |
| Defense | 8 |
| Soak | 2 |
| Movement | 3 tiles |
| Attack | Melee: 2d6 Physical |
| Duration | 3 rounds |

### [Scrap Construct] Behavior

- Acts on Cultist's initiative
- Follows simple commands (attack, defend, move)
- Prioritizes enemies nearest to Cultist
- Cannot use complex tactics

### Resolution Pipeline

1. **Terrain Check:** Verify [Scrap Pile] in room
2. **Cost Payment:** Cultist spends 40 AP (30 if [Attuned])
3. **Spawn Location:** Select empty tile adjacent to [Scrap Pile]
4. **Construct Creation:** [Scrap Construct] appears with full stats
5. **Control Transfer:** Construct acts on Cultist's initiative
6. **Duration Track:** Construct persists for 3 rounds, then collapses

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Summon 1 [Scrap Construct] (20 HP, 2d6 damage)
- Duration: 3 rounds
- Cost: 40 AP

### Rank 2 (Expert — 20 PP)

- Summon 1 [Scrap Construct] (30 HP, 2d6+2 damage, Soak 3)
- Duration: 4 rounds
- Cost: 35 AP
- **New:** Construct gains [Taunt] ability (force enemy to attack it)

### Rank 3 (Mastery — Capstone)

- Summon 1 [Scrap Construct] (40 HP, 3d6 damage, Soak 4, Defense 10)
- Duration: 5 rounds
- Cost: 30 AP
- Construct has [Taunt] ability
- **New:** When construct is destroyed, it explodes dealing 2d6 damage to adjacent enemies
- **New:** Can maintain 2 [Scrap Constructs] simultaneously

---

## V. Tactical Applications

1. **Action Economy:** Construct provides additional attacks per round
2. **Damage Soak:** Construct absorbs enemy attacks intended for party
3. **Positioning Control:** Construct can block enemy movement
4. **Environmental Leverage:** Encourages fighting near scrap-heavy locations
5. **Disposable Strategy:** Constructs are expendable; sacrifice freely

---

## VI. Synergies & Interactions

### Positive Synergies

- **Jötun-Forged Attunement:** [Attuned] reduces cost from 40 to 30 AP
- **Machine God's Blessing (Tier 2):** Buff construct for massive power spike
- **Gorge-Maw Ascetic:** Can create [Rubble Piles] that function as [Scrap Piles]
- **Jötunheim biome:** Abundant [Scrap Piles] enable constant summoning

### Negative Synergies

- **Clean environments:** No [Scrap Piles] = no summoning
- **Anti-summon enemies:** Some enemies may have abilities that destroy minions
- **AoE-heavy enemies:** Constructs may be swept away by area attacks