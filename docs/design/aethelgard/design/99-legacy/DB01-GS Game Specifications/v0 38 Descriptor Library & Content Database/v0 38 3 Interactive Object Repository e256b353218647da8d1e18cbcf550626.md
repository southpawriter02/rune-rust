# v0.38.3: Interactive Object Repository

**Parent Specification:** [v0.38: Descriptor Library & Content Database](../v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 8-10 hours

**Goal:** Build comprehensive interactive object library for Dynamic Room Engine

**Philosophy:** Define reusable investigatable objects, mechanisms, and containers

---

## I. Purpose

v0.38.3 creates the **Interactive Object Repository**, providing standardized descriptors for all player-interactable environmental objects:

- **8+ Base Object Templates** (levers, doors, containers, corpses)
- **15+ Object Descriptors** (interaction types, skill checks)
- **30+ Composite Objects** (base + modifier + function combinations)

**Strategic Function:**

Currently, interactive objects are defined ad-hoc per room:

- ❌ Inconsistent interaction patterns
- ❌ Duplicate skill check definitions
- ❌ Missed environmental storytelling opportunities

**v0.38.3 Solution:**

- Standardized interaction mechanics
- Reusable skill check patterns
- Thematic modifiers for biome flavor
- Function variants for different purposes

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
- **Composite generation**
- **Integration with v0.37.1 Navigation Commands**

### ❌ Out of Scope

- NPCs and dialogue (separate system)
- Loot contents (v0.38.5)
- Quest items (v0.40)
- Combat interactions (separate)
- Crafting stations (v0.36)
- UI/rendering changes

---

## III. Object Type Taxonomy

### A. Four Core Categories

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

- Pull/Push: Levers, switches
- Open/Close: Unlocked doors
- Read: Data slates

**2. Skill Checks**

- WITS: Puzzles, hacking, searching
- MIGHT: Forcing doors, moving objects
- Lockpicking: Locked containers
- Hacking: Consoles, terminals

**3. Resource Costs**

- Key Required: Specific key item
- Consumable: Explosive charge
- Time Cost: Multiple turns

---

## IV. Base Template Definitions (Part 1)

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

**Name Template:** `"{Modifier} Lever"`

**Description Template:**

`"A {Modifier_Adj} lever {Modifier_Detail}. Currently in {State} position."`

**Function Variants:**

- **Door_Control:** Opens/closes nearby door
- **Power_Diverter:** Redirects power, deactivates hazard
- **Bridge_Extender:** Extends bridge across chasm
- **Trap_Trigger:** Activates trap (hostile)
- **Secret_Revealer:** Reveals hidden passage

**Example Composites:**

- Lever_Base + [Rusted] + Door_Control = "Corroded Door Lever"
- Lever_Base + [Crystalline] + Power_Diverter = "Crystalline Power Lever"

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

**Name Template:** `"{Modifier} Control Console"`

**Description Template:**

`"A {Modifier_Adj} control console {Modifier_Detail}. Display {Display_State}."`

**Function Variants:**

- **Door_Override:** Unlocks security doors
- **Hazard_Control:** Disables environmental hazards
- **Data_Recovery:** Reveals quest information
- **Enemy_Summon:** Hostile: spawns Servitors
- **Power_Routing:** Restores power to area

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

**Name Template:** `"Hidden Pressure Plate"`

**Description Template:**

`"Concealed pressure plate {Modifier_Detail}. [Detected with WITS DC 18]"`

**Function Variants:**

- **Dart_Trap:** Fires poisoned darts (2d6 + Poison)
- **Alarm:** Alerts enemies in adjacent rooms
- **Collapse:** Triggers ceiling collapse
- **Lock_Doors:** Seals all exits

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

**Name Template:** `"{Modifier} Crate"`

**Description Template:**

`"A {Modifier_Adj} crate {Modifier_Detail}. Appears {Locked_State}."`

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

**Name Template:** `"{Modifier} Chest"`

**Function Variants:**

- **Supply_Cache:** Uncommon loot, 10% trap chance
- **Weapon_Locker:** Rare loot, 30% trap chance
- **Trapped_Decoy:** Common loot, 100% trap chance

---

*Page will be continued with remaining templates...*

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

**Name Template:** `"{Modifier} Locker"`

**Description Template:**

`"A {Modifier_Adj} personal locker {Modifier_Detail}. Nameplate reads '{Random_Name}'."`

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

**Name Template:** `"{Entity_Type} Corpse"`

**Description Template:**

`"Corpse of {Article} {Entity_Type}. {Decay_Description}. {Cause_Of_Death_Clue}."`

**Entity Type Variants:**

- **Scavenger:** Common loot, "Killed by Servitors"
- **Jötun-Reader:** Uncommon loot, "Research notes"
- **Warrior:** Uncommon loot, "Combat gear"
- **Dvergr Tinkerer:** Rare loot, "Schematic fragment"

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

**Name Template:** `"{Modifier} Data-Slate"`

**Description Template:**

`"A {Modifier_Adj} Data-Slate {Modifier_Detail}. Display {Display_State}."`

**Content Type Variants:**

- **Personal_Log:** Daily log entry
- **Warning_Message:** Danger alert
- **Research_Notes:** Technical data (quest hook)
- **Distress_Signal:** Call for help (quest hook)
- **Corrupted_Fragment:** Glitched text (high lore value)

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

**Name Template:** `"{Modifier} Door"`

**Description Template:**

`"A {Modifier_Adj} door {Modifier_Detail}. Currently {State}."`

**Function Variants:**

- **Standard:** Unlocked, HP 50
- **Security:** Locked, HP 80, requires Keycard
- **Blast:** Locked, HP 150, requires Console Override
- **Rusted_Stuck:** Unlocked but requires MIGHT DC 15

---

## V. Database Schema

### Interactive Objects Table

```sql
CREATE TABLE IF NOT EXISTS Interactive_Objects (
    object_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    
    -- Descriptor reference
    composite_descriptor_id INTEGER,
    
    -- Object identity
    object_name TEXT NOT NULL,
    description TEXT NOT NULL,
    object_type TEXT NOT NULL,
    
    -- Interaction mechanics
    interaction_type TEXT NOT NULL,
    requires_check BOOLEAN DEFAULT 0,
    check_type TEXT,
    check_dc INTEGER,
    
    -- State
    current_state TEXT,
    interacted BOOLEAN DEFAULT 0,
    
    -- Loot reference
    loot_table_id INTEGER,
    
    -- Consequence
    consequence_type TEXT,
    consequence_data TEXT,  -- JSON
    
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id),
    FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id)
);

CREATE TABLE IF NOT EXISTS Object_Interactions (
    interaction_id INTEGER PRIMARY KEY AUTOINCREMENT,
    object_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    interaction_timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    
    check_required BOOLEAN,
    check_roll TEXT,
    check_success BOOLEAN,
    consequence_executed TEXT,
    
    FOREIGN KEY (object_id) REFERENCES Interactive_Objects(object_id),
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);
```

### Insert Base Templates SQL

```sql
-- LEVER BASE
INSERT INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Lever_Base', 'Object', 'Mechanism',
    '{"interaction_type": "Pull", "requires_check": false, "states": ["Up", "Down"], "reversible": true}',
    '{Modifier} Lever',
    'A {Modifier_Adj} lever {Modifier_Detail}. Currently in {State} position.',
    '["Mechanism", "Simple", "Puzzle"]'
);

-- CONSOLE BASE
INSERT INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Console_Base', 'Object', 'Mechanism',
    '{"interaction_type": "Hack", "requires_check": true, "check_type": "WITS", "check_dc": 15}',
    '{Modifier} Control Console',
    'A {Modifier_Adj} control console {Modifier_Detail}. Display {Display_State}.',
    '["Mechanism", "Hacking", "Technical"]'
);

-- Continue for remaining 7 templates...
```

---

## VI. Interaction Mechanics

### Skill Check Resolution

```csharp
public class ObjectInteractionService
{
    public InteractionResult AttemptInteraction(
        Character character,
        InteractiveObject obj)
    {
        // Check if already interacted (one-time objects)
        if (obj.Interacted && !obj.Reversible)
        {
            return InteractionResult.AlreadyUsed(
                $"The {[obj.Name](http://obj.Name)} has already been used.");
        }
        
        // Simple interaction (no check)
        if (!obj.RequiresCheck)
        {
            return ExecuteInteraction(character, obj);
        }
        
        // Skill check required
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
    
    private InteractionResult ExecuteInteraction(
        Character character,
        InteractiveObject obj)
    {
        obj.Interacted = true;
        var consequence = ExecuteConsequence(character, obj);
        
        // Update state if reversible
        if (obj.Reversible && obj.CurrentState != null)
        {
            obj.CurrentState = ToggleState(obj.CurrentState, obj.States);
        }
        
        return InteractionResult.Success(
            $"You {obj.InteractionType.ToLower()} the {[obj.Name](http://obj.Name)}. {consequence.Description}",
            consequence);
    }
}
```

### Consequence Types

**1. Unlock/Open**

- Door opens
- Container accessible
- Passage revealed

**2. Trigger Mechanism**

- Execute lever consequence
- Run console command
- Fire trap

**3. Spawn**

- Enemies spawn
- NPC appears
- Event triggers

**4. Reveal Information**

- Display data slate text
- Show quest hook
- Update knowledge

**5. Grant Loot**

- Add items to inventory
- Currency reward
- Quest item obtained

---

*Page will be continued with integration and examples...*

## VII. Integration with v0.37.1 Navigation Commands

### Command: `investigate [object]`

```csharp
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
        
        var result = _objectService.AttemptInteraction(character, obj);
        return CommandResult.FromInteractionResult(result);
    }
}
```

### Command: `pull [object]` / `search [object]` / `read [object]`

All follow same pattern: resolve to appropriate InteractiveObject and call AttemptInteraction().

---

## VIII. Success Criteria

**v0.38.3 is DONE when:**

### Base Templates

- [ ]  9 object base templates in Descriptor_Base_Templates
- [ ]  All 4 core types covered (Mechanism, Container, Investigatable, Barrier)
- [ ]  Base mechanics JSON valid
- [ ]  Function variants documented

### Object Descriptors

- [ ]  15+ descriptor combinations
- [ ]  Interaction types defined
- [ ]  Skill check types mapped
- [ ]  Consequence types enumerated

### Composite Generation

- [ ]  30+ composites auto-generated
- [ ]  Base + modifier + function variants
- [ ]  States handled correctly
- [ ]  Consequences mapped properly

### Service Implementation

- [ ]  ObjectInteractionService complete
- [ ]  GenerateObject() functional
- [ ]  AttemptInteraction() functional
- [ ]  Skill check integration working
- [ ]  Consequence execution logic complete

### Database

- [ ]  Interactive_Objects table created
- [ ]  Object_Interactions log table created
- [ ]  State persistence working
- [ ]  Foreign keys valid

### Integration

- [ ]  v0.37.1 commands use service
- [ ]  investigate/pull/search/read commands work
- [ ]  Skill checks resolve correctly
- [ ]  Consequences execute properly

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Interaction flow tests
- [ ]  Skill check resolution tests
- [ ]  Consequence execution tests

---

## IX. Implementation Roadmap

**Phase 1: Database Schema** — 2 hours

- Object base templates (9)
- Interactive_Objects table
- Object_Interactions log

**Phase 2: Service Implementation** — 3 hours

- ObjectInteractionService
- Skill check integration
- Consequence execution

**Phase 3: Object Generation** — 2 hours

- Template processing
- Function variant application
- State management

**Phase 4: Integration** — 2 hours

- Update v0.37.1 commands
- Test interaction flows
- Verify consequences

**Phase 5: Testing** — 1 hour

- Unit tests
- Integration tests

**Total: 10 hours**

---

## X. Example Generated Objects

### Example 1: Corroded Door Lever

**Base:** Lever_Base

**Modifier:** [Rusted]

**Function:** Door_Control

**Generated:**

```json
{
  "name": "Corroded Door Lever",
  "description": "A corroded lever shows centuries of oxidation. Currently in Up position.",
  "interaction_type": "Pull",
  "requires_check": false,
  "consequence_type": "Unlock",
  "consequence_data": {"target": "nearby_door", "action": "open"}
}
```

**Interaction:**

> **Player:** `pull lever`
> 

> 
> 

> **System:** You pull the lever. It groans but moves to Down position. You hear a door mechanism unlock nearby.
> 

---

### Example 2: Crystalline Control Console

**Base:** Console_Base

**Modifier:** [Crystalline]

**Function:** Hazard_Control

**Generated:**

```json
{
  "name": "Crystalline Control Console",
  "description": "A crystalline console pulses with unstable energy. Display flickers.",
  "interaction_type": "Hack",
  "requires_check": true,
  "check_type": "WITS",
  "check_dc": 15
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

> Success! You access the hazard control. The steam vents shut down with a hiss.
> 

---

### Example 3: Scavenger Corpse

**Base:** Corpse_Base

**Entity:** Scavenger

**Generated:**

```json
{
  "name": "Scavenger Corpse",
  "description": "Corpse of a scavenger. Recently deceased. Puncture wounds suggest Servitors.",
  "interaction_type": "Search",
  "loot_tier": "Common",
  "narrative_clue": "Pack contains hastily scrawled map fragment"
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

> The pack contains a map fragment marking a location deeper in the ruins.
> 

---

**v0.38.3 Complete.**