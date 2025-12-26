# [Silenced] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Silenced]`

**Category:** Debuff (Ability Denial)

**Type:** Anti-caster control (highly specialized)

**Summary:**

Premier anti-caster tool. Prevents use of ANY ability with `[Vocal]` component. Devastating to casters/orators, nearly harmless to martial builds. Creates rock-paper-scissors dynamic.

**Thematic Framing (Layer 2):**

Command-line interface corrupted. Ability to project will through vocalized commands revoked. Physical throat damage, counter-resonant frequency, or magical curse chokes words of power.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 2 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (duration resets, cannot be "more silenced")

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** 🤐 (zipper-mouth face)

---

## III. Mechanical Implementation

### Primary Effect: Vocal Ability Denial

**Effect:**

```
Cannot use ANY ability with [Vocal] flag
```

**Vocal Component Flag System:**

Every ability has hidden `[Vocal]` boolean flag.

**Abilities WITH `[Vocal]` Flag (BLOCKED):**

- Almost all **Galdr-caster** spells (chanted magic)
- All **Skald/Thul** Sagas, Dirges, Orations
- All **Skald-Screamer** abilities
- Mystic prayers/chants
- Warrior vocal abilities (Guardian's Taunt, Unleashed Roar)

**Abilities WITHOUT `[Vocal]` Flag (ALLOWED):**

- Standard `attack` and `move` commands
- Physical martial abilities (Strike, Shadow Strike, Precise Thrust)
- Purely mental/gestural Mystic abilities (Echo-Caller's Psychic Lash)

**Rationale:** Highly strategic/specialized. Completely shuts down certain builds, minimal effect on others.

**Variable Definitions:**

- **Duration:** 2 rounds (typical)
- **Vocal Flag:** Boolean per ability (set in database)

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Anti-Caster Specialists

**Skald "Song of Silence":**

- Premier [Silenced] specialist
- Counter-resonant performance
- Targets vocal ability users

**Myrk-gengr "Throat-Cutter" / "Garrote":**

- Physical assassination attack
- Damages throat directly
- High-tier ability

**Hlekkr-master "Choke Chain":**

- Physical restraint + silence
- Potential high-tier ability

---

### Enemy Sources

- Rival Skalds
- Specialized assassins
- High-tech Undying (white noise emitters)

---

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Depends on source

- **Metaphysical (Arcane/Sonic):** WILL-based Resolve Check
- **Physical (throat injury):** STURDINESS-based Resolve Check

**DC Range:** 11-15 (set by ability)

**Success:** Resist, no [Silenced] applied

**Failure:** Apply [Silenced] effect

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Depends on source

**Physical Source:**

- Bone-Setter's "Administer Stimulant"
- Healing poultice
- Restore throat function

**Arcane Source:**

- Vard-Warden magical cleanse
- Counter-curse required

**Natural Expiration:**

- Often most reliable method
- Wait for duration to expire (2 rounds)

---

## VII. Tactical Implications

### The Caster's Nightmare

**Devastating to Vocal Builds:**

- **Galdr-caster:** Nearly as bad as being stunned
- **Skald:** Entire kit disabled
- **Thul:** Orations unusable
- **Skald-Screamer:** Complete shutdown

---

### The Tactical Niche

**Situational Effectiveness:**

- Useless vs. martial party (Berserker, Myrk-gengr, Bone-Setter)
- Fight-winning vs. caster party (Galdr-caster, Skald, Vard-Warden)
- Encourages scouting and strategy adaptation

**Rock-Paper-Scissors Dynamic:** Creates build counters

---

### Character Building Implications

**Importance of Diversity:**

- Pure vocal build = vulnerable to complete shutdown
- Mixed vocal/non-vocal = backup options when silenced
- Adds depth to character building

**Example:**

- Pure Galdr-caster: Only basic attack when silenced
- Galdr-caster + non-vocal utility spec: Alternative options available

---

### Database Implementation Note

**Abilities Table:**

- New column: `IsVocal` (boolean)
- Set per ability during content creation
- Core of silence system logic

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Silenced] Status Effect Feature Specification

**Target:** DB10 [Silenced] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)