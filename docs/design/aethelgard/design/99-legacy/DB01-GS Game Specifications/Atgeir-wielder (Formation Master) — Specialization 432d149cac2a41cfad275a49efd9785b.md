# Atgeir-wielder (Formation Master) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-ATGEIRWIELDER-v5.0
Mechanical Role: Controller/Debuffer, Tank/Durability
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Action Points (AP), Stamina
Sub-Type: Combat
Sub-item: Tier 1 Ability: Formal Training I (Tier%201%20Ability%20Formal%20Training%20I%2037816d407cf144449e4427784babfaa6.md), Tier 1 Ability: Skewer (Tier%201%20Ability%20Skewer%20c5a3a1723e184f6d854725ab062e01e1.md), Tier 1 Ability: Disciplined Stance (Tier%201%20Ability%20Disciplined%20Stance%208c114a68acbf4406b9caf9dd384ba628.md), Tier 2 Ability: Guarding Presence (Tier%202%20Ability%20Guarding%20Presence%20fdc5502e134f40478cbf315e1cd490ff.md), Tier 2 Ability: Hook and Drag (Tier%202%20Ability%20Hook%20and%20Drag%20a9a91c3bcfb848dbaef9aaf450f98473.md), Tier 2 Ability: Line Breaker (Tier%202%20Ability%20Line%20Breaker%20cf9a2336aaa1414d9a21cbe846859cdf.md), Tier 3 Ability: Brace for Charge (Tier%203%20Ability%20Brace%20for%20Charge%20c6fc2578e63f4a28a053889ee1c098c1.md), Tier 3 Ability: Unstoppable Phalanx (Tier%203%20Ability%20Unstoppable%20Phalanx%204bf1dec063404bda92910a0021fc5d89.md), Capstone Ability: Living Fortress (Capstone%20Ability%20Living%20Fortress%200739a44367bb4e60bac1272d004143bd.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## Core Identity

| Attribute | Value |
| --- | --- |
| **Specialization ID** | 12 (AtgeirWielder) |
| **Archetype** | Warrior |
| **Role** | Tactical Reach Combatant / Formation Anchor / Battlefield Controller |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina (standard Warrior resource) |
| **Trauma Economy Risk** | None (Coherent path) |
| **Unlock Cost** | 3 PP |
| **Minimum Legend** | 3 |

---

## I. Design Context (Layer 4)

### Core Design Intent

The Atgeir-wielder embodies **tactical discipline and formation warfare**. They are not wild berserkers or desperate duelists — they are the unbreachable anchor of any formation, the thinking person's warrior. Wielding a long polearm (the atgeir), they command the space around them, keeping enemies at a distance while creating opportunities for allies.

Their art is one of **precision, leverage, and logical battlefield control**. Where other Warriors fight with rage or faith, the Atgeir-wielder fights with geometry.

### The Three Pillars

| Pillar | Mechanic | Fantasy |
| --- | --- | --- |
| **[Reach]** | Attack Front Row from Back Row | Safe damage contribution through superior positioning |
| **Formation Breaking** | [Push]/[Pull] forced movement | Shatter enemy formations, expose vulnerabilities |
| **Defensive Anchor** | Stance abilities, movement immunity | The immovable center around which battles pivot |

### Player Fantasy

> *"You are the disciplined hoplite, the master of formation warfare. Wielding a long polearm, you command the space around you with tactical precision. Your [Reach] allows you to strike from safety while your Push and Pull effects shatter enemy formations. You are the immovable anchor that holds the line — the thinking warrior who controls where battles happen."*
> 

---

## II. Narrative Context (Layer 2)

### In-World Framing

In a world defined by the chaotic, glitching horror of the Runic Blight, the Atgeir-wielder is the **Eye of the Storm**. They are the thinking person's warrior, a master of tactical positioning who understands that the only way to survive a system crash is to enforce a zone of absolute, undeniable order.

Their fighting style is a **direct refutation of the Blight's chaos**. Where the Blight creates unpredictable movement and paradox, the Atgeir-wielder creates impassable lines and dictates the flow of battle with unerring precision.

### Thematic Resonance

The polearm is **logic made physical**. Every stance, every thrust, every sweep represents centuries of military tradition — the same disciplined formations that protected the ancients now adapted to protect the survivors of the broken world.

Their saga is one of **disciplined control**, of holding the line against a reality that is trying to tear itself apart.

---

## III. Mechanical Specification (Layer 3)

### The [Reach] Keyword

The Atgeir-wielder's signature mechanic allows attacks across the positioning grid:

| User Position | Normal Melee | With [Reach] |
| --- | --- | --- |
| **Front Row** | Front Row enemies | Front + Back Row enemies |
| **Back Row** | Cannot melee | Front Row enemies |

**Implementation**: [Reach] abilities bypass standard melee engagement rules when user is in Back Row.

### Forced Movement System

| Effect | Direction | Opposed Check |
| --- | --- | --- |
| **[Push]** | Front Row → Back Row | MIGHT vs STURDINESS |
| **[Pull]** | Back Row → Front Row | MIGHT vs STURDINESS |

**Resolution**: Attacker rolls MIGHT dice pool vs target's STURDINESS dice pool. On success, target is forcibly moved.

### Resource Economy

| Resource | Base | Regeneration | Notes |
| --- | --- | --- | --- |
| Stamina | 100 | 10/turn (base) | Formal Training adds +5/+7/+10 |

---

## IV. Ability Tree Structure

### Overview

```
TIER 1: FOUNDATION (3 PP each, Ranks 1→2→3)
├── Formal Training I (Passive) — Stamina regen, disruption resist
├── Skewer (Active) — [Reach] single-target attack
└── Disciplined Stance (Active) — Defensive stance, resist forced movement

TIER 2: ADVANCED (4 PP each, Ranks 2→3)
├── Hook and Drag (Active) — [Pull] back-row target to front
├── Line Breaker (Active) — AoE [Push] front-row enemies back
└── Guarding Presence (Passive) — Soak aura for adjacent allies

TIER 3: MASTERY (5 PP each, Ranks 2→3)
├── Brace for Charge (Active) — Counter-attack stance
└── Unstoppable Phalanx (Active) — Line-piercing attack

CAPSTONE (6 PP, Ranks 1→2→3)
└── Living Fortress (Passive) — Forced movement immunity, reactive Brace
```

### Rank Progression (Tree-Based)

| Tier | Starting Rank | → Rank 2 Trigger | → Rank 3 Trigger |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 | 2 Tier 2 abilities trained | Capstone trained |
| **Tier 2** | Rank 2 | — | Capstone trained |
| **Tier 3** | Rank 2 | — | Capstone trained |
| **Capstone** | Rank 1 | Tree progression | Full tree completion |

### PP Investment Milestones

| Milestone | Total PP | Abilities | Tier 1 Rank | Tier 2+ Rank |
| --- | --- | --- | --- | --- |
| Unlock Specialization | 3 PP | 0 | — | — |
| All Tier 1 | 12 PP | 3 | Rank 1 | — |
| 2× Tier 2 | 20 PP | 5 | **Rank 2** | Rank 2 |
| All Tier 2 | 24 PP | 6 | Rank 2 | Rank 2 |
| All Tier 3 | 34 PP | 8 | Rank 2 | Rank 2 |
| Capstone | 40 PP | 9 | **Rank 3** | **Rank 3** |

---

## V. Ability Summary

| ID | Ability | Tier | Type | Cost | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 1201 | Formal Training I | 1 | Passive | — | +Stamina regen, resist [Staggered] |
| 1202 | Skewer | 1 | Active | 40 Sta | [Reach] single-target Physical |
| 1203 | Disciplined Stance | 1 | Active | 30 Sta | +Soak, resist [Push]/[Pull], immobile |
| 1204 | Hook and Drag | 2 | Active | 45 Sta | [Pull] back→front, [Slowed] |
| 1205 | Line Breaker | 2 | Active | 50 Sta | AoE [Push] front→back |
| 1206 | Guarding Presence | 2 | Passive | — | Soak aura for adjacent allies |
| 1207 | Brace for Charge | 3 | Active | 40 Sta | Counter-attack on melee hit |
| 1208 | Unstoppable Phalanx | 3 | Active | 60 Sta | Line-piercing two-target attack |
| 1209 | Living Fortress | Cap | Passive | — | [Push]/[Pull] immune, reactive Brace |

---

## VI. Systemic Integration

### Formation Synergies

| Partner | Synergy |
| --- | --- |
| **Skjaldmær** | Combined stances create unbreakable frontline |
| **Berserkr** | Hook and Drag serves targets for burst damage |
| **Vargr-Born** | [Bleeding] from Skewer R3 feeds Taste for Blood |
| **Ranged Allies** | Line Breaker exposes back-row targets |

### Environmental Integration

| Hazard | Combo |
| --- | --- |
| **[Burning Ground]** | Push/Pull enemies into fire tiles |
| **[Chasms]** | Force enemies into environmental kills |
| **Chokepoints** | Living Fortress creates impassable blockade |

### Counter-Matchups

| Threat | Response |
| --- | --- |
| **Back-row casters** | Hook and Drag into melee range |
| **Charging beasts** | Brace for Charge counter-damage |
| **Push/Pull controllers** | Living Fortress immunity |

---

## VII. Status Effects Used

| Effect | Applied By | Duration | Description |
| --- | --- | --- | --- |
| **[Disciplined]** | Disciplined Stance | 2-3 turns | +Soak, resist forced movement, cannot move |
| **[Braced]** | Brace for Charge | 1 turn | +10 Soak vs melee, counter-damage |
| **[Slowed]** | Hook and Drag | 1 turn | Movement costs doubled |
| **[Off-Balance]** | Line Breaker, Phalanx | 1 turn | -2 to hit, -1 Defense |
| **[Bleeding]** | Skewer (R3 crit) | 2 turns | 1d6 Physical/turn |

---

## VIII. Implementation Notes

### Code References

| Component | Location | Status |
| --- | --- | --- |
| Data Seeding | `DataSeeder.cs` (line 1515) | ✅ Implemented |
| Specialization Enum | `Specialization.cs` | ✅ Defined |
| Specialization Factory | `SpecializationFactory.cs` | ✅ Referenced |
| [Reach] Targeting | Combat System | ⬜ Pending |
| Forced Movement | Combat System | ⬜ Pending |
| Aura System | Combat System | ⬜ Pending |

### Priority Implementation Order

1. **[Reach] keyword** — Back row → Front row targeting
2. **Row positioning system** — Front/Back for players and enemies
3. **Forced movement** — [Push] and [Pull] with opposed checks
4. **Stance system** — Duration-based buffs with restrictions
5. **Aura system** — Adjacent ally detection and bonuses
6. **Zone of Control** — Living Fortress enemy debuffs

---

## IX. Balance Notes

### Design Philosophy

The Atgeir-wielder trades **raw damage for control**. Their damage output is moderate compared to Berserkr or Hólmgangr, but their ability to dictate enemy positioning creates opportunities for the entire party.

### Power Budget

| Aspect | Rating | Notes |
| --- | --- | --- |
| Single-Target Damage | ⭐⭐⭐ | Moderate (Skewer, Phalanx) |
| AoE Damage | ⭐⭐ | Low-moderate (Line Breaker) |
| Survivability | ⭐⭐⭐⭐ | High (stances, Soak auras) |
| Crowd Control | ⭐⭐⭐⭐⭐ | Excellent (Push/Pull mastery) |
| Party Support | ⭐⭐⭐⭐ | High (Guarding Presence, formation anchor) |

### Risk Assessment

- **No Trauma Economy interaction** — Coherent warrior, no corruption mechanics
- **Positional dependency** — Many abilities require specific row positioning
- **Stamina-hungry** — High-cost abilities require Formal Training investment