---
id: SPEC-SPECIALIZATION-RUNASMIDR
title: "Rúnasmiðr (Rune-Smith)"
version: 1.0
status: draft
last-updated: 2024-05-23
---

# Rúnasmiðr (Rune-Smith) — Specialization Specification v5.0

## Identity
> "We do not write the future. We carve the present. Steel forgets, but the Rune remembers."

## Design Philosophy
The **Rúnasmiðr** is the bridge between the physical and the Aetheric. They are not wizards—they are engineers of the soul, inscribing logic onto reality through the medium of cold iron. Where the Berserkr channels internal fury and the Atgeir-Wielder enforces external order, the Rúnasmiðr **modifies the rules** of engagement.

Their core fantasy is **Preparation and Permanence**. While others spend resources to act in the moment, the Rúnasmiðr invests resources to create lasting advantages—enhanced gear, defensive wards, and situational tools. In the Post-Glitch world, they are the ones who make the broken things work again, not by fixing them, but by forcing them to obey a new pattern.

## Mechanics
**Resolution:** WITS + d10 vs DC (Crafter/Caster Profile).
**Damage Tier:** d8 (Hammer) / d6 (Runic Discharges).
**Resource:** Stamina (Physical) + Runeink (Material Component).

### Core Keywords
*   **[Inscribe]:** Apply a temporary or semi-permanent buff to an item. Requires **Runeink**.
*   **[Ward]:** Place a static trap or zone of control on the battlefield.
*   **[Overload]:** Destroy an inscribed rune to trigger a powerful, immediate effect.

## Voice Guidance
*   **Tone:** Reverent, obsessive, technical yet ritualistic.
*   **Keywords:** Etch, bind, pattern, logic, rot, stabilize, geometry, anchor.
*   **Avoid:** Magic, spell, mana, enchant, wizard.
*   **Style:** "The pattern requires blood," "I am merely aligning the grain of reality."

---

## Rank Progression (Tree Structure)

The Rúnasmiðr uses a **Tree Progression** system. Abilities unlock based on total PP invested and specific tree milestones.

### Tier 1: The Apprentice (3 PP each)

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Rune Reader** | Passive | None | Identify magical/tech properties. Bonus loot chance for Components. |
| **Carve Ward** | Active | 1 Ink | Place a trap tile. 2d6 Aetheric Dmg + [Slowed]. |
| **Inscription: Sharpness** | Active | 2 Ink | Buff Weapon: +1 Dmg Tier (e.g., d6->d8) for encounter. |

### Tier 2: The Journeyman (4 PP each)
*Unlocks when 2 Tier 1 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Engrave Weapon** | Active | 3 Ink | Imbue weapon with Element (Fire/Ice/Lightning). Deals +1d6 Elemental Dmg. |
| **Sigil of Binding** | Active | 3 Ink | Target single enemy. WITS vs RESIST to [Root] and silence abilities. |
| **Armor Inscription** | Active | 2 Ink | Buff Armor: +2 Soak / +1 Def. Duration: Encounter. |

### Tier 3: The Master (5 PP each)
*Unlocks when 2 Tier 2 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **Runelore Mastery** | Passive | None | Reduce Runeink costs by 1 (Min 1). +2 WITS. |
| **Elder Patterns** | Active | 5 Ink | AoE Buff: All allies gain [Shielded] (Absorb 10 Dmg). |

### Capstone: The All-Rune (6 PP)
*Unlocks when 2 Tier 3 abilities are trained.*

| Ability | Type | Cost | Effect |
| :--- | :--- | :--- | :--- |
| **All-Rune Glimpse** | Active | 10 Ink | Reveal entire map. All enemies [Marked] (Auto-Crit next hit). |

---

## Ability Details

### Tier 1

#### Rune Reader (Passive)
*You see the world not as it is, but as it was written.*
*   **Rank 1:** Automatically identify properties of "Unknown" artifacts. +10% chance to find Runeink/Scrap.
*   **Rank 2:** +1d10 to [Analysis] checks. +20% loot chance.
*   **Rank 3:** Can detect [Hidden] enemies within 3 tiles.

#### Carve Ward (Active)
*You scratch a hasty warning into the ground. Woe to those who ignore it.*
*   **Mechanics:** Place a visible Trap on a tile within Range 3. Triggers on entry.
*   **Rank 1:** 2d6 Aetheric Dmg. Applies [Slowed] (Movement = 1).
*   **Rank 2:** 3d6 Aetheric Dmg. Applies [Rooted] (No move).
*   **Rank 3:** 3d6 Aetheric Dmg. Explosion radius 1 (3x3 area).

#### Inscription: Sharpness (Active)
*You align the weapon's edge with the concept of 'severing'.*
*   **Mechanics:** Touch range. Target Weapon. Duration: End of Combat.
*   **Rank 1:** Increase weapon damage die by 1 step (d4->d6->d8->d10). Max d10.
*   **Rank 2:** If weapon is already d10, adds +2 Flat Damage. On Crit: Applies [Bleeding].
*   **Rank 3:** Duration becomes "Until next Long Rest".

### Tier 2

#### Engrave Weapon (Active)
*Fire is not a chemical reaction; it is a rune of hunger.*
*   **Mechanics:** Imbue weapon with elemental damage.
*   **Rank 2:** Select Fire (DoT), Ice (Slow), or Lightning (Stun chance). Adds +1d6 Elemental Dmg.
*   **Rank 3:** Effect applies to all allies in Range 1 (Aura of Forging).

#### Sigil of Binding (Active)
*You impose stillness upon the chaotic form.*
*   **Mechanics:** Ranged (5). WITS vs RESIST.
*   **Rank 2:** Target is [Rooted]. Cannot use Movement abilities.
*   **Rank 3:** Target is [Silenced]. Cannot use Active Abilities.

#### Armor Inscription (Active)
*The geometric perfection of the turtle's shell, replicated in scrap metal.*
*   **Mechanics:** Touch range. Target Armor.
*   **Rank 2:** +2 Soak (Physical Damage Reduction).
*   **Rank 3:** +2 Soak. +1 Defense (Dodge chance).

### Tier 3

#### Runelore Mastery (Passive)
*The patterns are burned into your eyelids. You no longer need to look to see.*
*   **Rank 2:** Runeink costs reduced by 1. Max Runeink capacity +5.
*   **Rank 3:** WITS attribute +2.

#### Elder Patterns (Active)
*A sequence so complex it hurts to look at. Reality bends around it to protect you.*
*   **Mechanics:** Party-wide buff.
*   **Rank 2:** All allies gain [Shielded] (Absorb next 10 Damage).
*   **Rank 3:** [Shielded] (Absorb 15 Damage). If Shield breaks, deals 2d6 Aetheric Dmg to attacker.

### Capstone

#### All-Rune Glimpse (Active)
*For a split second, you see the code behind the world. It is beautiful. It is terrible.*
*   **Mechanics:** Map-wide effect. High cost.
*   **Rank 1:** Reveal all Fog of War. Identify all enemies.
*   **Rank 2:** All enemies suffer [Marked] (Next attack against them is Auto-Crit).
*   **Rank 3:** You gain [Omniscient] for 3 turns (Can target enemies through walls/obstacles).

---

## Synergies

| Synergy Type | Role | Interaction |
| :--- | :--- | :--- |
| **Positive** | **Berserkr** | Your Armor Inscriptions allow them to survive deeper in enemy lines. |
| **Positive** | **Atgeir-Wielder** | "Sharpness" on a Polearm makes their Reach attacks devastating (d10->d10+2). |
| **Negative** | **Rogue/Assassin** | Your glowing runes and noisy inscriptions can ruin stealth approaches. |

## TUI/GUI Display Considerations

*   **Tables:** Ensure ability tables fit within 80 cols.
*   **Visuals:** In GUI, "Inscribed" weapons should have a unique border color (e.g., Cyan).
*   **Status Icons:** Use `[ᚠ]` (Fehu) for Wealth/Loot buffs, `[ᚦ]` (Thurisaz) for Attack buffs.
