---
id: SPEC-UI-TERMINAL-ADAPTER
title: "Terminal Adapter — Implementation Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Terminal/"
    status: Planned
---

# Terminal Adapter — Implementation Specification

---

## 1. Overview

The terminal adapter provides a fully functional text-based interface for gameplay and testing. It implements all 4 presentation interfaces using console output and ANSI escape sequences.

| Interface | Terminal Implementation |
|-----------|------------------------|
| `IPresenter` | `TerminalPresenter` |
| `IInputHandler` | `TerminalInputHandler` |
| `IRenderTarget` | `TerminalRenderTarget` |
| `IMapRenderer` | `TerminalMapRenderer` |

---

## 2. ANSI Color Codes

### 2.1 Message Types

| MessageType | Foreground | Code |
|-------------|------------|------|
| Info | White | `\x1b[37m` |
| Success | Green | `\x1b[32m` |
| Warning | Yellow | `\x1b[33m` |
| Error | Red | `\x1b[31m` |
| Combat | Cyan | `\x1b[36m` |
| Loot | Magenta | `\x1b[35m` |
| Quest | Blue | `\x1b[34m` |

### 2.2 Resource Bars

| Resource | Color | Example |
|----------|-------|---------|
| HP | Red | `\x1b[31m████████░░\x1b[0m` |
| Stamina | Yellow | `\x1b[33m██████████\x1b[0m` |
| AP | Blue | `\x1b[34m████░░░░░░\x1b[0m` |

### 2.3 Rarity Colors

| Rarity | Color | Code |
|--------|-------|------|
| Common | White | `\x1b[37m` |
| Uncommon | Green | `\x1b[32m` |
| Rare | Blue | `\x1b[34m` |
| Epic | Magenta | `\x1b[35m` |
| Legendary | Yellow | `\x1b[33m` |

---

## 3. Display Formats

### 3.1 Room Display

```
╔══════════════════════════════════════════════════════════════╗
║  ABANDONED WORKSHOP                                          ║
╟──────────────────────────────────────────────────────────────╢
║  Rust-eaten machinery fills this cramped space. A faint      ║
║  hum emanates from a cracked terminal in the corner.         ║
║                                                              ║
║  OBJECTS: [lever] [terminal] [corpse]                        ║
║  EXITS: [north] [east]                                       ║
╚══════════════════════════════════════════════════════════════╝
```

### 3.2 Combat Display

```
╔══════════════════════════════════════════════════════════════╗
║  COMBAT — Round 3                              Turn: YOU     ║
╟──────────────────────────────────────────────────────────────╢
║  YOU: 45/60 HP  ████████░░░░  [Focused]                      ║
║  Ally: 30/40 HP ████████░░░░  [—]                            ║
╟──────────────────────────────────────────────────────────────╢
║  Goblin: 12/30 HP  ████░░░░░░  [Bleeding]                    ║
║  Orc:    25/50 HP  █████░░░░░  [—]                           ║
╟──────────────────────────────────────────────────────────────╢
║  [1] attack goblin (2d8+3)                                   ║
║  [2] use Skewer II on orc (2d8+1d10, 35 Stam)                ║
║  [3] defend                                                  ║
║  [4] flee                                                    ║
╚══════════════════════════════════════════════════════════════╝
> _
```

### 3.3 Inventory Display

```
╔══════════════════════════════════════════════════════════════╗
║  INVENTORY                                   Gold: 127       ║
╟──────────────────────────────────────────────────────────────╢
║  EQUIPPED:                                                   ║
║    Weapon: Longsword (2d8+3)                                 ║
║    Armor:  Chainmail (+4 Soak)                               ║
║    Shield: —                                                 ║
╟──────────────────────────────────────────────────────────────╢
║  ITEMS:                                                      ║
║    [1] Health Draught x3                                     ║
║    [2] Antidote x1                                           ║
║    [3] Iron Key                                              ║
╚══════════════════════════════════════════════════════════════╝
```

### 3.4 Minimap

```
┌───────────┐
│ ·─·─·     │
│ │ │ │     │
│ ·─@─·─·   │
│   │   │   │
│   ·───·   │
└───────────┘
@ = You  · = Room  ─ = Passage
```

### 3.5 Combat Grid

```
  A B C D E F
1 . . . . . .
2 . P . . E .
3 . A . . . .
4 . . . . E .
5 . . . . . .

P = Player  A = Ally  E = Enemy
```

---

## 4. Command Parser

### 4.1 Syntax

```
<verb> [arguments...]
```

**Examples:**
```
> attack goblin
> use skewer on orc
> go north
> look terminal
> inventory
> equip longsword
> 1
```

### 4.2 Hotkey Support

Single digit executes corresponding smart command:

```
> 1          ← Executes first smart command
> 2          ← Executes second smart command
```

### 4.3 Aliases

| Command | Aliases |
|---------|---------|
| `attack` | `a`, `hit` |
| `go` | `move`, `walk`, `n/s/e/w` |
| `look` | `l`, `examine`, `x` |
| `inventory` | `i`, `inv` |
| `use` | `u` |
| `equip` | `eq`, `wear` |
| `unequip` | `uneq`, `remove` |
| `help` | `h`, `?` |
| `quit` | `q`, `exit` |

### 4.4 Direction Shortcuts

```
> n          → go north
> s          → go south
> e          → go east
> w          → go west
> u          → go up
> d          → go down
```

---

## 5. TerminalPresenter Implementation

```csharp
public class TerminalPresenter : IPresenter
{
    private const string Reset = "\x1b[0m";
    
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        var color = type switch
        {
            MessageType.Success => "\x1b[32m",
            MessageType.Warning => "\x1b[33m",
            MessageType.Error => "\x1b[31m",
            MessageType.Combat => "\x1b[36m",
            MessageType.Loot => "\x1b[35m",
            MessageType.Quest => "\x1b[34m",
            _ => "\x1b[37m"
        };
        
        Console.WriteLine($"{color}{message}{Reset}");
    }
    
    public void ShowCombatLog(CombatLogEntry entry)
    {
        var prefix = entry.Type switch
        {
            CombatLogType.Damage => entry.IsCritical ? "★ CRITICAL: " : "→ ",
            CombatLogType.Heal => "+ ",
            CombatLogType.Miss => "○ ",
            CombatLogType.Death => "✗ ",
            _ => "  "
        };
        
        ShowMessage($"{prefix}{entry.Message}", MessageType.Combat);
    }
    
    public void UpdateResource(ResourceType type, int current, int max)
    {
        var bar = RenderBar(current, max, 10);
        var color = type switch
        {
            ResourceType.HP => "\x1b[31m",
            ResourceType.Stamina => "\x1b[33m",
            ResourceType.AP => "\x1b[34m",
            _ => "\x1b[37m"
        };
        
        Console.WriteLine($"{type}: {current}/{max} {color}{bar}{Reset}");
    }
    
    private string RenderBar(int current, int max, int width)
    {
        var filled = (int)Math.Round((double)current / max * width);
        return new string('█', filled) + new string('░', width - filled);
    }
}
```

---

## 6. TerminalMapRenderer Implementation

```csharp
public class TerminalMapRenderer : IMapRenderer
{
    public void RenderMinimap(MinimapDisplay minimap)
    {
        Console.WriteLine("┌───────────┐");
        
        // Render 5x5 grid centered on current room
        for (int y = -2; y <= 2; y++)
        {
            Console.Write("│ ");
            for (int x = -2; x <= 2; x++)
            {
                var room = FindRoom(minimap, x, y);
                if (room == null)
                    Console.Write("  ");
                else if (room.RoomId == minimap.CurrentRoomId)
                    Console.Write("@ ");
                else if (!room.IsVisited)
                    Console.Write("? ");
                else if (room.HasEnemies)
                    Console.Write("! ");
                else
                    Console.Write("· ");
            }
            Console.WriteLine("│");
        }
        
        Console.WriteLine("└───────────┘");
    }
    
    public void RenderCombatGrid(CombatGridDisplay grid)
    {
        // Column headers
        Console.Write("  ");
        for (int x = 0; x < grid.Width; x++)
            Console.Write((char)('A' + x) + " ");
        Console.WriteLine();
        
        for (int y = 0; y < grid.Height; y++)
        {
            Console.Write($"{y + 1} ");
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
                var combatant = grid.Combatants.FirstOrDefault(c => c.X == x && c.Y == y);
                
                if (combatant != null)
                    Console.Write(combatant.IsPlayer ? "P " : combatant.IsAlly ? "A " : "E ");
                else if (cell?.InMovementRange == true)
                    Console.Write("○ ");
                else
                    Console.Write(". ");
            }
            Console.WriteLine();
        }
    }
}
```

---

## 7. Game Loop

### 7.1 Main Loop Structure

```
┌─────────────────────────────────────────────────┐
│              TERMINAL GAME LOOP                 │
├─────────────────────────────────────────────────┤
│  1. Render current state (room/combat/dialogue) │
│  2. Show smart commands                         │
│  3. Display prompt (> _)                        │
│  4. Read input                                  │
│  5. Parse command                               │
│  6. Execute command                             │
│  7. Process events                              │
│  8. Update state                                │
│  9. Loop to step 1                              │
└─────────────────────────────────────────────────┘
```

### 7.2 Context-Specific Rendering

| Context | Render Order |
|---------|--------------|
| Exploration | Room → Minimap → Smart Commands → Prompt |
| Combat | Combat Display → Grid → Smart Commands → Prompt |
| Dialogue | NPC Portrait → Dialogue → Options → Prompt |
| Inventory | Inventory Panel → Item Details → Prompt |

### 7.3 Prompt Formats

| Context | Prompt |
|---------|--------|
| Exploration | `> ` |
| Combat | `[Combat] > ` |
| Dialogue | `[Say] > ` |
| Inventory | `[Inv] > ` |

---

## 8. Error Handling

### 8.1 Error Messages

| Error Type | Message Format |
|------------|----------------|
| Unknown command | `Unknown command: '{verb}'. Type 'help' for commands.` |
| Invalid target | `Cannot find '{target}'. Try 'look' to see options.` |
| No target | `{verb} requires a target. Usage: {verb} <target>` |
| Not available | `Cannot {verb} right now.` |
| Insufficient resource | `Not enough {resource} ({required} needed, {current} available).` |
| Wrong context | `Cannot {verb} during {context}.` |

### 8.2 Examples

```
> attack
  Error: Attack requires a target. Usage: attack <target>

> go purple
  Error: Cannot go 'purple'. Valid exits: [north] [east]

> use skewer
  Error: Not enough Stamina (35 needed, 20 available).

> rest
  Error: Cannot rest during combat.
```

### 8.3 Confirmation Prompts

```
> quit
  Really quit? Unsaved progress will be lost. (y/n): _

> use Iron Key on door
  This will consume the Iron Key. Continue? (y/n): _

> attack merchant
  Warning: This will make the Merchant hostile. Continue? (y/n): _
```

---

## 9. Dialogue Display

### 9.1 NPC Dialogue

```
╔══════════════════════════════════════════════════════════════╗
║  BJORN THE MERCHANT                                          ║
╟──────────────────────────────────────────────────────────────╢
║  "Ah, a scavenger! You look like you've been through the     ║
║  rust-wastes. I've got supplies, if you've got the coin."    ║
╟──────────────────────────────────────────────────────────────╢
║  [1] "Show me your wares." [Trade]                           ║
║  [2] "Tell me about this place."                             ║
║  [3] [Intimidate DC 12] "Lower your prices."                 ║
║  [4] "Any work available?" [Quest]                           ║
║  [5] "Farewell."                                             ║
╚══════════════════════════════════════════════════════════════╝
[Say] > _
```

### 9.2 Skill Check Options

```
║  [3] [Intimidate DC 12] "Lower your prices."    (67% chance) ║
```

**Display Format**: `[{Skill} DC {DC}] "{Text}" ({chance}% chance)`

### 9.3 Dialogue Outcomes

```
> 3
  [Intimidate Check: MIGHT vs DC 12]
  You roll: 8 + 5 (MIGHT) = 13
  ✓ SUCCESS!
  
  Bjorn's eyes widen. "Alright, alright! I'll give you a discount..."
```

---

## 10. Character Sheet Display

```
╔══════════════════════════════════════════════════════════════╗
║  RANULF                                    Legend: 5         ║
║  Human Warrior — Atgeir-Wielder                              ║
╟──────────────────────────────────────────────────────────────╢
║  ATTRIBUTES                                                  ║
║    MIGHT: 7       FINESSE: 4       WITS: 3                   ║
║    RESOLVE: 5     STURDINESS: 6    PRESENCE: 2               ║
╟──────────────────────────────────────────────────────────────╢
║  RESOURCES                                                   ║
║    HP:      45/60  ████████░░░░                              ║
║    Stamina: 80/100 ████████░░                                ║
║    AP:      3/3    ███                                       ║
╟──────────────────────────────────────────────────────────────╢
║  ABILITIES (Atgeir-Wielder)                                  ║
║    Formal Training II      — Passive: +2 hit with polearms   ║
║    Skewer II               — 2d8+1d10, 35 Stamina            ║
║    Disciplined Stance II   — +1 Soak while in front row      ║
╟──────────────────────────────────────────────────────────────╢
║  STATUS EFFECTS                                              ║
║    [Focused] — +1 to hit (2 turns remaining)                 ║
╟──────────────────────────────────────────────────────────────╢
║  TRAUMAS                                                     ║
║    Battle Fatigue (Level 1) — -1 to initiative               ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 11. Skill Check Display

### 11.1 Check Announcement

```
> pull lever

  [MIGHT Check vs DC 4]
  Rolling...
```

### 11.2 Check Resolution

**Success:**
```
  You roll: 7 + 7 (MIGHT) = 14
  ★ TRIUMPH! (Natural 10)
  
  The lever moves with ease. A hidden panel slides open, 
  revealing a cache of supplies!
```

**Failure:**
```
  You roll: 2 + 7 (MIGHT) = 9
  ✗ FAILURE
  
  The lever is rusted solid. You'll need to find another way.
```

**Fumble:**
```
  You roll: 1 + 7 (MIGHT) = 8
  ✗ FUMBLE! (Natural 1)
  
  The lever snaps off in your hand! The mechanism 
  is now permanently jammed.
```

---

## 12. Status Effect Display

### 12.1 Application

```
  [Status] You are now [Bleeding] (1d6 damage per turn, 3 turns)
```

### 12.2 Per-Turn Tick

```
  [Status] [Bleeding] deals 4 damage. (2 turns remaining)
```

### 12.3 Removal

```
  [Status] [Bleeding] has worn off.
```

### 12.4 Combat Status Summary

```
╟──────────────────────────────────────╢
║  YOUR STATUS: [Focused] [Bleeding]   ║
║  Goblin:      [Poisoned]             ║
╟──────────────────────────────────────╢
```

---

## 13. Loot & Item Display

### 13.1 Loot Drop

```
  ═══════════════════════════════════
  LOOT FOUND
  ═══════════════════════════════════
    [Uncommon] Iron Longsword (2d8+3)
    [Common] Health Draught x2
    Gold: 47
  ═══════════════════════════════════
  
  Collect all? (y/n): _
```

### 13.2 Item Examination

```
> look longsword

  ┌────────────────────────────────────┐
  │ IRON LONGSWORD                     │
  │ [Uncommon] Weapon — Sword          │
  ├────────────────────────────────────┤
  │ Damage: 2d8+3 Physical             │
  │ Accuracy: -                        │
  │ Special: -                         │
  ├────────────────────────────────────┤
  │ A well-forged blade, still sharp   │
  │ despite the rust on its pommel.    │
  └────────────────────────────────────┘
```

---

## 14. Full Command Reference

### 14.1 Exploration Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| `go` | `go <direction>` | Move to adjacent room |
| `look` | `look [target]` | Examine room or object |
| `search` | `search` | Search current room |
| `take` | `take <item>` | Pick up item |
| `use` | `use <item/ability> [on <target>]` | Use item or ability |
| `talk` | `talk <npc>` | Start dialogue |
| `rest` | `rest` | Rest to recover resources |

### 14.2 Combat Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| `attack` | `attack <target>` | Basic attack |
| `use` | `use <ability> [on <target>]` | Use ability |
| `defend` | `defend` | Defensive stance (+2 Soak) |
| `flee` | `flee` | Attempt to escape combat |
| `move` | `move <cell>` | Move to grid position |
| `end` | `end` | End turn |

### 14.3 Inventory Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| `inventory` | `inventory` | Open inventory |
| `equip` | `equip <item>` | Equip item |
| `unequip` | `unequip <slot>` | Unequip from slot |
| `drop` | `drop <item>` | Drop item |
| `use` | `use <consumable>` | Use consumable item |

### 14.4 System Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| `character` | `character` | Open character sheet |
| `help` | `help [command]` | Show help |
| `save` | `save [slot]` | Save game |
| `load` | `load [slot]` | Load game |
| `settings` | `settings` | Open settings |
| `quit` | `quit` | Exit game |

---

## 15. Accessibility Features

### 15.1 Color Blindness Support

```
> settings accessibility

  [1] Color Mode: Normal / Deuteranopia / Protanopia / Tritanopia
  [2] High Contrast: Off / On
  [3] Symbol Markers: Off / On  (adds text markers to colors)
```

### 15.2 Symbol Markers

When enabled, adds text markers alongside colors:

| Type | Normal | With Markers |
|------|--------|--------------|
| Damage | Red text | `[DMG]` prefix |
| Heal | Green text | `[HEAL]` prefix |
| Warning | Yellow text | `[WARN]` prefix |

### 15.3 Screen Reader Mode

```
> settings screenreader on

  Screen reader mode enabled.
  - All UI boxes converted to plain text
  - Color codes removed
  - Verbose descriptions enabled
```

---

## 16. Implementation Status

| Component | File Path | Status |
|-----------|-----------|--------|
| TerminalPresenter | `RuneAndRust.Terminal/TerminalPresenter.cs` | ❌ Planned |
| TerminalInputHandler | `RuneAndRust.Terminal/TerminalInputHandler.cs` | ❌ Planned |
| TerminalRenderTarget | `RuneAndRust.Terminal/TerminalRenderTarget.cs` | ❌ Planned |
| TerminalMapRenderer | `RuneAndRust.Terminal/TerminalMapRenderer.cs` | ❌ Planned |
| CommandParser | `RuneAndRust.Terminal/CommandParser.cs` | ❌ Planned |
| TerminalGameLoop | `RuneAndRust.Terminal/TerminalGameLoop.cs` | ❌ Planned |
| AccessibilitySettings | `RuneAndRust.Terminal/AccessibilitySettings.cs` | ❌ Planned |
