# Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-SKARHORDEASPIRAN-v5.0
Mechanical Role: Controller/Debuffer, Damage Dealer
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Sub-Type: Combat
Sub-item: Tier 1 Ability: Heretical Augmentation (Tier%201%20Ability%20Heretical%20Augmentation%20c4b271e370c54c85a70d3bd9f29a10d3.md), Tier 1 Ability: Savage Strike (Tier%201%20Ability%20Savage%20Strike%20e678d8f718ef498bb29580bfed7f730d.md), Tier 1 Ability: Horrific Form (Tier%201%20Ability%20Horrific%20Form%20fc143f1c19ab4cd6a7c3689c26c980c8.md), Tier 2 Ability: Grievous Wound (Tier%202%20Ability%20Grievous%20Wound%2002bad36ce6fc4c52bcb23c9f64655451.md), Tier 2 Ability: Impaling Spike (Tier%202%20Ability%20Impaling%20Spike%209322405eb7424dfda386a70ea050021f.md), Tier 2 Ability: Pain Fuels Savagery (Tier%202%20Ability%20Pain%20Fuels%20Savagery%20f8a2292667ff47068c089e8d25083f5f.md), Tier 3 Ability: Overcharged Piston Slam (Tier%203%20Ability%20Overcharged%20Piston%20Slam%20c800bdac12324c36a08aa121cf47a9c5.md), Tier 3 Ability: The Price of Power (Tier%203%20Ability%20The%20Price%20of%20Power%2016f5892281604c87b0e0959da9bfa0ca.md), Capstone Ability: Monstrous Apotheosis (Capstone%20Ability%20Monstrous%20Apotheosis%20379b2bff771f4ddc864f21ceec0310d7.md), Weapon-Stump Augmentation System — Mechanic Specification v5.0 (Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## Core Identity

| Attribute | Value |
| --- | --- |
| **Specialization ID** | 10 (SkarHordeAspirant) |
| **Archetype** | Warrior |
| **Role** | Melee DPS / Armor-Breaker / Single-Target Disabler |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina + Savagery (unique) |
| **Trauma Economy Risk** | Extreme (Heretical path) |
| **Unlock Cost** | 3 PP |
| **Minimum Legend** | 5 |

---

## I. Design Context (Layer 4)

### Core Design Intent

The **Skar-Horde Aspirant** is the Warrior specialization that embodies the heretical philosophy of achieving power through savage, willful self-mutilation. You have ritualistically replaced your hand with a modular weapon-stump augment, trading humanity for devastating combat prowess. Build **Savagery** by fighting in melee, then unleash armor-bypassing attacks that ignore all defenses.

**You are no longer human. You are a weapon.**

### The Three Pillars

| Pillar | Mechanic | Fantasy |
| --- | --- | --- |
| **Savagery Resource** | Build via dealing/taking damage, spend on powerful abilities | Rage as a tangible, buildable force |
| **Modular Augmentation** | Swap weapon-stump augments to change damage types and ability access | Pre-combat preparation and tactical adaptability |
| **Armor Bypass** | [Grievous Wound] DoT bypasses all Soak | The ultimate tank-buster |

### Player Fantasy

> *"You are a warrior who has made a horrific bargain: permanent self-mutilation for power beyond mortal limits. Each scar you carve, each bone you shatter and allow to heal incorrectly, grants you augmentations that turn you into a living weapon. You are the heretical pragmatist who believes power is worth any cost—even your own humanity."*
> 

---

## II. Narrative Context (Layer 2)

### In-World Framing

In a world where the Runic Blight inflicts a constant, maddening psychic scream, the Skar-Horde have not sought to block it out—they have embraced it. Their grim philosophy posits that pain is the only truth in a glitching, untrustworthy reality. The Skar-Horde Aspirant is an outsider who, through desperation or conviction, chooses this path of self-inflicted agony.

Their augments are not just weapons; they are **crude antennas for the Great Silence**, turning their very being into a resonant amplifier for the world's pain. Their saga is a gruesome chronicle of self-mutilation, a deliberate descent into madness in search of the ultimate, undeniable truth of existence: suffering.

### Thematic Resonance

The Aspirant represents **power through corruption**—proof that the Trauma Economy can be exploited. The psychic weight crushing the world can be turned into a weapon. But this power comes at a terrible cost: the more they fight, the more insane they become, racing against their own mental collapse.

---

## III. Mechanical Specification (Layer 3)

### Core Mechanic: Savagery (Unique Resource)

| Aspect | Value | Notes |
| --- | --- | --- |
| **Range** | 0–100 | Hard cap at 100 |
| **Generation (dealing)** | Varies by ability | Savage Strike: 15-25/hit |
| **Generation (taking)** | 10-20% of damage taken | Via Pain Fuels Savagery |
| **Generation (Fear)** | +5 per enemy Feared | Via Horrific Form R3 |
| **Decay** | -10 per turn (no melee contact) | Slow decay out of combat |
| **Stress Cost** | 1 Stress per 15 Savagery generated | Via The Price of Power |

### Core Mechanic: Weapon-Stump Augmentation

| Augment | Damage Type | Enables | Special |
| --- | --- | --- | --- |
| **[Serrated Claw]** | Piercing/Slashing | Impaling Spike | +[Bleeding] on crit |
| **[Piston Hammer]** | Bludgeoning | Overcharged Piston Slam | [Armor Piercing] |
| **[Injector Spike]** | Piercing + Poison | Impaling Spike | +[Poisoned] on hit |
| **[Taser Gauntlet]** | Lightning | — | +[Shocked] on crit |

See [Weapon-Stump Augmentation System — Mechanic Specification v5.0](Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md) for full details.

### Resource Economy

| Resource | Base | Regeneration | Notes |
| --- | --- | --- | --- |
| Stamina | 100 | 10/turn (base) | Standard Warrior resource |
| Savagery | 0 | Build via combat | Caps at 100, decays out of combat |

---

## IV. Ability Tree Structure

### Overview

```jsx
TIER 1: FOUNDATION (0 PP each — free with spec, Ranks 1→2→3)
├── Heretical Augmentation (Passive) — Unlocks [Augmentation] slot
├── Savage Strike (Active) — Basic attack + Savagery generation
└── Horrific Form (Passive) — Fear melee attackers

TIER 2: ADVANCED (4 PP each, Ranks 2→3)
├── Grievous Wound (Active) — DoT that bypasses all Soak
├── Impaling Spike (Active) — Root enemy [Piercing required]
└── Pain Fuels Savagery (Passive) — Damage → Savagery conversion

TIER 3: MASTERY (5 PP each, Ranks 2→3)
├── Overcharged Piston Slam (Active) — Massive damage + Stun [Blunt required]
└── The Price of Power (Passive) — +Savagery gen, +Stress

CAPSTONE (6 PP, Ranks 1→2→3)
└── Monstrous Apotheosis (Active) — Transform: free attacks, immunities
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
| Unlock Specialization | 3 PP | 3 Tier 1 (free) | Rank 1 | — |
| 2× Tier 2 | 11 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 15 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 25 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 31 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## V. Ability Summary

| ID | Ability | Tier | Type | Cost | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 1001 | Heretical Augmentation | 1 | Passive | — | Unlocks [Augmentation] slot |
| 1002 | Savage Strike | 1 | Active | 40 Sta | 2d[Aug] + MIGHT, +15-25 Savagery |
| 1003 | Horrific Form | 1 | Passive | — | 25-50% Fear on melee attackers |
| 1004 | Grievous Wound | 2 | Active | 45 Sta + 30 Sav | 3-4d8 + DoT that bypasses Soak |
| 1005 | Impaling Spike | 2 | Active | 40 Sta + 25 Sav | 2-3d10 + 75-100% [Rooted] |
| 1006 | Pain Fuels Savagery | 2 | Passive | — | 10-20% damage → Savagery |
| 1007 | Overcharged Piston Slam | 3 | Active | 55 Sta + 40 Sav | 6-7d10 + 75-100% [Stunned] |
| 1008 | The Price of Power | 3 | Passive | — | +75-100% Savagery, +Stress |
| 1009 | Monstrous Apotheosis | Cap | Active | 20 Sta + 75 Sav | 3-4 turn transform, Fear/Stun immune |

---

## VI. Systemic Integration

### Party Synergies

| Partner | Synergy |
| --- | --- |
| **Bone-Setter** | **ESSENTIAL** — Manages Stress accumulation from The Price of Power |
| **Skjaldmær** | Tank holds aggro while you build Savagery safely |
| **Atgeir-wielder** | Roots enemies so you can unload Grievous Wound safely |
| **Scrap-Tinker** | Crafts superior [Optimized] augments |

### Trauma Economy Integration

| Aspect | Interaction |
| --- | --- |
| **Psychic Stress** | The Price of Power: 1 Stress per 15 Savagery generated |
| **Apotheosis Backlash** | 20 Stress at end of transformation (or exit early to avoid) |
| **Heretical Path** | Self-mutilation for power; requires mental health support |

### Counter-Matchups

| Threat | Response |
| --- | --- |
| **High-Soak tanks** | Grievous Wound bypasses all armor |
| **Fleeing enemies** | Impaling Spike roots them in place |
| **Elite bosses** | Overcharged Piston Slam guaranteed Stun |
| **Mental attacks** | Apotheosis grants Fear/Stun immunity |

---

## VII. Status Effects Used

| Effect | Applied By | Duration | Description |
| --- | --- | --- | --- |
| **[Feared]** | Horrific Form | 1 turn | Cannot approach, -2 attack, disadvantage WILL |
| **[Grievous Wound]** | Grievous Wound | 3-4 turns | 1d10-1d12/turn, **bypasses all Soak** |
| **[Rooted]** | Impaling Spike | 2-3 turns | Cannot move, -2 Defense |
| **[Stunned]** | Overcharged Piston Slam | 1 turn | Lose turn, -4 Defense, no reactions |
| **[Apotheosis]** | Monstrous Apotheosis | 3-4 turns | Free Strikes, +25% damage, Fear/Stun immune |

---

## VIII. Implementation Notes

### Code References

| Component | Location | Status |
| --- | --- | --- |
| Data Seeding | `DataSeeder.cs` (line 918) | ✅ Implemented |
| Specialization Enum | `Specialization.cs` | ✅ Defined |
| Savagery Resource | Resource System | ⬜ Pending |
| [Augmentation] Slot | Equipment System | ⬜ Pending |
| Soak Bypass | Combat System | ⬜ Pending |

### Priority Implementation Order

1. **Savagery resource system** — 0-100 bar, generation/decay
2. **[Augmentation] equipment slot** — Replace weapon slot permanently
3. **Augment crafting** — Workbench integration
4. **Soak bypass mechanic** — [Grievous Wound] ignores armor
5. **[Apotheosis] transformation** — State effects and Stress backlash

---

## IX. Balance Notes

### Design Philosophy

The Skar-Horde Aspirant trades **sanity for power**. They have the best single-target armor-bypass damage, but require dedicated Stress management support and careful augment preparation.

### Power Budget

| Aspect | Rating | Notes |
| --- | --- | --- |
| Single-Target Damage | ⭐⭐⭐⭐⭐ | Excellent (Grievous Wound bypasses armor) |
| AoE Damage | ⭐ | None (pure single-target) |
| Survivability | ⭐⭐ | Low (glass cannon, Stress accumulation) |
| Crowd Control | ⭐⭐⭐⭐ | Good (Root, Stun, Fear) |
| Party Support | ⭐ | Minimal (self-focused) |

### Risk Assessment

- **Extreme Trauma Economy interaction** — The Price of Power generates constant Stress
- **Healer dependency** — Requires Bone-Setter to manage Stress accumulation
- **Augment preparation** — Some abilities require specific augment types
- **Glass cannon** — High damage but fragile; no defensive tools

---

## X. Related Documents

- **Mechanic Spec:** [Weapon-Stump Augmentation System — Mechanic Specification v5.0](Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md)
- **Implementation:** v0.19.1: Skar-Horde Aspirant
- **Archetype:** Warrior Archetype Foundation
- **Related Specs:** Berserkr (Fury economy), Vargr-Born (aggressive melee)