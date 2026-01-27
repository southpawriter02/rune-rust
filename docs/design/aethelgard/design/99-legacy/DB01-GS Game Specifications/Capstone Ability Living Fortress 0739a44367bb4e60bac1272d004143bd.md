# Capstone Ability: Living Fortress

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-LIVINGFORTRESS-v5.0
Mechanical Role: Tank/Durability
Parent item: Atgeir-wielder (Formation Master) — Specialization Specification v5.0 (Atgeir-wielder%20(Formation%20Master)%20%E2%80%94%20Specialization%20432d149cac2a41cfad275a49efd9785b.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Atgeir-wielder (Formation Master) |
| **Tier** | Capstone (Ultimate Expression) |
| **Type** | Passive (Ultimate Defense) |
| **Prerequisite** | 40 PP spent in Atgeir-wielder tree |
| **Cost** | None (Passive) |
| **Condition** | Must be in Front Row |
| **Effect** | Immune to [Push]/[Pull]; Brace for Charge usable as Reaction |

---

## I. Design Context (Layer 4)

### Core Design Intent

Living Fortress is the Atgeir-wielder's **capstone and ultimate expression**—the moment they transcend mere discipline and become the **indomitable anchor of the battlefield**. They cannot be moved. They cannot be caught off-guard. They ARE the formation.

### Mechanical Role

- **Primary:** Permanent immunity to [Push] and [Pull] (while in Front Row)
- **Secondary:** Brace for Charge becomes usable as a Reaction
- **Fantasy Delivery:** The living fortress around which battles are won or lost

### Balance Considerations

- **Power Level:** Extremely High (capstone-appropriate)
- **Investment Required:** 40 PP is massive commitment
- **Positional Dependency:** Immunity only while in Front Row
- **Reaction Upgrade:** Transforms predictive defense into reactive counter

---

## II. Narrative Context (Layer 2)

### In-World Framing

There comes a moment in a warrior's journey when training transcends technique and becomes **identity**. The Atgeir-wielder who achieves this pinnacle is no longer *using* defensive stances—they *are* a defensive stance made flesh.

Enemies may hammer against them. Controllers may attempt to drag them from position. Charging beasts may throw their full weight against this lone warrior. And the Atgeir-wielder **does not move**.

Their footing is absolute. Their reflexes are instantaneous. They are the point around which the entire battle rotates—an immovable axis of disciplined destruction.

*"The formation does not protect the fortress. The fortress IS the formation."*

### Thematic Resonance

Living Fortress is the Atgeir-wielder's philosophy made perfect—proof that absolute discipline creates absolute defense.

---

## III. Mechanical Specification (Layer 3)

### Part 1: The Immovable Object

**Passive Effect (while in Front Row):**

- **Immune** to [Push] effects
- **Immune** to [Pull] effects
- No Resolve Check required—effects automatically fail

**Implementation:**

- When [Push] or [Pull] would be applied, check for Living Fortress passive
- If present AND character in Front Row, effect auto-fails
- Does not consume any resources

### Part 2: The Perfect Counter

**Ability Modification:**

- Brace for Charge can now be used as a **Reaction**
- **Trigger:** When Atgeir-wielder is targeted by a melee attack
- **Prompt:** "Use Brace for Charge as Reaction? (40 Stamina) (Y/N)"
- **Resolution:** If accepted, Brace activates *before* attack resolves

**Reaction Mechanics:**

- Still costs 40 Stamina
- Still requires Polearm equipped
- Triggers [Braced] status immediately
- Attack then resolves against [Braced] defenses

### Resolution Pipeline

**For Push/Pull Immunity:**

1. [Push] or [Pull] effect targets Atgeir-wielder
2. Check: Does character have Living Fortress?
3. Check: Is character in Front Row?
4. If both yes: Effect automatically fails

**For Reactive Brace:**

1. Enemy declares melee attack on Atgeir-wielder
2. System checks: Living Fortress passive present? Reaction available? Stamina sufficient?
3. If all yes: Prompt player for Reaction
4. If accepted: Execute Brace for Charge, then resolve incoming attack

---

## IV. Progression Path

### Capstone (This Ability)

- Immune to [Push] and [Pull] while in Front Row
- Brace for Charge usable as Reaction

*As a Capstone ability, Living Fortress has no further ranks. It is already at its maximum power.*

---

## V. Tactical Applications

1. **Absolute Anchor:** Cannot be displaced from chokepoints
2. **Counter-Controller:** Hard counter to enemy [Push]/[Pull] specialists
3. **Reactive Defense:** No longer need to predict charges—react to them
4. **Formation Keystone:** Entire party strategy can rely on your position
5. **Boss Counter:** Trivialize charge-based boss mechanics

---

## VI. Synergies & Interactions

### Positive Synergies

- **Brace for Charge:** Transforms from predictive to reactive
- **All Atgeir abilities:** Capstone elevates entire tree
- **Chokepoint defense:** Literally immovable blocker
- **Versus Hlekkr-master enemies:** Complete immunity to their toolkit

### Negative Synergies

- **Back Row positioning:** Immunity inactive in Back Row
- **Non-melee attacks:** Reactive Brace only triggers on melee
- **Stamina cost:** Reaction still costs 40 Stamina