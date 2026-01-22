# v0.38.3: Interactive Object Repository

Description: 8+ object base templates, 15+ descriptors, 30+ composite objects with interaction patterns
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.37.1
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 8-10 hours

**Goal:** Build comprehensive interactive object library for Dynamic Room Engine

**Philosophy:** Define reusable investigatable objects, mechanisms, and containers with interaction patterns

---

## I. Purpose

v0.38.3 creates the **Interactive Object Repository**, providing standardized descriptors for all player-interactable environmental objects. This repository provides:

- **8+ Base Object Templates** (levers, doors, containers, corpses)
- **15+ Object Descriptors** (interaction types, skill checks)
- **30+ Composite Objects** (base + modifier + function combinations)

**Strategic Function:**

Currently, interactive objects are defined ad-hoc per room. This leads to:

- ❌ Inconsistent interaction patterns
- ❌ Duplicate skill check definitions
- ❌ Missed opportunities for environmental storytelling

**v0.38.3 transforms this to:**

- Base templates with standardized interaction mechanics
- Thematic modifiers for biome-specific flavor
- Function variants for different object purposes
- Reusable across all biomes and room types

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- **Mechanisms:** Levers, switches, consoles, pressure plates
- **Containers:** Crates, chests, lockers, safes
- **Investigatable Objects:** Corpses, data slates, terminals
- **Barriers:** Doors, gates, hatches
- **Interaction Patterns:** Pull, push, open, search, read, hack
- **Skill Checks:** WITS, MIGHT, Lockpicking, Hacking
- **Consequences:** Unlock, spawn, reveal, trap activation
- **Base templates (8+)**
- **Functional variants (per template)**
- **Composite generation**
- **Integration with v0.37.1 (Navigation Commands)**
- **Database schema**
- **Unit tests (80%+ coverage)**
- **Serilog logging**

### ❌ Explicitly Out of Scope

- NPCs and dialogue (separate system)
- Loot contents (v0.38.5)
- Quest items (defer to v0.40)
- Combat interactions (separate system)
- Crafting stations (v0.36)
- Merchants (v0.9)
- UI/rendering changes

---

## III. Object Type Taxonomy

### A. Four Core Categories

From v0.37.1 (Navigation Commands) specification:

```csharp
public enum InteractiveObjectType
{
    Mechanism,          // Levers, switches, consoles
    Container,          // Searchable storage
    Investigatable,     // Examine for clues
    Barrier            // Doors, gates
}
```

### B. Interaction Pattern Types

**1. Simple Actions (No Check)**

- **Pull/Push:** Levers, switches
- **Open/Close:** Unlocked doors, containers
- **Read:** Data slates, inscriptions

**2. Skill Checks**

- **WITS Check:** Puzzles, hacking, searching
- **MIGHT Check:** Forcing doors, moving objects
- **Lockpicking:** Locked containers, doors
- **Hacking:** Consoles, terminals

**3. Resource Costs**

- **Key Required:** Specific key item
- **Consumable:** Explosive charge, acid vial
- **Time Cost:** Multiple turns to complete

---

## IV. Base Template Definitions

### Mechanism Templates

### Template 1: Lever_Base

**Category:** Object

**Type:** Mechanism

**Tags:** `["Mechanism", "Simple", "Puzzle"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Pull",
  "requires_check": false,
  "states": ["Up", "Down"],
  "reversible": true,
  "consequence_type": "Trigger",
  "range": "Adjacent"
}
```

**Name Template:**

`"{Modifier} Lever"`

**Description Template:**

```
"A {Modifier_Adj} lever {Modifier_Detail}. It is currently in the {State} position."
```

**Interaction Text:**

```
"You pull the lever. {Consequence_Description}."
```

**Example Composites:**

- Lever_Base + [Rusted] + "Door Control" = "Corroded Door Lever"
- Lever_Base + [Crystalline] + "Power Diverter" = "Crystalline Power Lever"

**Function Variants:**

```json
[
  {"function": "Door_Control", "consequence": "Opens/closes nearby door"},
  {"function": "Power_Diverter", "consequence": "Redirects power, deactivates hazard"},
  {"function": "Bridge_Extender", "consequence": "Extends bridge across chasm"},
  {"function": "Trap_Trigger", "consequence": "Activates trap (hostile)"},
  {"function": "Secret_Revealer", "consequence": "Reveals hidden passage"}
]
```

---

### Template 2: Console_Base

**Category:** Object

**Type:** Mechanism

**Tags:** `["Mechanism", "Hacking", "Technical"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Hack",
  "requires_check": true,
  "check_type": "WITS",
  "check_dc": 15,
  "attempts_allowed": 3,
  "failure_consequence": "Lockout",
  "consequence_type": "Data_Access"
}
```

**Name Template:**

`"{Modifier} Control Console"`

**Description Template:**

```
"A {Modifier_Adj} control console {Modifier_Detail}. Its display {Display_State}."
```

**Interaction Text:**

```
"You attempt to interface with the console. [WITS Check DC {DC}]"
```

**Function Variants:**

```json
[
  {"function": "Door_Override", "consequence": "Unlocks security doors"},
  {"function": "Hazard_Control", "consequence": "Disables environmental hazards"},
  {"function": "Data_Recovery", "consequence": "Reveals quest information"},
  {"function": "Enemy_Summon", "consequence": "Hostile: spawns Servitors"},
  {"function": "Power_Routing", "consequence": "Restores power to area"}
]
```

---

### Template 3: Pressure_Plate_Base

**Category:** Object

**Type:** Mechanism

**Tags:** `["Mechanism", "Trap", "Hidden"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Automatic",
  "trigger": "Weight",
  "detection_dc": 18,
  "disarm_dc": 20,
  "consequence_type": "Trap",
  "one_time": false
}
```

**Name Template:**

`"Hidden Pressure Plate"`

**Description Template:**

```
"A concealed pressure plate {Modifier_Detail}. [Detected only with WITS check DC 18]"
```

**Function Variants:**

```json
[
  {"function": "Dart_Trap", "consequence": "Fires poisoned darts (2d6 + Poison)"},
  {"function": "Alarm", "consequence": "Alerts enemies in adjacent rooms"},
  {"function": "Collapse", "consequence": "Triggers ceiling collapse"},
  {"function": "Lock_Doors", "consequence": "Seals all exits"}
]
```

---

### Container Templates

### Template 4: Crate_Base

**Category:** Object

**Type:** Container

**Tags:** `["Container", "Common", "Searchable"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Search",
  "requires_check": false,
  "locked": false,
  "destructible": true,
  "hp": 20,
  "loot_tier": "Common",
  "search_time": 1
}
```

**Name Template:**

`"{Modifier} Crate"`

**Description Template:**

```
"A {Modifier_Adj} crate {Modifier_Detail}. It appears {Locked_State}."
```

**Interaction Text:**

```
"You search the crate. {Loot_Result}."
```

**Example Composites:**

- Crate_Base + [Rusted] = "Corroded Storage Crate"
- Crate_Base + [Scorched] = "Fire-Damaged Crate"

---

### Template 5: Chest_Base

**Category:** Object

**Type:** Container

**Tags:** `["Container", "Valuable", "Lockable"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Open",
  "requires_check": true,
  "check_type": "Lockpicking",
  "check_dc": 15,
  "locked": true,
  "destructible": false,
  "loot_tier": "Uncommon",
  "trap_chance": 0.2
}
```

**Name Template:**

`"{Modifier} Chest"`

**Description Template:**

```
"A {Modifier_Adj} chest {Modifier_Detail}. It is {Locked_State}."
```

**Function Variants:**

```json
[
  {"function": "Supply_Cache", "loot_tier": "Uncommon", "trap_chance": 0.1},
  {"function": "Weapon_Locker", "loot_tier": "Rare", "trap_chance": 0.3},
  {"function": "Trapped_Decoy", "loot_tier": "Common", "trap_chance": 1.0}
]
```

---

### Template 6: Locker_Base

**Category:** Object

**Type:** Container

**Tags:** `["Container", "Personal", "Lockable"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Open",
  "requires_check": true,
  "check_type": "Lockpicking",
  "check_dc": 12,
  "locked": true,
  "destructible": true,
  "hp": 30,
  "loot_tier": "Common",
  "personal_effects": true
}
```

**Name Template:**

`"{Modifier} Locker"`

**Description Template:**

```
"A {Modifier_Adj} personal locker {Modifier_Detail}. A faded nameplate reads '{Random_Name}'."
```

---

### Investigatable Templates

### Template 7: Corpse_Base

**Category:** Object

**Type:** Investigatable

**Tags:** `["Investigatable", "Loot", "Narrative"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Search",
  "requires_check": false,
  "loot_tier": "Random",
  "search_time": 1,
  "narrative_clue": true,
  "decay_state": "Recent"
}
```

**Name Template:**

`"{Entity_Type} Corpse"`

**Description Template:**

```
"The corpse of {Article} {Entity_Type} lies here. {Decay_Description}. {Cause_Of_Death_Clue}."
```

**Entity Type Variants:**

```json
[
  {"type": "Scavenger", "loot_tier": "Common", "narrative": "Killed by Servitors"},
  {"type": "Jötun-Reader", "loot_tier": "Uncommon", "narrative": "Research notes"},
  {"type": "Warrior", "loot_tier": "Uncommon", "narrative": "Combat gear"},
  {"type": "Dvergr Tinkerer", "loot_tier": "Rare", "narrative": "Schematic fragment"}
]
```

---

### Template 8: Data_Slate_Base

**Category:** Object

**Type:** Investigatable

**Tags:** `["Investigatable", "Readable", "Quest"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Read",
  "requires_check": false,
  "text_length": "Short",
  "corruption_level": "None",
  "quest_hook": false,
  "lore_value": "Medium"
}
```

**Name Template:**

`"{Modifier} Data-Slate"`

**Description Template:**

```
"A {Modifier_Adj} Data-Slate {Modifier_Detail}. Its display {Display_State}."
```

**Content Type Variants:**

```json
[
  {"type": "Personal_Log", "narrative": "Daily log entry", "quest_hook": false},
  {"type": "Warning_Message", "narrative": "Danger alert", "quest_hook": false},
  {"type": "Research_Notes", "narrative": "Technical data", "quest_hook": true},
  {"type": "Distress_Signal", "narrative": "Call for help", "quest_hook": true},
  {"type": "Corrupted_Fragment", "narrative": "Glitched text", "lore_value": "High"}
]
```

---

### Barrier Templates

### Template 9: Door_Base

**Category:** Object

**Type:** Barrier

**Tags:** `["Barrier", "Passage", "Lockable"]`

**Base Mechanics:**

```json
{
  "interaction_type": "Open",
  "requires_check": false,
  "locked": false,
  "destructible": true,
  "hp": 50,
  "soak": 10,
  "blocks_los": true
}
```

**Name Template:**

`"{Modifier} Door"`

**Description Template:**

```
"A {Modifier_Adj} door {Modifier_Detail}. It is currently {State}."
```

**Function Variants:**

```json
[
  {"function": "Standard", "locked": false, "hp": 50},
  {"function": "Security", "locked": true, "hp": 80, "requires": "Keycard"},
  {"function": "Blast", "locked": true, "hp": 150, "requires": "Console_Override"},
  {"function": "Rusted_Stuck", "locked": false, "requires_check": true, "check_dc": 15}
]
```

---

## V. Interaction Mechanics

### A. Skill Check Resolution

**From v0.37.1 Navigation Commands:**

```csharp
public class ObjectInteractionService
{
    public InteractionResult AttemptInteraction(
        Character character,
        InteractiveObject obj)
    {
        // Check if interaction requires skill check
        if (!obj.RequiresCheck)
        {
            return ExecuteInteraction(character, obj);
        }
        
        // Resolve skill check
        var checkResult = ResolveSkillCheck(
            character,
            obj.CheckType,
            obj.CheckDC);
        
        if (checkResult.Success)
        {
            return ExecuteInteraction(character, obj);
        }
        else
        {
            return HandleFailure(character, obj, checkResult);
        }
    }
    
    private SkillCheckResult ResolveSkillCheck(
        Character character,
        string checkType,
        int dc)
    {
        var dicePool = GetDicePool(character, checkType);
        var roll = RollDice(dicePool);
        var successes = CountSuccesses(roll, dc);
        
        return new SkillCheckResult
        {
            Success = successes > 0,
            Successes = successes,
            Roll = roll
        };
    }
}
```

### B. Consequence Types

**1. Unlock/Open**

- Door opens
- Container accessible
- Passage revealed

**2. Trigger Mechanism**

- Activate lever consequence
- Execute console command
- Fire trap

**3. Spawn**

- Enemies spawn (hostile console)
- NPC appears
- Event triggers

**4. Reveal Information**

- Read data slate text
- Display quest hook
- Update player knowledge

**5. Grant Loot**

- Add items to inventory
- Currency reward
- Quest item obtained

---

## VI. Database Schema Implementation

### SQL: Interactive Objects Table

```sql
-- =====================================================
-- INTERACTIVE OBJECTS (Instances)
-- =====================================================

CREATE TABLE IF NOT EXISTS Interactive_Objects (
    object_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    
    -- Descriptor reference
    composite_descriptor_id INTEGER,
    
    -- Object identity
    object_name TEXT NOT NULL,
    description TEXT NOT NULL,
    object_type TEXT NOT NULL,  -- 'Mechanism', 'Container', etc.
    
    -- Interaction mechanics
    interaction_type TEXT NOT NULL,  -- 'Pull', 'Search', 'Read', 'Hack'
    requires_check BOOLEAN DEFAULT 0,
    check_type TEXT,  -- 'WITS', 'MIGHT', 'Lockpicking'
    check_dc INTEGER,
    
    -- State
    current_state TEXT,  -- 'Up'/'Down', 'Open'/'Closed', 'Locked'/'Unlocked'
    interacted BOOLEAN DEFAULT 0,
    
    -- Loot reference (for containers)
    loot_table_id INTEGER,
    
    -- Consequence
    consequence_type TEXT,
    consequence_data TEXT,  -- JSON
    
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id),
    FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id)
);

-- =====================================================
-- OBJECT INTERACTIONS LOG
-- =====================================================

CREATE TABLE IF NOT EXISTS Object_Interactions (
    interaction_id INTEGER PRIMARY KEY AUTOINCREMENT,
    object_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    interaction_timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    
    -- Check result
    check_required BOOLEAN,
    check_roll TEXT,  -- JSON array of dice
    check_success BOOLEAN,
    
    -- Consequence
    consequence_executed TEXT,
    
    FOREIGN KEY (object_id) REFERENCES Interactive_Objects(object_id),
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);
```

### SQL: Insert Base Templates

```sql
-- =====================================================
-- LEVER BASE
-- =====================================================

INSERT INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags
) VALUES (
    'Lever_Base',
    'Object',
    'Mechanism',
    '{
      "interaction_type": "Pull",
      "requires_check": false,
      "states": ["Up", "Down"],
      "reversible": true,
      "consequence_type": "Trigger",
      "range": "Adjacent"
    }',
    '{Modifier} Lever',
    'A {Modifier_Adj} lever {Modifier_Detail}. It is currently in the {State} position.',
    '["Mechanism", "Simple", "Puzzle"]'
);

-- =====================================================
-- CONSOLE BASE
-- =====================================================

INSERT INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags
) VALUES (
    'Console_Base',
    'Object',
    'Mechanism',
    '{
      "interaction_type": "Hack",
      "requires_check": true,
      "check_type": "WITS",
      "check_dc": 15,
      "attempts_allowed": 3,
      "failure_consequence": "Lockout",
      "consequence_type": "Data_Access"
    }',
    '{Modifier} Control Console',
    'A {Modifier_Adj} control console {Modifier_Detail}. Its display {Display_State}.',
    '["Mechanism", "Hacking", "Technical"]'
);

-- Continue for remaining 7 object templates...
```

---

## VII. Service Implementation

### ObjectInteractionService.cs

```csharp
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);

public class ObjectInteractionService : IObjectInteractionService
{
    private readonly IDescriptorRepository _repository;
    private readonly ISkillCheckService _skillCheckService;
    private readonly ILogger<ObjectInteractionService> _logger;
    
    /// <summary>
    /// Generate interactive object from descriptor composite.
    /// </summary>
    public InteractiveObject GenerateObject(
        string baseTemplateName,
        string modifierName,
        string functionVariant,
        int roomId)
    {
        var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
        var modifier = _repository.GetModifier(modifierName);
        
        if (baseTemplate == null || modifier == null)
        {
            _logger.LogError(
                "Template or modifier not found: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw new ArgumentException("Invalid template or modifier");
        }
        
        // Parse base mechanics
        var mechanics = JsonSerializer.Deserialize<ObjectMechanics>(
            baseTemplate.BaseMechanics);
        
        // Apply function variant
        if (!string.IsNullOrEmpty(functionVariant))
        {
            mechanics = ApplyFunctionVariant(mechanics, functionVariant);
        }
        
        // Generate name and description
        var name = GenerateName(baseTemplate.NameTemplate, modifier);
        var description = GenerateDescription(
            baseTemplate.DescriptionTemplate,
            baseTemplate,
            modifier,
            mechanics);
        
        _logger.LogDebug(
            "Generated interactive object: {Name}",
            name);
        
        return new InteractiveObject
        {
            RoomId = roomId,
            Name = name,
            Description = description,
            ObjectType = baseTemplate.Archetype,
            InteractionType = mechanics.InteractionType,
            RequiresCheck = mechanics.RequiresCheck,
            CheckType = mechanics.CheckType,
            CheckDC = mechanics.CheckDC,
            CurrentState = mechanics.States?[0] ?? "Default",
            ConsequenceType = mechanics.ConsequenceType,
            ConsequenceData = JsonSerializer.Serialize(mechanics.Consequence)
        };
    }
    
    /// <summary>
    /// Attempt interaction with object.
    /// </summary>
    public InteractionResult AttemptInteraction(
        Character character,
        InteractiveObject obj)
    {
        try
        {
            _logger.LogInformation(
                "Character {CharId} attempting interaction with {ObjName}",
                character.CharacterId,
                [obj.Name](http://obj.Name));
            
            // Check if already interacted (one-time objects)
            if (obj.Interacted && !obj.Reversible)
            {
                return InteractionResult.AlreadyUsed(
                    $"The {[obj.Name](http://obj.Name)} has already been used.");
            }
            
            // Check if requires skill check
            if (!obj.RequiresCheck)
            {
                return ExecuteInteraction(character, obj);
            }
            
            // Resolve skill check
            var checkResult = _skillCheckService.ResolveCheck(
                character,
                obj.CheckType,
                obj.CheckDC);
            
            // Log interaction
            LogInteraction(character, obj, checkResult);
            
            if (checkResult.Success)
            {
                return ExecuteInteraction(character, obj);
            }
            else
            {
                return HandleFailure(character, obj, checkResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error during interaction: Obj={ObjId}, Char={CharId}",
                obj.ObjectId,
                character.CharacterId);
            throw;
        }
    }
    
    private InteractionResult ExecuteInteraction(
        Character character,
        InteractiveObject obj)
    {
        // Mark as interacted
        obj.Interacted = true;
        
        // Execute consequence
        var consequence = ExecuteConsequence(character, obj);
        
        // Update state
        if (obj.Reversible && obj.CurrentState != null)
        {
            obj.CurrentState = ToggleState(obj.CurrentState, obj.States);
        }
        
        _logger.LogInformation(
            "Interaction successful: Obj={ObjName}, Consequence={Type}",
            [obj.Name](http://obj.Name),
            obj.ConsequenceType);
        
        return InteractionResult.Success(
            $"You {obj.InteractionType.ToLower()} the {[obj.Name](http://obj.Name)}. {consequence.Description}",
            consequence);
    }
    
    private ConsequenceResult ExecuteConsequence(
        Character character,
        InteractiveObject obj)
    {
        var consequenceData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            obj.ConsequenceData ?? "{}");
        
        return obj.ConsequenceType switch
        {
            "Unlock" => ExecuteUnlock(obj, consequenceData),
            "Trigger" => ExecuteTrigger(obj, consequenceData),
            "Spawn" => ExecuteSpawn(obj, consequenceData),
            "Reveal" => ExecuteReveal(obj, consequenceData),
            "Loot" => ExecuteLoot(character, obj, consequenceData),
            _ => ConsequenceResult.None()
        };
    }
    
    private InteractionResult HandleFailure(
        Character character,
        InteractiveObject obj,
        SkillCheckResult checkResult)
    {
        _logger.LogInformation(
            "Interaction failed: Obj={ObjName}, Roll={Roll}",
            [obj.Name](http://obj.Name),
            JsonSerializer.Serialize(checkResult.Roll));
        
        // Check for critical failure consequences
        if (obj.FailureConsequence != null)
        {
            var failureResult = ExecuteFailureConsequence(character, obj);
            return InteractionResult.Failure(
                $"You fail to {obj.InteractionType.ToLower()} the {[obj.Name](http://obj.Name)}. {failureResult.Description}",
                checkResult);
        }
        
        return InteractionResult.Failure(
            $"You fail to {obj.InteractionType.ToLower()} the {[obj.Name](http://obj.Name)}.",
            checkResult);
    }
}
```

---

## VIII. Integration with v0.37.1 (Navigation Commands)

### Command: `investigate [object]`

```csharp
// From v0.37.1 specification
public class InvestigateCommand : ICommand
{
    private readonly IObjectInteractionService _objectService;
    
    public CommandResult Execute(Character character, string[] args)
    {
        if (args.Length == 0)
        {
            return ListInvestigatableObjects(character.CurrentRoom);
        }
        
        var targetName = string.Join(" ", args);
        var obj = FindObject(character.CurrentRoom, targetName);
        
        if (obj == null)
        {
            return CommandResult.Failure($"You don't see '{targetName}' here.");
        }
        
        // Attempt interaction
        var result = _objectService.AttemptInteraction(character, obj);
        
        return CommandResult.FromInteractionResult(result);
    }
}
```

---

## IX. Success Criteria

**v0.38.3 is DONE when:**

### Base Templates

- [ ]  8+ object base templates
- [ ]  All 4 core types (Mechanism, Container, Investigatable, Barrier)
- [ ]  Base mechanics JSON valid
- [ ]  Interaction patterns defined
- [ ]  Function variants documented

### Object Descriptors

- [ ]  15+ object descriptors
- [ ]  Interaction types (Pull, Search, Read, Hack, Open)
- [ ]  Skill check types (WITS, MIGHT, Lockpicking)
- [ ]  Consequence types (Unlock, Trigger, Spawn, Reveal, Loot)

### Composite Generation

- [ ]  30+ composites auto-generated
- [ ]  Base + modifier + function combinations
- [ ]  Mechanics merge correctly
- [ ]  States handled properly

### Service Implementation

- [ ]  ObjectInteractionService complete
- [ ]  GenerateObject() functional
- [ ]  AttemptInteraction() functional
- [ ]  Skill check integration
- [ ]  Consequence execution logic
- [ ]  Serilog logging throughout

### Database Schema

- [ ]  Interactive_Objects table
- [ ]  Object_Interactions log table
- [ ]  State persistence
- [ ]  Loot table references

### Integration

- [ ]  v0.37.1 investigate command uses service
- [ ]  Commands functional (investigate, pull, search, read)
- [ ]  Skill checks resolve correctly
- [ ]  Consequences execute

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Interaction flow tests
- [ ]  Skill check tests
- [ ]  Consequence execution tests
- [ ]  Integration tests with v0.37.1

---

## X. Implementation Roadmap

**Phase 1: Database Schema** — 2 hours

- Object base templates (8+)
- Function variant definitions
- Interactive_Objects table
- Object_Interactions log

**Phase 2: Service Implementation** — 3 hours

- ObjectInteractionService
- Interaction resolution logic
- Skill check integration
- Consequence execution

**Phase 3: Object Generation** — 2 hours

- Template processing
- Function variant application
- State management
- Composite instantiation

**Phase 4: Integration** — 2 hours

- Update v0.37.1 commands
- Test interaction flows
- Verify skill checks
- Test consequences

**Phase 5: Testing & Validation** — 1 hour

- Unit tests
- Integration tests
- Interaction validation

**Total: 10 hours**

---

## XI. Example Generated Objects

### Example 1: Corroded Door Lever

**Base:** Lever_Base

**Modifier:** [Rusted]

**Function:** Door_Control

**Generated Object:**

```json
{
  "name": "Corroded Door Lever",
  "description": "A corroded lever shows centuries of oxidation and decay. It is currently in the Up position.",
  "interaction_type": "Pull",
  "requires_check": false,
  "consequence_type": "Unlock",
  "consequence_data": {
    "target": "nearby_door",
    "action": "open"
  }
}
```

**Interaction:**

> **Player:** `pull lever`
> 

> 
> 

> **System:** You pull the lever. It groans with resistance but moves into the Down position. You hear the heavy clank of a door mechanism unlocking nearby.
> 

---

### Example 2: Crystalline Control Console

**Base:** Console_Base

**Modifier:** [Crystalline]

**Function:** Hazard_Control

**Generated Object:**

```json
{
  "name": "Crystalline Control Console",
  "description": "A crystalline control console pulses with unstable Aetheric energy. Its display flickers with incomprehensible symbols.",
  "interaction_type": "Hack",
  "requires_check": true,
  "check_type": "WITS",
  "check_dc": 15,
  "consequence_type": "Trigger",
  "consequence_data": {
    "action": "disable_hazards",
    "room": "current"
  }
}
```

**Interaction:**

> **Player:** `investigate console`
> 

> 
> 

> **System:** You attempt to interface with the console. [WITS Check DC 15]
> 

> Roll: [5, 6, 3, 7, 8] = 2 successes
> 

> 
> 

> Success! You navigate the corrupted interface and access the hazard control subroutine. The steam vents in the room shut down with a hiss.
> 

---

### Example 3: Scavenger Corpse

**Base:** Corpse_Base

**Entity Type:** Scavenger

**Generated Object:**

```json
{
  "name": "Scavenger Corpse",
  "description": "The corpse of a scavenger lies here. The body is recently deceased, no more than a day old. Puncture wounds suggest Servitor weapons.",
  "interaction_type": "Search",
  "requires_check": false,
  "loot_tier": "Common",
  "narrative_clue": "The scavenger's pack contains a hastily scrawled map fragment."
}
```

**Interaction:**

> **Player:** `search corpse`
> 

> 
> 

> **System:** You search the corpse. You find: 35 Dvergr Cogs, 1× Healing Poultice, 1× Map Fragment.
> 

> 
> 

> The scavenger's pack contains a hastily scrawled map fragment marking a location deeper in the ruins.
> 

---

**Ready to implement the Interactive Object Repository.**