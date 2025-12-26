# Capstone Ability: The Ultimate Survivor

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-EINBUI-THEULTIMATESURVIVOR-v5.0
Mechanical Role: Support/Healer, Utility/Versatility
Parent item: Jötun-Reader (System Analyst) — Specialization Specification v5.0 (J%C3%B6tun-Reader%20(System%20Analyst)%20%E2%80%94%20Specialization%20Spe%209f9d73ebaf304e2d94a7e84521648919.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Capstone
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Einbúi (The Hermit) |
| **Tier** | Capstone (Ultimate Mastery) |
| **Type** | Passive + Granted Active |
| **Prerequisite** | 40 PP spent in Einbúi tree |
| **Passive** | +1 die to ALL out-of-combat skill checks not covered by Self-Reliance |
| **Active** | `establish hidden_camp` (once per expedition) |

---

## I. Design Context (Layer 4)

### Core Design Intent

The Ultimate Survivor is the **Capstone and ultimate expression** of the Einbúi—the moment they achieve **perfect harmony with their harsh environment**. They are a ghost in the wastes, a jack-of-all-trades and master of one: the art of enduring.

### Mechanical Role

- **Passive:** Complete the skill-monkey fantasy (+1 to remaining skills)
- **Active:** Create perfect sanctuary allowing PP spending in the field
- **Fantasy Delivery:** The ultimate survivor who makes the wilderness home

### Balance Considerations

- **Power Level:** Extreme (signature capstone power)
- **Once Per Expedition:** Strategic decision on when to use
- **Party-Defining:** Enables deep endgame exploration

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Einbúi has transcended mere survival. They are not just a visitor in the ruins—they have become **part of its ecosystem**. They can create a perfect, undetectable sanctuary anywhere, a place so safe it rivals civilization itself.

*"Rest here. Nothing will find us. Nothing CAN find us."*

### Thematic Resonance

The Ultimate Survivor transforms the entire wilderness into potential home—proof that mastery over survival is mastery over the world itself.

---

## III. Mechanical Specification (Layer 3)

### Part 1: Master Generalist (Passive)

**Effect:** +1 bonus die to all out-of-combat skill checks NOT already covered by Radical Self-Reliance I/II

**Covered by Self-Reliance:** Tracking, Foraging, Stealth, Climbing

**Now Gains +1:** System Bypass, Rhetoric

### Part 2: Hidden Camp (Granted Active)

- **Command:** `establish hidden_camp`
- **Use Context:** Out of Combat
- **Cooldown:** Once per expedition (resets when leaving Sanctuary)

**[Hidden Camp] Effects:**

| Benefit | Description |
| --- | --- |
| **Perfect Safety** | Ambush Chance = 0% |
| **No Camp Check** | Bypasses Camp Craft requirement |
| **Progression Access** | Party can spend PP as if in Sanctuary |

### Resolution Pipeline

1. **Command:** Player uses `establish hidden_camp`
2. **Cooldown Check:** Confirm not already used this expedition
3. **Camp Creation:** Flag current room as [Hidden Camp]
4. **Rest Benefits:** When `rest` used here, apply enhanced effects

---

## IV. Progression Path

### Capstone (This Ability)

- +1 to off-skill checks
- [Hidden Camp] once per expedition
- As Capstone, this ability is already at Mastery rank

---

## V. Tactical Applications

1. **Field Progression:** Level up mid-expedition without returning to town
2. **Guaranteed Safety:** No risk of ambush during critical rest
3. **Skill Coverage:** Competent at everything, not just survival skills
4. **Deep Exploration:** Enable multi-day endgame delves

---

## VI. Synergies & Interactions

### Positive Synergies

- **Long expeditions:** Maximum value on deep delves
- **PP-hungry builds:** Field progression is massive advantage
- **Entire party:** Everyone benefits from safe rest + PP spending

### Negative Synergies

- **Combat focus:** No direct combat benefit
- **Short missions:** Wasted if expedition is brief
- **Once per expedition:** Must choose the perfect moment

---

## VII. Balance Notes

The immense power of [Hidden Camp] is balanced by:

1. **Once Per Expedition:** Strategic, not spammable
2. **High Investment:** 40 PP tree investment required
3. **Non-Combat:** Pure utility, no damage contribution
4. **Timing Pressure:** Use too early = wasted potential; too late = might not survive