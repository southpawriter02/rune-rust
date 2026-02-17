# Physical Interaction Commands GUI Specification

## Version 1.0 - Comprehensive Physical Interaction Interface Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Target Framework:** Avalonia UI 11.x with ReactiveUI
**Architecture:** MVVM Pattern with Controllers
**Specification ID:** SPEC-WORLD-001
**Core Dependencies:** `RuneAndRust.Core.Descriptors`, `RuneAndRust.Engine.ObjectInteractionService`

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2024-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [ ]  **Review**: Ready for stakeholder review
- [ ]  **Approved**: Approved for implementation
- [ ]  **Active**: Currently implemented and maintained

---

## Table of Contents

1. [Overview](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
2. [Design Philosophy](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
3. [Command Lexicon](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
4. [Interaction Types & Mechanics](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
5. [Skill Check System](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
6. [Action Economy](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
7. [Fumble & Consequence System](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
8. [GUI Integration](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
9. [Smart Commands Panel](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
10. [Feedback & Combat Log](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
11. [Services & Controllers](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)
12. [Implementation Roadmap](Physical%20Interaction%20Commands%20GUI%20Specification%202ba55eb312da80c5ae5fc0cf7a97e7a0.md)

---

## 1. Overview

### 1.1 Purpose

This specification defines the GUI implementation and system mechanics for the Physical Interaction Commands in Rune & Rust. Physical interactions (pull, push, turn, press) represent a character performing direct, coherent physical acts upon environmental objects within the ruins of Aethelgard.

This system extends beyond simple "use" commands to provide specific, intuitive, and immersive ways for Survivors to manipulate the world. To **pull** a lever is a fundamentally different action than to **use** a lever, creating a more tactile and satisfying gameplay experience.

### 1.2 Core System Summary

The physical interaction system builds upon the existing InteractiveObject framework:

| Feature | Current Implementation | Extended Implementation |
| --- | --- | --- |
| Interaction Types | Pull, Open, Search, Read, Hack, Examine | Pull, Push, Turn, Press, Open, Search, Read, Hack, Examine |
| Skill Check Types | WITS, MIGHT, Lockpicking, Hacking | WITS, MIGHT, FINESSE, Lockpicking, Hacking, SystemBypass |
| Object Types | Mechanism, Container, Investigatable, Barrier | No change |
| Consequence Types | Unlock, Trigger, Spawn, Reveal, Loot, Trap | No change + Fumble |

### 1.3 Design Philosophy

- **Tactile Specificity**: Distinct verbs for distinct physical actions create immersion
- **Attribute Expression**: MIGHT characters excel at force-based interactions; FINESSE characters at precision tasks
- **Multiple Solutions**: Adepts (Scrap-Tinkers) can use System Bypass as an alternative to brute force
- **Meaningful Consequences**: Fumbles create emergent narrative through permanent environmental changes
- **Visual Feedback**: Clear GUI indicators for interaction type, skill requirements, and outcomes

### 1.4 Visual Design

| Element | Specification |
| --- | --- |
| Interaction Button Colors | Pull: Orange (#FF8C00), Push: Blue (#4169E1), Turn: Green (#32CD32), Press: Yellow (#FFD700) |
| Skill Indicator | MIGHT: Red (#DC143C), FINESSE: Cyan (#00CED1), WITS: Purple (#9400D3) |
| Success Feedback | Green text (#4CAF50) with checkmark icon |
| Failure Feedback | Gray text (#808080) with X icon |
| Fumble Warning | Red text (#DC143C) with hazard icon |

---

## 2. Design Philosophy

### 2.1 Design Pillars

1. **The Coherent Physical Act**
    - **Rationale**: Physical interactions should feel tangible and distinct, creating a bridge between character capabilities and environmental hardware
    - **Examples**: Pulling a rusted lever requires strength; pressing a button sequence requires precision
2. **Attribute-Based Problem Solving**
    - **Rationale**: Different character builds should have different approaches to environmental puzzles
    - **Examples**: MIGHT characters force mechanisms; FINESSE characters find precise solutions; Adepts bypass systems entirely
3. **Emergent Consequences**
    - **Rationale**: Failed interactions should create lasting narrative moments, not just retry opportunities
    - **Examples**: A broken lever becomes a permanent environmental change; a sprung trap remains active

### 2.2 Player Experience Goals

**Target Experience**: Players should feel physically present in the ruins, making meaningful choices about how to interact with ancient mechanisms.

**Moment-to-Moment Gameplay**:

- Examine objects to discover interaction hints
- Choose the appropriate physical action
- Execute skill checks when required
- Experience consequences (success, failure, or fumble)

**Learning Curve**:

- **Novice** (0-2 hours): Learn basic pull/push/turn/press commands through tutorial mechanisms
- **Intermediate** (2-10 hours): Understand when to use MIGHT vs FINESSE approaches
- **Expert** (10+ hours): Optimize character builds for puzzle-solving; know when to avoid risky interactions

### 2.3 Design Constraints

- **Technical**: Must integrate with existing `InteractiveObject` model and `ObjectInteractionService`
- **Gameplay**: Interactions must respect action economy in both exploration and combat
- **Narrative**: All mechanisms must feel consistent with Aethelgard's techno-arcane aesthetic
- **Scope**: Four core physical verbs (pull, push, turn, press) plus existing interaction types

---

## 3. Command Lexicon

### 3.1 Primary Commands

Each physical interaction verb has a clear and distinct purpose:

| Command | Concept | Physical Force | Primary Use | Object Types |
| --- | --- | --- | --- | --- |
| **pull** | Apply tensile force | Toward self | Levers, chains, ropes, stuck drawers | Mechanism |
| **push** | Apply compressive force | Away from self | Crates, buttons, loose walls, heavy objects | Mechanism, Barrier |
| **turn** | Apply rotational force | Clockwise/Counter | Valves, wheels, rusted gears, combination dials | Mechanism |
| **press** | Apply precise direct force | Targeted pressure | Buttons, runes, loose bricks, control panels | Mechanism |

### 3.2 Command Syntax

**Primary Syntax:**

```
[verb] [target_object]

```

**Examples:**

```
pull lever
push crate
turn valve
press button_alpha

```

**Target Resolution:**

- Match against `InteractiveObject.Name` (case-insensitive)
- Match against `InteractiveObject.ObjectType` as fallback
- Match against `InteractiveObject.BaseTemplateName` for template-based objects

### 3.3 Command Aliasing

The `use` command contextually aliases to the appropriate specific verb:

| User Input | Parsed As | Condition |
| --- | --- | --- |
| `use lever` | `pull lever` | Object.InteractionType == Pull |
| `use button` | `press button` | Object.InteractionType == Press |
| `use valve` | `turn valve` | Object.InteractionType == Turn |
| `use crate` | `push crate` | Object.InteractionType == Push |

---

## 4. Interaction Types & Mechanics

### 4.1 Extended InteractionType Enum

The existing `InteractionType` enum requires extension:

```csharp
public enum InteractionType
{
    // Physical Interactions
    Pull,       // Tensile force (existing)
    Push,       // Compressive force (NEW)
    Turn,       // Rotational force (NEW)
    Press,      // Precise pressure (NEW)

    // Existing Types
    Open,       // Open/close barriers
    Search,     // Search containers
    Read,       // Read data slates
    Hack,       // Hack terminals
    Automatic,  // Pressure plates
    Examine     // Investigate objects
}

```

### 4.2 Pull Interaction

**Concept**: Apply tensile force to an object.

**Primary Use**: Levers, chains, ropes, stuck drawers, handles.

**Default Skill Check**: MIGHT (for heavy/rusted objects)

**State Transitions**:

```
[Up] <--pull--> [Down]
[Extended] <--pull--> [Retracted]
[Closed] --pull--> [Open]

```

**Example Objects**:

| Object | DC | Check Type | Consequence |
| --- | --- | --- | --- |
| Standard Lever | 0 | None | Trigger mechanism |
| Rusted Lever | 4 | MIGHT | Trigger mechanism |
| Corroded Chain | 5 | MIGHT | Open gate |
| Stuck Drawer | 3 | MIGHT | Reveal loot |

### 4.3 Push Interaction

**Concept**: Apply compressive force to an object.

**Primary Use**: Crates, buttons, loose walls, heavy objects, obstacles.

**Default Skill Check**: MIGHT (for heavy objects)

**State Transitions**:

```
[Position A] --push--> [Position B]
[Standing] --push--> [Fallen]
[Sealed] --push--> [Open]

```

**Example Objects**:

| Object | DC | Check Type | Consequence |
| --- | --- | --- | --- |
| Light Crate | 0 | None | Move onto pressure plate |
| Heavy Crate | 3 | MIGHT | Move onto pressure plate |
| Loose Wall Section | 5 | MIGHT | Reveal passage |
| Debris Pile | 4 | MIGHT | Clear obstruction |

### 4.4 Turn Interaction

**Concept**: Apply rotational force to an object.

**Primary Use**: Valves, wheels, rusted gears, combination dials.

**Default Skill Check**: MIGHT (for stuck mechanisms), FINESSE (for precise combinations)

**State Transitions**:

```
[Open] <--turn--> [Closed]
[Position 0] --turn--> [Position 1] --turn--> [Position 2]

```

**Example Objects**:

| Object | DC | Check Type | Consequence |
| --- | --- | --- | --- |
| Standard Valve | 0 | None | Control flow |
| Rusted Valve | 4 | MIGHT | Shut off hazard |
| Combination Dial | 0 | None (logic puzzle) | Unlock |
| Precision Gear | 3 | FINESSE | Align mechanism |

### 4.5 Press Interaction

**Concept**: Apply precise, direct force to a small area.

**Primary Use**: Buttons, runes, loose bricks, control panels, keypads.

**Default Skill Check**: FINESSE (for sequences), None (for single presses)

**State Transitions**:

```
[Unpressed] --press--> [Pressed] --timeout--> [Unpressed]
[Inactive] --press correct sequence--> [Active]

```

**Example Objects**:

| Object | DC | Check Type | Consequence |
| --- | --- | --- | --- |
| Single Button | 0 | None | Toggle mechanism |
| Button Sequence | 0 | None (logic puzzle) | Unlock door |
| Pressure Rune | 0 | None | Activate ward |
| Hidden Brick | 2 | WITS (to find) | Reveal cache |

---

## 5. Skill Check System

### 5.1 Extended SkillCheckType Enum

```csharp
public enum SkillCheckType
{
    None,           // No check required
    WITS,           // Intelligence, puzzles, searching
    MIGHT,          // Physical power, force
    FINESSE,        // Precision, agility (NEW)
    Lockpicking,    // Locked containers/doors
    Hacking,        // Consoles, terminals
    SystemBypass    // Adept alternative (NEW)
}

```

### 5.2 Check Resolution

**Formula**:

```
NetSuccesses = Roll(AttributeDice) vs DC

```

Where:

- `AttributeDice` = Character's relevant attribute (MIGHT, FINESSE, or WITS)
- `DC` = Object's `CheckDC` property

**Outcome Determination**:

| Net Successes | Outcome | Description |
| --- | --- | --- |
| < -2 | Fumble | Critical failure with permanent consequences |
| -2 to 0 | Failure | Action fails, may retry |
| 1+ | Success | Action succeeds |
| 3+ | Critical Success | Bonus effect or reduced resource cost |

### 5.3 Alternative Approaches

**System Bypass (Adept Alternative)**:
Scrap-Tinker and similar specializations can use `SystemBypass` skill instead of MIGHT for mechanical interactions:

| Scenario | Standard Approach | Adept Alternative |
| --- | --- | --- |
| Rusted Lever | MIGHT DC 4 | SystemBypass DC 3 (with tools) |
| Stuck Valve | MIGHT DC 4 | SystemBypass DC 3 (with lubricant) |
| Heavy Crate | MIGHT DC 3 | No alternative (requires force) |

**Narrative Benefit**: Multiple valid solutions reward different character builds.

### 5.4 Skill Check Integration

**Properties on InteractiveObject**:

```csharp
public bool RequiresCheck { get; set; }
public SkillCheckType CheckType { get; set; }
public int CheckDC { get; set; }
public SkillCheckType? AlternativeCheckType { get; set; }  // NEW
public int? AlternativeCheckDC { get; set; }                // NEW

```

---

## 6. Action Economy

### 6.1 Exploration Mode

**Standard Action**: Physical interactions outside combat consume time on the World Clock.

| Interaction Type | Time Cost | World Clock Advance |
| --- | --- | --- |
| Simple (no check) | Minor | 1 minute |
| Standard (skill check) | Standard | 2 minutes |
| Complex (multi-step) | Extended | 5 minutes |
| Failed Attempt | Standard | 2 minutes |
| Fumble | Extended | 5 minutes + consequences |

### 6.2 Combat Mode

**Combat Actions**: Physical interactions during combat consume action economy.

| Action Type | Interaction Examples |
| --- | --- |
| **Standard Action** | Pull lever, push crate, turn valve |
| **Bonus Action** | Press single button (if adjacent) |
| **Free Action** | Automatic triggers (pressure plates) |

**Tactical Applications**:

- Pull a lever to drop a cage on enemies
- Push a crate to create cover
- Turn a valve to release steam hazard
- Press a button to seal a door

### 6.3 Combat Integration

**Combat-Relevant Properties on InteractiveObject**:

```csharp
public bool IsCombatUsable { get; set; }           // NEW
public ActionType CombatActionCost { get; set; }   // NEW: Standard, Bonus, Free
public bool RequiresAdjacency { get; set; }        // NEW

```

---

## 7. Fumble & Consequence System

### 7.1 Fumble Triggers

A **Fumble** occurs when a skill check results in Net Successes <= -2.

**Fumble Probability by DC**:

| Character Attribute | DC 3 | DC 4 | DC 5 | DC 6 |
| --- | --- | --- | --- | --- |
| 3 (Low) | 15% | 20% | 25% | 30% |
| 5 (Average) | 8% | 12% | 16% | 20% |
| 7 (High) | 3% | 6% | 10% | 14% |
| 9 (Exceptional) | 1% | 3% | 5% | 8% |

### 7.2 Fumble Consequences by Interaction Type

| Interaction | Fumble Consequence | Permanence |
| --- | --- | --- |
| **Pull** | Object breaks off, becomes [Broken] and unusable | Permanent |
| **Push** | Character loses footing, becomes [Staggered] (1 round in combat) | Temporary |
| **Turn** | Valve/gear shears off, mechanism permanently disabled | Permanent |
| **Press** | Wrong button triggered, alarm sounded or trap activated | Varies |

### 7.3 Fumble Data Model

**Extension to InteractiveObject**:

```csharp
public bool CanFumble { get; set; }                    // NEW
public string? FumbleConsequenceScript { get; set; }   // NEW
public FumbleType FumbleType { get; set; }             // NEW

```

**FumbleType Enum**:

```csharp
public enum FumbleType
{
    None,               // No fumble possible
    BreakObject,        // Object becomes unusable
    TriggerTrap,        // Activates trap
    AlertEnemies,       // Spawns/alerts enemies
    ApplyStatus,        // Applies status to character
    EnvironmentalChange // Permanent room modification
}

```

### 7.4 Emergent Narrative

Fumble consequences create lasting stories:

- A broken lever that the party must now find an alternative around
- A permanently active hazard that changes room navigation
- An alerted enemy patrol that creates ongoing tension

---

## 8. GUI Integration

### 8.1 Dungeon Exploration View Extensions

**ViewModel:** `DungeonExplorationViewModel.cs`

**New/Extended Properties**:

| Property | Type | Description |
| --- | --- | --- |
| `InteractiveObjects` | ObservableCollection<InteractiveObjectViewModel> | Physical interaction targets |
| `SelectedObject` | InteractiveObjectViewModel? | Currently selected object |
| `CanInteract` | bool | Whether interaction is available |
| `InteractionHint` | string | Suggested command for selected object |
| `HasPhysicalObjects` | bool | Whether room has physical interaction objects |

### 8.2 InteractiveObjectViewModel

**New ViewModel for physical interactions**:

```csharp
public class InteractiveObjectViewModel : ViewModelBase
{
    // Identity
    public int ObjectId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Interaction Info
    public InteractionType InteractionType { get; set; }
    public string InteractionVerb { get; set; }        // "Pull", "Push", etc.
    public string InteractionIcon { get; set; }        // Icon for verb
    public string InteractionColor { get; set; }       // Color for verb

    // Skill Check Info
    public bool RequiresCheck { get; set; }
    public SkillCheckType CheckType { get; set; }
    public int CheckDC { get; set; }
    public string CheckDisplay { get; set; }           // "MIGHT DC 4"
    public string CheckColor { get; set; }             // Attribute color
    public bool HasAlternative { get; set; }
    public string? AlternativeDisplay { get; set; }    // "or SystemBypass DC 3"

    // State Info
    public string CurrentState { get; set; }
    public bool CanInteract { get; set; }
    public bool IsInteracted { get; set; }
    public bool IsReversible { get; set; }
    public int AttemptsRemaining { get; set; }

    // Combat Info
    public bool IsCombatUsable { get; set; }
    public string CombatActionCost { get; set; }       // "Standard Action"

    // Visual
    public bool IsSelected { get; set; }
    public bool IsHighlighted { get; set; }
    public string StateIcon { get; set; }              // Up/Down, Open/Closed
}

```

### 8.3 Object Display in Room

**Layout Addition to Dungeon Exploration View**:

```
┌─────────────────────────────────────────────────────────────────┐
│ THE CONTROL CHAMBER                              [Map] [?]       │
├─────────────────────────────────────────────────────────────────┤
│ A dimly lit chamber filled with ancient machinery...            │
│                                                                  │
│ EXITS: [North] [East] [Down]                                    │
├─────────────────────────────────────────────────────────────────┤
│ INTERACTIVE OBJECTS:                                             │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ⚙ [Rusted Lever] - Pull (MIGHT DC 4)              [Interact]│ │
│ │   State: Up | "A corroded iron lever set into the wall..."  │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ 🔘 [Control Panel] - Press                        [Interact]│ │
│ │   State: Inactive | "A panel with four buttons..."          │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ 🔧 [Steam Valve] - Turn (MIGHT DC 4)              [Interact]│ │
│ │   State: Open | "A brass valve controlling steam flow..."   │ │
│ └─────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│ [Search] [Look] [Investigate] [Rest] [Character] [Inventory]    │
└─────────────────────────────────────────────────────────────────┘

```

### 8.4 Interaction Icons

| InteractionType | Icon | Unicode |
| --- | --- | --- |
| Pull | ⚙ | U+2699 |
| Push | ⬛ | U+2B1B |
| Turn | 🔧 | U+1F527 |
| Press | 🔘 | U+1F518 |
| Open | 🚪 | U+1F6AA |
| Search | 🔍 | U+1F50D |
| Read | 📜 | U+1F4DC |
| Hack | 💻 | U+1F4BB |

---

## 9. Smart Commands Panel

### 9.1 Purpose

The Smart Commands Panel provides contextual interaction suggestions based on nearby objects and player position.

### 9.2 Panel Layout

```
┌─────────────────────────────┐
│ QUICK ACTIONS               │
├─────────────────────────────┤
│ [1] pull lever (MIGHT)      │
│ [2] press button            │
│ [3] turn valve (MIGHT)      │
│ [4] search corpse           │
└─────────────────────────────┘

```

### 9.3 SmartCommandsViewModel

```csharp
public class SmartCommandsViewModel : ViewModelBase
{
    public ObservableCollection<SmartCommandViewModel> Commands { get; }
    public bool HasCommands => Commands.Any();
}

public class SmartCommandViewModel : ViewModelBase
{
    public int Hotkey { get; set; }           // 1-9
    public string CommandText { get; set; }   // "pull lever"
    public string SkillHint { get; set; }     // "(MIGHT)" or ""
    public string SkillColor { get; set; }    // Attribute color
    public ICommand ExecuteCommand { get; }
}

```

### 9.4 Command Generation Logic

Smart Commands are generated based on:

1. Objects in `CurrentRoom.InteractiveObjects`
2. Objects with `CanInteract() == true`
3. Priority: Physical interactions first, then search/investigate
4. Maximum 9 commands displayed (hotkeys 1-9)

---

## 10. Feedback & Combat Log

### 10.1 Success Feedback

**Format**:

```
[green]With a mighty heave, you wrench the rusted lever down. A loud grinding
sound echoes from the far side of the room as a stone gate begins to rise.[/green]

```

**Structure**:

1. Action description with character agency
2. Environmental feedback (sounds, visual changes)
3. Consequence description

### 10.2 Failure Feedback

**Format**:

```
[gray]You strain against the lever, but it refuses to budge. The rust has
fused the mechanism solid. Perhaps with more strength...[/gray]

```

**Structure**:

1. Attempted action description
2. Reason for failure
3. Hint for alternative approach

### 10.3 Fumble Feedback

**Format**:

```
[red]The lever snaps off in your hands with a sharp crack! The mechanism
is now permanently disabled. You'll need to find another way.[/red]

```

**Structure**:

1. Dramatic failure description
2. Permanent consequence statement
3. Guidance for player

### 10.4 Combat Log Integration

| Event Type | Log Format | Color |
| --- | --- | --- |
| Interaction Attempt | "[Name] attempts to pull the lever..." | White |
| Skill Check | "Rolling MIGHT (5d10) vs DC 4..." | Cyan |
| Success | "Success! The lever moves with a satisfying clunk." | Green |
| Failure | "The lever holds firm against your efforts." | Gray |
| Fumble | "FUMBLE! The lever breaks off in your hands!" | Red |
| Environmental | "The gate begins to rise..." | Yellow |

### 10.5 InteractionFeedbackViewModel

```csharp
public class InteractionFeedbackViewModel : ViewModelBase
{
    public string Description { get; set; }
    public string FeedbackColor { get; set; }
    public InteractionOutcome Outcome { get; set; }
    public string? SkillCheckDisplay { get; set; }
    public string? ConsequenceDisplay { get; set; }
    public bool ShowSkillCheck { get; set; }
    public bool ShowConsequence { get; set; }
}

public enum InteractionOutcome
{
    Success,
    CriticalSuccess,
    Failure,
    Fumble
}

```

---

## 11. Services & Controllers

### 11.1 PhysicalInteractionController

**Responsibilities**:

| Method | Description |
| --- | --- |
| `AttemptInteraction(objectId, verb)` | Execute physical interaction |
| `GetInteractionOptions(room)` | Get available interactions for Smart Commands |
| `ValidateInteraction(object, verb)` | Check if verb matches object type |
| `ResolveSkillCheck(character, checkType, dc)` | Perform skill check |
| `ApplyConsequence(result, object)` | Apply success/failure/fumble consequences |
| `GenerateFeedback(result)` | Create narrative feedback |

### 11.2 Extended ObjectInteractionService

**New Methods**:

```csharp
public class ObjectInteractionService
{
    // Existing
    public InteractionResult AttemptInteraction(InteractiveObject obj, int characterId);

    // New
    public InteractionResult AttemptPhysicalInteraction(
        InteractiveObject obj,
        int characterId,
        InteractionType verb);

    public bool CanUseAlternativeSkill(
        InteractiveObject obj,
        PlayerCharacter character);

    public FumbleResult ResolveFumble(
        InteractiveObject obj,
        PlayerCharacter character);

    public List<InteractiveObject> GetPhysicalObjects(Room room);
}

```

### 11.3 Service Dependencies

| Service | Responsibility |
| --- | --- |
| `IObjectInteractionService` | Core interaction resolution |
| `IDiceService` | Skill check dice rolling |
| `ISkillUsageFlavorTextService` | Narrative feedback generation |
| `ICombatLogService` | Combat log entries |
| `IWorldClockService` | Time advancement |
| `INavigationService` | View updates |

### 11.4 Events

| Event | Trigger | Handler |
| --- | --- | --- |
| `InteractionAttempted` | Player initiates interaction | Log attempt |
| `InteractionResolved` | Skill check completed | Show feedback |
| `ObjectStateChanged` | Object state changes | Update room display |
| `FumbleOccurred` | Fumble consequence applied | Show warning, update room |
| `EnvironmentModified` | Room permanently changed | Refresh room description |

---

## 12. Implementation Roadmap

### 12.1 Phase 1: Core Extensions (Priority: Critical)

| Task | Description | Complexity |
| --- | --- | --- |
| Extend `InteractionType` enum | Add Push, Turn, Press | Low |
| Extend `SkillCheckType` enum | Add FINESSE, SystemBypass | Low |
| Create `PhysicalInteractionCommand` variants | Push, Turn, Press commands | Medium |
| Extend `ObjectInteractionService` | Physical interaction logic | Medium |
| Add fumble resolution | Fumble detection and consequences | Medium |

### 12.2 Phase 2: GUI Integration (Priority: High)

| Task | Description | Complexity |
| --- | --- | --- |
| Create `InteractiveObjectViewModel` | Object display model | Low |
| Extend `DungeonExplorationViewModel` | Physical object list | Medium |
| Create object list panel | Room object display | Medium |
| Create `SmartCommandsViewModel` | Quick action panel | Medium |
| Add interaction feedback display | Success/failure/fumble UI | Medium |

### 12.3 Phase 3: Combat Integration (Priority: Medium)

| Task | Description | Complexity |
| --- | --- | --- |
| Add combat action properties | `IsCombatUsable`, `CombatActionCost` | Low |
| Integrate with `CombatViewModel` | Combat interaction panel | High |
| Add tactical positioning checks | Adjacency requirements | Medium |
| Combat log integration | Interaction events in combat | Low |

### 12.4 Phase 4: Polish (Priority: Low)

| Task | Description | Complexity |
| --- | --- | --- |
| Add interaction animations | Visual feedback | Medium |
| Add sound effects | Audio feedback | Low |
| Create tutorial mechanisms | Learning content | Medium |
| Balance skill DCs | Probability tuning | Medium |
| Add accessibility features | Screen reader, high contrast | Medium |

---

## Appendix A: Data Flow Diagram

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  User Input     │────▶│ PhysicalInteraction  │────▶│ Exploration     │
│  "pull lever"   │     │ Controller           │     │ ViewModel       │
└─────────────────┘     └──────────────────────┘     └─────────────────┘
                               │                            │
                               │                            ▼
                               │                    ┌─────────────────┐
                               │                    │ Interactive     │
                               │                    │ ObjectViewModel │
                               │                    └─────────────────┘
                               │                            │
                               ▼                            ▼
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ InteractiveObj  │◀────│ ObjectInteraction    │────▶│ Feedback        │
│ (Room data)     │     │ Service              │     │ Display         │
└─────────────────┘     └──────────────────────┘     └─────────────────┘
        │                       │                           │
        │                       ▼                           ▼
        │               ┌──────────────────────┐     ┌─────────────────┐
        │               │ DiceService          │     │ Combat Log      │
        │               │ (Skill Checks)       │     │ Service         │
        │               └──────────────────────┘     └─────────────────┘
        │                       │
        ▼                       ▼
┌─────────────────┐     ┌──────────────────────┐
│ Room State      │◀────│ SkillUsageFlavor     │
│ Update          │     │ TextService          │
└─────────────────┘     └──────────────────────┘

```

---

## Appendix B: Example Puzzles

### B.1 Hardware Malfunction Puzzle: The Steam Chamber

**Setup**:

- Room contains active `[High-Pressure Steam Vent]` hazard
- `[Rusted Valve]` (Turn, MIGHT DC 4) controls steam flow
- Alternative: Scrap-Tinker can use `SystemBypass DC 3` with tools

**Resolution Paths**:

1. **MIGHT Approach**: Turn valve with strength (standard)
2. **Adept Approach**: SystemBypass with tools (quiet, no fumble risk)
3. **Fumble**: Valve shears off, hazard permanent

### B.2 Logic Puzzle: The Keypad Sequence

**Setup**:

- Locked door with 4-button keypad
- `investigate` reveals clue: "The sequence follows the phases of creation"
- Buttons: Alpha (Fire), Beta (Water), Gamma (Earth), Delta (Air)

**Resolution**:

- Correct sequence: `press beta`, `press delta`, `press gamma`, `press alpha`
- No skill check (logic puzzle)
- Wrong sequence: Resets puzzle, may trigger alarm

### B.3 Environmental Puzzle: The Pressure Plate

**Setup**:

- `[Pressure Plate Trap]` blocks passage
- `[Heavy Crate]` nearby (Push, MIGHT DC 3)
- Alternative: Place any heavy item on plate

**Resolution**:

- `push crate` onto pressure plate
- Plate holds down, safe passage
- Fumble: Character stumbles onto plate, triggers trap

---

## Appendix C: Integration with Existing Systems

### C.1 Systemic Connections

| System | Integration Point |
| --- | --- |
| **Trauma Economy** | Fumbles may increase Psychic Stress |
| **Combat System** | Physical interactions as Standard Actions |
| **Quest System** | InteractObjective for quest completion |
| **Skill Check System** | MIGHT/FINESSE/WITS attribute rolls |
| **World Clock** | Time advancement for interactions |
| **Loot System** | Consequences may grant items |

### C.2 Attribute Value Expression

This system provides primary utility for the MIGHT attribute outside combat:

- High-MIGHT characters are the party's "crowbar"
- They force through obstacles that would stump finesse-oriented parties
- Creates meaningful build differentiation

### C.3 Adept Advantage

Adept specializations (Scrap-Tinker) gain alternative solution paths:

- Use intellect and tools instead of brute force
- Often quieter (no noise from forcing mechanisms)
- Sometimes lower DC when using proper tools
- Multiple valid solutions reward different builds

---

## Document History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2024 | Initial Physical Interaction GUI specification |

---

*This document is part of the Rune & Rust technical documentation suite.Related: GUI_SPECIFICATION.md (v0.45), QUEST_JOURNAL_SPECIFICATION.md (v1.0)Extends: InteractiveObject system (v0.38.3)*