# Smart Commands Panel GUI Specification

## Version 1.0 - Contextual Quick Actions Interface Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Target Framework:** Avalonia UI 11.x with ReactiveUI
**Architecture:** MVVM Pattern with Controllers
**Specification ID:** SPEC-WORLD-003
**Core Dependencies:** `RuneAndRust.DesktopUI.ViewModels`, `RuneAndRust.Core.Descriptors`

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

1. [Overview](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
2. [Design Philosophy](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
3. [Panel Layout & Structure](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
4. [Command Generation Logic](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
5. [Hotkey System](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
6. [Context Awareness](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
7. [Visual Design](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
8. [ViewModels & Data Binding](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
9. [Accessibility Features](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)
10. [Implementation Guidance](Smart%20Commands%20Panel%20GUI%20Specification%202ba55eb312da80f59c08e64a5a3dea95.md)

---

## 1. Overview

### 1.1 Purpose

The Smart Commands Panel provides players with contextual, one-click/one-key access to relevant actions based on their current situation. Rather than requiring players to type commands or navigate menus, the panel surfaces the most likely actions they want to perform.

This specification defines the GUI implementation, command generation logic, hotkey bindings, and context-awareness system for the Smart Commands Panel.

### 1.2 Scope

**In Scope**:

- Panel layout and visual design
- Command generation and prioritization algorithms
- Hotkey binding system (1-9 keys)
- Context detection (exploration, combat, dialogue)
- Skill check indicators and risk warnings
- Accessibility features (screen reader, high contrast)

**Out of Scope**:

- Command execution logic → Individual command specifications
- Skill check resolution → Skill Check System
- Fumble handling → SPEC-WORLD-002

### 1.3 Success Criteria

- **Player Experience**: 80%+ of desired actions available via Smart Commands
- **Technical**: Panel updates within 100ms of context change
- **Usability**: Players discover hotkey system within first 30 minutes
- **Accessibility**: Full keyboard navigation, screen reader compatible

---

## 2. Design Philosophy

### 2.1 Design Pillars

1. **Contextual Relevance**
    - **Rationale**: Show only actions that make sense in current situation
    - **Examples**: Show "pull lever" when near lever, not when in empty corridor
2. **Immediate Access**
    - **Rationale**: Reduce friction between intent and action
    - **Examples**: Press "1" to execute first suggested action
3. **Informed Decisions**
    - **Rationale**: Players should understand action requirements before committing
    - **Examples**: Show "(MIGHT DC 4)" next to risky actions
4. **Non-Intrusive Presence**
    - **Rationale**: Panel should help without dominating screen
    - **Examples**: Compact design, collapsible, semi-transparent

### 2.2 Player Experience Goals

**Target Experience**: Players should feel empowered by having relevant options at their fingertips, not overwhelmed by choices.

**Discoverability Flow**:

1. **First Encounter**: Panel appears with suggested actions
2. **Learning**: Player notices "[1]" prefix, tries pressing 1
3. **Mastery**: Player instinctively uses hotkeys for common actions
4. **Expertise**: Player glances at panel for situational awareness

### 2.3 Design Constraints

- **Maximum 9 commands**: Hotkeys 1-9 only
- **Single-line display**: Each command fits on one line
- **No nested menus**: All actions are immediate
- **Consistent position**: Always in same screen location
- **Responsive**: Updates immediately when context changes

---

## 3. Panel Layout & Structure

### 3.1 Panel Position

**Default Position**: Bottom-left of game view

**Position Options** (configurable):

| Position | Coordinates | Use Case |
| --- | --- | --- |
| Bottom-Left | (16, -16) from bottom-left | Default, exploration |
| Bottom-Right | (-16, -16) from bottom-right | Combat (near action bar) |
| Top-Right | (-16, 16) from top-right | Minimal UI mode |

### 3.2 Panel Layout

```
┌─────────────────────────────────────┐
│ QUICK ACTIONS                    [-]│
├─────────────────────────────────────┤
│ [1] pull lever (MIGHT DC 4)      ⚠ │
│ [2] press button                    │
│ [3] search corpse                   │
│ [4] investigate terminal (WITS)     │
│ [5] talk to merchant                │
├─────────────────────────────────────┤
│ [Tab] More actions...               │
└─────────────────────────────────────┘

```

### 3.3 Panel Elements

| Element | Type | Description |
| --- | --- | --- |
| Title Bar | Header | "QUICK ACTIONS" with collapse button |
| Command List | ListView | Scrollable list of commands |
| Hotkey Indicator | Label | "[1]" through "[9]" |
| Command Text | Label | Action description |
| Skill Indicator | Label | "(MIGHT DC 4)" or "(WITS)" |
| Risk Warning | Icon | "⚠" for high fumble chance |
| Overflow Indicator | Footer | "[Tab] More actions..." |

### 3.4 Panel States

| State | Appearance | Trigger |
| --- | --- | --- |
| **Expanded** | Full panel with all commands | Default |
| **Collapsed** | Title bar only | User clicks [-] |
| **Hidden** | Not visible | Combat animation, cutscene |
| **Highlighted** | Glow effect | Hotkey pressed |
| **Empty** | "No actions available" | No contextual commands |

### 3.5 Dimensions

| Property | Value | Notes |
| --- | --- | --- |
| Width | 280px | Fixed |
| Min Height | 60px | Title + 1 command |
| Max Height | 320px | Title + 9 commands + overflow |
| Command Height | 28px | Per command row |
| Padding | 8px | Internal padding |
| Margin | 16px | From screen edge |

---

## 4. Command Generation Logic

### 4.1 Command Sources

Commands are generated from multiple sources, prioritized:

| Priority | Source | Examples |
| --- | --- | --- |
| 1 (Highest) | Combat actions | Attack, Defend, Use Ability |
| 2 | Physical interactions | Pull lever, Push crate |
| 3 | Object interactions | Search, Read, Hack |
| 4 | NPC interactions | Talk, Trade |
| 5 | Navigation | Go north, Use stairs |
| 6 | Character actions | Rest, Inventory, Character |
| 7 (Lowest) | System actions | Save, Help |

### 4.2 Generation Algorithm

```csharp
public List<SmartCommand> GenerateCommands(GameContext context)
{
    var commands = new List<SmartCommand>();

    // 1. Get context-specific commands
    switch (context.CurrentMode)
    {
        case GameMode.Combat:
            commands.AddRange(GenerateCombatCommands(context));
            break;
        case GameMode.Exploration:
            commands.AddRange(GenerateExplorationCommands(context));
            break;
        case GameMode.Dialogue:
            commands.AddRange(GenerateDialogueCommands(context));
            break;
    }

    // 2. Sort by priority
    commands = commands.OrderBy(c => c.Priority).ToList();

    // 3. Limit to 9
    return commands.Take(9).ToList();
}

```

### 4.3 Exploration Command Generation

```csharp
private List<SmartCommand> GenerateExplorationCommands(GameContext context)
{
    var commands = new List<SmartCommand>();
    var room = context.CurrentRoom;

    // Physical interactions (highest priority)
    foreach (var obj in room.InteractiveObjects.Where(o => o.CanInteract()))
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"{obj.InteractionType.ToVerb()} {obj.Name}",
            TargetId = obj.ObjectId,
            Priority = 2,
            SkillCheck = obj.RequiresCheck ? obj.CheckType : null,
            DC = obj.CheckDC,
            FumbleRisk = CalculateFumbleRisk(context.Player, obj)
        });
    }

    // NPCs
    foreach (var npc in room.NPCs.Where(n => n.CanTalk))
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"talk to {npc.Name}",
            TargetId = npc.NpcId,
            Priority = 4
        });
    }

    // Navigation
    foreach (var exit in room.Exits)
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"go {exit.Direction}",
            TargetId = exit.TargetRoomId,
            Priority = 5
        });
    }

    // Standard actions
    if (room.CanSearch && !room.IsSearched)
    {
        commands.Add(new SmartCommand
        {
            CommandText = "search room",
            Priority = 3
        });
    }

    if (room.IsSanctuary || room.IsCleared)
    {
        commands.Add(new SmartCommand
        {
            CommandText = "rest",
            Priority = 6
        });
    }

    return commands;
}

```

### 4.4 Combat Command Generation

```csharp
private List<SmartCommand> GenerateCombatCommands(GameContext context)
{
    var commands = new List<SmartCommand>();
    var combat = context.CombatState;

    if (!combat.IsPlayerTurn) return commands;

    // Attack (if valid target)
    if (combat.HasValidTarget)
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"attack {combat.SelectedTarget.Name}",
            Priority = 1,
            IsCombatAction = true
        });
    }

    // Abilities (available and not on cooldown)
    foreach (var ability in context.Player.Abilities.Where(a => a.CanUse()))
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"use {ability.Name}",
            Priority = 1,
            IsCombatAction = true,
            ResourceCost = ability.StaminaCost
        });
    }

    // Environmental interactions
    foreach (var obj in combat.InteractableObjects)
    {
        commands.Add(new SmartCommand
        {
            CommandText = $"{obj.InteractionType.ToVerb()} {obj.Name}",
            Priority = 2,
            IsCombatAction = true,
            ActionCost = obj.CombatActionCost
        });
    }

    // Defensive actions
    commands.Add(new SmartCommand
    {
        CommandText = "defend",
        Priority = 3,
        IsCombatAction = true
    });

    commands.Add(new SmartCommand
    {
        CommandText = "end turn",
        Priority = 9,
        IsCombatAction = true
    });

    return commands;
}

```

### 4.5 Command Filtering

Commands are filtered based on:

| Filter | Condition | Effect |
| --- | --- | --- |
| Availability | `CanExecute()` returns true | Hide unavailable |
| Visibility | Object is visible to player | Hide hidden objects |
| Duplicate | Same action on same target | Remove duplicates |
| Relevance | Player has required items/skills | Deprioritize impossible |

---

## 5. Hotkey System

### 5.1 Hotkey Mapping

| Key | Command Index | Modifier |
| --- | --- | --- |
| 1 | First command | None |
| 2 | Second command | None |
| 3-9 | Third-ninth command | None |
| Tab | Show overflow menu | None |
| Shift+1-9 | Execute with confirmation | Risky actions |

### 5.2 Hotkey Behavior

**Standard Execution**:

1. Player presses hotkey (e.g., "1")
2. System identifies corresponding command
3. Command executes immediately
4. Panel updates with new context

**Risky Action Execution** (fumble chance > 20%):

1. Player presses hotkey
2. Confirmation dialog appears
3. Player confirms or cancels
4. On confirm, command executes

**Configurable Behaviors**:

| Setting | Default | Effect |
| --- | --- | --- |
| `HotkeyConfirmThreshold` | 0.20 | Fumble % requiring confirm |
| `HotkeyRepeatDelay` | 500ms | Delay before repeat |
| `HotkeyFeedbackDuration` | 200ms | Visual highlight time |

### 5.3 Hotkey Registration

```csharp
public void RegisterHotkeys(IKeyboardShortcutService shortcuts)
{
    for (int i = 1; i <= 9; i++)
    {
        int index = i - 1; // 0-based
        shortcuts.Register(
            key: (Key)((int)Key.D1 + index),
            modifiers: KeyModifiers.None,
            action: () => ExecuteCommand(index),
            context: "SmartCommands"
        );
    }

    shortcuts.Register(
        key: Key.Tab,
        modifiers: KeyModifiers.None,
        action: ShowOverflowMenu,
        context: "SmartCommands"
    );
}

```

### 5.4 Hotkey Visual Feedback

When a hotkey is pressed:

1. **Immediate** (0ms): Command row highlights
2. **Short** (100ms): Highlight pulses
3. **Complete** (200ms): Highlight fades, action executes
4. **Feedback** (300ms): Result shown in status/log

---

## 6. Context Awareness

### 6.1 Context Detection

The panel detects context from `GameStateController`:

```csharp
public class GameContext
{
    public GameMode CurrentMode { get; set; }
    public Room? CurrentRoom { get; set; }
    public CombatState? CombatState { get; set; }
    public DialogueState? DialogueState { get; set; }
    public PlayerCharacter Player { get; set; }
    public InteractiveObject? SelectedObject { get; set; }
    public NPC? SelectedNPC { get; set; }
}

public enum GameMode
{
    MainMenu,
    Exploration,
    Combat,
    Dialogue,
    Inventory,
    CharacterSheet,
    Cutscene
}

```

### 6.2 Context Change Events

| Event | Trigger | Panel Response |
| --- | --- | --- |
| `RoomChanged` | Player enters new room | Regenerate all commands |
| `ObjectInteracted` | Object state changed | Update affected commands |
| `CombatStarted` | Combat initiated | Switch to combat commands |
| `CombatEnded` | Combat resolved | Switch to exploration commands |
| `DialogueStarted` | NPC conversation begins | Show dialogue options |
| `TurnChanged` | Combat turn changes | Update if player turn |
| `TargetSelected` | Player selects target | Update attack commands |

### 6.3 Context-Specific Panels

**Exploration Panel**:

```
┌─────────────────────────────────────┐
│ QUICK ACTIONS                       │
├─────────────────────────────────────┤
│ [1] pull lever (MIGHT DC 4)         │
│ [2] search container                │
│ [3] talk to merchant                │
│ [4] go north                        │
│ [5] rest                            │
└─────────────────────────────────────┘

```

**Combat Panel**:

```
┌─────────────────────────────────────┐
│ COMBAT ACTIONS                      │
├─────────────────────────────────────┤
│ [1] attack Rust-Horror (12 dmg)     │
│ [2] Whirlwind Strike (15 Stam)      │
│ [3] pull lever [Standard Action]    │
│ [4] defend                          │
│ [5] use Health Draught              │
│ [6] end turn                        │
└─────────────────────────────────────┘

```

**Dialogue Panel**:

```
┌─────────────────────────────────────┐
│ DIALOGUE OPTIONS                    │
├─────────────────────────────────────┤
│ [1] "Tell me about the ruins."      │
│ [2] "I need supplies." [Trade]      │
│ [3] "Any work available?" [Quest]   │
│ [4] [Intimidate] "Lower your prices"│
│ [5] "Farewell."                     │
└─────────────────────────────────────┘

```

### 6.4 Proximity-Based Priority

Objects closer to player get higher priority:

```csharp
private int CalculateProximityPriority(InteractiveObject obj, PlayerPosition player)
{
    var distance = CalculateDistance(obj.Position, player.Position);

    return distance switch
    {
        <= 1 => 0,   // Adjacent: highest priority
        <= 3 => 1,   // Near: high priority
        <= 5 => 2,   // Medium: normal priority
        _ => 3       // Far: low priority
    };
}

```

---

## 7. Visual Design

### 7.1 Color Scheme

| Element | Color | Hex | Usage |
| --- | --- | --- | --- |
| Panel Background | Dark Gray | #1C1C1C | Panel body |
| Panel Border | Medium Gray | #3A3A3A | Panel outline |
| Title Text | White | #FFFFFF | "QUICK ACTIONS" |
| Command Text | Light Gray | #E0E0E0 | Action names |
| Hotkey Badge | Accent Blue | #4A90E2 | "[1]" indicators |
| MIGHT Indicator | Red | #DC143C | MIGHT checks |
| FINESSE Indicator | Cyan | #00CED1 | FINESSE checks |
| WITS Indicator | Purple | #9400D3 | WITS checks |
| Risk Warning | Orange | #FF8C00 | High fumble risk |
| Selected Highlight | Blue Glow | #4A90E280 | Hovered/selected |

### 7.2 Typography

| Element | Font | Size | Weight |
| --- | --- | --- | --- |
| Title | System | 14px | Bold |
| Hotkey | Monospace | 12px | Bold |
| Command | System | 13px | Regular |
| Skill Indicator | System | 11px | Regular |
| Overflow | System | 11px | Italic |

### 7.3 Icons

| Icon | Unicode | Meaning |
| --- | --- | --- |
| ⚠ | U+26A0 | High fumble risk (>20%) |
| ⚔ | U+2694 | Combat action |
| 🔒 | U+1F512 | Requires item/key |
| ⚡ | U+26A1 | Costs Stamina |
| 🕐 | U+1F550 | Costs time |
| ✓ | U+2713 | Already completed |

### 7.4 Animation

| Animation | Duration | Easing | Trigger |
| --- | --- | --- | --- |
| Panel Slide In | 200ms | EaseOut | Panel appears |
| Panel Slide Out | 150ms | EaseIn | Panel hides |
| Command Highlight | 200ms | EaseInOut | Hotkey pressed |
| Command Add | 150ms | EaseOut | New command appears |
| Command Remove | 100ms | EaseIn | Command leaves |
| Risk Pulse | 1000ms | Sine (loop) | High risk command |

---

## 8. ViewModels & Data Binding

### 8.1 SmartCommandsPanelViewModel

```csharp
public class SmartCommandsPanelViewModel : ViewModelBase
{
    private readonly IGameStateController _gameState;
    private readonly ICommandDispatcher _dispatcher;
    private ObservableCollection<SmartCommandViewModel> _commands;
    private bool _isExpanded = true;
    private bool _isVisible = true;
    private string _title = "QUICK ACTIONS";

    #region Properties

    /// <summary>
    /// Collection of available smart commands
    /// </summary>
    public ObservableCollection<SmartCommandViewModel> Commands
    {
        get => _commands;
        private set => this.RaiseAndSetIfChanged(ref _commands, value);
    }

    /// <summary>
    /// Whether panel is expanded (showing commands)
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>
    /// Whether panel is visible at all
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    /// <summary>
    /// Panel title (changes based on context)
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Whether there are commands to display
    /// </summary>
    public bool HasCommands => Commands?.Any() == true;

    /// <summary>
    /// Whether there are more commands than can be displayed
    /// </summary>
    public bool HasOverflow { get; private set; }

    /// <summary>
    /// Number of overflow commands
    /// </summary>
    public int OverflowCount { get; private set; }

    #endregion

    #region Commands

    public ICommand ToggleExpandedCommand { get; }
    public ICommand ExecuteCommandCommand { get; }
    public ICommand ShowOverflowCommand { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Regenerate commands based on current context
    /// </summary>
    public void RefreshCommands()
    {
        var context = _gameState.GetCurrentContext();
        var generator = new SmartCommandGenerator();
        var allCommands = generator.GenerateCommands(context);

        // Take first 9 for display
        var displayCommands = allCommands.Take(9)
            .Select((cmd, index) => new SmartCommandViewModel(cmd, index + 1))
            .ToList();

        Commands = new ObservableCollection<SmartCommandViewModel>(displayCommands);

        // Track overflow
        HasOverflow = allCommands.Count > 9;
        OverflowCount = Math.Max(0, allCommands.Count - 9);

        // Update title based on context
        Title = context.CurrentMode switch
        {
            GameMode.Combat => "COMBAT ACTIONS",
            GameMode.Dialogue => "DIALOGUE OPTIONS",
            _ => "QUICK ACTIONS"
        };

        this.RaisePropertyChanged(nameof(HasCommands));
        this.RaisePropertyChanged(nameof(HasOverflow));
        this.RaisePropertyChanged(nameof(OverflowCount));
    }

    /// <summary>
    /// Execute command by index (0-8)
    /// </summary>
    public void ExecuteCommand(int index)
    {
        if (index < 0 || index >= Commands.Count) return;

        var command = Commands[index];

        // Check if confirmation required
        if (command.RequiresConfirmation)
        {
            ShowConfirmationDialog(command);
            return;
        }

        // Execute
        _dispatcher.Execute(command.CommandString);

        // Refresh
        RefreshCommands();
    }

    #endregion
}

```

### 8.2 SmartCommandViewModel

```csharp
public class SmartCommandViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// Hotkey number (1-9)
    /// </summary>
    public int Hotkey { get; set; }

    /// <summary>
    /// Formatted hotkey display "[1]"
    /// </summary>
    public string HotkeyDisplay => $"[{Hotkey}]";

    /// <summary>
    /// Command text to display
    /// </summary>
    public string DisplayText { get; set; } = string.Empty;

    /// <summary>
    /// Full command string for execution
    /// </summary>
    public string CommandString { get; set; } = string.Empty;

    /// <summary>
    /// Target object/NPC ID (if applicable)
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// Skill check type (if required)
    /// </summary>
    public SkillCheckType? SkillCheck { get; set; }

    /// <summary>
    /// Difficulty class (if skill check)
    /// </summary>
    public int? DC { get; set; }

    /// <summary>
    /// Formatted skill indicator "(MIGHT DC 4)"
    /// </summary>
    public string SkillIndicator
    {
        get
        {
            if (SkillCheck == null) return string.Empty;
            if (DC.HasValue)
                return $"({SkillCheck} DC {DC})";
            return $"({SkillCheck})";
        }
    }

    /// <summary>
    /// Color for skill indicator
    /// </summary>
    public string SkillColor => SkillCheck switch
    {
        SkillCheckType.MIGHT => "#DC143C",
        SkillCheckType.FINESSE => "#00CED1",
        SkillCheckType.WITS => "#9400D3",
        SkillCheckType.Hacking => "#4A90E2",
        SkillCheckType.Lockpicking => "#FFD700",
        _ => "#E0E0E0"
    };

    /// <summary>
    /// Fumble risk percentage (0.0-1.0)
    /// </summary>
    public float FumbleRisk { get; set; }

    /// <summary>
    /// Whether to show risk warning icon
    /// </summary>
    public bool ShowRiskWarning => FumbleRisk > 0.20f;

    /// <summary>
    /// Whether this requires confirmation before execution
    /// </summary>
    public bool RequiresConfirmation => FumbleRisk > 0.20f;

    /// <summary>
    /// Resource cost (Stamina, etc.)
    /// </summary>
    public int? ResourceCost { get; set; }

    /// <summary>
    /// Formatted resource cost "15 Stam"
    /// </summary>
    public string ResourceDisplay => ResourceCost.HasValue
        ? $"{ResourceCost} Stam"
        : string.Empty;

    /// <summary>
    /// Whether this is a combat action
    /// </summary>
    public bool IsCombatAction { get; set; }

    /// <summary>
    /// Combat action type display "[Standard]"
    /// </summary>
    public string ActionTypeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Whether this command is currently selected/hovered
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Whether this command is highlighted (hotkey pressed)
    /// </summary>
    public bool IsHighlighted { get; set; }

    #endregion

    #region Commands

    public ICommand ExecuteCommand { get; }

    #endregion
}

```

### 8.3 SmartCommand Data Class

```csharp
/// <summary>
/// Raw command data before ViewModel conversion
/// </summary>
public class SmartCommand
{
    public string CommandText { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public int Priority { get; set; }
    public SkillCheckType? SkillCheck { get; set; }
    public int? DC { get; set; }
    public float FumbleRisk { get; set; }
    public bool IsCombatAction { get; set; }
    public ActionType? ActionCost { get; set; }
    public int? ResourceCost { get; set; }
}

```

---

## 9. Accessibility Features

### 9.1 Keyboard Navigation

| Key | Action |
| --- | --- |
| 1-9 | Execute corresponding command |
| Tab | Show overflow menu |
| Arrow Up/Down | Navigate command list |
| Enter | Execute selected command |
| Escape | Close overflow menu |
| F1 | Read current command aloud |

### 9.2 Screen Reader Support

Each command announces:

1. Hotkey number
2. Action name
3. Target name (if applicable)
4. Skill check requirement (if any)
5. Risk warning (if applicable)

**Example Announcement**:

```
"1. Pull rusted lever. Requires MIGHT check, difficulty 4. Warning: High failure risk."

```

### 9.3 High Contrast Mode

| Element | Standard | High Contrast |
| --- | --- | --- |
| Background | #1C1C1C | #000000 |
| Text | #E0E0E0 | #FFFFFF |
| Risk Warning | #FF8C00 | #FF0000 |
| Selected | #4A90E280 | #FFFFFF border |

### 9.4 Configurable Options

| Setting | Default | Range | Description |
| --- | --- | --- | --- |
| `PanelOpacity` | 0.95 | 0.5-1.0 | Panel transparency |
| `FontScale` | 1.0 | 0.8-1.5 | Text size multiplier |
| `ShowHotkeys` | true | bool | Display hotkey indicators |
| `ShowSkillChecks` | true | bool | Display skill requirements |
| `ShowRiskWarnings` | true | bool | Display fumble warnings |
| `AutoCollapse` | false | bool | Collapse when not in use |

---

## 10. Implementation Guidance

### 10.1 Service Architecture

```csharp
public interface ISmartCommandService
{
    /// <summary>Generate commands for current context</summary>
    List<SmartCommand> GenerateCommands(GameContext context);

    /// <summary>Execute a smart command</summary>
    CommandResult ExecuteCommand(SmartCommand command);

    /// <summary>Calculate fumble risk for a command</summary>
    float CalculateFumbleRisk(SmartCommand command, PlayerCharacter player);

    /// <summary>Register hotkey handlers</summary>
    void RegisterHotkeys(IKeyboardShortcutService shortcuts);
}

```

### 10.2 File Structure

```
RuneAndRust.DesktopUI/
  ViewModels/
    SmartCommands/
      SmartCommandsPanelViewModel.cs
      SmartCommandViewModel.cs
  Views/
    SmartCommands/
      SmartCommandsPanelView.axaml
      SmartCommandsPanelView.axaml.cs
  Services/
    SmartCommandService.cs
    SmartCommandGenerator.cs

RuneAndRust.Core/
  Commands/
    SmartCommand.cs

```

### 10.3 Implementation Checklist

**ViewModels**:

- [ ]  Create `SmartCommandsPanelViewModel`
- [ ]  Create `SmartCommandViewModel`
- [ ]  Implement command generation
- [ ]  Implement context detection
- [ ]  Add hotkey registration

**Views**:

- [ ]  Create AXAML layout
- [ ]  Implement animations
- [ ]  Add accessibility attributes
- [ ]  Style according to design spec

**Services**:

- [ ]  Create `SmartCommandService`
- [ ]  Create `SmartCommandGenerator`
- [ ]  Integrate with `GameStateController`
- [ ]  Integrate with `CommandDispatcher`

**Integration**:

- [ ]  Add panel to `DungeonExplorationView`
- [ ]  Add panel to `CombatView`
- [ ]  Add panel to `DialogueView`
- [ ]  Subscribe to context change events

---

## Appendix A: AXAML Template

```xml
<UserControl x:Class="RuneAndRust.DesktopUI.Views.SmartCommandsPanelView"
             xmlns="<https://github.com/avaloniaui>"
             xmlns:x="<http://schemas.microsoft.com/winfx/2006/xaml>"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels">

    <Border Background="#1C1C1C"
            BorderBrush="#3A3A3A"
            BorderThickness="1"
            CornerRadius="4"
            Width="280"
            IsVisible="{Binding IsVisible}">

        <StackPanel>
            <!-- Title Bar -->
            <Grid Background="#2A2A2A" Height="28">
                <TextBlock Text="{Binding Title}"
                           FontWeight="Bold"
                           Foreground="White"
                           VerticalAlignment="Center"
                           Margin="8,0"/>
                <Button Content="[-]"
                        Command="{Binding ToggleExpandedCommand}"
                        HorizontalAlignment="Right"
                        Margin="4"/>
            </Grid>

            <!-- Commands List -->
            <ItemsControl Items="{Binding Commands}"
                          IsVisible="{Binding IsExpanded}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border Background="{Binding IsSelected, Converter={StaticResource SelectionBrush}}"
                                Height="28"
                                Margin="2">
                            <Grid ColumnDefinitions="32,*,Auto,24">
                                <!-- Hotkey -->
                                <Border Background="#4A90E2"
                                        CornerRadius="2"
                                        Margin="4"
                                        Grid.Column="0">
                                    <TextBlock Text="{Binding HotkeyDisplay}"
                                               Foreground="White"
                                               FontFamily="Consolas"
                                               HorizontalAlignment="Center"/>
                                </Border>

                                <!-- Command Text -->
                                <TextBlock Text="{Binding DisplayText}"
                                           Foreground="#E0E0E0"
                                           Grid.Column="1"
                                           VerticalAlignment="Center"/>

                                <!-- Skill Indicator -->
                                <TextBlock Text="{Binding SkillIndicator}"
                                           Foreground="{Binding SkillColor}"
                                           Grid.Column="2"
                                           FontSize="11"
                                           VerticalAlignment="Center"
                                           Margin="4,0"/>

                                <!-- Risk Warning -->
                                <TextBlock Text="⚠"
                                           Foreground="#FF8C00"
                                           Grid.Column="3"
                                           IsVisible="{Binding ShowRiskWarning}"
                                           VerticalAlignment="Center"/>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>

            <!-- Overflow Indicator -->
            <TextBlock Text="{Binding OverflowCount, StringFormat='[Tab] {0} more actions...'}"
                       IsVisible="{Binding HasOverflow}"
                       Foreground="#808080"
                       FontStyle="Italic"
                       FontSize="11"
                       Margin="8,4"/>
        </StackPanel>
    </Border>
</UserControl>

```

---

## Document History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2024 | Initial Smart Commands Panel specification |

---

*This document is part of the Rune & Rust technical documentation suite.*

*Related: PHYSICAL_INTERACTION_SPECIFICATION.md, GUI_SPECIFICATION.md*