---
id: SPEC-LOOT-001
title: Loot Generation System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-INTERACT-001, SPEC-INV-001, SPEC-COMBAT-001]
---

# SPEC-LOOT-001: Loot Generation System

> **Version:** 1.0.1
> **Status:** Implemented (v0.3.1a)
> **Service:** `LootService`
> **Location:** `RuneAndRust.Engine/Services/LootService.cs`

---

## Overview

The **Loot Generation System** creates randomized items for container searches and combat rewards using danger-level-scaled quality tiers, biome-based type weighting, and WITS-bonus upgrade chances. The system provides 15 weapon templates, 13 armor templates, 13 consumable templates, 11 material templates, and 8 universal junk items—all with AAM-VOICE compliant descriptions.

### Core Design Principles

1. **Danger-Scaled Quality Distribution**: Lethal zones drop better loot than Safe zones
2. **WITS Bonus Upgrade Mechanic**: High WITS characters can shift quality tiers upward
3. **Biome-Themed Type Weights**: Industrial zones favor materials, Organic favors consumables
4. **Quality-Based Value Scaling**: Myth-Forged items worth ×3.0 base value
5. **AAM-VOICE Template Compliance**: All descriptions follow Domain 4 constraints

### System Boundaries

**IN SCOPE:**
- Quality tier determination with WITS-based upgrade chances
- Item count generation based on danger level
- Item type selection with biome weighting
- Template-based item instantiation
- Value and weight calculation
- Container loot tier override support

**OUT OF SCOPE:**
- Loot placement in containers (handled by ObjectSpawner/DungeonGenerator)
- Enemy death loot drops (future integration with Combat System)
- Loot tier balancing / drop rate tuning
- Player inventory addition (handled by InventoryService)
- Container state management (handled by InteractionService)

---

## Quality Tier System

### Quality Tier Enum

```csharp
public enum QualityTier
{
    JuryRigged = 0,   // Improvised, barely functional
    Scavenged = 1,    // Salvaged, worn
    ClanForged = 2,   // Standard craftsmanship
    Optimized = 3,    // Enhanced, refined
    MythForged = 4    // Legendary, exceptional
}
```

### Quality Characteristics

| Tier | Description | Value Multiplier | Typical Condition |
|------|-------------|-----------------|-------------------|
| **Jury-Rigged** | Improvised from scrap | ×0.5 | Fragile, unreliable |
| **Scavenged** | Salvaged and repaired | ×1.0 | Worn, functional |
| **Clan-Forged** | Standard craftsmanship | ×1.5 | Solid, dependable |
| **Optimized** | Enhanced and refined | ×2.0 | Superior, well-maintained |
| **Myth-Forged** | Legendary quality | ×3.0 | Exceptional, near-perfect |

---

## Behaviors

### Primary Behaviors

#### 1. Container Search Loot Generation (`SearchContainerAsync`)

**Signature:**
```csharp
Task<LootResult> SearchContainerAsync(
    InteractableObject container,
    LootGenerationContext context)
```

**Process:**

1. **Validate Container State**:
   ```csharp
   if (!container.IsContainer)
       return LootResult.Failure("Object is not a container");
   if (!container.IsOpen)
       return LootResult.Failure("Container is locked or closed");
   if (container.HasBeenSearched)
       return LootResult.Empty();
   ```

2. **Override Quality Tier** (if container has preset tier):
   ```csharp
   var effectiveContext = container.LootTier.HasValue
       ? context with { LootTier = container.LootTier.Value }
       : context;
   ```

3. **Generate Loot**:
   ```csharp
   var items = GenerateLoot(effectiveContext);
   ```

4. **Mark Container Searched**:
   ```csharp
   container.HasBeenSearched = true;
   ```

5. **Return Result**:
   ```csharp
   return new LootResult(
       Success: true,
       Message: $"Found {items.Count} items",
       Items: items,
       TotalValue: items.Sum(i => i.Value),
       TotalWeight: items.Sum(i => i.Weight)
   );
   ```

**Outcomes:**
- **Success**: Items generated, container marked searched
- **Failure**: Invalid container state (not container, locked, etc.)
- **Empty**: Container already searched

---

#### 2. Loot Generation (`GenerateLoot`)

**Signature:**
```csharp
IReadOnlyList<Item> GenerateLoot(LootGenerationContext context)
```

**Process:**

1. **Determine Item Count**:
   ```csharp
   var itemCount = RollItemCount(context.DangerLevel);
   // Safe: 1-2, Unstable: 1-3, Hostile: 2-4, Lethal: 2-5
   ```

2. **Generate Each Item**:
   ```csharp
   for (int i = 0; i < itemCount; i++)
   {
       var item = GenerateItem(context);
       items.Add(item);
   }
   ```

3. **Return Item List**:
   ```csharp
   return items.AsReadOnly();
   ```

**Example:**
- Danger: Hostile (2-4 items)
- Roll: 3 items
- Result: [Clan-Forged Sword, Scavenged Ration, Jury-Rigged Scrap Metal]

---

#### 3. Single Item Generation (`GenerateItem`)

**Signature:**
```csharp
Item GenerateItem(LootGenerationContext context)
```

**Process:**

1. **Determine Quality Tier**:
   ```csharp
   var quality = context.LootTier.HasValue
       ? context.LootTier.Value
       : RollQualityTier(context.DangerLevel, context.WitsBonus);
   ```

2. **Select Item Type** (biome-weighted):
   ```csharp
   var itemType = RollItemType(context.BiomeType);
   ```

3. **Select Template**:
   ```csharp
   var template = SelectTemplate(itemType, quality);
   ```

4. **Instantiate Item**:
   ```csharp
   var item = InstantiateFromTemplate(template, quality);
   ```

5. **Return Item**:
   ```csharp
   return item;
   ```

---

#### 4. Quality Tier Rolling (`RollQualityTier`)

**Signature:**
```csharp
QualityTier RollQualityTier(DangerLevel danger, int witsBonus)
```

**Algorithm:**

1. **Get Base Weights**:
   ```csharp
   var weights = QualityWeightsByDanger[danger];
   // Example (Hostile): [10, 35, 35, 18, 2]
   ```

2. **Apply WITS Upgrade Chance**:
   ```csharp
   var upgradeChance = Math.Min(witsBonus * 2, 20);  // Cap at 20%
   if (Random.Next(100) < upgradeChance)
   {
       weights = ShiftWeightsUp(weights);
       // Shifts all weights one tier higher
   }
   ```

3. **Weighted Random Selection**:
   ```csharp
   var quality = WeightedRandom(weights);
   return quality;
   ```

**Quality Weight Distribution by Danger Level:**

| Danger Level | Jury-Rigged | Scavenged | Clan-Forged | Optimized | Myth-Forged |
|--------------|-------------|-----------|-------------|-----------|-------------|
| **Safe** | 30% | 60% | 10% | 0% | 0% |
| **Unstable** | 20% | 50% | 25% | 5% | 0% |
| **Hostile** | 10% | 35% | 35% | 18% | 2% |
| **Lethal** | 5% | 25% | 40% | 25% | 5% |

**WITS Upgrade Mechanic:**
- **WITS Bonus 3**: 6% upgrade chance (3 × 2)
- **WITS Bonus 7**: 14% upgrade chance (7 × 2)
- **WITS Bonus 10+**: 20% upgrade chance (capped)

**Upgrade Effect:**
- Shifts entire weight distribution one tier higher
- Example: Hostile (10/35/35/18/2) → Upgraded (0/10/35/35/20)
- Cannot shift Myth-Forged higher (weights overflow into Myth-Forged)

---

#### 5. Item Type Selection (`RollItemType`)

**Signature:**
```csharp
ItemType RollItemType(BiomeType biome)
```

**Biome-Based Type Weights:**

| Biome | Weapon | Armor | Consumable | Material | Junk |
|-------|--------|-------|------------|----------|------|
| **Ruin** | 20% | 15% | 15% | 25% | 25% |
| **Industrial** | 15% | 20% | 10% | 35% | 20% |
| **Organic** | 10% | 10% | 40% | 25% | 15% |
| **Void** | 25% | 25% | 10% | 20% | 20% |

**Implementation:**
```csharp
var weights = BiomeTypeWeights[biome];
return WeightedRandom(weights);
```

**Design Rationale:**
- **Ruin**: Balanced scavenging (furniture debris, inscriptions)
- **Industrial**: High material yield (scrap metal, components)
- **Organic**: Consumable-rich (food, medicinal plants)
- **Void**: Combat-focused (weapons, armor from fallen warriors)

---

#### 6. Item Count Rolling (`RollItemCount`)

**Signature:**
```csharp
int RollItemCount(DangerLevel danger)
```

**Item Count Ranges:**

| Danger Level | Min Items | Max Items | Example Roll |
|--------------|-----------|-----------|--------------|
| **Safe** | 1 | 2 | 1-2 items |
| **Unstable** | 1 | 3 | 1-3 items |
| **Hostile** | 2 | 4 | 2-4 items |
| **Lethal** | 2 | 5 | 2-5 items |

**Implementation:**
```csharp
var (min, max) = ItemCountsByDanger[danger];
return Random.Next(min, max + 1);  // Inclusive range
```

---

#### 7. Template Selection (`SelectTemplate`)

**Signature:**
```csharp
ItemTemplate SelectTemplate(ItemType type, QualityTier quality)
```

**Process:**

1. **Get Template List**:
   ```csharp
   var templates = type switch
   {
       ItemType.Weapon => WeaponsByQuality[quality],
       ItemType.Armor => ArmorByQuality[quality],
       ItemType.Consumable => ConsumablesByQuality[quality],
       ItemType.Material => MaterialsByQuality[quality],
       ItemType.Junk => JunkItems  // Quality ignored for junk
   };
   ```

2. **Random Selection**:
   ```csharp
   var index = Random.Next(templates.Count);
   return templates[index];
   ```

**Template Counts by Type:**
- Weapons: 15 templates (3 per quality tier)
- Armor: 13 templates (varying per tier)
- Consumables: 13 templates (varying per tier)
- Materials: 11 templates (varying per tier)
- Junk: 8 universal templates (quality always Jury-Rigged)

---

#### 8. Item Instantiation (`InstantiateFromTemplate`)

**Purpose:** Create Item entity from template with quality-based scaling

**Process:**

1. **Copy Template Properties**:
   ```csharp
   var item = new Item
   {
       Id = Guid.NewGuid(),
       Name = template.Name,
       ItemType = template.ItemType,
       Description = template.Description,
       Weight = template.Weight,
       Value = ScaleValue(template.BaseValue, quality),
       Quality = quality
   };
   ```

2. **Apply Quality-Specific Scaling** (weapons/armor):
   ```csharp
   if (item is Equipment equipment)
   {
       equipment.Durability = ScaleDurability(template.BaseDurability, quality);
       equipment.SoakBonus = ScaleSoak(template.BaseSoak, quality);
   }
   ```

3. **Return Item**:
   ```csharp
   return item;
   ```

---

### Secondary Behaviors

#### 1. Value Scaling (`ScaleValue`)

**Formula:**
```
Scaled Value = Base Value × Quality Multiplier
```

**Quality Multipliers:**
- Jury-Rigged: ×0.5
- Scavenged: ×1.0
- Clan-Forged: ×1.5
- Optimized: ×2.0
- Myth-Forged: ×3.0

**Example:**
- Base Value: 50 scrip (Iron Sword)
- Quality: Clan-Forged
- **Scaled Value: 50 × 1.5 = 75 scrip**

---

#### 2. Weight Shifting (`ShiftWeightsUp`)

**Purpose:** Upgrade quality distribution when WITS bonus triggers

**Algorithm:**
```csharp
float[] ShiftWeightsUp(float[] weights)
{
    var shifted = new float[5];
    shifted[0] = 0;  // No Jury-Rigged after upgrade
    for (int i = 1; i < 5; i++)
    {
        shifted[i] = weights[i - 1];
    }
    // Overflow from Myth-Forged stays in Myth-Forged
    shifted[4] += weights[4];
    return shifted;
}
```

**Example (Hostile Base Weights):**
- Before: [10, 35, 35, 18, 2]
- After: [0, 10, 35, 35, 20]
- Result: Much higher chance of Optimized/Myth-Forged

---

#### 3. Weighted Random Selection (`WeightedRandom`)

**Purpose:** Select item from weighted distribution

**Algorithm:**
```csharp
T WeightedRandom<T>(Dictionary<T, float> weights)
{
    var total = weights.Values.Sum();
    var roll = Random.Next(0, (int)total);

    float cumulative = 0;
    foreach (var (item, weight) in weights)
    {
        cumulative += weight;
        if (roll < cumulative)
            return item;
    }

    return weights.Last().Key;  // Fallback
}
```

---

## Restrictions

### MUST Requirements

1. **MUST validate container state before loot generation**
   - **Reason:** Prevent duplicate searches, locked container exploitation
   - **Implementation:** LootService.cs:62-72

2. **MUST mark container HasBeenSearched after successful search**
   - **Reason:** Prevent infinite loot farming
   - **Implementation:** LootService.cs:98

3. **MUST respect container LootTier override**
   - **Reason:** Allow designers to guarantee specific quality loot
   - **Implementation:** LootService.cs:80-83

4. **MUST cap WITS upgrade chance at 20%**
   - **Reason:** Prevent guaranteed upgrades, maintain loot balance
   - **Implementation:** LootService.cs:152

5. **MUST use quality-appropriate templates**
   - **Reason:** Ensure item flavor matches quality tier
   - **Implementation:** LootService.cs:215-227

6. **MUST assign Jury-Rigged quality to all junk items**
   - **Reason:** Junk is inherently low-quality
   - **Implementation:** LootService.cs:241

7. **MUST scale item value by quality multiplier**
   - **Reason:** Higher quality = higher value for economy balance
   - **Implementation:** LootService.cs:267-279

8. **MUST generate at least 1 item for all danger levels**
   - **Reason:** Prevent empty containers (bad player experience)
   - **Implementation:** ItemCountsByDanger minimum = 1

---

### MUST NOT Requirements

1. **MUST NOT allow negative WITS bonus**
   - **Violation Impact:** Downgrade loot quality (punishes dump stats)
   - **Enforcement:** WITS bonus parameter should be validated >= 0

2. **MUST NOT exceed 5 items per container**
   - **Violation Impact:** Inventory overflow, UI issues
   - **Enforcement:** Lethal max = 5 (hardcoded)

3. **MUST NOT generate loot for searched containers**
   - **Violation Impact:** Infinite loot exploit
   - **Enforcement:** HasBeenSearched check (line 68)

4. **MUST NOT allow quality tier > MythForged**
   - **Violation Impact:** Array out of bounds, undefined quality
   - **Enforcement:** QualityTier enum bounds + ShiftWeightsUp() overflow handling

5. **MUST NOT use biome weights for non-biome contexts**
   - **Violation Impact:** Thematic inconsistency (fire gear in ice biome)
   - **Enforcement:** BiomeType parameter required in GenerateItem()

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Min items per container | 1 | All danger levels |
| Max items per container | 5 | Lethal danger level |
| Max WITS upgrade chance | 20% | Caps at WITS +10 |
| Quality tier count | 5 | Jury-Rigged to Myth-Forged |
| Max quality value multiplier | ×3.0 | Myth-Forged tier |

### Functional Limitations

1. **No Set Items / Unique Loot**
   - Current: All items procedural from templates
   - Future: Named unique items with fixed properties

2. **No Loot Rarity Tracking**
   - Current: Quality tier only
   - Future: Separate rarity system (Common, Rare, Legendary)

3. **No Enemy Death Loot Integration**
   - Current: Container-only loot
   - Future: EnemyTemplate.LootTableId integration

4. **No Loot Level Scaling**
   - Current: Quality tier only
   - Future: Item level system (Level 1-10 Iron Sword)

5. **No Conditional Loot**
   - Current: No "if player has X, drop Y" logic
   - Future: Quest-based loot drops

6. **No Template Variation**
   - Current: Fixed template properties
   - Future: Procedural stat ranges (1d8+2 weapon damage)

---

## Use Cases

### USE CASE 1: Standard Container Search (Hostile Zone)

**Setup:**
```csharp
var container = new InteractableObject
{
    Name = "Corroded Locker",
    IsContainer = true,
    IsOpen = true,
    HasBeenSearched = false,
    LootTier = null  // No override
};

var context = new LootGenerationContext(
    BiomeType: BiomeType.Industrial,
    DangerLevel: DangerLevel.Hostile,
    LootTier: null,
    WitsBonus: 5  // 10% upgrade chance
);
```

**Execution:**
```csharp
var result = await _lootService.SearchContainerAsync(container, context);
```

**Internal Flow:**

1. **Validate Container**: IsContainer ✓, IsOpen ✓, Not Searched ✓
2. **Item Count**: Hostile (2-4) → Roll 3 items
3. **Item 1 Generation**:
   - Quality Roll: Hostile weights [10, 35, 35, 18, 2]
   - WITS Upgrade: d100 = 38 (≥10) → **No upgrade**
   - Quality: Scavenged (rolled 42, falls in 35% band)
   - Type: Industrial biome → Material (35% weight, rolled)
   - Template: "Corroded Piping" (Scavenged material)
   - Value: 15 × 1.0 = 15 scrip
4. **Item 2 Generation**:
   - Quality: Clan-Forged (rolled lucky)
   - Type: Armor (20% weight, rolled)
   - Template: "Riveted Plates" (Clan-Forged armor)
   - Value: 120 × 1.5 = 180 scrip
5. **Item 3 Generation**:
   - Quality: Jury-Rigged
   - Type: Junk
   - Template: "Broken Gears"
   - Value: 5 × 0.5 = 2.5 scrip (rounded to 2)
6. **Mark Searched**: container.HasBeenSearched = true
7. **Return Result**:
   - Items: [Corroded Piping, Riveted Plates, Broken Gears]
   - TotalValue: 197 scrip
   - TotalWeight: 2500g + 8000g + 500g = 11000g

**Assertions:**
- `result.Success == true`
- `result.Items.Count == 3`
- `result.TotalValue == 197`
- `container.HasBeenSearched == true`

---

### USE CASE 2: Myth-Forged Guaranteed Drop (Container Override)

**Setup:**
```csharp
var bossChest = new InteractableObject
{
    Name = "Ancient Vault",
    IsContainer = true,
    IsOpen = true,
    HasBeenSearched = false,
    LootTier = QualityTier.MythForged  // Designer override
};

var context = new LootGenerationContext(
    BiomeType: BiomeType.Void,
    DangerLevel: DangerLevel.Lethal,
    LootTier: null,
    WitsBonus: 0
);
```

**Execution:**
```csharp
var result = await _lootService.SearchContainerAsync(bossChest, context);
```

**Internal Flow:**

1. **Override Quality**: Context tier = MythForged (from container)
2. **Item Count**: Lethal (2-5) → Roll 4 items
3. **All Items Generated at Myth-Forged Quality**:
   - Void biome type weights favor Weapon/Armor (25% each)
   - Example drops:
     - "Rune-Etched Greatsword" (Myth-Forged weapon, 500 × 3.0 = 1500 scrip)
     - "God-Plate Cuirass" (Myth-Forged armor, 600 × 3.0 = 1800 scrip)
     - "Aetheric Catalyst" (Myth-Forged material, 300 × 3.0 = 900 scrip)
     - "Bottled Starlight" (Myth-Forged consumable, 200 × 3.0 = 600 scrip)
4. **Total Value**: 4800 scrip (massive reward)

**Assertions:**
- All items have `Quality == QualityTier.MythForged`
- Total value significantly higher than normal Lethal loot

---

### USE CASE 3: High WITS Upgrade Success

**Setup:**
```csharp
var context = new LootGenerationContext(
    BiomeType: BiomeType.Ruin,
    DangerLevel: DangerLevel.Unstable,
    LootTier: null,
    WitsBonus: 10  // 20% upgrade chance (capped)
);
```

**Execution:**
```csharp
var item = _lootService.GenerateItem(context);
```

**Scenario: Upgrade Triggered**

1. **Base Weights** (Unstable): [20, 50, 25, 5, 0]
2. **Upgrade Roll**: d100 = 12 (< 20) → **Upgrade triggered**
3. **Shifted Weights**: [0, 20, 50, 25, 5]
   - Jury-Rigged: 0% (was 20%)
   - Scavenged: 20% (was 50%)
   - Clan-Forged: 50% (was 25%)
   - Optimized: 25% (was 5%)
   - Myth-Forged: 5% (was 0%)
4. **Quality Roll**: 68 → Clan-Forged (50% band)
5. **Result**: Clan-Forged item (would have been Scavenged without upgrade)

**Benefit:**
- 10 WITS player has 1-in-5 chance to get better loot tier
- Meaningful reward for investment in WITS attribute

---

### USE CASE 4: Already Searched Container

**Setup:**
```csharp
var container = new InteractableObject
{
    IsContainer = true,
    IsOpen = true,
    HasBeenSearched = true  // Already looted
};
```

**Execution:**
```csharp
var result = await _lootService.SearchContainerAsync(container, context);
```

**Outcome:**
```csharp
result.Success == true
result.Items.Count == 0
result.Message == "Container already searched"
result.TotalValue == 0
```

**Assertions:**
- No items generated
- No exception thrown
- Graceful empty result

---

### USE CASE 5: Locked Container (Failure)

**Setup:**
```csharp
var container = new InteractableObject
{
    IsContainer = true,
    IsOpen = false,  // Still locked
    IsLocked = true
};
```

**Execution:**
```csharp
var result = await _lootService.SearchContainerAsync(container, context);
```

**Outcome:**
```csharp
result.Success == false
result.Message == "Container is locked or closed"
result.Items.Count == 0
```

**Expected User Flow:**
1. Player attempts search → Failure result
2. Player uses lockpicking (WITS check via InteractionService)
3. On success: container.IsOpen = true
4. Player searches again → Loot generated

---

### USE CASE 6: Junk-Only Safe Zone

**Setup:**
```csharp
var context = new LootGenerationContext(
    BiomeType: BiomeType.Ruin,
    DangerLevel: DangerLevel.Safe,
    LootTier: null,
    WitsBonus: 0
);

// Multiple generations to observe distribution
var items = new List<Item>();
for (int i = 0; i < 20; i++)
{
    items.Add(_lootService.GenerateItem(context));
}
```

**Expected Distribution:**
- ~30% Jury-Rigged items
- ~60% Scavenged items
- ~10% Clan-Forged items
- ~0% Optimized/Myth-Forged

**Typical Results:**
- 6 Jury-Rigged (scrap metal, broken tools)
- 12 Scavenged (rations, cloth, common materials)
- 2 Clan-Forged (basic weapons/armor)
- Average value: ~25 scrip per item

---

## Decision Trees

### Decision Tree 1: SearchContainerAsync Flow

```
SearchContainerAsync(container, context)
│
├─ VALIDATE CONTAINER STATE
│  ├─ !IsContainer?
│  │  └─ RETURN Failure("Not a container")
│  ├─ !IsOpen?
│  │  └─ RETURN Failure("Locked or closed")
│  ├─ HasBeenSearched?
│  │  └─ RETURN Empty("Already searched")
│  └─ CONTINUE
│
├─ APPLY LOOT TIER OVERRIDE
│  ├─ container.LootTier.HasValue?
│  │  └─ YES → Override context tier
│  └─ NO → Use context tier (or roll)
│
├─ GENERATE LOOT
│  └─ items = GenerateLoot(context)
│
├─ MARK CONTAINER SEARCHED
│  └─ container.HasBeenSearched = true
│
└─ RETURN Success(items, totalValue, totalWeight)
```

---

### Decision Tree 2: Quality Tier Rolling

```
RollQualityTier(dangerLevel, witsBonus)
│
├─ GET BASE WEIGHTS
│  └─ weights = QualityWeightsByDanger[dangerLevel]
│
├─ WITS UPGRADE CHECK
│  ├─ Calculate upgradeChance = min(witsBonus * 2, 20)
│  ├─ Roll d100 < upgradeChance?
│  │  ├─ YES → ShiftWeightsUp(weights)
│  │  │        [10,35,35,18,2] → [0,10,35,35,20]
│  │  └─ NO → Use base weights
│  └─ CONTINUE
│
├─ WEIGHTED RANDOM SELECTION
│  ├─ total = sum(weights)  // e.g., 100
│  ├─ roll = Random(0, total)
│  ├─ Find cumulative band for roll
│  └─ RETURN quality tier
│
└─ RETURN QualityTier enum
```

---

### Decision Tree 3: Item Type Selection (Biome-Weighted)

```
RollItemType(biomeType)
│
├─ GET BIOME TYPE WEIGHTS
│  └─ weights = BiomeTypeWeights[biomeType]
│     Example (Industrial): {Weapon:15, Armor:20, Consumable:10, Material:35, Junk:20}
│
├─ WEIGHTED RANDOM SELECTION
│  ├─ total = sum(weights)  // 100
│  ├─ roll = Random(0, total)
│  └─ Determine item type from cumulative bands
│
└─ RETURN ItemType enum
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `Random` | .NET BCL | Weighted random selection, item count rolls, quality rolls |
| `ILogger` | Infrastructure | Loot generation event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `InteractionService` | [SPEC-INTERACT-001](SPEC-INTERACT-001.md) | Container searching via SearchContainerAsync() |
| `InventoryService` | [SPEC-INV-001](SPEC-INV-001.md) | Adding generated items to player inventory |
| `CombatService` (future) | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Enemy death loot drops via EnemyTemplate.LootTableId |

---

## Data Models

### LootResult

**Source:** `RuneAndRust.Core/Models/LootResult.cs`

```csharp
public record LootResult(
    bool Success,
    string Message,
    IReadOnlyList<Item> Items,
    int TotalValue,
    int TotalWeight
)
{
    public static LootResult Failure(string message) =>
        new(false, message, Array.Empty<Item>(), 0, 0);

    public static LootResult Empty() =>
        new(true, "Container already searched", Array.Empty<Item>(), 0, 0);
}
```

---

### LootGenerationContext

**Source:** `RuneAndRust.Engine/Services/LootService.cs` (internal record)

```csharp
public record LootGenerationContext(
    BiomeType BiomeType,
    DangerLevel DangerLevel,
    QualityTier? LootTier,
    int WitsBonus = 0
);
```

---

### Item (Base Entity)

**Source:** `RuneAndRust.Core/Entities/Item.cs`

```csharp
public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemType ItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Weight { get; set; }  // In grams
    public int Value { get; set; }   // In scrip
    public QualityTier Quality { get; set; }
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; } = 99;
    public List<string> Tags { get; set; } = new();
}
```

---

## Loot Tables (AAM-VOICE Compliant)

### Location

**Source:** `RuneAndRust.Engine/Services/LootTables.cs` (441 lines)

**Structure:**
- `WeaponsByQuality`: Dictionary<QualityTier, List<ItemTemplate>>
- `ArmorByQuality`: Dictionary<QualityTier, List<ItemTemplate>>
- `ConsumablesByQuality`: Dictionary<QualityTier, List<ItemTemplate>>
- `MaterialsByQuality`: Dictionary<QualityTier, List<ItemTemplate>>
- `JunkItems`: List<ItemTemplate> (quality-agnostic)

### Example Templates (Domain 4 Compliant)

**Weapon - Scavenged Tier:**
```csharp
new ItemTemplate(
    "Rust-Pitted Blade",
    "A corroded sword, its edge still capable of harm despite extensive decay.",
    ItemType.Weapon,
    baseValue: 35,
    weight: 1200,  // 1.2kg
    damageDie: 6
)
```

**Armor - Clan-Forged Tier:**
```csharp
new ItemTemplate(
    "Riveted Plate Cuirass",
    "Hammered steel plates joined by carefully worked rivets. Solid protection.",
    ItemType.Armor,
    baseValue: 120,
    weight: 8000,  // 8kg
    soakBonus: 5
)
```

**Consumable - Optimized Tier:**
```csharp
new ItemTemplate(
    "Concentrated Ration Block",
    "Dense nutrient paste compressed into a pale brick. Sustains far longer than appearance suggests.",
    ItemType.Consumable,
    baseValue: 45,
    weight: 200,  // 200g
    tags: ["Ration"]
)
```

**Material - Jury-Rigged Tier:**
```csharp
new ItemTemplate(
    "Bent Scrap Metal",
    "Twisted fragments of corroded plating. Barely useful even for improvised repairs.",
    ItemType.Material,
    baseValue: 8,
    weight: 500
)
```

**Junk (Universal):**
```csharp
new ItemTemplate(
    "Broken Gears",
    "Cracked mechanical components from unknown machinery. No discernible function remains.",
    ItemType.Junk,
    baseValue: 5,
    weight: 300
)
```

### Domain 4 Validation

All 60 templates follow AAM-VOICE Layer 2 constraints:
- ✅ No precision measurements ("1.2kg" rendered as weight field, not text)
- ✅ Qualitative descriptors ("extensive decay", "carefully worked")
- ✅ Epistemic uncertainty ("appears", "suggests", "seemingly")
- ✅ Clinical-archaic tone ("hammered steel", "corroded plating")
- ❌ No forbidden patterns ("95% pure", "2.5 meters long", "CPU core")

---

## Configuration

### Quality Weight Tables

```csharp
private static readonly Dictionary<DangerLevel, float[]> QualityWeightsByDanger = new()
{
    [DangerLevel.Safe] = new float[] { 30, 60, 10, 0, 0 },
    [DangerLevel.Unstable] = new float[] { 20, 50, 25, 5, 0 },
    [DangerLevel.Hostile] = new float[] { 10, 35, 35, 18, 2 },
    [DangerLevel.Lethal] = new float[] { 5, 25, 40, 25, 5 }
};
```

---

### Item Count Ranges

```csharp
private static readonly Dictionary<DangerLevel, (int Min, int Max)> ItemCountsByDanger = new()
{
    [DangerLevel.Safe] = (1, 2),
    [DangerLevel.Unstable] = (1, 3),
    [DangerLevel.Hostile] = (2, 4),
    [DangerLevel.Lethal] = (2, 5)
};
```

---

### Biome Type Weights

```csharp
private static readonly Dictionary<BiomeType, Dictionary<ItemType, float>> BiomeTypeWeights = new()
{
    [BiomeType.Ruin] = new()
    {
        [ItemType.Weapon] = 20,
        [ItemType.Armor] = 15,
        [ItemType.Consumable] = 15,
        [ItemType.Material] = 25,
        [ItemType.Junk] = 25
    },
    [BiomeType.Industrial] = new()
    {
        [ItemType.Weapon] = 15,
        [ItemType.Armor] = 20,
        [ItemType.Consumable] = 10,
        [ItemType.Material] = 35,
        [ItemType.Junk] = 20
    },
    // ... Organic, Void
};
```

---

### Quality Value Multipliers

```csharp
private static readonly Dictionary<QualityTier, float> QualityValueMultipliers = new()
{
    [QualityTier.JuryRigged] = 0.5f,
    [QualityTier.Scavenged] = 1.0f,
    [QualityTier.ClanForged] = 1.5f,
    [QualityTier.Optimized] = 2.0f,
    [QualityTier.MythForged] = 3.0f
};
```

---

### WITS Upgrade Constants

```csharp
private const int WitsUpgradeMultiplier = 2;  // 2% per WITS bonus point
private const int MaxWitsUpgradeChance = 20;  // 20% cap
```

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/LootServiceTests.cs` (542 lines)

**Test Count:** 34 tests

**Coverage Breakdown:**
- Container validation: 5 tests
- Quality tier rolling: 8 tests
- Item count generation: 4 tests
- Biome type weighting: 4 tests
- Value scaling: 3 tests
- WITS upgrade mechanic: 6 tests
- Integration tests: 4 tests

**Coverage Percentage:** ~82%

---

### Critical Test Scenarios

1. **Container State Validation** (lines 45-92)
   - Not a container → Failure
   - Container locked/closed → Failure
   - Already searched → Empty result
   - Valid container → Success

2. **Quality Weight Distribution** (lines 100-165)
   - All danger levels sum to 100%
   - Safe: High Scavenged (60%)
   - Lethal: High Clan-Forged (40%)
   - Weight distribution adhered to over 1000 rolls

3. **WITS Upgrade Mechanic** (lines 173-238)
   - WITS 5 → 10% upgrade chance
   - WITS 10 → 20% upgrade chance (capped)
   - WITS 15 → Still 20% (cap enforced)
   - Upgrade shifts weights correctly

4. **Item Count Ranges** (lines 246-295)
   - Safe: 1-2 items
   - Unstable: 1-3 items
   - Hostile: 2-4 items
   - Lethal: 2-5 items

5. **Biome Type Distribution** (lines 303-378)
   - Industrial favors Materials (35%)
   - Organic favors Consumables (40%)
   - Void favors Weapons/Armor (25% each)
   - Verified over 500 rolls

6. **Value Scaling** (lines 386-420)
   - Jury-Rigged: ×0.5
   - Scavenged: ×1.0
   - Clan-Forged: ×1.5
   - Optimized: ×2.0
   - Myth-Forged: ×3.0

7. **Container Tier Override** (lines 428-460)
   - Container with LootTier.MythForged → All Myth-Forged items
   - Overrides danger-based quality roll
   - Used for boss chests, quest rewards

8. **Total Value/Weight Calculation** (lines 468-510)
   - TotalValue = sum of all item values
   - TotalWeight = sum of all item weights
   - Both fields populated correctly

---

## Design Rationale

### Why Danger-Scaled Quality Distribution?

- **Risk/Reward**: Lethal zones should reward better loot
- **Progression Incentive**: Encourages players to tackle harder content
- **Economy Balance**: Prevents Safe zone farming for high-value items

### Why WITS Bonus Upgrade Mechanic?

- **WITS Attribute Value**: Makes WITS useful outside combat/lockpicking
- **Skill Reward**: Perceptive characters find better items (thematic)
- **20% Cap**: Prevents guaranteed upgrades, maintains randomness

### Why Biome-Based Type Weighting?

- **Thematic Consistency**: Industrial zones should have more materials
- **Playstyle Support**: Consumable builds benefit from Organic biomes
- **Exploration Incentive**: Encourages visiting diverse biomes

### Why Template-Based (Not Procedural Stats)?

- **AAM-VOICE Compliance**: Handwritten descriptions ensure Domain 4
- **Design Control**: Exact item properties for balance
- **Simplicity**: No stat range logic, easier testing

### Why Separate Junk Category?

- **Economic Sink**: Always low-value, prevents farming
- **Flavor**: Reinforces post-apocalyptic scavenging theme
- **Inventory Pressure**: Forces player to choose what to keep

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **FIX:** Corrected code traceability in implementation files:
  - `ILootService.cs` now references SPEC-LOOT-001 (was incorrectly SPEC-INV-001)
  - `LootService.cs` now references SPEC-LOOT-001 (was incorrectly SPEC-INV-001)
  - `LootTables.cs` now includes spec reference (was missing)

### v0.3.1a - Initial Loot Service Implementation (2025-12-10)
- **ADDED**: `SearchContainerAsync()` for container-based loot
- **ADDED**: `GenerateLoot()` core generation method
- **ADDED**: Quality tier rolling with danger-level weights
- **ADDED**: WITS bonus upgrade mechanic (2% per point, 20% cap)
- **ADDED**: Biome-based item type weighting
- **ADDED**: Quality value scaling (×0.5 to ×3.0)
- **ADDED**: LootTables.cs with 60 AAM-VOICE compliant templates
- **ADDED**: Container state validation (locked, searched)
- **ADDED**: LootGenerationContext record
- **ADDED**: LootResult record with Failure/Empty factory methods

---

## Related Specifications

- [SPEC-INTERACT-001](SPEC-INTERACT-001.md) - Interaction System (container searching)
- [SPEC-INV-001](SPEC-INV-001.md) - Inventory System (item storage)
- [SPEC-COMBAT-001](SPEC-COMBAT-001.md) - Combat Service (enemy death drops, future)

---

## Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/LootService.cs` (312 lines)
- `RuneAndRust.Engine/Services/LootTables.cs` (441 lines)

**Interface:**
- `RuneAndRust.Core/Interfaces/ILootService.cs` (21 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/LootServiceTests.cs` (542 lines, 34 tests)

**Data Models:**
- `RuneAndRust.Core/Models/LootResult.cs`
- `RuneAndRust.Core/Entities/Item.cs`
- `RuneAndRust.Core/Enums/QualityTier.cs`
- `RuneAndRust.Core/Enums/ItemType.cs`
- `RuneAndRust.Core/Enums/BiomeType.cs`
- `RuneAndRust.Core/Enums/DangerLevel.cs`

---

**END OF SPECIFICATION**
