# DiceService

Parent item: Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md)

**File Path:** `RuneAndRust.Engine/DiceService.cs`**Version:** v0.21.3
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `DiceService` is the foundational randomization service for Rune & Rust. It provides dice rolling mechanics for all game systems including combat, skill checks, damage calculation, and procedural generation. The service supports both seeded (deterministic) and unseeded (random) operation.

---

## Architecture

### Core Mechanics

Rune & Rust uses a **dice pool system** where:

- Roll multiple d6 dice equal to an attribute value
- Count successes (rolls of 5 or 6)
- Success probability per die: 33.33% (2/6)

```
Roll(MIGHT=5) → [6, 4, 5, 2, 6] → 3 successes

```

### Seeding

The service supports deterministic generation through seeding:

```csharp
// Random (non-deterministic)
var diceService = new DiceService();

// Seeded (deterministic - same seed = same results)
var diceService = new DiceService(seed: 12345);

```

---

## Public API

### Success-Counting Rolls (Dice Pool)

### `Roll(int diceCount)`

Rolls d6 dice and counts successes (5-6 are successes).

```csharp
public DiceResult Roll(int diceCount)

```

**Parameters:**

- `diceCount` - Number of d6 to roll (typically an attribute value)

**Returns:** `DiceResult` containing:

- `DiceCount` - Number of dice rolled
- `Rolls` - List of individual roll values
- `Successes` - Count of rolls showing 5 or 6

**Example:**

```csharp
var result = diceService.Roll(5);  // Roll 5d6
// result.Successes = 2
// result.Rolls = [6, 3, 2, 5, 1]

```

**Probability Table:**

| Dice Count | Expected Successes | P(0 successes) | P(3+ successes) |
| --- | --- | --- | --- |
| 1 | 0.33 | 67% | 0% |
| 2 | 0.67 | 44% | 0% |
| 3 | 1.00 | 30% | 7% |
| 4 | 1.33 | 20% | 15% |
| 5 | 1.67 | 13% | 29% |
| 6 | 2.00 | 9% | 42% |
| 8 | 2.67 | 4% | 65% |
| 10 | 3.33 | 2% | 79% |

---

### `SkillCheck(int attributeValue, int targetNumber)`

Performs a skill check against a target number.

```csharp
public bool SkillCheck(int attributeValue, int targetNumber)

```

**Parameters:**

- `attributeValue` - Dice pool size (attribute being tested)
- `targetNumber` - Number of successes required

**Returns:** `true` if successes >= targetNumber

**Example:**

```csharp
// WILL check, need 2 successes
bool passed = diceService.SkillCheck(player.Attributes.Will, targetNumber: 2);

```

---

### Damage Rolls (Sum-Based)

### `RollDamage(int diceCount)`

Rolls multiple d6 and returns the total (for damage).

```csharp
public int RollDamage(int diceCount)

```

**Parameters:**

- `diceCount` - Number of d6 to roll

**Returns:** Sum of all rolls

**Example:**

```csharp
int damage = diceService.RollDamage(3);  // 3d6 damage
// Returns 3-18

```

---

### `RollDamage(int diceCount, int dieSize)`

Rolls damage with variable die size.

```csharp
public int RollDamage(int diceCount, int dieSize)

```

**Parameters:**

- `diceCount` - Number of dice
- `dieSize` - Die type (4, 6, 8, 10, etc.)

**Returns:** Sum of all rolls

**Example:**

```csharp
int damage = diceService.RollDamage(2, 8);  // 2d8 damage
// Returns 2-16

```

---

### Variable Dice Rolls

### `Roll(int numDice, int dieSize)`

Rolls dice of any size and returns the sum.

```csharp
public int Roll(int numDice, int dieSize)

```

**Parameters:**

- `numDice` - Number of dice to roll
- `dieSize` - Size of each die

**Returns:** Total of all rolls

**Example:**

```csharp
int total = diceService.Roll(3, 4);  // 3d4
// Returns 3-12

```

---

### `RollDice(int numDice, int dieSize)`

Alias for `Roll(int, int)`.

```csharp
public int RollDice(int numDice, int dieSize)

```

---

### Single Die Rolls

| Method | Range | Usage |
| --- | --- | --- |
| `RollD6()` | 1-6 | Standard damage, simple checks |
| `RollD8()` | 1-8 | Enhanced weapons |
| `RollD10()` | 1-10 | Powerful attacks, parry rolls |
| `RollD100()` | 1-100 | Percentile checks, rare events |

---

### Utility Methods

### `RollPercentage()`

Returns a random percentage value.

```csharp
public int RollPercentage()

```

**Returns:** 0-100 (inclusive)

**Use Cases:**

- Loot drop chances
- Critical hit determination
- Random event triggers

---

### `RollBetween(int min, int max)`

Rolls a random value in a range (inclusive).

```csharp
public int RollBetween(int min, int max)

```

**Parameters:**

- `min` - Minimum value
- `max` - Maximum value (swapped if min > max)

**Returns:** Random value between min and max (inclusive)

**Example:**

```csharp
int enemyCount = diceService.RollBetween(2, 5);  // 2-5 enemies

```

---

## DiceResult Model

```csharp
public class DiceResult
{
    public int DiceCount { get; }      // Number of dice rolled
    public List<int> Rolls { get; }    // Individual roll values
    public int Successes { get; }      // Count of 5s and 6s
}

```

---

## Integration Points

### Used By (Major Services)

| Service | Usage |
| --- | --- |
| `CombatEngine` | Attack rolls, defense rolls, initiative |
| `EnemyAI` | Random action selection |
| `DungeonGenerator` | Procedural generation decisions |
| `PopulationPipeline` | Spawn probability rolls |
| `TraumaEconomyService` | Resolve checks |
| `TrapService` | Trap trigger/damage rolls |
| `LootService` | Loot table rolls |

### Logging

All rolls are logged at Debug level for debugging and analysis:

```
Dice rolled: DiceCount=5, Rolls=[6, 4, 5, 2, 6], Successes=3
Damage dice rolled: DiceCount=3, Rolls=[4, 6, 2], Total=12

```

---

## Statistical Reference

### Success Probability Formula

For n dice, probability of exactly k successes:

```
P(k) = C(n,k) × (1/3)^k × (2/3)^(n-k)

```

### Expected Value

Expected successes for n dice:

```
E[successes] = n × (1/3) ≈ 0.333n

```

### Damage Roll Averages

| Dice | Average | Range |
| --- | --- | --- |
| 1d6 | 3.5 | 1-6 |
| 2d6 | 7.0 | 2-12 |
| 3d6 | 10.5 | 3-18 |
| 1d8 | 4.5 | 1-8 |
| 2d8 | 9.0 | 2-16 |
| 1d10 | 5.5 | 1-10 |

---

## Version History

| Version | Changes |
| --- | --- |
| v0.1 | Initial implementation with d6 pool system |
| v0.21.3 | Added variable die size support |

---

## Cross-References

### Related Documentation

- [Combat Resolution](https://www.notion.so/01-systems/combat-resolution.md) - Dice usage in combat
- [Damage Calculation](https://www.notion.so/01-systems/damage-calculation.md) - Damage roll mechanics

### Related Services

- [CombatEngine](https://www.notion.so/combat-engine.md) - Primary consumer of dice rolls
- [EnemyAI](https://www.notion.so/enemy-ai.md) - Uses dice for random decisions

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27