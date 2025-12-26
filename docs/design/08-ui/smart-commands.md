---
id: SPEC-UI-SMART-COMMANDS
title: "Smart Commands — Context-Aware Actions"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/SmartCommandService.cs"
    status: Planned
---

# Smart Commands — Context-Aware Actions

---

## 1. Overview

Smart Commands surface relevant actions based on the player's current context — exploration, combat, or dialogue. Both terminal and GUI use the same generation logic but render differently.

| Context | Panel Title | Max Commands |
|---------|-------------|--------------|
| Exploration | QUICK ACTIONS | 9 |
| Combat | COMBAT ACTIONS | 9 |
| Dialogue | DIALOGUE OPTIONS | 9 |

---

## 2. Command Generation

### 2.1 SmartCommandService

```csharp
public class SmartCommandService
{
    public IReadOnlyList<SmartCommand> Generate(GameContext context)
    {
        var commands = context.Mode switch
        {
            GameMode.Exploration => GenerateExplorationCommands(context),
            GameMode.Combat => GenerateCombatCommands(context),
            GameMode.Dialogue => GenerateDialogueCommands(context),
            _ => Array.Empty<SmartCommand>()
        };
        
        return commands
            .OrderBy(c => c.Priority)
            .Take(9)
            .ToList();
    }
}
```

### 2.2 SmartCommand Record

```csharp
public record SmartCommand(
    string Verb,                      // attack, use, go
    string DisplayText,               // "attack goblin"
    int Priority,                     // 1 = highest
    string? TargetId = null,          // UUID of target
    SkillCheckInfo? SkillCheck = null,// MIGHT DC 4
    int? ResourceCost = null,         // Stamina/AP cost
    bool IsCombatAction = false,
    bool RequiresConfirmation = false // High-risk action
);

public record SkillCheckInfo(
    string Attribute,                 // MIGHT, FINESSE, WITS
    int DC,
    double SuccessChance
);
```

---

## 3. Exploration Commands

### 3.1 Priority Order

| Priority | Source | Examples |
|----------|--------|----------|
| 1 | Interactive Objects | pull lever, push crate |
| 2 | Search/Investigate | search room, examine body |
| 3 | NPCs | talk to merchant |
| 4 | Navigation | go north, use stairs |
| 5 | Rest/Camp | rest, make camp |
| 6 | Character | inventory, character |

### 3.2 Generation Logic

```csharp
private IEnumerable<SmartCommand> GenerateExplorationCommands(GameContext ctx)
{
    var room = ctx.CurrentRoom;
    
    // Interactive objects (priority 1)
    foreach (var obj in room.InteractiveObjects.Where(o => o.CanInteract))
    {
        yield return new SmartCommand(
            Verb: obj.InteractionVerb,
            DisplayText: $"{obj.InteractionVerb} {obj.Name}",
            Priority: 1,
            TargetId: obj.Id.ToString(),
            SkillCheck: obj.RequiresCheck ? new SkillCheckInfo(
                obj.CheckAttribute,
                obj.CheckDC,
                CalculateChance(ctx.Player, obj.CheckAttribute, obj.CheckDC)
            ) : null
        );
    }
    
    // Search (priority 2)
    if (!room.IsSearched)
    {
        yield return new SmartCommand(
            Verb: "search",
            DisplayText: "search room",
            Priority: 2
        );
    }
    
    // NPCs (priority 3)
    foreach (var npc in room.NPCs.Where(n => n.CanTalk))
    {
        yield return new SmartCommand(
            Verb: "talk",
            DisplayText: $"talk to {npc.Name}",
            Priority: 3,
            TargetId: npc.Id.ToString()
        );
    }
    
    // Navigation (priority 4)
    foreach (var exit in room.Exits)
    {
        yield return new SmartCommand(
            Verb: "go",
            DisplayText: $"go {exit.Direction}",
            Priority: 4,
            TargetId: exit.TargetRoomId.ToString()
        );
    }
    
    // Rest (priority 5)
    if (room.CanRest)
    {
        yield return new SmartCommand(
            Verb: "rest",
            DisplayText: "rest",
            Priority: 5
        );
    }
}
```

---

## 4. Combat Commands

### 4.1 Priority Order

| Priority | Source | Examples |
|----------|--------|----------|
| 1 | Attack/Abilities | attack goblin, use Skewer II |
| 2 | Environmental | pull lever, push barrel |
| 3 | Defensive | defend, switch stance |
| 4 | Items | use Health Draught |
| 5 | Movement | move to B3 |
| 9 | End Turn | end turn |

### 4.2 Generation Logic

```csharp
private IEnumerable<SmartCommand> GenerateCombatCommands(GameContext ctx)
{
    if (!ctx.Combat.IsPlayerTurn) yield break;
    
    // Attack selected target (priority 1)
    if (ctx.Combat.SelectedTarget != null)
    {
        yield return new SmartCommand(
            Verb: "attack",
            DisplayText: $"attack {ctx.Combat.SelectedTarget.Name}",
            Priority: 1,
            TargetId: ctx.Combat.SelectedTarget.Id.ToString(),
            IsCombatAction: true
        );
    }
    
    // Available abilities (priority 1)
    foreach (var ability in ctx.Player.Abilities.Where(a => a.CanUse()))
    {
        yield return new SmartCommand(
            Verb: "use",
            DisplayText: $"use {ability.DisplayName}",
            Priority: 1,
            TargetId: ability.Id.ToString(),
            ResourceCost: ability.StaminaCost,
            IsCombatAction: true
        );
    }
    
    // Defend (priority 3)
    yield return new SmartCommand(
        Verb: "defend",
        DisplayText: "defend",
        Priority: 3,
        IsCombatAction: true
    );
    
    // End turn (priority 9)
    yield return new SmartCommand(
        Verb: "end",
        DisplayText: "end turn",
        Priority: 9,
        IsCombatAction: true
    );
}
```

---

## 5. Dialogue Commands

### 5.1 Priority Order

| Priority | Type | Examples |
|----------|------|----------|
| 1 | Quest-related | "Any work available?" |
| 2 | Trade | "I need supplies." |
| 3 | Information | "Tell me about the ruins." |
| 4 | Skill checks | [Intimidate] "Lower your prices" |
| 5 | Exit | "Farewell." |

### 5.2 Generation Logic

```csharp
private IEnumerable<SmartCommand> GenerateDialogueCommands(GameContext ctx)
{
    var dialogue = ctx.Dialogue;
    int index = 0;
    
    foreach (var option in dialogue.CurrentNode.Options)
    {
        var skillCheck = option.RequiresCheck ? new SkillCheckInfo(
            option.CheckAttribute,
            option.CheckDC,
            CalculateChance(ctx.Player, option.CheckAttribute, option.CheckDC)
        ) : null;
        
        yield return new SmartCommand(
            Verb: "say",
            DisplayText: GetDialogueDisplay(option, skillCheck),
            Priority: GetDialoguePriority(option),
            TargetId: option.Id.ToString(),
            SkillCheck: skillCheck
        );
        
        index++;
    }
}

private string GetDialogueDisplay(DialogueOption opt, SkillCheckInfo? check)
{
    if (check != null)
        return $"[{check.Attribute}] \"{opt.Text}\"";
    if (opt.LeadsToTrade)
        return $"\"{opt.Text}\" [Trade]";
    if (opt.LeadsToQuest)
        return $"\"{opt.Text}\" [Quest]";
    return $"\"{opt.Text}\"";
}
```

---

## 6. Display Formats

### 6.1 Terminal

```
╔══════════════════════════════════════╗
║ QUICK ACTIONS                        ║
╟──────────────────────────────────────╢
║ [1] pull lever (MIGHT DC 4)       ⚠ ║
║ [2] search corpse                    ║
║ [3] go north                         ║
║ [4] talk to merchant                 ║
║ [5] rest                             ║
╚══════════════════════════════════════╝
```

### 6.2 GUI Panel

| Element | Display |
|---------|---------|
| Hotkey Badge | `[1]` in accent blue |
| Command Text | "pull lever" in white |
| Skill Check | "(MIGHT DC 4)" in red |
| Risk Warning | "⚠" icon if fumble > 20% |
| Resource Cost | "(35 Stam)" in yellow |

---

## 7. Hotkey Execution

### 7.1 Terminal

```csharp
// Single digit executes smart command
if (input.Length == 1 && char.IsDigit(input[0]))
{
    int index = input[0] - '1';
    if (index >= 0 && index < _smartCommands.Count)
    {
        ExecuteCommand(_smartCommands[index]);
        return;
    }
}
```

### 7.2 GUI

```csharp
for (int i = 1; i <= 9; i++)
{
    int index = i - 1;
    shortcuts.Register(
        key: (Key)((int)Key.D1 + index),
        action: () => ExecuteCommand(Commands[index])
    );
}
```

---

## 8. Implementation Status

| Component | File Path | Status |
|-----------|-----------|--------|
| SmartCommandService | `RuneAndRust.Engine/SmartCommandService.cs` | ❌ Planned |
| SmartCommand record | `RuneAndRust.Core/UI/SmartCommand.cs` | ❌ Planned |
| SkillCheckInfo | `RuneAndRust.Core/UI/SkillCheckInfo.cs` | ❌ Planned |
