# BEAST-[THREAT]-[NNN]: [Creature Name]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                          BESTIARY TEMPLATE                                    ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Creature and enemy documentation with combat integration            ║
║  LAYER: Layer 2 (Diagnostic) - Observer perspective, no precision            ║
║  AUDIENCE: Writers, Game Designers, Encounter Designers                       ║
║                                                                              ║
║  ⚠️  DOMAIN 4 COMPLIANCE REQUIRED - AAM-VOICE (Jötun-Reader)                 ║
║      All descriptions from field observer perspective                         ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: BEAST-[THREAT]-[NNN]
title: "[Creature Common Name]"
version: 1.0
status: draft                    # draft | review | canonical | archived
layer: Layer 2
domain-compliance: pending       # pending | passed | failed

# Classification
threat-level: [1-10]             # See threat matrix below
category: [CATEGORY]             # Natural | Corrupted | Construct | Hybrid | Unknown
sub-category: "[Type]"           # Predator | Scavenger | Territorial | Pack | Solitary
origin: "[Origin Type]"          # Native | Blight-touched | Machine-remnant | Mutated

# Combat Integration
encounter-type: "[Type]"         # Minion | Standard | Elite | Boss | Swarm
base-stat-tier: [1-5]            # Tier for stat block generation
ai-profile: "[Profile]"          # Aggressive | Defensive | Ambush | Swarm | Support

# Environment
primary-habitat: "[Habitat]"     # Forest | Ruin | Cavern | Settlement | Wasteland
activity-period: "[Period]"      # Diurnal | Nocturnal | Crepuscular | Constant
rarity: "[Rarity]"               # Common | Uncommon | Rare | Legendary | Unique

# Metadata
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []

# Cross-references
related-creatures:
  - BEAST-[THREAT]-[NNN]
related-specs:
  - SPEC-ENEMY-[NNN]
  - SPEC-COMBAT-[NNN]
related-lore:
  - LORE-[CAT]-[NNN]

tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Quick Reference Card](#1-quick-reference-card)
2. [Identity & Classification](#2-identity--classification)
3. [Physical Profile](#3-physical-profile)
4. [Behavioral Analysis](#4-behavioral-analysis)
5. [Combat Profile](#5-combat-profile)
6. [Encounter Design](#6-encounter-design)
7. [Ecological Role](#7-ecological-role)
8. [Cultural Impact](#8-cultural-impact)
9. [Field Observations](#9-field-observations)
10. [Loot & Salvage](#10-loot--salvage)
11. [Implementation Data](#11-implementation-data)
12. [Domain 4 Compliance](#12-domain-4-compliance)
13. [Changelog](#13-changelog)

---

## Threat Level Matrix

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      THREAT LEVEL CLASSIFICATION                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  LEVEL   ID PREFIX    DESCRIPTION                  TYPICAL ENCOUNTER        │
│  ─────   ─────────    ───────────                  ─────────────────        │
│    1     MINOR        Nuisance creature            Solo traveler handles    │
│    2     MINOR        Low threat predator          Trained warrior wins     │
│    3     MODERATE     Competent predator           Party recommended        │
│    4     MODERATE     Dangerous hunter             Party with prep needed   │
│    5     SEVERE       Apex territorial             Full party required      │
│    6     SEVERE       Regional threat              Experienced party        │
│    7     DEADLY       Settlement killer            Elite party required     │
│    8     DEADLY       Legendary predator           Near impossible solo     │
│    9     APEX         World-shaping threat         Army-scale response      │
│   10     APEX         Extinction-class             Plot device / Boss       │
│                                                                             │
│  ⚠️  Threat levels are relative to standard human capability                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Quick Reference Card

> ⚠️ REQUIRED | At-a-glance summary for encounter design

```
╔══════════════════════════════════════════════════════════════════════════════╗
║                         FIELD CARD: [CREATURE NAME]                          ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  THREAT ASSESSMENT                                                           ║
║  ─────────────────                                                           ║
║  ┌────────────────────┬──────────────────────────────────────────────────┐  ║
║  │ Threat Level       │ [N] - [MINOR/MODERATE/SEVERE/DEADLY/APEX]        │  ║
║  │ Encounter Type     │ [Minion/Standard/Elite/Boss/Swarm]               │  ║
║  │ Typical Group Size │ [Solitary / Pack (N-M) / Swarm]                  │  ║
║  │ Activity Pattern   │ [Diurnal/Nocturnal/Crepuscular/Constant]         │  ║
║  └────────────────────┴──────────────────────────────────────────────────┘  ║
║                                                                              ║
║  COMBAT SUMMARY                                                              ║
║  ──────────────                                                              ║
║  ┌────────────────────┬──────────────────────────────────────────────────┐  ║
║  │ Primary Attack     │ [Attack name] - [Brief description]              │  ║
║  │ Secondary Attack   │ [Attack name] - [Brief description]              │  ║
║  │ Special Ability    │ [Ability name] - [Brief description]             │  ║
║  │ Weakness           │ [Vulnerability description]                      │  ║
║  │ Resistance         │ [What it resists]                                │  ║
║  └────────────────────┴──────────────────────────────────────────────────┘  ║
║                                                                              ║
║  IDENTIFICATION                                                              ║
║  ──────────────                                                              ║
║  ┌────────────────────┬──────────────────────────────────────────────────┐  ║
║  │ Size               │ [Tiny/Small/Medium/Large/Huge/Gargantuan]        │  ║
║  │ Key Features       │ [2-3 identifying characteristics]                │  ║
║  │ Warning Signs      │ [How to detect presence]                         │  ║
║  │ Escape Difficulty  │ [Easy/Moderate/Hard/Near Impossible]             │  ║
║  └────────────────────┴──────────────────────────────────────────────────┘  ║
║                                                                              ║
║  TACTICAL ADVICE                                                             ║
║  ──────────────                                                              ║
║  ├── DO: [Recommended approach]                                             ║
║  ├── DO: [Recommended approach]                                             ║
║  ├── DON'T: [What to avoid]                                                 ║
║  └── DON'T: [What to avoid]                                                 ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

---

## 2. Identity & Classification

> ⚠️ REQUIRED

### 2.1 Nomenclature

| Type | Name | Notes |
|------|------|-------|
| **Common Name** | [Name] | [Most widely used] |
| **Regional Variants** | [Name(s)] | [Where used] |
| **Scholarly Designation** | [Name] | [Jötun-Reader classification] |
| **Folk Names** | [Name(s)] | [Superstitious/local terms] |
| **PRE-Glitch Record** | [UNKNOWN / Name] | [If recovered from archives] |

### 2.2 Taxonomic Classification

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CLASSIFICATION HIERARCHY                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  KINGDOM:    [Natural / Artificial / Hybrid]                                │
│              │                                                              │
│              └── Certainty: ████████░░ [HIGH/MEDIUM/LOW]                    │
│                                                                             │
│  CATEGORY:   [Corrupted Fauna / Machine Remnant / Blight Spawn / etc.]      │
│              │                                                              │
│              └── Based on: [What evidence supports this]                    │
│                                                                             │
│  FAMILY:     [Grouping - e.g., "Iron-Beasts", "Rust-Crawlers"]              │
│              │                                                              │
│              └── Related species: [List similar creatures]                  │
│                                                                             │
│  SPECIES:    [Specific type]                                                │
│                                                                             │
│  VARIANT:    [Regional/behavioral variant if applicable]                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Origin Assessment

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ORIGIN DECISION TREE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                      ┌───────────────────────┐                              │
│                      │ Does it show signs of │                              │
│                      │ PRE-Glitch design?    │                              │
│                      └───────────┬───────────┘                              │
│                                  │                                          │
│                    ┌─────────────┼─────────────┐                            │
│                   YES         UNCLEAR          NO                           │
│                    │             │             │                            │
│                    ▼             ▼             ▼                            │
│            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                  │
│            │ Construct/  │ │ Corrupted/  │ │ Natural or  │                  │
│            │ Machine-    │ │ Hybrid      │ │ Mutated     │                  │
│            │ Remnant     │ │ Origin      │ │ Origin      │                  │
│            └──────┬──────┘ └──────┬──────┘ └──────┬──────┘                  │
│                   │              │               │                          │
│                   ▼              ▼               ▼                          │
│            Is it still     Shows Blight    Evidence of                      │
│            functional?     corruption?     mutation?                        │
│                   │              │               │                          │
│            ┌──────┴──────┐ ┌──────┴──────┐ ┌──────┴──────┐                  │
│           YES           NO YES          NO YES          NO                  │
│            │             │  │            │  │            │                  │
│            ▼             ▼  ▼            ▼  ▼            ▼                  │
│         ACTIVE       DORMANT BLIGHT-   HYBRID MUTATED  NATIVE              │
│         CONSTRUCT    WRECK   TOUCHED                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Physical Profile

> ⚠️ REQUIRED | Written in Layer 2 (Diagnostic) voice

### 3.1 General Description

> ⚠️ Domain 4 Compliant - No precision measurements

[Write a 2-3 paragraph description from the Jötun-Reader perspective. Use relative measurements only.]

**Example Format:**
*"Field observations describe a quadrupedal predator standing roughly waist-height to an adult human. The specimen's hide appears to consist of overlapping plates reminiscent of rusted iron, though whether this represents natural armor or integrated machine-remnants remains uncertain. Witnesses consistently report eyes that emit a dim amber luminescence, visible at considerable distance in low-light conditions."*

### 3.2 Anatomical Features

| Feature | Description (Layer 2 Voice) | Combat Relevance |
|---------|----------------------------|------------------|
| **Head/Sensory** | [Description] | [How it affects combat] |
| **Body/Core** | [Description] | [Vulnerable points, armor] |
| **Limbs/Locomotion** | [Description] | [Movement capability] |
| **Natural Weapons** | [Description] | [Attack types available] |
| **Special Organs** | [Description] | [Unusual capabilities] |

### 3.3 Size Comparison

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SIZE COMPARISON                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│                    [CREATURE]                                               │
│                        █                                                    │
│                       ███                                                   │
│                      █████                                                  │
│                     ███████        Human (reference)                        │
│                    █████████            █                                   │
│                                        ███                                  │
│                                       █████                                 │
│  ───────────────────────────────────────────────────────────────────────   │
│                                                                             │
│  SIZE CATEGORY: [Tiny / Small / Medium / Large / Huge / Gargantuan]         │
│                                                                             │
│  HEIGHT:   [Relative description - "twice a man's height"]                  │
│  LENGTH:   [Relative description - "long as a wagon"]                       │
│  WEIGHT:   [Relative description - "heavy as a draft horse"]                │
│  WINGSPAN: [If applicable - relative description]                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.4 Sensory Profile

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      SENSORY IDENTIFICATION                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  👁️  VISUAL INDICATORS                                                      │
│      ├── Primary: [Most notable visual feature]                             │
│      ├── Movement: [How it moves - gait, speed impression]                  │
│      └── Lighting: [Any luminescence, reflections]                          │
│                                                                             │
│  👂 AUDITORY INDICATORS                                                      │
│      ├── Idle: [Sounds when not alerted]                                    │
│      ├── Alert: [Sounds when aware of prey]                                 │
│      ├── Attack: [Sounds during combat]                                     │
│      └── Range: [How far sounds carry - "audible from a bowshot"]           │
│                                                                             │
│  👃 OLFACTORY INDICATORS                                                     │
│      ├── Scent: [Distinctive smell]                                         │
│      └── Tracking: [Can it be tracked by smell? Can it track by smell?]     │
│                                                                             │
│  ⚡ AETHERIC SIGNATURE                                                       │
│      ├── Presence: [None / Faint / Moderate / Strong / Overwhelming]        │
│      └── Character: [Description of aetheric feel]                          │
│                                                                             │
│  ⚠️  WARNING SIGNS (Pre-Encounter)                                           │
│      ├── Environmental: [Changes to surroundings]                           │
│      ├── Wildlife: [Behavior of other animals]                              │
│      └── Other: [Any other warning signs]                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.5 Life Cycle

> 📎 OPTIONAL - Include if relevant to gameplay

| Stage | Duration (Qualitative) | Characteristics | Threat Change |
|-------|------------------------|-----------------|---------------|
| [Stage] | [Description] | [Features] | [vs Adult threat] |

---

## 4. Behavioral Analysis

> ⚠️ REQUIRED

### 4.1 Temperament Profile

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      TEMPERAMENT ASSESSMENT                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  AGGRESSION SCALE:                                                          │
│  ═══════════════                                                            │
│  [░░░░░░░░░░] Passive       - Flees from most threats                       │
│  [████░░░░░░] Cautious      - Avoids confrontation, defends if cornered     │
│  [██████░░░░] Neutral       - Neither seeks nor avoids conflict             │
│  [████████░░] Aggressive    - Initiates conflict readily                    │
│  [██████████] Hyper-Aggr.   - Attacks on sight, pursues relentlessly        │
│                                                                             │
│  This Creature: [████████░░] Aggressive                                     │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  TERRITORIAL SCALE:                                                         │
│  ══════════════════                                                         │
│  [░░░░░░░░░░] Nomadic       - No fixed territory                            │
│  [████░░░░░░] Loose         - Prefers area but doesn't defend               │
│  [██████░░░░] Moderate      - Defends core territory                        │
│  [████████░░] Strong        - Aggressively patrols borders                  │
│  [██████████] Absolute      - Kills all intruders                           │
│                                                                             │
│  This Creature: [██████░░░░] Moderate                                       │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  INTELLIGENCE SCALE:                                                        │
│  ═══════════════════                                                        │
│  [░░░░░░░░░░] Instinctual   - Pure instinct, no learning                    │
│  [████░░░░░░] Cunning       - Shows basic problem-solving                   │
│  [██████░░░░] Clever        - Learns from encounters, uses tactics          │
│  [████████░░] Intelligent   - Complex strategies, tool use                  │
│  [██████████] Sapient       - Human-level or greater intelligence           │
│                                                                             │
│  This Creature: [████░░░░░░] Cunning                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Behavioral Patterns

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    BEHAVIORAL STATE MACHINE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         ┌─────────────┐                                     │
│                         │    IDLE     │                                     │
│                         │  (Default)  │                                     │
│                         └──────┬──────┘                                     │
│                                │                                            │
│              ┌─────────────────┼─────────────────┐                          │
│              │                 │                 │                          │
│         [Hunger]          [Threat]           [Prey]                         │
│              │                 │                 │                          │
│              ▼                 ▼                 ▼                          │
│       ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                     │
│       │    HUNT     │  │   ALERT     │  │   STALK     │                     │
│       │             │  │             │  │             │                     │
│       └──────┬──────┘  └──────┬──────┘  └──────┬──────┘                     │
│              │                │                 │                           │
│         [Target]         [Confirm]         [Position]                       │
│              │                │                 │                           │
│              └────────────────┼─────────────────┘                           │
│                               │                                             │
│                               ▼                                             │
│                        ┌─────────────┐                                      │
│                        │   ATTACK    │◄─────────┐                           │
│                        │             │          │                           │
│                        └──────┬──────┘          │                           │
│                               │            [Target                          │
│              ┌────────────────┼──────────  Escapes]                         │
│              │                │                 │                           │
│         [Victory]        [Wounded]         [Pursuit]                        │
│              │                │                 │                           │
│              ▼                ▼                 ▼                           │
│       ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                     │
│       │    FEED     │  │   RETREAT   │  │   CHASE     │──────┘              │
│       │             │  │             │  │             │                     │
│       └──────┬──────┘  └──────┬──────┘  └─────────────┘                     │
│              │                │                                             │
│              └────────────────┴──────────────────┐                          │
│                                                  │                          │
│                                                  ▼                          │
│                                           ┌─────────────┐                   │
│                                           │    IDLE     │                   │
│                                           └─────────────┘                   │
│                                                                             │
│  TRANSITION TRIGGERS:                                                       │
│  ├── Hunger: [Time since last feed / resource depletion]                    │
│  ├── Threat: [Perception of danger]                                         │
│  ├── Prey: [Detection of suitable target]                                   │
│  ├── Wounded: [Health drops below threshold]                                │
│  └── Victory: [Target eliminated or incapacitated]                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Social Structure

| Aspect | Description |
|--------|-------------|
| **Group Type** | Solitary / Mated Pair / Pack / Herd / Swarm / Colony |
| **Hierarchy** | [How leadership/dominance is determined] |
| **Communication** | [How individuals communicate] |
| **Territory Sharing** | [How space is divided] |

### 4.4 Hunting/Feeding Behavior

[Describe hunting methods, preferred prey, feeding habits in Layer 2 voice]

---

## 5. Combat Profile

> ⚠️ REQUIRED

### 5.1 Stat Block Reference

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         COMBAT STATISTICS                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ══════════════════════════════════════════════════════════════════════    │
│  BASE TIER: [1-5]          ENCOUNTER TYPE: [Minion/Standard/Elite/Boss]     │
│  ══════════════════════════════════════════════════════════════════════    │
│                                                                             │
│  DEFENSIVE PROFILE                                                          │
│  ─────────────────                                                          │
│  │ Health Pool    │ [Tier-appropriate: Low/Medium/High/Very High]     │     │
│  │ Armor Class    │ [None/Light/Medium/Heavy/Exceptional]             │     │
│  │ Evasion        │ [Poor/Low/Average/Good/Excellent]                 │     │
│  │ Resistances    │ [Damage types resisted]                           │     │
│  │ Immunities     │ [Damage types immune to]                          │     │
│  │ Vulnerabilities│ [Damage types deal extra damage]                  │     │
│                                                                             │
│  OFFENSIVE PROFILE                                                          │
│  ─────────────────                                                          │
│  │ Attack Speed   │ [Slow/Average/Fast/Very Fast]                     │     │
│  │ Damage Output  │ [Low/Medium/High/Very High/Devastating]           │     │
│  │ Range          │ [Melee Only/Short/Medium/Long]                    │     │
│  │ Multi-target   │ [Single/Cleave/AoE]                               │     │
│                                                                             │
│  MOBILITY PROFILE                                                           │
│  ────────────────                                                           │
│  │ Base Speed     │ [Very Slow/Slow/Average/Fast/Very Fast]           │     │
│  │ Movement Type  │ [Walk/Climb/Fly/Swim/Burrow/Teleport]             │     │
│  │ Pursuit        │ [Gives up easily/Moderate/Relentless]             │     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Attack Catalog

#### Primary Attack: [Attack Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ATTACK: [Attack Name]                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Type:        [Melee/Ranged/Special]                                        │
│  Damage Type: [Physical/Fire/Cold/Lightning/Poison/Aetheric/etc.]           │
│  Range:       [Melee/Close/Medium/Long] - [Qualitative description]         │
│  Speed:       [Slow/Average/Fast]                                           │
│  Target:      [Single/Cleave/Cone/AoE]                                      │
│                                                                             │
│  DESCRIPTION (Layer 2):                                                     │
│  [Describe the attack from observer perspective without precision]          │
│                                                                             │
│  TELLS/TELEGRAPHS:                                                          │
│  ├── Visual: [What players see before the attack]                           │
│  ├── Audio: [What players hear before the attack]                           │
│  └── Timing: [How much warning - "a breath's time," "nearly instant"]       │
│                                                                             │
│  EFFECTS:                                                                   │
│  ├── On Hit: [Primary effect]                                               │
│  ├── Status: [Any status effects applied]                                   │
│  └── Special: [Any additional effects]                                      │
│                                                                             │
│  COUNTER-PLAY:                                                              │
│  ├── Dodge: [Can it be dodged? How?]                                        │
│  ├── Block: [Can it be blocked? Effectiveness?]                             │
│  └── Interrupt: [Can the wind-up be interrupted?]                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Secondary Attack: [Attack Name]

[Repeat attack template]

#### Special Ability: [Ability Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  SPECIAL ABILITY: [Ability Name]                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Type:        [Offensive/Defensive/Utility/Passive]                         │
│  Trigger:     [When does this ability activate?]                            │
│  Cooldown:    [Qualitative - "frequently," "once per encounter"]            │
│  Target:      [Self/Single/Multiple/Area]                                   │
│                                                                             │
│  DESCRIPTION (Layer 2):                                                     │
│  [Describe the ability from observer perspective]                           │
│                                                                             │
│  EFFECTS:                                                                   │
│  [Detail all effects]                                                       │
│                                                                             │
│  INTERACTION:                                                               │
│  ├── Can be interrupted: [Yes/No - How?]                                    │
│  ├── Can be dispelled: [Yes/No - How?]                                      │
│  └── Player response: [Recommended counter-play]                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.3 AI Behavior Profile

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       AI BEHAVIOR PROFILE                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PROFILE TYPE: [Aggressive / Defensive / Ambush / Swarm / Support]          │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  TARGETING PRIORITY:                                                        │
│  1. [First priority target type]                                            │
│  2. [Second priority target type]                                           │
│  3. [Third priority target type]                                            │
│                                                                             │
│  ATTACK PATTERN:                                                            │
│  ├── Opening: [What it does first]                                          │
│  ├── Main Loop: [Primary combat rotation]                                   │
│  ├── Low Health: [Behavior when wounded]                                    │
│  └── Special Trigger: [Conditional behaviors]                               │
│                                                                             │
│  POSITIONING:                                                               │
│  ├── Preferred Range: [Melee/Mid/Long]                                      │
│  ├── Flanking: [Yes/No - tendency]                                          │
│  └── Retreat Conditions: [When it withdraws]                                │
│                                                                             │
│  PACK TACTICS (if applicable):                                              │
│  ├── Role Assignment: [How pack members specialize]                         │
│  ├── Coordination: [How they work together]                                 │
│  └── Morale Break: [What causes pack to flee]                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.4 Weaknesses & Vulnerabilities

| Weakness | Exploitation | Effect |
|----------|--------------|--------|
| [Weak point] | [How to exploit] | [Bonus/advantage gained] |
| [Element] | [Attack type] | [Extra damage/stun/etc.] |
| [Behavioral] | [How to trigger] | [Opens vulnerability] |

---

## 6. Encounter Design

> ⚠️ REQUIRED

### 6.1 Encounter Context Matrix

| Context | Likelihood | Party Level | Group Size | Difficulty |
|---------|------------|-------------|------------|------------|
| Exploration | [Common/Uncommon/Rare] | [Range] | [N-M] | [Easy/Medium/Hard] |
| Dungeon | [Common/Uncommon/Rare] | [Range] | [N-M] | [Easy/Medium/Hard] |
| Ambush | [Common/Uncommon/Rare] | [Range] | [N-M] | [Easy/Medium/Hard] |
| Lair | [Common/Uncommon/Rare] | [Range] | [N-M] | [Easy/Medium/Hard] |

### 6.2 Encounter Templates

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ENCOUNTER: [Name] - [Difficulty]                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  COMPOSITION:                                                               │
│  ├── [N]x [This Creature]                                                   │
│  ├── [N]x [Support creature, if any]                                        │
│  └── [Environmental hazards, if any]                                        │
│                                                                             │
│  SETUP:                                                                     │
│  [Describe the encounter scenario]                                          │
│                                                                             │
│  TACTICS:                                                                   │
│  [How the creatures behave together]                                        │
│                                                                             │
│  ESCALATION:                                                                │
│  [How the encounter can get harder if players delay]                        │
│                                                                             │
│  VICTORY CONDITIONS:                                                        │
│  ├── Combat: [Kill all / Kill leader / Reduce to X%]                        │
│  ├── Escape: [Possible? Conditions?]                                        │
│  └── Alternative: [Non-combat solutions]                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Environmental Integration

| Environment | Advantage | Disadvantage | Special Interaction |
|-------------|-----------|--------------|---------------------|
| [Terrain type] | [Creature benefit] | [Creature hindrance] | [Unique behavior] |

### 6.4 Narrative Integration

[How does encountering this creature advance story or world-building?]

---

## 7. Ecological Role

> 📋 RECOMMENDED

### 7.1 Food Web Position

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ECOLOGICAL POSITION                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                          [Apex Predator]                                    │
│                               │                                             │
│                               ▼                                             │
│                    ┌─────────────────────┐                                  │
│                    │   [THIS CREATURE]   │ ◄── You are here                 │
│                    └──────────┬──────────┘                                  │
│                               │                                             │
│              ┌────────────────┼────────────────┐                            │
│              │                │                │                            │
│              ▼                ▼                ▼                            │
│       [Prey Type A]    [Prey Type B]    [Prey Type C]                       │
│                                                                             │
│  ROLE: [Apex Predator / Mesopredator / Prey / Scavenger / Decomposer]       │
│                                                                             │
│  IMPACT ON ECOSYSTEM:                                                       │
│  [Description of ecological function]                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Habitat Requirements

| Requirement | Necessity | Notes |
|-------------|-----------|-------|
| Shelter | [Essential/Preferred/Optional] | [Description] |
| Water | [Essential/Preferred/Optional] | [Description] |
| Food Source | [Essential/Preferred/Optional] | [Description] |
| Territory Size | [Qualitative] | [Description] |

### 7.3 Population Dynamics

[Describe breeding, population density, migration patterns if relevant]

---

## 8. Cultural Impact

> 📋 RECOMMENDED

### 8.1 Folk Knowledge

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        FOLK WISDOM                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SAYINGS:                                                                   │
│  ├── "[Proverb involving the creature]" - [Meaning]                         │
│  ├── "[Proverb involving the creature]" - [Meaning]                         │
│  └── "[Proverb involving the creature]" - [Meaning]                         │
│                                                                             │
│  OMENS:                                                                     │
│  ├── Seeing one means: [Interpretation]                                     │
│  └── [Other omen]: [Interpretation]                                         │
│                                                                             │
│  TABOOS:                                                                    │
│  ├── [Action forbidden]: [Consequence believed]                             │
│  └── [Action forbidden]: [Consequence believed]                             │
│                                                                             │
│  REMEDIES/PROTECTIONS:                                                      │
│  ├── [Folk remedy]: [Believed effect]                                       │
│  └── [Protective charm/action]: [Believed effect]                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Economic Value

| Resource | Source | Use | Rarity | Value |
|----------|--------|-----|--------|-------|
| [Part] | [How obtained] | [Crafting/Trade use] | [Availability] | [Worth] |

### 8.3 Faction Relationships

| Faction | Stance | Interaction |
|---------|--------|-------------|
| [Faction] | [Hunt/Avoid/Revere/Study] | [How they interact] |

---

## 9. Field Observations

> 📋 RECOMMENDED

### 9.1 Sample Field Report

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     FIELD OBSERVATION RECORD                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  REPORT: [FOR-YYYY-NNN]                                                     │
│  OBSERVER: [Name], [Title]                                                  │
│  DATE: [Season], [Year] POST-Glitch                                         │
│  LOCATION: [Region/Landmark]                                                │
│  CONDITIONS: [Weather, visibility, time]                                    │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  OBSERVATION NARRATIVE:                                                     │
│                                                                             │
│  [Written in Layer 2 voice - detailed account of the observation.           │
│   Include sensory details, behavioral notes, and any unusual events.        │
│   Maintain epistemic uncertainty where appropriate.]                        │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  KEY FINDINGS:                                                              │
│  ├── [Finding 1]                                                            │
│  ├── [Finding 2]                                                            │
│  └── [Finding 3]                                                            │
│                                                                             │
│  QUESTIONS RAISED:                                                          │
│  ├── [Unanswered question]                                                  │
│  └── [Unanswered question]                                                  │
│                                                                             │
│  RELIABILITY ASSESSMENT: [HIGH / MEDIUM / LOW]                              │
│  REASON: [Why this rating]                                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Historical Encounters

| Date | Location | Event | Casualties | Outcome |
|------|----------|-------|------------|---------|
| [Era] | [Place] | [Brief description] | [If any] | [Result] |

---

## 10. Loot & Salvage

> ⚠️ REQUIRED

### 10.1 Drop Table

| Item | Drop Chance | Quantity | Notes |
|------|-------------|----------|-------|
| [Common drop] | Common | [N-M] | [Harvesting notes] |
| [Uncommon drop] | Uncommon | [N-M] | [Harvesting notes] |
| [Rare drop] | Rare | [N] | [Special conditions] |

### 10.2 Harvesting Guide

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      HARVESTING PROCEDURES                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TOOL REQUIREMENTS:                                                         │
│  ├── Basic: [Minimum tools needed]                                          │
│  ├── Skilled: [Better tools for better yields]                              │
│  └── Expert: [Specialized tools for rare parts]                             │
│                                                                             │
│  TIME REQUIREMENT: [Qualitative - "a brief while," "half a candle-mark"]    │
│                                                                             │
│  SKILL CHECK: [If applicable - what skill, what difficulty]                 │
│                                                                             │
│  HAZARDS:                                                                   │
│  ├── [Hazard during harvesting]                                             │
│  └── [Hazard during harvesting]                                             │
│                                                                             │
│  PRESERVATION:                                                              │
│  [How long materials remain useful, storage requirements]                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.3 Crafting Applications

| Material | Crafting Use | Resulting Item | Skill Required |
|----------|--------------|----------------|----------------|
| [Material] | [Category] | [Example item] | [Skill/Level] |

---

## 11. Implementation Data

> ⚠️ REQUIRED | For game system integration

### 11.1 Specification References

| Spec ID | Relationship | Notes |
|---------|--------------|-------|
| SPEC-ENEMY-[NNN] | Primary | [Main enemy spec] |
| SPEC-COMBAT-[NNN] | Uses | [Combat system integration] |
| SPEC-AI-[NNN] | Uses | [AI behavior system] |
| SPEC-LOOT-[NNN] | Uses | [Loot generation] |

### 11.2 Asset Requirements

| Asset Type | Description | Priority |
|------------|-------------|----------|
| Model/Sprite | [Visual requirements] | [P1-P4] |
| Animation | [Movement/attack animations needed] | [P1-P4] |
| Sound | [Audio requirements] | [P1-P4] |
| VFX | [Visual effects needed] | [P1-P4] |

### 11.3 Implementation Notes

[Technical notes for developers implementing this creature]

---

## 12. Domain 4 Compliance

> ⚠️ REQUIRED

### 12.1 Compliance Verification

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DOMAIN 4 COMPLIANCE CHECK                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PRECISION MEASUREMENT SCAN:                                                │
│  ├── [ ] No metric units (meters, kg, liters)                               │
│  ├── [ ] No imperial units (feet, pounds, gallons)                          │
│  ├── [ ] No percentages                                                     │
│  ├── [ ] No exact time (seconds, minutes as numbers)                        │
│  ├── [ ] No temperatures (°C, °F)                                           │
│  ├── [ ] No frequencies (Hz)                                                │
│  └── [ ] No exact counts where estimation appropriate                       │
│                                                                             │
│  TERMINOLOGY SCAN:                                                          │
│  ├── [ ] No "robot," "android," "automaton" → use "iron-walker"             │
│  ├── [ ] No "sensor," "detector" → use "spirit-sense"                       │
│  ├── [ ] No "program," "code" → use "rune-binding"                          │
│  ├── [ ] No "malfunction," "bug" → use "corruption," "aberration"           │
│  └── [ ] No modern scientific terms inappropriate to Layer 2                │
│                                                                             │
│  VOICE DISCIPLINE:                                                          │
│  ├── [ ] Observer perspective maintained                                    │
│  ├── [ ] Epistemic uncertainty markers present                              │
│  ├── [ ] No omniscient knowledge claims                                     │
│  └── [ ] Technology treated with appropriate mysticism                      │
│                                                                             │
│  OVERALL: [ ] COMPLIANT  [ ] VIOLATIONS FOUND                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 12.2 Violation Log

| Section | Original | Corrected | Status |
|---------|----------|-----------|--------|
| [Location] | "[Violating text]" | "[Compliant text]" | Fixed |

---

## 13. Changelog

> 🔒 LOCKED

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | YYYY-MM-DD | [Author] | Initial creation |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  SUBMISSION CHECKLIST:                                                        ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ Quick Reference Card completed                                             ║
║  □ All attacks and abilities documented                                       ║
║  □ At least one encounter template provided                                   ║
║  □ Loot table completed                                                       ║
║  □ Domain 4 compliance verified                                               ║
║  □ Specification references linked                                            ║
║  □ All Layer 2 text uses proper voice                                         ║
║  □ Status set to 'review' when ready                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
