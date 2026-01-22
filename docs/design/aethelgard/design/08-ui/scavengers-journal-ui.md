---
id: SPEC-UI-SCAVENGERS-JOURNAL
title: "Scavenger's Journal UI — TUI & GUI Specification"
version: 1.0
status: draft
last-updated: 2025-12-15
related-files:
  - path: "docs/08-ui/quest-journal-ui.md"
    status: Reference (Contracts tab)
  - path: "docs/08-ui/dialogue-ui.md"
    status: Reference (keyword unlocks)
  - path: "docs/01-core/resources/stress.md"
    status: Reference (glitch system)
  - path: "docs/01-core/resources/corruption.md"
    status: Reference (margin notes)
  - path: "docs/99-legacy/Imported Game Docs/specifications/Scavenger's Journal GUI Specification 2ba55eb312da806fa31ecf83dd2c4155.md"
    status: Legacy Reference
---

# Scavenger's Journal UI — TUI & GUI Specification

> *"The world's memory is shattered. Every data-slate, every echo-recording, every scrawled note is a fragment of what was. Piece them together, and perhaps you'll understand why it all fell apart."*

---

## 1. Overview

The Scavenger's Journal is a **diegetic, in-world object** that serves as the player's personal chronicle and research tool for navigating Aethelgard's corrupted reality. It is simultaneously a codex, bestiary, quest log, and fragmented puzzle — populated through discovery, examination, and the assembly of collected **Data Captures**.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-SCAVENGERS-JOURNAL` |
| Category | UI System |
| Priority | High |
| Status | Draft |

### 1.2 Core Concept: The Fragmented Puzzle

Unlike traditional codex systems where entries appear fully-formed, the Scavenger's Journal treats knowledge as a **puzzle to be assembled**. Players collect **Data Captures** — fragments of information scattered throughout the world — and piece them together to form complete entries.

| Traditional Codex | Scavenger's Journal |
|-------------------|---------------------|
| Find lore object → Entry unlocks | Find Data Capture → Fragment added |
| Entry is complete immediately | Entry builds progressively |
| Passive collection | Active assembly |
| Information given | Information earned |

### 1.3 Journal Sections

| Section | Icon | Purpose | Primary Content |
|---------|------|---------|-----------------|
| **Codex** | `📜` | World lore and history | Data-log fragments → assembled entries |
| **Bestiary** | `📖` | Creature knowledge | Combat data, weaknesses, lore |
| **Field Guide** | `📋` | Mechanics glossary | In-world rules explanations |
| **Contracts** | `📝` | Quest tracking | Active/completed quests |
| **Contacts** | `👥` | NPCs and factions | Met characters, reputation |
| **Data Captures** | `💾` | Unassigned fragments | Captures awaiting entry assignment |

### 1.4 Design Philosophy

- **Diegetic Interface**: The journal exists as an in-world object, not a game menu
- **Knowledge as Currency**: Information is bought with effort and sometimes sanity
- **Progressive Assembly**: Entries are puzzles built from scattered fragments
- **Dynamic Corruption**: UI reflects character's mental state through visual glitches
- **Rewarded Analysis**: Observation and investigation are core progression mechanics
- **No Inventory Burden**: Data Captures are transcribed into the journal, not carried

### 1.5 Storage Model

> [!IMPORTANT]
> **Data Captures do not consume inventory space.** The journal is a separate knowledge repository — when players "collect" a data-slate or examine an object, the information is transcribed into the journal. The physical object is not carried.

This means:
- Players never choose between knowledge and loot
- Exploration is always rewarded without penalty
- There is no limit to how many captures can be collected

---

## 2. Data Capture System

### 2.1 What Are Data Captures?

Data Captures are **fragments of information** found throughout the world. They are the building blocks of Codex and Bestiary entries — collectible pieces that must be assembled to reveal complete knowledge.

### 2.2 Capture Types

| Type | Icon | Source | Example |
|------|------|--------|---------|
| **Text Fragment** | `📄` | Readable objects, inscriptions, data-slates | *"...the Rust Lord held dominion..."* |
| **Echo Recording** | `🔊` | Audio logs, memory echoes | *Corrupted voice recording* |
| **Visual Record** | `📸` | Diagrams, schematics, images | *Partial schematic fragment* |
| **Specimen** | `🧪` | Creature examination, material analysis | *Rust-Horror carapace sample* |
| **Oral History** | `💬` | NPC dialogue, specialist insights | *"My grandfather said..."* |
| **Runic Trace** | `ᚱ` | Rune analysis, aether residue | *Decoded rune pattern* |

### 2.3 Capture Acquisition

| Method | Trigger | Stress Cost |
|--------|---------|-------------|
| **Discovery** | Find readable object | None |
| **Examination** | Use `examine` on entity | 5-15 |
| **Specialist Analysis** | Use specialization ability | Varies |
| **Dialogue** | Complete conversation branch | None |
| **Quest Reward** | Complete objective | None |
| **Environmental** | Enter specific locations | None |

### 2.4 TUI: Capture Acquisition Display

```
╔═══════════════════════════════════════════════════════════════════╗
║  DATA CAPTURE ACQUIRED                                            ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  📄 TEXT FRAGMENT                                                 ║
║  "The Rust Lord's Domain"                                         ║
║                                                                   ║
║  ─────────────────────────────────────────────────────────────    ║
║  "...and the Rust Lord held dominion over the Iron Crypts,        ║
║  his iron heart beating in rhythm with the forge's forgotten      ║
║  fires. None who entered his domain returned unchanged..."        ║
║  ─────────────────────────────────────────────────────────────    ║
║                                                                   ║
║  Source: Data-slate in Ironhold Ruins                             ║
║                                                                   ║
║  ═══════════════════════════════════════════════════════════════  ║
║  MATCHING ENTRY: The Rust Lord                                    ║
║  Progress: ███████████░░░░░░ 3/5 fragments                        ║
║  ═══════════════════════════════════════════════════════════════  ║
║                                                                   ║
║  [V] View in Codex  [A] Auto-assign  [C] Continue                 ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 2.5 Fragment-to-Entry Assignment

Data Captures can match **one or more** potential entries. The system suggests matches, but players can manually assign fragments:

| Assignment Mode | Description |
|-----------------|-------------|
| **Auto-assign** | System assigns to best-matching entry |
| **Manual assign** | Player chooses which entry receives fragment |
| **Unassigned** | Fragment stored in Data Captures section |

### 2.6 Entry Completion Rewards

| Completion % | Status | Reward |
|--------------|--------|--------|
| 1+ fragments | `Stub` | Entry visible, +10 Legend |
| 50% fragments | `Partial` | Core info revealed, +25 Legend |
| 100% fragments | `Complete` | Full entry, +50-100 Legend |
| 100% + Specialist | `Mastered` | Tactical notes, +100 Legend |

---

## 3. TUI: Journal Main View

### 3.1 Full Journal Screen

Accessed via `journal` or `J`:

```
┌─────────────────────────────────────────────────────────────────────┐
│  THE SCAVENGER'S JOURNAL                                   [J]ournal │
├─────────────────────────────────────────────────────────────────────┤
│  [1] Codex  [2] Bestiary  [3] Field Guide  [4] Contracts  [5] More  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  CODEX                                              47/156 entries  │
│  ═══════════════════════════════════════════════════════════════   │
│                                                                     │
│  Filter: [All Categories ▼]    Sort: [Progress ▼]    🔍 Search...  │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  📜 THE RUST LORD                              ████████████░░░ 4/5  │
│     BlightOrigin | The Silence                              [NEW]   │
│                                                                     │
│  📜 THE ALL-RUNE PARADOX                       ████████████████ 5/5  │
│     BlightOrigin | Pre-Silence                          [COMPLETE]  │
│                                                                     │
│  📜 DVERGR ENGINEERING PRINCIPLES              ████░░░░░░░░░░░ 2/7  │
│     TechnicalKnowledge | Pre-Silence                                │
│                                                                     │
│  📜 THE IRON HEART PROTOCOL                    ██░░░░░░░░░░░░░ 1/6  │
│     TechnicalKnowledge | The Silence                                │
│                                                                     │
│  📜 THE GOD-SLEEPER CULT                       ░░░░░░░░░░░░░░░ 0/4  │
│     ReligiousText | Post-Silence                            [STUB]  │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [↑↓] Navigate  [Enter] View  [F] Filter  [S] Sort  [C] Close       │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.2 Tab System

| Tab | Key | Shows |
|-----|-----|-------|
| **Codex** | `1` | Lore entries with fragment progress |
| **Bestiary** | `2` | Creature entries with discovery levels |
| **Field Guide** | `3` | Mechanics entries |
| **Contracts** | `4` | Opens Quest Journal (see [quest-journal-ui.md](quest-journal-ui.md)) |
| **More** | `5` | Contacts, Data Captures, Stats |

### 3.3 Entry List Format

```
📜 THE RUST LORD                              ████████████░░░ 4/5
   BlightOrigin | The Silence                              [NEW]
```

| Element | Description |
|---------|-------------|
| `📜` | Entry type icon |
| `THE RUST LORD` | Entry title |
| `████████████░░░` | Fragment progress bar |
| `4/5` | Fragments collected / total |
| `BlightOrigin` | Category |
| `The Silence` | Era/time period |
| `[NEW]` | Status badge |

### 3.4 Status Badges

| Badge | Meaning |
|-------|---------|
| `[NEW]` | Recently updated, unread |
| `[STUB]` | 0 fragments, only name known |
| `[PARTIAL]` | 1-99% fragments |
| `[COMPLETE]` | 100% fragments |
| `[MASTERED]` | Complete + specialist insight |
| `[CORRUPTED]` | Entry affected by Blight |

---

## 4. TUI: Codex Entry View

### 4.1 Incomplete Entry Display

Entries with missing fragments show **redacted content**:

```
╔═══════════════════════════════════════════════════════════════════╗
║  📜 THE RUST LORD                                                  ║
║  ─────────────────────────────────────────────────────────────────  ║
║  Category: BlightOrigin | Era: The Silence | Fragments: 3/5        ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  [Fragment 001 — Data-slate, Ironhold Ruins]                       ║
║  ════════════════════════════════════════════                      ║
║  "The Rust Lord held dominion over the Iron Crypts, his iron       ║
║  heart beating in rhythm with the forge's forgotten fires.         ║
║  None who entered his domain returned unchanged..."                ║
║                                                                   ║
║  [Fragment 002 — Echo Recording, Sector 7]                         ║
║  ════════════════════════════════════════════                      ║
║  "...they say he was once a █████████ of the Dvergr forges,        ║
║  before the Blight twisted his purpose. Now he guards what         ║
║  he was made to create..."                                         ║
║                                                                   ║
║  [Fragment 003 — Oral History, Kjartan]                            ║
║  ════════════════════════════════════════════                      ║
║  "My grandfather spoke of a ████████████ beneath Ironhold.         ║
║  He called it the 'heart that never stops.' Said the Rust          ║
║  Lord was bound to it, couldn't leave even if he wanted..."        ║
║                                                                   ║
║  [Fragment 004 — MISSING]                                          ║
║  ════════════════════════════════════════════                      ║
║  ████████████████████████████████████████████████████████          ║
║  Hint: Explore deeper into the Iron Crypts                         ║
║                                                                   ║
║  [Fragment 005 — MISSING]                                          ║
║  ════════════════════════════════════════════════════              ║
║  ████████████████████████████████████████████████████████          ║
║  Hint: Defeat the Rust Lord to learn his true nature               ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  KEYWORDS UNLOCKED                                                 ║
║  • "Rust Lord" — Can be used in dialogue with Ironhold NPCs        ║
║  • "Iron Heart" — Opens new dialogue with Dvergr scholars          ║
╟───────────────────────────────────────────────────────────────────╢
║  RELATED ENTRIES                                                   ║
║  • Bestiary: Rust-Horror (mentions Rust Lord's servants)           ║
║  • Codex: The Iron Heart Protocol (2/6 fragments)                  ║
╟───────────────────────────────────────────────────────────────────╢
║  [B] Back  [T] Track  [R] Mark Read                                ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 4.2 Complete Entry Display

```
╔═══════════════════════════════════════════════════════════════════╗
║  📜 THE ALL-RUNE PARADOX                              [COMPLETE]   ║
║  ─────────────────────────────────────────────────────────────────  ║
║  Category: BlightOrigin | Era: Pre-Silence | Fragments: 5/5        ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  ASSEMBLED ENTRY                                                   ║
║  ════════════════                                                  ║
║  The All-Rune was the crowning achievement of Dvergr artificers    ║
║  — an attempt to create a single rune that contained all meaning,  ║
║  all possibility, all outcomes. The Aesir Council authorized       ║
║  Project Gungnir with the belief that a unified runic language     ║
║  would perfect the FUTHARK Protocol.                               ║
║                                                                   ║
║  Instead, when the final inscription was carved, reality itself    ║
║  rejected the paradox of infinite meaning. The All-Rune could      ║
║  not define itself while defining everything else. The resulting   ║
║  recursive cascade corrupted the compiler, not the substrate —     ║
║  and the Runic Blight was born.                                    ║
║                                                                   ║
║  Some whisper that the All-Rune still exists, buried deep          ║
║  beneath Jötunheim. They say it pulses with anti-meaning,          ║
║  rewriting any who approach...                                     ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  KEYWORDS UNLOCKED                                                 ║
║  • "All-Rune" — Can be used in dialogue with scholars              ║
║  • "Project Gungnir" — Opens new dialogue with Jötun-Readers       ║
║  • "The Paradox" — Opens new dialogue with Vard-Wardens            ║
╟───────────────────────────────────────────────────────────────────╢
║  FRAGMENTS COLLECTED                                               ║
║  ☑ Data-slate, Hall of Echoes                                      ║
║  ☑ Echo Recording, ODIN Archive                                    ║
║  ☑ Runic Trace, Collapsed Laboratory                               ║
║  ☑ Oral History, Elder Sigrún                                      ║
║  ☑ Visual Record, Dvergr Schematic                                 ║
╟───────────────────────────────────────────────────────────────────╢
║  RELATED ENTRIES                                                   ║
║  • Codex: The Iron Heart Protocol                                  ║
║  • Codex: The God-Sleeper Cult                                     ║
║  • Bestiary: Void-Touched (mentions All-Rune exposure)             ║
╟───────────────────────────────────────────────────────────────────╢
║  +100 Legend (Entry Complete)                                      ║
╟───────────────────────────────────────────────────────────────────╢
║  [B] Back  [T] Track  [R] Mark Read                                ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 4.3 Fragment Hint System

Missing fragments show contextual hints based on game state:

| Hint Type | Example |
|-----------|---------|
| **Location** | *"Explore deeper into the Iron Crypts"* |
| **Action** | *"Defeat the Rust Lord to learn his true nature"* |
| **Examination** | *"Examine a Rust-Horror corpse with WITS 14+"* |
| **Dialogue** | *"Ask Kjartan about Dvergr legends"* |
| **Specialist** | *"A Jötun-Reader could analyze this mechanism"* |
| **Quest** | *"Complete 'The Iron Path' to unlock"* |

---

## 5. TUI: Bestiary Section

### 5.1 Bestiary Entry List

```
┌─────────────────────────────────────────────────────────────────────┐
│  BESTIARY                                           23/89 creatures │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Filter: [All Types ▼]    Sort: [Discovery ▼]    🔍 Search...      │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  ⚙ RUSTED SERVITOR                                 ●●● [MASTERED]  │
│    Mechanical | Jötun-Forged | Common                               │
│                                                                     │
│  💀 FORLORN WANDERER                               ●●○ [PARTIAL]   │
│    Undying | Corrupted | Common                            [NEW]    │
│                                                                     │
│  ☣ RUST-HORROR                                     ●○○ [STUB]      │
│    Blighted | Unknown | Uncommon                                    │
│                                                                     │
│  ★ THE RUST LORD                                   ░░░ [UNKNOWN]   │
│    Boss | ??? | Unique                                              │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [↑↓] Navigate  [Enter] View  [E] Examine  [T] Track  [C] Close     │
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 Discovery Levels

| Level | Icon | Content Visible | Acquisition |
|-------|------|-----------------|-------------|
| **Unknown** | `░░░` | Name only (if seen) | First sighting |
| **Stub** | `●○○` | Name, type, basic description | Defeat enemy |
| **Partial** | `●●○` | + Combat stats, some resistances | Examine (DC 10-14) |
| **Complete** | `●●●` | + All abilities, vulnerabilities, lore | Examine (DC 18+) |
| **Mastered** | `★★★` | + Tactical notes, hidden abilities | Specialist analysis |

### 5.3 Bestiary Entry View

```
╔═══════════════════════════════════════════════════════════════════╗
║  ⚙ RUSTED SERVITOR                                 ●●● [MASTERED] ║
╟───────────────────────────────────────────────────────────────────╢
║  Classification: Mechanical | Jötun-Forged | Common               ║
║  Corruption Level: ██░░░░░░░░ Minimal                             ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  DESCRIPTION                                                      ║
║  ════════════                                                     ║
║  A crudely humanoid automaton, its code seems to be a corrupted   ║
║  version of a basic security or labor unit from the Old World.    ║
║  Heavy plating protects vital systems, but rusted joints betray   ║
║  structural weaknesses.                                           ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  COMBAT DATA                                                      ║
║  ════════════                                                     ║
║  ┌─────────────┬─────────────┬─────────────┐                      ║
║  │ HP: 45      │ Soak: 3     │ Speed: 4    │                      ║
║  └─────────────┴─────────────┴─────────────┘                      ║
║                                                                   ║
║  Resistances:    Physical (High), Fire (Low)                      ║
║  Vulnerabilities: Corrosion, Lightning                            ║
║  Immunities:     Poison, Psychic                                  ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  ABILITIES                                                        ║
║  ═════════                                                        ║
║  • Crushing Blow — Heavy melee attack (2d8+3 damage)              ║
║  • Defensive Protocol — Gains +2 Soak for 2 turns                 ║
║  • Emergency Repair — Restores 10 HP (1/encounter)                ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  TACTICAL NOTES (Jötun-Reader Insight)                            ║
║  ════════════════════════════════════                             ║
║  Analysis of the core logic reveals a critical flaw in its        ║
║  power regulation subroutine. A powerful electrical surge has     ║
║  a high probability of causing a catastrophic system reboot,      ║
║  manifesting as the [Stunned] status for 2 turns.                 ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  LORE FRAGMENTS                                                   ║
║  ═══════════════                                                  ║
║  ☑ "The Servitors were the backbone of Dvergr industry..."        ║
║  ☑ "Model 7 units were mass-produced for mining operations..."    ║
║  ☐ [Undiscovered — Examine in Jötunheim]                          ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  [B] Back  [T] Track Weaknesses  [R] Mark Read                    ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 5.4 Creature Classification

| Category | Icon | Examples |
|----------|------|----------|
| **Mechanical** | `⚙` | Servitor, Automaton, Construct |
| **Undying** | `💀` | Forlorn, Revenant, Wraith |
| **Blighted** | `☣` | Rust-Horror, Blight-Spawn |
| **Beast** | `🐺` | Corrupted Wolf, Cave Lurker |
| **Humanoid** | `👤` | Raider, Cultist, Scavenger |
| **Metaphysical** | `✧` | Void-Touched, Aether Entity |
| **Boss** | `★` | Sector Guardian, Named Enemy |

---

## 6. TUI: Field Guide Section

### 6.1 Purpose

The Field Guide provides **in-world explanations of game mechanics** — written as the character's understanding of how corrupted reality operates.

### 6.2 Field Guide Entry List

```
┌─────────────────────────────────────────────────────────────────────┐
│  FIELD GUIDE                                       12/34 topics     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Filter: [All Topics ▼]    Sort: [Category ▼]                      │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  🧠 TRAUMA ECONOMY                                                  │
│  ─────────────────                                                  │
│  📋 Psychic Stress: The Weight of Knowing          [DISCOVERED]    │
│  📋 Runic Blight Corruption                        [DISCOVERED]    │
│  📋 Breaking Points & Traumas                      [DISCOVERED]    │
│  📋 Sanctuary Mechanics                            [UNDISCOVERED]  │
│                                                                     │
│  ⚔ COMBAT SYSTEMS                                                   │
│  ─────────────────                                                  │
│  📋 Dice Pool Mechanics                            [DISCOVERED]    │
│  📋 Stances & Positioning                          [UNDISCOVERED]  │
│  📋 Status Effects                                 [DISCOVERED]    │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [↑↓] Navigate  [Enter] View  [C] Close                             │
└─────────────────────────────────────────────────────────────────────┘
```

### 6.3 Field Guide Entry View

```
╔═══════════════════════════════════════════════════════════════════╗
║  📋 PSYCHIC STRESS: THE WEIGHT OF KNOWING                         ║
╟───────────────────────────────────────────────────────────────────╢
║  Category: Trauma Economy | Status: Discovered                    ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  SURVIVOR'S NOTES                                                 ║
║  ════════════════                                                 ║
║  "The mind was not meant to comprehend the Blight's anti-logic.   ║
║  Every time I peer too deeply into corrupted systems, I feel      ║
║  something crack inside me. The old-timers call it 'Stress' —     ║
║  the weight of knowing too much about a reality that shouldn't    ║
║  exist."                                                          ║
║                                                                   ║
║  MECHANICAL UNDERSTANDING                                         ║
║  ════════════════════════                                         ║
║  • Psychic Stress ranges from 0-100                               ║
║  • Examining Blighted entities costs 5-15 Stress                  ║
║  • At 25/50/75 thresholds, negative effects trigger               ║
║  • Reaching 100 causes a Breaking Point                           ║
║  • Rest at Sanctuary rooms to reduce Stress                       ║
║                                                                   ║
║  THRESHOLD EFFECTS                                                ║
║  ┌────────────┬──────────────────────────────────────────────┐    ║
║  │ 25% Low    │ Minor penalties to WITS checks               │    ║
║  │ 50% Mod    │ Random intrusive thoughts, -1 to all rolls   │    ║
║  │ 75% High   │ Disadvantage on mental saves                 │    ║
║  │ 100% Break │ Permanent Trauma acquired                    │    ║
║  └────────────┴──────────────────────────────────────────────┘    ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  RELATED ENTRIES                                                  ║
║  • Runic Blight Corruption                                        ║
║  • Breaking Points & Traumas                                      ║
║  • Sanctuary Mechanics                                            ║
╟───────────────────────────────────────────────────────────────────╢
║  [B] Back  [R] Mark Read                                          ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 6.4 Field Guide Categories

| Category | Icon | Topics |
|----------|------|--------|
| **Trauma Economy** | `🧠` | Stress, Corruption, Traumas, Recovery |
| **Combat Systems** | `⚔` | Dice pools, stances, abilities, status effects |
| **Exploration** | `🧭` | Rooms, hazards, rest, travel |
| **Progression** | `📈` | Legend, Milestones, Progression Points |
| **Equipment** | `🛡` | Qualities, crafting, consumables |
| **Factions** | `🏴` | Reputation, standing, rewards |
| **The Blight** | `☣` | Corruption effects, Blighted enemies |

### 6.5 Discovery Triggers

Field Guide entries unlock **contextually** through gameplay:

| Entry | Trigger |
|-------|---------|
| Psychic Stress | First time stress exceeds 10 |
| Runic Blight Corruption | First Corruption gain |
| Dice Pool Mechanics | First combat encounter |
| Equipment Quality | First equipment pickup |
| Faction Reputation | First faction interaction |
| Sanctuary Mechanics | First rest at a Sanctuary |
| Breaking Points | First Trauma check |

---

## 7. TUI: Data Captures Section

### 7.1 Unassigned Captures Inventory

Captures that haven't been assigned to entries:

```
┌─────────────────────────────────────────────────────────────────────┐
│  DATA CAPTURES                                    8 unassigned      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  UNASSIGNED FRAGMENTS                                               │
│  ═════════════════════                                              │
│                                                                     │
│  📄 "...the forge-priests whispered of a heart..."                  │
│     Source: Crumbling inscription, Sector 4                         │
│     Possible matches: The Rust Lord, The Iron Heart Protocol        │
│                                                                     │
│  🔊 [Corrupted audio — static and fragments]                        │
│     Source: Echo Recording, Collapsed Tunnel                        │
│     Possible matches: Unknown (requires analysis)                   │
│                                                                     │
│  📸 [Partial schematic — gear mechanism]                            │
│     Source: Torn blueprint, Engineering Bay                         │
│     Possible matches: Dvergr Engineering Principles                 │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [↑↓] Navigate  [A] Assign to Entry  [D] Discard  [C] Close         │
└─────────────────────────────────────────────────────────────────────┘
```

### 7.2 Manual Assignment Flow

```
╔═══════════════════════════════════════════════════════════════════╗
║  ASSIGN DATA CAPTURE                                              ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  FRAGMENT:                                                        ║
║  📄 "...the forge-priests whispered of a heart..."                ║
║                                                                   ║
║  ─────────────────────────────────────────────────────────────    ║
║                                                                   ║
║  SUGGESTED MATCHES:                                               ║
║                                                                   ║
║  [1] 📜 The Rust Lord (85% match)                                 ║
║      Currently: 3/5 fragments                                     ║
║      → Would become: 4/5 fragments                                ║
║                                                                   ║
║  [2] 📜 The Iron Heart Protocol (72% match)                       ║
║      Currently: 1/6 fragments                                     ║
║      → Would become: 2/6 fragments                                ║
║                                                                   ║
║  [3] Create New Entry                                             ║
║      Start a new Codex entry with this fragment                   ║
║                                                                   ║
║  [0] Cancel                                                       ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  > _                                                              ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## 8. Glitch System

### 8.1 Stress-Based Visual Corruption

The journal's appearance degrades based on the character's **Psychic Stress** level:

| Stress Range | Tier | Visual Effects |
|--------------|------|----------------|
| 0-24% | **Stable** | Clean text, minor wear |
| 25-49% | **Unstable** | Occasional character swaps |
| 50-74% | **Degraded** | Fragmented sentences, flickering |
| 75-89% | **Critical** | Heavy corruption, intrusive messages |
| 90-100% | **Compromised** | Nearly unreadable |

### 8.2 Glitch Effect Types

| Effect | Threshold | Implementation |
|--------|-----------|----------------|
| **Character Substitution** | 25% | `e→3`, `a→4`, `s→5`, `o→0` |
| **Static Injection** | 40% | Random `%$#@!*&` characters |
| **Word Fragmentation** | 50% | Words split with `...` or `---` |
| **Bracket Noise** | 60% | Random `[ERROR]` `[???]` insertions |
| **Flicker Effect** | 70% | Text appears/disappears |
| **Data-Log Intrusion** | 75% | Flash of corrupted system messages |
| **Complete Corruption** | 90% | Words replaced with `▓▓▓▓` |

### 8.3 TUI Glitch Examples

**Stable (0-24% Stress):**
```
The Rusted Servitor stands motionless, its joints creaking with age.
```

**Unstable (25-49% Stress):**
```
Th3 Rust3d S3rv1t0r st4nds m0t10nl3ss, its j01nts cr34king with 4g3.
```

**Degraded (50-74% Stress):**
```
The Rust%d S#rv!tor st@nds... motion...less, its j$ints cre%king w#th... age.
```

**Critical (75-89% Stress):**
```
The ▓▓▓▓▓ Serv░░░r s█ands [SYSTEM ERROR: MEMORY CORRUPTED]... ░░░king with ▓▓▓.
```

**Compromised (90-100% Stress):**
```
▓▓▓ ▓▓▓▓▓▓ ▓▓▓▓▓▓▓▓ [THE SILENCE WATCHES] ▓▓▓▓▓ ▓▓▓▓▓▓▓▓▓▓ ▓▓▓ ▓▓▓▓▓▓...
```

### 8.4 Corruption-Based Margin Notes

As **Runic Blight Corruption** increases, the journal develops autonomous annotations:

| Corruption % | Effect |
|--------------|--------|
| 0-24% | No margin notes |
| 25-49% | Occasional scribbled symbols |
| 50-74% | Coherent but unsettling notes |
| 75-100% | Entries "you don't remember writing" |

**Example Margin Note Display:**

```
╔═══════════════════════════════════════════════════════════════════╗
║  📜 THE RUST LORD                                                  ║
╟───────────────────────────────────────────────────────────────────╢
║                                                       ╭──────────╮║
║  [Fragment 001]                                       │ They     │║
║  "The Rust Lord held dominion over the Iron Crypts,   │ remember.│║
║  his iron heart beating in rhythm with the forge's    │ They all │║
║  forgotten fires..."                                  │ remember.│║
║                                                       ╰──────────╯║
```

---

## 9. GUI Layout

### 9.1 Main Journal Panel

```
┌───────────────────────────────────────────────────────────────────────┐
│  THE SCAVENGER'S JOURNAL                                       [X]    │
├───────────────────────────────────────────────────────────────────────┤
│  [Codex] [Bestiary] [Field Guide] [Contracts] [Contacts] [Captures]   │
├─────────────────────────────┬─────────────────────────────────────────┤
│  ENTRY LIST                 │  ENTRY DETAILS                          │
│  ─────────────────────────  │  ───────────────────────────────────── │
│  Filter: [All ▼]            │                                        │
│  Sort:   [Progress ▼]       │  📜 THE RUST LORD                       │
│  🔍 [Search...]             │  BlightOrigin | The Silence             │
│  ─────────────────────────  │                                        │
│                             │  Progress: ████████████░░░ 4/5         │
│  📜 The Rust Lord           │                                        │
│     ████████████░░░ 4/5     │  FRAGMENTS:                            │
│                             │  ────────────────────────────────────  │
│  📜 The All-Rune Paradox    │  ☑ Fragment 001 (Data-slate)           │
│     ████████████████ 5/5 ✓  │  ☑ Fragment 002 (Echo Recording)       │
│                             │  ☑ Fragment 003 (Oral History)         │
│  📜 Dvergr Engineering      │  ☐ Fragment 004 (Missing)              │
│     ████░░░░░░░░░░░ 2/7     │  ☐ Fragment 005 (Missing)              │
│                             │                                        │
│  📜 The Iron Heart          │  ASSEMBLED TEXT:                       │
│     ██░░░░░░░░░░░░░ 1/6     │  ────────────────────────────────────  │
│                             │  "The Rust Lord held dominion over     │
│                             │  the Iron Crypts, his iron heart       │
│                             │  beating in rhythm with..."            │
│                             │                                        │
│                             │  [More content with redactions...]     │
│                             │                                        │
│                             │  [Track Entry] [Mark Read]             │
├─────────────────────────────┴─────────────────────────────────────────┤
│  Stress: ████████░░░░░░ 42%  │  Entries: 47/156  │  Legend: +2,450    │
└───────────────────────────────────────────────────────────────────────┘
```

### 9.2 GUI Components

| Component | Description |
|-----------|-------------|
| **Tab Bar** | Section navigation |
| **Entry List** | Scrollable, filterable, with progress indicators |
| **Filter/Sort** | Dropdowns for filtering and sorting |
| **Search** | Text search across all entries |
| **Detail Panel** | Full entry information with fragments |
| **Progress Bar** | Fragment collection progress |
| **Status Bar** | Stress level, entry counts, Legend earned |

### 9.3 Fragment Detail Popup

```
┌─────────────────────────────────────────────────────┐
│  FRAGMENT DETAILS                              [X]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  📄 TEXT FRAGMENT                                   │
│  "The Rust Lord's Domain"                           │
│                                                     │
│  ─────────────────────────────────────────────────  │
│  "...and the Rust Lord held dominion over the Iron  │
│  Crypts, his iron heart beating in rhythm with the  │
│  forge's forgotten fires. None who entered his      │
│  domain returned unchanged..."                      │
│  ─────────────────────────────────────────────────  │
│                                                     │
│  Source: Data-slate in Ironhold Ruins               │
│  Discovered: 2 hours ago                            │
│  Assigned to: The Rust Lord (Fragment 1/5)          │
│                                                     │
│  [Reassign]  [Close]                                │
└─────────────────────────────────────────────────────┘
```

---

## 10. Keyword System

### 10.1 Keyword Unlocks

Completing Codex entries (or reaching certain fragment thresholds) unlocks **dialogue keywords**:

| Entry | Fragment Threshold | Keyword | Effect |
|-------|-------------------|---------|--------|
| The Rust Lord | 3/5 | "Rust Lord" | New dialogue with Ironhold NPCs |
| The All-Rune Paradox | 5/5 | "All-Rune" | New dialogue with scholars |
| The Iron Heart Protocol | 4/6 | "Iron Heart" | Access to Dvergr histories |
| God-Sleeper Cult | 3/4 | "God-Sleeper" | Identify cult members |

### 10.2 Keyword Display in Dialogue

See [dialogue-ui.md](dialogue-ui.md) for integration details.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ELDER SIGRÚN                                        [Neutral]      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  "You seek knowledge of the old times? Few ask such questions       │
│  anymore. What would you know?"                                     │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│  [1] "Tell me about the Dvergr forges."                             │
│  [2] "What do you know of the Great Silence?"                       │
│  [3] 📜 "I've heard whispers of the All-Rune..." [KEYWORD]          │
│  [4] 📜 "The Rust Lord — is that name known to you?" [KEYWORD]      │
│  [0] "I should go."                                                 │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  > _                                                                │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 11. Examination System

### 11.1 In-World Examination

The `examine` command interfaces with the Blight's anti-logic, costing **Psychic Stress**:

| Examination Level | WITS DC | Stress Cost | Information Gained |
|-------------------|---------|-------------|-------------------|
| **Cursory** | — | 0 | Basic stub entry |
| **Basic** | 10 | 5 | Partial entry (combat stats) |
| **Detailed** | 14 | 10 | Full entry (vulnerabilities) |
| **Expert** | 18 | 15 | Complete entry (all data) |

### 11.2 Specialist Analysis

Certain specializations can perform enhanced examinations:

| Specialization | Ability | Stress Cost | Benefit |
|----------------|---------|-------------|---------|
| **Jötun-Reader** | Analyze Corrupted Code | 15 | Full mechanical entries for Mechanical creatures |
| **Bone-Setter** | Anatomical Analysis | 12 | Biological vulnerabilities for organic creatures |
| **Skald** | Saga Recollection | 10 | Historical/lore connections, keyword unlocks |
| **Vard-Warden** | Blight Reading | 15 | Corruption mechanics, Blight-related entries |

### 11.3 Examination Flow (TUI)

```
> examine rust-horror

  You focus on the Rust-Horror, attempting to parse its corrupted data...

  ═══════════════════════════════════════════════════════════════════
  EXAMINATION: Rust-Horror
  ─────────────────────────────────────────────────────────────────
  Examination Level: Basic (WITS DC 10)
  Stress Cost: 5 Psychic Stress

  Current Stress: 37/100 → 42/100

  Roll WITS (5d10) vs DC 10...
  ═══════════════════════════════════════════════════════════════════

  Rolling: [8] [3] [10] [6] [2] = 2 successes

  ✓ EXAMINATION SUCCESSFUL

  ─────────────────────────────────────────────────────────────────
  INFORMATION GAINED:
  • HP: ~60
  • Soak: 2
  • Speed: 6
  • Resistances: Physical (Low)

  Journal Updated: Rust-Horror → Partial Entry
  +25 Legend
  ─────────────────────────────────────────────────────────────────

  [Press any key to continue]
```

---

## 12. Saga Integration

### 12.1 Legend Rewards

| Discovery Type | Legend Reward |
|----------------|---------------|
| New creature stub | +10 Legend |
| Partial creature entry | +25 Legend |
| Complete creature entry | +50 Legend |
| Mastered creature entry | +100 Legend |
| Codex fragment collected | +15 Legend |
| Codex entry complete | +75 Legend |
| Field Guide discovery | +20 Legend |

### 12.2 Saga Feats

Major discoveries unlock permanent character bonuses:

| Achievement | Feat Name | Effect |
|-------------|-----------|--------|
| Complete all Jötun-Forged entries | "System Administrator" | +2 to abilities vs Mechanical |
| Complete all corrupted data-logs | "Mind of Steel" | +10 max Psychic Stress |
| Master all Undying entries | "Death's Chronicler" | +1 damage vs Undying |
| Complete all Blight Origin codex | "Blight Scholar" | 25% Corruption resistance |
| Discover all Field Guide entries | "Veteran Survivor" | +5% all Legend gains |

### 12.3 Journal Statistics

```
┌─────────────────────────────────────────────────────────────────────┐
│  JOURNAL STATISTICS                                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  CODEX                                                              │
│  Entries: 47/156 discovered                                         │
│  Complete: 12 entries                                               │
│  Fragments: 234/780 collected                                       │
│                                                                     │
│  BESTIARY                                                           │
│  Creatures: 23/89 encountered                                       │
│  Mastered: 5 entries                                                │
│                                                                     │
│  FIELD GUIDE                                                        │
│  Topics: 12/34 discovered                                           │
│                                                                     │
│  PROGRESS                                                           │
│  Legend from Journal: +2,450                                        │
│  Saga Feats Unlocked: 1/5                                           │
│  Keywords Unlocked: 8                                               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 13. Keyboard Shortcuts

### 13.1 Global Shortcuts

| Key | Command | Description |
|-----|---------|-------------|
| `J` | `ToggleJournalCommand` | Open/close journal |
| `B` | `OpenBestiaryCommand` | Open journal to Bestiary |

### 13.2 Journal Navigation

| Key | Command | Description |
|-----|---------|-------------|
| `1` | `SelectCodexCommand` | Switch to Codex |
| `2` | `SelectBestiaryCommand` | Switch to Bestiary |
| `3` | `SelectFieldGuideCommand` | Switch to Field Guide |
| `4` | `SelectContractsCommand` | Switch to Contracts (Quest Journal) |
| `5` | `SelectMoreCommand` | Open More menu |
| `Tab` | `NextSectionCommand` | Cycle sections |
| `Shift+Tab` | `PreviousSectionCommand` | Reverse cycle |
| `↑/↓` | `NavigateEntriesCommand` | Navigate entry list |
| `Enter` | `SelectEntryCommand` | View selected entry |
| `E` | `ExamineEntryCommand` | Examine (if applicable) |
| `T` | `TrackEntryCommand` | Track entry |
| `/` | `FocusSearchCommand` | Focus search box |
| `Escape` | `CloseCommand` | Close journal |

### 13.3 Entry-Specific Shortcuts

| Key | Command | Context | Description |
|-----|---------|---------|-------------|
| `R` | `MarkReadCommand` | Any new entry | Mark as read |
| `F` | `FilterCommand` | Any section | Open filter menu |
| `S` | `SortCommand` | Any section | Open sort menu |
| `A` | `AssignCaptureCommand` | Data Captures | Assign to entry |

---

## 14. ViewModels

### 14.1 ScavengersJournalViewModel

```csharp
public interface IScavengersJournalViewModel
{
    // Section Management
    JournalSection SelectedSection { get; set; }
    IReadOnlyList<JournalEntryViewModel> FilteredEntries { get; }
    JournalEntryViewModel? SelectedEntry { get; set; }

    // Search & Filter
    string SearchQuery { get; set; }
    EntryTypeFilter FilterType { get; set; }
    EntrySortOption SortOption { get; set; }

    // Player State
    int PsychicStress { get; }
    int Corruption { get; }
    float PsychicStressPercent { get; }
    float CorruptionPercent { get; }
    GlitchTier CurrentGlitchTier { get; }

    // Statistics
    int TotalEntries { get; }
    int DiscoveredEntries { get; }
    string DiscoveryStats { get; }
    int LegendFromJournal { get; }
    IReadOnlyList<SagaFeat> UnlockedFeats { get; }

    // Data Captures
    IReadOnlyList<DataCaptureViewModel> UnassignedCaptures { get; }
    int UnassignedCaptureCount { get; }

    // Commands
    ICommand CloseCommand { get; }
    ICommand SelectSectionCommand { get; }
    ICommand SelectEntryCommand { get; }
    ICommand ExamineEntryCommand { get; }
    ICommand TrackEntryCommand { get; }
    ICommand MarkReadCommand { get; }
    ICommand AssignCaptureCommand { get; }
}

public enum JournalSection { Codex, Bestiary, FieldGuide, Contracts, Contacts, DataCaptures }
public enum GlitchTier { Stable, Unstable, Degraded, Critical, Compromised }
```

### 14.2 CodexEntryViewModel

```csharp
public record CodexEntryViewModel(
    string EntryId,
    string Title,
    CodexCategory Category,
    HistoricalEra Era,
    int TotalFragments,
    int DiscoveredFragments,
    float CompletionPercent,
    bool IsComplete,
    bool IsNew,
    string AssembledText,
    IReadOnlyList<FragmentViewModel> Fragments,
    IReadOnlyList<string> UnlockedKeywords,
    IReadOnlyList<string> RelatedEntries,
    string? Hint
);

public record FragmentViewModel(
    string FragmentId,
    CaptureType Type,
    string Title,
    string Content,
    string Source,
    bool IsDiscovered,
    DateTimeOffset? DiscoveredAt
);

public enum CodexCategory
{
    BlightOrigin, PreBlightSociety, HistoricalEvent,
    TechnicalKnowledge, CulturalArtifact, ReligiousText,
    EvacuationRecord, FactionHistory
}
```

### 14.3 BestiaryEntryViewModel

```csharp
public record BestiaryEntryViewModel(
    string EntryId,
    string CreatureName,
    CreatureClassification Classification,
    CreatureRarity Rarity,
    EntryDiscoveryLevel DiscoveryLevel,
    bool IsNew,
    string Description,
    CombatDataViewModel? CombatData,
    IReadOnlyList<CreatureAbilityViewModel> Abilities,
    string? TacticalNotes,
    string? TacticalNoteSource,
    IReadOnlyList<FragmentViewModel> LoreFragments
);

public record CombatDataViewModel(
    int? HP,
    int? Soak,
    int? Speed,
    IReadOnlyList<ResistanceViewModel> Resistances,
    IReadOnlyList<VulnerabilityViewModel> Vulnerabilities,
    IReadOnlyList<ImmunityViewModel> Immunities
);

public enum EntryDiscoveryLevel { Unknown, Stub, Partial, Complete, Mastered }
public enum CreatureClassification { Mechanical, Undying, Blighted, Beast, Humanoid, Metaphysical, Boss }
```

### 14.4 DataCaptureViewModel

```csharp
public record DataCaptureViewModel(
    string CaptureId,
    CaptureType Type,
    string Title,
    string Content,
    string Source,
    DateTimeOffset DiscoveredAt,
    bool IsAssigned,
    string? AssignedEntryId,
    IReadOnlyList<CaptureMatchViewModel> PossibleMatches
);

public record CaptureMatchViewModel(
    string EntryId,
    string EntryTitle,
    float MatchConfidence,
    int CurrentFragments,
    int TotalFragments
);

public enum CaptureType { TextFragment, EchoRecording, VisualRecord, Specimen, OralHistory, RunicTrace }
```

---

## 15. Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `AutoAssignCaptures` | true | true/false |
| `ShowFragmentHints` | true | true/false |
| `GlitchEffectsEnabled` | true | true/false |
| `MarginNotesEnabled` | true | true/false |
| `ShowCompletionNotifications` | true | true/false |
| `DefaultCodexSort` | Progress | Progress/Title/Category/Recent |
| `DefaultBestiarySort` | Discovery | Discovery/Name/Type/Recent |

---

## 16. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Journal main screen | ❌ Planned | ❌ Planned |
| Section navigation | ❌ Planned | ❌ Planned |
| Codex section | ❌ Planned | ❌ Planned |
| Bestiary section | ❌ Planned | ❌ Planned |
| Field Guide section | ❌ Planned | ❌ Planned |
| Data Captures section | ❌ Planned | ❌ Planned |
| Fragment collection system | ❌ Planned | ❌ Planned |
| Fragment assignment | ❌ Planned | ❌ Planned |
| Entry assembly | ❌ Planned | ❌ Planned |
| Glitch effects | ❌ Planned | ❌ Planned |
| Margin notes | ❌ Planned | ❌ Planned |
| Keyword unlocks | ❌ Planned | ❌ Planned |
| Examination system | ❌ Planned | ❌ Planned |
| Legend rewards | ❌ Planned | ❌ Planned |
| ViewModels | ❌ Planned | ❌ Planned |

---

## 17. Phased Implementation Guide

### Phase 1: Core Framework
- [ ] Journal screen layout
- [ ] Section tab navigation
- [ ] Entry list component
- [ ] Entry detail panel
- [ ] Basic persistence

### Phase 2: Data Capture System
- [ ] Capture acquisition events
- [ ] Capture storage
- [ ] Auto-assignment logic
- [ ] Manual assignment UI
- [ ] Fragment progress tracking

### Phase 3: Codex Section
- [ ] CodexEntryViewModel
- [ ] Fragment assembly display
- [ ] Incomplete/complete states
- [ ] Keyword unlock system
- [ ] Related entries linking

### Phase 4: Bestiary Section
- [ ] BestiaryEntryViewModel
- [ ] Discovery levels
- [ ] Combat data display
- [ ] Examination integration

### Phase 5: Field Guide & Integration
- [ ] FieldGuideEntryViewModel
- [ ] Discovery triggers
- [ ] Quest Journal integration (Contracts tab)

### Phase 6: Glitch System
- [ ] GlitchService
- [ ] Stress-tier effects
- [ ] Text corruption algorithms
- [ ] Margin note generation

### Phase 7: Polish
- [ ] Search functionality
- [ ] Filter/sort controls
- [ ] Keyboard shortcuts
- [ ] Notifications
- [ ] Sound effects

---

## 18. Related Specifications

| Spec | Relationship |
|------|--------------|
| [quest-journal-ui.md](quest-journal-ui.md) | Contracts section (shared UI) |
| [dialogue-ui.md](dialogue-ui.md) | Keyword unlock integration |
| [tui-layout.md](tui-layout.md) | Screen composition |
| [stress.md](../01-core/resources/stress.md) | Glitch tier calculation |
| [corruption.md](../01-core/resources/corruption.md) | Margin note triggers |

---

## 19. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-15 | Initial specification — migrated from legacy with Data Capture puzzle system |
