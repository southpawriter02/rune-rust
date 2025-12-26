# Tier 3 Ability: One with the Rot

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-ONEWITHTHEROT-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 3
Template Validated: No
Voice Validated: No

## Core Identity

**One with the Rot** represents the apex of your symbiotic relationship with the Runic Blight. You don't just resist decay—you metabolize it. Poison becomes sustenance, corruption becomes vitality.

---

## Mechanics

### Always Active (Passive)

**Metabolize Decay:**

- When you would apply [Poisoned] to an already-poisoned enemy, **heal HP equal to their current poison stacks**
- When standing in a [Toxic Cloud] you created: **+2 HP per turn** (stacks with Blighted Symbiosis)

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 5 | Heal = target's poison stacks; +2 HP in own clouds |
| 2 | 25 | Heal = stacks × 1.5; +3 HP in clouds |
| 3 | 40 | Heal = stacks × 2; +5 HP in clouds; [Corrupted] terrain also heals you |

### Healing Calculation

| Target Stacks | Rank 1 Heal | Rank 2 Heal | Rank 3 Heal |
| --- | --- | --- | --- |
| 2 | 2 HP | 3 HP | 4 HP |
| 4 | 4 HP | 6 HP | 8 HP |
| 5 | 5 HP | 7 HP | 10 HP |

---

## Tactical Applications

**Sustained Combat Loop:**

```jsx
Venom-Laced Shiv on 5-stack target: +5 HP heal
Stand in own Toxic Cloud: +2 HP/turn
Blighted Symbiosis in hazard: +5 HP/turn

Result: Constant HP regeneration while fighting
```

**Multi-Target Healing:**

```jsx
3 enemies with [Poisoned]: Corrosive Haze all 3
Healing: Sum of all targets' stacks

Result: AoE poison = AoE healing
```

**Synergies:**

- **Blighted Symbiosis:** Environmental + cloud healing stack
- **Venom-Laced Shiv:** Primary stack trigger for healing
- **Corrosive Haze:** Multi-target healing via AoE poison

---

## Failure Modes

<aside>
⚠️

**Requires Existing Stacks** — No healing on first poison application. Must maintain DoT for sustain.

</aside>

<aside>
⚠️

**Low Healing Values** — This is sustain, not burst healing. Won't save you from spike damage.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** Self-Sustain, Attrition Fighter

**Prerequisite:** Tier 2 (2 abilities)