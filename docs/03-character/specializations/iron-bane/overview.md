---
id: SPEC-SPECIALIZATION-IRON-BANE
title: "Iron-Bane (Zealous Purifier)"
version: 1.0
status: draft
last-updated: 2024-05-22
---

# Iron-Bane (Zealous Purifier) — Specialization Specification v5.0

## Identity
> "They are not alive. They are errors in the weaving. We do not kill them; we correct the pattern."

## Design Philosophy
The **Iron-Bane** is the "Paladin" of the Post-Glitch world, but stripped of radiant light and given the weight of a sledgehammer. They are defined by **Hatred** and **Study**. They don't just fight the **Undying** (Machines); they dismantle them with the precision of a surgeon and the brutality of a butcher.

Unlike the Berserkr (Chaos) or Atgeir-Wielder (Order), the Iron-Bane represents **Purification**. Their gameplay loop involves building **[Vengeance]** through engagement and releasing it to inflict debilitating status effects on mechanical foes.

## Mechanics
**Resolution:** MIGHT + d10 vs DC (Standard).
**Damage Tier:** d8 / d10 (Crushing Weapons).
**Resource:** Stamina + **[Vengeance]** (Stacks).

### Core Keywords
*   **[Vengeance]:** A unique stackable resource (Max 5). Gained by striking Automata or resisting [Fear].
*   **[Corroded]:** Status Effect. Reduces Soak/Armor. Machines take damage over time.
*   **[Sunder]:** Reduces target's Defense die.

## Voice Guidance
*   **Tone:** Fanatical, grim, academic yet hateful.
*   **Keywords:** Pattern, Rust, Unmake, Silence, Scrap, Error, Purge.
*   **Avoid:** "Robot", "AI", "Code", "Program", "Download".
*   **Style:** "The gear-heart stops," "I return you to the earth," "Silence the hum."

---

## Rank Progression (Tree Structure)

### Tier 1: The Vow (3 PP each)

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Undying Insight** | Passive | None | +Dice to Inspect/Identify Automata. |
| **Sanctified Steel** | Active | 40 Stamina | Attack. +1 [Vengeance] on hit (2 vs Automata). |
| **Indomitable Will** | Passive | None | +Dice vs [Fear] and [Mind-Static]. |

### Tier 2: The Rust (4 PP each)
*Unlocks when 2 Tier 1 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Corrosive Strike** | Active | 2 Vengeance | Med Dmg + [Corroded]. Melts Armor. |
| **Mirror of Hate** | Reaction | 1 Vengeance | Reflect [Fear] back at attacker. |
| **Purging Alacrity** | Passive | None | Gain Defense when inflicting [Corroded]. |

### Tier 3: The Silence (5 PP each)
*Unlocks when 2 Tier 2 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Chains of Decay** | Active | 3 Vengeance | AoE Front Row. Dmg + [Corroded] + [Slow]. |
| **Heart of Iron** | Passive | None | Immune to [Fear]. [Vengeance] allows ignoring Pain. |

### Capstone: The Unmaking (6 PP)
*Unlocks when 2 Tier 3 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Sever the Heart** | Active | 5 Vengeance | Finisher. Massive Dmg. Instakill vulnerable Automata. |

---

## Ability Details

### Tier 1

#### Undying Insight (Passive)
*You see the flaws in their metal skin like open wounds.*
*   **Rank 1:** +1d10 to WITS (Inspect) vs Automata.
*   **Rank 2:** +2d10 to WITS (Inspect). Reveal Resistances.
*   **Rank 3:** Inspect is a Free Action once per turn.

#### Sanctified Steel (Active)
*A heavy blow meant to test the metal, not just ring against it.*
*   **Mechanics:** MIGHT Attack.
*   **Rank 1:** 2d8 Physical. Gain 1 [Vengeance].
*   **Rank 2:** 2d8 + 1d8. Gain 2 [Vengeance] if target is Automaton.
*   **Rank 3:** 4d8 Physical. Critical Hit grants max [Vengeance].

#### Indomitable Will (Passive)
*The humming of their minds is just noise to one who knows the truth.*
*   **Rank 1:** +1d10 vs [Fear] / [Mind-Static].
*   **Rank 2:** +2d10 vs [Fear]. Success generates 1 [Vengeance].
*   **Rank 3:** +3d10. Allies adjacent to you gain +1d10.

### Tier 2

#### Corrosive Strike (Active)
*You strike with an oil that eats the false-life from their shells.*
*   **Cost:** 45 Stamina + 2 Vengeance.
*   **Rank 2:** 3d8 Acid/Physical. Applies [Corroded] (Stack 1).
*   **Rank 3:** 4d8 Acid/Physical. [Corroded] (Stack 2). Strips 2 Soak.

#### Mirror of Hate (Reaction)
*Your hatred is a shield that reflects their own terror.*
*   **Trigger:** Targeted by [Fear] or Mental Attack.
*   **Rank 2:** Reflect effect on Success. Cost 1 Vengeance.
*   **Rank 3:** Reflect + inflict [Stunned] on attacker. No Cost.

#### Purging Alacrity (Passive)
*To destroy the abomination is to be lighter, faster, purer.*
*   **Rank 2:** When you apply [Corroded], gain +2 Defense (1 Turn).
*   **Rank 3:** +3 Defense and +1 Action Point (once per round).

### Tier 3

#### Chains of Decay (Active)
*You slam your weapon down, and the rust spreads like a plague.*
*   **Cost:** 50 Stamina + 3 Vengeance.
*   **Rank 2:** Front Row AoE. 3d8 Dmg. All hit are [Corroded].
*   **Rank 3:** 4d8 Dmg. [Corroded] enemies are [Rooted].

#### Heart of Iron (Passive)
*You have become the thing you hate: unfeeling, unbreaking.*
*   **Rank 2:** Immune to [Fear].
*   **Rank 3:** While you have 3+ [Vengeance], take -2 Damage from all sources.

### Capstone

#### Sever the Heart (Active)
*You find the spark-vessel and crush it.*
*   **Cost:** 60 Stamina + 5 Vengeance.
*   **Requirement:** Target must be [Analyzed] (Inspected).
*   **Rank 1:** 8d10 Dmg. Ignores Soak.
*   **Rank 2:** 10d10 Dmg. If target is Automaton & < 25% HP, instant kill.
*   **Rank 3:** 12d10 Dmg. Instant kill threshold < 40% HP.

---

## Synergies

| Synergy Type | Role | Interaction |
| :--- | :--- | :--- |
| **Positive** | **Jötun-Reader** | Their analysis abilities enable your Capstone instantly. |
| **Positive** | **Bone-Setter** | They keep you alive while you tank the Front Row. |
| **Negative** | **Seidkona** | Their Aether-manipulation can trigger volatility you aren't built to resist. |

## TUI/GUI Display Considerations

*   **Vengeance:** Display as `[V]` or `(*)` icons near name.
*   **Corroded:** Use Green/Brown text color for status.
*   **Analysis:** Show a "Target Scope" icon on analyzed foes.
