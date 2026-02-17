---
id: SPEC-UI-COMMAND-PARSER
title: "Command Parser — Grammar & Resolution Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Terminal/CommandParser.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Commands/CommandDispatcher.cs"
    status: Planned
---

# Command Parser — Grammar & Resolution Specification

---

## 1. Overview

The command parser interprets player text input and routes it to the appropriate command handler. It handles:
- Grammar parsing (verb + arguments)
- Target resolution and disambiguation
- Alias expansion
- Context validation
- Error messaging

**Design Philosophy**: Verb-first syntax with smart target resolution creates a natural, discoverable command language.

---

## 2. Grammar Specification

### 2.1 Basic Syntax

```
<verb> [<target>] [<preposition> <secondary_target>]
```

| Component | Required | Description |
|-----------|----------|-------------|
| `verb` | ✓ | Action to perform |
| `target` | Optional | Primary target of action |
| `preposition` | Optional | Relationship keyword |
| `secondary_target` | Optional | Secondary target |

### 2.2 Valid Prepositions

| Preposition | Usage |
|-------------|-------|
| `on` | `use skewer on goblin` |
| `at` | `look at lever` |
| `with` | `unlock door with key` |
| `to` | `talk to merchant` |
| `from` | `take sword from corpse` |

### 2.3 Command Forms

**No Arguments:**
```
> look
> inventory
> defend
> rest
```

**Single Target:**
```
> attack goblin
> examine lever
> equip longsword
> go north
```

**Target + Preposition + Secondary:**
```
> use skewer on orc
> unlock door with iron key
> talk to merchant
> look at rusty lever
```

### 2.4 Special Forms

**Hotkey Execution:**
```
> 1          → Execute smart command #1
> 2          → Execute smart command #2
> 1-9        → Execute smart command #N
```

**Direction Shortcuts:**
```
> n          → go north
> s          → go south
> e          → go east
> w          → go west
> u          → go up
> d          → go down
```

**Dialogue Options:**
```
> 1          → Select dialogue option #1
> say 2      → Select dialogue option #2
```

---

## 3. Target Resolution

### 3.1 Resolution Order

When resolving a target, the parser attempts matches in order:

1. **Exact Match** — `attack goblin scout` matches "Goblin Scout"
2. **Case-Insensitive Match** — `attack GOBLIN` matches "Goblin"
3. **Prefix Match** — `attack gob` matches "Goblin Scout"
4. **Partial Match** — `attack scout` matches "Goblin Scout"
5. **Fuzzy Match** — `attack gobblin` matches "Goblin" (typo tolerance)

### 3.2 Resolution Scope

| Context | Search Scope |
|---------|--------------|
| Exploration | Room objects, NPCs, exits, items on ground |
| Combat | Combatants, environmental objects |
| Dialogue | Dialogue options |
| Inventory | Player inventory, equipped items |

### 3.3 Multi-Word Targets

Parser uses **smart boundary detection**:

```
> attack rust horror       → Target: "Rust Horror"
> attack goblin scout      → Target: "Goblin Scout"
> use healing draught      → Target: "Healing Draught"
```

**Algorithm:**
1. Try entire remaining input as target
2. If no match, try progressively shorter substrings
3. If still no match, try word-by-word matching

### 3.4 Target Resolution Record

```csharp
public record ResolvedTarget(
    Guid EntityId,
    string EntityName,
    EntityType Type,           // NPC, Enemy, Object, Item, Exit
    float MatchConfidence,     // 0.0 - 1.0
    string MatchedInput        // What user typed
);
```

---

## 4. Disambiguation

### 4.1 Ambiguity Detection

Ambiguity occurs when multiple entities match the target:

```
> attack goblin
  
  Which goblin?
  (1) Goblin Scout [20/20 HP]
  (2) Goblin Warrior [35/35 HP]
  
  > _
```

### 4.2 Disambiguation Strategies

**Interactive Prompt (Default):**
```
> attack goblin

  Which goblin?
  (1) Goblin Scout [20/20 HP]
  (2) Goblin Warrior [35/35 HP]
  
> 1
  You attack the Goblin Scout...
```

**Numbered Suffix:**
```
> attack goblin 1          → Attacks first goblin
> attack goblin 2          → Attacks second goblin
```

**Ordinal Keywords:**
```
> attack first goblin      → Attacks first goblin
> attack second goblin     → Attacks second goblin
> attack last goblin       → Attacks last goblin
```

**Descriptive Suffix:**
```
> attack goblin scout      → Matches by subtype
> attack wounded goblin    → Matches by status
```

### 4.3 Disambiguation Order

When multiple matches exist, order by:

1. **Proximity** — Nearest entity first
2. **Threat** — Enemies before neutrals
3. **Interaction** — Previously targeted first
4. **Alphabetical** — Fallback ordering

---

## 5. Alias System

### 5.1 Verb Aliases

| Canonical | Aliases |
|-----------|---------|
| `look` | `l`, `examine`, `x`, `inspect` |
| `go` | `move`, `walk` |
| `attack` | `a`, `hit`, `strike` |
| `defend` | `block` |
| `use` | `u` |
| `inventory` | `i`, `inv` |
| `equip` | `eq`, `wear` |
| `unequip` | `uneq`, `remove` |
| `take` | `get`, `pickup` |
| `search` | `find` |
| `talk` | `speak` |
| `help` | `h`, `?` |
| `quit` | `q`, `exit` |
| `character` | `char`, `stats` |
| `journal` | `j`, `quests` |

### 5.2 Direction Aliases

| Canonical | Aliases |
|-----------|---------|
| `go north` | `n`, `north` |
| `go south` | `s`, `south` |
| `go east` | `e`, `east` |
| `go west` | `w`, `west` |
| `go up` | `u`, `up` |
| `go down` | `d`, `down` |

### 5.3 Context-Aware Verb Mapping

The `use` command maps to specific verbs based on object type:

| Object Type | `use` Maps To |
|-------------|---------------|
| Lever | `pull lever` |
| Button | `press button` |
| Valve | `turn valve` |
| Crate | `push crate` |
| Door | `open door` |
| Container | `search container` |
| Consumable | `consume item` |
| Ability | `activate ability` |

```
> use lever         → pull lever
> use button        → press button  
> use health potion → consume health potion
```

---

## 6. Context Validation

### 6.1 Context States

| Context | Description |
|---------|-------------|
| `MainMenu` | Title screen |
| `Exploration` | Normal gameplay |
| `Combat` | Turn-based combat |
| `Dialogue` | NPC conversation |
| `Inventory` | Inventory management |
| `CharacterSheet` | Viewing character |
| `Cutscene` | Non-interactive sequence |

### 6.2 Command Availability by Context

| Command | Menu | Exploration | Combat | Dialogue | Inventory |
|---------|------|-------------|--------|----------|-----------|
| `go` | ✗ | ✓ | ✗ | ✗ | ✗ |
| `look` | ✗ | ✓ | ✓ | ✗ | ✓ |
| `attack` | ✗ | ✗ | ✓ | ✗ | ✗ |
| `defend` | ✗ | ✗ | ✓ | ✗ | ✗ |
| `talk` | ✗ | ✓ | ✗ | ✗ | ✗ |
| `say` | ✗ | ✗ | ✗ | ✓ | ✗ |
| `inventory` | ✗ | ✓ | ✓ | ✗ | — |
| `equip` | ✗ | ✓ | ✗ | ✗ | ✓ |
| `rest` | ✗ | ✓ | ✗ | ✗ | ✗ |
| `save` | ✓ | ✓ | ✗ | ✗ | ✓ |
| `help` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `quit` | ✓ | ✓ | ✓ | ✓ | ✓ |

### 6.3 Context Transition Commands

| Command | From | To |
|---------|------|-----|
| `inventory` | Exploration | Inventory |
| `close` | Inventory | Exploration |
| `leave` | Dialogue | Exploration |
| `flee` | Combat | Exploration (if successful) |
| `end` | Combat (your turn) | Combat (next turn) |

---

## 7. Error Handling

### 7.1 Error Types

| Error | Message Template |
|-------|------------------|
| `UnknownCommand` | Unknown command: '{verb}'. Type 'help' for commands. |
| `InvalidTarget` | Cannot find '{target}'. |
| `AmbiguousTarget` | Which one? (1) {option1} (2) {option2}... |
| `MissingTarget` | {verb} requires a target. Usage: {verb} <target> |
| `WrongContext` | Cannot {verb} during {context}. |
| `InvalidDirection` | There is no exit '{direction}'. Valid exits: {exits} |
| `InsufficientResource` | Not enough {resource} ({required} needed, {current} available). |
| `TargetOutOfRange` | {target} is out of range. |
| `ActionUnavailable` | Cannot {verb} right now. |

### 7.2 Error Display Format

**Terminal:**
```
> attack
  Error: attack requires a target. Usage: attack <target>

> go purple
  Error: There is no exit 'purple'. Valid exits: [north] [east]

> rest
  Error: Cannot rest during combat.
```

### 7.3 Suggestions

When possible, provide helpful suggestions:

```
> attck goblin
  Unknown command: 'attck'. Did you mean 'attack'?

> use skewe
  Cannot find 'skewe'. Did you mean 'Skewer II'?
```

---

## 8. Parsed Command Structure

### 8.1 ParsedCommand Record

```csharp
public record ParsedCommand(
    CommandType Type,
    string Verb,
    string? Target,
    string? SecondaryTarget,
    string? Preposition,
    string? Direction,
    string[] Arguments,
    string RawInput,
    InputContext Context
);

public enum CommandType
{
    // Navigation
    Look, Go, Search, Investigate, Examine,
    
    // Combat
    Attack, Defend, UseAbility, Flee, Stance, Move, EndTurn,
    
    // Interaction
    Pull, Push, Turn, Press,
    
    // Inventory
    Inventory, Equip, Unequip, Take, Drop, Use,
    
    // System
    Save, Load, Help, Quit, Character, Journal, Settings,
    
    // Dialogue
    Say, Leave,
    
    // Meta
    Unknown, Hotkey
}
```

### 8.2 Parsing Pipeline

```
Input: "attack goblin scout with longsword"
  │
  ├─ Tokenize        → ["attack", "goblin", "scout", "with", "longsword"]
  │
  ├─ Extract Verb    → verb: "attack"
  │
  ├─ Resolve Aliases → verb: "attack" (no change)
  │
  ├─ Find Preposition → prep: "with", index: 3
  │
  ├─ Extract Targets → target: "goblin scout", secondary: "longsword"
  │
  ├─ Resolve Targets → target_id: "goblin_scout_01", secondary_id: "longsword_01"
  │
  ├─ Validate Context → context: Combat ✓
  │
  └─ Build Command   → ParsedCommand { Type: Attack, ... }
```

---

## 9. Command Dispatcher Integration

### 9.1 Dispatcher Pattern

```csharp
public interface ICommandDispatcher
{
    bool IsCommandRegistered(CommandType type);
    CommandResult Dispatch(ParsedCommand command, GameState state);
    IEnumerable<CommandType> GetRegisteredCommands();
}
```

### 9.2 Command Registration

```csharp
public CommandDispatcher()
{
    RegisterCommand(CommandType.Look, new LookCommand());
    RegisterCommand(CommandType.Go, new GoCommand());
    RegisterCommand(CommandType.Attack, new AttackCommand());
    RegisterCommand(CommandType.Search, new SearchCommand());
    // ... etc
}
```

### 9.3 Command Result

```csharp
public record CommandResult(
    bool Success,
    string Message,
    bool ShouldRedrawRoom = false,
    bool EndsPlayerTurn = false,
    IReadOnlyList<GameEvent>? Events = null
);
```

---

## 10. Terminal Display

### 10.1 Input Prompt

| Context | Prompt |
|---------|--------|
| Exploration | `> ` |
| Combat | `[Combat] > ` |
| Dialogue | `[Say] > ` |
| Inventory | `[Inv] > ` |
| Disambiguation | `> ` |

### 10.2 Input History

- Up/Down arrows navigate command history
- History persists across game sessions (optional)
- Maximum 100 commands in history

### 10.3 Tab Completion (Future)

```
> att<TAB>     → attack
> attack gob<TAB> → attack goblin
```

---

## 11. Implementation Status

| Component | File Path | Status |
|-----------|-----------|--------|
| CommandParser | `RuneAndRust.Terminal/CommandParser.cs` | ❌ Planned |
| CommandDispatcher | `RuneAndRust.Engine/Commands/CommandDispatcher.cs` | ❌ Planned |
| ParsedCommand | `RuneAndRust.Core/Commands/ParsedCommand.cs` | ❌ Planned |
| CommandResult | `RuneAndRust.Core/Commands/CommandResult.cs` | ❌ Planned |
| TargetResolver | `RuneAndRust.Engine/Commands/TargetResolver.cs` | ❌ Planned |
| AliasRegistry | `RuneAndRust.Engine/Commands/AliasRegistry.cs` | ❌ Planned |
