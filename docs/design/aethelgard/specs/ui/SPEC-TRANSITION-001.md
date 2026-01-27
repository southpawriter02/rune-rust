---
id: SPEC-TRANSITION-001
title: Screen Transition System
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-THEME-001, SPEC-GAME-001, SPEC-UI-001]
---

# SPEC-TRANSITION-001: Screen Transition System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `ScreenTransitionService`
> **Location:** `RuneAndRust.Terminal/Services/ScreenTransitionService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The **Screen Transition System** provides ASCII-based visual effects for game phase changes, adding "juice" to the Terminal UI experience. Transitions play automatically when the game moves between phases (e.g., Exploration → Combat), using `Spectre.Console.Live` for frame-based animation rendering.

### Key Responsibilities

1. **Phase Transition Animations**: Play visual effects when game phase changes
2. **Accessibility Compliance**: Respect `GameSettings.ReduceMotion` by skipping all animations
3. **Theme Integration**: Use semantic colors from `IThemeService` for themed animation colors
4. **Non-Blocking Completion**: Animations block until complete, ensuring clean screen state

### Architecture Pattern

```
GameService Loop → Phase Change Detected → HandlePhaseTransitionAsync()
                                                    ↓
                                          Determine TransitionType
                                                    ↓
                                   IScreenTransitionService.PlayAsync(type)
                                                    ↓
                                    ReduceMotion Check → Skip if enabled
                                                    ↓
                                        Render Animation Frames
                                                    ↓
                                          AnsiConsole.Clear()
```

**Key Design Decision**: The system is **optional** (nullable injection) to avoid breaking existing tests. When not injected, phase transitions occur instantly without animation.

**Technology Stack**:
- **Spectre.Console.Live**: Frame-based terminal rendering
- **GameSettings.ReduceMotion**: Accessibility toggle
- **IThemeService**: Semantic color lookup for animation colors

---

## Core Concepts

### 1. TransitionType Enum

**Definition**: Enumeration of available screen transition animation types.

```csharp
public enum TransitionType
{
    None = 0,       // Instant screen clear
    Shatter = 1,    // Red noise fills screen (Combat start)
    Dissolve = 2,   // Green dots fade out (Combat victory)
    GlitchDecay = 3 // Text decays to garbage (Game Over)
}
```

**Type Mapping**:

| TransitionType | Phase Change | Visual Effect | Theme Color |
|----------------|--------------|---------------|-------------|
| `None` | Default/Other | Instant clear | N/A |
| `Shatter` | Exploration → Combat | Red noise fills screen | `EnemyColor` |
| `Dissolve` | Combat → Exploration | Green dots fade out | `SuccessColor` |
| `GlitchDecay` | Combat → Quit | Garbage text decay | `StressHigh` |

---

### 2. Phase Transition Matrix

**Definition**: Mapping of phase changes to transition effects.

```
┌─────────────────────────────────────────────────────────────┐
│                    PHASE TRANSITION MATRIX                   │
├─────────────────┬─────────────────┬─────────────────────────┤
│ From Phase      │ To Phase        │ Transition Effect       │
├─────────────────┼─────────────────┼─────────────────────────┤
│ Exploration     │ Combat          │ Shatter (red noise)     │
│ Combat          │ Exploration     │ Dissolve (green fade)   │
│ Combat          │ Quit            │ GlitchDecay (garbage)   │
│ MainMenu        │ Exploration     │ None (instant)          │
│ MainMenu        │ Quit            │ None (instant)          │
│ Any Other       │ Any Other       │ None (instant)          │
└─────────────────┴─────────────────┴─────────────────────────┘
```

---

### 3. Accessibility Guard (ReduceMotion)

**Definition**: When `GameSettings.ReduceMotion` is enabled, all animations are skipped instantly.

**Behavior**:
```csharp
if (GameSettings.ReduceMotion)
{
    _logger.LogDebug("[VFX] Transition skipped (ReduceMotion: ON)");
    AnsiConsole.Clear();
    return;
}
```

**Compliance**: WCAG 2.3.3 (Animation from Interactions) - users can disable motion animations.

---

## Behaviors

### Primary Behaviors

#### 1. Play Transition (`PlayAsync`)

```csharp
Task PlayAsync(TransitionType type)
```

**Purpose**: Plays a screen transition animation, blocking until complete.

**Logic**:
1. Check `GameSettings.ReduceMotion` - if enabled, clear screen and return
2. If `type == None`, clear screen and return
3. Log transition start
4. Execute effect-specific animation method
5. Clear screen when complete
6. Log transition complete

**Example**:
```csharp
await _transitionService.PlayAsync(TransitionType.Shatter);
```

---

#### 2. Shatter Effect (`PlayShatterEffectAsync`)

**Purpose**: Dramatic red noise fills screen for combat entry.

**Algorithm**:
1. Get `EnemyColor` from theme service
2. For each frame (1 to 10):
   - Calculate density: `frame / FrameCount` (10% → 100%)
   - Build grid with random characters at density %
   - Render frame with Live context
   - Delay 50ms
3. Clear screen

**Character Set**: `/ \ # X % & █ ▓ ▒`

---

#### 3. Dissolve Effect (`PlayDissolveEffectAsync`)

**Purpose**: Calming green fade-out for combat victory.

**Algorithm**:
1. Get `SuccessColor` from theme service
2. For each frame (1 to 10):
   - Calculate density: `(FrameCount - frame) / FrameCount` (100% → 10%)
   - Build grid with dot character (`·`) at density %
   - Render frame with Live context
   - Delay 50ms
3. Clear screen

---

#### 4. GlitchDecay Effect (`PlayGlitchDecayEffectAsync`)

**Purpose**: Ominous garbage text decay for game over.

**Algorithm**:
1. Get `StressHigh` from theme service
2. Frames 1-7: Render glitch characters at 60% density
3. Frames 8-10: Fade to black (increasing empty space overlay)
4. Delay 70ms between frames (700ms total)
5. Clear screen

**Character Set**: `░ ▒ ▓ █ ▀ ▄ ▌ ▐`

---

#### 5. Handle Phase Transition (`HandlePhaseTransitionAsync`)

```csharp
private async Task HandlePhaseTransitionAsync(GamePhase from, GamePhase to)
```

**Purpose**: Determines and plays appropriate transition for phase change.

**Logic**:
```csharp
var transitionType = (from, to) switch
{
    (GamePhase.Exploration, GamePhase.Combat) => TransitionType.Shatter,
    (GamePhase.Combat, GamePhase.Exploration) => TransitionType.Dissolve,
    (GamePhase.Combat, GamePhase.Quit) => TransitionType.GlitchDecay,
    _ => TransitionType.None
};

await _transitionService.PlayAsync(transitionType);
```

---

## Restrictions

### What This System MUST NOT Do

1. **Block Indefinitely**: All animations must complete within timeout (700ms max)
2. **Ignore ReduceMotion**: Accessibility setting must always be respected
3. **Crash on Missing Theme**: Use fallback colors if theme lookup fails
4. **Render Without Console**: Skip rendering if `Console.WindowWidth` is 0 (test environment)

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Max Animation Duration | 700ms | User experience - too long feels sluggish |
| Frame Count | 10 | Balance between smoothness and performance |
| Frame Delay | 50-70ms | Visible animation speed |
| Max Terminal Height | 24 | Avoid memory issues with huge terminals |

---

## Use Cases

### UC-1: Combat Entry Transition

**Actor:** Game System
**Trigger:** Player moves to room with enemies, combat initiates
**Preconditions:** Phase is Exploration, enemies present in room

```csharp
// In GameService, phase changes from Exploration to Combat
await HandlePhaseTransitionAsync(GamePhase.Exploration, GamePhase.Combat);
// Shatter effect plays: red noise fills screen over 500ms
```

**Postconditions:** Screen cleared, combat UI ready to render

---

### UC-2: Victory Transition

**Actor:** Game System
**Trigger:** All enemies defeated in combat
**Preconditions:** Phase is Combat, no living enemies

```csharp
// In GameService, phase changes from Combat to Exploration
await HandlePhaseTransitionAsync(GamePhase.Combat, GamePhase.Exploration);
// Dissolve effect plays: green dots fade out over 500ms
```

**Postconditions:** Screen cleared, exploration UI ready to render

---

### UC-3: Game Over Transition

**Actor:** Game System
**Trigger:** Player dies and quits from death screen
**Preconditions:** Phase is Combat, player HP <= 0, player chooses quit

```csharp
// In GameService, phase changes from Combat to Quit
await HandlePhaseTransitionAsync(GamePhase.Combat, GamePhase.Quit);
// GlitchDecay effect plays: garbage text fades to black over 700ms
```

**Postconditions:** Screen cleared, application exits

---

### UC-4: Accessibility Skip

**Actor:** Player with motion sensitivity
**Trigger:** Any phase transition with ReduceMotion enabled
**Preconditions:** `GameSettings.ReduceMotion == true`

```csharp
// Any PlayAsync call with ReduceMotion ON
await _transitionService.PlayAsync(TransitionType.Shatter);
// Returns immediately after AnsiConsole.Clear()
```

**Postconditions:** Screen cleared instantly, no animation rendered

---

## Decision Trees

### Phase Transition Decision Tree

```
┌─────────────────────────────────────┐
│  Phase Change Detected              │
│  (from, to)                         │
└───────────────┬─────────────────────┘
                │
    ┌───────────┴───────────┐
    │ from == Exploration?  │
    └───────────┬───────────┘
                │
        ┌───────┴───────┐
        │ YES           │ NO
        ▼               ▼
   ┌─────────────┐  ┌─────────────┐
   │to == Combat?│  │from==Combat?│
   └──────┬──────┘  └──────┬──────┘
          │                │
    ┌─────┴─────┐    ┌─────┴─────┐
    │YES        │NO  │YES        │NO
    ▼           ▼    ▼           ▼
 SHATTER     NONE  ┌───────────┐ NONE
                   │to==Explore│
                   │or to==Quit│
                   └─────┬─────┘
                         │
                   ┌─────┴─────┐
                   │Explore    │Quit
                   ▼           ▼
                DISSOLVE  GLITCH_DECAY
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IThemeService` | [SPEC-THEME-001](./SPEC-THEME-001.md) | Semantic color lookup for animation colors |
| `GameSettings` | N/A | ReduceMotion accessibility check |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `GameService` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | Phase transition detection and animation triggering |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `ScreenTransitionService.cs` | Animation rendering service | Full file |
| `TransitionType.cs` | Transition type enum | Full file |
| `IScreenTransitionService.cs` | Service interface | Full file |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `GameService.cs` | Phase transition integration | HandlePhaseTransitionAsync method |
| `Program.cs` | DI registration | Line ~162 |

---

## Data Models

### TransitionType Enum

```csharp
namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines screen transition animation types (v0.3.14b).
/// Used by IScreenTransitionService to play visual effects between game phases.
/// </summary>
public enum TransitionType
{
    /// <summary>No transition - instant screen clear.</summary>
    None = 0,

    /// <summary>Screen shatters into noise - Combat start.</summary>
    Shatter = 1,

    /// <summary>Screen fades/dissolves out - Combat victory.</summary>
    Dissolve = 2,

    /// <summary>Text decays to garbage then black - Game Over (quit after death).</summary>
    GlitchDecay = 3
}
```

### IScreenTransitionService Interface

```csharp
namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for playing screen transition animations between game phases (v0.3.14b).
/// Implements ASCII-based visual effects using Spectre.Console.Live rendering.
/// </summary>
public interface IScreenTransitionService
{
    /// <summary>
    /// Plays a transition animation. Blocks until complete.
    /// Respects GameSettings.ReduceMotion - returns immediately if enabled.
    /// </summary>
    /// <param name="type">The transition effect to play.</param>
    /// <returns>A task that completes when the transition finishes.</returns>
    Task PlayAsync(TransitionType type);
}
```

---

## Configuration

### Constants

```csharp
// Animation timing
private const int FrameCount = 10;      // Frames per animation
private const int FrameDelayMs = 50;    // Standard delay (500ms total)
// GlitchDecay uses: FrameDelayMs + 20 = 70ms per frame (700ms total)
private const int MaxHeight = 24;       // Maximum terminal height

// Character sets
private static readonly char[] ShardChars = { '/', '\\', '#', 'X', '%', '&', '█', '▓', '▒' };
private static readonly char[] GlitchChars = { '░', '▒', '▓', '█', '▀', '▄', '▌', '▐' };
```

### Settings

| Setting | Default | Range | Purpose |
|---------|---------|-------|---------|
| `GameSettings.ReduceMotion` | false | true/false | Skip all animations when enabled |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `ScreenTransitionServiceTests.cs` | 8 | ReduceMotion, transitions, theme integration, logging |

### Critical Test Scenarios

1. **ReduceMotion Skips Animation**: All transition types complete in <100ms when ReduceMotion is enabled
2. **Theme Color Integration**: Correct semantic color keys used for each transition type
3. **Exception-Free Handling**: All TransitionType values handled without exception
4. **Logging Behavior**: Debug log emitted when ReduceMotion skips animation

### Test Categories

**ReduceMotion Tests (3 tests)**:
- `PlayAsync_SkipsAnimation_WhenReduceMotionEnabled` (Shatter)
- `PlayAsync_SkipsAnimation_WhenReduceMotionEnabled_Dissolve`
- `PlayAsync_SkipsAnimation_WhenReduceMotionEnabled_GlitchDecay`

**TransitionType Handling Tests (5 tests)**:
- `PlayAsync_HandlesAllTransitionTypes_WithReduceMotion` (Theory: None, Shatter, Dissolve, GlitchDecay)
- `PlayAsync_WithNone_CompletesInstantly`

**Theme Integration Tests (4 tests)**:
- `Constructor_InjectsThemeService`
- `ThemeService_IsConfiguredForTransitionColors` (Theory: Shatter/EnemyColor, Dissolve/SuccessColor, GlitchDecay/StressHigh)

**Logging Tests (1 test)**:
- `PlayAsync_LogsDebug_WhenReduceMotionSkips`

### Validation Checklist

- [x] All 8 tests passing
- [x] ReduceMotion respected for all transition types
- [x] Theme service integration verified
- [x] No console rendering in test environment (prevents test failures)

---

## Design Rationale

### Why Optional IScreenTransitionService Injection?

- **Problem**: Required injection would break existing GameService tests
- **Decision**: Optional parameter with `= null` default
- **Benefit**: Non-invasive integration, existing tests unchanged

### Why Track _previousPhase in Game Loop?

- **Problem**: Phase changes occur via direct `state.Phase` assignment in multiple locations
- **Decision**: Track `_previousPhase` at loop start, compare each iteration
- **Benefit**: Detects any phase change regardless of source

### Why GlitchDecay on Quit After Death (Not Immediate Death)?

- **Problem**: Should death transition play at HP=0 or quit confirmation?
- **Decision**: On `(Combat, Quit)` transition (quit after death)
- **Rationale**: Player should see death state before dramatic fade; quit is definitive "game over"

### Why Grey as Ultimate Fallback Color?

- **Problem**: Missing theme colors would cause rendering issues
- **Decision**: Use grey as fallback (via ThemeService fallback chain)
- **Benefit**: Always renders, visible on light/dark backgrounds

---

## Changelog

### v1.0.1 (2025-12-25) - Documentation Accuracy

**Fixed:**
- Corrected test count: 13 → 8 (test methods, not counting Theory data points)
- Clarified GlitchDecay delay: uses `FrameDelayMs + 20` (computed value, not named constant)

**Added:**
- Code traceability remarks to 3 implementation files

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.14b implementation
- 4 transition types: None, Shatter, Dissolve, GlitchDecay
- ReduceMotion accessibility compliance
- Theme integration via IThemeService
- 8 unit tests, 100% passing
