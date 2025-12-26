# Scavenger's Journal GUI Specification

> Specification ID: SPEC-NARRATIVE-003
Template Version: 1.0
Last Updated: November 2024
Status: Draft
Target Framework: Avalonia UI 11.x with ReactiveUI
Architecture: MVVM Pattern with Controllers
Core Dependencies: RuneAndRust.Core.Journal, RuneAndRust.Engine.JournalService
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2024-11-27 | Claude | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [ ]  **Review**: Ready for stakeholder review
- [ ]  **Approved**: Approved for implementation
- [ ]  **Active**: Currently implemented and maintained

---

## Table of Contents

1. [Overview](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
2. [Core Philosophy](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
3. [Journal Main View](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
4. [Glitching Interface System](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
5. [Bestiary Section](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
6. [Codex Section](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
7. [Field Guide Section](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
8. [The Log Section](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
9. [Dynamic Entry System](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
10. [Saga Integration](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
11. [Keyboard Shortcuts](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
12. [Services & Controllers](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
13. [Data Models](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
14. [Implementation Roadmap](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)
15. [Appendices](Scavenger's%20Journal%20GUI%20Specification%202ba55eb312da806fa31ecf83dd2c4155.md)

---

## 1. Overview

### 1.1 Purpose

The Scavenger's Journal is a diegetic, in-world object that serves as the player's personal debug log for Aethelgard's corrupted reality. It is a comprehensive knowledge management system that grows in depth and utility as the player interfaces with the world's corrupted operating system.

### 1.2 Core System Summary

| Feature | Implementation |
| --- | --- |
| Journal Sections | Bestiary, Codex, Field Guide, The Log |
| Entry States | Stub, Partial, Complete, Corrupted |
| Discovery Types | Encounter, Examination, Specialist Insight |
| Knowledge Cost | Psychic Stress on examination |
| Visual Effects | Text corruption based on stress levels |
| Progression | Legend rewards, Saga Feats unlocks |

### 1.3 Design Philosophy

- **Diegetic Interface**: The journal exists as an in-world object, not a game menu
- **Knowledge as Currency**: Information is bought with sanity (Psychic Stress)
- **Dynamic Corruption**: UI reflects character's mental state through visual glitches
- **Rewarded Analysis**: Observation and investigation are core progression mechanics
- **Trauma Economy Integration**: Deep integration with Psychic Stress and Corruption systems

### 1.4 Visual Design

| Element | Specification |
| --- | --- |
| Background | Aged parchment texture (#2A2520) with darker panels (#1C1915) |
| Text Normal | Faded ink (#D4C4A8) with occasional handwritten annotations |
| Text Corrupted | Glitched characters, static overlays, color bleeding |
| Section Tabs | Leather bookmark aesthetic with embossed icons |
| Entry Cards | Worn paper style with quality-based border accents |

---

## 2. Core Philosophy

### 2.1 The Survivor's Debug Log

In the broken, glitching reality of Aethelgard, the Scavenger's Journal is not a simple diary—it is the player's personal debug log for a crashed reality. The journal is a chronicle of their saga, but also a vital tool for:

1. **Parsing Corrupted Data**: Understanding enemy weaknesses and behaviors
2. **Decoding System Errors**: Piecing together the mysteries of the Great Silence
3. **Imposing Coherence**: Making sense of the Runic Blight's anti-logic

### 2.2 Dual Purpose

| Purpose | Mechanism |
| --- | --- |
| **Teaching Through Trauma** | Primary tool for learning Runic Blight complexities; profound knowledge costs sanity |
| **Rewarding Analysis** | Observation and investigation become core progression mechanics |

### 2.3 Design Pillars

1. **Immersive Discovery**
    - **Rationale**: Knowledge feels earned, not given
    - **Examples**: Entries unlock through gameplay actions, not menus
2. **Visible Consequences**
    - **Rationale**: Player choices have tangible effects on the interface itself
    - **Examples**: High stress corrupts readable text; corruption adds margin notes
3. **Strategic Depth**
    - **Rationale**: Journal information provides tactical advantages
    - **Examples**: Bestiary entries reveal vulnerabilities for combat preparation

---

## 3. Journal Main View

**ViewModel:** `ScavengersJournalViewModel.cs`**Controller:** `JournalController`**View:** `ScavengersJournalView.axaml`

### 3.1 Layout Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│ ≡ THE SCAVENGER'S JOURNAL                           [?] [X] Close   │
│ ═══════════════════════════════════════════════════════════════════ │
├─────────────────────────────────────────────────────────────────────┤
│ [📖 Bestiary] [📜 Codex] [📋 Field Guide] [📝 The Log]              │
├───────────────────────────────┬─────────────────────────────────────┤
│                               │                                     │
│  ┌─────────────────────────┐  │  ENTRY DETAILS                      │
│  │ 🔍 Search...            │  │  ─────────────────────────────────  │
│  ├─────────────────────────┤  │                                     │
│  │ Filter: [All Types ▼]   │  │  [Entry Title]                      │
│  │ Sort: [Recently Added▼] │  │  ══════════════════════════════     │
│  ├─────────────────────────┤  │                                     │
│  │                         │  │  [Classification Badge]             │
│  │  ▸ Rusted Servitor ●●●  │  │  [Completion: ████████░░ 80%]       │
│  │    Mechanical | Jötun   │  │                                     │
│  │                         │  │  [Entry content with possible       │
│  │  ▸ Forlorn Wanderer ●●○ │  │   corruption effects based on       │
│  │    Undying | Corrupted  │  │   current Psychic Stress level]     │
│  │                         │  │                                     │
│  │  ▸ Rust-Horror ●○○      │  │  ─────────────────────────────────  │
│  │    Blighted | Unknown   │  │  TACTICAL NOTES                     │
│  │                         │  │  [If unlocked via specialist]       │
│  │  ▸ [CORRUPTED] ░░░      │  │                                     │
│  │    ▓▓▓ | ▓▓▓            │  │  ─────────────────────────────────  │
│  │                         │  │  RELATED ENTRIES                    │
│  │                         │  │  • Codex: Jötun Engineering         │
│  │                         │  │  • Field Guide: Mechanical Enemies  │
│  └─────────────────────────┘  │                                     │
│                               │  [Examine] [Track] [Mark Read]      │
├───────────────────────────────┴─────────────────────────────────────┤
│ Stress: ████████░░░░░░░░░░░░ 42%  │  Corruption: ██░░░░░░░░ 12%      │
│ Entries: 47/156 discovered        │  Legend from Journal: +2,450    │
└─────────────────────────────────────────────────────────────────────┘

```

### 3.2 Display Elements

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Title | Label | - | "The Scavenger's Journal" |
| Section Tabs | TabControl | `SelectedSection` | Bestiary/Codex/Field Guide/The Log |
| Entry List | ListView | `FilteredEntries` | Entries for current section |
| Detail Panel | Panel | `SelectedEntry` | Full entry information |
| Stress Meter | ProgressBar | `PsychicStressPercent` | Current stress visualization |
| Corruption Meter | ProgressBar | `CorruptionPercent` | Current corruption level |
| Discovery Count | Label | `DiscoveryStats` | "X/Y discovered" |
| Legend Earned | Label | `LegendFromJournal` | Total Legend from discoveries |

### 3.3 Properties

| Property | Type | Description |
| --- | --- | --- |
| `SelectedSection` | JournalSection | Currently selected tab |
| `SelectedEntry` | JournalEntryViewModel? | Currently selected entry |
| `FilteredEntries` | ObservableCollection | Entries matching filters |
| `SearchQuery` | string | Text search input |
| `FilterType` | EntryTypeFilter | Type filter selection |
| `SortOption` | EntrySortOption | Sort order |
| `PsychicStress` | int | Current stress (0-100) |
| `Corruption` | int | Current corruption (0-100) |
| `GlitchIntensity` | float | Calculated glitch level (0.0-1.0) |
| `IsVisible` | bool | Journal visibility state |

### 3.4 Section Definitions

| Tab | Enum Value | Icon | Content |
| --- | --- | --- | --- |
| Bestiary | `JournalSection.Bestiary` | 📖 | Creature entries |
| Codex | `JournalSection.Codex` | 📜 | Lore and history |
| Field Guide | `JournalSection.FieldGuide` | 📋 | Mechanics glossary |
| The Log | `JournalSection.Log` | 📝 | Quest tracking |

### 3.5 Commands

| Button | Command | Parameter | Behavior | Enabled Condition |
| --- | --- | --- | --- | --- |
| **Close** | `CloseCommand` | - | Hide journal | Always |
| **Examine** | `ExamineEntryCommand` | Entry | Attempt deeper examination | Entry is examinable |
| **Track** | `TrackEntryCommand` | Entry | Pin to HUD | Entry is trackable |
| **Mark Read** | `MarkReadCommand` | Entry | Remove "new" indicator | Entry is new |
| **Help** | `ShowHelpCommand` | - | Display journal help | Always |

---

## 4. Glitching Interface System

### 4.1 Stress-Based Corruption Tiers

The journal's visual presentation degrades based on the character's Psychic Stress level:

| Stress Range | Tier | Visual Effects |
| --- | --- | --- |
| 0-24% | **Stable** | Clean text, minor wear |
| 25-49% | **Unstable** | Occasional character swaps, "leetspeak" substitutions |
| 50-74% | **Degraded** | Fragmented sentences, static characters, flickering |
| 75-89% | **Critical** | Heavy corruption, intrusive data-log snippets |
| 90-100% | **Compromised** | Nearly unreadable, constant interference |

### 4.2 Glitch Effect Types

| Effect | Stress Threshold | Implementation |
| --- | --- | --- |
| **Character Substitution** | 25% | `e→3`, `a→4`, `s→5`, `o→0` |
| **Static Injection** | 40% | Random `%$#@!*&` characters |
| **Word Fragmentation** | 50% | Words split with `...` or `---` |
| **Color Bleeding** | 60% | Text color shifts, RGB separation |
| **Flicker Effect** | 70% | Text opacity fluctuation |
| **Data-Log Intrusion** | 75% | Flash of corrupted system messages |
| **Complete Corruption** | 90% | Entire words replaced with `▓▓▓▓` |

### 4.3 Glitch Text Properties

| Property | Type | Description |
| --- | --- | --- |
| `GlitchIntensity` | float | 0.0-1.0 based on stress |
| `GlitchSeed` | int | Random seed for consistent corruption |
| `GlitchUpdateInterval` | TimeSpan | How often glitches refresh |
| `IntrustiveMessages` | List<string> | Pool of corrupted data-log snippets |

### 4.4 Corruption-Based Margin Notes

When character's Runic Blight Corruption increases, the journal develops autonomous notes:

| Corruption Range | Effect |
| --- | --- |
| 0-24% | No margin notes |
| 25-49% | Occasional scribbled symbols in margins |
| 50-74% | Coherent but unsettling notes appear |
| 75-100% | Full margin annotations the player "doesn't remember writing" |

**Margin Note Properties:**

| Property | Type | Description |
| --- | --- | --- |
| `HasMarginNotes` | bool | Whether corruption notes appear |
| `MarginNoteText` | string | The autonomous annotation |
| `MarginNoteStyle` | CorruptionStyle | Visual style of the note |

### 4.5 Glitch ViewModel

```csharp
public class GlitchTextViewModel
{
    public string OriginalText { get; set; }
    public string DisplayText { get; }  // Computed with glitch effects
    public float GlitchIntensity { get; set; }
    public bool IsGlitching { get; }
    public string GlitchClass { get; }  // CSS class for styling
}

```

---

## 5. Bestiary Section

### 5.1 Purpose

The Bestiary is a collection of entries on every hostile "process" the player has encountered—a guide to corrupted entities framed as analyzing system errors and malformed processes.

### 5.2 Bestiary Entry Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ RUSTED SERVITOR                                    [●●●] 100%   │
│ ─────────────────────────────────────────────────────────────── │
│ Classification: Mechanical | Jötun-Forged | Common              │
│ Corruption Level: ██░░░░░░░░ Minimal                            │
├─────────────────────────────────────────────────────────────────┤
│ DESCRIPTION                                                     │
│ A crudely humanoid automaton, its code seems to be a corrupted  │
│ version of a basic security or labor unit from the Old World.   │
│ Heavy plating protects vital systems, but rusted joints betray  │
│ structural weaknesses.                                          │
├─────────────────────────────────────────────────────────────────┤
│ COMBAT DATA                                                     │
│ ┌─────────────┬─────────────┬─────────────┐                     │
│ │ HP: 45      │ Soak: 3     │ Speed: 4    │                     │
│ └─────────────┴─────────────┴─────────────┘                     │
│                                                                 │
│ Resistances: Physical (High), Fire (Low)                        │
│ Vulnerabilities: Corrosion, Lightning                           │
│ Immunities: Poison, Psychic                                     │
├─────────────────────────────────────────────────────────────────┤
│ ABILITIES                                                       │
│ • Crushing Blow - Heavy melee attack (2d6+3 damage)             │
│ • Defensive Protocol - Gains +2 Soak for 2 turns                │
│ • [LOCKED - Requires Expert Examination]                        │
├─────────────────────────────────────────────────────────────────┤
│ TACTICAL NOTES (Jötun-Reader Insight)                           │
│ Analysis of the core logic reveals a critical flaw in its       │
│ power regulation subroutine. A powerful electrical surge has    │
│ a high probability of causing a catastrophic system reboot,     │
│ manifesting as the Stunned status for 2 turns.                  │
├─────────────────────────────────────────────────────────────────┤
│ LORE FRAGMENTS                                                  │
│ ◇ "The Servitors were the backbone of Dvergr industry..."       │
│ ◇ [Undiscovered - Examine in Jötunheim]                         │
└─────────────────────────────────────────────────────────────────┘

```

### 5.3 Bestiary Entry Properties

| Property | Type | Description |
| --- | --- | --- |
| `EntryId` | string | Unique identifier |
| `CreatureName` | string | Display name |
| `Classification` | CreatureClassification | Type categorization |
| `CorruptionLevel` | CorruptionTier | Blight corruption severity |
| `Description` | string | Narrative description |
| `DiscoveryLevel` | EntryDiscoveryLevel | Stub/Partial/Complete |
| `CompletionPercent` | float | 0.0-1.0 completion |
| `IsNew` | bool | Recently discovered |

### 5.4 Combat Data Properties

| Property | Type | Description |
| --- | --- | --- |
| `HP` | int? | Known HP (null if undiscovered) |
| `Soak` | int? | Known damage reduction |
| `Speed` | int? | Known initiative/speed |
| `Resistances` | List<DamageResistance> | Known resistances |
| `Vulnerabilities` | List<DamageVulnerability> | Known weaknesses |
| `Immunities` | List<DamageImmunity> | Known immunities |
| `Abilities` | List<CreatureAbilityEntry> | Known abilities |
| `TacticalNotes` | string? | Specialist insight text |
| `TacticalNoteSource` | Specialization? | Which spec provided insight |

### 5.5 Creature Classification

| Category | Examples | Icon |
| --- | --- | --- |
| Mechanical | Servitor, Automaton, Construct | ⚙ |
| Undying | Forlorn, Revenant, Wraith | 💀 |
| Blighted | Rust-Horror, Blight-Spawn | ☣ |
| Beast | Corrupted Wolf, Cave Lurker | 🐺 |
| Humanoid | Raider, Cultist, Scavenger | 👤 |
| Metaphysical | Void-Touched, Aether Entity | ✧ |
| Boss | Sector Guardian, Named Enemy | ★ |

### 5.6 Discovery Levels

| Level | Icon | Content Visible |
| --- | --- | --- |
| **Stub** | ●○○ | Name, classification, basic description |
| **Partial** | ●●○ | + Combat stats, some resistances/vulnerabilities |
| **Complete** | ●●● | + All abilities, all resistances, lore fragments |
| **Mastered** | ★★★ | + Tactical notes, hidden abilities, full lore |

### 5.7 Bestiary Commands

| Command | Behavior | Cost |
| --- | --- | --- |
| `ExamineCreatureCommand` | Attempt WITS check for more info | 5-15 Psychic Stress |
| `TrackCreatureCommand` | Show weakness reminder in combat | None |
| `FilterByTypeCommand` | Filter list by classification | None |

---

## 6. Codex Section

### 6.1 Purpose

The Codex collects all narrative and historical lore, framed as salvaged data-logs from the crashed system of reality. It allows players to piece together the mysteries of the Great Silence.

### 6.2 Codex Entry Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ THE ALL-RUNE PARADOX                               [Complete]   │
│ ─────────────────────────────────────────────────────────────── │
│ Category: BlightOrigin | Era: The Silence | Sources: 3/3       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ [Data-Log Fragment 001-A]                                       │
│ ════════════════════════                                        │
│ "The All-Rune was meant to unify the disparate magical          │
│ traditions of the Nine Realms. Instead, when the Dvergr         │
│ artificers completed the final inscription, reality itself      │
│ rejected the paradox of infinite meaning..."                    │
│                                                                 │
│ [Data-Log Fragment 001-B]                                       │
│ ════════════════════════                                        │
│ "The Blight is not corruption—it is reality's immune response   │
│ to a logical impossibility that was carved into existence..."   │
│                                                                 │
│ [Data-Log Fragment 001-C]                                       │
│ ════════════════════════                                        │
│ "Some whisper that the All-Rune still exists, buried deep       │
│ beneath Jötunheim. They say it pulses with anti-meaning,        │
│ rewriting any who approach..."                                  │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ KEYWORDS UNLOCKED                                               │
│ • "All-Rune" - Can be used in dialogue with scholars            │
│ • "The Paradox" - Opens new dialogue with Vard-Wardens          │
├─────────────────────────────────────────────────────────────────┤
│ RELATED ENTRIES                                                 │
│ • Codex: The Iron Heart Protocol                                │
│ • Codex: The God-Sleeper Cult                                   │
│ • Bestiary: Void-Touched (mentions All-Rune exposure)           │
└─────────────────────────────────────────────────────────────────┘

```

### 6.3 Codex Categories

| Category | Icon | Content |
| --- | --- | --- |
| BlightOrigin | ☣ | Origins and nature of the Runic Blight |
| PreBlightSociety | 🏛 | Life before the Great Silence |
| HistoricalEvent | 📅 | Key events in Aethelgard's history |
| TechnicalKnowledge | ⚙ | Jötun engineering, rune crafting |
| CulturalArtifact | 🏺 | Art, customs, traditions |
| ReligiousText | ✝ | Beliefs, cults, divine entities |
| EvacuationRecord | 🚪 | Records of the exodus |
| FactionHistory | 🏴 | Faction origins and conflicts |

### 6.4 Codex Entry Properties

| Property | Type | Description |
| --- | --- | --- |
| `EntryId` | string | Unique identifier |
| `Title` | string | Lore entry title |
| `Category` | CodexCategory | Topic categorization |
| `Era` | HistoricalEra | When this occurred |
| `Fragments` | List<LoreFragment> | Collected data-log pieces |
| `TotalFragments` | int | Total fragments to collect |
| `DiscoveredFragments` | int | Currently discovered |
| `Keywords` | List<DialogueKeyword> | Unlocked dialogue options |
| `RelatedEntries` | List<string> | Cross-references |
| `DiscoveryLocations` | List<string> | Where fragments are found |

### 6.5 Lore Fragment Properties

| Property | Type | Description |
| --- | --- | --- |
| `FragmentId` | string | Fragment identifier |
| `FragmentText` | string | The lore content |
| `Source` | string | Where it was found |
| `IsDiscovered` | bool | Whether player has found it |
| `DiscoveryDate` | DateTime? | When discovered |
| `RequiredExamination` | ExaminationLevel | Detail level needed |

### 6.6 Dialogue Keyword Integration

Completing certain Codex entries unlocks dialogue keywords:

| Codex Entry | Keyword | Effect |
| --- | --- | --- |
| The All-Rune Paradox | "All-Rune" | New dialogue with scholars |
| Iron Heart Protocol | "Iron Heart" | Access to Dvergr histories |
| God-Sleeper Cult | "God-Sleeper" | Identify cult members |

---

## 7. Field Guide Section

### 7.1 Purpose

The Field Guide acts as an in-world glossary for the rules of corrupted reality—in-character explanations of core game mechanics as they are discovered.

### 7.2 Field Guide Entry Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ PSYCHIC STRESS: THE WEIGHT OF KNOWING                          │
│ ─────────────────────────────────────────────────────────────── │
│ Category: Trauma Economy | Status: Discovered                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ SURVIVOR'S NOTES                                                │
│ ════════════════                                                │
│ "The mind was not meant to comprehend the Blight's anti-logic.  │
│ Every time I peer too deeply into corrupted systems, I feel     │
│ something crack inside me. The old-timers call it 'Stress'—     │
│ the weight of knowing too much about a reality that shouldn't   │
│ exist."                                                         │
│                                                                 │
│ MECHANICAL UNDERSTANDING                                        │
│ ════════════════════════                                        │
│ • Psychic Stress ranges from 0-100                              │
│ • Examining Blighted entities costs 5-15 Stress                 │
│ • At 25/50/75 thresholds, negative effects trigger              │
│ • Reaching 100 causes a Breaking Point                          │
│ • Rest at Sanctuary rooms to reduce Stress                      │
│                                                                 │
│ THRESHOLD EFFECTS                                               │
│ ┌────────────┬──────────────────────────────────────────────┐   │
│ │ 25% Low    │ Minor penalties to WITS checks               │   │
│ │ 50% Mod    │ Random intrusive thoughts, -1 to all rolls   │   │
│ │ 75% High   │ Disadvantage on mental saves                 │   │
│ │ 100% Break │ Permanent Trauma acquired                    │   │
│ └────────────┴──────────────────────────────────────────────┘   │
│                                                                 │
│ RELATED ENTRIES                                                 │
│ • Runic Blight Corruption                                       │
│ • Breaking Points & Traumas                                     │
│ • Sanctuary Mechanics                                           │
└─────────────────────────────────────────────────────────────────┘

```

### 7.3 Field Guide Categories

| Category | Icon | Topics |
| --- | --- | --- |
| Trauma Economy | 🧠 | Stress, Corruption, Traumas |
| Combat Systems | ⚔ | Dice pools, stances, abilities |
| Exploration | 🧭 | Rooms, hazards, rest |
| Progression | 📈 | Legend, Milestones, PP |
| Equipment | 🛡 | Qualities, crafting, consumables |
| Factions | 🏴 | Reputation, standing, rewards |
| The Blight | ☣ | Corruption effects, Blighted enemies |

### 7.4 Field Guide Entry Properties

| Property | Type | Description |
| --- | --- | --- |
| `EntryId` | string | Unique identifier |
| `Title` | string | Topic title |
| `Category` | FieldGuideCategory | Categorization |
| `FlavorText` | string | In-world narrative explanation |
| `MechanicalText` | string | Gameplay mechanics explanation |
| `DataTables` | List<DataTable> | Visual data displays |
| `RelatedEntries` | List<string> | Cross-references |
| `IsDiscovered` | bool | Player has encountered this |
| `DiscoveryTrigger` | string | What unlocks this entry |

### 7.5 Discovery Triggers

Field Guide entries unlock contextually:

| Entry | Trigger |
| --- | --- |
| Psychic Stress | First time stress exceeds 10 |
| Runic Blight Corruption | First Corruption gain |
| Dice Pool Mechanics | First combat encounter |
| Equipment Quality | First equipment pickup |
| Faction Reputation | First faction interaction |

---

## 8. The Log Section

### 8.1 Purpose

The Log tracks the player's active and completed quests—their personal chronicle of the survivor's saga. This section integrates with the Quest Journal system.

### 8.2 Integration with Quest Journal

The Log section shares data with `QuestJournalViewModel` but presents it through the journal's aesthetic:

| Quest Journal Feature | Log Equivalent |
| --- | --- |
| Active Quests Tab | "Current Pursuits" |
| Completed Quests Tab | "Saga Chapters" |
| Failed Quests Tab | "Abandoned Paths" |
| Quest Details | "Chronicle Entry" |

### 8.3 Log Entry Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ 📜 CURRENT PURSUITS                                             │
│ ─────────────────────────────────────────────────────────────── │
│                                                                 │
│ ★ THE IRON PATH [Main Quest]                                    │
│   "The ancient forge calls. The Rust-Lord must fall."           │
│   ────────────────────────────────────────────────              │
│   ☑ Find the ancient forge                                      │
│   ☐ Defeat the Rust-Lord (2/3 lieutenants)                      │
│   ☐ Return to Kjartan                                           │
│                                                                 │
│ ○ LOST TOOLS [Side Quest]                                       │
│   "A merchant needs salvage from the old workshops."            │
│   ────────────────────────────────────────────────              │
│   ☐ Collect Dvergr Hammers (1/5)                                │
│                                                                 │
│ ◇ SECTOR PATROL [Dynamic]                                       │
│   "Strange activity in Sector 4 requires investigation."        │
│   ────────────────────────────────────────────────              │
│   ☐ Clear hostiles from Sector 4                                │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ 📖 SAGA CHAPTERS (Completed)                                    │
│ ─────────────────────────────────────────────────────────────── │
│ ✓ The First Steps (Legend: +100)                                │
│ ✓ Voices in the Dark (Legend: +250, Keyword: "Whispers")        │
│ ✓ Blood and Rust (Legend: +400)                                 │
└─────────────────────────────────────────────────────────────────┘

```

### 8.4 Quest Display Properties

| Property | Type | Description |
| --- | --- | --- |
| `QuestId` | string | Quest identifier |
| `Title` | string | Quest name |
| `JournalEntry` | string | Narrative summary |
| `Type` | QuestType | Main/Side/Dynamic/Repeatable |
| `TypeIcon` | string | ★/○/◇/↻ |
| `Objectives` | List<ObjectiveViewModel> | Quest objectives |
| `LegendReward` | int | Legend earned on completion |
| `KeywordsUnlocked` | List<string> | Dialogue keywords earned |
| `SagaFeatUnlocked` | string? | Feat earned if any |

---

## 9. Dynamic Entry System

### 9.1 The Price of Knowledge

The journal is written by the player's actions. Knowledge is a resource bought with sanity.

### 9.2 Discovery Tiers

### Tier 1: First Encounter (Free)

The first time a player encounters a new creature, a basic "stub" entry is created at no cost.

| Trigger | Result | Cost |
| --- | --- | --- |
| Defeat enemy | Stub entry created | None |
| Enter new area | Location noted | None |
| Find lore object | Codex stub created | None |

**Example Stub Entry:**

> Rusted Servitor: A crudely humanoid automaton, its code seems to be a corrupted version of a basic security or labor unit from the Old World.
> 

### Tier 2: Active Investigation (Stress Cost)

Using the `examine` command on a corrupted entity interfaces directly with the Blight's anti-logic.

| Action | WITS DC | Success Result | Stress Cost |
| --- | --- | --- | --- |
| Examine (Basic) | 10 | Partial entry | 5 Stress |
| Examine (Detailed) | 14 | Combat stats revealed | 10 Stress |
| Examine (Expert) | 18 | Vulnerabilities revealed | 15 Stress |

**Example Updated Entry:**

> Rusted Servitor: ...Its heavy plating seems effective against direct blows, but the rusted joints look brittle, a flaw in its physical rendering. It emits a low electrical hum, a sign of its unstable power core.
> 
> 
> **[NEW]** Resistances: High Physical Soak
> **[NEW]** Vulnerabilities: ???
> 

### Tier 3: Specialist Insight (High Stress Cost)

Specialists with knowledge-based abilities can perform the ultimate "debug":

| Specialization | Ability | Stress Cost | Result |
| --- | --- | --- | --- |
| Jötun-Reader | Analyze Corrupted Code | 15 | Full mechanical entries |
| Bone-Setter | Anatomical Analysis | 12 | Biological vulnerabilities |
| Skald | Saga Recollection | 10 | Historical/lore connections |
| Vard-Warden | Blight Reading | 15 | Corruption mechanics |

**Example Specialist Entry:**

> TACTICAL NOTES (Jötun-Reader Insight):
Analysis of the core logic reveals a critical flaw in its power regulation subroutine. A powerful electrical surge has a high probability of causing a catastrophic system reboot, which would manifest as the Stunned status.
> 

### 9.3 Examination Commands

| Command | Parameter | Behavior |
| --- | --- | --- |
| `ExamineCommand` | Target | Basic examination (5 Stress) |
| `DetailedExamineCommand` | Target | Detailed examination (10 Stress) |
| `SpecialistAnalysisCommand` | Target | Use spec ability (varies) |

### 9.4 Examination Result Properties

| Property | Type | Description |
| --- | --- | --- |
| `Success` | bool | Whether check succeeded |
| `ExaminationLevel` | ExaminationLevel | Cursory/Detailed/Expert |
| `StressCost` | int | Stress incurred |
| `InformationGained` | List<string> | New data revealed |
| `EntryUpdated` | bool | Whether entry was updated |
| `LegendAwarded` | int | Legend for discovery |

---

## 10. Saga Integration

### 10.1 Legend Rewards

The journal is a direct path of progression:

| Discovery Type | Legend Reward |
| --- | --- |
| New creature stub | 10 Legend |
| Partial creature entry | 25 Legend |
| Complete creature entry | 50 Legend |
| Mastered creature entry | 100 Legend |
| Codex fragment | 15 Legend |
| Complete Codex entry | 75 Legend |
| Field Guide discovery | 20 Legend |

### 10.2 Saga Feats

Major discoveries unlock permanent character feats:

| Achievement | Feat Name | Effect |
| --- | --- | --- |
| Complete all Jötun-Forged entries | "System Administrator" | +2 to abilities vs Mechanical |
| Survive reading all corrupted data-logs | "Mind of Steel" | +10 max Psychic Stress |
| Master all Undying entries | "Death's Chronicler" | +1 damage vs Undying |
| Complete all Blight Origin codex | "Blight Scholar" | 25% Corruption resistance |
| Discover all Field Guide entries | "Veteran Survivor" | +5% all Legend gains |

### 10.3 Corruption-Altered Entries

As a character's Runic Blight Corruption increases, their journal entries may become subtly altered:

| Corruption Level | Effect |
| --- | --- |
| 25% | Occasional crossed-out words with corrections |
| 50% | Margin notes in different handwriting |
| 75% | Entries the player doesn't remember writing |
| 90% | The Blight "responds" to journal entries |

**Example Corrupted Entry:**

> Rusted Servitor: A crudely humanoid automaton...
> 
> 
> *[Margin note in scratchy handwriting:]"They remember. They all remember. The Old Code still runs in their bones."*
> 

### 10.4 Saga Integration Properties

| Property | Type | Description |
| --- | --- | --- |
| `TotalLegendFromJournal` | int | Cumulative Legend earned |
| `SagaFeatsUnlocked` | List<SagaFeat> | Feats earned from journal |
| `CorruptionNotes` | List<CorruptionNote> | Auto-generated margin notes |
| `JournalMilestones` | List<JournalMilestone> | Major discovery achievements |

---

## 11. Keyboard Shortcuts

### 11.1 Global Shortcuts

| Key | Command | Description |
| --- | --- | --- |
| J | `ToggleJournalCommand` | Open/close journal |
| B | `OpenBestiaryCommand` | Open journal to Bestiary |

### 11.2 Journal Navigation

| Key | Command | Description |
| --- | --- | --- |
| 1 | `SelectBestiaryCommand` | Switch to Bestiary |
| 2 | `SelectCodexCommand` | Switch to Codex |
| 3 | `SelectFieldGuideCommand` | Switch to Field Guide |
| 4 | `SelectLogCommand` | Switch to The Log |
| Tab | `NextSectionCommand` | Cycle sections |
| Shift+Tab | `PreviousSectionCommand` | Reverse cycle |
| ↑/↓ | `NavigateEntriesCommand` | Navigate entry list |
| Enter | `SelectEntryCommand` | View selected entry |
| E | `ExamineEntryCommand` | Examine (if applicable) |
| T | `TrackEntryCommand` | Track entry |
| / | `FocusSearchCommand` | Focus search box |
| Escape | `CloseCommand` | Close journal |

### 11.3 Entry-Specific Shortcuts

| Key | Command | Context | Description |
| --- | --- | --- | --- |
| R | `MarkReadCommand` | Any new entry | Mark as read |
| F | `FilterCommand` | Any section | Open filter menu |
| S | `SortCommand` | Any section | Open sort menu |

---

## 12. Services & Controllers

### 12.1 JournalController

**Responsibilities:**

| Method | Description |
| --- | --- |
| `OpenJournal(section?)` | Show journal, optionally to specific section |
| `CloseJournal()` | Hide journal |
| `SelectEntry(entryId)` | Select and display entry |
| `ExamineTarget(target)` | Perform examination with stress cost |
| `SpecialistAnalysis(target, spec)` | Use specialist ability |
| `AddStubEntry(entityType, data)` | Create new stub entry |
| `UpdateEntry(entryId, data)` | Update existing entry |
| `ApplyGlitchEffects(stress)` | Calculate and apply visual corruption |

### 12.2 Service Dependencies

| Service | Responsibility |
| --- | --- |
| `IJournalService` | Entry CRUD, discovery tracking |
| `IExaminationService` | Examination checks, stress costs |
| `IGlitchService` | Text corruption effects |
| `IPlayerService` | Stress/Corruption access |
| `IQuestService` | Quest data for The Log |
| `IProgressionService` | Legend awards, feat unlocks |
| `IDialogueService` | Keyword integration |
| `INavigationService` | View navigation |

### 12.3 Events

| Event | Trigger | Handler |
| --- | --- | --- |
| `EntryDiscovered` | New entry created | Update list, show notification |
| `EntryUpdated` | Entry data changed | Refresh detail view |
| `ExaminationComplete` | Examine finished | Apply results, update stress |
| `SagaFeatUnlocked` | Achievement earned | Show celebration, apply feat |
| `StressChanged` | Player stress modified | Update glitch intensity |
| `CorruptionChanged` | Player corruption modified | Update margin notes |

### 12.4 Notifications

| Notification | Display | Duration |
| --- | --- | --- |
| New Entry | Toast: "Journal Updated: [Name]" | 3 seconds |
| Entry Complete | Toast: "Entry Complete! +X Legend" | 3 seconds |
| Saga Feat Unlocked | Modal: Feat celebration | Until dismissed |
| High Stress Warning | Toast: "Your mind strains..." | 2 seconds |
| Margin Note Added | Subtle flash in journal icon | 1 second |

---

## 13. Data Models

### 13.1 Core Enums

```csharp
public enum JournalSection
{
    Bestiary,
    Codex,
    FieldGuide,
    Log
}

public enum EntryDiscoveryLevel
{
    Unknown,    // Not yet encountered
    Stub,       // Basic info only
    Partial,    // Some details
    Complete,   // All standard info
    Mastered    // Specialist insight added
}

public enum CreatureClassification
{
    Mechanical,
    Undying,
    Blighted,
    Beast,
    Humanoid,
    Metaphysical,
    Boss
}

public enum CodexCategory
{
    BlightOrigin,
    PreBlightSociety,
    HistoricalEvent,
    TechnicalKnowledge,
    CulturalArtifact,
    ReligiousText,
    EvacuationRecord,
    FactionHistory
}

public enum FieldGuideCategory
{
    TraumaEconomy,
    CombatSystems,
    Exploration,
    Progression,
    Equipment,
    Factions,
    TheBlight
}

public enum ExaminationLevel
{
    Cursory,    // Free, basic info
    Detailed,   // WITS DC 12, combat stats
    Expert,     // WITS DC 18, vulnerabilities
    Specialist  // Spec ability, tactical notes
}

public enum GlitchTier
{
    Stable,     // 0-24% stress
    Unstable,   // 25-49%
    Degraded,   // 50-74%
    Critical,   // 75-89%
    Compromised // 90-100%
}

```

### 13.2 Database Schema

```sql
-- Bestiary Entries
CREATE TABLE BestiaryEntries (
    EntryId TEXT PRIMARY KEY,
    CharacterId INTEGER NOT NULL,
    CreatureName TEXT NOT NULL,
    Classification TEXT NOT NULL,
    CorruptionLevel TEXT,
    Description TEXT,
    DiscoveryLevel TEXT DEFAULT 'Unknown',
    CompletionPercent REAL DEFAULT 0.0,
    DiscoveredAt DATETIME,
    HP INTEGER,
    Soak INTEGER,
    Speed INTEGER,
    ResistancesJson TEXT,
    VulnerabilitiesJson TEXT,
    ImmunitiesJson TEXT,
    AbilitiesJson TEXT,
    TacticalNotes TEXT,
    TacticalNoteSource TEXT,
    IsNew BOOLEAN DEFAULT 1,
    FOREIGN KEY (CharacterId) REFERENCES Characters(CharacterId)
);

-- Codex Entries
CREATE TABLE CodexEntries (
    EntryId TEXT PRIMARY KEY,
    CharacterId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Category TEXT NOT NULL,
    Era TEXT,
    TotalFragments INTEGER DEFAULT 1,
    DiscoveredFragments INTEGER DEFAULT 0,
    KeywordsJson TEXT,
    RelatedEntriesJson TEXT,
    IsComplete BOOLEAN DEFAULT 0,
    DiscoveredAt DATETIME,
    FOREIGN KEY (CharacterId) REFERENCES Characters(CharacterId)
);

-- Codex Fragments
CREATE TABLE CodexFragments (
    FragmentId TEXT PRIMARY KEY,
    EntryId TEXT NOT NULL,
    FragmentText TEXT NOT NULL,
    Source TEXT,
    IsDiscovered BOOLEAN DEFAULT 0,
    DiscoveredAt DATETIME,
    FOREIGN KEY (EntryId) REFERENCES CodexEntries(EntryId)
);

-- Field Guide Entries
CREATE TABLE FieldGuideEntries (
    EntryId TEXT PRIMARY KEY,
    CharacterId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Category TEXT NOT NULL,
    FlavorText TEXT,
    MechanicalText TEXT,
    DataTablesJson TEXT,
    RelatedEntriesJson TEXT,
    IsDiscovered BOOLEAN DEFAULT 0,
    DiscoveryTrigger TEXT,
    DiscoveredAt DATETIME,
    FOREIGN KEY (CharacterId) REFERENCES Characters(CharacterId)
);

-- Saga Feats
CREATE TABLE JournalSagaFeats (
    FeatId TEXT PRIMARY KEY,
    CharacterId INTEGER NOT NULL,
    FeatName TEXT NOT NULL,
    Description TEXT,
    Effect TEXT,
    UnlockedAt DATETIME,
    FOREIGN KEY (CharacterId) REFERENCES Characters(CharacterId)
);

-- Corruption Margin Notes
CREATE TABLE CorruptionMarginNotes (
    NoteId INTEGER PRIMARY KEY AUTOINCREMENT,
    CharacterId INTEGER NOT NULL,
    EntryId TEXT NOT NULL,
    NoteText TEXT NOT NULL,
    CorruptionLevelWhenWritten INTEGER,
    CreatedAt DATETIME,
    FOREIGN KEY (CharacterId) REFERENCES Characters(CharacterId)
);

```

---

## 14. Implementation Roadmap

### 14.1 Phase 1: Core Framework (Priority: Critical)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `ScavengersJournalViewModel` | Main ViewModel with section management | Medium |
| Create `JournalEntryViewModel` | Base entry display model | Low |
| Create `ScavengersJournalView.axaml` | Main journal layout | Medium |
| Implement section tab navigation | Bestiary/Codex/Field Guide/Log tabs | Low |
| Create entry list component | Scrollable, filterable entry list | Medium |
| Create detail panel component | Entry information display | Medium |
| Implement basic persistence | SQLite integration for entries | Medium |

### 14.2 Phase 2: Bestiary System (Priority: Critical)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `BestiaryEntryViewModel` | Creature entry model | Medium |
| Implement creature classification | Type icons and filtering | Low |
| Implement discovery levels | Stub/Partial/Complete/Mastered | Medium |
| Create combat data display | Stats, resistances, abilities | Medium |
| Implement stub creation on encounter | Auto-create entries | Medium |
| Create examination commands | WITS checks with stress cost | High |

### 14.3 Phase 3: Codex & Field Guide (Priority: High)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `CodexEntryViewModel` | Lore entry model | Medium |
| Implement fragment collection | Partial codex completion | Medium |
| Create `FieldGuideEntryViewModel` | Mechanics entry model | Low |
| Implement discovery triggers | Contextual entry unlocks | Medium |
| Add keyword integration | Dialogue keyword unlocks | Medium |
| Implement cross-references | Related entries linking | Low |

### 14.4 Phase 4: Glitch System (Priority: High)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `GlitchService` | Text corruption engine | High |
| Implement stress-tier effects | Visual degradation by tier | High |
| Create character substitution | Leetspeak conversion | Low |
| Implement static injection | Random corrupted characters | Low |
| Create flicker animation | Text opacity fluctuation | Medium |
| Implement data-log intrusions | Random corrupted messages | Medium |

### 14.5 Phase 5: Dynamic Entry System (Priority: High)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `ExaminationService` | Examination logic | High |
| Implement stress cost system | Stress deduction on examine | Medium |
| Create specialist abilities | Per-specialization analysis | High |
| Implement entry updates | Progressive information reveal | Medium |
| Add examination UI feedback | Success/failure indicators | Medium |

### 14.6 Phase 6: Saga Integration (Priority: Medium)

| Task | Description | Complexity |
| --- | --- | --- |
| Implement Legend rewards | Discovery-based Legend | Medium |
| Create Saga Feat system | Achievement-based feats | High |
| Implement corruption margin notes | Auto-generated annotations | Medium |
| Add journal milestones | Major discovery tracking | Low |
| Create feat unlock celebrations | Modal notifications | Low |

### 14.7 Phase 7: Log Integration (Priority: Medium)

| Task | Description | Complexity |
| --- | --- | --- |
| Integrate Quest Journal data | Share with existing system | Medium |
| Create journal-styled quest display | Thematic presentation | Medium |
| Implement saga chapters | Completed quest history | Low |

### 14.8 Phase 8: Polish (Priority: Low)

| Task | Description | Complexity |
| --- | --- | --- |
| Add search functionality | Text search across entries | Medium |
| Add filter controls | Type/category filtering | Medium |
| Add sort options | Multiple sort criteria | Low |
| Implement keyboard shortcuts | Full keyboard navigation | Medium |
| Add sound effects | Page turns, discoveries | Low |
| Add accessibility features | Screen reader support | Medium |

---

## 15. Appendices

### Appendix A: Terminology

| Term | Definition |
| --- | --- |
| Stub Entry | Basic entry created on first encounter, free of cost |
| Partial Entry | Entry with some combat/lore data, requires examination |
| Complete Entry | Entry with all standard information revealed |
| Mastered Entry | Entry with specialist tactical notes added |
| Glitch Tier | Visual corruption level based on Psychic Stress |
| Margin Note | Corruption-generated autonomous annotations |
| Data-Log | In-world framing for lore fragments |
| Saga Feat | Permanent bonus unlocked through journal completion |
| Dialogue Keyword | Unlocked conversation option from Codex entries |

### Appendix B: Data Flow Diagram

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Combat/Explore │────▶│ JournalController│────▶│ JournalViewModel│
│                 │     │                  │     │                 │
│ - Enemy Defeat  │     │ - Add Entry      │     │ - Entry Lists   │
│ - Object Exam   │     │ - Update Entry   │     │ - Selected      │
│ - Lore Found    │     │ - Award Legend   │     │ - Filters       │
└─────────────────┘     └──────────────────┘     └─────────────────┘
        │                       │                       │
        │                       │                       ▼
        │                       │               ┌─────────────────┐
        │                       │               │  JournalView    │
        │                       │               │                 │
        │                       │               │ - Section Tabs  │
        │                       │               │ - Entry List    │
        │                       │               │ - Detail Panel  │
        │                       │               │ - Glitch Effects│
        │                       │               └─────────────────┘
        │                       │                       │
        ▼                       ▼                       ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ PlayerCharacter │     │  GlitchService   │     │  HUD Widget     │
│                 │     │                  │     │                 │
│ - PsychicStress │────▶│ - Glitch Tier    │     │ - Tracked Entry │
│ - Corruption    │     │ - Text Corrupt   │     │ - Quick Access  │
└─────────────────┘     └──────────────────┘     └─────────────────┘

```

### Appendix C: ViewModel Property Summary

```csharp
// ScavengersJournalViewModel
public class ScavengersJournalViewModel : ViewModelBase
{
    // Section Management
    public JournalSection SelectedSection { get; set; }
    public ObservableCollection<JournalEntryViewModel> FilteredEntries { get; }
    public JournalEntryViewModel? SelectedEntry { get; set; }

    // Search & Filter
    public string SearchQuery { get; set; }
    public EntryTypeFilter FilterType { get; set; }
    public EntrySortOption SortOption { get; set; }

    // Player State
    public int PsychicStress { get; }
    public int Corruption { get; }
    public float PsychicStressPercent { get; }
    public float CorruptionPercent { get; }
    public GlitchTier CurrentGlitchTier { get; }

    // Statistics
    public int TotalEntries { get; }
    public int DiscoveredEntries { get; }
    public string DiscoveryStats { get; }
    public int LegendFromJournal { get; }
    public List<SagaFeat> UnlockedFeats { get; }

    // Visibility
    public bool IsVisible { get; set; }
    public bool HasSelectedEntry { get; }
    public bool CanExamineEntry { get; }

    // Commands
    public ICommand CloseCommand { get; }
    public ICommand SelectSectionCommand { get; }
    public ICommand SelectEntryCommand { get; }
    public ICommand ExamineEntryCommand { get; }
    public ICommand TrackEntryCommand { get; }
    public ICommand MarkReadCommand { get; }
    public ICommand SetFilterCommand { get; }
    public ICommand SetSortCommand { get; }
    public ICommand ClearFiltersCommand { get; }
}

```

### Appendix D: Glitch Effect Examples

**Stable (0-24% Stress):**

> The Rusted Servitor stands motionless, its joints creaking with age.
> 

**Unstable (25-49% Stress):**

> Th3 Rust3d S3rv1t0r st4nds m0t10nl3ss, its j01nts cr34king with 4g3.
> 

**Degraded (50-74% Stress):**

> The Rust%d S#rv!tor st@nds... motion...less, its j$ints cre%king w#th... age.
> 

**Critical (75-89% Stress):**

> The ▓▓▓▓▓ Serv░░░r s█ands ███ionless... [SYSTEM ERROR: MEMORY CORRUPTED]... joints ░░░░king with ▓▓▓.
> 

**Compromised (90-100% Stress):**

> ▓▓▓ ▓▓▓▓▓▓ ▓▓▓▓▓▓▓▓ [THE SILENCE WATCHES] ▓▓▓▓▓ ▓▓▓▓▓▓▓▓▓▓ ▓▓▓ ▓▓▓▓▓▓...
> 

### Appendix E: Intrusive Data-Log Messages

Pool of corrupted system messages that flash during high stress:

```
"[ERROR 0x8F3A] REALITY CHECKSUM MISMATCH"
"[WARNING] OBSERVER DETECTED IN SECTOR ▓▓▓"
"THE ALL-RUNE COMPILES. THE ALL-RUNE EXECUTES."
"[FATAL] CAUSALITY LOOP DETECTED AT TIMESTAMP ∞"
"memory of {PLAYER} flagged for deletion"
"[SYSTEM] Initiating Protocol SILENCE... FAILED"
"YOU WERE NOT MEANT TO READ THIS"
"the Old Code runs in your bones now"
"[ALERT] Consciousness integrity at ▓▓%"

```

---

## Document Completeness Checklist

### Structure

- [x]  All required sections present
- [x]  Version history populated
- [x]  Related documentation referenced
- [x]  Code references included

### Content

- [x]  Executive summary complete
- [x]  All journal sections documented
- [x]  Glitch system fully specified
- [x]  Dynamic entry system explained
- [x]  Integration points identified
- [x]  Implementation guidance provided

### Quality

- [x]  Technical accuracy verified
- [x]  Examples provided for complex concepts
- [x]  Cross-references included
- [x]  Formatting consistent

---

**End of Specification**

*This document is part of the Rune & Rust technical documentation suite.Related: GUI_SPECIFICATION.md (v0.45), QUEST_JOURNAL_SPECIFICATION.md (v1.0), DIALOGUE_SYSTEM_SPECIFICATION.md*