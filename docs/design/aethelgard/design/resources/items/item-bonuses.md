# Item Bonus & Stacking Specification

## Codex Entry: CODX-M-BON-001

> "The Rúnasmiðr's first lesson is limits. You can pour only so much power into steel before it melts. You can pour only so much magic into a mind before it breaks.
>
> A warrior can wear a ring that makes them stronger. They can wear a belt that makes them tougher. They can wear boots that make them faster. But if you try to make them all three at once, and then add a cloak that makes them invisible and a helm that lets them see the future... well.
>
> The steel might survive. The warrior will not.
>
> Infinite power is a child's dream. Real power is knowing exactly how much you can take before you shatter."
>
> — Forge-Master Thrain Ironhand, Dvergr Smiths Guild

---

## Overview

| Property | Value |
|----------|-------|
| Document ID | AAM-SPEC-MECHANIC-BONUSES-v1.0 |
| Classification | AETHELGARD-ARCHIVES-MASTER // MECHANICS |
| Subject Scope | Item Bonuses, Stacking Rules, Attribute Caps |
| Document Layer | Layer 2 (Diagnostic) |
| Author Faction | Jötun-Reader Scriptorium (System Analysis Division) |
| Core Doctrine | "Power must be finite to be stable." |

This specification defines the rules for item-derived bonuses in Aethelgard. It establishes which bonuses are permitted, how they interact when multiple items provide similar benefits, and the absolute limits (caps) on character power to prevent system destabilization.

---

## 1. Bonus Categories

### 1.1 Flat Stat Bonuses (Stackable)
These bonuses provide a fixed numerical increase to a core pool or attribute.

| Bonus Type | Example | Stacking Behavior |
|------------|---------|-------------------|
| **Attribute** | +1 MIGHT, +2 WILL | **STACKS** (Subject to Global Cap) |
| **Resource** | +10 HP, +5 Stamina | **STACKS** |
| **Rating** | +1 Defense, +2 Vigilance | **STACKS** |
| **Soak** | +1 Soak (Damage Reduction) | **STACKS** |

### 1.2 Rating Bonuses (Stackable)
These bonuses improve derived combat statistics.

| Bonus Type | Example | Stacking Behavior |
|------------|---------|-------------------|
| **Accuracy** | +1 to Attack Rolls | **STACKS** |
| **Defense** | +1 to Defense Score | **STACKS** |
| **Resistance** | +10 Fire Resistance | **STACKS** (Subject to Resistance Cap) |

### 1.3 Status Effects (OVERWRITE ONLY)
These bonuses apply a named status effect or condition.

| Bonus Type | Example | Stacking Behavior |
|------------|---------|-------------------|
| **Beneficial** | [Fortified], [Hasted] | **NEVER STACKS — NEW OVERWRITES OLD** |
| **Detrimental** | [Burning], [Slowed] | **NEVER STACKS — NEW OVERWRITES OLD** |

> [!IMPORTANT]
> **The Status Rule**: A character can never have more than **one instance** of the same status effect. Applying [Fortified] to a character already [Fortified] simply resets the duration to the new source's value. It does not increase the effect's intensity or add durations together.

---

## 2. Stacking Rules

### 2.1 The Cardinal Rules of Stacking

1.  **Source-Based Stacking**: Bonuses from **different equipment slots** DO stack.
    *   *Example:* +1 MIGHT from a Helm and +1 MIGHT from a Ring = +2 MIGHT.

2.  **Name-Based Exclusion**: Bonuses from **duplicate items** (items with the exact same name) DO NOT stack.
    *   *Example:* Equipped with two "Rings of the Ox" (+1 MIGHT). The second ring provides **NO BENEFIT** to MIGHT.

3.  **Status Priority**: Status effects **NEVER** stack. The most recently applied instance overwrites the previous instance.
    *   *Example:* Drinking a potion of [Fortified] (Duration 3) while wearing armor that grants [Fortified] (Permanent) results in the permanent effect resuming only after the potion effect clears (or the potion overwrite fails if continuous sources have priority—standard rule: continuous sources provide the floor, temp sources override if higher/longer, but never add).
    *   *Simplified Rule:* If you have [Fortified] from Armor and cast [Fortified], you are just [Fortified]. You do *not* gain double soak.

### 2.2 Bonus Interaction Matrix

| Source A | Source B | Interaction | Result |
|----------|----------|-------------|--------|
| Armor (+HP) | Ring (+HP) | **Stack** | Total HP = A + B |
| Ring of Might (+1 MIGHT) | Ring of Might (+1 MIGHT) | **No Stack** | Total = +1 MIGHT (Duplicate Item) |
| Ring of Might (+1 MIGHT) | Amulet of Might (+1 MIGHT) | **Stack** | Total = +2 MIGHT (Different Items) |
| Ability ([Hasted]) | Potion ([Hasted]) | **Overwrite** | Newest application applies |
| Passive (+Fire Res) | Spell (+Fire Res) | **Stack** | Total Res (Subject to Cap) |

---

## 3. Allowed vs. Prohibited Bonuses

To maintain game balance and "grit," certain types of bonuses are strictly prohibited.

### 3.1 Allowed Bonuses
*   **Flat Adds to Pools**: +HP, +Stamina, +Aether (AP).
*   **Flat Adds to Attributes**: +MIGHT, +FINESSE, etc.
*   **Flat Adds to Ratings**: +Defense, +Accuracy, +Soak.
*   **Flat Resistance**: +Physical Resistance, +Fire Resistance (integer values).
*   **Skill Ranks**: +1 to [Athletics], +1 to [Runelore].
*   **Dice Adds**: +1d10 to damage (Conditional or Flat).

### 3.2 Prohibited Bonuses (System Hard-Bans)

| Prohibited Bonus | Reason | Alternative |
|------------------|--------|-------------|
| **% Damage Increase** | Values scale uncontrollably; creates "required" items. | Use +1d10 Damage or +Attribute. |
| **% Damage Reduction** | Invalidates high-damage threats; math breaks at high %. | Use Flat Soak or HP. |
| **Action Economy** | "Free Actions" or "Extra Actions" destroys turn balance. | Use [Hasted] (move speed) or Stamina reduction. |
| **Multipliers** | "Double Damage", "x3 Critical Multiplier". | Use +Dice bonuses. |
| **Stat Swapping** | "Use WITS instead of MIGHT for damage". | Unlocks completely SAD (Single Attribute Dependent) builds. |

> [!CAUTION]
> **No Percentages**: The system logic is integer-based. Do not create items that grant "+10% Strength". Always use "+1 Strength".

---

## 4. Global Caps & Limits

Regardless of how many items a character equips, the following hard caps apply to their total bonuses.

### 4.1 Attribute Bonus Cap
The total bonus to any single attribute from **all equipment combined** cannot exceed **+10**.

*   *Base:* 5–20 (Natural)
*   *Equipment Max:* +10
*   *Absolute Theoretical Max:* 30 (God-Tier)

> *Rationale:* Prevents a single stat from trivializing the dice system (d10).

### 4.2 Resistance Hard Cap
Damage resistance from all sources combined (Armor + Magic + Traits) cannot exceed **75%**.

*   *Note:* Resistance is calculated *after* Soak.
*   *Formula:* `(Damage - Soak) * (1 - Resistance%)`
*   *Cap:* Resistance % is clamped at 0.75.

### 4.3 Skill Rank Bonus Cap
The total bonus to any single Skill Rank from equipment cannot exceed **+5**.

### 4.4 Attunement Cap
A character acts as the ultimate bottleneck for high-tier power.
*   **Max Attunement Slots:** 3
*   *Consequence:* A character can benefit from at most 3 Myth-Forged (Tier 4) items.

---

## 5. Voice Guidance

### Layer 1 (Mythic) — The Survivor's Creed

> "Don't be greedy. That's the first rule of the ruins.
>
> You find a ring that makes you strong? Good. You find a belt that makes you tough? Good. You try to wear ten rings and three belts? You're dead. Not because the monsters get you, but because the magic gets you. It fights itself. It twists inside you.
>
> The ancestors built their toys to work alone. They didn't design them to be stacked like cordwood. Wear what works. Trust your steel, not your trinkets.
>
> And never, ever try to wear two of the same ring. The echoes get confused. And when the echoes get confused, your finger falls off."

### Layer 2 (Diagnostic) — System Analysis

> "AETHERIC RESONANCE LIMITERS — SPECIFICATION
>
> Multiple signal sources operating on identical resonance frequencies (duplicate items) result in destructive interference; net benefit is nullified for secondary sources.
>
> Signal amplitude stacking (attribute bonuses) is permissible from distinct sources but subject to biological tolerance thresholds (Global Attribute Cap). Exceeding tolerance results in somatic rejection or signal plateau.
>
> Temporal effect matrices (Status Effects) lack recursive capability. Attempting to overlay an active matrix with a new instance triggers a complete refresh/rewrite of the local reality state. The newest truth becomes the only truth."

---

## 6. Appendix: Quick Reference

| Problem | Rule |
|---------|------|
| "I have two Rings of Fire." | Only **one** works. |
| "I have a Helm of Might (+1) and Ring of Might (+1)." | You have **+2 MIGHT**. |
| "I have Armor of Fortitude (+10 HP) and Ring of Life (+10 HP)." | You have **+20 HP**. |
| "I cast [Haste] on my [Hasted] ally." | Duration resets. **No double speed.** |
| "I have +50% Fire Res from Armor and +50% from Ring." | You have **75% Fire Res** (Hard Cap). |
