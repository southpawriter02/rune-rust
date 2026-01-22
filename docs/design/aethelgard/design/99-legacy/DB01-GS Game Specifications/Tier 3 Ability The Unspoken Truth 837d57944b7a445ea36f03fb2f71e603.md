# Tier 3 Ability: The Unspoken Truth

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-THUL-UNSPOKENTRUTH-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0 (Thul%20(J%C3%B6tun-Reader%20Diagnostician)%20%E2%80%94%20Specialization%206740a2ac8e2a4a4fafa8694c56818d48.md)
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
| **Specialization** | Thul (Jötun-Reader Diagnostician) |
| **Tier** | 3 (Mastery of the Mind) |
| **Type** | Passive (Upgrade) |
| **Prerequisite** | 20 PP spent in Thul tree + Demoralizing Diatribe |
| **Cost** | None (Passive) |

---

## I. Design Context (Layer 4)

### Core Design Intent

The Unspoken Truth is a **passive upgrade** to Demoralizing Diatribe—enhancing Critical Successes to also inflict [Feared]. This rewards mastery and creates devastating control combinations.

### Mechanical Role

- **Primary:** On Critical Success with Demoralizing Diatribe, also inflict [Feared]
- **Secondary:** Upgrades existing ability rather than adding new one
- **Fantasy Delivery:** The insight so profound it reveals the target's deepest fears

### Balance Considerations

- **Power Level:** High (adds [Feared] on Crit)
- **Conditional:** Only triggers on Critical Success
- **Dependency:** Requires Demoralizing Diatribe to be useful

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Thul's insight has reached a terrifying depth. When they truly *see* an enemy—when their analysis achieves perfect clarity—they perceive not just tactical weaknesses but **existential ones**. The deep-seated fears, the buried traumas, the unspoken truths the enemy hides even from themselves.

And the Thul speaks those truths aloud.

The effect is devastating. The enemy doesn't just become disoriented; they become **terrified** of the creature who can see into their very soul.

### Thematic Resonance

Some truths are better left unspoken. The Thul speaks them anyway. This is the dark side of perfect insight—the ability to weaponize someone's deepest vulnerabilities.

---

## III. Mechanical Specification (Layer 3)

### Trigger Condition

- **When:** Thul uses Demoralizing Diatribe and achieves a **Critical Success** on the opposed check
- **Critical Success:** Exceed target's roll by 10+ (or system-specific critical threshold)

### Effect

**On Critical Success with Demoralizing Diatribe:**

- Target gains **[Disoriented]** (standard effect) AND **[Feared]** (bonus effect)
- Both debuffs have same duration (2 rounds base, modified by Demoralizing Diatribe rank)

### [Feared] Effect Reminder

- Must move away from source of fear if possible
- -2 dice to all offensive actions
- Cannot willingly approach source of fear
- Duration: Same as [Disoriented] from Demoralizing Diatribe

### Resolution Pipeline

1. **Demoralizing Diatribe Used:** Thul activates Demoralizing Diatribe
2. **Opposed Check:** Roll WILL + Rhetoric vs Target WILL
3. **Critical Check:** Determine if result is Critical Success
4. **Standard Effect:** Apply [Disoriented] as normal
5. **Bonus Effect (Crit only):** Also apply [Feared] for same duration

### Integration with Demoralizing Diatribe Ranks

- **Rank 1:** Crit adds [Feared] for 2 rounds
- **Rank 2:** Crit adds [Feared] for 3 rounds + 5 Psychic Stress
- **Rank 3:** Crit adds [Feared] for 4 rounds

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Critical Success on Demoralizing Diatribe also inflicts [Feared]
- Duration matches [Disoriented] duration

### Rank 2 (Expert — 20 PP)

- Critical Success also inflicts [Feared] + 10 Psychic Stress
- **New:** Solid Success (exceed by 5+) also adds [Feared] for 1 round

### Rank 3 (Mastery — Capstone)

- Any Success on Demoralizing Diatribe adds [Feared] for at least 1 round
- Critical Success adds [Feared] for full duration + 15 Psychic Stress
- **New:** [Feared] from The Unspoken Truth cannot be resisted by normal means (requires magical cleanse or specific abilities)

---

## V. Tactical Applications

1. **Control Escalation:** Turns good roll into devastating double-debuff
2. **Action Denial:** [Feared] forces movement away, wasting enemy turn
3. **Offensive Penalty:** -2 dice to offensive actions severely cripples output
4. **Positioning Control:** Enemy must flee, disrupting their tactics
5. **Psychological Dominance:** Complete mental shutdown of intelligent enemies

---

## VI. Synergies & Interactions

### Positive Synergies

- **Keeper of Sagas I:** Higher Rhetoric bonus = more Critical Successes
- **High-WILL builds:** Better opposed checks = more Crits
- **Demoralizing Diatribe Rank 2+:** Longer durations for both debuffs
- **Party coordination:** Feared enemy is vulnerable while fleeing

### Negative Synergies

- **Mindless enemies:** Demoralizing Diatribe doesn't work; passive is irrelevant
- **Low opposed check success:** If base ability fails, upgrade never triggers
- **Single-target limitation:** Only affects one enemy per use