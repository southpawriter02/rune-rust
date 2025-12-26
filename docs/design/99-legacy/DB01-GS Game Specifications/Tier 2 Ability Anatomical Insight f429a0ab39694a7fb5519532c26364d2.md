# Tier 2 Ability: Anatomical Insight

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-ANATOMICALINSIGHT-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer
Parent item: Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0 (Bone-Setter%20(Restorer%20of%20Coherence)%20%E2%80%94%20Specializati%20b254ea23b3664889b545c17166ce7e7f.md)
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
| **Specialization** | Bone-Setter (Restorer of Coherence) |
| **Tier** | 2 (Advanced Treatment) |
| **Type** | Active (Debuff) |
| **Prerequisite** | 8 PP spent in Bone-Setter tree |
| **Cost** | Standard Action |
| **Target** | Single organic enemy |
| **Duration** | 2 turns |

---

## I. Design Context (Layer 4)

### Core Design Intent

Anatomical Insight gives the Bone-Setter **offensive utility**—their knowledge of healing applies equally to knowing where to hurt. This establishes that medical knowledge is a double-edged sword.

### Mechanical Role

- **Primary:** Apply [Vulnerable] debuff to organic enemy (bonus Physical damage taken)
- **Secondary:** Expands Bone-Setter's tactical options beyond pure healing
- **Fantasy Delivery:** The healer who knows exactly where it hurts

### Balance Considerations

- **Power Level:** High (party-wide damage increase via debuff)
- **Targeting Restriction:** Only affects organic creatures (not Undying/mechanical)
- **Check Required:** WITS check vs enemy, can fail

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Bone-Setter studies the creature's movements—the way it favors one leg, the tension in its shoulder, the exposed vein pulsing beneath thin skin. Their eyes trace invisible lines across its body, marking the places where a blade would slide between ribs, where a blow would shatter joints, where pressure would collapse organs.

### Thematic Resonance

The same knowledge that saves lives can end them. The Bone-Setter's deep understanding of anatomy makes them dangerous in ways that complement their healing role.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** None (skill-based)
- **Range:** 6 meters (visual range)
- **Target:** Single **organic** enemy

### Resolution

**Accuracy Check:**

- Roll: WITS + Medicine (or Anatomy skill if available)
- DC: Target's Mental Defense or static DC based on creature complexity

### On Success

- Target gains **[Vulnerable]** for 2 turns

### [Vulnerable] Effect

- Target takes **+25% damage from all Physical attacks**
- Duration: 2 turns
- Does not stack with itself

### Targeting Restriction

**Valid Targets (Organic):**

- Humans, beasts, mutated creatures
- Most Forlorn (corrupted but still organic)
- Biological entities with anatomy

**Invalid Targets (Non-Organic):**

- Undying (Draugr, Revenants—their "anatomy" is corrupted code)
- Mechanical constructs
- Spirits, ghosts, ethereal entities
- Elemental creatures

### Resolution Pipeline

1. **Targeting:** Bone-Setter selects organic enemy within 6 meters
2. **Organic Check:** System verifies target is organic
3. **WITS Check:** Roll WITS + Medicine vs target's defense
4. **Success/Failure:** On success, apply [Vulnerable]; on failure, no effect
5. **Duration Track:** [Vulnerable] expires after 2 turns

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Apply [Vulnerable] for 2 turns to organic enemy
- WITS check required

### Rank 2 (Expert — 20 PP)

- Apply [Vulnerable] for 3 turns
- +1 bonus die to the WITS check
- **New:** [Vulnerable] effect increased to +30% Physical damage

### Rank 3 (Mastery — Capstone)

- Apply [Vulnerable] for 4 turns
- +2 bonus dice to the WITS check
- +35% Physical damage taken
- **New:** Can affect 2 enemies simultaneously
- **New:** Partially effective against Undying (reduced to +15% damage, 2 turns)

---

## V. Tactical Applications

1. **Damage Amplification:** Party-wide damage boost against single target
2. **Focus Fire Enhancement:** Mark priority target for team to burn down
3. **Boss Fights:** Significant total damage increase over fight duration
4. **Beast Encounters:** Maximum value against organic wildlife
5. **Offensive Option:** Gives Bone-Setter something to do when healing isn't needed

---

## VI. Synergies & Interactions

### Positive Synergies

- **Berserkr:** Massive damage output amplified by [Vulnerable]
- **Veiðimaðr:** Ranged physical damage significantly boosted
- **Jötun-Reader (Exploit Design Flaw):** Debuff stacking for extreme damage
- **Physical damage dealers:** Any character dealing Physical damage benefits

### Negative Synergies

- **Undying encounters:** Primary enemies in many areas are immune
- **Magic-heavy parties:** Mystic damage doesn't benefit from Physical vulnerability
- **Solo healing focus:** Using action for debuff means not healing