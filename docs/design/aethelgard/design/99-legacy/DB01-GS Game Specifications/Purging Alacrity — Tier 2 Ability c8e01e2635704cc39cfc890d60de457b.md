# Purging Alacrity — Tier 2 Ability

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-PURGINGALACRITY-v5.0
Mechanical Role: Damage Dealer
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Stamina, Vengeance
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Purging Alacrity |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 2 (Advanced Extermination) |
| **Type** | Passive (Combat Enhancement) |
| **PP Cost** | 4 PP |
| **Resource Cost** | None (Passive) |
| **Trigger** | Apply [Corroded] or [Bleeding] to Undying |

---

## Description

The very act of successfully wounding their hated foe—of seeing the profane machine begin to bleed or corrode under their sanctified assault—fills the Iron-Bane with a righteous, invigorating speed. They are not just fighting; they are performing a holy act of purification, and this conviction makes them faster, more alert, and harder to hit.

The zeal of the crusader is a tangible force.

> *"Each wound I inflict is a prayer answered. Each prayer makes me faster."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Defense Bonus | Duration | Special |
| --- | --- | --- | --- | --- |
| **Rank 2** | Train ability (4 PP) | +3 Defense | 1 round | — |
| **Rank 3** | Train Capstone | +3 Defense, +2 Vigilance | 2 rounds | Also reduces next attack Stamina cost by 10 |

### Formula

```
TriggerCondition = (EffectApplied ∈ {Corroded, Bleeding}) AND
                   (Target.Faction ∈ {Undying, Mechanical}) AND
                   (Source == Iron-Bane)

BuffEffect[R2] = {Defense: +3, Duration: 1 round}
BuffEffect[R3] = {Defense: +3, Vigilance: +2, Duration: 2 rounds,
                  NextAttackStaminaReduction: 10}

Note: Buff refreshes on re-trigger (does not stack)
```

### Resolution Pipeline

1. **Trigger Detection:** Iron-Bane successfully applies [Corroded] or [Bleeding]
2. **Target Validation:** Confirm target is Undying/Mechanical
3. **Buff Application:** Apply [Purging Alacrity] status to Iron-Bane
4. **Duration Tracking:** 1 round (R2) or 2 rounds (R3)
5. **Expiration:** Buff fades at end of duration; can be refreshed by new trigger

---

## Worked Examples

### Example 1: Rank 2 — Basic Defensive Buff

**Situation:** Grizelda (Defense 12) uses Corrosive Strike on Iron Husk

```
Action: Corrosive Strike → Solid Hit
Effect Applied: [Corroded] to Iron Husk (Undying) ✓

Trigger: Purging Alacrity activates
Buff: [Purging Alacrity] — +3 Defense for 1 round

Grizelda's Defense: 12 + 3 = 15
Duration: Until end of next round

Combat Log: "The act of purification fills Grizelda with righteous 
            speed! [Purging Alacrity] activates!"
```

### Example 2: Rank 3 — Extended Buff with Vigilance

**Situation:** Mastery Grizelda applies [Corroded] via Chains of Decay

```
Action: Chains of Decay → Hits 3 Undying (all [Corroded])
Note: Multiple applications in one action = single buff (refreshed)

Buff: [Purging Alacrity] R3
  - Defense: +3
  - Vigilance: +2 (better reaction checks)
  - Duration: 2 rounds
  - Next attack costs -10 Stamina

Grizelda Stats During Buff:
  Defense: 12 + 3 = 15
  Vigilance: 8 + 2 = 10
  Next Sanctified Steel: 30 - 10 = 20 Stamina (extremely efficient)
```

### Example 3: Buff Refresh Mid-Combat

**Situation:** Grizelda maintains pressure with repeated debuffs

```
Round 1: Corrosive Strike → [Corroded] applied
  [Purging Alacrity] active (2 rounds remaining at R3)

Round 2: Sanctified Steel (benefits from -10 Stamina)
  [Purging Alacrity] still active (1 round remaining)

Round 3: Corrosive Strike → [Corroded] reapplied
  [Purging Alacrity] REFRESHED (back to 2 rounds)
  Stamina reduction resets

Result: Sustained offensive pressure = sustained defensive buff
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Wrong Faction** | Debuffing organic enemies does not trigger buff |
| **Wrong Debuff** | [Slowed], [Disoriented], etc. do not trigger — only [Corroded] or [Bleeding] |
| **Missed Attack** | If debuff attack misses, no debuff applied, no buff gained |
| **Duration Expires** | Buff fades; must re-trigger with new debuff application |
| **No Stacking** | Multiple triggers refresh duration but don't stack Defense bonus |

---

## Tactical Applications

1. **Aggressive Defense:** Offense triggers defense — stay on the attack to stay protected
2. **Dodge Tank Window:** +3 Defense makes Iron-Bane significantly harder to hit post-debuff
3. **Action Economy (R3):** Stamina reduction enables more attacks per combat
4. **Vigilance Boost (R3):** Better reaction checks for interrupts and awareness
5. **Rhythm Combat:** Maintain buff by consistently applying debuffs each round

---

## Integration Notes

### Synergies

- **Corrosive Strike:** Primary trigger for buff activation
- **Chains of Decay:** AoE debuff = guaranteed buff trigger
- **Sanctified Steel:** Benefits from R3 Stamina reduction
- **Aggressive Playstyle:** Rewards staying in melee and pressing attacks

### Anti-Synergies

- **Non-Undying Campaigns:** No trigger opportunities vs organic enemies
- **Passive/Defensive Play:** Buff requires offensive action to maintain
- **Ranged Builds:** Debuff abilities are melee; can't trigger from distance

### Status Effect: [Purging Alacrity]

```
[Purging Alacrity]
- Source: Self-applied on successful debuff
- Defense: +3
- Vigilance: +2 (R3 only)
- Duration: 1 round (R2) / 2 rounds (R3)
- Special (R3): Next attack costs -10 Stamina
- Stacking: No (refreshes duration on re-trigger)
```

### Combat Log Example

```
> Grizelda's Corrosive Strike eats at the Warden's armor!
> It is now [Corroded]!
> The act of purification fills Grizelda with righteous speed!
> [Purging Alacrity] activates!
> (Defense +3 for 2 rounds, next attack -10 Stamina)
```