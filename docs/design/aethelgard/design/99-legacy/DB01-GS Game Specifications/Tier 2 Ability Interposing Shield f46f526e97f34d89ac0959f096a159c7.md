# Tier 2 Ability: Interposing Shield

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-INTERPOSINGSHIELD-v5.0
Mechanical Role: Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
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
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Reaction |
| **Prerequisite** | 8 PP spent in Skjaldmær tree |
| **Cost** | 25 Stamina |
| **Frequency** | Once per round |
| **Trigger** | Adjacent ally about to be hit by Critical Hit |

---

## I. Design Context (Layer 4)

### Core Design Intent

Interposing Shield is the Skjaldmær's **clutch save Reaction**—a skill-based protection ability that allows her to intercept devastating attacks meant for allies. This rewards awareness and positioning, embodying the "selfless protector" fantasy.

### Mechanical Role

- **Primary:** Redirect Critical Hit damage from ally to self (50% damage taken)
- **Secondary:** Skill expression through reaction timing and awareness
- **Fantasy Delivery:** The heroic interception—throwing yourself between an ally and certain doom

### Balance Considerations

- **Power Level:** High (Critical Hit prevention is extremely valuable)
- **Limitation:** Once per round, adjacent allies only
- **Damage Splitting:** 50% damage transfer prevents full damage negation
- **Positioning Requirement:** Must be adjacent to ally being attacked

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær sees it—the devastating blow about to land on an ally, the killing strike telegraphed by posture and momentum. In that fraction of a second, she **moves**. Her shield swings into the path of the attack, taking the force meant for another. It hurts—coherent matter accepting the damage that would have shattered someone else.

### Thematic Resonance

Protection is not passive. The Skjaldmær doesn't simply stand and hope allies are safe; she actively interposes herself between them and harm. This is the defining act of the shield-bearer—the willingness to take pain that isn't yours.

---

## III. Mechanical Specification (Layer 3)

### Trigger Condition

- **When:** An adjacent ally is about to be hit by a **Critical Hit**
- **Timing:** Declared after attack roll is resolved as Critical Hit, before damage is rolled

### Activation

- **Action Type:** Reaction
- **Cost:** 25 Stamina
- **Frequency:** Once per round

### Effect

1. The attack is **redirected** to the Skjaldmær
2. Skjaldmær takes **50% of the damage** that would have been dealt
3. The original target takes **0 damage**
4. Any status effects from the attack apply to Skjaldmær instead of the original target

### Resolution Pipeline

1. **Trigger Detection:** Enemy attack resolves as Critical Hit against adjacent ally
2. **Reaction Window:** Skjaldmær may declare Interposing Shield
3. **Cost Payment:** Skjaldmær spends 25 Stamina
4. **Frequency Check:** Verify not already used this round
5. **Damage Calculation:** Enemy rolls damage normally
6. **Damage Splitting:** Skjaldmær takes 50% of calculated damage (rounded down)
7. **Status Transfer:** Any status effects apply to Skjaldmær
8. **Cleanup:** Original target is unharmed

### Worked Example

> **Scenario:** Draugr Juggernaut scores Critical Hit on Jötun-Reader
> 

> - Draugr damage roll: 8d6 = 32 damage
> 

> - Without intervention: Jötun-Reader takes 32 damage (likely fatal)
> 

> 
> 

> **With Interposing Shield:**
> 

> - Skjaldmær spends 25 Stamina, declares Interposing Shield
> 

> - Skjaldmær takes 32 × 50% = 16 damage
> 

> - Jötun-Reader takes 0 damage
> 

> - If attack inflicts [Bleeding], Skjaldmær gains [Bleeding]
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Intercept Critical Hit, take 50% damage
- Cost: 25 Stamina
- Once per round, adjacent allies only

### Rank 2 (Expert — 20 PP)

- Intercept Critical Hit, take 40% damage
- Cost: 20 Stamina
- Twice per round
- **New:** Can intercept Solid Hits as well (not just Critical Hits)

### Rank 3 (Mastery — Capstone)

- Intercept any hit, take 30% damage
- Cost: 15 Stamina
- Three times per round
- Range: 2 meters (not just adjacent)
- **New:** Skjaldmær gains +2 Soak against intercepted damage
- **New:** If Skjaldmær would be reduced to 0 HP by intercepted damage, she instead stays at 1 HP (once per combat)

---

## V. Tactical Applications

1. **Clutch Saves:** Prevent one-shot kills on fragile allies
2. **Critical Hit Denial:** Remove the devastating bonus damage of Critical Hits from the equation
3. **Healer Protection:** Keep Bone-Setter alive to heal the party
4. **Analyst Safety:** Protect Jötun-Reader during high-risk analysis
5. **Damage Efficiency:** 50% damage on a high-Soak target is better than 100% on a fragile one

---

## VI. Synergies & Interactions

### Positive Synergies

- **High-Soak builds:** Reduced damage is further mitigated by Soak
- **Shield Wall:** Combined Soak makes intercepted damage trivial
- **Fragile allies (Jötun-Reader, Bone-Setter):** Maximum value when protecting low-HP characters
- **Awareness skills:** Better enemy tracking helps predict Critical Hit opportunities

### Negative Synergies

- **Spread formations:** Must be adjacent to trigger
- **Low-Stamina situations:** 25 Stamina cost can be problematic in extended fights
- **Multiple Critical Hits per round:** Only one interception available