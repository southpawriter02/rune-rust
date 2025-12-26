# v0.37: Command System & Parser

Type: Technical
Description: Implements the primary player interface with Verb-Noun-Specifier syntax, command aliasing, tab-completion, contextual hints, and comprehensive error handling. 30+ core commands across navigation, combat, inventory, and system categories. 35-50 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1 (Vertical Slice), v0.10 (Combat System), v0.20 (Tactical Grid)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.37.1: Core Navigation Commands (v0%2037%201%20Core%20Navigation%20Commands%20b6200fdd7d2b42ff9b0da9fd30cc0513.md), v0.37.2: Combat Commands (v0%2037%202%20Combat%20Commands%208937ccd53d6f47b894664281c9ca75e4.md), v0.37.3: Inventory & Equipment Commands (v0%2037%203%20Inventory%20&%20Equipment%20Commands%203fc2227456e64a1080d679383466c39e.md), v0.37.4: System Commands (v0%2037%204%20System%20Commands%20d68148f55ae44321896fbc0ee07a2938.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.37-COMMAND-SYSTEM

**Status:** Design Complete — Ready for Implementation

**Timeline:** 35-50 hours total across 4 sub-specifications

**Prerequisites:** v0.1 (Vertical Slice), v0.10 (Combat System), v0.20 (Tactical Grid)

**v2.0 Source:** Feature Specification: The Command System[[1]](https://www.notion.so/Feature-Specification-The-Command-System-2a355eb312da806f8b03ed1a7642ea52?pvs=21)

---

## I. Executive Summary

### The Fantasy

"You type `look`. The room materializes in text—exits, enemies, hidden levers. You type `investigate corpse`. A skill check resolves. You find a key. You type `use key on door`. The door opens. This is the language of coherence—the programming language for survivors. Every command is a line of code written to a broken world."

**The Command System** is the primary player interface with Rune & Rust. It transforms player intent into game actions through a **Verb-Noun syntax** with smart features: aliasing, tab-completion, and contextual hints. Commands are not just UI—they are the diegetic language characters use to impose will on Aethelgard's glitching reality.

### v2.0 Canonical Foundation

This specification migrates the v2.0 Command System:

- **Feature Specification: The Command System**[[1]](https://www.notion.so/Feature-Specification-The-Command-System-2a355eb312da806f8b03ed1a7642ea52?pvs=21)
- **Feature Specification: The Command Parser**[[2]](https://www.notion.so/Feature-Specification-The-Command-Parser-2a355eb312da8056abe2e525c4673647?pvs=21)
- **Conceptual Design: The Command Language System**[[3]](https://www.notion.so/Conceptual-Design-The-Command-Language-System-2a355eb312da8040932dc0125323d001?pvs=21)

**v2.0 Core Concepts Preserved:**

- Verb-Noun-Specifier syntax structure
- Command aliasing for intuitive synonyms
- Tab-completion for context-aware suggestions
- Contextual hints (Smart Commands Pane)
- Clear, helpful error messages
- Command validation pipeline

**v5.0 Adaptations:**

- Simplified command set (focus on core gameplay)
- Enhanced error handling with suggestions
- Integration with v0.20 Tactical Grid
- Companion command integration (v0.34)
- Console-first design (no GUI dependencies)

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.37 Complete)

**Core Command Categories:**

- **Navigation Commands:** look, go, investigate, search
- **Combat Commands:** attack, [ability_name], stance, block, parry, flee
- **Inventory Commands:** inventory, equipment, take, drop, use
- **System Commands:** journal, saga, skills, rest
- **Social Commands:** talk, command (companions)

**Parser Features:**

- Verb-Noun-Specifier parsing
- Command aliasing (synonyms)
- Tab-completion suggestions
- Context validation
- Helpful error messages
- Fuzzy matching for typos

**Technical Deliverables:**

- CommandParser service (~500 lines)
- Command validation pipeline
- Error handling system
- Alias dictionary
- Context-aware hint system
- Unit test suite (25+ tests, ~85% coverage)

### ❌ Explicitly Out of Scope

- GUI/graphical interface (console-only for v1.0)
- Voice commands or natural language processing
- Macro system or scripting language
- Command history beyond basic console features
- Advanced tab-completion (defer to polish)
- Command corruption at high Psychic Stress (defer to v2.0)
- Multiplayer-specific commands (defer to v2.0+)

---

## III. Command Syntax Structure

### The Anatomy of a Command (v2.0 Canonical)

**Structure:** `[Verb] [Noun] [Specifier]`

**Components:**

1. **Verb** (Required) — The action intent
    - Examples: `go`, `look`, `attack`, `use`
    - Always first word
    - Case-insensitive
2. **Noun** (Usually Required) — The target
    - Examples: `north`, `warden`, `potion`, `lever`
    - Second word or phrase
    - Can be fuzzy matched
3. **Specifier** (Optional) — Refinement with preposition
    - Examples: `on`, `from`, `in`, `with`
    - Provides additional context
    - Used for complex interactions

**Examples:**

```
go north                    // Verb + Noun
look at lever               // Verb + Noun
attack warden               // Verb + Noun
use potion on Kara          // Verb + Noun + Specifier
take key from chest         // Verb + Noun + Specifier
```

---

## IV. Command Categories Overview

### A. Navigation Commands (v0.37.1)

**Purpose:** Explore environment, gather information

| Command | Aliases | Example | Function |
| --- | --- | --- | --- |
| `look` | `l`, `examine`, `x` | `look`, `look at lever` | Primary perception |
| `go` | `g`, `move` | `go north`, `go forge` | Move between rooms |
| `investigate` | `inv` | `investigate corpse` | Active perception check |
| `search` | - | `search chest` | Broad container search |

### B. Combat Commands (v0.37.2)

**Purpose:** Execute combat actions

| Command | Aliases | Example | Function |
| --- | --- | --- | --- |
| `attack` | `a`, `hit`, `kill` | `attack warden` | Basic weapon attack |
| `[ability_name]` | - | `shield_bash warden` | Use specialization ability |
| `stance` | - | `stance aggressive` | Change combat stance |
| `block` | - | `block` | Enter blocking stance |
| `parry` | - | `parry` | Declare parry reaction |
| `flee` | `run`, `escape` | `flee` | Attempt escape |

### C. Inventory Commands (v0.37.3)

**Purpose:** Manage items and equipment

| Command | Aliases | Example | Function |
| --- | --- | --- | --- |
| `inventory` | `inv`, `i` | `inventory` | Open inventory screen |
| `equipment` | `eq` | `equipment` | Open equipment screen |
| `take` | `get`, `pickup` | `take axe` | Acquire item |
| `drop` | - | `drop key` | Drop item |
| `use` | - | `use potion`, `use key on door` | Use consumable/interactive |

### D. System Commands (v0.37.4)

**Purpose:** Access character systems and meta-game features

| Command | Aliases | Example | Function |
| --- | --- | --- | --- |
| `journal` | `j` | `journal` | Open Scavenger's Journal |
| `saga` | - | `saga` | Open progression menu |
| `skills` | - | `skills` | View skill sheet |
| `rest` | - | `rest` | Initiate rest |
| `talk` | - | `talk to Kara` | Initiate dialogue |
| `command` | `cmd` | `command Kara attack warden` | Direct companion |

---

## V. Parser Architecture

### CommandParser Service

**Location:** `RuneAndRust.Engine/Services/CommandParser.cs` (~500 lines)

**Parsing Pipeline (10 Steps):**

1. **Receive Raw Input** — Get string from console
2. **Normalize** — Lowercase, trim whitespace
3. **Resolve Aliases** — Map synonyms to canonical verbs
4. **Tokenize** — Split into Verb, Noun, Specifier
5. **Validate Verb** — Check against known command lexicon
6. **Validate Context** — Can verb be used in current game state?
7. **Validate Target** — Does target exist and is it accessible?
8. **Validate Resources** — Does player have required Stamina/AP?
9. **Create Command Object** — Instantiate ICommand implementation
10. **Return Result** — Pass to GameEngine or return error

**Key Methods:**

```csharp
public ParsedCommand Parse(string input, GameState state)
public string ResolveAlias(string verb)
public List<string> GetValidTargets(string verb, GameState state)
public string SuggestCorrection(string invalidVerb)
public bool CanExecuteInContext(string verb, GameContext context)
```

### Error Handling (v2.0 Canonical)

**Principle:** Never return generic "Invalid command." Always provide helpful, specific feedback.

**Error Types:**

```
Unknown Verb:
> hit warden
> Unknown command: 'hit'. Did you mean 'attack'?

Invalid Target:
> attack goblin
> There is no 'goblin' to target here.

Invalid Context:
> shield_bash warden
> You must be in combat to use that ability.

Invalid Direction:
> go west
> There is no exit to the west.

Insufficient Resources:
> shield_bash warden
> Not enough Stamina. (Need 10, have 5)
```

---

## VI. Smart Features

### A. Command Aliasing

**Purpose:** Allow intuitive synonyms for common verbs

**Implementation:** `CommandAliasService.cs`

**Alias Dictionary:**

```csharp
private readonly Dictionary<string, string> _aliases = new()
{
    // Navigation
    { "l", "look" },
    { "examine", "look" },
    { "x", "look" },
    { "n", "go north" },
    { "s", "go south" },
    { "e", "go east" },
    { "w", "go west" },
    
    // Combat
    { "a", "attack" },
    { "hit", "attack" },
    { "kill", "attack" },
    { "fight", "attack" },
    { "run", "flee" },
    { "escape", "flee" },
    
    // Inventory
    { "i", "inventory" },
    { "inv", "inventory" },
    { "eq", "equipment" },
    { "get", "take" },
    { "pickup", "take" },
    
    // System
    { "j", "journal" }
};
```

### B. Tab-Completion

**Purpose:** Suggest valid completions based on context

**Behavior:**

- Press Tab to cycle through valid options
- Context-aware (only shows valid targets in current room)
- Completes both verbs and nouns

**Examples:**

```
> go [Tab] → go north → go south → go east
> attack [Tab] → attack warden_1 → attack warden_2
> use [Tab] → use potion → use key
```

### C. Contextual Hints (Smart Commands Pane)

**Purpose:** Suggest high-priority commands for current situation

**When to Update:**

- After `look` command
- After entering new room
- At start of combat turn
- After major state changes

**Priority Order:**

1. **Combat Actions** (if in combat) — `attack warden`, `shield_bash cultist`
2. **Interactive Objects** — `pull lever`, `search chest`
3. **Exits** — `go north`, `go forge`
4. **Quest Objectives** — Commands that advance active quests

**Display Format:**

```
[Suggested Commands]
[1] attack warden_1
[2] pull lever
[3] go north
```

---

## VII. Implementation Structure

### Sub-Specification Breakdown

v0.37 is divided into 4 focused sub-specifications:

**v0.37.1: Core Navigation Commands** (10-12 hours)

- `look`, `go`, `investigate`, `search` implementations
- Room description system
- Perception check integration
- Interactive object highlighting

**v0.37.2: Combat Commands** (12-15 hours)

- `attack`, `[ability_name]`, `stance`, `block`, `parry`, `flee`
- Combat state validation
- Resource consumption
- Tactical grid integration

**v0.37.3: Inventory & Equipment Commands** (8-10 hours)

- `inventory`, `equipment`, `take`, `drop`, `use`
- Item management
- Equipment slot validation
- Container interactions

**v0.37.4: System Commands** (5-8 hours)

- `journal`, `saga`, `skills`, `rest`, `talk`, `command`
- Menu navigation
- Dialogue system integration
- Companion command parsing

---

## VIII. Database Schema Requirements

### CommandAliases Table

```sql
CREATE TABLE CommandAliases (
    alias_id INTEGER PRIMARY KEY AUTOINCREMENT,
    alias_text TEXT NOT NULL UNIQUE,
    canonical_command TEXT NOT NULL,
    is_active BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_aliases_text ON CommandAliases(alias_text);
```

### CommandHistory Table

```sql
CREATE TABLE CommandHistory (
    history_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    command_text TEXT NOT NULL,
    executed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    success BOOLEAN NOT NULL,
    error_message TEXT,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

CREATE INDEX idx_history_character ON CommandHistory(character_id, executed_at);
```

---

## IX. Quality Standards

### v5.0 Compliance

✅ **Layer 2 Voice (Clinical/Diagnostic):**

- Error messages are clear and technical
- No unnecessary flavor text in parser feedback
- Command syntax is logical and consistent

✅ **Implementation-Ready Depth:**

- All parsing steps defined
- Error cases enumerated
- Integration points specified

✅ **Serilog Logging:**

- Log all command inputs
- Log validation failures
- Log execution results

### Testing Requirements

✅ **85%+ Coverage Target:**

- Command parsing tests (20+ tests)
- Alias resolution tests (5+ tests)
- Error handling tests (15+ tests)
- Context validation tests (10+ tests)
- Integration tests with GameEngine

---

## X. Success Criteria Checklist

### Functional Requirements

- [ ]  All 30+ core commands parseable
- [ ]  Alias system functional
- [ ]  Tab-completion context-aware
- [ ]  Error messages specific and helpful
- [ ]  Combat commands validate state
- [ ]  Inventory commands validate slots
- [ ]  Navigation commands validate exits
- [ ]  Fuzzy matching suggests corrections

### Quality Gates

- [ ]  85%+ unit test coverage
- [ ]  Serilog logging implemented
- [ ]  v2.0 command syntax preserved
- [ ]  All v0.37.X sub-specs complete
- [ ]  Integration with GameEngine functional

### Player Experience

- [ ]  Commands feel responsive
- [ ]  Error messages don't frustrate
- [ ]  Tab-completion saves typing
- [ ]  Aliases work intuitively
- [ ]  No common commands missing

---

## XI. Implementation Timeline

**Phase 1: Core Parser** (v0.37.1) - 10-12 hours

- CommandParser service
- Navigation commands
- Alias system

**Phase 2: Combat Integration** (v0.37.2) - 12-15 hours

- Combat commands
- Ability execution
- Stance system integration

**Phase 3: Inventory System** (v0.37.3) - 8-10 hours

- Item commands
- Equipment management
- Container interactions

**Phase 4: System Commands** (v0.37.4) - 5-8 hours

- Menu commands
- Dialogue integration
- Companion commands

**Total: 35-50 hours**

---

## XII. Related Documents

**v2.0 Canonical Sources:**

- Feature Specification: The Command System[[1]](https://www.notion.so/Feature-Specification-The-Command-System-2a355eb312da806f8b03ed1a7642ea52?pvs=21)
- Feature Specification: The Command Parser[[2]](https://www.notion.so/Feature-Specification-The-Command-Parser-2a355eb312da8056abe2e525c4673647?pvs=21)
- Conceptual Design: The Command Language System[[3]](https://www.notion.so/Conceptual-Design-The-Command-Language-System-2a355eb312da8040932dc0125323d001?pvs=21)

**Prerequisites:**

- v0.1: Vertical Slice Specification[[4]](https://www.notion.so/v0-1-Vertical-Slice-Specification-ba9d23ddfa1d44aabbad363e5338a797?pvs=21)
- v0.10: Combat System
- v0.20: Tactical Combat Grid System[[5]](https://www.notion.so/v0-20-Tactical-Combat-Grid-System-987463086e1547219f70810a6e99fe01?pvs=21)

**Integrations:**

- v0.14: Quest System[[6]](https://www.notion.so/v0-14-Quest-System-Narrative-Integration-19fb9dd195df4fd2b9bf4dbddce6448f?pvs=21)
- v0.34: NPC Companion System[[7]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

**This parent specification provides the framework. Implementation details are in v0.37.1-v0.37.4 child specifications.**