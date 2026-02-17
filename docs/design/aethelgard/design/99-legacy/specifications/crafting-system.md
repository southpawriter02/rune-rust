# Crafting System — Mechanic Specification v5.0

Type: Mechanic
Description: Recipe-based item creation system with skill checks, crafting stations, component consumption, quality calculation, and equipment modifications (runic inscriptions).
Priority: Should-Have
Status: Review
Target Version: Alpha
Dependencies: Equipment System, Character System (WITS attribute), Dice Pool System, Inventory System
Implementation Difficulty: Medium
Balance Validated: No
Document ID: AAM-SPEC-MECH-CRAFTING-v5.0
Parent item: Equipment System — Core System Specification v5.0 (Equipment%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%200ec604d185934907915e1ba9cd3e8800.md)
Proof-of-Concept Flag: No
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## **I. Core Philosophy: Salvage Engineering**

The Crafting System enables players to **create consumables, modify equipment, and craft items** through a skill-based recipe system. Crafting represents the survivor's ability to repurpose salvaged materials into functional gear using specialized stations scattered throughout Aethelgard.

**Design Pillars:**

- **Crafting as Skill Expression:** Character build choices (WITS, specialization) directly impact crafting success
- **Component Scarcity Creates Value:** Rare components feel meaningful to find and use
- **Station-Based Specialization:** Different stations enable different crafting types
- **Risk-Reward Tension:** Components consumed on attempt (success OR failure)

---

## **II. System Scope & Dependencies**

### **System Classification**

- **Type:** Mechanic (Child of Equipment System)
- **Parent System:** Equipment System — Core System Specification v5.0

### **Integration Points**

**Upstream Dependencies:**

- Character System (WITS attribute for skill checks)
- Dice Pool System (skill check resolution)
- Inventory System (component storage, item management)
- Equipment System (modification targets)

**Downstream Dependencies:**

- Field Medicine (Bone-Setter crafting recipes)
- Combat System (consumable usage)
- Economy System (component trading, sell values)

### **Code References**

- **Primary Service:** `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs`
- **Modification Service:** `RuneAndRust.Engine/Crafting/ModificationService.cs`
- **Recipe Service:** `RuneAndRust.Engine/Crafting/RecipeService.cs`
- **Core Models:** `RuneAndRust.Core/CraftingRecipe.cs`, `RuneAndRust.Core/CraftingComponent.cs`
- **Repository:** `RuneAndRust.Persistence/CraftingRepository.cs`

---

## **III. Crafting Stations**

### **Station Types**

| Station Type | Description | Recipe Examples | Max Quality Tier |
| --- | --- | --- | --- |
| **Medical** | Field medicine, bandages | Health Potions, Trauma Kits | Varies by station tier |
| **Forge** | Weapons, armor | Equipment crafting, repairs | Varies by station tier |
| **Alchemy** | Potions, buffs | Stamina Elixirs, Antidotes | Varies by station tier |
| **Runic** | Inscriptions | Equipment modifications | Varies by station tier |
| **Any** | No station required | Basic items, field repairs | Scavenged (1) |

### **Station Quality Tiers**

| Station Tier | Name | Max Quality Output | Location Rarity |
| --- | --- | --- | --- |
| 1 | Makeshift | Scavenged (1) | Common |
| 2 | Functional | Clan-Forged (2) | Uncommon |
| 3 | Advanced | Optimized (3) | Rare |
| 4 | Master | Myth-Forged (4) | Very Rare |

**Quality Cap Logic:**

Even with Epic components (Tier 4), output is capped by station tier. Finding better stations enables better crafting outcomes.

---

## **IV. Component System**

### **Component Categories**

| Category | Types | Rarity Range | Primary Use |
| --- | --- | --- | --- |
| **Field Medicine** | CommonHerb, CleanCloth, Suture, Antiseptic, Splint, Stimulant | Common-Rare | Medical consumables |
| **Economy Common** | ScrapMetal, RustedComponents, ClothScraps, BoneShards | Common | Basic crafting |
| **Economy Uncommon** | StructuralScrap, AethericDust, TemperedSprings, MedicinalHerbs | Uncommon | Quality crafting |
| **Economy Rare** | DvergrAlloyIngot, CorruptedCrystal, AncientCircuitBoard | Rare | Advanced crafting |
| **Economy Epic** | JotunCoreFragment, RunicEtchingTemplate | Epic | Legendary items |

### **Component Rarity Effects**

| Rarity | Drop Rate | Sell Value (Cogs) | Quality Contribution |
| --- | --- | --- | --- |
| Common | High | 5-20 | Tier 1 |
| Uncommon | Medium | 25-75 | Tier 2 |
| Rare | Low | 100-300 | Tier 3 |
| Epic | Very Low | 500-1000 | Tier 4 |

---

## **V. Crafting Mechanics**

### **A. Core Crafting Flow**

```
1. Player approaches crafting station
2. System validates station type matches recipe requirement
3. System checks component availability in inventory
4. Player initiates craft attempt
5. Skill check resolved (WITS vs Recipe DC)
6. Components consumed (regardless of outcome)
7. On success: Item created with calculated quality
8. On failure: No item created, components lost
```

### **B. Skill Check Resolution**

**Formula:**

```jsx
Skill Check = Roll(WITS d6).TotalValue vs Recipe.SkillCheckDC

Example:
  Recipe.SkillAttribute = "WITS"
  Character.WITS = 5
  Recipe.SkillCheckDC = 10

  Roll: 5d6 = [3, 5, 6, 2, 4] = 20
  Result: 20 >= 10 → SUCCESS
```

### **Recipe DC Guidelines**

| Difficulty | DC | Target Success Rate (WITS 5) |
| --- | --- | --- |
| Easy | 8 | ~99% |
| Standard | 10 | ~95% |
| Moderate | 12 | ~85% |
| Difficult | 15 | ~65% |
| Expert | 18 | ~40% |

### **C. Quality Calculation**

**Formula:**

```jsx
Final Quality = min(LowestComponentQuality, Station.MaxQualityTier) + Recipe.QualityBonus

Quality Tiers:
  0 = Jury-Rigged
  1 = Scavenged
  2 = Clan-Forged
  3 = Optimized
  4 = Myth-Forged

Example:
  Lowest component quality = Clan-Forged (2)
  Station max tier = 3 (Optimized)
  Recipe quality bonus = 0

  Final Quality = min(2, 3) + 0 = 2 (Clan-Forged)
```

---

## **VI. Equipment Modifications**

### **Modification System**

**Rules:**

- Only equipment can receive modifications (runic inscriptions)
- Maximum **3 modifications** per item
- Requires Runic station type
- Consumes components + credits
- Cannot be undone without losing the modification

### **Apply Modification Flow**

```jsx
1. Validate inscription exists
2. Validate equipment exists in inventory
3. Validate equipment type matches inscription target
4. Check modification slot limit (max 3)
5. Validate and consume component requirements
6. Deduct credit cost if applicable
7. Apply modification to equipment
```

### **Remove Modification**

- Modifications can be removed at any Runic station
- No component refund on removal
- Equipment reverts to base stats

---

## **VII. Database Schema**

### **CraftingRecipes Table**

```sql
CREATE TABLE CraftingRecipes (
  recipe_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  station_required TEXT DEFAULT 'Any',
  skill_attribute TEXT DEFAULT 'WITS',
  skill_check_dc INTEGER DEFAULT 10,
  quality_bonus INTEGER DEFAULT 0,
  output_item_id INTEGER NOT NULL,
  output_quantity INTEGER DEFAULT 1,
  specialization_required TEXT,
  FOREIGN KEY (output_item_id) REFERENCES Items(item_id)
);
```

### **RecipeComponents Table**

```sql
CREATE TABLE RecipeComponents (
  recipe_id INTEGER NOT NULL,
  component_type TEXT NOT NULL,
  quantity INTEGER DEFAULT 1,
  FOREIGN KEY (recipe_id) REFERENCES CraftingRecipes(recipe_id)
);
```

### **Modifications Table**

```sql
CREATE TABLE Modifications (
  modification_id INTEGER PRIMARY KEY,
  item_instance_id INTEGER NOT NULL,
  inscription_id INTEGER NOT NULL,
  applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (item_instance_id) REFERENCES Item_Instances(instance_id)
);
```

---

## **VIII. Service Architecture**

### **AdvancedCraftingService**

```csharp
public class AdvancedCraftingService
{
  public CraftResult CraftItem(int characterId, int recipeId, int stationId)
  public bool ValidateStation(CraftingRecipe recipe, CraftingStation station)
  public bool ValidateComponents(int characterId, CraftingRecipe recipe)
  public int CalculateQuality(List<CraftingComponent> components, CraftingStation station, CraftingRecipe recipe)
  public void ConsumeComponents(int characterId, CraftingRecipe recipe)
}
```

### **ModificationService**

```csharp
public class ModificationService
{
  public ModResult ApplyModification(int characterId, int equipmentId, int inscriptionId)
  public bool RemoveModification(int characterId, int modificationId)
  public int GetModificationCount(int equipmentId)
  public bool CanApplyModification(int equipmentId)
}
```

---

## **IX. Specialization Integration**

### **Bone-Setter (Field Medicine)**

**Exclusive Recipes:**

- Advanced Trauma Kits
- Splint Repairs
- Emergency Stimulants

**Bonuses:**

- +2 bonus to medical crafting skill checks
- Reduced component costs for medical items

### **Einbui (Survivalist)**

**Exclusive Recipes:**

- Improvised Explosives
- Camouflage Gear
- Trap Components

**Bonuses:**

- Can craft at lower-tier stations without quality penalty
- Reduced failure component loss (50% recovered)

---

## **X. Balance Considerations**

### **Component Consumption on Failure**

**Design Intent:** Creates tension and meaningful resource decisions

- Components ALWAYS consumed on attempt
- Encourages WITS investment for crafting-focused builds
- Rare components feel precious
- Prevents infinite retry loops

### **Station Quality Caps**

**Design Intent:** Rewards exploration and creates progression

- Early game: Makeshift stations limit to Scavenged
- Mid game: Functional stations enable Clan-Forged
- Late game: Master stations unlock Myth-Forged potential

### **Tunable Parameters**

| Parameter | Location | Current | Range | Impact |
| --- | --- | --- | --- | --- |
| BaseSkillDC | Per recipe | 10 | 8-20 | Success rate |
| MaxModifications | ModificationService | 3 | 1-5 | Equipment power ceiling |
| ComponentConsume | AdvancedCraftingService | Always | - | Economy pressure |

---

## **XI. UI Integration**

### **Crafting Screen**

```
╔══════════════════════════════════════════════╗
║ CRAFTING - Medical Station (Functional)      ║
╠══════════════════════════════════════════════╣
║ Recipe: Health Potion                        ║
║ DC: 10 (WITS)    Your WITS: 5                ║
║                                              ║
║ Components Required:                         ║
║   CommonHerb x2     [✓] 5 in inventory       ║
║   CleanCloth x1     [✓] 3 in inventory       ║
║                                              ║
║ Output Quality: Clan-Forged (max at station) ║
║                                              ║
║ [CRAFT]                    [CANCEL]          ║
╚══════════════════════════════════════════════╝
```

---

*This specification follows the v5.0 Three-Tier Template standard. Source: SPEC-SYSTEM-004 (Imported Game Docs). The Crafting System is a child mechanic of the Equipment System.*