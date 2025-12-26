# Indomitable Will I — Tier 1 Ability

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-INDOMITABLEWILL-v5.0
Mechanical Role: Tank/Durability
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Vengeance
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Indomitable Will I |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 1 (Foundational Vows) |
| **Type** | Passive (Defensive) |
| **PP Cost** | 3 PP |
| **Resource Cost** | None (Passive) |
| **Trigger** | WILL check vs [Fear] or [Psychic Static] |

---

## Description

The Undying broadcast their corrupted signals—the psychic hum of broken code, the static of the Great Silence, the whispered promises of the machine gods. Lesser minds crumble. The Iron-Bane **laughs**.

Their faith is a fortress. Each prayer memorized, each scripture internalized, each vow renewed—these are the stones of an impregnable wall. The corruption crashes against it and breaks, and from each failed assault, the Iron-Bane draws **strength**.

> *"Your whispers are nothing. I have heard the screams of my brothers. I have not broken."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Bonus Dice | Vengeance on Resist | Special |
| --- | --- | --- | --- | --- |
| **Rank 1** | Train ability (3 PP) | +2 dice | Fear: +20, Static: +10 | — |
| **Rank 2** | Train 2 Tier 2 abilities | +3 dice | Fear: +25, Static: +15 | Also applies to [Charmed] |
| **Rank 3** | Train Capstone | +4 dice | Fear: +30, Static: +20 | Integrates Righteous Retribution: 100% reflect chance |

### Formula

```
ResolvePool = BaseWILL + BonusDice[Rank]
VengeanceOnSuccess = VengeanceTable[EffectType][Rank]

Where:
  BonusDice[R1] = 2
  BonusDice[R2] = 3
  BonusDice[R3] = 4

  VengeanceTable[Fear][R1-R3] = 20, 25, 30
  VengeanceTable[PsychicStatic][R1-R3] = 10, 15, 20
  VengeanceTable[Charmed][R2-R3] = 15, 20

Condition: Effect ∈ {Fear, Psychic Static, Charmed (R2+)}
```

### Resolution Pipeline

1. **Trigger:** Enemy attempts to inflict [Fear], [Psychic Static], or [Charmed] (R2+)
2. **Bonus Application:** Add rank-based bonus dice to WILL pool
3. **Resolve Check:** Roll augmented pool vs effect DC
4. **Success Branch:** If successful:
    - Effect resisted (does not apply)
    - Generate Vengeance based on effect type
    - At R3: Automatically reflect effect back to caster (no save)
5. **Failure Branch:** Effect applies normally; no Vengeance generated

---

## Worked Examples

### Example 1: Rank 1 — Resisting Fear

**Situation:** Forlorn attempts [Fear] on Grizelda (WILL 3, R1)

```
Effect: [Fear] DC 14
Base Pool: 3 (WILL)
Indomitable Will Bonus: +2 dice
Total Pool: 5d10 vs DC 14

Roll: [9, 7, 8, 5, 6] → 4 successes
Result: SUCCESS — Fear resisted
Vengeance: +20 → Grizelda's fury grows
```

### Example 2: Rank 2 — Resisting Psychic Static with Expanded Coverage

**Situation:** God-Sleeper Cultist uses [Psychic Static] aura

```
Effect: [Psychic Static] DC 12
Base Pool: 4 (WILL)
Indomitable Will Bonus: +3 dice (R2)
Total Pool: 7d10 vs DC 12

Roll: [10, 8, 6, 4, 7, 9, 5] → 5 successes
Result: SUCCESS — Static resisted
Vengeance: +15 → Psychic assault becomes fuel

Note: At R2, [Charmed] effects would also trigger this bonus
```

### Example 3: Rank 3 — Reflection Integration

**Situation:** Iron Sentinel broadcasts [Fear] at mastery-rank Grizelda

```
Effect: [Fear] DC 16 (boss-level)
Base Pool: 5 (WILL)
Indomitable Will Bonus: +4 dice (R3)
Total Pool: 9d10 vs DC 16

Roll: [10, 9, 8, 7, 6, 8, 5, 4, 7] → 7 successes
Result: SUCCESS — Fear resisted
Vengeance: +30 → Maximum generation

R3 Reflection: 100% chance triggers
Effect: [Fear] automatically applied to Iron Sentinel (no save)
Combat Log: "With a glare of pure contempt, Grizelda turns the 
            psychic attack back on its source!"
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Failed Resolve Check** | Effect applies normally; no Vengeance generated; no reflection |
| **Non-Qualifying Effect** | [Dominated], [Disoriented] do not trigger bonus (use Heart of Iron for broader coverage) |
| **Non-Psychic Attack** | Purely physical effects bypass this ability entirely |
| **Organic Source (R3 Reflect)** | Reflection only works if source is Undying/Mechanical |

---

## Tactical Applications

1. **Psychic Sentinel:** Stand firm where other Warriors break — ideal for Jötunheim/Forlorn encounters
2. **Vengeance Engine (Defensive):** Generate significant Vengeance by *being attacked* — passive resource gain
3. **Anti-God-Sleeper:** Hard counter to Cultist psychic-focused tactics
4. **R3 Counter-Play:** At mastery, enemies fear targeting you — their own weapons become liabilities
5. **Trauma Economy Interaction:** Positive interaction — resisting Stress-inducing effects generates combat resources

---

## Integration Notes

### Synergies

- **Righteous Retribution (T2):** Stacks reflection chances at lower ranks; R3 integrates fully
- **Heart of Iron (T3):** Combined creates near-immunity to all psychic effects
- **Vengeance Spenders:** Defensive Vengeance generation enables offense without attacking
- **Party Protection:** Allies benefit when Iron-Bane draws psychic aggro

### Anti-Synergies

- **Non-Psychic Enemies:** Zero benefit against Blighted Beasts or physical-only threats
- **Low WILL Builds:** Bonus dice can't compensate for fundamentally weak base
- **Failed Resists:** All benefits require successful check — no consolation prize

### Thematic Notes

This ability is the **weaponization of faith**. The Iron-Bane's mind is not merely defended; it is consecrated. When corruption touches it, the purity of their belief acts as a mirror, reflecting the attack back to its source. Those who seek to corrupt the faithful will be burned by their own sin.

### Combat Log Examples

```
> The Cultist's Static Lash assaults your mind...
> Your Indomitable Will fortifies your resolve! (Resolve Check: Success!)
> You stand firm against the mental assault! Your Vengeance grows!
> You gain 10 Vengeance!
```

```
> The Forlorn's wail of despair washes over Grizelda...
> Her Indomitable Will holds firm! (Resolve Check: Success!)
> With a glare of pure contempt, she turns the psychic attack 
> back on its source! [Righteous Retribution] triggers!
> The Forlorn is blasted by its own corrupted code and is now [Feared]!
```