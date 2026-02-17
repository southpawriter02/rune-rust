# Berserkr (The Roaring Fire) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-BERSERKR-v5.0
Mechanical Role: Burst Damage, Damage Dealer
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Charges/Uses, Stamina
Sub-Type: Combat
Sub-item: Tier 1 Ability: Wild Swing (Tier%201%20Ability%20Wild%20Swing%200f89291ea28a4f528e3a96a723117b4d.md), Tier 2 Ability: Unleashed Roar (Tier%202%20Ability%20Unleashed%20Roar%20cbd20917558942c2ba21fa3910e98e06.md), Tier 1 Ability: Reckless Assault (Tier%201%20Ability%20Reckless%20Assault%200bf50fcba8fd4207bd67ad4f576149be.md), Tier 1 Ability: Primal Vigor I (Tier%201%20Ability%20Primal%20Vigor%20I%20b3ff033b90484403b5ef696dd21270ee.md), Tier 2 Ability: Whirlwind of Destruction (Tier%202%20Ability%20Whirlwind%20of%20Destruction%20edafdab060be4b47852cb7f6beb095cb.md), Capstone Ability: Unstoppable Fury (Capstone%20Ability%20Unstoppable%20Fury%20b939779777ba410684b9276839cbe528.md), Tier 3 Ability: Hemorrhaging Strike (Tier%203%20Ability%20Hemorrhaging%20Strike%2059101c75b2384e4f832bf6497f64d188.md), Tier 2 Ability: Blood-Fueled (Tier%202%20Ability%20Blood-Fueled%20e7bc7416a88f4de09bff0abf2881b179.md), Tier 3 Ability: Death or Glory (Tier%203%20Ability%20Death%20or%20Glory%20af3274fd0cd543eab61d658d2c968cb2.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## Core Identity

| Attribute | Value |
| --- | --- |
| **Specialization ID** | 11 (Berserkr) |
| **Archetype** | Warrior |
| **Role** | Melee Damage Engine / Shock Trooper / Crowd Clearer |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | STURDINESS |
| **Resource System** | Stamina + Fury (unique) |
| **Trauma Economy Risk** | High (Heretical path) |
| **Unlock Cost** | 3 PP |
| **Minimum Legend** | 3 |

---

## I. Design Context (Layer 4)

### Core Design Intent

The **Berserkr** is the Warrior specialization that embodies the heretical philosophy of **channeling the world's trauma into pure, untamed physical power**. They are not disciplined soldiers; they are **roaring fires** of destruction. They have learned to open their minds to the violent, screaming psychic static of the Great Silence, using that chaotic feedback to fuel a battle-lust that pushes their bodies far beyond their normal limits.

### The Three Pillars

| Pillar | Mechanic | Fantasy |
| --- | --- | --- |
| **Fury Resource** | Build by dealing/taking damage, spend on powerful abilities | Rage as a tangible, buildable force |
| **Damage-to-Power** | Blood-Fueled converts HP loss into Fury | Pain fuels strength, wounds become weapons |
| **High-Risk/Reward** | [Vulnerable], [Bloodied] bonuses, friendly fire | Trading safety for overwhelming destruction |

### Player Fantasy

> *"You are the unstoppable barbarian, the roaring fire of destruction. Your rage is not mere emotion—it's a resource you build and spend. Every wound you take fuels your fury. Every enemy you strike adds to your momentum. You trade defense for the highest sustained melee damage in the game, becoming more dangerous the closer you are to death."*
> 

---

## II. Narrative Context (Layer 2)

### In-World Framing

In a world defined by the psychological weight of the Great Silence, the Berserkr is an **anomaly**—a heretic who has learned to weaponize what destroys others. They open their minds to the screaming psychic static that drives lesser minds to madness, channeling that chaos into pure, uncontrolled battle-lust.

Their fighting style is a **direct embrace of the world's trauma**. Where others build walls against the Silence, the Berserkr tears those walls down and lets the storm flow through them. The result is terrifying: a warrior whose fury grows with every wound, whose power peaks at the moment of near-death.

### Thematic Resonance

The Berserkr represents the **temptation of power through corruption**. They are living proof that the Trauma Economy can be exploited, that the psychic weight crushing the world can be turned into a weapon. But this power comes at a cost—the -2 WILL penalty while holding Fury represents the erosion of self-control, the gradual surrender to the roaring fire within.

Their saga is one of **glorious destruction and inevitable burnout**. To be a Berserkr is to accept that the fire that empowers you will eventually consume you.

---

## III. Mechanical Specification (Layer 3)

### Core Mechanic: Fury (Unique Resource)

| Aspect | Value | Notes |
| --- | --- | --- |
| **Range** | 0–100 | Hard cap at 100 |
| **Base Generation (dealing)** | Varies by ability | Wild Swing: 5-10/hit, Reckless Assault: 15-20 |
| **Base Generation (taking)** | 1 Fury per 1 HP damage | Multiplied by Blood-Fueled (2×/3×) |
| **Decay** | None during combat | Reset to 0 on Sanctuary Rest |
| **WILL Penalty** | -2 dice while Fury > 0 | Applies to all WILL-based Resolve Checks |

### The Trauma Economy Interface

The Berserkr's -2 WILL penalty while holding Fury creates a fundamental tension:

- **Hold Fury** → More powerful abilities, Primal Vigor regen, but vulnerable to Fear/psychic attacks
- **Spend Fury** → Remove penalty, but lose damage potential

**Design Intent:** This is the "cost" of heretical power. Unstoppable Fury (Capstone) provides permanent Fear immunity as the ultimate answer to this weakness.

### Resource Economy

| Resource | Base | Regeneration | Notes |
| --- | --- | --- | --- |
| Stamina | 100 | 10/turn (base) | Primal Vigor adds +2/+3/+4 per 25 Fury |
| Fury | 0 | Build via abilities/damage | Caps at 100, resets on Sanctuary Rest |

---

## IV. Ability Tree Structure

### Overview

```jsx
TIER 1: FOUNDATION (3 PP each, Ranks 1→2→3)
├── Primal Vigor I (Passive) — Stamina regen scales with Fury
├── Wild Swing (Active) — AoE Front Row, +Fury per enemy hit
└── Reckless Assault (Active) — High single-target, +Fury, [Vulnerable]

TIER 2: ESCALATION (4 PP each, Ranks 2→3)
├── Unleashed Roar (Active) — Taunt + Fury on attacks received
├── Whirlwind of Destruction (Active) — AoE all rows, friendly fire risk
└── Blood-Fueled (Passive) — 2×/3× Fury from damage taken

TIER 3: MASTERY (5 PP each, Ranks 2→3)
├── Hemorrhaging Strike (Active) — High burst + [Bleeding] DoT
└── Death or Glory (Passive) — Bonuses while [Bloodied]

CAPSTONE (6 PP, Ranks 1→2→3)
└── Unstoppable Fury (Passive) — Fear/Stun immune, death prevention
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
| 1101 | Primal Vigor I | 1 | Passive | — | +Stamina regen per 25 Fury |
| 1102 | Wild Swing | 1 | Active | 40 Sta | AoE Front Row, +5-10 Fury/hit |
| 1103 | Reckless Assault | 1 | Active | 35 Sta | 4-6d10 + MIGHT, +15-20 Fury, [Vulnerable] |
| 1104 | Unleashed Roar | 2 | Active | 30 Sta + 20 Fury | [Taunt], +10-15 Fury/attack received |
| 1105 | Whirlwind of Destruction | 2 | Active | 50 Sta + 30 Fury | AoE all rows, 35%/15% friendly fire |
| 1106 | Blood-Fueled | 2 | Passive | — | 2×/3× Fury from damage taken |
| 1107 | Hemorrhaging Strike | 3 | Active | 45 Sta + 40 Fury | 5d10 burst + 3d10 [Bleeding]/turn |
| 1108 | Death or Glory | 3 | Passive | — | +5 damage, +75% Fury, 18-20 crit while [Bloodied] |
| 1109 | Unstoppable Fury | Cap | Passive | — | [Feared]/[Stunned] immune, death prevention |

---

## VI. Systemic Integration

### Party Synergies

| Partner | Synergy |
| --- | --- |
| **Bone-Setter** | Essential healer support for HP management in [Bloodied] range |
| **Skjaldmær** | Tank absorbs overflow when Berserkr needs recovery |
| **Skald** | Saga buffs stack with Death or Glory bonuses |
| **Thul/Jötun-Reader** | Controllers set up multi-target Wild Swing/Whirlwind |

### Trauma Economy Integration

| Aspect | Interaction |
| --- | --- |
| **Psychic Stress** | Reckless Assault has 25-50% Stress chance per use |
| **WILL Vulnerability** | -2 WILL while Fury > 0; solved by Capstone Fear immunity |
| **Heretical Path** | Channels world's trauma as power source |

### Counter-Matchups

| Threat | Response |
| --- | --- |
| **Fear effects** | Unstoppable Fury immunity; Blood-Fueled R3 [Inspired] |
| **Stun effects** | Unstoppable Fury immunity |
| **Regenerating enemies** | Hemorrhaging Strike anti-heal |
| **Multi-target swarms** | Wild Swing + Whirlwind AoE clear |

---

## VII. Status Effects Used

| Effect | Applied By | Duration | Description |
| --- | --- | --- | --- |
| **[Vulnerable]** | Reckless Assault | 1 turn | +15-25% damage taken (self-applied) |
| **[Taunted]** | Unleashed Roar | 2-3 turns | Must attack the Berserkr |
| **[Bleeding]** | Hemorrhaging Strike, Whirlwind R3 | 3 turns | 2d6-3d10 Physical/turn, prevents healing |
| **[Inspired]** | Blood-Fueled R3 | 1 turn | +2 action rolls, Fear immune |
| **[Furious]** | Blood-Fueled R3 | 2 turns | +4 MIGHT, +2 attack rolls |

---

## VIII. Implementation Notes

### Code References

| Component | Location | Status |
| --- | --- | --- |
| Data Seeding | `DataSeeder.cs` | ⬜ Pending alignment |
| Specialization Enum | `Specialization.cs` | ✅ Defined |
| Fury Resource | Resource System | ⬜ Pending |
| Friendly Fire | Combat System | ⬜ Pending |
| [Bloodied] Trigger | Status System | ⬜ Pending |

### Priority Implementation Order

1. **Fury resource system** — 0-100 bar, generation/spending
2. **[Bloodied] threshold detection** — 50% HP trigger
3. **Friendly fire system** — % chance to hit allies
4. **Death prevention** — Capstone 1 HP trigger
5. **Conditional immunities** — Fear/Stun at Capstone

---

## IX. Balance Notes

### Design Philosophy

The Berserkr trades **defense for offense** more aggressively than any other specialization. They have the highest sustained melee damage output, but require healer support and careful HP management to survive.

### Power Budget

| Aspect | Rating | Notes |
| --- | --- | --- |
| Single-Target Damage | ⭐⭐⭐⭐⭐ | Excellent (Reckless Assault, Hemorrhaging Strike) |
| AoE Damage | ⭐⭐⭐⭐⭐ | Excellent (Wild Swing, Whirlwind) |
| Survivability | ⭐⭐ | Low (offset by Death or Glory, Unstoppable Fury) |
| Crowd Control | ⭐⭐ | Limited ([Taunt] only) |
| Party Support | ⭐ | Minimal (threat generation via Taunt) |

### Risk Assessment

- **High Trauma Economy interaction** — Heretical specialization, Stress from Reckless Assault
- **Healer dependency** — Requires Bone-Setter to sustain [Bloodied] range safely
- **WILL vulnerability** — -2 dice penalty solved by Capstone
- **Friendly fire risk** — Whirlwind can harm allies