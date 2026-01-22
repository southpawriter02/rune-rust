# Physical Interaction Action Economy Specification

## Version 1.0 - Time & Action Cost System Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Specification ID:** SPEC-WORLD-004
**Status:** Draft
**Domain:** World Systems
**Core Dependencies:** `RuneAndRust.Engine.WorldClockService`, `RuneAndRust.Engine.CombatEngine`

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

1. [Overview](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
2. [Design Philosophy](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
3. [Exploration Action Economy](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
4. [Combat Action Economy](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
5. [World Clock Integration](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
6. [Resource Costs](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
7. [Action Modifiers](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
8. [GUI Integration](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
9. [Balance & Tuning](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)
10. [Implementation Guidance](Physical%20Interaction%20Action%20Economy%20Specification%202ba55eb312da8083aa6afb696a768073.md)

---

## 1. Overview

### 1.1 Purpose

This specification defines the Action Economy for physical interactions in Rune & Rust. The Action Economy governs how much time and resources actions consume, creating meaningful trade-offs between different approaches and rewarding efficient play.

Physical interactions cost different amounts based on:

- **Context**: Exploration vs Combat
- **Complexity**: Simple vs Complex actions
- **Skill**: Character attributes and specializations
- **Outcome**: Success, failure, or fumble

### 1.2 Scope

**In Scope**:

- Exploration time costs (World Clock)
- Combat action costs (Standard, Bonus, Free)
- Stamina consumption for physical actions
- Time pressure mechanics
- Efficiency bonuses from specializations
- Failed action costs

**Out of Scope**:

- Combat turn structure → SPEC-COMBAT-001
- World Clock event triggers → World Event System
- Stamina regeneration → Character Progression
- Specific ability costs → Ability specifications

### 1.3 Success Criteria

- **Player Experience**: Actions feel appropriately weighted
- **Tactical Depth**: Action economy creates meaningful choices
- **Balance**: No dominant strategies emerge from action costs
- **Clarity**: Players understand costs before committing

---

## 2. Design Philosophy

### 2.1 Design Pillars

1. **Meaningful Trade-offs**
    - **Rationale**: Every action should have opportunity cost
    - **Examples**: Spend time searching thoroughly or move quickly
2. **Tactical Combat Integration**
    - **Rationale**: Physical interactions should be viable combat options
    - **Examples**: Pull lever costs Standard Action but may be worth it
3. **Character Expression**
    - **Rationale**: Specialized characters should be more efficient
    - **Examples**: Scrap-Tinker interacts faster with mechanisms
4. **Pressure Without Punishment**
    - **Rationale**: Time pressure adds tension but shouldn't softlock
    - **Examples**: Taking too long increases danger, doesn't end run

### 2.2 Core Principles

**Exploration**:

- Time is a renewable but consequential resource
- Thoroughness trades time for rewards
- Rest consumes significant time

**Combat**:

- Actions are strictly limited per turn
- Environmental interaction competes with combat actions
- Smart use of environment can save actions overall

---

## 3. Exploration Action Economy

### 3.1 Action Categories

| Category | Time Cost | Examples |
| --- | --- | --- |
| **Instant** | 0 minutes | Look, examine inventory |
| **Quick** | 1 minute | Simple interaction, press button |
| **Standard** | 2 minutes | Skill check interaction, search |
| **Extended** | 5 minutes | Complex interaction, detailed search |
| **Long** | 15 minutes | Rest (partial), crafting |
| **Full** | 60 minutes | Full rest, major repair |

### 3.2 Physical Interaction Time Costs

| Interaction | Base Time | With Check | On Failure | On Fumble |
| --- | --- | --- | --- | --- |
| **Pull** (simple) | 1 min | - | - | - |
| **Pull** (stuck) | 2 min | +1 min | +2 min | +5 min |
| **Push** (light) | 1 min | - | - | - |
| **Push** (heavy) | 2 min | +1 min | +2 min | +3 min |
| **Turn** (simple) | 1 min | - | - | - |
| **Turn** (stuck) | 2 min | +1 min | +2 min | +5 min |
| **Press** (single) | 0 min | - | - | Varies |
| **Press** (sequence) | 1 min | - | Reset | Alarm |

### 3.3 Other Exploration Actions

| Action | Time Cost | Notes |
| --- | --- | --- |
| **Move** (adjacent room) | 1 min | Per room |
| **Search** (quick) | 2 min | Basic search |
| **Search** (thorough) | 5 min | +30% find chance |
| **Investigate** | 2 min | WITS check |
| **Lockpick** | 3 min | Per attempt |
| **Hack** | 5 min | Per attempt |
| **Talk** (brief) | 2 min | Quick exchange |
| **Talk** (full) | 10 min | Full dialogue tree |
| **Trade** | 5 min | Buy/sell session |
| **Rest** (short) | 15 min | Recover 25% HP/Stamina |
| **Rest** (full) | 60 min | Full recovery |

### 3.4 Time Cost Calculation

```csharp
public int CalculateInteractionTime(
    InteractiveObject obj,
    PlayerCharacter character,
    InteractionResult result)
{
    int baseTime = GetBaseTime(obj.InteractionType);

    // Add skill check time
    if (obj.RequiresCheck)
        baseTime += 1;

    // Add failure time
    if (!result.Success)
        baseTime += 2;

    // Add fumble time
    if (result.IsFumble)
        baseTime += 3;

    // Apply efficiency modifiers
    float efficiency = CalculateEfficiency(character, obj);
    baseTime = (int)(baseTime * efficiency);

    // Minimum 1 minute for any non-instant action
    return Math.Max(1, baseTime);
}

```

---

## 4. Combat Action Economy

### 4.1 Action Types

| Action Type | Per Turn | Examples |
| --- | --- | --- |
| **Standard Action** | 1 | Attack, most abilities, pull lever |
| **Bonus Action** | 1 | Quick abilities, press button |
| **Free Action** | Unlimited | Drop item, speak, automatic triggers |
| **Reaction** | 1 | Parry, opportunity attack |
| **Movement** | 1 (6 tiles) | Position on grid |

### 4.2 Physical Interaction Combat Costs

| Interaction | Action Cost | Requirements | Notes |
| --- | --- | --- | --- |
| **Pull** (lever) | Standard | Adjacent to object | May trigger mechanism |
| **Push** (object) | Standard | Adjacent to object | May create cover/hazard |
| **Turn** (valve) | Standard | Adjacent to object | May affect hazards |
| **Press** (button) | Bonus | Adjacent to object | Quick activation |
| **Search** | Not allowed | - | Too time-consuming |
| **Hack** | Not allowed | - | Too time-consuming |

### 4.3 Environmental Interaction Examples

| Scenario | Action Cost | Effect |
| --- | --- | --- |
| Pull lever to drop cage | Standard | Cage drops on target (3d6 damage) |
| Push crate for cover | Standard | Create cover at position |
| Turn valve to release steam | Standard | Create steam hazard zone |
| Press button to seal door | Bonus | Close door, block reinforcements |
| Kick loose pillar | Standard + MIGHT | Pillar falls (area damage) |

### 4.4 Combat Interaction Resolution

```csharp
public CombatActionResult ResolveEnvironmentalAction(
    Combatant actor,
    InteractiveObject target,
    CombatState combat)
{
    // Check action availability
    var actionCost = GetCombatActionCost(target);
    if (!actor.HasAction(actionCost))
        return CombatActionResult.InsufficientActions;

    // Check adjacency
    if (target.RequiresAdjacency && !IsAdjacent(actor, target))
        return CombatActionResult.OutOfRange;

    // Resolve skill check if required
    if (target.RequiresCheck)
    {
        var checkResult = ResolveSkillCheck(actor, target);
        if (!checkResult.Success)
        {
            actor.ConsumeAction(actionCost);
            return new CombatActionResult
            {
                Success = false,
                ActionConsumed = actionCost,
                Message = checkResult.FailureMessage
            };
        }
    }

    // Apply interaction
    var interactionResult = ApplyInteraction(target, combat);

    // Consume action
    actor.ConsumeAction(actionCost);

    return new CombatActionResult
    {
        Success = true,
        ActionConsumed = actionCost,
        Effects = interactionResult.Effects
    };
}

```

### 4.5 Tactical Considerations

**When to Use Environmental Actions**:

| Situation | Recommendation | Reasoning |
| --- | --- | --- |
| Single enemy | Attack directly | More efficient damage |
| Multiple enemies | Environmental AOE | Affects multiple targets |
| Tough enemy | Environmental hazard | Bypasses defense |
| Escape needed | Seal door/create barrier | Prevents pursuit |
| Hazard on field | Disable hazard | Protects party |

**Action Economy Trade-offs**:

| Choice | Cost | Benefit |
| --- | --- | --- |
| Attack vs Pull Lever | Both Standard | Lever may do more damage |
| Move vs Push Crate | Both actions | Crate provides cover |
| Ability vs Turn Valve | Standard + Stamina vs Standard | Valve may affect area |

---

## 5. World Clock Integration

### 5.1 World Clock Overview

The World Clock tracks in-game time and triggers events:

| Time Unit | Real Time | Significance |
| --- | --- | --- |
| 1 Minute | ~1 second | Basic action unit |
| 10 Minutes | ~10 seconds | Short rest threshold |
| 1 Hour | ~60 seconds | Patrol cycles, event checks |
| 6 Hours | ~6 minutes | Major event threshold |
| 24 Hours | ~24 minutes | Day cycle (if applicable) |

### 5.2 Time-Triggered Events

| Threshold | Event Type | Effect |
| --- | --- | --- |
| Every 10 min | Ambient check | Random encounter chance |
| Every 30 min | Patrol cycle | Enemy positions shift |
| Every 1 hour | Status check | Hunger/thirst if enabled |
| Every 2 hours | Stress accumulation | +1 Psychic Stress |
| Every 6 hours | Major event | Escalation trigger |

### 5.3 Time Pressure Mechanics

**Escalation System**:

| Time Spent | Escalation Level | Effects |
| --- | --- | --- |
| 0-30 min | Normal | Standard difficulty |
| 30-60 min | Elevated | +10% enemy spawns |
| 1-2 hours | High | +20% spawns, patrols active |
| 2-4 hours | Critical | +30% spawns, elite enemies |
| 4+ hours | Dire | Boss-tier enemies roam |

**Sector Time Limits** (optional mode):

| Sector Type | Soft Limit | Hard Limit |
| --- | --- | --- |
| Standard | 2 hours | None |
| Timed Challenge | 1 hour | 90 minutes |
| Speed Run | 30 minutes | 45 minutes |

### 5.4 Time Display

**HUD Element**:

```
┌──────────────┐
│ TIME: 1:23:45│
│ ⚠ Elevated   │
└──────────────┘

```

**Properties**:

| Property | Type | Description |
| --- | --- | --- |
| `CurrentTime` | TimeSpan | Time since sector start |
| `EscalationLevel` | EscalationLevel | Current danger level |
| `TimeWarning` | bool | Show warning indicator |
| `NextEventIn` | TimeSpan | Time until next event |

---

## 6. Resource Costs

### 6.1 Stamina for Physical Actions

Some physical interactions cost Stamina:

| Interaction | Stamina Cost | Condition |
| --- | --- | --- |
| **Pull** (heavy) | 5 Stamina | MIGHT check required |
| **Push** (heavy) | 5 Stamina | MIGHT check required |
| **Turn** (stuck) | 3 Stamina | MIGHT check required |
| **Forced Entry** | 10 Stamina | Breaking locks/barriers |
| **Repeated Attempt** | +2 Stamina | Each retry |

### 6.2 Stamina Calculation

```csharp
public int CalculateStaminaCost(
    InteractiveObject obj,
    PlayerCharacter character,
    bool isRetry)
{
    int baseCost = 0;

    // Heavy objects cost stamina
    if (obj.RequiresCheck && obj.CheckType == SkillCheckType.MIGHT)
    {
        baseCost = obj.CheckDC; // Higher DC = more effort
    }

    // Retry penalty
    if (isRetry)
        baseCost += 2;

    // Efficiency reduces cost
    if (character.HasSpecialization("Scrap-Tinker"))
        baseCost = (int)(baseCost * 0.75f);

    // Fatigue increases cost
    if (character.IsFatigued)
        baseCost = (int)(baseCost * 1.5f);

    return baseCost;
}

```

### 6.3 Other Resource Costs

| Resource | When Consumed | Amount |
| --- | --- | --- |
| **Lockpicks** | Failed lockpick | 1 pick |
| **Hacking Tools** | Failed hack | 1 charge |
| **Repair Kits** | Fixing broken objects | 1 kit |
| **Lubricant** | Reducing MIGHT DC | 1 dose |

### 6.4 Resource Display

```
┌────────────────────────────┐
│ pull rusted lever          │
│ MIGHT DC 4                 │
│ ─────────────────────────  │
│ Time: 2 minutes            │
│ Stamina: 5                 │
│ Fumble Risk: 15%           │
│                            │
│ [Confirm] [Cancel]         │
└────────────────────────────┘

```

---

## 7. Action Modifiers

### 7.1 Efficiency Modifiers

Character traits and conditions affect action costs:

| Modifier | Effect | Source |
| --- | --- | --- |
| **Efficient** | -25% time | Scrap-Tinker specialization |
| **Rushed** | -50% time, +fumble risk | Player choice |
| **Careful** | +50% time, -fumble risk | Player choice |
| **Fatigued** | +50% time, +50% stamina | Low stamina state |
| **Focused** | -25% time | Buff effect |
| **Distracted** | +25% time | Debuff/stress |

### 7.2 Environmental Modifiers

| Condition | Effect | Examples |
| --- | --- | --- |
| **Darkness** | +50% time | No light source |
| **Hazardous** | +1 min per action | Active hazards nearby |
| **Combat Adjacent** | No extended actions | Enemies in adjacent room |
| **Time Pressure** | Cannot use "Careful" | Timed events active |

### 7.3 Equipment Modifiers

| Equipment | Effect | Notes |
| --- | --- | --- |
| **Toolkit** | -1 min mechanism time | Requires inventory slot |
| **Crowbar** | -1 DC for Pull/Push | +1 damage on fumble |
| **Lockpicks (Quality)** | -1 min lockpick time | Higher quality = more uses |
| **Light Source** | Removes darkness penalty | Consumes fuel |

### 7.4 Modifier Stacking

```csharp
public float CalculateTotalModifier(
    PlayerCharacter character,
    Room room,
    InteractiveObject obj)
{
    float modifier = 1.0f;

    // Character modifiers (multiplicative)
    if (character.HasTrait("Efficient"))
        modifier *= 0.75f;
    if (character.IsFatigued)
        modifier *= 1.5f;
    if (character.HasBuff("Focused"))
        modifier *= 0.75f;

    // Environmental modifiers (multiplicative)
    if (room.IsDark && !character.HasLightSource)
        modifier *= 1.5f;
    if (room.HasActiveHazards)
        modifier *= 1.25f;

    // Equipment modifiers (additive to time, not multiplier)
    // Applied separately in time calculation

    // Clamp to reasonable range
    return Math.Clamp(modifier, 0.5f, 3.0f);
}

```

---

## 8. GUI Integration

### 8.1 Action Cost Preview

Before executing an action, show costs:

```
┌────────────────────────────────────┐
│ Pull Rusted Lever                  │
├────────────────────────────────────┤
│ Requirements:                      │
│   • MIGHT Check (DC 4)             │
│   • Adjacent to lever              │
│                                    │
│ Costs:                             │
│   ⏱ Time: 3 minutes               │
│   ⚡ Stamina: 5                    │
│                                    │
│ Risk:                              │
│   ⚠ Fumble Chance: 15%            │
│                                    │
│ Current Resources:                 │
│   Stamina: 45/60 ✓                │
│   Time: 47 min (Elevated) ⚠       │
│                                    │
│ [Attempt] [Careful +50%] [Cancel]  │
└────────────────────────────────────┘

```

### 8.2 ActionCostPreviewViewModel

```csharp
public class ActionCostPreviewViewModel : ViewModelBase
{
    // Action Info
    public string ActionName { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;

    // Requirements
    public ObservableCollection<RequirementViewModel> Requirements { get; }
    public bool AllRequirementsMet { get; }

    // Costs
    public int TimeCostMinutes { get; set; }
    public string TimeCostDisplay => $"{TimeCostMinutes} minute{(TimeCostMinutes != 1 ? "s" : "")}";
    public int StaminaCost { get; set; }
    public string StaminaCostDisplay => StaminaCost > 0 ? StaminaCost.ToString() : "-";
    public bool HasStaminaCost => StaminaCost > 0;

    // Risk
    public float FumbleChance { get; set; }
    public string FumbleDisplay => $"{FumbleChance:P0}";
    public bool ShowFumbleWarning => FumbleChance > 0.15f;

    // Current Resources
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public bool HasEnoughStamina => CurrentStamina >= StaminaCost;
    public TimeSpan CurrentTime { get; set; }
    public EscalationLevel Escalation { get; set; }
    public bool TimeWarning => Escalation >= EscalationLevel.Elevated;

    // Modifiers
    public bool CanUseCareful { get; set; }
    public bool CanUseRushed { get; set; }
    public int CarefulTimeCost => (int)(TimeCostMinutes * 1.5f);
    public float CarefulFumbleChance => FumbleChance * 0.5f;
    public int RushedTimeCost => (int)(TimeCostMinutes * 0.5f);
    public float RushedFumbleChance => FumbleChance * 1.5f;

    // Commands
    public ICommand AttemptCommand { get; }
    public ICommand AttemptCarefulCommand { get; }
    public ICommand AttemptRushedCommand { get; }
    public ICommand CancelCommand { get; }
}

```

### 8.3 Combat Action Display

In combat, show action type alongside command:

```
┌─────────────────────────────────────┐
│ COMBAT ACTIONS                      │
├─────────────────────────────────────┤
│ [1] attack Rust-Horror              │
│     [Standard Action]               │
│                                     │
│ [2] pull lever                      │
│     [Standard Action] (MIGHT DC 3)  │
│                                     │
│ [3] press button                    │
│     [Bonus Action]                  │
│                                     │
│ [4] defend                          │
│     [Standard Action]               │
└─────────────────────────────────────┘

Actions Remaining: Standard ● Bonus ● Movement ●●●●●●

```

### 8.4 Time HUD Widget

```
┌─────────────────┐
│ ⏱ 1:23:45      │
│ ▲ Elevated     │
│ Next: 36:15    │
└─────────────────┘

```

| Element | Binding | Description |
| --- | --- | --- |
| Time Display | `CurrentTime` | HH:MM:SS format |
| Escalation | `EscalationLevel` | Color-coded level |
| Next Event | `NextEventIn` | Countdown to event |

---

## 9. Balance & Tuning

### 9.1 Balance Targets

| Metric | Target | Rationale |
| --- | --- | --- |
| Average room clear time | 5-10 minutes | Pace feels active |
| Interaction % of time | 20-30% | Meaningful but not dominant |
| Combat interaction rate | 10-20% of actions | Viable tactical option |
| Time pressure triggers | Every 30-60 min | Tension without frustration |

### 9.2 Tunable Parameters

| Parameter | Location | Default | Range |
| --- | --- | --- | --- |
| `BaseInteractionTime` | WorldConfig.json | 2 | 1-5 |
| `FailureTimePenalty` | WorldConfig.json | 2 | 1-5 |
| `FumbleTimePenalty` | WorldConfig.json | 5 | 3-10 |
| `StaminaPerDC` | GameConstants.cs | 1 | 0.5-2 |
| `EscalationThreshold1` | WorldConfig.json | 30 | 15-60 |
| `EscalationThreshold2` | WorldConfig.json | 60 | 30-120 |
| `PatrolCycleMinutes` | WorldConfig.json | 30 | 15-60 |

### 9.3 Balance Scenarios

**Scenario 1: Speed vs Thoroughness**

- Player can rush through sector in ~30 minutes
- Thorough exploration takes ~90 minutes
- Rushing means missing 40-60% of loot
- Thorough exploration triggers escalation

**Scenario 2: Combat Environment Use**

- Using lever to drop cage: Standard Action, 3d6 damage
- Direct attack: Standard Action, ~8 damage average
- Lever is better for groups, attack for single targets

**Scenario 3: Resource Management**

- Heavy interactions drain stamina
- Running low forces rest (15-60 minutes)
- Rest triggers escalation
- Plan interactions to conserve stamina

---

## 10. Implementation Guidance

### 10.1 Service Architecture

```csharp
public interface IActionEconomyService
{
    /// <summary>Calculate time cost for interaction</summary>
    int CalculateTimeCost(
        InteractiveObject obj,
        PlayerCharacter character,
        InteractionResult? result = null);

    /// <summary>Calculate stamina cost for interaction</summary>
    int CalculateStaminaCost(
        InteractiveObject obj,
        PlayerCharacter character,
        bool isRetry = false);

    /// <summary>Get combat action type for interaction</summary>
    ActionType GetCombatActionType(InteractiveObject obj);

    /// <summary>Apply time cost to world clock</summary>
    void ApplyTimeCost(int minutes);

    /// <summary>Check if character can afford action</summary>
    bool CanAffordAction(
        PlayerCharacter character,
        InteractiveObject obj);

    /// <summary>Get preview of action costs</summary>
    ActionCostPreview GetActionPreview(
        PlayerCharacter character,
        InteractiveObject obj);
}

```

### 10.2 Data Models

```csharp
public class ActionCostPreview
{
    public int TimeCostMinutes { get; set; }
    public int StaminaCost { get; set; }
    public ActionType CombatActionType { get; set; }
    public float FumbleChance { get; set; }
    public List<string> Requirements { get; set; } = new();
    public bool CanAfford { get; set; }
    public string? AffordanceReason { get; set; }
}

public enum ActionType
{
    Free,
    Bonus,
    Standard,
    Full,      // Takes entire turn
    Extended,  // Multiple turns (exploration only)
    Reaction
}

```

### 10.3 Implementation Checklist

**Core System**:

- [ ]  Create `IActionEconomyService` interface
- [ ]  Implement time cost calculation
- [ ]  Implement stamina cost calculation
- [ ]  Integrate with `WorldClockService`
- [ ]  Add action type to `InteractiveObject`

**Combat Integration**:

- [ ]  Add `CombatActionType` property to objects
- [ ]  Integrate with `CombatEngine` action resolution
- [ ]  Update combat UI with action indicators

**GUI**:

- [ ]  Create `ActionCostPreviewViewModel`
- [ ]  Create action preview dialog
- [ ]  Add time HUD widget
- [ ]  Update Smart Commands with costs

**Balance**:

- [ ]  Configure default parameters
- [ ]  Create test scenarios
- [ ]  Playtest and tune values

---

## Appendix A: Time Cost Reference Table

| Action | Base | +Check | +Failure | +Fumble | Total Range |
| --- | --- | --- | --- | --- | --- |
| Pull (simple) | 1 | - | - | - | 1 min |
| Pull (check) | 2 | +1 | +2 | +5 | 3-10 min |
| Push (simple) | 1 | - | - | - | 1 min |
| Push (check) | 2 | +1 | +2 | +3 | 3-8 min |
| Turn (simple) | 1 | - | - | - | 1 min |
| Turn (check) | 2 | +1 | +2 | +5 | 3-10 min |
| Press (single) | 0 | - | - | - | 0 min |
| Press (sequence) | 1 | - | Reset | Varies | 1-5 min |
| Search (quick) | 2 | - | - | - | 2 min |
| Search (thorough) | 5 | - | - | - | 5 min |
| Lockpick | 3 | Included | +2 | +3 | 3-8 min |
| Hack | 5 | Included | +3 | +5 | 5-13 min |

---

## Appendix B: Combat Action Reference

| Interaction | Action Type | Stamina | Adjacency | Skill Check |
| --- | --- | --- | --- | --- |
| Pull lever | Standard | 0 | Yes | Optional |
| Push object | Standard | 5 | Yes | MIGHT |
| Turn valve | Standard | 0 | Yes | Optional |
| Press button | Bonus | 0 | Yes | No |
| Kick obstacle | Standard | 5 | Yes | MIGHT |
| Throw object | Standard | 3 | No (range 3) | FINESSE |

---

## Document History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2024 | Initial Action Economy specification |

---

*This document is part of the Rune & Rust technical documentation suite.Related: PHYSICAL_INTERACTION_SPECIFICATION.md, SPEC-COMBAT-001*