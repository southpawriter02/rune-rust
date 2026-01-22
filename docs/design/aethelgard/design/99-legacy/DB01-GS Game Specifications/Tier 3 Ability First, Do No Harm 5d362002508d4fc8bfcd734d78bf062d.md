# Tier 3 Ability: First, Do No Harm

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-FIRSTDONOHARM-v5.0
Mechanical Role: Support/Healer, Tank/Durability
Parent item: Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0 (Bone-Setter%20(Restorer%20of%20Coherence)%20%E2%80%94%20Specializati%20b254ea23b3664889b545c17166ce7e7f.md)
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
| **Specialization** | Bone-Setter (Restorer of Coherence) |
| **Tier** | 3 (Mastery of Anatomy) |
| **Type** | Passive (Reactive) |
| **Prerequisite** | 20 PP spent in Bone-Setter tree |
| **Cost** | None (Passive) |
| **Trigger** | After using single-target healing ability on ally |
| **Duration** | Until end of current round |

---

## I. Design Context (Layer 4)

### Core Design Intent

"First, Do No Harm" is the Bone-Setter's **survivability passive**—a defensive buff that activates after healing, protecting the healer from becoming a casualty while performing their critical role.

### Mechanical Role

- **Primary:** +2 Defense for remainder of round after using single-target healing
- **Secondary:** Ensures Bone-Setter can heal without immediately dying
- **Fantasy Delivery:** Heightened awareness while focused on saving another's life

### Balance Considerations

- **Power Level:** Moderate (conditional defensive bonus)
- **Trigger Requirement:** Must use healing ability first
- **Duration Limited:** Only lasts until end of current round

---

## II. Narrative Context (Layer 2)

### In-World Framing

When focused on saving a life, the Bone-Setter enters a state of **heightened awareness**. Every peripheral threat is catalogued—the Draugr shifting its weight to charge, the Forlorn raising its claw. The medic doesn't stop treating the patient, but their body adjusts automatically, positioning to minimize exposure, presenting smaller target profiles, ready to dodge at the first sign of attack.

### Thematic Resonance

The oath "First, do no harm" takes on new meaning in Aethelgard. A healer who dies is a healer who can't save anyone else. The Bone-Setter's training includes self-preservation as a sacred duty.

---

## III. Mechanical Specification (Layer 3)

### Trigger Condition

- **When:** Bone-Setter uses any **single-target healing ability** on an ally
- **Qualifying Abilities:** Mend Wound, Miracle Worker, etc.
- **Non-Qualifying:** AoE heals, self-heals, non-healing abilities

### Effect

**Bone-Setter gains +2 Defense for the rest of the current combat round.**

### Duration

- Begins immediately after healing ability resolves
- Ends at the end of the current round (all combatants have acted)
- Refreshes if another qualifying healing ability is used

### Resolution Pipeline

1. **Healing Action:** Bone-Setter uses single-target healing on ally
2. **Trigger Check:** Verify ability is single-target healing (not self)
3. **Defense Buff:** Bone-Setter gains +2 Defense
4. **Duration Track:** Buff persists until end of current round
5. **Refresh Check:** If another heal is used, buff is refreshed (not stacked)

### Edge Cases

- **Self-Healing:** Does NOT trigger the passive
- **Multiple Heals:** Refreshes duration, does not stack to +4
- **Out-of-Combat Healing:** Does not trigger (no combat round to protect during)

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +2 Defense after using single-target healing on ally
- Duration: Until end of current round

### Rank 2 (Expert — 20 PP)

- +3 Defense after using single-target healing on ally
- Duration: Until end of next round (extends duration)
- **New:** Also triggers from cleanse abilities (Apply Tourniquet, Administer Antidote, Cognitive Realignment)

### Rank 3 (Mastery — Capstone)

- +4 Defense after using any ally-targeted ability
- Duration: Until end of next round
- **New:** Also grants +1 bonus die to Resolve Checks while active
- **New:** If Bone-Setter is attacked while buff is active, attacker has -1 die (distracted by Bone-Setter's patient)

---

## V. Tactical Applications

1. **Healer Survival:** Reduces likelihood of Bone-Setter being killed mid-combat
2. **Priority Target Protection:** Enemies often target healers; this discourages that
3. **Aggressive Positioning:** Bone-Setter can stay near front line to heal melee allies
4. **Action Economy Reward:** Healing (already good) now also provides defense
5. **Sustained Presence:** Bone-Setter survives longer = more healing over fight

---

## VI. Synergies & Interactions

### Positive Synergies

- **High-frequency healing:** More heals = more Defense uptime
- **Melee Warriors:** Staying near front line to heal them is now safer
- **Triage:** Healing [Bloodied] allies is more efficient AND triggers defense
- **Skjaldmær (Guardian's Taunt):** Combined with taunt, enemies are forced to attack Skjaldmær or face Defense penalty attacking Bone-Setter

### Negative Synergies

- **Ranged/safe positioning:** Less valuable if Bone-Setter is never targeted
- **Low-threat encounters:** Overkill when enemies aren't dangerous
- **Non-healing actions:** Using Anatomical Insight or other abilities doesn't trigger