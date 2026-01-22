# Resource Pools (HP, Stamina, AP)

Parent item: System Documentation (System%20Documentation%202ba55eb312da801a9aa4f30d6e439959.md)

**Version**: v0.17 (Based on v0.7 implementation)
**Status**: Core System (Tier 1)
**Dependencies**: Combat Engine, Equipment Service, Saga Service, Consumables
**Integration Points**: Damage Calculation, Ability Usage, Milestone System, Equipment System

---

## Table of Contents

1. [Functional Overview](Resource%20Pools%20(HP,%20Stamina,%20AP)%202ba55eb312da8081ba73de96323af8dc.md)
2. [Statistical Reference](#2-statistical reference)
3. [Technical Implementation](Resource%20Pools%20(HP,%20Stamina,%20AP)%202ba55eb312da8081ba73de96323af8dc.md)
4. [Testing Coverage](Resource%20Pools%20(HP,%20Stamina,%20AP)%202ba55eb312da8081ba73de96323af8dc.md)
5. [Balance Considerations](Resource%20Pools%20(HP,%20Stamina,%20AP)%202ba55eb312da8081ba73de96323af8dc.md)

---

## 1. Functional Overview

### 1.1 Purpose

**Resource Pools** are the three expendable resources that govern character survival and action economy in Rune & Rust:

1. **Hit Points (HP)**: Life total; reaching 0 HP causes death
2. **Stamina**: Energy for using abilities; recovers over time
3. **Action Points (AP)**: Movement/exploration currency (v0.9+)

These resources create strategic tension between aggression (spending Stamina for powerful abilities) and sustainability (conserving resources for long-term survival).

### 1.2 Hit Points (HP)

### Purpose

HP represents physical health and vitality. When HP reaches 0, the character dies and the run ends.

### Core Mechanics

- **Maximum HP (MaxHP)**: Total HP capacity
- **Current HP**: Current health remaining (0 to MaxHP)
- **Death Threshold**: 0 HP = instant death (no "unconscious" state)
- **Healing**: Consumables, abilities, rest, and Milestones restore HP
- **Over-Healing**: Cannot exceed MaxHP (excess healing is wasted)

### HP Sources

1. **Base HP by Class** (starting values):
    - Warrior: **50 HP** (tank, frontline)
    - Scavenger: **40 HP** (balanced)
    - Mystic: **30 HP** (glass cannon)
    - Adept: **35 HP** (skill specialist)
2. **Milestone Bonuses**: +10 MaxHP per Milestone
    - Milestone 0 → 1: +10 MaxHP
    - Milestone 1 → 2: +10 MaxHP
    - Milestone 2 → 3: +10 MaxHP
    - **Total by Milestone 3**: +30 MaxHP
3. **Equipment Bonuses**: Armor can grant HP bonuses
    - Light Armor: +5 to +15 HP
    - Medium Armor: +10 to +20 HP
    - Heavy Armor: +15 to +30 HP
4. **Passive Abilities**: Warrior's Vigor grants +10% MaxHP
    - Applied multiplicatively after base + milestones
    - Example: (50 base + 30 milestones) × 1.10 = 88 HP

### HP Loss

- **Combat Damage**: Primary source of HP loss
- **Environmental Hazards**: Spike traps, poison, fire
- **Heretical Abilities**: Blood Sacrifice costs HP (20 HP per use)
- **Corruption Effects**: Breaking Point events can inflict HP damage

### HP Recovery

- **Consumables**: Healing Salve (+2d6 HP), Medicinal Tonic (+3d6 HP)
- **Abilities**: Survivalist (Scavenger, 2d6 HP), Mend Wound (Bone-Setter)
- **Rest**: Full HP recovery at Sanctuary (not during exploration)
- **Milestones**: Full HP restoration when reaching a Milestone

---

### 1.3 Stamina

### Purpose

Stamina is the resource spent to activate abilities. Managing Stamina is crucial for sustained combat effectiveness.

### Core Mechanics

- **Maximum Stamina (MaxStamina)**: Total Stamina capacity
- **Current Stamina**: Current Stamina remaining (0 to MaxStamina)
- **Zero Stamina**: At 0 Stamina, cannot use abilities (but can still use basic Attack/Defend actions)
- **Regeneration**: Stamina does **not** regenerate during combat in v0.1 (future feature)
- **Over-Recovery**: Cannot exceed MaxStamina (excess recovery is wasted)

### Stamina Sources

1. **Base Stamina by Class** (starting values):
    - Warrior: **30 Stamina** (few abilities, high HP)
    - Scavenger: **40 Stamina** (balanced)
    - Mystic: **50 Stamina** (many abilities, low HP)
    - Adept: **40 Stamina** (skill specialist)
2. **Milestone Bonuses**: +5 MaxStamina per Milestone
    - Milestone 0 → 1: +5 MaxStamina
    - Milestone 1 → 2: +5 MaxStamina
    - Milestone 2 → 3: +5 MaxStamina
    - **Total by Milestone 3**: +15 MaxStamina
3. **Equipment Bonuses**: Some equipment grants Stamina bonuses (rare)
    - Example: "Endurance Belt" (+10 MaxStamina)

### Stamina Costs

Abilities have fixed Stamina costs ranging from **5 to 50 Stamina**:

| Ability Type | Typical Cost | Examples |
| --- | --- | --- |
| Basic Attack | 10-15 Stamina | Power Strike (10), Improvised Strike (10) |
| Defense | 10-15 Stamina | Shield Wall (12), Quick Dodge (10), Aetheric Shield (10) |
| Utility | 5-25 Stamina | Exploit Weakness (5), Analyze (20), Rousing Verse (15) |
| Control | 12-20 Stamina | Disrupt (12), Song of Silence (20) |
| AOE/Ultimate | 25-50 Stamina | Chain Lightning (15), Desperate Gambit (40), Corruption Nova (50) |

**Design Principle**: More powerful effects cost proportionally more Stamina, creating resource management tension.

### Stamina Recovery

- **Consumables**: Stamina Tonic (+2d6 Stamina), Rousing Verse ability (+3d6 Stamina)
- **Rest**: Full Stamina restoration at Sanctuary
- **Milestones**: Full Stamina restoration when reaching a Milestone
- **Future (v0.5+)**: Per-turn regeneration (+3-5 Stamina per turn)

---

### 1.4 Action Points (AP)

### Purpose

AP is the currency for **movement and exploration** (v0.9+). Moving between rooms costs AP.

### Core Mechanics

- **Maximum AP**: 10 (fixed for all classes)
- **Current AP**: Remaining movement budget
- **Movement Cost**: Typically 1-3 AP per room traversal
- **Zero AP**: Cannot move to new rooms (must rest or use consumables)
- **Regeneration**: AP regenerates at Sanctuary

### AP Usage (v0.9+ - Not Fully Implemented)

- **Room Traversal**: 1-3 AP depending on room type
- **Backtracking**: May cost less AP (0-1 AP)
- **Fast Travel**: May cost reduced AP via shortcuts

### AP Recovery

- **Rest**: Full AP restoration at Sanctuary
- **Consumables**: Stimulant items (future feature)

**Design Note**: AP system is minimal in v0.7; full implementation planned for v0.9+.

---

### 1.5 Gameplay Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Character Creation: Set Base MaxHP, MaxStamina, AP      │
│    - Warrior: 50 HP, 30 Stamina, 10 AP                     │
│    - Scavenger: 40 HP, 40 Stamina, 10 AP                   │
│    - Mystic: 30 HP, 50 Stamina, 10 AP                      │
│    ↓                                                         │
│ 2. Combat Turn: Player Uses Ability                         │
│    - Check if Stamina >= Ability.StaminaCost               │
│    - IF YES: Deduct Stamina, execute ability               │
│    - IF NO: Cannot use ability, must use basic action      │
│    ↓                                                         │
│ 3. Enemy Turn: Enemy Attacks                                │
│    - Calculate damage (see Damage Calculation doc)         │
│    - Deduct HP from player.HP                              │
│    - IF HP <= 0: Player dies, game over                    │
│    ↓                                                         │
│ 4. End of Combat: Check Resources                           │
│    - Remaining HP and Stamina carried forward              │
│    - Use consumables to recover if needed                   │
│    ↓                                                         │
│ 5. Reach Milestone: Full Restoration                        │
│    - HP = MaxHP                                             │
│    - Stamina = MaxStamina                                   │
│    - AP = 10 (if applicable)                                │
│    - MaxHP += 10, MaxStamina += 5                           │
│    ↓                                                         │
│ 6. Sanctuary Rest: Full Recovery                            │
│    - HP = MaxHP, Stamina = MaxStamina, AP = 10             │
│    - Psychic Stress reduced to 0                            │
└─────────────────────────────────────────────────────────────┘

```

---

## 2. Statistical Reference

### 2.1 Base Resource Pools by Class

| Class | Base HP | Base Stamina | AP | HP per Milestone | Stamina per Milestone |
| --- | --- | --- | --- | --- | --- |
| Warrior | 50 | 30 | 10 | +10 (+20% base) | +5 (+16.7% base) |
| Scavenger | 40 | 40 | 10 | +10 (+25% base) | +5 (+12.5% base) |
| Mystic | 30 | 50 | 10 | +10 (+33.3% base) | +5 (+10% base) |
| Adept | 35 | 40 | 10 | +10 (+28.6% base) | +5 (+12.5% base) |

**Design Insight**: Milestone bonuses are **flat (+10/+5)** not percentage-based, causing Mystic's HP to grow faster proportionally while Warrior's remains dominant in absolute terms.

### 2.2 Resource Progression Table (Warrior Example)

| Milestone | Base HP | Equipment HP | Warrior's Vigor (+10%) | Total MaxHP | Total MaxStamina |
| --- | --- | --- | --- | --- | --- |
| 0 | 50 | +0 | +5 | 55 | 30 |
| 1 | 60 | +10 | +7 | 77 | 35 |
| 2 | 70 | +20 | +9 | 99 | 40 |
| 3 | 80 | +30 | +11 | 121 | 45 |

**Calculation**: `MaxHP = (Base + (Milestone × 10) + EquipmentHP) × (1 + PassiveBonuses)`

**Example at Milestone 3**:

- Base: 50
- Milestone bonuses: +30 (3 × 10)
- Equipment (Heavy Armor): +30
- Subtotal: 110
- Warrior's Vigor (+10%): 110 × 1.10 = **121 HP**

### 2.3 Resource Progression Table (All Classes at Milestone 3)

| Class | Base HP | +Milestones | +Equipment | +Passive | **Total MaxHP** | **MaxStamina** |
| --- | --- | --- | --- | --- | --- | --- |
| Warrior | 50 | +30 | +30 | +11 (10%) | **121** | 45 |
| Scavenger | 40 | +30 | +20 | +0 | **90** | 55 |
| Mystic | 30 | +30 | +10 | +0 | **70** | 65 |
| Adept | 35 | +30 | +15 | +0 | **80** | 55 |

**Design**: Warriors remain tankiest (121 HP), Mystics remain glassiest (70 HP) but with highest Stamina (65).

### 2.4 Ability Stamina Cost Distribution

**Ability Sample** (55+ abilities):

| Stamina Cost Range | Count | Percentage | Examples |
| --- | --- | --- | --- |
| 0 (Passive) | 5 | 9% | Keen Eye, Warrior's Vigor |
| 5-10 | 15 | 27% | Exploit Weakness (5), Power Strike (10) |
| 11-20 | 25 | 45% | Shield Wall (12), Aetheric Bolt (8), Disrupt (12) |
| 21-30 | 8 | 15% | Survivalist (20), Psychic Lash (25), Blight Surge (30) |
| 31-50 | 7 | 13% | Desperate Gambit (40), Corruption Nova (50) |

**Key Insight**: Most abilities cost **11-20 Stamina**, allowing 2-4 uses per combat with base Stamina pools.

### 2.5 Stamina Efficiency Analysis

**Warrior (30 Base Stamina at M0)**:

- 3 Power Strikes (10 Stamina each) = 30 Stamina
- 2 Shield Walls (12 Stamina each) = 24 Stamina
- **Result**: 2-3 abilities per combat before exhaustion

**Mystic (50 Base Stamina at M0)**:

- 4 Aetheric Bolts (8 Stamina each) = 32 Stamina
- 3 Disrupts (12 Stamina each) = 36 Stamina
- **Result**: 3-5 abilities per combat before exhaustion

**Design**: Mystics can cast more often but die faster; Warriors cast less but survive longer.

### 2.6 HP Recovery Rates

| Recovery Method | Amount | Availability | Notes |
| --- | --- | --- | --- |
| **Healing Salve** | 2d6 (avg 7) | Consumable | Limited quantity |
| **Medicinal Tonic** | 3d6 (avg 10.5) | Consumable (rare) | Limited quantity |
| **Survivalist Ability** | 2d6 (avg 7) | 20 Stamina | Once per combat |
| **Mend Wound (Bone-Setter)** | 3d6 (avg 10.5) | 5 Stamina | Specialization required |
| **Rest at Sanctuary** | Full MaxHP | Free | Only at safe rooms |
| **Milestone** | Full MaxHP | Progression | Rare |

**Design**: In-combat healing is limited and costly, encouraging tactical play over brute-force attrition.

### 2.7 Effective Hit Points (EHP) by Class

**EHP Formula** (simplified):

```
EHP = HP / (1 - Damage_Reduction_%)

```

Assuming 20% average damage reduction from STURDINESS defense:

| Class | MaxHP (M3) | STURDINESS | Avg DR | **Effective HP** |
| --- | --- | --- | --- | --- |
| Warrior | 121 | 4-5 | 25% | **161 EHP** |
| Scavenger | 90 | 3-4 | 20% | **113 EHP** |
| Mystic | 70 | 2-3 | 15% | **82 EHP** |

**Key Insight**: Warriors have **~2× effective HP** of Mystics when accounting for defense.

### 2.8 Damage Thresholds (How Many Hits to Kill?)

Assuming average enemy damage of **15 HP per hit** (elite enemy at Milestone 2-3):

| Class | MaxHP (M3) | Hits to Kill (Raw) | Hits to Kill (w/ Defense) |
| --- | --- | --- | --- |
| Warrior | 121 | 8.1 hits | **~10-11 hits** |
| Scavenger | 90 | 6.0 hits | **~7-8 hits** |
| Mystic | 70 | 4.7 hits | **~5-6 hits** |

**Design**: Warriors can survive 2× as many hits as Mystics, creating clear survivability differentiation.

---

## 3. Technical Implementation

### 3.1 Data Model

**File**: `RuneAndRust.Core/PlayerCharacter.cs:24-30`

```csharp
// Resources
public int HP { get; set; }
public int MaxHP { get; set; }
public int Stamina { get; set; }
public int MaxStamina { get; set; }
public int AP { get; set; }

public bool IsAlive => HP > 0;

```

**Design**: Simple integer properties, no complex types. `IsAlive` is a computed property based on HP > 0.

### 3.2 Character Creation: Base HP/Stamina

**File**: `RuneAndRust.Engine/CharacterFactory.cs:49-98`

### Warrior Initialization

```csharp
private static void InitializeWarrior(PlayerCharacter character)
{
    var warriorArchetype = new WarriorArchetype();
    character.Archetype = warriorArchetype;
    character.Attributes = warriorArchetype.GetBaseAttributes();

    // Resources (base values before equipment)
    character.MaxHP = 50;
    character.HP = 50;
    character.MaxStamina = 30;
    character.Stamina = 30;
    character.AP = 10;

    // ... equipment setup ...

    // Recalculate stats to apply Warrior's Vigor bonus
    var equipService = new EquipmentService();
    equipService.RecalculatePlayerStats(character);
}

```

### Scavenger Initialization

```csharp
private static void InitializeScavenger(PlayerCharacter character)
{
    character.Attributes = new Attributes(might: 3, finesse: 3, wits: 3, will: 2, sturdiness: 3);

    character.MaxHP = 40;
    character.HP = 40;
    character.MaxStamina = 40;
    character.Stamina = 40;
    character.AP = 10;

    // ... equipment and abilities ...
}

```

### Mystic Initialization

```csharp
private static void InitializeMystic(PlayerCharacter character)
{
    character.Attributes = new Attributes(might: 2, finesse: 2, wits: 3, will: 4, sturdiness: 2);

    character.MaxHP = 30;
    character.HP = 30;
    character.MaxStamina = 50;
    character.Stamina = 50;
    character.AP = 10;

    // ... equipment and abilities ...
}

```

**Design**: Each class has hard-coded base values. AP is always 10 for all classes.

### 3.3 Equipment Recalculation: MaxHP Calculation

**File**: `RuneAndRust.Engine/EquipmentService.cs:220-241`

```csharp
/// <summary>
/// Recalculate player stats based on equipped items
/// </summary>
public void RecalculatePlayerStats(PlayerCharacter player)
{
    // Store HP ratio to preserve proportional HP after recalc
    float hpRatio = player.MaxHP > 0 ? (float)player.HP / player.MaxHP : 1.0f;

    // Base MaxHP depends on class and level
    int baseMaxHP = GetBaseMaxHP(player);
    player.MaxHP = baseMaxHP;

    // v0.7.1: Apply Warrior's Vigor passive (+10% Max HP)
    if (player.Abilities.Any(a => a.Name == "Warrior's Vigor"))
    {
        player.MaxHP = (int)(player.MaxHP * 1.10f);
    }

    // Apply armor HP bonus
    if (player.EquippedArmor != null)
    {
        player.MaxHP += player.EquippedArmor.HPBonus;
    }

    // Restore HP ratio (but don't exceed new MaxHP)
    player.HP = Math.Min((int)(player.MaxHP * hpRatio), player.MaxHP);
}

```

**Key Design Decisions**:

1. **HP Ratio Preservation**: Changing equipment doesn't arbitrarily heal/damage player
2. **Order of Operations**:
    - Calculate base HP (class + milestones)
    - Apply percentage bonuses (Warrior's Vigor)
    - Apply flat bonuses (equipment HP)
3. **HP Clamping**: Final HP cannot exceed new MaxHP

### Base MaxHP Calculation

```csharp
private int GetBaseMaxHP(PlayerCharacter player)
{
    // Base HP from character creation (from CharacterFactory)
    int baseHP = player.Class switch
    {
        CharacterClass.Warrior => 50,
        CharacterClass.Scavenger => 40,
        CharacterClass.Mystic => 30,
        _ => 40  // Default for Adept and future classes
    };

    // Add +10 HP per milestone (from v0.2 progression system)
    baseHP += player.CurrentMilestone * 10;

    return baseHP;
}

```

**Formula**: `BaseHP = ClassBaseHP + (Milestone × 10)`

### 3.4 Milestone Rewards: Resource Restoration

**File**: `RuneAndRust.Engine/SagaService.cs:51-80`

```csharp
public void ReachMilestone(PlayerCharacter player)
{
    if (!CanReachMilestone(player))
    {
        throw new InvalidOperationException("Player cannot reach milestone yet.");
    }

    // Increase milestone
    player.CurrentMilestone++;

    // Update Legend threshold for next milestone
    player.LegendToNextMilestone = CalculateLegendToNextMilestone(player.CurrentMilestone);

    // Grant milestone rewards
    player.ProgressionPoints += 1;
    player.MaxHP += 10;         // Flat +10 MaxHP
    player.MaxStamina += 5;     // Flat +5 MaxStamina

    // Full heal on milestone
    player.HP = player.MaxHP;
    player.Stamina = player.MaxStamina;

    // Logging...
}

```

**Critical Feature**: Full restoration (HP = MaxHP, Stamina = MaxStamina) happens **after** increasing MaxHP/MaxStamina, so player gets full benefit immediately.

### 3.5 Combat: Stamina Deduction

**File**: `RuneAndRust.Engine/CombatEngine.cs:364-367`

```csharp
// Pay stamina cost
player.Stamina -= ability.StaminaCost;

combatState.AddLogEntry($"{player.Name} uses {ability.Name}!");

```

**Design**: Simple integer subtraction. Validation happens before this line (checking `player.Stamina >= ability.StaminaCost`).

### 3.6 Combat: HP Damage

**File**: `RuneAndRust.Engine/EnemyAI.cs:788-790`

```csharp
player.HP -= damage;
combatState.AddLogEntry($"{indent}{player.Name} takes {damage} damage! (HP: {Math.Max(0, player.HP)}/{player.MaxHP})");

```

**Design**: Direct HP subtraction. No clamping to 0 here (happens in display logic via `Math.Max(0, player.HP)`).

### 3.7 Death Check

**File**: `RuneAndRust.Core/PlayerCharacter.cs:85`

```csharp
public bool IsAlive => HP > 0;

```

**Design**: Computed property. HP = 0 means dead, HP = 1 means alive with 1 HP.

### 3.8 HP Recovery (Consumables)

**File**: `RuneAndRust.Engine/ConsumableDatabase.cs` (example)

```csharp
new Consumable
{
    Name = "Healing Salve",
    Description = "Restores 2d6 HP",
    Type = ConsumableType.Healing,
    HealAmount = 2,  // 2d6
    Effect = "Heals 2d6 HP (avg 7)",
    Rarity = ConsumableRarity.Common
}

```

**Usage in Code** (conceptual, actual implementation varies):

```csharp
public void UseConsumable(PlayerCharacter player, Consumable consumable)
{
    if (consumable.Type == ConsumableType.Healing)
    {
        int healAmount = _diceService.RollDamage(consumable.HealAmount);
        player.HP = Math.Min(player.HP + healAmount, player.MaxHP);
        // Cannot exceed MaxHP
    }
}

```

---

## 4. Testing Coverage

### 4.1 Unit Tests Required

### HP/Stamina Initialization Tests

**Test Case**: `CharacterFactory_CreateWarrior_SetsCorrectResources`

```csharp
[Test]
public void CharacterFactory_CreateWarrior_SetsCorrectResources()
{
    // Arrange & Act
    var warrior = CharacterFactory.CreateCharacter(CharacterClass.Warrior);

    // Assert
    Assert.That(warrior.MaxHP, Is.GreaterThanOrEqualTo(50)); // Base 50 + potential Warrior's Vigor
    Assert.AreEqual(50, warrior.HP);
    Assert.AreEqual(30, warrior.MaxStamina);
    Assert.AreEqual(30, warrior.Stamina);
    Assert.AreEqual(10, warrior.AP);
}

```

**Test Case**: `CharacterFactory_AllClasses_HaveCorrectBaseValues`

```csharp
[TestCase(CharacterClass.Warrior, 50, 30)]
[TestCase(CharacterClass.Scavenger, 40, 40)]
[TestCase(CharacterClass.Mystic, 30, 50)]
[TestCase(CharacterClass.Adept, 35, 40)]
public void CharacterFactory_AllClasses_HaveCorrectBaseValues(
    CharacterClass characterClass, int expectedHP, int expectedStamina)
{
    // Arrange & Act
    var character = CharacterFactory.CreateCharacter(characterClass);

    // Assert (HP may be higher due to equipment/passives)
    Assert.That(character.HP, Is.GreaterThanOrEqualTo(expectedHP));
    Assert.That(character.Stamina, Is.EqualTo(expectedStamina));
}

```

### Milestone Resource Tests

**Test Case**: `ReachMilestone_IncreasesMaxHPAndStamina`

```csharp
[Test]
public void ReachMilestone_IncreasesMaxHPAndStamina()
{
    // Arrange
    var player = CreateTestPlayer(currentMilestone: 0, currentLegend: 100);
    player.MaxHP = 50;
    player.MaxStamina = 30;
    player.HP = 25; // Damaged
    player.Stamina = 15; // Partially spent
    var sagaService = new SagaService();

    // Act
    sagaService.ReachMilestone(player);

    // Assert
    Assert.AreEqual(60, player.MaxHP);      // 50 + 10
    Assert.AreEqual(35, player.MaxStamina);  // 30 + 5
    Assert.AreEqual(60, player.HP);          // Full heal
    Assert.AreEqual(35, player.Stamina);     // Full heal
}

```

**Test Case**: `ReachMilestone_RestoresFullHP_EvenWhenDamaged`

```csharp
[Test]
public void ReachMilestone_RestoresFullHP_EvenWhenDamaged()
{
    // Arrange
    var player = CreateTestPlayer(currentLegend: 100, hp: 1); // Near death
    player.MaxHP = 50;
    player.MaxStamina = 30;
    var sagaService = new SagaService();

    // Act
    sagaService.ReachMilestone(player);

    // Assert
    Assert.AreEqual(60, player.HP); // Full heal to new max
    Assert.AreEqual(35, player.Stamina);
}

```

### Equipment HP Bonus Tests

**Test Case**: `RecalculatePlayerStats_WithArmorHPBonus_IncreasesMaxHP`

```csharp
[Test]
public void RecalculatePlayerStats_WithArmorHPBonus_IncreasesMaxHP()
{
    // Arrange
    var player = new PlayerCharacter
    {
        Class = CharacterClass.Warrior,
        CurrentMilestone = 0,
        MaxHP = 50,
        HP = 50,
        EquippedArmor = new Equipment
        {
            Name = "Iron Plate",
            HPBonus = 20
        }
    };
    var equipmentService = new EquipmentService();

    // Act
    equipmentService.RecalculatePlayerStats(player);

    // Assert
    Assert.AreEqual(70, player.MaxHP); // 50 base + 20 armor (assuming no Warrior's Vigor)
}

```

**Test Case**: `RecalculatePlayerStats_PreservesHPRatio`

```csharp
[Test]
public void RecalculatePlayerStats_PreservesHPRatio()
{
    // Arrange
    var player = new PlayerCharacter
    {
        Class = CharacterClass.Warrior,
        CurrentMilestone = 0,
        MaxHP = 50,
        HP = 25, // 50% HP
        EquippedArmor = new Equipment { HPBonus = 20 }
    };
    var equipmentService = new EquipmentService();

    // Act
    equipmentService.RecalculatePlayerStats(player);

    // Assert
    Assert.AreEqual(70, player.MaxHP);       // 50 + 20
    Assert.AreEqual(35, player.HP);          // 50% of 70 = 35
}

```

### Combat Resource Tests

**Test Case**: `UseAbility_DeductsStaminaCost`

```csharp
[Test]
public void UseAbility_DeductsStaminaCost()
{
    // Arrange
    var player = CreateTestPlayer(stamina: 30);
    var enemy = CreateTestEnemy();
    var combatState = new CombatState { Player = player, Enemies = new List<Enemy> { enemy } };
    var combatEngine = new CombatEngine(...);
    var ability = new Ability { Name = "Power Strike", StaminaCost = 10 };

    // Act
    combatEngine.PlayerAttack(player, enemy, ability);

    // Assert
    Assert.AreEqual(20, player.Stamina); // 30 - 10
}

```

**Test Case**: `TakeDamage_ReducesHP`

```csharp
[Test]
public void TakeDamage_ReducesHP()
{
    // Arrange
    var player = CreateTestPlayer(hp: 50);
    int damage = 15;

    // Act
    player.HP -= damage;

    // Assert
    Assert.AreEqual(35, player.HP); // 50 - 15
}

```

**Test Case**: `IsAlive_ReturnsFalse_WhenHPIsZero`

```csharp
[Test]
public void IsAlive_ReturnsFalse_WhenHPIsZero()
{
    // Arrange
    var player = CreateTestPlayer(hp: 0);

    // Act & Assert
    Assert.IsFalse(player.IsAlive);
}

```

**Test Case**: `IsAlive_ReturnsTrue_WhenHPIsOne`

```csharp
[Test]
public void IsAlive_ReturnsTrue_WhenHPIsOne()
{
    // Arrange
    var player = CreateTestPlayer(hp: 1);

    // Act & Assert
    Assert.IsTrue(player.IsAlive);
}

```

### Healing Tests

**Test Case**: `HealingConsumable_RestoresHP_CappedAtMaxHP`

```csharp
[Test]
public void HealingConsumable_RestoresHP_CappedAtMaxHP()
{
    // Arrange
    var player = CreateTestPlayer(hp: 45, maxHP: 50);
    int healAmount = 10;

    // Act
    player.HP = Math.Min(player.HP + healAmount, player.MaxHP);

    // Assert
    Assert.AreEqual(50, player.HP); // Capped at MaxHP (45 + 10 = 55 → 50)
}

```

**Test Case**: `HealingConsumable_DoesNotExceedMaxHP`

```csharp
[Test]
public void HealingConsumable_DoesNotExceedMaxHP()
{
    // Arrange
    var player = CreateTestPlayer(hp: 50, maxHP: 50); // Full HP
    int healAmount = 10;

    // Act
    player.HP = Math.Min(player.HP + healAmount, player.MaxHP);

    // Assert
    Assert.AreEqual(50, player.HP); // No change, already at max
}

```

### 4.2 Integration Tests Required

**Test Case**: `FullCombat_ResourceManagement_AllClasses`

```csharp
[TestCase(CharacterClass.Warrior)]
[TestCase(CharacterClass.Scavenger)]
[TestCase(CharacterClass.Mystic)]
public void FullCombat_ResourceManagement_AllClasses(CharacterClass characterClass)
{
    // Arrange
    var player = CharacterFactory.CreateCharacter(characterClass);
    var enemy = EnemyFactory.CreateEnemy(EnemyType.Scourge_Ambusher);
    var combatEngine = new CombatEngine(...);

    // Act: Simulate full combat
    int initialHP = player.HP;
    int initialStamina = player.Stamina;

    // Use abilities until Stamina is depleted
    while (player.Stamina >= 10 && enemy.IsAlive)
    {
        var ability = player.Abilities.First(a => a.StaminaCost <= player.Stamina);
        combatEngine.PlayerAttack(player, enemy, ability);
        if (enemy.IsAlive)
        {
            combatEngine.EnemyTurn(...);
        }
    }

    // Assert
    Assert.Less(player.Stamina, initialStamina); // Stamina was spent
    Assert.Less(player.HP, initialHP);           // HP was lost
    Assert.That(player.HP, Is.GreaterThan(0));   // Player survived
}

```

**Test Case**: `MilestoneProgression_ResourcesIncreaseCorrectly`

```csharp
[Test]
public void MilestoneProgression_ResourcesIncreaseCorrectly()
{
    // Arrange
    var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior);
    var sagaService = new SagaService();
    int baseHP = player.MaxHP;
    int baseStamina = player.MaxStamina;

    // Act: Reach 3 milestones
    for (int i = 1; i <= 3; i++)
    {
        player.CurrentLegend = 1000; // Ensure enough Legend
        sagaService.ReachMilestone(player);
    }

    // Assert
    Assert.AreEqual(baseHP + 30, player.MaxHP);           // +10 × 3
    Assert.AreEqual(baseStamina + 15, player.MaxStamina); // +5 × 3
    Assert.AreEqual(player.MaxHP, player.HP);             // Full HP
    Assert.AreEqual(player.MaxStamina, player.Stamina);   // Full Stamina
}

```

### 4.3 Manual Testing Scenarios

1. **Resource Depletion Test**
    - Start combat with full Stamina
    - Use abilities until Stamina = 0
    - Verify: Cannot use abilities, must use basic Attack/Defend
    - Take damage until HP = 1
    - Verify: Character is still alive (IsAlive = true)
    - Take 1 more damage
    - Verify: Character dies (IsAlive = false), game over
2. **Milestone Healing Test**
    - Reduce HP to 1, Stamina to 0
    - Reach Milestone
    - Verify: HP = MaxHP, Stamina = MaxStamina
    - Verify: MaxHP increased by +10, MaxStamina increased by +5
3. **Equipment HP Bonus Test**
    - Character with 50 HP, 50% damaged (25 HP)
    - Equip armor with +20 HP bonus
    - Verify: MaxHP = 70, HP = 35 (50% ratio preserved)
    - Unequip armor
    - Verify: MaxHP = 50, HP = 25 (50% ratio preserved)
4. **Warrior's Vigor Test**
    - Create Warrior (should have Warrior's Vigor passive)
    - Verify: MaxHP = base × 1.10 (e.g., 50 → 55)
    - Reach Milestone 1
    - Verify: MaxHP = (base + 10) × 1.10 (e.g., 60 → 66)
5. **Healing Consumable Test**
    - Character with 30/50 HP
    - Use Healing Salve (2d6, avg 7 HP)
    - Verify: HP increases by heal amount, capped at MaxHP
    - Use consumable at full HP
    - Verify: HP remains at MaxHP (no over-healing)

---

## 5. Balance Considerations

### 5.1 HP Pools: Class Identity

**Design Goal**: HP pools should reflect class archetypes (tank, balanced, glass cannon)

**Current Balance**:

- **Warrior (50 HP)**: Survives 2× as long as Mystic
- **Scavenger (40 HP)**: Balanced between offense and defense
- **Mystic (30 HP)**: High risk, must end fights quickly

**Balance Check**: HP differential is **67%** (50 vs 30), creating clear distinction without making Mystics unplayable.

### 5.2 Stamina Pools: Resource Management

**Design Goal**: Stamina should be scarce enough to create tension but plentiful enough to use abilities

**Current Balance**:

- **Warrior (30 Stamina)**: 2-3 abilities per combat (conservative playstyle)
- **Scavenger (40 Stamina)**: 3-4 abilities per combat (balanced)
- **Mystic (50 Stamina)**: 4-6 abilities per combat (ability-focused)

**Balance Check**: Warriors must choose when to use abilities (resource scarcity); Mystics can use abilities more freely (resource abundance). **Trade-off**: Warriors have 67% more HP to compensate for 67% less Stamina.

### 5.3 Milestone Bonuses: Flat vs Percentage

**Current Design**: Flat bonuses (+10 HP, +5 Stamina)

**Pros**:

- Simple to understand and calculate
- Benefits low-HP classes proportionally more (Mystic gains +33.3% HP per Milestone vs Warrior's +20%)
- Reduces HP disparity over time

**Cons**:

- High-HP classes remain dominant in absolute terms
- No exponential growth for long campaigns

**Alternative Design (Rejected)**: Percentage bonuses (+20% HP, +15% Stamina)

- **Problem**: Widens HP gap (Warrior gains +10 HP, Mystic gains +6 HP per Milestone)
- **Result**: Mystics become even more fragile over time

**Conclusion**: Flat bonuses are better for class balance convergence.

### 5.4 Stamina Costs: Ability Balance

**Design Principle**: More powerful effects should cost proportionally more Stamina

**Balance Tiers**:

1. **Basic Abilities (10-15 Stamina)**: Bread-and-butter attacks, consistent use
2. **Utility/Defense (5-20 Stamina)**: Situational tools, moderate cost
3. **Ultimate Abilities (30-50 Stamina)**: Powerful effects, 1-2 uses per combat

**Balance Check**: At base Stamina (30-50), players can afford:

- Warrior: 2-3 basic abilities OR 1 ultimate
- Mystic: 4-6 basic abilities OR 2 ultimates

**Trade-off**: Warriors use fewer, more impactful abilities; Mystics use many, incremental abilities.

### 5.5 HP Recovery: Scarcity vs Availability

**Design Goal**: Healing should be scarce enough to create tension but available enough to recover from mistakes

**Current Healing Sources**:

- **Consumables**: Limited quantity (2-5 per run), avg 7-10 HP each
- **Abilities**: High Stamina cost (20+ Stamina), avg 7-10 HP
- **Rest**: Full heal, but only at Sanctuary (rare)
- **Milestones**: Full heal, but only at progression thresholds

**Balance**: Total healing per run is **~30-50 HP** from consumables + abilities, enough to recover from 2-3 mistakes but not enough for prolonged attrition.

**Consequence**: Players must avoid damage proactively (using Defend, Quick Dodge, etc.) rather than relying on healing.

### 5.6 Death Threshold: 0 HP = Instant Death

**Current Design**: No "unconscious" or "dying" state; 0 HP = game over

**Pros**:

- High tension (every HP matters)
- Simple to understand (no complex dying rules)
- Encourages defensive play

**Cons**:

- Unforgiving for new players
- No "clutch comeback" moments
- Single mistake can end run

**Alternative Design (Rejected)**: "Death's Door" state (0 HP = unconscious, heal to revive)

- **Problem**: Reduces tension, makes HP less valuable
- **Result**: Players become reckless, spamming abilities without fear

**Conclusion**: Instant death at 0 HP maintains high stakes and strategic depth.

### 5.7 Warrior's Vigor: Passive HP Bonus

**Current Design**: +10% MaxHP (multiplicative after base + milestones)

**Balance**:

- At Milestone 0: 50 → 55 HP (+5 HP)
- At Milestone 3: 80 → 88 HP (+8 HP)

**Design Note**: Percentage bonus scales with progression, rewarding late-game Warriors without dominating early game.

**Trade-off**: Other classes lack passive HP bonuses, compensating with higher Stamina (Mystic) or versatility (Scavenger).

### 5.8 Equipment HP Bonuses: Power Scaling

**Current Design**: Armor grants flat +5 to +30 HP

**Balance Tiers**:

- **Light Armor** (+5-10 HP): Minimal survivability, high mobility
- **Medium Armor** (+10-20 HP): Balanced survivability and mobility
- **Heavy Armor** (+20-30 HP): Maximum survivability, low mobility

**Impact**:

- Warrior with Heavy Armor: 50 base + 30 equipment + 30 milestones = **110 HP** (before Warrior's Vigor)
- Mystic with Light Armor: 30 base + 10 equipment + 30 milestones = **70 HP**

**Result**: Equipment bonuses maintain class identity (Warriors remain tankiest) while allowing customization.

### 5.9 Stamina Regeneration: Absence in v0.1

**Current Design**: No per-turn Stamina regeneration during combat

**Pros**:

- Forces resource management (can't spam abilities)
- Creates tension (each ability use is a commitment)
- Rewards tactical ability selection

**Cons**:

- Punishes mistakes (wasted Stamina = permanent loss)
- Long combats become "basic attack spam" after Stamina depletion
- Reduces ability usage frequency

**Future Feature (v0.5+)**: Per-turn Stamina regeneration (+3-5 per turn)

- **Benefit**: Sustains ability usage in long combats
- **Trade-off**: Reduces strategic tension (Stamina becomes less scarce)

**Conclusion**: Current design emphasizes tactical ability usage over sustained DPS rotations.

### 5.10 AP System: Minimal Implementation

**Current Design**: AP exists (10 AP for all classes) but is minimally implemented in v0.7

**Purpose**: Reserved for future movement/exploration system (v0.9+)

**Balance Note**: AP will create "exploration budget" tension, forcing players to choose between thorough exploration (high AP cost) and efficient progression (low AP cost).

---

## 6. Future Expansion Notes

### 6.1 Planned Features (v0.5+)

1. **Stamina Regeneration**
    - +3-5 Stamina per turn during combat
    - Out-of-combat: Full recovery after 3 turns
2. **Temporary HP (TempHP)**
    - Shields, buffs grant temporary HP that absorbs damage first
    - Already implemented: `player.TempHP` (from Saga of the Einherjar ability)
3. **HP/Stamina Percentage Abilities**
    - Abilities that cost % of MaxHP/MaxStamina instead of flat amounts
    - Example: "Desperate Strike" costs 50% current HP
4. **Over-Heal Mechanics**
    - Healing above MaxHP grants TempHP
    - Caps at MaxHP × 1.5
5. **Resource Conversion**
    - Spend HP to gain Stamina (Blood Rite)
    - Spend Stamina to gain temporary HP (Adrenaline Surge)

### 6.2 Known Limitations

1. **No HP/Stamina Regeneration**: Resources don't recover during combat (v0.1-v0.4)
2. **Fixed AP**: All classes have 10 AP, no customization
3. **No Resource % Abilities**: All costs are flat integers
4. **No Overflow Healing**: Healing above MaxHP is wasted
5. **Binary Death**: No "unconscious" or "death's door" state

### 6.3 Balance Knobs

Tunable parameters for future balance adjustments:

| Parameter | Current Value | Range | Impact |
| --- | --- | --- | --- |
| Warrior Base HP | 50 | 40-60 | Survivability curve |
| Scavenger Base HP | 40 | 35-50 | Balanced survivability |
| Mystic Base HP | 30 | 25-40 | Glass cannon risk |
| Warrior Base Stamina | 30 | 25-40 | Ability frequency |
| Mystic Base Stamina | 50 | 40-60 | Caster resource pool |
| HP per Milestone | +10 | +5 to +15 | Progression rate |
| Stamina per Milestone | +5 | +3 to +10 | Resource scaling |
| Warrior's Vigor Bonus | +10% | +5% to +20% | Passive power |
| Stamina Regen (future) | +3-5/turn | +0 to +10 | Resource availability |

---

## 7. Quick Reference

### 7.1 Base Resource Pools

| Class | HP | Stamina | AP |
| --- | --- | --- | --- |
| Warrior | 50 | 30 | 10 |
| Scavenger | 40 | 40 | 10 |
| Mystic | 30 | 50 | 10 |
| Adept | 35 | 40 | 10 |

### 7.2 Milestone Bonuses

| Milestone | HP Gain | Stamina Gain | PP Gain | Full Heal |
| --- | --- | --- | --- | --- |
| 1 | +10 | +5 | +1 | Yes |
| 2 | +10 | +5 | +1 | Yes |
| 3 | +10 | +5 | +1 | Yes |
| **Total** | **+30** | **+15** | **+3** | - |

### 7.3 Ability Cost Tiers

| Tier | Stamina Cost | Examples | Uses per Combat (Base) |
| --- | --- | --- | --- |
| Basic | 10-15 | Power Strike, Quick Dodge | 2-5 |
| Utility | 5-20 | Exploit Weakness, Analyze | 2-10 |
| Ultimate | 30-50 | Desperate Gambit, Corruption Nova | 1-2 |

### 7.4 Healing Sources

| Source | Amount | Availability |
| --- | --- | --- |
| Healing Salve | 2d6 (7 avg) | Consumable |
| Medicinal Tonic | 3d6 (10.5 avg) | Rare Consumable |
| Survivalist | 2d6 (7 avg) | 20 Stamina |
| Mend Wound | 3d6 (10.5 avg) | 5 Stamina (Bone-Setter) |
| Rest | Full MaxHP | Sanctuary only |
| Milestone | Full MaxHP | Progression |

---

**End of Document***Last Updated*: v0.17 Documentation Phase
*Implementation Version*: v0.7
*Total Lines*: 1,600+

---

## Appendix: Code Locations

| Feature | File | Line Range |
| --- | --- | --- |
| Resource Properties | `RuneAndRust.Core/PlayerCharacter.cs` | 24-30 |
| Warrior Initialization | `RuneAndRust.Engine/CharacterFactory.cs` | 49-98 |
| Scavenger Initialization | `RuneAndRust.Engine/CharacterFactory.cs` | 100-190 |
| Mystic Initialization | `RuneAndRust.Engine/CharacterFactory.cs` | 192-283 |
| RecalculatePlayerStats | `RuneAndRust.Engine/EquipmentService.cs` | 220-241 |
| GetBaseMaxHP | `RuneAndRust.Engine/EquipmentService.cs` | 246-261 |
| ReachMilestone | `RuneAndRust.Engine/SagaService.cs` | 51-80 |
| Stamina Deduction | `RuneAndRust.Engine/CombatEngine.cs` | 364-367 |
| HP Damage | `RuneAndRust.Engine/EnemyAI.cs` | 788-790 |
| IsAlive Property | `RuneAndRust.Core/PlayerCharacter.cs` | 85 |