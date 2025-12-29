# LORE-[CATEGORY]-[NNN]: [Lore Entry Title]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                            LORE TEMPLATE                                      ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Narrative world-building content for the Rune & Rust universe       ║
║  LAYER: Layer 1 (Mythic) or Layer 2 (Diagnostic)                             ║
║  AUDIENCE: Writers, Game Masters, Archivists                                  ║
║                                                                              ║
║  ⚠️  DOMAIN 4 COMPLIANCE REQUIRED FOR ALL CONTENT                            ║
║      "POST-Glitch societies are archaeologists, not engineers."               ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: LORE-[CATEGORY]-[NNN]
title: "[Human-Readable Title]"
version: 1.0
status: draft                    # draft | review | canonical | archived
layer: Layer 2                   # Layer 1 (Mythic) | Layer 2 (Diagnostic)
domain-compliance: pending       # pending | passed | failed

# Classification
category: [CATEGORY]             # ENT | FAC | FAU | FLO | GEO | HAZ | HIS | ALC | LNG
sub-category: "[Sub-category]"   # e.g., "Predator", "Settlement", "Ritual"
classification: "[Type]"         # e.g., "Population Dossier", "Historical Record"

# Relationships
faction-association: "[Faction]" # None | Remnant | Covenant | Dvergr | etc.
geographic-scope: "[Region]"     # Global | Regional | Local
era: "POST-Glitch"               # PRE-Glitch | The Glitch | POST-Glitch

# Metadata
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []

# Cross-references
related-lore:
  - LORE-[CAT]-[NNN]
related-specs:
  - SPEC-[DOM]-[NNN]

# Tags
tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Document Overview](#1-document-overview)
2. [Identity](#2-identity)
3. [Physical Description](#3-physical-description)
4. [Origins & History](#4-origins--history)
5. [Cultural Significance](#5-cultural-significance)
6. [Behaviors & Patterns](#6-behaviors--patterns)
7. [Relationships & Interactions](#7-relationships--interactions)
8. [Knowledge & Beliefs](#8-knowledge--beliefs)
9. [Encounters & Observations](#9-encounters--observations)
10. [Data Captures](#10-data-captures)
11. [Narrative Hooks](#11-narrative-hooks)
12. [Domain 4 Compliance](#12-domain-4-compliance)
13. [Reviewer Assessment](#13-reviewer-assessment)
14. [Changelog](#14-changelog)

---

## Voice & Layer Selection Guide

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    LAYER SELECTION DECISION TREE                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                    ┌─────────────────────────┐                              │
│                    │  What perspective is    │                              │
│                    │  this content from?     │                              │
│                    └───────────┬─────────────┘                              │
│                                │                                            │
│           ┌────────────────────┼────────────────────┐                       │
│           │                    │                    │                       │
│           ▼                    ▼                    ▼                       │
│   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐                 │
│   │ Oral Tradition│   │Field Observer │   │ PRE-Glitch    │                 │
│   │ Sagas, Myths  │   │Clinical Notes │   │ Technical Docs│                 │
│   │ Folk Tales    │   │Jötun-Reader   │   │ Archives      │                 │
│   └───────┬───────┘   └───────┬───────┘   └───────┬───────┘                 │
│           │                   │                   │                         │
│           ▼                   ▼                   ▼                         │
│   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐                 │
│   │   LAYER 1     │   │   LAYER 2     │   │   LAYER 3     │                 │
│   │    Mythic     │   │  Diagnostic   │   │  Technical    │                 │
│   │               │   │               │   │               │                 │
│   │ No precision  │   │ Relative only │   │ Full precision│                 │
│   │ Pure story    │   │ Clinical tone │   │ Archive data  │                 │
│   └───────────────┘   └───────────────┘   └───────────────┘                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Layer 1: Mythic Voice

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LAYER 1: MYTHIC VOICE                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PERSPECTIVE: Oral tradition, sagas, campfire stories                       │
│  TONE: Reverent, fearful, awe-inspired                                      │
│  PRECISION: None - purely qualitative                                       │
│                                                                             │
│  CHARACTERISTICS:                                                           │
│  ├── Uses metaphor and simile heavily                                       │
│  ├── References the Old Ones, the Machine-Gods                              │
│  ├── Treats technology as magic/divine                                      │
│  ├── Embeds moral lessons                                                   │
│  └── Employs rhythmic, memorable phrasing                                   │
│                                                                             │
│  EXAMPLE:                                                                   │
│  "The Iron-Walker came from the mist, its eyes burning with the fire        │
│   of the Old Ones. Three warriors fell before its gaze, and the village     │
│   knew then that the spirits of the ruined world had not forgotten us."     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Layer 2: Diagnostic Voice (AAM-VOICE)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      LAYER 2: DIAGNOSTIC VOICE                               │
│                         (AAM-VOICE / Jötun-Reader)                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PERSPECTIVE: Field observer, system pathologist                            │
│  TONE: Clinical but archaic, epistemic uncertainty                          │
│  PRECISION: Relative measurements only                                      │
│                                                                             │
│  REQUIRED MARKERS:                                                          │
│  ├── "appears to"       ├── "suggests"        ├── "observations indicate"  │
│  ├── "field reports"    ├── "documented"      ├── "specimen analysis"      │
│  └── "uncertain origin" └── "theorized"       └── "requires further study" │
│                                                                             │
│  MEASUREMENT TRANSLATIONS:                                                  │
│  ├── Distance: "a spear's throw," "several paces," "within earshot"        │
│  ├── Time: "heartbeats," "candle-marks," "moon cycles"                     │
│  ├── Temperature: "searing," "frigid," "blood-warm"                        │
│  ├── Weight: "heavy as a child," "light as dry bone"                       │
│  └── Speed: "faster than sight," "lumbering," "swift as an arrow"          │
│                                                                             │
│  EXAMPLE:                                                                   │
│  "Field observations suggest the specimen exhibits territorial behavior     │
│   within roughly a bowshot of its lair. Recovered artifacts indicate       │
│   previous encounters resulted in significant casualties. The mechanism    │
│   by which it locates prey remains uncertain, though survivors describe    │
│   a low rumble preceding each attack."                                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Document Overview

> ⚠️ REQUIRED | 2-3 paragraphs

### 1.1 Summary

[Provide a brief executive summary of what this lore entry covers. What is the subject? Why is it significant to the world of Rune & Rust?]

### 1.2 Narrative Function

| Attribute | Value |
|-----------|-------|
| **Primary Role** | [What role does this play in the narrative?] |
| **Player Relevance** | [How will players encounter or interact with this?] |
| **Mystery Level** | Low / Medium / High / Forbidden |
| **Discovery Method** | [How is this knowledge obtained in-world?] |

### 1.3 Content Warnings

> 📎 OPTIONAL - Include if applicable

- [ ] Violence/Combat
- [ ] Body Horror
- [ ] Psychological Horror
- [ ] Existential Themes
- [ ] Loss/Grief

---

## 2. Identity

> ⚠️ REQUIRED

### 2.1 Names & Designations

| Type | Name | Origin |
|------|------|--------|
| **Common Name** | [Name] | [Who uses this name] |
| **Scholarly Name** | [Name] | [Academic/Diagnostic term] |
| **Folk Name(s)** | [Name(s)] | [Regional variations] |
| **PRE-Glitch Designation** | [UNKNOWN / Recovered name] | [If known, source] |

### 2.2 Classification

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        TAXONOMIC CLASSIFICATION                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Kingdom:     [Natural / Artificial / Hybrid / Unknown]                     │
│  Category:    [Entity / Fauna / Flora / Phenomenon / Artifact]              │
│  Family:      [Grouping within category]                                    │
│  Type:        [Specific type]                                               │
│  Variant:     [Regional/behavioral variant, if any]                         │
│                                                                             │
│  Certainty:   ████████░░ [HIGH/MEDIUM/LOW] - [Reason for uncertainty]       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Quick Reference

| Attribute | Value |
|-----------|-------|
| **Threat Level** | [1-10 or descriptive: Minimal/Low/Moderate/High/Severe/Extreme] |
| **Rarity** | Common / Uncommon / Rare / Legendary / Unique |
| **Status** | Active / Dormant / Extinct / Theoretical |
| **Primary Habitat** | [Environment or location] |

---

## 3. Physical Description

> ⚠️ REQUIRED

### 3.1 General Appearance

> Write in the selected Layer voice

[Layer 1 Example: "The beast stands as tall as two men, its hide the color of rusted iron left too long in the rain. Those who have glimpsed it speak of eyes that glow like dying embers."]

[Layer 2 Example: "Specimen exhibits bipedal locomotion with a height estimated at twice that of an average adult human. External integument displays significant oxidation consistent with ferrous composition. Optical organs emit low-intensity luminescence in the amber spectrum."]

### 3.2 Distinguishing Features

| Feature | Description | Significance |
|---------|-------------|--------------|
| [Feature 1] | [Description in Layer voice] | [Why this matters] |
| [Feature 2] | [Description in Layer voice] | [Why this matters] |
| [Feature 3] | [Description in Layer voice] | [Why this matters] |

### 3.3 Sensory Profile

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SENSORY PROFILE                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SIGHT:   [What does it look like? Colors, textures, movement]              │
│           └── [Description in Layer voice]                                  │
│                                                                             │
│  SOUND:   [What sounds does it make or is associated with it?]              │
│           └── [Description in Layer voice]                                  │
│                                                                             │
│  SMELL:   [Any distinctive odors?]                                          │
│           └── [Description in Layer voice]                                  │
│                                                                             │
│  FEEL:    [Temperature, texture if touched]                                 │
│           └── [Description in Layer voice]                                  │
│                                                                             │
│  OTHER:   [Aetheric resonance, sixth sense warnings, etc.]                  │
│           └── [Description in Layer voice]                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.4 Variations

> 📎 OPTIONAL - If regional/temporal variants exist

| Variant | Region | Distinguishing Traits |
|---------|--------|----------------------|
| [Variant Name] | [Location] | [How it differs] |

---

## 4. Origins & History

> 📋 RECOMMENDED

### 4.1 Creation/Origin

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ORIGIN THEORIES                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  MYTHIC ACCOUNT (Layer 1):                                                  │
│  ─────────────────────────                                                  │
│  "[The folk tale or saga version of origin]"                                │
│                                                                             │
│  DIAGNOSTIC ASSESSMENT (Layer 2):                                           │
│  ────────────────────────────────                                           │
│  "[The clinical analysis of likely origin]"                                 │
│                                                                             │
│  PRE-GLITCH RECORDS (Layer 3, if available):                                │
│  ──────────────────────────────────────────                                 │
│  "[Recovered archive data, technical specifications]"                       │
│                                                                             │
│  CERTAINTY: ███░░░░░░░ LOW - [Explanation]                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Timeline of Known Events

| Era | Event | Source | Reliability |
|-----|-------|--------|-------------|
| PRE-Glitch | [Event description] | [How we know] | High/Medium/Low |
| The Glitch | [Event description] | [How we know] | High/Medium/Low |
| POST-Glitch | [Event description] | [How we know] | High/Medium/Low |

### 4.3 Evolution/Changes Over Time

[Describe how this subject has changed since the Glitch, if applicable]

---

## 5. Cultural Significance

> 📋 RECOMMENDED

### 5.1 Religious/Spiritual Meaning

[How do POST-Glitch peoples view this subject spiritually? Is it sacred, profane, feared, worshipped?]

### 5.2 Faction Perspectives

| Faction | View | Interaction |
|---------|------|-------------|
| **Remnant** | [Their perspective] | [How they interact] |
| **Covenant of the Code** | [Their perspective] | [How they interact] |
| **Dvergr** | [Their perspective] | [How they interact] |
| **Free Settlements** | [Their perspective] | [How they interact] |

### 5.3 Folk Traditions

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       FOLK TRADITIONS & BELIEFS                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SAYINGS & PROVERBS:                                                        │
│  ├── "[Proverb 1]" - [Meaning]                                              │
│  ├── "[Proverb 2]" - [Meaning]                                              │
│  └── "[Proverb 3]" - [Meaning]                                              │
│                                                                             │
│  RITUALS:                                                                   │
│  ├── [Ritual Name]: [Brief description]                                     │
│  └── [Ritual Name]: [Brief description]                                     │
│                                                                             │
│  TABOOS:                                                                    │
│  ├── [Taboo]: [What is forbidden and why]                                   │
│  └── [Taboo]: [What is forbidden and why]                                   │
│                                                                             │
│  SUPERSTITIONS:                                                             │
│  ├── [Superstition]: [What people believe]                                  │
│  └── [Superstition]: [What people believe]                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Behaviors & Patterns

> ⚠️ REQUIRED (for entities/fauna/phenomena)

### 6.1 Typical Behaviors

[Describe normal behavioral patterns in the selected Layer voice]

### 6.2 Behavioral Cycle

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       BEHAVIORAL CYCLE                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────┐      ┌──────────┐      ┌──────────┐      ┌──────────┐        │
│  │  REST    │ ───► │  HUNT    │ ───► │  FEED    │ ───► │ PATROL   │        │
│  │          │      │          │      │          │      │          │        │
│  │ [Time]   │      │ [Time]   │      │ [Time]   │      │ [Time]   │        │
│  │ [Signs]  │      │ [Signs]  │      │ [Signs]  │      │ [Signs]  │        │
│  └──────────┘      └──────────┘      └──────────┘      └────┬─────┘        │
│       ▲                                                      │              │
│       └──────────────────────────────────────────────────────┘              │
│                                                                             │
│  TRIGGERS FOR STATE CHANGE:                                                 │
│  ├── Rest → Hunt: [Trigger description]                                     │
│  ├── Hunt → Feed: [Trigger description]                                     │
│  ├── Feed → Patrol: [Trigger description]                                   │
│  └── Patrol → Rest: [Trigger description]                                   │
│                                                                             │
│  ⚠️  All time references use qualitative measures (Layer 2 compliant)       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Threat Response Matrix

| Stimulus | Response | Warning Signs | Escalation |
|----------|----------|---------------|------------|
| [Stimulus] | [Behavioral response] | [Observable signs] | [How it escalates] |
| [Stimulus] | [Behavioral response] | [Observable signs] | [How it escalates] |

### 6.4 Communication

[How does this subject communicate? What signals, sounds, or displays does it use?]

---

## 7. Relationships & Interactions

> 📋 RECOMMENDED

### 7.1 Ecological Web

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       RELATIONSHIP MAP                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                        ┌─────────────────┐                                  │
│                        │   [SUBJECT]     │                                  │
│                        └────────┬────────┘                                  │
│                                 │                                           │
│          ┌──────────────────────┼──────────────────────┐                    │
│          │                      │                      │                    │
│          ▼                      ▼                      ▼                    │
│   ┌─────────────┐       ┌─────────────┐       ┌─────────────┐              │
│   │  PREDATORS  │       │  PREY       │       │  SYMBIOTES  │              │
│   │             │       │             │       │             │              │
│   │ - [Name]    │       │ - [Name]    │       │ - [Name]    │              │
│   │ - [Name]    │       │ - [Name]    │       │ - [Name]    │              │
│   └─────────────┘       └─────────────┘       └─────────────┘              │
│                                                                             │
│   LEGEND:                                                                   │
│   ──────                                                                    │
│   ──► Predation (eats/hunts)                                               │
│   ◄── Predated by (eaten/hunted)                                           │
│   ═══ Symbiotic relationship                                               │
│   ─ ─ Competition                                                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Interactions with Humans

| Interaction Type | Frequency | Outcome | Notes |
|------------------|-----------|---------|-------|
| Territorial encounter | [Freq] | [Typical result] | [Additional info] |
| Resource competition | [Freq] | [Typical result] | [Additional info] |
| Accidental contact | [Freq] | [Typical result] | [Additional info] |

### 7.3 Domestication/Exploitation Potential

| Aspect | Assessment | Notes |
|--------|------------|-------|
| **Tamability** | Impossible / Difficult / Possible / Easy | [Details] |
| **Utility** | [What value could be extracted] | [Details] |
| **Risks** | [Dangers of interaction] | [Details] |
| **Historical Attempts** | [Known attempts to domesticate/use] | [Outcomes] |

---

## 8. Knowledge & Beliefs

> 📋 RECOMMENDED

### 8.1 What is Known (Verified)

[List facts that have been confirmed through observation or multiple sources]

- [Verified fact 1]
- [Verified fact 2]
- [Verified fact 3]

### 8.2 What is Believed (Unverified)

[List common beliefs that may or may not be true]

- [Belief 1] - *Source: [Where this belief comes from]*
- [Belief 2] - *Source: [Where this belief comes from]*
- [Belief 3] - *Source: [Where this belief comes from]*

### 8.3 What is Unknown (Mysteries)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        OPEN QUESTIONS                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ❓ [Question 1]                                                            │
│     └── Current theories: [Brief summary of theories]                       │
│     └── Significance: [Why this matters]                                    │
│                                                                             │
│  ❓ [Question 2]                                                            │
│     └── Current theories: [Brief summary of theories]                       │
│     └── Significance: [Why this matters]                                    │
│                                                                             │
│  ❓ [Question 3]                                                            │
│     └── Current theories: [Brief summary of theories]                       │
│     └── Significance: [Why this matters]                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.4 Forbidden Knowledge

> 📎 OPTIONAL - Knowledge that is dangerous or taboo

[What knowledge about this subject is considered dangerous? Who guards this knowledge?]

---

## 9. Encounters & Observations

> 📋 RECOMMENDED

### 9.1 Field Report Template

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        FIELD OBSERVATION LOG                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  REPORT ID:      [FOL-YYYY-NNN]                                             │
│  OBSERVER:       [Name/Title]                                               │
│  DATE:           [Season, Year POST-Glitch]                                 │
│  LOCATION:       [General area - no precise coordinates]                    │
│  CONDITIONS:     [Weather, time of day]                                     │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  OBSERVATION:                                                               │
│  [Detailed account written in Layer 2 voice]                                │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  SPECIMEN BEHAVIOR:                                                         │
│  [Description of observed behaviors]                                        │
│                                                                             │
│  ENVIRONMENTAL FACTORS:                                                     │
│  [Note any relevant environmental conditions]                               │
│                                                                             │
│  ASSESSMENT:                                                                │
│  [Observer's conclusions and recommendations]                               │
│                                                                             │
│  RELIABILITY:    ████████░░ HIGH / MEDIUM / LOW                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Notable Historical Encounters

| Date | Location | Event | Outcome | Source |
|------|----------|-------|---------|--------|
| [Era] | [Place] | [What happened] | [Result] | [How we know] |

### 9.3 Survivor Testimony

> 📎 OPTIONAL - First-person accounts

*"[Quote from survivor in appropriate voice]"*
— [Name], [Title], [Date]

---

## 10. Data Captures

> ⚠️ REQUIRED

### 10.1 Core Data Capture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DATA CAPTURE: [ID]                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CAPTURE TYPE:     [Population / Artifact / Location / Event]               │
│  CAPTURE DATE:     [YYYY-MM-DD]                                             │
│  CAPTURED BY:      [Author]                                                 │
│  STATUS:           [Draft / Review / Canonical]                             │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  IDENTITY FIELDS:                                                           │
│  ├── primary_name:      "[Value]"                                           │
│  ├── alternate_names:   ["[Value1]", "[Value2]"]                            │
│  ├── classification:    "[Value]"                                           │
│  └── threat_level:      [1-10]                                              │
│                                                                             │
│  DESCRIPTIVE FIELDS:                                                        │
│  ├── physical_desc:     "[Layer 2 compliant description]"                   │
│  ├── behavior_summary:  "[Layer 2 compliant description]"                   │
│  └── habitat:           "[Layer 2 compliant description]"                   │
│                                                                             │
│  RELATIONSHIP FIELDS:                                                       │
│  ├── faction_ties:      "[None / Faction name]"                             │
│  ├── geographic_range:  "[Region description]"                              │
│  └── related_entries:   ["LORE-XXX-001", "LORE-YYY-002"]                    │
│                                                                             │
│  GAME INTEGRATION:                                                          │
│  ├── encounter_context: "[When players might encounter this]"               │
│  ├── narrative_hooks:   ["[Hook 1]", "[Hook 2]"]                            │
│  └── spec_references:   ["SPEC-XXX-001"]                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Integration Notes

| Target System | Data Required | Notes |
|---------------|---------------|-------|
| Bestiary UI | [Fields needed] | [Integration details] |
| Encounter System | [Fields needed] | [Integration details] |
| Codex | [Fields needed] | [Integration details] |

---

## 11. Narrative Hooks

> 📋 RECOMMENDED

### 11.1 Quest/Story Seeds

| Hook | Type | Trigger | Potential Outcomes |
|------|------|---------|-------------------|
| [Hook Name] | Side Quest / Main Quest / Discovery | [What triggers it] | [Possible results] |
| [Hook Name] | Side Quest / Main Quest / Discovery | [What triggers it] | [Possible results] |

### 11.2 NPC Connections

| NPC | Relationship | Story Potential |
|-----|--------------|-----------------|
| [NPC Name] | [How they relate to this subject] | [What stories could emerge] |

### 11.3 Environmental Storytelling

[How can this subject be used to tell stories through the environment? What traces does it leave?]

---

## 12. Domain 4 Compliance

> ⚠️ REQUIRED

### 12.1 Compliance Checklist

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DOMAIN 4 COMPLIANCE VERIFICATION                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  FORBIDDEN PRECISION CHECK:                                                 │
│  ├── [ ] No percentages (95%, 50%, etc.)                                    │
│  ├── [ ] No metric measurements (meters, kilometers, kg)                    │
│  ├── [ ] No imperial measurements (feet, miles, pounds)                     │
│  ├── [ ] No temperature values (°C, °F)                                     │
│  ├── [ ] No frequency values (Hz, kHz)                                      │
│  ├── [ ] No time precision (seconds, minutes as numbers)                    │
│  └── [ ] No modern technical terms (API, bug, glitch, sensor)               │
│                                                                             │
│  VOICE DISCIPLINE CHECK:                                                    │
│  ├── [ ] Layer-appropriate perspective maintained                           │
│  ├── [ ] Epistemic uncertainty language present (if Layer 2)                │
│  ├── [ ] Observer perspective maintained (not omniscient)                   │
│  └── [ ] Technology treated with appropriate reverence/fear                 │
│                                                                             │
│  TERMINOLOGY CHECK:                                                         │
│  ├── [ ] "Anomaly" not "bug/glitch"                                         │
│  ├── [ ] "Spirit-lights" not "LEDs/displays"                                │
│  ├── [ ] "Thinking-stone" not "computer/processor"                          │
│  ├── [ ] "Iron-walker" not "robot/automaton"                                │
│  └── [ ] "Rune-script" not "code/programming"                               │
│                                                                             │
│  OVERALL STATUS:   [ ] COMPLIANT   [ ] VIOLATIONS FOUND                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 12.2 Violation Log

> Complete only if violations found

| Location | Violation | Original Text | Corrected Text |
|----------|-----------|---------------|----------------|
| Section X.Y | [Type] | "[Original]" | "[Corrected]" |

### 12.3 Translation Reference

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   DOMAIN 4 TRANSLATION GUIDE                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DISTANCE:                                                                  │
│  ├── 1 meter      → "a pace"                                                │
│  ├── 5 meters     → "several paces"                                         │
│  ├── 20 meters    → "a stone's throw"                                       │
│  ├── 50 meters    → "a bowshot"                                             │
│  ├── 100 meters   → "within shouting distance"                              │
│  └── 1 kilometer  → "a morning's walk"                                      │
│                                                                             │
│  TIME:                                                                      │
│  ├── Seconds      → "heartbeats," "breaths"                                 │
│  ├── Minutes      → "a brief while," "time enough to..."                    │
│  ├── Hours        → "candle-marks," "until the sun moved"                   │
│  └── Days         → "suns," "sleeps"                                        │
│                                                                             │
│  QUANTITY:                                                                  │
│  ├── Percentage   → "most," "many," "few," "nearly all"                     │
│  ├── Exact count  → "a handful," "a score," "uncountable"                   │
│  └── Probability  → "almost certain," "likely," "rare"                      │
│                                                                             │
│  TEMPERATURE:                                                               │
│  ├── Cold         → "frigid," "bone-chilling," "breath-frosting"            │
│  ├── Hot          → "searing," "blistering," "forge-hot"                    │
│  └── Moderate     → "blood-warm," "comfortable," "mild"                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 13. Reviewer Assessment

> ⚠️ REQUIRED (completed by reviewer)

### 13.1 Review Summary

| Aspect | Rating | Notes |
|--------|--------|-------|
| **Completeness** | ★★★★☆ | [Comments] |
| **Voice Consistency** | ★★★★☆ | [Comments] |
| **Domain 4 Compliance** | ★★★★☆ | [Comments] |
| **Narrative Value** | ★★★★☆ | [Comments] |
| **Game Integration** | ★★★★☆ | [Comments] |

### 13.2 Reviewer Recommendation

- [ ] **APPROVED** - Ready for canonical status
- [ ] **MINOR REVISIONS** - Small changes needed
- [ ] **MAJOR REVISIONS** - Significant rework required
- [ ] **REJECTED** - Does not meet standards

### 13.3 Reviewer Notes

[Detailed feedback from reviewer]

---

## 14. Changelog

> 🔒 LOCKED - Format is standardized

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | YYYY-MM-DD | [Author] | Initial creation |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  CHECKLIST BEFORE SUBMISSION:                                                 ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ All [PLACEHOLDER] text replaced                                           ║
║  □ All ⚠️ REQUIRED sections completed                                         ║
║  □ Frontmatter is valid YAML                                                  ║
║  □ Layer voice is consistent throughout                                       ║
║  □ Domain 4 compliance verified                                               ║
║  □ Cross-references link to existing documents                                ║
║  □ Data capture fields populated                                              ║
║  □ Status set to 'review' when ready                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
