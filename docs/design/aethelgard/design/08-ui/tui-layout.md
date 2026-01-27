---
id: SPEC-UI-TUI-LAYOUT
title: "TUI Layout — Screen Composition Specification"
version: 1.0
status: draft
last-updated: 2025-12-11
related-files:
  - path: "docs/08-ui/terminal-adapter.md"
    status: Reference
  - path: "docs/08-ui/presentation-layer.md"
    status: Reference
  - path: "docs/01-core/logging.md"
    status: Reference
---

# TUI Layout — Screen Composition Specification

> *"Information is power. Know where to look."*

---

## 1. Overview

This specification defines how TUI screen regions compose together to create a cohesive player experience. It bridges the gap between individual display components (defined in `terminal-adapter.md`) and the unified activity log system (defined in `logging.md`).

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-TUI-LAYOUT` |
| Category | UI System |
| Priority | Should-Have |
| Status | Draft |

### 1.2 Design Pillars

- **Information Density** — Show relevant information without overwhelming
- **Context Awareness** — Layout adapts to game mode (exploration, combat, dialogue)
- **Activity Visibility** — All significant events surfaced in activity log
- **Keyboard-First** — All interactions accessible via keyboard

---

## 2. Screen Regions

### 2.1 Region Map

```
┌─────────────────────────────────────────────────────────────────────┐
│                           HEADER BAR                                │
│  HP: 45/60 ████████░░  Stamina: 80/100 ████████░░  [Bleeding]       │
├─────────────────────────────────────────────────────┬───────────────┤
│                                                     │               │
│                    MAIN PANEL                       │    SIDE       │
│                                                     │    PANEL      │
│  (Room display, Combat grid, Dialogue, etc.)        │               │
│                                                     │  (Minimap,    │
│                                                     │   Turn Order, │
│                                                     │   Status)     │
│                                                     │               │
├─────────────────────────────────────────────────────┴───────────────┤
│                         ACTIVITY LOG                                │
│  → You attack Goblin for 12 damage                                  │
│  ★ CRITICAL HIT!                                                    │
│  [Status] Goblin is now [Bleeding]                                  │
├─────────────────────────────────────────────────────────────────────┤
│                      COMMAND PANEL                                  │
│  [1] attack goblin  [2] use Skewer  [3] defend  [4] flee            │
├─────────────────────────────────────────────────────────────────────┤
│  [Combat] > _                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Region Definitions

| Region | Purpose | Height |
|--------|---------|--------|
| **Header Bar** | Resources, active status effects | 2 lines |
| **Main Panel** | Context-specific display (room, combat, dialogue) | Variable (fills space) |
| **Side Panel** | Minimap, turn order, secondary info | Same as Main |
| **Activity Log** | Rolling event feed | 4-6 lines |
| **Command Panel** | Smart commands for current context | 2 lines |
| **Input Prompt** | Command entry | 1 line |

---

## 3. Activity Log Panel

### 3.1 Purpose

The Activity Log provides a **rolling feed** of all significant game events, not just combat. It bridges developer logging (Serilog) with player-facing visibility.

### 3.2 Event Sources

| Source System | Example Events |
|---------------|----------------|
| **Combat** | Attacks, damage, abilities, deaths |
| **Status Effects** | Applied, stacked, expired, cleansed |
| **Exploration** | Room entered, items found, traps triggered |
| **Character** | Level up, specialization unlocked, trauma acquired |
| **Death/Resurrection** | Death saves, resurrection, corruption gained |
| **Dice Rolls** | Significant rolls (crits, fumbles, skill checks) |

### 3.3 Event Types and Formatting

| Type | Prefix | Color | Example |
|------|--------|-------|---------|
| **Attack** | `→` | Cyan | `→ You attack Goblin for 12 damage` |
| **Critical** | `★` | Yellow | `★ CRITICAL HIT!` |
| **Heal** | `+` | Green | `+ Potion restores 15 HP` |
| **Status Applied** | `[Status]` | Purple | `[Status] Goblin is now [Bleeding]` |
| **Status Expired** | `[Status]` | Gray | `[Status] [Focused] has worn off` |
| **Death** | `✗` | Red | `✗ Goblin has been slain!` |
| **System** | `[System]` | White | `[System] Game saved` |
| **Warning** | `[!]` | Yellow | `[!] Low HP warning` |
| **Discovery** | `◆` | Blue | `◆ Found: Iron Key` |
| **Movement** | `»` | White | `» Entered Abandoned Workshop` |

### 3.4 Log Buffer

| Property | Value |
|----------|-------|
| **Visible lines** | 4-6 (configurable) |
| **Buffer size** | 100 entries |
| **Scroll** | `PgUp`/`PgDn` to scroll history |
| **Export** | `log` command exports to file |

### 3.5 Activity Log Interface

```csharp
public interface IActivityLog
{
    /// <summary>Add entry to activity log</summary>
    void AddEntry(ActivityLogEntry entry);
    
    /// <summary>Get visible log entries</summary>
    IReadOnlyList<ActivityLogEntry> GetVisibleEntries(int count);
    
    /// <summary>Get full history</summary>
    IReadOnlyList<ActivityLogEntry> GetHistory();
    
    /// <summary>Clear log</summary>
    void Clear();
}

public record ActivityLogEntry(
    string Message,
    ActivityLogType Type,
    DateTime Timestamp,
    Guid? SourceId = null,
    Guid? TargetId = null
);

public enum ActivityLogType
{
    // Combat
    Attack, Damage, Heal, Miss, Block, Death,
    
    // Status
    StatusApplied, StatusExpired, StatusStacked,
    
    // Exploration
    RoomEntered, ItemFound, TrapTriggered,
    
    // Character
    LevelUp, TraumaAcquired, CorruptionGained,
    
    // System
    GameSaved, GameLoaded, Warning
}
```

---

## 4. Context-Specific Layouts

### 4.1 Exploration Mode

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 45/60 ████████░░  Stamina: 80/100 ████████░░                   │
├─────────────────────────────────────────────────────┬───────────────┤
│  ╔══════════════════════════════════════════════╗   │ ┌───────────┐ │
│  ║  ABANDONED WORKSHOP                          ║   │ │ ·─·─·     │ │
│  ╟──────────────────────────────────────────────╢   │ │ │ │ │     │ │
│  ║  Rust-eaten machinery fills this cramped     ║   │ │ ·─@─·─·   │ │
│  ║  space. A faint hum emanates from a cracked  ║   │ │   │   │   │ │
│  ║  terminal in the corner.                     ║   │ │   ·───·   │ │
│  ║                                              ║   │ └───────────┘ │
│  ║  OBJECTS: [lever] [terminal] [corpse]        ║   │               │
│  ║  EXITS: [north] [east]                       ║   │               │
│  ╚══════════════════════════════════════════════╝   │               │
├─────────────────────────────────────────────────────┴───────────────┤
│  » Entered Abandoned Workshop                                       │
│  ◆ Found: Rusty Key                                                 │
├─────────────────────────────────────────────────────────────────────┤
│  [1] go north  [2] look terminal  [3] search  [4] inventory         │
├─────────────────────────────────────────────────────────────────────┤
│  > _                                                                │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.2 Combat Mode

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 45/60 ████████░░  Stamina: 55/100 ██████░░░░  [Focused]        │
├─────────────────────────────────────────────────────┬───────────────┤
│  ╔══════════════════════════════════════════════╗   │ TURN ORDER    │
│  ║  COMBAT — Round 3                  Turn: YOU ║   │ ────────────  │
│  ╟──────────────────────────────────────────────╢   │ → YOU         │
│  ║  YOU:    45/60 HP  ████████░░░░  [Focused]   ║   │   Goblin      │
│  ║  Ally:   30/40 HP  ████████░░░░  [—]         ║   │   Orc         │
│  ╟──────────────────────────────────────────────╢   │               │
│  ║  Goblin: 12/30 HP  ████░░░░░░  [Bleeding]    ║   │ ┌───────────┐ │
│  ║  Orc:    25/50 HP  █████░░░░░  [—]           ║   │ │   A B C   │ │
│  ╚══════════════════════════════════════════════╝   │ │ 1 P . .   │ │
│                                                     │ │ 2 A . E   │ │
│                                                     │ │ 3 . . E   │ │
│                                                     │ └───────────┘ │
├─────────────────────────────────────────────────────┴───────────────┤
│  → You attack Goblin for 12 damage                                  │
│  ★ CRITICAL HIT!                                                    │
│  [Status] Goblin is now [Bleeding] (1d4/turn, 3 turns)              │
│  → Goblin attacks you for 5 damage (soaked 3)                       │
├─────────────────────────────────────────────────────────────────────┤
│  [1] attack goblin  [2] use Skewer on orc  [3] defend  [4] flee     │
├─────────────────────────────────────────────────────────────────────┤
│  [Combat] > _                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.3 Dialogue Mode

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 60/60 ██████████  Stamina: 100/100 ██████████                  │
├─────────────────────────────────────────────────────────────────────┤
│  ╔══════════════════════════════════════════════════════════════╗   │
│  ║  BJORN THE MERCHANT                                          ║   │
│  ╟──────────────────────────────────────────────────────────────╢   │
│  ║  "Ah, a scavenger! You look like you've been through the     ║   │
│  ║  rust-wastes. I've got supplies, if you've got the coin."    ║   │
│  ╟──────────────────────────────────────────────────────────────╢   │
│  ║  [1] "Show me your wares." [Trade]                           ║   │
│  ║  [2] "Tell me about this place."                             ║   │
│  ║  [3] [Intimidate DC 12] "Lower your prices." (67%)           ║   │
│  ║  [4] "Any work available?" [Quest]                           ║   │
│  ║  [5] "Farewell."                                             ║   │
│  ╚══════════════════════════════════════════════════════════════╝   │
├─────────────────────────────────────────────────────────────────────┤
│  » Speaking with Bjorn the Merchant                                 │
├─────────────────────────────────────────────────────────────────────┤
│  [Say] > _                                                          │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 5. Dual Logging Pattern

### 5.1 Developer vs Player Logs

| Aspect | Developer (Serilog) | Player (IPresenter) |
|--------|---------------------|---------------------|
| **Purpose** | Debugging, telemetry | Game feedback |
| **Destination** | File, console, Seq | Activity Log panel |
| **Level** | Verbose → Fatal | All visible |
| **Content** | Technical details | Player-friendly text |
| **Persistence** | Log files | Session only |

### 5.2 Dual Logging Example

```csharp
public void ApplyBleeding(Guid targetId, int stacks)
{
    // Developer log (Serilog)
    _logger.Debug(
        "[Bleeding] applied to {CharacterId}: {StackCount} stacks",
        targetId, stacks);
    
    // Player log (IPresenter)
    var targetName = GetCharacterName(targetId);
    _presenter.ShowCombatLog(new CombatLogEntry(
        $"{targetName} is now [Bleeding] ({stacks}d4/turn)",
        CombatLogType.StatusApplied,
        TargetId: targetId));
    
    // Or use the unified activity log
    _activityLog.AddEntry(new ActivityLogEntry(
        $"[Status] {targetName} is now [Bleeding]",
        ActivityLogType.StatusApplied,
        TargetId: targetId));
}
```

### 5.3 When to Log to Each

| Event | Developer Log | Player Log |
|-------|---------------|------------|
| Damage dealt | ✅ Debug (with formula) | ✅ `→ 12 damage` |
| Status applied | ✅ Debug | ✅ `[Status] ...` |
| Internal state change | ✅ Verbose | ❌ |
| Room transition | ✅ Debug | ✅ `» Entered...` |
| Game saved | ✅ Information | ✅ `[System] Saved` |
| Validation error | ✅ Warning | ✅ `[!] Error` |
| Exception | ✅ Error | ✅ Simplified message |

---

## 6. Integration with Events System

The activity log subscribes to game events for automatic logging:

```csharp
public class ActivityLogSubscriber
{
    private readonly IActivityLog _activityLog;
    
    public ActivityLogSubscriber(IEventBus eventBus, IActivityLog activityLog)
    {
        _activityLog = activityLog;
        
        eventBus.Subscribe<DamageDealtEvent>(OnDamage, priority: 150);
        eventBus.Subscribe<StatusEffectAppliedEvent>(OnStatus, priority: 150);
        eventBus.Subscribe<RoomEnteredEvent>(OnRoomEntered, priority: 150);
        eventBus.Subscribe<CharacterDiedEvent>(OnDeath, priority: 150);
    }
    
    private void OnDamage(DamageDealtEvent evt)
    {
        var crit = evt.WasCritical ? "★ CRITICAL! " : "";
        _activityLog.AddEntry(new ActivityLogEntry(
            $"{crit}→ {evt.AttackerName} hits {evt.TargetName} for {evt.FinalDamage} damage",
            ActivityLogType.Damage,
            evt.TargetId));
    }
    
    private void OnStatus(StatusEffectAppliedEvent evt)
    {
        _activityLog.AddEntry(new ActivityLogEntry(
            $"[Status] {evt.TargetName} is now [{evt.EffectName}]",
            ActivityLogType.StatusApplied,
            evt.TargetId));
    }
}
```

---

## 7. Configuration

### 7.1 Layout Settings

| Setting | Default | Options |
|---------|---------|---------|
| `ActivityLogLines` | 4 | 2-10 |
| `ShowMinimap` | true | true/false |
| `ShowTurnOrder` | true | true/false |
| `CompactMode` | false | true/false |

### 7.2 Compact Mode

For smaller terminals (80x24), compact mode reduces padding and merges panels.

---

## 8. Implementation Status

| Component | Status |
|-----------|--------|
| Screen region layout | ❌ Planned |
| Activity log panel | ❌ Planned |
| IActivityLog interface | ❌ Planned |
| ActivityLogSubscriber | ❌ Planned |
| Exploration layout | ❌ Planned |
| Combat layout | ❌ Planned |
| Dialogue layout | ❌ Planned |

---

## 9. Related Specifications

| Spec | Relationship |
|------|--------------|
| [terminal-adapter.md](terminal-adapter.md) | Individual display components |
| [presentation-layer.md](presentation-layer.md) | IPresenter interface |
| [logging.md](../01-core/logging.md) | Developer logging (Serilog) |
| [events.md](../01-core/events.md) | Event subscriptions |

---

## 10. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-11 | Initial specification |
