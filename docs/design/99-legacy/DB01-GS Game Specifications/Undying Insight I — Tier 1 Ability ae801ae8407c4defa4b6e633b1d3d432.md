# Undying Insight I — Tier 1 Ability

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-UNDYINGINSIGHT-v5.0
Mechanical Role: Utility/Versatility
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Undying Insight I |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 1 (Foundational Vows) |
| **Type** | Passive |
| **PP Cost** | 3 PP |
| **Resource Cost** | None (Passive) |
| **Target** | Self (triggers on investigation) |

---

## Description

The Iron-Bane has spent countless hours obsessively studying their hated foe. They do not merely see a monster—they see a **living schematic**. Their gaze penetrates rusted plating to trace power conduits, spot unshielded servos, and identify precise structural weaknesses in the machine's profane anatomy.

This knowledge is not academic. It is **religious**. To know the enemy is the first step in destroying them.

> *"I don't see monsters. I see machines with failure points."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Effect |
| --- | --- | --- |
| **Rank 1** | Train ability (3 PP) | +2 dice to WITS checks investigating Undying/Mechanical weaknesses |
| **Rank 2** | Train 2 Tier 2 abilities | +3 dice; auto-identify Undying sub-type on sight |
| **Rank 3** | Train Capstone | +4 dice; Critical Success reveals exploitable "glitch" or AI Archetype; enables Annihilate Iron Heart |

### Formula

```
InvestigationBonus = BaseWITS + BonusDice[Rank]

Where:
  BonusDice[R1] = 2
  BonusDice[R2] = 3
  BonusDice[R3] = 4

Condition: Target.Faction ∈ {Undying, Mechanical}
```

### Resolution Pipeline

1. **Trigger:** Iron-Bane uses `investigate` command on target
2. **Faction Check:** Verify target is Undying or Mechanical
3. **Bonus Application:** Add rank-based bonus dice to WITS pool
4. **Roll Resolution:** Standard investigation check vs DC
5. **Information Delivery:** On success, reveal weaknesses; on Critical Success (R3), flag target as [Iron_Heart_Exposed]

---

## Worked Examples

### Example 1: Rank 1 Investigation (Standard Success)

**Situation:** Grizelda (WITS 3, R1) investigates a Rusted Warden

```
Base Pool: 3 (WITS)
Undying Insight Bonus: +2 dice
Total Pool: 5d10 vs DC 12

Roll: [8, 6, 4, 9, 7] → 3 successes (6+ on d10)
Result: SUCCESS — GM reveals Warden's weak point (exposed servo joint)
```

### Example 2: Rank 2 Auto-Identification

**Situation:** Combat begins against unknown mechanical enemy

```
Trigger: Combat start, Iron-Bane sees mechanical enemy
Auto-ID (R2): Immediately identify as "Iron Husk - Warden Pattern"
Information: Basic behavioral patterns, standard armament, known weaknesses
No check required for common Undying types
```

### Example 3: Rank 3 Critical Success (Capstone Setup)

**Situation:** Grizelda (WITS 4, R3) investigates an Iron Hulk boss

```
Base Pool: 4 (WITS)
Undying Insight Bonus: +4 dice
Total Pool: 8d10 vs DC 15

Roll: [10, 9, 8, 7, 6, 5, 3, 2] → 5 successes + 1 crit (10)
Result: CRITICAL SUCCESS
  - Reveals all weaknesses and resistances
  - Reveals AI Archetype: "Berserker Protocol"
  - Target flagged as [Iron_Heart_Exposed]
  - Annihilate Iron Heart now usable against this target
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Wrong Faction** | No bonus dice; standard investigation only |
| **Failed Check** | No information revealed; may retry next round |
| **Non-Critical at R3** | Standard info revealed but [Iron_Heart_Exposed] NOT applied; cannot use Annihilate |
| **Unknown Variant** | Auto-ID (R2+) may fail on rare/unique Undying; requires investigation |

---

## Tactical Applications

1. **Pre-Combat Intel:** Investigate before engaging to identify priority targets and weak points
2. **Capstone Setup:** R3 Critical Success is **required** for Annihilate Iron Heart — always investigate high-value targets
3. **Party Coordination:** Share discovered weaknesses with allies for focused damage
4. **Action Economy:** R2+ auto-identification is free, saving actions for combat
5. **Risk Assessment:** Identify enemy AI Archetype to predict behavior patterns

---

## Integration Notes

### Synergies

- **Annihilate Iron Heart (Capstone):** Critical Success investigation is a hard prerequisite
- **Corrosive Strike:** Identified weak points inform where to apply [Corroded]
- **Jötun-Reader:** Combined analysis creates comprehensive enemy profiles (stacking specialists)
- **Party Damage Dealers:** Shared intel enables coordinated burst damage

### Anti-Synergies

- **Non-Undying Campaigns:** Zero benefit against organic enemies — purely specialist ability
- **Fast Combat:** If party kills before investigation, ability goes unused
- **Low WITS Builds:** Reduces effectiveness despite bonus dice

### Thematic Notes

This ability establishes the Iron-Bane as a **scholar-slayer** — their hatred is not blind rage but informed, targeted, and devastatingly effective. Every Iron-Bane initiate memorizes the seventeen primary classifications of Undying and the forty-three known weak points in standard Iron Husk armor.