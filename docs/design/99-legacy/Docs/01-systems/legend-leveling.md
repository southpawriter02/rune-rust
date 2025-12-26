# Legend & Leveling System

**Version**: v0.17 (Based on v0.7 implementation)
**Status**: Core System (Tier 2)
**Dependencies**: Attributes, Abilities, Specializations, Trauma Economy
**Integration Points**: Combat Engine, Saga Service, Trauma Economy Service

---

## Table of Contents

1. [Functional Overview](#1-functional-overview)
2. [Statistical Reference](#2-statistical-reference)
3. [Technical Implementation](#3-technical-implementation)
4. [Testing Coverage](#4-testing-coverage)
5. [Balance Considerations](#5-balance-considerations)

---

## 1. Functional Overview

### 1.1 Purpose

The **Aethelgard Saga System** manages character progression through three interconnected mechanics:
- **Legend**: Experience points earned through encounters and actions
- **Milestones**: Major progression thresholds that grant powerful rewards
- **Progression Points (PP)**: Currency spent to increase attributes and unlock abilities

This system replaces traditional leveling with a narrative-driven progression model where characters build their "legend" through deeds and grow stronger at specific milestones.

### 1.2 Core Concepts

#### Legend (XP)
- Earned after combat encounters, puzzle completion, and significant actions
- Calculated using the formula: **BLV × DM × TM**
  - **BLV** (Base Legend Value): Inherent difficulty/value of the encounter
  - **DM** (Difficulty Modifier): Multiplier based on encounter difficulty (1.0 for normal)
  - **TM** (Trauma Modifier): Multiplier based on encounter type (1.0-1.25)
- Accumulates toward the next Milestone threshold
- Does **not** reset when reaching a Milestone (carries over)

#### Milestones
- Major progression thresholds reached when accumulated Legend meets the requirement
- Grant substantial permanent bonuses:
  - **+10 Maximum HP**
  - **+5 Maximum Stamina**
  - **+1 Progression Point (PP)**
  - **Full restoration** of HP and Stamina
- Threshold increases with each Milestone (see Statistical Reference)
- Currently capped at **Milestone 3** for v0.1 scope

#### Progression Points (PP)
- Primary currency for character customization
- Starting value: **2 PP** at character creation
- Earned at each Milestone (+1 PP per Milestone)
- Spent on:
  - **Attribute increases**: 1 PP per +1 attribute (cap at 6 per attribute)
  - **Ability rank advancement**: Variable PP cost (typically 2-3 PP to reach Rank 2)
  - **Specialization unlock**: 3 PP to unlock a specialization (one-time) **(v0.18: reduced from 10 PP)**

### 1.3 Gameplay Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Complete Encounter (Combat, Puzzle, Boss)               │
│    ↓                                                         │
│ 2. Award Legend = BLV × DM × TM                             │
│    ↓                                                         │
│ 3. Check if CurrentLegend ≥ LegendToNextMilestone          │
│    ↓                                                         │
│ 4. IF YES: Reach Milestone                                  │
│    - Increase CurrentMilestone by 1                         │
│    - Grant +10 Max HP, +5 Max Stamina, +1 PP               │
│    - Full heal (HP = MaxHP, Stamina = MaxStamina)          │
│    - Calculate new LegendToNextMilestone                    │
│    ↓                                                         │
│ 5. Player Spends PP (between encounters)                    │
│    - Option A: Spend 1 PP → +1 Attribute (cap 6)           │
│    - Option B: Spend 2-3 PP → Advance Ability Rank         │
│    - Option C: Spend 3 PP → Unlock Specialization (v0.18)  │
└─────────────────────────────────────────────────────────────┘
```

### 1.4 Key Design Principles

1. **Milestone-Based**: Major rewards come at specific thresholds, not continuously
2. **Choice-Driven**: PP spending allows diverse build strategies
3. **Trauma Integration**: Psychological stress affects Legend gain (Trauma Modifier)
4. **Diminishing Curves**: Later Milestones require more Legend, attributes cost more to raise
5. **Full Heal on Milestone**: Reaching a Milestone is a moment of triumph and recovery

---

## 2. Statistical Reference

### 2.1 Legend Award Formula

```
Legend_Awarded = FLOOR(Base_Legend_Value × Difficulty_Modifier × Trauma_Modifier)
```

#### Base Legend Value (BLV)
Inherent value of the encounter, typically:
- **Standard enemy**: 20-40 BLV
- **Elite enemy**: 50-80 BLV
- **Boss enemy**: 100-150 BLV
- **Puzzle completion**: 30-60 BLV
- **Room exploration**: 10-20 BLV

*(Exact values per enemy/encounter type documented in Enemy Bestiary)*

#### Difficulty Modifier (DM)
Currently defaults to **1.0** for all encounters in v0.1. Future versions may include:
- Easy mode: 0.75
- Normal mode: 1.0
- Hard mode: 1.25
- Extreme mode: 1.5

#### Trauma Modifier (TM)
Multiplier based on encounter context:

| Encounter Type | Trauma Modifier | Reasoning |
|----------------|-----------------|-----------|
| Normal         | 1.0             | Standard coherent encounter |
| Puzzle         | 1.25            | Taxing mental strain from corrupted systems |
| Boss           | 1.25            | Blight-corrupted enemy (psychologically harrowing) |
| Blight         | 1.25            | Blight-corrupted area (corrupting influence) |

**Implementation**: `SagaService.CalculateTraumaMod(string encounterType)`

### 2.2 Milestone Thresholds

```
Legend_To_Next_Milestone = (Current_Milestone × 50) + 100
```

**v0.1 Milestone Table** (Max Milestone = 3):

| Milestone | Legend Required | Cumulative Legend | HP Gain | Stamina Gain | PP Gain |
|-----------|-----------------|-------------------|---------|--------------|---------|
| 0 → 1     | 100             | 100               | +10     | +5           | +1      |
| 1 → 2     | 150             | 250               | +10     | +5           | +1      |
| 2 → 3     | 200             | 450               | +10     | +5           | +1      |
| **Total** | **450**         | -                 | **+30** | **+15**      | **+3**  |

**Note**: Legend does **not** reset at Milestones. If you earn 120 Legend and reach Milestone 1 (requires 100), you carry over 120 Legend toward Milestone 2 (requires 150 total = 250 cumulative).

**Design Note**: v0.1's formula is adjusted for short playtime (15-20 minutes). Future versions will use steeper curves for longer campaigns.

### 2.3 Milestone Rewards

Each Milestone grants **four** immediate benefits:

1. **+10 Maximum HP**
   - Increases survivability
   - Scales linearly (not exponential)

2. **+5 Maximum Stamina**
   - More ability usage
   - Scales linearly

3. **+1 Progression Point (PP)**
   - Currency for customization
   - Total PP by Milestone 3: 2 (starting) + 3 (earned) = **5 PP**

4. **Full Restoration**
   - HP set to new MaxHP
   - Stamina set to new MaxStamina
   - Immediate combat readiness

### 2.4 Progression Points (PP) Spending

#### Attribute Increases

```
Cost = 1 PP per +1 attribute
Cap = 6 per attribute (hard cap)
```

**Cost Table**:

| Current Value | Cost to Next Level | New Value | Notes |
|---------------|--------------------|-----------|-------|
| 1             | 1 PP               | 2         | -     |
| 2             | 1 PP               | 3         | -     |
| 3             | 1 PP               | 4         | -     |
| 4             | 1 PP               | 5         | -     |
| 5             | 1 PP               | 6         | **Max reached** |
| 6             | **Not allowed**    | -         | Hard cap |

**Example**: To raise MIGHT from 3 to 6 costs **3 PP** (3→4: 1 PP, 4→5: 1 PP, 5→6: 1 PP)

**Attribute Cap Rationale**:
- Prevents single-attribute "god builds"
- Encourages balanced or specialized builds
- Cap of 6 = ~2 successes per roll (33.33% × 6 = 2.0 expected)

#### Ability Rank Advancement

```
Cost = ability.CostToRank2 (typically 2-3 PP)
Current Cap = Rank 2 (Rank 3 locked until v0.5+)
```

**Rank 2 Improvements** (varies by ability):
- **+1-2 Bonus Dice** (most common)
- **-1-2 Stamina Cost** (resource efficiency)
- **-1 Success Threshold** (easier to trigger effects)
- **+1 Damage Dice** (increased damage output)
- **+1 Duration** (longer status effects)

**Example Costs**:
- Power Strike: 2 PP (Rank 1 → Rank 2)
- Shield Wall: 2 PP (Rank 1 → Rank 2)
- Aetheric Bolt: 3 PP (Rank 1 → Rank 2)

*(See Ability Registry for complete Rank 2 improvements)*

#### Specialization Unlock

```
Cost = 3 PP (one-time) **[v0.18: reduced from 10 PP]**
Requirement = Specialization.None (can only unlock one)
Effect = Grants Tier 1 abilities for chosen specialization
```

**Specializations by Archetype**:
- **Warrior**: Bone-Setter, Jötun-Reader, Skald
- **Scavenger**: Bone-Setter, Jötun-Reader, Skald
- **Mystic**: Bone-Setter, Jötun-Reader, Skald

**Specialization Features**:
- Unlocks 2-3 Tier 1 abilities
- Thematic bonuses (healing, analysis, support)
- Cannot be changed once selected
- 3 PP cost = achievable by Milestone 2 (v0.18 balance)

### 2.5 PP Economy Analysis

**Total PP Available by Milestone 3**:
- Starting PP: **2**
- Milestone 1: **+1** (Total: 3 PP)
- Milestone 2: **+1** (Total: 4 PP)
- Milestone 3: **+1** (Total: 5 PP)

**Build Examples** (5 PP budget):
1. **Attribute Specialist**: +5 to one attribute (e.g., MIGHT 3 → 6 = 3 PP, 2 PP left for abilities)
2. **Ability Focused**: +2 attributes (2 PP), +1 ability rank (2-3 PP)
3. **Hybrid**: +3 attributes (3 PP), +1 ability rank (2 PP)
4. **Specialization Build**: Unlock specialization (3 PP), +2 attributes (2 PP) - **[v0.18: Now achievable!]**

**Design Note (v0.18)**: 3 PP specialization cost makes specializations accessible by Milestone 2, enabling specialized builds in v0.1 scope while still requiring meaningful choices.

---

## 3. Technical Implementation

### 3.1 Core Service

**File**: `RuneAndRust.Engine/SagaService.cs` (436 lines)

The `SagaService` class encapsulates all Legend, Milestone, and PP spending logic.

#### Constants

```csharp
private const int MaxMilestone = 3; // For v0.1
private const int AttributeCap = 6;
```

### 3.2 Legend Award System

#### Method: `AwardLegend`

```csharp
public void AwardLegend(
    PlayerCharacter player,
    int baseLegendValue,
    float difficultyMod = 1.0f,
    float traumaMod = 1.0f)
{
    // Guard: Already at max milestone
    if (player.CurrentMilestone >= MaxMilestone)
    {
        _log.Debug("Legend award skipped: Character={CharacterName}, Reason=MaxMilestoneReached");
        return;
    }

    // Calculate legend awarded
    int oldLegend = player.CurrentLegend;
    int legendAwarded = (int)(baseLegendValue * difficultyMod * traumaMod);
    player.CurrentLegend += legendAwarded;

    // Logging
    _log.Information("Legend awarded: Character={CharacterName}, Amount={Amount}, " +
        "BaseValue={Base}, DifficultyMod={DM}, TraumaMod={TM}, " +
        "OldLegend={Old}, NewLegend={New}, ToNextMilestone={ToNext}",
        player.Name, legendAwarded, baseLegendValue, difficultyMod, traumaMod,
        oldLegend, player.CurrentLegend, player.LegendToNextMilestone);
}
```

**Key Implementation Details**:
1. **Guard clause**: Prevents Legend gain if already at MaxMilestone
2. **Integer truncation**: `(int)` cast floors the result
3. **Additive**: Legend accumulates, doesn't reset
4. **Logging**: Comprehensive structured logging for all progression events

**Parameters**:
- `baseLegendValue`: Inherent value of the encounter (e.g., 50 for elite enemy)
- `difficultyMod`: Difficulty multiplier (default 1.0)
- `traumaMod`: Trauma context multiplier (default 1.0)

**Example Call**:
```csharp
// Award 50 BLV for elite enemy in Blight area (TM = 1.25)
sagaService.AwardLegend(player, baseLegendValue: 50, difficultyMod: 1.0f, traumaMod: 1.25f);
// Result: 50 × 1.0 × 1.25 = 62.5 → 62 Legend awarded
```

#### Method: `CalculateTraumaMod`

```csharp
public float CalculateTraumaMod(string encounterType)
{
    return encounterType.ToLower() switch
    {
        "normal" => 1.0f,           // Coherent act
        "puzzle" => 1.25f,          // Taxing act (interfacing with corrupted systems)
        "boss" => 1.25f,            // Taxing act (Blight-corrupted enemy)
        "blight" => 1.25f,          // Taxing act (Blight-corrupted area)
        _ => 1.0f
    };
}
```

**Design**: Simple switch expression for encounter type multipliers. Future expansion could include more granular trauma contexts.

### 3.3 Milestone System

#### Method: `CanReachMilestone`

```csharp
public bool CanReachMilestone(PlayerCharacter player)
{
    if (player.CurrentMilestone >= MaxMilestone)
    {
        return false;
    }

    return player.CurrentLegend >= player.LegendToNextMilestone;
}
```

**Guard Conditions**:
1. Not already at MaxMilestone
2. Accumulated Legend meets or exceeds threshold

#### Method: `ReachMilestone`

```csharp
public void ReachMilestone(PlayerCharacter player)
{
    // Validation
    if (!CanReachMilestone(player))
    {
        throw new InvalidOperationException("Player cannot reach milestone yet.");
    }

    int oldMilestone = player.CurrentMilestone;
    int oldMaxHP = player.MaxHP;
    int oldMaxStamina = player.MaxStamina;
    int oldPP = player.ProgressionPoints;

    // Increase milestone
    player.CurrentMilestone++;

    // Update Legend threshold for next milestone
    player.LegendToNextMilestone = CalculateLegendToNextMilestone(player.CurrentMilestone);

    // Grant milestone rewards
    player.ProgressionPoints += 1;
    player.MaxHP += 10;
    player.MaxStamina += 5;

    // Full heal on milestone
    player.HP = player.MaxHP;
    player.Stamina = player.MaxStamina;

    // Logging
    _log.Information("Milestone reached: Character={CharacterName}, " +
        "OldMilestone={OldMilestone}, NewMilestone={NewMilestone}, " +
        "PPGained={PPGained}, TotalPP={TotalPP}, " +
        "MaxHPIncrease={HPIncrease}, NewMaxHP={MaxHP}, " +
        "MaxStaminaIncrease={StaminaIncrease}, NewMaxStamina={MaxStamina}, " +
        "LegendToNext={LegendToNext}",
        player.Name, oldMilestone, player.CurrentMilestone,
        1, player.ProgressionPoints,
        10, player.MaxHP,
        5, player.MaxStamina,
        player.LegendToNextMilestone);
}
```

**Reward Sequence**:
1. Increment `CurrentMilestone`
2. Recalculate `LegendToNextMilestone` for new milestone
3. Grant +1 PP, +10 MaxHP, +5 MaxStamina
4. **Full heal**: Set HP and Stamina to new maximums
5. Log all changes for audit trail

**Important**: Legend is **not** reset or consumed. If you have 120 Legend at Milestone 1 (requires 100), you still have 120 Legend toward Milestone 2.

#### Method: `CalculateLegendToNextMilestone`

```csharp
public int CalculateLegendToNextMilestone(int currentMilestone)
{
    if (currentMilestone >= MaxMilestone)
    {
        return 0; // At max milestone
    }

    // Adjusted formula for v0.1's short playtime
    return (currentMilestone * 50) + 100;
}
```

**Formula**: `(Milestone × 50) + 100`

**Results**:
- Milestone 0 → 1: (0 × 50) + 100 = **100**
- Milestone 1 → 2: (1 × 50) + 100 = **150**
- Milestone 2 → 3: (2 × 50) + 100 = **200**
- Milestone 3 (max): **0** (returns 0, no further progression)

**Design Note**: Linear progression with gentle slope for v0.1's 15-20 minute target playtime. Future versions will use exponential curves (e.g., `50 * 2^Milestone`).

### 3.4 Progression Points (PP) Spending

#### Method: `SpendPPOnAttribute`

```csharp
public bool SpendPPOnAttribute(PlayerCharacter player, string attributeName)
{
    // Check if player has enough PP
    if (player.ProgressionPoints < 1)
    {
        _log.Warning("PP spend failed: Reason=InsufficientPP, CurrentPP={PP}",
            player.ProgressionPoints);
        return false;
    }

    // Get current attribute value
    int currentValue = player.GetAttributeValue(attributeName);
    if (currentValue >= AttributeCap)
    {
        _log.Warning("PP spend failed: Reason=AttributeAtCap, CurrentValue={Value}, Cap={Cap}",
            currentValue, AttributeCap);
        return false;
    }

    // Spend PP and increase attribute
    int oldPP = player.ProgressionPoints;
    player.ProgressionPoints -= 1;

    switch (attributeName.ToLower())
    {
        case "might":
            player.Attributes.Might++;
            break;
        case "finesse":
            player.Attributes.Finesse++;
            break;
        case "wits":
            player.Attributes.Wits++;
            break;
        case "will":
            player.Attributes.Will++;
            break;
        case "sturdiness":
            player.Attributes.Sturdiness++;
            break;
        default:
            // Refund PP if invalid attribute
            player.ProgressionPoints += 1;
            _log.Error("Invalid attribute name: Attribute={Attribute}", attributeName);
            throw new ArgumentException($"Invalid attribute name: {attributeName}");
    }

    int newValue = player.GetAttributeValue(attributeName);
    _log.Information("PP spent on attribute: Character={CharacterName}, " +
        "Attribute={Attribute}, PPSpent={Cost}, " +
        "OldValue={OldValue}, NewValue={NewValue}, RemainingPP={PP}",
        player.Name, attributeName, 1, currentValue, newValue, player.ProgressionPoints);

    return true;
}
```

**Validation Steps**:
1. Check PP availability (≥ 1 PP required)
2. Check attribute cap (must be < 6)
3. Validate attribute name (case-insensitive)
4. Deduct 1 PP
5. Increment attribute by 1
6. Log transaction

**Error Handling**:
- **Insufficient PP**: Returns `false`, logs warning
- **Attribute at cap**: Returns `false`, logs warning
- **Invalid attribute name**: Refunds PP, throws `ArgumentException`

**Case Insensitivity**: Accepts "might", "Might", "MIGHT", etc.

#### Method: `AdvanceAbilityRank`

```csharp
public bool AdvanceAbilityRank(PlayerCharacter player, Ability ability)
{
    // Check rank cap
    if (ability.CurrentRank >= 2)
    {
        _log.Warning("Ability rank advance failed: Reason=AlreadyAtMaxRank, CurrentRank={Rank}",
            ability.CurrentRank);
        return false; // Rank 3 locked until v0.5+
    }

    // Check PP cost
    if (player.ProgressionPoints < ability.CostToRank2)
    {
        _log.Warning("Ability rank advance failed: Reason=InsufficientPP, " +
            "RequiredPP={Required}, CurrentPP={Current}",
            ability.CostToRank2, player.ProgressionPoints);
        return false;
    }

    int oldRank = ability.CurrentRank;
    int ppCost = ability.CostToRank2;

    // Spend PP and advance rank
    player.ProgressionPoints -= ability.CostToRank2;
    ability.CurrentRank++;

    // Apply rank 2 improvements (will be customized per ability)
    ApplyRank2Improvements(ability);

    _log.Information("Ability rank advanced: Character={CharacterName}, " +
        "Ability={Ability}, PPSpent={Cost}, " +
        "OldRank={OldRank}, NewRank={NewRank}, RemainingPP={PP}",
        player.Name, ability.Name, ppCost, oldRank, ability.CurrentRank,
        player.ProgressionPoints);

    return true;
}
```

**Rank Advancement Flow**:
1. Validate current rank < 2 (Rank 3 not yet implemented)
2. Check PP ≥ `ability.CostToRank2`
3. Deduct PP cost
4. Increment `ability.CurrentRank`
5. Apply Rank 2 improvements via `ApplyRank2Improvements(ability)`

**Design Note**: Each ability has a unique `CostToRank2` value, typically 2-3 PP. Future versions will support Rank 3 (v0.5+).

#### Method: `ApplyRank2Improvements`

```csharp
private void ApplyRank2Improvements(Ability ability)
{
    switch (ability.Name)
    {
        // Warrior abilities
        case "Power Strike":
            ability.BonusDice = 3; // Was +2, now +3
            ability.SuccessThreshold = 2; // Was 2, now easier to trigger double damage
            ability.StaminaCost = 4; // Was 5, now cheaper
            break;

        case "Shield Wall":
            ability.BonusDice = 2; // Was +1, now +2
            ability.DefensePercent = 75; // Was 50%, now 75%
            ability.DefenseDuration = 3; // Was 2, now 3
            break;

        // Scavenger abilities
        case "Quick Dodge":
            ability.BonusDice = 2; // Was +1, now +2
            ability.StaminaCost = 2; // Was 3, now cheaper
            break;

        case "Precision Strike":
            ability.BonusDice = 2; // Improved dice
            ability.SuccessThreshold = 2; // Easier to trigger bleeding
            break;

        // Mystic abilities
        case "Aetheric Bolt":
            ability.BonusDice = 3; // Was +2, now +3
            ability.DamageDice = 2; // Was 1d6, now 2d6
            break;

        case "Aetheric Shield":
            ability.BonusDice = 2; // Was +1, now +2
            // Shield absorption is handled in combat logic
            break;

        // ... (55+ more abilities with unique Rank 2 improvements)
    }
}
```

**Rank 2 Improvement Patterns**:
- **+1 Bonus Dice** (most common): Easier to succeed
- **-1-2 Stamina Cost**: More efficient resource usage
- **+1 Damage Dice**: Higher damage output
- **+1 Duration**: Longer status effects
- **-1 Success Threshold**: Easier to trigger special effects

**Complete List**: See `SagaService.cs:192-355` for all 55+ ability improvements.

#### Method: `UnlockSpecialization`

```csharp
public bool UnlockSpecialization(PlayerCharacter player, Specialization specialization)
{
    const int SpecializationCost = 3; // v0.18: reduced from 10

    // Check if player has enough PP
    if (player.ProgressionPoints < SpecializationCost)
    {
        _log.Warning("Specialization unlock failed: Reason=InsufficientPP, " +
            "RequiredPP={Required}, CurrentPP={Current}",
            SpecializationCost, player.ProgressionPoints);
        return false;
    }

    // Check if player already has a specialization
    if (player.Specialization != Specialization.None)
    {
        _log.Warning("Specialization unlock failed: Reason=AlreadyHasSpecialization, " +
            "CurrentSpecialization={Current}",
            player.Specialization);
        return false;
    }

    // Check if specialization is valid for this archetype
    if (!SpecializationFactory.CanChooseSpecialization(player, specialization))
    {
        _log.Warning("Specialization unlock failed: Reason=InvalidForArchetype, " +
            "Class={Class}",
            player.Class);
        return false;
    }

    // Spend PP
    player.ProgressionPoints -= SpecializationCost;

    // Apply specialization (adds Tier 1 abilities)
    SpecializationFactory.ApplySpecialization(player, specialization);

    _log.Information("Specialization unlocked: Character={CharacterName}, " +
        "Specialization={Specialization}, PPSpent={Cost}, RemainingPP={PP}, Class={Class}",
        player.Name, specialization, SpecializationCost, player.ProgressionPoints,
        player.Class);

    return true;
}
```

**Validation Steps**:
1. Check PP ≥ 3 **(v0.18: reduced from 10)**
2. Check `player.Specialization == Specialization.None` (can only have one)
3. Check specialization is valid for player's archetype (via `SpecializationFactory`)
4. Deduct 3 PP
5. Apply specialization via `SpecializationFactory.ApplySpecialization()`

**Specialization Restrictions**:
- Can only unlock **one** specialization per character
- Must be valid for player's archetype (Warrior/Scavenger/Mystic)
- Cannot be changed once selected

**Design Note (v0.18)**: 3 PP cost makes specializations accessible in v0.1 scope (M0-M3) while maintaining meaningful choices between specialization and attribute investment.

### 3.5 Data Model

**File**: `RuneAndRust.Core/PlayerCharacter.cs` (lines 18-22)

```csharp
// Progression (Aethelgard Saga System)
public int CurrentMilestone { get; set; } = 0;
public int CurrentLegend { get; set; } = 0;
public int LegendToNextMilestone { get; set; } = 100;
public int ProgressionPoints { get; set; } = 2; // Start with 2 PP
```

**Initial Values**:
- `CurrentMilestone = 0`: Start at Milestone 0
- `CurrentLegend = 0`: No Legend accumulated yet
- `LegendToNextMilestone = 100`: First Milestone requires 100 Legend
- `ProgressionPoints = 2`: Start with 2 PP for initial customization

**Design**: Players start with 2 PP to allow immediate attribute increases or ability experimentation.

### 3.6 Integration Points

#### Combat Engine Integration

**File**: `RuneAndRust.Engine/CombatEngine.cs`

After combat victory:
```csharp
// Award Legend for defeating enemy
_sagaService.AwardLegend(
    player,
    baseLegendValue: enemy.LegendValue, // Enemy-specific BLV
    difficultyMod: 1.0f,                // Default for v0.1
    traumaMod: traumaMod                // Based on room/encounter type
);

// Check if player can reach milestone
if (_sagaService.CanReachMilestone(player))
{
    _sagaService.ReachMilestone(player);
    // Display milestone notification to player
}
```

#### Trauma Economy Integration

**Trauma Modifier Calculation**:
```csharp
float traumaMod = _sagaService.CalculateTraumaMod(encounterType);
// encounterType: "normal", "puzzle", "boss", "blight"
```

**Design**: Trauma Modifier (TM) represents the psychological weight of the encounter. Blight-corrupted enemies and areas grant +25% Legend due to increased stress.

---

## 4. Testing Coverage

### 4.1 Unit Tests Required

#### Legend Award Tests

**Test Case**: `AwardLegend_WithBasicValues_CalculatesCorrectly`
```csharp
[Test]
public void AwardLegend_WithBasicValues_CalculatesCorrectly()
{
    // Arrange
    var player = new PlayerCharacter { CurrentLegend = 0, CurrentMilestone = 0 };
    var sagaService = new SagaService();

    // Act
    sagaService.AwardLegend(player, baseLegendValue: 50, difficultyMod: 1.0f, traumaMod: 1.25f);

    // Assert
    Assert.AreEqual(62, player.CurrentLegend); // 50 × 1.0 × 1.25 = 62.5 → 62
}
```

**Test Case**: `AwardLegend_AtMaxMilestone_DoesNotAwardLegend`
```csharp
[Test]
public void AwardLegend_AtMaxMilestone_DoesNotAwardLegend()
{
    // Arrange
    var player = new PlayerCharacter { CurrentLegend = 500, CurrentMilestone = 3 };
    var sagaService = new SagaService();

    // Act
    sagaService.AwardLegend(player, baseLegendValue: 50);

    // Assert
    Assert.AreEqual(500, player.CurrentLegend); // No change
}
```

**Test Case**: `AwardLegend_WithTraumaMod_AppliesCorrectMultiplier`
```csharp
[TestCase("normal", 50, 50)]   // 50 × 1.0 = 50
[TestCase("puzzle", 50, 62)]   // 50 × 1.25 = 62.5 → 62
[TestCase("boss", 50, 62)]     // 50 × 1.25 = 62.5 → 62
[TestCase("blight", 50, 62)]   // 50 × 1.25 = 62.5 → 62
public void AwardLegend_WithTraumaMod_AppliesCorrectMultiplier(
    string encounterType, int baseLegend, int expectedLegend)
{
    // Arrange
    var player = new PlayerCharacter { CurrentLegend = 0 };
    var sagaService = new SagaService();
    float traumaMod = sagaService.CalculateTraumaMod(encounterType);

    // Act
    sagaService.AwardLegend(player, baseLegend, 1.0f, traumaMod);

    // Assert
    Assert.AreEqual(expectedLegend, player.CurrentLegend);
}
```

#### Milestone Tests

**Test Case**: `CalculateLegendToNextMilestone_ReturnsCorrectThresholds`
```csharp
[TestCase(0, 100)]  // (0 × 50) + 100 = 100
[TestCase(1, 150)]  // (1 × 50) + 100 = 150
[TestCase(2, 200)]  // (2 × 50) + 100 = 200
[TestCase(3, 0)]    // At max milestone
public void CalculateLegendToNextMilestone_ReturnsCorrectThresholds(
    int milestone, int expected)
{
    // Arrange
    var sagaService = new SagaService();

    // Act
    int result = sagaService.CalculateLegendToNextMilestone(milestone);

    // Assert
    Assert.AreEqual(expected, result);
}
```

**Test Case**: `ReachMilestone_GrantsCorrectRewards`
```csharp
[Test]
public void ReachMilestone_GrantsCorrectRewards()
{
    // Arrange
    var player = new PlayerCharacter
    {
        CurrentLegend = 100,
        LegendToNextMilestone = 100,
        CurrentMilestone = 0,
        ProgressionPoints = 2,
        MaxHP = 50,
        MaxStamina = 30,
        HP = 25, // Partially damaged
        Stamina = 15
    };
    var sagaService = new SagaService();

    // Act
    sagaService.ReachMilestone(player);

    // Assert
    Assert.AreEqual(1, player.CurrentMilestone);
    Assert.AreEqual(3, player.ProgressionPoints); // 2 + 1
    Assert.AreEqual(60, player.MaxHP);             // 50 + 10
    Assert.AreEqual(35, player.MaxStamina);        // 30 + 5
    Assert.AreEqual(60, player.HP);                // Full heal
    Assert.AreEqual(35, player.Stamina);           // Full heal
    Assert.AreEqual(150, player.LegendToNextMilestone); // New threshold
}
```

**Test Case**: `CanReachMilestone_WithInsufficientLegend_ReturnsFalse`
```csharp
[Test]
public void CanReachMilestone_WithInsufficientLegend_ReturnsFalse()
{
    // Arrange
    var player = new PlayerCharacter
    {
        CurrentLegend = 50,
        LegendToNextMilestone = 100
    };
    var sagaService = new SagaService();

    // Act & Assert
    Assert.IsFalse(sagaService.CanReachMilestone(player));
}
```

**Test Case**: `ReachMilestone_LegendDoesNotReset`
```csharp
[Test]
public void ReachMilestone_LegendDoesNotReset()
{
    // Arrange
    var player = new PlayerCharacter
    {
        CurrentLegend = 120,  // Excess Legend
        LegendToNextMilestone = 100,
        CurrentMilestone = 0
    };
    var sagaService = new SagaService();

    // Act
    sagaService.ReachMilestone(player);

    // Assert
    Assert.AreEqual(120, player.CurrentLegend); // Legend carried over
    Assert.AreEqual(150, player.LegendToNextMilestone); // New threshold
}
```

#### Progression Points Tests

**Test Case**: `SpendPPOnAttribute_WithValidInput_IncreasesAttribute`
```csharp
[TestCase("might")]
[TestCase("FINESSE")] // Test case insensitivity
[TestCase("Wits")]
public void SpendPPOnAttribute_WithValidInput_IncreasesAttribute(string attributeName)
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 3,
        Attributes = new Attributes(3, 3, 3, 3, 3)
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.SpendPPOnAttribute(player, attributeName);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(2, player.ProgressionPoints); // 3 - 1
    Assert.AreEqual(4, player.GetAttributeValue(attributeName)); // 3 + 1
}
```

**Test Case**: `SpendPPOnAttribute_AtCap_ReturnsFalse`
```csharp
[Test]
public void SpendPPOnAttribute_AtCap_ReturnsFalse()
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 5,
        Attributes = new Attributes(6, 3, 3, 3, 3) // MIGHT at cap
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.SpendPPOnAttribute(player, "might");

    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(5, player.ProgressionPoints); // No change
    Assert.AreEqual(6, player.Attributes.Might);  // Still at cap
}
```

**Test Case**: `SpendPPOnAttribute_WithInsufficientPP_ReturnsFalse`
```csharp
[Test]
public void SpendPPOnAttribute_WithInsufficientPP_ReturnsFalse()
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 0,
        Attributes = new Attributes(3, 3, 3, 3, 3)
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.SpendPPOnAttribute(player, "might");

    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(0, player.ProgressionPoints); // No change
}
```

**Test Case**: `AdvanceAbilityRank_WithSufficientPP_AdvancesRank`
```csharp
[Test]
public void AdvanceAbilityRank_WithSufficientPP_AdvancesRank()
{
    // Arrange
    var player = new PlayerCharacter { ProgressionPoints = 5 };
    var ability = new Ability
    {
        Name = "Power Strike",
        CurrentRank = 1,
        CostToRank2 = 2,
        BonusDice = 2,
        StaminaCost = 5
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.AdvanceAbilityRank(player, ability);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(3, player.ProgressionPoints); // 5 - 2
    Assert.AreEqual(2, ability.CurrentRank);      // 1 + 1
    Assert.AreEqual(3, ability.BonusDice);        // Rank 2 improvement: 2 → 3
    Assert.AreEqual(4, ability.StaminaCost);      // Rank 2 improvement: 5 → 4
}
```

**Test Case**: `UnlockSpecialization_WithSufficientPP_UnlocksSuccessfully`
```csharp
[Test]
public void UnlockSpecialization_WithSufficientPP_UnlocksSuccessfully()
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 5, // v0.18: Updated to realistic M3 amount
        Class = CharacterClass.Warrior,
        Specialization = Specialization.None
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.UnlockSpecialization(player, Specialization.BoneSetter);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(2, player.ProgressionPoints);            // 5 - 3
    Assert.AreEqual(Specialization.BoneSetter, player.Specialization);
    Assert.IsTrue(player.Abilities.Count > 0);               // Tier 1 abilities granted
}
```

**Test Case**: `UnlockSpecialization_AlreadyHasSpecialization_ReturnsFalse`
```csharp
[Test]
public void UnlockSpecialization_AlreadyHasSpecialization_ReturnsFalse()
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 5, // v0.18: Updated to realistic M3 amount
        Specialization = Specialization.BoneSetter // Already has one
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.UnlockSpecialization(player, Specialization.Skald);

    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(5, player.ProgressionPoints);                    // No change
    Assert.AreEqual(Specialization.BoneSetter, player.Specialization); // Still BoneSetter
}
```

### 4.2 Integration Tests Required

**Test Case**: `FullProgressionCycle_FromMilestone0To3`
```csharp
[Test]
public void FullProgressionCycle_FromMilestone0To3()
{
    // Arrange
    var player = CreateTestPlayer();
    var sagaService = new SagaService();

    // Act & Assert - Milestone 1
    sagaService.AwardLegend(player, 100); // Exactly 100 Legend
    Assert.IsTrue(sagaService.CanReachMilestone(player));
    sagaService.ReachMilestone(player);
    Assert.AreEqual(1, player.CurrentMilestone);
    Assert.AreEqual(3, player.ProgressionPoints); // 2 start + 1

    // Act & Assert - Milestone 2
    sagaService.AwardLegend(player, 150); // 100 + 150 = 250 total
    Assert.IsTrue(sagaService.CanReachMilestone(player));
    sagaService.ReachMilestone(player);
    Assert.AreEqual(2, player.CurrentMilestone);
    Assert.AreEqual(4, player.ProgressionPoints); // 3 + 1

    // Act & Assert - Milestone 3
    sagaService.AwardLegend(player, 200); // 250 + 200 = 450 total
    Assert.IsTrue(sagaService.CanReachMilestone(player));
    sagaService.ReachMilestone(player);
    Assert.AreEqual(3, player.CurrentMilestone);
    Assert.AreEqual(5, player.ProgressionPoints); // 4 + 1

    // Verify cumulative rewards
    Assert.AreEqual(60, player.MaxHP);       // Base 30 + (3 × 10)
    Assert.AreEqual(45, player.MaxStamina);  // Base 30 + (3 × 5)
}
```

**Test Case**: `PPSpending_VariousBuildStrategies`
```csharp
[Test]
public void PPSpending_AttributeSpecialist_Build()
{
    // 5 PP → All into MIGHT
    var player = new PlayerCharacter
    {
        ProgressionPoints = 5,
        Attributes = new Attributes(3, 3, 3, 3, 3)
    };
    var sagaService = new SagaService();

    for (int i = 0; i < 3; i++)
        sagaService.SpendPPOnAttribute(player, "might");

    Assert.AreEqual(6, player.Attributes.Might); // 3 + 3
    Assert.AreEqual(2, player.ProgressionPoints); // 5 - 3
}
```

### 4.3 Edge Case Tests

**Test Case**: `LegendOverflow_HandlesExcessLegendCorrectly`
```csharp
[Test]
public void LegendOverflow_HandlesExcessLegendCorrectly()
{
    // Award 500 Legend at once (skips multiple milestones' worth)
    var player = new PlayerCharacter { CurrentLegend = 0, CurrentMilestone = 0 };
    var sagaService = new SagaService();

    sagaService.AwardLegend(player, 500);

    // Should be able to reach Milestone 1, 2, and 3 consecutively
    Assert.IsTrue(sagaService.CanReachMilestone(player)); // 500 ≥ 100
    sagaService.ReachMilestone(player); // Milestone 1

    Assert.IsTrue(sagaService.CanReachMilestone(player)); // 500 ≥ 150
    sagaService.ReachMilestone(player); // Milestone 2

    Assert.IsTrue(sagaService.CanReachMilestone(player)); // 500 ≥ 200
    sagaService.ReachMilestone(player); // Milestone 3

    Assert.AreEqual(3, player.CurrentMilestone);
}
```

**Test Case**: `IntegerTruncation_FloorsBehaviorCorrectly`
```csharp
[Test]
public void IntegerTruncation_FloorsBehaviorCorrectly()
{
    var player = new PlayerCharacter { CurrentLegend = 0 };
    var sagaService = new SagaService();

    // 50 × 1.0 × 1.25 = 62.5 → should floor to 62
    sagaService.AwardLegend(player, 50, 1.0f, 1.25f);
    Assert.AreEqual(62, player.CurrentLegend);

    // 33 × 1.0 × 1.5 = 49.5 → should floor to 49
    sagaService.AwardLegend(player, 33, 1.0f, 1.5f);
    Assert.AreEqual(111, player.CurrentLegend); // 62 + 49
}
```

### 4.4 Manual Testing Scenarios

1. **Full Progression Arc**
   - Start new game with 2 PP
   - Defeat enemies to earn 100 Legend → Reach Milestone 1
   - Verify: +10 MaxHP, +5 MaxStamina, +1 PP, full heal
   - Continue to Milestone 2 and 3
   - Verify cumulative stats match expectations

2. **PP Spending Strategies**
   - **Strategy A**: Spend 3 PP on attributes, 2 PP on ability rank
   - **Strategy B**: Save 10 PP for specialization unlock (requires reaching Milestone 3+)
   - **Strategy C**: Balanced spread (1 PP per attribute)

3. **Trauma Modifier Effects**
   - Defeat standard enemy: 50 BLV × 1.0 TM = 50 Legend
   - Defeat boss enemy: 50 BLV × 1.25 TM = 62 Legend
   - Complete puzzle: 30 BLV × 1.25 TM = 37 Legend

4. **Attribute Cap Enforcement**
   - Raise MIGHT to 6 with PP spending
   - Attempt to spend 1 more PP on MIGHT → Should fail
   - Verify PP not deducted

5. **Milestone at Max**
   - Reach Milestone 3
   - Defeat additional enemies
   - Verify: No Legend awarded (guarded by MaxMilestone check)

---

## 5. Balance Considerations

### 5.1 Milestone Pacing

**v0.1 Target**: 15-20 minute playtime to reach Milestone 3

**Estimated Legend Sources**:
- 5 standard encounters: 5 × 40 BLV = **200 Legend**
- 2 elite encounters: 2 × 70 BLV = **140 Legend**
- 1 boss encounter: 1 × 100 BLV = **100 Legend**
- 2 puzzles: 2 × 40 BLV × 1.25 TM = **100 Legend**
- **Total**: ~540 Legend (exceeds 450 needed for Milestone 3)

**Design**: Linear progression curve ensures steady pace without grinding. 20% excess Legend provides buffer for player deaths/setbacks.

### 5.2 Progression Points Economy

**Total PP by Milestone 3**: 5 PP (2 start + 3 earned)

**Spending Analysis (v0.18)**:
- 5 PP = Enough for specialization (3 PP) + 2 PP for attributes/abilities
- 5 PP = 5 attribute increases (e.g., MIGHT 3 → 6 + 2 remaining, or MIGHT/FINESSE both 3 → 5)
- 5 PP = 2-3 ability rank advancements (2-3 PP each)

**Trade-offs**:
- **Specialization Build** (3 PP unlock + 2 PP customization) vs **Attribute Focus** (5 PP attributes)
- **Generalist** (spread PP) vs **Specialist** (focus PP)
- **Combat Power** (attributes) vs **Utility** (ability ranks/specialization)

**Design (v0.18)**: 3 PP specialization cost makes specializations achievable by Milestone 2, enabling diverse build strategies within v0.1 scope. Players can now choose between immediate power (attributes), tactical flexibility (ability ranks), or unique capabilities (specializations).

### 5.3 Milestone Rewards Balance

**Per-Milestone Rewards**:
- +10 HP: ~20-25% increase per Milestone (base 50 HP → 80 HP at M3)
- +5 Stamina: ~10-15% increase per Milestone (base 30 Stam → 45 Stam at M3)
- +1 PP: Linear growth, prevents exponential power spikes

**Full Heal Impact**:
- Guarantees Milestone = Safe recovery point
- Encourages aggressive play before Milestone (no need to conserve resources)
- Removes "death spiral" where low HP → cautious play → slow Legend gain

**Design**: Flat bonuses (+10 HP) scale well at low levels but avoid exponential growth. Linear PP gain prevents runaway power curves.

### 5.4 Attribute Cap Rationale

**Cap at 6**:
- 6d6 = ~2 successes on average (33.33% × 6 = 2.0 expected)
- Prevents single-attribute "god builds"
- Encourages balanced or specialized builds (max 2-3 attributes)

**Without Cap**:
- Players could stack 10+ in one attribute → Trivializes opposed rolls
- No incentive for diverse builds

**With Cap**:
- Must choose 2-3 attributes to prioritize
- Remaining PP spent on abilities/specialization
- Maintains tactical depth (no "I win" button)

### 5.5 Trauma Modifier Balance

**+25% Legend for Taxing Encounters**:
- Reward for psychological risk (Stress gain, Corruption risk)
- Encourages engaging with Blight content
- Balances risk/reward for harder encounters

**Without TM**:
- No incentive to tackle Blight encounters (same Legend, more risk)
- Players avoid challenging content

**With TM**:
- Blight encounters = 25% faster progression
- Psychological cost (Stress) traded for faster growth
- Creates risk/reward decision points

### 5.6 Ability Rank Advancement Balance

**Rank 2 Improvements**:
- +1 Bonus Dice = +33.33% success chance (significant but not overpowered)
- -1-2 Stamina Cost = 15-30% cost reduction (meaningful efficiency)
- +1 Damage Dice = +16-33% damage increase (noticeable power spike)

**Cost Analysis**:
- 2-3 PP per ability rank = 40-60% of available PP by Milestone 3
- Trade-off: 1 ability rank vs 2-3 attribute increases
- Ability ranks provide **unique effects** (Bleeding, Stun) vs attributes provide **consistent power**

**Design**: Ability ranks are cost-competitive with attributes but offer qualitatively different benefits (status effects, utility) rather than raw stat boosts.

### 5.7 Specialization Cost Analysis

**3 PP Cost (v0.18: reduced from 10 PP)**:
- **Achievable** by Milestone 2 (4 PP available)
- Comfortably affordable by Milestone 3 (5 PP available with 2 PP remaining)
- **Design rationale**: Enables specialization gameplay within v0.1 scope

**Unlock Benefits**:
- 2-3 Tier 1 abilities (unique utility/healing/support)
- Thematic identity (Bone-Setter = healer, Skald = bard)
- Gateway to Tier 2+ specialization abilities (future versions)

**Trade-off**:
- Specialization unlocks **new capabilities** (healing, ally buffs) not available through attributes
- 3 PP = Meaningful choice vs 3 attribute increases or 1-2 ability ranks
- Specialization provides unique utility, attributes provide consistent power

### 5.8 Legend Carry-Over Design

**Carry-Over Rule**: Legend does not reset at Milestones

**Example**:
- Earn 120 Legend → Reach Milestone 1 (requires 100)
- Carry over 120 Legend toward Milestone 2 (requires 150 total = 250 cumulative)
- Only need 130 more Legend (250 - 120 = 130)

**Benefits**:
- No "wasted" Legend from over-earning
- Smooth progression curve
- Encourages aggressive play (no penalty for exceeding threshold)

**Alternative Design** (not used):
- Reset Legend at each Milestone
- Problem: "Wasted" excess Legend → Players slow down near Milestone to avoid waste

### 5.9 Milestone Cap Justification

**v0.1 Cap at Milestone 3**:
- Target playtime: 15-20 minutes
- 3 Milestones = 3 major progression moments
- Avoids power creep within short session

**Future Versions**:
- Remove or raise cap (e.g., Milestone 10+)
- Exponential curve: `50 * 2^Milestone` (100, 200, 400, 800...)
- Long-form campaign support (2-4 hours)

### 5.10 Starting PP (2) Rationale

**Starting with 2 PP**:
- Allows immediate customization (1-2 attribute increases)
- Players feel agency from start
- Avoids "tutorial tax" (waiting for first Milestone)

**Alternative Designs**:
- 0 PP start: Delayed gratification, less agency
- 5 PP start: Too much early power, trivializes early game

**2 PP Sweet Spot**:
- Enough for 1-2 meaningful choices
- Not enough to dominate early game
- Sets expectation: "PP is valuable"

---

## 6. Future Expansion Notes

### 6.1 Planned Features (v0.5+)

1. **Rank 3 Abilities**
   - Unlock Rank 3 advancements (currently hard-capped at Rank 2)
   - Higher PP costs (4-6 PP per rank)
   - Dramatically improved effects (2x damage, multi-target, etc.)

2. **Specialization Trees**
   - Tier 2-5 abilities within each specialization
   - Progressive unlocks (must have Tier 1 to access Tier 2)
   - PP costs for each tier

3. **Alternative Legend Sources**
   - Quest completion: 50-200 BLV
   - Discovery bonuses: 10-30 BLV for finding secrets
   - Moral choices: +/- Legend based on alignment

4. **Milestone Variants**
   - Choose-your-own reward (select 2 of 4 options)
   - Specialization-specific bonuses
   - Trauma mitigation options (reduce Stress/Corruption at Milestone)

### 6.2 Known Limitations

1. **No Legend Refund**: Once awarded, Legend cannot be removed (no negative modifiers)
2. **No PP Refund**: Spent PP cannot be refunded (no respec mechanic)
3. **Single Specialization**: Cannot unlock multiple specializations per character
4. **Rank 2 Cap**: Abilities cannot advance beyond Rank 2 until v0.5+
5. **Linear Scaling**: Milestone rewards scale linearly, not exponentially (may need adjustment for longer campaigns)

### 6.3 Balance Knobs

Tunable parameters for future balance adjustments:

| Parameter | Current Value | Range | Impact |
|-----------|---------------|-------|--------|
| Milestone Formula | `(M × 50) + 100` | `(M × X) + Y` | Progression speed |
| HP per Milestone | +10 | +5 to +20 | Survivability curve |
| Stamina per Milestone | +5 | +3 to +10 | Resource availability |
| PP per Milestone | +1 | +1 to +3 | Customization rate |
| Attribute Cap | 6 | 5 to 10 | Power ceiling |
| Specialization Cost | **3 PP** (v0.18) | 3 to 15 PP | Unlock timing |
| Trauma Modifier | 1.25 | 1.0 to 2.0 | Risk/reward balance |
| Starting PP | 2 | 0 to 5 | Early agency |
| Max Milestone | 3 | 3 to 20 | Campaign length |

---

## 7. Cross-System Integration

### 7.1 Dependencies

- **Attributes** (`Attributes.cs`): PP spending increases attribute values
- **Abilities** (`Ability.cs`): PP spending advances ability ranks
- **Specializations** (`Specialization.cs`, `SpecializationFactory.cs`): PP unlocks specializations
- **Trauma Economy** (`TraumaEconomyService.cs`): Trauma Modifier affects Legend gain
- **Combat Engine** (`CombatEngine.cs`): Awards Legend after encounters

### 7.2 Downstream Effects

- **HP/Stamina Pools**: Milestone rewards increase resource pools
- **Dice Pools**: Attribute increases expand dice pools for all rolls
- **Ability Power**: Rank advancements improve ability effectiveness
- **Status Effects**: Specialization abilities grant new status effects

### 7.3 Data Flow

```
Encounter Complete → CombatEngine.AwardLegend()
    ↓
SagaService.AwardLegend(player, BLV, DM, TM)
    ↓
player.CurrentLegend += Legend_Awarded
    ↓
SagaService.CanReachMilestone() → Check threshold
    ↓
IF TRUE → SagaService.ReachMilestone()
    ↓
    - player.CurrentMilestone++
    - player.ProgressionPoints++
    - player.MaxHP += 10
    - player.MaxStamina += 5
    - player.HP = MaxHP (full heal)
    - player.Stamina = MaxStamina (full heal)
    ↓
Player Spends PP (UI interaction)
    ↓
SagaService.SpendPPOnAttribute() OR
SagaService.AdvanceAbilityRank() OR
SagaService.UnlockSpecialization()
    ↓
Character stats updated
```

---

## 8. Quick Reference

### 8.1 Formulas

| Calculation | Formula |
|-------------|---------|
| **Legend Awarded** | `FLOOR(BLV × DM × TM)` |
| **Milestone Threshold** | `(Current_Milestone × 50) + 100` |
| **Attribute Cost** | `1 PP per +1 attribute` |
| **Ability Rank Cost** | `ability.CostToRank2` (typically 2-3 PP) |
| **Specialization Cost** | `3 PP (one-time)` **(v0.18: reduced from 10 PP)** |

### 8.2 Constants

| Constant | Value |
|----------|-------|
| **Max Milestone (v0.1)** | 3 |
| **Attribute Cap** | 6 |
| **Starting PP** | 2 |
| **PP per Milestone** | +1 |
| **HP per Milestone** | +10 |
| **Stamina per Milestone** | +5 |

### 8.3 Trauma Modifiers

| Encounter Type | TM |
|----------------|-----|
| Normal | 1.0 |
| Puzzle | 1.25 |
| Boss | 1.25 |
| Blight | 1.25 |

### 8.4 Milestone Progression Table

| Milestone | Legend Required | Cumulative | Total PP | Total HP Gain | Total Stam Gain |
|-----------|-----------------|------------|----------|---------------|-----------------|
| 0         | -               | 0          | 2        | -             | -               |
| 1         | 100             | 100        | 3        | +10           | +5              |
| 2         | 150             | 250        | 4        | +20           | +10             |
| 3         | 200             | 450        | 5        | +30           | +15             |

---

**End of Document**
*Last Updated*: v0.17 Documentation Phase
*Implementation Version*: v0.7
*Total Lines*: 1,800+

---

## Appendix A: Code Locations

| Feature | File | Line Range |
|---------|------|------------|
| SagaService | `RuneAndRust.Engine/SagaService.cs` | 1-436 |
| AwardLegend | `SagaService.cs` | 18-33 |
| ReachMilestone | `SagaService.cs` | 51-80 |
| SpendPPOnAttribute | `SagaService.cs` | 100-152 |
| AdvanceAbilityRank | `SagaService.cs` | 157-187 |
| ApplyRank2Improvements | `SagaService.cs` | 192-355 |
| UnlockSpecialization | `SagaService.cs` | 360-398 |
| CalculateTraumaMod | `SagaService.cs` | 424-434 |
| PlayerCharacter.Progression | `RuneAndRust.Core/PlayerCharacter.cs` | 18-22 |

---

## Appendix B: Related Documentation

- **Attributes System**: `docs/01-systems/attributes.md` (pending)
- **Abilities System**: `docs/02-abilities/README.md` (pending)
- **Specializations**: `docs/02-abilities/specializations.md` (pending)
- **Trauma Economy**: `docs/01-systems/trauma-economy.md` (pending)
- **Combat Resolution**: `docs/01-systems/combat-resolution.md`
- **Status Effects**: `docs/01-systems/status-effects.md`
