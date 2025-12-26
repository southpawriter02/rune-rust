# TraumaEconomyService

Parent item: Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md)

**File Path:** `RuneAndRust.Engine/TraumaEconomyService.cs`**Version:** v0.15
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `TraumaEconomyService` manages the psychological horror mechanics of Rune & Rust through two interconnected systems:

1. **Psychic Stress** - Mental strain that accumulates during gameplay, triggering Breaking Points at 100
2. **Runic Blight Corruption** - Permanent transformation caused by exposure to corrupted technology

Both systems create meaningful consequences for player decisions and reinforce the game's horror atmosphere.

---

## Architecture

### Dual-Resource System

```
┌─────────────────────────────────────────────────────────────┐
│                   TRAUMA ECONOMY                            │
├─────────────────────────┬───────────────────────────────────┤
│    PSYCHIC STRESS       │    RUNIC BLIGHT CORRUPTION        │
│    (0-100, Resettable)  │    (0-100, Permanent)             │
├─────────────────────────┼───────────────────────────────────┤
│ • Environmental horror  │ • Using heretical abilities       │
│ • Combat stress         │ • Touching corrupted artifacts    │
│ • Traumatic events      │ • Prolonged Blight exposure       │
├─────────────────────────┼───────────────────────────────────┤
│ Thresholds:             │ Thresholds:                       │
│  0-25: Safe             │  0-20: Minimal                    │
│  26-50: Strained        │  21-40: Low                       │
│  51-75: Severe          │  41-60: Moderate                  │
│  76-99: Critical        │  61-80: High                      │
│  100: BREAKING POINT    │  81-100: Extreme → TERMINAL       │
├─────────────────────────┼───────────────────────────────────┤
│ Recovery:               │ Recovery:                         │
│ • Sanctuary Rest: Full  │ • NONE (Permanent)                │
│ • Consumables: Partial  │                                   │
└─────────────────────────┴───────────────────────────────────┘

```

---

## Public API

### Stress Management

### `AddStress`

Adds Psychic Stress to a character with optional resistance.

```csharp
public (int stressGained, Trauma? traumaAcquired) AddStress(
    PlayerCharacter character,
    int baseAmount,
    string source = "unknown",
    bool allowResolveCheck = false,
    int resolveSuccesses = 0)

```

**Parameters:**

- `character` - Character gaining stress
- `baseAmount` - Base stress before reductions
- `source` - Cause of stress (for trauma selection)
- `allowResolveCheck` - If true, WILL can reduce stress
- `resolveSuccesses` - Successes from WILL Resolve Check

**Returns:** Tuple of:

- `stressGained` - Actual stress added after reductions
- `traumaAcquired` - Trauma if Breaking Point occurred, null otherwise

**Reduction Formula:**

```
actualAmount = baseAmount - resolveSuccesses
actualAmount = actualAmount × character.GetTraumaStressMultiplier(source)

```

**Example:**

```csharp
// Combat stress with WILL resistance
var (gained, trauma) = traumaService.AddStress(
    player,
    baseAmount: 15,
    source: "combat",
    allowResolveCheck: true,
    resolveSuccesses: willCheck.Successes);

if (trauma != null)
{
    Console.WriteLine($"Breaking Point! Acquired: {trauma.Name}");
}

```

---

### `ClearStress`

Clears all Psychic Stress (Sanctuary Rest).

```csharp
public void ClearStress(PlayerCharacter character)

```

---

### `ApplyPassiveTraumaStress`

Applies stress from trauma-triggered conditions.

```csharp
public int ApplyPassiveTraumaStress(PlayerCharacter character, string condition)

```

**Parameters:**

- `character` - Character with traumas
- `condition` - Environmental condition (e.g., "darkness", "flooded")

**Returns:** Actual stress gained

**Use Case:** Characters with certain traumas gain stress in specific environments (e.g., claustrophobia in tight spaces).

---

### Corruption Management

### `AddCorruption`

Adds Corruption to a character (cannot be resisted).

```csharp
public (int corruptionGained, List<int> thresholdsCrossed) AddCorruption(
    PlayerCharacter character,
    int amount,
    string source = "unknown")

```

**Parameters:**

- `character` - Character gaining corruption
- `amount` - Corruption amount (always applies in full)
- `source` - Cause of corruption

**Returns:** Tuple of:

- `corruptionGained` - Actual corruption added
- `thresholdsCrossed` - List of thresholds crossed (25, 50, 75, 100)

**Threshold Effects:**

| Threshold | Effect |
| --- | --- |
| 25 | +1 Tech checks, -1 Social checks |
| 50 | +2 Tech, -2 Social, cannot gain human faction reputation |
| 75 | Force acquire [MACHINE AFFINITY] trauma, NPCs react with fear |
| 100 | **TERMINAL CORRUPTION** (game over state) |

---

### Threshold Queries

### `GetStressThreshold`

Gets the current stress threshold tier.

```csharp
public StressThreshold GetStressThreshold(PlayerCharacter character)

```

**Returns:** `StressThreshold` enum:

- `Safe` (0-25)
- `Strained` (26-50)
- `Severe` (51-75)
- `Critical` (76-100)

---

### `GetCorruptionThreshold`

Gets the current corruption threshold tier.

```csharp
public CorruptionThreshold GetCorruptionThreshold(PlayerCharacter character)

```

**Returns:** `CorruptionThreshold` enum:

- `Minimal` (0-20)
- `Low` (21-40)
- `Moderate` (41-60)
- `High` (61-80)
- `Extreme` (81-100)

---

### UI Support Methods

| Method | Purpose |
| --- | --- |
| `GetStressColor(threshold)` | Returns ConsoleColor for stress display |
| `GetCorruptionColor(threshold)` | Returns ConsoleColor for corruption display |
| `GetStressThresholdText(threshold)` | Returns "Safe", "Strained", etc. |
| `GetCorruptionThresholdText(threshold)` | Returns "Minimal", "Low", etc. |
| `GenerateMeterBar(current, max, length)` | Generates visual meter `[████░░░░░░]` |
| `CheckForThresholdWarning(character)` | Returns warning message if near dangerous thresholds |

---

### Narrative Generation

### `GetBreakingPointNarrative`

Generates narrative text for Breaking Point events.

```csharp
public string GetBreakingPointNarrative(Trauma trauma, string source)

```

**Returns:** Multi-line narrative text with:

- Atmospheric description
- Trauma name and description
- Effect list
- Stress reset notification

---

### `GetCorruptionThresholdNarrative`

Generates narrative text for corruption threshold crossings.

```csharp
public string GetCorruptionThresholdNarrative(int threshold, PlayerCharacter character)

```

---

## Breaking Point Mechanics

When stress reaches 100:

1. **Trauma Selection** - `TraumaLibrary.SelectTraumaForSource()` picks appropriate trauma
2. **Duplicate Check** - No duplicate traumas acquired
3. **Effect Application** - Trauma effects immediately apply
4. **Stress Reset** - Stress resets to 60 (not 0—character remains rattled)

```
Stress: 95 → +10 stress → 100 (capped)
                ↓
        BREAKING POINT
                ↓
    Acquire Trauma (e.g., "Paranoia")
                ↓
    Apply Effects (-1 WILL, +stress in darkness)
                ↓
        Stress Reset to 60

```

---

## Data Flow

### Stress Accumulation Flow

```
Event Triggers Stress
        │
        ▼
┌───────────────────────┐
│ Check Resolve Check?  │
│ (allowResolveCheck)   │
└───────────┬───────────┘
            │
    Yes     │     No
    ┌───────┴───────┐
    │               │
    ▼               │
Reduce by           │
successes           │
    │               │
    └───────┬───────┘
            │
            ▼
┌───────────────────────┐
│ Apply Trauma          │
│ Multipliers           │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ Add to Current Stress │
│ (Clamp 0-100)         │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ Check for Breaking    │
│ Point (stress >= 100) │
└───────────┬───────────┘
            │
     Yes    │    No
    ┌───────┴───────┐
    │               │
    ▼               ▼
Trigger          Done
Breaking
Point

```

---

## Integration Points

### Called By

| Caller | Context |
| --- | --- |
| `CombatEngine` | Combat stress, heretical ability costs |
| `EnvironmentalStressService` | Room-based stress |
| `DialogueService` | Story-triggered stress |
| `HazardService` | Hazard exposure stress |

### Works With

| Service | Interaction |
| --- | --- |
| `TraumaLibrary` | Trauma definitions and selection |
| `TraumaManagementService` | Trauma treatment/management |
| `PlayerCharacter` | Stores stress, corruption, traumas |

---

## Constants

| Constant | Value | Purpose |
| --- | --- | --- |
| `MaxTraumaValue` | 100 | Maximum stress/corruption |
| `MinTraumaValue` | 0 | Minimum stress/corruption |
| `BreakingPointStress` | 60 | Stress resets to this after Breaking Point |

---

## Version History

| Version | Changes |
| --- | --- |
| v0.1 | Initial stress/corruption tracking |
| v0.15 | Breaking Point mechanics, Trauma acquisition, threshold effects |

---

## Cross-References

### Related Documentation

- [Stress & Corruption](https://www.notion.so/01-systems/stress-corruption.md) - System overview
- [Traumas](https://www.notion.so/01-systems/traumas.md) - Trauma mechanics

### Related Services

- [TraumaLibrary](https://www.notion.so/trauma-library.md) - Trauma definitions
- [CombatEngine](https://www.notion.so/combat-engine.md) - Combat stress integration

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27