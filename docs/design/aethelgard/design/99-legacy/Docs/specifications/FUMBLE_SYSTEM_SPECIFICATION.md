# Fumble & Critical Failure System Specification

## Version 1.0 - Comprehensive Critical Failure Mechanics Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Specification ID:** SPEC-WORLD-002
**Status:** Draft
**Domain:** World Systems
**Core Dependencies:** `RuneAndRust.Engine.DiceService`, `RuneAndRust.Core.Descriptors`

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2024-11-27 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained

---

## Table of Contents

1. [Overview](#1-overview)
2. [Design Philosophy](#2-design-philosophy)
3. [Fumble Triggers](#3-fumble-triggers)
4. [Fumble Types & Consequences](#4-fumble-types--consequences)
5. [Context-Specific Fumbles](#5-context-specific-fumbles)
6. [Trauma Economy Integration](#6-trauma-economy-integration)
7. [Data Model](#7-data-model)
8. [GUI Feedback](#8-gui-feedback)
9. [Balance & Tuning](#9-balance--tuning)
10. [Implementation Guidance](#10-implementation-guidance)

---

## 1. Overview

### 1.1 Purpose

This specification defines the Fumble & Critical Failure System for Rune & Rust. Fumbles represent catastrophic failures that create lasting consequences, transforming routine skill checks into moments of tension and emergent narrative.

Unlike standard failures (which allow retry), fumbles produce permanent or semi-permanent effects that change the game state, creating stories about the party's mistakes and the new challenges they must overcome.

### 1.2 Scope

**In Scope**:
- Fumble trigger conditions across all skill check contexts
- Fumble consequence types and severity
- Physical interaction fumbles (pull, push, turn, press)
- Combat fumbles (attack, ability, parry)
- Exploration fumbles (lockpicking, hacking, investigation)
- Trauma Economy integration (stress from fumbles)
- GUI feedback and combat log integration

**Out of Scope**:
- Standard failure handling (non-fumble) → Skill Check System
- Critical success mechanics → Separate specification
- Specific puzzle designs → SPEC-WORLD-005
- Individual ability fumble effects → Ability specifications

### 1.3 Success Criteria

- **Player Experience**: Fumbles feel dramatic but fair, creating memorable moments
- **Technical**: Fumble resolution completes within 50ms
- **Design**: Fumble rate remains between 3-15% depending on character build
- **Balance**: Fumbles punish recklessness without being run-ending

---

## 2. Design Philosophy

### 2.1 Design Pillars

1. **Emergent Narrative**
   - **Rationale**: Fumbles create unique stories that players remember and recount
   - **Examples**: "Remember when Kjartan broke the only lever and we had to find another way through?"

2. **Meaningful Risk**
   - **Rationale**: High-DC checks should carry genuine tension beyond pass/fail
   - **Examples**: Attempting a DC 6 check with low attribute means accepting fumble risk

3. **Character Expression**
   - **Rationale**: High-attribute characters fumble less, reinforcing build identity
   - **Examples**: MIGHT 8 character rarely fumbles force checks; MIGHT 3 character fears them

4. **Recoverable Setbacks**
   - **Rationale**: Fumbles should create obstacles, not dead ends
   - **Examples**: Broken lever means finding alternative route, not softlock

### 2.2 Player Experience Goals

**Target Experience**: Players should feel genuine tension when attempting difficult checks, knowing that failure could mean more than "try again."

**Emotional Arc**:
1. **Assessment**: "This is DC 5, my MIGHT is 4... risky."
2. **Decision**: "Do I risk it or find another way?"
3. **Tension**: Rolling dice with fumble possibility
4. **Resolution**: Relief (success), acceptance (failure), drama (fumble)

### 2.3 Design Constraints

- **No Softlocks**: Every fumble must leave an alternative path forward
- **Telegraphed Risk**: Players must know fumble is possible before attempting
- **Proportional Consequence**: Fumble severity scales with check importance
- **Recovery Options**: Some fumbles can be mitigated with resources or abilities

---

## 3. Fumble Triggers

### 3.1 Core Fumble Condition

A **Fumble** occurs when a skill check results in **Net Successes <= -2**.

```
NetSuccesses = AttackerSuccesses - DefenderSuccesses (opposed)
NetSuccesses = RollSuccesses - DC (static)

If NetSuccesses <= -2: FUMBLE
If NetSuccesses <= 0 and > -2: FAILURE
If NetSuccesses >= 1: SUCCESS
If NetSuccesses >= 3: CRITICAL SUCCESS
```

### 3.2 Fumble Probability Calculation

**Formula**:
```
P(Fumble) = P(Successes <= DC - 2)
```

For a dice pool of N d10s where success = 6+:
- P(success per die) = 0.5
- P(k successes from N dice) = C(N,k) * 0.5^N

### 3.3 Fumble Probability Tables

**Static Check (vs DC)**:

| Attribute (Dice) | DC 2 | DC 3 | DC 4 | DC 5 | DC 6 |
|------------------|------|------|------|------|------|
| 3 | 12.5% | 50.0% | 87.5% | 96.9% | 99.2% |
| 4 | 6.3% | 31.3% | 68.8% | 93.8% | 98.4% |
| 5 | 3.1% | 18.8% | 50.0% | 81.3% | 96.9% |
| 6 | 1.6% | 10.9% | 34.4% | 65.6% | 89.1% |
| 7 | 0.8% | 6.3% | 22.7% | 50.0% | 77.3% |
| 8 | 0.4% | 3.5% | 14.5% | 36.3% | 63.7% |
| 9 | 0.2% | 2.0% | 9.0% | 25.4% | 50.0% |
| 10 | 0.1% | 1.1% | 5.5% | 17.2% | 37.7% |

**Interpretation**:
- A character with 5 dice attempting DC 4 has a **50% fumble chance**
- A character with 8 dice attempting DC 4 has only a **14.5% fumble chance**
- High-attribute characters can safely attempt checks that would be suicidal for low-attribute characters

### 3.4 Fumble Immunity

Certain conditions grant **Fumble Immunity**:

| Condition | Effect | Duration |
|-----------|--------|----------|
| Specialization Rank 3 | Immune to fumbles in specialty | Permanent |
| "Steady Hands" Trait | Immune to FINESSE fumbles | Permanent |
| "Focused Mind" Buff | Immune to next fumble | Until triggered |
| Using Proper Tools | Fumble threshold reduced to -3 | Per check |

### 3.5 Fumble Severity Modifiers

Some contexts increase fumble severity:

| Modifier | Effect | Example |
|----------|--------|---------|
| Combat Pressure | +1 fumble severity tier | Fumbling in combat |
| Damaged Equipment | Fumble breaks equipment | Using damaged tools |
| Hazardous Environment | Environmental damage on fumble | Fumbling near hazard |
| Fatigued State | Fumble threshold becomes -1 | Exhausted character |

---

## 4. Fumble Types & Consequences

### 4.1 FumbleType Enumeration

```csharp
public enum FumbleType
{
    /// <summary>No fumble consequence defined</summary>
    None,

    /// <summary>Object becomes permanently unusable</summary>
    BreakObject,

    /// <summary>Trap or hazard activates</summary>
    TriggerHazard,

    /// <summary>Enemies alerted or spawned</summary>
    AlertEnemies,

    /// <summary>Status effect applied to character</summary>
    ApplyStatus,

    /// <summary>Room or environment permanently changed</summary>
    EnvironmentalChange,

    /// <summary>Equipment damaged or destroyed</summary>
    DamageEquipment,

    /// <summary>Resource lost (items, currency)</summary>
    LoseResource,

    /// <summary>Character injured (HP damage)</summary>
    SelfInjury,

    /// <summary>Time lost (World Clock advance)</summary>
    TimeLoss,

    /// <summary>Position compromised (forced movement)</summary>
    Displacement
}
```

### 4.2 Fumble Severity Tiers

| Tier | Name | Persistence | Examples |
|------|------|-------------|----------|
| 1 | **Minor** | Temporary (1 encounter) | Staggered status, minor time loss |
| 2 | **Moderate** | Session-persistent | Broken tool, alert state, moderate damage |
| 3 | **Severe** | Permanent (run) | Destroyed mechanism, permanent room change |
| 4 | **Catastrophic** | Meta-persistent | Achievement blocked, unique item destroyed |

### 4.3 Fumble Consequence Details

#### 4.3.1 BreakObject (Tier 2-3)

**Description**: The interacted object breaks and becomes unusable.

**Mechanical Effects**:
- Object state set to `[Broken]`
- `CanInteract` returns false permanently
- Alternative solutions must be found

**Narrative Template**:
```
"The [object] snaps/shatters/crumbles in your hands. The mechanism is
now permanently disabled."
```

**Recovery Options**:
- Find alternative route/solution
- Scrap-Tinker "Jury Rig" ability (if available)
- Some objects have backup mechanisms

#### 4.3.2 TriggerHazard (Tier 2-3)

**Description**: A trap or environmental hazard activates.

**Mechanical Effects**:
- Hazard deals damage (defined by `TrapDamage`)
- May apply status effects
- May become persistent hazard

**Damage Ranges**:
| Hazard Tier | Damage | Status Effect |
|-------------|--------|---------------|
| Minor | 1d6 | None |
| Moderate | 2d6 | Bleeding, Burning, or Poisoned |
| Severe | 3d6+5 | Multiple effects possible |

#### 4.3.3 AlertEnemies (Tier 2)

**Description**: Noise attracts enemies or triggers alarm.

**Mechanical Effects**:
- Enemy patrol spawns or approaches
- Room enters "Alert" state
- Potential ambush (enemies get surprise round)

**Alert Levels**:
| Level | Effect | Duration |
|-------|--------|----------|
| Suspicious | Enemies investigate | 3 turns |
| Alerted | Combat initiated | Immediate |
| Alarmed | Reinforcements called | Until cleared |

#### 4.3.4 ApplyStatus (Tier 1-2)

**Description**: Character suffers a debilitating status effect.

**Common Fumble Statuses**:
| Status | Duration | Effect |
|--------|----------|--------|
| Staggered | 1 round | -2 to next action |
| Off-Balance | 2 rounds | -1 Evasion |
| Disoriented | 3 rounds | -1 to all checks |
| Injured | Until rest | -1 to physical checks |

#### 4.3.5 EnvironmentalChange (Tier 3)

**Description**: The room or environment is permanently altered.

**Examples**:
- Steam vent becomes permanently active
- Passage collapses
- Water floods chamber
- Power permanently fails

**Mechanical Effects**:
- Room gains new hazard or loses feature
- May block or reveal passages
- Affects all future visits to room

#### 4.3.6 DamageEquipment (Tier 2)

**Description**: Character's equipment is damaged or destroyed.

**Damage Levels**:
| Level | Effect | Repair Cost |
|-------|--------|-------------|
| Worn | -1 to equipment bonus | 25% value |
| Damaged | -2 to equipment bonus | 50% value |
| Broken | Unusable until repaired | 75% value |
| Destroyed | Permanently lost | N/A |

#### 4.3.7 SelfInjury (Tier 1-2)

**Description**: Character injures themselves during failed action.

**Damage Calculation**:
```
SelfDamage = 1d6 + (DC - AttributeDice)
Minimum: 1
Maximum: 2d6 + 5
```

#### 4.3.8 TimeLoss (Tier 1)

**Description**: Additional time consumed by fumbled action.

**Time Costs**:
| Fumble Context | Base Time | Fumble Time |
|----------------|-----------|-------------|
| Simple interaction | 1 min | 10 min |
| Complex interaction | 5 min | 30 min |
| Combat action | 0 | Lose next turn |

#### 4.3.9 Displacement (Tier 1-2)

**Description**: Character forced to move or fall.

**Displacement Effects**:
| Trigger | Movement | Additional Effect |
|---------|----------|-------------------|
| Push fumble | Knocked back 1 tile | Prone |
| Pull fumble | Pulled into object | 1d6 damage |
| Turn fumble | Spin disoriented | -1 next action |
| Climb fumble | Fall | Fall damage |

---

## 5. Context-Specific Fumbles

### 5.1 Physical Interaction Fumbles

| Interaction | Fumble Type | Consequence |
|-------------|-------------|-------------|
| **Pull** (lever/chain) | BreakObject | Object breaks off, unusable |
| **Push** (heavy object) | Displacement + ApplyStatus | Knocked back, Staggered |
| **Turn** (valve/wheel) | BreakObject | Mechanism shears, disabled |
| **Press** (button/panel) | TriggerHazard or AlertEnemies | Wrong input triggered |

### 5.2 Combat Fumbles

| Action | Fumble Type | Consequence |
|--------|-------------|-------------|
| **Melee Attack** | SelfInjury or DamageEquipment | Hit self or weapon damaged |
| **Ranged Attack** | LoseResource or Displacement | Ammo wasted, knocked prone |
| **Ability Use** | ApplyStatus or TimeLoss | Backfire effect, lose action |
| **Parry** | SelfInjury + ApplyStatus | Counter-hit, Off-Balance |

### 5.3 Exploration Fumbles

| Action | Fumble Type | Consequence |
|--------|-------------|-------------|
| **Lockpicking** | BreakObject or AlertEnemies | Lock jammed or alarm |
| **Hacking** | AlertEnemies or TimeLoss | System lockout, traced |
| **Investigation** | TriggerHazard | Hidden trap triggered |
| **Climbing** | Displacement + SelfInjury | Fall damage |

### 5.4 Social Fumbles (Future)

| Action | Fumble Type | Consequence |
|--------|-------------|-------------|
| **Persuasion** | Reputation loss | NPC hostility |
| **Deception** | Exposed | Combat or imprisonment |
| **Intimidation** | Backfire | Target attacks |

---

## 6. Trauma Economy Integration

### 6.1 Fumble Stress Generation

Fumbles generate **Psychic Stress** based on severity:

| Fumble Tier | Stress Generated | Narrative |
|-------------|------------------|-----------|
| Minor | +3 Stress | Frustration, embarrassment |
| Moderate | +5 Stress | Fear, self-doubt |
| Severe | +8 Stress | Panic, despair |
| Catastrophic | +12 Stress | Trauma trigger potential |

### 6.2 Fumble-Triggered Trauma

If a fumble pushes Psychic Stress past a **Trauma Threshold** (25, 50, 75), the character may acquire a permanent Trauma:

**Fumble-Related Traumas**:
| Trauma | Trigger | Effect |
|--------|---------|--------|
| "Clumsy Hands" | 3+ physical fumbles | -1 to FINESSE checks |
| "Hesitant" | Fumble in critical moment | Must spend Bonus Action to attempt DC 5+ checks |
| "Equipment Anxiety" | Equipment destruction fumble | -1 with that equipment type |

### 6.3 Corruption from Fumbles

Certain fumbles generate **Runic Blight Corruption**:

| Fumble Context | Corruption Generated |
|----------------|---------------------|
| Aetheric device fumble | +5 Corruption |
| Runic mechanism fumble | +3 Corruption |
| Forlorn-touched object fumble | +8 Corruption |

---

## 7. Data Model

### 7.1 FumbleResult Class

```csharp
/// <summary>
/// Result of a fumble occurrence
/// </summary>
public class FumbleResult
{
    /// <summary>Unique identifier for this fumble event</summary>
    public int FumbleId { get; set; }

    /// <summary>Character who fumbled</summary>
    public int CharacterId { get; set; }

    /// <summary>Context where fumble occurred</summary>
    public FumbleContext Context { get; set; }

    /// <summary>Type of fumble consequence</summary>
    public FumbleType Type { get; set; }

    /// <summary>Severity tier (1-4)</summary>
    public int Severity { get; set; }

    /// <summary>Narrative description of fumble</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Mechanical effects applied</summary>
    public List<FumbleEffect> Effects { get; set; } = new();

    /// <summary>Stress generated</summary>
    public int StressGenerated { get; set; }

    /// <summary>Corruption generated (if any)</summary>
    public int CorruptionGenerated { get; set; }

    /// <summary>Whether fumble can be mitigated</summary>
    public bool CanMitigate { get; set; }

    /// <summary>Cost to mitigate (if applicable)</summary>
    public string? MitigationCost { get; set; }

    /// <summary>Timestamp of fumble</summary>
    public DateTime Timestamp { get; set; }
}
```

### 7.2 FumbleEffect Class

```csharp
/// <summary>
/// Individual mechanical effect from a fumble
/// </summary>
public class FumbleEffect
{
    /// <summary>Type of effect</summary>
    public FumbleEffectType EffectType { get; set; }

    /// <summary>Target of effect (character, object, room)</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Effect value (damage amount, duration, etc.)</summary>
    public int Value { get; set; }

    /// <summary>Duration in turns (0 = permanent)</summary>
    public int Duration { get; set; }

    /// <summary>Status effect to apply (if applicable)</summary>
    public StatusEffectType? StatusEffect { get; set; }
}

public enum FumbleEffectType
{
    Damage,
    StatusApplication,
    ObjectStateChange,
    RoomModification,
    ResourceLoss,
    TimeAdvance,
    Displacement,
    EquipmentDamage
}
```

### 7.3 FumbleContext Enum

```csharp
public enum FumbleContext
{
    PhysicalInteraction,
    CombatAttack,
    CombatAbility,
    CombatParry,
    Lockpicking,
    Hacking,
    Investigation,
    Climbing,
    Social,
    Crafting
}
```

### 7.4 InteractiveObject Fumble Properties

```csharp
// Extensions to InteractiveObject class
public partial class InteractiveObject
{
    /// <summary>Whether this object can cause fumbles</summary>
    public bool CanFumble { get; set; } = true;

    /// <summary>Type of fumble this object causes</summary>
    public FumbleType FumbleType { get; set; } = FumbleType.BreakObject;

    /// <summary>Severity tier of fumble (1-4)</summary>
    public int FumbleSeverity { get; set; } = 2;

    /// <summary>Custom fumble description template</summary>
    public string? FumbleDescriptionTemplate { get; set; }

    /// <summary>Custom fumble consequence script (JSON)</summary>
    public string? FumbleConsequenceScript { get; set; }

    /// <summary>Alternative fumble type on critical fumble (-3 or worse)</summary>
    public FumbleType? CriticalFumbleType { get; set; }
}
```

---

## 8. GUI Feedback

### 8.1 Fumble Notification

**Visual Design**:
```
┌─────────────────────────────────────────┐
│ ⚠ FUMBLE!                               │
├─────────────────────────────────────────┤
│ The rusted lever snaps off in your      │
│ hands with a sharp crack!               │
│                                         │
│ [BreakObject] Lever is now unusable     │
│ [+5 Stress] Fear and frustration        │
│                                         │
│ "You'll need to find another way..."    │
│                                         │
│                          [Acknowledge]  │
└─────────────────────────────────────────┘
```

### 8.2 FumbleNotificationViewModel

```csharp
public class FumbleNotificationViewModel : ViewModelBase
{
    public string Title { get; set; } = "FUMBLE!";
    public string TitleColor { get; set; } = "#DC143C"; // Crimson

    public string Description { get; set; } = string.Empty;
    public string NarrativeText { get; set; } = string.Empty;

    public ObservableCollection<FumbleEffectViewModel> Effects { get; }

    public int StressGained { get; set; }
    public bool ShowStress => StressGained > 0;

    public int CorruptionGained { get; set; }
    public bool ShowCorruption => CorruptionGained > 0;

    public string GuidanceText { get; set; } = string.Empty;

    public ICommand AcknowledgeCommand { get; }
}

public class FumbleEffectViewModel : ViewModelBase
{
    public string Icon { get; set; } = string.Empty;
    public string EffectText { get; set; } = string.Empty;
    public string EffectColor { get; set; } = "#FF6B6B";
}
```

### 8.3 Combat Log Integration

| Event | Format | Color |
|-------|--------|-------|
| Fumble Trigger | "[Name] fumbles the [action]!" | Red (#DC143C) |
| Fumble Effect | "  > [Effect description]" | Orange (#FF8C00) |
| Stress Gain | "  > +[X] Psychic Stress" | Purple (#9400D3) |
| Object Break | "  > [Object] is now [Broken]" | Gray (#808080) |

### 8.4 Fumble Warning (Pre-Roll)

When attempting a check with >10% fumble chance:

```
┌─────────────────────────────────────────┐
│ ⚠ Risky Action                          │
├─────────────────────────────────────────┤
│ Attempting: Pull Rusted Lever           │
│ Check: MIGHT (4 dice) vs DC 4           │
│                                         │
│ Fumble Chance: ~31%                     │
│ Fumble Consequence: Lever breaks        │
│                                         │
│ [Attempt Anyway]  [Find Alternative]    │
└─────────────────────────────────────────┘
```

---

## 9. Balance & Tuning

### 9.1 Target Fumble Rates

| Character Competency | Target Fumble Rate | Scenario |
|---------------------|-------------------|----------|
| Specialized (8+ dice) | 1-5% | Attempting specialty tasks |
| Competent (6-7 dice) | 5-15% | Moderate challenges |
| Average (4-5 dice) | 15-30% | Difficult challenges |
| Weak (3 dice) | 30-50% | Very difficult challenges |

### 9.2 Tunable Parameters

| Parameter | Location | Current | Range | Impact |
|-----------|----------|---------|-------|--------|
| FumbleThreshold | GameConstants.cs | -2 | -3 to -1 | Fumble frequency |
| FumbleStressMultiplier | TraumaConfig.json | 1.0 | 0.5-2.0 | Stress per fumble |
| CriticalFumbleThreshold | GameConstants.cs | -4 | -5 to -3 | Extra-bad fumbles |
| FumbleWarningThreshold | UIConfig.json | 0.10 | 0.05-0.25 | When to warn player |

### 9.3 Known Balance Considerations

| Issue | Description | Mitigation |
|-------|-------------|------------|
| Low-stat punishment | Low attribute characters fumble constantly | Fumble immunity from specialization |
| Retry avoidance | Players never attempt risky checks | Make alternatives also have costs |
| Snowballing | Fumble stress leads to more fumbles | Stress doesn't affect fumble chance |
| Run-ending | Single fumble can doom a run | Always provide alternative paths |

---

## 10. Implementation Guidance

### 10.1 Service Architecture

```csharp
public interface IFumbleService
{
    /// <summary>Check if roll result is a fumble</summary>
    bool IsFumble(int netSuccesses, FumbleContext context);

    /// <summary>Generate fumble result based on context</summary>
    FumbleResult GenerateFumble(
        int characterId,
        FumbleContext context,
        FumbleType type,
        int severity);

    /// <summary>Apply fumble effects to game state</summary>
    void ApplyFumble(FumbleResult fumble, GameState state);

    /// <summary>Calculate fumble probability for UI display</summary>
    float CalculateFumbleProbability(int dicePool, int dc);

    /// <summary>Check if fumble can be mitigated</summary>
    bool CanMitigateFumble(FumbleResult fumble, PlayerCharacter character);

    /// <summary>Attempt to mitigate fumble (costs resources)</summary>
    bool AttemptMitigation(FumbleResult fumble, PlayerCharacter character);
}
```

### 10.2 Implementation Checklist

**Core System**:
- [ ] Create `FumbleType` enum
- [ ] Create `FumbleResult` class
- [ ] Create `FumbleEffect` class
- [ ] Implement `IFumbleService`
- [ ] Integrate with `DiceService` for fumble detection

**Integration**:
- [ ] Add fumble check to `ObjectInteractionService`
- [ ] Add fumble check to `CombatEngine`
- [ ] Add fumble properties to `InteractiveObject`
- [ ] Integrate with `TraumaEconomyService` for stress

**GUI**:
- [ ] Create `FumbleNotificationViewModel`
- [ ] Create fumble notification view
- [ ] Add fumble warning to interaction dialogs
- [ ] Integrate with combat log

**Content**:
- [ ] Define fumble consequences for all interaction types
- [ ] Create fumble description templates
- [ ] Configure fumble severity for existing objects

---

## Appendix A: Fumble Description Templates

### A.1 Physical Interaction Templates

**Pull Fumble**:
```
"The {object_name} snaps off in your hands with a {sound}!
{damage_text}The mechanism is now permanently disabled."
```

**Push Fumble**:
```
"You put your weight into the {object_name}, but your footing slips!
You stumble {direction}, {injury_text}."
```

**Turn Fumble**:
```
"The {object_name} resists, then suddenly gives way with a metallic
shriek as the {component} shears off. {consequence_text}"
```

**Press Fumble**:
```
"Your finger slips, hitting {wrong_button} instead! {alarm_text}"
```

### A.2 Variable Substitutions

| Variable | Source | Example |
|----------|--------|---------|
| `{object_name}` | InteractiveObject.Name | "rusted lever" |
| `{sound}` | FumbleDescriptor | "sharp crack", "grinding snap" |
| `{damage_text}` | If SelfInjury | "A shard cuts your hand for 3 damage. " |
| `{direction}` | Random | "backward", "to the side" |
| `{component}` | Object-specific | "handle", "valve stem", "gear teeth" |
| `{wrong_button}` | Context | "the alarm button" |

---

## Appendix B: Fumble Recovery Options

### B.1 Mitigation Abilities

| Ability | Source | Effect |
|---------|--------|--------|
| "Steady Recovery" | Trait | Reduce fumble severity by 1 tier |
| "Jury Rig" | Scrap-Tinker | Temporarily repair broken object |
| "Second Chance" | Meta-progression | Reroll one fumble per run |
| "Unshakeable" | Warrior Mastery | Immune to ApplyStatus fumbles |

### B.2 Resource-Based Mitigation

| Resource | Cost | Effect |
|----------|------|--------|
| Repair Kit | 1 use | Repair damaged equipment |
| Calming Draught | 1 use | Negate stress from fumble |
| Emergency Override | 1 use | Bypass broken mechanism |

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Nov 2024 | Initial Fumble System specification |

---

*This document is part of the Rune & Rust technical documentation suite.*
*Related: PHYSICAL_INTERACTION_SPECIFICATION.md, TRAUMA_ECONOMY_SPEC.md*
