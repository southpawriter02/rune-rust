# Tier 1 Ability: The Sage's Insight

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-THUL-SAGESINSIGHT-v5.0
Mechanical Role: Controller/Debuffer, Utility/Versatility
Parent item: Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0 (Thul%20(J%C3%B6tun-Reader%20Diagnostician)%20%E2%80%94%20Specialization%206740a2ac8e2a4a4fafa8694c56818d48.md)
Proof-of-Concept Flag: No
Resource System: Corruption/Psychic Stress, Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Thul (Jötun-Reader Diagnostician) |
| **Tier** | 1 (Foundational Rhetoric) |
| **Type** | Active (Analysis + Debuff) |
| **Prerequisite** | Unlock Thul Specialization (10 PP) |
| **Cost** | 30 Stamina + small Psychic Stress |
| **Target** | Single enemy |
| **Duration** | 2 rounds ([Analyzed]) |

---

## I. Design Context (Layer 4)

### Core Design Intent

The Sage's Insight is the Thul's **signature analysis ability**—a focused examination that reveals enemy weaknesses AND marks them for the party. This establishes the Thul's core loop: analyze, then exploit.

### Mechanical Role

- **Primary:** Reveal one Resistance and one Vulnerability of target
- **Secondary:** Apply [Analyzed] debuff (+1 Accuracy die for allies)
- **Self-Cost:** Small Psychic Stress from intense analysis
- **Fantasy Delivery:** The penetrating gaze that sees through pretense to weakness

### Balance Considerations

- **Power Level:** High (information + party-wide accuracy buff)
- **Self-Damage Trade-off:** Psychic Stress cost creates meaningful risk
- **Duration Limited:** 2 rounds requires tactical timing

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Thul's eyes narrow, focusing with predatory intensity on a single opponent. Every twitch, every favored stance, every piece of equipment is catalogued and cross-referenced against the vast archive of their memory. The creature's strengths are noted—and more importantly, the gaps in those strengths are mapped.

"There," the Thul announces, voice flat and clinical. "It favors the left. Scarring suggests a past injury to the right shoulder. Strike high and to the right."

But such intense analysis has a cost. Staring too deeply into an enemy's nature means glimpsing the corruption that drives them.

### Thematic Resonance

The Thul's power is knowledge—but knowledge in Aethelgard comes with a price. Every insight into an enemy's weakness is also a glimpse into the world's fundamental brokenness.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 30 Stamina + **small Psychic Stress** (3-5, TBD by system)
- **Range:** 8 meters (visual range)
- **Target:** Single enemy (any type)

### Resolution

**Analysis Check:**

- Roll: WITS + Investigation (or Perception)
- DC: Target's Mental Defense or static DC based on creature rarity

### On Success

**Dual Effect:**

1. **Information Reveal:**
    - Reveals **one Resistance** of the target
    - Reveals **one Vulnerability** of the target
2. **[Analyzed] Debuff:**
    - Target gains [Analyzed] for 2 rounds
    - All allies gain **+1 Accuracy die** against the [Analyzed] target

### [Analyzed] Effect

- Duration: 2 rounds
- Benefit: Allies attacking target gain +1d10 to Accuracy rolls
- Does not stack with itself (reapplication refreshes duration)

### Resolution Pipeline

1. **Targeting:** Thul selects enemy within 8 meters
2. **Cost Payment:** Thul spends 30 Stamina
3. **Stress Cost:** Thul gains small Psychic Stress (3-5)
4. **WITS Check:** Roll WITS + Investigation vs target's defense
5. **Success/Failure:** On success, reveal Resistance + Vulnerability
6. **Debuff Application:** Apply [Analyzed] to target for 2 rounds

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Reveal 1 Resistance + 1 Vulnerability
- Apply [Analyzed] for 2 rounds (+1 Accuracy die)
- Cost: 30 Stamina + 3 Psychic Stress

### Rank 2 (Expert — 20 PP)

- Reveal 2 Resistances + 2 Vulnerabilities
- Apply [Analyzed] for 3 rounds (+1 Accuracy die)
- Cost: 25 Stamina + 2 Psychic Stress
- **New:** Also reveals target's current HP percentage

### Rank 3 (Mastery — Capstone)

- Reveal ALL Resistances + ALL Vulnerabilities
- Apply [Analyzed] for 4 rounds (+2 Accuracy dice)
- Cost: 20 Stamina + 0 Psychic Stress
- **New:** [Analyzed] also grants allies +1 damage die against target
- **New:** Information persists permanently (target always shows revealed info)

---

## V. Tactical Applications

1. **Combat Opening:** Analyze priority target before party engages
2. **Damage Optimization:** Reveal vulnerabilities to maximize party damage
3. **Party Accuracy Boost:** [Analyzed] makes coordinated attacks more reliable
4. **Boss Fights:** Essential for understanding powerful enemy capabilities
5. **Tactical Planning:** Information enables better strategic decisions

---

## VI. Synergies & Interactions

### Positive Synergies

- **Jötun-Reader (Analyze Weakness):** Combined analysis provides complete picture
- **High-accuracy attackers:** [Analyzed] bonus stacks with their inherent accuracy
- **Berserkr:** More hits = more Fury generation
- **Veiðimaðr:** Accuracy bonus enhances already-precise ranged attacks
- **Keeper of Sagas I:** Bonus die improves success rate

### Negative Synergies

- **Short fights:** May not last long enough to leverage information
- **Multiple weak enemies:** Single-target analysis less efficient vs swarms
- **Mindless enemies:** Still works mechanically, but less strategic value