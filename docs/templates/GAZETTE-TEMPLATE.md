# GAZ-[REGION]-[NNN]: [Location Name]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                           GAZETTE TEMPLATE                                    ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Location, region, and geographic documentation                      ║
║  LAYER: Layer 2 (Diagnostic) with Layer 1 (Mythic) elements                  ║
║  AUDIENCE: Writers, Level Designers, Encounter Designers                      ║
║                                                                              ║
║  ⚠️  DOMAIN 4 COMPLIANCE REQUIRED                                            ║
║      "The land remembers what the people have forgotten."                     ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: GAZ-[REGION]-[NNN]
title: "[Location Name]"
version: 1.0
status: draft                    # draft | review | canonical | archived
layer: Layer 2
domain-compliance: pending       # pending | passed | failed

# Classification
location-type: "[Type]"          # Settlement | Ruin | Wilderness | Landmark | Dungeon
region: "[REGION]"               # NORTH | SOUTH | EAST | WEST | UNDER | COASTAL | URBAN | WILD
scale: "[Scale]"                 # Site | Area | District | Region | Territory
accessibility: "[Access]"        # Open | Restricted | Hidden | Forbidden | Lost

# Danger Assessment
threat-level: [1-10]             # Overall danger rating
primary-hazards:
  - "[Hazard 1]"
  - "[Hazard 2]"

# Political/Social
controlling-faction: "[Faction or None]"
population: "[Descriptor]"       # Uninhabited | Sparse | Moderate | Dense | Teeming
settlement-type: "[Type]"        # N/A | Camp | Village | Town | City | Fortress

# Environment
biome: "[Biome]"                 # Boreal | Temperate | Coastal | Mountain | Wasteland | Underground
climate: "[Climate]"             # Frozen | Cold | Temperate | Warm | Arid | Variable
terrain: "[Primary Terrain]"     # Forest | Plains | Hills | Mountains | Swamp | Urban | Subterranean

# Metadata
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []

# Cross-references
parent-region: GAZ-[REGION]-[NNN]   # If this is a sub-location
child-locations:
  - GAZ-[REGION]-[NNN]
related-lore:
  - LORE-[CAT]-[NNN]
related-specs:
  - SPEC-NAV-[NNN]
  - SPEC-ENV-[NNN]

tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Location Card](#1-location-card)
2. [Geographic Profile](#2-geographic-profile)
3. [Environmental Conditions](#3-environmental-conditions)
4. [History & Significance](#4-history--significance)
5. [Points of Interest](#5-points-of-interest)
6. [Inhabitants & Encounters](#6-inhabitants--encounters)
7. [Resources & Economy](#7-resources--economy)
8. [Travel & Navigation](#8-travel--navigation)
9. [Hazards & Challenges](#9-hazards--challenges)
10. [Cultural Elements](#10-cultural-elements)
11. [Narrative Hooks](#11-narrative-hooks)
12. [Room/Area Generation](#12-roomarea-generation)
13. [Implementation Data](#13-implementation-data)
14. [Domain 4 Compliance](#14-domain-4-compliance)
15. [Changelog](#15-changelog)

---

## Location Scale Reference

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       LOCATION SCALE HIERARCHY                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TERRITORY (Largest)                                                        │
│  └── A vast expanse, multiple days' travel across                           │
│      │                                                                      │
│      └── REGION                                                             │
│          └── A significant area, perhaps a day's journey                    │
│              │                                                              │
│              └── DISTRICT / AREA                                            │
│                  └── A defined zone, hours of exploration                   │
│                      │                                                      │
│                      └── SITE                                               │
│                          └── A specific location, single session scope      │
│                              │                                              │
│                              └── ROOM / POINT                               │
│                                  └── Individual encounter space             │
│                                                                             │
│  THIS DOCUMENT COVERS: [SCALE LEVEL]                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Location Card

> ⚠️ REQUIRED | Quick reference summary

```
╔══════════════════════════════════════════════════════════════════════════════╗
║                    LOCATION CARD: [LOCATION NAME]                            ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  OVERVIEW                                                                    ║
║  ────────                                                                    ║
║  ┌──────────────────┬─────────────────────────────────────────────────────┐ ║
║  │ Type             │ [Settlement / Ruin / Wilderness / Dungeon / etc.]   │ ║
║  │ Scale            │ [Site / Area / District / Region / Territory]       │ ║
║  │ Biome            │ [Primary biome type]                                │ ║
║  │ Threat Level     │ [N] - [DESCRIPTOR]                                  │ ║
║  │ Population       │ [Uninhabited / Sparse / Moderate / Dense]           │ ║
║  │ Faction Control  │ [Faction name or "Contested" or "None"]             │ ║
║  └──────────────────┴─────────────────────────────────────────────────────┘ ║
║                                                                              ║
║  AT A GLANCE (Layer 1 - Mythic Voice)                                        ║
║  ───────────────────────────────────                                         ║
║  "[A 2-3 sentence evocative description in mythic voice. What travelers      ║
║   say about this place. The impression it leaves.]"                          ║
║                                                                              ║
║  KEY FEATURES                                                                ║
║  ────────────                                                                ║
║  ├── [Most notable feature]                                                 ║
║  ├── [Second notable feature]                                               ║
║  └── [Third notable feature]                                                ║
║                                                                              ║
║  WHY VISIT                          WHY AVOID                                ║
║  ──────────                         ──────────                               ║
║  ├── [Reason 1]                     ├── [Danger 1]                          ║
║  ├── [Reason 2]                     ├── [Danger 2]                          ║
║  └── [Reason 3]                     └── [Danger 3]                          ║
║                                                                              ║
║  KNOWN ROUTES                                                                ║
║  ────────────                                                                ║
║  ├── From [Place A]: [Travel description - time/difficulty]                 ║
║  └── From [Place B]: [Travel description - time/difficulty]                 ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

---

## 2. Geographic Profile

> ⚠️ REQUIRED

### 2.1 Position & Boundaries

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       GEOGRAPHIC CONTEXT                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                              NORTH                                          │
│                                ▲                                            │
│                                │                                            │
│              ┌─────────────────┼─────────────────┐                          │
│              │                 │                 │                          │
│              │    [Neighbor]   │   [Neighbor]    │                          │
│              │                 │                 │                          │
│  WEST ◄──────┼─────────────────┼─────────────────┼──────► EAST              │
│              │                 │                 │                          │
│              │   [Neighbor]  ╔═╧═╗  [Neighbor]   │                          │
│              │               ║   ║               │                          │
│              │               ║ X ║ ◄── THIS      │                          │
│              │               ║   ║    LOCATION   │                          │
│              │               ╚═╤═╝               │                          │
│              │    [Neighbor]   │   [Neighbor]    │                          │
│              │                 │                 │                          │
│              └─────────────────┼─────────────────┘                          │
│                                │                                            │
│                                ▼                                            │
│                              SOUTH                                          │
│                                                                             │
│  PARENT REGION: [Region Name] (GAZ-XXX-NNN)                                 │
│                                                                             │
│  BORDERS:                                                                   │
│  ├── North: [What lies north - natural or political boundary]               │
│  ├── South: [What lies south]                                               │
│  ├── East: [What lies east]                                                 │
│  └── West: [What lies west]                                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Terrain Composition

| Terrain Type | Prevalence | Description | Traversability |
|--------------|------------|-------------|----------------|
| [Type] | [Dominant/Common/Sparse] | [Brief description] | [Easy/Moderate/Difficult/Dangerous] |
| [Type] | [Dominant/Common/Sparse] | [Brief description] | [Easy/Moderate/Difficult/Dangerous] |

### 2.3 Elevation Profile

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      ELEVATION PROFILE                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  HIGH   │           ╱╲                                                      │
│         │          ╱  ╲        [Peak/High Point]                            │
│         │         ╱    ╲                                                    │
│  MID    │────────╱      ╲───────────────[Notable Plateau]                   │
│         │       ╱        ╲              ╱                                   │
│         │      ╱          ╲────────────╱                                    │
│  LOW    │─────╱            ╲__________╱       [Valley/Low Point]            │
│         │    ╱                                                              │
│  SEA    │───╱─────────────────────────────────────────────                  │
│         └───────────────────────────────────────────────────────────────    │
│           WEST ─────────────────────────────────────────► EAST              │
│                                                                             │
│  GENERAL ELEVATION: [Lowland / Upland / Highland / Mountainous]             │
│  NOTABLE CHANGES: [Describe significant elevation shifts]                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.4 Water Features

| Feature | Type | Description | Significance |
|---------|------|-------------|--------------|
| [Name] | River/Lake/Spring/Coast | [Description] | [Why it matters] |

### 2.5 Sub-Locations

| Location | Type | Link | Brief Description |
|----------|------|------|-------------------|
| [Name] | [Type] | GAZ-XXX-NNN | [One-line summary] |

---

## 3. Environmental Conditions

> ⚠️ REQUIRED

### 3.1 Climate Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CLIMATE PROFILE                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  GENERAL CLIMATE: [Frozen / Cold / Temperate / Warm / Arid / Variable]      │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  SEASONAL VARIATION:                                                        │
│  ┌─────────┬──────────────┬──────────────┬──────────────────────────────┐  │
│  │ Season  │ Temperature  │ Precipitation│ Special Conditions           │  │
│  ├─────────┼──────────────┼──────────────┼──────────────────────────────┤  │
│  │ Spring  │ [Qualitative]│ [Qualitative]│ [Notable effects]            │  │
│  │ Summer  │ [Qualitative]│ [Qualitative]│ [Notable effects]            │  │
│  │ Autumn  │ [Qualitative]│ [Qualitative]│ [Notable effects]            │  │
│  │ Winter  │ [Qualitative]│ [Qualitative]│ [Notable effects]            │  │
│  └─────────┴──────────────┴──────────────┴──────────────────────────────┘  │
│                                                                             │
│  ⚠️ Temperature/precipitation descriptions must be qualitative              │
│     (e.g., "bitter cold," "frequent rains," NOT "5°C," "200mm")             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Weather Patterns

| Condition | Frequency | Effects on Gameplay | Duration |
|-----------|-----------|---------------------|----------|
| [Weather] | [Rare/Occasional/Common/Frequent] | [Mechanical effects] | [Qualitative] |

### 3.3 Day/Night Cycle

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      DIURNAL CYCLE                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DAWN                    MIDDAY                   DUSK                      │
│   │                        │                        │                       │
│   ▼                        ▼                        ▼                       │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │░░░░░▓▓▓▓████████████████████████████████████████▓▓▓▓░░░░░│    │
│  └────────────────────────────────────────────────────────────────────┘    │
│  │        │                 │                 │        │                   │
│  │        │                 │                 │        │                   │
│  │    [Dawn      [Daylight Period]        [Dusk      │                     │
│  │    Activities]                          Activities]                      │
│  │                                                    │                     │
│  [Night            NIGHT PERIOD                       Night                 │
│   Activities]      [What happens at night]            Activities]           │
│                                                                             │
│  DAYLIGHT HOURS: [Qualitative - "long summer days," "short winter light"]   │
│  NOTABLE CYCLES: [Moons, auroras, other phenomena]                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.4 Environmental Hazards

| Hazard | Type | Frequency | Severity | Warning Signs |
|--------|------|-----------|----------|---------------|
| [Hazard] | Natural/Supernatural/Blight | [How often] | [1-10] | [How to detect] |

---

## 4. History & Significance

> 📋 RECOMMENDED

### 4.1 Timeline

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      LOCATION HISTORY                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PRE-GLITCH ERA                                                             │
│  ══════════════                                                             │
│  │                                                                          │
│  ├── [Event/State] - [What existed or happened here before]                 │
│  │   └── Evidence: [How we know this]                                       │
│  │                                                                          │
│  └── [Event/State] - [Last known PRE-Glitch status]                         │
│      └── Evidence: [How we know this]                                       │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                          THE GLITCH                                          │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  │                                                                          │
│  ├── [What happened during the Glitch]                                      │
│  │   └── Impact: [How the location changed]                                 │
│  │                                                                          │
│                                                                             │
│  POST-GLITCH ERA                                                            │
│  ═══════════════                                                            │
│  │                                                                          │
│  ├── Early POST-Glitch: [Initial state after the Glitch]                    │
│  │                                                                          │
│  ├── [Notable Event]: [What happened]                                       │
│  │   └── Impact: [Lasting effects]                                          │
│  │                                                                          │
│  ├── [Notable Event]: [What happened]                                       │
│  │   └── Impact: [Lasting effects]                                          │
│  │                                                                          │
│  └── Present Day: [Current state]                                           │
│                                                                             │
│  RELIABILITY: [HIGH/MEDIUM/LOW] - [Based on what sources]                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Legends & Myths

> Written in Layer 1 (Mythic) voice

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        LOCAL LEGENDS                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  LEGEND: "[Name of Legend]"                                                 │
│  ─────────────────────────                                                  │
│                                                                             │
│  "[The story as told by locals, in mythic voice. Include the moral,         │
│   the warning, or the wonder that the story conveys. This should read       │
│   like something told around a fire.]"                                      │
│                                                                             │
│  KERNEL OF TRUTH: [What factual basis might this legend have?]              │
│  NARRATIVE USE: [How GMs might incorporate this legend]                     │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  LEGEND: "[Name of Legend]"                                                 │
│  ─────────────────────────                                                  │
│                                                                             │
│  "[Another local legend...]"                                                │
│                                                                             │
│  KERNEL OF TRUTH: [Basis]                                                   │
│  NARRATIVE USE: [Usage]                                                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Historical Significance

| Aspect | Significance | Notes |
|--------|--------------|-------|
| Strategic | [Military/trade importance] | [Details] |
| Cultural | [Cultural/religious importance] | [Details] |
| Economic | [Resource/trade importance] | [Details] |
| Mysterious | [Unknown/supernatural importance] | [Details] |

---

## 5. Points of Interest

> ⚠️ REQUIRED

### 5.1 Major Landmarks

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    POINT OF INTEREST: [Name]                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TYPE: [Ruin / Natural Feature / Settlement / Monument / Anomaly]           │
│  VISIBILITY: [Obvious / Hidden / Seasonal / Conditional]                    │
│  ACCESSIBILITY: [Easy / Moderate / Difficult / Dangerous / Impossible]      │
│                                                                             │
│  DESCRIPTION (Layer 2):                                                     │
│  [Diagnostic description of the point of interest, 2-3 sentences]           │
│                                                                             │
│  LOCAL LORE (Layer 1):                                                      │
│  "[What locals say about this place - rumors, warnings, tales]"             │
│                                                                             │
│  GAMEPLAY SIGNIFICANCE:                                                     │
│  ├── Resources: [What can be found here]                                    │
│  ├── Encounters: [What might be encountered]                                │
│  ├── Quests: [Quest connections]                                            │
│  └── Secrets: [Hidden elements - GM eyes only]                              │
│                                                                             │
│  CONNECTED TO: [Other POIs, quests, NPCs]                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Points of Interest Index

| Name | Type | Threat | Notable For | Link |
|------|------|--------|-------------|------|
| [POI Name] | [Type] | [Level] | [Key feature] | [Section/Doc] |
| [POI Name] | [Type] | [Level] | [Key feature] | [Section/Doc] |

### 5.3 Hidden Locations

> 📎 OPTIONAL - Locations not commonly known

| Location | Discovery Method | Requirements | Contents |
|----------|------------------|--------------|----------|
| [Name] | [How to find] | [Skills/items needed] | [What's there] |

---

## 6. Inhabitants & Encounters

> ⚠️ REQUIRED

### 6.1 Population Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      POPULATION PROFILE                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  OVERALL POPULATION DENSITY: [Uninhabited / Sparse / Moderate / Dense]      │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  INTELLIGENT INHABITANTS                                                    │
│  ───────────────────────                                                    │
│  │                                                                          │
│  ├── [Group/Species]: [Population descriptor] - [Disposition to outsiders] │
│  │   └── [Brief description of their presence here]                         │
│  │                                                                          │
│  ├── [Group/Species]: [Population descriptor] - [Disposition]               │
│  │   └── [Brief description]                                                │
│  │                                                                          │
│  └── [Group/Species]: [Population descriptor] - [Disposition]               │
│      └── [Brief description]                                                │
│                                                                             │
│  FAUNA                                                                      │
│  ─────                                                                      │
│  │                                                                          │
│  ├── [Creature]: [Common/Uncommon/Rare] - Threat [N]                        │
│  ├── [Creature]: [Common/Uncommon/Rare] - Threat [N]                        │
│  └── [Creature]: [Common/Uncommon/Rare] - Threat [N]                        │
│                                                                             │
│  FLORA                                                                      │
│  ─────                                                                      │
│  │                                                                          │
│  ├── [Plant]: [Prevalence] - [Useful/Dangerous/Both]                        │
│  └── [Plant]: [Prevalence] - [Useful/Dangerous/Both]                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Encounter Table

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      RANDOM ENCOUNTER TABLE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TERRAIN: [Primary terrain type this table covers]                          │
│  TIME: [Day / Night / Any]                                                  │
│                                                                             │
│  d10  │ Encounter                      │ Threat │ Notes                     │
│  ─────┼────────────────────────────────┼────────┼────────────────────────── │
│   1   │ [Environmental/non-combat]     │  ---   │ [Flavor, discovery, etc.] │
│   2   │ [Low threat encounter]         │  Low   │ [Brief description]       │
│   3   │ [Low threat encounter]         │  Low   │ [Brief description]       │
│   4   │ [Low-moderate encounter]       │  Low   │ [Brief description]       │
│   5   │ [Moderate encounter]           │  Med   │ [Brief description]       │
│   6   │ [Moderate encounter]           │  Med   │ [Brief description]       │
│   7   │ [Moderate-high encounter]      │  Med   │ [Brief description]       │
│   8   │ [High threat encounter]        │  High  │ [Brief description]       │
│   9   │ [High threat encounter]        │  High  │ [Brief description]       │
│  10   │ [Special/rare encounter]       │  Var   │ [Brief description]       │
│                                                                             │
│  FREQUENCY: Roll every [time period - qualitative]                          │
│  MODIFIERS: [Conditions that modify the table]                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Notable NPCs

| Name | Role | Location | Disposition | Quest Giver? |
|------|------|----------|-------------|--------------|
| [Name] | [Role/Title] | [Where found] | [Friendly/Neutral/Hostile] | [Yes/No] |

### 6.4 Faction Presence

| Faction | Strength | Territory | Activities |
|---------|----------|-----------|------------|
| [Faction] | [Weak/Moderate/Strong/Dominant] | [Where specifically] | [What they do] |

---

## 7. Resources & Economy

> 📋 RECOMMENDED

### 7.1 Natural Resources

| Resource | Type | Abundance | Extraction Difficulty | Notes |
|----------|------|-----------|----------------------|-------|
| [Resource] | [Raw Material/Food/Water/etc.] | [Scarce/Limited/Moderate/Abundant] | [Easy/Moderate/Difficult/Dangerous] | [Details] |

### 7.2 Trade & Commerce

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      ECONOMIC PROFILE                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ECONOMIC STATUS: [Destitute / Poor / Modest / Prosperous / Wealthy]        │
│                                                                             │
│  EXPORTS (What leaves):                                                     │
│  ├── [Export 1]: [Description, destination]                                 │
│  ├── [Export 2]: [Description, destination]                                 │
│  └── [Export 3]: [Description, destination]                                 │
│                                                                             │
│  IMPORTS (What arrives):                                                    │
│  ├── [Import 1]: [Description, source]                                      │
│  ├── [Import 2]: [Description, source]                                      │
│  └── [Import 3]: [Description, source]                                      │
│                                                                             │
│  TRADE ROUTES:                                                              │
│  ├── To [Destination]: [Route description, dangers]                         │
│  └── To [Destination]: [Route description, dangers]                         │
│                                                                             │
│  CURRENCY: [What's used for trade here]                                     │
│  MARKET: [None / Periodic / Daily / Always Available]                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.3 Services Available

| Service | Provider | Quality | Cost Level |
|---------|----------|---------|------------|
| [Service] | [Who provides] | [Poor/Average/Good/Excellent] | [Cheap/Fair/Expensive/Exorbitant] |

---

## 8. Travel & Navigation

> ⚠️ REQUIRED

### 8.1 Access Routes

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      TRAVEL ROUTES                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ROUTE: [Origin] → [This Location]                                          │
│  ═══════════════════════════════════                                        │
│                                                                             │
│  DISTANCE:     [Qualitative - "three days' travel," "a morning's walk"]     │
│  DIFFICULTY:   [Easy / Moderate / Difficult / Treacherous]                  │
│  TERRAIN:      [Primary terrain encountered]                                │
│  HAZARDS:      [Main dangers along the way]                                 │
│  LANDMARKS:    [Navigation waypoints]                                       │
│  SEASON:       [When passable - "all year," "summer only," etc.]            │
│                                                                             │
│  PATH TYPE:                                                                 │
│  ├── [ ] Established road                                                   │
│  ├── [ ] Maintained trail                                                   │
│  ├── [ ] Game trail / unmaintained                                          │
│  ├── [ ] Cross-country / no path                                            │
│  └── [ ] Secret / hidden route                                              │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  ROUTE: [Origin] → [This Location]                                          │
│  [Repeat for each significant route...]                                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Internal Navigation

| Area | Connection To | Traversal | Notes |
|------|---------------|-----------|-------|
| [Area A] | [Area B] | [How to move between] | [Conditions, hazards] |

### 8.3 Navigation Challenges

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   NAVIGATION HAZARDS                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  GETTING LOST:                                                              │
│  ├── Risk Level: [Low / Moderate / High / Extreme]                          │
│  ├── Factors: [What makes navigation difficult]                             │
│  └── Recovery: [How to reorient]                                            │
│                                                                             │
│  BLOCKED PATHS:                                                             │
│  ├── [Obstacle]: [Seasonal? Permanent? How to bypass?]                      │
│  └── [Obstacle]: [Details]                                                  │
│                                                                             │
│  DANGEROUS CROSSINGS:                                                       │
│  ├── [Crossing]: [Risk level, mitigation]                                   │
│  └── [Crossing]: [Risk level, mitigation]                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 9. Hazards & Challenges

> ⚠️ REQUIRED

### 9.1 Environmental Hazards

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    HAZARD: [Hazard Name]                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TYPE:        [Natural / Supernatural / Blight / Machine-remnant]           │
│  FREQUENCY:   [Constant / Common / Occasional / Rare]                       │
│  SEVERITY:    [Nuisance / Dangerous / Deadly / Catastrophic]                │
│  LOCATION:    [Where in this location it occurs]                            │
│                                                                             │
│  DESCRIPTION (Layer 2):                                                     │
│  [Clinical description of the hazard]                                       │
│                                                                             │
│  WARNING SIGNS:                                                             │
│  ├── [Sign 1]: [What to look for]                                           │
│  └── [Sign 2]: [What to look for]                                           │
│                                                                             │
│  EFFECTS:                                                                   │
│  ├── Immediate: [What happens on exposure]                                  │
│  ├── Prolonged: [Effects of extended exposure]                              │
│  └── Recovery: [How effects are treated/recovered from]                     │
│                                                                             │
│  MITIGATION:                                                                │
│  ├── Prevention: [How to avoid]                                             │
│  └── Protection: [How to resist/survive]                                    │
│                                                                             │
│  GAME MECHANICS: [Reference to relevant spec or simple rule]                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Hazard Summary Table

| Hazard | Type | Location | Frequency | Severity |
|--------|------|----------|-----------|----------|
| [Name] | [Type] | [Where] | [How often] | [Impact] |

### 9.3 Blight Presence

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      BLIGHT ASSESSMENT                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  BLIGHT CONTAMINATION LEVEL:                                                │
│                                                                             │
│  [░░░░░░░░░░] Clear        - No detectable Blight presence                  │
│  [██░░░░░░░░] Trace        - Minimal, localized corruption                  │
│  [████░░░░░░] Light        - Noticeable corruption, manageable              │
│  [██████░░░░] Moderate     - Significant corruption, caution needed         │
│  [████████░░] Heavy        - Severe corruption, serious danger              │
│  [██████████] Saturated    - Total corruption, extreme danger               │
│                                                                             │
│  This Location: [████░░░░░░] Light                                          │
│                                                                             │
│  MANIFESTATIONS:                                                            │
│  ├── [How the Blight appears here]                                          │
│  ├── [Effects on environment]                                               │
│  └── [Effects on inhabitants]                                               │
│                                                                             │
│  PROGRESSION: [Stable / Spreading / Receding / Fluctuating]                 │
│                                                                             │
│  SOURCES: [Known or suspected sources of corruption]                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 10. Cultural Elements

> 📋 RECOMMENDED

### 10.1 Local Customs

| Custom | Context | Significance | Outsider Impact |
|--------|---------|--------------|-----------------|
| [Custom] | [When practiced] | [Why important] | [How visitors should behave] |

### 10.2 Beliefs & Superstitions

[Describe local beliefs specific to this location]

### 10.3 Architecture & Aesthetics

[Describe the visual character of any structures - building styles, materials, state of repair]

### 10.4 Language & Communication

| Aspect | Description |
|--------|-------------|
| Languages Spoken | [What languages are used] |
| Literacy | [Level of literacy] |
| Signs/Markers | [How locations are marked] |

---

## 11. Narrative Hooks

> 📋 RECOMMENDED

### 11.1 Quest Seeds

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    QUEST SEED: [Quest Name]                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TYPE: [Main / Side / Discovery / Repeatable]                               │
│  TRIGGER: [How players learn of this]                                       │
│                                                                             │
│  PREMISE:                                                                   │
│  [2-3 sentence quest hook]                                                  │
│                                                                             │
│  OBJECTIVES:                                                                │
│  ├── [Primary objective]                                                    │
│  ├── [Secondary objective - optional]                                       │
│  └── [Hidden objective - optional]                                          │
│                                                                             │
│  COMPLICATIONS:                                                             │
│  ├── [Potential complication]                                               │
│  └── [Potential complication]                                               │
│                                                                             │
│  REWARDS:                                                                   │
│  ├── [Tangible reward]                                                      │
│  └── [Intangible reward - reputation, knowledge, etc.]                      │
│                                                                             │
│  CONNECTIONS: [Related quests, NPCs, locations]                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 11.2 Mysteries & Secrets

| Mystery | Clues Available | Truth | Discovery Impact |
|---------|-----------------|-------|------------------|
| [Mystery] | [What players can find] | [The actual answer] | [What changes when revealed] |

### 11.3 Conflict Opportunities

| Conflict | Parties | Stakes | Player Entry Points |
|----------|---------|--------|---------------------|
| [Conflict] | [Who vs who] | [What's at stake] | [How players get involved] |

---

## 12. Room/Area Generation

> ⚠️ REQUIRED (for dungeons/explorable sites)

### 12.1 Area Theme Tags

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      GENERATION PARAMETERS                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  THEME TAGS (for procedural generation):                                    │
│  ├── Primary: [main theme tag]                                              │
│  ├── Secondary: [supporting theme tag]                                      │
│  └── Atmosphere: [mood/atmosphere tag]                                      │
│                                                                             │
│  ROOM TYPE DISTRIBUTION:                                                    │
│  ├── [Room Type A]: [Frequency weight]                                      │
│  ├── [Room Type B]: [Frequency weight]                                      │
│  ├── [Room Type C]: [Frequency weight]                                      │
│  └── [Special Room]: [Frequency weight]                                     │
│                                                                             │
│  ENCOUNTER DENSITY: [Sparse / Normal / Dense]                               │
│  LOOT DENSITY: [Poor / Normal / Rich]                                       │
│  HAZARD DENSITY: [Safe / Normal / Dangerous]                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 12.2 Sample Room Descriptions

> For procedural room generation flavor text

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ROOM TEMPLATE: [Room Type]                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DESCRIPTION VARIANTS (Layer 2 compliant):                                  │
│                                                                             │
│  1. "[Room description variant - use for pristine/intact version]"          │
│                                                                             │
│  2. "[Room description variant - use for damaged/ruined version]"           │
│                                                                             │
│  3. "[Room description variant - use for corrupted/blighted version]"       │
│                                                                             │
│  SENSORY DETAILS:                                                           │
│  ├── Sight: [Visual elements]                                               │
│  ├── Sound: [Audio elements]                                                │
│  ├── Smell: [Olfactory elements]                                            │
│  └── Feel: [Tactile/atmospheric elements]                                   │
│                                                                             │
│  INTERACTIVE ELEMENTS:                                                      │
│  ├── [Element]: [What players can do with it]                               │
│  └── [Element]: [What players can do with it]                               │
│                                                                             │
│  ENCOUNTER SPAWN POINTS: [Where enemies might appear]                       │
│  LOOT LOCATIONS: [Where treasure might be found]                            │
│  EXIT TYPES: [Door/Passage/Ladder/etc. options]                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 13. Implementation Data

> ⚠️ REQUIRED

### 13.1 Specification References

| Spec ID | Relationship | Notes |
|---------|--------------|-------|
| SPEC-NAV-[NNN] | Uses | Navigation system integration |
| SPEC-ENV-[NNN] | Uses | Environmental system integration |
| SPEC-SPAWN-[NNN] | Uses | Spawn/population system |
| SPEC-DUNGEON-[NNN] | Uses | Dungeon generation (if applicable) |

### 13.2 Asset Requirements

| Asset Type | Description | Priority |
|------------|-------------|----------|
| Tileset | [Visual requirements] | [P1-P4] |
| Ambient Audio | [Sound requirements] | [P1-P4] |
| Music | [Soundtrack requirements] | [P1-P4] |
| VFX | [Visual effects needed] | [P1-P4] |

### 13.3 Map Data

> 📎 OPTIONAL - Include if detailed map exists

[Reference to map file or ASCII map representation]

---

## 14. Domain 4 Compliance

> ⚠️ REQUIRED

### 14.1 Compliance Verification

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DOMAIN 4 COMPLIANCE CHECK                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PRECISION MEASUREMENT SCAN:                                                │
│  ├── [ ] No metric distances (meters, kilometers)                           │
│  ├── [ ] No imperial distances (feet, miles)                                │
│  ├── [ ] No exact populations (use "handful," "scores," "teeming")          │
│  ├── [ ] No exact temperatures                                              │
│  ├── [ ] No exact times (use "candle-marks," "a day's travel")              │
│  └── [ ] No exact areas/volumes                                             │
│                                                                             │
│  TRAVEL TIME FORMAT:                                                        │
│  ├── [ ] Uses qualitative travel times                                      │
│  └── [ ] References landmarks, not coordinates                              │
│                                                                             │
│  VOICE DISCIPLINE:                                                          │
│  ├── [ ] Layer 1 sections maintain mythic voice                             │
│  ├── [ ] Layer 2 sections maintain diagnostic voice                         │
│  └── [ ] No omniscient perspective claims                                   │
│                                                                             │
│  OVERALL: [ ] COMPLIANT  [ ] VIOLATIONS FOUND                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 14.2 Distance Translation Reference

| Concept | Compliant Expression |
|---------|----------------------|
| 1 km | "A morning's steady walk" |
| 5 km | "Half a day's travel" |
| 20 km | "A full day's journey" |
| 100 km | "Several days' travel" |
| Exact coordinates | "Near the old stone circle, north of the river fork" |

---

## 15. Changelog

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
║  □ Location Card completed                                                    ║
║  □ Geographic profile established                                             ║
║  □ At least one travel route documented                                       ║
║  □ Encounter table populated                                                  ║
║  □ Major hazards identified                                                   ║
║  □ Points of interest documented                                              ║
║  □ Domain 4 compliance verified                                               ║
║  □ All distances use qualitative measures                                     ║
║  □ Status set to 'review' when ready                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
