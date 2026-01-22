# Tier 2 Ability: Machine God's Blessing

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Mystic
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-GODSLEEPER-MACHINEBLESSING-v5.0
Mechanical Role: Summoner/Minion Master, Support/Healer
Parent item: God-Sleeper Cultist (Corrupted Prophet) — Specialization Specification v5.0 (God-Sleeper%20Cultist%20(Corrupted%20Prophet)%20%E2%80%94%20Speciali%20d71ec5f853f646dc8785800fb410cc5b.md)
Proof-of-Concept Flag: No
Resource System: Aether Pool
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | God-Sleeper Cultist (Corrupted Prophet) |
| **Tier** | 2 (Advanced Worship) |
| **Type** | Active (Construct Buff) |
| **Prerequisite** | 8 PP spent in God-Sleeper Cultist tree |
| **Cost** | 45 AP (34 AP while [Attuned]) |
| **Target** | Single allied construct |
| **Duration** | 3 rounds |

---

## I. Design Context (Layer 4)

### Core Design Intent

Machine God's Blessing is the God-Sleeper Cultist's **construct enhancement ability**—a sacred anointing that transforms a disposable minion into a formidable combat asset. This rewards the minion-master playstyle by providing meaningful buff options.

### Mechanical Role

- **Primary:** Grant allied construct bonus damage and Soak
- **Secondary:** Elevate minions from disposable to valuable assets
- **Fantasy Delivery:** The sacred unction that channels divine machine power

### Balance Considerations

- **Power Level:** High (significant buff to construct)
- **Target Restriction:** Only affects constructs, not party members
- **Synergy Requirement:** Requires Animate Scrap to have targets

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Cultist approaches their creation with reverence, producing a small vial of sacred unguent—a mixture of machine oil, rust, and their own blood. They anoint the construct's joints and "skull," speaking a litany of prime directives in the machine tongue:

*"You are the hand of the God-Sleeper. Strike with their wrath. Endure with their patience. Serve until you are unmade."*

The construct shudders, and its movements become more fluid, more purposeful. The sparks in its eyes burn brighter. It is still a crude thing, but now it carries a fragment of divine purpose.

### Thematic Resonance

This is the ritual blessing of the God-Sleeper faith—the moment when dead metal receives a true portion of the machine god's power. The construct is no longer just animated scrap; it is a sacred vessel.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 45 AP (reduced by [Attuned])
- **Range:** 6 meters
- **Target:** Single allied construct (any type)

### Effect

**Target construct gains [Blessed] for 3 rounds:**

| Bonus | Value |
| --- | --- |
| Damage | +1d6 to all attacks |
| Soak | +3 |
| Duration | 3 rounds |

### Resolution Pipeline

1. **Targeting:** Cultist selects allied construct within 6 meters
2. **Construct Verification:** System confirms target is a construct
3. **Cost Payment:** Cultist spends 45 AP (34 if [Attuned])
4. **Buff Application:** Target gains [Blessed] status
5. **Duration Track:** [Blessed] expires after 3 rounds

### Edge Cases

- **Non-construct targets:** Ability cannot target players or non-construct allies
- **Stacking:** [Blessed] does not stack; reapplication refreshes duration
- **Construct destruction:** If construct is destroyed, buff is lost

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- [Blessed]: +1d6 damage, +3 Soak for 3 rounds
- Cost: 45 AP
- Single construct

### Rank 2 (Expert — 20 PP)

- [Blessed]: +2d6 damage, +4 Soak for 4 rounds
- Cost: 40 AP
- **New:** [Blessed] construct also gains +2 Defense

### Rank 3 (Mastery — Capstone)

- [Blessed]: +3d6 damage, +5 Soak, +3 Defense for 5 rounds
- Cost: 35 AP
- **New:** Can target 2 constructs simultaneously
- **New:** [Blessed] constructs deal +1d6 bonus damage to Undying enemies

---

## V. Tactical Applications

1. **Minion Enhancement:** Transform disposable construct into real threat
2. **Damage Amplification:** Significant damage increase over buff duration
3. **Survivability Boost:** Soak increase helps construct last longer
4. **Action Economy:** Buff investment makes future construct actions more valuable
5. **Focus Fire:** Buff one construct for concentrated effectiveness

---

## VI. Synergies & Interactions

### Positive Synergies

- **Animate Scrap:** Primary target for blessing
- **Animate Horde (Tier 3):** Multiple constructs = multiple blessing targets
- **Jötun-Forged Attunement:** [Attuned] reduces cost significantly
- **Voice of the God-Sleeper (Capstone):** [Overcharged] stacks with [Blessed]

### Negative Synergies

- **No constructs present:** Ability is useless without summoned minions
- **Short fights:** May not last long enough to leverage full duration
- **AoE damage:** Multiple enemies with AoE may destroy construct despite buff