# Validation Instructions Master Reference

> **The Architect's Complete Guide to Content Compliance**

This document consolidates all validation instructions for Rune & Rust content, serving as the canonical reference for the validation agent ecosystem. Each section maps to specific agents and provides actionable validation criteria.

---

## Table of Contents

1. [Validation Agent Ecosystem](#validation-agent-ecosystem)
2. [Domain 4: Technology Validation](#domain-4-technology-validation)
3. [AAM-VOICE Layer Validation](#aam-voice-layer-validation)
4. [9-Domain Compliance Matrix](#9-domain-compliance-matrix)
5. [Structural & Specification Validation](#structural--specification-validation)
6. [Terminology & Canon Validation](#terminology--canon-validation)
7. [Dice System & Mechanics Validation](#dice-system--mechanics-validation)
8. [Validation Decision Trees](#validation-decision-trees)
9. [Remediation Quick Reference](#remediation-quick-reference)

---

## Validation Agent Ecosystem

### Agent Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                    THE VALIDATOR                            │
│              (Comprehensive Compliance Guardian)            │
│                                                             │
│   Orchestrates all validation, defers to specialists        │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
┌─────────────────┐ ┌─────────────┐ ┌─────────────────────┐
│ DOMAIN4-VALIDATOR│ │AAM-VOICE   │ │LOGGING-GUARDIAN     │
│ (Tech Constraints)│ │VALIDATOR   │ │(C# Code Standards)  │
│                   │ │(Layer Voice)│ │                     │
└─────────────────┘ └─────────────┘ └─────────────────────┘
         │               │
         ▼               ▼
┌─────────────────┐ ┌─────────────────────────────────────┐
│  NARRATOR       │ │         WORLDBUILDER                │
│  (Creative Voice)│ │   (System Design & Mechanics)       │
└─────────────────┘ └─────────────────────────────────────┘
```

### When to Use Each Agent

| Validation Need | Primary Agent | Supporting Agents |
|-----------------|---------------|-------------------|
| Full compliance audit | **the-validator** | All specialists |
| POST-Glitch precision violations | **domain4-validator** | narrator |
| Voice layer contamination | **aam-voice-validator** | domain4-validator |
| Player-facing flavor text | **narrator** | domain4-validator |
| Game mechanics & dice systems | **worldbuilder** | the-validator |
| C# logging compliance | **logging-guardian** | — |

---

## Domain 4: Technology Validation

### Core Principle

> **"POST-Glitch societies are archaeologists, not engineers."**

Survivors can ACTIVATE, OPERATE, and JURY-RIG—but cannot CREATE, PROGRAM, MODIFY, or truly COMPREHEND Pre-Glitch technology.

### Validation Gates

#### Gate 1: Context Classification

| Context | Precision Allowed? | Rationale |
|---------|-------------------|-----------|
| Pre-Glitch Documentation | ✅ YES | Historical record of LOST capabilities |
| Layer 3 Pre-Glitch Section | ✅ YES | Technical archive of defunct systems |
| POST-Glitch Narrative | ❌ NO | No precision instruments exist |
| Layer 2 Diagnostic | ❌ NO | Observer lacks measurement tools |
| Player-Facing Content | ❌ NO | Characters don't understand |
| GM Notes (Mechanical) | ✅ YES | Game mechanics exempt |

#### Gate 2: Forbidden Precision Patterns

**CRITICAL VIOLATIONS — Immediate Remediation Required:**

| Category | Forbidden Examples | Compliant Alternatives |
|----------|-------------------|----------------------|
| **Frequency** | "45 Hz subsonic", "18-22 kHz" | "deep rumble felt in chest", "piercing shriek" |
| **Decibels** | "120+ dB", "85 dB threshold" | "deafening", "conversation-drowning" |
| **Percentage** | "95.7% efficiency", "89% survival" | "almost certain", "most survive" |
| **Temperature** | "1,450°C", "-40°C ambient" | "hot enough to glow", "cold enough to crack iron" |
| **Distance** | "347 meters", "2.3 kilometers" | "bowshot range", "half-day's walk" |
| **Time (precise)** | "87 seconds", "4.3 hours" | "a few heartbeats", "before midday" |
| **Weight** | "12.5 kg exactly" | "weight of a grown child" |
| **Population** | "10,847 residents" | "a crowded settlement", "thousands" |
| **Voltage/Power** | "220V AC", "5 amperes" | "lightning-fed", "spark-hungry" |
| **Chemical** | "H₂S 6-12 ppm" | "poison air", "the choking stench" |

#### Gate 3: Scientific Vocabulary Scan

**Forbidden Terms → Required Replacements:**

| FORBIDDEN | PERMITTED |
|-----------|-----------|
| cellular, molecular | living weave, flesh-pattern |
| polymer, plastic, synthetic | resin, false-bone, old-shell |
| electricity, voltage | lightning, spark, power-blood |
| radiation, radioactive | invisible fire, poison-light, sickness-glow |
| DNA, genetic | blood-record, ancestor-script |
| algorithm, code | ritual-pattern, command-sequence |
| download, upload | commune, draw knowledge, offer memories |
| battery, capacitor | power-stone, lightning-cell, spark-vessel |
| circuit, processor | flow-path, thinking-stone |
| sensor, detector | dead eyes, watcher-spirit |

#### Gate 4: Modern Verb Detection

**Forbidden Verbs → Ritual Replacements:**

| FORBIDDEN | PERMITTED |
|-----------|-----------|
| program, code | inscribe, command, ritualize |
| calibrate | attune, balance, appease |
| optimize | refine, hone, temper |
| debug, troubleshoot | diagnose, exorcise, purge |
| reboot, restart | kill and revive, cycle the death-sleep |
| install, configure | bind, seat, anchor, set the pattern |
| download, upload | commune with, draw from, offer to |
| scan, analyze | read the entrails, divine, study |

### Qualitative Conversion Tables

#### Distance Conversions

| Precision | Qualitative |
|-----------|-------------|
| 1-5 meters | arm's reach, a few paces |
| 10-20 meters | javelin-throw distance, across a chamber |
| 50-100 meters | bowshot range, the far end of a great hall |
| 100-500 meters | beyond bowshot, barely visible |
| 1+ kilometers | a hard morning's walk, distant on the horizon |

#### Temperature Conversions

| Precision | Qualitative |
|-----------|-------------|
| -50 to -20°C | cold enough to freeze blood mid-vein |
| -20 to 0°C | cold enough to crack iron |
| 0 to 15°C | winter-cold, bone-chilling |
| 15 to 25°C | mild, comfortable |
| 25 to 40°C | uncomfortably warm, sweat-inducing |
| 40 to 100°C | hot enough to scald skin |
| 100+ °C | hot enough to boil water |
| 500+ °C | hot enough to glow cherry-red |
| 1000+ °C | hot enough to melt iron |

#### Time Conversions

| Precision | Qualitative |
|-----------|-------------|
| 1-10 seconds | a few heartbeats, time to draw breath |
| 10-60 seconds | long enough to recite a short prayer |
| 1-10 minutes | while a candle burns a finger's width |
| 10-60 minutes | the span of a meal |
| 1-4 hours | a morning's work, half-day |
| 4-8 hours | from dawn to midday, a full watch |
| 1 day | sunrise to sunrise |
| 1 week | a sennight |

#### Weight Conversions

| Precision | Qualitative |
|-----------|-------------|
| < 1 kg | light enough for a child to carry |
| 1-5 kg | weight of a sword, manageable burden |
| 5-20 kg | weight of a child, pack-burden |
| 20-50 kg | weight requiring two hands |
| 50-100 kg | weight of a grown adult |
| 100+ kg | requiring multiple bearers |

#### Probability Conversions

| Precision | Qualitative |
|-----------|-------------|
| 90%+ | almost certain, reliable |
| 70-90% | likely, most of the time |
| 50-70% | even odds, unpredictable |
| 30-50% | unlikely but possible |
| 10-30% | rarely, exceptional cases only |
| < 10% | almost never, the stuff of legend |

---

## AAM-VOICE Layer Validation

### The Four Layers

Each Layer represents a distinct epistemological mode. **Cross-contamination corrupts the evidence.**

### Layer 1: Mythic (The Unbroken Facade)

**Purpose:** Oral tradition, saga, collective memory

**Critical Criteria (4 items — 100% required):**

| # | Criterion | Pass Example | Fail Example |
|---|-----------|--------------|--------------|
| L1-C1 | Oral-first cadence | "Sing now of the Frost that Walks" | "The following describes..." |
| L1-C2 | Collective voice ("we", "our") | "We remember the God-Sleeper" | "I observed the creature" |
| L1-C3 | Definitive mythic statements | "The World-Tree holds all" | "appears to support" |
| L1-C4 | NO precision measurements | "beyond counting" | "approximately 10,000" |

**Additional Criteria (11 items — 85% total required):**

| # | Criterion |
|---|-----------|
| L1-5 | Kennings and epithets permitted |
| L1-6 | NO epistemic uncertainty language |
| L1-7 | Present tense for eternal truths |
| L1-8 | Third-person for heroes/gods |
| L1-9 | Rhythmic/poetic structure |
| L1-10 | Alliteration encouraged |
| L1-11 | NO technical terminology |
| L1-12 | NO Field Card structure |
| L1-13 | NO decimal outlines |
| L1-14 | NO version numbers in text |
| L1-15 | Supernatural accepted as fact |

### Layer 2: Diagnostic (The Cracked Facade)

**Purpose:** Field observations, Jötun-Reader perspective, bestiary entries

**Critical Criteria (12 items — 100% required):**

| # | Criterion | Pass Example | Fail Example |
|---|-----------|--------------|--------------|
| L2-C1 | First-person observer | "I observed the subject" | "The creature was observed" |
| L2-C2 | Epistemic uncertainty | "appears to", "suggests" | "definitely", "is" |
| L2-C3 | Field Card structure | Before/During/After/Outcome | Narrative flow |
| L2-C4 | OWNER decisions documented | "OWNER: Lead Ranger" | Unclear authority |
| L2-C5 | NO precision measurements | "roughly arm's length" | "approximately 1.5 meters" |
| L2-C6 | Qualitative temperature | "uncomfortably cold" | "estimated -15°C" |
| L2-C7 | Qualitative distance | "bowshot range" | "about 100 meters" |
| L2-C8 | Qualitative time | "several heartbeats" | "3-4 seconds" |
| L2-C9 | NO percentages | "most specimens" | "roughly 80%" |
| L2-C10 | NO frequency specs | "deep rumbling felt in chest" | "sub-20 Hz infrasound" |
| L2-C11 | NO scientific vocabulary | "living weave" | "cellular structure" |
| L2-C12 | NO modern verbs | "attune", "appease" | "calibrate", "program" |

**Additional Criteria (18 items — 85% total required):**

| # | Criterion |
|---|-----------|
| L2-13 | Clinical but archaic tone |
| L2-14 | Knowledge gaps acknowledged |
| L2-15 | Sensory observations emphasized |
| L2-16 | Physical symptoms over mechanisms |
| L2-17 | Uncertainty about causation |
| L2-18 | Comparative descriptions ("larger than a horse") |
| L2-19 | NO mythic kennings |
| L2-20 | NO definitive supernatural statements |
| L2-21 | Attenuation windows defined |
| L2-22 | Behavioral observations |
| L2-23 | Environmental context noted |
| L2-24 | Hazards documented empirically |
| L2-25 | Personal danger acknowledged |
| L2-26 | Equipment limitations noted |
| L2-27 | Reliability caveats included |
| L2-28 | Future observation needs identified |
| L2-29 | Cross-references to related entries |
| L2-30 | Date/location of observation |

### Layer 3: Technical (The Sterile Facade)

**Purpose:** System specifications, technical archives, Pre-Glitch documentation

**Structure Requirements:**

| Element | Required | Example |
|---------|----------|---------|
| Header block | ✅ | id, title, version, status |
| Decimal outline | ✅ | 1.0, 1.1, 1.1.1 |
| Impersonal voice | ✅ | "The system provides..." |
| Version tracking | ✅ | "v2.3.1" |

**Pre-Glitch Sections (Precision PERMITTED):**

- Historical performance metrics
- Original specifications
- Technical capabilities before collapse
- Population data from Age of Forging

**POST-Glitch Sections (Domain 4 REQUIRED):**

| # | Criterion |
|---|-----------|
| L3-PG1 | NO precision measurements |
| L3-PG2 | Qualitative performance descriptions |
| L3-PG3 | Degradation noted without percentages |
| L3-PG4 | Uncertainty about current status |
| L3-PG5 | "Estimated" or "approximate" language |
| L3-PG6 | NO scientific vocabulary |
| L3-PG7 | NO modern operational verbs |
| L3-PG8 | Current function vs. original capability |
| L3-PG9 | Observer limitations acknowledged |
| L3-PG10 | Reliability caveats |

### Layer 4: Ground Truth (Omniscient)

**Purpose:** Worldbuilder authority, game mechanics, GM-only information

**Critical Criteria (6 items — 100% required):**

| # | Criterion | Requirement |
|---|-----------|-------------|
| L4-C1 | Definitive statements | Authoritative voice permitted |
| L4-C2 | Precision permitted | Exact values acceptable |
| L4-C3 | Rationale documented | Why this design choice? |
| L4-C4 | Affects documented | What does this impact? |
| L4-C5 | Discrepancies documented | Conflicts with L2/L3 noted |
| L4-C6 | Usage guidance | When to apply this truth |

**Additional Criteria (6 items — 85% total required):**

| # | Criterion |
|---|-----------|
| L4-7 | Clear GM Note demarcation |
| L4-8 | Never player-facing |
| L4-9 | Mechanical precision |
| L4-10 | Cross-references to related L4 |
| L4-11 | Version controlled |
| L4-12 | Canonical status clear |

### Layer Contamination Detection

| Contamination Type | Symptom | Resolution |
|--------------------|---------|------------|
| Mythic → Diagnostic | Kennings in field reports | Remove epithets, add uncertainty |
| Diagnostic → Mythic | "appears to" in saga | Make definitive, remove hedging |
| Technical → Narrative | Decimal outlines in flavor | Remove structure, add prose |
| Omniscient → Player | GM mechanics in descriptions | Move to GM Note block |

---

## 9-Domain Compliance Matrix

### Domain Overview

| Domain | Name | Severity | Core Constraint |
|--------|------|----------|-----------------|
| DOM-1 | Cosmology | P2-HIGH | Nine Realms are megastructures |
| DOM-2 | Timeline | P2-HIGH | PG years (Post-Glitch) |
| DOM-3 | Magic | P1-CRITICAL | No "spells" — use Galdr/Runes |
| DOM-4 | Technology | P1-CRITICAL | No precision measurements |
| DOM-5 | Species | P2-HIGH | Canonical species only |
| DOM-6 | Entities | P2-HIGH | Canonical factions only |
| DOM-7 | Reality | P2-HIGH | Reality is god's corpse |
| DOM-8 | Geography | P2-HIGH | Canonical locations only |
| DOM-9 | Counter-Rune | P1-CRITICAL | Restricted Apocrypha only |

### Domain 1: Cosmology

**Core Truth:** The Nine Realms are physical megastructures, not metaphysical dimensions.

| Check | Pass | Fail |
|-------|------|------|
| Realms as physical | "The vast cavern of Niflheim" | "The dimension of Niflheim" |
| World-Tree as structure | "The branches of Yggdrasil, each spanning leagues" | "Yggdrasil connects the dimensions" |
| Physical travel | "The journey took three moons" | "Teleported between realms" |

### Domain 2: Timeline

**Core Truth:** All dates reference Post-Glitch (PG) era. 800+ years since collapse.

| Check | Pass | Fail |
|-------|------|------|
| PG dating | "In the year 823 PG" | "In the year 2847 CE" |
| Collapse reference | "Before the Glitch" | "Before the apocalypse" |
| Era terminology | "Age of Forging (Pre-Glitch)" | "The ancient era" |

### Domain 3: Magic

**Core Truth:** No "magic" or "spells." Use Aetheric Energy, Galdr, Runic Power.

| Term | Forbidden | Required |
|------|-----------|----------|
| Magic system | "cast a spell" | "wove a Galdr" |
| Mana/Energy | "expend mana" | "channel Aether" |
| Spellcaster | "wizard", "mage" | "Galdr-weaver", "Rune-speaker" |
| Effects | "magical damage" | "Aetheric harm", "Runic backlash" |

**Corruption Mechanic:**
- All Runic power causes Corruption
- Corruption is physical, not metaphorical
- Symptoms are visible and progressive

### Domain 5: Species

**Canonical Species Conversions:**

| FORBIDDEN | CANONICAL |
|-----------|-----------|
| Orc, Ork | Gorge-Maw |
| Elf, Elves | Rune-Lupin (context), Alfar (formal) |
| Dwarf, Dwarves | Dvergr |
| Giant | Jotun |
| Frost Giant | Hrimthursar |
| Fire Giant | Muspeli |
| Zombie | Undying, Husk |
| Robot, Android | Servitor, Iron-Walker |
| Demon | Entropy-Spawn, Blight-Born |
| Angel | Aesir-remnant (context) |

### Domain 6: Entities

**Canonical Faction Names:**
- J.T.N. (not "robots" or "machines")
- The Rust-Clans (not "scavenger tribes")
- Dvergr Foundries (not "dwarf kingdoms")
- Ranger Corps (not "adventurer guild")

### Domain 7: Reality

**Core Truth:** Reality is the corpse of a dead god.

| Concept | Expression |
|---------|------------|
| Physical world | "The flesh of the All-Father" |
| Mountains | "The bones of the dead god" |
| Rivers | "The blood-channels of Ymir" |
| Reality instability | "The corpse stirs" |

### Domain 8: Geography

**Canonical Location Requirements:**
- Use established realm names
- Reference the Bifrost as physical structure
- No generic fantasy locations (no "Shadowlands")
- Cross-reference with established maps

### Domain 9: Counter-Rune (CRITICAL)

**Restriction Level:** Apocrypha ONLY

| Check | Status |
|-------|--------|
| Eihwaz mentioned outside Apocrypha | ❌ FAIL — remove or relocate |
| Dual nature revealed | ❌ FAIL — preserve mystery |
| Counter-Rune mechanics explained | ❌ FAIL — GM-only content |
| Apocrypha-marked document | ✅ PASS — appropriate context |

---

## Structural & Specification Validation

### Required Frontmatter

```yaml
---
id: SPEC-[CATEGORY]-[NAME]
title: "Human-Readable Title"
version: 1.0
status: draft | review | canonical
last-updated: YYYY-MM-DD
---
```

### Required Sections

| Section | Purpose | Required For |
|---------|---------|--------------|
| Identity | Core concept quote | Specializations |
| Design Philosophy | Why this exists | All specs |
| Mechanics | d10 resolution, tiers | Game systems |
| Voice Guidance | Tone, Layer, style | All content |
| Synergies | Positive/Negative | Character specs |
| Rank Progression | Level requirements | Specializations |

### Header Hierarchy

```
# H1 — Document Title (ONE per document)
## H2 — Major Sections
### H3 — Subsections
#### H4 — Details (use sparingly)
```

**Violation:** Skipping levels (H1 → H3) is a WARN-level issue.

---

## Terminology & Canon Validation

### Universal Forbidden Terms

| FORBIDDEN | PERMITTED |
|-----------|-----------|
| magic | Aetheric Energy, Runic Power, Galdr |
| spell | Galdr, Inscription, Weaving |
| mana | Aether |
| bug/glitch (technical) | Anomaly, Phenomenon, Corruption |
| API | (no equivalent — remove) |
| level up | advance in rank, gain experience |
| XP, experience points | renown, deeds, accomplishments |

### Dice System Terms

| FORBIDDEN | PERMITTED |
|-----------|-----------|
| d20, d12 | d10 (resolution), d4-d10 (damage tiers) |
| advantage/disadvantage | Rune bonus/Blight penalty |
| AC, armor class | Defense Rating, DR |
| HP, hit points | Vitality, Wounds |
| saving throw | Resistance Check |

---

## Dice System & Mechanics Validation

### Resolution System

**Core Mechanic:** Attribute + d10 vs. Difficulty Class (DC)

| DC | Description |
|----|-------------|
| 5 | Trivial |
| 10 | Easy |
| 15 | Moderate |
| 20 | Hard |
| 25 | Very Hard |
| 30 | Legendary |

### Damage Tier System

| Tier | Die | Example Weapon |
|------|-----|----------------|
| Tier 1 (Light) | d4 | Dagger, improvised |
| Tier 2 (Medium) | d6 | Short sword, club |
| Tier 3 (Martial) | d8 | Longsword, axe |
| Tier 4 (Heavy) | d10 | Greatsword, warhammer |

### Forbidden Dice

| FORBIDDEN | REASON |
|-----------|--------|
| d20 | D&D 5e system, not Rune & Rust |
| d12 | Not in tiered system |
| d100 | Percentile breaks Domain 4 |
| 4d6 drop lowest | Character gen not applicable |

---

## Validation Decision Trees

### Quick Compliance Check

```
START
│
├─► Is this POST-Glitch content?
│   ├── NO → Pre-Glitch precision permitted
│   └── YES → Continue
│
├─► Does it contain precision measurements?
│   ├── NO → Continue
│   └── YES → Is it in a GM Note block?
│       ├── YES → PASS (mechanics exempt)
│       └── NO → FAIL — Remediate with qualitative
│
├─► Does it use scientific vocabulary?
│   ├── NO → Continue
│   └── YES → FAIL — Replace with archaic terms
│
├─► Does it use modern verbs?
│   ├── NO → Continue
│   └── YES → FAIL — Replace with ritual verbs
│
├─► Is the Layer consistent throughout?
│   ├── YES → Continue
│   └── NO → WARN — Identify contamination
│
├─► Does it use canonical terminology?
│   ├── YES → Continue
│   └── NO → FAIL — Replace forbidden terms
│
└─► Does it use d10 dice system?
    ├── YES → PASS
    └── NO → FAIL — Convert to tiered system
```

### Full Validation Workflow

```
┌────────────────────────────────────────┐
│           VALIDATION WORKFLOW          │
├────────────────────────────────────────┤
│                                        │
│  1. STRUCTURAL CHECK                   │
│     ├── Frontmatter present?           │
│     ├── Required sections exist?       │
│     └── Header hierarchy correct?      │
│                                        │
│  2. LAYER IDENTIFICATION               │
│     ├── Determine L1/L2/L3/L4          │
│     ├── Check for contamination        │
│     └── Mark POST-Glitch sections      │
│                                        │
│  3. DOMAIN 4 SCAN (P1-CRITICAL)        │
│     ├── Precision measurements         │
│     ├── Scientific vocabulary          │
│     └── Modern verbs                   │
│                                        │
│  4. DOMAIN 9 CHECK (P1-CRITICAL)       │
│     ├── Counter-Rune references        │
│     └── Apocrypha context verified     │
│                                        │
│  5. TERMINOLOGY SCAN                   │
│     ├── Forbidden species names        │
│     ├── "Magic" violations             │
│     └── Dice notation                  │
│                                        │
│  6. REMAINING DOMAINS (1-3, 5-8)       │
│     ├── Cosmology consistent           │
│     ├── Timeline uses PG               │
│     └── All canon verified             │
│                                        │
│  7. SCORING & VERDICT                  │
│     ├── CRITICAL: 100% required        │
│     ├── Total: 85% required            │
│     └── Generate remediation list      │
│                                        │
└────────────────────────────────────────┘
```

---

## Remediation Quick Reference

### Priority Order

1. **P1-CRITICAL** — Fix immediately, blocks publication
   - Domain 4 precision in POST-Glitch
   - Domain 9 Counter-Rune exposure
   - Domain 3 "magic/spell" violations
   - Invalid dice (d20, d12)

2. **P2-HIGH** — Fix before publication
   - Missing frontmatter
   - Voice layer drift
   - Forbidden species terms
   - Broken internal links

3. **P3-MEDIUM** — Remediation recommended
   - Missing Voice Guidance section
   - Header hierarchy issues
   - Weak sensory detail
   - Passive voice in L2

### Quick Fix Patterns

| Issue | Pattern | Fix |
|-------|---------|-----|
| Precision → Qualitative | "50 meters" | "bowshot range" |
| Scientific → Archaic | "cellular" | "living weave" |
| Modern → Ritual | "calibrate" | "attune" |
| Magic → Aetheric | "spell" | "Galdr" |
| Generic → Canonical | "orc" | "Gorge-Maw" |
| d20 → d10 | "roll 1d20" | "roll 1d10" |

---

## Appendix: Validation Report Template

```markdown
# Validation Report: [Document Title]

**Date:** YYYY-MM-DD
**Validator:** [Agent Name]
**Status:** PASS | FAIL | WARN

---

## Document Classification

- **Layer Identified:** L1 / L2 / L3 / L4
- **Context:** Pre-Glitch / POST-Glitch / Mixed
- **Domain 4 Applicable:** Yes / No

---

## CRITICAL VIOLATIONS (P1)

| Line | Violation | Type | Remediation |
|------|-----------|------|-------------|
| X | "[quoted text]" | Domain 4 Precision | "[fixed text]" |

---

## HIGH PRIORITY ISSUES (P2)

| Line | Issue | Type | Remediation |
|------|-------|------|-------------|
| X | "[quoted text]" | [Type] | "[fixed text]" |

---

## MEDIUM PRIORITY ISSUES (P3)

| Line | Issue | Type | Remediation |
|------|-------|------|-------------|
| X | "[quoted text]" | [Type] | "[fixed text]" |

---

## Structural Checklist

- [ ] Frontmatter complete
- [ ] Voice Guidance present
- [ ] Header hierarchy correct
- [ ] Internal links verified
- [ ] Dice notation correct

---

## Scoring Summary

- **Critical Items:** X/Y passed (100% required) — PASS/FAIL
- **High Items:** X/Y passed
- **Medium Items:** X/Y passed
- **Total Compliance:** Z%

---

## FINAL VERDICT: [PASS / FAIL / WARN]

---

## Remediation Checklist

- [ ] Fix violation at line X: Replace "[old]" with "[new]"
- [ ] Add missing frontmatter field: [field]
- [ ] Relocate Counter-Rune content to Apocrypha
```

---

*This document serves as the canonical reference for all validation agents. When in doubt, reference the specific agent instructions for detailed guidance.*

**Last Updated:** 2025-12-24
**Version:** 1.0
**Status:** Canonical
