# Tier 3 Ability: Master Improviser

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-EINBUI-MASTERIMPROVISER-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer
Parent item: Jötun-Reader (System Analyst) — Specialization Specification v5.0 (J%C3%B6tun-Reader%20(System%20Analyst)%20%E2%80%94%20Specialization%20Spe%209f9d73ebaf304e2d94a7e84521648919.md)
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
| **Specialization** | Einbúi (The Hermit) |
| **Tier** | 3 (Master Survival) |
| **Type** | Passive (Ability Upgrade) |
| **Prerequisite** | 20 PP spent in Einbúi tree |
| **Effect** | Permanently upgrades Improvised Trap and Basic Concoction |

---

## I. Design Context (Layer 4)

### Core Design Intent

Master Improviser is the **primary field crafting upgrade** for the Einbúi—the moment their practical skills transcend mere function and become an **art form**. Their hands move with practiced, unconscious efficiency.

### Mechanical Role

- **Primary:** Upgrade Improvised Trap effects
- **Secondary:** Upgrade Basic Concoction potency
- **Fantasy Delivery:** The master craftsman of improvised solutions

### Balance Considerations

- **Power Level:** High (significant upgrades to core abilities)
- **Late-Game Investment:** Rewards long-term specialization
- **Scaling:** Keeps Tier 1 abilities relevant endgame

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Einbúi's field crafting has reached its peak. The traps they create are no longer simple snares—they're **cleverly designed wounding mechanisms**. The poultices they mix are not crude remedies—they're **potent, refined concoctions**.

*"Same materials. Better results. That's experience."*

### Thematic Resonance

Master Improviser transforms basic survival skills into **mastered arts**—proof that expertise elevates even humble tools.

---

## III. Mechanical Specification (Layer 3)

### Upgraded Improvised Trap

| Property | Original | Upgraded |
| --- | --- | --- |
| [Rooted] Duration | 1 round | **2 rounds** |
| Additional Effect | None | **+[Bleeding]** |

### Upgraded Basic Concoction

| Property | Original | Upgraded |
| --- | --- | --- |
| Crude Poultice | 20 HP | **40 HP** |
| Weak Stimulant | 25 STA | **50 STA** |

### Resolution Pipeline

1. **Ability Execution:** Player uses Improvised Trap or Basic Concoction
2. **Passive Check:** Does character have Master Improviser?
3. **Script Selection:** If yes, execute upgraded effect script
4. **Enhanced Output:** Apply improved effects

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Trap: +[Bleeding], 2-round [Rooted]
- Concoction: Doubled potency

### Rank 2 (Expert — Base form for Tier 3)

- As Rank 1 (this is the base form)

### Rank 3 (Mastery — Capstone)

- Trap becomes [Serrated Snare]: Enhanced [Bleeding]
- Concoction becomes [Refined Concoction]:
    - Poultice also cleanses 1 [Poisoned] stack
    - Stimulant also cleanses [Staggered]

---

## V. Tactical Applications

1. **Enhanced Control:** Longer root + bleed damage from traps
2. **Better Support:** Meaningful healing/stamina restoration
3. **Late-Game Viability:** Tier 1 abilities remain competitive
4. **Condition Cleansing:** Remove debilitating status effects

---

## VI. Synergies & Interactions

### Positive Synergies

- **Improvised Trap:** Core ability becomes powerful control
- **Basic Concoction:** Becomes meaningful emergency support
- **Bleed-focused parties:** Trap contributes to bleed stacks

### Negative Synergies

- **Requires base abilities:** Must have Trap/Concoction to benefit
- **Component dependency:** Still requires foraged materials