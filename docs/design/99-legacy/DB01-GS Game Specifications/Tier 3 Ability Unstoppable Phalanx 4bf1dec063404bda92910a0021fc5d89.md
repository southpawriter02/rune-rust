# Tier 3 Ability: Unstoppable Phalanx

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-UNSTOPPABLEPHALANX-v5.0
Mechanical Role: Damage Dealer
Parent item: Atgeir-wielder (Formation Master) — Specialization Specification v5.0 (Atgeir-wielder%20(Formation%20Master)%20%E2%80%94%20Specialization%20432d149cac2a41cfad275a49efd9785b.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Atgeir-wielder (Formation Master) |
| **Tier** | 3 (Mastery of Formation Warfare) |
| **Type** | Active (Line Attack) |
| **Prerequisite** | 20 PP spent in Atgeir-wielder tree |
| **Cost** | 60 Stamina |
| **Target** | Front Row enemy + enemy directly behind them |
| **Effect** | High damage to primary, moderate damage to secondary |

---

## I. Design Context (Layer 4)

### Core Design Intent

Unstoppable Phalanx is the Atgeir-wielder's **signature offensive maneuver**—a devastating lunge so powerful it punches through the primary target and into the enemy behind them. This rewards positional awareness and punishes dense enemy formations.

### Mechanical Role

- **Primary:** Deal high Physical damage to Front Row target
- **Secondary:** Deal 50% damage to enemy directly behind (automatic)
- **Fantasy Delivery:** The piercing line that skewers two enemies with one strike

### Balance Considerations

- **Power Level:** Very High (two-target damage)
- **High Cost:** 60 Stamina is significant investment
- **Positional Requirement:** Secondary target must be directly behind primary
- **No Secondary if Empty:** Just expensive single-target without aligned target

---

## II. Narrative Context (Layer 2)

### In-World Framing

This is not a thrust. This is a **siege weapon made flesh**.

The Atgeir-wielder commits everything—every ounce of strength, every gram of momentum, every inch of the polearm's reach—into a single, devastating lunge. The tip strikes the frontline warrior and does not stop. It punches *through*, emerging from the other side to find the fool standing behind.

Two enemies. One strike. The phalanx formation that served the ancients serves the Atgeir-wielder still.

### Thematic Resonance

Unstoppable Phalanx is the Atgeir-wielder's offensive pinnacle—proof that mastery of the polearm creates possibilities other weapons cannot match.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 60 Stamina
- **Range:** Melee
- **Primary Target:** Single enemy in Enemy Front Row
- **Secondary Target:** Enemy in Back Row directly behind primary (same column)

### Effect

**Primary Attack:**

- Attack Roll: FINESSE + Weapon Skill vs primary target Defense
- Damage: Weapon + MIGHT + 2d10 Physical (Piercing)

**"Punch-Through" Secondary Attack:**

- **Condition:** Primary attack must hit; enemy must exist in aligned Back Row tile
- **No Attack Roll:** Secondary target is automatically hit
- **Damage:** 50% of damage dealt to primary target

### Resolution Pipeline

1. **Target Selection:** Player selects Front Row primary target
2. **Cost Payment:** Spend 60 Stamina
3. **Primary Attack Roll:** FINESSE + Weapon Skill vs Defense
4. **Primary Damage:** On hit, deal Weapon + MIGHT + 2d10 Piercing
5. **Secondary Check:** Is there an enemy in aligned Back Row tile?
6. **Secondary Damage:** If yes, automatically deal 50% of primary damage

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Primary: Weapon + MIGHT + 2d10 Physical
- Secondary: 50% of primary damage (automatic hit)
- Cost: 60 Stamina

### Rank 2 (Expert — This is Tier 3's base form)

- As above (Tier 3 abilities start at "Expert" equivalent)

### Rank 3 (Mastery — Capstone)

- Primary: Weapon + MIGHT + 3d10 Physical
- Secondary: 75% of primary damage
- Cost: 55 Stamina
- **New:** If primary target is killed, secondary target is afflicted with [Feared] for 1 round
- **New:** Critical hit on primary causes secondary to also be [Staggered]

---

## V. Tactical Applications

1. **Formation Punisher:** Maximum value vs dense two-row formations
2. **Back-Row Harassment:** Damage protected targets without pulling them
3. **Burst Damage:** Highest single-ability damage in Atgeir tree
4. **Psychological Warfare:** Rank 3 [Feared] creates morale cascade
5. **Setup Reward:** Use Hook and Drag to align targets first

---

## VI. Synergies & Interactions

### Positive Synergies

- **Hook and Drag:** Pull target into aligned position, then Phalanx
- **Party controllers:** Allies who [Root] enemies in place = perfect targets
- **Versus packed formations:** Maximum value when enemies cluster
- **Formal Training I:** Stamina regen helps afford high cost

### Negative Synergies

- **Scattered enemies:** No secondary if Back Row empty/misaligned
- **High cost:** 60 Stamina limits uses per combat
- **Single-target situations:** Expensive for one target only