---
id: SPEC-PROJECT-GLOSSARY
title: "Rune & Rust — Glossary"
version: 1.0
status: draft
last-updated: 2025-12-07
---

# Rune & Rust — Glossary

Standardized terminology used across all specifications.

---

## Keywords (Combat Mechanics)

Keywords appear in brackets `[Keyword]` and have specific mechanical definitions.

| Keyword | Definition |
|---------|------------|
| `[Reach]` | Can attack enemies in front row while positioned in back row |
| `[Push]` | Force target from front row to back row (MIGHT vs STURDINESS) |
| `[Pull]` | Force target from back row to front row (MIGHT vs STURDINESS) |
| `[Stagger]` | Target loses next bonus action |
| `[Stunned]` | Target cannot take actions, -4 Defense |
| `[Bleeding]` | Target takes 1d6 Physical damage at start of turn |
| `[Slowed]` | Movement costs doubled, cannot use movement abilities |
| `[Off-Balance]` | -2 to attack rolls, -1 Defense |
| `[Disoriented]` | -2 to all skill checks |
| `[Knocked Down]` | Prone, must spend action to stand |
| `[Fear]` | -2 to attack rolls, may flee |

---

## Attributes

Primary attributes that define character capabilities.

| Attribute | Abbr | Description |
|-----------|------|-------------|
| **Might** | MIG | Physical power, melee damage, forced movement |
| **Wits** | WIT | Mental acuity, perception, reaction speed |
| **Sturdiness** | STU | Physical resilience, resisting forced movement |
| **Will** | WIL | Mental fortitude, magic resistance |
| **Agility** | AGI | Speed, evasion, reflexes |
| **Presence** | PRE | Charisma, intimidation, leadership |

---

## Derived Stats

Values calculated from attributes and other factors.

| Stat | Description | Typical Formula |
|------|-------------|-----------------|
| **HP** | Health Points, reaching 0 causes defeat | Base + (STU × modifier) |
| **Stamina** | Resource for physical abilities | Base + (MIG × modifier) |
| **Mana** | Resource for magical abilities | Base + (WIT × modifier) |
| **Soak** | Damage reduction from armor/abilities | Armor + bonuses |
| **Defense** | Target number enemies must exceed to hit | 10 + AGI + bonuses |

---

## Combat Terms

| Term | Definition |
|------|------------|
| **Front Row** | Melee range, can be targeted by melee attacks |
| **Back Row** | Protected position, safe from melee (unless [Reach]) |
| **Opposed Check** | Roll attribute dice; compare successes |
| **Soak** | Damage subtracted before applying to HP |
| **Stance** | Mode that modifies stats/abilities while active |
| **Aura** | Passive effect affecting allies/enemies in range |
| **Zone of Control** | Area where enemies suffer penalties |

---

## Progression Terms

| Term | Definition |
|------|------------|
| **Legend** | Primary advancement level (like "level") |
| **PP** | Profession Points, spent to unlock abilities |
| **Tier** | Ability tree level (Tier 1→2→3→Capstone) |
| **Rank** | Ability power level (Rank 1→2→3) |
| **Archetype** | Broad class category (Warrior, Mage, etc.) |
| **Specialization** | Specific class with unique ability tree |

---

## Documentation Terms

| Term | Definition |
|------|------------|
| **Spec ID** | Unique identifier for a specification document |
| **Frontmatter** | YAML metadata at top of specification |
| **Formula** | Exact calculation for a mechanic |
| **GUI Display** | Specification for how element appears in UI |

---

## Status Effect Duration

| Duration | Meaning |
|----------|---------|
| **1 turn** | Expires at start of affected creature's next turn |
| **X turns** | Lasts for X of affected creature's turns |
| **Until end of combat** | Lasts entire combat encounter |
| **Permanent** | Lasts until removed by specific means |
