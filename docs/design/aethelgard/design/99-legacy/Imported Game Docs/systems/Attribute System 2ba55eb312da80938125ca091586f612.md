# Attribute System

Parent item: System Documentation (System%20Documentation%202ba55eb312da801a9aa4f30d6e439959.md)

**Version**: v0.17 (Based on v0.7 implementation)
**Status**: Core System (Tier 1)
**Dependencies**: Dice Service, Equipment System, Trauma Economy
**Integration Points**: Combat Engine, Saga Service, Equipment Service, Resolve Check Service

---

## Table of Contents

1. [Functional Overview](Attribute%20System%202ba55eb312da80938125ca091586f612.md)
2. [Statistical Reference](Attribute%20System%202ba55eb312da80938125ca091586f612.md)
3. [Technical Implementation](Attribute%20System%202ba55eb312da80938125ca091586f612.md)
4. [Testing Coverage](Attribute%20System%202ba55eb312da80938125ca091586f612.md)
5. [Balance Considerations](Attribute%20System%202ba55eb312da80938125ca091586f612.md)

---

## 1. Functional Overview

### 1.1 Purpose

**Attributes** are the five core statistics that define a character's capabilities in Rune & Rust. Every character action—combat, skill checks, resistance rolls—is resolved by rolling a number of dice equal to the relevant attribute value and counting successes (5-6 on d6).

The attribute system serves three primary functions:

1. **Dice Pool Determination**: Attribute value = number of d6 rolled for checks
2. **Character Differentiation**: Different archetypes excel in different attributes
3. **Progression Currency**: Players spend Progression Points (PP) to increase attributes

### 1.2 The Five Attributes

### MIGHT

**Archetype**: Physical Strength & Power

**Primary Uses**:

- **Attack Rolls**: Axes, Greatswords, Heavy Blunt weapons
- **Damage Calculation**: MIGHT-based weapons deal damage based on these rolls
- **Physical Challenges**: Breaking down doors, lifting heavy objects

**Thematic Description**: Raw physical power, brute force, and muscular strength. The domain of warriors who crush enemies with overwhelming might.

**Example Weapons**: Battle Axe, Greatsword, War Hammer, Dvergr Maul

---

### FINESSE

**Archetype**: Speed, Precision & Dexterity

**Primary Uses**:

- **Initiative Rolls**: Determines turn order in combat (FINESSE dice pool)
- **Attack Rolls**: Spears, Daggers, Precision weapons
- **Dodge/Evasion**: Quick reflexes and agile movement
- **Crafting**: Fine manipulation for delicate work

**Thematic Description**: Lightning-fast reflexes, surgical precision, and graceful movement. The domain of rogues, skirmishers, and those who strike first.

**Example Weapons**: Spear, Dagger, Rapier, Aether-Blade

---

### WITS

**Archetype**: Intelligence, Perception & Problem-Solving

**Primary Uses**:

- **Puzzle Solving**: Navigational challenges, logic puzzles, pattern recognition
- **Perception**: Spotting hidden threats, detecting traps
- **Crafting**: Complex recipes and experimental items
- **Dialogue Checks**: Insight, deduction, reading situations

**Thematic Description**: Sharp intellect, keen observation, and analytical thinking. The domain of scholars, investigators, and tacticians.

**Example Uses**: Deciphering Jötun runes, analyzing enemy patterns, solving the Silent Arbiter's riddles

---

### WILL

**Archetype**: Mental Fortitude & Spiritual Strength

**Primary Uses**:

- **Attack Rolls**: Staves, Foci, Psychic weapons
- **Resolve Checks**: Resisting Psychic Stress, mental attacks, Forlorn auras
- **Stress Resistance**: WILL successes reduce incoming Stress by 1:1 ratio
- **Magical Abilities**: Casting spells and channeling aetheric energy

**Thematic Description**: Unshakable resolve, inner strength, and force of personality. The domain of mystics, willworkers, and those who resist the Blight's corruption.

**Example Weapons**: Wooden Staff, Iron-Bound Staff, Aetheric Focus, Jötun Focus

**Critical Role**: WILL is the **only** defense against Psychic Stress and Corruption, making it essential for survival in Blight-corrupted areas.

---

### STURDINESS

**Archetype**: Durability, Endurance & Defense

**Primary Uses**:

- **Defense Rolls**: Determines dice pool for all defense rolls in combat
- **Damage Mitigation**: Higher STURDINESS = more likely to reduce incoming damage
- **Endurance**: Resisting physical hazards (poison, disease, exhaustion)
- **HP Calculation**: Indirectly affects survivability through defense

**Thematic Description**: Toughness, resilience, and the ability to withstand punishment. The domain of tanks, survivors, and those who refuse to fall.

**Example Uses**: Defense against attacks, resisting environmental hazards, enduring long expeditions

**Critical Role**: STURDINESS is the **universal defense attribute**, used against all physical attacks regardless of enemy type.

---

### 1.3 Attribute Ranges

| Value | Descriptor | Expected Successes | Competency Level |
| --- | --- | --- | --- |
| 0 | Incapacitated | 0 | Cannot perform action |
| 1 | Weak | 0.33 | Barely functional |
| 2 | Below Average | 0.67 | Struggling |
| 3 | Average | 1.0 | Competent baseline |
| 4 | Above Average | 1.33 | Skilled |
| 5 | Exceptional | 1.67 | Expert |
| 6 | Legendary (Cap) | 2.0 | Peak human performance |

**Success Rate**: Each die has a **33.33%** chance of success (rolling 5-6 on d6).

**Expected Successes Formula**: `Attribute_Value × 0.333`

### 1.4 Gameplay Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Action Declared (Attack, Defend, Skill Check)           │
│    ↓                                                         │
│ 2. Determine Relevant Attribute                             │
│    - Attack with Greatsword → MIGHT                         │
│    - Defend against attack → STURDINESS                     │
│    - Resist Stress → WILL                                   │
│    - Initiative roll → FINESSE                              │
│    ↓                                                         │
│ 3. Calculate Dice Pool                                      │
│    Base_Dice = Attribute_Value                              │
│    + Equipment Bonuses (if any)                             │
│    + Ability Bonuses (if any)                               │
│    + Status Effect Modifiers (if any)                       │
│    ↓                                                         │
│ 4. Roll Xd6 (DiceService)                                   │
│    ↓                                                         │
│ 5. Count Successes (5-6 = success)                          │
│    ↓                                                         │
│ 6. Apply Successes to Context                               │
│    - Combat: Compare attack vs defense successes            │
│    - Skill Check: Meet DC threshold                         │
│    - Stress Resistance: Reduce stress by successes          │
└─────────────────────────────────────────────────────────────┘

```

---

## 2. Statistical Reference

### 2.1 Success Probability Table

| Attribute Value | Dice Rolled | Expected Successes | 0 Successes | 1+ Success | 2+ Successes | 3+ Successes |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 1d6 | 0.33 | 66.67% | 33.33% | 0% | 0% |
| 2 | 2d6 | 0.67 | 44.44% | 55.56% | 11.11% | 0% |
| 3 | 3d6 | 1.0 | 29.63% | 70.37% | 25.93% | 3.70% |
| 4 | 4d6 | 1.33 | 19.75% | 80.25% | 39.51% | 9.88% |
| 5 | 5d6 | 1.67 | 13.17% | 86.83% | 50.21% | 17.90% |
| 6 | 6d6 | 2.0 | 8.78% | 91.22% | 59.20% | 27.49% |

**Key Insights**:

- **Attribute 1-2**: High failure rate, unreliable
- **Attribute 3**: Baseline competence (~70% chance of 1+ success)
- **Attribute 4-5**: Consistent performance, occasional multiple successes
- **Attribute 6 (Cap)**: ~91% chance of at least 1 success, ~59% chance of 2+ successes

### 2.2 Opposed Roll Probabilities

**Scenario**: Attacker with X dice vs Defender with Y dice

| Attacker | Defender | Attacker Wins | Tie | Defender Wins |
| --- | --- | --- | --- | --- |
| 3d6 | 3d6 | 42.5% | 15% | 42.5% |
| 4d6 | 3d6 | 54.3% | 12% | 33.7% |
| 5d6 | 3d6 | 63.8% | 10% | 26.2% |
| 6d6 | 3d6 | 71.2% | 8% | 20.8% |
| 4d6 | 4d6 | 42.5% | 15% | 42.5% |
| 5d6 | 4d6 | 54.1% | 12% | 33.9% |
| 6d6 | 4d6 | 63.5% | 10% | 26.5% |
| 6d6 | 5d6 | 54.2% | 12% | 33.8% |

**Key Insights**:

- **+1 dice advantage**: ~54% win rate (~12% improvement)
- **+2 dice advantage**: ~64% win rate (~22% improvement)
- **+3 dice advantage**: ~71% win rate (~29% improvement)
- **Equal dice pools**: ~42.5% each to win, ~15% tie

**Design Note**: Each additional die provides diminishing returns but remains valuable.

### 2.3 Attribute Increase Costs

**PP Cost**: 1 PP per +1 attribute (flat cost)

| Current Value | Target Value | Total PP Cost | Cumulative PP |
| --- | --- | --- | --- |
| 3 (starting) | 4 | 1 PP | 1 PP |
| 4 | 5 | 1 PP | 2 PP |
| 5 | 6 (cap) | 1 PP | 3 PP |
| **3 → 6** | - | **3 PP** | - |

**Design**: Linear cost prevents exponential price increases, making all attribute increases equally valuable.

### 2.4 Starting Attributes by Archetype

| Archetype | MIGHT | FINESSE | WITS | WILL | STURDINESS | Total |
| --- | --- | --- | --- | --- | --- | --- |
| Warrior | 4 | 3 | 2 | 2 | 4 | 15 |
| Scavenger | 3 | 4 | 3 | 2 | 3 | 15 |
| Mystic | 2 | 3 | 3 | 4 | 3 | 15 |

**Design Philosophy**:

- Each archetype starts with **15 total attribute points**
- Primary attributes at **4** (above average)
- Secondary attributes at **3** (average competence)
- Dump attributes at **2** (below average)

### 2.5 Equipment Attribute Bonuses

**Equipment can grant +1 to +3 attribute bonuses**:

```
Effective_Attribute = Base_Attribute + Weapon_Bonuses + Armor_Bonuses

```

**Example Equipment Bonuses**:

- **Aether-Forged Greatsword**: +2 MIGHT
- **Sentinel's Plate**: +2 STURDINESS
- **Scholar's Robes**: +1 WITS
- **Jötun Focus (Legendary)**: +3 WILL

**Cap Interaction**: Equipment bonuses can **exceed the 6-attribute cap**. If you have MIGHT 6 and equip +2 MIGHT weapon, your effective MIGHT is **8** (8d6 for attack rolls).

**Balance Note**: Equipment bonuses are the only way to exceed the 6-attribute hard cap, making legendary equipment highly valuable.

### 2.6 Attribute Use Cases by System

| System | Attributes Used | Purpose |
| --- | --- | --- |
| **Combat** | MIGHT/FINESSE/WILL | Attack rolls based on weapon type |
| **Combat** | STURDINESS | Universal defense rolls |
| **Combat** | FINESSE | Initiative (turn order) |
| **Trauma Economy** | WILL | Resist Psychic Stress (1:1 reduction) |
| **Trauma Economy** | WILL | Forlorn aura resistance (DC check) |
| **Puzzles** | WITS | Navigational checks, logic puzzles |
| **Crafting** | WITS/FINESSE | Recipe skill checks |
| **Dialogue** | WITS/WILL | Insight, persuasion, intimidation |
| **Hazards** | Various | Room-specific hazard checks |

---

## 3. Technical Implementation

### 3.1 Core Data Model

**File**: `RuneAndRust.Core/Attributes.cs` (22 lines)

```csharp
namespace RuneAndRust.Core;

public class Attributes
{
    public int Might { get; set; }
    public int Finesse { get; set; }
    public int Wits { get; set; }
    public int Will { get; set; }
    public int Sturdiness { get; set; }

    public Attributes() { }

    public Attributes(int might, int finesse, int wits, int will, int sturdiness)
    {
        Might = might;
        Finesse = finesse;
        Wits = wits;
        Will = will;
        Sturdiness = sturdiness;
    }
}

```

**Design**: Simple POCO (Plain Old C# Object) with no logic, only data storage.

### 3.2 Attribute Value Retrieval

**File**: `RuneAndRust.Core/PlayerCharacter.cs:87-98`

```csharp
public int GetAttributeValue(string attributeName)
{
    return attributeName.ToLower() switch
    {
        "might" => Attributes.Might,
        "finesse" => Attributes.Finesse,
        "wits" => Attributes.Wits,
        "will" => Attributes.Will,
        "sturdiness" => Attributes.Sturdiness,
        _ => 0  // Invalid attribute returns 0
    };
}

```

**Features**:

- **Case-insensitive**: Accepts "might", "Might", "MIGHT"
- **Safe default**: Returns 0 for invalid attribute names
- **Used throughout codebase**: Combat, abilities, equipment, dialogue

**Usage Example**:

```csharp
int mightValue = player.GetAttributeValue("might"); // Returns player.Attributes.Might
int dicePool = player.GetAttributeValue("FINESSE"); // Returns player.Attributes.Finesse

```

### 3.3 Effective Attribute Calculation (Equipment Bonuses)

**File**: `RuneAndRust.Engine/EquipmentService.cs:266-290`

```csharp
/// <summary>
/// Get effective attribute value including equipment bonuses
/// </summary>
public int GetEffectiveAttributeValue(PlayerCharacter player, string attributeName)
{
    int baseValue = player.GetAttributeValue(attributeName);
    int bonus = 0;

    // Add bonuses from equipped weapon
    if (player.EquippedWeapon != null)
    {
        foreach (var equipBonus in player.EquippedWeapon.Bonuses)
        {
            if (equipBonus.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            {
                bonus += equipBonus.BonusValue;
            }
        }
    }

    // Add bonuses from equipped armor
    if (player.EquippedArmor != null)
    {
        foreach (var equipBonus in player.EquippedArmor.Bonuses)
        {
            if (equipBonus.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            {
                bonus += equipBonus.BonusValue;
            }
        }
    }

    return baseValue + bonus;
}

```

**Logic Flow**:

1. Get base attribute value from character
2. Iterate through weapon bonuses, sum matching attributes
3. Iterate through armor bonuses, sum matching attributes
4. Return base + total bonuses

**Example**:

```csharp
// Player: MIGHT 4, equipped Aether-Forged Greatsword (+2 MIGHT)
int effectiveMight = equipmentService.GetEffectiveAttributeValue(player, "might");
// Result: 4 + 2 = 6

```

### 3.4 Attribute Increase via Progression Points

**File**: `RuneAndRust.Engine/SagaService.cs:100-152`

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
    if (currentValue >= AttributeCap)  // AttributeCap = 6
    {
        _log.Warning("PP spend failed: Reason=AttributeAtCap, CurrentValue={Value}, Cap={Cap}",
            currentValue, AttributeCap);
        return false;
    }

    // Spend PP and increase attribute
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
            throw new ArgumentException($"Invalid attribute name: {attributeName}");
    }

    return true;
}

```

**Validation**:

1. Check PP ≥ 1 (cost of attribute increase)
2. Check attribute < 6 (hard cap enforcement)
3. Deduct 1 PP
4. Increment attribute by 1
5. Return true on success, false on failure

**Error Handling**: Invalid attribute name throws `ArgumentException` and refunds PP.

### 3.5 Combat Integration: Attack Rolls

**File**: `RuneAndRust.Engine/CombatEngine.cs:462`

```csharp
// Get attribute value for attack roll based on weapon or ability
var attributeValue = player.GetAttributeValue(ability.AttributeUsed);
var attackRoll = _diceService.Roll(attributeValue + bonusDice);

```

**Weapons have an `AttributeUsed` property**:

- Greatsword: "MIGHT"
- Spear: "FINESSE"
- Staff: "WILL"

**Example Flow**:

```csharp
// Player attacks with Greatsword (MIGHT-based weapon)
// Player MIGHT: 4
// Ability bonus dice: +2 (Power Strike ability)
int attributeValue = player.GetAttributeValue("might");  // Returns 4
int totalDice = attributeValue + 2;  // 4 + 2 = 6
var attackRoll = diceService.Roll(6);  // Roll 6d6

```

### 3.6 Combat Integration: Defense Rolls

**File**: `RuneAndRust.Engine/CombatEngine.cs` (defense roll logic)

```csharp
// STURDINESS is always used for defense rolls
int defenseAttribute = player.Attributes.Sturdiness;
var defenseRoll = _diceService.Roll(defenseAttribute);

```

**Design**: STURDINESS is the universal defense attribute, regardless of attack type.

### 3.7 Combat Integration: Initiative Rolls

**File**: `RuneAndRust.Engine/CombatEngine.cs` (initiative logic)

```csharp
// FINESSE determines initiative (turn order)
var playerInitiativeRoll = _diceService.Roll(player.Attributes.Finesse);

```

**Tie-Breaking**: If initiative successes are equal, higher FINESSE attribute value breaks the tie.

### 3.8 Resolve Check Integration: WILL-based Stress Resistance

**File**: `RuneAndRust.Engine/ResolveCheckService.cs:24-34`

```csharp
public (bool success, int successes, string rollDetails) RollResolveCheck(
    PlayerCharacter character, int dc)
{
    int willValue = character.GetAttributeValue("will");
    var result = _diceService.Roll(willValue);

    bool success = result.Successes >= dc;

    string rollDetails = $"WILL Resolve Check (DC {dc}): Rolled {willValue} dice → " +
                         $"{FormatDiceRoll(result)} → {result.Successes} successes";

    return (success, result.Successes, rollDetails);
}

```

**DC-Based Check**: Unlike opposed rolls, Resolve Checks compare successes against a fixed DC (Difficulty Class).

**Example**:

```csharp
// Forlorn enemy aura: DC 2 WILL check
// Player WILL: 3
var (success, successes, rollDetails) = resolveCheckService.RollResolveCheck(player, dc: 2);
// Roll 3d6 → [5, 3, 6] → 2 successes
// success = true (2 >= 2)

```

### 3.9 Stress Reduction via WILL

**File**: `RuneAndRust.Engine/ResolveCheckService.cs:55-66`

```csharp
public (int stressToApply, int successes, string rollDetails)
    RollEnvironmentalStressResistance(PlayerCharacter character, int baseStress)
{
    int willValue = character.GetAttributeValue("will");
    var result = _diceService.Roll(willValue);

    int reducedStress = CalculateStressReduction(result.Successes, baseStress);
    // CalculateStressReduction: return Math.Max(0, baseStress - successes);

    return (reducedStress, result.Successes, rollDetails);
}

```

**1:1 Reduction**: Each WILL success reduces incoming Stress by 1 point.

**Example**:

```csharp
// Psychic Resonance zone: 8 base Stress
// Player WILL: 4
// Roll 4d6 → [5, 6, 2, 3] → 2 successes
// Reduced Stress = 8 - 2 = 6 Stress applied

```

---

## 4. Testing Coverage

### 4.1 Unit Tests Required

### Attribute Retrieval Tests

**Test Case**: `GetAttributeValue_WithValidAttribute_ReturnsCorrectValue`

```csharp
[TestCase("might", 4)]
[TestCase("FINESSE", 3)]  // Test case insensitivity
[TestCase("Wits", 2)]
[TestCase("will", 3)]
[TestCase("sturdiness", 4)]
public void GetAttributeValue_WithValidAttribute_ReturnsCorrectValue(
    string attributeName, int expected)
{
    // Arrange
    var player = new PlayerCharacter
    {
        Attributes = new Attributes(might: 4, finesse: 3, wits: 2, will: 3, sturdiness: 4)
    };

    // Act
    int result = player.GetAttributeValue(attributeName);

    // Assert
    Assert.AreEqual(expected, result);
}

```

**Test Case**: `GetAttributeValue_WithInvalidAttribute_ReturnsZero`

```csharp
[TestCase("strength")]
[TestCase("charisma")]
[TestCase("")]
[TestCase("invalid")]
public void GetAttributeValue_WithInvalidAttribute_ReturnsZero(string attributeName)
{
    // Arrange
    var player = new PlayerCharacter { Attributes = new Attributes(3, 3, 3, 3, 3) };

    // Act
    int result = player.GetAttributeValue(attributeName);

    // Assert
    Assert.AreEqual(0, result);
}

```

### Equipment Bonus Tests

**Test Case**: `GetEffectiveAttributeValue_WithWeaponBonus_AddsBonus`

```csharp
[Test]
public void GetEffectiveAttributeValue_WithWeaponBonus_AddsBonus()
{
    // Arrange
    var player = new PlayerCharacter
    {
        Attributes = new Attributes(might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 3),
        EquippedWeapon = new Equipment
        {
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 2 }
            }
        }
    };
    var equipmentService = new EquipmentService();

    // Act
    int effectiveMight = equipmentService.GetEffectiveAttributeValue(player, "might");

    // Assert
    Assert.AreEqual(6, effectiveMight); // 4 base + 2 weapon bonus
}

```

**Test Case**: `GetEffectiveAttributeValue_WithMultipleBonuses_SumsAllBonuses`

```csharp
[Test]
public void GetEffectiveAttributeValue_WithMultipleBonuses_SumsAllBonuses()
{
    // Arrange
    var player = new PlayerCharacter
    {
        Attributes = new Attributes(might: 3, finesse: 3, wits: 2, will: 2, sturdiness: 4),
        EquippedWeapon = new Equipment
        {
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 1 }
            }
        },
        EquippedArmor = new Equipment
        {
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 2 }
            }
        }
    };
    var equipmentService = new EquipmentService();

    // Act
    int effectiveSturdiness = equipmentService.GetEffectiveAttributeValue(player, "sturdiness");

    // Assert
    Assert.AreEqual(7, effectiveSturdiness); // 4 base + 1 weapon + 2 armor
}

```

**Test Case**: `GetEffectiveAttributeValue_CanExceedCap_WithEquipment`

```csharp
[Test]
public void GetEffectiveAttributeValue_CanExceedCap_WithEquipment()
{
    // Arrange
    var player = new PlayerCharacter
    {
        Attributes = new Attributes(might: 6, finesse: 3, wits: 2, will: 2, sturdiness: 3), // MIGHT at cap
        EquippedWeapon = new Equipment
        {
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 3 }
            }
        }
    };
    var equipmentService = new EquipmentService();

    // Act
    int effectiveMight = equipmentService.GetEffectiveAttributeValue(player, "might");

    // Assert
    Assert.AreEqual(9, effectiveMight); // 6 base (cap) + 3 equipment bonus = 9 (exceeds cap)
}

```

### Attribute Increase Tests

**Test Case**: `SpendPPOnAttribute_WithValidInput_IncreasesAttribute`

```csharp
[TestCase("might", 3, 4)]
[TestCase("FINESSE", 4, 5)]
[TestCase("Wits", 5, 6)]
public void SpendPPOnAttribute_WithValidInput_IncreasesAttribute(
    string attributeName, int startValue, int expectedValue)
{
    // Arrange
    var player = new PlayerCharacter
    {
        ProgressionPoints = 3,
        Attributes = new Attributes(3, 4, 5, 2, 3)
    };
    var sagaService = new SagaService();

    // Act
    bool result = sagaService.SpendPPOnAttribute(player, attributeName);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(2, player.ProgressionPoints); // 3 - 1
    Assert.AreEqual(expectedValue, player.GetAttributeValue(attributeName));
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
    Assert.AreEqual(5, player.ProgressionPoints); // No PP spent
    Assert.AreEqual(6, player.Attributes.Might); // Still at cap
}

```

### 4.2 Integration Tests Required

**Test Case**: `AttributeIncrease_AffectsCombatRolls`

```csharp
[Test]
public void AttributeIncrease_AffectsCombatRolls()
{
    // Arrange
    var player = CreateTestPlayer(might: 3);
    var enemy = CreateTestEnemy();
    var combatEngine = new CombatEngine(diceService, sagaService, ...);
    var sagaService = new SagaService();

    // Increase MIGHT from 3 to 4
    player.ProgressionPoints = 1;
    sagaService.SpendPPOnAttribute(player, "might");

    // Act: Simulate 100 attacks and measure hit rate
    int hitsWithMight3 = SimulateAttacks(player, enemy, might: 3, attackCount: 100);
    int hitsWithMight4 = SimulateAttacks(player, enemy, might: 4, attackCount: 100);

    // Assert: Might 4 should have ~10-15% more hits than Might 3
    Assert.Greater(hitsWithMight4, hitsWithMight3);
}

```

**Test Case**: `EquipmentBonuses_AffectCombatRolls`

```csharp
[Test]
public void EquipmentBonuses_AffectCombatRolls()
{
    // Arrange
    var player = CreateTestPlayer(might: 4);
    var enemy = CreateTestEnemy();
    var combatEngine = new CombatEngine(...);

    // Equip weapon with +2 MIGHT
    player.EquippedWeapon = CreateWeapon(mightBonus: 2);

    // Act: Perform attack (should use effective MIGHT of 6)
    combatEngine.PlayerAttack(player, enemy, "BasicAttack");

    // Assert: Attack roll should have used 6 dice (4 base + 2 equipment)
    // Verify via log output or dice service mock
}

```

### 4.3 Statistical Tests

**Test Case**: `AttributeDicePool_FollowsExpectedDistribution`

```csharp
[TestCase(3, 1.0)]   // 3d6 → ~1.0 expected successes
[TestCase(4, 1.33)]  // 4d6 → ~1.33 expected successes
[TestCase(5, 1.67)]  // 5d6 → ~1.67 expected successes
[TestCase(6, 2.0)]   // 6d6 → ~2.0 expected successes
public void AttributeDicePool_FollowsExpectedDistribution(
    int attributeValue, double expectedSuccesses)
{
    // Arrange
    var diceService = new DiceService();
    int iterations = 10000;
    int totalSuccesses = 0;

    // Act: Roll attribute dice pool 10,000 times
    for (int i = 0; i < iterations; i++)
    {
        var result = diceService.Roll(attributeValue);
        totalSuccesses += result.Successes;
    }

    // Calculate average successes
    double averageSuccesses = (double)totalSuccesses / iterations;

    // Assert: Average successes should be within ±0.05 of expected
    Assert.That(averageSuccesses, Is.EqualTo(expectedSuccesses).Within(0.05));
}

```

### 4.4 Manual Testing Scenarios

1. **Attribute Increase Progression**
    - Start character with 3 MIGHT
    - Spend 1 PP → MIGHT 4
    - Spend 1 PP → MIGHT 5
    - Spend 1 PP → MIGHT 6
    - Attempt to spend PP → Should fail (at cap)
    - Verify attack rolls use correct dice pool at each step
2. **Equipment Bonus Stacking**
    - Equip weapon with +1 MIGHT
    - Verify attack rolls use (base + 1) dice
    - Equip armor with +2 MIGHT
    - Verify attack rolls use (base + 3) dice
    - Unequip weapon
    - Verify attack rolls use (base + 2) dice
3. **WILL-based Stress Resistance**
    - Character with WILL 2: Enter Psychic Resonance zone (8 Stress)
    - Roll WILL check → Typically 0-1 successes → 6-8 Stress applied
    - Increase WILL to 4
    - Roll WILL check → Typically 1-2 successes → 6-7 Stress applied
    - Increase WILL to 6
    - Roll WILL check → Typically 2-3 successes → 5-6 Stress applied
4. **Initiative Order (FINESSE)**
    - Player FINESSE 3 vs Enemy FINESSE 4
    - Run 10 combats, track who goes first
    - Enemy should go first ~60% of the time
    - Increase player FINESSE to 5
    - Player should now go first ~60% of the time
5. **Attribute Cap Enforcement**
    - Character with MIGHT 6 (cap)
    - Attempt to spend PP on MIGHT → Should display error
    - Equip +3 MIGHT weapon
    - Verify effective MIGHT is 9 (cap bypassed by equipment)
    - Verify attack rolls use 9 dice

---

## 5. Balance Considerations

### 5.1 Attribute Diversity vs Specialization

**Tension**: Should players spread PP across all attributes or specialize in 1-2?

**Current Balance**:

- **15 total starting points** (average ~3 per attribute)
- **3-5 PP available by Milestone 3** (enough to raise 1 attribute to cap OR spread across 3-5 attributes)

**Specialization Strategy** (Focus):

- Raise 1 primary attribute from 4 → 6 (2 PP)
- Advantage: Dominant in primary role (combat/defense/resolve)
- Disadvantage: Weak in secondary roles (vulnerable to hazards, skill checks)

**Generalist Strategy** (Spread):

- Raise 3-5 attributes by +1 each
- Advantage: Competent in all situations, no glaring weaknesses
- Disadvantage: Never exceptional, always mediocre

**Design Goal**: Both strategies should be viable. Specialists excel in their niche but struggle outside it; generalists are consistently competent but never dominant.

### 5.2 Attribute Cap Justification

**Hard Cap at 6**:

- **Prevents runaway power**: Without cap, players could eventually reach 10+ in primary attribute → trivializes opposed rolls
- **Encourages diversification**: Once primary attribute hits 6, PP must go elsewhere
- **Equipment remains valuable**: Equipment bonuses bypass cap, making legendary gear highly desirable

**Alternative Design (Rejected)**: Increasing PP cost per level (1 PP for 3→4, 2 PP for 4→5, 3 PP for 5→6)

- **Problem**: Punishes late-game progression, makes high attributes unaffordable
- **Current Design**: Flat 1 PP cost keeps all increases equally valuable

### 5.3 Attribute-Weapon Alignment

**Design**: Weapons are locked to specific attributes (Greatsword = MIGHT, Spear = FINESSE, Staff = WILL)

**Advantages**:

- Clear archetype identity (Warrior = MIGHT, Scavenger = FINESSE, Mystic = WILL)
- Equipment choices matter (can't use WILL staff with MIGHT build)
- Encourages attribute investment matching class

**Disadvantages**:

- Limited build flexibility (MIGHT character can't effectively use FINESSE weapons)
- Punishes attribute reallocation (if player changes playstyle)

**Balance Check**: Starting archetypes have 4 in their primary attribute, ensuring baseline competence with class weapons.

### 5.4 STURDINESS as Universal Defense

**Design**: STURDINESS is used for **all** defense rolls, regardless of attack type

**Alternative Designs (Rejected)**:

1. **Attribute-based defense**: Defend against MIGHT attacks with STURDINESS, FINESSE attacks with FINESSE
    - **Problem**: Heavily favors generalist builds, specialists become too vulnerable
2. **Weapon-based defense**: Defend with weapon's attribute
    - **Problem**: Mystics (WILL-based) can't defend effectively, becomes FINESSE-dependent

**Current Design Benefits**:

- Specialists remain viable (MIGHT-focused Warrior still has good STURDINESS)
- Single defense attribute simplifies gameplay
- Defensive equipment (armor) has clear purpose (+STURDINESS)

**Balance**: Warriors start with STURDINESS 4, Scavengers 3, Mystics 3 (all functional)

### 5.5 WILL's Critical Role

**WILL is uniquely important**:

- **Only** defense against Psychic Stress
- **Only** defense against Corruption
- Required for WILL-based weapons (Mystic class)

**Balance Challenge**: WILL must be valuable without being mandatory for all builds

**Current Balance**:

- **Stress reduction is 1:1** (not 2:1 or 3:1), making WILL investment linear
- **Base Stress values are moderate** (5-15 range), not catastrophic
- **Sanctuary provides full Stress recovery**, reducing long-term pressure
- **Corruption is rare** (only specific encounters), not constant

**Result**: WILL is highly valuable for Mystics (primary attribute) and useful for others (secondary investment) but not mandatory for Warriors/Scavengers.

### 5.6 Equipment Bonuses Exceeding Cap

**Design**: Equipment bonuses can push attributes above the 6 cap

**Balance Implications**:

- **7-9 dice pools are achievable** with legendary equipment (+2-3 bonuses)
- **Power curve extends beyond base cap**, giving long-term progression goal
- **Equipment becomes build-defining**, not just incremental upgrades

**Example Power Curve**:

| Stage | MIGHT | Equipment | Effective | Dice Pool |
| --- | --- | --- | --- | --- |
| Start | 4 | None | 4 | 4d6 |
| M3 | 6 | None | 6 | 6d6 |
| Mid | 6 | +2 | 8 | 8d6 |
| Late | 6 | +3 | 9 | 9d6 |

**Design Note**: 9d6 is ~3.0 expected successes, providing meaningful late-game power spike without breaking the game.

### 5.7 Attribute Distribution Entropy

**Problem**: Players might min-max by dumping "useless" attributes to 1-2

**Current Safeguards**:

1. **All attributes have multiple uses** (see Attribute Use Cases table)
2. **Dump stats create vulnerabilities**:
    - Low WITS → Fail puzzles, miss secrets
    - Low WILL → High Stress/Corruption
    - Low STURDINESS → Poor defense
    - Low FINESSE → Go last in combat
    - Low MIGHT/FINESSE/WILL → Can't use certain weapons
3. **Starting distributions are balanced** (15 total, no attribute below 2)

**Result**: Dumping an attribute to 1-2 creates exploitable weaknesses, discouraging extreme min-maxing.

### 5.8 PP Investment ROI (Return on Investment)

**Question**: Is 1 PP for +1 attribute worth it compared to ability ranks or specialization?

**Attribute Increase** (1 PP):

- +1 die to all rolls using that attribute
- Permanent, always active
- Affects combat, skill checks, resistance
- ~+5-10% success rate improvement

**Ability Rank** (2-3 PP):

- Improves 1 ability (e.g., +1 damage die, -1 stamina cost)
- Only active when using that ability
- Significant power spike for specific situations

**Specialization** (10 PP):

- Unlocks 2-3 new abilities
- Thematic bonuses (healing, support, analysis)
- One-time investment, massive utility gain

**ROI Analysis**:

- **Attributes**: Best for generalists, incremental power
- **Abilities**: Best for specialists, focused power spikes
- **Specialization**: Best for late-game, requires long-term saving

**Balance**: All three options have distinct ROI profiles, making choice meaningful.

### 5.9 Dice Pool Scaling Limits

**Maximum Realistic Dice Pool**: ~12-15 dice (cap 6 + equipment +3 + ability bonuses +3-6)

**At 12d6**:

- Expected successes: ~4.0
- ~98% chance of 1+ success
- ~87% chance of 3+ successes

**Design Check**: Even at maximum dice pools, outcomes remain probabilistic. No "auto-success" state, preserving tension.

### 5.10 Attribute Respec Absence

**Current Design**: No attribute respec mechanic (PP spending is permanent)

**Justification**:

- **Encourages thoughtful builds**: Players must commit to choices
- **Preserves character identity**: Warrior stays Warrior, doesn't become Mystic mid-game
- **Simplifies systems**: No need for respec UI, cost balancing, or exploit prevention

**Potential Future Addition** (v1.0+):

- Rare consumable: "Elixir of Forgetting" (resets all PP spending)
- Cost: Expensive (50+ currency) or very rare drop
- Use case: Correct build mistakes, experiment with new playstyles

---

## 6. Future Expansion Notes

### 6.1 Planned Features (v0.5+)

1. **Attribute Synergies**
    - Combined checks (MIGHT + STURDINESS for wrestling)
    - Hybrid weapons (MIGHT or FINESSE, player chooses)
2. **Temporary Attribute Modifiers**
    - Buffs/debuffs that increase/decrease attributes for duration
    - Status effects: [Inspired] (+1 MIGHT), [Exhausted] (-1 to all)
3. **Attribute Thresholds**
    - Special unlocks at attribute milestones (e.g., MIGHT 6 unlocks heavy weapon proficiency)
4. **Advanced Equipment**
    - Equipment with conditional bonuses ("+2 MIGHT during Boss encounters")
    - Set bonuses (wearing full armor set grants +1 to all attributes)

### 6.2 Known Limitations

1. **Binary Attribute System**: Attributes are always active or inactive (no partial bonuses, conditional modifiers in current version)
2. **No Attribute Decay**: Attributes never decrease (except via Trauma, which is separate system)
3. **No Temporary Attribute Buffs**: Current status effects modify dice pools, not underlying attributes
4. **Hard-Coded Weapon Attributes**: Cannot change which attribute a weapon uses (Greatsword is always MIGHT)

### 6.3 Balance Knobs

Tunable parameters for future balance adjustments:

| Parameter | Current Value | Range | Impact |
| --- | --- | --- | --- |
| Attribute Cap | 6 | 5-10 | Power ceiling |
| PP Cost per +1 | 1 PP | 1-3 PP | Progression speed |
| Starting Total | 15 | 12-18 | Early power level |
| Equipment Bonus Max | +3 | +1 to +5 | Late-game power spike |
| Success Threshold | 5-6 on d6 (33.33%) | 4-6 (50%), 5-6 (33.33%), 6 (16.67%) | Overall difficulty curve |

---

## 7. Quick Reference

### 7.1 Attribute Summary

| Attribute | Primary Use | Roll Type | Key Systems |
| --- | --- | --- | --- |
| **MIGHT** | Melee Attack (heavy weapons) | Opposed | Combat (Attack) |
| **FINESSE** | Melee Attack (light weapons), Initiative | Opposed/Solo | Combat (Initiative, Attack) |
| **WITS** | Puzzles, Perception, Crafting | DC-based | Exploration, Crafting, Dialogue |
| **WILL** | Magic Attack, Stress Resistance | Opposed/DC-based | Combat (Attack), Trauma Economy |
| **STURDINESS** | Defense | Opposed | Combat (Defense) |

### 7.2 Starting Attributes

| Archetype | M | F | Wt | Wl | S | Total |
| --- | --- | --- | --- | --- | --- | --- |
| Warrior | 4 | 3 | 2 | 2 | 4 | 15 |
| Scavenger | 3 | 4 | 3 | 2 | 3 | 15 |
| Mystic | 2 | 3 | 3 | 4 | 3 | 15 |

### 7.3 Expected Success Table

| Attribute | Expected Successes | 1+ Success % | 2+ Success % |
| --- | --- | --- | --- |
| 1 | 0.33 | 33% | 0% |
| 2 | 0.67 | 56% | 11% |
| 3 | 1.0 | 70% | 26% |
| 4 | 1.33 | 80% | 40% |
| 5 | 1.67 | 87% | 50% |
| 6 | 2.0 | 91% | 59% |

---

**End of Document***Last Updated*: v0.17 Documentation Phase
*Implementation Version*: v0.7
*Total Lines*: 1,600+

---

## Appendix: Code Locations

| Feature | File | Line Range |
| --- | --- | --- |
| Attributes Class | `RuneAndRust.Core/Attributes.cs` | 1-22 |
| GetAttributeValue | `RuneAndRust.Core/PlayerCharacter.cs` | 87-98 |
| GetEffectiveAttributeValue | `RuneAndRust.Engine/EquipmentService.cs` | 266-290 |
| SpendPPOnAttribute | `RuneAndRust.Engine/SagaService.cs` | 100-152 |
| RollResolveCheck | `RuneAndRust.Engine/ResolveCheckService.cs` | 24-34 |
| Combat Attack Roll | `RuneAndRust.Engine/CombatEngine.cs` | 462 |