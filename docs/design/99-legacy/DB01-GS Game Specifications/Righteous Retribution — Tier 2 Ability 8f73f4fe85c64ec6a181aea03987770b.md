# Righteous Retribution — Tier 2 Ability

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-RIGHTEOUSRETRIBUTION-v5.0
Mechanical Role: Damage Dealer, Tank/Durability
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
| **Ability Name** | Righteous Retribution |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 2 (Advanced Extermination) |
| **Type** | Passive (Reactive) |
| **PP Cost** | 4 PP |
| **Resource Cost** | None (Passive) |
| **Trigger** | Successfully resist [Fear] or [Psychic Static] from Undying |

---

## Description

The Iron-Bane's mind is not merely defended; it is **consecrated**. When corruption touches it, the purity of their belief acts as a mirror, reflecting the attack back to its source. The Undying's own weapon becomes its punishment.

Their mind is not just a fortress that resists—it is a perfectly polished surface that catches hostile, corrupted "data-streams" and **hurls them back with contemptuous force**.

> *"You dare attack my mind? Let me show you what true conviction feels like."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Reflection Chance | Vengeance on Reflect | Special |
| --- | --- | --- | --- | --- |
| **Rank 2** | Train ability (4 PP) | 50% | +0 | Reflected effect bypasses target's save |
| **Rank 3** | Train Capstone | 75% | +15 | Reflection also applies [Disoriented] for 1 round |

### Formula

```
ReflectionTrigger = (ResistSuccess == true) AND 
                   (Effect ∈ {Fear, PsychicStatic}) AND
                   (Source.Faction ∈ {Undying, Mechanical})

ReflectionChance = ChanceTable[Rank]
  ChanceTable[R2] = 0.50 (50%)
  ChanceTable[R3] = 0.75 (75%)

OnReflection:
  Apply(OriginalEffect, Source, BypassSave=true)
  If Rank >= 3: Apply([Disoriented], Source, Duration=1)
  If Rank >= 3: GainVengeance(15)
```

### Resolution Pipeline

1. **Trigger Check:** Iron-Bane successfully resists [Fear] or [Psychic Static]
2. **Source Validation:** Confirm source is Undying/Mechanical
3. **Reflection Roll:** Roll percentage vs reflection chance (50%/75%)
4. **On Success:**
    - Original effect applied to source (NO save allowed)
    - R3: [Disoriented] also applied to source
    - R3: Iron-Bane gains +15 Vengeance
5. **On Failure:** No reflection; resistance still successful

---

## Worked Examples

### Example 1: Rank 2 — Basic Reflection

**Situation:** God-Sleeper Cultist (Undying-aligned) uses [Psychic Static] on Grizelda

```
Step 1: Grizelda resists [Psychic Static] (via Indomitable Will)
Step 2: Source check — Cultist is Undying-aligned ✓
Step 3: Reflection roll — 50% chance
Roll: d100 = 38 → SUCCESS (≤50)

Result:
  - [Psychic Static] applied to Cultist (no save)
  - Cultist now suffers own debuff (-2 to all checks)
  - Combat Log: "With a glare of pure contempt, Grizelda turns 
    the psychic attack back on its source!"
```

### Example 2: Rank 2 — Reflection Fails

**Situation:** Same scenario, unlucky roll

```
Step 1: Grizelda resists [Fear] successfully
Step 2: Source is Undying ✓
Step 3: Reflection roll — 50% chance
Roll: d100 = 67 → FAILURE (>50)

Result:
  - Fear still resisted (Grizelda is fine)
  - No reflection occurs
  - Indomitable Will Vengeance still gained (+20 from Fear resist)
```

### Example 3: Rank 3 — Full Reflection with Bonuses

**Situation:** Iron Sentinel broadcasts [Fear] aura at mastery Grizelda

```
Step 1: Grizelda resists [Fear] (9d10 pool from Indomitable Will R3)
Step 2: Source is Undying ✓
Step 3: Reflection roll — 75% chance
Roll: d100 = 42 → SUCCESS

Result:
  - [Fear] applied to Iron Sentinel (no save) — boss now [Feared]!
  - [Disoriented] applied to Sentinel for 1 round
  - Grizelda gains +15 Vengeance (from reflection)
  - Grizelda also gains +30 Vengeance (from Indomitable Will R3 resist)
  - Total Vengeance gained: +45

Combat Log: "The Sentinel's fear protocol backfires catastrophically!
            It is now [Feared] and [Disoriented]!"
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Failed Resist** | Effect applies to Grizelda; no reflection possible |
| **Wrong Source** | Organic/non-Mechanical sources cannot be reflected |
| **Wrong Effect** | [Dominated], [Charmed], etc. do not trigger reflection |
| **Reflection Roll Fails** | Resist still works; just no counter-attack |
| **Immune Target** | If source is immune to own effect, reflection deals no mechanical effect (but looks cool) |

---

## Tactical Applications

1. **Punish Psychic Attackers:** Enemies learn targeting Iron-Bane is dangerous
2. **Passive Counter-Attack:** Damage/debuff enemies without spending actions
3. **Boss Disruption:** [Feared] or [Disoriented] on boss = lost actions
4. **Vengeance Generation (R3):** +15 per reflection stacks with Indomitable Will gains
5. **Deterrent Effect:** High-rank Iron-Banes discourage psychic strategies entirely

---

## Integration Notes

### Synergies

- **Indomitable Will I:** Higher resist chance = more reflection opportunities
- **Heart of Iron:** Fear immunity still triggers vs Fear *attempts* (enemy tries, fails, gets reflected)
- **Anti-Undying Focus:** Maximum value against psychic-heavy Undying factions
- **God-Sleeper Counter:** Hard counter to Cultist psychic builds

### Anti-Synergies

- **Non-Psychic Enemies:** Zero trigger opportunities vs physical-only threats
- **Organic Attackers:** Thul rhetoric, beast fear effects cannot be reflected
- **Failed Resists:** Must succeed to trigger; failure = no reflection

### Design Note: Indomitable Will R3 Integration

At Indomitable Will R3, reflection becomes 100% reliable. Righteous Retribution at lower ranks provides the 50%/75% chance before that mastery is achieved. This creates progression:

- Early game: 50% chance (R2 Righteous Retribution)
- Mid game: 75% chance (R3 Righteous Retribution)
- Late game: 100% chance (Indomitable Will R3 integration)

### Combat Log Example

```
> The Cultist's Static Lash assaults your mind...
> Your Indomitable Will helps you shrug off the mental assault!
> With a glare of pure contempt, you turn the psychic attack 
> back on its source! [Righteous Retribution] triggers!
> The Cultist is blasted by its own corrupted code and is now 
> afflicted with [Psychic Static]!
```