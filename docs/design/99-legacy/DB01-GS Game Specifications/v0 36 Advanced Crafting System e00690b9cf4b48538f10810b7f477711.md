# v0.36: Advanced Crafting System

Type: Feature
Description: Implements weapon/armor crafting, consumable crafting expansion, runic inscription system, and crafting stations with recipe discovery mechanics. 100+ recipes, 50+ components, 20+ inscriptions. 30-45 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.3 (Equipment & Loot), v0.9 (Merchants & Economy), v0.27.2 (Einbui field crafting)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.36.4: Service Integration & UI (v0%2036%204%20Service%20Integration%20&%20UI%205704b5d7e07d445daa341936981a5087.md), v0.36.1: Database Schema & Recipe Definitions (v0%2036%201%20Database%20Schema%20&%20Recipe%20Definitions%20d66072af2f3b4a57a9a0ca5d31eb253e.md), v0.36.2: Crafting Mechanics & Station System (v0%2036%202%20Crafting%20Mechanics%20&%20Station%20System%2082671594b9b84a5f8be0eaff16304701.md), v0.36.3: Modification & Inscription Systems (v0%2036%203%20Modification%20&%20Inscription%20Systems%2013a379086cd94dda93cf06141f6904fe.md)
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.36-CRAFTING

**Status:** Design Complete — Ready for Implementation

**Timeline:** 30-45 hours (4 specifications @ 7-12 hours each)

**Prerequisites:** v0.3 (Equipment & Loot), v0.9 (Merchants & Economy), v0.27.2 (Einbui field crafting)

**Master Roadmap Reference:** v0.36 is Phase 12's first component[[1]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

## I. Executive Summary

### The Deliverable

This specification defines the **Advanced Crafting System** that transforms salvaged materials into equipment:

- **Weapon crafting & modification** — Create and enhance weapons from components
- **Armor crafting & modification** — Forge armor with custom properties
- **Consumable crafting expansion** — Advanced alchemy and fabrication
- **Runic inscription system** — Apply mystical modifications to equipment
- **Crafting stations & recipes** — Facility-based crafting with discovery mechanics

v0.36 transforms equipment from pure loot drops into player-driven creation, enabling build customization and resource investment.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.36)

**v0.36.1: Database Schema & Recipe Definitions (7-10 hours)**

- Crafting recipe database (100+ recipes)
- Component tracking system
- Crafting station definitions
- Material requirements database
- Recipe discovery mechanics

**v0.36.2: Crafting Mechanics & Station System (8-12 hours)**

- CraftingService implementation
- Station-based crafting (forge, workshop, laboratory)
- Quality tier determination
- Component consumption
- Crafting success/failure mechanics

**v0.36.3: Modification & Inscription Systems (7-11 hours)**

- Weapon/armor modification system
- Runic inscription crafting
- Stat adjustment mechanics
- Special effect application
- Modification slot system

**v0.36.4: Service Integration & UI (8-10 hours)**

- Complete integration with v0.3 (Equipment), v0.9 (Merchants)
- Crafting UI/menu system
- Recipe book interface
- Unit test suite (15+ tests, 85%+ coverage)
- Performance optimization

### ❌ Explicitly Out of Scope

- Legendary crafting (defer to v0.37)
- Set item crafting (defer to v0.37)
- Enchantment system (covered by runic inscriptions only)
- Advanced material transmutation (defer to v2.0+)
- Crafting skill progression (using PP/Legend only)
- Visual crafting animations (separate polish phase)
- Real-time crafting minigames (defer to v2.0+)

---

## III. Core Mechanics Overview

### Crafting Philosophy

**Player-Driven Equipment Creation:**

- Players craft equipment from salvaged components
- Quality determined by component quality + crafting station tier
- Recipes discovered through exploration, quests, NPC merchants
- Crafting stations found in safe zones or established in player camps

**Resource Investment:**

```
Crafted Quality = min(Component Quality, Station Tier) + Recipe Bonus
```

### Crafting Categories

**1. Weapon Crafting**

- **New Weapons:** Create weapons from scratch (base stats)
- **Stat Range:** Tier 2-4 quality (Tier 5 Legendary in v0.37)
- **Components:** Metal, power cores, grips, weapon frames
- **Station:** Forge or Workshop

**2. Armor Crafting**

- **New Armor:** Create armor pieces (helmet, chest, gloves, legs, boots)
- **Stat Range:** Tier 2-4 quality
- **Components:** Alloys, plating, mesh, insulation
- **Station:** Forge or Workshop

**3. Consumable Crafting**

- **Healing Items:** Stim-packs, repair kits, medical supplies
- **Utility Items:** EMP grenades, smoke charges, signal flares
- **Buff Consumables:** Combat stims, focus enhancers, resistance boosters
- **Station:** Laboratory or Field (Einbui ability)

**4. Runic Inscriptions**

- **Weapon Runes:** +damage, elemental effects, status application
- **Armor Runes:** +resistance, damage reduction, special properties
- **Temporary vs Permanent:** Temporary runes (5-10 uses), Permanent (expensive)
- **Station:** Runic Altar (rare, found in specific locations)

### Crafting Stations

**Station Types:**

| Station | Crafts | Max Quality | Location |
| --- | --- | --- | --- |
| **Forge** | Weapons, Armor | Tier 3 | Safe zones, strongholds |
| **Workshop** | Weapons, Armor, Utility | Tier 4 | Major settlements |
| **Laboratory** | Consumables, Advanced items | Tier 4 | Research facilities |
| **Runic Altar** | Inscriptions, Rune items | Tier 5 | Sacred/ancient sites |
| **Field Station** | Basic consumables | Tier 2 | Einbui ability only |

**Station Requirements:**

- Must be at station to craft (no inventory crafting except Einbui)
- Station tier limits crafted quality
- Some recipes require specific stations

### Component System

**Component Types:**

**Weapon Components:**

- **Metal Ingots** (Tier 1-4): Scrap Iron → Star-Metal
- **Power Cores** (Tier 2-4): Energy cells for powered weapons
- **Weapon Frames** (Tier 2-4): Structural base
- **Grips/Handles** (Tier 1-3): Ergonomic components

**Armor Components:**

- **Alloy Plates** (Tier 1-4): Armor plating
- **Mesh Weave** (Tier 2-4): Flexible protection
- **Insulation** (Tier 1-3): Environmental protection
- **Servo Actuators** (Tier 3-4): Powered armor systems

**Consumable Components:**

- **Chemical Compounds** (Tier 1-3): Base reagents
- **Biological Samples** (Tier 1-4): Organic materials
- **Electronic Parts** (Tier 1-4): Circuitry and chips

**Runic Components:**

- **Aetheric Shards** (Tier 3-5): Crystallized Aether
- **Glyphs** (Tier 2-4): Pre-inscribed rune patterns
- **Binding Reagents** (Tier 2-4): Stabilization materials

### Recipe Discovery

**Discovery Methods:**

1. **Purchase from Merchants:** 50-200 credits per recipe
2. **Quest Rewards:** Special recipes from faction quests
3. **Loot Drops:** Found in data-logs, schematics in ruins
4. **Experimentation:** Einbui/Jötun-Reader abilities can discover recipes
5. **Archetype Abilities:** Some specializations grant bonus recipes

**Recipe Tiers:**

- **Basic:** Common, cheap, Tier 2 quality
- **Advanced:** Uncommon, moderate cost, Tier 3 quality
- **Expert:** Rare, expensive, Tier 4 quality
- **Master:** Very rare, very expensive, Tier 4 with bonuses

### Quality Determination

**Crafting Quality Formula:**

```csharp
int CraftedQuality = Math.Min(
    lowestComponentQuality,
    stationMaxTier
) + recipeBonus;

CraftedQuality = Math.Clamp(CraftedQuality, 1, 4); // v0.36 caps at Tier 4
```

**Example:**

- Recipe: Advanced Plasma Rifle (requires Tier 3 components)
- Components: Tier 3 Metal, Tier 4 Power Core, Tier 3 Frame
- Station: Workshop (max Tier 4)
- Recipe Bonus: +1 (Advanced recipe)
- **Result:** min(3, 4) + 1 = **Tier 4 Plasma Rifle**

---

## IV. System Architecture

### Database Schema (v0.36.1)

**New Tables:**

**1. Crafting_Recipes**

```sql
CREATE TABLE Crafting_Recipes (
    recipe_id INTEGER PRIMARY KEY,
    recipe_name TEXT NOT NULL,
    recipe_tier TEXT CHECK(recipe_tier IN ('Basic', 'Advanced', 'Expert', 'Master')),
    crafted_item_type TEXT NOT NULL, -- 'Weapon', 'Armor', 'Consumable', 'Inscription'
    crafted_item_id INTEGER, -- NULL for dynamic generation
    required_station TEXT NOT NULL,
    crafting_time_minutes INTEGER DEFAULT 5,
    quality_bonus INTEGER DEFAULT 0,
    discovery_method TEXT, -- 'Merchant', 'Quest', 'Loot', 'Ability'
    recipe_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**2. Recipe_Components**

```sql
CREATE TABLE Recipe_Components (
    component_id INTEGER PRIMARY KEY,
    recipe_id INTEGER NOT NULL,
    component_item_id INTEGER NOT NULL,
    quantity_required INTEGER NOT NULL,
    minimum_quality INTEGER DEFAULT 1,
    
    FOREIGN KEY (recipe_id) REFERENCES Crafting_Recipes(recipe_id),
    FOREIGN KEY (component_item_id) REFERENCES Items(item_id)
);
```

**3. Crafting_Stations**

```sql
CREATE TABLE Crafting_Stations (
    station_id INTEGER PRIMARY KEY,
    station_name TEXT NOT NULL,
    station_type TEXT NOT NULL CHECK(station_type IN ('Forge', 'Workshop', 'Laboratory', 'Runic_Altar', 'Field_Station')),
    max_quality_tier INTEGER NOT NULL,
    location_sector_id INTEGER,
    location_room_id TEXT,
    is_portable BOOLEAN DEFAULT 0,
    
    FOREIGN KEY (location_sector_id) REFERENCES Sectors(sector_id)
);
```

**4. Character_Recipes**

```sql
CREATE TABLE Character_Recipes (
    character_recipe_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    recipe_id INTEGER NOT NULL,
    discovered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    times_crafted INTEGER DEFAULT 0,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (recipe_id) REFERENCES Crafting_Recipes(recipe_id),
    UNIQUE(character_id, recipe_id)
);
```

**5. Equipment_Modifications**

```sql
CREATE TABLE Equipment_Modifications (
    modification_id INTEGER PRIMARY KEY,
    equipment_item_id INTEGER NOT NULL,
    modification_type TEXT NOT NULL, -- 'Stat_Boost', 'Resistance', 'Special_Effect', 'Inscription'
    modification_value TEXT NOT NULL, -- JSON: {"stat": "damage", "value": 5}
    is_permanent BOOLEAN DEFAULT 1,
    remaining_uses INTEGER, -- NULL if permanent
    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (equipment_item_id) REFERENCES Character_Inventory(inventory_id)
);
```

**6. Runic_Inscriptions**

```sql
CREATE TABLE Runic_Inscriptions (
    inscription_id INTEGER PRIMARY KEY,
    inscription_name TEXT NOT NULL,
    inscription_tier INTEGER NOT NULL,
    target_equipment_type TEXT NOT NULL, -- 'Weapon', 'Armor'
    effect_type TEXT NOT NULL, -- 'Damage_Bonus', 'Elemental', 'Resistance', 'Status'
    effect_value TEXT NOT NULL, -- JSON effect data
    is_temporary BOOLEAN DEFAULT 0,
    uses_if_temporary INTEGER DEFAULT 10,
    component_requirements TEXT NOT NULL, -- JSON array of components
    inscription_description TEXT
);
```

### Service Architecture

**CraftingService** (v0.36.2) — Core crafting operations

- CraftItem() — Execute recipe with components
- CalculateQuality() — Determine output quality
- ConsumeComponents() — Remove components from inventory
- ValidateRecipe() — Check requirements met

**ModificationService** (v0.36.3) — Equipment modification

- ApplyModification() — Add modification to equipment
- RemoveModification() — Strip modifications
- ValidateModSlots() — Check modification limits
- ApplyInscription() — Add runic effects

**RecipeService** (v0.36.4) — Recipe management

- DiscoverRecipe() — Unlock recipe for character
- GetAvailableRecipes() — Query known recipes
- GetCraftableRecipes() — Filter by components available
- PurchaseRecipe() — Buy from merchant

---

## V. Implementation Phases

### Phase 1: v0.36.1 — Database Schema & Recipe Definitions

**Timeline:** 7-10 hours

**Deliverables:**

- 6 new tables (Recipes, Components, Stations, Character_Recipes, Modifications, Inscriptions)
- 100+ recipe definitions
- 50+ component items
- 20+ runic inscriptions
- SQL migration script

**Success Criteria:**

- ✅ All tables created with constraints
- ✅ 100+ recipes seeded (25 weapons, 25 armor, 30 consumables, 20 inscriptions)
- ✅ 50+ component items in Items table
- ✅ 10+ crafting stations defined
- ✅ Foreign key relationships enforced

---

### Phase 2: v0.36.2 — Crafting Mechanics & Station System

**Timeline:** 8-12 hours

**Deliverables:**

- CraftingService implementation
- Station validation logic
- Quality calculation engine
- Component consumption system
- Unit tests (10+ tests, 85%+ coverage)

**Success Criteria:**

- ✅ Players can craft items at appropriate stations
- ✅ Quality correctly calculated from components + station
- ✅ Components consumed on successful craft
- ✅ Failure handling (insufficient materials, wrong station)
- ✅ Crafting time tracking

---

### Phase 3: v0.36.3 — Modification & Inscription Systems

**Timeline:** 7-11 hours

**Deliverables:**

- ModificationService implementation
- Equipment stat modification system
- Runic inscription application
- Temporary vs permanent modification tracking
- Unit tests (10+ tests, 85%+ coverage)

**Success Criteria:**

- ✅ Modifications apply to equipment correctly
- ✅ Inscriptions add special effects
- ✅ Temporary modifications track remaining uses
- ✅ Stat bonuses calculate correctly in combat
- ✅ Modification limits enforced (max 3 per item)

---

### Phase 4: v0.36.4 — Service Integration & UI

**Timeline:** 8-10 hours

**Deliverables:**

- Complete integration with v0.3 (Equipment), v0.9 (Merchants)
- Crafting menu UI
- Recipe book interface
- Component management UI
- Complete unit test suite (15+ tests)
- Serilog logging throughout

**Success Criteria:**

- ✅ Crafting menu accessible at stations
- ✅ Recipe book shows discovered recipes
- ✅ Players can preview crafting costs
- ✅ Merchants sell recipes
- ✅ Einbui field crafting integration
- ✅ 85%+ test coverage

---

## VI. Integration with Existing Systems

### v0.3 Equipment & Loot Integration

```csharp
// Crafted items use same equipment system
public async Task<Equipment> CraftEquipment(
    int recipeId, 
    List<Component> components,
    CraftingStation station)
{
    // Calculate quality
    int quality = CalculateQuality(components, station);
    
    // Generate equipment using existing system
    var equipment = await _equipmentService.GenerateEquipment(
        itemType: recipe.CraftedItemType,
        qualityTier: quality,
        isCrafted: true
    );
    
    // Add crafted flag for identification
    equipment.IsCrafted = true;
    equipment.CrafterName = [character.Name](http://character.Name);
    
    return equipment;
}
```

### v0.9 Merchants & Economy Integration

```csharp
// Merchants sell recipes
public async Task<bool> PurchaseRecipe(
    int characterId,
    int recipeId,
    int merchantId)
{
    var recipe = await _db.QuerySingleAsync<Recipe>(
        "SELECT * FROM Crafting_Recipes WHERE recipe_id = ?",
        recipeId);
    
    int cost = CalculateRecipeCost(recipe.RecipeTier);
    
    // Use existing economy system
    if (await _economyService.SpendCredits(characterId, cost))
    {
        await _recipeService.DiscoverRecipe(characterId, recipeId);
        return true;
    }
    
    return false;
}
```

### v0.27.2 Einbui Field Crafting Integration

```csharp
// Einbui can craft at Field Station (special case)
public async Task<bool> CanFieldCraft(int characterId, int recipeId)
{
    var hasEinbui = await _db.QuerySingleAsync<bool>(@"
        SELECT COUNT(*) > 0
        FROM Character_Specializations
        WHERE character_id = ? AND specialization_id = 27002",
        characterId);
    
    if (!hasEinbui)
        return false;
    
    var recipe = await _recipeService.GetRecipe(recipeId);
    
    // Einbui can only field craft consumables
    return recipe.CraftedItemType == "Consumable" &&
           recipe.RecipeTier == "Basic";
}
```

---

## VII. Recipe Examples

### Weapon Recipe: Plasma Rifle

```sql
INSERT INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, recipe_description)
VALUES (1001, 'Plasma Rifle', 'Advanced', 'Weapon', 'Workshop', 1, 
    'Energy weapon firing superheated plasma bolts. Requires Tier 3+ components.');

INSERT INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
(1001, 5001, 1, 3), -- Star-Metal Frame (Tier 3)
(1001, 5002, 1, 3), -- Plasma Core (Tier 3)
(1001, 5003, 1, 2); -- Synthetic Grip (Tier 2)
```

### Armor Recipe: Reinforced Plating

```sql
INSERT INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus)
VALUES (2001, 'Reinforced Chest Plate', 'Expert', 'Armor', 'Forge', 2);

INSERT INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
(2001, 6001, 2, 3), -- Titanium Alloy (Tier 3, x2)
(2001, 6002, 1, 3), -- Ballistic Mesh (Tier 3)
(2001, 6003, 1, 2); -- Thermal Insulation (Tier 2)
```

### Consumable Recipe: Combat Stim

```sql
INSERT INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus)
VALUES (3001, 'Combat Stimulant', 'Basic', 'Consumable', 'Laboratory', 0);

INSERT INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
(3001, 7001, 2, 1), -- Chemical Compound (Tier 1, x2)
(3001, 7002, 1, 1); -- Adrenaline Extract (Tier 1)
```

### Runic Inscription: Flame Rune

```sql
INSERT INTO Runic_Inscriptions (inscription_id, inscription_name, inscription_tier, target_equipment_type, effect_type, effect_value, is_temporary, uses_if_temporary)
VALUES (8001, 'Rune of Flame', 3, 'Weapon', 'Elemental', 
    '{"element": "Fire", "bonus_damage": 5, "burn_chance": 0.15}',
    1, 10);
```

---

## VIII. Quality & Balance

### Component Drop Rates

**Tier 1 Components:** 40% drop from enemies

**Tier 2 Components:** 25% drop from enemies, purchasable from merchants

**Tier 3 Components:** 15% drop from elites, quest rewards

**Tier 4 Components:** 5% drop from bosses, rare merchant stock

### Recipe Costs

**Basic Recipes:** 50-100 credits

**Advanced Recipes:** 150-300 credits

**Expert Recipes:** 400-600 credits

**Master Recipes:** 800-1200 credits

### Crafting Time

**Consumables:** 2-5 minutes

**Weapons:** 10-15 minutes

**Armor:** 10-15 minutes

**Inscriptions:** 5-10 minutes

*(Real-time if implementing time system, instant otherwise)*

---

## IX. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Usage:**

- "Crafting station access protocols"
- "Component synthesis parameters"
- "Runic inscription data-weaving"
- "Equipment modification subroutines"

**❌ Incorrect Usage:**

- ~~"Magical enchantment"~~
- ~~"Divine blessing forging"~~
- ~~"Alchemical transmutation"~~

### Technology Framing

**Runic Inscriptions = Data Weaving:**

- Runes are "compiled instruction sets" etched into equipment
- "Aetheric Shards" are crystallized data fragments from Pre-Glitch
- Inscriptions "reprogram" equipment properties at quantum level
- Not magic — advanced nanotechnology interfacing with Aetheric substrate

**Crafting Stations = Fabrication Facilities:**

- Forges are "metal synthesis chambers"
- Workshops are "precision fabrication bays"
- Laboratories are "molecular assembly stations"
- Runic Altars are "quantum weaving interfaces"

---

## X. Testing Strategy

### Unit Tests (Target: 85%+ coverage)

```csharp
[TestClass]
public class CraftingServiceTests
{
    [TestMethod]
    public async Task CraftItem_ValidRecipeAndComponents_Success()
    {
        // Arrange: Recipe requires Tier 3 metal, Tier 3 core
        // Player has Tier 3+ components, at Workshop (max Tier 4)
        // Act: CraftItem()
        // Assert: Tier 4 weapon created, components consumed
    }
    
    [TestMethod]
    public async Task CraftItem_InsufficientComponents_Fails()
    {
        // Arrange: Recipe requires 2x Tier 3 metal
        // Player only has 1x Tier 3 metal
        // Act: CraftItem()
        // Assert: Returns error, no components consumed
    }
    
    [TestMethod]
    public async Task CalculateQuality_LowQualityComponent_CapsOutput()
    {
        // Arrange: Recipe with Tier 4 station, but Tier 2 component
        // Act: CalculateQuality()
        // Assert: Output capped at Tier 2 (lowest component quality)
    }
    
    [TestMethod]
    public async Task ApplyInscription_TemporaryRune_TracksUses()
    {
        // Arrange: Weapon with temporary Flame Rune (10 uses)
        // Act: Use weapon in combat 3 times
        // Assert: Rune has 7 uses remaining
    }
}
```

---

## XI. Success Criteria

**Functional Requirements:**

- ✅ Players can craft weapons, armor, consumables at appropriate stations
- ✅ Quality determined by components + station tier
- ✅ Recipes discoverable through multiple methods
- ✅ Modifications apply stat bonuses correctly
- ✅ Runic inscriptions add special effects
- ✅ Temporary modifications track uses
- ✅ Einbui field crafting works without station

**Quality Gates:**

- ✅ 85%+ unit test coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice, technology framing)
- ✅ ASCII-only entity names
- ✅ Complete integration with v0.3, v0.9, v0.27.2

**Content:**

- ✅ 100+ recipes implemented
- ✅ 50+ component items defined
- ✅ 20+ runic inscriptions
- ✅ 10+ crafting station locations

---

## XII. Child Specifications

This parent specification spawns 4 child specifications:

1. **v0.36.1: Database Schema & Recipe Definitions** (7-10 hours)
    - Complete database schema
    - 100+ recipe definitions
    - Component and station data
2. **v0.36.2: Crafting Mechanics & Station System** (8-12 hours)
    - CraftingService implementation
    - Quality calculation
    - Component consumption
3. **v0.36.3: Modification & Inscription Systems** (7-11 hours)
    - ModificationService
    - Runic inscription application
    - Temporary modification tracking
4. **v0.36.4: Service Integration & UI** (8-10 hours)
    - Complete integration
    - Crafting menu UI
    - Recipe book interface

**Total Timeline:** 30-45 hours

---

**Implementation-ready specification for Advanced Crafting System complete.**

**Next:** Implement v0.36.1 (Database Schema & Recipe Definitions)