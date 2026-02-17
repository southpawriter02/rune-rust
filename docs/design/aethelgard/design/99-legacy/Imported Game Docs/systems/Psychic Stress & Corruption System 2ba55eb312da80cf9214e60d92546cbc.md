# Psychic Stress & Corruption System

Parent item: System Documentation (System%20Documentation%202ba55eb312da801a9aa4f30d6e439959.md)

**Version**: v0.17 (Based on v0.15 implementation)
**Status**: Core System (Tier 2)
**Dependencies**: Attributes (WILL), Resolve Check Service, Combat Engine
**Integration Points**: Combat, Exploration, Heretical Abilities, Rest System

---

## Table of Contents

1. [Functional Overview](Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md)
2. [Statistical Reference](Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md)
3. [Technical Implementation](Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md)
4. [Testing Coverage](Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md)
5. [Balance Considerations](Psychic%20Stress%20&%20Corruption%20System%202ba55eb312da80cf9214e60d92546cbc.md)

---

## 1. Functional Overview

### 1.1 Purpose

The **Trauma Economy** represents the psychological cost of survival in Aethelgard. Two resources govern mental health:

1. **Psychic Stress (0-100)**: Recoverable mental strain
2. **Corruption (0-100)**: Permanent transformation by the Blight

These systems create strategic tension: powerful actions (heretical abilities, Blight encounters) offer power at the cost of mental stability.

---

### 1.2 Psychic Stress

### Core Mechanics

- **Range**: 0-100
- **Recoverable**: Yes (full recovery at Sanctuary)
- **Resistance**: WILL-based Resolve Checks reduce Stress (1:1 ratio)
- **Breaking Point**: Reaching 100 Stress triggers permanent Trauma acquisition
- **Thresholds**:
    - 0-25: Safe (green)
    - 26-50: Strained (yellow)
    - 51-75: Severe (orange)
    - 76-99: Critical (red)
    - 100: Breaking Point (trauma acquired, reset to 60)

### Stress Sources

1. **Combat Encounters**: Boss fights, Forlorn enemies (5-15 Stress)
2. **Environmental Hazards**: Psychic Resonance zones, Blight areas (5-20 Stress)
3. **Heretical Abilities**: Psychic Lash (10 Stress), Mass Psychic Lash (20 Stress)
4. **Trauma Effects**: Passive Stress per turn from existing Traumas
5. **Narrative Events**: Witnessing horrors, moral choices

### Stress Resistance

**WILL-based Resolve Checks** reduce Stress gain:

```
Reduced_Stress = Base_Stress - WILL_Successes

```

**Example**:

- Psychic Resonance zone: 8 base Stress
- Player WILL: 4 (roll 4d6)
- Roll result: [5, 6, 3, 2] = 2 successes
- Actual Stress: 8 - 2 = **6 Stress**

---

### 1.3 Corruption

### Core Mechanics

- **Range**: 0-100
- **Recoverable**: No (permanent)
- **Resistance**: Cannot be resisted
- **Thresholds**: 25, 50, 75, 100 (each triggers narrative/mechanical effects)
- **Game Over**: 100 Corruption = Terminal Corruption (character lost to Blight)

### Corruption Sources

1. **Heretical Abilities**:
    - Void Strike (3 Corruption)
    - Blight Surge (2 Corruption)
    - Blood Sacrifice (3 Corruption)
    - Desperate Gambit (5 Corruption)
    - Corruption Nova (10 Corruption)
2. **Blight Encounters**: Direct Blight exposure
3. **Narrative Choices**: Dark pacts, forbidden knowledge
4. **Corruption Threshold Effects**: Machine Affinity trauma grants passive Corruption

### Corruption Thresholds

| Threshold | Effect | Description |
| --- | --- | --- |
| **25** | +1 Tech, -1 Social | Machine logic feels intuitive |
| **50** | +2 Tech, -2 Social, No human faction rep | Prefer machines to humans |
| **75** | Acquire [MACHINE AFFINITY] trauma, NPCs fear you | Humanity questioned |
| **100** | **Terminal Corruption (Game Over)** | No longer human |

---

### 1.4 Breaking Points

**Breaking Point**: When Stress reaches 100

**Consequences**:

1. Acquire **permanent Trauma** (see [traumas.md](http://traumas.md/))
2. Stress resets to **60** (not 0 - still rattled)
3. Trauma applies immediate mechanical effects

**Trauma Selection**: Based on source of Breaking Point

- Combat stress → [HYPERVIGILANCE], [PARANOIA]
- Isolation → [ISOLOPHOBIA]
- Dark spaces → [NYCTOPHOBIA]
- etc.

---

## 2. Statistical Reference

### 2.1 Stress Thresholds

| Stress Range | Threshold | Color | Description |
| --- | --- | --- | --- |
| 0-25 | Safe | Green | Mental stability intact |
| 26-50 | Strained | Yellow | Cracks forming |
| 51-75 | Severe | Orange | Mind fracturing |
| 76-99 | Critical | Red | Near Breaking Point |
| 100 | Breaking Point | Red | Trauma acquired |

### 2.2 Stress Gain by Source

| Source | Base Stress | WILL Resistance? | Frequency |
| --- | --- | --- | --- |
| Standard Combat | 0 | N/A | Always |
| Boss Combat | 10-15 | Yes | Rare |
| Forlorn Aura | 5-10 | Yes (DC check) | Uncommon |
| Psychic Resonance | 8 | Yes | Uncommon |
| Psychic Lash (ability) | 10 | No (self-inflicted) | Player choice |
| Mass Psychic Lash | 20 | No | Player choice |
| Desperate Gambit | 15 | No | Player choice |
| Trauma Passive Stress | 2-3/turn | No | Continuous if trauma present |

### 2.3 WILL-Based Stress Reduction

| WILL Value | Expected Successes | Avg Stress Reduction | 8 Stress → Result |
| --- | --- | --- | --- |
| 2 | 0.67 | ~1 Stress | 7 Stress |
| 3 | 1.0 | ~1 Stress | 7 Stress |
| 4 | 1.33 | ~1-2 Stress | 6-7 Stress |
| 5 | 1.67 | ~2 Stress | 6 Stress |
| 6 | 2.0 | ~2 Stress | 6 Stress |

**Key Insight**: High WILL (4-6) reduces Stress by ~20-30%, making it essential for Blight-heavy runs.

### 2.4 Corruption Gain by Source

| Source | Corruption | Recoverable? | Player Choice? |
| --- | --- | --- | --- |
| Void Strike | 3 | No | Yes (heretical ability) |
| Blight Surge | 2 | No | Yes (heretical ability) |
| Blood Sacrifice | 3 | No | Yes (heretical ability) |
| Desperate Gambit | 5 | No | Yes (heretical ability) |
| Corruption Nova | 10 | No | Yes (heretical ability) |
| Machine Affinity Trauma | +0.5/day | No | No (from Corruption 75 threshold) |
| Narrative Corruption | Variable | No | Sometimes |

### 2.5 Stress vs Corruption: Recovery Comparison

| Resource | Max | Recoverable? | Recovery Method | Recovery Amount |
| --- | --- | --- | --- | --- |
| Psychic Stress | 100 | Yes | Sanctuary Rest | Full (to 0) |
| Psychic Stress | 100 | No | Combat/Abilities | N/A |
| Corruption | 100 | **No** | **None** | 0 |

**Critical Design**: Stress is a **short-term threat** (recoverable), Corruption is a **long-term cost** (permanent).

---

## 3. Technical Implementation

### 3.1 Core Service

**File**: `RuneAndRust.Engine/TraumaEconomyService.cs` (572 lines)

### 3.2 Adding Psychic Stress

**Method**: `AddStress`

```csharp
public (int stressGained, Trauma? traumaAcquired) AddStress(
    PlayerCharacter character,
    int baseAmount,
    string source = "unknown",
    bool allowResolveCheck = false,
    int resolveSuccesses = 0)
{
    if (baseAmount <= 0) return (0, null);

    int actualAmount = baseAmount;

    // Apply Resolve Check reduction if allowed
    if (allowResolveCheck && resolveSuccesses > 0)
    {
        actualAmount = Math.Max(0, baseAmount - resolveSuccesses);
    }

    // Apply trauma stress multipliers
    float stressMultiplier = character.GetTraumaStressMultiplier(source);
    actualAmount = (int)(actualAmount * stressMultiplier);

    // Apply stress gain
    int oldStress = character.PsychicStress;
    character.PsychicStress = Math.Clamp(character.PsychicStress + actualAmount, 0, 100);
    int actualGained = character.PsychicStress - oldStress;

    // Check for Breaking Point
    Trauma? acquiredTrauma = null;
    if (character.PsychicStress >= 100 && oldStress < 100)
    {
        acquiredTrauma = TriggerBreakingPoint(character, source);
    }

    return (actualGained, acquiredTrauma);
}

```

**Key Features**:

1. WILL-based reduction via `resolveSuccesses`
2. Trauma multipliers (e.g., [PARANOIA] = 1.2× Stress)
3. Clamped to 0-100 range
4. Triggers Breaking Point at 100

### 3.3 Adding Corruption

**Method**: `AddCorruption`

```csharp
public (int corruptionGained, List<int> thresholdsCrossed) AddCorruption(
    PlayerCharacter character,
    int amount,
    string source = "unknown")
{
    if (amount <= 0) return (0, new List<int>());

    int oldCorruption = character.Corruption;
    character.Corruption = Math.Clamp(character.Corruption + amount, 0, 100);
    int actualGained = character.Corruption - oldCorruption;

    var thresholdsCrossed = new List<int>();

    // Check threshold crossings
    if (oldCorruption < 25 && character.Corruption >= 25)
    {
        TriggerCorruptionThreshold25(character);
        thresholdsCrossed.Add(25);
    }

    if (oldCorruption < 50 && character.Corruption >= 50)
    {
        TriggerCorruptionThreshold50(character);
        thresholdsCrossed.Add(50);
    }

    if (oldCorruption < 75 && character.Corruption >= 75)
    {
        TriggerCorruptionThreshold75(character);
        thresholdsCrossed.Add(75);
    }

    if (oldCorruption < 100 && character.Corruption >= 100)
    {
        TriggerTerminalCorruption(character);
        thresholdsCrossed.Add(100);
    }

    return (actualGained, thresholdsCrossed);
}

```

**Key Features**:

1. No resistance (always applies full amount)
2. Tracks threshold crossings
3. Triggers threshold effects at 25/50/75/100

### 3.4 Clearing Stress (Sanctuary Rest)

**Method**: `ClearStress`

```csharp
public void ClearStress(PlayerCharacter character)
{
    int oldStress = character.PsychicStress;
    character.PsychicStress = 0;

    _log.Information("Stress cleared: Character={CharacterName}, OldStress={Old}, Recovered={Recovered}",
        character.Name, oldStress, oldStress);
}

```

**Design**: Simple reset to 0. Corruption is **not** clearable.

---

## 4. Testing Coverage

### 4.1 Unit Tests Required

**Test Case**: `AddStress_WithWILLResistance_ReducesStress`

```csharp
[Test]
public void AddStress_WithWILLResistance_ReducesStress()
{
    // Arrange
    var player = CreateTestPlayer(will: 4);
    var traumaService = new TraumaEconomyService();

    // Act
    var (stressGained, trauma) = traumaService.AddStress(
        player,
        baseAmount: 10,
        allowResolveCheck: true,
        resolveSuccesses: 2);

    // Assert
    Assert.AreEqual(8, stressGained); // 10 - 2
}

```

**Test Case**: `AddStress_TriggersBreakingPoint_At100`

```csharp
[Test]
public void AddStress_TriggersBreakingPoint_At100()
{
    // Arrange
    var player = CreateTestPlayer(stress: 95);
    var traumaService = new TraumaEconomyService();

    // Act
    var (stressGained, trauma) = traumaService.AddStress(player, 10);

    // Assert
    Assert.IsNotNull(trauma); // Trauma acquired
    Assert.AreEqual(60, player.PsychicStress); // Reset to 60
}

```

---

## 5. Balance Considerations

### 5.1 Stress Recovery vs Corruption Permanence

**Design Goal**: Stress = short-term threat, Corruption = long-term cost

**Current Balance**:

- Stress recovers fully at Sanctuary (accessible every 3-5 rooms)
- Corruption is permanent (no recovery)
- Players can manage Stress via rest; Corruption accumulates irreversibly

**Trade-off**: Heretical abilities offer power but cost Corruption (permanent price for temporary power).

---

**End of Document***See also*: [traumas.md](http://traumas.md/) (Breaking Points & Permanent Traumas)