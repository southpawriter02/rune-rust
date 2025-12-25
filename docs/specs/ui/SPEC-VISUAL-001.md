---
id: SPEC-VISUAL-001
title: Visual Effects System
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-THEME-001, SPEC-COMBAT-001, SPEC-SETTINGS-001]
---

# SPEC-VISUAL-001: Visual Effects System

> **Version:** 1.0.1
> **Status:** Implemented
> **Service:** `VisualEffectService`
> **Location:** `RuneAndRust.Terminal/Services/VisualEffectService.cs`

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

The Visual Effects System provides real-time visual feedback during gameplay through border flash effects. Introduced in **v0.3.9a "The Impact"**, this system enables immediate player feedback for combat events such as damage, critical hits, healing, trauma, and victory.

The implementation uses Spectre.Console color overrides to flash the combat grid border, creating a subtle but effective feedback mechanism that doesn't disrupt gameplay. All effects respect the `GameSettings.ReduceMotion` accessibility setting, allowing players with motion sensitivity to disable visual effects entirely.

The system is designed for extensibility, with a clear separation between effect types (enumerated) and their visual representations (color mapping), enabling future effect additions without service modifications.

---

## Core Concepts

### VisualEffectType Enum

Defines the available visual effect types, each with a specific purpose and color:

```csharp
public enum VisualEffectType
{
    None = 0,          // No visual effect
    DamageFlash = 1,   // Red border - player/enemy takes damage
    CriticalFlash = 2, // Gold border - critical hit landed
    HealFlash = 3,     // Green border - healing occurs
    TraumaFlash = 4,   // Purple border - stress/trauma event
    VictoryFlash = 5   // Bright gold - combat victory
}
```

### Intensity Scaling

Effects support intensity levels (1-3) that scale the flash duration:

| Intensity | Duration | Use Case |
|-----------|----------|----------|
| 1 (Low) | 150ms | Minor events (chip damage) |
| 2 (Medium) | 300ms | Significant events (heavy hit) |
| 3 (High) | 450ms | Major events (critical hit, near-death) |

**Formula:** `duration = 150ms × intensity`

### Border Override Pattern

The service maintains a single `_borderOverride` string that renderers can query:

1. Effect triggers → sets border color override
2. Renderer checks `GetBorderOverride()` during render
3. If non-null, renderer uses override color for border
4. After delay, service clears override
5. Next render uses default border color

---

## Behaviors

### Primary Behaviors

#### 1. Trigger Effect (`TriggerEffectAsync`)

```csharp
Task TriggerEffectAsync(VisualEffectType effectType, int intensity = 1)
```

**Purpose:** Triggers a visual effect with specified type and intensity.

**Logic:**
1. Check `GameSettings.ReduceMotion` - if true, exit immediately
2. Check if `effectType == None` - if true, exit immediately
3. Get color for effect type via `GetColorForEffect()`
4. Clamp intensity to 1-3 range
5. Calculate duration: `150 * intensity` milliseconds
6. Set border override to effect color
7. Wait for calculated duration
8. Clear border override

**Example:**
```csharp
// Player takes critical hit - high intensity flash
await visualEffectService.TriggerEffectAsync(VisualEffectType.CriticalFlash, intensity: 3);

// Minor healing - low intensity flash
await visualEffectService.TriggerEffectAsync(VisualEffectType.HealFlash, intensity: 1);
```

#### 2. Set Border Override (`SetBorderOverride`)

```csharp
void SetBorderOverride(string? colorOverride)
```

**Purpose:** Sets the border color override for renderers to use.

**Logic:**
1. Store the color string in `_borderOverride`
2. Log the change for traceability

**Example:**
```csharp
// Direct override (not typically called externally)
visualEffectService.SetBorderOverride("red");
```

#### 3. Get Border Override (`GetBorderOverride`)

```csharp
string? GetBorderOverride()
```

**Purpose:** Returns the current border override color for renderers.

**Logic:**
1. Return `_borderOverride` (may be null)

**Example:**
```csharp
// In CombatGridRenderer
var borderColor = visualEffectService.GetBorderOverride();
if (borderColor != null)
{
    // Use override color
    panel.BorderStyle = Style.Parse(borderColor);
}
else
{
    // Use default color
    panel.BorderStyle = Style.Parse("white");
}
```

#### 4. Clear Border Override (`ClearBorderOverride`)

```csharp
void ClearBorderOverride()
```

**Purpose:** Clears any active border override, returning to default styling.

**Logic:**
1. Set `_borderOverride = null`
2. Log the clear action

---

## Restrictions

### What This System MUST NOT Do

1. **No Blocking Main Thread:** All effects are async with `Task.Delay`, never blocking the render loop.

2. **No Ignoring ReduceMotion:** The accessibility setting must be checked before every effect. No bypass.

3. **No Direct Rendering:** The service only sets state. Renderers are responsible for checking and applying the override.

4. **No Permanent Overrides:** Effects must always clear the override after their duration expires.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Max Intensity | 3 | Prevents excessively long flashes (capped at 450ms) |
| Min Intensity | 1 | Ensures minimum visible duration (150ms) |
| Concurrent Effects | 1 | Only one border override can be active; later effects overwrite earlier |
| Color Palette | Fixed | Effect colors are hardcoded; no theme integration yet |

---

## Use Cases

### UC-1: Player Takes Damage

**Actor:** CombatService
**Trigger:** Player character receives damage from enemy attack
**Preconditions:** Combat is active, player HP reduced

```csharp
// In CombatService after damage calculation
await _visualEffectService.TriggerEffectAsync(
    VisualEffectType.DamageFlash,
    intensity: damage > 20 ? 2 : 1
);
```

**Postconditions:** Border flashes red for 150-300ms depending on damage severity

### UC-2: Critical Hit Landed

**Actor:** AttackResolutionService
**Trigger:** Attack roll results in critical hit
**Preconditions:** Combat is active, attack succeeded with critical

```csharp
// In AttackResolutionService after critical determination
await _visualEffectService.TriggerEffectAsync(
    VisualEffectType.CriticalFlash,
    intensity: 3  // Always max intensity for crits
);
```

**Postconditions:** Border flashes gold for 450ms

### UC-3: Healing Applied

**Actor:** AbilityService
**Trigger:** Healing ability or item used
**Preconditions:** Character receives HP restoration

```csharp
// After healing applied
await _visualEffectService.TriggerEffectAsync(
    VisualEffectType.HealFlash,
    intensity: Math.Min(3, healAmount / 10)
);
```

**Postconditions:** Border flashes green proportional to heal amount

### UC-4: Player with ReduceMotion Enabled

**Actor:** System
**Trigger:** Any effect triggered while accessibility setting is on
**Preconditions:** `GameSettings.ReduceMotion = true`

```csharp
// Effect is triggered but immediately exits
GameSettings.ReduceMotion = true;
await _visualEffectService.TriggerEffectAsync(VisualEffectType.DamageFlash);
// No visual change occurs
```

**Postconditions:** No border change, no delay, immediate return

---

## Decision Trees

### Effect Trigger Flow

```
┌─────────────────────────────┐
│  TriggerEffectAsync()       │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │ ReduceMotion?   │
    └────────┬────────┘
             │
    ┌────────┼────────┐
    │ YES    │        │ NO
    ▼        │        ▼
  Return     │    ┌────────────┐
 (no-op)     │    │EffectType? │
             │    └────┬───────┘
             │         │
             │    ┌────┼────┐
             │    │None│    │Other
             │    ▼    │    ▼
             │  Return │  Get Color
             │         │    │
             │         │    ▼
             │         │  Set Override
             │         │    │
             │         │    ▼
             │         │  Wait (duration)
             │         │    │
             │         │    ▼
             │         │  Clear Override
             │         │    │
             └─────────┴────┘
```

### Color Resolution

```
┌─────────────────────────────┐
│  GetColorForEffect(type)    │
└────────────┬────────────────┘
             │
    ┌────────┼────────────────────────┐
    │        │        │        │      │
    ▼        ▼        ▼        ▼      ▼
Damage   Critical   Heal    Trauma Victory
   │         │        │        │      │
   ▼         ▼        ▼        ▼      ▼
 "red"    "gold1"  "green" "magenta1" "bold gold1"
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `GameSettings` | [SPEC-SETTINGS-001](./SPEC-SETTINGS-001.md) | Checks `ReduceMotion` accessibility flag |
| `ILogger` | (Framework) | Structured logging for traceability |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](../combat/SPEC-COMBAT-001.md) | Triggers damage/victory effects |
| `AttackResolutionService` | [SPEC-ATTACK-001](../combat/SPEC-ATTACK-001.md) | Triggers critical hit effects |
| `CombatGridRenderer` | (Terminal Layer) | Reads border override for rendering |
| `TraumaService` | [SPEC-TRAUMA-001](../character/SPEC-TRAUMA-001.md) | Triggers trauma flash effects |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `VisualEffectService.cs` | Main service implementation | 1-106 |
| `VisualEffectType.cs` | Effect type enum | 1-38 |
| `IVisualEffectService.cs` | Interface contract | 1-37 |

### Integration Points

| File | Purpose | Key Lines |
|------|---------|-----------|
| `CombatGridRenderer.cs` | Reads border override for panel styling | (border check) |
| `GameSettings.cs` | Provides ReduceMotion flag | 16 |

---

## Data Models

### VisualEffectType

```csharp
public enum VisualEffectType
{
    None = 0,          // Skip effect processing
    DamageFlash = 1,   // Red border on damage
    CriticalFlash = 2, // Gold border on critical hit
    HealFlash = 3,     // Green border on healing
    TraumaFlash = 4,   // Purple border on trauma/stress
    VictoryFlash = 5   // Bright gold on combat victory
}
```

### Color Mapping

| Effect Type | Spectre.Console Color | Visual Result |
|-------------|----------------------|---------------|
| DamageFlash | `"red"` | Bright red border |
| CriticalFlash | `"gold1"` | Golden yellow border |
| HealFlash | `"green"` | Vibrant green border |
| TraumaFlash | `"magenta1"` | Purple/magenta border |
| VictoryFlash | `"bold gold1"` | Bold golden border |

---

## Configuration

### Constants

```csharp
// Base duration per intensity level
private const int BaseDurationMs = 150;

// Intensity clamp range
private const int MinIntensity = 1;
private const int MaxIntensity = 3;
```

### Settings

| Setting | Default | Location | Purpose |
|---------|---------|----------|---------|
| ReduceMotion | `false` | `GameSettings.ReduceMotion` | Disables all visual effects when true |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `VisualEffectServiceTests.cs` | 15 | All effect types, intensity, ReduceMotion |

### Critical Test Scenarios

1. **SetBorderOverride stores color** - Verifies override is stored and retrievable
2. **SetBorderOverride accepts null** - Verifies null clearing works
3. **SetBorderOverride overwrites previous** - Verifies no accumulation
4. **GetBorderOverride returns null initially** - Verifies clean state
5. **GetBorderOverride returns set color** - Verifies retrieval after set
6. **ClearBorderOverride removes override** - Verifies clearing mechanism
7. **ClearBorderOverride is idempotent** - Safe to call multiple times
8. **TriggerEffectAsync skips when ReduceMotion enabled** - Accessibility compliance
9. **TriggerEffectAsync skips when None type** - No-op for None
10. **TriggerEffectAsync clears override after delay** - Self-cleanup
11. **TriggerEffectAsync uses correct color per type** - Color mapping validation (Theory)
12. **TriggerEffectAsync scales duration with intensity** - Duration calculation (Theory)
13. **TriggerEffectAsync clamps intensity to minimum** - Prevents zero/negative
14. **TriggerEffectAsync clamps intensity to maximum** - Prevents excessive duration
15. **TriggerEffectAsync respects ReduceMotion changed mid-session** - Dynamic setting check

### Validation Checklist

- [x] All effect types map to valid colors
- [x] ReduceMotion bypasses all effects
- [x] Intensity clamping works correctly
- [x] Border override cleared after effect completes
- [x] No blocking during effect duration

---

## Design Rationale

### Why Border Flash Instead of Screen Effects?

- **Performance:** Border-only effects are lightweight, no full-screen redraw needed
- **Subtlety:** Doesn't disrupt combat grid readability
- **Compatibility:** Works with any terminal that supports Spectre.Console colors
- **Accessibility:** Easy to disable entirely via ReduceMotion

### Why Async with Task.Delay?

- **Non-blocking:** Game loop continues during effect
- **Precise timing:** `Task.Delay` provides consistent duration
- **Cancellable:** Could be extended to support effect cancellation
- **Composable:** Multiple systems can trigger effects without coordination

### Why Single Border Override State?

- **Simplicity:** No complex effect queue or layering
- **Predictable:** Latest effect always wins
- **Fast:** No iteration over effect list during render
- **Clear cleanup:** Simple null check in renderer

### Why Hardcoded Colors Instead of Theme Integration?

- **Phase 1 implementation:** Future work could add theme-aware effect colors
- **Consistency:** Effect colors have semantic meaning (red=danger) regardless of theme
- **Accessibility themes:** Color-blind themes may need specialized effect colors anyway

---

## Changelog

### v1.0.1 (2025-12-25) - Documentation Accuracy

**Fixed:**
- Corrected status field capitalization: implemented → Implemented
- Added missing test scenario #5 (GetBorderOverride returns set color) to critical test checklist

**Added:**
- Code traceability remarks to VisualEffectType.cs

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.9a implementation
- Documents VisualEffectService, VisualEffectType enum
- Covers intensity scaling, ReduceMotion accessibility
- Includes border override pattern for renderer integration
- References 15 tests with full coverage
